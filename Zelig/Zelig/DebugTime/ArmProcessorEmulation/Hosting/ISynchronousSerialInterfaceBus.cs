//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    //
    // Master mode, from controller to bus.
    //
    public interface ISynchronousSerialInterfaceBus
    {
        uint ShiftData( uint value          ,
                        int  bitSize        ,
                        int  clockFrequency );
    }
}
