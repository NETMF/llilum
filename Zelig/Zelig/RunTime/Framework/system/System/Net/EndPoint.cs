//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;
    using System.Net.Sockets;

    // Generic abstraction to identify network addresses

    /// <devdoc>
    ///    <para>
    ///       Identifies a network address.
    ///    </para>
    /// </devdoc>
    [Serializable]
    public abstract class EndPoint
    {
        public abstract SocketAddress Serialize();
        public abstract EndPoint Create(SocketAddress socketAddress);

    }; // abstract class EndPoint

} // namespace System.Net


