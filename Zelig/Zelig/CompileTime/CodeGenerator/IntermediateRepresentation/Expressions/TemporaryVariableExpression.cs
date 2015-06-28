//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class TemporaryVariableExpression : VariableExpression
    {
        public static new readonly TemporaryVariableExpression[] SharedEmptyArray = new TemporaryVariableExpression[0];

        //
        // Constructor Methods
        //

        internal TemporaryVariableExpression( TypeRepresentation type      ,
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

            return RegisterAndCloneState( context, context.ControlFlowGraphDestination.AllocateTemporary( td, m_debugInfo ) );
        }

        //--//

        public override int GetVariableKind()
        {
            return c_VariableKind_Temporary;
        }

        //--//

        //
        // Access Methods
        //

        public override CanBeNull CanBeNull
        {
            get
            {
                return CanBeNull.Unknown;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "$Temp_" );

            base.InnerToString( sb );
        }
    }
}