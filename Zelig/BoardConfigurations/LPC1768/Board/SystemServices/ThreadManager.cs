//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Llilum.LPC1768
{
    using Chipset = Microsoft.CortexM3OnMBED;
    using LLOS    = Zelig.LlilumOSAbstraction.API;

    public sealed class ThreadManager : Chipset.ThreadManager
    {
        private const int DefaultStackSizeLPC1768 = 2 * 1024;

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
                    return DefaultStackSizeLPC1768;
                }

                return (int)stackSize;
            }
        }
    }
}
