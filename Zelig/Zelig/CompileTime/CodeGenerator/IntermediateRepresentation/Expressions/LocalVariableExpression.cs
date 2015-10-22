//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class LocalVariableExpression : VariableExpression
    {
        public static new readonly LocalVariableExpression[] SharedEmptyArray = new LocalVariableExpression[0];

        //
        // Constructor Methods
        //

        internal LocalVariableExpression( TypeRepresentation type      ,
                                          DebugInfo          debugInfo ) : base( type, debugInfo )
        {
        }

        //--//

        //
        // Helper Methods
        //

        public override Expression Clone( CloningContext context )
        {
            TypeRepresentation td = context.ConvertType( m_type );

            return RegisterAndCloneState( context, context.ControlFlowGraphDestination.AllocateLocal( td, m_debugInfo ) );
        }

        //--//

        public override int GetVariableKind()
        {
            return c_VariableKind_Local;
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "Local" );

            base.InnerToString( sb );
        }
    }
}