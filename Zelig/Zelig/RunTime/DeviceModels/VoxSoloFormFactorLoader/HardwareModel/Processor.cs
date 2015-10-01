//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.VoxSoloFormFactorLoader
{
    using System;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class Processor : RT.TargetPlatform.ARMv4.ProcessorARMv4
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
