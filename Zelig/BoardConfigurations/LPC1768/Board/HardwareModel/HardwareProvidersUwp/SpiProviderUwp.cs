//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.LPC1768
{
    using System;
    using Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM3OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;

    public sealed class SpiProviderUwp : Microsoft.Zelig.Runtime.SpiProviderUwp
    {
        private static readonly string[] m_spiDevices = { "SPI0", "SPI1" };

        public static readonly SpiChannelInfoUwp SPI0 = new SpiChannelInfoUwp()
        {
            ChipSelectLines = 1,
            MinFreq = 1000,
            MaxFreq = 30000000,
            Supports16 = true,
            ChannelInfoKernel = SpiProvider.SPI0,
        };

        public static readonly SpiChannelInfoUwp SPI1 = new SpiChannelInfoUwp()
        {
            ChipSelectLines = 1,
            MinFreq = 1000,
            MaxFreq = 30000000,
            Supports16 = true,
            ChannelInfoKernel = SpiProvider.SPI1,
        };

        public override SpiChannelInfoUwp GetSpiChannelInfo(string busId)
        {
            switch (busId)
            {
                case "SPI0":
                    return SPI0;
                case "SPI1":
                    return SPI1;
                default:
                    return null;
            }
        }

        public override string[] GetSpiChannels()
        {
            return m_spiDevices;
        }
    }
}
