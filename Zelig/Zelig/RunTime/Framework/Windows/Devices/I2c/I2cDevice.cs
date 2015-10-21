//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Windows.Devices.I2c
{
    using System;
    using System.Runtime.CompilerServices;
    using Llilum = Microsoft.Llilum.Devices.I2c;

    public sealed class I2cDevice : IDisposable
    {
        private struct I2cChannelContainer
        {
            public Llilum.I2cDevice Channel;
            public object TransactionLock;
            public bool Exclusive;
            public int RefCount;
        }

        // Initialize this array to the number of I2C channels on this board
        static I2cChannelContainer[] channels = new I2cChannelContainer[I2cChannelCount()];

        // This lock is used for creating and removing channels
        internal static object channelLock = new object();

        // Bus speed constants
        private const int HIGH_SPEED_FREQUENCY = 400000;
        private const int LOW_SPEED_FREQUENCY = 100000;

        private readonly I2cConnectionSettings m_settings;
        private readonly string m_deviceId;
        private I2cChannelContainer m_channelContainer;
        private bool m_disposed;
        
        private I2cDevice(I2cChannelContainer channelContainer, string deviceId, I2cConnectionSettings settings)
        {
            m_channelContainer = channelContainer;
            m_deviceId = deviceId;
            m_settings = settings;
            m_disposed = false;
        }

        /// <summary>Closes the connection to the inter-integrated circuit (I2C) device.</summary>
        public void Dispose()
        {
            if (!m_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                m_disposed = true;
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Ensure that channel creation and destruction do not collide
                lock(channelLock)
                {
                    if (System.Threading.Interlocked.Decrement(ref m_channelContainer.RefCount) == 0)
                    {
                        // There are no more references to the channel. Release the pins and channel
                        m_channelContainer.Channel.Dispose();
                        m_channelContainer.Channel = null;
                    }
                    m_disposed = true;
                }
            }
        }


        /// <summary>Gets the connection settings used for communication with the inter-integrated circuit (I2C) device.</summary>
		/// <returns>The connection settings used for communication with the inter-integrated circuit (I2C) device.</returns>
		public I2cConnectionSettings ConnectionSettings
        {
            get
            {
                return m_settings;
            }
        }

        /// <summary>
        /// Gets the plug and play device identifier of the inter-integrated circuit (I2C) bus controller for the device.
        /// </summary>
        public string DeviceId
        {
            get
            {
                return m_deviceId;
            }
        }

        /// <summary>
        /// Retrieves an I2cDevice object asynchronously for the inter-integrated circuit (I2C) bus controller that has the specified plug and play device identifier, using the specified connection settings.
        /// </summary>
        /// <param name="deviceId">Device identifier</param>
        /// <param name="settings">The connection settings to use for communication with the I2C bus controller that deviceId specifies.</param>
        /// <returns></returns>
        public static /*IAsyncOperation<I2cDevice>*/I2cDevice FromIdAsync(string deviceId, I2cConnectionSettings settings)
        {
            Llilum.II2cChannelInfo channelInfo = GetI2cChannelInfo(deviceId);
            if(channelInfo == null)
            {
                throw new ArgumentException();
            }

            int channelIndex = channelInfo.PortIndex;

            // Ensure that we do not double-create channels
            lock (channelLock)
            {
                if (channels[channelIndex].Channel != null)
                {
                    if (channels[channelIndex].Exclusive)
                    {
                        // The user is making a mistake by trying to access an exclusive resource
                        throw new InvalidOperationException("DeviceId is marked as exclusive");
                    }

                    // The channel is open and we have access to it. Increase the ref count
                    channels[channelIndex].RefCount++;
                }
                else
                {
                    // Perform failing operation first
                    channels[channelIndex].TransactionLock = new object();

                    // The channel has not been opened. We create it here
                    Llilum.I2cDevice newChannel = new Llilum.I2cDevice(channelInfo);
                    if (newChannel == null)
                    {
                        // We were unable to get a channel. It is either busy or does not exist
                        throw new ArgumentException();
                    }

                    // Set the new channel frequency
                    if (settings.BusSpeed == I2cBusSpeed.FastMode)
                    {
                        newChannel.Frequency = HIGH_SPEED_FREQUENCY;
                    }
                    else
                    {
                        // Default
                        newChannel.Frequency = LOW_SPEED_FREQUENCY;
                    }

                    newChannel.Open();

                    channels[channelIndex].Channel = newChannel;
                    channels[channelIndex].Exclusive = (settings.SharingMode == I2cSharingMode.Exclusive);
                    channels[channelIndex].RefCount = 1;
                }
            }

            return new I2cDevice(channels[channelIndex], deviceId, settings);
        }

        /// <summary>Writes data to the inter-integrated circuit (I2C) bus on which the device is connected, based on the bus address specified in the I2cConnectionSettings object that you used to create the I2cDevice object.</summary>
        /// <param name="buffer">A buffer that contains the data that you want to write to the I2C device. This data should not include the bus address.</param>
        public void Write(byte[] buffer)
        {
            ThrowIfDisposed();

            int transferCount = WriteImpl(buffer, true);
            CheckAndThrowTransferException(transferCount, buffer.Length);
        }

        /// <summary>Writes data to the inter-integrated circuit (I2C) bus on which the device is connected, based on the bus address specified in the I2cConnectionSettings object that you used to create the I2cDevice object.</summary>
        /// <param name="buffer">A buffer that contains the data that you want to write to the I2C device. This data should not include the bus address.</param>
        public I2cTransferResult WritePartial(byte[] buffer)
        {
            ThrowIfDisposed();

            I2cTransferResult result = new I2cTransferResult();
            int transferCount = WriteImpl(buffer, true);
            if(transferCount < 0)
            {
                // Device not found at given address
                result.BytesTransferred = 0;
                result.Status = I2cTransferStatus.SlaveAddressNotAcknowledged;
            }
            else if(transferCount < buffer.Length)
            {
                // Transfer was interrupted
                result.BytesTransferred = (uint)transferCount;
                result.Status = I2cTransferStatus.PartialTransfer;
            }
            else
            {
                result.BytesTransferred = (uint)transferCount;
                result.Status = I2cTransferStatus.FullTransfer;
            }

            return result;
        }

        /// <summary>Reads data from the inter-integrated circuit (I2C) bus on which the device is connected into the specified buffer.</summary>
        /// <param name="buffer">The buffer to which you want to read the data from the I2C bus. The length of the buffer determines how much data to request from the device.</param>
        public void Read(byte[] buffer)
        {
            ThrowIfDisposed();

            int transferCount = ReadImpl(buffer, true);
            CheckAndThrowTransferException(transferCount, buffer.Length);
        }

        /// <summary>Reads data from the inter-integrated circuit (I2C) bus on which the device is connected into the specified buffer.</summary>
        /// <param name="buffer">The buffer to which you want to read the data from the I2C bus. The length of the buffer determines how much data to request from the device.</param>
        public I2cTransferResult ReadPartial(byte[] buffer)
        {
            ThrowIfDisposed();

            I2cTransferResult result = new I2cTransferResult();
            int transferCount = ReadImpl(buffer, true);
            if (transferCount < 0)
            {
                // Device not found at given address
                result.BytesTransferred = 0;
                result.Status = I2cTransferStatus.SlaveAddressNotAcknowledged;
            }
            else if (transferCount < buffer.Length)
            {
                // Transfer was interrupted
                result.BytesTransferred = (uint)transferCount;
                result.Status = I2cTransferStatus.PartialTransfer;
            }
            else
            {
                result.BytesTransferred = (uint)transferCount;
                result.Status = I2cTransferStatus.FullTransfer;
            }

            return result;
        }

        /// <summary>Performs an atomic operation to write data to and then read data from the inter-integrated circuit (I2C) bus on which the device is connected, and sends a restart condition between the write and read operations.</summary>
        /// <param name="writeBuffer">A buffer that contains the data that you want to write to the I2C device. This data should not include the bus address.</param>
        /// <param name="readBuffer">The buffer to which you want to read the data from the I2C bus. The length of the buffer determines how much data to request from the device.</param>
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            ThrowIfDisposed();

            // Do not send a stop bit on WriteRead for repeated start
            int transferCount = WriteImpl(writeBuffer, false);
            CheckAndThrowTransferException(transferCount, writeBuffer.Length);

            transferCount = ReadImpl(readBuffer, true);
            CheckAndThrowTransferException(transferCount, readBuffer.Length);
        }

        /// <summary>Performs an atomic operation to write data to and then read data from the inter-integrated circuit (I2C) bus on which the device is connected, and sends a restart condition between the write and read operations.</summary>
        /// <param name="writeBuffer">A buffer that contains the data that you want to write to the I2C device. This data should not include the bus address.</param>
        /// <param name="readBuffer">The buffer to which you want to read the data from the I2C bus. The length of the buffer determines how much data to request from the device.</param>
        public I2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer)
        {
            ThrowIfDisposed();

            I2cTransferResult result = new I2cTransferResult();

            // Do not send a stop bit on WriteRead for repeated start
            int transferCountWrite = WriteImpl(writeBuffer, false);

            if(transferCountWrite < writeBuffer.Length)
            {
                if(transferCountWrite < 0)
                {
                    // Device not found at given address
                    result.BytesTransferred = 0;
                    result.Status = I2cTransferStatus.SlaveAddressNotAcknowledged;
                }
                else
                {
                    // Transfer was interrupted
                    result.BytesTransferred = (uint)transferCountWrite;
                    result.Status = I2cTransferStatus.PartialTransfer;
                }
                return result;
            }

            int transferCountRead = ReadImpl(readBuffer, true);
            if (transferCountRead < readBuffer.Length)
            {
                if (transferCountRead < 0)
                {
                    // Device not found at given address
                    result.BytesTransferred = 0;
                    result.Status = I2cTransferStatus.SlaveAddressNotAcknowledged;
                }
                else
                {
                    // Transfer was interrupted
                    result.BytesTransferred = (uint)transferCountRead;
                    result.Status = I2cTransferStatus.PartialTransfer;
                }
                return result;
            }

            result.BytesTransferred = (uint)(transferCountWrite + transferCountRead);
            result.Status = I2cTransferStatus.FullTransfer;

            return result;
        }

        /// <summary>Retrieves an Advanced Query Syntax (AQS) string for all of the inter-integrated circuit (I2C) bus controllers on the system.</summary>
        /// <returns>An AQS string for all of the I2C bus controllers on the system, which you can use with the DeviceInformation.FindAllAsync method to get DeviceInformation objects for those bus controllers.</returns>
        public static string GetDeviceSelector()
        {
            string[] channels = GetI2cChannels();
            string allChannels = "";

            if (channels != null)
            {
                foreach (string channel in channels)
                {
                    allChannels += (channel + ";");
                }
            }

            return allChannels;
        }

        /// <summary>
        /// Retrieves an Advanced Query Syntax (AQS) string for the inter-integrated circuit (I2C) bus that has the specified friendly name
        /// </summary>
        /// <param name="friendlyName">A friendly name for the particular I²C bus on a particular hardware platform for which you want to get the AQS string.</param>
        /// <returns>An AQS string for the I²C bus that friendlyName specifies, which you can use with the DeviceInformation.FindAllAsync method to get a DeviceInformation object for that bus.</returns>
        public static string GetDeviceSelector(string friendlyName)
        {
            string[] channels = GetI2cChannels();

            foreach (string channel in channels)
            {
                if (channel.Equals(friendlyName))
                {
                    return friendlyName;
                }
            }

            return null;
        }

        //--//

        /// <summary>
        /// Writes the buffer over the I2C channel
        /// </summary>
        /// <param name="buffer">Data to write</param>
        /// <param name="sendStop">Send stop bit, or repeat start</param>
        /// <returns>Number of bytes written</returns>
        private int WriteImpl(byte[] buffer, bool sendStop)
        {
            lock(m_channelContainer.TransactionLock)
            {
                ChangeFrequencyIfneeded();
                return m_channelContainer.Channel.Write(buffer, m_settings.SlaveAddress, 0, buffer.Length, sendStop);
            }
        }

        /// <summary>
        /// Reads from the I2C bus into the buffer
        /// </summary>
        /// <param name="buffer">Placeholder for read data</param>
        /// <param name="sendStop">Send stop bit, or repeat</param>
        /// <returns>Number of bytes read</returns>
        private int ReadImpl(byte[] buffer, bool sendStop)
        {
            lock (m_channelContainer.TransactionLock)
            {
                ChangeFrequencyIfneeded();
                return m_channelContainer.Channel.Read(buffer, m_settings.SlaveAddress, 0, buffer.Length, sendStop);
            }
        }

        /// <summary>
        /// Since channels are shared between devices, we need this helper method to check the
        /// current channel frequency, and change it if the device being used needs a different one
        /// </summary>
        private void ChangeFrequencyIfneeded()
        {
            int requiredFrequency = (m_settings.BusSpeed == I2cBusSpeed.FastMode) ? HIGH_SPEED_FREQUENCY : LOW_SPEED_FREQUENCY;
            if (m_channelContainer.Channel.Frequency != requiredFrequency)
            {
                m_channelContainer.Channel.Frequency = requiredFrequency;
            }
        }

        /// <summary>
        /// If the object is disposed, it should not be allowed to do anything
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        /// <summary>
        /// Throw an exception after an I2C transfer, if necessary
        /// </summary>
        /// <param name="transferred">Number of bytes transferred</param>
        /// <param name="bufferLength">Number of bytes that should have been transferred</param>
        private void CheckAndThrowTransferException(int transferred, int bufferLength)
        {
            if (transferred < 0)
            {
                throw new Exception("The bus address was not acknowledged.");
            }
            else if(transferred < bufferLength)
            {
                throw new Exception("The I2C device negatively acknowledged the data transfer before the entire buffer was written.");
            }
        }

        //--//

        /// <summary>
        /// Gets the number of I2C channels on the current board
        /// </summary>
        /// <returns>Number of I2C channels on the current board</returns>
        private static int I2cChannelCount()
        {
            return GetI2cChannels().Length;
        }
        
        //--//
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern string[] GetI2cChannels();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern Llilum.II2cChannelInfo GetI2cChannelInfo(string busId);
    }
}
