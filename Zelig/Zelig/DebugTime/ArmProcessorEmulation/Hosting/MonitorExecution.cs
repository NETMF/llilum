//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    public abstract class MonitorExecution
    {
        //
        // Access Methods
        //

        public abstract bool MonitorMemory
        {
            get;
            set;
        }

        public abstract bool MonitorRegisters
        {
            get;
            set;
        }

        public abstract bool MonitorOpcodes
        {
            get;
            set;
        }

        public abstract bool MonitorCalls
        {
            get;
            set;
        }

        public abstract bool MonitorInterrupts
        {
            get;
            set;
        }

        public abstract bool MonitorInterruptDisabling
        {
            get;
            set;
        }

        public abstract bool NoSleep
        {
            get;
            set;
        }
    }
}
