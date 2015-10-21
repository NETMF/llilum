//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.Devices.Spi
{
    using System;
    using System.Runtime.CompilerServices;

    public class SpiDevice : IDisposable
    {
        private SpiChannel m_spiChannel;
        private int m_chipSelectPin;
        private bool m_disposed = false;

        const int defaultFrequency = 100000;
        const SpiMode defaultMode = SpiMode.Cpol0Cpha0;
        const int defaultBitLength = 8;
        const bool defaultIsSlave = false;

        /// <summary>
        /// SpiDevice constructor
        /// </summary>
        /// <param name="portIndex">Index of the port that maps to the board assembly</param>
        public SpiDevice(int portIndex)
        {
            m_spiChannel = AcquireSpiChannel(portIndex);
            if (m_spiChannel == null)
            {
                throw new ArgumentException();
            }

            // Set default values
            Initialize();
        }

        /// <summary>
        /// SpiDevice constructor
        /// </summary>
        /// <param name="channelInfo">Channel information necesssary to create a SPI channel</param>
        public SpiDevice(ISpiChannelInfo channelInfo)
        {
            m_spiChannel = AcquireSpiChannel(channelInfo);
            if (m_spiChannel == null)
            {
                throw new ArgumentException();
            }

            // Set default values
            Initialize();
        }

        ~SpiDevice()
        {
            Dispose(false);
        }

        /// <summary>
        /// Initializes the default values
        /// </summary>
        private void Initialize()
        {
            SetChipSelect(m_spiChannel.GetChannelInfo().ChipSelect, true);
            DataBitLength = defaultBitLength;
            Mode = defaultMode;
            ClockFrequency = defaultFrequency;
            IsSlave = defaultIsSlave;
        }

        private void ThrowIfDisposed()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(null);
            }
        }

        /// <summary>
        /// Initializes the SPI port to the values currently set in the channel
        /// </summary>
        public void Open()
        {
            ThrowIfDisposed();

            ISpiChannelInfo channelInfo = m_spiChannel.GetChannelInfo();

            m_spiChannel.SetupPins(channelInfo, (ChipSelectPin != channelInfo.ChipSelect), ChipSelectPin);
            m_spiChannel.SetupChannel(DataBitLength, Mode, IsSlave);
            m_spiChannel.SetupTiming(ClockFrequency, channelInfo.SetupTime, channelInfo.HoldTime);
        }

        /// <summary>
        /// Is the device operating in slave mode.
        /// Slave mode is currently not supported
        /// </summary>
        public bool IsSlave { get; private set; }
        
        /// <summary>
        /// Gets or sets the chip select line for the connection to the SPI device.
        /// </summary>
        /// <value>The chip select line.</value>
        public int ChipSelectPin
        {
            get
            {
                return m_chipSelectPin;
            }
            set
            {
                SetChipSelect(value, false);
            }
        }

        /// <summary>
        /// Gets or sets the clock frequency for the connection.
        /// </summary>
        /// <value>Value of the clock frequency in Hz.</value>
        public int ClockFrequency { get; set; }

        /// <summary>
        /// Gets or sets the bit length for data on this connection.
        /// </summary>
        /// <value>The data bit length.</value>
        public int DataBitLength { get; set; }

        /// <summary>
        /// Gets or sets the SpiMode for this connection.
        /// </summary>
        /// <value>The communication mode.</value>
        public SpiMode Mode { get; set; }

        /// <summary>
        /// Performs a SPI transaction. Either writeBuffer, readBuffer, or both must be non-empty
        /// </summary>
        /// <param name="writeBuffer">Bytes to write. If null, writes 0x0 for read</param>
        /// <param name="readBuffer">Bytes to read. If null, only does write</param>
        /// <param name="startReadOffset">Index in the writeBuffer at which to start filling the readBuffer</param>
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer, int startReadOffset)
        {
            ThrowIfDisposed();

            m_spiChannel.WriteRead(writeBuffer, readBuffer, startReadOffset);
        }

        /// <summary>
        /// Acquires the chip select pin if a new one is entered and releases the old one.
        /// </summary>
        /// <param name="newPin">Pin number</param>
        /// <param name="initialSet">Is this the first time the CS pin is being set</param>
        private void SetChipSelect(int newPin, bool initialSet)
        {
            ThrowIfDisposed();

            if (initialSet)
            {
                m_chipSelectPin = newPin;
            }
            else
            {
                if (m_chipSelectPin != newPin)
                {
                    // Release the old CS pin and reserve the new one
                    if(TryChangeCsPin(m_chipSelectPin, newPin))
                    {
                        m_chipSelectPin = newPin;
                    }
                    else
                    {
                        // This would only happen if the new CS pin is busy
                        throw new InvalidOperationException();
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                ISpiChannelInfo channelInfo = m_spiChannel.GetChannelInfo();
                ReleaseSpiPins(channelInfo.Mosi, channelInfo.Miso, channelInfo.Sclk, ChipSelectPin);

                if (disposing)
                {
                    m_spiChannel.Dispose();
                }
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            m_disposed = true;
            GC.SuppressFinalize(this);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern SpiChannel AcquireSpiChannel(int port);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern SpiChannel AcquireSpiChannel(ISpiChannelInfo channelInfo);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool TryChangeCsPin(int oldPin, int newPin);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void ReleaseSpiPins(int mosiPin, int misoPin, int sclPin, int csPin);
    }
}
