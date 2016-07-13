//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define K64F

namespace Microsoft.Zelig.Test.mbed.SimpleNet
{
    using System.Net.Sockets;
    using System.Net;
    using System.Threading;
    using Microsoft.Llilum.Lwip;
    using Microsoft.Zelig.Runtime;
    using System.Text;
    using System;
    
    
    class Program
    {
        static void Main( )
        {
            NetworkInterface netif = NetworkInterface.GetAllNetworkInterfaces()[0];
            netif.EnableDhcp( );

            BugCheck.Log( "Acquired IPv4 Address" );
            BugCheck.Log( netif.IPAddress.ToString( ) );

            //netif.EnableStaticIP("10.125.148.136", "255.255.254.0", "10.125.148.1");

            //IPHostEntry entry = Dns.GetHostEntry("bing.com");

            //IPAddress dnsAddress = entry.AddressList[0];

            //byte[] dnsCmd = new byte[entry.HostName.Length];

            //for(int i = 0; i < entry.HostName.Length; i++)
            //{
            //    dnsCmd[ i ] = (byte)entry.HostName[ i ];
            //}

            string msg = "GET /media/uploads/mbed_official/hello.txt HTTP/1.0\n\n";
            string end = "TEST_COMPLETED";
            var msgBytes = ASCIIEncoding.ASCII.GetBytes(msg);
            var endBytes = ASCIIEncoding.ASCII.GetBytes(end);
            
            // NOTE: Be sure to change this to your local machine IP that is running the NetworkTest app
            IPEndPoint endPoint = new IPEndPoint( IPAddress.Parse("10.0.1.28"), 11000);

            var recBuffer = new byte[512];

            while(true)
            {
                Socket sock = new Socket(AddressFamily.Unspecified, SocketType.Stream, ProtocolType.Unspecified);

                try
                {
                    sock.Connect( endPoint );

                    int count = 0;

                    while(++count <= 10)
                    {
                        if(sock.Send( msgBytes ) > 0)
                        {
                            sock.Receive( recBuffer );
                        }
                    }

                    sock.Send( endBytes );
                }
                catch
                {

                }
                finally
                {
                    sock.Close( );
                }
            }
        }
    }
}
