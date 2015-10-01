//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x.Drivers
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    using RT = Microsoft.Zelig.Runtime;


    internal class SerialPort : RT.BaseSerialStream
    {
        //
        // State
        //

        private UART.Id                     m_portNo;
        private UART.Port                   m_port;
        private InterruptController.Handler m_interrupt;
        private int                         m_fifoTxLevel;

        private int                         m_stat_OverrunErrors;
        private int                         m_stat_FramingErrors;
        private int                         m_stat_FIFOErrors;

        //
        // Constructor Methods
        //

        internal SerialPort( ref Configuration cfg    ,
                                 UART.Id       portNo ,
                                 int           irq    ) : base( ref cfg )
        {
            m_portNo    = portNo;
            m_interrupt = InterruptController.Handler.Create( irq, InterruptPriority.Highest, InterruptSettings.ActiveHigh, ProcessInterrupt );
        }

        //
        // Helper Methods
        //

        internal void Open()
        {
            m_port = UART.Instance.Configure( m_portNo, ref m_cfg );

            if(m_port == null)
            {
                throw new ArgumentException();
            }

            m_port.EnableReceiveInterrupt ();
            m_port.EnableTransmitInterrupt();

            //--//

            using(RT.SmartHandles.InterruptState.Disable())
            {
                InterruptController.Instance.RegisterAndEnable( m_interrupt );
            }
        }

        public override void Close()
        {
            if(m_port != null)
            {
                using(RT.SmartHandles.InterruptState.Disable())
                {
                    InterruptController.Instance.DeregisterAndDisable( m_interrupt );
                }

                m_port.DisableTransmitInterrupt();
                m_port.DisableReceiveInterrupt ();

                m_port = null;
            }
        }

        //--//

        public override void Write( byte[] array   ,
                                    int    offset  ,
                                    int    count   ,
                                    int    timeout )
        {
            base.Write( array, offset, count, timeout );

            using(RT.SmartHandles.InterruptState.Disable())
            {
                WriteData();
            }
        }

        public override void WriteByte( byte value   ,
                                        int  timeout )
        {
            base.WriteByte( value, timeout );

            using(RT.SmartHandles.InterruptState.Disable())
            {
                WriteData();
            }
        }

        public override void Flush()
        {
            using(RT.SmartHandles.InterruptState.Disable())
            {
                WriteData();
            }

            base.Flush();
        }

        //--//

        private void ProcessInterrupt( InterruptController.Handler handler )
        {
            while(true)
            {
                var status = m_port.IIR;

                if(status.nIP)
                {
                    return;
                }

                switch(status.IID)
                {
                    case UART.IIR_bitfield.InterruptSource.RLS:
                        CheckForOverflow( m_port.LSR );
                        break;

                    case UART.IIR_bitfield.InterruptSource.CTI:
                    case UART.IIR_bitfield.InterruptSource.RDA:
                        CheckForOverflow( m_port.LSR );

                        ReadData();
                        break;

                    case UART.IIR_bitfield.InterruptSource.TFDR:
                        WriteData();
                        break;
                }
            }
        }

        private void CheckForOverflow( UART.LSR_bitfield val )
        {
            if(val.OE)
            {
                m_stat_OverrunErrors++;
            }

            if(val.FE)
            {
                m_stat_FramingErrors++;
            }

            if(val.FIFOE)
            {
                m_stat_FIFOErrors++;
            }
        }

        private void WriteData()
        {
            while(true)
            {
                //
                // FIFO empty? Reset level counter.
                //
                if(m_port.CanSend)
                {
                    //
                    // Transmitter empty register means FIFO is completely empty
                    //
                    if(m_port.LSR.TEMT == true)
                    {
                        m_fifoTxLevel = 0;
                    }
                    //
                    // Otherwise, FIFO is at least half empty. Reset to half if we are over.
                    //
                    else if(m_fifoTxLevel > 32)
                    {
                        m_fifoTxLevel = 32;
                    }
                }

                //
                // Write up to 64 characters in the FIFO before we have to wait for it to empty.
                //
                if(m_fifoTxLevel >= 64)
                {
                    break;
                }

                byte tx;

                if(m_transmitQueue.DequeueNonblocking( out tx ) == false)
                {
                    break;
                }

                m_port.THR = tx;

                m_fifoTxLevel++;
            }
        }

        private void ReadData()
        {
            byte rx;

            while(m_port.ReadByte( out rx ))
            {
                if(m_receiveQueue.EnqueueNonblocking( rx ) == false)
                {
                    m_stat_OverrunErrors++;
                }
            }
        }

        //
        // Access Methods
        //

        public int OverrunErrors
        {
            get
            {
                return m_stat_OverrunErrors;
            }
        }

        public override bool IsOpen
        {
	        get
	        {
	            return m_port != null;
	        }
        }

    }
}
