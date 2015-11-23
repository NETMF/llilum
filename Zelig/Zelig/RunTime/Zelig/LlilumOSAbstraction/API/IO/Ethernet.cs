//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.API.IO
{
    using System.Runtime.InteropServices;

    public static unsafe class EthernetInterface
    {
        [DllImport("C")]
        public static extern uint LLOS_ethernet_address_from_string(char* address, uint addrlen);

        [DllImport("C")]
        public static extern int LLOS_ethernet_dhcp_init();

        [DllImport("C")]
        public static extern int LLOS_ethernet_staticIP_init(char* ip, uint ipLen, char* mask, uint maskLen, char* gateway, uint gatewayLen);

        [DllImport("C")]
        public static extern int LLOS_ethernet_connect(uint timeout);

        [DllImport("C")]
        public static extern int LLOS_ethernet_disconnect();

        [DllImport("C")]
        public static extern int LLOS_ethernet_get_macAddress(char* address, uint bufferLen);

        [DllImport("C")]
        public static extern int LLOS_ethernet_get_IPv4Address(char* address, uint bufferLen);

        [DllImport("C")]
        public static extern int LLOS_ethernet_get_gatewayIPv4Address(char* address, uint bufferLen);

        [DllImport("C")]
        public static extern int LLOS_ethernet_get_networkIPv4Mask(char* mask, uint bufferLen);
    }
}
