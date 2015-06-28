//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class StoreIndirectOperator : IndirectOperator
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        private StoreIndirectOperator( Debugging.DebugInfo   debugInfo    ,
                                       OperatorCapabilities  capabilities ,
                                       OperatorLevel         level        ,
                                       TypeRepresentation    td           ,
                                       FieldRepresentation[] accessPath   ,
                                       int                   offset       ) : base( debugInfo, capabilities, level, td, accessPath, offset )
        {
        }

        //--//

        public static StoreIndirectOperator New( Debugging.DebugInfo   debugInfo  ,
                                                 TypeRepresentation    td         ,
                                                 Expression            rhsAddr    ,
                                                 Expression            rhsValue   ,
                                                 FieldRepresentation[] accessPath ,
                                                 int                   offset     ,
                                                 bool                  fNullCheck )
        {
            OperatorLevel        level;
            OperatorCapabilities capabilities = OperatorCapabilities.IsNonCommutative                   |
                                                OperatorCapabilities.MayMutateExistingStorage           | // TODO: method analysis could yield a better answer.
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                OperatorCapabilities.MayWriteThroughPointerOperands     |
                                                OperatorCapabilities.MayCapturePointerOperands          ;

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

            StoreIndirectOperator res = new StoreIndirectOperator( debugInfo, capabilities, level, td, accessPath, offset );

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

            return RegisterAndCloneState( context, new StoreIndirectOperator( m_debugInfo, m_capabilities, m_level, td, accessPath, m_offset ) );
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
            sb.Append( "StoreIndirectOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "[{0},#{2}] <= {1} as {3}{4}", this.FirstArgument, this.SecondArgument, m_offset, m_td, this.MayThrow ? " MayThrow" : "" );
        }
    }
}