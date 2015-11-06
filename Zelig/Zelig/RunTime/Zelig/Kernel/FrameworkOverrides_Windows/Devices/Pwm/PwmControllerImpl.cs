//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using Windows.Devices.Pwm;
    using Microsoft.Llilum.Devices.Pwm;

    [ExtendClass(typeof(PwmController), NoConstructors = true)]
    public class PwmControllerImpl
    {
        public static IPwmChannelInfoUwp GetPwmProviderInfo()
        {
            return PwmProviderUwp.Instance.GetPwmChannelInfo();
        }
    }
}