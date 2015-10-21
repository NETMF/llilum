//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using Windows.Devices.I2c;
    using Llilum = Microsoft.Llilum.Devices.I2c;

    [ExtendClass(typeof(I2cDevice), NoConstructors = true)]
    public class I2cDeviceImplUwp
    {
        /// <summary>
        /// Returns an array of I2C channel identifiers
        /// </summary>
        /// <returns>I2C channel identifiers</returns>
        public static string[] GetI2cChannels()
        {
            return I2cProviderUwp.Instance.GetI2cChannels();
        }

        public static Llilum.II2cChannelInfo GetI2cChannelInfo(string busId)
        {
            return I2cProviderUwp.Instance.GetI2cChannelInfo(busId);
        }
    }
}
