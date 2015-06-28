//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class ElementOperator : Operator
    {
        //
        // State
        //

        protected FieldRepresentation[] m_accessPath;

        //
        // Constructor Methods
        //

        protected ElementOperator( Debugging.DebugInfo   debugInfo    ,
                                   OperatorCapabilities  capabilities ,
                                   OperatorLevel         level        ,
                                   FieldRepresentation[] accessPath   ) : base( debugInfo, capabilities, level )
        {
            m_accessPath = accessPath;
        }

        //--//

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_accessPath );

            context.Pop();
        }

        //
        // Access Methods
        //

        public FieldRepresentation[] AccessPath
        {
            get
            {
                return m_accessPath;
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
        }
     }
}