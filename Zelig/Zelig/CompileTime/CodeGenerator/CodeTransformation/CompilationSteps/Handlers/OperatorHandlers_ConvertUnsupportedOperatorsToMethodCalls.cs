//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Handlers
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class OperatorHandlers_ConvertUnsupportedOperatorsToMethodCalls
    {
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ConvertUnsupportedOperatorsToMethodCalls) )]
        [CompilationSteps.OperatorHandler( typeof(BinaryOperator) )]
        private static void Handle_BinaryOperator( PhaseExecution.NotificationContext nc )
        {
            BinaryOperator op = (BinaryOperator)nc.CurrentOperator;

            ControlFlowGraphStateForCodeTransformation cfg      = nc.CurrentCFG;
            TypeSystemForCodeTransformation            ts       = nc.TypeSystem;
            WellKnownMethods                           wkm      = ts.WellKnownMethods;
            VariableExpression                         exRes    = op.FirstResult;
            Expression                                 exSrc1   = op.FirstArgument;
            Expression                                 exSrc2   = op.SecondArgument;
            TypeRepresentation                         tdRes    = exRes .Type;
            TypeRepresentation                         tdSrc1   = exSrc1.Type;
            TypeRepresentation                         tdSrc2   = exSrc2.Type;
            uint                                       sizeRes  = tdRes .SizeOfHoldingVariableInWords;
            uint                                       sizeSrc1 = tdSrc1.SizeOfHoldingVariableInWords;
            uint                                       sizeSrc2 = tdSrc2.SizeOfHoldingVariableInWords;
            bool                                       fSigned  = op.Signed;

            if(sizeSrc1 != sizeSrc2)
            {
                switch(op.Alu)
                {
                    case BinaryOperator.ALU.SHL:
                    case BinaryOperator.ALU.SHR:
                        CHECKS.ASSERT( sizeSrc2 == 1, "Incompatible input for {0}", op );
                        break;

                    default:
                        throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                }
            }

            if(sizeSrc1 != sizeRes)
            {
                if(op.Alu != BinaryOperator.ALU.MUL)
                {
                    throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                }
            }
            if((tdSrc1.IsNumeric == false || tdSrc1.IsInteger) &&
               (tdSrc2.IsNumeric == false || tdSrc2.IsInteger)  )
            {
                if(sizeSrc1 == 1)
                {
                    switch(op.Alu)
                    {
                        case BinaryOperator.ALU.DIV:
                        case BinaryOperator.ALU.REM:
                            {
                                MethodRepresentation md;

                                if(fSigned)
                                {
                                    switch(op.Alu)
                                    {
                                        case BinaryOperator.ALU.DIV: md = wkm.Helpers_BinaryOperations_IntDiv; break;
                                        case BinaryOperator.ALU.REM: md = wkm.Helpers_BinaryOperations_IntRem; break;

                                        default:
                                            throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                                    }
                                }
                                else
                                {
                                    switch(op.Alu)
                                    {
                                        case BinaryOperator.ALU.DIV: md = wkm.Helpers_BinaryOperations_UintDiv; break;
                                        case BinaryOperator.ALU.REM: md = wkm.Helpers_BinaryOperations_UintRem; break;

                                        default:
                                            throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                                    }
                                }

                                ts.SubstituteWithCallToHelper( md, op );

                                nc.MarkAsModified();
                            }
                            break;
                    }
                }
                else if(sizeSrc1 == 2)
                {
                    switch(op.Alu)
                    {
                        case BinaryOperator.ALU.MUL:
                        case BinaryOperator.ALU.DIV:
                        case BinaryOperator.ALU.REM:
                            {
                                MethodRepresentation md;

                                if(fSigned)
                                {
                                    switch(op.Alu)
                                    {
                                        case BinaryOperator.ALU.MUL: md = wkm.Helpers_BinaryOperations_LongMul; break;
                                        case BinaryOperator.ALU.DIV: md = wkm.Helpers_BinaryOperations_LongDiv; break;
                                        case BinaryOperator.ALU.REM: md = wkm.Helpers_BinaryOperations_LongRem; break;

                                        default:
                                            throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                                    }
                                }
                                else
                                {
                                    switch(op.Alu)
                                    {
                                        case BinaryOperator.ALU.MUL: md = wkm.Helpers_BinaryOperations_UlongMul; break;
                                        case BinaryOperator.ALU.DIV: md = wkm.Helpers_BinaryOperations_UlongDiv; break;
                                        case BinaryOperator.ALU.REM: md = wkm.Helpers_BinaryOperations_UlongRem; break;

                                        default:
                                            throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                                    }
                                }

                                ts.SubstituteWithCallToHelper( md, op );

                                nc.MarkAsModified();
                            }
                            break;

                        case BinaryOperator.ALU.SHL:
                        case BinaryOperator.ALU.SHR:
                            {
                                ConstantExpression exShift = exSrc2 as ConstantExpression;

                                //
                                // Conversion from 64 to 32 bits? Or from 32 to 64 bits?
                                //
                                if(exShift != null && exShift.Value is int && (int)exShift.Value == 32)
                                {
                                    //
                                    // This will be expanded later.
                                    //
                                }
                                else
                                {
                                    MethodRepresentation md;

                                    if(fSigned)
                                    {
                                        switch(op.Alu)
                                        {
                                            case BinaryOperator.ALU.SHL: md = wkm.Helpers_BinaryOperations_LongShl; break;
                                            case BinaryOperator.ALU.SHR: md = wkm.Helpers_BinaryOperations_LongShr; break;

                                            default:
                                                throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                                        }
                                    }
                                    else
                                    {
                                        switch(op.Alu)
                                        {
                                            case BinaryOperator.ALU.SHL: md = wkm.Helpers_BinaryOperations_UlongShl; break;
                                            case BinaryOperator.ALU.SHR: md = wkm.Helpers_BinaryOperations_UlongShr; break;

                                            default:
                                                throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                                        }
                                    }

                                    ts.SubstituteWithCallToHelper( md, op );

                                    nc.MarkAsModified();
                                }
                            }
                            break;
                    }
                }
            }
        }
    }
}
