//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    public class ReduceNumberOfTemporaries
    {
        //
        // State
        //

        private readonly ControlFlowGraphStateForCodeTransformation m_cfg;

        private VariableExpression[]                                m_variables;
        private VariableExpression.Property[]                       m_varProps;
        private Operator[]                                          m_operators;
        private Operator[][]                                        m_useChains; // Operator[<variable index>][<Operator set>]
        private Operator[][]                                        m_defChains; // Operator[<variable index>][<Operator set>]

        //
        // Constructor Methods
        //

        private ReduceNumberOfTemporaries( ControlFlowGraphStateForCodeTransformation cfg )
        {
            m_cfg = cfg;
        }

        //
        // Helper Methods
        //

        public static void Execute( ControlFlowGraphStateForCodeTransformation cfg )
        {
            cfg.TraceToFile( "ReduceNumberOfTemporaries" );

            using(new PerformanceCounters.ContextualTiming( cfg, "ReduceNumberOfTemporaries" ))
            {
                ReduceNumberOfTemporaries pThis = new ReduceNumberOfTemporaries( cfg );

                while(pThis.ExecuteSteps() == true)
                {
                    Transformations.RemoveDeadCode.Execute( cfg, true );
                }
            }
        }

        private bool ExecuteSteps()
        {
            using(m_cfg.GroupLock( m_cfg.LockSpanningTree         () ,
                                   m_cfg.LockPropertiesOfVariables() ,
                                   m_cfg.LockUseDefinitionChains  () ))
            {
                m_variables = m_cfg.DataFlow_SpanningTree_Variables;
                m_operators = m_cfg.DataFlow_SpanningTree_Operators;
                m_useChains = m_cfg.DataFlow_UseChains;
                m_defChains = m_cfg.DataFlow_DefinitionChains;
                m_varProps  = m_cfg.DataFlow_PropertiesOfVariables;
                

                if(PropagateTemporaries                        ()) return true;
                if(StrengthReduceForConditionalControlOperators()) return true;
                if(PropagateConstantToChecks                   ()) return true;

                return false;
            }
        }

        //--//

        private bool PropagateTemporaries()
        {
            bool fChanged = false;

            foreach(VariableExpression var in m_variables)
            {
                int idx = var.SpanningTreeIndex;

                if(var is TemporaryVariableExpression && 
                   m_defChains[idx].Length == 1        )
                {
                    Operator   opDef  = m_defChains[idx][0];
                    Operator[] opUses = m_useChains[idx];

                    switch(opUses.Length)
                    {
                        case 1:
                            //
                            // One definition and one use, let's see if we can short-circuit the two.
                            //
                            {
                                Operator opUse = opUses[0];

                                if(opUse.BasicBlock == opDef.BasicBlock)
                                {
                                    int start = opDef.SpanningTreeIndex;
                                    int end   = opUse.SpanningTreeIndex;

                                    if(start < end)
                                    {
                                        VariableExpression exDef = opDef.FirstResult;

                                        if(opUse is SingleAssignmentOperator)
                                        {
                                            VariableExpression exUse = opUse.FirstResult;

                                            if(IsRedefinedInRange( exUse, start, end ) == false)
                                            {
                                                opDef.SubstituteDefinition( exDef, exUse );
                                                opUse.Delete();
                                                fChanged = true;
                                            }
                                        }
                                        else if(opDef is ZeroExtendOperator               &&
                                                opUse is BinaryConditionalControlOperator  )
                                        {
                                            //
                                            // Detect pattern:
                                            //
                                            //      $TempA = zeroextend bool <var> from 8 bits 
                                            //      if $TempA != ZERO then goto BasicBlock(T) else goto BasicBlock(F)
                                            //
                                            // Convert into:
                                            //
                                            //      if <var> != ZERO then goto BasicBlock(T) else goto BasicBlock(F)
                                            //
                                            Expression exDefSrc = opDef.FirstArgument;

                                            if(IsRedefinedInRange( exDefSrc, start, end ) == false             && 
                                               exDefSrc.Type == m_cfg.TypeSystem.WellKnownTypes.System_Boolean  )
                                            {
                                                opDef.Delete();
                                                opUse.SubstituteUsage( exDef, exDefSrc );
                                                fChanged = true;
                                            }
                                        }
                                        else if(opDef is CompareAndSetOperator &&
                                                opUse is CompareAndSetOperator  )
                                        {
                                            //
                                            // Detect pattern:
                                            //
                                            //      $TempA = <var1> <rel> <var2>
                                            //      $TempB = $TempA EQ.signed $Const(int 0)
                                            //
                                            // Convert into:
                                            //
                                            //      $TempB = <var1> !<rel> <var2>
                                            //
                                            CompareAndSetOperator opCmpDef = (CompareAndSetOperator)opDef;
                                            CompareAndSetOperator opCmpUse = (CompareAndSetOperator)opUse;

                                            if(opCmpUse.Condition == CompareAndSetOperator.ActionCondition.EQ)
                                            {
                                                opCmpUse.EnsureConstantToTheRight();

                                                ConstantExpression exRight = opCmpUse.SecondArgument as ConstantExpression;
                                                if(exRight != null && exRight.IsEqualToZero())
                                                {
                                                    CompareAndSetOperator opNew = CompareAndSetOperator.New( opCmpDef.DebugInfo, opCmpDef.InvertedCondition, opCmpDef.Signed, opCmpUse.FirstResult, opCmpDef.FirstArgument, opCmpDef.SecondArgument );

                                                    opUse.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
                                                    opDef.Delete();
                                                    fChanged = true;
                                                }
                                            }
                                        }
                                        else if(opDef is SingleAssignmentOperator)
                                        {
                                            Expression exDefSrc = opDef.FirstArgument;

                                            if(IsRedefinedInRange    (        exDefSrc, start, end ) == false &&
                                               opUse.CanPropagateCopy( exDef, exDefSrc             )          &&
                                               AreTypesCompatible    ( exDef, exDefSrc             )           )
                                            {
                                                opUse.SubstituteUsage( exDef, exDefSrc );
                                                fChanged = true;
                                            }
                                        }
                                    }
                                }
                            }
                            break;

                        default:
                            //
                            // One definition and multiple uses, let's see if one of the uses is a local/arg assignment and
                            // all the other can be short-circuit to the local/arg variable.
                            //
                            {
                                Operator target = null;
                                bool     fSkip  = false;
                                int      posDef = opDef.SpanningTreeIndex;

                                foreach(Operator opUse in opUses)
                                {
                                    if(opUse.BasicBlock != opDef.BasicBlock)
                                    {
                                        fSkip = true;
                                        break;
                                    }
                                    
                                    int posUse = opUse.SpanningTreeIndex;

                                    if(posUse <= posDef)
                                    {
                                        fSkip = true;
                                        break;
                                    }

                                    if(opUse is SingleAssignmentOperator)
                                    {
                                        VariableExpression lhs = opUse.FirstResult;

                                        if(lhs is LocalVariableExpression    ||
                                           lhs is ArgumentVariableExpression  )
                                        {
                                            if(target != null)
                                            {
                                                fSkip = true;
                                                break;
                                            }

                                            target = opUse;
                                        }
                                    }
                                }

                                if(fSkip == false && target != null)
                                {
                                    VariableExpression exUse = target.FirstResult;

                                    foreach(Operator opUse in opUses)
                                    {
                                        int posUse = opUse.SpanningTreeIndex;

                                        if(opUse != target)
                                        {
                                            if(IsRedefinedInRange    (      exUse, posDef, posUse )          ||
                                               opUse.CanPropagateCopy( var, exUse                 ) == false ||
                                               AreTypesCompatible    ( var, exUse                 ) == false  )
                                            {
                                                fSkip = true;
                                                break;
                                            }
                                        }
                                    }

                                    if(fSkip == false)
                                    {
                                        opDef.SubstituteDefinition( var, exUse );

                                        foreach(Operator opUse in opUses)
                                        {
                                            if(opUse != target)
                                            {
                                                opUse.SubstituteUsage( var, exUse );
                                            }
                                        }

                                        target.Delete();
                                        fChanged = true;
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            return fChanged;
        }

        private bool IsRedefinedInRange( Expression ex    ,
                                         int        start ,
                                         int        end   )
        {
            VariableExpression var = ex as VariableExpression;

            if(var != null)
            {
                bool fAddressTaken = (m_varProps[ex.SpanningTreeIndex] & VariableExpression.Property.AddressTaken) != 0;

                for(int i = start + 1; i < end; i++)
                {
                    Operator op = m_operators[i];

                    if(ArrayUtility.FindInNotNullArray( op.Results, var ) >= 0)
                    {
                        return true;
                    }

                    if(op.MayWriteThroughPointerOperands && fAddressTaken)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool AreTypesCompatible( Expression exDef    ,
                                         Expression exDefSrc )
        {
            return exDef.Type.CanBeAssignedFrom( exDefSrc.Type, null );
        }

        //--//

        private bool StrengthReduceForConditionalControlOperators()
        {
            bool fChanged = false;

            foreach(Operator op in m_operators)
            {
                BinaryConditionalControlOperator opCtrl = op as BinaryConditionalControlOperator;

                if(opCtrl != null)
                {
                    Operator opPrev = opCtrl.GetPreviousOperator();

                    if(opPrev is SingleAssignmentOperator)
                    {
                        //
                        // Detect pattern:
                        //
                        //      <varA> = <varB>
                        //      if <varA> != ZERO then goto BasicBlock(T) else goto BasicBlock(T)
                        //
                        // Convert into:
                        //
                        //      <varA> = <varB>
                        //      if <varB> != ZERO then goto BasicBlock(T) else goto BasicBlock(T)
                        //
                        VariableExpression exSrc = opPrev.FirstResult;
                        Expression         exDst = opCtrl.FirstArgument;

                        if(exSrc == exDst)
                        {
                            opCtrl.SubstituteUsage( exDst, opPrev.FirstArgument );
                            fChanged = true;
                        }
                    }
                    else if(opPrev is CompareAndSetOperator)
                    {
                        //
                        // Detect pattern:
                        //
                        //      <var3> = <var1> <rel> <var2>
                        //      if <var3> != ZERO then goto BasicBlock(T) else goto BasicBlock(T)
                        //
                        // Convert into:
                        //
                        //      <var3> = <var1> <rel> <var2>
                        //      if <var1> <rel> <var2> then goto BasicBlock(T) else goto BasicBlock(T)
                        //
                        CompareAndSetOperator opPrevCmp = (CompareAndSetOperator)opPrev;
                        VariableExpression    exSrc     = opPrevCmp.FirstResult;
                        Expression            exDst     = opCtrl.FirstArgument;

                        if(exSrc == exDst)
                        {
                            CompareConditionalControlOperator ctrl = CompareConditionalControlOperator.New( opPrevCmp.DebugInfo, opPrevCmp.Condition, opPrevCmp.Signed, opPrevCmp.FirstArgument, opPrevCmp.SecondArgument, opCtrl.TargetBranchNotTaken, opCtrl.TargetBranchTaken );

                            opCtrl.SubstituteWithOperator( ctrl, Operator.SubstitutionFlags.Default );
                            fChanged = true;
                        }
                    }
                }
            }

            return fChanged;
        }

        //--//

        private bool PropagateConstantToChecks()
        {
            bool fChanged = false;

            //
            // If a variable is only used in a BinaryConditionalControlOperator,
            // try to replicate the check close to all the definitions of the variable.
            // Some definitions could be constants and the checks will straighten out.
            //
            foreach(VariableExpression var in m_variables)
            {
                BinaryConditionalControlOperator op = ControlFlowGraphState.CheckSingleUse( m_useChains, var ) as BinaryConditionalControlOperator;
                if(op != null)
                {
                    foreach(Operator def in m_defChains[var.SpanningTreeIndex])
                    {
                        if(CanPropagate( def, op ))
                        {
                            ControlOperator opCtrl = null;

                            if(def is SingleAssignmentOperator)
                            {
                                ConstantExpression exConst = def.FirstArgument as ConstantExpression;

                                if(exConst != null && exConst.IsValueInteger)
                                {
                                    opCtrl = UnconditionalControlOperator.New( op.DebugInfo, exConst.IsEqualToZero() ? op.TargetBranchNotTaken : op.TargetBranchTaken );
                                }
                            }

                            if(opCtrl == null)
                            {
                                opCtrl = BinaryConditionalControlOperator.New( op.DebugInfo, op.FirstArgument, op.TargetBranchNotTaken, op.TargetBranchTaken );
                            }

                            def.BasicBlock.FlowControl.SubstituteWithOperator( opCtrl, Operator.SubstitutionFlags.Default );
                            fChanged = true;
                        }
                    }
                }
            }

            return fChanged;
        }

        private static bool CanPropagate( Operator                         def ,
                                          BinaryConditionalControlOperator op  )
        {
            var defBB = def.BasicBlock;
            var opBB  = op .BasicBlock;

            if(defBB == opBB)
            {
                return false;
            }

            if(!(defBB is NormalBasicBlock) ||
               !(opBB  is NormalBasicBlock)  )
            {
                return false;
            }

            def = def.GetNextOperator();

            //
            // Make sure all the operators between 'def' and 'op' are either Nops or unconditional branches.
            //
            while(def != null)
            {
                if(def == op)
                {
                    return true;
                }

                if(def is NopOperator)
                {
                    def = def.GetNextOperator();
                    continue;
                }

                UnconditionalControlOperator ctrl = def as UnconditionalControlOperator;
                if(ctrl != null)
                {
                    def = ctrl.TargetBranch.Operators[0];
                    continue;
                }

                break;
            }

            return false;
        }
    }
}
