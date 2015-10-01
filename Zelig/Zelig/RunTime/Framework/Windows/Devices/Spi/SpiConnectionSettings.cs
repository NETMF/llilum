//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Windows.Devices.Spi
{
    public sealed class SpiConnectionSettings
    {
        /// <summary>
        /// Initializes new instance of SpiConnectionSettings.
        /// </summary>
        /// <param name="chipSelectLine">The chip select line on which the connection will be made.</param>
        public SpiConnectionSettings(int chipSelectLine)
        {
            ChipSelectLine = chipSelectLine;

            // These are the default values as per IoT SPI WinRT Spec
            ClockFrequency = 4000000;
            DataBitLength = 8;
            Mode = SpiMode.Mode0;
            SharingMode = SpiSharingMode.Exclusive;
        }

        /// <summary>
        /// Gets or sets the chip select line for the connection to the SPI device.
        /// </summary>
        /// <value>The chip select line.</value>
        public int ChipSelectLine { get; set; }

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
        /// Gets or sets the sharing mode for the SPI connection.
        /// </summary>
        /// <value>The sharing mode.</value>
        public SpiSharingMode SharingMode { get; set; }
    }
}
