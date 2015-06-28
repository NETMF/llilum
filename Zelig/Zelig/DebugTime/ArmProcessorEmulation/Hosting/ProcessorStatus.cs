//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;

    using IR = Microsoft.Zelig.CodeGeneration.IR;


    public abstract class ProcessorStatus
    {
        public delegate void TrackExternalProgramFlowChange();

        //
        // State
        //

        protected TrackExternalProgramFlowChange m_eventExternalProgramFlowChange;

        //
        // Helper Methods
        //

        public abstract BinaryBlob GetRegister( IR.Abstractions.RegisterDescriptor reg );

        public abstract bool SetRegister( IR.Abstractions.RegisterDescriptor reg ,
                                          BinaryBlob                         bb  );

        public virtual void RaiseExternalProgramFlowChange()
        {
            var dlg = m_eventExternalProgramFlowChange;
            if(dlg != null)
            {
                dlg();
            }
        }

        //--//

        //
        // Access Methods
        //

        public abstract uint ProgramCounter
        {
            get;
            set;
        }

        public abstract uint StackPointer
        {
            get;
            set;
        }

        public event TrackExternalProgramFlowChange NotifyOnExternalProgramFlowChange
        {
            add
            {
                m_eventExternalProgramFlowChange += value;
            }

            remove
            {
                m_eventExternalProgramFlowChange -= value;
            }
        }
    }
}
