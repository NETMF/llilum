//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32F401
{
    using System;
    using Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM0OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM0;

    public sealed class SpiProviderUwp : Microsoft.Zelig.Runtime.SpiProviderUwp
    {
        // NOTICE: Names are 1-indexed as per STM Board Description
        private static readonly string[] m_spiDevices = { "SPI1", "SPI2" };

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
                case "SPI1":
                    return SPI0;
                case "SPI2":
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
