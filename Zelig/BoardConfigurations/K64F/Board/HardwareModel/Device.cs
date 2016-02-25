//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.K64F
{
    using RT = Microsoft.Zelig.Runtime;

    public sealed class Device : Microsoft.CortexM4OnMBED.Device
    {
        [RT.MemoryUsage(RT.MemoryUsage.Stack, ContentsUninitialized = true, AllocateFromHighAddress = true)]
        static readonly uint[] s_bootstrapStackK64F = new uint[ 1024 / sizeof( uint ) ]; 

        //
        // Access Methods
        //

        public override uint[] BootstrapStack
        {
            get
            {
                return s_bootstrapStackK64F;
            }
        }
    }
}
