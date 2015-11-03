//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.LPC1768
{
    using RT            = Microsoft.Zelig.Runtime;
    using ChipsetModel  = Microsoft.CortexM3OnMBED;

    public sealed class Processor : Microsoft.CortexM3OnMBED.Processor
    {
        [RT.ProductFilter("Microsoft.Llilum.BoardConfigurations.LPC1768MBEDHosted")]
        public new class Context : ChipsetModel.Processor.Context
        {
        }

        //
        // Helper methods
        //
        
        [RT.Inline]
        public override RT.Processor.Context AllocateProcessorContext()
        {
            return new Context();
        }
    }
}
