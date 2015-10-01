//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.DeviceModels.Chipset.MM9691LP.Drivers
{
    using System;
    using System.Runtime.CompilerServices;

    using RT = Microsoft.Zelig.Runtime;


    internal class SerialPort : RT.BaseSerialStream
    {
        //
        // State
        //

        private int                         m_usartNum;
        private Chipset.MM9691LP.USARTx     m_core;
        private InterruptController.Handler m_interruptRx;
        private InterruptController.Handler m_interruptTx;

        //
        // Constructor Methods
        //

        internal SerialPort( ref Configuration cfg      ,
                                 int           usartNum ) : base( ref cfg )
        {
            m_usartNum = usartNum;

            uint maskRX;
            uint maskTX;

            if(usartNum == 0)
            {
                maskRX = MM9691LP.INTC.IRQ_MASK_USART0_Rx;
                maskTX = MM9691LP.INTC.IRQ_MASK_USART0_Tx;
            }
            else
            {
                maskRX = MM9691LP.INTC.IRQ_MASK_USART1_Rx;
                maskTX = MM9691LP.INTC.IRQ_MASK_USART1_Tx;
            }

            m_interruptRx = InterruptController.Handler.Create( maskRX, ProcessRX );
            m_interruptTx = InterruptController.Handler.Create( maskTX, ProcessTX );
        }

        //
        // Helper Methods
        //

        internal void Open()
        {
            if(m_usartNum == 0)
            {
                m_core = MM9691LP.USART0.Initialize( ref m_cfg );
            }
            else
            {
                m_core = MM9691LP.USART1.Initialize( ref m_cfg );
            }

            if(m_core == null)
            {
                throw new ArgumentException();
            }

            using(RT.SmartHandles.InterruptState.Disable())
            {
                InterruptController.Instance.RegisterAndEnable( m_interruptRx );
                InterruptController.Instance.Register         ( m_interruptTx );
            }
        }

        public override void Close()
        {
            if(m_core != null)
            {
                using(RT.SmartHandles.InterruptState.Disable())
                {
                    InterruptController.Instance.DeregisterAndDisable( m_interruptRx );
                    InterruptController.Instance.DeregisterAndDisable( m_interruptTx );
                }

                m_core = null;
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
                m_interruptTx.Enable();
            }
        }

        public override void WriteByte( byte value   ,
                                        int  timeout )
        {
            base.WriteByte( value, timeout );

            using(RT.SmartHandles.InterruptState.Disable())
            {
                m_interruptTx.Enable();
            }
        }

        //--//

        private void ProcessRX( InterruptController.Handler handler )
        {
            byte val = m_core.UnRBUF;

            if(m_receiveQueue.EnqueueNonblocking( val ) == false)
            {
                //
                // Overflow...
                //
            }
        }

        private void ProcessTX( InterruptController.Handler handler )
        {
            byte val;

            if(m_transmitQueue.DequeueNonblocking( out val ))
            {
                m_core.UnTBUF = val;
            }
            else
            {
                m_interruptTx.Disable();
            }
        }

        //--//


        public override bool IsOpen
        {
            get
            {
                return m_core != null;
            }
        }
    }
}
