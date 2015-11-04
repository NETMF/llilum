//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Llilum.STM32L152
{
    using RT      = Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM3OnMBED;


    public sealed class ThreadManager : Chipset.ThreadManager
    {
        private const int DefaultStackSizeSTM32L152 = (4 * 512) / sizeof( uint );

        //--//

        //
        // Helper Methods
        //

        public override int DefaultStackSize
        {
            get
            {
                return DefaultStackSizeSTM32L152;
            }
        }
    }
}
