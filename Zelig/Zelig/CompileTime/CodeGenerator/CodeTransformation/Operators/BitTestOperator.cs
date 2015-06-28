//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class BitTestOperator : Operator
    {
        private const OperatorCapabilities cCapabilities = OperatorCapabilities.IsCommutative                      |
                                                           OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                           OperatorCapabilities.DoesNotAllocateStorage             |
                                                           OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                           OperatorCapabilities.DoesNotThrow                       |
                                                           OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                           OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                           OperatorCapabilities.DoesNotCapturePointerOperands      ;
        //
        // State
        //

        //
        // Constructor Methods
        //

        private BitTestOperator( Debugging.DebugInfo debugInfo ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
        }

        //--//

        public static BitTestOperator New( Debugging.DebugInfo debugInfo ,
                                           VariableExpression  lhs       ,
                                           Expression          rhsLeft   ,
                                           Expression          rhsRight  )
        {
            CHECKS.ASSERT( lhs.AliasedVariable is ConditionCodeExpression, "Expecting a ConditionCodeExpression for the 'lhs' parameter, got '{0}'", lhs );

            BitTestOperator res = new BitTestOperator( debugInfo );

            res.SetLhs( lhs               );
            res.SetRhs( rhsLeft, rhsRight );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new BitTestOperator( m_debugInfo ) );
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
            sb.Append( "BitTestOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = bittest {1} <-> {2}", this.FirstResult, this.FirstArgument, this.SecondArgument );
        }
   }
}