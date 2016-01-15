//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.CortexM0
{
    using System;

    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.CortexM;


    public abstract class Processor : ChipsetModel.Processor
    {
        //
        // Helper Methods
        //

        public override void InitializeProcessor()
        {
            base.InitializeProcessor();

            //
            // Ensure privileged Handler mode
            //
            
            if(!RT.TargetPlatform.ARMv7.ProcessorARMv7M.VerifyHandlerMode())
            {
                RT.BugCheck.Log( "Cannot bootstrap in Thread mode" );
                RT.BugCheck.Assert( false, RT.BugCheck.StopCode.FailedBootstrap );
            }

            //
            // For a Cortex-M with caches or MPU, we could initialize it here
            //

            //
            // Reset the priority grouping that we assume not used
            //
            Microsoft.DeviceModels.Chipset.CortexM.NVIC.SetPriorityGrouping( 0 ); 
        }
    }
}
