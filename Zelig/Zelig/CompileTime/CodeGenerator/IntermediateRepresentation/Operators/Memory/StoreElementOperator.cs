//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class StoreElementOperator : ElementOperator
    {
        //
        // Constructor Methods
        //

        private StoreElementOperator( Debugging.DebugInfo   debugInfo    ,
                                      OperatorCapabilities  capabilities ,
                                      OperatorLevel         level        ,
                                      FieldRepresentation[] accessPath   ) : base( debugInfo, capabilities, level, accessPath )
        {
        }

        //--//

        public static StoreElementOperator New( Debugging.DebugInfo   debugInfo  ,
                                                Expression            rhsArray   ,
                                                Expression            rhsIndex   ,
                                                Expression            rhsVal     ,
                                                FieldRepresentation[] accessPath ,
                                                bool                  fNullCheck )
        {
            OperatorLevel        level;
            OperatorCapabilities capabilities = OperatorCapabilities.IsNonCommutative                   |
                                                OperatorCapabilities.MayMutateExistingStorage           | // TODO: method analysis could yield a better answer.
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                OperatorCapabilities.DoesNotCapturePointerOperands      ;

            if(fNullCheck)
            {
                level         = OperatorLevel       .ConcreteTypes;
                capabilities |= OperatorCapabilities.MayThrow; // Out-of-bounds and null pointer checks can throw.
            }
            else
            {
                level         = OperatorLevel       .Lowest;
                capabilities |= OperatorCapabilities.DoesNotThrow;
            }

            StoreElementOperator res = new StoreElementOperator( debugInfo, capabilities, level, accessPath );

            res.SetRhs( rhsArray, rhsIndex, rhsVal );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            FieldRepresentation[] accessPath = context.ConvertFields( m_accessPath );

            return RegisterAndCloneState( context, new StoreElementOperator( m_debugInfo, m_capabilities, m_level, accessPath ) );
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
            sb.Append( "StoreElementOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0}[{1}] <= {2}{3}", this.FirstArgument, this.SecondArgument, this.ThirdArgument, this.MayThrow ? " MayThrow" : "" );
        }
    }
}