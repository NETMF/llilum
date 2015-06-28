//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;

    using IR = Microsoft.Zelig.CodeGeneration.IR;
    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    internal sealed class SlicedValueHandle : AbstractValueHandle
    {
        //
        // State
        //

        private readonly AbstractValueHandle m_value;
        private readonly int                 m_offset;
        private readonly int                 m_count;

        //
        // Contructor Methods
        //

        internal SlicedValueHandle( TS.TypeRepresentation            type                     ,
                                    TS.CustomAttributeRepresentation caMemoryMappedPeripheral ,
                                    TS.CustomAttributeRepresentation caMemoryMappedRegister   ,
                                    bool                             fAsHoldingVariable       ,
                                    AbstractValueHandle              value                    ,
                                    int                              offset                   ,
                                    int                              count                    ) : base( type, caMemoryMappedPeripheral, caMemoryMappedRegister, fAsHoldingVariable )
        {
            m_value  = value;
            m_offset = offset;
            m_count  = count;
        }

        //
        // Helper Methods
        //

        public override bool IsEquivalent( AbstractValueHandle abstractValueHandle )
        {
            var other = abstractValueHandle as SlicedValueHandle;

            if(other != null)
            {
                if(this.m_offset == other.m_offset &&
                   this.m_count  == other.m_count   )
                {
                    return this.m_value.IsEquivalent( other.m_value );
                }
            }

            return false;
        }

        public override Emulation.Hosting.BinaryBlob Read(     int  offset   ,
                                                               int  count    ,
                                                           out bool fChanged )
        {
            var bb        = new Emulation.Hosting.BinaryBlob( count );
            int offsetSub = offset + m_offset;
            int countSub  = count;

            int start = Math.Max( offsetSub           , m_offset           ); 
            int end   = Math.Min( offsetSub + countSub, m_offset + m_count );

            if(start >= end)
            {
                //
                // Non-overlapping regions.
                //
                fChanged = false;

                return null;
            }

            var bbSub = m_value.Read( start, end - start, out fChanged );
            if(bbSub == null)
            {
                return null;
            }

            bb.Insert( bbSub, start - offsetSub );

            return bb;
        }

        public override bool Write( Emulation.Hosting.BinaryBlob bb     ,
                                    int                          offset ,
                                    int                          count  )
        {
            int offsetSub = offset + m_offset;
            int countSub  = count;

            int start = Math.Max( offsetSub           , m_offset           ); 
            int end   = Math.Min( offsetSub + countSub, m_offset + m_count );

            if(start >= end)
            {
                //
                // Non-overlapping regions.
                //
                return true;
            }

            var bbSub = bb.Extract( start - m_offset, end - start );

            return m_value.Write( bbSub, start, end - start );
        }

        //
        // Access Methods
        //

        public override bool CanUpdate
        {
            get
            {
                return m_value.CanUpdate;
            }
        }

        public override bool HasAddress
        {
            get
            {
                return m_value.HasAddress;
            }
        }

        public override uint Address
        {
            get
            {
                return (uint)(m_value.Address + m_offset);
            }
        }
    }
}