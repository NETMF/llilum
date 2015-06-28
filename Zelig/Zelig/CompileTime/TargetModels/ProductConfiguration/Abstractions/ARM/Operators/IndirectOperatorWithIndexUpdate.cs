//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.ARM
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.CodeGeneration.IR;

    public abstract class IndirectOperatorWithIndexUpdate : IndirectOperator
    {
        //
        // State
        //

        protected bool m_fPostUpdate;

        //
        // Constructor Methods
        //

        protected IndirectOperatorWithIndexUpdate( Debugging.DebugInfo   debugInfo    ,
                                                   OperatorCapabilities  capabilities ,
                                                   TypeRepresentation    td           ,
                                                   FieldRepresentation[] accessPath   ,
                                                   int                   offset       ,
                                                   bool                  fPostUpdate  ) : base( debugInfo, capabilities, OperatorLevel.Lowest, td, accessPath, offset )
        {
            m_fPostUpdate = fPostUpdate;
        }

        //--//

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_fPostUpdate );

            context.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public bool PostUpdate
        {
            get
            {
                return m_fPostUpdate;
            }
        }

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            base.InnerToString( sb );

            sb.Append( m_fPostUpdate ? ",PostUpdate" : ",PreUpdate" );
        }
    }
}