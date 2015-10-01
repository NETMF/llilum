//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.DeviceModels.Chipset.LPC3180.Drivers
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

        const uint c_QuarterCycle = 0x40000000u;
        const uint c_OverflowFlag = 0x80000000u;

        private uint                        m_lastCount;
        private uint                        m_highPart;
        private InterruptController.Handler m_interrupt;
        private RT.KernelList< Timer >      m_timers;

        //
        // Helper Methods
        //

        public void Initialize()
        {
            m_timers = new RT.KernelList< Timer >();

            m_interrupt = InterruptController.Handler.Create( LPC3180.INTC.IRQ_INDEX_MSTIMER_INT, InterruptSettings.ActiveHigh, ProcessTimeout );

            //--//

            LPC3180.MilliSecondTimer timer = LPC3180.MilliSecondTimer.Instance;

            timer.MSTIM_COUNTER = 0;

            //
            // No interrupts for now.
            //
            {
                var val = new MilliSecondTimer.MSTIM_MCTRL_bitfield();

                val.MR0_INT = false;
                val.MR1_INT = false;

                timer.MSTIM_MCTRL = val;
            }

            {
                var val = new MilliSecondTimer.MSTIM_CTRL_bitfield();

                val.COUNT_ENAB = true;
                val.PAUSE_EN   = true; // Allow hardware debugging to stop the counter.

                timer.MSTIM_CTRL = val;
            }

            InterruptController.Instance.RegisterAndEnable( m_interrupt );

            Refresh();
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
            Timer target = m_timers.FirstTarget();
            ulong timeout;

            if(target != null)
            {
                timeout = target.Timeout;
            }
            else
            {
                timeout = ulong.MaxValue;
            }

            //--//

            ulong now = this.CurrentTime;

            //
            // Timeout in the past? Trigger the match now.
            //
            if(now > timeout)
            {
                timeout = now;
            }

            //
            // Timeout too far in the future? Generate match closer in time, so we handle wrap arounds.
            //
            if(now + c_QuarterCycle < timeout)
            {
                timeout = now + c_QuarterCycle;
            }

            uint timeoutLow = (uint)timeout;

            LPC3180.MilliSecondTimer timer = LPC3180.MilliSecondTimer.Instance;

            //
            // Create two matches, to protect against race conditions (at least one will fire).
            //
            timer.MSTIM_MATCH0 = timeoutLow;
            timer.MSTIM_MATCH1 = timeoutLow + 1;

            //
            // Configure interrupts.
            //
            {
                var val = new MilliSecondTimer.MSTIM_MCTRL_bitfield();

                val.MR0_INT = true;
                val.MR1_INT = true;

                timer.MSTIM_MCTRL = val;
            }

            //
            // Clear previous interrupts.
            //
            {
                var val = new MilliSecondTimer.MSTIM_INT_bitfield();

                val.MATCH0_INT = true;
                val.MATCH1_INT = true;

                timer.MSTIM_INT = val;
            }

            //
            // Configure the Start Controller.
            //
            var ctrl = LPC3180.SystemControl.Instance;

            ctrl.START_ER_INT  |= LPC3180.SystemControl.START_INT.MSTIMER_INT;
            ctrl.START_APR_INT |= LPC3180.SystemControl.START_INT.MSTIMER_INT;
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
                using(RT.SmartHandles.InterruptState.Disable())
                {
                    uint value    = LPC3180.MilliSecondTimer.Instance.MSTIM_COUNTER;
                    uint highPart = m_highPart;

                    //
                    // Wrap around? Update high part.
                    //
                    if(((value ^ m_lastCount) & c_OverflowFlag) != 0)
                    {
                        highPart++;

                        m_lastCount = value;
                        m_highPart  = highPart;
                    }

                    return (ulong)highPart << 32 | value;
                }
            }
        }
    }
}
