//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define LPC1768
//#define K64F

namespace Microsoft.Zelig.Test.SerialPortTest
{
    using System;
    using System.IO.Ports;
    using System.Threading;

    //--//

    class Program
    {
        static void Main( )
        {
            while(true)
            {
                if (!TestSerialPortLoopBack())
                {
                    Console.WriteLine( "TestSerialPortLoopBack failed!" );
                    break;
                }

                // Sleep 1 second between test runs
                Thread.Sleep( 1000 );
            }
        }

        static bool TestSerialPortLoopBack()
        { 
            string serialPortName;
            bool result = true;
            
#if (LPC1768)
            serialPortName = "UART1";
#elif (K64F)
            serialPortName = "UART3";
#else
            throw new NotSupportedException();
#endif

            using (SerialPort port = new SerialPort(serialPortName, 115200, Parity.None, 8, StopBits.One))
            {
                port.Open();
                byte[] txtBuffer = new byte[32];

                result = TestLoopback( port, ref txtBuffer );
            }

            return result;
        }

        //
        // This test requires the RX/TX pins to be connected for hardware loop-back.
        // For LPC1768, connect pins p13 and p14.
        // For FRDM K64F, connect pins PTC17 and PTC16.
        //
        private static bool TestLoopback( SerialPort port, ref byte[] txtBuffer )
        {
            byte b = 0;
            bool result = true;
            int len = txtBuffer.Length;

            //
            // Fill TX buffer with incrementing pattern
            //
            for(int i = 0; i<len; i++)
            {
                txtBuffer[ i ] = b++;
            }

            port.Write( txtBuffer, 0, len );

            //
            // Clear the buffer for the read
            //
            Array.Clear( txtBuffer, 0, len );

            //
            // The read can return less than the requested length if
            // the buffer contains data.
            //
            int read = 0;
            while(read < len)
            {
                read += port.Read( txtBuffer, read, len-read );
            }

            //
            // Reset the test byte value and increment over the length
            // of the RX buffer and verify the pattern.
            //
            b = 0;
            for(int i = 0; i < len; i++)
            {
                if(txtBuffer[ i ] != b++)
                {
                    result = false;
                    break;
                }
            }

            return result;
        }
    }
}
