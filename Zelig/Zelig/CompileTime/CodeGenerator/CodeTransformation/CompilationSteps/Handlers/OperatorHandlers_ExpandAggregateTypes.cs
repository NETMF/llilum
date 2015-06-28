//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Handlers
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class OperatorHandlers_ExpandAggregateTypes
    {
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(InitialValueOperator) )]
        private static void Handle_InitialValueOperator( PhaseExecution.NotificationContext nc )
        {
            InitialValueOperator            op = (InitialValueOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation ts =                       nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                Debugging.DebugInfo                        debugInfo    = op.DebugInfo;
                VariableExpression                         exDst        = op.FirstResult;
                Expression[]                               dstFragments = cfg.GetFragmentsForExpression( exDst );

                foreach(Expression dstFragment in dstFragments)
                {
                    InitialValueOperator opNew = InitialValueOperator.New( debugInfo, (VariableExpression)dstFragment );

                    opNew.CopyAnnotations( op );

                    op.AddOperatorBefore( opNew );
                }

                op.Delete();
                
                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(SingleAssignmentOperator) )]
        private static void Handle_AssignmentOperator( PhaseExecution.NotificationContext nc )
        {
            SingleAssignmentOperator        op = (SingleAssignmentOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation ts =                           nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                BasicBlock                                 current      = op.BasicBlock;
                Debugging.DebugInfo                        debugInfo    = op.DebugInfo;
                Expression                                 exSrc        = op.FirstArgument;
                VariableExpression                         exDst        = op.FirstResult;
                Expression[]                               srcFragments = cfg.GetFragmentsForExpression( exSrc );
                Expression[]                               dstFragments = cfg.GetFragmentsForExpression( exDst );
                uint                                       srcSize      = (uint)srcFragments.Length;
                uint                                       dstSize      = (uint)dstFragments.Length;

                if(srcSize != dstSize)
                {
                    if(srcSize == 1)
                    {
                        for(uint offset = 0; offset < dstSize; offset++)
                        {
                            PartialAssignmentOperator opNew = PartialAssignmentOperator.New( debugInfo, (VariableExpression)dstFragments[offset], 0, srcFragments[0], offset );
                            opNew.CopyAnnotations( op );
                            op.AddOperatorBefore( opNew );
                        }

                        op.Delete();
                    }
                    else if(dstSize == 1)
                    {
                        for(uint offset = 0; offset < srcSize; offset++)
                        {
                            PartialAssignmentOperator opNew = PartialAssignmentOperator.New( debugInfo, (VariableExpression)srcFragments[0], offset, dstFragments[offset], 0 );
                            opNew.CopyAnnotations( op );
                            op.AddOperatorBefore( opNew );
                        }

                        op.Delete();
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Unexpected kind of assignment: {0}", op );
                    }
                }
                else
                {
                    CHECKS.ASSERT( exSrc.Type.SizeOfHoldingVariableInWords == exDst.Type.SizeOfHoldingVariableInWords, "Assignments between elements of different sizes not supported at IR level: {0} <= {1}", exSrc, exDst );

                    if(srcSize == 1)
                    {
                        SingleAssignmentOperator opNew = SingleAssignmentOperator.New( debugInfo, (VariableExpression)dstFragments[0], srcFragments[0] );
                        op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                    }
                    else
                    {
                        for(int offset = 0; offset < srcSize; offset++)
                        {
                            SingleAssignmentOperator opNew = SingleAssignmentOperator.New( debugInfo, (VariableExpression)dstFragments[offset], srcFragments[offset] );
                            opNew.CopyAnnotations( op );
                            op.AddOperatorBefore( opNew );
                        }

                        op.Delete();
                    }
                }
                
                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(AddressAssignmentOperator) )]
        private static void Handle_AddressAssignmentOperator( PhaseExecution.NotificationContext nc )
        {
            AddressAssignmentOperator       op = (AddressAssignmentOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation ts =                            nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                BasicBlock                                 current      = op.BasicBlock;
                Debugging.DebugInfo                        debugInfo    = op.DebugInfo;
                Expression                                 exSrc        = op.FirstArgument;
                VariableExpression                         exDst        = op.FirstResult;
                Expression[]                               srcFragments = cfg.GetFragmentsForExpression( exSrc );
                Expression[]                               dstFragments = cfg.GetFragmentsForExpression( exDst );

                CHECKS.ASSERT( exDst.Type.SizeOfHoldingVariableInWords == 1, "Pointer {0} is wider than a word", exDst );
                CHECKS.ASSERT( srcFragments[0] is StackLocationExpression  , "Cannot take address of {0}, expecting a stack location", srcFragments[0] );

                AddressAssignmentOperator opNew = AddressAssignmentOperator.New( debugInfo, (VariableExpression)dstFragments[0], srcFragments[0] );
                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                opNew.AddAnnotation( NotNullAnnotation.Create( ts ) );
                
                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(CompareConditionalControlOperator) )]
        private static void Handle_CompareConditionalControlOperator( PhaseExecution.NotificationContext nc )
        {
            CompareConditionalControlOperator op = (CompareConditionalControlOperator)nc.CurrentOperator;

            //--//

            ControlFlowGraphStateForCodeTransformation cfg           = nc.CurrentCFG;
            Debugging.DebugInfo                        debugInfo     = op.DebugInfo;
            Expression                                 exSrc1        = op.FirstArgument;
            Expression                                 exSrc2        = op.SecondArgument;
            Expression[]                               src1Fragments = cfg.GetFragmentsForExpression( exSrc1 );
            Expression[]                               src2Fragments = cfg.GetFragmentsForExpression( exSrc2 );
            TypeRepresentation                         tdSrc1        = exSrc1.Type;
            TypeRepresentation                         tdSrc2        = exSrc2.Type;
            uint                                       sizeSrc1      = tdSrc1.SizeOfHoldingVariableInWords;
            uint                                       sizeSrc2      = tdSrc2.SizeOfHoldingVariableInWords;
            bool                                       fSigned       = op.Signed;

            CHECKS.ASSERT( sizeSrc1 == sizeSrc2, "Cannot compare entities of different size: {0} <=> {1}", exSrc1, exSrc2 );

            //--//

            ConditionCodeExpression.Comparison condition;

            switch(op.Condition)
            {
                case CompareAndSetOperator.ActionCondition.EQ:
                    condition = ConditionCodeExpression.Comparison.Equal;
                    break;

                case CompareAndSetOperator.ActionCondition.GE:
                    if(fSigned)
                    {
                        condition = ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual;
                    }
                    else
                    {
                        condition = ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame;
                    }
                    break;

                case CompareAndSetOperator.ActionCondition.GT:
                    if(fSigned)
                    {
                        condition = ConditionCodeExpression.Comparison.SignedGreaterThan;
                    }
                    else
                    {
                        condition = ConditionCodeExpression.Comparison.UnsignedHigherThan;
                    }
                    break;

                case CompareAndSetOperator.ActionCondition.LE:
                    if(fSigned)
                    {
                        condition = ConditionCodeExpression.Comparison.SignedLessThanOrEqual;
                    }
                    else
                    {
                        condition = ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame;
                    }
                    break;

                case CompareAndSetOperator.ActionCondition.LT:
                    if(fSigned)
                    {
                        condition = ConditionCodeExpression.Comparison.SignedLessThan;
                    }
                    else
                    {
                        condition = ConditionCodeExpression.Comparison.UnsignedLowerThan;
                    }
                    break;

                case CompareAndSetOperator.ActionCondition.NE:
                    condition = ConditionCodeExpression.Comparison.NotEqual;
                    break;


                default: throw TypeConsistencyErrorException.Create( "Unexpected value {0} in {1}", op.Condition, op );
            }

            //--//

            if(exSrc1 is ConstantExpression)
            {
                //
                // Swap the left and right operands and change the direction of disequality conditions.
                //
                Expression[] tmp;

                tmp           = src1Fragments;
                src1Fragments = src2Fragments;
                src2Fragments = tmp;

                switch(condition)
                {
                    case ConditionCodeExpression.Comparison.Equal                    : /* Keep the same */                                                      break;
                    case ConditionCodeExpression.Comparison.NotEqual                 : /* Keep the same */                                                      break;
                    case ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame : condition = ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame ; break;
                    case ConditionCodeExpression.Comparison.UnsignedLowerThan        : condition = ConditionCodeExpression.Comparison.UnsignedHigherThan      ; break;
                    case ConditionCodeExpression.Comparison.UnsignedHigherThan       : condition = ConditionCodeExpression.Comparison.UnsignedLowerThan       ; break;
                    case ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame  : condition = ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame; break;
                    case ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual : condition = ConditionCodeExpression.Comparison.SignedLessThanOrEqual   ; break;
                    case ConditionCodeExpression.Comparison.SignedLessThan           : condition = ConditionCodeExpression.Comparison.SignedGreaterThan       ; break;
                    case ConditionCodeExpression.Comparison.SignedGreaterThan        : condition = ConditionCodeExpression.Comparison.SignedLessThan          ; break;
                    case ConditionCodeExpression.Comparison.SignedLessThanOrEqual    : condition = ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual; break;

                    default:
                        throw TypeConsistencyErrorException.Create( "Unsupported compare operation: {0}", op );
                }
            }

            if(tdSrc1.IsFloatingPoint &&
               tdSrc2.IsFloatingPoint  )
            {
                CHECKS.ASSERT( src1Fragments.Length == 1, "Expecting left argument in a single floating point register: {0}" , op );
                CHECKS.ASSERT( src2Fragments.Length == 1, "Expecting right argument in a single floating point register: {0}", op );

                ConditionCodeExpression cc = cfg.AllocateConditionCode();

                op.AddOperatorBefore( CompareOperator.New( debugInfo, cc, src1Fragments[0], src2Fragments[0] ) );

                //--//

                ConditionCodeConditionalControlOperator opNew = ConditionCodeConditionalControlOperator.New( debugInfo, condition, cc, op.TargetBranchNotTaken, op.TargetBranchTaken );
                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
            }
            else if((tdSrc1.IsNumeric == false || tdSrc1.IsInteger) &&
                    (tdSrc2.IsNumeric == false || tdSrc2.IsInteger)  )
            {
                ConditionCodeExpression cc = cfg.AllocateConditionCode();

                if(sizeSrc1 == 1)
                {
                    op.AddOperatorBefore( CompareOperator.New( debugInfo, cc, src1Fragments[0], src2Fragments[0] ) );

                    //--//

                    ConditionCodeConditionalControlOperator opNew = ConditionCodeConditionalControlOperator.New( debugInfo, condition, cc, op.TargetBranchNotTaken, op.TargetBranchTaken );
                    op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                }
                else if(sizeSrc1 == 2)
                {
                    op.AddOperatorBefore( LongCompareOperator.New( debugInfo, cc, src1Fragments[0], src1Fragments[1], src2Fragments[0], src2Fragments[1] ) );

                    //--//

                    ConditionCodeConditionalControlOperator opNew = ConditionCodeConditionalControlOperator.New( debugInfo, condition, cc, op.TargetBranchNotTaken, op.TargetBranchTaken );
                    op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unsupported compare operation larger than 64 bits: {0}", op );
                }
            }
            else
            {
                throw TypeConsistencyErrorException.Create( "Unsupported mixed-mode compare operation: {0}", op );
            }
            
            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(CompareAndSetOperator) )]
        private static void Handle_CompareAndSetOperator( PhaseExecution.NotificationContext nc )
        {
            CompareAndSetOperator op = (CompareAndSetOperator)nc.CurrentOperator;

            //--//

            ControlFlowGraphStateForCodeTransformation cfg           = nc.CurrentCFG;
            TypeSystemForCodeTransformation            ts            = nc.TypeSystem;
            BasicBlock                                 current       = op.BasicBlock;
            Debugging.DebugInfo                        debugInfo     = op.DebugInfo;
            VariableExpression                         exRes         = op.FirstResult;
            Expression                                 exSrc1        = op.FirstArgument;
            Expression                                 exSrc2        = op.SecondArgument;
            Expression[]                               resFragments  = cfg.GetFragmentsForExpression( exRes  );
            Expression[]                               src1Fragments = cfg.GetFragmentsForExpression( exSrc1 );
            Expression[]                               src2Fragments = cfg.GetFragmentsForExpression( exSrc2 );
            TypeRepresentation                         tdSrc1        = exSrc1.Type;
            TypeRepresentation                         tdSrc2        = exSrc2.Type;
            uint                                       sizeSrc1      = tdSrc1.SizeOfHoldingVariableInWords;
            uint                                       sizeSrc2      = tdSrc2.SizeOfHoldingVariableInWords;
            bool                                       fSigned       = op.Signed;

            CHECKS.ASSERT( resFragments.Length == 1, "The lvalue of CompareAndSetOperator should be a 32bit word: {0}", op );
            CHECKS.ASSERT( sizeSrc1 == sizeSrc2, "Cannot compare entities of different size: {0} <=> {1}", exSrc1, exSrc2 );

            //--//

            ConditionCodeExpression.Comparison condition;

            switch(op.Condition)
            {
                case CompareAndSetOperator.ActionCondition.EQ:
                    condition = ConditionCodeExpression.Comparison.Equal;
                    break;

                case CompareAndSetOperator.ActionCondition.GE:
                    if(fSigned)
                    {
                        condition = ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual;
                    }
                    else
                    {
                        condition = ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame;
                    }
                    break;

                case CompareAndSetOperator.ActionCondition.GT:
                    if(fSigned)
                    {
                        condition = ConditionCodeExpression.Comparison.SignedGreaterThan;
                    }
                    else
                    {
                        condition = ConditionCodeExpression.Comparison.UnsignedHigherThan;
                    }
                    break;

                case CompareAndSetOperator.ActionCondition.LE:
                    if(fSigned)
                    {
                        condition = ConditionCodeExpression.Comparison.SignedLessThanOrEqual;
                    }
                    else
                    {
                        condition = ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame;
                    }
                    break;

                case CompareAndSetOperator.ActionCondition.LT:
                    if(fSigned)
                    {
                        condition = ConditionCodeExpression.Comparison.SignedLessThan;
                    }
                    else
                    {
                        condition = ConditionCodeExpression.Comparison.UnsignedLowerThan;
                    }
                    break;

                case CompareAndSetOperator.ActionCondition.NE:
                    condition = ConditionCodeExpression.Comparison.NotEqual;
                    break;


                default: throw TypeConsistencyErrorException.Create( "Unexpected value {0} in {1}", op.Condition, op );
            }

            //--//

            if(exSrc1 is ConstantExpression)
            {
                //
                // Swap the left and right operands and change the direction of disequality conditions.
                //
                Expression[] tmp;

                tmp           = src1Fragments;
                src1Fragments = src2Fragments;
                src2Fragments = tmp;

                switch(condition)
                {
                    case ConditionCodeExpression.Comparison.Equal                    : /* Keep the same */                                                      break;
                    case ConditionCodeExpression.Comparison.NotEqual                 : /* Keep the same */                                                      break;
                    case ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame : condition = ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame ; break;
                    case ConditionCodeExpression.Comparison.UnsignedLowerThan        : condition = ConditionCodeExpression.Comparison.UnsignedHigherThan      ; break;
                    case ConditionCodeExpression.Comparison.UnsignedHigherThan       : condition = ConditionCodeExpression.Comparison.UnsignedLowerThan       ; break;
                    case ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame  : condition = ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame; break;
                    case ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual : condition = ConditionCodeExpression.Comparison.SignedLessThanOrEqual   ; break;
                    case ConditionCodeExpression.Comparison.SignedLessThan           : condition = ConditionCodeExpression.Comparison.SignedGreaterThan       ; break;
                    case ConditionCodeExpression.Comparison.SignedGreaterThan        : condition = ConditionCodeExpression.Comparison.SignedLessThan          ; break;
                    case ConditionCodeExpression.Comparison.SignedLessThanOrEqual    : condition = ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual; break;

                    default:
                        throw TypeConsistencyErrorException.Create( "Unsupported compare operation: {0}", op );
                }
            }

            if(tdSrc1.IsFloatingPoint &&
               tdSrc2.IsFloatingPoint  )
            {
                CHECKS.ASSERT( src1Fragments.Length == 1, "Expecting left argument in a single floating point register: {0}" , op );
                CHECKS.ASSERT( src2Fragments.Length == 1, "Expecting right argument in a single floating point register: {0}", op );

                ConditionCodeExpression cc = cfg.AllocateConditionCode();

                op.AddOperatorBefore( CompareOperator.New( debugInfo, cc, src1Fragments[0], src2Fragments[0] ) );

                //--//

                SetIfConditionIsTrueOperator opNew = SetIfConditionIsTrueOperator.New( debugInfo, condition, (VariableExpression)resFragments[0], cc );
                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
            }
            else if((tdSrc1.IsNumeric == false || tdSrc1.IsInteger) &&
                    (tdSrc2.IsNumeric == false || tdSrc2.IsInteger)  )
            {
                ConditionCodeExpression cc = cfg.AllocateConditionCode();

                if(sizeSrc1 == 1)
                {
                    op.AddOperatorBefore( CompareOperator.New( debugInfo, cc, src1Fragments[0], src2Fragments[0] ) );

                    //--//

                    SetIfConditionIsTrueOperator opNew = SetIfConditionIsTrueOperator.New( debugInfo, condition, (VariableExpression)resFragments[0], cc );
                    op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                }
                else if(sizeSrc1 == 2)
                {
                    op.AddOperatorBefore( LongCompareOperator.New( debugInfo, cc, src1Fragments[0], src1Fragments[1], src2Fragments[0], src2Fragments[1] ) );

                    //--//

                    SetIfConditionIsTrueOperator opNew = SetIfConditionIsTrueOperator.New( debugInfo, condition, (VariableExpression)resFragments[0], cc );
                    op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unsupported compare operation larger than 64 bits: {0}", op );
                }
            }
            else
            {
                throw TypeConsistencyErrorException.Create( "Unsupported mixed-mode compare operation: {0}", op );
            }
            
            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(AbstractBinaryOperator) )]
        private static void Handle_BinaryOperator( PhaseExecution.NotificationContext nc )
        {
            var                             op = (AbstractBinaryOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation ts =                         nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ControlFlowGraphStateForCodeTransformation cfg           = nc.CurrentCFG;
                BasicBlock                                 current       = op.BasicBlock;
                Debugging.DebugInfo                        debugInfo     = op.DebugInfo;
                VariableExpression                         exRes         = op.FirstResult;
                Expression                                 exSrc1        = op.FirstArgument;
                Expression                                 exSrc2        = op.SecondArgument;
                Expression[]                               resFragments  = cfg.GetFragmentsForExpression( exRes  );
                Expression[]                               src1Fragments = cfg.GetFragmentsForExpression( exSrc1 );
                Expression[]                               src2Fragments = cfg.GetFragmentsForExpression( exSrc2 );
                TypeRepresentation                         tdRes         = exRes .Type;
                TypeRepresentation                         tdSrc1        = exSrc1.Type;
                TypeRepresentation                         tdSrc2        = exSrc2.Type;
                uint                                       sizeRes       = tdRes .SizeOfHoldingVariableInWords;
                uint                                       sizeSrc1      = tdSrc1.SizeOfHoldingVariableInWords;
                uint                                       sizeSrc2      = tdSrc2.SizeOfHoldingVariableInWords;
                bool                                       fSigned       = op.Signed;
                var                                        alu           = op.Alu;

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
                    if(alu != BinaryOperator.ALU.MUL)
                    {
                        throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                    }
                }

                if(tdSrc1.IsFloatingPoint &&
                   tdSrc2.IsFloatingPoint  )
                {
                    CHECKS.ASSERT( resFragments .Length == 1, "Expecting result in a single floating point register: {0}"        , op );
                    CHECKS.ASSERT( src1Fragments.Length == 1, "Expecting left argument in a single floating point register: {0}" , op );
                    CHECKS.ASSERT( src2Fragments.Length == 1, "Expecting right argument in a single floating point register: {0}", op );

                    VariableExpression     newRes  = (VariableExpression)resFragments [0];
                    Expression             newSrc1 =                     src1Fragments[0];
                    Expression             newSrc2 =                     src2Fragments[0];
                    AbstractBinaryOperator opNew;
                    
                    if     (op is BinaryOperator                 ) { opNew = BinaryOperator                 .New( debugInfo, alu, fSigned, false, newRes,                  newSrc1, newSrc2                                       ); }
                    else if(op is BinaryOperatorWithCarryIn      ) { opNew = BinaryOperatorWithCarryIn      .New( debugInfo, alu, fSigned, false, newRes,                  newSrc1, newSrc2, (VariableExpression)op.ThirdArgument ); }
                    else if(op is BinaryOperatorWithCarryInAndOut) { opNew = BinaryOperatorWithCarryInAndOut.New( debugInfo, alu, fSigned, false, newRes, op.SecondResult, newSrc1, newSrc2, (VariableExpression)op.ThirdArgument ); }
                    else if(op is BinaryOperatorWithCarryOut     ) { opNew = BinaryOperatorWithCarryOut     .New( debugInfo, alu, fSigned, false, newRes, op.SecondResult, newSrc1, newSrc2                                       ); }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                    }

                    op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                }
                else if((tdSrc1.IsNumeric == false || tdSrc1.IsInteger) &&
                        (tdSrc2.IsNumeric == false || tdSrc2.IsInteger)  )
                {
                    if(sizeSrc1 == 1)
                    {
                        switch(alu)
                        {
                            case BinaryOperator.ALU.DIV:
                            case BinaryOperator.ALU.REM:
                                {
                                    //
                                    // These should have been removed by a previous phase.
                                    //
                                    throw TypeConsistencyErrorException.Create( "Unexpected operator: {0}", op );
                                }

                            default:
                                if(sizeRes == 1)
                                {
                                    VariableExpression     newRes  = (VariableExpression)resFragments [0];
                                    Expression             newSrc1 =                     src1Fragments[0];
                                    Expression             newSrc2 =                     src2Fragments[0];
                                    AbstractBinaryOperator opNew;

                                    if     (op is BinaryOperator                 ) { opNew = BinaryOperator                 .New( debugInfo, alu, fSigned, false, newRes,                  newSrc1, newSrc2                                       ); }
                                    else if(op is BinaryOperatorWithCarryIn      ) { opNew = BinaryOperatorWithCarryIn      .New( debugInfo, alu, fSigned, false, newRes,                  newSrc1, newSrc2, (VariableExpression)op.ThirdArgument ); }
                                    else if(op is BinaryOperatorWithCarryInAndOut) { opNew = BinaryOperatorWithCarryInAndOut.New( debugInfo, alu, fSigned, false, newRes, op.SecondResult, newSrc1, newSrc2, (VariableExpression)op.ThirdArgument ); }
                                    else if(op is BinaryOperatorWithCarryOut     ) { opNew = BinaryOperatorWithCarryOut     .New( debugInfo, alu, fSigned, false, newRes, op.SecondResult, newSrc1, newSrc2                                       ); }
                                    else
                                    {
                                        throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                                    }

                                    op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                                }
                                else if(sizeRes == 2)
                                {
                                    VariableExpression newResLo = (VariableExpression)resFragments [0];
                                    VariableExpression newResHi = (VariableExpression)resFragments [1];
                                    Expression         newSrc1  =                     src1Fragments[0];
                                    Expression         newSrc2  =                     src2Fragments[0];

                                    if(op is BinaryOperator)
                                    {
                                        var opNew = LongBinaryOperator.New( debugInfo, alu, fSigned, false, newResLo, newResHi, newSrc1, newSrc2 );
                                        op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                                    }
                                    else
                                    {
                                        throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                                    }
                                }
                                else
                                {
                                    throw TypeConsistencyErrorException.Create( "Unexpected operator: {0}", op );
                                }
                                break;
                        }
                    }
                    else if(sizeSrc1 == 2)
                    {
                        switch(alu)
                        {
                            case BinaryOperator.ALU.ADD:
                            case BinaryOperator.ALU.SUB:
                                if(op is BinaryOperator)
                                {
                                    ConditionCodeExpression cc = cfg.AllocateConditionCode();

                                    var opLow = BinaryOperatorWithCarryOut.New( debugInfo, alu, fSigned, false, (VariableExpression)resFragments[0], cc, src1Fragments[0], src2Fragments[0] );
                                    op.AddOperatorBefore( opLow );

                                    var opHigh = BinaryOperatorWithCarryIn.New( debugInfo, alu, fSigned, false, (VariableExpression)resFragments[1], src1Fragments[1], src2Fragments[1], cc );
                                    op.AddOperatorBefore( opHigh );
                                }
                                else
                                {
                                    throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                                }
                                break;

                            case BinaryOperator.ALU.MUL:
                            case BinaryOperator.ALU.DIV:
                            case BinaryOperator.ALU.REM:
                                {
                                    //
                                    // These should have been removed by a previous phase.
                                    //
                                    throw TypeConsistencyErrorException.Create( "Unexpected operator: {0}", op );
                                }

                            case BinaryOperator.ALU.AND:
                            case BinaryOperator.ALU.OR :
                            case BinaryOperator.ALU.XOR:
                                if(op is BinaryOperator)
                                {
                                    BinaryOperator opLow = BinaryOperator.New( debugInfo, alu, fSigned, false, (VariableExpression)resFragments[0], src1Fragments[0], src2Fragments[0] );
                                    op.AddOperatorBefore( opLow );

                                    BinaryOperator opHigh = BinaryOperator.New( debugInfo, alu, fSigned, false, (VariableExpression)resFragments[1], src1Fragments[1], src2Fragments[1] );
                                    op.AddOperatorBefore( opHigh );
                                }
                                else
                                {
                                    throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                                }
                                break;

                            case BinaryOperator.ALU.SHL:
                            case BinaryOperator.ALU.SHR:
                                if(op is BinaryOperator)
                                {
                                    ConstantExpression exShift = src2Fragments[0] as ConstantExpression;

                                    //
                                    // Conversion from 64 to 32 bits? Or from 32 to 64 bits?
                                    //
                                    if(exShift != null && exShift.Value is int && (int)exShift.Value == 32)
                                    {
                                        Operator opLow;
                                        Operator opHigh;

                                        switch(alu)
                                        {
                                            case BinaryOperator.ALU.SHL:
                                                //
                                                // Assign low part of Left to high part of Res, clear low part of Res.
                                                //
                                                opLow  = SingleAssignmentOperator.New( debugInfo, (VariableExpression)resFragments[0], ts.CreateConstant( (int)0 ) );
                                                opHigh = SingleAssignmentOperator.New( debugInfo, (VariableExpression)resFragments[1], src1Fragments[0]            );
                                                break;

                                            case BinaryOperator.ALU.SHR:
                                                if(fSigned)
                                                {
                                                    //
                                                    // Assign high part of Left to low part of Res, sign extend high part of Res.
                                                    //
                                                    opLow  = SingleAssignmentOperator.New( debugInfo,                                      (VariableExpression)resFragments[0], src1Fragments[1]                               );
                                                    opHigh = BinaryOperator          .New( debugInfo, BinaryOperator.ALU.SHR, true, false, (VariableExpression)resFragments[1], src1Fragments[1], ts.CreateConstant( (int)31 ) );
                                                }
                                                else
                                                {
                                                    //
                                                    // Assign high part of Left to low part of Res, clear high part of Res.
                                                    //
                                                    opLow  = SingleAssignmentOperator.New( debugInfo, (VariableExpression)resFragments[0], src1Fragments[1]            );
                                                    opHigh = SingleAssignmentOperator.New( debugInfo, (VariableExpression)resFragments[1], ts.CreateConstant( (int)0 ) );
                                                }
                                                break;

                                            default:
                                                throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                                        }

                                        op.AddOperatorBefore     ( opLow                                      );
                                        op.SubstituteWithOperator( opHigh, Operator.SubstitutionFlags.Default );
                                    }
                                    else
                                    {
                                        //
                                        // These should have been removed by a previous phase.
                                        //
                                        throw TypeConsistencyErrorException.Create( "Unexpected operator: {0}", op );
                                    }
                                }
                                else
                                {
                                    throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                                }
                                break;

                            default:
                                throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                        }

                        op.Delete();
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                    }
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                }
                
                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(AbstractUnaryOperator) )]
        private static void Handle_UnaryOperator( PhaseExecution.NotificationContext nc )
        {
            var op = (AbstractUnaryOperator)nc.CurrentOperator;
            var ts =                        nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                Debugging.DebugInfo                        debugInfo    = op.DebugInfo;
                VariableExpression                         exRes        = op.FirstResult;
                Expression                                 exSrc        = op.FirstArgument;
                Expression[]                               resFragments = cfg.GetFragmentsForExpression( exRes );
                Expression[]                               srcFragments = cfg.GetFragmentsForExpression( exSrc );
                TypeRepresentation                         tdSrc        = exSrc.Type;
                uint                                       sizeSrc      = tdSrc.SizeOfHoldingVariableInWords;
                bool                                       fSigned      = op.Signed;
                var                                        alu          = op.Alu;

                if(tdSrc.IsFloatingPoint)
                {
                    if(op is UnaryOperator)
                    {
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Unsupported inputs for unary operator: {0}", op );
                    }

                    CHECKS.ASSERT( resFragments.Length == 1, "Expecting result in a single floating point register: {0}"        , op );
                    CHECKS.ASSERT( srcFragments.Length == 1, "Expecting left argument in a single floating point register: {0}" , op );

                    VariableExpression newRes = (VariableExpression)resFragments[0];
                    Expression         newSrc =                     srcFragments[0];

                    UnaryOperator opNew = UnaryOperator.New( debugInfo, alu, fSigned, false, newRes, newSrc );
                    op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                }
                else if(tdSrc.IsNumeric == false || tdSrc.IsInteger)
                {
                    if(sizeSrc == 1)
                    {
                        VariableExpression    newRes = (VariableExpression)resFragments[0];
                        Expression            newSrc =                     srcFragments[0];
                        AbstractUnaryOperator opNew;

                        if     (op is UnaryOperator            ) { opNew = UnaryOperator            .New( debugInfo, alu, fSigned, false, newRes,                  newSrc ); }
                        else if(op is UnaryOperatorWithCarryOut) { opNew = UnaryOperatorWithCarryOut.New( debugInfo, alu, fSigned, false, newRes, op.SecondResult, newSrc ); }
                        else
                        {
                            throw TypeConsistencyErrorException.Create( "Unsupported inputs for unary operator: {0}", op );
                        }

                        op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                    }
                    else if(sizeSrc == 2)
                    {
                        switch(alu)
                        {
                            case UnaryOperator.ALU.NEG:
                                if(op is UnaryOperator)
                                {
                                    ConditionCodeExpression cc = cfg.AllocateConditionCode();

                                    var opLow = BinaryOperatorWithCarryOut.New( debugInfo, BinaryOperator.ALU.SUB, fSigned, false, (VariableExpression)resFragments[0], cc, ts.CreateConstant( (int)0 ), srcFragments[0] );
                                    op.AddOperatorBefore( opLow );

                                    var opHigh = BinaryOperatorWithCarryIn.New( debugInfo, BinaryOperator.ALU.SUB, fSigned, false, (VariableExpression)resFragments[1], ts.CreateConstant( (int)0 ), srcFragments[1], cc );
                                    op.AddOperatorBefore( opHigh );
                                }
                                else
                                {
                                    throw TypeConsistencyErrorException.Create( "Unsupported inputs for unary operator: {0}", op );
                                }
                                break;

                            case UnaryOperator.ALU.NOT:
                                if(op is UnaryOperator)
                                {
                                    UnaryOperator opLow = UnaryOperator.New( debugInfo, alu, fSigned, false, (VariableExpression)resFragments[0], srcFragments[0] );
                                    op.AddOperatorBefore( opLow );

                                    UnaryOperator opHigh = UnaryOperator.New( debugInfo, alu, fSigned, false, (VariableExpression)resFragments[1], srcFragments[1] );
                                    op.AddOperatorBefore( opHigh );
                                }
                                else
                                {
                                    throw TypeConsistencyErrorException.Create( "Unsupported inputs for unary operator: {0}", op );
                                }
                                break;

                            default:
                                throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                        }

                        op.Delete();
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Unsupported inputs for unary operator: {0}", op );
                    }
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unsupported inputs for unary operator: {0}", op );
                }
                
                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(ConvertOperator) )]
        private static void Handle_ConvertOperator( PhaseExecution.NotificationContext nc )
        {
            ConvertOperator                 op = (ConvertOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation ts =                  nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                BasicBlock                                 current      = op.BasicBlock;
                Debugging.DebugInfo                        debugInfo    = op.DebugInfo;
                VariableExpression                         exRes        = op.FirstResult;
                Expression                                 exSrc        = op.FirstArgument;
                Expression[]                               resFragments = cfg.GetFragmentsForExpression( exRes );
                Expression[]                               srcFragments = cfg.GetFragmentsForExpression( exSrc );

                CHECKS.ASSERT( resFragments.Length == 1, "Expecting result of conversion to fit in one register: {0}" , op );
                CHECKS.ASSERT( srcFragments.Length == 1, "Expecting argument of conversion to fit in one register: {0}", op );

                VariableExpression newRes = (VariableExpression)resFragments[0];
                Expression         newSrc =                     srcFragments[0];

                ConvertOperator opNew = ConvertOperator.New( debugInfo, op.InputKind, op.OutputKind, op.CheckOverflow, newRes, newSrc );
                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                
                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(SignExtendOperator) )]
        private static void Handle_SignExtendOperator( PhaseExecution.NotificationContext nc )
        {
            SignExtendOperator              op = (SignExtendOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation ts =                     nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                BasicBlock                                 current      = op.BasicBlock;
                Debugging.DebugInfo                        debugInfo    = op.DebugInfo;
                VariableExpression                         exRes        = op.FirstResult;
                Expression                                 exSrc        = op.FirstArgument;
                Expression[]                               resFragments = cfg.GetFragmentsForExpression( exRes );
                Expression[]                               srcFragments = cfg.GetFragmentsForExpression( exSrc );
                uint                                       sizeRes      = exRes.Type.SizeOfHoldingVariableInWords;
                uint                                       sizeSrc      = exSrc.Type.SizeOfHoldingVariableInWords;

                if(sizeRes == 1 && sizeSrc == 1)
                {
                    VariableExpression newRes = (VariableExpression)resFragments[0];
                    Expression         newSrc =                     srcFragments[0];

                    SignExtendOperator opNew = SignExtendOperator.New( debugInfo, op.SignificantSize, false, newRes, newSrc );
                    op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );
                }
                else if(sizeRes == 2 && sizeSrc == 1)
                {
                    VariableExpression newResLo = (VariableExpression)resFragments[0];
                    VariableExpression newResHi = (VariableExpression)resFragments[1];
                    Expression         newSrc   =                     srcFragments[0];

                    if(op.SignificantSize < 4)
                    {
                        op.AddOperatorBefore( SignExtendOperator.New( debugInfo, op.SignificantSize, false, newResLo, newSrc ) );
                    }
                    else
                    {
                        op.AddOperatorBefore( SingleAssignmentOperator.New( debugInfo, newResLo, newSrc ) );
                    }

                    //
                    // Copy the sign bit through the high word.
                    //
                    BinaryOperator opNew = BinaryOperator.New( debugInfo, BinaryOperator.ALU.SHR, true, false, newResHi, newResLo, ts.CreateConstant( 31 ) );
                    op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unsupported inputs for sign extension: {0}", op );
                }
                
                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(ZeroExtendOperator) )]
        private static void Handle_ZeroExtendOperator( PhaseExecution.NotificationContext nc )
        {
            ZeroExtendOperator              op = (ZeroExtendOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation ts =                     nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                BasicBlock                                 current      = op.BasicBlock;
                Debugging.DebugInfo                        debugInfo    = op.DebugInfo;
                VariableExpression                         exRes        = op.FirstResult;
                Expression                                 exSrc        = op.FirstArgument;
                Expression[]                               resFragments = cfg.GetFragmentsForExpression( exRes );
                Expression[]                               srcFragments = cfg.GetFragmentsForExpression( exSrc );
                uint                                       sizeRes      = exRes.Type.SizeOfHoldingVariableInWords;
                uint                                       sizeSrc      = exSrc.Type.SizeOfHoldingVariableInWords;

                if(sizeRes == 1 && sizeSrc == 1)
                {
                    VariableExpression newRes = (VariableExpression)resFragments[0];
                    Expression         newSrc =                     srcFragments[0];

                    ZeroExtendOperator opNew = ZeroExtendOperator.New( debugInfo, op.SignificantSize, false, newRes, newSrc );
                    op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                }
                else if(sizeRes == 2 && sizeSrc == 1)
                {
                    VariableExpression newResLo = (VariableExpression)resFragments[0];
                    VariableExpression newResHi = (VariableExpression)resFragments[1];
                    Expression         newSrc   =                     srcFragments[0];

                    if(op.SignificantSize < 4)
                    {
                        op.AddOperatorBefore( ZeroExtendOperator.New( debugInfo, op.SignificantSize, false, newResLo, newSrc ) );
                    }
                    else
                    {
                        op.AddOperatorBefore( SingleAssignmentOperator.New( debugInfo, newResLo, newSrc ) );
                    }

                    //
                    // Clear high word.
                    //
                    var opNew = SingleAssignmentOperator.New( debugInfo, newResHi, ts.CreateConstant( newResHi.Type, 0 ) );
                    op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unsupported inputs for sign extension: {0}", op );
                }
                
                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(TruncateOperator) )]
        private static void Handle_TruncateOperator( PhaseExecution.NotificationContext nc )
        {
            TruncateOperator                op = (TruncateOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation ts =                   nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                BasicBlock                                 current      = op.BasicBlock;
                Debugging.DebugInfo                        debugInfo    = op.DebugInfo;
                VariableExpression                         exRes        = op.FirstResult;
                Expression                                 exSrc        = op.FirstArgument;
                Expression[]                               resFragments = cfg.GetFragmentsForExpression( exRes );
                Expression[]                               srcFragments = cfg.GetFragmentsForExpression( exSrc );
                uint                                       sizeRes      = exRes.Type.SizeOfHoldingVariableInWords;
                uint                                       sizeSrc      = exSrc.Type.SizeOfHoldingVariableInWords;

                if(sizeRes == 1 && sizeSrc == 1)
                {
                    VariableExpression newRes = (VariableExpression)resFragments[0];
                    Expression         newSrc =                     srcFragments[0];

                    TruncateOperator opNew = TruncateOperator.New( debugInfo, op.SignificantSize, false, newRes, newSrc );
                    op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                }
                else if(sizeRes == 1 && sizeSrc == 2)
                {
                    VariableExpression newRes   = (VariableExpression)resFragments[0];
                    Expression         newSrcLo =                     srcFragments[0];
                    Expression         newSrcHi =                     srcFragments[1];
                    Operator           opNew;

                    if(op.SignificantSize < 4)
                    {
                        opNew = TruncateOperator.New( debugInfo, op.SignificantSize, false, newRes, newSrcLo );
                    }
                    else if(op.SignificantSize == 4)
                    {
                        opNew = SingleAssignmentOperator.New( debugInfo, newRes, newSrcLo );
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Unsupported inputs for truncation: {0}", op );
                    }

                    op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unsupported inputs for truncation: {0}", op );
                }
                
                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(LoadIndirectOperator) )]
        private static void Handle_LoadIndirectOperator( PhaseExecution.NotificationContext nc )
        {
            LoadIndirectOperator            op = (LoadIndirectOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation ts =                       nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                Debugging.DebugInfo                        debugInfo    = op.DebugInfo;
                Expression                                 exSrc        = op.FirstArgument;
                VariableExpression                         exDst        = op.FirstResult;
                TypeRepresentation                         tdLoad       = op.Type;
                FieldRepresentation[]                      accessPath   = op.AccessPath;
                Expression[]                               srcFragments = cfg.GetFragmentsForExpression( exSrc );
                Expression[]                               dstFragments = cfg.GetFragmentsForExpression( exDst );
                int                                        sizeDst      = dstFragments.Length;
                bool                                       fIsVolatile  = op.MayMutateExistingStorage;


                for(int offset = 0; offset < sizeDst; offset++)
                {
                    VariableExpression exDstFragment = (VariableExpression)dstFragments[offset];
                    Expression         exSrcFragment =                     srcFragments[     0];
                    TypeRepresentation tdDstFragment =                     exDstFragment.Type;
                    TypeRepresentation td;
                    
                    if(sizeDst > 1)
                    {
                        td = tdDstFragment;
                    }
                    else if(tdDstFragment == tdLoad)
                    {
                        td = tdDstFragment;
                    }
                    else if(tdDstFragment.SizeOfHoldingVariable <= tdLoad.SizeOfHoldingVariable)
                    {
                        td = tdDstFragment;
                    }
                    else
                    {
                        td = tdLoad;
                    }

                    var fd = tdLoad.FindFieldAtOffset( offset * sizeof(uint) );

                    FieldRepresentation[] newAccessPath;

                    if(fd != null && fd.FieldType == td)
                    {
                        newAccessPath = ArrayUtility.AppendToArray( accessPath, fd );
                    }
                    else
                    {
                        newAccessPath = accessPath;
                    }

                    LoadIndirectOperator opNew = LoadIndirectOperator.New( debugInfo, td, exDstFragment, exSrcFragment, newAccessPath, op.Offset + offset * sizeof(uint), fIsVolatile, false );
                    opNew.CopyAnnotations( op );
                    op.AddOperatorBefore( opNew );
                }

                op.Delete();
                
                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(StoreIndirectOperator) )]
        private static void Handle_StoreIndirectOperator( PhaseExecution.NotificationContext nc )
        {
            StoreIndirectOperator           op = (StoreIndirectOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation ts =                        nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                BasicBlock                                 current      = op.BasicBlock;
                Debugging.DebugInfo                        debugInfo    = op.DebugInfo;
                Expression                                 exSrc        = op.SecondArgument;
                Expression                                 exDst        = op.FirstArgument;
                Expression[]                               srcFragments = cfg.GetFragmentsForExpression( exSrc );
                Expression[]                               dstFragments = cfg.GetFragmentsForExpression( exDst );
                int                                        sizeSrc      = srcFragments.Length;

                for(int offset = 0; offset < sizeSrc; offset++)
                {
                    Expression         exSrcFragment = srcFragments[offset];
                    Expression         exDstFragment = dstFragments[     0];
                    TypeRepresentation td            = sizeSrc > 1 ? exSrcFragment.Type : op.Type;

                    StoreIndirectOperator opNew = StoreIndirectOperator.New( debugInfo, td, exDstFragment, exSrcFragment, op.AccessPath, op.Offset + offset * sizeof(uint), false );
                    opNew.CopyAnnotations( op );
                    op.AddOperatorBefore( opNew );
                }

                op.Delete();
                
                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(BinaryConditionalControlOperator) )]
        private static void Handle_BinaryConditionalControlOperator( PhaseExecution.NotificationContext nc )
        {
            BinaryConditionalControlOperator op = (BinaryConditionalControlOperator)nc.CurrentOperator;

            //--//

            TypeSystemForCodeTransformation            ts           = nc.TypeSystem;
            ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
            Debugging.DebugInfo                        debugInfo    = op.DebugInfo;
            Expression                                 exSrc        = op.FirstArgument;
            Expression[]                               srcFragments = cfg.GetFragmentsForExpression( exSrc );
            TypeRepresentation                         tdSrc        = exSrc.Type;
            uint                                       sizeSrc      = tdSrc.SizeOfHoldingVariableInWords;

            //--//

            ConditionCodeExpression cc = cfg.AllocateConditionCode();

            if(tdSrc.IsFloatingPoint)
            {
                throw TypeConsistencyErrorException.Create( "Unsupported inputs for compare: {0}", op );
            }
            else if(tdSrc.IsNumeric == false || tdSrc.IsInteger)
            {
                if(sizeSrc == 1)
                {
                    op.AddOperatorBefore( CompareOperator.New( debugInfo, cc, srcFragments[0], ts.CreateConstant( 0 ) ) );
                }
                else if(sizeSrc == 2)
                {
                    VariableExpression tmpFragment = cfg.AllocatePseudoRegister( srcFragments[0].Type );

                    var opLow = BinaryOperatorWithCarryOut.New( debugInfo, BinaryOperator.ALU.OR, false, false, tmpFragment, cc, srcFragments[0], srcFragments[1] );
                    op.AddOperatorBefore( opLow );
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Unsupported inputs for compare: {0}", op );
                }
            }
            else
            {
                throw TypeConsistencyErrorException.Create( "Unsupported inputs for compare: {0}", op );
            }

            ConditionCodeConditionalControlOperator opNew = ConditionCodeConditionalControlOperator.New( debugInfo, ConditionCodeExpression.Comparison.NotEqual, cc, op.TargetBranchNotTaken, op.TargetBranchTaken );
            op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(MultiWayConditionalControlOperator) )]
        private static void Handle_MultiWayConditionalControlOperator( PhaseExecution.NotificationContext nc )
        {
            MultiWayConditionalControlOperator op = (MultiWayConditionalControlOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation    ts =                                     nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                Debugging.DebugInfo                        debugInfo    = op.DebugInfo;
                Expression                                 exSrc        = op.FirstArgument;
                Expression[]                               srcFragments = cfg.GetFragmentsForExpression( exSrc );
                uint                                       sizeSrc      = exSrc.Type.SizeOfHoldingVariableInWords;

                //--//

                if(sizeSrc == 1)
                {
                    Expression newSrc = srcFragments[0];

                    MultiWayConditionalControlOperator opNew = MultiWayConditionalControlOperator.New( debugInfo, newSrc, op.TargetBranchNotTaken, op.Targets );
                    op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.CopyAnnotations );

                    nc.MarkAsModified();
                }
                else
                {
                    throw TypeConsistencyErrorException.Create( "Multi-way branches only support 32bit inputs: {0}", op );
                }
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(ReturnControlOperator) )]
        private static void Handle_ReturnControlOperator( PhaseExecution.NotificationContext nc )
        {
            ReturnControlOperator           op = (ReturnControlOperator)nc.CurrentOperator;
            TypeSystemForCodeTransformation ts =                        nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;

                //
                // No need to do anything special here.
                // The code for mapping arguments to words also maps the return value to a set of fragments.
                // As a consequence, the assignments to the return value are already directed to the right spot.
                //
                ReturnControlOperator opNew = ReturnControlOperator.New( cfg.GetFragmentsForExpression( cfg.ReturnValue ) );
                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.PhaseFilter( typeof(Phases.ExpandAggregateTypes) )]
        [CompilationSteps.OperatorHandler( typeof(CallOperator) )]
        private static void Handle_CallOperator( PhaseExecution.NotificationContext nc )
        {
            CallOperator op = (CallOperator)nc.CurrentOperator;

            TypeSystemForCodeTransformation            ts              = nc.TypeSystem;
            ControlFlowGraphStateForCodeTransformation cfg             = nc.CurrentCFG;
            Abstractions.CallingConvention             cc              = ts.CallingConvention;
            Abstractions.CallingConvention.CallState   callState       = cc.CreateCallState( Abstractions.CallingConvention.Direction.Caller );
            BasicBlock                                 current         = op.BasicBlock;
            Debugging.DebugInfo                        debugInfo       = op.DebugInfo;
            VariableExpression                         returnValue     = op.Results.Length == 1 ? op.FirstResult : null;
            Expression[]                               resSrcFragments = cc.AssignReturnValue( cfg, op, returnValue, callState );
            Expression[][]                             srcFragments    = cfg.GetFragmentsForExpressionArray( op.Arguments );
            int                                        offset          = op is IndirectCallOperator ? 1 : 0;
            SingleAssignmentOperator                   opReg           = null;
            Expression[]                               rhs             = Expression.SharedEmptyArray;

            bool fIsImportedMethod = op is StaticCallOperator && 0 != ( ( (StaticCallOperator)op ).TargetMethod.BuildTimeFlags & MethodRepresentation.BuildTimeAttributes.Imported );

            for(int arg = 0; arg + offset < srcFragments.Length; arg++)
            {
                Expression[] argFragments  = srcFragments[arg+offset];
                Expression   dst           = op.Arguments[arg+offset];

                ///
                /// For external calls, we do not want the first argument to be the 'this' pointer.
                ///
                if(arg == 0 && fIsImportedMethod)
                {
                    continue;
                }

                Expression[] dstFragments  = cc.AssignArgument( cfg, op, dst, callState );
                int          requiredWords = argFragments.Length;

                if(arg == 0 && op is StaticCallOperator)
                {
                    //
                    // Don't bother loading the 'this' pointer for static methods.
                    //
                    continue;
                }

                for(int wordOffset = 0; wordOffset < requiredWords; wordOffset++)
                {
                    Expression               argEx =                     argFragments[wordOffset];
                    VariableExpression       dstEx = (VariableExpression)dstFragments[wordOffset];

                    SingleAssignmentOperator opNew = SingleAssignmentOperator.New( debugInfo, dstEx, argEx );

                    if(fIsImportedMethod)
                    {
                        opNew.AddAnnotation( ExternalCallArgumentAnnotation.Create(ts) );
                    }

                    rhs = ArrayUtility.AppendToNotNullArray( rhs, (Expression)dstEx );

                    //
                    // Well, we want to assign first the values that get passed on the stack, then the ones that get passed in registers.
                    // The idea is that, if the result of a previous call is used for a stack value, we could pick it up directly from the volatile registers.
                    //
                    Operator opTarget = op;

                    if(dstEx is PhysicalRegisterExpression)
                    {
                        if(opReg == null)
                        {
                            opReg = opNew;
                        }
                    }
                    else if(opReg != null)
                    {
                        opTarget = opReg;
                    }

                    opTarget.AddOperatorBefore( opNew );
                }
            }

            SubroutineOperator newCall;

            if(op is IndirectCallOperator)
            {
                newCall = IndirectSubroutineOperator.New( debugInfo, op.TargetMethod, srcFragments[0][0], rhs );
            }
            else
            {
                newCall = DirectSubroutineOperator.New( debugInfo, op.TargetMethod, rhs );
            }

            Annotation[] annotations = op.Annotations;
            foreach(Annotation an in annotations)
            {
                if(an is NotNullAnnotation          ||
                   an is FixedLengthArrayAnnotation  )
                {
                }
                else
                {
                    newCall.AddAnnotation( an );
                }
            }

            op.SubstituteWithOperator( newCall, Operator.SubstitutionFlags.Default );

            if(resSrcFragments != null)
            {
                Expression[] resDstFragments = cfg.GetFragmentsForExpression( returnValue );

                for(int wordOffset = 0; wordOffset < resDstFragments.Length; wordOffset++)
                {
                    var opNew = SingleAssignmentOperator.New( debugInfo, (VariableExpression)resDstFragments[wordOffset], resSrcFragments[wordOffset] );

                    foreach(Annotation an in annotations)
                    {
                        if(an is NotNullAnnotation          ||
                           an is FixedLengthArrayAnnotation  )
                        {
                            opNew.AddAnnotation( an );
                        }
                    }

                    newCall.AddOperatorAfter( opNew );
                }
            }

            if(newCall.GetNextOperator() is DeadControlOperator)
            {
                //
                // No need to add side effects after a dead call.
                //
            }
            else
            {
                foreach(VariableExpression exToInvalidate in cc.CollectExpressionsToInvalidate( cfg, op, resSrcFragments ))
                {
                    newCall.AddAnnotation( PostInvalidationAnnotation.Create( ts, exToInvalidate ) );
                }
            }

            nc.MarkAsModified();
        }
    }
}
