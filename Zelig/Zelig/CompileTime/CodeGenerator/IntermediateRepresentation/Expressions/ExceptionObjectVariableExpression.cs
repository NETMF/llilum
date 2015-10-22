//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class ExceptionObjectVariableExpression : VariableExpression
    {
        public static new readonly ExceptionObjectVariableExpression[] SharedEmptyArray = new ExceptionObjectVariableExpression[0];

        //
        // Constructor Methods
        //

        internal ExceptionObjectVariableExpression( TypeRepresentation type      ,
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

            return RegisterAndCloneState( context, context.ControlFlowGraphDestination.AllocateExceptionObjectVariable( td ) );
        }

        //--//

        public override int GetVariableKind()
        {
            return c_VariableKind_Exception;
        }

        //--//

        //
        // Access Methods
        //

        public override CanBeNull CanBeNull
        {
            get
            {
                return CanBeNull.No;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "Exception" );

            base.InnerToString( sb );
        }
    }
}