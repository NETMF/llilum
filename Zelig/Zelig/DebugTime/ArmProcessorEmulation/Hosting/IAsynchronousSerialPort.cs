//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    public interface IAsynchronousSerialInterface
    {
        void Send( byte value );

        bool Receive( int timeout, out byte value );

        int PortNumber
        {
            get;
        }
    }
}
