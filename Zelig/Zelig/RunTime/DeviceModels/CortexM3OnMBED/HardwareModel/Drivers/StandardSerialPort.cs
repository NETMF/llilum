//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    using RT = Microsoft.Zelig.Runtime;

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


        private unsafe SerialObj* m_serial;
        private bool m_disposed;

        //
        // Constructor Methods
        //

        internal StandardSerialPort(ref Configuration cfg    //,
                                                             //StandardUART.Id portNo ,
                                                             /*int             irq     */) : base(ref cfg)
        {
            m_disposed = false;

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
        }

        public override void Close()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //--//

        public override void Write(byte[] array, int offset, int count, int timeout)
        {
            base.Write(array, offset, count, timeout);

            using (RT.SmartHandles.InterruptState.Disable())
            {
                WriteData();
            }
            //for (int pos = 0; pos < count; pos++)
            //{
            //    if (tmp_serial_writable(m_serial))
            //    {
            //        tmp_serial_putc(m_serial, (int)array[offset++]);
            //    }
            //    else
            //    {
            //        pos--;
            //    }
            //}
        }

        public override void WriteByte(byte value, int timeout)
        {
            base.WriteByte(value, timeout);

            using (RT.SmartHandles.InterruptState.Disable())
            {
                WriteData();
            }
        }

        public override void Flush()
        {
            using (RT.SmartHandles.InterruptState.Disable())
            {
                WriteData();
            }

            base.Flush();
        }

        //--//

        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
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

        private unsafe void WriteData()
        {
            while (true)
            {
                if (tmp_serial_writable(m_serial))
                {
                    byte tx;
                    if (!m_transmitQueue.DequeueNonblocking(out tx))
                    {
                        break;
                    }
                    tmp_serial_putc(m_serial, (int)tx);
                }
            }
        }

        public override int Read(byte[] array, int offset, int count, int timeout)
        {
            int got = 0;
            
            while (got < count)
            {
                using (RT.SmartHandles.InterruptState.Disable())
                {
                    byte val;
                    
                    ReadData();

                    //if (m_receiveQueue.DequeueBlocking(timeout, out val) == false)
                    if (m_receiveQueue.DequeueNonblocking(out val))
                    //{
                    //    //if (timeout != 0)
                    //    //{
                    //    //    throw new TimeoutException();
                    //    //}

                    //    break;
                    //}
                    //else
                    {
                        array[offset] = val;
                        offset++;
                        got++;
                    }

                }
            }

            return got;
        }

        public override int ReadByte(int timeout)
        {
            int got = 0;

            while (got < 1)
            {
                using (RT.SmartHandles.InterruptState.Disable())
                {
                    ReadData();

                    byte val;
                    //if (m_receiveQueue.DequeueBlocking(timeout, out val) == false)
                    if (!m_receiveQueue.DequeueNonblocking(out val))
                    {
                        //if (timeout != 0)
                        //{
                        //    throw new TimeoutException();
                        //}

                        break;
                    }
                    else
                    {
                        got = val;
                    }
                }
            }

            return got;
        }

        private unsafe void ReadData()
        {
            while (tmp_serial_readable(m_serial) && !m_receiveQueue.IsFull)
            {
                byte rx = (byte)tmp_serial_getc(m_serial);

                if (!m_receiveQueue.EnqueueNonblocking(rx))
                {
                    m_stat_OverrunErrors++;
                }
            }
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
                ReadData();
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
