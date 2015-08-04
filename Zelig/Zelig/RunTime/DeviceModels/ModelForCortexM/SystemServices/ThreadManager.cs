//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.CortexM.Runtime
{
    using System;

    using RT        = Microsoft.Zelig.Runtime;
    using TS        = Microsoft.Zelig.Runtime.TypeSystem;
    using Drivers   = Microsoft.DeviceModels.Chipset.CortexM.Drivers;

    
    public abstract class ThreadManager : RT.ThreadManager
    {
        const ulong c_TimeQuantumMsec = 20;

        //
        // State
        //

        Drivers.SystemTimer.Timer m_timer;
        Drivers.SystemTimer.Timer m_timerForWaits;

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
            m_timer         = Drivers.SystemTimer.Instance.CreateTimer( TimeQuantumExpired );
            m_timerForWaits = Drivers.SystemTimer.Instance.CreateTimer( WaitExpired        );
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

        private void TimeQuantumExpired( Drivers.SystemTimer.Timer timer       ,
                                         ulong                     currentTime )
        {
            TimeQuantumExpired();
        }

        private void WaitExpired( Drivers.SystemTimer.Timer timer       ,
                                  ulong                     currentTime )
        {
            WaitExpired( RT.SchedulerTime.FromUnits( currentTime ) );
        }
    } 
}
