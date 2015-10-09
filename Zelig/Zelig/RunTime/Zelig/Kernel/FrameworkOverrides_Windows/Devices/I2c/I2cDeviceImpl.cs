//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using Windows.Devices.I2c;
    using Windows.Devices.I2c.Provider;

    //--//

    [ExtendClass(typeof(I2cDevice), NoConstructors = true)]
    public class I2cDeviceImpl
    {
        /// <summary>
        /// Gets the index of the I2C channel by its deviceId
        /// </summary>
        /// <param name="deviceId">I2C Channel ID</param>
        /// <returns>Index of the I2C channel or -1</returns>
        private static int GetI2cChannelIndex(string deviceId)
        {
            return HardwareProvider.Instance.GetI2cChannelIndexFromString(deviceId);
        }

        /// <summary>
        /// Returns an array of I2C channel identifiers
        /// </summary>
        /// <returns>I2C channel identifiers</returns>
        private static string[] GetI2cChannels()
        {
            return HardwareProvider.Instance.GetI2CChannels();
        }

        /// <summary>
        /// Creates an I2C channel and reserves the pins for it
        /// </summary>
        /// <param name="index">I2C channel index</param>
        /// <returns>I2C Channel</returns>
        private static I2cChannel AcquireI2cChannel(int index)
        {
            HardwareProvider provider = HardwareProvider.Instance;

            int sdaPin, sclPin;
            if(!provider.GetI2CPinsFromChannelIndex(index, out sdaPin, out sclPin))
            {
                // Could not get pins for the given index
                return null;
            }

            // Ensure all pins are available
            if (!provider.TryReservePins(sdaPin, sclPin))
            {
                return null;
            }

            I2cChannel newChannel = provider.CreateI2cChannel();
            newChannel.Initialize(sdaPin, sclPin);

            return newChannel;
        }

        /// <summary>
        /// Releases the pins associated with the given I2C bus
        /// </summary>
        /// <param name="deviceId">I2C channel ID</param>
        public static void ReleaseI2cBus(string deviceId)
        {
            HardwareProvider provider = HardwareProvider.Instance;
            int index = HardwareProvider.Instance.GetI2cChannelIndexFromString(deviceId);
            if(index >= 0)
            {
                int sdaPin, sclPin;
                if (provider.GetI2CPinsFromChannelIndex(index, out sdaPin, out sclPin))
                {
                    provider.ReleasePins(sdaPin, sclPin);
                }
            }
        }
    }
}
