//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class UnaryOperatorWithCarryOut : AbstractUnaryOperator
    {
        //
        // Constructor Methods
        //

        private UnaryOperatorWithCarryOut( Debugging.DebugInfo  debugInfo    ,
                                        OperatorCapabilities capabilities ,
                                        OperatorLevel        level        ,
                                        ALU                  alu          ,
                                        bool                 fSigned      ,
                                        bool                 fOverflow    ) : base( debugInfo, capabilities, level, alu, fSigned, fOverflow )
        {
        }

        //--//

        public static UnaryOperatorWithCarryOut New( Debugging.DebugInfo debugInfo ,
                                                     ALU                 alu       ,
                                                     bool                fSigned   ,
                                                     bool                fOverflow ,
                                                     VariableExpression  lhs       ,
                                                     VariableExpression  cc        ,
                                                     Expression          rhs       )
        {
            CHECKS.ASSERT( cc.AliasedVariable is ConditionCodeExpression, "Invalid type for condition code: {0}", cc );

            OperatorCapabilities capabilities = OperatorCapabilities.IsNonCommutative                   |
                                                OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                OperatorCapabilities.DoesNotCapturePointerOperands      ;

            if(fOverflow) capabilities |= OperatorCapabilities.MayThrow;
            else          capabilities |= OperatorCapabilities.DoesNotThrow;

            OperatorLevel level = fOverflow ? OperatorLevel.ConcreteTypes : OperatorLevel.Lowest;

            var res = new UnaryOperatorWithCarryOut( debugInfo, capabilities, level, alu, fSigned, fOverflow );

            res.SetLhs( lhs, cc );
            res.SetRhs( rhs     );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new UnaryOperatorWithCarryOut( m_debugInfo, m_capabilities, m_level, m_alu, m_fSigned, m_fOverflow ) );
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
            sb.Append( "UnaryOperatorWithCarry(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0}, {1} = {2} {3}{4}", this.FirstResult, this.SecondResult, m_alu, this.FirstArgument, this.MayThrow ? "  MayThrow" : "" );
        }
    }
}