//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class ArgumentVariableExpression : VariableExpression
    {
        //
        // Constructor Methods
        //

        public ArgumentVariableExpression( TypeRepresentation type      ,
                                           DebugInfo          debugInfo ,
                                           int                number    ) : base( type, debugInfo )
        {
            m_number = number;
        }

        //--//

        //
        // Helper Methods
        //

        public override Expression Clone( CloningContext context )
        {
            //
            // Arguments cannot be allocated after control flow graph creation.
            //
            return RegisterAndCloneState( context, context.ControlFlowGraphDestination.Arguments[ m_number ] );
        }

        //--//

        public override int GetVariableKind()
        {
            return c_VariableKind_Argument;
        }

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "Arg_" );

            base.InnerToString( sb );
        }
    }
}