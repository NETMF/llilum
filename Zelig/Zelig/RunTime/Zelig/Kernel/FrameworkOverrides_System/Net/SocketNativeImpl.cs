//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    [ExtendClass(typeof(Microsoft.Llilum.Net.SocketNative), NoConstructors = true)]
    public abstract class SocketNativeImpl
    {
        public static int socket(int family, int type, int protocol)
        {
            return SocketProvider.Instance.socket(family, type, protocol);
        }
        
        public static int bind(int socket, byte[] address)
        {
            return SocketProvider.Instance.bind(socket, address);
        }

        
        public static int connect(int socket, byte[] address, bool fThrowOnWouldBlock)
        {
            return SocketProvider.Instance.connect(socket, address, fThrowOnWouldBlock);
        }


        public static int send(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms)
        {
            return SocketProvider.Instance.send(socket, buf, offset, count, flags, timeout_ms);
        }


        public static int recv(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms)
        {
            return SocketProvider.Instance.recv(socket, buf, offset, count, flags, timeout_ms);
        }


        public static int close(int socket)
        {
            return SocketProvider.Instance.close(socket);
        }


        public static int listen(int socket, int backlog)
        {
            return SocketProvider.Instance.listen(socket, backlog);
        }


        public static int accept(int socket)
        {
            return SocketProvider.Instance.accept(socket);
        }

        public static void getaddrinfo(string name, out string canonicalName, out byte[][] addresses)
        {
            SocketProvider.Instance.getaddrinfo(name, out canonicalName, out addresses);
        }


        public static void shutdown(int socket, int how, out int err)
        {
            SocketProvider.Instance.shutdown(socket, how, out err);
        }


        public static int sendto(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms, byte[] address)
        {
            return SocketProvider.Instance.sendto(socket, buf, offset, count, flags, timeout_ms, address);
        }


        public static int recvfrom(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms, ref byte[] address)
        {
            return SocketProvider.Instance.recvfrom(socket, buf, offset, count, flags, timeout_ms, ref address);
        }


        public static void getpeername(int socket, out byte[] address)
        {
            SocketProvider.Instance.getpeername(socket, out address);
        }


        public static void getsockname(int socket, out byte[] address)
        {
            SocketProvider.Instance.getsockname(socket, out address);
        }


        public static void getsockopt(int socket, int level, int optname, byte[] optval, out uint optlen)
        {
            SocketProvider.Instance.getsockopt(socket, level, optname, optval, out optlen);
        }


        public static void setsockopt(int socket, int level, int optname, byte[] optval)
        {
            SocketProvider.Instance.setsockopt(socket, level, optname, optval);
        }


        public static bool poll(int socket, int mode, int microSeconds)
        {
            return SocketProvider.Instance.poll(socket, mode, microSeconds);
        }


        public static void ioctl(int socket, uint cmd, ref uint arg)
        {
            SocketProvider.Instance.ioctl(socket, cmd, ref arg);
        }
    }
}
