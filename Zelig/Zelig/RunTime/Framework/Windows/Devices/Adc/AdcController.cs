//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Windows.Devices.Adc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using Provider;
    using Llilum = Microsoft.Llilum.Devices.Adc;

    public sealed class AdcController
    {
        internal IAdcControllerProvider m_adcControllerProvider;

        private AdcController(IAdcControllerProvider provider)
        {
            m_adcControllerProvider = provider;
        }

        public AdcChannelMode ChannelMode
        {
            get
            {
                return (AdcChannelMode)m_adcControllerProvider.ChannelMode;
            }
            set
            {
                m_adcControllerProvider.ChannelMode = (ProviderAdcChannelMode)value;
            }
        }

        public int ChannelCount
        {
            get
            {
                return m_adcControllerProvider.ChannelCount;
            }
        }

        public int MaxValue
        {
            get
            {
                return m_adcControllerProvider.MaxValue;
            }
        }

        public int MinValue
        {
            get
            {
                return m_adcControllerProvider.MinValue;
            }
        }

        public int ResolutionInBits
        {
            get
            {
                return m_adcControllerProvider.ResolutionInBits;
            }
        }

        public bool IsChannelModeSupported(AdcChannelMode channelMode)
        {
            return m_adcControllerProvider.IsChannelModeSupported((ProviderAdcChannelMode)channelMode);
        }

        public AdcChannel OpenChannel(int channelNumber)
        {
            // If the channel cannot be acquired, this will throw
            m_adcControllerProvider.AcquireChannel(channelNumber);

            return new AdcChannel(this, channelNumber);
        }

        public static /*IAsyncOperation<AdcController>*/AdcController GetDefaultAsync()
        {
            return new AdcController(new DefaultAdcProvider());
        }

        public static /*IAsyncOperation*/List<AdcController> GetControllersAsync(IAdcProvider provider)
        {
            List<AdcController> controllers = new List<AdcController>();
            IList<IAdcControllerProvider> providers = provider.GetControllers();

            foreach (IAdcControllerProvider controllerProvider in providers)
            {
                controllers.Add(new AdcController(controllerProvider));
            }
            controllers.Add(new AdcController(providers[0]));
            return controllers;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern Llilum.IAdcChannelInfoUwp GetAdcProviderInfo();

        /// <summary>
        /// This is the default ADC provider that uses the ADC implementation in Microsoft.Llilum.Devices
        /// </summary>
        internal sealed class DefaultAdcProvider : IAdcControllerProvider
        {
            private Llilum.IAdcChannelInfoUwp   m_providerInfo;
            private Llilum.AdcPin[]             m_adcChannels;
            private object                      m_channelLock;

            public DefaultAdcProvider()
            {
                m_providerInfo = AdcController.GetAdcProviderInfo();
                m_adcChannels = new Llilum.AdcPin[m_providerInfo.AdcPinNumbers.Length];
                m_channelLock = new object();
            }

            public int ChannelCount
            {
                get
                {
                    return m_providerInfo.AdcPinNumbers.Length;
                }
            }

            public ProviderAdcChannelMode ChannelMode
            {
                get
                {
                    return ProviderAdcChannelMode.SingleEnded;
                }

                set
                {
                    if (value != ProviderAdcChannelMode.SingleEnded)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            public int MaxValue
            {
                get
                {
                    return m_providerInfo.MaxValue;
                }
            }

            public int MinValue
            {
                get
                {
                    return m_providerInfo.MinValue;
                }
            }

            public int ResolutionInBits
            {
                get
                {
                    return m_providerInfo.ResolutionInBits;
                }
            }

            public void AcquireChannel(int channel)
            {
                if (channel >= m_providerInfo.AdcPinNumbers.Length || channel < 0)
                {
                    throw new InvalidOperationException();
                }

                lock(m_channelLock)
                {
                    if (m_adcChannels[channel] == null)
                    {
                        Llilum.AdcPin newChannel = new Llilum.AdcPin(m_providerInfo.AdcPinNumbers[channel]);
                        m_adcChannels[channel] = newChannel;
                    }
                }
            }

            public bool IsChannelModeSupported(ProviderAdcChannelMode channelMode)
            {
                if (channelMode == ProviderAdcChannelMode.SingleEnded)
                {
                    return true;
                }
                return false;
            }

            public int ReadValue(int channel)
            {
                if (channel >= m_adcChannels.Length || channel < 0)
                {
                    throw new InvalidOperationException();
                }

                lock (m_channelLock)
                {
                    if (m_adcChannels[channel] == null)
                    {
                        throw new InvalidOperationException();
                    }

                    return (int)m_adcChannels[channel].ReadUnsigned();
                }
            }

            public void ReleaseChannel(int channel)
            {
                if (channel >= m_adcChannels.Length || channel < 0)
                {
                    throw new InvalidOperationException();
                }

                lock (m_channelLock)
                {
                    if (m_adcChannels[channel] == null)
                    {
                        throw new InvalidOperationException();
                    }

                    m_adcChannels[channel] = null;
                }
            }
        }
    }
}
