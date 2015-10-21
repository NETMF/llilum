//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.K64F
{
    using System;
    using Runtime;
    using Chipset = Microsoft.CortexM3OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;

    public sealed class AdcProviderUwp : Runtime.AdcProviderUwp
    {
        private static readonly AdcChannelInfoUwp m_adcInfo = new AdcChannelInfoUwp()
        {
            MaxValue            = UInt16.MaxValue,
            MinValue            = 0,
            ResolutionInBits    = 16,
            AdcPinNumbers       = new int[] { (int)PinName.A0, (int)PinName.A1, (int)PinName.A2, (int)PinName.A3, (int)PinName.A4, (int)PinName.A5 }
        };

        public override AdcChannelInfoUwp GetAdcChannelInfo()
        {
            return m_adcInfo;
        }
    }
}