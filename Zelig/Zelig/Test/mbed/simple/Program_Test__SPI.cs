//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TEST_SPI


#define LPC1768
//#define K64F

#if LPC1768
    using TARGET = Microsoft.Llilum.LPC1768;
#elif K64F
    using TARGET = Microsoft.Llilum.K64F;
#endif


namespace Microsoft.Zelig.Test.mbed.Simple
{
    using Windows.Devices.Enumeration;
    using Windows.Devices.Spi;


    internal partial class Program
    {
        private static SpiDevice s_spiDevice;
        private static byte[]    s_writeBuffer  = new byte[1];
        private static byte[]    s_writeBuffer2 = new byte[] { 0x1, 0x2, 0x3 };
        private static byte[]    s_readBuffer   = new byte[1];


        private static void InitSPI( )
        {
#if TEST_SPI
            // Get the device selector by friendly name
            string deviceSelector = SpiDevice.GetDeviceSelector("SPI0");

            var acqs = DeviceInformation.FindAllAsync(deviceSelector);

            string busId = acqs[0].Id;

            // Set up a non-default frequency
#if (LPC1768)
            int chipSelect = (int)TARGET.PinName.p8;
#elif (K64F)
            int chipSelect = (int)K64F.PinName.PTD0;
#else
#error No target board defined.
#endif
            SpiConnectionSettings settings = new SpiConnectionSettings(chipSelect)
            {
                ClockFrequency = 1000000,
                Mode           = SpiMode.Mode2
            };

            s_spiDevice = SpiDevice.FromIdAsync(busId, settings);
#endif
        }

        private static void TestSPI( int mode )
        {
#if TEST_SPI           
            s_writeBuffer[0] = (byte)mode;

            s_spiDevice.TransferFullDuplex( s_writeBuffer, s_readBuffer );

            if (s_readBuffer[0] == s_writeBuffer[0])
            {
                s_writeBuffer[0] = (byte)5;

                s_spiDevice.TransferSequential( s_writeBuffer, s_readBuffer );
            }

            s_spiDevice.Write( s_writeBuffer2 );
#endif
        }
    }
}
