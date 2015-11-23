//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("System")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.Zelig.Runtime")]

namespace Microsoft.Llilum.Net
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal static class SocketNative
    {
        public const int FIONREAD = 0x4004667F;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int socket(int family, int type, int protocol);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int bind(int socket, byte[] address);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int connect(int socket, byte[] address, bool fThrowOnWouldBlock);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int send(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int recv(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int close(int socket);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int listen(int socket, int backlog);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int accept(int socket);

        //Non standard non-blocking api
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void getaddrinfo(string name, out string canonicalName, out byte[][] addresses);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void shutdown(int socket, int how, out int err);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int sendto(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms, byte[] address);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern int recvfrom(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms, ref byte[] address);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void getpeername(int socket, out byte[] address);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void getsockname(int socket, out byte[] address);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void getsockopt(int socket, int level, int optname, byte[] optval, out uint optlen);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void setsockopt(int socket, int level, int optname, byte[] optval);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern bool poll(int socket, int mode, int microSeconds);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void ioctl(int socket, uint cmd, ref uint arg);
    }
}


