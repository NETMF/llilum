//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Llilum.K64F
{
    using Chipset = Microsoft.CortexM4OnMBED;
    using LLOS    = Zelig.LlilumOSAbstraction.API;

    public sealed class ThreadManager : Chipset.ThreadManager
    {
        private const int DefaultStackSizeK64F = 4 * 1024;

        //--//

        //
        // Helper Methods
        //

        public override int DefaultStackSize
        {
            get
            {
                uint stackSize = LLOS.RuntimeMemory.LLOS_MEMORY_GetDefaultManagedStackSize();

                if(stackSize == 0)
                {
                    return DefaultStackSizeK64F;
                }

                return (int)stackSize;
            }
        }
    }
}

