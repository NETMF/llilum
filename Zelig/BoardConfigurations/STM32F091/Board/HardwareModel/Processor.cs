//
// Copyright ((c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32F091
{
    using System.Runtime.InteropServices;

    using RT            = Microsoft.Zelig.Runtime;
    using ChipsetModel  = Microsoft.CortexM0OnMBED;


    [RT.ProductFilter("Microsoft.Llilum.BoardConfigurations.STM32F091")]
    public sealed class Processor : Microsoft.CortexM0OnMBED.Processor
    {
        public new class Context : ChipsetModel.Processor.Context
        {
            public Context(RT.ThreadImpl owner) : base(owner)
            {
            }

            public override unsafe void SwitchTo( )
            {
                base.SwitchTo( ); 
            }
        }

        //
        // Helper methods
        //

        [RT.Inline]
        public override void InitializeProcessor()
        {
            base.InitializeProcessor( );
        }
        
        [RT.Inline]
        public override Microsoft.Zelig.Runtime.Processor.Context AllocateProcessorContext(RT.ThreadImpl owner)
        {
            return new Context(owner);
        }
    }
}
