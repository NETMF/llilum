//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;

    using IR = Microsoft.Zelig.CodeGeneration.IR;


    public abstract class ProcessorPerformance
    {
        //
        // Helper Methods
        //

        public abstract void SuspendTimingUpdates();
        public abstract void ResumeTimingUpdates();

        //--//

        //
        // Access Methods
        //

        public abstract ulong ClockCycles
        {
            get;
        }

        public abstract ulong WaitStates
        {
            get;
        }
    }
}
