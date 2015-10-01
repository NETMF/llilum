//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Windows.Devices.I2c
{
    public enum I2cBusSpeed
    {
        StandardMode,
        /// <summary>A fast speed of 400 kHz.</summary>
        FastMode
    }

    /// <summary>Describes the modes in which you can connect to an inter-integrated circuit (I2C)
    ///  bus address. These modes determine whether other connections to the I2C bus address can 
    /// be opened while you are connected to the I2C bus address. This API is for evaluation 
    /// purposes only and is subject to change or removal.</summary>
    public enum I2cSharingMode
    {
        Exclusive,
        /// <summary>Connects to the I2C bus address in shared mode, so that other connections 
        /// to the I2C bus address can be made while you remain connected. You can perform 
        /// all operations on shared connections, but use such connections with care. When 
        /// multiple client apps change the global state of the I2C device, race conditions 
        /// can result.An example use case for using a shared connection is a sensor that 
        /// obtains readings without changing the state of the device.</summary>
        Shared
    }

    /// <summary>Describes whether the data transfers that the ReadPartial, WritePartial, or 
    /// WriteReadPartial method performed succeeded, or provides the reason that the transfers 
    /// did not succeed.</summary>
    public enum I2cTransferStatus
    {
        FullTransfer,
        /// <summary>The I2C device negatively acknowledged the data transfer before all of 
        /// the data was transferred.For this status code, the value of the I2cTransferResult.BytesTransferred 
        /// member that the method returns is the number of bytes actually transferred. For 
        /// WriteReadPartial, the value is the sum of the number of bytes that the operation 
        /// wrote and the number of bytes that the operation read.</summary>
        PartialTransfer,
        SlaveAddressNotAcknowledged
    }
    
    public struct I2cTransferResult
    {
        public I2cTransferStatus Status;

        /// <summary>The actual number of bytes that the operation actually transferred. 
        /// The following table describes what this value represents for each 
        /// method.MethodDescriptionReadPartialThe actual number of bytes that the read 
        /// operation read into the buffer. If the value of the Status member is 
        /// I2CTransferStatus.PartialTransfer, this value may be less than the number of 
        /// bytes in the buffer that you specified in the buffer parameter. WritePartialThe 
        /// actual number of bytes that the write operation transferred to the I2C device. 
        /// If the value of the Status member is I2CTransferStatus.PartialTransfer, this 
        /// value may be less than the number of bytes in the buffer that you specified in 
        /// the buffer parameter. WriteReadPartial The actual number of bytes that the 
        /// operation transferred, which is the sum of the number of bytes that the operation 
        /// wrote and the number of bytes that the operation read. If the value of the Status 
        /// member is I2CTransferStatus.PartialTransfer, this value may be less than the sum 
        /// of lengths of the buffers that you specified in the writeBuffer and readBuffer 
        /// parameters.</summary>
        public uint BytesTransferred;
    }
}
