//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using Runtime = Microsoft.Zelig.Runtime;
    using LLIO = Zelig.LlilumOSAbstraction.API.IO;
    using Llilum.Devices.Pwm;

    public class PwmChannel : Llilum.Devices.Pwm.PwmChannel
    {
        private unsafe LLIO.PwmContext* m_pwm;
        private int m_pinNumber;

        internal PwmChannel(int pinNumber)
        {
            m_pinNumber = pinNumber;
        }

        ~PwmChannel()
        {
            Dispose(false);
        }

        public override void Dispose()
        {
             Dispose(true);
        }

        private unsafe void Dispose(bool disposing)
        {
            if (m_pwm != null)
            {
                LLIO.Pwm.LLOS_PWM_Uninitialize(m_pwm);
                m_pwm = null;

                if (disposing)
                {
                    Runtime.HardwareProvider.Instance.ReleasePins(m_pinNumber);
                    GC.SuppressFinalize(this);
                }
            }
        }

        public override void InitializePin()
        {
            unsafe
            {
                fixed (LLIO.PwmContext** ppAdc = &m_pwm)
                {
                    LLIO.Pwm.LLOS_PWM_Initialize((uint)m_pinNumber, ppAdc);
                }
            }
        }

        public unsafe override void SetDutyCycle(float ratio)
        {
            ThrowIfDisposed();

            LLIO.Pwm.LLOS_PWM_SetDutyCycle(m_pwm, (uint)(ratio * 1024.0), 1024);
        }

        public unsafe override void SetPulseWidth(int microSeconds)
        {
            ThrowIfDisposed();

            LLIO.Pwm.LLOS_PWM_SetPulseWidth(m_pwm, (uint)microSeconds);
        }

        public unsafe override void SetPeriod(int microSeconds)
        {
            ThrowIfDisposed();

            LLIO.Pwm.LLOS_PWM_SetPeriod(m_pwm, (uint)microSeconds);
        }

        private unsafe void ThrowIfDisposed()
        {
            if(m_pwm == null)
            {
                throw new ObjectDisposedException(null);
            }
        }

        public unsafe override void SetPolarity( PwmPolarity polarity )
        {
            ThrowIfDisposed();

            LLIO.Pwm.LLOS_PWM_SetPolarity(m_pwm, (LLIO.PwmPolarity)polarity);
        }

        public unsafe override void SetPrescaler( PwmPrescaler prescaler )
        {
            ThrowIfDisposed();

            LLIO.Pwm.LLOS_PWM_SetPrescaler(m_pwm, (LLIO.PwmPrescaler) prescaler);
        }

        public unsafe override void Start( )
        {
            ThrowIfDisposed();

            LLIO.Pwm.LLOS_PWM_Start(m_pwm);
        }

        public unsafe override void Stop( )
        {
            ThrowIfDisposed();

            LLIO.Pwm.LLOS_PWM_Stop(m_pwm);
        }
    }
}
