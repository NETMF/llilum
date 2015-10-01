//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using Windows.Devices.Gpio;
    using Windows.Devices.Gpio.Provider;

    //--//

    [ExtendClass( typeof( GpioController ), NoConstructors = true )]
    public class GpioControllerImpl
    {
        public static GpioPinProvider AcquireGpioPin(int pin)
        {
            HardwareProvider provider = HardwareProvider.Instance;

            if (!provider.TryReservePins(pin))
            {
                return null;
            }

            GpioPinProvider gpioPin;
            try
            {
                // Our pin is reserved. Now we need to return its implementation
                gpioPin = provider.CreateGpioPin();

                gpioPin.InitializePin(pin);
            }
            catch
            {
                ReleaseGpioPin(pin);

                throw;
            }

            return gpioPin;
        }

        public static int GetBoardPinCount()
        {
            return HardwareProvider.Instance.PinCount;
        }

        public static void ReleaseGpioPin(int pinNumber)
        {
            HardwareProvider provider = HardwareProvider.Instance;
            provider.ReleasePins(pinNumber);
        }
    }
}
