//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32L152
{
    using System;
    using Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM3OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;

    public sealed class SpiProvider : Chipset.HardwareModel.SpiProvider
    {
        public static readonly SpiChannelInfo SPI0 = new SpiChannelInfo() {
            Mosi = (int)PinName.PA_7,
            Miso = (int)PinName.PA_6,
            Sclk = (int)PinName.PA_5,
            DefaultChipSelect = unchecked((int)PinName.NC),
            SetupTime = 0,
            HoldTime = 0,
            ActiveLow = true,
        };

        public static readonly SpiChannelInfo SPI1 = new SpiChannelInfo()
        {
            Mosi = (int)PinName.PB_5,
            Miso = (int)PinName.PB_4,
            Sclk = (int)PinName.PB_3,
            DefaultChipSelect = unchecked((int)PinName.NC),
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
