//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class BinaryOperatorWithCarryIn : AbstractBinaryOperator
    {
        //
        // Constructor Methods
        //

        private BinaryOperatorWithCarryIn( Debugging.DebugInfo  debugInfo    ,
                                           OperatorCapabilities capabilities ,
                                           OperatorLevel        level        ,
                                           ALU                  alu          ,
                                           bool                 fSigned      ,
                                           bool                 fOverflow    ) : base( debugInfo, capabilities, level, alu, fSigned, fOverflow )
        {
        }

        //--//

        public static BinaryOperatorWithCarryIn New( Debugging.DebugInfo debugInfo ,
                                                     ALU                 alu       ,
                                                     bool                fSigned   ,
                                                     bool                fOverflow ,
                                                     VariableExpression  lhs       ,
                                                     Expression          rhsLeft   ,
                                                     Expression          rhsRight  ,
                                                     VariableExpression  rhsCC     )
        {
            CHECKS.ASSERT( rhsCC.AliasedVariable is ConditionCodeExpression, "rhsCC parameter has to alias a condition code" );

            OperatorCapabilities capabilities = OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                OperatorCapabilities.DoesNotCapturePointerOperands      ;

            switch(alu)
            {
                case ALU.ADD:
                case ALU.MUL:
                case ALU.AND:
                case ALU.OR:
                case ALU.XOR:
                    capabilities |= OperatorCapabilities.IsCommutative;
                    break;

                default:
                    capabilities |= OperatorCapabilities.IsNonCommutative;
                    break;
            }

            if(fOverflow) capabilities |= OperatorCapabilities.MayThrow;
            else          capabilities |= OperatorCapabilities.DoesNotThrow;

            OperatorLevel level = fOverflow ? OperatorLevel.ConcreteTypes : OperatorLevel.Lowest;

            var res = new BinaryOperatorWithCarryIn( debugInfo, capabilities, level, alu, fSigned, fOverflow );

            res.SetLhs( lhs                      );
            res.SetRhs( rhsLeft, rhsRight, rhsCC );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new BinaryOperatorWithCarryIn( m_debugInfo, m_capabilities, m_level, m_alu, m_fSigned, m_fOverflow ) );
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
            sb.Append( "BinaryOperatorWithCarryIn(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = {1} {2} {3} <= {4}", this.FirstResult, this.FirstArgument, m_alu, this.SecondArgument, this.ThirdArgument );
        }
    }
}
