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
            m_SysTickTimer = Drivers.ContextSwitchTimer.Instance.CreateTimer( TimeQuantumExpired );
            m_SysTickTimer.Muted = false;
            m_SysTickTimer.Schedule();
        }

        public override void CancelQuantumTimer()
        {
            //
            // We will let the SysTick run forever, but 
            // we do not want to hear from any ISR...
            //
            //m_SysTickTimer.Cancel();
            m_SysTickTimer.Muted = true;
        }

        public override void SetNextQuantumTimer()
        {
            //
            // Normally we would use a value from the configuration, but for a Cortex-M the SysTick timer 
            // is equipped to be efficiently scheduled for a context switch and does not need to be reloaed
            // 
            m_SysTickTimer.Muted = false;
        }

        public override void SetNextQuantumTimer( RT.SchedulerTime nextTimeout )
        {
            ulong timeout = nextTimeout.Units;

            if(timeout > 0x00FFFFFF)
            {
                RT.BugCheck.Assert( false, RT.BugCheck.StopCode.IllegalSchedule );
            }

            //
            // only reschedule if changed
            //
            if(m_SysTickTimer.RelativeTimeout != (uint)timeout)
            {
                m_SysTickTimer.RelativeTimeout = (uint)timeout;
            }
            
            //
            // Unmute 
            //
            m_SysTickTimer.Muted = false;
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

        private void TimeQuantumExpired( Drivers.ContextSwitchTimer.SysTickTimer sysTickTimer       ,
                                         ulong                     currentTime )
        {
            TimeQuantumExpired();
        }

    } 
}
