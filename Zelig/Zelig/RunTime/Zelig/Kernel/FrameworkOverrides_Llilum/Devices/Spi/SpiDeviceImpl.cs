//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using Microsoft.Llilum.Devices.Spi;

    //--//

    [ExtendClass(typeof(SpiDevice), NoConstructors = true)]
    public class SpiDeviceImpl
    {
        public static SpiChannel TryAcquireSpiChannel(int port, bool writeOnly)
        {
            SpiChannelInfo channelInfo = SpiProvider.Instance.GetSpiChannelInfo(port);

            return TryAcquireSpiChannel(channelInfo, channelInfo.DefaultChipSelect, writeOnly);
        }

        public static SpiChannel TryAcquireSpiChannel(ISpiChannelInfo channelInfo, int chipSelectPin, bool writeOnly)
        {
            SpiProvider spiProvider = SpiProvider.Instance;

            if (channelInfo != null)
            {
                // Ensure all pins are available
                if (!HardwareProvider.Instance.TryReservePins(channelInfo.Mosi, writeOnly ? HardwareProvider.Instance.InvalidPin : channelInfo.Miso, channelInfo.Sclk, chipSelectPin))
                {
                    return null;
                }

                return spiProvider.CreateSpiChannel(channelInfo);
            }

            return null;
        }

        public static bool TryChangeCsPin(int oldPin, int newPin)
        {
            HardwareProvider provider = HardwareProvider.Instance;

            // If pin is invalid, short circuit the if statement, and skip pin reservation
            if (newPin == provider.InvalidPin || provider.TryReservePins(newPin))
            {
                provider.ReleasePins(oldPin);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void ReleaseSpiPins(ISpiChannelInfo channelInfo, int csPin)
        {
            HardwareProvider.Instance.ReleasePins(channelInfo.Mosi, channelInfo.Miso, channelInfo.Sclk, csPin);
        }
    }
}
