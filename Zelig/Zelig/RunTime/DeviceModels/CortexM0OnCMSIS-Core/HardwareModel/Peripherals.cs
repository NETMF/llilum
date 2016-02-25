//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define ALLOW_PAUSE


namespace Microsoft.CortexM0OnCMSISCore
{

    using RT    = Microsoft.Zelig.Runtime;
    using LLOS  = Zelig.LlilumOSAbstraction.HAL;
    using CMSIS = Microsoft.DeviceModels.Chipset.CortexM;
    using ARMv6 = Microsoft.Zelig.Runtime.TargetPlatform.ARMv6;

    public abstract class Peripherals : RT.Peripherals
    {

        //
        // State
        //

        //
        // Helper Methods
        //

        public override void Initialize()
        {
            RT.BugCheck.AssertInterruptsOff();
            
            //
            // Faults, never disabled
            //
            // nothing to do, on an M0/1 faults are always enabled
            
            //
            // System exceptions 
            //
            CMSIS.NVIC.SetPriority( (int)ARMv6.ProcessorARMv6M.IRQn_Type.SVCall_IRQn          , ARMv6.ProcessorARMv6M.c_Priority__SVCCall ); 
            CMSIS.NVIC.SetPriority( (int)ARMv6.ProcessorARMv6M.IRQn_Type.SysTick_IRQn_Optional, ARMv6.ProcessorARMv6M.c_Priority__SysTick ); 
            CMSIS.NVIC.SetPriority( (int)ARMv6.ProcessorARMv6M.IRQn_Type.PendSV_IRQn          , ARMv6.ProcessorARMv6M.c_Priority__PendSV ); 
        }
        
        public override void Activate()
        {
            CMSIS.Drivers.InterruptController.Instance.Initialize();
            CMSIS.Drivers.ContextSwitchTimer .Instance.Initialize();
        }

        public override void EnableInterrupt( uint index )
        {
        }

        public override void DisableInterrupt( uint index )
        {
        }

        public override void CauseInterrupt()
        {
            ARMv6.ProcessorARMv6M.CompleteContextSwitch( ); 
            //Drivers.InterruptController.Instance.CauseInterrupt( ); 
        }

        public override void ContinueUnderNormalInterrupt(Continuation dlg)
        {
            Drivers.InterruptController.Instance.ContinueUnderNormalInterrupt(dlg);
        }

        public override void WaitForInterrupt()
        {
            ARMv6.ProcessorARMv6M.WaitForInterrupt( );
        }

        public override void ProcessInterrupt()
        {
            using (RT.SmartHandles.SwapCurrentThreadUnderInterrupt hnd = RT.ThreadManager.InstallInterruptThread())
            {
                Drivers.InterruptController.Instance.ProcessInterrupt();
            }
        }

        [RT.MemoryRequirements(RT.MemoryAttributes.RAM)]
        public override void ProcessFastInterrupt()
        {
            using (RT.SmartHandles.SwapCurrentThreadUnderInterrupt hnd = RT.ThreadManager.InstallFastInterruptThread())
            {
                Drivers.InterruptController.Instance.ProcessFastInterrupt();
            }
        }

        public override ulong GetPerformanceCounterFrequency()
        {
            return LLOS.Clock.LLOS_CLOCK_GetPerformanceCounterFrequency();
        }

        [RT.Inline]
        [RT.DisableNullChecks()]
        public override uint ReadPerformanceCounter()
        {
            return (uint)LLOS.Clock.LLOS_CLOCK_GetPerformanceCounter();
        }
    }
}
