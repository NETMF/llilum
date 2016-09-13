//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.LPC1768
{
    using RT            = Microsoft.Zelig.Runtime;
    using ChipsetModel  = Microsoft.CortexM3OnMBED;


    [RT.ProductFilter("Microsoft.Llilum.BoardConfigurations.LPC1768")]
    public sealed class Processor : Microsoft.CortexM3OnMBED.Processor
    {
        public new class Context : ChipsetModel.Processor.Context
        {
            public Context(RT.ThreadImpl owner) : base(owner)
            {
            }
        }

        //
        // Helper methods
        //
        
        [RT.Inline]
        public override RT.Processor.Context AllocateProcessorContext(RT.ThreadImpl owner)
        {
            return new Context(owner);
        }
    }
}
