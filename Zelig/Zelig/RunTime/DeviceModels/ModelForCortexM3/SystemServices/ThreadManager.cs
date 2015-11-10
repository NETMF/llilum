//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using Microsoft.Zelig.Runtime.TargetPlatform.ARMv7;

namespace Microsoft.DeviceModels.Chipset.CortexM3.Runtime
{
    using System;

    using RT        = Microsoft.Zelig.Runtime;
    using TS        = Microsoft.Zelig.Runtime.TypeSystem;
    using Drivers   = Microsoft.DeviceModels.Chipset.CortexM3.Drivers;

    
    public abstract class ThreadManager : RT.ARMv7ThreadManager
    {

        //
        // State
        //

        protected Drivers.ContextSwitchTimer m_ContextSwitchTimer;        

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
        
        //--//
        
        //
        // Extensibility 
        //
        
        public override void Activate()
        {
            //
            // Activate the quantum timer, when the Idle Thread will run will enable exceptions, 
            // thus letting the context switching to start 
            //
            RT.BugCheck.AssertInterruptsOff( );

            m_ContextSwitchTimer = Drivers.ContextSwitchTimer.Instance;
            m_ContextSwitchTimer.Reset();
        }

        public override void CancelQuantumTimer()
        {
            m_ContextSwitchTimer.Cancel();
        }

        public override void SetNextQuantumTimer()
        {
            m_ContextSwitchTimer.Reset( );
        }

        public override void SetNextQuantumTimer( RT.SchedulerTime nextTimeout )
        {
            DateTime   dt                  = ( DateTime )nextTimeout;
            const long TicksPerMillisecond = 10000; // Number of 100ns ticks per time unit
            long       ms                  = dt.Ticks / TicksPerMillisecond;

            if(ms > Drivers.ContextSwitchTimer.c_MaxCounterValue)
            {
                RT.BugCheck.Assert( false, RT.BugCheck.StopCode.IllegalSchedule );
            }

            m_ContextSwitchTimer.Schedule( (uint)ms );
        }
        
        public override void TimeQuantumExpired()
        {
            //
            // this will cause the reschedule
            //
            base.TimeQuantumExpired( );

            //
            // stage a PendSV request to complete the ContextSwitch
            //
            ProcessorARMv7M.CompleteContextSwitch( );
        }
    } 
}
