//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Llilum.STM32F401
{
    using Chipset = Microsoft.CortexM0OnMBED;
    using LLOS    = Zelig.LlilumOSAbstraction.API;

    public sealed class ThreadManager : Chipset.ThreadManager
    {
        private const int DefaultStackSizeSTM32F091 = 2 * 1024;

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
                    return DefaultStackSizeSTM32F091;
                }

                return (int)stackSize;
            }
        }
    }
}

