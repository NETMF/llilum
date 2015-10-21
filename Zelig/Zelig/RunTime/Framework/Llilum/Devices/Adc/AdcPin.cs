//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.Devices.Adc
{
    using System;
    using System.Runtime.CompilerServices;

    public class AdcPin : IDisposable
    {
        private AdcChannel m_adcPin;
        private bool m_disposed = false;
        private int m_pinNumber;

        public AdcPin(int pinNumber)
        {
            m_adcPin = TryAcquireAdcPin(pinNumber);
            if (m_adcPin == null)
            {
                throw new ArgumentException();
            }

            m_adcPin.InitializePin();
            m_pinNumber = pinNumber;
        }

        ~AdcPin()
        {
            Dispose(false);
        }

        public uint ReadUnsigned()
        {
            return m_adcPin.ReadUnsigned();
        }

        public float Read()
        {
            return m_adcPin.Read();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                ReleaseAdcPin(m_pinNumber);
                if (disposing)
                {
                    m_adcPin.Dispose();
                }

                m_disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern AdcChannel TryAcquireAdcPin(int pinNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void ReleaseAdcPin(int pinNumber);
    }
}
