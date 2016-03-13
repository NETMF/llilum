//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.Lwip
{
    using System;
    using System.Collections;
    using System.Threading;
    using Microsoft.Zelig.Runtime;

    public enum NetworkInterfaceType
    {
        Unknown = 1,
        Ethernet = 6,
        Wireless80211 = 71,
    }

    public class NetworkInterface
    {
        //set update flags...
        private const int UPDATE_FLAGS_DNS          = 0x1;
        private const int UPDATE_FLAGS_DHCP         = 0x2;
        private const int UPDATE_FLAGS_DHCP_RENEW   = 0x4;
        private const int UPDATE_FLAGS_DHCP_RELEASE = 0x8;
        private const int UPDATE_FLAGS_MAC          = 0x10;
        //--//
        private const uint FLAGS_DHCP               = 0x1;
        private const uint FLAGS_DYNAMIC_DNS        = 0x2;

        //--//

        private static NetworkInterface s_netInterface = new NetworkInterface(0);

        //--//

        private readonly int                    m_interfaceIndex;
        private          uint                   m_flags;
        private          uint                   m_ipAddress;
        private          uint                   m_gatewayAddress;
        private          uint                   m_subnetMask;
        private          NetworkInterfaceType   m_networkInterfaceType;
        private          byte[]                 m_macAddress;
        //private          uint _dnsAddress1;
        //privae           uint _dnsAddress2;

        //--//

        internal NetworkInterface( int interfaceIndex )
        {
            this.m_interfaceIndex = interfaceIndex;
            m_networkInterfaceType = NetworkInterfaceType.Unknown;
        }

        public static NetworkInterface[ ] GetAllNetworkInterfaces( )
        {
            int count = GetNetworkInterfaceCount();
            NetworkInterface[] ifaces = new NetworkInterface[count];

            for(uint i = 0; i < count; i++)
            {
                ifaces[ i ] = GetNetworkInterface( i );
            }

            return ifaces;
        }

        private static NetworkInterface GetNetworkInterface( uint interfaceIndex )
        {
            if(interfaceIndex >= GetNetworkInterfaceCount( ))
            {
                throw new ArgumentOutOfRangeException( nameof( interfaceIndex ) );
            }

            return s_netInterface;
        }

        public static int GetNetworkInterfaceCount( )
        {
            return NetworkInterfaceProvider.Instance.GetNetworkInterfaceCount( );
        }

        public void InitializeNetworkInterfaceSettings( NetworkInterface netInterface )
        {
            netInterface.IPAddress      = NetworkInterfaceProvider.Instance.GetIPAddress( );
            netInterface.GatewayAddress = NetworkInterfaceProvider.Instance.GetGatewayAddress( );
            netInterface.SubnetMask     = NetworkInterfaceProvider.Instance.GetMask( );
        }

        public void UpdateConfiguration( NetworkInterface netInterface, int updateType )
        {
            var ni = NetworkInterfaceProvider.Instance;

            // Handle only DHCP updates for now
            if(updateType == 0x2)
            {
                ni.Disconnect( );

                if(netInterface.IsDhcpEnabled)
                {
                    ni.InitializeEthernet( );
                }
                else
                {
                    ni.InitializeEthernet(
                        netInterface.IPAddress,
                        netInterface.SubnetMask,
                        netInterface.GatewayAddress );
                }

                //
                // After initialization is complete, we need to remap all interrupts 
                // to the safe LLILUM wrapper
                //
                ni.RemapInterrupts( );

                ni.Connect( 15000 );
            }
        }

        public static uint IPv4AddressFromString( string ipAddress )
        {
            return NetworkInterfaceProvider.Instance.IPv4AddressFromString( ipAddress );
        }

        private string IPv4AddressToString( uint ipAddress )
        {
            return string.Concat(
                            ((ipAddress >> 0) & 0xFF).ToString( ),
                                ".",
                            ((ipAddress >> 8) & 0xFF).ToString( ),
                                ".",
                            ((ipAddress >> 16) & 0xFF).ToString( ),
                                ".",
                            ((ipAddress >> 24) & 0xFF).ToString( )
                            );
        }

        public void EnableStaticIP( string ipAddress, string subnetMask, string gatewayAddress )
        {
            try
            {
                m_ipAddress = IPv4AddressFromString( ipAddress );
                m_subnetMask = IPv4AddressFromString( subnetMask );
                m_gatewayAddress = IPv4AddressFromString( gatewayAddress );
                m_flags &= ~FLAGS_DHCP;

                UpdateConfiguration( this, UPDATE_FLAGS_DHCP );
            }
            finally
            {
                ReloadSettings( );
            }
        }

        public void EnableDhcp( )
        {
            try
            {
                m_flags |= FLAGS_DHCP;
                UpdateConfiguration( this, UPDATE_FLAGS_DHCP );
            }
            finally
            {
                ReloadSettings( );
            }
        }

        public void EnableStaticDns( string[ ] dnsAddresses )
        {
            throw new NotImplementedException( );

            //if (dnsAddresses == null || dnsAddresses.Length == 0 || dnsAddresses.Length > 2)
            //{
            //    throw new ArgumentException();
            //}

            //uint[] addresses = new uint[2];

            //int iAddress = 0;
            //for (int i = 0; i < dnsAddresses.Length; i++)
            //{
            //    uint address = IPAddressFromString(dnsAddresses[i]);

            //    addresses[iAddress] = address;

            //    if (address != 0)
            //    {
            //        iAddress++;
            //    }
            //}

            //try
            //{
            //    _dnsAddress1 = addresses[0];
            //    _dnsAddress2 = addresses[1];

            //    _flags &= ~FLAGS_DYNAMIC_DNS;

            //    UpdateConfiguration(this, UPDATE_FLAGS_DNS);
            //}
            //finally
            //{
            //    ReloadSettings();
            //}
        }

        public void EnableDynamicDns( )
        {
            throw new NotImplementedException( );

            //try
            //{
            //    _flags |= FLAGS_DYNAMIC_DNS;

            //    UpdateConfiguration(this, UPDATE_FLAGS_DNS);
            //}
            //finally
            //{
            //    ReloadSettings();
            //}
        }

        public string IPAddress
        {
            get { return IPv4AddressToString( m_ipAddress ); }
            internal set { m_ipAddress = IPv4AddressFromString( value ); }
        }

        public string GatewayAddress
        {
            get { return IPv4AddressToString( m_gatewayAddress ); }
            internal set { m_gatewayAddress = IPv4AddressFromString( value ); }
        }

        public string SubnetMask
        {
            get { return IPv4AddressToString( m_subnetMask ); }
            internal set { m_subnetMask = IPv4AddressFromString( value ); }
        }

        public bool IsDhcpEnabled
        {
            get { return (m_flags & FLAGS_DHCP) != 0; }
        }

        public bool IsDynamicDnsEnabled
        {
            get
            {
                return (m_flags & FLAGS_DYNAMIC_DNS) != 0;
            }
        }

        public string[ ] DnsAddresses
        {
            get
            {
                ArrayList list = new ArrayList();

                //if (_dnsAddress1 != 0)
                //{
                //    list.Add(IPAddressToString(_dnsAddress1));
                //}

                //if (_dnsAddress2 != 0)
                //{
                //    list.Add(IPAddressToString(_dnsAddress2));
                //}

                return (string[ ])list.ToArray( typeof( string ) );
            }
        }

        private void ReloadSettings( )
        {
            Thread.Sleep( 100 );
            InitializeNetworkInterfaceSettings( this );
        }

        public void ReleaseDhcpLease( )
        {
            throw new NotImplementedException( );

            //try
            //{
            //    UpdateConfiguration(this, UPDATE_FLAGS_DHCP_RELEASE);
            //}
            //finally
            //{
            //    ReloadSettings();
            //}
        }

        public void RenewDhcpLease( )
        {
            throw new NotImplementedException( );

            //try
            //{
            //    UpdateConfiguration(this, UPDATE_FLAGS_DHCP_RELEASE | UPDATE_FLAGS_DHCP_RENEW);
            //}
            //finally
            //{
            //    ReloadSettings();
            //}
        }

        public byte[ ] PhysicalAddress
        {
            get { return m_macAddress; }
            set
            {
                m_macAddress = value;
                throw new NotImplementedException( );

                //try
                //{
                //    _macAddress = value;
                //    UpdateConfiguration(this, UPDATE_FLAGS_MAC);
                //}
                //finally
                //{
                //    ReloadSettings();
                //}
            }
        }

        public NetworkInterfaceType NetworkInterfaceType
        {
            get { return m_networkInterfaceType; }
        }
    }
}


