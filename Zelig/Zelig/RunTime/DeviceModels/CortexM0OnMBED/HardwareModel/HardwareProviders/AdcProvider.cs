//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM0OnMBED.HardwareModel
{
    using Framework = Microsoft.Llilum.Devices.Adc;
    using RT        = Microsoft.Zelig.Runtime;

    public abstract class AdcProvider : RT.AdcProvider
    {
        public override Framework.AdcChannel CreateAdcPin(int pinNumber)
        {
            if(!RT.HardwareProvider.Instance.TryReservePins(pinNumber))
            {
                return null;
            }
            return new AdcChannel(pinNumber);
        }
    }
}
