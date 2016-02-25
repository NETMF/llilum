//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define ALLOW_PAUSE


namespace Microsoft.CortexM0OnMBED
{

    using RT            = Microsoft.Zelig.Runtime;
    using ChipsetModel  = Microsoft.CortexM0OnCMSISCore;


    public abstract class Peripherals : ChipsetModel.Peripherals
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
