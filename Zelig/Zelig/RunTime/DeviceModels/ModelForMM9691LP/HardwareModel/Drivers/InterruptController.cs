//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.DeviceModels.Chipset.MM9691LP.Drivers
{
    using System;
    using System.Runtime.CompilerServices;

    using RT = Microsoft.Zelig.Runtime;


    public abstract class InterruptController
    {
        public delegate void Callback( Handler handler );

        public class Handler
        {
            //
            // State
            //

            private  uint                     m_mask;
            private  int                      m_index;
            private  Callback                 m_callback;
            internal RT.KernelNode< Handler > m_node;

            //
            // Constructor Methods
            //

            private Handler( uint     mask     ,
                             Callback callback )
            {
                m_mask     = mask;
                m_callback = callback;
            }

            private Handler( int      index    ,
                             Callback callback )
            {
                m_index    = index;
                m_callback = callback;
            }

            //
            // Helper Methods
            //

            public static Handler Create( uint     mask     ,
                                          Callback callback )
            {
                Handler hnd = new Handler( mask, callback );

                hnd.m_node = new RT.KernelNode< Handler >( hnd );

                return hnd;
            }

            public static Handler CreateFast( int      index    ,
                                              Callback callback )
            {
                Handler hnd = new Handler( index, callback );

                hnd.m_node = new RT.KernelNode< Handler >( hnd );

                return hnd;
            }

            public void Enable()
            {
                if(this.IsFastHandler)
                {
                    MM9691LP.INTC.Instance.Fiq.Select    = m_index;
                    MM9691LP.INTC.Instance.Fiq.EnableSet = 1u;
                }
                else
                {
                    MM9691LP.INTC.Instance.Irq.EnableSet = m_mask;
                }
            }

            public void Disable()
            {
                if(this.IsFastHandler)
                {
                    MM9691LP.INTC.Instance.Fiq.Select      = 0;
                    MM9691LP.INTC.Instance.Fiq.EnableClear = 1u;
                }
                else
                {
                    MM9691LP.INTC.Instance.Irq.EnableClear = m_mask;
                }
            }

            public void Invoke()
            {
                m_callback( this );
            }

            //
            // Access Methods
            //

            public uint Mask
            {
                get
                {
                    return m_mask;
                }
            }

            public int Index
            {
                get
                {
                    return m_index;
                }
            }

            public bool IsFastHandler
            {
                get
                {
                    return m_index != 0;
                }
            }
        }

        //
        // State
        //

        private RT.KernelList< Handler >    m_handlers;
        private Handler                     m_softInterrupt;
        private Handler                     m_fastInterrupt;
        private RT.Peripherals.Continuation m_softCallback;

        //
        // Helper Methods
        //

        public void Initialize()
        {
            m_handlers = new RT.KernelList< Handler >();

            MM9691LP.INTC.Instance.Irq.EnableClear = MM9691LP.INTC.IRQ_MASK_All;
            MM9691LP.INTC.Instance.Fiq.EnableClear = MM9691LP.INTC.IRQ_MASK_All;

            //--//

            m_softInterrupt = InterruptController.Handler.Create( MM9691LP.INTC.IRQ_MASK_Programmed_Interrupt, ProcessSoftInterrupt );

            Register( m_softInterrupt );

            m_softInterrupt.Enable();
        }

        public void RegisterAndEnable( Handler hnd )
        {
            Register( hnd );

            hnd.Enable();
        }

        public void Register( Handler hnd )
        {
            if(hnd.IsFastHandler)
            {
                m_fastInterrupt = hnd;
            }
            else
            {
                m_handlers.InsertAtTail( hnd.m_node );
            }
        }

        public void Deregister( Handler hnd )
        {
            if(hnd.IsFastHandler)
            {
                m_fastInterrupt = null;
            }
            else
            {
                hnd.m_node.RemoveFromList();
            }
        }

        public void DeregisterAndDisable( Handler hnd )
        {
            RT.BugCheck.AssertInterruptsOff();

            hnd.Disable();

            Deregister( hnd );
        }

        public void ProcessInterrupt()
        {
            while(true)
            {
                uint status = MM9691LP.INTC.Instance.Irq.Status;
                if(status == 0)
                {
                    break;
                }

                do
                {
                    RT.KernelNode< Handler > node = m_handlers.StartOfForwardWalk;

                    while(true)
                    {
                        if(node.IsValidForForwardMove == false)
                        {
                            //
                            // BUGBUG: Unhandled interrupts. We should crash. For now we just disable them.
                            //
                            MM9691LP.INTC.Instance.Irq.EnableClear = status;
                            break;
                        }

                        Handler hnd  = node.Target;
                        uint    mask = hnd.Mask;

                        if((status & mask) != 0)
                        {
                            status &= ~mask;

                            hnd.Invoke();
                            break;
                        }

                        node = node.Next;
                    }
                }
                while(status != 0);
            }
        }

        [RT.MemoryRequirements( RT.MemoryAttributes.RAM )]
        public void ProcessFastInterrupt()
        {
            while(true)
            {
                uint status = MM9691LP.INTC.Instance.Fiq.Status;
                if(status == 0)
                {
                    break;
                }

                Handler hnd = m_fastInterrupt;

                if(hnd == null)
                {
                    //
                    // BUGBUG: Unhandled interrupts. We should crash. For now we just disable them.
                    //
                    MM9691LP.INTC.Instance.Fiq.EnableClear = status;
                    break;
                }

                hnd.Invoke();
            }
        }

        public void CauseInterrupt()
        {
            MM9691LP.INTC.Instance.Irq.Soft = MM9691LP.INTC.IRQ_MASK_Programmed_Interrupt;
        }

        public void ContinueUnderNormalInterrupt( RT.Peripherals.Continuation dlg )
        {
            m_softCallback = dlg;

            CauseInterrupt();
        }

        private void ProcessSoftInterrupt( InterruptController.Handler handler )
        {
            MM9691LP.INTC.Instance.Irq.Soft = 0;

            RT.Peripherals.Continuation dlg = System.Threading.Interlocked.Exchange( ref m_softCallback, null );

            if(dlg != null)
            {
                dlg();
            }
        }

        //
        // Access Methods
        //

        public static extern InterruptController Instance
        {
            [RT.SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}
