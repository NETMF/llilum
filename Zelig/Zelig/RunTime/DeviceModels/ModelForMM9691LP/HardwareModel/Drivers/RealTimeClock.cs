//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.DeviceModels.Chipset.MM9691LP.Drivers
{
    using System;
    using System.Runtime.CompilerServices;

    using RT = Microsoft.Zelig.Runtime;


    public abstract class RealTimeClock
    {
        public delegate void Callback( Timer timer, ulong currentTime );

        public class Timer
        {
            //
            // State
            //

            private RealTimeClock          m_owner;
            private RT.KernelNode< Timer > m_node;
            private ulong                  m_timeout;
            private Callback               m_callback;

            //
            // Constructor Methods
            //

            internal Timer( RealTimeClock owner    ,
                            Callback      callback )
            {
                m_owner    = owner;
                m_node     = new RT.KernelNode< Timer >( this );
                m_callback = callback;
            }

            //
            // Helper Methods
            //

            public void Cancel()
            {
                m_owner.Deregister( this );
            }

            internal void Invoke( ulong currentTime )
            {
                m_callback( this, currentTime );
            }

            //
            // Access Methods
            //

            internal RT.KernelNode< Timer > Node
            {
                get
                {
                    return m_node;
                }
            }

            public ulong Timeout
            {
                get
                {
                    return m_timeout;
                }

                set
                {
                    m_timeout = value;

                    m_owner.Register( this );
                }
            }

            public ulong RelativeTimeout
            {
                get
                {
                    return m_timeout - RealTimeClock.Instance.CurrentTime;
                }

                set
                {
                    m_timeout = value +  RealTimeClock.Instance.CurrentTime;

                    m_owner.Register( this );
                }
            }
        }

        //
        // State
        //

        private InterruptController.Handler m_interrupt;
        private RT.KernelList< Timer >      m_timers;

        //
        // Helper Methods
        //

        public void Initialize()
        {
            m_timers = new RT.KernelList< Timer >();

            m_interrupt = InterruptController.Handler.Create( MM9691LP.INTC.IRQ_MASK_Real_Time_Clock, ProcessTimeout );

            InterruptController.Instance.Register( m_interrupt );
        }

        public Timer CreateTimer( Callback callback )
        {
            return new Timer( this, callback );
        }

        //--//

        private void Register( Timer timer )
        {
            RT.KernelNode< Timer > node = timer.Node;

            node.RemoveFromList();

            ulong timeout = timer.Timeout;

            RT.KernelNode< Timer > node2 = m_timers.StartOfForwardWalk;

            while(node2.IsValidForForwardMove)
            {
                if(node2.Target.Timeout > timeout)
                {
                    break;
                }

                node2 = node2.Next;
            }

            node.InsertBefore( node2 );

            Refresh();
        }

        private void Deregister( Timer timer )
        {
            var node = timer.Node;

            if(node.IsLinked)
            {
                node.RemoveFromList();

                Refresh();
            }
        }

        //--//

        private void ProcessTimeout( InterruptController.Handler handler )
        {
            ulong currentTime = this.CurrentTime;

            while(true)
            {
                RT.KernelNode< Timer > node = m_timers.StartOfForwardWalk;
                
                if(node.IsValidForForwardMove == false)
                {
                    break;
                }

                if(node.Target.Timeout > currentTime)
                {
                    break;
                }

                node.RemoveFromList();

                node.Target.Invoke( currentTime );
            }

            Refresh();
        }

        void Refresh()
        {
            RT.KernelNode< Timer > node = m_timers.FirstNode();

            if(node != null)
            {
                MM9691LP.RTC.Instance.COMP = node.Target.Timeout;

                m_interrupt.Enable();
            }
            else
            {
                m_interrupt.Disable();
            }
        }

        //
        // Access Methods
        //

        public static extern RealTimeClock Instance
        {
            [RT.SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }

        public ulong CurrentTime
        {
            get
            {
                MM9691LP.RTC.Instance.LD_HREG = 1;

                Processor.Delay( 12 );

                return MM9691LP.RTC.Instance.HREG;
            }
        }
    }
}
