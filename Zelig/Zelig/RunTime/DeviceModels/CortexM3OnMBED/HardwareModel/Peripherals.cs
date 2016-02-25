//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define ALLOW_PAUSE


namespace Microsoft.CortexM3OnMBED
{

    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.CortexM0OnMBED;
    using CMSIS        = Microsoft.DeviceModels.Chipset.CortexM;
    using ARMv7        = Microsoft.Zelig.Runtime.TargetPlatform.ARMv7;

    public abstract class Peripherals : ChipsetModel.Peripherals
    {
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
            CMSIS.NVIC.SetPriority( (int)ARMv7.ProcessorARMv7M.IRQn_Type.SVCall_IRQn , ARMv7.ProcessorARMv7M.c_Priority__SVCCall ); 
            CMSIS.NVIC.SetPriority( (int)ARMv7.ProcessorARMv7M.IRQn_Type.SysTick_IRQn, ARMv7.ProcessorARMv7M.c_Priority__SysTick ); 
            CMSIS.NVIC.SetPriority( (int)ARMv7.ProcessorARMv7M.IRQn_Type.PendSV_IRQn , ARMv7.ProcessorARMv7M.c_Priority__PendSV  ); 
        }

        public override void WaitForInterrupt()
        {
            ARMv7.ProcessorARMv7M.WaitForInterrupt( );
        }

        public override void CauseInterrupt()
        {
            ARMv7.ProcessorARMv7M.CompleteContextSwitch( ); 
        }
    }
}
