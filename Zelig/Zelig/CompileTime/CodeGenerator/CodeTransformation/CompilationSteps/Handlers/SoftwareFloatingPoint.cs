//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Handlers
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class SoftwareFloatingPoint
    {
        const string c_CompareAndSet_FloatEqual           = "SoftFP_CompareAndSet_FloatEqual";
        const string c_CompareAndSet_FloatGreaterOrEqual  = "SoftFP_CompareAndSet_FloatGreaterOrEqual";
        const string c_CompareAndSet_FloatGreater         = "SoftFP_CompareAndSet_FloatGreater";
        const string c_CompareAndSet_FloatLessOrEqual     = "SoftFP_CompareAndSet_FloatLessOrEqual";
        const string c_CompareAndSet_FloatLess            = "SoftFP_CompareAndSet_FloatLess";
        const string c_CompareAndSet_FloatNotEqual        = "SoftFP_CompareAndSet_FloatNotEqual";

        const string c_CompareAndSet_DoubleEqual          = "SoftFP_CompareAndSet_DoubleEqual";
        const string c_CompareAndSet_DoubleGreaterOrEqual = "SoftFP_CompareAndSet_DoubleGreaterOrEqual";
        const string c_CompareAndSet_DoubleGreater        = "SoftFP_CompareAndSet_DoubleGreater";
        const string c_CompareAndSet_DoubleLessOrEqual    = "SoftFP_CompareAndSet_DoubleLessOrEqual";
        const string c_CompareAndSet_DoubleLess           = "SoftFP_CompareAndSet_DoubleLess";
        const string c_CompareAndSet_DoubleNotEqual       = "SoftFP_CompareAndSet_DoubleNotEqual";

        //--//

        const string c_BinaryOperations_FloatAdd          = "SoftFP_BinaryOperations_FloatAdd";
        const string c_BinaryOperations_FloatSub          = "SoftFP_BinaryOperations_FloatSub";
        const string c_BinaryOperations_FloatMul          = "SoftFP_BinaryOperations_FloatMul";
        const string c_BinaryOperations_FloatDiv          = "SoftFP_BinaryOperations_FloatDiv";
        const string c_BinaryOperations_FloatRem          = "SoftFP_BinaryOperations_FloatRem";

        const string c_BinaryOperations_DoubleAdd         = "SoftFP_BinaryOperations_DoubleAdd";
        const string c_BinaryOperations_DoubleSub         = "SoftFP_BinaryOperations_DoubleSub";
        const string c_BinaryOperations_DoubleMul         = "SoftFP_BinaryOperations_DoubleMul";
        const string c_BinaryOperations_DoubleDiv         = "SoftFP_BinaryOperations_DoubleDiv";
        const string c_BinaryOperations_DoubleRem         = "SoftFP_BinaryOperations_DoubleRem";

        //--//

        const string c_UnaryOperations_FloatNeg           = "SoftFP_UnaryOperations_FloatNeg";
        const string c_UnaryOperations_FloatFinite        = "SoftFP_UnaryOperations_FloatFinite";

        const string c_UnaryOperations_DoubleNeg          = "SoftFP_UnaryOperations_DoubleNeg";
        const string c_UnaryOperations_DoubleFinite       = "SoftFP_UnaryOperations_DoubleFinite";

        //--//

        const string c_Convert_IntToFloat                 = "SoftFP_Convert_IntToFloat";
        const string c_Convert_LongToFloat                = "SoftFP_Convert_LongToFloat";
        const string c_Convert_UnsignedIntToFloat         = "SoftFP_Convert_UnsignedIntToFloat";
        const string c_Convert_UnsignedLongToFloat        = "SoftFP_Convert_UnsignedLongToFloat";
        const string c_Convert_DoubleToFloat              = "SoftFP_Convert_DoubleToFloat";
        const string c_Convert_IntToDouble                = "SoftFP_Convert_IntToDouble";
        const string c_Convert_LongToDouble               = "SoftFP_Convert_LongToDouble";
        const string c_Convert_UnsignedIntToDouble        = "SoftFP_Convert_UnsignedIntToDouble";
        const string c_Convert_UnsignedLongToDouble       = "SoftFP_Convert_UnsignedLongToDouble";
        const string c_Convert_FloatToDouble              = "SoftFP_Convert_FloatToDouble";
        const string c_Convert_FloatToInt                 = "SoftFP_Convert_FloatToInt";
        const string c_Convert_FloatToUnsignedInt         = "SoftFP_Convert_FloatToUnsignedInt";
        const string c_Convert_DoubleToInt                = "SoftFP_Convert_DoubleToInt";
        const string c_Convert_DoubleToUnsignedInt        = "SoftFP_Convert_DoubleToUnsignedInt";
        const string c_Convert_FloatToLong                = "SoftFP_Convert_FloatToLong";
        const string c_Convert_FloatToUnsignedLong        = "SoftFP_Convert_FloatToUnsignedLong";
        const string c_Convert_DoubleToLong               = "SoftFP_Convert_DoubleToLong";
        const string c_Convert_DoubleToUnsignedLong       = "SoftFP_Convert_DoubleToUnsignedLong";

        //--//

        [CompilationSteps.CallClosureHandler( typeof(CompareConditionalControlOperator) )]
        private static void Protect_CompareConditionalControlOperator( ComputeCallsClosure.Context host   ,
                                                                       Operator                    target )
        {
            CompareConditionalControlOperator op = (CompareConditionalControlOperator)target;

            Protect_CommonFloatingPointCompare( host, op, op.Condition );
        }

        [CompilationSteps.CallClosureHandler( typeof(CompareAndSetOperator) )]
        private static void Protect_CompareAndSetOperators( ComputeCallsClosure.Context host   ,
                                                            Operator                    target )
        {
            CompareAndSetOperator op = (CompareAndSetOperator)target;

            Protect_CommonFloatingPointCompare( host, op, op.Condition );
        }

        private static void Protect_CommonFloatingPointCompare( ComputeCallsClosure.Context           host      ,
                                                                Operator                              target    ,
                                                                CompareAndSetOperator.ActionCondition condition )
        {
            TypeRepresentation              td   = target.FirstArgument.Type;
            TypeSystemForCodeTransformation ts   = host.TypeSystem;
            WellKnownTypes                  wkt  = ts.WellKnownTypes;
            string                          name = null;

            if(td == wkt.System_Single)
            {
                switch(condition)
                {
                    case CompareAndSetOperator.ActionCondition.EQ: name = c_CompareAndSet_FloatEqual         ; break;
                    case CompareAndSetOperator.ActionCondition.GE: name = c_CompareAndSet_FloatGreaterOrEqual; break;
                    case CompareAndSetOperator.ActionCondition.GT: name = c_CompareAndSet_FloatGreater       ; break;
                    case CompareAndSetOperator.ActionCondition.LE: name = c_CompareAndSet_FloatLessOrEqual   ; break;
                    case CompareAndSetOperator.ActionCondition.LT: name = c_CompareAndSet_FloatLess          ; break;
                    case CompareAndSetOperator.ActionCondition.NE: name = c_CompareAndSet_FloatNotEqual      ; break;
                }
            }
            else if(td == wkt.System_Double)
            {
                switch(condition)
                {
                    case CompareAndSetOperator.ActionCondition.EQ: name = c_CompareAndSet_DoubleEqual         ; break;
                    case CompareAndSetOperator.ActionCondition.GE: name = c_CompareAndSet_DoubleGreaterOrEqual; break;
                    case CompareAndSetOperator.ActionCondition.GT: name = c_CompareAndSet_DoubleGreater       ; break;
                    case CompareAndSetOperator.ActionCondition.LE: name = c_CompareAndSet_DoubleLessOrEqual   ; break;
                    case CompareAndSetOperator.ActionCondition.LT: name = c_CompareAndSet_DoubleLess          ; break;
                    case CompareAndSetOperator.ActionCondition.NE: name = c_CompareAndSet_DoubleNotEqual      ; break;
                }
            }

            if(name != null)
            {
                host.CoverObject( ts.GetWellKnownMethod( name ) );
            }
        }

        [CompilationSteps.CallClosureHandler( typeof(BinaryOperator) )]
        private static void Protect_BinaryOperator( ComputeCallsClosure.Context host   ,
                                                    Operator                    target )
        {
            BinaryOperator                  op   = (BinaryOperator)target;
            TypeRepresentation              td   =                 op.FirstArgument.Type;
            TypeSystemForCodeTransformation ts   =                 host.TypeSystem;
            WellKnownTypes                  wkt  =                 ts.WellKnownTypes;
            string                          name =                 null;

            if(td == wkt.System_Single)
            {
                switch(op.Alu)
                {
                    case BinaryOperator.ALU.ADD: name = c_BinaryOperations_FloatAdd; break;
                    case BinaryOperator.ALU.SUB: name = c_BinaryOperations_FloatSub; break;
                    case BinaryOperator.ALU.MUL: name = c_BinaryOperations_FloatMul; break;
                    case BinaryOperator.ALU.DIV: name = c_BinaryOperations_FloatDiv; break;
                    case BinaryOperator.ALU.REM: name = c_BinaryOperations_FloatRem; break;
                }
            }
            else if(td == wkt.System_Double)
            {
                switch(op.Alu)
                {
                    case BinaryOperator.ALU.ADD: name = c_BinaryOperations_DoubleAdd; break;
                    case BinaryOperator.ALU.SUB: name = c_BinaryOperations_DoubleSub; break;
                    case BinaryOperator.ALU.MUL: name = c_BinaryOperations_DoubleMul; break;
                    case BinaryOperator.ALU.DIV: name = c_BinaryOperations_DoubleDiv; break;
                    case BinaryOperator.ALU.REM: name = c_BinaryOperations_DoubleRem; break;
                }
            }

            if(name != null)
            {
                host.CoverObject( ts.GetWellKnownMethod( name ) );
            }
        }

        [CompilationSteps.CallClosureHandler( typeof(UnaryOperator) )]
        private static void Protect_UnaryOperator( ComputeCallsClosure.Context host   ,
                                                   Operator                    target )
        {
            UnaryOperator                   op   = (UnaryOperator)target;
            TypeRepresentation              td   =                op.FirstArgument.Type;
            TypeSystemForCodeTransformation ts   =                host.TypeSystem;
            WellKnownTypes                  wkt  =                ts.WellKnownTypes;
            string                          name =                null;

            if(td == wkt.System_Single)
            {
                switch(op.Alu)
                {
                    case UnaryOperator.ALU.NEG   : name = c_UnaryOperations_FloatNeg   ; break;
                    case UnaryOperator.ALU.FINITE: name = c_UnaryOperations_FloatFinite; break;
                }
            }
            else if(td == wkt.System_Double)
            {
                switch(op.Alu)
                {
                    case UnaryOperator.ALU.NEG   : name = c_UnaryOperations_DoubleNeg   ; break;
                    case UnaryOperator.ALU.FINITE: name = c_UnaryOperations_DoubleFinite; break;
                }
            }

            if(name != null)
            {
                host.CoverObject( ts.GetWellKnownMethod( name ) );
            }
        }

        [CompilationSteps.CallClosureHandler( typeof(ConvertOperator) )]
        private static void Protect_ConvertOperator( ComputeCallsClosure.Context host   ,
                                                     Operator                    target )
        {
            ConvertOperator                 op         = (ConvertOperator)target;
            TypeSystemForCodeTransformation ts         =                  host.TypeSystem;
            TypeRepresentation.BuiltInTypes kindInput  =                  op.InputKind;
            TypeRepresentation.BuiltInTypes kindOutput =                  op.OutputKind;
            string                          name       =                  null;

            switch(kindOutput)
            {
                case TypeRepresentation.BuiltInTypes.R4:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.I4: name = c_Convert_IntToFloat         ; break;
                        case TypeRepresentation.BuiltInTypes.U4: name = c_Convert_UnsignedIntToFloat ; break;
                        case TypeRepresentation.BuiltInTypes.I8: name = c_Convert_LongToFloat        ; break;
                        case TypeRepresentation.BuiltInTypes.U8: name = c_Convert_UnsignedLongToFloat; break;
                        case TypeRepresentation.BuiltInTypes.R8: name = c_Convert_DoubleToFloat      ; break;
                    }
                    break;

                case TypeRepresentation.BuiltInTypes.R8:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.I4: name = c_Convert_IntToDouble         ; break;
                        case TypeRepresentation.BuiltInTypes.U4: name = c_Convert_UnsignedIntToDouble ; break;
                        case TypeRepresentation.BuiltInTypes.I8: name = c_Convert_LongToDouble        ; break;
                        case TypeRepresentation.BuiltInTypes.U8: name = c_Convert_UnsignedLongToDouble; break;
                        case TypeRepresentation.BuiltInTypes.R4: name = c_Convert_FloatToDouble       ; break;
                    }
                    break;

                case TypeRepresentation.BuiltInTypes.I4:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.R4: name = c_Convert_FloatToInt ; break;
                        case TypeRepresentation.BuiltInTypes.R8: name = c_Convert_DoubleToInt; break;
                    }
                    break;

                case TypeRepresentation.BuiltInTypes.U4:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.R4: name = c_Convert_FloatToUnsignedInt ; break;
                        case TypeRepresentation.BuiltInTypes.R8: name = c_Convert_DoubleToUnsignedInt; break;
                    }
                    break;

                case TypeRepresentation.BuiltInTypes.I8:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.R4: name = c_Convert_FloatToLong ; break;
                        case TypeRepresentation.BuiltInTypes.R8: name = c_Convert_DoubleToLong; break;
                    }
                    break;

                case TypeRepresentation.BuiltInTypes.U8:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.R4: name = c_Convert_FloatToUnsignedLong ; break;
                        case TypeRepresentation.BuiltInTypes.R8: name = c_Convert_DoubleToUnsignedLong; break;
                    }
                    break;
            }

            if(name != null)
            {
                host.CoverObject( ts.GetWellKnownMethod( name ) );
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ConvertUnsupportedOperatorsToMethodCalls) )]
        [CompilationSteps.OperatorHandler( typeof(CompareConditionalControlOperator) )]
        private static void Handle_CompareConditionalControlOperator( PhaseExecution.NotificationContext nc )
        {
            CompareConditionalControlOperator op = (CompareConditionalControlOperator)nc.CurrentOperator;

            if(op.FirstArgument.Type.IsFloatingPoint)
            {
                ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;
                TypeSystemForCodeTransformation            ts  = nc.TypeSystem;
                TemporaryVariableExpression                tmp = cfg.AllocateTemporary( ts.WellKnownTypes.System_Boolean, null );

                CompareAndSetOperator opCmp = CompareAndSetOperator.New( op.DebugInfo, op.Condition, op.Signed, tmp, op.FirstArgument, op.SecondArgument );
                op.AddOperatorBefore( opCmp );

                BinaryConditionalControlOperator opCtrl = BinaryConditionalControlOperator.New( op.DebugInfo, tmp, op.TargetBranchNotTaken, op.TargetBranchTaken );

                op.SubstituteWithOperator( opCtrl, Operator.SubstitutionFlags.Default );
                
                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ConvertUnsupportedOperatorsToMethodCalls) )]
        [CompilationSteps.OperatorHandler( typeof(CompareAndSetOperator) )]
        private static void Handle_CompareAndSetOperator( PhaseExecution.NotificationContext nc )
        {
            CompareAndSetOperator op     = (CompareAndSetOperator)nc.CurrentOperator;
            Expression            exSrc1 =                        op.FirstArgument;
            Expression            exSrc2 =                        op.SecondArgument;
            TypeRepresentation    tdSrc1 =                        exSrc1.Type;
            TypeRepresentation    tdSrc2 =                        exSrc2.Type;

            if(tdSrc1.IsFloatingPoint &&
               tdSrc2.IsFloatingPoint  )
            {
                ControlFlowGraphStateForCodeTransformation cfg      = nc.CurrentCFG;
                TypeSystemForCodeTransformation            ts       = nc.TypeSystem;
                uint                                       sizeSrc1 = tdSrc1.SizeOfHoldingVariableInWords;
                uint                                       sizeSrc2 = tdSrc2.SizeOfHoldingVariableInWords;
                string                                     name;

                CHECKS.ASSERT( sizeSrc1 == sizeSrc2, "Cannot compare entities of different size: {0} <=> {1}", exSrc1, exSrc2 );

                if(sizeSrc1 == 1)
                {
                    switch(op.Condition)
                    {
                        case CompareAndSetOperator.ActionCondition.EQ: name = c_CompareAndSet_FloatEqual         ; break;
                        case CompareAndSetOperator.ActionCondition.GE: name = c_CompareAndSet_FloatGreaterOrEqual; break;
                        case CompareAndSetOperator.ActionCondition.GT: name = c_CompareAndSet_FloatGreater       ; break;
                        case CompareAndSetOperator.ActionCondition.LE: name = c_CompareAndSet_FloatLessOrEqual   ; break;
                        case CompareAndSetOperator.ActionCondition.LT: name = c_CompareAndSet_FloatLess          ; break;
                        case CompareAndSetOperator.ActionCondition.NE: name = c_CompareAndSet_FloatNotEqual      ; break;

                        default: throw TypeConsistencyErrorException.Create( "Unexpected value {0} in {1}", op.Condition, op );
                    }
                }
                else if(sizeSrc1 == 2)
                {
                    switch(op.Condition)
                    {
                        case CompareAndSetOperator.ActionCondition.EQ: name = c_CompareAndSet_DoubleEqual         ; break;
                        case CompareAndSetOperator.ActionCondition.GE: name = c_CompareAndSet_DoubleGreaterOrEqual; break;
                        case CompareAndSetOperator.ActionCondition.GT: name = c_CompareAndSet_DoubleGreater       ; break;
                        case CompareAndSetOperator.ActionCondition.LE: name = c_CompareAndSet_DoubleLessOrEqual   ; break;
                        case CompareAndSetOperator.ActionCondition.LT: name = c_CompareAndSet_DoubleLess          ; break;
                        case CompareAndSetOperator.ActionCondition.NE: name = c_CompareAndSet_DoubleNotEqual      ; break;

                        default: throw TypeConsistencyErrorException.Create( "Unexpected value {0} in {1}", op.Condition, op );
                    }
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unsupported compare operation larger than 64 bits: {0}", op );
                }

                ts.SubstituteWithCallToHelper( name, op );

                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ConvertUnsupportedOperatorsToMethodCalls) )]
        [CompilationSteps.OperatorHandler( typeof(BinaryOperator) )]
        private static void Handle_BinaryOperator( PhaseExecution.NotificationContext nc )
        {
            BinaryOperator     op     = (BinaryOperator)nc.CurrentOperator;
            Expression         exSrc1 =                 op.FirstArgument;
            Expression         exSrc2 =                 op.SecondArgument;
            TypeRepresentation tdSrc1 =                 exSrc1.Type;
            TypeRepresentation tdSrc2 =                 exSrc2.Type;

            if(tdSrc1.IsFloatingPoint &&
               tdSrc2.IsFloatingPoint  )
            {
                ControlFlowGraphStateForCodeTransformation cfg      = nc.CurrentCFG;
                TypeSystemForCodeTransformation            ts       = nc.TypeSystem;
                VariableExpression                         exRes    = op.FirstResult;
                TypeRepresentation                         tdRes    = exRes .Type;
                uint                                       sizeRes  = tdRes .SizeOfHoldingVariableInWords;
                uint                                       sizeSrc1 = tdSrc1.SizeOfHoldingVariableInWords;
                uint                                       sizeSrc2 = tdSrc2.SizeOfHoldingVariableInWords;
                string                                     name;

                CHECKS.ASSERT( sizeSrc1 == sizeSrc2, "Cannot compare entities of different size: {0} <=> {1}", exSrc1, exSrc2 );

                if(sizeSrc1 == 1)
                {
                    switch(op.Alu)
                    {
                        case BinaryOperator.ALU.ADD: name = c_BinaryOperations_FloatAdd; break;
                        case BinaryOperator.ALU.SUB: name = c_BinaryOperations_FloatSub; break;
                        case BinaryOperator.ALU.MUL: name = c_BinaryOperations_FloatMul; break;
                        case BinaryOperator.ALU.DIV: name = c_BinaryOperations_FloatDiv; break;
                        case BinaryOperator.ALU.REM: name = c_BinaryOperations_FloatRem; break;

                        default:
                            throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                    }
                }
                else if(sizeSrc1 == 2)
                {
                    switch(op.Alu)
                    {
                        case BinaryOperator.ALU.ADD: name = c_BinaryOperations_DoubleAdd; break;
                        case BinaryOperator.ALU.SUB: name = c_BinaryOperations_DoubleSub; break;
                        case BinaryOperator.ALU.MUL: name = c_BinaryOperations_DoubleMul; break;
                        case BinaryOperator.ALU.DIV: name = c_BinaryOperations_DoubleDiv; break;
                        case BinaryOperator.ALU.REM: name = c_BinaryOperations_DoubleRem; break;

                        default:
                            throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                    }
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                }

                ts.SubstituteWithCallToHelper( name, op );

                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ConvertUnsupportedOperatorsToMethodCalls) )]
        [CompilationSteps.OperatorHandler( typeof(UnaryOperator) )]
        private static void Handle_UnaryOperator( PhaseExecution.NotificationContext nc )
        {
            UnaryOperator      op    = (UnaryOperator)nc.CurrentOperator;
            Expression         exSrc =                op.FirstArgument;
            TypeRepresentation tdSrc =                exSrc.Type;

            if(tdSrc.IsFloatingPoint)
            {
                ControlFlowGraphStateForCodeTransformation cfg     = nc.CurrentCFG;
                TypeSystemForCodeTransformation            ts      = nc.TypeSystem;
                uint                                       sizeSrc = tdSrc.SizeOfHoldingVariableInWords;
                string                                     name;

                if(sizeSrc == 1)
                {
                    switch(op.Alu)
                    {
                        case UnaryOperator.ALU.NEG   : name = c_UnaryOperations_FloatNeg   ; break;
                        case UnaryOperator.ALU.FINITE: name = c_UnaryOperations_FloatFinite; break;

                        default:
                            throw TypeConsistencyErrorException.Create( "Unsupported inputs for unary operator: {0}", op );
                    }
                }
                else if(sizeSrc == 2)
                {
                    switch(op.Alu)
                    {
                        case UnaryOperator.ALU.NEG   : name = c_UnaryOperations_DoubleNeg   ; break;
                        case UnaryOperator.ALU.FINITE: name = c_UnaryOperations_DoubleFinite; break;

                        default:
                            throw TypeConsistencyErrorException.Create( "Unsupported inputs for unary operator: {0}", op );
                    }
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unsupported inputs for unary operator: {0}", op );
                }

                ts.SubstituteWithCallToHelper( name, op );

                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ConvertUnsupportedOperatorsToMethodCalls) )]
        [CompilationSteps.OperatorHandler( typeof(BinaryConditionalControlOperator) )]
        private static void Handle_BinaryConditionalControlOperator( PhaseExecution.NotificationContext nc )
        {
            BinaryConditionalControlOperator op    = (BinaryConditionalControlOperator)nc.CurrentOperator;
            Expression                       exSrc =                                   op.FirstArgument;
            TypeRepresentation               tdSrc =                                   exSrc.Type;

            //--//

            if(tdSrc.IsFloatingPoint)
            {
                TypeSystemForCodeTransformation            ts        = nc.TypeSystem;
                ControlFlowGraphStateForCodeTransformation cfg       = nc.CurrentCFG;
                Debugging.DebugInfo                        debugInfo = op.DebugInfo;
                uint                                       sizeSrc   = tdSrc.SizeOfHoldingVariableInWords;
                string                                     name;
                object                                     val;

                if(sizeSrc == 1)
                {
                    name = c_CompareAndSet_FloatEqual;
                    val  = (float)0;

                }
                else if(sizeSrc == 2)
                {
                    name = c_CompareAndSet_DoubleEqual;
                    val  = (double)0;
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unsupported inputs for compare: {0}", op );
                }

                MethodRepresentation md          = ts.GetWellKnownMethod( name );
                VariableExpression   tmpFragment = cfg.AllocatePseudoRegister( ts.WellKnownTypes.System_Boolean );

                Expression[] rhs = ts.AddTypePointerToArgumentsOfStaticMethod( md, op.FirstArgument, ts.CreateConstant( md.OwnerType, val ) );
                StaticCallOperator opCall = StaticCallOperator.New( op.DebugInfo, CallOperator.CallKind.Direct, md, VariableExpression.ToArray( tmpFragment ), rhs );
                op.AddOperatorBefore( opCall );

                //--//

                var cc = cfg.AllocateConditionCode();

                op.AddOperatorBefore( CompareOperator.New( debugInfo, cc, tmpFragment, ts.CreateConstant( 0 ) ) );

                ConditionCodeConditionalControlOperator opNew = ConditionCodeConditionalControlOperator.New( debugInfo, ConditionCodeExpression.Comparison.NotEqual, cc, op.TargetBranchNotTaken, op.TargetBranchTaken );
                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.FromImplicitToExplictExceptions) )]
        [CompilationSteps.OperatorHandler( typeof(ConvertOperator) )]
        private static void Handle_ConvertOperator_Exceptions( PhaseExecution.NotificationContext nc )
        {
            ConvertOperator    op  = (ConvertOperator)nc.CurrentOperator;
            VariableExpression lhs = op.FirstResult;
            Expression         rhs = op.FirstArgument;

            if(op.CheckOverflow)
            {
                ConvertOperator opNew = ConvertOperator.New( op.DebugInfo, op.InputKind, op.OutputKind, false, lhs, rhs );

                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                //
                // BUGBUG: We are dropping the overflow check!!
                //
////            CreateOverflowCheck( nc, op, opNew );

                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ConvertUnsupportedOperatorsToMethodCalls) )]
        [CompilationSteps.OperatorHandler( typeof(ConvertOperator) )]
        private static void Handle_ConvertOperator( PhaseExecution.NotificationContext nc )
        {
            ConvertOperator                 op         = (ConvertOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation ts         =             nc.TypeSystem;
            TypeRepresentation.BuiltInTypes kindInput  =             op.InputKind;
            TypeRepresentation.BuiltInTypes kindOutput =             op.OutputKind;
            string                          name       =             null;

            switch(kindOutput)
            {
                case TypeRepresentation.BuiltInTypes.R4:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.I4: name = c_Convert_IntToFloat         ; break;
                        case TypeRepresentation.BuiltInTypes.U4: name = c_Convert_UnsignedIntToFloat ; break;
                        case TypeRepresentation.BuiltInTypes.I8: name = c_Convert_LongToFloat        ; break;
                        case TypeRepresentation.BuiltInTypes.U8: name = c_Convert_UnsignedLongToFloat; break;
                        case TypeRepresentation.BuiltInTypes.R8: name = c_Convert_DoubleToFloat      ; break;
                    }
                    break;

                case TypeRepresentation.BuiltInTypes.R8:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.I4: name = c_Convert_IntToDouble         ; break;
                        case TypeRepresentation.BuiltInTypes.U4: name = c_Convert_UnsignedIntToDouble ; break;
                        case TypeRepresentation.BuiltInTypes.I8: name = c_Convert_LongToDouble        ; break;
                        case TypeRepresentation.BuiltInTypes.U8: name = c_Convert_UnsignedLongToDouble; break;
                        case TypeRepresentation.BuiltInTypes.R4: name = c_Convert_FloatToDouble       ; break;
                    }
                    break;

                case TypeRepresentation.BuiltInTypes.I4:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.R4: name = c_Convert_FloatToInt ; break;
                        case TypeRepresentation.BuiltInTypes.R8: name = c_Convert_DoubleToInt; break;
                    }
                    break;

                case TypeRepresentation.BuiltInTypes.U4:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.R4: name = c_Convert_FloatToUnsignedInt ; break;
                        case TypeRepresentation.BuiltInTypes.R8: name = c_Convert_DoubleToUnsignedInt; break;
                    }
                    break;

                case TypeRepresentation.BuiltInTypes.I8:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.R4: name = c_Convert_FloatToLong ; break;
                        case TypeRepresentation.BuiltInTypes.R8: name = c_Convert_DoubleToLong; break;
                    }
                    break;

                case TypeRepresentation.BuiltInTypes.U8:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.R4: name = c_Convert_FloatToUnsignedLong ; break;
                        case TypeRepresentation.BuiltInTypes.R8: name = c_Convert_DoubleToUnsignedLong; break;
                    }
                    break;
            }

            if(name != null)
            {
                MethodRepresentation md = ts.GetWellKnownMethod( name );

                Expression[] rhs = ts.AddTypePointerToArgumentsOfStaticMethod( md, op.FirstArgument, ts.CreateConstant( ts.WellKnownTypes.System_Boolean, op.CheckOverflow ) );

                StaticCallOperator opCall = StaticCallOperator.New( op.DebugInfo, CallOperator.CallKind.Direct, md, op.Results, rhs );

                op.SubstituteWithOperator( opCall, Operator.SubstitutionFlags.Default );

                nc.MarkAsModified();
            }
        }
    }
}
