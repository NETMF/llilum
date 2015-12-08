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

// Enable more advanced optimizations which look for non-equivalent comparisons. This appears to be
// overly aggressive when not in SSA form, resulting in incorrect code.
// TODO: Remove this tag when this issue is resolved.
//#define ENABLE_ADVANCED_COMPARES

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
                   RemoveRedundantChecks( cfg, defLookup ) )
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

            foreach(var controlOp in cfg.FilterOperators< BinaryConditionalControlOperator >())
            {
                BasicBlock takenBranch = TryGetUnconditionalBranchTarget( controlOp, defLookup, useChains );
                if(takenBranch != null)
                {
                    UnconditionalControlOperator opNewCtrl = UnconditionalControlOperator.New( controlOp.DebugInfo, takenBranch );
                    controlOp.SubstituteWithOperator( opNewCtrl, Operator.SubstitutionFlags.Default );
                    fRunSimplify = true;
                }
            }

            var cscUpperBound = new Transformations.ConstraintSystemCollector( cfg, Transformations.ConstraintSystemCollector.Kind.LessThanOrEqual    );
            var cscLowerBound = new Transformations.ConstraintSystemCollector( cfg, Transformations.ConstraintSystemCollector.Kind.GreaterThanOrEqual );

            foreach(var controlOp in cfg.FilterOperators< CompareConditionalControlOperator >())
            {
                BasicBlock takenBranch = TryGetUnconditionalBranchTarget( cscUpperBound, cscLowerBound, controlOp, defLookup, useChains );
                if(takenBranch != null)
                {
                    UnconditionalControlOperator opNewCtrl = UnconditionalControlOperator.New( controlOp.DebugInfo, takenBranch );
                    controlOp.SubstituteWithOperator( opNewCtrl, Operator.SubstitutionFlags.Default );
                    fRunSimplify = true;
                }
            }

#if ENABLE_LOW_LEVEL_OPTIMIZATIONS
            foreach(var controlOp in cfg.FilterOperators< ConditionCodeConditionalControlOperator >())
            {
                bool fNotEqual;

                switch(controlOp.Condition)
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

                var opCmp = ControlFlowGraphState.CheckSingleDefinition( defLookup, controlOp.FirstArgument ) as CompareOperator;
                if(opCmp != null)
                {
                    bool fNullOnRight;
                    Expression ex = opCmp.IsBinaryOperationAgainstZeroValue( out fNullOnRight );

                    if(ex != null)
                    {
                        if(ProveNonZero( defLookup, ex ) == ProveResult.True)
                        {
                            UnconditionalControlOperator opNewCtrl = UnconditionalControlOperator.New( controlOp.DebugInfo, fNotEqual ? controlOp.TargetBranchTaken : controlOp.TargetBranchNotTaken );
                            controlOp.SubstituteWithOperator( opNewCtrl, Operator.SubstitutionFlags.Default );

                            fRunSimplify = true;
                        }
                    }
                }
            }

            foreach (var controlOp in cfg.FilterOperators< ConditionCodeConditionalControlOperator >())
            {
                var opCmp = ControlFlowGraphState.CheckSingleDefinition( defLookup, controlOp.FirstArgument ) as CompareOperator;

                if(opCmp != null)
                {
                    ProveResult result = ProveComparison( cscUpperBound, cscLowerBound, opCmp.FirstArgument, opCmp.SecondArgument, controlOp.Condition );
                    if(result != ProveResult.Unknown)
                    {
                        BasicBlock takenBranch = (result == ProveResult.AlwaysTrue) ? controlOp.TargetBranchTaken : controlOp.TargetBranchNotTaken;
                        UnconditionalControlOperator opNewCtrl = UnconditionalControlOperator.New( controlOp.DebugInfo, takenBranch );
                        controlOp.SubstituteWithOperator( opNewCtrl, Operator.SubstitutionFlags.Default );
                        fRunSimplify = true;
                    }
                }
            }
