//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define ALLOW_PAUSE


namespace Microsoft.CortexM3OnMBED
{
    using Microsoft.Zelig.Runtime.TargetPlatform.ARMv7;

    using RT      = Microsoft.Zelig.Runtime;
    using CMSIS   = Microsoft.DeviceModels.Chipset.CortexM3;
    using Chipset = Microsoft.CortexM3OnCMSISCore;


    public class Peripherals : Chipset.Peripherals
    {
        //
        // State
        //

        //
        // Helper Methods
        //
        
        public override void Activate()
        {
            base.Activate( ); 
            
            Drivers.SystemTimer.Instance.Initialize();
            Drivers.InterruptController.Instance.Activate();

            //Drivers.GPIO.Instance.Initialize();
            //Drivers.I2C.Instance.Initialize();
            //Drivers.SPI.Instance.Initialize();
        }
        
        [RT.Inline]
        [RT.DisableNullChecks()]
        public override uint ReadPerformanceCounter()
        {
            return Drivers.SystemTimer.Instance.Counter;
        }
    }
}
