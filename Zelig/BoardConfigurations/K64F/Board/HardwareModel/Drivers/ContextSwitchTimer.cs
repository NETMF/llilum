//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TIMERS_SELF_TEST

namespace Microsoft.Llilum.K64F.Drivers
{
    using System.Runtime.CompilerServices;

    using RT      = Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.DeviceModels.Chipset.CortexM3.Drivers; 

    public sealed class ContextSwitchTimer : Chipset.ContextSwitchTimer
    {
        protected override uint GetTicksForQuantumValue( uint ms )
        {
            // K64F uses the Core clock (120Mhz) for SysTick 
            return (uint)( RT.Configuration.CoreClockFrequency / 1000 ) * ms; 
        }
    }
}
