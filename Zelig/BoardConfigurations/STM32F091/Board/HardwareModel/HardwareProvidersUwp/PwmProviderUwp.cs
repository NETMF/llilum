//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32F091
{
    using System;
    using Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM0OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM0;

    public sealed class PwmProviderUwp : Microsoft.Zelig.Runtime.PwmProviderUwp
    {
        private static readonly PwmChannelInfoUwp m_pwmInfo = new PwmChannelInfoUwp()
        {
            MaxFrequency  = 1000000,
            MinFrequency  = 0,
            PwmPinNumbers = new int[] 
            {
                (int)PinName.PWM_OUT,
            }
        };

        public override PwmChannelInfoUwp GetPwmChannelInfo()
        {
            return m_pwmInfo;
        }
    }
}