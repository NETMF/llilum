//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class FieldOperator : Operator
    {
        //
        // State
        //

        private FieldRepresentation m_fd;

        //
        // Constructor Methods
        //

        protected FieldOperator( Debugging.DebugInfo  debugInfo    ,
                                 OperatorCapabilities capabilities ,
                                 OperatorLevel        level        ,
                                 FieldRepresentation  field        ) : base( debugInfo, capabilities, level )
        {
            m_fd = field;
        }

        //--//

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_fd );

            context.Pop();
        }

        //
        // Access Methods
        //

        public FieldRepresentation Field
        {
            get
            {
                return m_fd;
            }
        }

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.AppendFormat( "{0} ", m_fd );

            base.InnerToString( sb );
        }
    }
}