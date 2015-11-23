//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using Chipset = Microsoft.CortexM3OnCMSISCore;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;
    using Framework = Microsoft.Llilum.Devices.Adc;
    using Runtime = Microsoft.Zelig.Runtime;
    using Zelig.LlilumOSAbstraction.API.IO;
    using System.Text;

    public sealed class SocketProvider : Runtime.SocketProvider
    {
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

        //No standard non-blocking api

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
            uint optlenInternal = (uint)optval.Length;
            int result;
            fixed (byte* pOptval = optval)
            {
                result = SocketNative.LLOS_lwip_getsockopt(socket, level, optname, pOptval, &optlenInternal);
            }
            optlen = optlenInternal;

            return result;
        }


        public unsafe override int setsockopt(int socket, int level, int optname, byte[] optval)
        {
            fixed(byte* pOptval = optval)
            {
                return SocketNative.LLOS_lwip_setsockopt(socket, level, optname, pOptval, (uint)optval.Length);
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
    }
}
