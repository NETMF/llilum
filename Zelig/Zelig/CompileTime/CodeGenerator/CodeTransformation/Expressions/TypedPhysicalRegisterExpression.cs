//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class TypedPhysicalRegisterExpression : PhysicalRegisterExpression
    {
        //
        // Constructor Methods
        //

        internal TypedPhysicalRegisterExpression( TypeRepresentation              type         ,
                                                  Abstractions.RegisterDescriptor regDesc      ,
                                                  VariableExpression.DebugInfo    debugInfo    ,
                                                  VariableExpression              sourceVar    ,
                                                  uint                            sourceOffset ) : base( type, regDesc, debugInfo, sourceVar, sourceOffset )
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

            return RegisterAndCloneState( context, cfg.AllocateTypedPhysicalRegister( td, m_regDesc, m_debugInfo, sourceVar, m_sourceOffset ) );
        }

        //--//

        public override int GetVariableKind()
        {
            return c_VariableKind_Physical;
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
    }
}