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
        private static readonly string[] m_spiDevices = { "SPI0", "SPI1", "SPI0WriteOnly" };

        public static readonly SpiChannelInfo SPI0WriteOnlyChannelInfo = new SpiChannelInfo()
        {
            Mosi = SpiProvider.SPI0.Mosi,
            Miso = unchecked((int)PinName.NC),
            Sclk = SpiProvider.SPI0.Sclk,
            DefaultChipSelect = SpiProvider.SPI0.DefaultChipSelect,
            SetupTime = SpiProvider.SPI0.SetupTime,
            HoldTime = SpiProvider.SPI0.HoldTime,
            ActiveLow = SpiProvider.SPI0.ActiveLow,
        };

        public static readonly SpiChannelInfoUwp SPI0 = new SpiChannelInfoUwp()
        {
            ChipSelectLines = 1,
            MinFreq = 1000,
            MaxFreq = 30000000,
            Supports16 = true,
            ChannelInfoKernel = SpiProvider.SPI0,
        };

        public static readonly SpiChannelInfoUwp SPI0WriteOnly = new SpiChannelInfoUwp()
        {
            ChipSelectLines = 1,
            MinFreq = 1000,
            MaxFreq = 30000000,
            Supports16 = true,
            ChannelInfoKernel = SPI0WriteOnlyChannelInfo,
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
                case "SPI0WriteOnly":
                    return SPI0WriteOnly;
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
