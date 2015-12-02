//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using Microsoft.Zelig.Runtime;
    using Microsoft.Llilum.Devices.Spi;
    using LlilumGpio = Microsoft.Llilum.Devices.Gpio;
    using LLOS       = Zelig.LlilumOSAbstraction;
    using LLIO       = Zelig.LlilumOSAbstraction.API.IO;

    public class SpiChannel : Microsoft.Llilum.Devices.Spi.SpiChannel
    {
        private unsafe LLIO.SpiContext* m_spi;
        private unsafe LLIO.SpiConfig* m_spiCfg;
        private unsafe LlilumGpio.GpioPin m_altCsPin;
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
        public unsafe override void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes of all resources associated with this SPI device
        /// </summary>
        /// <param name="disposing">True if called from Dispose, false if called from the finalizer.</param>
        private unsafe void Dispose(bool disposing)
        {
            // Native resources need to be freed unconditionally
            if(m_spi != null)
            {
                LLIO.Spi.LLOS_SPI_Uninitialize(m_spi);
                m_spi = null;
            }
            if(disposing)
            {
                GC.SuppressFinalize(this);
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
        /// <param name="writeOffset">Offset into the writeBuffer</param>
        /// <param name="writeLength">Length of the data in bytes to write</param>
        /// <param name="readBuffer">Bytes to read. Leave null for write-only</param>
        /// <param name="readOffset">Offset into the readBuffer</param>
        /// <param name="readLength">Length in bytes to read</param>
        /// <param name="startReadOffset">Index of the SPI response at which to start reading bytes into readBuffer at readOffset.  This is used if the SPI response requires multiple writes before the response data is ready.</param>
        public unsafe override void WriteRead( byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, int startReadOffset )
        {
            EnableChipSelect( );

            // Are we reading, writing, or both?
            // We need to be at least one of these
            if(readBuffer == null && writeBuffer == null)
            {
                throw new ArgumentException( );
            }

            ArrayImpl writeImpl = (ArrayImpl)(object)writeBuffer;
            ArrayImpl readImpl = (ArrayImpl)(object)readBuffer;

            LLIO.Spi.LLOS_SPI_Transfer(
                m_spi,
                (byte*)writeImpl.GetDataPointer( ),
                writeOffset,
                writeLength,
                (byte*)readImpl.GetDataPointer( ),
                readOffset,
                readLength,
                startReadOffset );

            DisableChipselect( );
        }

        public unsafe override void Write( byte[] writeBuffer, int writeOffset, int writeLength )
        {
            ArrayImpl writeImpl = (ArrayImpl)(object)writeBuffer;

            EnableChipSelect( );
            LLIO.Spi.LLOS_SPI_Write( m_spi, (byte*)writeImpl.GetDataPointer(), writeOffset, writeLength );
            DisableChipselect( );
        }

        public unsafe override void Read( byte[] readBuffer, int readOffset, int readLength )
        {
            ArrayImpl readImpl = (ArrayImpl)(object)readBuffer;

            EnableChipSelect( );
            LLIO.Spi.LLOS_SPI_Read( m_spi, (byte*)readImpl.GetDataPointer(), readOffset, readLength, 0 );            
            DisableChipselect( );
        }

        public unsafe override void SetupChannel(int bits, SpiMode mode, bool isSlave)
        {
            m_spiCfg->Master          = isSlave ? 0u : 1u;
            m_spiCfg->DataWidth       = (uint)bits;
            m_spiCfg->PhaseMode       = ( mode == SpiMode.Cpol0Cpha1 || mode == SpiMode.Cpol1Cpha1 ) ? 1u : 0u;
            m_spiCfg->InversePolarity = ( mode == SpiMode.Cpol1Cpha0 || mode == SpiMode.Cpol1Cpha1 ) ? 1u : 0u;

            LLIO.Spi.LLOS_SPI_Configure( m_spi, m_spiCfg );
        }

        public unsafe override void SetupTiming(int frequencyInHz, int setupTime, int holdTime)
        {
            const int defaultDelayCycles = 100;

            if (setupTime < 0)
            {
                m_spiCfg->ChipSelectSetupCycles = ConvertToCoreClockTicks( defaultDelayCycles, frequencyInHz );
            }
            else
            {
                m_spiCfg->ChipSelectSetupCycles = ConvertToCoreClockTicks( setupTime, frequencyInHz );
            }

            if (holdTime < 0)
            {
                m_spiCfg->ChipSelectHoldCycles = ConvertToCoreClockTicks( defaultDelayCycles, frequencyInHz );
            }
            else
            {
                m_spiCfg->ChipSelectHoldCycles = ConvertToCoreClockTicks( holdTime, frequencyInHz );
            }

            m_spiCfg->ClockRateHz = (uint)frequencyInHz;
            LLIO.Spi.LLOS_SPI_SetFrequency(m_spi, (uint)frequencyInHz);
        }

        /// <summary>
        /// Initializes the mbed SpiChannel with the appropriate pins
        /// </summary>
        /// <param name="channelInfo">SPI channel pin info</param>
        /// <param name="writeOnly">Determines whether this is a write-only SPI channel, in which case the MISO pin is not used.</param>
        public unsafe override void SetupPins(ISpiChannelInfo channelInfo, bool writeOnly)
        {
            fixed (LLIO.SpiContext** spi_ptr = &m_spi)
            fixed (LLIO.SpiConfig** spicfg_ptr = &m_spiCfg)
            {
                LLIO.Spi.LLOS_SPI_Initialize((uint)channelInfo.Mosi, writeOnly ? (uint)HardwareProvider.Instance.InvalidPin : (uint)channelInfo.Miso, (uint)channelInfo.Sclk, (uint)channelInfo.DefaultChipSelect, spi_ptr, spicfg_ptr );
            }

            Initialize( channelInfo );
        }

        /// <summary>
        /// Initializes the mbed SpiChannel with an alternate chip select pin
        /// </summary>
        /// <param name="channelInfo">SPI channel pin info</param>
        /// <param name="alternateCsPin">Manually toggled CS pin</param>
        /// <param name="writeOnly">Determines whether this is a write-only SPI channel, in which case the MISO pin is not used.</param>
        public unsafe override void SetupPins(ISpiChannelInfo channelInfo, int alternateCsPin, bool writeOnly)
        {
            fixed(LLIO.SpiContext** ppSpi = &m_spi)
            fixed (LLIO.SpiConfig** spicfg_ptr = &m_spiCfg)
            {
                // Since we are using an alternate CS pin, initialize CS with the Invalid Pin
                LLIO.Spi.LLOS_SPI_Initialize( (uint)channelInfo.Mosi, writeOnly ? (uint)HardwareProvider.Instance.InvalidPin : (uint)channelInfo.Miso, (uint)channelInfo.Sclk, (uint)HardwareProvider.Instance.InvalidPin, ppSpi, spicfg_ptr );
            }

            Initialize( channelInfo );

            // Set up the pin, unless it's the invalid pin. It has already been reserved
            if (alternateCsPin != HardwareProvider.Instance.InvalidPin)
            {
                m_altCsPin = GpioPin.TryCreateGpioPin(alternateCsPin);

                if (m_altCsPin == null)
                {
                    throw new ArgumentException();
                }

                m_altCsPin.Direction = LlilumGpio.PinDirection.Output;
                m_altCsPin.Mode = LlilumGpio.PinMode.Default;

                // Set to high for the lifetime of the SpiChannel (except on transfers)
                m_altCsPin.Write(m_spiCfg->ActiveLow == 1 ? 1 : 0);
            }
        }

        private unsafe void Initialize(ISpiChannelInfo channelInfo)
        {
            // Mbed assumes active low, so we only set up active low/high when using the alternate CS pin
            m_spiCfg->ActiveLow = channelInfo.ActiveLow ? 1u : 0u;
            m_spiCfg->BusyPin = (uint)HardwareProvider.Instance.InvalidPin;
            m_spiCfg->ClockIdleLevel = 0;
            m_spiCfg->ClockSamplingEdge = 0;
            m_spiCfg->LoopbackMode = 0;
            m_spiCfg->MSBTransferMode = 0;
            m_spiCfg->ChipSelect = (uint)channelInfo.DefaultChipSelect;
        }

        private uint ConvertToCoreClockTicks(int spiCycles, int spiFrequencyInHz)
        {
            ulong cpu =  LLOS.HAL.Clock.LLOS_CLOCK_GetClockFrequency( );

            return (uint)( (ulong)spiCycles * cpu / (ulong)spiFrequencyInHz );
        }

        private unsafe void EnableChipSelect()
        {
            // Enable the chip select
            if (m_altCsPin != null)
            {
                m_altCsPin.Write(m_spiCfg->ActiveLow == 1 ? 0 : 1);

                // Setup time in cycles
                Processor.Delay( (int)m_spiCfg->ChipSelectSetupCycles );
            }
        }

        private unsafe void DisableChipselect()
        {
            // Disable the chip select
            if (m_altCsPin != null)
            {
                // Some boards (K64F) do not support checking if SPI is busy through mbed
                if (SpiProvider.Instance.SpiBusySupported)
                {
                    uint isBusy = 0;

                    while( LLOS.LlilumErrors.Succeeded( LLIO.Spi.LLOS_SPI_IsBusy( m_spi, &isBusy ) ) )
                    {
                        // Spin until transaction is complete
                        if( isBusy == 0 )
                        {
                            break;
                        }
                    }
                }

                // Hold time in cycles
                Processor.Delay( (int)m_spiCfg->ChipSelectHoldCycles );

                m_altCsPin.Write(m_spiCfg->ActiveLow == 1 ? 1 : 0);
            }
        }
    }
}
