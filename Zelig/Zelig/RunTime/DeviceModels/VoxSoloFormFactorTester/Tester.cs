//
// Copyright (c) Microsoft Corporation.   All rights reserved.
//

//#define TEST_SERIAL

namespace Microsoft.VoxSoloFormFactorTester
{
    using System;
    using System.Collections.Generic;

    class Tester
    {
#if TEST_SERIAL
        static void TestSerial()
        {
            var port = new System.IO.Ports.SerialPort( "COM2" );

            port.BaudRate        = 115200;
            port.ReadBufferSize  = 256;
            port.WriteBufferSize = 256;

            port.Open();

            port.Write( "Hello World!" );

            byte[] writeBuf = new byte[1];

            while(true)
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    
                stopwatch.Start();
    
                const int total = 32 * 1024;
    
                int ch = 0;
    
                for(int j = 0; j < total; j++)
                {
                    writeBuf[0] = (byte)(48 + ch);

                    port.Write( writeBuf, 0, 1 );
    
                    ch = (ch + 1) % 32;
                }
    
                stopwatch.Stop();
    
                port.Write( "\r\n" );
                port.Write( "\r\n" );
    
                port.Write( string.Format( "Took {0} to send {1} bytes", stopwatch.ElapsedMilliseconds, total ) );
    
////            byte[] buf = new byte[64];
////
////            while(true)
////            {
////                for(int i = 0; i < 64; i++)
////                {
////                    buf[i] = port.Read();
////                }
////
////                //port.Write( (byte)'>' );
////                for(int i = 0; i < 64; i++)
////                {
////                    port.Write( buf[i] );
////                }
////                //port.Write( (byte)'<' );
////            }
    
                GC.Collect();
            }
        }
#endif


        static void Main()
        {
#if TEST_SERIAL

            TestSerial();

#endif
        }
    }
}
