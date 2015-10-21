//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using Microsoft.Llilum.Devices.I2c;

    [ExtendClass(typeof(I2cDevice), NoConstructors = true)]
    public class I2cDeviceImpl
    {
        public static I2cChannel TryAcquireI2cChannel(int port)
        {
            I2cChannelInfo channelInfo = I2cProvider.Instance.GetI2cChannelInfo(port);

            return TryAcquireI2cChannel(channelInfo);
        }

        public static I2cChannel TryAcquireI2cChannel(II2cChannelInfo channelInfo)
        {
            I2cProvider i2cProvider = I2cProvider.Instance;

            if (channelInfo == null)
            {
                return null;
            }

            return i2cProvider.CreateI2cChannel(channelInfo);
        }
    }
}
