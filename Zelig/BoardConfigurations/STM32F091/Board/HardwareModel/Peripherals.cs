//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define ALLOW_PAUSE


namespace Microsoft.Llilum.STM32F091
{
    using RT        = Microsoft.Zelig.Runtime;
    using Chipset   = Microsoft.CortexM0OnMBED;
    using CMSIS     = Microsoft.DeviceModels.Chipset.CortexM;
    

    public sealed class Peripherals : Chipset.Peripherals
    {
        public override void Initialize()
        {
            base.Initialize( );

            //
            // Peripherals exceptions all have same priority
            //
            for(int i = 0; i < (int)IRQn.LAST; ++i)
            {
                CMSIS.NVIC.SetPriority( i , RT.TargetPlatform.ARMv7.ProcessorARMv7M.c_Priority__GenericPeripherals );
            }
        }
    }
}
