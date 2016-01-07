//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define ALLOW_PAUSE


namespace Microsoft.DeviceModels.Win32
{
    using System;
    using RT    = Microsoft.Zelig.Runtime;
    using LLOS  = Zelig.LlilumOSAbstraction.HAL;

    public class Peripherals : RT.Peripherals
    {

        //
        // State
        //

        //
        // Helper Methods
        //

        public override void Initialize()
        {
        }
        
        public override void Activate()
        {
        }

        public override void EnableInterrupt( uint index )
        {
            throw new NotImplementedException( );
        }

        public override void DisableInterrupt( uint index )
        {
            throw new NotImplementedException( );
        }

        public override void CauseInterrupt()
        {
        }

        public override void ContinueUnderNormalInterrupt(Continuation dlg)
        {
            throw new NotImplementedException( );
        }

        public override void WaitForInterrupt()
        {
        }

        public override void ProcessInterrupt()
        {
            throw new NotImplementedException( );
        }

        [RT.MemoryRequirements(RT.MemoryAttributes.RAM)]
        public override void ProcessFastInterrupt()
        {
            throw new NotImplementedException( );
        }

        public override ulong GetPerformanceCounterFrequency()
        {
            return LLOS.Clock.LLOS_CLOCK_GetPerformanceCounterFrequency();
        }

        [RT.Inline]
        [RT.DisableNullChecks()]
        public override uint ReadPerformanceCounter()
        {
            return (uint)LLOS.Clock.LLOS_CLOCK_GetPerformanceCounter();
        }
    }
}
