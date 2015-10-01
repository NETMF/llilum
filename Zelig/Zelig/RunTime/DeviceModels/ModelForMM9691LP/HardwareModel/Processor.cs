//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class Processor : RT.TargetPlatform.ARMv4.ProcessorARMv4
    {
        public sealed new class Context : RT.TargetPlatform.ARMv4.ProcessorARMv4.Context
        {
            //
            // Constructor Methods
            //

            //
            // Helper Methods
            //

            //
            // Access Methods
            //

        }

        //
        // Helper Methods
        //

        public override void InitializeProcessor()
        {
            InitializeCache();
        }

        public override UIntPtr GetCacheableAddress( UIntPtr ptr )
        {
            return new UIntPtr( ptr.ToUInt32() | MM9691LP.REMAP_PAUSE.CacheableAddressMask );
        }

        public override UIntPtr GetUncacheableAddress( UIntPtr ptr )
        {
            return new UIntPtr( ptr.ToUInt32() & ~MM9691LP.REMAP_PAUSE.CacheableAddressMask );
        }

        public override unsafe void FlushCacheLine( UIntPtr target )
        {
            uint* ptr = (uint*)(target.ToUInt32() | MM9691LP.REMAP_PAUSE.CacheFlushAddressMask);

            *ptr = 0;
        }

        //--//

        private unsafe void InitializeCache()
        {
            MM9691LP.REMAP_PAUSE remap = MM9691LP.REMAP_PAUSE.Instance;

            remap.SystemConfiguration.PU_DIS = true;

            remap.InitializeCache();
        }

        [RT.Inline]
        public override Microsoft.Zelig.Runtime.Processor.Context AllocateProcessorContext()
        {
            return new Context();
        }
    }
}
