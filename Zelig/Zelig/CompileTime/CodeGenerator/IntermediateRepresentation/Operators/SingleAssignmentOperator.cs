//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class SingleAssignmentOperator : AbstractAssignmentOperator
    {
        //
        // Constructor Methods
        //

        private SingleAssignmentOperator( Debugging.DebugInfo debugInfo ) : base( debugInfo )
        {
        }

        //--//

        public static SingleAssignmentOperator New( Debugging.DebugInfo debugInfo ,
                                                    VariableExpression  lhs       ,
                                                    Expression          rhs       )
        {
            var res = new SingleAssignmentOperator( debugInfo );

            res.SetLhs( lhs );
            res.SetRhs( rhs );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new SingleAssignmentOperator( m_debugInfo ) );
        }

        //--//

        //
        // Access Methods
        //

        public override bool PerformsNoActions
        {
            get
            {
                return this.FirstResult == this.FirstArgument; // We need a strong identity, because of use/definition chains.
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "SingleAssignmentOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = {1}", this.FirstResult, this.FirstArgument );
        }
   }
}
