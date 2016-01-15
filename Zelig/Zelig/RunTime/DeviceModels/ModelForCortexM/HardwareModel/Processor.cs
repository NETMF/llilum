//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.CortexM
{
    using System;

    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.Zelig.Runtime.TargetPlatform.ARMv7;


    public abstract class Processor : ChipsetModel.ProcessorARMv7M
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
            
            if(!ChipsetModel.ProcessorARMv7M.VerifyHandlerMode())
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
            NVIC.SetPriorityGrouping( 0 ); 
        }
    }
}
