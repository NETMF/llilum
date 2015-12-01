//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DUMP_CONSTRAINTSYSTEM

// LLVM translation doesn't yet recognize SSA format, and will not produce correct code.
// TODO: Remove this tag when that issue has been resolved.
//#define ALLOW_SSA_FORM

// Enable optimizations that only work for direct ARM translation (non-LLVM legacy path).
// TODO: Remove this tag when comparison is no longer needed.
//#define ENABLE_LOW_LEVEL_OPTIMIZATIONS

namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Handlers
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class Optimizations
    {
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

#if ALLOW_SSA_FORM
        [CompilationSteps.OptimizationHandler(RunOnce = true, RunInExtendedSSAForm = true)]
#else // ALLOW_SSA_FORM
        [CompilationSteps.OptimizationHandler(RunOnce = true)]
#endif // ALLOW_SSA_FORM
        private static void RemoveRedundantChecks( PhaseExecution.NotificationContext nc )
        {
            ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;

            while(true)
            {
                GrowOnlyHashTable< VariableExpression, Operator > defLookup = cfg.DataFlow_SingleDefinitionLookup;

                if(PropagateFixedArrayLength( cfg, defLookup ) ||
                   RemoveRedundantChecks( cfg, defLookup ) ||
                   RemoveRedundantBoundChecks( cfg, defLookup ) )
                {
                    Transformations.RemoveDeadCode.Execute( cfg, false );
                    continue;
                }

                break;
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        private static bool PropagateFixedArrayLength( ControlFlowGraphStateForCodeTransformation        cfg       ,
                                                       GrowOnlyHashTable< VariableExpression, Operator > defLookup )
        {
            bool fRunSimplify = false;

#if ENABLE_LOW_LEVEL_OPTIMIZATIONS
            foreach(var op in cfg.FilterOperators< LoadIndirectOperator >())
#else // ENABLE_LOW_LEVEL_OPTIMIZATIONS
            foreach(var op in cfg.FilterOperators< LoadInstanceFieldOperator >())
#endif // ENABLE_LOW_LEVEL_OPTIMIZATIONS
            {
                if(op.HasAnnotation< ArrayLengthAnnotation >())
                {
                    FixedLengthArrayAnnotation an = FindFixedLengthArrayAnnotation( defLookup, op.FirstArgument );
                    if(an != null)
                    {
                        var var   = op.FirstResult;
                        var opNew = SingleAssignmentOperator.New( op.DebugInfo, var, cfg.TypeSystem.CreateConstant( (uint)an.ArrayLength ) );

                        op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );

                        defLookup[var] = op;
                        fRunSimplify = true;
                    }
                }
            }

            return fRunSimplify;
        }

        private static FixedLengthArrayAnnotation FindFixedLengthArrayAnnotation( GrowOnlyHashTable< VariableExpression, Operator > defLookup ,
                                                                                  Expression                                        ex        )
        {
            return FindFixedLengthArrayAnnotation( defLookup, SetFactory.NewWithReferenceEquality< VariableExpression >(), ex );
        }

        private static FixedLengthArrayAnnotation FindFixedLengthArrayAnnotation( GrowOnlyHashTable< VariableExpression, Operator > defLookup ,
                                                                                  GrowOnlySet< VariableExpression >                 history   ,
                                                                                  Expression                                        ex        )
        {
            var array = ex as VariableExpression;

            if(array == null || history.Insert( array ))
            {
                //
                // Detected loop, bail out.
                //
                return null;
            }

            //--//

            Operator arrayDef;

            if(defLookup.TryGetValue( array, out arrayDef ))
            {
                var an = arrayDef.GetAnnotation< FixedLengthArrayAnnotation >();
                if(an != null)
                {
                    return an;
                }

                if(arrayDef is PhiOperator)
                {
                    foreach(Expression rhs in arrayDef.Arguments)
                    {
                        FixedLengthArrayAnnotation an2 = FindFixedLengthArrayAnnotation( defLookup, history, rhs );

                        if(an2 == null)
                        {
                            return null;
                        }

                        if(an == null)
                        {
                            an = an2;
                        }
                        else if(an.ArrayLength != an2.ArrayLength)
                        {
                            return null;
                        }
                    }

                    return an;
                }

                if(arrayDef is SingleAssignmentOperator ||
                   arrayDef is PiOperator                )
                {
                    return FindFixedLengthArrayAnnotation( defLookup, history, arrayDef.FirstArgument );
                }
            }

            return null;
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        private static bool RemoveRedundantChecks(
            ControlFlowGraphStateForCodeTransformation cfg,
            GrowOnlyHashTable< VariableExpression, Operator > defLookup )
        {
            var useChains = cfg.DataFlow_UseChains;
            bool fRunSimplify = false;

            // Search for branch-if-non-zero.
            foreach(var opCtrl in cfg.FilterOperators< BinaryConditionalControlOperator >())
            {
                BasicBlock takenBranch;

                if( opCtrl.TargetBranchTaken == opCtrl.TargetBranchNotTaken )
                {
                    // Replace binary branches with unconditional ones when both targets are the same block.
                    takenBranch = opCtrl.TargetBranchTaken;
                }
                else
                {
                    switch(ProveIsNull( opCtrl.FirstArgument, defLookup, useChains ))
                    {
                    case ProveNullResult.NeverNull:
                        takenBranch = opCtrl.TargetBranchTaken;
                        break;

                    case ProveNullResult.AlwaysNull:
                        takenBranch = opCtrl.TargetBranchNotTaken;
                        break;

                    default:
                        continue;
                    }
                }

                UnconditionalControlOperator opNewCtrl = UnconditionalControlOperator.New( opCtrl.DebugInfo, takenBranch );
                opCtrl.SubstituteWithOperator( opNewCtrl, Operator.SubstitutionFlags.Default );

                fRunSimplify = true;
            }

            // Search for branch if (a == null) and branch if (a != null).
            foreach(var opCtrl in cfg.FilterOperators< CompareConditionalControlOperator >())
            {
                BasicBlock takenBranch;

                if( opCtrl.TargetBranchTaken == opCtrl.TargetBranchNotTaken )
                {
                    // Replace binary branches with unconditional ones when both targets are the same block.
                    takenBranch = opCtrl.TargetBranchTaken;
                }
                else
                {
                    bool trueIfNull = false;
                    Expression expr;

                    switch (opCtrl.Condition)
                    {
                    case CompareAndSetOperator.ActionCondition.EQ:
                        trueIfNull = true;
                        break;

                    case CompareAndSetOperator.ActionCondition.NE:
                        trueIfNull = false;
                        break;

                    default:
                        continue;
                    }

                    bool fNullOnRight;
                    expr = opCtrl.IsBinaryOperationAgainstZeroValue( out fNullOnRight );
                    if(expr == null)
                    {
                        continue;
                    }

                    ProveNullResult result = ProveIsNull( expr, defLookup, useChains );
                    if(result == ProveNullResult.Unknown)
                    {
                        continue;
                    }

                    switch(result)
                    {
                    case ProveNullResult.NeverNull:
                        takenBranch = trueIfNull ? opCtrl.TargetBranchNotTaken : opCtrl.TargetBranchTaken;
                        break;

                    case ProveNullResult.AlwaysNull:
                        takenBranch = trueIfNull ? opCtrl.TargetBranchTaken : opCtrl.TargetBranchNotTaken;
                        break;

                    default:
                        continue;
                    }
                }

                UnconditionalControlOperator opNewCtrl = UnconditionalControlOperator.New( opCtrl.DebugInfo, takenBranch );
                opCtrl.SubstituteWithOperator( opNewCtrl, Operator.SubstitutionFlags.Default );

                fRunSimplify = true;
            }

#if ENABLE_LOW_LEVEL_OPTIMIZATIONS
            foreach(var opCtrl in cfg.FilterOperators< ConditionCodeConditionalControlOperator >())
            {
                bool fNotEqual;

                switch(opCtrl.Condition)
                {
                    case ConditionCodeExpression.Comparison.NotEqual:
                        fNotEqual = true;
                        break;

                    case ConditionCodeExpression.Comparison.Equal:
                        fNotEqual = false;
                        break;

                    default:
                        continue;
                }

                var opCmp = ControlFlowGraphState.CheckSingleDefinition( defLookup, opCtrl.FirstArgument ) as CompareOperator;
                if(opCmp != null)
                {
                    bool fNullOnRight;
                    Expression ex = opCmp.IsBinaryOperationAgainstZeroValue( out fNullOnRight );

                    if(ex != null)
                    {
                        if(ProveNotNull( defLookup, ex ) == ProveNotNullResult.True)
                        {
                            UnconditionalControlOperator opNewCtrl = UnconditionalControlOperator.New( opCtrl.DebugInfo, fNotEqual ? opCtrl.TargetBranchTaken : opCtrl.TargetBranchNotTaken );
                            opCtrl.SubstituteWithOperator( opNewCtrl, Operator.SubstitutionFlags.Default );

                            fRunSimplify = true;
                        }
                    }
                }
            }
#endif // ENABLE_LOW_LEVEL_OPTIMIZATIONS

            return fRunSimplify;
        }

        private enum ProveNullResult
        {
            Unknown = 0,
            NeverNull,
            AlwaysNull,
        }

        private static ProveNullResult ProveIsNull(
            Expression ex,
            GrowOnlyHashTable< VariableExpression, Operator > defLookup,
            Operator[][] useChains )
        {
            return ProveIsNull( ex, defLookup, useChains, SetFactory.NewWithReferenceEquality< VariableExpression >() );
        }

        private static ProveNullResult ProveIsNull(
            Expression ex,
            GrowOnlyHashTable< VariableExpression, Operator > defLookup,
            Operator[][] useChains,
            GrowOnlySet< VariableExpression > history )
        {
            if(ex is ConstantExpression)
            {
                var exConst = (ConstantExpression)ex;

                if(exConst.IsEqualToZero())
                {
                    return ProveNullResult.AlwaysNull;
                }

                // Data descriptors will always resolve to pointers to a global object, and therefore can't be null.
                if(exConst.Value is DataManager.DataDescriptor)
                {
                    return ProveNullResult.NeverNull;
                }

                return ProveNullResult.NeverNull;
            }

            var exVar = ex as VariableExpression;

            // If we detected a loop, the value may change between iterations.
            if(exVar == null || history.Insert( exVar ) == true)
            {
                return ProveNullResult.Unknown;
            }

            //--//

            Operator def;

            if(defLookup.TryGetValue( exVar, out def ))
            {
                // If we ever take the address of the expression, assume it can be modified.
                foreach (Operator op in useChains[exVar.SpanningTreeIndex])
                {
                    if (op is AddressAssignmentOperator)
                    {
                        return ProveNullResult.Unknown;
                    }
                }

                if (def.HasAnnotation< NotNullAnnotation >())
                {
                    return ProveNullResult.NeverNull;
                }

                // Phi: If all predecessors are provably the same result, then so is the operator's result.
                if(def is PhiOperator)
                {
                    bool isFirstResult = true;
                    ProveNullResult result = ProveNullResult.Unknown;

                    foreach(Expression rhs in def.Arguments)
                    {
                        ProveNullResult curResult = ProveIsNull( rhs, defLookup, useChains, history );
                        if(isFirstResult)
                        {
                            isFirstResult = false;
                            result = curResult;
                        }
                        else if(curResult != result)
                        {
                            // We got two different results from different blocks.
                            return ProveNullResult.Unknown;
                        }
                    }

                    return result;
                }

                // Assignment is transitive, so look up the source value.
                if(def is SingleAssignmentOperator)
                {
                    return ProveIsNull( def.FirstArgument, defLookup, useChains, history );
                }

#if FUTURE // This block isn't strictly correct and assumes low-level knowledge. We should revisit it.
                if(def is BinaryOperator)
                {
                    var binOp = (BinaryOperator)def;

                    ConstantExpression exConst;
                    bool               fConstOnRight;
 
                    var varSrc = binOp.IsBinaryOperationAgainstConstant( out exConst, out fConstOnRight );
                    if(varSrc != null)
                    {
                        //
                        // Only apply the optimization to pointer-like variables.
                        //
                        if(varSrc.CanPointToMemory || exVar.CanPointToMemory)
                        {
                            switch(binOp.Alu)
                            {
                                case BinaryOperator.ALU.ADD:
                                case BinaryOperator.ALU.SUB:
                                    //
                                    // Adding/removing a constant from a non-null value doesn't change its nullness.
                                    //
                                    return ProveIsNull( defLookup, history, varSrc );

                                case BinaryOperator.ALU.OR:
                                    //
                                    // OR'ing with a non-zero constant ensures the results are not null.
                                    //
                                    if(!exConst.IsEqualToZero())
                                    {
                                        return ProveNullResult.NeverNull;
                                    }
                                    break;
                            }
                        }
                    }

                    return ProveNullResult.Unknown;
                }
#endif // FUTURE

                if(def is PiOperator)
                {
                    var piOp = (PiOperator)def;

                    ex = def.FirstArgument;

                    if((piOp.LeftExpression  == ex && piOp.RightExpression.IsEqualToZero()) ||
                       (piOp.RightExpression == ex && piOp.LeftExpression .IsEqualToZero())  )
                    {
                        switch(piOp.RelationOperator)
                        {
                            case PiOperator.Relation.Equal:
                                return ProveNullResult.AlwaysNull;

                            case PiOperator.Relation.NotEqual:
                                return ProveNullResult.NeverNull;
                        }
                    }

                    return ProveIsNull( ex, defLookup, useChains, history );
                }
            }

            return ProveNullResult.Unknown;
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        private static bool RemoveRedundantBoundChecks( ControlFlowGraphStateForCodeTransformation        cfg       ,
                                                        GrowOnlyHashTable< VariableExpression, Operator > defLookup )
        {
            bool fRunSimplify = false;

#if DUMP_CONSTRAINTSYSTEM
////        if(cfg.ToString() != "FlowGraph(int Microsoft.Zelig.Runtime.Helpers.BinaryOperations::IntDiv(int,int))")
////        if(cfg.ToString() != "FlowGraph(uint Microsoft.Zelig.Runtime.Helpers.BinaryOperations::UintDiv(uint,uint))")
////        if(cfg.ToString() != "FlowGraph(bool Microsoft.Zelig.Runtime.LinearMemoryManager::RefersToMemory(System.UIntPtr))")
            if(cfg.ToString() != "FlowGraph(int[] Microsoft.NohauLPC3180Loader.Loader::TestArrayBoundChecks(int[],int,int))")
////        if(cfg.ToString() != "FlowGraph(uint Microsoft.NohauLPC3180Loader.Loader::Sqrt(uint))")
////        if(cfg.ToString() != "FlowGraph(void Microsoft.NohauLPC3180Loader.Loader::QuickSort(int[],object[],int,int))")
////        if(cfg.ToString() != "FlowGraph(void System.Collections.Generic.ArraySortHelper`1<System.Int32>::QuickSort<object>(int[],object[],int,int,System.Collections.Generic.IComparer`1<int>))")
            {
                return false;
            }
#endif

            var cscUpperBound = new Transformations.ConstraintSystemCollector( cfg, Transformations.ConstraintSystemCollector.Kind.LessThanOrEqual    );
            var cscLowerBound = new Transformations.ConstraintSystemCollector( cfg, Transformations.ConstraintSystemCollector.Kind.GreaterThanOrEqual );

#if DUMP_CONSTRAINTSYSTEM
            System.IO.TextWriter orig   = Console.Out;
            System.IO.TextWriter writer = new System.IO.StreamWriter( "constraintsystem.txt", false, System.Text.Encoding.ASCII );
            Console.SetOut( writer );
 
            Console.WriteLine( "#############################################################################" );
            Console.WriteLine( "#############################################################################" );

            cfg.Dump();

            Console.WriteLine( "#############################################################################" );
            cscUpperBound.Dump();
            Console.WriteLine( "#############################################################################" );
            cscLowerBound.Dump();
            Console.WriteLine( "#############################################################################" );
#endif

            // TODO: This doesn't work on LLVM and needs to be revisited.
            foreach(var opCtrl in cfg.FilterOperators< ConditionCodeConditionalControlOperator >())
            {
                var opCmp = ControlFlowGraphState.CheckSingleDefinition( defLookup, opCtrl.FirstArgument ) as CompareOperator;

                if(opCmp != null)
                {
                    bool fTaken;

#if DUMP_CONSTRAINTSYSTEM
                    Console.WriteLine( "#############################################################################" );
                    Console.WriteLine( "#############################################################################" );
#endif

                    if(ProveComparison( cscUpperBound, cscLowerBound, opCmp.FirstArgument, opCmp.SecondArgument, opCtrl.Condition, out fTaken ))
                    {
                        UnconditionalControlOperator opNewCtrl = UnconditionalControlOperator.New( opCtrl.DebugInfo, fTaken ? opCtrl.TargetBranchTaken : opCtrl.TargetBranchNotTaken );
                        opCtrl.SubstituteWithOperator( opNewCtrl, Operator.SubstitutionFlags.Default );

                        fRunSimplify = true;
                    }
                }
            }

#if DUMP_CONSTRAINTSYSTEM
            Console.WriteLine( "#############################################################################" );

            Console.SetOut( orig );
            writer.Close();
#endif

            return fRunSimplify;
        }

        private static bool ProveComparison(     Transformations.ConstraintSystemCollector cscUpperBound ,
                                                 Transformations.ConstraintSystemCollector cscLowerBound ,
                                                 Expression                                exLeft        ,
                                                 Expression                                exRight       ,
                                                 ConditionCodeExpression.Comparison        condition     ,
                                             out bool                                      fTaken        )
        {
            switch(condition)
            {
                case ConditionCodeExpression.Comparison.UnsignedLowerThan:
                case ConditionCodeExpression.Comparison.SignedLessThan:
                    return ProveLessThanOrEqual( cscUpperBound, cscLowerBound, exLeft, exRight, -1, out fTaken );

                case ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame:
                case ConditionCodeExpression.Comparison.SignedLessThanOrEqual:
                    return ProveLessThanOrEqual( cscUpperBound, cscLowerBound, exLeft, exRight, 0, out fTaken );

                case ConditionCodeExpression.Comparison.UnsignedHigherThan:
                case ConditionCodeExpression.Comparison.SignedGreaterThan:
                    return ProveLessThanOrEqual( cscUpperBound, cscLowerBound, exRight, exLeft, -1, out fTaken );

                case ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame:
                case ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual:
                    return ProveLessThanOrEqual( cscUpperBound, cscLowerBound, exRight, exLeft, 0, out fTaken );
            }

            fTaken = false;
            return false;
        }

        private static bool ProveLessThanOrEqual(     Transformations.ConstraintSystemCollector cscUpperBound ,
                                                      Transformations.ConstraintSystemCollector cscLowerBound ,
                                                      Expression                                exLeft        ,
                                                      Expression                                exRight       ,
                                                      double                                    weight        ,
                                                  out bool                                      fTaken        )
        {
            if(exLeft is VariableExpression)
            {
                if(cscUpperBound.Prove( exLeft, exRight, weight ))
                {
                    fTaken = true;
                    return true;
                }
            }

            if(exRight is VariableExpression)
            {
                if(cscLowerBound.Prove( exRight, exLeft, -weight ))
                {
                    fTaken = true;
                    return true;
                }
            }

            fTaken = false;
            return false;
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

#if ENABLE_LOW_LEVEL_OPTIMIZATIONS
        [CompilationSteps.OptimizationHandler(RunOnce=true, RunInSSAForm=true)]
#endif // ENABLE_LOW_LEVEL_OPTIMIZATIONS
        private static void ConvertLongCompareToNormalCompare( PhaseExecution.NotificationContext nc )
        {
            ControlFlowGraphStateForCodeTransformation cfg       = nc.CurrentCFG;
            Operator[][]                               useChains = cfg.DataFlow_UseChains;

            foreach(var opLong in cfg.FilterOperators< LongCompareOperator >())
            {
                Debugging.DebugInfo debugInfo = opLong.DebugInfo;
                VariableExpression  lhs       = opLong.FirstResult;
                Expression[]        rhs       = opLong.Arguments;
                Operator[]          uses      = useChains[lhs.SpanningTreeIndex];
                bool                fOk       = false;

                switch(uses.Length)
                {
                    case 0:
                        fOk = true;
                        break;

                    case 1:
                        {
                            bool                               fNullOnRight;
                            Operator                           use    = uses[0];
                            Expression[]                       target = opLong.IsCompareToNull( out fNullOnRight );
                            ConditionCodeExpression.Comparison cond   = ConditionCodeExpression.Comparison.NotValid;

                            if(use is ConditionalCompareOperator)
                            {
                                var ccOp = (ConditionalCompareOperator)use;
                                
                                cond = ccOp.Condition;
                            }
                            else if(use is ConditionCodeConditionalControlOperator)
                            {
                                var ccOp = (ConditionCodeConditionalControlOperator)use;
                                
                                cond = ccOp.Condition;
                            }
                            else if(use is SetIfConditionIsTrueOperator)
                            {
                                var setOp = (SetIfConditionIsTrueOperator)use;
                                
                                cond  = setOp.Condition;
                            }

                            if(cond != ConditionCodeExpression.Comparison.NotValid)
                            {
                                ConditionCodeExpression.Comparison condOrig   = cond;
                                Expression                         rhsLeftLo  = rhs[0];
                                Expression                         rhsLeftHi  = rhs[1];
                                Expression                         rhsRightLo = rhs[2];
                                Expression                         rhsRightHi = rhs[3];

                                if(target != null)
                                {
                                    if(!fNullOnRight)
                                    {
                                        rhsLeftLo  = rhs[2];
                                        rhsLeftHi  = rhs[3];
                                        rhsRightLo = rhs[0];
                                        rhsRightHi = rhs[1];

                                        cond = ConditionCodeExpression.NegateCondition( cond );
                                    }

                                    //
                                    // We are comparing against zero.
                                    //
                                    // Can we use a single instruction to perform the comparison?
                                    //
                                    // For Zero/NonZero, we just OR together the two halves, and set the condition code.
                                    //
                                    // For Negative/PositiveOrZero, we just need to look at the high part.
                                    //
                                    // For unsigned:
                                    //                  >= is always true.
                                    //                  <  is always false.
                                    //                  <= is like Equal.
                                    //                  >  is like NotEqual.
                                    //
                                    //
                                    // For signed:
                                    //                  >= is like PositiveOrZero.
                                    //                  <  is like Negative.
                                    //                  <= requires full comparison.
                                    //                  >  requires full comparison.
                                    //
                                    switch(cond)
                                    {
                                        case ConditionCodeExpression.Comparison.Equal   :
                                        case ConditionCodeExpression.Comparison.NotEqual:
                                            {
                                                VariableExpression tmp = cfg.AllocatePseudoRegister( rhsLeftLo.Type );

                                                var op2 = BinaryOperatorWithCarryOut.New( debugInfo, BinaryOperator.ALU.OR, false, false, tmp, lhs, rhsLeftLo, rhsLeftHi );
                                                opLong.SubstituteWithOperator( op2, Operator.SubstitutionFlags.Default );
                                            }
                                            fOk = true;
                                            break;

                                        case ConditionCodeExpression.Comparison.Negative                :
                                        case ConditionCodeExpression.Comparison.PositiveOrZero          :
                                            {
                                                CompareOperator op2 = CompareOperator.New( debugInfo, lhs, rhsLeftHi, nc.TypeSystem.CreateConstant( rhsLeftHi.Type, 0 ) );
                                                opLong.AddOperatorBefore( op2 );
                                                opLong.Delete();
                                            }
                                            fOk = true;
                                            break;

                                            //--//

                                        case ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame:
                                            cond = ConditionCodeExpression.Comparison.Equal;
                                            goto case ConditionCodeExpression.Comparison.Equal;

                                        case ConditionCodeExpression.Comparison.UnsignedHigherThan:
                                            cond = ConditionCodeExpression.Comparison.NotEqual;
                                            goto case ConditionCodeExpression.Comparison.NotEqual;

                                        case ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame:
                                            // Always true.
                                            goto case ConditionCodeExpression.Comparison.PositiveOrZero;

                                        case ConditionCodeExpression.Comparison.UnsignedLowerThan:
                                            // Always false.
                                            goto case ConditionCodeExpression.Comparison.PositiveOrZero;

                                            //--//

                                        case ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual:
                                            cond = ConditionCodeExpression.Comparison.PositiveOrZero;
                                            goto case ConditionCodeExpression.Comparison.PositiveOrZero;

                                        case ConditionCodeExpression.Comparison.SignedLessThan:
                                            cond = ConditionCodeExpression.Comparison.Negative;
                                            goto case ConditionCodeExpression.Comparison.Negative;

                                        case ConditionCodeExpression.Comparison.SignedGreaterThan:
                                        case ConditionCodeExpression.Comparison.SignedLessThanOrEqual:
                                            //
                                            // We have to use the full 64bit comparison.
                                            //
                                            break;
                                    }
                                }

                                if(fOk == false)
                                {
                                    //
                                    // We are comparing two variables.
                                    //
                                    // For Zero/NonZero and all the unsigned comparisons,
                                    // first compare the high parts and then compare the low ones, if the high parts are equal.
                                    //
                                    // For signed:
                                    //                  >= do a subtraction and set the result if SignedGreaterThanOrEqual.
                                    //                  <  do a subtraction and set the result if SignedLessThan.
                                    //                  <= is like >= with the arguments swapped.
                                    //                  >  is like < with the arguments swapped.
                                    //
                                    switch(cond)
                                    {
                                        case ConditionCodeExpression.Comparison.Equal                   :
                                        case ConditionCodeExpression.Comparison.NotEqual                :

                                        case ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame:
                                        case ConditionCodeExpression.Comparison.UnsignedLowerThan       :
                                        case ConditionCodeExpression.Comparison.UnsignedHigherThan      :
                                        case ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame :
                                            {
                                                CompareOperator opHigh = CompareOperator.New( debugInfo, lhs, rhsLeftHi, rhsRightHi );
                                                opLong.AddOperatorBefore( opHigh );

                                                ConditionalCompareOperator opLow = ConditionalCompareOperator.New( debugInfo, ConditionCodeExpression.Comparison.Equal, lhs, rhsLeftLo, rhsRightLo, lhs );
                                                opLong.AddOperatorBefore( opLow );
                                                opLong.Delete();
                                            }
                                            fOk = true;
                                            break;

                                        case ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual:
                                            {
                                                VariableExpression tmpLo = cfg.AllocatePseudoRegister( rhsLeftLo.Type );
                                                VariableExpression tmpHi = cfg.AllocatePseudoRegister( rhsLeftHi.Type );

                                                var opLow = BinaryOperatorWithCarryOut.New( debugInfo, BinaryOperator.ALU.SUB, true, false, tmpLo, lhs, rhsLeftLo, rhsRightLo );
                                                opLong.AddOperatorBefore( opLow );

                                                var opHigh = BinaryOperatorWithCarryInAndOut.New( debugInfo, BinaryOperator.ALU.SUB, true, false, tmpHi, lhs, rhsLeftHi, rhsRightHi, lhs );
                                                opLong.AddOperatorBefore( opHigh );

                                                opLong.Delete();

                                                cond = ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual;
                                            }
                                            fOk = true;
                                            break;

                                        case ConditionCodeExpression.Comparison.SignedLessThan:
                                            {
                                                VariableExpression tmpLo = cfg.AllocatePseudoRegister( rhsLeftLo.Type );
                                                VariableExpression tmpHi = cfg.AllocatePseudoRegister( rhsLeftHi.Type );

                                                var opLow = BinaryOperatorWithCarryOut.New( debugInfo, BinaryOperator.ALU.SUB, true, false, tmpLo, lhs, rhsLeftLo, rhsRightLo );
                                                opLong.AddOperatorBefore( opLow );

                                                var opHigh = BinaryOperatorWithCarryInAndOut.New( debugInfo, BinaryOperator.ALU.SUB, true, false, tmpHi, lhs, rhsLeftHi, rhsRightHi, lhs );
                                                opLong.AddOperatorBefore( opHigh );

                                                opLong.Delete();

                                                cond = ConditionCodeExpression.Comparison.SignedLessThan;
                                            }
                                            fOk = true;
                                            break;

                                        case ConditionCodeExpression.Comparison.SignedGreaterThan:
                                            {
                                                Expression tmpLo;
                                                Expression tmpHi;

                                                tmpLo      = rhsLeftLo ;
                                                tmpHi      = rhsLeftHi ;
                                                rhsLeftLo  = rhsRightLo;
                                                rhsLeftHi  = rhsRightHi;
                                                rhsRightLo = tmpLo     ;
                                                rhsRightHi = tmpHi     ;

                                                goto case ConditionCodeExpression.Comparison.SignedLessThan;
                                            }

                                        case ConditionCodeExpression.Comparison.SignedLessThanOrEqual:
                                            {
                                                Expression tmpLo;
                                                Expression tmpHi;

                                                tmpLo      = rhsLeftLo ;
                                                tmpHi      = rhsLeftHi ;
                                                rhsLeftLo  = rhsRightLo;
                                                rhsLeftHi  = rhsRightHi;
                                                rhsRightLo = tmpLo     ;
                                                rhsRightHi = tmpHi     ;

                                                goto case ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual;
                                            }
                                    }
                                }

                                if(condOrig != cond)
                                {
                                    if(use is ConditionalCompareOperator)
                                    {
                                        var ccOp = (ConditionalCompareOperator)use;
                                        
                                        ccOp.Condition = cond;
                                    }
                                    else if(use is ConditionCodeConditionalControlOperator)
                                    {
                                        var ccOp = (ConditionCodeConditionalControlOperator)use;
                                        
                                        ccOp.Condition = cond;
                                    }
                                    else if(use is SetIfConditionIsTrueOperator)
                                    {
                                        var setOp = (SetIfConditionIsTrueOperator)use;
                                        
                                        setOp.Condition = cond;
                                    }
                                }
                            }
                        }
                        break;

                    default:
                        throw TypeConsistencyErrorException.Create( "Unexpected form of '{0}' with multiple uses for the condition code", opLong );
                }

                if(fOk == false)
                {
                    throw TypeConsistencyErrorException.Create( "Unexpected form of '{0}'", opLong );
                }
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

#if ENABLE_SSA_FORM
        [CompilationSteps.OptimizationHandler(RunOnce = true, RunInSSAForm = true)]
#else // ENABLE_SSA_FORM
        [CompilationSteps.OptimizationHandler(RunOnce = true)]
#endif // ENABLE_SSA_FORM
        private static void RemoveRedundantConversions( PhaseExecution.NotificationContext nc )
        {
            ControlFlowGraphStateForCodeTransformation cfg       = nc.CurrentCFG;
            Operator[][]                               defChains = cfg.DataFlow_DefinitionChains;
            Operator[][]                               useChains = cfg.DataFlow_UseChains;

            foreach(var opConv in cfg.FilterOperators< ConversionOperator >())
            {
                var lhs = opConv.FirstResult;
                var rhs = opConv.FirstArgument;

                if(rhs is VariableExpression)
                {
                    var  def         = ControlFlowGraphState.CheckSingleDefinition( defChains, (VariableExpression)rhs );
                    bool fSubstitute = false;

                    var defLoad = def as LoadIndirectOperator;
                    if(defLoad != null)
                    {
                        TypeRepresentation tdSrc = defLoad.Type;

                        if(tdSrc.SizeOfHoldingVariable == opConv.SignificantSize)
                        {
                            if((opConv is ZeroExtendOperator && tdSrc.IsSigned == false) ||
                               (opConv is SignExtendOperator && tdSrc.IsSigned == true )  )
                            {
                                fSubstitute = true;
                            }
                        }
                    }
                    else if(def is BinaryOperator)
                    {
                        var defBinOp = (BinaryOperator)def;

                        switch(defBinOp.Alu)
                        {
                            case BinaryOperator.ALU.AND:
                                defBinOp.EnsureConstantToTheRight();

                                var   ex = defBinOp.SecondArgument as ConstantExpression;
                                ulong val;

                                if(ex != null && ex.GetAsUnsignedInteger( out val ) && val != 0)
                                {
                                    int  msb = BitVector.GetPositionOfLastBitSet( val );
                                    uint max = opConv.SignificantSize * 8;

                                    //
                                    // Masking by a constant smaller than the destination size.
                                    //
                                    if((opConv is ZeroExtendOperator && (msb < max    )) ||
                                       (opConv is SignExtendOperator && (msb < max - 1))  )
                                    {
                                        fSubstitute = true;
                                    }
                                }
                                break;
                        }
                    }

                    if(fSubstitute)
                    {
                        foreach(Operator use in useChains[lhs.SpanningTreeIndex])
                        {
                            use.SubstituteUsage( lhs, rhs );
                        }
                    }
                }

                foreach(Operator use in useChains[lhs.SpanningTreeIndex])
                {
                    var useStore = use as StoreIndirectOperator;
                    if(useStore != null)
                    {
                        if(opConv.SignificantSize >= useStore.Type.SizeOfHoldingVariable)
                        {
                            use.SubstituteUsage( lhs, rhs );
                        }
                    }
                }
            }
        }

#if ENABLE_LOW_LEVEL_OPTIMIZATIONS
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.OptimizationHandler(RunOnce = false, RunInSSAForm = true)]
        private static void ConstantMemoryDereference( PhaseExecution.NotificationContext nc )
        {
            ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;
            TypeSystemForCodeTransformation            ts  = nc.TypeSystem;

            foreach(var opLoad in cfg.FilterOperators< LoadIndirectOperator >())
            {
                var debugInfo = opLoad.DebugInfo;
                var lhs       = opLoad.FirstResult;
                var rhs       = opLoad.FirstArgument as ConstantExpression;

                if(rhs != null)
                {
                    var dd = rhs.Value as DataManager.DataDescriptor;

                    if(dd != null)
                    {
                        var val2 = dd.GetDataAtOffset( opLoad.AccessPath, 0, opLoad.Offset );

                        var ddVal = val2 as DataManager.DataDescriptor;
                        if(ddVal != null && ddVal.CanPropagate)
                        {
                            opLoad.SubstituteWithOperator( SingleAssignmentOperator.New( debugInfo, lhs, ts.CreateConstant( lhs.Type, val2 ) ), Operator.SubstitutionFlags.Default );
                            continue;
                        }
                    }
                }
            }
        }
#endif // ENABLE_LOW_LEVEL_OPTIMIZATIONS
    }
}
