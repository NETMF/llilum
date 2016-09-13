//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32L152
{
    using RT            = Microsoft.Zelig.Runtime;
    using ChipsetModel  = Microsoft.CortexM3OnMBED;

    public sealed class Processor : Microsoft.CortexM3OnMBED.Processor
    {
        [RT.ProductFilter("Microsoft.Llilum.BoardConfigurations.STM32L152")]
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
