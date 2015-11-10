//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Llilum.LPC1768
{
    using Chipset = Microsoft.CortexM3OnMBED;
    using System.Runtime.InteropServices;

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
                uint stackSize = CUSTOM_STUB_GetDefaultStackSize();

                if(stackSize >= int.MaxValue)
                {
                    return DefaultStackSizeLPC1768;
                }

                return (int)stackSize;
            }
        }

        [DllImport("C")]
        private static unsafe extern uint CUSTOM_STUB_GetDefaultStackSize();
    }
}
