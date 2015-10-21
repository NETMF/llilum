//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using Llilum = Microsoft.Llilum.Devices.Spi;
    using Windows.Devices.Spi;
    using Windows.Devices.Spi.Provider;

    //--//

    [ExtendClass( typeof( SpiDevice ), NoConstructors = true )]
    public class SpiDeviceImplUwp
    {
        /// <summary>
        /// Returns SPI channel characteristics that are defined in each Board provider
        /// </summary>
        /// <param name="busId">SPI bus identifier</param>
        public static Llilum.ISpiChannelInfoUwp GetSpiChannelInfo( string busId )
        {
            return SpiProviderUwp.Instance.GetSpiChannelInfo(busId);
        }

        /// <summary>
        /// Gets the string identifiers of SPI busses
        /// </summary>
        /// <returns>Array of SPI busIds</returns>
        public static string[] GetSpiChannels()
        {
            return SpiProviderUwp.Instance.GetSpiChannels();
        }
    }
}
