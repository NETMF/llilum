//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    public abstract class SimulatorControl
    {
        public abstract void Wait( TimeSpan tm );
    }
}
