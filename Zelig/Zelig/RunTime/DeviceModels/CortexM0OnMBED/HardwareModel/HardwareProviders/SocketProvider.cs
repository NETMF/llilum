//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM0OnMBED.HardwareModel
{
    using System;
    using Zelig.LlilumOSAbstraction.API.IO;
    using System.Text;

    using RT = Microsoft.Zelig.Runtime;

    public abstract class SocketProvider : RT.SocketProvider
    {
        //       
        // Socket protocol types (TCP/UDP/RAW) 
        //
        private const int SOCK_STREAM                                       = 1;
        private const int SOCK_DGRAM                                        = 2;
        private const int SOCK_RAW                                          = 3;

        //
        // Option flags per-socket. These must match the SOF_ flags in ip.h (checked in init.c)
        //
        private const int  SO_DEBUG                                         = 0x0001; // Unimplemented: turn on debugging info recording 
        private const int  SO_ACCEPTCONN                                    = 0x0002; // socket has had listen() 
        private const int  SO_REUSEADDR                                     = 0x0004; // Allow local address reuse 
        private const int  SO_KEEPALIVE                                     = 0x0008; // keep connections alive 
        private const int  SO_DONTROUTE                                     = 0x0010; // Unimplemented: just use interface addresses 
        private const int  SO_BROADCAST                                     = 0x0020; // permit to send and to receive broadcast messages (see IP_SOF_BROADCAST option) 
        private const int  SO_USELOOPBACK                                   = 0x0040; // Unimplemented: bypass hardware when possible 
        private const int  SO_LINGER                                        = 0x0080; // linger on close if data present 
        private const int  SO_OOBINLINE                                     = 0x0100; // Unimplemented: leave received OOB data in line 
        private const int  SO_REUSEPORT                                     = 0x0200; // Unimplemented: allow local address & port reuse 

        private const int SO_DONTLINGER                                     = ((int)(~SO_LINGER));

        //
        // Additional options, not kept in so_options.
        //
        private const int SO_SNDBUF                                         = 0x1001; // Unimplemented: send buffer size 
        private const int SO_RCVBUF                                         = 0x1002; // receive buffer size 
        private const int SO_SNDLOWAT                                       = 0x1003; // Unimplemented: send low-water mark 
        private const int SO_RCVLOWAT                                       = 0x1004; // Unimplemented: receive low-water mark 
        private const int SO_SNDTIMEO                                       = 0x1005; // Unimplemented: send timeout 
        private const int SO_RCVTIMEO                                       = 0x1006; // receive timeout 
        private const int SO_ERROR                                          = 0x1007; // get error status and clear 
        private const int SO_TYPE                                           = 0x1008; // get socket type 
        private const int SO_CONTIMEO                                       = 0x1009; // Unimplemented: connect timeout 
        private const int SO_NO_CHECK                                       = 0x100a; // don't create UDP checksum 

        //
        // Level number for (get/set)sockopt() to apply to socket itself.
        //
        private const int SOL_SOCKET                                        = 0xfff;  // options for socket level 
        
        private const int AF_UNSPEC                                         = 0;
        private const int AF_INET                                           = 2;
        private const int PF_INET                                           = AF_INET;
        private const int PF_UNSPEC                                         = AF_UNSPEC;

        private const int IPPROTO_IP                                        = 0;
        private const int IPPROTO_TCP                                       = 6;
        private const int IPPROTO_UDP                                       = 17;
        private const int IPPROTO_UDPLITE                                   = 136;

        //
        // Options for level IPPROTO_IP
        //
        private const int IP_TOS                                            = 1;
        private const int IP_TTL                                            = 2;

        //
        // Options for level IPPROTO_TCP
        //
        private const int TCP_NODELAY                                       = 0x01; // don't delay send to coalesce packets                                             
        private const int TCP_KEEPALIVE                                     = 0x02; // send KEEPALIVE probes when idle for pcb->keep_idle milliseconds 
        private const int TCP_KEEPIDLE                                      = 0x03; // set pcb->keep_idle  - Same as TCP_KEEPALIVE, but use seconds for get/setsockopt 
        private const int TCP_KEEPINTVL                                     = 0x04; // set pcb->keep_intvl - Use seconds for get/setsockopt 
        private const int TCP_KEEPCNT                                       = 0x05; // set pcb->keep_cnt   - Use number of probes sent for get/setsockopt 

        //
        // Options for level IPPROTO_UDPLITE
        //
        private const int UDPLITE_SEND_CSCOV                                = 0x01; // sender checksum coverage 
        private const int UDPLITE_RECV_CSCOV                                = 0x02; // minimal receiver checksum coverage 

        //
        // Options and types for UDP multicast traffic handling
        //
        private const int IP_ADD_MEMBERSHIP                                 = 3;
        private const int IP_DROP_MEMBERSHIP                                = 4;
        private const int IP_MULTICAST_TTL                                  = 5;
        private const int IP_MULTICAST_IF                                   = 6;
        private const int IP_MULTICAST_LOOP                                 = 7;
            
        //--//
        //--//
        //--//

        private const int SOCK_SOCK_STREAM                                  = 1;
        private const int SOCK_SOCK_DGRAM                                   = 2;
        private const int SOCK_SOCK_RAW                                     = 3;
        private const int SOCK_SOCK_RDM                                     = 4;
        private const int SOCK_SOCK_SEQPACKET                               = 5;
        private const int SOCK_SOCK_PACK_EX                                 = 6;

        private const int SOCK_TCP_NODELAY                                  = 0x0001;

        private const int SOCK_IPPROTO_IP                                   = 0;
        private const int SOCK_IPPROTO_ICMP                                 = 1;
        private const int SOCK_IPPROTO_IGMP                                 = 2;
        private const int SOCK_IPPROTO_IPV4                                 = 4;    // IP-in-IP encapsulation 
        private const int SOCK_IPPROTO_TCP                                  = 6;
        private const int SOCK_IPPROTO_PUP                                  = 12;
        private const int SOCK_IPPROTO_UDP                                  = 17;
        private const int SOCK_IPPROTO_IDP                                  = 22;
        private const int SOCK_IPPROTO_IPV6                                 = 41;
        private const int SOCK_IPPROTO_IPv6RoutingHeader                    = 43;
        private const int SOCK_IPPROTO_IPv6FragmentHeader                   = 44;
        private const int SOCK_IPPROTO_RDP                                  = 46;
        private const int SOCK_IPPROTO_GRE                                  = 47;
        private const int SOCK_IPPROTO_IPSecEncapsulatingSecurityPayload    = 50;
        private const int SOCK_IPPROTO_IPSecAuthenticationHeader            = 51;
        private const int SOCK_IPPROTO_IcmpV6                               = 58;
        private const int SOCK_IPPROTO_IPv6NoNextHeader                     = 59;
        private const int SOCK_IPPROTO_IPv6DestinationOptions               = 60;
        private const int SOCK_IPPROTO_ND                                   = 77;
        private const int SOCK_IPPROTO_OSPF                                 = 89;
        private const int SOCK_IPPROTO_TPACKET                              = 127;
        private const int SOCK_IPPROTO_RAW                                  = 255;
        private const int SOCK_IPPROTO_IPX                                  = 1000;
        private const int SOCK_IPPROTO_SPX                                  = 1256;
        private const int SOCK_IPPROTO_SPXII                                = 1257;
        private const int SOCK_SOL_SOCKET                                   = 0xFFFF;

        //
        // Option flags per-socket 
        //
        private const int SOCK_SOCKO_DEBUG                                  = 0x0001;                   // turn on debugging info recording
        private const int SOCK_SOCKO_NOCHECKSUM                             = 0x0001;
        private const int SOCK_SOCKO_ACCEPTCONNECTION                       = 0x0002;                   // socket has had listen()
        private const int SOCK_SOCKO_REUSEADDRESS                           = 0x0004;                   // allow local address reuse
        private const int SOCK_SOCKO_KEEPALIVE                              = 0x0008;                   // keep connections alive
        private const int SOCK_SOCKO_DONTROUTE                              = 0x0010;                   // just use interface addresses
        private const int SOCK_SOCKO_BROADCAST                              = 0x0020;                   // permit sending of broadcast msgs
        private const int SOCK_SOCKO_USELOOPBACK                            = 0x0040;                   // bypass hardware when possible
        private const int SOCK_SOCKO_LINGER                                 = 0x0080;                   // linger on close if data present
        private const int SOCK_SOCKO_OUTOFBANDINLINE                        = 0x0100;                   // leave received OOB data in line
        private const int SOCK_SOCKO_DONTLINGER                             = ~SOCK_SOCKO_LINGER;
        private const int SOCK_SOCKO_EXCLUSIVEADDRESSUSE                    = ~SOCK_SOCKO_REUSEADDRESS; // disallow local address reuse
        private const int SOCK_SOCKO_SENDBUFFER                             = 0x1001;                   // send buffer size
        private const int SOCK_SOCKO_SNDBUF                                 = SOCK_SOCKO_SENDBUFFER;
        private const int SOCK_SOCKO_RECEIVEBUFFER                          = 0x1002;                   // receive buffer size
        private const int SOCK_SOCKO_RCVBUF                                 = SOCK_SOCKO_RECEIVEBUFFER;
        private const int SOCK_SOCKO_SENDLOWWATER                           = 0x1003;                   // send low-water mark
        private const int SOCK_SOCKO_RECEIVELOWWATER                        = 0x1004;                   // receive low-water mark
        private const int SOCK_SOCKO_SENDTIMEOUT                            = 0x1005;                   // send timeout
        private const int SOCK_SOCKO_RECEIVETIMEOUT                         = 0x1006;                   // receive timeout
        private const int SOCK_SOCKO_ERROR                                  = 0x1007;                   // get error status and clear
        private const int SOCK_SOCKO_TYPE                                   = 0x1008;                   // get socket type
        private const int SOCK_SOCKO_UPDATE_ACCEPT_CTX                      = 0x700B;                   // This option updates the properties of the socket which are inherited from the listening socket.
        private const int SOCK_SOCKO_UPDATE_CONNECT_CTX                     = 0x7010;                   // This option updates the properties of the socket after the connection is established.
        private const int SOCK_SOCKO_MAXCONNECTIONS                         = 0x7FFFFFFF;               // Maximum queue length specifiable by listen.

        //
        // Option flags per-IP  
        //
        private const int SOCK_IPO_OPTIONS                                  = 0x0001;
        private const int SOCK_IPO_HDRINCL                                  = 0x0002;
        private const int SOCK_IPO_TOS                                      = 0x0003;
        private const int SOCK_IPO_TTL                                      = 0x0004;
        private const int SOCK_IPO_MULTICAST_IF                             = 0x0009;
        private const int SOCK_IPO_MULTICAST_TTL                            = 0x000A;
        private const int SOCK_IPO_MULTICAST_LOOP                           = 0x000B;
        private const int SOCK_IPO_ADD_MEMBERSHIP                           = 0x000C;
        private const int SOCK_IPO_DROP_MEMBERSHIP                          = 0x000D;
        private const int SOCK_IPO_IP_DONTFRAGMENT                          = 0x000E;
        private const int SOCK_IPO_ADD_SOURCE_MEMBERSHIP                    = 0x000F;
        private const int SOCK_IPO_DROP_SOURCE_MEMBERSHIP                   = 0x0010;
        private const int SOCK_IPO_BLOCK_SOURCE                             = 0x0011;
        private const int SOCK_IPO_UBLOCK_SOURCE                            = 0x0012;
        private const int SOCK_IPO_PACKET_INFO                              = 0x0013;
        
        private const int SOCK_SOCKET_ERROR                                 = -1;
        
        //--//
        //--//
        //--//

        public override int socket(int family, int type, int protocol)
        {
            return SocketNative.LLOS_lwip_socket(family, type, protocol);
        }

        public override int bind(int socket, byte[] address)
        {
            return SocketNative.LLOS_lwip_bind(socket, address);
        }
        
        public unsafe override int connect(int socket, byte[] address, bool fThrowOnWouldBlock)
        {
            fixed (byte* addressPtr = address)
            {
                return SocketNative.LLOS_lwip_connect(socket, addressPtr, fThrowOnWouldBlock);
            }
        }
        
        public unsafe override int send(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms)
        {
            fixed (byte* buffer = &buf[offset])
            {
                return SocketNative.LLOS_lwip_send(socket, buffer, count, flags, timeout_ms);
            }
        }
        
        public unsafe override int recv(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms)
        {
            fixed (byte* buffer = &buf[offset])
            {
                return SocketNative.LLOS_lwip_recv(socket, buffer, count, flags, timeout_ms);
            }
        }
        
        public override int close(int socket)
        {
            return SocketNative.LLOS_lwip_close(socket);
        }
        
        public override int listen(int socket, int backlog)
        {
            return SocketNative.LLOS_lwip_listen(socket, backlog);
        }
        
        public unsafe override int accept(int socket)
        {
            byte[] address = new byte[20];
            uint* pAddrlen = null;

            fixed(byte* pAddress = address)
            {
                return SocketNative.LLOS_lwip_accept(socket, pAddress, pAddrlen);
            }
        }

        //
        // No standard non-blocking api
        //

        public unsafe override int getaddrinfo(string name, out string canonicalName, out byte[][] addresses)
        {
            byte[] canonicalNameArr = new byte[256];

            // We know this will be the size of the address object
            byte[] addressArr = new byte[16];

            int result;

            fixed (char* pName = name.ToCharArray())
            {
                fixed (byte* pCanonicalName = canonicalNameArr)
                {
                    fixed (byte* pAddress = addressArr)
                    {
                        result = SocketNative.LLOS_lwip_getaddrinfo(pName, (uint)name.Length, pCanonicalName, (uint)canonicalNameArr.Length, pAddress, (uint)addressArr.Length);
                    }
                }
            }

            if(result < 0)
            {
                // The format of address in addressArr may not be what DNS wants. Need to verify
                canonicalName = null;
                addresses = null;
                addresses[0] = null;

                return result;
            }

            // The format of address in addressArr may not be what DNS wants. Need to verify
            canonicalName = ASCIIEncoding.ASCII.GetString(canonicalNameArr);
            addresses = new byte[1][];
            addresses[0] = addressArr;

            return result;
        }
        
        public unsafe override void shutdown(int socket, int how, out int err)
        {
            err = SocketNative.LLOS_lwip_shutdown(socket, how);
        }
        
        public unsafe override int sendto(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms, byte[] address)
        {
            fixed(byte* pBuf = &buf[offset])
            {
                fixed(byte* pAddress = address)
                {
                    return SocketNative.LLOS_lwip_sendto(socket, pBuf, count, flags, timeout_ms, pAddress, (uint)address.Length);
                }
            }
        }
        
        public unsafe override int recvfrom(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms, ref byte[] address)
        {
            uint fromlen = (uint)address.Length;
            fixed (byte* pBuf = &buf[offset])
            {
                fixed (byte* pAddress = address)
                {
                    return SocketNative.LLOS_lwip_recvfrom(socket, pBuf, count, flags, timeout_ms, pAddress, &fromlen);
                }
            }
        }
        
        public unsafe override int getpeername(int socket, out byte[] address)
        {
            uint namelen = 20;
            byte[] addressBuffer = new byte[namelen];
            
            int result;
            fixed(byte* pAddress = addressBuffer)
            {
                result = SocketNative.LLOS_lwip_getpeername(socket, pAddress, &namelen);
            }

            if(result < 0)
            {
                address = null;
                return result;
            }

            address = new byte[namelen];
            Array.Copy(addressBuffer, address, (int)namelen);

            return result;
        }
        
        public unsafe override int getsockname(int socket, out byte[] address)
        {
            uint namelen = 20;
            byte[] addressBuffer = new byte[namelen];

            int result;
            fixed (byte* pAddress = addressBuffer)
            {
                result = SocketNative.LLOS_lwip_getsockname(socket, pAddress, &namelen);
            }

            if (result < 0)
            {
                address = null;
                return result;
            }

            address = new byte[namelen];
            Array.Copy(addressBuffer, address, (int)namelen);

            return result;
        }
        
        public unsafe override int getsockopt(int socket, int level, int optname, byte[] optval, out uint optlen)
        {
            int nativeOptionName;
            int nativeLevel;

            switch(level)
            {
                case SOCK_IPPROTO_IP:
                    nativeLevel         = IPPROTO_IP;
                    nativeOptionName    = GetNativeIPOption(optname);
                    break;
                case SOCK_IPPROTO_TCP:    
                    nativeLevel         = IPPROTO_TCP;
                    nativeOptionName    = GetNativeTcpOption(optname);
                    break;
                case SOCK_IPPROTO_UDP: 
                case SOCK_IPPROTO_ICMP:
                case SOCK_IPPROTO_IGMP:
                case SOCK_IPPROTO_IPV4:
                case SOCK_SOL_SOCKET:
                    nativeLevel         = SOL_SOCKET;
                    nativeOptionName    = GetNativeSockOption(optname);
                    switch(optname)
                    {        
                        // LINGER is not implemented in LWIP
                        case SOCK_SOCKO_LINGER:
                            optval[0] = 0; optlen = 1;
                            return 0;
                        case SOCK_SOCKO_DONTLINGER:
                            optval[0] = 1; optlen = 1;
                            return 0;
                        default:
                            break;
                    }
                    break;
                default:
                    nativeLevel         = level;
                    nativeOptionName    = optname;
                    break;
            }
            
            uint optlenInternal = (uint)optval.Length;
            int result;
            fixed (byte* pNativeOptval = optval)
            {
                result = SocketNative.LLOS_lwip_getsockopt( socket, nativeLevel, nativeOptionName, pNativeOptval, &optlenInternal );
                optlen = optlenInternal;

                if(result == 0)
                {
                    switch(level)
                    {
                        case SOCK_SOL_SOCKET:
                            switch(optname)
                            {
                                case SOCK_SOCKO_EXCLUSIVEADDRESSUSE:
                                    optval[0] = (!( *(int*)pNativeOptval != 0 ) == true) ? (byte)1 : (byte)0;
                                    optlen = 1;
                                    break;

                                case SOCK_SOCKO_ACCEPTCONNECTION:
                                case SOCK_SOCKO_BROADCAST:
                                case SOCK_SOCKO_KEEPALIVE:
                                    optval[0] = (( *(int*)pNativeOptval != 0 ) == true ) ? (byte)1 : (byte)0;
                                    optlen = 1;
                                    break;
                            }
                            break;
                    }
                }
            }

            return result;
        }

        public unsafe override int setsockopt( int socket, int level, int optname, byte[] optval )
        {
            int nativeLevel;
            int nativeOptionName;
            int nativeIntValue;

            fixed (byte* pNativeOptionValue = optval)
            {
                switch(level)
                {
                    case SOCK_IPPROTO_IP:
                        nativeLevel = IPPROTO_IP;
                        nativeOptionName = GetNativeIPOption( optname );
                        break;
                    case SOCK_IPPROTO_TCP:
                        nativeLevel = IPPROTO_TCP;
                        nativeOptionName = GetNativeTcpOption( optname );
                        break;
                    case SOCK_IPPROTO_UDP:
                    case SOCK_IPPROTO_ICMP:
                    case SOCK_IPPROTO_IGMP:
                    case SOCK_IPPROTO_IPV4:
                    case SOCK_SOL_SOCKET:
                        nativeLevel = SOL_SOCKET;
                        nativeOptionName = GetNativeSockOption( optname );

                        switch(optname)
                        {
                            // LINGER is not implemented in LWIP
                            case SOCK_SOCKO_LINGER:
                                if(*(int*)pNativeOptionValue != 0)
                                {
                                    //errno = SOCK_ENOPROTOOPT;
                                    return SOCK_SOCKET_ERROR;
                                }
                                return 0;
                            case SOCK_SOCKO_DONTLINGER:
                                if(*(int*)pNativeOptionValue == 0)
                                {
                                    //errno = SOCK_ENOPROTOOPT;
                                    return SOCK_SOCKET_ERROR;
                                }
                                return 0;

                            // ignore this item to enable http to work
                            case SOCK_SOCKO_REUSEADDRESS:
                                return 0;

                            case SOCK_SOCKO_EXCLUSIVEADDRESSUSE:
                                nativeIntValue = *(int*)&pNativeOptionValue[0] == 0 ? 1 : 0;
                                optval[ 0 ] = (byte)nativeIntValue;
                                //pNativeOptionValue = (char*)&nativeIntValue;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        nativeLevel = 0;
                        nativeOptionName = 0;
                        break;
                }

                return SocketNative.LLOS_lwip_setsockopt( socket, nativeLevel, nativeOptionName, pNativeOptionValue, (uint)optval.Length );
            }
        }
        
        public unsafe override bool poll(int socket, int mode, int microSeconds)
        {
            return SocketNative.LLOS_lwip_poll(socket, mode, microSeconds);
        }
        
        public unsafe override int ioctl(int socket, uint cmd, ref uint arg)
        {
            fixed (uint* pArg = &arg)
            {
                return SocketNative.LLOS_lwip_ioctl(socket, cmd, pArg);
            }
        }

        //--//
        //--//
        //--//

        static private int GetNativeTcpOption( int optname )
        {
            int nativeOptionName = 0;

            switch(optname)
            {
                case SOCK_TCP_NODELAY:
                    nativeOptionName = TCP_NODELAY;
                    break;

                case SOCK_SOCKO_KEEPALIVE:
                    nativeOptionName = TCP_KEEPALIVE;
                    break;

                // allow the C# user to specify LWIP options that our managed enum
                // doesn't support
                default:
                    nativeOptionName = optname;
                    break;
            }
            return nativeOptionName;
        }

        static private int GetNativeSockOption( int optname )
        {
            int nativeOptionName = 0;

            switch(optname)
            {
                case SOCK_SOCKO_DONTLINGER:
                case SOCK_SOCKO_LINGER:
                    nativeOptionName = SO_LINGER;
                    break;
                case SOCK_SOCKO_SENDTIMEOUT:
                    nativeOptionName = SO_SNDTIMEO;
                    break;
                case SOCK_SOCKO_RECEIVETIMEOUT:
                    nativeOptionName = SO_RCVTIMEO;
                    break;
                case SOCK_SOCKO_EXCLUSIVEADDRESSUSE:
                case SOCK_SOCKO_REUSEADDRESS:
                    nativeOptionName = SO_REUSEADDR;
                    break;
                case SOCK_SOCKO_KEEPALIVE:
                    nativeOptionName = SO_KEEPALIVE;
                    break;
                case SOCK_SOCKO_ERROR:
                    nativeOptionName = SO_ERROR;
                    break;
                case SOCK_SOCKO_BROADCAST:
                    nativeOptionName = SO_BROADCAST;
                    break;
                case SOCK_SOCKO_RECEIVEBUFFER:
                    nativeOptionName = SO_RCVBUF;
                    break;
                case SOCK_SOCKO_SENDBUFFER:
                    nativeOptionName = SO_SNDBUF;
                    break;
                case SOCK_SOCKO_ACCEPTCONNECTION:
                    nativeOptionName = SO_ACCEPTCONN;
                    break;
                case SOCK_SOCKO_TYPE:
                    nativeOptionName = SO_TYPE;
                    break;

                case SOCK_SOCKO_USELOOPBACK:
                    nativeOptionName = SO_USELOOPBACK;
                    break;
                case SOCK_SOCKO_DONTROUTE:
                    nativeOptionName = SO_DONTROUTE;
                    break;
                case SOCK_SOCKO_OUTOFBANDINLINE:
                    nativeOptionName = SO_OOBINLINE;
                    break;

                case SOCK_SOCKO_DEBUG:
                    nativeOptionName = SO_DEBUG;
                    break;

                case SOCK_SOCKO_SENDLOWWATER:
                    nativeOptionName = SO_SNDLOWAT;
                    break;

                case SOCK_SOCKO_RECEIVELOWWATER:
                    nativeOptionName = SO_RCVLOWAT;
                    break;

                //        case SOCK_SOCKO_MAXCONNECTIONS:         //don't support
                case SOCK_SOCKO_UPDATE_ACCEPT_CTX:
                case SOCK_SOCKO_UPDATE_CONNECT_CTX:
                    nativeOptionName = 0;
                    break;

                // allow the C# user to specify LWIP options that our managed enum
                // doesn't support
                default:
                    nativeOptionName = optname;
                    break;

            }

            return nativeOptionName;
        }

        static private int GetNativeIPOption( int optname )
        {
            int nativeOptionName = 0;

            switch(optname)
            {
                case SOCK_IPO_TTL:
                    nativeOptionName = IP_TTL;
                    break;
                case SOCK_IPO_TOS:
                    nativeOptionName = IP_TOS;
                    break;
#if LWIP_IGMP
        case SOCK_IPO_MULTICAST_IF:
            nativeOptionName = IP_MULTICAST_IF;
            break;
        case SOCK_IPO_MULTICAST_TTL:  
            nativeOptionName = IP_MULTICAST_TTL;
            break;
        case SOCK_IPO_MULTICAST_LOOP: 
            nativeOptionName = IP_MULTICAST_LOOP;
            break;
        case SOCK_IPO_ADD_MEMBERSHIP:
            nativeOptionName = IP_ADD_MEMBERSHIP;
            break;
        case SOCK_IPO_DROP_MEMBERSHIP:
            nativeOptionName = IP_DROP_MEMBERSHIP;
            break;
#else
                case SOCK_IPO_MULTICAST_IF:
                case SOCK_IPO_MULTICAST_TTL:
                case SOCK_IPO_MULTICAST_LOOP:
                case SOCK_IPO_ADD_MEMBERSHIP:
                case SOCK_IPO_DROP_MEMBERSHIP:
#endif
                case SOCK_IPO_ADD_SOURCE_MEMBERSHIP:
                case SOCK_IPO_DROP_SOURCE_MEMBERSHIP:
                case SOCK_IPO_OPTIONS:
                case SOCK_IPO_HDRINCL:
                case SOCK_IPO_IP_DONTFRAGMENT:
                case SOCK_IPO_BLOCK_SOURCE:
                case SOCK_IPO_UBLOCK_SOURCE:
                case SOCK_IPO_PACKET_INFO:
                    nativeOptionName = 0;
                    break;

                // allow the C# user to specify LWIP options that our managed enum
                // doesn't support
                default:
                    nativeOptionName = optname;
                    break;
            }

            return nativeOptionName;
        }
    }
}
