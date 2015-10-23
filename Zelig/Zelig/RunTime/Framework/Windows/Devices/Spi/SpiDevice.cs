//
// Copyright (c) Microsoft Corporation. All rights reserved.
//


namespace Windows.Devices.Spi
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Llilum = Microsoft.Llilum.Devices.Spi;
    using Windows.Devices.Spi.Provider;

    //--//

    public sealed class SpiDevice : IDisposable
    {
        // Connection 
        private bool m_disposed;
        private readonly Llilum.SpiDevice m_channel;
        private readonly SpiConnectionSettings m_connectionSettings;
        private readonly String m_deviceId;

        /// <summary>
        /// Private SpiDevice constructor
        /// </summary>
        internal SpiDevice(string busId, SpiConnectionSettings settings, Llilum.SpiDevice channel)
        {
            m_deviceId = busId;
            m_connectionSettings = settings;
            m_channel = channel;
        }

        ~SpiDevice()
        {
            Dispose(false);
        }

        /// <summary>
        /// Closes resources associated with this SPI device
        /// </summary>
        public void Dispose()
        {
            if (!m_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                m_disposed = true;
            }
        }

        /// <summary>
        /// Disposes of all resources associated with this SPI device
        /// </summary>
        /// <param name="disposing">True if called from Dispose, false if called from the finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_channel.Dispose();
            }
        }

        /// <summary>
        /// Opens a device with the connection settings provided.
        /// </summary>
        /// <param name="busId">The id of the bus.</param>
        /// <param name="settings">The connection settings.</param>
        /// <returns>The SPI device requested.</returns>
        /// [RemoteAsync]
        public static SpiDevice FromIdAsync(string busId, SpiConnectionSettings settings)
        {
            Llilum.ISpiChannelInfoUwp channelInfoUwp = GetSpiChannelInfo(busId);
            if (channelInfoUwp == null)
            {
                throw new InvalidOperationException();
            }

            Llilum.ISpiChannelInfo channelInfo = channelInfoUwp.ChannelInfo;
            if (channelInfo == null)
            {
                throw new InvalidOperationException();
            }

            Llilum.SpiDevice spiChannel = new Llilum.SpiDevice(channelInfo, settings.ChipSelectLine);
            if (spiChannel == null)
            {
                throw new InvalidOperationException();
            }

            spiChannel.ClockFrequency = settings.ClockFrequency;
            spiChannel.DataBitLength = settings.DataBitLength;
            spiChannel.Mode = (Llilum.SpiMode)settings.Mode;
            spiChannel.Open();

            return new SpiDevice(busId, settings, spiChannel);
        }


        /// <summary>
        /// Gets the connection settings for the device.
        /// </summary>
        /// <value>The connection settings.</value>
        public SpiConnectionSettings ConnectionSettings
        {
            get
            {
                return m_connectionSettings;
            }
        }

        /// <summary>
        /// Gets the unique ID associated with the device.
        /// </summary>
        /// <value>The ID.</value>
        public string DeviceId
        {
            get
            {
                return m_deviceId;
            }
        }

        /// <summary>
        /// Retrieves the info about a certain bus.
        /// </summary>
        /// <param name="busId">The id of the bus.</param>
        /// <returns>The bus info requested.</returns>
        public static SpiBusInfo GetBusInfo(string busId)
        {
            Llilum.ISpiChannelInfoUwp channelInfo = GetSpiChannelInfo(busId);
            if (channelInfo == null)
            {
                return null;
            }

            var supportedDataBitLengths = new List<int>()
            {
                8,
            };
            if (channelInfo.Supports16)
            {
                supportedDataBitLengths.Add(16);
            }

            return new SpiBusInfo()
            {
                ChipSelectLineCount = channelInfo.ChipSelectLines,
                MaxClockFrequency = channelInfo.MaxFreq,
                MinClockFrequency = channelInfo.MinFreq,
                SupportedDataBitLengths = supportedDataBitLengths,
            };
        }

        /// <summary>
        /// Gets all the SPI buses found on the system.
        /// </summary>
        /// <returns>String containing all the buses found on the system.</returns>
        /// [Overload("GetDeviceSelector")]
        public static string GetDeviceSelector()
        {
            string[] channels = GetSpiChannels();
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
        /// Gets all the SPI buses found on the system that match the input parameter.
        /// </summary>
        /// <param name="friendlyName">Input parameter specifying an identifying name for the desired bus. This usually corresponds to a name on the schematic.</param>
        /// <returns>String containing all the buses that have the input in the name.</returns>
        public static string GetDeviceSelector(string friendlyName)
        {
            // UWP prescribes returning the channels as one single string that looks like the following: "CH1;CH2;"
            string[] channels = GetSpiChannels();

            foreach (string channel in channels)
            {
                if (channel.Equals(friendlyName))
                {
                    return friendlyName;
                }
            }

            return null;
        }

        /// <summary>
        /// Reads from the connected device.
        /// </summary>
        /// <param name="buffer">Array containing data read from the device</param>
        public void Read(byte[] buffer)
        {
            // Read sends buffer.Length 0s, and places read values into buffer
            if (buffer != null && buffer.Length > 0)
            {
                m_channel.WriteRead(null, buffer, 0);
            }
        }

        /// <summary>
        /// Transfer data using a full duplex communication system.
        /// </summary>
        /// <param name="writeBuffer">Array containing data to write to the device.</param>
        /// <param name="readBuffer">Array containing data read from the device.</param>
        public void TransferFullDuplex(byte[] writeBuffer, byte[] readBuffer)
        {
            if (writeBuffer != null && writeBuffer.Length > 0)
            {
                int readOffset = writeBuffer.Length;
                if (readBuffer != null)
                {
                    // If the read buffer is smaller than the write buffer, we want to read
                    // the last part of the transmission. Otherwise, read from the start
                    readOffset = writeBuffer.Length - readBuffer.Length;
                    if (readOffset < 0)
                    {
                        readOffset = 0;
                    }
                }
                m_channel.WriteRead(writeBuffer, readBuffer, readOffset);
            }
        }

        /// <summary>
        /// Transfer data sequentially to the device.
        /// </summary>
        /// <param name="writeBuffer">Array containing data to write to the device.</param>
        /// <param name="readBuffer">Array containing data read from the device.</param>
        public void TransferSequential(byte[] writeBuffer, byte[] readBuffer)
        {
            Write(writeBuffer);
            Read(readBuffer);
        }

        /// <summary>
        /// Writes to the connected device.
        /// </summary>
        /// <param name="buffer">Array containing the data to write to the device.</param>
        public void Write(byte[] buffer)
        {
            if (buffer != null && buffer.Length > 0)
            {
                m_channel.WriteRead(buffer, null, 0);
            }
        }

        //--//

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern Llilum.ISpiChannelInfoUwp GetSpiChannelInfo(string busId);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern string[] GetSpiChannels();
    }
}
