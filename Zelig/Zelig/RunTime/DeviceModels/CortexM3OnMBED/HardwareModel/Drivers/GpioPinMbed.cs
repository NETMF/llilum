//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using System.Runtime.InteropServices;
    using Windows.Devices.Gpio.Provider;

    public class GpioPinMbed : GpioPinProvider
    {
        // This acts as a void* so we can hold a reference to the gpio_t struct
        private unsafe GPIOimpl* _gpio;
        private bool m_disposed;

        ~GpioPinMbed()
        {
            Dispose(false);
        }

        public override void Dispose()
        {
            if (!m_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                m_disposed = true;
            }
        }

        private void Dispose(bool disposing)
        {
            unsafe
            {
                tmp_gpio_free(_gpio);
            }
            if (disposing)
            {
            }
        }

        public override void InitializePin(int pinNumber)
        {
            unsafe
            {
                fixed (GPIOimpl** gpio_ptr = &_gpio)
                {
                    tmp_gpio_alloc(gpio_ptr);
                }

                tmp_gpio_init(_gpio, pinNumber);
            }
        }

        public override int Read()
        {
            unsafe
            {
                return tmp_gpio_read(_gpio);
            }
        }
        
        public override void SetPinDriveMode(GpioDriveMode driveMode)
        {
            unsafe
            {
                switch (driveMode)
                {
                    case GpioDriveMode.Input:
                        tmp_gpio_mode(_gpio, PinMode.PullDefault);
                        tmp_gpio_dir(_gpio, PinDirection.PIN_INPUT);
                        break;

                    case GpioDriveMode.Output:
                        tmp_gpio_mode(_gpio, PinMode.PullDefault);
                        tmp_gpio_dir(_gpio, PinDirection.PIN_OUTPUT);
                        break;

                    case GpioDriveMode.InputPullUp:
                        tmp_gpio_mode(_gpio, PinMode.PullUp);
                        tmp_gpio_dir(_gpio, PinDirection.PIN_INPUT);
                        break;

                    case GpioDriveMode.InputPullDown:
                        tmp_gpio_mode(_gpio, PinMode.PullDown);
                        tmp_gpio_dir(_gpio, PinDirection.PIN_INPUT);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public override void Write(int value)
        {
            unsafe
            {
                tmp_gpio_write(_gpio, value);
            }
        }

        [DllImport("C")]
        private static unsafe extern void tmp_gpio_write(GPIOimpl* obj, int value);
        [DllImport("C")]
        private static unsafe extern int tmp_gpio_read(GPIOimpl* obj);
        [DllImport("C")]
        private static unsafe extern int tmp_gpio_alloc(GPIOimpl** obj);
        [DllImport("C")]
        private static unsafe extern int tmp_gpio_init(GPIOimpl* obj, int pinNumber);
        [DllImport("C")]
        private static unsafe extern int tmp_gpio_free(GPIOimpl* obj);
        [DllImport("C")]
        private static unsafe extern void tmp_gpio_mode(GPIOimpl* obj, PinMode mode);
        [DllImport("C")]
        private static unsafe extern void tmp_gpio_dir(GPIOimpl* obj, PinDirection direction);
    }

    public enum PinDirection
    {
        PIN_INPUT = 0,
        PIN_OUTPUT = 1
    }

    public enum PinMode
    {
        PullUp = 0,
        PullDown = 3,
        PullNone = 2,
        Repeater = 1,
        OpenDrain = 4,
        PullDefault = PullDown
    }

    internal unsafe struct GPIOimpl
    {
    };
}
