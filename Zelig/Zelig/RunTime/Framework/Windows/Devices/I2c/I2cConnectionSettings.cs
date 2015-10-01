//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Windows.Devices.I2c
{
    /// <summary>Represents the connection settings you want to use for an inter-integrated 
    /// circuit (I2C) device. This API is for evaluation purposes only and is subject to change 
    /// or removal.</summary>
    public sealed class I2cConnectionSettings
    {
        public I2cConnectionSettings(int slaveAddress)
        {
            SlaveAddress = slaveAddress;
        }

        /// <summary>Gets or sets the bus address of the inter-integrated circuit (I2C) device.</summary>
        /// <returns>The bus address of the I2C device. Only 7-bit addressing is supported, so the range of values that are valid is from 8 to 119.</returns>
        public int SlaveAddress
        {
            get;
            set;
        }

        public I2cSharingMode SharingMode
        {
            get;
            set;
        }

        /// <summary>Gets or sets the bus speed to use for connecting to an inter-integrated circuit (I2C) device. The bus speed is the frequency at which to clock the I2C bus when accessing the device.</summary>
        /// <returns>The bus speed to use for connecting to anI2C device.</returns>
        public I2cBusSpeed BusSpeed
        {
            get;
            set;
        }
    }
}
