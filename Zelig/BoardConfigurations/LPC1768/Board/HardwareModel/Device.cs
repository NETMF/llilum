//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.LPC1768
{
    using RT = Microsoft.Zelig.Runtime;

    public sealed class Device : Microsoft.CortexM3OnMBED.Device
    {
        // TODO: When the compiler optimizations are complete, revisit this stack size since it is likely
        // it could be reduced.
        [RT.MemoryUsage(RT.MemoryUsage.Stack, ContentsUninitialized = true, AllocateFromHighAddress = true)]
        static readonly uint[] s_bootstrapStackLPC1768 = new uint[ 512 / sizeof( uint ) ];

        public override uint[] BootstrapStack
        {
            get
            {
                return s_bootstrapStackLPC1768;
            }
        }
    }
}
