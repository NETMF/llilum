//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.API.IO
{
    using System.Runtime.InteropServices;

    public static unsafe class SocketNative
    {
        [DllImport("C")]
        public static extern int LLOS_lwip_socket(int family, int type, int protocol);

        [DllImport("C")]
        public static extern int LLOS_lwip_bind(int socket, byte[] address);

        [DllImport("C")]
        public static extern int LLOS_lwip_connect(int socket, byte* address, bool fThrowOnWouldBlock);

        [DllImport("C")]
        public static extern int LLOS_lwip_send(int socket, byte* buf, int count, int flags, int timeout_ms);

        [DllImport("C")]
        public static extern int LLOS_lwip_recv(int socket, byte* buf, int count, int flags, int timeout_ms);

        [DllImport("C")]
        public static extern int LLOS_lwip_close(int socket);

        [DllImport("C")]
        public static extern int LLOS_lwip_listen(int socket, int backlog);

        [DllImport("C")]
        public static extern int LLOS_lwip_accept(int socket, byte* address, uint* addrlen);

        //Non standard non-blocking api
        [DllImport("C")]
        public static extern int LLOS_lwip_getaddrinfo(char* name, uint namelen, byte* canonicalName, uint canonicalNameBufferSize, byte* addresses, uint addressesBufferSize);

        [DllImport("C")]
        public static extern int LLOS_lwip_shutdown(int socket, int how);

        [DllImport("C")]
        public static extern int LLOS_lwip_sendto(int socket, byte* buf, int count, int flags, int timeout_ms, byte* address, uint tolen);

        [DllImport("C")]
        public static extern int LLOS_lwip_recvfrom(int socket, byte* buf, int count, int flags, int timeout_ms, byte* address, uint* fromlen);

        [DllImport("C")]
        public static extern int LLOS_lwip_getpeername(int socket, byte* address, uint* namelen);

        [DllImport("C")]
        public static extern int LLOS_lwip_getsockname(int socket, byte* address, uint* namelen);

        [DllImport("C")]
        public static extern int LLOS_lwip_getsockopt(int socket, int level, int optname, byte* optval, uint* optlen);

        [DllImport("C")]
        public static extern int LLOS_lwip_setsockopt(int socket, int level, int optname, byte* optval, uint optlen);

        [DllImport("C")]
        public static extern bool LLOS_lwip_poll(int socket, int mode, int microSeconds);

        [DllImport("C")]
        public static extern int LLOS_lwip_ioctl(int socket, uint cmd, uint* arg);
    }
}
