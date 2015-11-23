//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class SocketProvider
    {
        private sealed class EmptySocketProvider : SocketProvider
        {
            public override int accept(int socket)
            {
                throw new NotImplementedException();
            }

            public override int bind(int socket, byte[] address)
            {
                throw new NotImplementedException();
            }

            public override int close(int socket)
            {
                throw new NotImplementedException();
            }

            public override int connect(int socket, byte[] address, bool fThrowOnWouldBlock)
            {
                throw new NotImplementedException();
            }

            public override int getaddrinfo(string name, out string canonicalName, out byte[][] addresses)
            {
                throw new NotImplementedException();
            }

            public override int getpeername(int socket, out byte[] address)
            {
                throw new NotImplementedException();
            }

            public override int getsockname(int socket, out byte[] address)
            {
                throw new NotImplementedException();
            }

            public override int getsockopt(int socket, int level, int optname, byte[] optval, out uint optlen)
            {
                throw new NotImplementedException();
            }

            public override int ioctl(int socket, uint cmd, ref uint arg)
            {
                throw new NotImplementedException();
            }

            public override int listen(int socket, int backlog)
            {
                throw new NotImplementedException();
            }

            public override bool poll(int socket, int mode, int microSeconds)
            {
                throw new NotImplementedException();
            }

            public override int recv(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms)
            {
                throw new NotImplementedException();
            }

            public override int recvfrom(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms, ref byte[] address)
            {
                throw new NotImplementedException();
            }

            public override int send(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms)
            {
                throw new NotImplementedException();
            }

            public override int sendto(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms, byte[] address)
            {
                throw new NotImplementedException();
            }

            public override int setsockopt(int socket, int level, int optname, byte[] optval)
            {
                throw new NotImplementedException();
            }

            public override void shutdown(int socket, int how, out int err)
            {
                throw new NotImplementedException();
            }

            public override int socket(int family, int type, int protocol)
            {
                throw new NotImplementedException();
            }
        }


        public abstract int socket(int family, int type, int protocol);

        
        public abstract int bind(int socket, byte[] address);

        
        public abstract int connect(int socket, byte[] address, bool fThrowOnWouldBlock);

        
        public abstract int send(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms);

        
        public abstract int recv(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms);

        
        public abstract int close(int socket);

        
        public abstract int listen(int socket, int backlog);

        
        public abstract int accept(int socket);

        //Non standard non-blocking api
        
        public abstract int getaddrinfo(string name, out string canonicalName, out byte[][] addresses);

        
        public abstract void shutdown(int socket, int how, out int err);

        
        public abstract int sendto(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms, byte[] address);

        
        public abstract int recvfrom(int socket, byte[] buf, int offset, int count, int flags, int timeout_ms, ref byte[] address);

        
        public abstract int getpeername(int socket, out byte[] address);

        
        public abstract int getsockname(int socket, out byte[] address);

        
        public abstract int getsockopt(int socket, int level, int optname, byte[] optval, out uint optlen);

        
        public abstract int setsockopt(int socket, int level, int optname, byte[] optval);

        
        public abstract bool poll(int socket, int mode, int microSeconds);

        
        public abstract int ioctl(int socket, uint cmd, ref uint arg);

        public static extern SocketProvider Instance
        {
            [SingletonFactory(Fallback = typeof(EmptySocketProvider))]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
    }
}
