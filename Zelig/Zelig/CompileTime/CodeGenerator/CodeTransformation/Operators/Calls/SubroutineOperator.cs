//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class SubroutineOperator : Operator
    {
        //
        // State
        //

        protected MethodRepresentation m_md;

        //
        // Constructor Methods
        //

        protected SubroutineOperator( Debugging.DebugInfo  debugInfo    ,
                                      OperatorCapabilities capabilities ,
                                      MethodRepresentation md           ) : base( debugInfo, capabilities, OperatorLevel.Lowest )
        {
            m_md = md;
        }

        //--//

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_md );

            context.Pop();
        }

        //--//

        public override bool CanPropagateCopy( Expression exOld ,
                                               Expression exNew )
        {
            //
            // The Rvalues are used to keep alive actual method arguments, so we cannot change them!!
            //
            return false;
        }

        //--//

        //
        // Access Methods
        //

        public MethodRepresentation TargetMethod
        {
            get
            {
                return m_md;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            base.InnerToString( sb );

            sb.AppendFormat( " {0}", m_md );
        }
    }
}