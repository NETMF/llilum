//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using System.Runtime.InteropServices;
    using Llilum = Llilum.Devices.Gpio;
    using Runtime = Microsoft.Zelig.Runtime;

    public class GpioPin : Llilum.GpioPin
    {
        private unsafe GpioImpl*    m_gpio;
        private readonly int        m_pinNumber;

        internal GpioPin(int pinNumber)
        {
            m_pinNumber = pinNumber;
            unsafe
            {
                fixed (GpioImpl** gpio_ptr = &m_gpio)
                {
                    tmp_gpio_alloc_init(gpio_ptr, m_pinNumber);
                }
            }
        }

        ~GpioPin()
        {
            Dispose(false);
        }

        public override int PinNumber
        {
            get
            {
                return m_pinNumber;
            }
        }

        public override int Read()
        {
            unsafe
            {
                return tmp_gpio_read(m_gpio);
            }
        }

        public override void Write(int value)
        {
            unsafe
            {
                tmp_gpio_write(m_gpio, value);
            }
        }

        public override void SetPinMode(Llilum.PinMode pinMode)
        {
            unsafe
            {
                tmp_gpio_mode(m_gpio, pinMode);
            }
        }

        public override void SetPinDirection(Llilum.PinDirection pinDirection)
        {
            unsafe
            {
                tmp_gpio_dir(m_gpio, pinDirection);
            }
        }

        public override void Dispose()
        {
            unsafe
            {
                if (m_gpio != null)
                {
                    Dispose(true);
                    m_gpio = null;
                    GC.SuppressFinalize(this);
                }
            }
        }

        private void Dispose(bool disposing)
        {
            unsafe
            {
                tmp_gpio_free(m_gpio);
            }
            if (disposing)
            {
                Runtime.HardwareProvider.Instance.ReleasePins(m_pinNumber);
            }
        }

        [DllImport("C")]
        private static unsafe extern int tmp_gpio_alloc_init(GpioImpl** obj, int pinNumber);
        [DllImport("C")]
        private static unsafe extern int tmp_gpio_free(GpioImpl* obj);
        [DllImport("C")]
        private static unsafe extern void tmp_gpio_write(GpioImpl* obj, int value);
        [DllImport("C")]
        private static unsafe extern int tmp_gpio_read(GpioImpl* obj);
        [DllImport("C")]
        private static unsafe extern void tmp_gpio_mode(GpioImpl* obj, Llilum.PinMode mode);
        [DllImport("C")]
        private static unsafe extern void tmp_gpio_dir(GpioImpl* obj, Llilum.PinDirection direction);
    }

    internal unsafe struct GpioImpl
    {
    };
}
