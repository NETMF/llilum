//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace System.Net
{
    using System.Text;
    using System.Collections;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Globalization;

    using NativeSocket = Microsoft.Llilum.Net.SocketNative;

    public static class Dns
    {
        public static IPHostEntry GetHostEntry(string hostNameOrAddress)
        {
            //Do we need to try to pase this as an Address????
            string canonicalName;
            byte[][] addresses;

            NativeSocket.getaddrinfo(hostNameOrAddress, out canonicalName, out addresses);

            int cAddresses = addresses.Length;
            IPAddress[] ipAddresses = new IPAddress[cAddresses];
            IPHostEntry ipHostEntry = new IPHostEntry();

            for (int i = 0; i < cAddresses; i++)
            {
                byte[] address = addresses[i];

                SocketAddress sockAddress = new SocketAddress(address);

                AddressFamily family;
                
                family = (AddressFamily)address[1];

                if (family == AddressFamily.InterNetwork)
                {
                    //This only works with IPv4 addresses
                    uint ipAddr = 0;
                    ipAddr |= ((uint)address[7] << 24)  & 0xFF000000;
                    ipAddr |= ((uint)address[6] << 16)  & 0x00FF0000;
                    ipAddr |= ((uint)address[5] << 8)   & 0x0000FF00;
                    ipAddr |= ((uint)address[4])        & 0x000000FF;

                    ipAddresses[i] = new IPAddress((long)ipAddr);
                }
            }

            ipHostEntry.hostName = canonicalName;
            ipHostEntry.addressList = ipAddresses;

            return ipHostEntry;
        }
    }
}


