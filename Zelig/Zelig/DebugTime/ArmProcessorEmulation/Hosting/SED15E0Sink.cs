//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    public abstract class SED15E0Sink
    {
        public abstract void NewScreenShot( uint[] buffer, int width, int height );
    }
}
