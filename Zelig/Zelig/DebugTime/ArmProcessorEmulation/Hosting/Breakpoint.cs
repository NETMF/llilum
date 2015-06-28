//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    public class Breakpoint
    {
        public enum Status
        {
            NotSet        ,
            HardBreakpoint,
            SoftBreakpoint,
        }

        [Flags]
        public enum Response
        {
            DoNothing       = 0x00000000,
            StopExecution   = 0x00000001,
            NextInstruction = 0x00000002,
        }

        public delegate Response Callback( Breakpoint bp );

        //
        // State
        //

        private readonly uint                m_address;
        private readonly Debugging.DebugInfo m_di;
        private readonly Callback            m_target;
        private          int                 m_version;
                 
        private          bool                m_fActive;
        private          bool                m_fIsOptional;
        private          bool                m_fIsTemporary;
        private          bool                m_fImplementInHardware;
        private          bool                m_fShowInUI;
        private          long                m_hitCount;
        private          Status              m_status;
                 
        private          bool                m_fHit;
        private          bool                m_fIgnoreOnce;

        //
        // Constructor Methods
        //

        public Breakpoint( uint                address ,
                           Debugging.DebugInfo di      ,
                           Callback            target  )
        {
            m_address = address;
            m_di      = di;
            m_target  = target;
        }

        //
        // Helper Methods
        //

        public bool ShouldStopOverStatement( uint pc )
        {
            m_fHit = false;

            if(!m_fActive)
            {
                return false;
            }

            if(pc == m_address)
            {
                m_fIgnoreOnce = true;

                return true;
            }

            return false;
        }

        public void ClearIgnoreFlag()
        {
            m_fIgnoreOnce = false;
        }

        public Response Hit()
        {
            m_fHit = true;
            m_hitCount++;

            if(m_target == null)
            {
                return Response.StopExecution;
            }

            return m_target( this );
        }

        //--//

        //
        // Access Methods
        //

        public uint Address
        {
            get
            {
                return m_address;
            }
        }

        public Debugging.DebugInfo DebugInfo
        {
            get
            {
                return m_di;
            }
        }

        public int Version
        {
            get
            {
                return m_version;
            }
        }

        public bool IsActive
        {
            get
            {
                return m_fActive;
            }

            set
            {
                m_fActive = value;
                m_version++;
            }
        }

        public Status SetAs
        {
            get
            {
                return m_status;
            }

            set
            {
                m_status = value;
                m_version++;
            }
        }

        public bool ShouldIgnoreOnce
        {
            get
            {
                return m_fIgnoreOnce;
            }
        }

        public bool IsOptional
        {
            get
            {
                return m_fIsOptional;
            }

            set
            {
                m_fIsOptional = value;
                m_version++;
            }
        }

        public bool ShowInUI
        {
            get
            {
                return m_fShowInUI;
            }

            set
            {
                m_fShowInUI = value;
                m_version++;
            }
        }

        public bool IsTemporary
        {
            get
            {
                return m_fIsTemporary;
            }

            set
            {
                m_fIsTemporary = value;
                m_version++;
            }
        }

        public bool ShouldImplementInHardware
        {
            get
            {
                return m_fImplementInHardware;
            }

            set
            {
                m_fImplementInHardware = value;
                m_version++;
            }
        }

        public bool WasHit
        {
            get
            {
                return m_fHit;
            }
        }

        public long HitCount
        {
            get
            {
                return m_hitCount;
            }

            set
            {
                m_hitCount = value;
            }
        }

        public Callback Target
        {
            get
            {
                return m_target;
            }
        }
    }
}
