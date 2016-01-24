//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32F091
{
    using System;
    using Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM0OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM0;

    public sealed class SpiProvider : Chipset.HardwareModel.SpiProvider
    {
        public static readonly SpiChannelInfo SPI0 = new SpiChannelInfo()
        {
            Mosi = (int)PinName.PA_7,
            Miso = (int)PinName.PA_6,
            Sclk = (int)PinName.PA_5,
            DefaultChipSelect = unchecked((int)PinName.NC),
            SetupTime = 10,
            HoldTime = 10,
            ActiveLow = true,
        };

        public static readonly SpiChannelInfo SPI1 = new SpiChannelInfo()
        {
            Mosi = (int)PinName.PB_15,
            Miso = (int)PinName.PB_14,
            Sclk = (int)PinName.PB_13,
            DefaultChipSelect = unchecked((int)PinName.NC),
            SetupTime = 10,
            HoldTime = 10,
            ActiveLow = true,
        };

        public override bool SpiBusySupported
        {
            get
            {
                return false;
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
