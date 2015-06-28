//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class LoadIndirectOperator : IndirectOperator
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        private LoadIndirectOperator( Debugging.DebugInfo   debugInfo    ,
                                      OperatorCapabilities  capabilities ,
                                      OperatorLevel         level        ,
                                      TypeRepresentation    td           ,
                                      FieldRepresentation[] accessPath   ,
                                      int                   offset       ) : base( debugInfo, capabilities, level, td, accessPath, offset )
        {
        }

        //--//

        public static LoadIndirectOperator New( Debugging.DebugInfo   debugInfo   ,
                                                TypeRepresentation    td          ,
                                                VariableExpression    lhs         ,
                                                Expression            rhs         ,
                                                FieldRepresentation[] accessPath  ,
                                                int                   offset      ,
                                                bool                  fIsVolatile ,
                                                bool                  fNullCheck  )
        {
            OperatorLevel        level;
            OperatorCapabilities capabilities = OperatorCapabilities.IsNonCommutative                   |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.MayReadExistingMutableStorage      | // TODO: method analysis could discover the pointer is outside the stack.
                                                OperatorCapabilities.MayReadThroughPointerOperands      |
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                OperatorCapabilities.MayCapturePointerOperands          ; // TODO: Is it true?

            if(fIsVolatile)
            {
                capabilities |= OperatorCapabilities.MayMutateExistingStorage;
            }
            else
            {
                capabilities |= OperatorCapabilities.DoesNotMutateExistingStorage;
            }

            if(fNullCheck)
            {
                level         = OperatorLevel       .ConcreteTypes;
                capabilities |= OperatorCapabilities.MayThrow;
            }
            else
            {
                level         = OperatorLevel       .Lowest;
                capabilities |= OperatorCapabilities.DoesNotThrow;
            }

            LoadIndirectOperator res = new LoadIndirectOperator( debugInfo, capabilities, level, td, accessPath, offset );

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
            TypeRepresentation    td         = context.ConvertType  ( m_td         );
            FieldRepresentation[] accessPath = context.ConvertFields( m_accessPath );

            return RegisterAndCloneState( context, new LoadIndirectOperator( m_debugInfo, m_capabilities, m_level, td, accessPath, m_offset ) );
        }

        //--//

        //
        // Access Methods
        //

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "LoadIndirectOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} <= [{1},#{2}] as {3}{4}", this.FirstResult, this.FirstArgument, m_offset, m_td, this.MayThrow ? " MayThrow" : "" );
        }
    }
}