//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class NetworkInterfaceProvider
    {
        private sealed class EmptyNetworkInterfaceProvider : NetworkInterfaceProvider
        {
            public override int Connect(uint timeout)
            {
                throw new NotImplementedException();
            }

            public override int Disconnect()
            {
                throw new NotImplementedException();
            }

            public override string GetDefaultLocalAddress()
            {
                throw new NotImplementedException();
            }

            public override string GetGatewayAddress()
            {
                throw new NotImplementedException();
            }

            public override string GetIPAddress()
            {
                throw new NotImplementedException();
            }

            public override string GetMacAddress()
            {
                throw new NotImplementedException();
            }

            public override string GetMask()
            {
                throw new NotImplementedException();
            }

            public override int GetNetworkInterfaceCount()
            {
                throw new NotImplementedException();
            }

            public override int InitializeEthernet()
            {
                return -1; // Fail
            }

            public override int InitializeEthernet(string ipAddress, string mask, string gateway)
            {
                return -1; // Fail
            }

            public override uint IPv4AddressFromString(string ipAddress)
            {
                throw new NotImplementedException();
            }

            public override void RemapInterrupts()
            {
                throw new NotImplementedException();
            }
        }

        public abstract int InitializeEthernet();

        public abstract int InitializeEthernet(string ipAddress, string mask, string gateway);

        public abstract int Connect(uint timeout);

        public abstract int Disconnect();

        public abstract int GetNetworkInterfaceCount();

        public abstract string GetIPAddress();

        public abstract string GetMacAddress();

        public abstract string GetGatewayAddress();

        public abstract string GetMask();
        
        public abstract uint IPv4AddressFromString(string ipAddress);

        public abstract string GetDefaultLocalAddress();

        public abstract void RemapInterrupts();

        //--//

        public static extern NetworkInterfaceProvider Instance
        {
            [SingletonFactory(Fallback = typeof(EmptyNetworkInterfaceProvider))]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
    }
}
