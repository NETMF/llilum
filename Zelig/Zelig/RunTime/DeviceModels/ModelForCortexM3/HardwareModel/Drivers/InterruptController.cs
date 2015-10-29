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
        public delegate void Callback( InterruptData data );

        /// <summary>
        /// This structure contains the interrupt handler and any data associated with the interrupt.
        /// Context and Subcontext is interrupt dependent data and is not required to be set.
        /// </summary>
        public struct InterruptData
        {
            public uint    Context;
            public uint    Subcontext;
            public Handler Handler;
        }

        public class Handler
        {
            //
            // State
            //

            private  readonly ProcessorARMv7M.IRQn_Type m_index;
            internal readonly InterruptPriority         m_priority;
            private  readonly InterruptSettings         m_settings;
            private  readonly Callback                  m_callback;
            internal readonly RT.KernelNode< Handler >  m_node;

            //
            // Constructor Methods
            //

            private Handler( ProcessorARMv7M.IRQn_Type index    ,
                             InterruptPriority         priority ,
                             InterruptSettings         settings ,
                             Callback                  callback )
            {
                m_index    = index;
                m_priority = priority;
                m_settings = settings;
                m_callback = callback;

                m_node     = new RT.KernelNode< Handler >( this );
            }

            //
            // Helper Methods
            //

            public static Handler Create( ProcessorARMv7M.IRQn_Type index    ,
                                          InterruptPriority         priority ,
                                          InterruptSettings         settings ,
                                          Callback                  callback )
            {
                return new Handler( index, priority, settings, callback );
            }

            public void Enable()
            {
                NVIC.EnableInterrupt( m_index );
            }

            public void Disable()
            {
                NVIC.DisableInterrupt( m_index );
            }

            public void Invoke( InterruptData interruptData )
            {
                m_callback( interruptData );
            }

            //
            // Access Methods
            //


            public ProcessorARMv7M.IRQn_Type Index
            {
                get
                {
                    return m_index;
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

        private RT.KernelList< Handler >               m_handlers;
        private System.Threading.Thread                m_interruptThread;
        private RT.KernelCircularBuffer<InterruptData> m_interrupts;

        //
        // Helper Methods
        //

        public void Initialize()
        {
            m_handlers = new RT.KernelList< Handler >();
            m_interrupts = new RT.KernelCircularBuffer<InterruptData>(32);
            m_interruptThread = new System.Threading.Thread(DispatchInterrupts);
            m_interruptThread.Priority = System.Threading.ThreadPriority.Highest;
        }

        public void Activate()
        {
            m_interruptThread.Start();
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

                NVIC.SetPriority( node.Target.Index, priorityIdx++ );
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
            InterruptData data;
            ProcessorARMv7M.IRQn_Type activeInterrupt = GetNextActiveInterupt();

            data.Context    = 0;
            data.Subcontext = 0;

            while (activeInterrupt != ProcessorARMv7M.IRQn_Type.Invalid)
            {
                RT.KernelNode<Handler> node = m_handlers.StartOfForwardWalk;

                while (true)
                {
                    if (node.IsValidForForwardMove == false)
                    {
                        break;
                    }

                    Handler hnd = node.Target;

                    if (hnd.Index == activeInterrupt)
                    {
                        data.Handler = hnd;

                        if ( hnd.IsFastHandler )
                        {
                            hnd.Invoke(data);
                        }
                        else
                        {
                            PostInterrupt(data);
                        }
                    }

                    node = node.Next;
                }

                ClearInterrupt(activeInterrupt);
                activeInterrupt = GetNextActiveInterupt();
            }
        }
    
        public virtual ProcessorARMv7M.IRQn_Type GetNextActiveInterupt()
        {
            return ProcessorARMv7M.IRQn_Type.Invalid;
        }

        public virtual void ClearInterrupt( ProcessorARMv7M.IRQn_Type interrupt )
        {
        }

        public void CauseInterrupt()
        {
            NVIC.SetPending( (ProcessorARMv7M.IRQn_Type)Board.Instance.GetSystemTimerIRQNumber() ); 
        }

        public void ContinueUnderNormalInterrupt( RT.Peripherals.Continuation dlg )
        {
        }

        public void PostInterrupt(InterruptData interruptData)
        {
            RT.BugCheck.AssertInterruptsOff();

            m_interrupts.EnqueueNonblocking(interruptData);
        }

        private void DispatchInterrupts()
        {
            while (true)
            {
               InterruptData intr = m_interrupts.DequeueBlocking();

                if (intr.Handler != null)
                {
                    intr.Handler.Invoke( intr );
                }
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
