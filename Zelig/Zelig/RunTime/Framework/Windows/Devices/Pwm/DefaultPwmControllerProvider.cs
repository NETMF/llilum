//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System;
using Windows.Devices.Pwm.Provider;

using Llilum = Microsoft.Llilum.Devices.Pwm;

namespace Windows.Devices.Pwm
{
    internal class DefaultPwmControllerProvider : IPwmControllerProvider
    {
        /// <summary>
        /// Used to keep track of the pin past and present state for UWP
        /// </summary>
        private class ControllerPin
        {
            public Llilum.PwmPin Pin;
            public bool          Enabled;
        }

        private readonly    Llilum.IPwmChannelInfoUwp   m_providerInfo;
        private             ControllerPin[]             m_pwmPins;
        private             object                      m_channelLock;
        private             double                      m_frequency;

        public DefaultPwmControllerProvider(Llilum.IPwmChannelInfoUwp pwmInfoUwp)
        {
            m_providerInfo = pwmInfoUwp;
            m_pwmPins = new ControllerPin[m_providerInfo.PwmPinNumbers.Length];
            m_channelLock = new object();
        }

        public double ActualFrequency
        {
            get
            {
                return m_frequency;
            }
        }

        public double MaxFrequency
        {
            get
            {
                return m_providerInfo.MaxFrequency;
            }
        }

        public double MinFrequency
        {
            get
            {
                return m_providerInfo.MinFrequency;
            }
        }

        public int PinCount
        {
            get
            {
                return m_providerInfo.PwmPinNumbers.Length;
            }
        }

        public void AcquirePin(int pin)
        {
            int pinIndex = GetPinIndex(pin);

            if (pinIndex == -1)
            {
                throw new ArgumentException(null, nameof(pin));
            }

            lock (m_channelLock)
            {
                if (m_pwmPins[pinIndex] == null)
                {
                    // Try allocating first, to avoid releasing the pin if allocation fails
                    ControllerPin controlPin = new ControllerPin();
                    Llilum.PwmPin newPin = new Llilum.PwmPin(m_providerInfo.PwmPinNumbers[pinIndex]);

                    // Set frequency to current, and duty cycle to 0 (disabled)
                    int usPeriod = (int)(1000000.0 / ActualFrequency);
                    newPin.SetPeriod(usPeriod);

                    // Initialize the pin to disabled by default
                    newPin.Stop();

                    controlPin.Pin = newPin;
                    m_pwmPins[pinIndex] = controlPin;
                }
            }
        }

        public void DisablePin(int pin)
        {
            int pinIndex = GetPinIndex(pin);

            if (pinIndex == -1)
            {
                throw new ArgumentException(null, nameof(pinIndex));
            }

            lock (m_channelLock)
            {
                if (m_pwmPins[pinIndex] == null)
                {
                    throw new InvalidOperationException();
                }

                m_pwmPins[pinIndex].Pin.Stop();
                m_pwmPins[pinIndex].Enabled = false;
            }
        }

        public void EnablePin(int pin)
        {
            int pinIndex = GetPinIndex(pin);

            if (pinIndex == -1)
            {
                throw new ArgumentException(null, nameof(pin));
            }

            lock (m_channelLock)
            {
                if (m_pwmPins[pinIndex] == null)
                {
                    throw new InvalidOperationException();
                }

                m_pwmPins[ pinIndex ].Pin.Start();
                m_pwmPins[pinIndex].Enabled = true;
            }
        }

        public void ReleasePin(int pin)
        {
            int pinIndex = GetPinIndex(pin);

            if (pinIndex == -1)
            {
                throw new ArgumentException(null, nameof(pin));
            }

            lock (m_channelLock)
            {
                if (m_pwmPins[pinIndex] == null)
                {
                    throw new InvalidOperationException();
                }

                m_pwmPins[pinIndex].Pin.Dispose();
                m_pwmPins[pinIndex] = null;
            }
        }

        public double SetDesiredFrequency(double frequency)
        {
            // UWP does not have a notion of having a separate frequency per pin
            // so we need to conform to the API and use the same frequency for all pins
            m_frequency = frequency;

            return m_frequency;
        }

        public void SetPulseParameters(int pin, double dutyCycle, bool invertPolarity)
        {
            int pinIndex = GetPinIndex(pin);

            if (pinIndex == -1)
            {
                throw new ArgumentException(null, nameof(pin));
            }

            if(dutyCycle < 0 || dutyCycle > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(dutyCycle));
            }

            lock (m_channelLock)
            {
                if (m_pwmPins[pinIndex] == null)
                {
                    throw new InvalidOperationException();
                }

                // If polarity is inverted, inverted duty cycle is 100% - dutyCycle
                if(invertPolarity)
                {
                    dutyCycle = 1.0 - dutyCycle;
                }

                m_pwmPins[pinIndex].Pin.SetDutyCycle((float)dutyCycle);
            }
        }

        /// <summary>
        /// Get the index of the pin in m_pwmPins
        /// </summary>
        /// <param name="pin">Real pin number</param>
        /// <returns>Pin index</returns>
        private int GetPinIndex(int pin)
        {
            for(int i = m_providerInfo.PwmPinNumbers.Length - 1; i >= 0; i--)
            {
                if(m_providerInfo.PwmPinNumbers[i] == pin)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
