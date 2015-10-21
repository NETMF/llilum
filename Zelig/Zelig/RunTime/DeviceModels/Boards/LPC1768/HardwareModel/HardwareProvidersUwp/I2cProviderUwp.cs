//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LPC1768
{
    using System;
    using Runtime;
    using Chipset = Microsoft.CortexM3OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;

    public sealed class I2cProviderUwp : Runtime.I2cProviderUwp
    {
        private static string[] m_i2cChannels = { "I2C0", "I2C1" };

        public override I2cChannelInfo GetI2cChannelInfo(string busId)
        {
            switch (busId)
            {
                case "I2C0":
                    return I2cProvider.I2C0;
                case "I2C1":
                    return I2cProvider.I2C1;
                default:
                    return null;
            }
        }

        public override string[] GetI2cChannels()
        {
            return m_i2cChannels;
        }
    }
}