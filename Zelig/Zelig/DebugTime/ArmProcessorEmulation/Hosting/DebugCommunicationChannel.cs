//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    public abstract class DebugCommunicationChannel
    {
        //
        // State
        //

        protected int m_useCount;

        //
        // Helper Methods
        //

        public virtual void Start()
        {
            m_useCount++;
        }

        public virtual void Stop()
        {
            m_useCount--;
        }

        public abstract bool ReadFromProcessor ( out uint value );
        public abstract bool WriteFromProcessor(     uint value );

        public abstract bool ReadFromDebugger ( out uint value, int Timeout );
        public abstract bool WriteFromDebugger(     uint value, int Timeout );

        public abstract void AbortDebuggerRead ();
        public abstract void AbortDebuggerWrite();

        //--//

        //
        // Access Methods
        //

        public bool IsActive
        {
            get
            {
                return m_useCount != 0;
            }
        }
    }
}
