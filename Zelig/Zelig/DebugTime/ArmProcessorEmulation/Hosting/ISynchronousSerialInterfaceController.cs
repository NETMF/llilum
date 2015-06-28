//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    //
    // Slave mode, from bus to controller.
    //
    public interface ISynchronousSerialInterfaceController
    {
        uint ShiftData( uint value          ,
                        int  bitSize        ,
                        int  clockFrequency );

        void StartTransaction();

        void EndTransaction();
    }
}
