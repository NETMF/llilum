//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Zelig.Runtime;
    using Microsoft.Llilum.Devices.Spi;
    using LlilumGpio = Microsoft.Llilum.Devices.Gpio;

    public class SpiChannel : Microsoft.Llilum.Devices.Spi.SpiChannel
    {
        private unsafe SpiImpl* m_spi;
        private unsafe LlilumGpio.GpioPin m_altCsPin;
        private UInt16 m_dataWidth;
        private bool m_disposed;
        private int m_setupTimeInCycles;
        private int m_holdTimeInCycles;
        private bool m_activeLow;
        private ISpiChannelInfo m_channelInfo;

        //--//

        public SpiChannel(ISpiChannelInfo info)
        {
            m_channelInfo = info;
        }

        ~SpiChannel()
        {
            Dispose(false);
        }

        /// <summary>
        /// Closes resources associated with this SPI device
        /// </summary>
        public override void Dispose()
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
        private unsafe void Dispose(bool disposing)
        {
            // Native resources need to be freed unconditionally
            tmp_spi_free(m_spi);

            if (disposing)
            {
            }
        }

        /// <summary>
        /// Gets hardware information for this channel
        /// </summary>
        /// <returns></returns>
        public override ISpiChannelInfo GetChannelInfo()
        {
            return m_channelInfo;
        }

        /// <summary>
        /// Performs a synchronous transfer over the SpiChannel
        /// </summary>
        /// <param name="writeBuffer">Bytes to write. Leave null for read-only</param>
        /// <param name="readBuffer">Bytes to read. Leave null for write-only</param>
        /// <param name="startReadOffset">Index at which to start reading bytes</param>
        public unsafe override void WriteRead(byte[] writeBuffer, byte[] readBuffer, int startReadOffset)
        {

            // Enable the chip select
            if (m_altCsPin != null)
            {
                m_altCsPin.Write(m_activeLow ? 0 : 1);

                // Setup time in cycles
                Processor.Delay(m_setupTimeInCycles);
            }

            // Are we reading, writing, or both?
            bool isRead = (readBuffer != null) && readBuffer.Length > 0;
            bool isWrite = (writeBuffer != null) && writeBuffer.Length > 0;

            // We need to be at least one of these
            if (!isRead && !isWrite)
            {
                throw new ArgumentException();
            }

            // The number of times we will be transferring for 8bit transactions
            int transferCount = isWrite ? writeBuffer.Length : readBuffer.Length;

            if (m_dataWidth == 8)
            {
                // 8 bit transactions
                int i = 0;
                while (i < transferCount)
                {
                    int writeVal = isWrite ? writeBuffer[i] : 0;

                    int result = tmp_spi_master_write(m_spi, writeVal);

                    if (isRead && i >= startReadOffset)
                    {
                        readBuffer[i] = (byte)result;
                    }

                    i++;
                }
            }
            else
            {
                // 16 bit transactions
                int i = 0;
                while (i < transferCount)
                {
                    int writeVal = isWrite ? writeBuffer[i] | (writeBuffer[i + 1] << 8) : 0;

                    int result = tmp_spi_master_write(m_spi, writeVal);

                    if (isRead && i >= startReadOffset)
                    {
                        readBuffer[i] = (byte)(result & 0x000000FF);
                        readBuffer[i + 1] = (byte)((result & 0x0000FF00) >> 8);
                    }

                    i += 2;
                }

            }

            // Disable the chip select
            if (m_altCsPin != null)
            {
                // Some boards (K64F) do not support checking if SPI is busy through mbed
                if (SpiProvider.Instance.SpiBusySupported)
                {
                    while (tmp_spi_busy(m_spi) != 0)
                    {
                        // Spin until transaction is complete
                    }
                }

                // Hold time in cycles
                Processor.Delay(m_holdTimeInCycles);

                m_altCsPin.Write(m_activeLow ? 1 : 0);
            }
        }

        public unsafe override void SetupChannel(int bits, SpiMode mode, bool isSlave)
        {
            int slave = isSlave ? 1 : 0;

            // We will only support 8 and 16 bit transmissions
            if (bits > 8)
            {
                m_dataWidth = 16;
            }
            else
            {
                m_dataWidth = 8;
            }

            tmp_spi_format(m_spi, m_dataWidth, (int)mode, slave);
        }

        public unsafe override void SetupTiming(int frequencyInHz, int setupTime, int holdTime)
        {
            m_setupTimeInCycles = setupTime;
            m_holdTimeInCycles = holdTime;

            if (m_setupTimeInCycles < 0)
            {
                m_setupTimeInCycles = 100;
            }

            if (m_holdTimeInCycles < 0)
            {
                m_holdTimeInCycles = 100;
            }

            tmp_spi_frequency(m_spi, frequencyInHz);
        }

        /// <summary>
        /// Initializes the mbed SpiChannel with the appropriate pins
        /// </summary>
        /// <param name="channelInfo">SPI channel pin info</param>
        public unsafe override void SetupPins(ISpiChannelInfo channelInfo)
        {
            fixed (SpiImpl** spi_ptr = &m_spi)
            {
                tmp_spi_alloc_init(spi_ptr, channelInfo.Mosi, channelInfo.Miso, channelInfo.Sclk, channelInfo.DefaultChipSelect);
            }
        }

        /// <summary>
        /// Initializes the mbed SpiChannel with an alternate chip select pin
        /// </summary>
        /// <param name="channelInfo">SPI channel pin info</param>
        /// <param name="alternateCsPin">Manually toggled CS pin</param>
        public unsafe override void SetupPins(ISpiChannelInfo channelInfo, int alternateCsPin)
        {
            fixed (SpiImpl** spi_ptr = &m_spi)
            {
                // Since we are using an alternate CS pin, initialize CS with the Invalid Pin
                tmp_spi_alloc_init(spi_ptr, channelInfo.Mosi, channelInfo.Miso, channelInfo.Sclk, HardwareProvider.Instance.InvalidPin);
            }

            // Mbed assumes active low, so we only set up active low/high when using the alternate CS pin
            m_activeLow = channelInfo.ActiveLow;

            // Set up the pin, unless it's the invalid pin. It has already been reserved
            if (alternateCsPin != HardwareProvider.Instance.InvalidPin)
            {
                m_altCsPin = GpioPin.TryCreateGpioPin(alternateCsPin);

                if (m_altCsPin == null)
                {
                    throw new ArgumentException();
                }

                m_altCsPin.Direction = LlilumGpio.PinDirection.Output;
                m_altCsPin.Mode = LlilumGpio.PinMode.PullDefault;

                // Set to high for the lifetime of the SpiChannel (except on transfers)
                m_altCsPin.Write(m_activeLow ? 1 : 0);
            }
        }

        [DllImport("C")]
        private static unsafe extern void tmp_spi_alloc_init(SpiImpl** obj, int mosi, int miso, int scl, int cs);

        [DllImport("C")]
        private static unsafe extern void tmp_spi_free(SpiImpl* obj);
        
        [DllImport("C")]
        private static unsafe extern void tmp_spi_format(SpiImpl* obj, int bits, int mode, int slave);

        [DllImport("C")]
        private static unsafe extern void tmp_spi_frequency(SpiImpl* obj, int hz);

        [DllImport("C")]
        private static unsafe extern int tmp_spi_master_write(SpiImpl* obj, int value);

        [DllImport("C")]
        private static unsafe extern int tmp_spi_busy(SpiImpl* obj);

    }

    internal unsafe struct SpiImpl
    {
    };
}
