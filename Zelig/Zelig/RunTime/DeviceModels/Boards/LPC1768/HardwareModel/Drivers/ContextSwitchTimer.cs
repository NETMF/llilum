//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TIMERS_SELF_TEST

namespace Microsoft.Zelig.LPC1768.Drivers
{
    using System.Runtime.CompilerServices;

    using RT      = Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.DeviceModels.Chipset.CortexM3.Drivers; 

    public sealed class ContextSwitchTimer : Chipset.ContextSwitchTimer
    {
    }
}
