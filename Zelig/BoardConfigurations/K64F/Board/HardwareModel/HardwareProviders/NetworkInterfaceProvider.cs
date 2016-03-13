//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.K64F
{
    using RT      = Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM3OnMBED.HardwareModel;

    public sealed class NetworkInterfaceProvider : Chipset.NetworkInterfaceProvider
    {
        public override void RemapInterrupts()
        {
            Processor.RemapInterrupt( IRQn.ENET_Receive_IRQn  ); 
            Processor.RemapInterrupt( IRQn.ENET_Transmit_IRQn ); 
        }
    }
}
