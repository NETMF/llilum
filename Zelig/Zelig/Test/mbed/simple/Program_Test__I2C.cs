//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TEST_I2C



namespace Microsoft.Zelig.Test.mbed.Simple
{
    using Windows.Devices.Enumeration;
    using Windows.Devices.I2c;


    partial class Program
    {
        private static I2cDevice s_i2cDevice     = null;
        private static byte[]    s_i2cReadWrite1 = new byte[1];
        private static byte[]    s_i2cReadWrite2 = new byte[2];
        private static bool      s_initialized   = false;

        //--//

        private static int TestI2C( int currentMode )
        {
            int mode = currentMode;
#if TEST_I2C

            //
            // I2C Init
            //
            if(s_initialized == false)
            {
                string i2cDeviceSelector = I2cDevice.GetDeviceSelector("I2C1");

                var i2cAcqs = DeviceInformation.FindAllAsync(i2cDeviceSelector);

                string i2cBusId = i2cAcqs[0].Id;

                s_i2cDevice = I2cDevice.FromIdAsync(i2cBusId, new I2cConnectionSettings(0x90)
                {
                    SharingMode = I2cSharingMode.Shared
                });

                //
                // Init temp sensor
                //
                s_i2cReadWrite2[ 0 ] = 0x1;
                s_i2cReadWrite2[ 1 ] = 0x0;

                s_i2cDevice.Write( s_i2cReadWrite2 );

                s_initialized = true;
            }

            try
            {
                double temp = ReadTemp( s_i2cDevice );

                System.Diagnostics.Debug.WriteLine( "Temperature: " + temp.ToString( ) + " F" );

                if (temp < 31.0)
                {
                    mode = 0;
                }
                else if (temp >= 31.0 && temp < 32.5)
                {
                    mode = 1;
                }
                else
                {
                    mode = 2;
                }
            }
            catch
            {
                // If an application board is not attached, we will get an Exception.
                // Continue as normal in this case
            }

#endif // TEST_GPIO_PERF && LPC1768
            return mode; 
        }
        
        //--//

        private static double ReadTemp(I2cDevice device)
        {
            s_i2cReadWrite1[0] = 0x0;

            device.WriteRead( s_i2cReadWrite1, s_i2cReadWrite2 );

            return ((s_i2cReadWrite2[0] << 8) | s_i2cReadWrite2[1]) / 256.0; 
        }
    }
}
