//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class LowLevelVariableExpression : VariableExpression
    {
        protected const int c_VariableKind_Pseudo        = 4;
        protected const int c_VariableKind_Physical      = 5;
        protected const int c_VariableKind_Condition     = 6;
        protected const int c_VariableKind_StackLocation = 7;  // 7-9, depending on the placement.

        //
        // State
        //

        protected VariableExpression m_sourceVar;
        protected uint               m_sourceOffset;

        //
        // Constructor Methods
        //

        protected LowLevelVariableExpression( TypeRepresentation type         ,
                                              DebugInfo          debugInfo    ,
                                              VariableExpression sourceVar    ,
                                              uint               sourceOffset ) : base( type, debugInfo )
        {
            m_sourceVar    = sourceVar;
            m_sourceOffset = sourceOffset;
        }

        //--//

        //
        // Helper Methods
        //

        protected override void CloneState( CloningContext context ,
                                            Expression     clone   )
        {
            LowLevelVariableExpression clone2 = (LowLevelVariableExpression)clone;

            clone2.m_sourceVar = (VariableExpression)context.Clone( m_sourceVar );

            base.CloneState( context, clone );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_sourceVar    );
            context.Transform( ref m_sourceOffset );

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public VariableExpression SourceVariable
        {
            get
            {
                return m_sourceVar;
            }
        }

        public uint SourceOffset
        {
            get
            {
                return m_sourceOffset;
            }
        }

        public VariableExpression AggregateVariable
        {
            get
            {
                return m_sourceVar != null ? m_sourceVar : this;
            }
        }

        //--//

        //
        // Debug Methods
        //

        protected void AppendOffsetInfo( System.Text.StringBuilder sb )
        {
            if(m_sourceVar != null)
            {
                sb.Append( "<<" );
                
                m_sourceVar.InnerToString( sb );

                if(m_sourceOffset != 0 || m_sourceVar.Type.Size != this.Type.Size)
                {
                    sb.AppendFormat( " at offset {0}", m_sourceOffset );
                }

                sb.Append( ">>" );
            }
        }
    }
}