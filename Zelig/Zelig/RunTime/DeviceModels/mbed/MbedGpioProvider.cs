//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Support.mbed
{
    using System;
    using System.Collections.Generic;
    using Windows.Devices.Gpio;
    using Windows.Devices.Gpio.Provider;
    
    //--//

    public class MbedGpioProvider : IGpioControllerProvider
    {
        private IDictionary<int, MbedGpioPin> m_pins;

        public MbedGpioProvider()
        {
            m_pins = new Dictionary<int, MbedGpioPin>();
        }

        public int PinCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool AllocatePin(int pinNumber)
        {
            // Don't allow a pin to be opened twice.
            if (m_pins.ContainsKey(pinNumber))
            {
                return false;
            }

            m_pins.Add(pinNumber, new MbedGpioPin((PinName)pinNumber));
            return true;
        }

        public void ReleasePin(int pinNumber)
        {
            m_pins[pinNumber].Dispose();
            m_pins.Remove(pinNumber);
        }

        public GpioPinValue Read(int pinNumber)
        {
            return (m_pins[pinNumber].Read() == 0) ? GpioPinValue.Low : GpioPinValue.High;
        }

        public void Write(int pinNumber, GpioPinValue value)
        {
            m_pins[pinNumber].Write((int)value);
        }

        public void SetPinDriveMode(int pinNumber, GpioPinDriveMode driveMode)
        {
            m_pins[pinNumber].SetDriveMode(driveMode);
        }
    }
}
