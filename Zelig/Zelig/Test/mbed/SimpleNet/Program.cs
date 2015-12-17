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
    
    using System.Text;

    class Program
    {
        static void Main( )
        {
            NetworkInterface netif = NetworkInterface.GetAllNetworkInterfaces()[0];
            netif.EnableDhcp( );
            //netif.EnableStaticIP("10.125.148.136", "255.255.254.0", "10.125.148.1");

            //IPHostEntry entry = Dns.GetHostEntry("bing.com");

            //IPAddress dnsAddress = entry.AddressList[0];

            //byte[] dnsCmd = new byte[entry.HostName.Length];

            //for(int i = 0; i < entry.HostName.Length; i++)
            //{
            //    dnsCmd[ i ] = (byte)entry.HostName[ i ];
            //}

            while(true)
            {
                Socket sock = new Socket(AddressFamily.Unspecified, SocketType.Stream, ProtocolType.Unspecified);

                //byte[] address = new byte[] {(byte)217, (byte)140, (byte)101, (byte)20};
                //IPEndPoint endPoint = new IPEndPoint(new IPAddress((long)0x14658CD9), 80);
                IPEndPoint endPoint = new IPEndPoint( IPAddress.Parse( "10.125.148.170" ), 11000);

                sock.Connect( endPoint );

                string command = "GET /media/uploads/mbed_official/hello.txt HTTP/1.0\n\n";

                byte[] byteCmd = new byte[] {
                                        (byte)'G',
                                        (byte)'E',
                                        (byte)'T',
                                        (byte)' ',
                                        (byte)'/',
                                        (byte)' ',
                                        (byte)'H',
                                        (byte)'T',
                                        (byte)'T',
                                        (byte)'P',
                                        (byte)'/',
                                        (byte)'1',
                                        (byte)'.',
                                        (byte)'0',
                                        0xa,
                                        0xa,
                                    };

                int count = 0;

                var recBuffer = new byte[512];

                while(++count <= 10)
                {
                    if(sock.Send( byteCmd ) > 0)
                    //if (sock.Send(ASCIIEncoding.ASCII.GetBytes(command)) > 0)
                    {
                        sock.Receive( recBuffer );
                    }
                }

                sock.Close( );
            }
        }
    }
}
