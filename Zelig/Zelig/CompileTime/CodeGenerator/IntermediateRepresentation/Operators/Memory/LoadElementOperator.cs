//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class LoadElementOperator : ElementOperator
    {
        //
        // Constructor Methods
        //

        private LoadElementOperator( Debugging.DebugInfo   debugInfo    ,
                                     OperatorCapabilities  capabilities ,
                                     OperatorLevel         level        ,
                                     FieldRepresentation[] accessPath   ) : base( debugInfo, capabilities, level, accessPath )
        {
        }

        //--//

        public static LoadElementOperator New( Debugging.DebugInfo   debugInfo  ,
                                               VariableExpression    lhs        ,
                                               Expression            rhsArray   ,
                                               Expression            rhsIndex   ,
                                               FieldRepresentation[] accessPath ,
                                               bool                  fNullCheck )
        {
            OperatorLevel        level;
            OperatorCapabilities capabilities = OperatorCapabilities.IsNonCommutative                   |
                                                OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.MayReadExistingMutableStorage      |
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

            LoadElementOperator res = new LoadElementOperator( debugInfo, capabilities, level, accessPath );

            res.SetLhs( lhs                );
            res.SetRhs( rhsArray, rhsIndex );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            FieldRepresentation[] accessPath = context.ConvertFields( m_accessPath );

            return RegisterAndCloneState( context, new LoadElementOperator( m_debugInfo, m_capabilities, m_level, accessPath ) );
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
            sb.Append( "LoadElementOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} <= {1}[{2}]{3}", this.FirstResult, this.FirstArgument, this.SecondArgument, this.MayThrow ? " MayThrow" : "" );
        }
    }
}