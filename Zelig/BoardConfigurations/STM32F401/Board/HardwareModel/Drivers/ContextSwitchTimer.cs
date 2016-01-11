//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TIMERS_SELF_TEST

namespace Microsoft.Llilum.STM32F401.Drivers
{
    using System.Runtime.CompilerServices;

    using RT      = Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.DeviceModels.Chipset.CortexM4.Drivers; 

    public sealed class ContextSwitchTimer : Chipset.ContextSwitchTimer
    {
        protected override uint GetTicksForQuantumValue( uint ms )
        {
            // STM32F411 uses the Core clock (100Mhz) for SysTick 
            return (uint)( RT.Configuration.CoreClockFrequency / 1000 ) * ms; 
        }
    }
}
