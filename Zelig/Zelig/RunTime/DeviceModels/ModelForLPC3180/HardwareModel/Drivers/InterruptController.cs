//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.DeviceModels.Chipset.LPC3180.Drivers
{
    using System;
    using System.Runtime.CompilerServices;

    using RT = Microsoft.Zelig.Runtime;

    [Flags]
    public enum InterruptSettings
    {
        Normal             = 0x00000000,
        Fast               = 0x00000001,

        LevelSensitive     = 0x00000000,
        EdgeSensitive      = 0x00000002,

        ActiveLowOrFalling = 0x00000000,
        ActiveHighOrRising = 0x00000004,

        ActiveLow          = LevelSensitive | ActiveLowOrFalling,
        ActiveHigh         = LevelSensitive | ActiveHighOrRising,
        FallingEdge        = EdgeSensitive  | ActiveLowOrFalling,
        RisingEdge         = EdgeSensitive  | ActiveHighOrRising,
    }

    public abstract class InterruptController
    {
        public delegate void Callback( Handler handler );

        public class Handler
        {
            //
            // State
            //

            private  int                      m_index;
            private  int                      m_section;
            private  InterruptSettings        m_settings;
            private  Callback                 m_callback;
            internal RT.KernelNode< Handler > m_node;

            //
            // Constructor Methods
            //

            private Handler( int               index    ,
                             InterruptSettings settings ,
                             Callback          callback )
            {
                m_index    = index % 32;
                m_section  = index / 32;
                m_settings = settings;
                m_callback = callback;
            }

            //
            // Helper Methods
            //

            public static Handler Create( int               index    ,
                                          InterruptSettings settings ,
                                          Callback          callback )
            {
                Handler hnd = new Handler( index, settings, callback );

                hnd.m_node = new RT.KernelNode< Handler >( hnd );

                return hnd;
            }

            public void Enable()
            {
                LPC3180.INTC.Section ctrl = LPC3180.INTC.Instance.Sections[m_section];
                uint                 mask = this.Mask;

                if(this.IsEdgeSensitive)
                {
                    ctrl.ATR |= mask;
                }
                else
                {
                    ctrl.ATR &= ~mask;
                }

                if(this.IsActiveHighOrRising)
                {
                    ctrl.APR |= mask;
                }
                else
                {
                    ctrl.APR &= ~mask;
                }

                if(this.IsFastHandler)
                {
                    ctrl.ITR |= mask;
                }
                else
                {
                    ctrl.ITR &= ~mask;
                }

                ctrl.ER |= mask;
            }

            public void Disable()
            {
                LPC3180.INTC.Section ctrl = LPC3180.INTC.Instance.Sections[m_section];
                uint                 mask = this.Mask;

                ctrl.ER &= ~mask;
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
                [RT.Inline]
                get
                {
                    return 1U << m_index;
                }
            }

            public int Index
            {
                get
                {
                    return m_index;
                }
            }

            public int Section
            {
                get
                {
                    return m_section;
                }
            }

            public bool IsFastHandler
            {
                [RT.Inline]
                get
                {
                    return (m_settings & InterruptSettings.Fast) != 0;
                }
            }

            public bool IsEdgeSensitive
            {
                [RT.Inline]
                get
                {
                    return (m_settings & InterruptSettings.EdgeSensitive) != 0;;
                }
            }

            public bool IsActiveHighOrRising
            {
                [RT.Inline]
                get
                {
                    return (m_settings & InterruptSettings.ActiveHighOrRising) != 0;;
                }
            }
        }

        //
        // State
        //

        private RT.KernelList< Handler >    m_handlers;
        private Handler                     m_sub1Interrupt_IRQ;
        private Handler                     m_sub1Interrupt_FIQ;
        private Handler                     m_sub2Interrupt_IRQ;
        private Handler                     m_sub2Interrupt_FIQ;
        private Handler                     m_softInterrupt;
        private RT.Peripherals.Continuation m_softCallback;

        //
        // Helper Methods
        //

        public void Initialize()
        {
            m_handlers = new RT.KernelList< Handler >();

            LPC3180.INTC.Instance.Sections[0].ER = 0;
            LPC3180.INTC.Instance.Sections[1].ER = 0;
            LPC3180.INTC.Instance.Sections[2].ER = 0;

            //--//

            m_sub1Interrupt_IRQ = InterruptController.Handler.Create( LPC3180.INTC.IRQ_INDEX_Sub1IRQn, InterruptSettings.ActiveLow , ProcessSub1Interrupt );
            m_sub1Interrupt_FIQ = InterruptController.Handler.Create( LPC3180.INTC.IRQ_INDEX_Sub1FIQn, InterruptSettings.ActiveLow , ProcessSub1Interrupt );
            m_sub2Interrupt_IRQ = InterruptController.Handler.Create( LPC3180.INTC.IRQ_INDEX_Sub2IRQn, InterruptSettings.ActiveLow , ProcessSub2Interrupt );
            m_sub2Interrupt_FIQ = InterruptController.Handler.Create( LPC3180.INTC.IRQ_INDEX_Sub2FIQn, InterruptSettings.ActiveLow , ProcessSub2Interrupt );
            m_softInterrupt     = InterruptController.Handler.Create( LPC3180.INTC.IRQ_INDEX_SW_INT  , InterruptSettings.ActiveHigh, ProcessSoftInterrupt );

            RegisterAndEnable( m_sub1Interrupt_IRQ );
            RegisterAndEnable( m_sub1Interrupt_FIQ );
            RegisterAndEnable( m_sub2Interrupt_IRQ );
            RegisterAndEnable( m_sub2Interrupt_FIQ );
            RegisterAndEnable( m_softInterrupt     );
        }

        //--//

        public void RegisterAndEnable( Handler hnd )
        {
            Register( hnd );

            hnd.Enable();
        }

        public void Register( Handler hnd )
        {
            RT.BugCheck.AssertInterruptsOff();

            m_handlers.InsertAtTail( hnd.m_node );
        }

        //--//

        public void DeregisterAndDisable( Handler hnd )
        {
            RT.BugCheck.AssertInterruptsOff();

            hnd.Disable();

            Deregister( hnd );
        }

        public void Deregister( Handler hnd )
        {
            RT.BugCheck.AssertInterruptsOff();

            hnd.m_node.RemoveFromList();
        }

        //--//

        public void ProcessInterrupt()
        {
            ProcessSection( LPC3180.INTC.Instance.Sections[0], 0 );
        }

        private void ProcessSection( LPC3180.INTC.Section ctrl    ,
                                     int                  section )
        {
            while(true)
            {
                uint status = ctrl.SR;
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
                            ctrl.ER &= ~status;
                            break;
                        }

                        Handler hnd  = node.Target;

                        if(hnd.Section == section)
                        {
                            uint mask = hnd.Mask;

                            if((status & mask) != 0)
                            {
                                status &= ~mask;

                                hnd.Invoke();
                                break;
                            }
                        }

                        node = node.Next;
                    }
                }
                while(status != 0);
            }
        }

////    [RT.MemoryRequirements( RT.MemoryAttributes.Unpaged )]
        public void ProcessFastInterrupt()
        {
            ProcessInterrupt();
        }

        public void CauseInterrupt()
        {
            var val = new SystemControl.SW_INT_bitfield();

            val.Raise = true;

            LPC3180.SystemControl.Instance.SW_INT = val;
        }

        public void ContinueUnderNormalInterrupt( RT.Peripherals.Continuation dlg )
        {
            m_softCallback = dlg;

            CauseInterrupt();
        }

        private void ProcessSub1Interrupt( InterruptController.Handler handler )
        {
            ProcessSection( LPC3180.INTC.Instance.Sections[1], 1 );
        }

        private void ProcessSub2Interrupt( InterruptController.Handler handler )
        {
            ProcessSection( LPC3180.INTC.Instance.Sections[2], 2 );
        }

        private void ProcessSoftInterrupt( InterruptController.Handler handler )
        {
            var val = new SystemControl.SW_INT_bitfield();

            val.Raise = false;

            LPC3180.SystemControl.Instance.SW_INT = val;

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
