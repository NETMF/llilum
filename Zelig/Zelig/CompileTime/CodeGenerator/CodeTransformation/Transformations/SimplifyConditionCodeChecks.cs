//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class SimplifyConditionCodeChecks
    {
        //
        // State
        //

        private readonly ControlFlowGraphStateForCodeTransformation m_cfg;
        private readonly BasicBlock[]                               m_basicBlocks;          // All the basic blocks, in spanning tree order.
        private readonly Operator[]                                 m_operators;            // All the operators, in spanning tree order.
        private readonly VariableExpression[]                       m_variables;            // All the variables, in spanning tree order.
        private readonly int                                        m_bbNum;
        private readonly int                                        m_opsNum;
        private readonly int                                        m_varNum;
                                                            
        private readonly Operator[][]                               m_variableUses;         // It's indexed as Operator[<variable index>][<Operator set>]
        private readonly Operator[][]                               m_variableDefinitions;  // It's indexed as Operator[<variable index>][<Operator set>]
                                                            
        private readonly Operator[]                                 m_covered;              // All the operators reached during processing, in spanning tree order.

        //
        // Constructor Methods
        //

        private SimplifyConditionCodeChecks( ControlFlowGraphStateForCodeTransformation cfg )
        {
            m_cfg                 = cfg;
            m_basicBlocks         = cfg.DataFlow_SpanningTree_BasicBlocks;
            m_operators           = cfg.DataFlow_SpanningTree_Operators;
            m_variables           = cfg.DataFlow_SpanningTree_Variables;

            m_bbNum               = m_basicBlocks.Length;
            m_opsNum              = m_operators.Length;
            m_varNum              = m_variables.Length;

            m_variableUses        = cfg.DataFlow_UseChains;
            m_variableDefinitions = cfg.DataFlow_DefinitionChains;

            m_covered             = new Operator[m_operators.Length];
        }

        //
        // Helper Methods
        //

        public static bool Execute( ControlFlowGraphStateForCodeTransformation cfg )
        {
            cfg.TraceToFile( "SimplifyConditionCodeChecks" );

            using(new PerformanceCounters.ContextualTiming( cfg, "SimplifyConditionCodeChecks" ))
            {
                SimplifyConditionCodeChecks sccc = new SimplifyConditionCodeChecks( cfg );

                return sccc.Apply();
            }
        }

        public bool Apply()
        {
            bool fModified = false;

            for(int opIdx = 0; opIdx < m_opsNum; opIdx++)
            {
                //
                // We are looking for patterns that can be compressed.
                //
                // For example:
                //
                //  <var> = set if $CC<x> is <conditionX>   ||   <var> = address of <stack location>
                //  [ <var>] = zeroextend [<var>] ]
                //  $CC<y> = compare <var> <-> $Const(int 0)
                //  if $CC<y> is <conditionY> then goto $Label_3 else goto $Label_2
                //
                // If $CC<y> is defined only there, we can change $CC<y> with $CC<x>, after we adjust the condition.
                //
                ConditionCodeConditionalControlOperator opY = m_operators[opIdx] as ConditionCodeConditionalControlOperator;
                if(opY != null)
                {
                    Operator def = GetSingleDef( opY.FirstArgument );

                    fModified |= ProcessSingleDefinition( opY, def, false, 64 );
                }
            }

            return fModified;
        }

        private bool ProcessSingleDefinition( ConditionCodeConditionalControlOperator opY       ,
                                              Operator                                def       ,
                                              bool                                    fNegate   ,
                                              uint                                    validBits )
        {
            if(def == null)
            {
                return false;
            }

            if(m_covered[def.SpanningTreeIndex] != null)
            {
                //
                // Break loops.
                //
                return false;
            }

            bool fModified = false;
            int  idx       = def.SpanningTreeIndex;

            m_covered[idx] = def;

            if(def is SingleAssignmentOperator)
            {
                fModified = ProcessSingleAssignmentOperator( opY, (SingleAssignmentOperator)def, fNegate, validBits );
            }
            else if(def is ZeroExtendOperator)
            {
                fModified = ProcessZeroExtendOperator( opY, (ZeroExtendOperator)def, fNegate, validBits );
            }
            else if(def is PhiOperator)
            {
                fModified = ProcessPhiOperator( opY, (PhiOperator)def, fNegate, validBits );
            }
            else if(def is CompareOperator)
            {
                fModified = ProcessCompareOperator( opY, (CompareOperator)def, fNegate, validBits );
            }
            else if(def is LongCompareOperator)
            {
                fModified = ProcessLongCompareOperator( opY, (LongCompareOperator)def, fNegate, validBits );
            }
            else if(def is SetIfConditionIsTrueOperator)
            {
                fModified = ProcessSetIfConditionIsTrueOperator( opY, (SetIfConditionIsTrueOperator)def, fNegate, validBits );
            }
            else if(def is AddressAssignmentOperator)
            {
                fModified = ProcessAddressAssignmentOperator( opY, (AddressAssignmentOperator)def, fNegate, validBits );
            }

            m_covered[idx] = null;

            return fModified;
        }

        private bool ProcessSingleAssignmentOperator( ConditionCodeConditionalControlOperator opY       ,
                                                      SingleAssignmentOperator                assignDef ,
                                                      bool                                    fNegate   ,
                                                      uint                                    validBits )
        {
            Expression assignSrc = assignDef.FirstArgument;

            if(assignSrc is ConstantExpression)
            {
                ConstantExpression exConst = (ConstantExpression)assignSrc;

                if(exConst.IsValueInteger       ||
                   exConst.IsValueFloatingPoint  )
                {
                    bool fSubstitute = false;
                    bool fTaken      = false;
                    bool fZero       = exConst.IsEqualToZero();

                    switch(opY.Condition)
                    {
                        case ConditionCodeExpression.Comparison.Equal:
                            fSubstitute = true;
                            fTaken      = fZero == fNegate;
                            break;

                        case ConditionCodeExpression.Comparison.NotEqual:
                            fSubstitute = true;
                            fTaken      = fZero != fNegate;
                            break;
                    }

                    if(fSubstitute)
                    {
                        UnconditionalControlOperator opNew;
                        BasicBlock                   branch;
                        
                        if(fTaken)
                        {
                            branch = opY.TargetBranchTaken;
                        }
                        else
                        {
                            branch = opY.TargetBranchNotTaken;
                        }

                        opNew = UnconditionalControlOperator.New( opY.DebugInfo, branch );

                        opY.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                        return true;
                    }
                }

                return false;
            }
            else
            {
                Operator def = GetSingleDef( assignSrc );

                return ProcessSingleDefinition( opY, def, fNegate, validBits );
            }
        }

        private bool ProcessZeroExtendOperator( ConditionCodeConditionalControlOperator opY       ,
                                                ZeroExtendOperator                      extendDef ,
                                                bool                                    fNegate   ,
                                                uint                                    validBits )
        {
            Expression extendSrc = extendDef.FirstArgument;
            Operator   def       = GetSingleDef( extendSrc );

            return ProcessSingleDefinition( opY, def, fNegate, extendDef.SignificantSize );
        }

        private bool ProcessPhiOperator( ConditionCodeConditionalControlOperator opY       ,
                                         PhiOperator                             phiDef    ,
                                         bool                                    fNegate   ,
                                         uint                                    validBits )
        {
            Expression[] rhs     = phiDef.Arguments;
            BasicBlock[] origins = phiDef.Origins;

            for(int idx = 0; idx < rhs.Length; idx++)
            {
                VariableExpression var    = (VariableExpression)rhs[idx];
                Operator           varDef =                     GetSingleDef( var );

                if(varDef is SingleAssignmentOperator)
                {
                    ConstantExpression exConst = varDef.FirstArgument as ConstantExpression;

                    if(exConst != null)
                    {
                        bool fSourceIsZero  = exConst.IsEqualToZero();
                        bool fSubstitute    = false;
                        bool fCompareToZero = false;

                        switch(opY.Condition)
                        {
                            case ConditionCodeExpression.Comparison.Equal:
                                fSubstitute    = true;
                                fCompareToZero = fNegate;
                                break;

                            case ConditionCodeExpression.Comparison.NotEqual:
                                fSubstitute    = true;
                                fCompareToZero = !fNegate;
                                break;
                        }

                        if(fSubstitute)
                        {
                            UnconditionalControlOperator ctrl = varDef.BasicBlock.FlowControl as UnconditionalControlOperator;

                            if(ctrl != null)
                            {
                                //
                                // For now, we only handle the case of a single phi operator per basic block.
                                //
                                foreach(Operator op in phiDef.BasicBlock.Operators)
                                {
                                    PhiOperator phiOp = op as PhiOperator;

                                    if(phiOp != null && phiOp != phiDef)
                                    {
                                        fSubstitute = false;
                                    }
                                }

                                if(fSubstitute)
                                {
                                    //
                                    // We detect that one branch coming in into the conditional is always zero or false.
                                    // 
                                    // We'd like to short-circuit the origin of the conditional input to its destination,
                                    // but we have to make sure that all the results in the trace to the conditional
                                    // are dead after the conditional.
                                    //
                                    Operator start     = m_operators[varDef.SpanningTreeIndex+1];
                                    bool     fContinue = true;

                                    while(fContinue)
                                    {
                                        ControlOperator endCtrl = start.BasicBlock.FlowControl;

                                        int  idxStart  = start  .SpanningTreeIndex;
                                        int  idxEnd    = endCtrl.SpanningTreeIndex;

                                        while(idxStart < idxEnd)
                                        {
                                            Operator op = m_covered[idxStart];
                                            
                                            if(op                                                      == null ||
                                               op.Results.Length                                       == 0    ||
                                               m_variableUses[op.FirstResult.SpanningTreeIndex].Length != 1     )
                                            {
                                                //
                                                // There's an operator that was not involved in the computation of the conditional,
                                                // or its result are used also outside the conditional,
                                                // we cannot prove that the short-circuit is safe.
                                                //
                                                fContinue   = false;
                                                fSubstitute = false;
                                                break;
                                            }

                                            idxStart++;

                                            if(idxStart == opY.SpanningTreeIndex)
                                            {
                                                fContinue = false;
                                                break;
                                            }
                                        }

                                        if(fContinue)
                                        {
                                            UnconditionalControlOperator endCtrl2 = endCtrl as UnconditionalControlOperator;

                                            if(endCtrl2 == null)
                                            {
                                                //
                                                // Multiple destinations, we cannot prove that the short-circuit is safe.
                                                //
                                                fContinue   = false;
                                                fSubstitute = false;
                                                break;
                                            }

                                            start = endCtrl2.TargetBranch.Operators[0];
                                        }
                                    }
                                }

                                if(fSubstitute)
                                {
                                    BasicBlock branch;
                        
                                    if(fSourceIsZero == fCompareToZero)
                                    {
                                        branch = opY.TargetBranchTaken;
                                    }
                                    else
                                    {
                                        branch = opY.TargetBranchNotTaken;
                                    }

                                    phiDef.RemoveEffect( ctrl.BasicBlock );

                                    ctrl.SubstituteWithOperator( UnconditionalControlOperator.New( ctrl.DebugInfo, branch ), Operator.SubstitutionFlags.Default );
                                }
                            }
                        }
                    }
                }
            }

            if(phiDef.Arguments.Length == 1)
            {
                SingleAssignmentOperator opNew = SingleAssignmentOperator.New( phiDef.DebugInfo, phiDef.FirstResult, phiDef.FirstArgument );

                var opTarget = phiDef.BasicBlock.GetFirstDifferentOperator( typeof( PhiOperator ) );

                opTarget.AddOperatorBefore( opNew );
                phiDef.Delete();

                return true;
            }

            return false;
        }

        private bool ProcessCompareOperator( ConditionCodeConditionalControlOperator opY       ,
                                             CompareOperator                         cmpDef    ,
                                             bool                                    fNegate   ,
                                             uint                                    validBits )
        {
            bool       fNullOnRight;
            Expression target = cmpDef.IsBinaryOperationAgainstZeroValue( out fNullOnRight );

            if(target is ConstantExpression)
            {
                bool fSubstitute = false;
                bool fTaken      = false;
                bool fZero       = target.IsEqualToZero();

                switch(opY.Condition)
                {
                    case ConditionCodeExpression.Comparison.Equal:
                        fSubstitute = true;
                        fTaken      = fZero != fNegate;
                        break;

                    case ConditionCodeExpression.Comparison.NotEqual:
                        fSubstitute = true;
                        fTaken      = fZero == fNegate;
                        break;
                }

                if(fSubstitute)
                {
                    UnconditionalControlOperator opNew;
                    BasicBlock                   branch;
                    
                    if(fTaken)
                    {
                        branch = opY.TargetBranchTaken;
                    }
                    else
                    {
                        branch = opY.TargetBranchNotTaken;
                    }

                    opNew = UnconditionalControlOperator.New( opY.DebugInfo, branch );

                    opY.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                    return true;
                }
            }
            /*  // TODO: revisit this later and perform this optimzation only when appropriate (when only one conditional is used per block
                // and the target for the conditional is in the same block).
                // 
                // This does not always work.  If there are multiple conditionals initialized before they are referenced, then only the last 
                // conditional value will be used.  In this case we need to store the conditional value in a register.
            else if(target is VariableExpression)
            {
                Operator targetDef = GetSingleDef( (VariableExpression)target );

                return ProcessSingleDefinition( opY, targetDef, !fNegate, target.Type.Size );
            }
            */

            return false;
        }

        private bool ProcessLongCompareOperator( ConditionCodeConditionalControlOperator opY       ,
                                                 LongCompareOperator                     cmpDef    ,
                                                 bool                                    fNegate   ,
                                                 uint                                    validBits )
        {
            bool         fNullOnRight;
            Expression[] target = cmpDef.IsCompareToNull( out fNullOnRight );

            if(target != null)
            {
                if(target[0] is ConstantExpression &&
                   target[1] is ConstantExpression  )
                {
                    bool fSubstitute = false;
                    bool fTaken      = false;
                    bool fZero       = target[0].IsEqualToZero() && target[1].IsEqualToZero();

                    switch(opY.Condition)
                    {
                        case ConditionCodeExpression.Comparison.Equal:
                            fSubstitute = true;
                            fTaken      = fZero != fNegate;
                            break;

                        case ConditionCodeExpression.Comparison.NotEqual:
                            fSubstitute = true;
                            fTaken      = fZero == fNegate;
                            break;
                    }

                    if(fSubstitute)
                    {
                        UnconditionalControlOperator opNew;
                        BasicBlock                   branch;
                        
                        if(fTaken)
                        {
                            branch = opY.TargetBranchTaken;
                        }
                        else
                        {
                            branch = opY.TargetBranchNotTaken;
                        }

                        opNew = UnconditionalControlOperator.New( opY.DebugInfo, branch );

                        opY.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                        return true;
                    }
                }
                else if(target[0] is VariableExpression &&
                        target[1] is VariableExpression)
                {
                    //
                    // TODO: Not sure how to propagate through two variables at the same time...
                    //
////                Operator targetDefLo = GetSingleDef( (VariableExpression)target[0] );
////                Operator targetDefHi = GetSingleDef( (VariableExpression)target[1] );
////
////                return ProcessSingleDefinition( opY, targetDef, !fNegate, target.Type.Size );
                }
            }

            return false;
        }

        private bool ProcessSetIfConditionIsTrueOperator( ConditionCodeConditionalControlOperator opY       ,
                                                          SetIfConditionIsTrueOperator            setOp     ,
                                                          bool                                    fNegate   ,
                                                          uint                                    validBits )
        {
            bool fSubstitute = false;
            bool fTaken      = false;

            switch(opY.Condition)
            {
                case ConditionCodeExpression.Comparison.Equal:
                    fSubstitute = true;
                    fTaken      = !fNegate;
                    break;

                case ConditionCodeExpression.Comparison.NotEqual:
                    fSubstitute = true;
                    fTaken      = fNegate;
                    break;
            }

            if(fSubstitute)
            {
                ConditionCodeConditionalControlOperator opNew;
                BasicBlock                              branchTaken;
                BasicBlock                              branchNotTaken;
                
                if(fTaken)
                {
                    branchTaken    = opY.TargetBranchTaken;
                    branchNotTaken = opY.TargetBranchNotTaken;
                }
                else
                {
                    branchTaken    = opY.TargetBranchNotTaken;
                    branchNotTaken = opY.TargetBranchTaken;
                }

                opNew = ConditionCodeConditionalControlOperator.New( opY.DebugInfo, setOp.Condition, (VariableExpression)setOp.FirstArgument, branchNotTaken, branchTaken );

                opY.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                return true;
            }

            return false;
        }

        private bool ProcessAddressAssignmentOperator( ConditionCodeConditionalControlOperator opY       ,
                                                       AddressAssignmentOperator               addrDef   ,
                                                       bool                                    fNegate   ,
                                                       uint                                    validBits )
        {
            //
            // So <var> can never be zero.
            //
            bool fSubstitute = false;
            bool fTaken      = false;

            switch(opY.Condition)
            {
                case ConditionCodeExpression.Comparison.Equal:
                    fSubstitute = true;
                    fTaken      = !fNegate;
                    break;

                case ConditionCodeExpression.Comparison.NotEqual:
                    fSubstitute = true;
                    fTaken      = fNegate;
                    break;
            }

            if(fSubstitute)
            {
                UnconditionalControlOperator opNew;
                BasicBlock                   branch;
                
                if(fTaken)
                {
                    branch = opY.TargetBranchTaken;
                }
                else
                {
                    branch = opY.TargetBranchNotTaken;
                }

                opNew = UnconditionalControlOperator.New( opY.DebugInfo, branch );

                opY.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                return true;
            }

            return false;
        }

        private Operator GetSingleDef( Expression ex )
        {
            if(ex is VariableExpression)
            {
                Operator def = ControlFlowGraphState.CheckSingleDefinition( m_variableDefinitions, (VariableExpression)ex );

                if(def != null)
                {
                    //
                    // Is it still in the graph?
                    //
                    if(def.BasicBlock != null)
                    {
                        return def;
                    }
                }
            }

            return null;
        }
    }
}
