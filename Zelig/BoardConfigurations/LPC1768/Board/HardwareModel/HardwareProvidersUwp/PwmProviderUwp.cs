//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.LPC1768
{
    using System;
    using Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM3OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;

    public sealed class PwmProviderUwp : Microsoft.Zelig.Runtime.PwmProviderUwp
    {
        private static readonly PwmChannelInfoUwp m_pwmInfo = new PwmChannelInfoUwp()
        {
            MaxFrequency  = 1000000,
            MinFrequency  = 0,
            PwmPinNumbers = new int[] 
            {
                (int)PinName.p21,
                (int)PinName.p22,
                (int)PinName.p23,
                (int)PinName.p24,
                (int)PinName.p25,
                (int)PinName.p26,
            }
        };

        public override PwmChannelInfoUwp GetPwmChannelInfo()
        {
            return m_pwmInfo;
        }
    }
}