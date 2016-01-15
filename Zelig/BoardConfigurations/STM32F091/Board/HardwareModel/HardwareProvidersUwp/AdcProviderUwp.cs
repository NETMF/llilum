//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32F401
{
    using System;
    using RT = Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM0OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM0;

    public sealed class AdcProviderUwp : RT.AdcProviderUwp
    {
        private static readonly RT.AdcChannelInfoUwp m_adcInfo = new RT.AdcChannelInfoUwp()
        {
            MaxValue            = UInt16.MaxValue,
            MinValue            = 0,
            ResolutionInBits    = 16,
            AdcPinNumbers       = new int[] { (int)PinName.PB_1, (int)PinName.PC_2, (int)PinName.PC_3, (int)PinName.PC_4, (int)PinName.PC_5 }
        };

        public override RT.AdcChannelInfoUwp GetAdcChannelInfo()
        {
            return m_adcInfo;
        }
    }
}