#endif // ENABLE_LOW_LEVEL_OPTIMIZATIONS

            return fRunSimplify;
        }

        bool SomeProperty
        {
            get; set;
        }

        private static BasicBlock TryGetUnconditionalBranchTarget(
            BinaryConditionalControlOperator controlOp,
            GrowOnlyHashTable<VariableExpression, Operator> defLookup,
            Operator[][] useChains)
        {
            // Replace binary branches with unconditional ones when both targets are the same block.
            if (controlOp.TargetBranchTaken == controlOp.TargetBranchNotTaken)
            {
                return controlOp.TargetBranchTaken;
            }

            switch (ProveNonZero(controlOp.FirstArgument, defLookup, useChains))
            {
            case ProveResult.AlwaysTrue:
                return controlOp.TargetBranchTaken;

            case ProveResult.NeverTrue:
                return controlOp.TargetBranchNotTaken;
            }

            return null;
        }

        private static BasicBlock TryGetUnconditionalBranchTarget(
            Transformations.ConstraintSystemCollector cscUpperBound,
            Transformations.ConstraintSystemCollector cscLowerBound,
            CompareConditionalControlOperator controlOp,
            GrowOnlyHashTable<VariableExpression, Operator> defLookup,
            Operator[][] useChains)
        {
            // Replace binary branches with unconditional ones when both targets are the same block.
            if (controlOp.TargetBranchTaken == controlOp.TargetBranchNotTaken)
            {
                return controlOp.TargetBranchTaken;
            }

            ProveResult result = ProveResult.Unknown;

            switch (controlOp.Condition)
            {
            case CompareAndSetOperator.ActionCondition.EQ:
            case CompareAndSetOperator.ActionCondition.NE:
                bool fNullOnRight;
                Expression expr = controlOp.IsBinaryOperationAgainstZeroValue(out fNullOnRight);
                if (expr == null)
                {
                    break;
                }

                result = ProveNonZero(expr, defLookup, useChains);
                if (result == ProveResult.Unknown)
                {
                    break;
                }

                // Invert the result for (expr == 0)
                if (controlOp.Condition == CompareAndSetOperator.ActionCondition.EQ)
                {
                    if (result == ProveResult.AlwaysTrue)
                    {
                        result = ProveResult.NeverTrue;
                    }
                    else
                    {
                        result = ProveResult.AlwaysTrue;
                    }
                }
                break;

#if ENABLE_ADVANCED_COMPARES
            case CompareAndSetOperator.ActionCondition.LT:
                result = ProveLessThanOrEqual(cscUpperBound, cscLowerBound, controlOp.FirstArgument, controlOp.SecondArgument, weight: -1);
                break;

            case CompareAndSetOperator.ActionCondition.LE:
                result = ProveLessThanOrEqual(cscUpperBound, cscLowerBound, controlOp.FirstArgument, controlOp.SecondArgument, weight: 0);
                break;

            case CompareAndSetOperator.ActionCondition.GT:
                result = ProveLessThanOrEqual(cscUpperBound, cscLowerBound, controlOp.SecondArgument, controlOp.FirstArgument, weight: -1);
                break;

            case CompareAndSetOperator.ActionCondition.GE:
                result = ProveLessThanOrEqual(cscUpperBound, cscLowerBound, controlOp.SecondArgument, controlOp.FirstArgument, weight: 0);
                break;
#endif // ENABLE_ADVANCED_COMPARES
            }

            switch (result)
            {
            case ProveResult.AlwaysTrue:
                return controlOp.TargetBranchTaken;

            case ProveResult.NeverTrue:
                return controlOp.TargetBranchNotTaken;

            default:
                return null;
            }
        }

        private enum ProveResult
        {
            Unknown = 0,
            AlwaysTrue,
            NeverTrue,
        }

        private static ProveResult ProveNonZero(
            Expression ex,
            GrowOnlyHashTable< VariableExpression, Operator > defLookup,
            Operator[][] useChains )
        {
            return ProveNonZero( ex, defLookup, useChains, SetFactory.NewWithReferenceEquality< VariableExpression >() );
        }

        private static ProveResult ProveNonZero(
            Expression ex,
            GrowOnlyHashTable< VariableExpression, Operator > defLookup,
            Operator[][] useChains,
            GrowOnlySet< VariableExpression > history )
        {
            var exConst = ex as ConstantExpression;
            if (exConst != null)
            {
                if(exConst.IsEqualToZero())
                {
                    return ProveResult.NeverTrue;
                }

                return ProveResult.AlwaysTrue;
            }

            var exVar = ex as VariableExpression;

            // If we detected a loop, the value may change between iterations.
            if(exVar == null || history.Insert( exVar ))
            {
                return ProveResult.Unknown;
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
                        return ProveResult.Unknown;
                    }
                }

                if (def.HasAnnotation< NotNullAnnotation >())
                {
                    return ProveResult.AlwaysTrue;
                }

                // Phi: If all predecessors are provably the same result, then so is the operator's result.
                if(def is PhiOperator)
                {
                    bool isFirstResult = true;
                    ProveResult result = ProveResult.Unknown;

                    foreach(Expression rhs in def.Arguments)
                    {
                        ProveResult curResult = ProveNonZero( rhs, defLookup, useChains, history );
                        if(isFirstResult)
                        {
                            isFirstResult = false;
                            result = curResult;
                        }
                        else if(curResult != result)
                        {
                            // We got two different results from different blocks.
                            return ProveResult.Unknown;
                        }
                    }

                    return result;
                }

                // Assignment is transitive, so look up the source value.
                if(def is SingleAssignmentOperator)
                {
                    return ProveNonZero( def.FirstArgument, defLookup, useChains, history );
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
                                    return ProveNonZero( defLookup, history, varSrc );

                                case BinaryOperator.ALU.OR:
                                    //
                                    // OR'ing with a non-zero constant ensures the results are not null.
                                    //
                                    if(!exConst.IsEqualToZero())
                                    {
                                        return ProveResult.AlwaysTrue;
                                    }
                                    break;
                            }
                        }
                    }

                    return ProveResult.Unknown;
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
                                return ProveResult.NeverTrue;

                            case PiOperator.Relation.NotEqual:
                                return ProveResult.AlwaysTrue;
                        }
                    }

                    return ProveNonZero( ex, defLookup, useChains, history );
                }
            }

            return ProveResult.Unknown;
        }

