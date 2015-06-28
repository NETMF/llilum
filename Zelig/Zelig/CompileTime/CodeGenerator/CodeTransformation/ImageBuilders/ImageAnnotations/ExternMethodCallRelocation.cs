//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ImageBuilders
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public sealed class ExternMethodCallRelocation : ImageAnnotation
    {
        public delegate uint UpdateExternalMethodCall( uint opcode, uint callerAddress, uint calleeAddress );

        //
        // State
        //
        private UpdateExternalMethodCall m_callback;
        private uint                     m_methodOffset;
        private int                      m_skew;

        //
        // Constructor Methods
        //

        private ExternMethodCallRelocation() // Default constructor required by TypeSystemSerializer.
        {
        }

        public ExternMethodCallRelocation( SequentialRegion region,
                               uint offset,
                               UpdateExternalMethodCall callback,
                               uint methodOffset,
                               object target,
                               int skew)
            : base( region, offset, sizeof( uint ), target )
        {
            m_callback     = callback;
            m_methodOffset = methodOffset;
            m_skew         = skew;
        }

        //
        // Helper Methods
        //

        //--//

        public override bool IsCompileTimeAnnotation
        {
            get { return true; }
        }

        public override void GetAllowedRelocationRange( out int lowerBound,
                                                        out int upperBound )
        {
            lowerBound = m_skew + ( int.MinValue >> 8 ) * sizeof( uint );
            upperBound = m_skew + ( int.MaxValue >> 8 ) * sizeof( uint );
        }

        public override bool ApplyRelocation()
        {
            uint methodAddr = m_region.Owner.Resolve( m_target ) + m_methodOffset;
            uint srcAddress = m_region.GetAbsoluteAddress( m_offset );

            if(m_target is ExternalCallOperator)
            {
                methodAddr += ( (ExternalCallOperator)m_target ).Context.OperatorOffset;
            }

            if(this.CanRelocateToAddress( srcAddress, methodAddr ))
            {
                m_region.Write( m_offset, m_callback( m_region.ReadUInt( m_offset ), srcAddress, methodAddr) );

                return true;
            }

            return false;
        }
    }
}
