//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32F401
{
    using System;
    using Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM4OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM4;

    public sealed class I2cProviderUwp : Microsoft.Zelig.Runtime.I2cProviderUwp
    {
        private static string[] m_i2cChannels = { "I2C0" };

        public override I2cChannelInfo GetI2cChannelInfo(string busId)
        {
            switch (busId)
            {
                case "I2C0":
                    return I2cProvider.I2C0;
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