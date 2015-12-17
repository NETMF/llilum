//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32F411
{
    using System;
    using RT = Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM4OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM4;

    public sealed class AdcProviderUwp : RT.AdcProviderUwp
    {
        private static readonly RT.AdcChannelInfoUwp m_adcInfo = new RT.AdcChannelInfoUwp()
        {
            MaxValue            = UInt16.MaxValue,
            MinValue            = 0,
            ResolutionInBits    = 16,
            AdcPinNumbers       = new int[] { (int)PinName.A0, (int)PinName.A1, (int)PinName.A2, (int)PinName.A3, (int)PinName.A4, (int)PinName.A5 }
        };

        public override RT.AdcChannelInfoUwp GetAdcChannelInfo()
        {
            return m_adcInfo;
        }
    }
}