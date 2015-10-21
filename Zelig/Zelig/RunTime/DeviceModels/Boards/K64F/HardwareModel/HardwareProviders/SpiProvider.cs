//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.K64F
{
    using System;
    using Runtime;
    using Chipset = Microsoft.CortexM4OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM4;

    public sealed class SpiProvider : Chipset.HardwareModel.SpiProvider
    {
        public static readonly SpiChannelInfo SPI0 = new SpiChannelInfo()
        {
            Mosi = (int)PinName.PTD2,
            Miso = unchecked((int)PinName.NC),
            Sclk = (int)PinName.PTD1,
            ChipSelect = unchecked((int)PinName.NC),
            SetupTime = 10,
            HoldTime = 10,
            ReserveMisoPin = true,
            ActiveLow = true,
        };

        public static readonly SpiChannelInfo SPI1 = new SpiChannelInfo()
        {
            Mosi = (int)PinName.PTD6,
            Miso = (int)PinName.PTD7,
            Sclk = (int)PinName.PTD5,
            ChipSelect = (int)PinName.PTD4,
            SetupTime = 10,
            HoldTime = 10,
            ReserveMisoPin = true,
            ActiveLow = true,
        };

        public static readonly SpiChannelInfo SPI2 = new SpiChannelInfo()
        {
            Mosi = (int)PinName.PTE3,
            Miso = (int)PinName.PTE1,
            Sclk = (int)PinName.PTE2,
            ChipSelect = (int)PinName.PTE4,
            SetupTime = 10,
            HoldTime = 10,
            ReserveMisoPin = true,
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
                case 2:
                    return SPI2;
                default:
                    return null;
            }
        }
    }
}
