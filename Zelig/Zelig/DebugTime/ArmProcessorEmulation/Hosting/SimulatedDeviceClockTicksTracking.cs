//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    public abstract class SimulatedDeviceClockTicksTracking : DeviceClockTicksTracking
    {
        class FakeTimingStateSmartHandler : IDisposable
        {
            //
            // Helper Methods
            //

            public void Dispose()
            {
            }
        }

        //
        // State
        //

        System.Diagnostics.Stopwatch m_clockTicks;
                                            
        //
        // Constructor Methods
        //

        protected SimulatedDeviceClockTicksTracking()
        {
            m_clockTicks = new System.Diagnostics.Stopwatch();
        }

        //
        // Helper Methods
        //

        //--//

        public override void ResetState()
        {
            base.ResetState();

            m_clockTicks.Reset();
        }

        public void Start()
        {
            m_clockTicks.Start();
        }

        public void Stop()
        {
            m_clockTicks.Stop();
        }

        public override bool GetAbsoluteTime( out ulong clockTicks  ,
                                              out ulong nanoseconds )
        {
            clockTicks  = 0;
            nanoseconds = 0;
            return false;
        }

        public override IDisposable SuspendTiming()
        {
            return new FakeTimingStateSmartHandler();
        }

        //--//

        //
        // Access Methods
        //

        public override ulong ClockTicks
        {
            get
            {
                long clockTicks;

                lock(this.LockRoot)
                {
                    clockTicks = m_clockTicks.ElapsedTicks;
                }

                return FromPerformanceCounterToClockTicks( clockTicks );
            }
        }
    }
}
