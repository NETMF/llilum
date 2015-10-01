//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    public abstract class CacheControllerCategory : BusControllerCategory
    {
        public abstract uint GetUncacheableAddress( uint address );
    }
}
