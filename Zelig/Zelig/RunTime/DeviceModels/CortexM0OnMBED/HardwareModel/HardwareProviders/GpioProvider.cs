//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM0OnMBED.HardwareModel
{
    using Framework = Microsoft.Llilum.Devices.Gpio;
    using RT        = Microsoft.Zelig.Runtime;

    public abstract class GpioProvider : RT.GpioProvider
    {
        public override Framework.GpioPin CreateGpioPin(int pinNumber)
        {
            if (!RT.HardwareProvider.Instance.TryReservePins(pinNumber))
            {
                return null;
            }
            return new GpioPin(pinNumber);
        }
    }
}
