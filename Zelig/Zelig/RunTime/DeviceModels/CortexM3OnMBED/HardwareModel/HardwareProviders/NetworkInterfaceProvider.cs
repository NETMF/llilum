//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using Zelig.LlilumOSAbstraction.API.IO;
    using Microsoft.Llilum.Lwip;

    using RT = Microsoft.Zelig.Runtime;


    public abstract class NetworkInterfaceProvider : RT.NetworkInterfaceProvider
    {
        public override int Connect(uint timeout)
        {
            return EthernetInterface.LLOS_ethernet_connect(timeout);
        }

        public override int Disconnect()
        {
            return EthernetInterface.LLOS_ethernet_disconnect();
        }

        public override int GetNetworkInterfaceCount()
        {
            // TODO: Eventually, we may want to make this extensible
            return 1;
        }

        public unsafe override string GetIPAddress()
        {
            char[] address = new char[16];
            fixed(char* addr = address)
            {
                EthernetInterface.LLOS_ethernet_get_IPv4Address(addr, (uint)address.Length);
            }
            return new string(address);
        }

        public override int InitializeEthernet()
        {
            return EthernetInterface.LLOS_ethernet_dhcp_init();
        }

        public unsafe override int InitializeEthernet(string ipAddress, string mask, string gateway)
        {
            char[] ipBytes = ipAddress.ToCharArray();
            char[] maskBytes = mask.ToCharArray();
            char[] gatewayBytes = gateway.ToCharArray();
            
            fixed (char* ipAddr = ipBytes)
            {
                fixed (char* subnetMask = maskBytes)
                {
                    fixed(char* gatewayAddr = gatewayBytes)
                    {
                        return EthernetInterface.LLOS_ethernet_staticIP_init(ipAddr, (uint)ipAddress.Length, subnetMask, (uint)mask.Length, gatewayAddr, (uint)gateway.Length);
                    }
                }
            }
        }

        public unsafe override uint IPv4AddressFromString(string ipAddress)
        {
            char[] addrArray = ipAddress.ToCharArray();
            fixed (char* ipAddr = addrArray)
            {
                return EthernetInterface.LLOS_ethernet_address_from_string(ipAddr, (uint)ipAddress.Length);
            }
        }

        public unsafe override string GetMacAddress()
        {
            char[] address = new char[20];
            fixed (char* addr = address)
            {
                EthernetInterface.LLOS_ethernet_get_macAddress(addr, (uint)address.Length);
            }
            return new string(address);
        }

        public unsafe override string GetGatewayAddress()
        {
            char[] address = new char[16];
            fixed (char* addr = address)
            {
                EthernetInterface.LLOS_ethernet_get_gatewayIPv4Address(addr, (uint)address.Length);
            }
            return new string(address);
        }

        public unsafe override string GetMask()
        {
            char[] address = new char[16];
            fixed (char* addr = address)
            {
                EthernetInterface.LLOS_ethernet_get_networkIPv4Mask(addr, (uint)address.Length);
            }
            return new string(address);
        }

        public override string GetDefaultLocalAddress()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            int cnt = interfaces.Length;
            for (int i = 0; i < cnt; i++)
            {
                NetworkInterface ni = interfaces[i];

                if (ni.IPAddress != "0.0.0.0" && ni.SubnetMask != "0.0.0.0")
                {
                    return ni.IPAddress;
                }
            }
            return null;
        }
    }
}
