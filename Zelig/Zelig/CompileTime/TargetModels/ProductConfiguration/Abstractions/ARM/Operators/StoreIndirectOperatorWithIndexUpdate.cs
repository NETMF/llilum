//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.ARM
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.CodeGeneration.IR;

    public sealed class StoreIndirectOperatorWithIndexUpdate : IndirectOperatorWithIndexUpdate
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        private StoreIndirectOperatorWithIndexUpdate( Debugging.DebugInfo   debugInfo    ,
                                                      OperatorCapabilities  capabilities ,
                                                      TypeRepresentation    td           ,
                                                      FieldRepresentation[] accessPath   ,
                                                      int                   offset       ,
                                                      bool                  fPostUpdate  ) : base( debugInfo, capabilities, td, accessPath, offset, fPostUpdate )
        {
        }

        //--//

        public static StoreIndirectOperatorWithIndexUpdate New( Debugging.DebugInfo   debugInfo   ,
                                                                TypeRepresentation    td          ,
                                                                VariableExpression    lhsAddr     ,
                                                                Expression            rhsAddr     ,
                                                                Expression            rhsValue    ,
                                                                FieldRepresentation[] accessPath  ,
                                                                int                   offset      ,
                                                                bool                  fIsVolatile ,
                                                                bool                  fPostUpdate )
        {
            OperatorCapabilities capabilities = OperatorCapabilities.IsNonCommutative                   |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.MayReadExistingMutableStorage      | // TODO: method analysis could discover the pointer is outside the stack.
                                                OperatorCapabilities.MayReadThroughPointerOperands      |
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                OperatorCapabilities.MayCapturePointerOperands          | // TODO: Is it true?
                                                OperatorCapabilities.DoesNotThrow                       ;

            if(fIsVolatile)
            {
                capabilities |= OperatorCapabilities.MayMutateExistingStorage;
            }
            else
            {
                capabilities |= OperatorCapabilities.DoesNotMutateExistingStorage;
            }

            var res = new StoreIndirectOperatorWithIndexUpdate( debugInfo, capabilities, td, accessPath, offset, fPostUpdate );

            res.SetLhs( lhsAddr           );
            res.SetRhs( rhsAddr, rhsValue );

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

            return RegisterAndCloneState( context, new StoreIndirectOperatorWithIndexUpdate( m_debugInfo, m_capabilities, td, accessPath, m_offset, m_fPostUpdate ) );
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
            sb.Append( "StoreIndirectOperatorWithUpdate(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            string fmt;

            if(m_fPostUpdate)
            {
                fmt = "{0} <= [{1}],#{3} <= {2} as {4}";
            }
            else
            {
                fmt = "{0} <= [{1},#{3}] <= {2} as {4}";
            }

            return dumper.FormatOutput( fmt, this.FirstResult, this.FirstArgument, this.SecondArgument, m_offset, m_td );
        }
    }
}