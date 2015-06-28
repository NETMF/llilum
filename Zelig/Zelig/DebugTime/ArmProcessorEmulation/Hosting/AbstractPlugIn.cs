//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    public abstract class AbstractPlugIn
    {
        //
        // Helper Methods
        //

        public abstract void Start();

        public abstract void Stop();
    }
}
