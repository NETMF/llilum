//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using Cfg = Microsoft.Zelig.Configuration.Environment;


    public abstract class JTagConnector
    {
        public struct RegisterSet
        {
            public string Name;
            public object Value;
        }

        //
        // State
        //

        bool             m_fStopExecution;
        Timer            m_timer;

        //
        // Constructor Methods
        //

        protected JTagConnector()
        {
             m_timer = new Timer( new TimerCallback(OnTimer), this, -1, -1 );
        }

        //
        // Helper Methods
        //

        public abstract bool IsTargetStopped();

        public abstract void StopTarget();

        public abstract bool ExecuteCode( int           timeout ,
                                          RegisterSet[] input   ,
                                          RegisterSet[] output  );

        public void RunDevice()
        {
            RunDevice( Timeout.Infinite );
        }

        public bool RunDevice( int timeout )
        {
            m_fStopExecution = false;

            if(timeout == Timeout.Infinite)
            {
                RunDeviceInner( ref m_fStopExecution );
            }
            else
            {
                m_timer.Change( timeout, -1 );

                RunDeviceInner( ref m_fStopExecution );

                m_timer.Change( -1, -1 );
            }

            return m_fStopExecution == false;
        }

        public void AbortRunDevice()
        {
            m_fStopExecution = true;
        }

        public abstract uint[] ReadMemoryBlock( uint address ,
                                                int  count   );

        public abstract void WriteMemoryBlock( uint   address ,
                                               uint[] values  ,
                                               int    offset  ,
                                               int    count   );

        public void WriteMemoryBlock( uint   address ,
                                      uint[] values  )
        {
            WriteMemoryBlock( address, values, 0, values.Length );
        }

        //--//

        public virtual void Cleanup()
        {
            m_timer.Change( -1, -1 );
        }

        //--//

        protected abstract void RunDeviceInner( ref bool fStop );

        //--//

        private static void OnTimer(object state)
        {
            JTagConnector pThis = (JTagConnector)state;

            pThis.m_fStopExecution = true;
        }

        //
        // Access Methods
        //

        public abstract uint ProgramCounter
        {
            set;
        }
    }
}
