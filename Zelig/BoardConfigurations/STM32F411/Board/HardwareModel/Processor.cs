//
// Copyright ((c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32F411
{
    using System.Runtime.InteropServices;

    using RT            = Microsoft.Zelig.Runtime;
    using ChipsetModel  = Microsoft.CortexM4OnMBED;


    [RT.ProductFilter("Microsoft.Llilum.BoardConfigurations.STM32F411")]
    public sealed class Processor : Microsoft.CortexM4OnMBED.Processor
    {
        public new class Context : ChipsetModel.Processor.Context
        {
            public Context(RT.ThreadImpl owner) : base(owner)
            {
            }

            public override unsafe void SwitchTo( )
            {
                //
                // BUGBUG: return to thread using VFP state as well 
                //
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

            DisableMPU( ); 
        }
        
        [RT.Inline]
        public override Microsoft.Zelig.Runtime.Processor.Context AllocateProcessorContext(RT.ThreadImpl owner)
        {
            return new Context(owner);
        }

        //--//

        private unsafe void DisableMPU()
        {
            CUSTOM_STUB_STM32F4xx_DisableMPU( ); 
        }

        [DllImport( "C" )]
        private static extern void CUSTOM_STUB_STM32F4xx_DisableMPU( ); 
    }
}
