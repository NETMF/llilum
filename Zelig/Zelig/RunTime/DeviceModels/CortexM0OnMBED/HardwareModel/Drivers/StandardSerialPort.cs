//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM0OnMBED.HardwareModel
{
    using System;
    using System.Threading;

    using RT = Zelig.Runtime;
    using TS = Zelig.Runtime.TypeSystem;
    using LLIO = Zelig.LlilumOSAbstraction.API.IO;
    using LLOS = Zelig.LlilumOSAbstraction;

    public class StandardSerialPort : RT.BaseSerialStream
    {
        //
        // State
        //

        private int m_stat_OverrunErrors;
        private AutoResetEvent m_evtTx;

        private unsafe LLIO.SerialPortContext* m_serial;
        private unsafe LLIO.SerialPortConfiguration* m_serialCfg;
        private bool m_disposed;
        private bool m_shutdown;

        //
        // Constructor Methods
        //

        internal StandardSerialPort(ref Configuration cfg    //,
                                                             //StandardUART.Id portNo ,
                                                             /*int             irq     */) : base(ref cfg)
        {
            m_disposed = false;
            m_shutdown = false;

            // Get pins from Board config
            int txPin;
            int rxPin;
            int ctsPin;
            int rtsPin;

            if (!HardwareProvider.Instance.GetSerialPinsFromPortName(cfg.PortName, out txPin, out rxPin, out rtsPin, out ctsPin))
            {
                throw new ArgumentException();
            }

            if (!HardwareProvider.Instance.TryReservePins(txPin, rxPin, ctsPin, rtsPin))
            {
                throw new ArgumentException();
            }

            try
            {
                // Set pins onto BaseSerialStream Configuration
                m_cfg.TxPin = txPin;
                m_cfg.RxPin = rxPin;
                m_cfg.CtsPin = ctsPin;
                m_cfg.RtsPin = rtsPin;

                int invalidPin = HardwareProvider.Instance.InvalidPin;
                if (invalidPin != rtsPin)
                {
                    m_cfg.RtsEnable = true;
                    m_cfg.Handshake = System.IO.Ports.Handshake.RequestToSend;
                }
                if (invalidPin != ctsPin)
                {
                    m_cfg.CtsEnable = true;
                    m_cfg.Handshake = System.IO.Ports.Handshake.RequestToSend;
                }

                m_evtTx = new AutoResetEvent(false);
            }
            catch
            {
                HardwareProvider.Instance.ReleasePins(txPin, rxPin, ctsPin, rtsPin);
                throw;
            }
        }

        //
        // Helper Methods
        //

        internal unsafe void Open()
        {
            fixed (LLIO.SerialPortContext** ppSerial = &m_serial)
            fixed( LLIO.SerialPortConfiguration** ppCfg = &m_serialCfg)
            {
                LLOS.LlilumErrors.ThrowOnError( LLIO.SerialPort.LLOS_SERIAL_Open(m_cfg.RxPin, m_cfg.TxPin, ppCfg, ppSerial), true );
            }

            m_serialCfg->BaudRate = (uint)m_cfg.BaudRate;
            m_serialCfg->DataBits = (uint)m_cfg.DataBits;
            m_serialCfg->Parity   = (LLIO.SerialPortParity)m_cfg.Parity;
            m_serialCfg->StopBits = (LLIO.SerialPortStopBits)m_cfg.StopBits;

            LLIO.SerialPort.LLOS_SERIAL_Configure( m_serial, m_serialCfg );

            if (m_cfg.RtsEnable || m_cfg.CtsEnable)
            {
                // Splitting Rts/Cts into separate modes is mBed specific
                int pinRts = -1;
                int pinCts = -1;
                
                if (m_cfg.RtsEnable)
                {
                    pinRts = m_cfg.RtsPin;
                }

                if (m_cfg.CtsEnable)
                {
                    pinCts = m_cfg.CtsPin;
                }

                LLIO.SerialPort.LLOS_SERIAL_SetFlowControl(m_serial, pinRts, pinCts );
            }

            LLIO.SerialPort.LLOS_SERIAL_SetCallback( m_serial, HandleSerialPortInterrupt, ((RT.ObjectImpl)(object)this).ToPointer() );
            LLIO.SerialPort.LLOS_SERIAL_Enable( m_serial, LLIO.SerialPortIrq.IrqRx );
            LLIO.SerialPort.LLOS_SERIAL_Disable( m_serial, LLIO.SerialPortIrq.IrqTx );
        }

        public override void Close()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //--//

        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                m_shutdown = true;

                // signal write thread to exit
                m_evtTx.Set();

                //
                // Send a dummy character in case the write thread is waiting on the transmit queue.
                //
                using (RT.SmartHandles.InterruptState.Disable())
                {
                    m_transmitQueue.EnqueueNonblocking(0);
                }

                unsafe
                {
                    LLIO.SerialPort.LLOS_SERIAL_Disable( m_serial, LLIO.SerialPortIrq.IrqBoth );
                }


                if (disposing)
                {
                    HardwareProvider.Instance.ReleasePins(m_cfg.TxPin, m_cfg.RxPin, m_cfg.CtsPin, m_cfg.RtsPin);
                }
                
                unsafe
                {
                    LLIO.SerialPort.LLOS_SERIAL_Close(m_serial);
                }

                m_disposed = true;
            }

            base.Dispose(disposing);
        }

        ~StandardSerialPort()
        {
            Dispose(false);
        }

        //--//

        public override int ReadByte(int timeout)
        {
            SignalRead();

            return base.ReadByte(timeout);
        }

        public override int Read(byte[] array, int offset, int count, int timeout)
        {
            SignalRead();

            return m_receiveQueue.DequeueMultipleBlocking(ref array, offset, count, timeout);
        }

        public override void WriteByte(byte value, int timeout)
        {
            WriteToTx(value, timeout);
        }

        public override void Write(byte[] array, int offset, int count, int timeout)
        {
            int end = offset + count;

            if( offset + count > array.Length )
            {
                throw new ArgumentException();
            }

            while (offset < end && !m_shutdown)
            {
                WriteToTx(array[offset++], timeout);
            }
        }

        public override void Flush()
        {
            //
            // This implementation of the serial port does not use a write buffer because it would require
            // a separate write thread.  Therefore, the transmit queue from the base class should be empty.
            //
            RT.BugCheck.Assert(m_transmitQueue.IsEmpty, RT.BugCheck.StopCode.IllegalConfiguration);
        }

        //
        // Access Methods
        //

        public override int BaudRate
        {
            set
            {
                m_cfg.BaudRate = value;
                unsafe
                {
                    m_serialCfg->BaudRate = (uint)value;
                    LLIO.SerialPort.LLOS_SERIAL_Configure(m_serial, m_serialCfg);
                }
            }
        }

        public override int BytesToRead
        {
            get
            {
                return base.BytesToRead;
            }
        }

        public override int DataBits
        {
            set
            {
                unsafe
                {
                    m_cfg.DataBits = value;
                    m_serialCfg->DataBits = (uint)value;
                    LLIO.SerialPort.LLOS_SERIAL_Configure(m_serial, m_serialCfg);
                }
            }
        }

        public override System.IO.Ports.Parity Parity
        {
            set
            {
                unsafe
                {
                    m_cfg.Parity = value;
                    m_serialCfg->Parity = (LLIO.SerialPortParity)value;
                    LLIO.SerialPort.LLOS_SERIAL_Configure(m_serial, m_serialCfg);
                }
            }
        }

        public override System.IO.Ports.StopBits StopBits
        {
            set
            {
                unsafe
                {
                    m_cfg.StopBits = value;
                    m_serialCfg->StopBits = (LLIO.SerialPortStopBits)value;
                    LLIO.SerialPort.LLOS_SERIAL_Configure(m_serial, m_serialCfg);
                }
            }
        }

        public int OverrunErrors
        {
            get
            {
                return m_stat_OverrunErrors;
            }
        }

        public unsafe override bool IsOpen
        {
            get
            {
                return m_serial != null;
            }
        }

        //
        // Helper functions
        //

        private unsafe void WriteToTx(byte val, int timeout)
        {
            while (!m_shutdown)
            {
                uint isWritable = 1;

                using (RT.SmartHandles.InterruptState.Disable())
                {
                    LLIO.SerialPort.LLOS_SERIAL_CanWrite( m_serial, &isWritable );

                    if (isWritable != 0)
                    {
                        LLIO.SerialPort.LLOS_SERIAL_Write(m_serial, &val, 0, 1);
                        break;
                    }
                    else
                    {
                        //
                        // Enable TX interrupt to wake us up from m_evtTx.WaitOne() below.
                        //
                        LLIO.SerialPort.LLOS_SERIAL_Enable(m_serial, LLIO.SerialPortIrq.IrqTx);
                    }
                }

                if (isWritable != 0)
                {
                    if (!m_evtTx.WaitOne(timeout, false))
                    {
                        if (timeout == 0)
                        {
                            break;
                        }
                        else
                        {
                            throw new TimeoutException();
                        }
                    }
                }
            }
        }

        //
        // This function kick starts the read if the read buffer was previously filled
        // while there was still data in the hardware buffer.  If this is not done, we
        // could potentially be leaving data in the hardware and not receiving an RX
        // interrupt.
        //
        private void SignalRead()
        {
            if (m_receiveQueue.IsEmpty)
            {
                using (RT.SmartHandles.InterruptState.Disable())
                {
                    ReadData();
                }
            }
        }


        private unsafe void ReadData()
        {
            RT.BugCheck.AssertInterruptsOff();

            while (true)
            {
                uint canRead = 1;
                LLIO.SerialPort.LLOS_SERIAL_CanRead( m_serial, &canRead );

                if ((canRead == 0) || m_receiveQueue.IsFull)
                {
                    break;
                }
                else
                {
                    byte c;
                    int length = 1;

                    LLOS.LlilumErrors.ThrowOnError( LLIO.SerialPort.LLOS_SERIAL_Read( m_serial, &c, 0, &length), true );

                    if (!m_receiveQueue.EnqueueNonblocking(c))
                    {
                        m_stat_OverrunErrors++;
                        break;
                    }
                }
            }
        }

        [TS.GenerateUnsafeCast()]
        private extern static StandardSerialPort CastAsSerialPort(UIntPtr ptr);

        private static unsafe void HandleSerialPortInterrupt(LLIO.SerialPortContext* port, UIntPtr callbackCtx, LLIO.SerialPortEvent serialEvent)
        {
            StandardSerialPort sp = CastAsSerialPort(callbackCtx);

            using (RT.SmartHandles.InterruptState.Disable())
            {
                if (serialEvent == LLIO.SerialPortEvent.Rx)
                {
                    sp.ReadData();
                }
                else
                {
                    //
                    // Disable TX interrupts so that we can process the TX
                    // buffer without being interrupted by TX events.
                    //
                    unsafe
                    {
                        LLIO.SerialPort.LLOS_SERIAL_Disable(sp.m_serial, LLIO.SerialPortIrq.IrqTx);
                    }

                    sp.m_evtTx.Set();
                }
            }
        }
    }
}
