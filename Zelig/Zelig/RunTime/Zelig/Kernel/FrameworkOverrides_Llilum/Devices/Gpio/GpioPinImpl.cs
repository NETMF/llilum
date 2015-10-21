//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using Microsoft.Llilum.Devices.Gpio;

    [ExtendClass(typeof(GpioPin), NoConstructors = true)]
    class GpioPinImpl
    {
        public static GpioPin TryAcquireGpioPin(int pinNumber)
        {
            return GpioProvider.Instance.CreateGpioPin(pinNumber);
        }

        public static int GetBoardPinCount()
        {
            return HardwareProvider.Instance.PinCount;
        }
    }
}
