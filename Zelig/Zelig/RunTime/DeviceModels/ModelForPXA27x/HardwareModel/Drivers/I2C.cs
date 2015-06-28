//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.DeviceModels.Chipset.PXA27x.Drivers
{
    using System;
    using System.Threading;
    using System.Runtime.CompilerServices;

    using Microsoft.Zelig.Runtime;


    /// <summary>
    /// I2C device driver for the PXA271 I2C hardware.
    /// </summary>
    public abstract class I2C
    {

        #region Constructor/Destructor
        /// <summary>
        /// The current transaction in progress.
        /// </summary>
        private I2CBus_Transaction m_currentTransaction;
        /// <summary>
        /// The current sub transaction of the <see cref="m_currentTransaction"/>
        /// in progress.
        /// </summary>
        private I2CBus_TransactionUnit m_currentTransactionUnit;

        private const byte c_DirectionWrite = 0x00;
        private const byte c_DirectionRead = 0x01;

        /// <summary>
        /// The serial data line (SDA) of the I2C port.
        /// </summary>
        private GPIO.Port m_sdaPort;
        /// <summary>
        /// The serial clock line (SCL) of the I2C port.
        /// </summary>
        private GPIO.Port m_sclPort;

        /// <summary>
        /// Initialized the I2C driver.
        /// </summary>
        /// <remarks>
        /// This function configures the slave mode address
        /// of the I2C device to 0x01, i.e., clients are not
        /// able to talk to devices configured for this address.
        /// </remarks>
        public void Initialize()
        {
            //no active action
            m_currentTransaction = null;
            m_currentTransactionUnit = null;

            //configure SCL
            m_sclPort = new GPIO.Port(PXA27x.I2C.PIN_I2C_SCL, false);
            GPIO.Instance.EnableAsInputAlternateFunction(m_sclPort, 1);

            //configure SDA
            m_sdaPort = new GPIO.Port(PXA27x.I2C.PIN_I2C_SDA, false);
            GPIO.Instance.EnableAsInputAlternateFunction(m_sdaPort, 1);

            //configure the priority of the interrupt source in the
            //InterruptController Hardware
            //note: this is not the driver used to register
            //      the interrupt handler with!
            PXA27x.InterruptController.Instance.SetPriority(PXA27x.InterruptController.IRQ_INDEX_I2C, 0);

            //enable and register interrupt for the I2C controller
            m_i2cInterruptHandler = InterruptController.Handler.Create(PXA27x.InterruptController.IRQ_INDEX_I2C,
                                                                        InterruptPriority.Normal,
                                                                        InterruptSettings.RisingEdge /*| InterruptSettings.Fast*/,
                                                                        new InterruptController.Callback(this.ProcessI2CInterrupt));
            InterruptController.Instance.RegisterAndEnable(m_i2cInterruptHandler);

            //enable clock to I2C unit
            //note: no access to the I2C registers
            //      before setting this bit
            ClockManager.Instance.CKEN.EnI2C = true;

            //setup our slave address - only used for slave mode
            PXA27x.I2C.Instance.ISAR = 0x1; //our default address
        }
        #endregion

        #region Public API
        /// <summary>
        /// Executes a transaction on the I2C bus.
        /// </summary>
        /// <param name="transaction">The action to execute on 
        /// the I2C bus.</param>
        public void StartTransactionAsMaster(I2CBus_Transaction transaction)
        {
            if (null == transaction)
                throw new ArgumentNullException("transaction");

            //reset the state of this transaction
            transaction.ResetTransaction();

            //setup current action and action unit
            //fail if there is a transaction going on at the moment
            if (null != Interlocked.CompareExchange<I2CBus_Transaction>(ref this.m_currentTransaction, transaction, null))
                throw new InvalidOperationException("m_currentTransaction != null");

            //get the first transaction unit
            m_currentTransactionUnit = transaction.Pop();

            //setup default control field for new transfer
            PXA27x.I2C.ICR_Bitfield control = new PXA27x.I2C.ICR_Bitfield
            {
                //configure interrupts
                DRFIE = true, //enable Data Buffer Register (IDBR) receive interrupt
                ITEIE = true, //enable IDBR transit empty interrupt
                BEIE = true,  //enable Bus error interrupt
                GCD = true,   //disable General call

                //setup unit as master
                IUE = true,   //enable I2C unit
                SCLE = true,  //enable driving the SCL line

                //send first byte indication
                START = true, //send start condition
                TB = true,    //Transfer Byte
            };

            //setup I2C address
            uint address = (uint)(0xFE & (transaction.Address << 1));

            //setup the direction bit
            address |= m_currentTransactionUnit.IsReadTransaction() ? I2C.c_DirectionRead : I2C.c_DirectionWrite;

            //the peer address is the first byte to send
            PXA27x.I2C.Instance.IDBR = address;

            //Use high bit rate if requested
            control.FM = (transaction.ClockRate >= 400);

            //Initiate the send operation
            PXA27x.I2C.Instance.ICR = control;
        }
        #endregion

        #region Private API
        /// <summary>
        /// The handler for all interrupts generated by the I2C device.
        /// </summary>
        private InterruptController.Handler m_i2cInterruptHandler;

        /// <summary>
        /// Ends any active transaction started as
        /// a I2C master device.
        /// </summary>
        public void StopTransactionAsMaster()
        {
            //unset all flags that depend on the transaction flow
            PXA27x.I2C.Instance.ICR.MA = true; //signal master abort command

            m_currentTransaction = null;
            m_currentTransactionUnit = null;
        }

        /// <summary>
        /// Processes interrupts generated by the I2C
        /// device.
        /// </summary>
        /// <param name="handler">The handler that was registered with the
        /// interrupt controller for the interrupt.</param>
        private void ProcessI2CInterrupt(InterruptController.Handler handler)
        {
            //read control and status
            PXA27x.I2C.ISR_Bitfield status = PXA27x.I2C.Instance.ISR;

            if (status.ITE) //If Transmit Buffer Empty interrupt
            {
                OnTransmitBufferEmpty();
            }
            else if (status.IRF) //If Receive Buffer Full interrupt
            {
                OnReceiveBufferFull();
            }
            else if (status.BED || status.ALD) //If there was a bus error or arbitration lost indication
            {
                //save the data in case we call "StopTransactionAsMaster" later on
                //which will clear the variable
                I2CBus_Transaction transaction = m_currentTransaction;

                //Shut down I2C interrupts, stop driving bus
                PXA27x.I2C.Instance.ICR.IUE = false;
                StopTransactionAsMaster();

                //Problem talking to slave - we're done
                transaction.Signal(I2CBus_Transaction.CompletionStatus.Aborted);
            }
        }

        /// <summary>
        /// Called from the interrupt handler when the data register
        /// is full and data needs to be saved locally for later processing.
        /// </summary>
        private void OnReceiveBufferFull()
        {
            PXA27x.I2C i2c = PXA27x.I2C.Instance;

            //save the data in case we call "StopTransactionAsMaster" later on
            //which will clear the variables
            I2CBus_Transaction transaction = m_currentTransaction;
            I2CBus_TransactionUnit transactionUnit = m_currentTransactionUnit;

            //clear interrupt flag
            i2c.ISR.IRF = true;

            //Read receive buffer data
            if (!transactionUnit.IsTransferComplete)
            {
                byte data = (byte)i2c.IDBR;
                transactionUnit.Push(data);
            }

            //If last byte was just received
            if (transactionUnit.IsTransferComplete)
            {
                //Tidy up the control register
                i2c.ICR.STOP = i2c.ICR.ACKNAK = false;

                //Finish up the transaction
                if (!transaction.IsProcessingLastUnit)
                {
                    //If more to transaction
                    StartTransactionAsMaster(transaction /*, true */);
                }
                else
                {
                    StopTransactionAsMaster();

                    //signal the succesfull complection
                    transaction.Signal(I2CBus_Transaction.CompletionStatus.Completed);

                    //Shut down I2C master
                    i2c.ICR.IUE = false;
                }
            }
            else if (transactionUnit.IsLastByte) //If exactly 1 byte left to receive
            {
                PXA27x.I2C.ICR_Bitfield ctrl = i2c.ICR;

                //Initiate next byte read from slave
                ctrl.START = false;
                ctrl.STOP = ctrl.ALDIE = ctrl.ACKNAK = ctrl.TB = true;

                i2c.ICR = ctrl;
            }
            else //If more than one byte left to receive
            {
                PXA27x.I2C.ICR_Bitfield ctrl = i2c.ICR;

                //Initiate next byte read from slave
                ctrl.START = ctrl.STOP = ctrl.ACKNAK = false;
                ctrl.ALDIE = ctrl.TB = true;

                i2c.ICR = ctrl;
            }
        }

        /// <summary>
        /// Called from the interrupt handler when the data register
        /// is empty and new data needs to be send to the client device.
        /// </summary>
        private void OnTransmitBufferEmpty()
        {
            //save status and configuration
            PXA27x.I2C i2c = PXA27x.I2C.Instance;
            PXA27x.I2C.ISR_Bitfield status = i2c.ISR;
            PXA27x.I2C.ICR_Bitfield ctrl = i2c.ICR;

            //save the data in case we call "StopTransactionAsMaster" later on
            //which will clear the variables
            I2CBus_Transaction transaction = m_currentTransaction;
            I2CBus_TransactionUnit transactionUnit = m_currentTransactionUnit;

            //clear transmit empty indication flag
            //(writing "true" will clear the flag)
            i2c.ISR.ITE = true;// PXA27x.I2C.I2C.ISR__ITE;

            //If arbitration loss detected, clear that, too
            //(writing "true" will clear the flag)
            if (status.ALD)
            {
                i2c.ISR.ALD = true;// = PXA27x.I2C.I2C.ISR__ALD;
            }

            //If we just finished sending an address in Read mode
            if (status.RWM)
            {
                //If we are expecting only one byte
                if (transactionUnit.IsLastByte)
                {
                    //Must send stop and NAK after receiving last byte
                    ctrl.START = false;

                    //Initiate read from slave
                    ctrl.STOP = ctrl.ALDIE = ctrl.ACKNAK = ctrl.TB = true;
                }
                else //If we are expecting more than one byte
                {
                    //Do not send a stop and ACK the next received byte
                    ctrl.START = ctrl.STOP = ctrl.ACKNAK = false;

                    //Initiate read from slave
                    ctrl.ALDIE = ctrl.TB = true;
                }

                //Initiate the read operation
                i2c.ICR = ctrl;
            }
            else //If we have just finished sending address or data in Write mode 
            {
                //if the data transfer is complete
                if (transactionUnit.IsTransferComplete)
                {
                    //Clear the stop bit
                    i2c.ICR.STOP = false;

                    if (transaction.IsProcessingLastUnit)
                    {
                        StopTransactionAsMaster();

                        //signal the succesfull complection
                        transaction.Signal(I2CBus_Transaction.CompletionStatus.Completed);

                        //Shut down I2C port master
                        i2c.ICR.IUE = false;
                    }
                    else
                    {
                        StartTransactionAsMaster(transaction /*, true */ );
                    }
                }
                else if (status.BED) //if there has been a bus error detected
                {
                    //Clear the bus error
                    i2c.ISR.BED = true;

                    //and abort the transaction
                    StopTransactionAsMaster();

                    //signal the abortion
                    transaction.Signal(I2CBus_Transaction.CompletionStatus.Aborted);

                    //Shut down I2C master
                    i2c.ICR.IUE = false;
                }
                else //in the middle of the send operation
                {
                    //If sending the last byte we must signal
                    //the stop condition after the byte was sent
                    if (transactionUnit.IsLastByte)
                    {
                        ctrl.START = false;
                        ctrl.STOP = true;
                    }
                    else
                    {
                        ctrl.START = false;
                        ctrl.STOP = false;
                    }

                    //Set up start/stop and arbitration loss detect
                    //after the operation has started
                    ctrl.ALDIE = true;
                    i2c.ICR = ctrl;

                    //Set up next byte to transmit
                    //Get data ready to send and put it
                    //into the send register
                    i2c.IDBR = transactionUnit.Pop();

                    //Initiate the transmission
                    i2c.ICR.TB = true;
                }
            }
        }
        #endregion

        #region Singleton API
#if TESTMODE_DESKTOP
        private static I2C s_instance;
        /// <summary>
        /// Retrieves the I2C driver singleton.
        /// </summary>
        public static I2C Instance
        {
            get
            {
                if (null != s_instance) return s_instance;
                s_instance = new I2C();
                s_instance.Initialize();
                return s_instance;
            }
        }
#else
        /// <summary>
        /// Retrieves the I2C driver singleton.
        /// </summary>
        extern public static I2C Instance
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            [SingletonFactory]
            get;
        }
#endif
        #endregion

    }


    /// <summary>
    /// Base class for an I2C Transaction.
    /// </summary>
    public class I2CBus_TransactionUnit
    {

        #region Constructor/Destructor
        /// <summary>
        /// Initializes an I2C transaction
        /// </summary>
        /// <param name="isReadTransaction">True, if the data is read from the device. False if
        /// data it to be written to the device.</param>
        /// <param name="buffer">The buffer to fill from (when reading) the device
        /// or transfer to (when writing) the device.</param>
        protected I2CBus_TransactionUnit(bool isReadTransaction, byte[] buffer)
        {
            if (null == buffer)
                throw new ArgumentNullException("buffer");

            this.Buffer = buffer;
            this.m_isReadTransactionUnit = isReadTransaction;
            //this.m_queueIndex = 0;
        }
        #endregion

        #region Public API
        /// <summary>
        /// The current index into the buffer
        /// used for Push/Pop operations.
        /// </summary>
        private int m_queueIndex;

        /// <summary>
        /// reset the internal state of this transaction
        /// </summary>
        internal void ResetTransactionUnit()
        {
            this.m_queueIndex = 0;
        }

        /// <summary>
        /// The buffer to fill from (when reading) the device
        /// or transfer to (when writing) the device.
        /// </summary>
        public readonly byte[] Buffer;
        #endregion

        #region Internal API
        /// <summary>
        /// Indicates the direction of the transaction
        /// </summary>
        private bool m_isReadTransactionUnit;

        /// <summary>
        /// Indicates the direction of the transaction
        /// </summary>
        /// <returns>
        /// If true, the transaction is a read transaction, i.e.,
        /// data is read from the device and written to the buffer.
        /// If false, this is a write transaction, i.e., 
        /// data is read from the buffer and written to the device.
        /// </returns>
        internal bool IsReadTransaction()
        {
            return m_isReadTransactionUnit;
        }

        /// <summary>
        /// Returns the next byte to send to the device
        /// and adjusts the internal pointer to the next byte.
        /// </summary>
        /// <remarks>
        /// note: An exception will be raised if the transaction
        ///       is not a write transaction.
        /// </remarks>
        /// <returns>The next byte to send to the device.</returns>
        internal byte Pop()
        {
            if (m_isReadTransactionUnit)
                throw new ArgumentOutOfRangeException();

            byte ret = this.Buffer[m_queueIndex];
            m_queueIndex++;
            return ret;
        }

        /// <summary>
        /// Pushes the data read from the device to the internal
        /// buffer and adjusts the internal pointer to the next byte.
        /// </summary>
        /// <remarks>
        /// note: An exception will be raised if the transaction
        ///       is not a read transaction.
        /// </remarks>
        /// <param name="val">The data read from the device to be
        /// pushed to the buffer.</param>
        internal void Push(byte val)
        {
            if (!m_isReadTransactionUnit)
                throw new ArgumentOutOfRangeException("val");

            this.Buffer[m_queueIndex] = val;
            m_queueIndex++;
        }

        /// <summary>
        /// Indicates if the transaction is complete,
        /// i.e. all data has been sent/received.
        /// </summary>
        internal bool IsTransferComplete
        {
            get
            {
                return m_queueIndex == this.Buffer.Length;
            }
        }

        /// <summary>
        /// Indicates if the last byte of the transaction
        /// is to be processed.
        /// </summary>
        /// <remarks>
        /// note: the last byte is special in that the I2C
        ///       bus requires special signalling for the
        ///       last byte.
        /// </remarks>
        internal bool IsLastByte
        {
            get
            {
                return m_queueIndex == this.Buffer.Length - 1;
            }
        }
        #endregion

    }


    /// <summary>
    /// Represents a comlex I2C transaction consisting
    /// of several <see cref="I2C_HAL_TransactionUnit"/> elements.
    /// </summary>
    public class I2CBus_Transaction : IDisposable
    {

        #region Constructor/Destructor
        /// <summary>
        /// Initializes a complex transaction from a set of user transactions for
        /// a given peer address and clock rate.
        /// </summary>
        /// <param name="address">The address of the peer to talk to.</param>
        /// <param name="clockRate">The clock rate (either 100 or 400khz) to use as transmission speed.</param>
        /// <param name="units">The sequence of user transaction to run against the given peer.</param>
        public I2CBus_Transaction(ushort address, int clockRate, I2CBus_TransactionUnit[] units)
        {
            m_completed = new ManualResetEvent(false);
            m_status = CompletionStatus.Aborted;
            m_address = address;
            m_clockRate = clockRate;
            m_transactionUnits = units;
            //m_bytesTransacted = 0;
            //m_current = 0;
        }
        #endregion

        #region Public API
        /// <summary>
        /// Waits for the transaction to complete for a specified amount of time.
        /// </summary>
        /// <param name="milliseconds">The maximum time to wait for the transaction to complete.</param>
        /// <returns>True, if the transaction is complete, false, if the transaction is still
        /// running.
        /// note: having a transaction completed does not means that it was successfull.
        ///       Look at <see cref="BytesTransacted"/> to see how many bytes have been successfully
        ///       transacted.</returns>
        /// <seealso cref="BytesTransacted"/>
        public bool WaitForCompletion(int milliseconds)
        {
            return m_completed.WaitOne(milliseconds, false);
        }

        /// <summary>
        /// The number of bytes successfully sent/received.
        /// </summary>
        public int BytesTransacted
        {
            get
            {
                return m_bytesTransacted;
            }
            internal set
            {
                m_bytesTransacted = value;
            }
        }
        /// <summary>
        /// The address of the peer.
        /// </summary>
        public ushort Address
        {
            get
            {
                return m_address;
            }
        }

        /// <summary>
        /// The clockrate at which to talk to the client.
        /// </summary>
        public int ClockRate
        {
            get
            {
                return m_clockRate;
            }
        }

        /// <summary>
        /// If true, this is the last user transaction.
        /// </summary>
        public bool IsProcessingLastUnit
        {
            get
            {
                return (null == m_transactionUnits || m_current >= m_transactionUnits.Length);
            }
        }

        /// <summary>
        /// The completion status of this transaction.
        /// </summary>
        public CompletionStatus TransactionState
        {
            get { return m_status; }
        }
        #endregion

        /// <summary>
        /// resets the state of this transaction
        /// </summary>
        internal void ResetTransaction() {
            m_completed.Reset();
            m_status = CompletionStatus.Aborted;
            m_bytesTransacted = 0;
            m_current = 0;

            foreach (I2CBus_TransactionUnit unit in m_transactionUnits)
            {
                unit.ResetTransactionUnit();
            }
        }

        #region Internal API
        private ManualResetEvent m_completed;
        private I2CBus_TransactionUnit[] m_transactionUnits;
        private int m_bytesTransacted;
        private ushort m_address;
        private int m_clockRate;
        private CompletionStatus m_status;
        private int m_current;

        /// <summary>
        /// Pops the next user transaction from the transaction queue
        /// and adjusts the internal pointer to the next transaction.
        /// </summary>
        /// <returns>The next user transaction to execute.</returns>
        internal I2CBus_TransactionUnit Pop()
        {
            if (m_current > 0)
                BytesTransacted += m_transactionUnits[m_current - 1].Buffer.Length;

            return m_transactionUnits[m_current++];
        }

        /// <summary>
        /// Signal handler called by the I2C state machine to signal the end of the transactions.
        /// </summary>
        /// <param name="status">The result status of the user transactions.</param>
        internal void Signal(CompletionStatus status)
        {
            if (status == CompletionStatus.Completed)
            {
                m_bytesTransacted = 0;
                for (int i = 0; i < m_transactionUnits.Length; i++)
                {
                    m_bytesTransacted += m_transactionUnits[i].Buffer.Length;
                }
            }

            m_status = status;
            m_completed.Set();
        }
        #endregion

        #region Nested data types
        /// <summary>
        /// The completion status of the entire
        /// transaction.
        /// </summary>
        public enum CompletionStatus
        {
            /// <summary>
            /// The transaction was aborted at
            /// some point. Some of the user transactions
            /// have possibly been completed, some possibly not.
            /// </summary>
            Aborted,
            /// <summary>
            /// All user transaction that were part of the combined
            /// transaction completed successfully.
            /// </summary>
            Completed
        };
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Disposes all resources held by the Transaction
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all resources held by the Transaction
        /// </summary>
        /// <param name="fDisposing">True, if called through Dispose(). False otherwise.</param>
        protected virtual void Dispose(bool fDisposing)
        {
            if (m_completed != null)
                m_completed.Close();
            m_completed = null;
        }
        #endregion

    }

}
