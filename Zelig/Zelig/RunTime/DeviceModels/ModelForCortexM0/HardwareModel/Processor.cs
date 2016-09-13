//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.CortexM0
{
    using System;

    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.Zelig.Runtime.TargetPlatform.ARMv6;


    public abstract class Processor : ChipsetModel.ProcessorARMv6MForLlvm
    {
        //
        // Helper Methods
        //

        public override void InitializeProcessor()
        {
            base.InitializeProcessor();            
        }
    }
}
