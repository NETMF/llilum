//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class IndirectOperator : Operator
    {
        //
        // State
        //

        protected TypeRepresentation    m_td;
        protected int                   m_offset;
        protected FieldRepresentation[] m_accessPath;

        //
        // Constructor Methods
        //

        protected IndirectOperator( Debugging.DebugInfo   debugInfo    ,
                                    OperatorCapabilities  capabilities ,
                                    OperatorLevel         level        ,
                                    TypeRepresentation    td           ,
                                    FieldRepresentation[] accessPath   ,
                                    int                   offset       ) : base( debugInfo, capabilities, level )
        {
            m_td         = td;
            m_accessPath = accessPath;
            m_offset     = offset;
        }

        //--//

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_td         );
            context.Transform( ref m_offset     );
            context.Transform( ref m_accessPath );

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public TypeRepresentation Type
        {
            get
            {
                return m_td;
            }
        }

        public FieldRepresentation[] AccessPath
        {
            get
            {
                return m_accessPath;
            }
        }

        public int Offset
        {
            get
            {
                return m_offset;
            }
        }

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            base.InnerToString( sb );

            if(m_accessPath != null)
            {
                foreach(FieldRepresentation fd in m_accessPath)
                {
                    sb.AppendFormat(",Field={0}", fd );
                }
            }

            if(m_offset != 0)
            {
                sb.AppendFormat(",Offset={0}", m_offset );
            }
        }
    }
}