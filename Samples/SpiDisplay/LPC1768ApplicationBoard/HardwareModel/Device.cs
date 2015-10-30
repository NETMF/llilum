//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LPC1768
{
    using RT            = Microsoft.Zelig.Runtime;
    using ChipsetModel  = Microsoft.CortexM3OnMBED;

    
    public sealed class Device : Microsoft.CortexM3OnMBED.Device
    {
        public override uint ManagedHeapSize
        {
            get
            { 
                return 0x5800u;
            }
        }
    }
}
