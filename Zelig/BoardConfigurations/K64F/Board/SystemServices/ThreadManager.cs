//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Llilum.K64F
{
    using Chipset = Microsoft.CortexM4OnMBED;
    using System.Runtime.InteropServices;

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
                uint stackSize = CUSTOM_STUB_GetDefaultStackSize();

                if(stackSize >= int.MaxValue)
                {
                    return DefaultStackSizeK64F;
                }

                return (int)stackSize;
            }
        }

        [DllImport("C")]
        private static unsafe extern uint CUSTOM_STUB_GetDefaultStackSize();
    }
}

