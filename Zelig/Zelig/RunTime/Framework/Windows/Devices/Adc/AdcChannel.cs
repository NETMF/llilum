//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Windows.Devices.Adc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class AdcChannel : IDisposable
    {
        private readonly int    m_channelNumber;
        private AdcController   m_adcController;

        internal AdcChannel(AdcController controller, int channelNumber)
        {
            m_adcController = controller;
            m_channelNumber = channelNumber;
        }

        ~AdcChannel()
        {
            Dispose(false);
        }

        public AdcController Controller
        {
            get
            {
                return m_adcController;
            }
        }

        public int ReadValue()
        {
            ThrowIfDisposed();

            return m_adcController.m_adcControllerProvider.ReadValue(m_channelNumber);
        }

        public double ReadRatio()
        {
            ThrowIfDisposed();

            return (double)ReadValue() / (double)m_adcController.MaxValue;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            if(m_adcController == null)
            {
                throw new ObjectDisposedException(null);
            }
        }

        private void Dispose(bool disposing)
        {
            if (m_adcController != null)
            {
                if(disposing)
                {
                    m_adcController.m_adcControllerProvider.ReleaseChannel(m_channelNumber);
                    m_adcController = null;
                }
            }
        }
    }
}
