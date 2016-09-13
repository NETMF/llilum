//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.DeviceModels.Chipset.CortexM3
{
    using RT           = Microsoft.Zelig.Runtime; 
    using ChipsetModel = Microsoft.Zelig.Runtime.TargetPlatform.ARMv7;


    public abstract class Processor : ChipsetModel.ProcessorARMv7MForLlvm
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
