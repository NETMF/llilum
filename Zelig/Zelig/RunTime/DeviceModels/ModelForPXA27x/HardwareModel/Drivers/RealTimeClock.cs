//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.DeviceModels.Chipset.PXA27x.Drivers
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

        readonly OSTimers.OMCR_bitfield c_Timer1Ctrl = 
            new OSTimers.OMCR_bitfield
            {
                P    = true , // Keep the counter running after a match.
                C    = false, // Channel 5 match against Channel 4 counter.
                CRES = OSTimers.OMCR_bitfield.Resolution.Freq1MHz,
            };

        readonly OSTimers.OMCR_bitfield c_Timer2Ctrl = 
            new OSTimers.OMCR_bitfield
            {
                P    = true, // Keep the counter running after a match.
                C    = true, // Channel 4 matches against its counter.
                CRES = OSTimers.OMCR_bitfield.Resolution.Freq1MHz,
            };


        private uint                        m_lastCount;
        private uint                        m_highPart;
        private InterruptController.Handler m_interrupt;
        private InterruptController.Handler m_simulatedSoftInterrupt;
        private RT.KernelList< Timer >      m_timers;

        //
        // Helper Methods
        //

        public void Initialize()
        {
            m_timers = new RT.KernelList< Timer >();

            m_interrupt              = InterruptController.Handler.Create( PXA27x.InterruptController.IRQ_INDEX_OS_TIMER , InterruptPriority.Normal, InterruptSettings.ActiveHigh, ProcessTimeout       );
            m_simulatedSoftInterrupt = InterruptController.Handler.Create( PXA27x.InterruptController.IRQ_INDEX_OS_TIMER0, InterruptPriority.Lowest, InterruptSettings.ActiveHigh, ProcessSoftInterrupt );

            //--//

            var clockControl = PXA27x.ClockManager.Instance;

            clockControl.CKEN.EnOsTimer = true;

            //--//

            var timer = PXA27x.OSTimers.Instance;

            timer.OIER = 0;

            //--//

            //
            // We use Timer0 as a soft interrupt, so we have to wait until it has fired and NEVER reset the match value.
            //
            timer.EnableInterrupt( 0 );

            timer.WriteCounter( 0, 0   );
            timer.SetMatch    ( 0, 100 );

            while(timer.HasFired( 0 ) == false)
            {
            }

            //--//

            timer.SetControl( 5, c_Timer1Ctrl );
            timer.SetControl( 4, c_Timer2Ctrl );

            //
            // Start the timer.
            //
            timer.WriteCounter( 4, 0 );

            timer.EnableInterrupt( 4 );
            timer.EnableInterrupt( 5 );

            //--//

            var intc = InterruptController.Instance;

            intc.RegisterAndEnable( m_interrupt              );
            intc.Register         ( m_simulatedSoftInterrupt );

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

        internal void CauseInterrupt()
        {
            var timer = PXA27x.OSTimers.Instance;

            timer.SetMatch(0, timer.ReadCounter(0) + 8);

            m_simulatedSoftInterrupt.Enable();
        }

        private void ProcessSoftInterrupt( InterruptController.Handler handler )
        {
            m_simulatedSoftInterrupt.Disable();

            InterruptController.Instance.ProcessSoftInterrupt();
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
            ulong nowPlusQuarterCycle = now + c_QuarterCycle;
            if(nowPlusQuarterCycle < timeout)
            {
                timeout = nowPlusQuarterCycle;
            }

            uint timeoutLow = (uint)timeout;

            var timer = PXA27x.OSTimers.Instance;

            // disable second interrupt so we don't handle this timeout twice
            timer.DisableInterrupt( 5 );

            //
            // Clear previous interrupts.
            //
            timer.ClearFired( 4 );
            timer.ClearFired( 5 );

            //
            // Create two matches, to protect against race conditions (at least one will fire).
            //
            timer.SetMatch( 4, timeoutLow      );
            timer.SetMatch( 5, timeoutLow + 10 );

            timer.EnableInterrupt( 5 );
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

        public uint CurrentTimeRaw
        {
            get
            {
                return PXA27x.OSTimers.Instance.ReadCounter( 4 );
            }
        }

        public ulong CurrentTime
        {
            get
            {
                using(RT.SmartHandles.InterruptState.Disable())
                {
                    uint value    = PXA27x.OSTimers.Instance.ReadCounter( 4 );
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
