//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    public interface IMemoryMapper
    {
        uint GetCacheableAddress( uint address );

        uint GetUncacheableAddress( uint address );
    }
}
