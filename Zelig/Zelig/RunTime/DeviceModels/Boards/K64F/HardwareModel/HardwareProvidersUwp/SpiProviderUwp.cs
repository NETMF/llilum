//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.K64F
{
    using System;
    using Runtime;
    using Chipset = Microsoft.CortexM4OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;

    public sealed class SpiProviderUwp : Runtime.SpiProviderUwp
    {
        private static readonly string[] m_spiDevices = { "SPI0", "SPI1", "SPI2" };

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

        public static readonly SpiChannelInfoUwp SPI2 = new SpiChannelInfoUwp()
        {
            ChipSelectLines = 1,
            MinFreq = 1000,
            MaxFreq = 30000000,
            Supports16 = true,
            ChannelInfoKernel = SpiProvider.SPI2,
        };

        public override SpiChannelInfoUwp GetSpiChannelInfo(string busId)
        {
            switch (busId)
            {
                case "SPI0":
                    return SPI0;
                case "SPI1":
                    return SPI1;
                case "SPI2":
                    return SPI2;
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
