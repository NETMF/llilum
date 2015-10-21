//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Windows.Devices.Adc.Provider
{
    public interface IAdcControllerProvider
    {
        int ChannelCount
        {
            get;
        }

        ProviderAdcChannelMode ChannelMode
        {
            get;
            set;
        }

        int MaxValue
        {
            get;
        }

        int MinValue
        {
            get;
        }

        int ResolutionInBits
        {
            get;
        }

        bool IsChannelModeSupported(ProviderAdcChannelMode channelMode);

        void AcquireChannel(int channel);

        void ReleaseChannel(int channel);

        int ReadValue(int channelNumber);
    }
}
