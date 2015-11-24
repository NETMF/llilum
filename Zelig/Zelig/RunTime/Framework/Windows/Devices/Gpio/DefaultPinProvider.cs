//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System;
using Windows.Devices.Gpio.Provider;
using Windows.Foundation;

using Llilum = Microsoft.Llilum.Devices.Gpio;

namespace Windows.Devices.Gpio
{
    internal class DefaultPinProvider : IGpioPinProvider
    {
        private Llilum.GpioPin m_gpioPin;
        private TypedEventHandler<GpioPin, GpioPinValueChangedEventArgs> m_evt;

        internal DefaultPinProvider(int pinNumber)
        {
            Llilum.GpioPin newPin = Llilum.GpioPin.TryCreateGpioPin(pinNumber);
            if (newPin == null)
            {
                throw new InvalidOperationException();
            }
            m_gpioPin = newPin;
        }

        public TimeSpan DebounceTimeout
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int PinNumber
        {
            get
            {
                return m_gpioPin.PinNumber;
            }
        }

        public GpioSharingMode SharingMode
        {
            get
            {
                return GpioSharingMode.Exclusive;
            }
        }

        public void Dispose()
        {
            m_gpioPin.Dispose();
        }

        public event TypedEventHandler<GpioPin, GpioPinValueChangedEventArgs> ValueChanged
        {
            add
            {
                var old = m_evt;
                m_evt += value;

                if( old == null )
                {
                    m_gpioPin.ValueChanged += HandleGpioInterrupt;
                }
            }
            remove
            {
                m_evt -= value;

                if( m_evt == null )
                {
                    m_gpioPin.ValueChanged -= HandleGpioInterrupt;
                }
            }
        }

        private void HandleGpioInterrupt(object sender, Llilum.PinEdge pinEdge)
        {
            m_evt?.Invoke(null, new GpioPinValueChangedEventArgs(pinEdge == Llilum.PinEdge.RisingEdge ? GpioPinEdge.RisingEdge : GpioPinEdge.FallingEdge));
        }

        public GpioPinDriveMode GetDriveMode()
        {
            switch (m_gpioPin.Mode)
            {
                case Llilum.PinMode.PullNone:
                case Llilum.PinMode.Default:
                    return (m_gpioPin.Direction == Llilum.PinDirection.Input) ? 
                        GpioPinDriveMode.Input : GpioPinDriveMode.Output;
                case Llilum.PinMode.PullDown:
                    return GpioPinDriveMode.InputPullDown;
                case Llilum.PinMode.PullUp:
                    return GpioPinDriveMode.InputPullUp;
            }
            throw new NotSupportedException();
        }

        public bool IsDriveModeSupported(GpioPinDriveMode driveMode)
        {
            switch (driveMode)
            {
                case GpioPinDriveMode.Input:
                case GpioPinDriveMode.Output:
                case GpioPinDriveMode.InputPullUp:
                case GpioPinDriveMode.InputPullDown:
                    return true;
                default:
                    return false;
            }
        }

        public GpioPinValue Read()
        {
            return (m_gpioPin.Read() == 0) ? GpioPinValue.Low : GpioPinValue.High;
        }

        public void SetPinDriveMode(GpioDriveMode driveMode)
        {
            switch (driveMode)
            {
                case GpioDriveMode.Input:
                    m_gpioPin.Mode = Llilum.PinMode.PullNone;
                    m_gpioPin.Direction = Llilum.PinDirection.Input;
                    break;

                case GpioDriveMode.Output:
                    m_gpioPin.Mode = Llilum.PinMode.Default;
                    m_gpioPin.Direction = Llilum.PinDirection.Output;
                    break;

                case GpioDriveMode.InputPullUp:
                    m_gpioPin.Mode = Llilum.PinMode.PullUp;
                    m_gpioPin.Direction = Llilum.PinDirection.Input;
                    break;

                case GpioDriveMode.InputPullDown:
                    m_gpioPin.Mode = Llilum.PinMode.PullDown;
                    m_gpioPin.Direction = Llilum.PinDirection.Input;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public void Write(GpioPinValue value)
        {
            m_gpioPin.Write((int)value);
        }
    }
}
