//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    using RT = Zelig.Runtime;
    using TS = Zelig.Runtime.TypeSystem;

    public sealed class StandardSerialPort : RT.BaseSerialStream
    {
        //
        // State
        //

        //private StandardUART.Id             m_portNo;
        //private StandardUART.Port           m_port;
        //private InterruptController.Handler m_interrupt;
        //private int                         m_fifoTxLevel;

        private int m_stat_OverrunErrors;
        //private int                         m_stat_FramingErrors;
        //private int                         m_stat_FIFOErrors;
        private AutoResetEvent                 m_evtTx;


        private unsafe SerialObj* m_serial;
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

        private unsafe void EnableInterrupt(SerialPortInterruptHandler callback)
        {
            using (RT.SmartHandles.InterruptState.Disable())
            {
                //
                // TODO: If GC compaction is ever supported, we will need to
                // come up with way to handle this differently.  Otherwise,
                // the object might move while its address is referenced in 
                // native code.
                //
                UIntPtr hndPtr = ((RT.ObjectImpl)(object)callback).ToPointer();

                tmp_serial_set_irq_handler(m_serial, (uint)hndPtr);
                tmp_serial_irq_set(m_serial, (uint)MbedSerialPortIrq.Rx, 1);
                tmp_serial_irq_set(m_serial, (uint)MbedSerialPortIrq.Tx, 0);
            }
        }

        private unsafe void DisableInterrupt()
        {
            using (RT.SmartHandles.InterruptState.Disable())
            {
                tmp_serial_irq_set(m_serial, (uint)MbedSerialPortIrq.Rx, 0);
                tmp_serial_irq_set(m_serial, (uint)MbedSerialPortIrq.Tx, 0);
            }
        }

        //
        // Helper Methods
        //

        internal unsafe void Open()
        {
            // TODO: Consider combining alloc and init methods
            fixed (SerialObj** serialTemp = &m_serial)
            {
                tmp_serial_alloc(serialTemp);
            }
            tmp_serial_init(m_serial, m_cfg.TxPin, m_cfg.RxPin);
            tmp_serial_baud(m_serial, m_cfg.BaudRate);
            tmp_serial_format(m_serial, m_cfg.DataBits, (int)m_cfg.Parity, (int)m_cfg.StopBits);

            if (m_cfg.RtsEnable || m_cfg.CtsEnable)
            {
                // Splitting Rts/Cts into separate modes is mBed specific
                FlowControlMode mode = FlowControlMode.None;
                if (m_cfg.RtsEnable && m_cfg.CtsEnable)
                {
                    mode = FlowControlMode.RtsCts;
                }
                else
                {
                    mode = m_cfg.RtsEnable ? FlowControlMode.Rts : FlowControlMode.Cts;
                }

                tmp_serial_set_flow_control(m_serial, (int)mode, m_cfg.RtsPin, m_cfg.CtsPin);
            }

            EnableInterrupt(HandleSerialPortInterrupt);
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

                DisableInterrupt();

                if (disposing)
                {
                    HardwareProvider.Instance.ReleasePins(m_cfg.TxPin, m_cfg.RxPin, m_cfg.CtsPin, m_cfg.RtsPin);
                }
                
                unsafe
                {
                    tmp_serial_free(m_serial);
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
                    tmp_serial_baud(m_serial, value);
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
                    tmp_serial_format(m_serial, m_cfg.DataBits, (int)m_cfg.Parity, (int)m_cfg.StopBits);
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
                    tmp_serial_format(m_serial, m_cfg.DataBits, (int)m_cfg.Parity, (int)m_cfg.StopBits);
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
                    tmp_serial_format(m_serial, m_cfg.DataBits, (int)m_cfg.Parity, (int)m_cfg.StopBits);
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
                bool isWritable = false;

                using (RT.SmartHandles.InterruptState.Disable())
                {
                    if (isWritable = tmp_serial_writable(m_serial))
                    {
                        tmp_serial_putc(m_serial, val);
                        break;
                    }
                    else
                    {
                        //
                        // Enable TX interrupt to wake us up from m_evtTx.WaitOne() below.
                        //
                        tmp_serial_irq_set(m_serial, (uint)MbedSerialPortIrq.Tx, 1);
                    }
                }

                if (isWritable)
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

            while (tmp_serial_readable(m_serial))
            {
                if (!m_receiveQueue.IsFull)
                {
                    if (!m_receiveQueue.EnqueueNonblocking((byte)tmp_serial_getc(m_serial)))
                    {
                        m_stat_OverrunErrors++;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }


        private void HandleSerialPortInterrupt(MbedSerialPortIrq irq)
        {
            using (RT.SmartHandles.InterruptState.Disable())
            {
                if (irq == MbedSerialPortIrq.Rx)
                {
                    ReadData();
                }
                else
                {
                    //
                    // Disable TX interrupts so that we can process the TX
                    // buffer without being interrupted by TX events.
                    //
                    unsafe
                    {
                        tmp_serial_irq_set(m_serial, (uint)irq, 0);
                    }

                    m_evtTx.Set();
                }
            }
        }

        //--//

        private enum MbedSerialPortIrq
        {
            Rx = 0,
            Tx = 1,
        }

        private delegate void SerialPortInterruptHandler(MbedSerialPortIrq irq);

        [TS.GenerateUnsafeCast()]
        private extern static SerialPortInterruptHandler CastAsSerialPortInterruptHandler(UIntPtr ptr);

        [RT.ExportedMethod]
        private static void HandleSerialPortInterrupt(uint id, MbedSerialPortIrq evt)
        {
            SerialPortInterruptHandler hnd = CastAsSerialPortInterruptHandler((UIntPtr)id);

            if( hnd != null )
            {
                hnd(evt);
            }
        }

        [DllImport("C")]
        private static unsafe extern void tmp_serial_alloc(SerialObj** obj);
        [DllImport("C")]
        private static unsafe extern void tmp_serial_init(SerialObj* obj, int txPin, int rxPin);
        [DllImport("C")]
        private static unsafe extern void tmp_serial_free(SerialObj* obj);
        [DllImport("C")]
        private static unsafe extern void tmp_serial_baud(SerialObj* obj, int baudRate);
        [DllImport("C")]
        private static unsafe extern void tmp_serial_format(SerialObj* obj, int dataBits, int parity, int stopBits);
        [DllImport("C")]
        private static unsafe extern void tmp_serial_putc(SerialObj* obj, int c);
        [DllImport("C")]
        private static unsafe extern int tmp_serial_getc(SerialObj* obj);
        [DllImport("C")]
        private static unsafe extern void tmp_serial_set_flow_control(SerialObj* obj, int flowControlType, int rtsPin, int ctsPin);
        [DllImport("C")]
        private static unsafe extern bool tmp_serial_readable(SerialObj* obj);
        [DllImport("C")]
        private static unsafe extern bool tmp_serial_writable(SerialObj* obj);
        [DllImport("C")]
        private static unsafe extern void tmp_serial_set_irq_handler(SerialObj* obj, uint id);
        [DllImport("C")]
        private static unsafe extern void tmp_serial_irq_set(SerialObj* obj, uint irq, uint enable);
    }

    internal unsafe struct SerialObj
    {
    };

    enum FlowControlMode
    {
        None = 0,
        Rts,
        Cts,
        RtsCts,
    }
}
