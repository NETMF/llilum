//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    public abstract class DeviceClockTicksTracking
    {
        public delegate void Callback();

        public class ClockTickCallbackDescriptor
        {
            //
            // State
            //

            public readonly ulong    ClockTicks;
            public readonly Callback Callback;

            //
            // Constructor Methods
            //

            public ClockTickCallbackDescriptor( ulong    clockTicks ,
                                                Callback callback   )
            {
                this.ClockTicks = clockTicks;
                this.Callback   = callback;
            }
        }

        //
        // State
        //

        ulong                               m_clockFrequency;
        ulong                               m_nextClockTicksCallback;
        List< ClockTickCallbackDescriptor > m_clockTicksCallbacks;
                                            
        //
        // Constructor Methods
        //

        protected DeviceClockTicksTracking()
        {
            m_nextClockTicksCallback = ulong.MaxValue;
            m_clockTicksCallbacks    = new List< ClockTickCallbackDescriptor >();
        }

        //
        // Helper Methods
        //

        public virtual void ResetState()
        {
            m_nextClockTicksCallback = ulong.MaxValue;
            m_clockTicksCallbacks.Clear();
        }

        public TimeSpan ClockTicksToTime( long clockTicks )
        {
            return new TimeSpan( (long)((double)clockTicks / m_clockFrequency * TimeSpan.TicksPerSecond) );
        }

        public long TimeToClockTicks( TimeSpan time )
        {
            return (long)(time.TotalSeconds * m_clockFrequency);
        }
 
        public void RequestRelativeClockTickCallback( long     relativeClockTicks ,
                                                      Callback callback           )
        {
            RequestAbsoluteClockTickCallback( this.ClockTicks + (ulong)relativeClockTicks, callback );
        }

        public void RequestAbsoluteClockTickCallback( ulong    absoluteClockTicks ,
                                                      Callback callback           )
        {
            CancelClockTickCallback( callback );

            lock(this.LockRoot)
            {
                int pos = 0;

                while(pos < m_clockTicksCallbacks.Count)
                {
                    ClockTickCallbackDescriptor cd = m_clockTicksCallbacks[pos];

                    if(cd.ClockTicks > absoluteClockTicks)
                    {
                        break;
                    }

                    pos++;
                }

                m_clockTicksCallbacks.Insert( pos, new ClockTickCallbackDescriptor( absoluteClockTicks, callback ) );

                if(pos == 0)
                {
                    m_nextClockTicksCallback = absoluteClockTicks;

                    NotifyActivity();
                }
            }
        }

        public void CancelClockTickCallback( Callback callback )
        {
            lock(this.LockRoot)
            {
                for(int pos = 0; pos < m_clockTicksCallbacks.Count; pos++)
                {
                    ClockTickCallbackDescriptor cd = m_clockTicksCallbacks[pos];

                    if(cd.Callback == callback)
                    {
                        m_clockTicksCallbacks.RemoveAt( pos );

                        if(pos == 0)
                        {
                            SetNextClockTickCallback();
                        }

                        return;
                    }
                }
            }
        }

        //--//

        public abstract bool GetAbsoluteTime( out ulong clockTicks  ,
                                              out ulong nanoseconds );

        public abstract IDisposable SuspendTiming();

        //--//

        private void SetNextClockTickCallback()
        {
            if(m_clockTicksCallbacks.Count > 0)
            {
                m_nextClockTicksCallback = m_clockTicksCallbacks[0].ClockTicks;

                NotifyActivity();
            }
            else
            {
                m_nextClockTicksCallback = ulong.MaxValue;
            }
        }

        public bool ShouldProcessClockTicksCallback( ulong clockTicks )
        {
            return m_nextClockTicksCallback <= clockTicks;
        }

        public long TimeToNextCallback( ulong clockTicks )
        {
            return (long)(m_nextClockTicksCallback - clockTicks);
        }

        public void ProcessClockTicksCallback()
        {
////        DumpTimestamped( "ProcessClockTicksCallback", clockTicks, "{0,9} Delta:{1}", m_nextClockTicksCallback, clockTicks - m_nextClockTicksCallback );

            ClockTickCallbackDescriptor cd;

            lock(this.LockRoot)
            {
                CHECKS.ASSERT( m_clockTicksCallbacks.Count > 0, "'m_nextClockTicksCallback < m_clockTicks' and no work items to execute!" );

                cd = m_clockTicksCallbacks[0];

                if(cd.ClockTicks > this.ClockTicks)
                {
                    //
                    // If we get here and there's a race condition (someone modified the top callback), just exit.
                    //
                    return;
                }

                m_clockTicksCallbacks.RemoveAt( 0 );

                SetNextClockTickCallback();
            }

////        DumpTimestamped( "Callback Start", this.ClockTicks, "{0} {1}", cd.m_callback.Method, GC.CollectionCount( 0 ) );

            cd.Callback();

////        DumpTimestamped( "Callback End", this.ClockTicks, "{0} {1}", cd.m_callback.Method, GC.CollectionCount( 0 ) );
        }

        protected abstract void NotifyActivity();

        //--//

        public ulong FromPerformanceCounterToClockTicks( long perfTicks )
        {
            return (ulong)((double)perfTicks / System.Diagnostics.Stopwatch.Frequency * m_clockFrequency);
        }

        public ulong FromMillisecondsToClockTicks( int milliSeconds )
        {
            return (ulong)((double)milliSeconds / 1000 * m_clockFrequency);
        }

        public long FromClockTicksToMilliseconds( long clockTicks )
        {
            return (long)((double)clockTicks / m_clockFrequency * 1000);
        }

        public double FromClockTicksToSeconds( long clockTicks )
        {
            return (double)clockTicks / m_clockFrequency;
        }

        //
        // Access Methods
        //

        public ulong ClockFrequency
        {
            get
            {
                return m_clockFrequency;
            }

            set
            {
                m_clockFrequency = value;
            }
        }

        public abstract ulong ClockTicks
        {
            get;
        }

        protected abstract object LockRoot
        {
            get;
        }
    }
}
