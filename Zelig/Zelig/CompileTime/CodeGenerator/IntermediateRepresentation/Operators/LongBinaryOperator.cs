//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class LongBinaryOperator : AbstractBinaryOperator
    {
        //
        // Constructor Methods
        //

        private LongBinaryOperator( Debugging.DebugInfo  debugInfo    ,
                                    OperatorCapabilities capabilities ,
                                    OperatorLevel        level        ,
                                    ALU                  alu          ,
                                    bool                 fSigned      ,
                                    bool                 fOverflow    ) : base( debugInfo, capabilities, level, alu, fSigned, fOverflow )
        {
        }

        //--//

        public static LongBinaryOperator New( Debugging.DebugInfo debugInfo ,
                                              ALU                 alu       ,
                                              bool                fSigned   ,
                                              bool                fOverflow ,
                                              VariableExpression  lhsLo     ,
                                              VariableExpression  lhsHi     ,
                                              Expression          rhsLeft   ,
                                              Expression          rhsRight  )
        {
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

            var res = new LongBinaryOperator( debugInfo, capabilities, level, alu, fSigned, fOverflow );

            res.SetLhs( lhsLo  , lhsHi    );
            res.SetRhs( rhsLeft, rhsRight );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new LongBinaryOperator( m_debugInfo, m_capabilities, m_level, m_alu, m_fSigned, m_fOverflow ) );
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
            sb.Append( "LongBinaryOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0}, {1} = {2} {3} {4}{5}", this.FirstResult, this.SecondResult, this.FirstArgument, m_alu, this.SecondArgument, this.MayThrow ? "  MayThrow" : "" );
        }
    }
}