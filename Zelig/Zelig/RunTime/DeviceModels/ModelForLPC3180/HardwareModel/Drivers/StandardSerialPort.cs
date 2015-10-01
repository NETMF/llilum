//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.DeviceModels.Chipset.LPC3180.Drivers
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    using RT = Microsoft.Zelig.Runtime;


    internal class StandardSerialPort : RT.BaseSerialStream
    {
        //
        // State
        //

        private StandardUART.Id             m_portNo;
        private StandardUART.Port           m_port;
        private InterruptController.Handler m_interrupt;
        private int                         m_fifoTxLevel;

        private int                         m_stat_OverrunErrors;
        private int                         m_stat_FramingErrors;
        private int                         m_stat_FIFOErrors;

        //
        // Constructor Methods
        //

        internal StandardSerialPort( ref Configuration   cfg    ,
                                         StandardUART.Id portNo ,
                                         int             irq    ) : base( ref cfg )
        {
            m_portNo    = portNo;
            m_interrupt = InterruptController.Handler.Create( irq, InterruptSettings.ActiveHigh, ProcessInterrupt );
        }

        //
        // Helper Methods
        //

        internal void Open( bool fAutoClock )
        {
            m_port = StandardUART.Instance.Configure( m_portNo, fAutoClock, ref m_cfg );

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
                var status = m_port.UnIIR;

                if(status.NoIntPending)
                {
                    return;
                }

                switch(status.IntId)
                {
                    case LPC3180.StandardUART.UnIIR_bitfield.InterruptType.RLS:
                        CheckForOverflow( m_port.UnLSR );
                        break;

                    case LPC3180.StandardUART.UnIIR_bitfield.InterruptType.CTI:
                    case LPC3180.StandardUART.UnIIR_bitfield.InterruptType.RDA:
                        CheckForOverflow( m_port.UnLSR );

                        ReadData();
                        break;

                    case LPC3180.StandardUART.UnIIR_bitfield.InterruptType.THRE:
                        WriteData();
                        break;
                }
            }
        }

        private void CheckForOverflow( StandardUART.UnLSR_bitfield val )
        {
            if(val.OE)
            {
                m_stat_OverrunErrors++;
            }

            if(val.FE)
            {
                m_stat_FramingErrors++;
            }

            if(val.FIFO_Rx_Error)
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
                    m_fifoTxLevel = 0;
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

                m_port.UnTHR = tx;

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
