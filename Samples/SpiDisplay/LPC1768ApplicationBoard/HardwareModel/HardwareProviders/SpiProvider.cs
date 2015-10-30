//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LPC1768
{
    using System;
    using Runtime;
    using Chipset = Microsoft.CortexM3OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;

    public sealed class SpiProvider : Chipset.HardwareModel.SpiProvider
    {
        public static readonly SpiChannelInfo SPI0 = new SpiChannelInfo() {
            Mosi = (int)PinName.p5,
            Miso = unchecked((int)PinName.NC),
            Sclk = (int)PinName.p7,
            DefaultChipSelect = unchecked((int)PinName.NC),
            SetupTime = 0,
            HoldTime = 0,
            ActiveLow = true,
        };

        public static readonly SpiChannelInfo SPI1 = new SpiChannelInfo()
        {
            Mosi = (int)PinName.p11,
            Miso = (int)PinName.p12,
            Sclk = (int)PinName.p13,
            DefaultChipSelect = (int)PinName.p14,
            SetupTime = 10,
            HoldTime = 10,
            ActiveLow = true,
        };

        public override bool SpiBusySupported
        {
            get
            {
                return true;
            }
        }

        public override SpiChannelInfo GetSpiChannelInfo(int id)
        {
            switch (id)
            {
                case 0:
                    return SPI0;
                case 1:
                    return SPI1;
                default:
                    return null;
            }
        }
    }
}
