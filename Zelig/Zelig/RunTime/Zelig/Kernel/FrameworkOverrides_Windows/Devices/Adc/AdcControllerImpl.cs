//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using Windows.Devices.Adc;
    using Microsoft.Llilum.Devices.Adc;

    [ExtendClass(typeof(AdcController), NoConstructors = true)]
    public class AdcControllerImpl
    {
        public static IAdcChannelInfoUwp GetAdcProviderInfo()
        {
            return AdcProviderUwp.Instance.GetAdcChannelInfo();
        }
    }
}