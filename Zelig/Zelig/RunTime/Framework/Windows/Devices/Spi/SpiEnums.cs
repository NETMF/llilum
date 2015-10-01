//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Windows.Devices.Spi
{
    /// <summary>
    /// Defines the SPI communication mode. This API is for evaluation purposes only and is subject to change or removal.
    /// </summary>
    public enum SpiMode
    {
        /// <summary>
        /// CPOL = 0, CPHA = 0.
        /// </summary>
        Mode0 = 0,

        /// <summary>
        /// CPOL = 0, CPHA = 1.
        /// </summary>
        Mode1 = 1,

        /// <summary>
        /// CPOL = 1, CPHA = 0.
        /// </summary>
        Mode2 = 2,

        /// <summary>
        /// CPOL = 1, CPHA = 1.
        /// </summary>
        Mode3 = 3
    }

    /// <summary>
    /// Defines the sharing mode for the SPI bus. This API is for evaluation purposes only and is subject to change or removal.
    /// </summary>
    public enum SpiSharingMode
    {
        /// <summary>
        /// SPI bus segment is not shared.
        /// </summary>
        Exclusive = 0,

        /// <summary>
        /// SPI bus is shared.
        /// </summary>
        Shared = 1
    }
}
