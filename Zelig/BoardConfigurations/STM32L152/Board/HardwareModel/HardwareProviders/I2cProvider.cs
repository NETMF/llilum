//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32L152
{
    using System;
    using Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM3OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;

    public sealed class I2cProvider : Chipset.HardwareModel.I2cProvider
    {
        public static readonly I2cChannelInfo I2C0 = new I2cChannelInfo()
        {
            SdaPin      = (int)PinName.PB_9,
            SclPin      = (int)PinName.PB_8,
            PortIndex   = 0,
        };

        public static readonly I2cChannelInfo I2C1 = new I2cChannelInfo()
        {
            SdaPin      = (int)PinName.PB_10,
            SclPin      = (int)PinName.PB_6,
            PortIndex   = 1,
        };

        public override I2cChannelInfo GetI2cChannelInfo(int id)
        {
            switch (id)
            {
                case 0:
                    return I2C0;
                case 1:
                    return I2C1;
                default:
                    return null;
            }
        }
    }
}
