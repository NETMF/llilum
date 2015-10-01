//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    public abstract class BusControllerCategory : MemoryCategory
    {
        protected BusControllerCategory()
        {
            this.BaseAddress = 0;
            this.SizeInBytes = 1L << 32; // A bus controller looks at the whole address range.
        }
    }
}
