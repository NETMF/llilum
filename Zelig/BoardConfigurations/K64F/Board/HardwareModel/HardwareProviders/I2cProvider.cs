//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.K64F
{
    using Microsoft.Zelig.Runtime;

    using Chipset = Microsoft.CortexM4OnMBED;


    public sealed class I2cProvider : Chipset.HardwareModel.I2cProvider
    {
        public static readonly I2cChannelInfo I2C0 = new I2cChannelInfo()
        {
            SclPin      = (int)PinName.I2C_SCL,
            SdaPin      = (int)PinName.I2C_SDA,
            PortIndex   = 0,
        };

        public override I2cChannelInfo GetI2cChannelInfo(int id)
        {
            switch (id)
            {
                case 0:
                    return I2C0;
                default:
                    return null;
            }
        }
    }
}
