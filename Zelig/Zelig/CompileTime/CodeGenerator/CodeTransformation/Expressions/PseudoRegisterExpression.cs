//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class PseudoRegisterExpression : LowLevelVariableExpression
    {
        //
        // Constructor Methods
        //

        internal PseudoRegisterExpression( TypeRepresentation type         ,
                                           DebugInfo          debugInfo    ,
                                           VariableExpression sourceVar    ,
                                           uint               sourceOffset ) : base( type, debugInfo, sourceVar, sourceOffset )
        {
        }

        //--//

        //
        // Helper Methods
        //

        public override Expression Clone( CloningContext context )
        {
            ControlFlowGraphStateForCodeTransformation cfg       = (ControlFlowGraphStateForCodeTransformation)context.ControlFlowGraphDestination;
            TypeRepresentation                         td        =                     context.ConvertType( m_type );
            VariableExpression                         sourceVar = (VariableExpression)context.Clone( m_sourceVar );

            return RegisterAndCloneState( context, cfg.AllocatePseudoRegister( td, m_debugInfo, sourceVar, m_sourceOffset ) );
        }

        //--//

        public override Operator.OperatorLevel GetLevel( Operator.IOperatorLevelHelper helper )
        {
            return Operator.OperatorLevel.ScalarValues; // Could go to either register or stack.
        }

        public override int GetVariableKind()
        {
            return c_VariableKind_Pseudo;
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

        public override bool CanTakeAddress
        {
            get
            {
                return false;
            }
        }
        
        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "$PseudoReg_" );

            base.InnerToString( sb );

            AppendOffsetInfo( sb );
        }
    }
}