#if ENABLE_LOW_LEVEL_OPTIMIZATIONS
        private static ProveResult ProveComparison(
            Transformations.ConstraintSystemCollector cscUpperBound,
            Transformations.ConstraintSystemCollector cscLowerBound,
            Expression exLeft,
            Expression exRight,
            ConditionCodeExpression.Comparison condition)
        {
            switch(condition)
            {
                case ConditionCodeExpression.Comparison.UnsignedLowerThan:
                case ConditionCodeExpression.Comparison.SignedLessThan:
                    return ProveLessThanOrEqual( cscUpperBound, cscLowerBound, exLeft, exRight, -1 );

                case ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame:
                case ConditionCodeExpression.Comparison.SignedLessThanOrEqual:
                    return ProveLessThanOrEqual( cscUpperBound, cscLowerBound, exLeft, exRight, 0 );

                case ConditionCodeExpression.Comparison.UnsignedHigherThan:
                case ConditionCodeExpression.Comparison.SignedGreaterThan:
                    return ProveLessThanOrEqual( cscUpperBound, cscLowerBound, exRight, exLeft, -1 );

                case ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame:
                case ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual:
                    return ProveLessThanOrEqual( cscUpperBound, cscLowerBound, exRight, exLeft, 0 );
            }

            return ProveResult.Unknown;
        }
#endif // ENABLE_LOW_LEVEL_OPTIMIZATIONS

        private static ProveResult ProveLessThanOrEqual(
            Transformations.ConstraintSystemCollector cscUpperBound,
            Transformations.ConstraintSystemCollector cscLowerBound,
            Expression exLeft,
            Expression exRight,
            double weight)
        {
            if(exLeft is VariableExpression)
            {
                if(cscUpperBound.Prove( exLeft, exRight, weight ))
                {
                    return ProveResult.AlwaysTrue;
                }
            }

            if(exRight is VariableExpression)
            {
                if(cscLowerBound.Prove( exRight, exLeft, -weight ))
                {
                    return ProveResult.AlwaysTrue;
                }
            }

            return ProveResult.Unknown;
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

#if ENABLE_LOW_LEVEL_OPTIMIZATIONS
        [CompilationSteps.OptimizationHandler(RunOnce=true, RunInSSAForm=true)]
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
#endif // ENABLE_LOW_LEVEL_OPTIMIZATIONS

#if ALLOW_SSA_FORM
        [CompilationSteps.OptimizationHandler(RunOnce = true, RunInSSAForm = true)]
#else // ALLOW_SSA_FORM
        [CompilationSteps.OptimizationHandler(RunOnce = true)]
#endif // ALLOW_SSA_FORM
        private static void ReduceComparisons(PhaseExecution.NotificationContext nc)
        {
            ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;
            Operator[][] defChains = cfg.DataFlow_DefinitionChains;
            Operator[][] useChains = cfg.DataFlow_UseChains;

            // Replace split compare + branch operators with CompareConditionalControlOperator.
            foreach (var branchOp in cfg.FilterOperators<BinaryConditionalControlOperator>())
            {
                var comparand = branchOp.FirstArgument as VariableExpression;
                if (comparand == null)
                {
                    continue;
                }

                var compareOp = ControlFlowGraphState.CheckSingleDefinition(defChains, comparand) as CompareAndSetOperator;
                if (compareOp == null)
                {
                    continue;
                }

                var conditionalOp = CompareConditionalControlOperator.New(
                    branchOp.DebugInfo,
                    compareOp.Condition,
                    compareOp.Signed,
                    compareOp.FirstArgument,
                    compareOp.SecondArgument,
                    branchOp.TargetBranchNotTaken,
                    branchOp.TargetBranchTaken);

                branchOp.SubstituteWithOperator(conditionalOp, Operator.SubstitutionFlags.Default);
                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

#if ALLOW_SSA_FORM
        [CompilationSteps.OptimizationHandler(RunOnce = true, RunInSSAForm = true)]
#else // ALLOW_SSA_FORM
        [CompilationSteps.OptimizationHandler(RunOnce = true)]
#endif // ALLOW_SSA_FORM
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
