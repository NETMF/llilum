//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TEST_SPI_LCD

#define LPC1768


namespace Microsoft.Zelig.Test.mbed.Simple
{
    using System;
    using System.Threading;

    using LPC1768   = Llilum.LPC1768;
    using K64F      = Llilum.K64F;


    partial class Program
    {
        private static void TestSpiLcd( )
        {
#if TEST_SPI_LCD && LPC1768
            int commandPin = (int)LPC1768.PinName.p8;
            int resetPin   = (int)LPC1768.PinName.p6;
            int chipSelPin = (int)LPC1768.PinName.p11;

            Runtime.SpiChannelInfo channelInfo = Runtime.SpiProvider.Instance.GetSpiChannelInfo( 0 );

            using(SpiLcdC12832 lcd = new SpiLcdC12832( channelInfo, commandPin, resetPin, chipSelPin ))
            {
                lcd.Clear( );

                for(int x = 0; x<128; x++)
                {
                    for(int y = 0; y<32; y++)
                    {
                        lcd.SetPixel( x, y, 1 );

                        if(0x10 == ( y & 0x10 ))
                        {
                            lcd.Flush( );
                        }
                    }
                }

                lcd.Clear( );
                Thread.Sleep( 200 );
                lcd.Fill( );

                for(byte contrast = 0; contrast < 0x3f; contrast++)
                {
                    lcd.SetContast( contrast );
                    Thread.Sleep( 100 );
                }
            }
#endif // USE_SPI && LPC1768
        }
    }
}
