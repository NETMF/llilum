//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Support.mbed
{
    using System;
    using Windows.Devices.Gpio;

    //--//

    internal class MbedGpioPin : IDisposable
    {
        unsafe public MbedGpioPin(PinName pin)
        {
            fixed (GPIOimpl** gpio_ptr = &m_gpio)
            {
                GPIO.tmp_gpio_alloc(gpio_ptr);
            }

            GPIO.gpio_init_in(m_gpio, pin);
        }

        ~MbedGpioPin()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (!m_disposed)
            {
                GC.SuppressFinalize(this);
                Dispose(true);
                m_disposed = true;
            }
        }

        unsafe public void Write(int value)
        {
            GPIO.tmp_gpio_write(m_gpio, value);
        }
        unsafe public int Read()
        {
            return GPIO.tmp_gpio_read(m_gpio);
        }

        unsafe public void SetDriveMode(GpioPinDriveMode driveMode)
        {
            switch (driveMode)
            {
            case GpioPinDriveMode.Input:
                GPIO.gpio_mode(m_gpio, PinMode.PullDefault);
                GPIO.gpio_dir(m_gpio, PinDirection.PIN_INPUT);
                break;

            case GpioPinDriveMode.Output:
                GPIO.gpio_mode(m_gpio, PinMode.PullDefault);
                GPIO.gpio_dir(m_gpio, PinDirection.PIN_OUTPUT);
                break;

            case GpioPinDriveMode.InputPullUp:
                GPIO.gpio_mode(m_gpio, PinMode.PullUp);
                GPIO.gpio_dir(m_gpio, PinDirection.PIN_INPUT);
                break;

            case GpioPinDriveMode.InputPullDown:
                GPIO.gpio_mode(m_gpio, PinMode.PullDown);
                GPIO.gpio_dir(m_gpio, PinDirection.PIN_INPUT);
                break;
            }
        }

        unsafe private void Dispose(bool disposing)
        {
            GPIO.tmp_gpio_free(m_gpio);
            m_gpio = null;
        }

        private bool m_disposed = false;
        private unsafe GPIOimpl* m_gpio;
    }
}
