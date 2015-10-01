//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x.Runtime
{
    using System;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    
    public abstract class ThreadManager : RT.ARMv5ThreadManager
    {
        const ulong c_TimeQuantumMsec = 20;

        //
        // State
        //

        Drivers.RealTimeClock.Timer m_timer;
        Drivers.RealTimeClock.Timer m_timerForWaits;

        //
        // Helper Methods
        //

        public override void InitializeBeforeStaticConstructors()
        {
            base.InitializeBeforeStaticConstructors();
        }

        public override void InitializeAfterStaticConstructors( uint[] systemStack )
        {
            base.InitializeAfterStaticConstructors( systemStack );
        }

        public override void Activate()
        {
            m_timer         = Drivers.RealTimeClock.Instance.CreateTimer( TimeQuantumExpired );
            m_timerForWaits = Drivers.RealTimeClock.Instance.CreateTimer( WaitExpired        );
        }

        public override void Reschedule()
        {
            base.Reschedule();
        }

        public override void SetNextWaitTimer( RT.SchedulerTime nextTimeout )
        {
            if(nextTimeout != RT.SchedulerTime.MaxValue)
            {
                m_timerForWaits.Timeout = nextTimeout.Units;
            }
            else
            {
                m_timerForWaits.Cancel();
            }
        }

        public override void CancelQuantumTimer()
        {
            m_timer.Cancel();
        }

        public override void SetNextQuantumTimer()
        {
            m_timer.RelativeTimeout = c_TimeQuantumMsec * 1000 * RT.Configuration.RealTimeClockFrequency / 1000000;
        }

        public override void SetNextQuantumTimer( RT.SchedulerTime nextTimeout )
        {
            m_timer.RelativeTimeout = nextTimeout.Units;
        }

        //--//

        private void TimeQuantumExpired( Drivers.RealTimeClock.Timer timer       ,
                                         ulong                       currentTime )
        {
            TimeQuantumExpired();
        }

        private void WaitExpired( Drivers.RealTimeClock.Timer timer       ,
                                  ulong                       currentTime )
        {
            WaitExpired( RT.SchedulerTime.FromUnits( currentTime ) );
        }
    } 
}
