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

        protected Drivers.ContextSwitchTimer.SysTickTimer m_SysTickTimer;        

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
            // Activate the quantum timer, when the Idle Thread will run will enable execptions, 
            // thus letting the context switching to start 
            //
            RT.BugCheck.AssertInterruptsOff( );

            m_SysTickTimer = Drivers.ContextSwitchTimer.Instance.CreateTimer( TimeQuantumExpired );
            m_SysTickTimer.Schedule();
        }

        public override void CancelQuantumTimer()
        {
            m_SysTickTimer.Cancel();
        }

        public override void SetNextQuantumTimer()
        {
            m_SysTickTimer.Schedule( ); 
        }

        public override void SetNextQuantumTimer( RT.SchedulerTime nextTimeout )
        {
            ulong timeout = nextTimeout.Units;
            
            if(timeout > Drivers.ContextSwitchTimer.c_MaxCounterValue)
            {
                RT.BugCheck.Assert( false, RT.BugCheck.StopCode.IllegalSchedule );
            }

            m_SysTickTimer.RelativeTimeout = (uint)timeout;
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
        
        //--//

        private void TimeQuantumExpired( Drivers.ContextSwitchTimer.SysTickTimer sysTickTimer, ulong currentTime )
        {
            TimeQuantumExpired();
        }
    } 
}
