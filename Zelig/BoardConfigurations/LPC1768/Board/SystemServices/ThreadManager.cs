//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Llilum.LPC1768
{
    using RT      = Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM3OnMBED;


    public sealed class ThreadManager : Chipset.ThreadManager
    {
        private const int DefaultStackSizeLPC1768 = (4 * 512) / sizeof( uint );

        //--//

        //
        // Helper Methods
        //

        public override int DefaultStackSize
        {
            get
            {
                return DefaultStackSizeLPC1768;
;
            }
        }
    }
}
