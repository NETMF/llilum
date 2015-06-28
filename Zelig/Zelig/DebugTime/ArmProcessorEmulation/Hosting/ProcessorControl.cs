//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    public abstract class ProcessorControl
    {
        public enum State
        {
            Stopped  ,
            Starting ,
            Executing,
            Stopping ,
        }

        public delegate void ProgressCallback( string format, params object[] args );

        public delegate void StateChangeCallback( State oldState, State newState );

        //
        // State
        //

        protected StateChangeCallback m_eventStateChange;
        private   bool                m_fLastStopWasForced;

        //
        // Helper Methods
        //

        public abstract void GetBreakpointCapabilities( out object softBreakpointOpcode ,
                                                        out int    maxHardBreakpoints   );

        public virtual void Shutdown()
        {
            m_eventStateChange = null;
        }

        public virtual void ResetState( Configuration.Environment.ProductCategory product )
        {
            m_eventStateChange = null;
        }

        public abstract void PrepareHardwareModels( Configuration.Environment.ProductCategory product );

        public abstract void DeployImage( List< Configuration.Environment.ImageSection > image    ,
                                          ProgressCallback                               callback );

        public abstract void StartPlugIns( List< Type > extraHandlers );
        public abstract void StopPlugIns();

        public abstract void Execute    ( List< Breakpoint > breakpoints );
        public abstract void ExecuteStep( List< Breakpoint > breakpoints );

        public void Notify( State oldState ,
                            State newState )
        {
            switch(newState)
            {
                case State.Starting:
                    m_fLastStopWasForced = false;
                    break;

                case State.Stopping:
                    m_fLastStopWasForced = true;
                    break;
            }

            var notify = m_eventStateChange;

            if(notify != null)
            {
                notify( oldState, newState );
            }
        }

        //--//

        //
        // Access Methods
        //

        public abstract bool StopExecution
        {
            get;
            set;
        }

        public bool LastStopWasForced
        {
            get
            {
                return m_fLastStopWasForced;
            }
        }

        public event StateChangeCallback NotifyStateChange
        {
            add
            {
                m_eventStateChange += value;
            }

            remove
            {
                m_eventStateChange -= value;
            }
        }
    }
}
