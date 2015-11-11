//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System;

namespace Windows.Devices.Pwm
{
    public sealed class PwmPin : IDisposable
    {
        private readonly    int             m_pinNumber;
        private             PwmController   m_pwmController;
        private             double          m_dutyCycle;

        internal PwmPin(PwmController controller, int pinNumber)
        {
            m_pwmController = controller;
            m_pinNumber = pinNumber;

            // Default to 50% duty cycle
            m_dutyCycle = 0.5;
        }

        ~PwmPin()
        {
            Dispose(false);
        }

        public PwmPulsePolarity Polarity
        {
            get;
            set;
        }

        public PwmController Controller
        {
            get
            {
                return m_pwmController;
            }
        }

        public bool IsStarted
        {
            get;
            private set;
        }

        public double GetActiveDutyCyclePercentage()
        {
            ThrowIfDisposed();

            return m_dutyCycle;
        }

        public void SetActiveDutyCyclePercentage(double dutyCyclePercentage)
        {
            ThrowIfDisposed();

            if(dutyCyclePercentage < 0 || dutyCyclePercentage > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(dutyCyclePercentage));
            }

            m_dutyCycle = dutyCyclePercentage;
            m_pwmController.m_pwmControllerProvider.SetPulseParameters(m_pinNumber, dutyCyclePercentage, (Polarity == PwmPulsePolarity.ActiveLow));
        }

        public void Start()
        {
            ThrowIfDisposed();

            // In case polarity changed after setting duty cycle, update the duty cycle accordingly
            SetActiveDutyCyclePercentage(m_dutyCycle);

            m_pwmController.m_pwmControllerProvider.EnablePin(m_pinNumber);
            IsStarted = true;
        }

        public void Stop()
        {
            ThrowIfDisposed();

            m_pwmController.m_pwmControllerProvider.DisablePin(m_pinNumber);
            IsStarted = false;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void ThrowIfDisposed()
        {
            if (m_pwmController == null)
            {
                throw new ObjectDisposedException(null);
            }
        }

        private void Dispose(bool disposing)
        {
            if (m_pwmController != null)
            {
                if (disposing)
                {
                    m_pwmController.m_pwmControllerProvider.ReleasePin(m_pinNumber);
                }
                m_pwmController = null;
            }
        }
    }
}
