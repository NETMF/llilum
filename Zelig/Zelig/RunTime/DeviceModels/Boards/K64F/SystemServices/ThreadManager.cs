//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.K64F
{
    using RT      = Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM4OnMBED;


    public sealed class ThreadManager : Chipset.ThreadManager
    {
        private const int DefaultStackSizeK64F = (4 * 1024) / sizeof( uint );

        //--//

        //
        // Helper Methods
        //

        public override int DefaultStackSize
        {
            get
            {
                return DefaultStackSizeK64F;
            }
        }
    }
}

