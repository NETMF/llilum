//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DUMP_CONSTRAINTSYSTEM


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

        [CompilationSteps.OptimizationHandler(RunOnce=true, RunInExtendedSSAForm=true)]
        private static void RemoveRedundantChecks( PhaseExecution.NotificationContext nc )
        {
            ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;

////        if(cfg.ToString() == "FlowGraph(void Microsoft.Zelig.Runtime.Bootstrap::EntryPoint())")
////        if(cfg.ToString() == "FlowGraph(void Microsoft.Zelig.Runtime.MarkAndSweepCollector::InitializeGarbageCollectionManager())")
////        {
////        }

            while(true)
            {
                GrowOnlyHashTable< VariableExpression, Operator > defLookup = cfg.DataFlow_SingleDefinitionLookup;

                if(PropagateFixedArrayLength ( cfg, defLookup ) ||
                   RemoveRedundantNullChecks ( cfg, defLookup ) ||
                   RemoveRedundantBoundChecks( cfg, defLookup )  )
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

            foreach(var op in cfg.FilterOperators< LoadIndirectOperator >())
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

            if(array == null || history.Insert( array ) == true)
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

        private static bool RemoveRedundantNullChecks( ControlFlowGraphStateForCodeTransformation        cfg       ,
                                                       GrowOnlyHashTable< VariableExpression, Operator > defLookup )
        {
            bool fRunSimplify = false;

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
                    bool       fNullOnRight;
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

            return fRunSimplify;
        }

        private enum ProveNotNullResult
        {
            Maybe,
            True ,
            False,
        }

        private static ProveNotNullResult ProveNotNull( GrowOnlyHashTable< VariableExpression, Operator > defLookup ,
                                                        Expression                                        ex        )
        {
            return ProveNotNull( defLookup, SetFactory.NewWithReferenceEquality< VariableExpression >(), ex );
        }

        private static ProveNotNullResult ProveNotNull( GrowOnlyHashTable< VariableExpression, Operator > defLookup ,
                                                        GrowOnlySet< VariableExpression >                 history   ,
                                                        Expression                                        ex        )
        {
            if(ex is ConstantExpression)
            {
                var exConst = (ConstantExpression)ex;

                if(exConst.IsEqualToZero())
                {
                    return ProveNotNullResult.False;
                }

                if(exConst.Value is DataManager.DataDescriptor)
                {
                    return ProveNotNullResult.True;
                }

                return ProveNotNullResult.False;
            }

            var exVar = ex as VariableExpression;

            if(exVar == null || history.Insert( exVar ) == true)
            {
                //
                // Detected loop.
                //
                return ProveNotNullResult.Maybe;
            }

            //--//

            Operator def;

            if(defLookup.TryGetValue( exVar, out def ))
            {
                if(def.HasAnnotation< NotNullAnnotation >())
                {
                    return ProveNotNullResult.True;
                }

                if(def is PhiOperator)
                {
                    ProveNotNullResult res = ProveNotNullResult.False;

                    foreach(Expression rhs in def.Arguments)
                    {
                        switch(ProveNotNull( defLookup, history, rhs ))
                        {
                            case ProveNotNullResult.True:
                                res = ProveNotNullResult.True;
                                break;

                            case ProveNotNullResult.False:
                                return ProveNotNullResult.False;
                        }
                    }

                    return res;
                }

                if(def is SingleAssignmentOperator)
                {
                    return ProveNotNull( defLookup, history, def.FirstArgument );
                }

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
                                    return ProveNotNull( defLookup, history, varSrc );

                                case BinaryOperator.ALU.OR:
                                    //
                                    // OR'ing with a non-zero constant ensures the results are not null.
                                    //
                                    if(exConst.IsEqualToZero() == false)
                                    {
                                        return ProveNotNullResult.True;
                                    }
                                    break;
                            }
                        }
                    }

                    return ProveNotNullResult.False;
                }

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
                                return ProveNotNullResult.False;

                            case PiOperator.Relation.NotEqual:
                                return ProveNotNullResult.True;
                        }
                    }

                    return ProveNotNull( defLookup, history, ex );
                }
            }

            return ProveNotNullResult.False;
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

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.OptimizationHandler(RunOnce=true, RunInSSAForm=true)]
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

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [CompilationSteps.OptimizationHandler(RunOnce=false, RunInSSAForm=true)]
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
    }
}
