//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using Windows.Devices.Spi;
    using Windows.Devices.Spi.Provider;

    //--//

    [ExtendClass( typeof( SpiDevice ), NoConstructors = true )]
    public class SpiDeviceImpl
    {
        /// <summary>
        /// Returns a SpiChannel that handles all SPI transactions on a single port
        /// </summary>
        /// <param name="busId">UWP bus ID</param>
        /// <param name="settings">SPI behavior settings</param>
        /// <returns>SPI channel or null if one could not be found for the given busId</returns>
        /// 
        public static SpiChannel AcquireSpiChannel(string busId, SpiConnectionSettings settings)
        {
            HardwareProvider provider = HardwareProvider.Instance;
            int mosi, miso, sclk, chipSelect;

            int spiChannelId = provider.GetSpiChannelIndexFromString(busId);
            
            if(provider.GetSpiPinsFromBusId( spiChannelId, out mosi, out miso, out sclk, out chipSelect ) )
            {
                // If the user provides an alternative CS pin, we will toggle it for them
                bool alternate = false;
                if(chipSelect != settings.ChipSelectLine)
                {
                    alternate = true;
                    chipSelect = settings.ChipSelectLine;
                }
                
                // Ensure all pins are available
                if(provider.TryReservePins(mosi, miso, sclk, chipSelect))
                {
                    SpiChannel spiChannel = provider.CreateSpiChannel();

                    bool activeLow;
                    if (provider.GetSpiChannelActiveLow(spiChannelId, out activeLow))
                    {
                        spiChannel.SetupPins(mosi, miso, sclk, chipSelect, alternate, activeLow);
                    }
                    else
                    {
                        spiChannel.SetupPins(mosi, miso, sclk, chipSelect, alternate, true /* default active low chip select*/);
                    }
                    
                    spiChannel.SetupChannel(settings.DataBitLength, (int)settings.Mode, false);

                    int setupTime, holdTime;
                    if (provider.GetSpiChannelTimingInfo(spiChannelId, out setupTime, out holdTime))
                    {
                        spiChannel.SetupTiming(settings.ClockFrequency, setupTime, holdTime);
                    }
                    else
                    {
                        spiChannel.SetupTiming(settings.ClockFrequency, -1, -1);
                    }
                    
                    return spiChannel;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns SPI channel characteristics that are defined in each Board provider
        /// </summary>
        /// <param name="id">SPI bus index</param>
        /// <param name="csLineCount">Number of chip select lines on bus. Generally just 1</param>
        /// <param name="maxFreq">Maximum supported frequency</param>
        /// <param name="minFreq">Minimum supported frequency</param>
        /// <returns>True if the bus exists</returns>
        public static bool GetSpiChannelInfo( int id, out int csLineCount, out int maxFreq, out int minFreq, out bool supports16 )
        {
            HardwareProvider provider = HardwareProvider.Instance;
            
            if(provider.GetSpiChannelInfo( id, out csLineCount, out maxFreq, out minFreq, out supports16 ))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the string identifiers of SPI busses
        /// </summary>
        /// <returns>Array of SPI busIds</returns>
        public static string[] GetSpiChannels()
        {
            return HardwareProvider.Instance.GetSpiChannels();
        }

        /// <summary>
        /// Releases the pins that are used for the SpiChannel
        /// </summary>
        /// <param name="busId">String identifier of the SPI bus</param>
        public static void ReleaseSpiBus(string busId, SpiConnectionSettings settings)
        {
            HardwareProvider hp = HardwareProvider.Instance;
            int mosi, miso, sclk, chipSelect;
            if (hp.GetSpiPinsFromBusId(hp.GetSpiChannelIndexFromString(busId), out mosi, out miso, out sclk, out chipSelect))
            {
                // If the CS lines are not equal, we need to release the one from settings because it's the one that was acquired
                if (chipSelect != settings.ChipSelectLine)
                {
                    chipSelect = settings.ChipSelectLine;
                }
                hp.ReleasePins(mosi, miso, sclk, chipSelect);
            }
        }
    }
}
