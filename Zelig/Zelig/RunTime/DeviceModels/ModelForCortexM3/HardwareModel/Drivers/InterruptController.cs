//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.DeviceModels.Chipset.CortexM3.Drivers
{
    using System;
    using System.Runtime.CompilerServices;

    using Microsoft.Zelig.Runtime.TargetPlatform.ARMv7;

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

    public enum InterruptPriority
    {
        Lowest      = 255,
        BelowNormal = 200,
        Normal      = 127,
        AboveNormal = 50,
        Highest     = 0,
    }

    public abstract class InterruptController
    {
        public delegate void Callback( Handler handler );

        public class Handler
        {
            //
            // State
            //

            private  readonly int                      m_index;
            private  readonly int                      m_section;
            internal readonly InterruptPriority        m_priority;
            private  readonly InterruptSettings        m_settings;
            private  readonly Callback                 m_callback;
            internal readonly RT.KernelNode< Handler > m_node;

            //
            // Constructor Methods
            //

            private Handler( int               index    ,
                             InterruptPriority priority ,
                             InterruptSettings settings ,
                             Callback          callback )
            {
                m_index    = index % 32;
                m_section  = index / 32;
                m_priority = priority;
                m_settings = settings;
                m_callback = callback;

                m_node     = new RT.KernelNode< Handler >( this );
            }

            //
            // Helper Methods
            //

            public static Handler Create( int               index    ,
                                          InterruptPriority priority ,
                                          InterruptSettings settings ,
                                          Callback          callback )
            {
                return new Handler( index, priority, settings, callback );
            }

            public void Enable()
            {
                NVIC.EnableInterrupt( (ProcessorARMv7M.IRQn_Type)m_index );
            }

            public void Disable()
            {
                NVIC.DisableInterrupt( (ProcessorARMv7M.IRQn_Type)m_index );
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
                    return (m_settings & InterruptSettings.EdgeSensitive) != 0;
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
        private RT.Peripherals.Continuation m_softCallback;

        //
        // Helper Methods
        //

        public void Initialize()
        {
            m_handlers = new RT.KernelList< Handler >();
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

            var newPriority = hnd.m_priority;
            int priorityIdx = 0;

            //
            // Insert the handler in priority order and rebuild the Interrupt Priority Registers table.
            //
            for(RT.KernelNode<Handler> node = m_handlers.StartOfForwardWalk; ; node = node.Next)
            {
                if(hnd != null)
                {
                    if(node.IsValidForForwardMove == false || node.Target.m_priority < newPriority)
                    {
                        var newNode = hnd.m_node;

                        newNode.InsertBefore( node );

                        node = newNode;
                        hnd  = null;
                    }
                }

                if(node.IsValidForForwardMove == false)
                {
                    break;
                }

                NVIC.SetPriority( (ProcessorARMv7M.IRQn_Type)node.Target.Index, priorityIdx++ );
            }
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
            ProcessInterrupt( false );
        }

////    [RT.MemoryRequirements( RT.MemoryAttributes.Unpaged )]
        public void ProcessFastInterrupt()
        {
            ProcessInterrupt( true );
        }

        public void ProcessInterrupt( bool fFastOnly )
        {
        //    PXA27x.InterruptController ctrl = PXA27x.InterruptController.Instance;

        //    while(true)
        //    {
        //        var  ichp = ctrl.ICHP;
        //        uint vector;

        //        if(fFastOnly)
        //        {
        //            if(ichp.VAL_FIQ == false)
        //            {
        //                break;
        //            }

        //            vector = ichp.FIQ;
        //        }
        //        else
        //        {
        //            if(ichp.VAL_IRQ == false)
        //            {
        //                break;
        //            }

        //            vector = ichp.IRQ;
        //        }

        //        uint index   = vector % 32;
        //        uint section = vector / 32;

        //        RT.KernelNode< Handler > node = m_handlers.StartOfForwardWalk;

        //        while(true)
        //        {
        //            if(node.IsValidForForwardMove == false)
        //            {
        //                //
        //                // BUGBUG: Unhandled interrupts. We should crash. For now we just disable them.
        //                //
        //                uint mask = 1U << (int)index;

        //                if(section == 0)
        //                {
        //                    ctrl.ICMR &= ~mask;
        //                }
        //                else
        //                {
        //                    ctrl.ICMR2 &= ~mask;
        //                }
        //                break;
        //            }

        //            Handler hnd = node.Target;

        //            if(hnd.Section == section)
        //            {
        //                if(hnd.Index == index && hnd.Section == section)
        //                {
        //                    hnd.Invoke();
        //                    break;
        //                }
        //            }

        //            node = node.Next;
        //        }
        //    }
        }

        public void CauseInterrupt()
        {
            NVIC.SetPending( (ProcessorARMv7M.IRQn_Type)Board.Instance.GetSystemTimerIRQNumber() ); 
        }

        public void ContinueUnderNormalInterrupt( RT.Peripherals.Continuation dlg )
        {
            m_softCallback = dlg;

            CauseInterrupt();
        }

        internal void ProcessSoftInterrupt()
        {
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
