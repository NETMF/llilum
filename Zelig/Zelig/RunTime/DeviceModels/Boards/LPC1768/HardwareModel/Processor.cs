//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LPC1768
{
    using RT            = Microsoft.Zelig.Runtime;
    using ChipsetModel  = Microsoft.CortexM3OnMBED;

    
    [RT.ProductFilter("Microsoft.Zelig.Configuration.Environment.LPC1768MBEDHosted")]
    public sealed class Processor : Microsoft.CortexM3OnMBED.Processor
    {
        public new class Context : ChipsetModel.Processor.Context
        {
        }

        //
        // Helper methods
        //
        
        [RT.Inline]
        public override Microsoft.Zelig.Runtime.Processor.Context AllocateProcessorContext()
        {
            return new Context();
        }
    }
}
