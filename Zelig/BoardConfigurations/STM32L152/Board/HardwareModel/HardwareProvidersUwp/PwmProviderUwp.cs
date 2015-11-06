//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32L152
{
    using System;
    using Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM3OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;

    public sealed class PwmProviderUwp : Microsoft.Zelig.Runtime.PwmProviderUwp
    {
        private static readonly PwmChannelInfoUwp m_pwmInfo = new PwmChannelInfoUwp()
        {
            MaxFrequency = 1000000,
            MinFrequency = 0,
            PwmPinNumbers = new int[] 
            {
                (int)PinName.D0,
                (int)PinName.D1,
                (int)PinName.D3,
                (int)PinName.D4,
                (int)PinName.D5,
                (int)PinName.D6,
                (int)PinName.D9,
                (int)PinName.D10,
                (int)PinName.D11,
                (int)PinName.D12,
                (int)PinName.D13,
            }
        };

        public override PwmChannelInfoUwp GetPwmChannelInfo()
        {
            return m_pwmInfo;
        }
    }
}