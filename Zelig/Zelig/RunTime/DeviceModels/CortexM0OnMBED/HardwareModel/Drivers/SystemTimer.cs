//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.CortexM0OnMBED.Drivers
{
    using System;
    using System.Runtime.CompilerServices;

    using RT            = Microsoft.Zelig.Runtime;
    using LLOS          = Zelig.LlilumOSAbstraction;
    using ChipsetModel  = Microsoft.DeviceModels.Chipset.CortexM;

    /// <summary>
    /// This class implements the internal system timer. All times are in ticks (time agnostic)
    /// however in practice, and due to limitations of mbed 1 tick is equal to 1 uS (micro second)
    /// </summary>
    public abstract class SystemTimer
    {
        public delegate void Callback(Timer timer, ulong currentTime);

        public class Timer
        {
            //
            // State
            //

            private readonly Callback               m_callback;
            private readonly RT.KernelNode<Timer>   m_node;
            private readonly SystemTimer            m_owner;
            private ulong                           m_timeout;
            

            //
            // Constructor Methods
            //

            internal Timer(SystemTimer owner, Callback callback)
            {
                m_owner = owner;
                m_node = new RT.KernelNode<Timer>(this);
                m_callback = callback;
            }

            //
            // Helper Methods
            //

            public void Cancel()
            {
                m_owner.Deregister(this);
            }

            /// <summary>
            /// Call to the Timer handler
            /// </summary>
            /// <param name="currentTime">Time in ticks</param>
            internal void Invoke(ulong currentTime)
            {
                m_callback(this, currentTime);
            }

            //
            // Access Methods
            //

            internal RT.KernelNode<Timer> Node
            {
                get
                {
                    return m_node;
                }
            }

            /// <summary>
            /// Set/get the timeout in absolute time
            /// </summary>
            public ulong Timeout
            {
                get
                {
                    return m_timeout;
                }
                set
                {
                    m_timeout = value;
                    m_owner.Register(this);
                }
            }

            /// <summary>
            /// Set/get the timeout in relation to current time
            /// </summary>
            public ulong RelativeTimeout
            {
                get
                {
                    return m_timeout - m_owner.CurrentTime;
                }
                set
                {
                    m_timeout = value + m_owner.CurrentTime;
                    m_owner.Register(this);
                }
            }
        }

        //--//

        //
        // System Timer Implementation
        //
        
        public const uint c_MaxCounterValue   = uint.MaxValue;
        public const uint c_HalfCycle         = c_MaxCounterValue >> 1;
        public const uint c_QuarterCycle      = c_MaxCounterValue >> 2;
        public const uint c_ThreeQuarterCycle = c_HalfCycle + c_QuarterCycle;

        //--//
        
        private RT.KernelList<Timer>        m_timers;
        private InterruptController.Handler m_interrupthandler;
        private ulong                       m_accumulator;
        private uint                        m_lastAccumulatorUpdate;

        // This is only used as a placeholder to pass into timer_insert_event
        private unsafe LLOS.HAL.TimerContext*  m_timerEvent;
        //--//
        //private static Timer            s_guard;

        public void Initialize()
        {
            m_timers            = new RT.KernelList<Timer>();
            m_interrupthandler  = InterruptController.Handler.Create( 
                                                                Board.Instance.GetSystemTimerIRQ( )         , 
                                                                ChipsetModel.Drivers.InterruptPriority.BelowNormal, 
                                                                ChipsetModel.Drivers.InterruptSettings.Normal     , 
                                                                ProcessTimerInterrupt 
                                                                );

            m_accumulator           = 0;
            m_lastAccumulatorUpdate = this.Counter;

            // Allocate our one, and only, timer event
            unsafe
            {
                fixed (LLOS.HAL.TimerContext** timer_ptr = &m_timerEvent)
                {
                    var callbackContext = InterruptController.CastInterruptHandlerAsPtr( m_interrupthandler ); 
                    LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_AllocateTimer( HandleSystemTimerNative, callbackContext, ulong.MaxValue, timer_ptr );
                }
            }
            
            ChipsetModel.NVIC.SetPriority( 
                ChipsetModel.Board.Instance.GetSystemTimerIRQ(), 
                RT.TargetPlatform.ARMv6.ProcessorARMv6M.c_Priority__SystemTimer 
                );

            //
            // Set up a guard to never suffer from shutting down the underlying circuitry
            //
            //s_guard = CreateTimer( (timer, currentTime) => { timer.RelativeTimeout = QuarterCycle;  }  );
            //s_guard.RelativeTimeout = QuarterCycle; 
            
            InterruptController.Instance.RegisterAndEnable( m_interrupthandler );

            // no need to Refresh because guard causes a refresh already
            Refresh();
        }
        
        /// <summary>
        /// Create a new Timer and return to the user
        /// </summary>
        /// <param name="callback">Handler to run on timer expiration</param>
        /// <returns>New Timer instance</returns>
        public Timer CreateTimer(Callback callback)
        {
            return new Timer(this, callback);
        }

        /// <summary>
        /// Gets the current accumulator time
        /// </summary>
        public ulong CurrentTime
        {
            get
            {
                using (RT.SmartHandles.InterruptState.Disable())
                {
                    // Current time is the accumulator + time since it was updated
                    return m_accumulator + TimeSinceAccumulatorUpdate( this.Counter ); 
                }
            }
        }

        /// <summary>
        /// Gets the current value of the timer accumulator
        /// </summary>
        public unsafe uint Counter
        {
            [RT.Inline]
            get
            {
                return (uint)LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_GetTicks( m_timerEvent );
            }
        }

        /// <summary>
        /// Protected constructor to ensure only we can create this
        /// </summary>
        protected SystemTimer()
        {
        }

        public static extern SystemTimer Instance
        {
            [RT.SingletonFactory()]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        //--//

        /// <summary>
        /// Handle the timer expiration interrupt
        /// </summary>
        /// <param name="ticks">Time when the timer was fired</param>
        internal void ProcessTimeout(ulong ticks)
        {
            // Ensure that lastAccumulatorUpdate is always updated with the accumulator!
            uint counter = this.Counter;

            //
            // BUGBUG: this logic does not cover the case of multiple wraparounds
            //
            m_accumulator           += TimeSinceAccumulatorUpdate(counter);
            m_lastAccumulatorUpdate =  counter;

            // we just updated this above, so it will be precise
            ulong now = m_accumulator;

            while (true)
            {
                RT.KernelNode<Timer> node = m_timers.StartOfForwardWalk;

                // If the next node is null, break and call Refresh
                if (node.IsValidForForwardMove == false)
                {
                    break;
                }

                ulong timeout = node.Target.Timeout;

                if (timeout > now)
                {
                    // If we get here, there are no timers that need to be cleared/invoked
                    break;
                }

                // The current timeout is for the current node (Timer). Remove from List
                // so we do not try to Reload its time.
                node.RemoveFromList();

                // Invoke the handler for the expired timer
                node.Target.Invoke(now);
            }

            Refresh();
        }
        
        /// <summary>
        /// Reload the next expiring timer, if there is one. Otherwise, reload QuarterCycle
        /// </summary>
        private void Refresh()
        {
            ulong absTimeout;

            Timer target = m_timers.FirstTarget();
                
            ulong now = this.CurrentTime;

            if(target != null)
            {
                absTimeout = target.Timeout;
            }
            else
            {
                absTimeout = c_QuarterCycle + now;
            }

            // 
            // Timeout in the past? Trigger the match immediately by loading 1 
            // Timeout too far in the future? Generate match for 
            // a fraction of largest counter value, so we have time to handle wraparounds 
            // 
            Reload((now > absTimeout) ? 1 : (absTimeout - now));
        }

        /// <summary>
        /// Place the timer closest to expiration on the mbed queue
        /// </summary>
        /// <param name="remainder"></param>
        private void Reload(ulong remainder)
        {
            // trim to quarter cycle, so we have time to handle wraparounds
            // This is guaranteed to fit in a uint
            uint trimmed = (uint)Math.Min(remainder, c_QuarterCycle);

            unsafe
            {
                LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_ScheduleTimer( m_timerEvent, trimmed );
            }
        }
        
        //
        // Timer registration
        //

        /// <summary>
        /// Add the timer to the queue in the chronologically appropriate position
        /// </summary>
        /// <param name="timer">Timer to add</param>
        private void Register(Timer timer)
        {
            RT.KernelNode<Timer> node = timer.Node;

            node.RemoveFromList();

            ulong timeout = timer.Timeout;

            RT.KernelNode<Timer> node2 = m_timers.StartOfForwardWalk;

            while (node2.IsValidForForwardMove)
            {
                if (node2.Target.Timeout > timeout)
                {
                    break;
                }

                node2 = node2.Next;
            }

            node.InsertBefore(node2);

            Refresh();
        }

        /// <summary>
        /// Remove the timer from the queue
        /// </summary>
        /// <param name="timer">Timer to be removed</param>
        private void Deregister(Timer timer)
        {
            var node = timer.Node;

            if (node.IsLinked)
            {
                node.RemoveFromList();

                Refresh();
            }
        }

        /// <summary>
        /// Gets the difference in ticks between when the accumulator, and its last recorded value
        /// </summary>
        /// <returns>Difference in ticks</returns>
        private uint TimeSinceAccumulatorUpdate( uint current )
        {
            // If the current timer value is greater than last accumulator update,
            // the counter is still going up. Otherwise, the timer hit its max value
            // and started counting from 0
            return (current >= m_lastAccumulatorUpdate) ?
                current           - m_lastAccumulatorUpdate          :
                c_MaxCounterValue - m_lastAccumulatorUpdate + current;
        }

        //--//

        private static void HandleSystemTimerNative( UIntPtr context, ulong ticks )
        {
            ChipsetModel.Drivers.InterruptController.InterruptData data;

            data.Context = ticks;
            data.Handler = InterruptController.CastAsInterruptHandler( context );

            //
            // This interrupt handler does not come from the ISR vector table, but rather from 'us_ticker_irq_handler' 
            // being lazily set through 'NVIC_SetVector( <ISR NUMBER>, (uint32_t)us_ticker_irq_handler)' during initialization. 
            // Therefore we need to wrap this specific handler here, which is where it first shows up. 
            //
            using(RT.SmartHandles.InterruptState.Disable( ))
            {
                using(RT.SmartHandles.SwapCurrentThreadUnderInterrupt hnd = RT.ThreadManager.InstallInterruptThread( ))
                {
                    InterruptController.Instance.PostInterrupt( data );
                }
            }
        }
        
        private void ProcessTimerInterrupt( ChipsetModel.Drivers.InterruptController.InterruptData data )
        {
            using(RT.SmartHandles.InterruptState.Disable())
            {
                SystemTimer.Instance.ProcessTimeout( data.Context );
            }
        }
    }
}
