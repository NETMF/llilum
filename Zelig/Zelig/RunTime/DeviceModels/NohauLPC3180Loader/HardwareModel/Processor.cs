//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.NohauLPC3180Loader
{
    using System;

    using RT           = Microsoft.Zelig.Runtime;
    using TS           = Microsoft.Zelig.Runtime.TypeSystem;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.LPC3180;


    public sealed class Processor : ChipsetModel.Processor
    {
        //
        // Helper Methods
        //

        public override void InitializeProcessor()
        {
        }

        public override UIntPtr GetCacheableAddress( UIntPtr ptr )
        {
            return ptr;
        }

        public override UIntPtr GetUncacheableAddress( UIntPtr ptr )
        {
            return ptr;
        }

        public override unsafe void FlushCacheLine( UIntPtr target )
        {
        }

        //--//

        [RT.Inline]
        public override RT.Processor.Context AllocateProcessorContext()
        {
            return null;
        }
    }
}
