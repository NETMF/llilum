//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.DataFlow
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    //
    // This class computes, for each program point or basic block, the set of variables that are alive.
    //
    public class LivenessAnalysis
    {
        //
        // State
        //

        private readonly BasicBlock[]           m_basicBlocks;
        private readonly Operator[]             m_operators;
        private readonly VariableExpression[]   m_variables;
        private readonly VariableExpression[][] m_variablesByStorage;
        private readonly VariableExpression[][] m_variablesByAggregate;
        private readonly int                    m_bbNum;
        private readonly int                    m_varNum; 
                                      
        private          BitVector[]            m_state_LVinAtOperator; // Variables live at the beginning of an operator. 
        private readonly BitVector[]            m_state_LVin;           // Variables live at the beginning of a basic block.
        private readonly BitVector[]            m_state_LVout;          // Variables live at the end of a basic block.
        private readonly BitVector[]            m_state_DEF;            // Variables defined in a basic block, BEFORE any use.
        private readonly BitVector[]            m_state_USE;            // Variables used in a basic block BEFORE any definition.
        private          bool                   m_foundPhiOperators;
                                      
        private          BitVector[]            m_variableLivenessMap;  // For each variable, the set of operators it's alive for.

        //
        // Constructor Methods
        //

        private LivenessAnalysis( ControlFlowGraphStateForCodeTransformation cfg                         ,
                                  bool                                       fExpandLivenessToAggregates )
        {
            m_basicBlocks          = cfg.DataFlow_SpanningTree_BasicBlocks;
            m_operators            = cfg.DataFlow_SpanningTree_Operators;
            m_variables            = cfg.DataFlow_SpanningTree_Variables;
            m_variablesByStorage   = cfg.DataFlow_SpanningTree_VariablesByStorage;

            m_bbNum                = m_basicBlocks.Length;
            m_varNum               = m_variables.Length;
                         
            m_state_LVin           = BitVector.AllocateBitVectors( m_bbNum, m_varNum );
            m_state_LVout          = BitVector.AllocateBitVectors( m_bbNum, m_varNum );
            m_state_DEF            = BitVector.AllocateBitVectors( m_bbNum, m_varNum );
            m_state_USE            = BitVector.AllocateBitVectors( m_bbNum, m_varNum );

            if(fExpandLivenessToAggregates)
            {
                m_variablesByAggregate = cfg.DataFlow_SpanningTree_VariablesByAggregate;
            }
        }

        public static LivenessAnalysis Compute( ControlFlowGraphStateForCodeTransformation cfg                         ,
                                                bool                                       fExpandLivenessToAggregates )
        {
            using(cfg.LockSpanningTree())
            {
                LivenessAnalysis liveness = new LivenessAnalysis( cfg, fExpandLivenessToAggregates );

                liveness.Compute();

                return liveness;
            }
        }

        //--//

        private void Compute()
        {
            ComputeEquationParameters();

            SolveEquations();
        }

        //--//

        private void ComputeEquationParameters()
        {
            for(int i = 0; i < m_bbNum; i++)
            {
                BasicBlock bb  = m_basicBlocks[i];
                BitVector  def = m_state_DEF  [i];
                BitVector  use = m_state_USE  [i];
                Operator[] ops = bb.Operators;

                for(int pos = ops.Length; --pos >= 0;)
                {
                    VariableExpression var;
                    Operator           op = ops[pos];

                    foreach(var an in op.FilterAnnotations< PostInvalidationAnnotation >())
                    {
                        if(m_variablesByAggregate != null)
                        {
                            if(ShouldSkip( an ))
                            {
                                continue;
                            }
                        }

                        int idx = an.Target.SpanningTreeIndex;

                        //
                        // We actually use multiple instances of physical registers and stack locations, each one with a different type.
                        // But whenever we encounter one, we need to toggle all of them.
                        //
                        foreach(VariableExpression var2 in m_variablesByStorage[idx])
                        {
                            int idx2 = var2.SpanningTreeIndex;

                            use.Clear( idx2 );
                            def.Set  ( idx2 );
                        }
                    }

                    foreach(var lhs in op.Results)
                    {
                        int idx = lhs.SpanningTreeIndex;

                        //
                        // We actually use multiple instances of physical registers and stack locations, each one with a different type.
                        // But whenever we encounter one, we need to toggle all of them.
                        //
                        foreach(VariableExpression var2 in m_variablesByStorage[idx])
                        {
                            int idx2 = var2.SpanningTreeIndex;

                            use.Clear( idx2 );
                            def.Set  ( idx2 );
                        }
                    }

                    if(op is PhiOperator)
                    {
                        //
                        // Do not include the contributions from Phi operators!
                        // Their semantic is different from that of a normal operator, it's flow-sensitive,
                        // the use of an Rvalue is conditional to the path the execution took to reach the point.
                        //
                        // Their contribution will be handled later.
                        //
                        m_foundPhiOperators = true;
                    }
                    else
                    {
                        foreach(Expression ex in op.Arguments)
                        {
                            var = ex as VariableExpression;
                            if(var != null)
                            {
                                int idx = var.SpanningTreeIndex;

                                def.Clear( idx );
                                use.Set  ( idx );
                            }
                        }

                        if(m_variablesByAggregate != null)
                        {
                            if(op is AddressAssignmentOperator)
                            {
                                var rhs = op.FirstArgument as VariableExpression;

                                if(rhs != null)
                                {
                                    foreach(var rhsAggregate in m_variablesByAggregate[rhs.SpanningTreeIndex])
                                    {
                                        int idx = rhsAggregate.SpanningTreeIndex;
        
                                        def.Clear( idx );
                                        use.Set  ( idx );
                                    }
                                }
                            }
                        }
                    }

                    foreach(var an in op.FilterAnnotations< PreInvalidationAnnotation >())
                    {
                        if(m_variablesByAggregate != null)
                        {
                            if(ShouldSkip( an ))
                            {
                                continue;
                            }
                        }

                        int idx = an.Target.SpanningTreeIndex;

                        //
                        // We actually use multiple instances of physical registers and stack locations, each one with a different type.
                        // But whenever we encounter one, we need to toggle all of them.
                        //
                        foreach(VariableExpression var2 in m_variablesByStorage[idx])
                        {
                            int idx2 = var2.SpanningTreeIndex;

                            use.Clear( idx2 );
                            def.Set  ( idx2 );
                        }
                    }
                }
            }
        }

        private void SolveEquations()
        {
            BitVector tmp = new BitVector( m_varNum );

            while(true)
            {
                bool fDone = true;

                if(m_foundPhiOperators)
                {
                    //
                    // We need to modify LVout to include the contribution of phi variables, which is flow-sensitive.
                    //
                    for(int i = 0; i < m_bbNum; i++)
                    {
                        BasicBlock bb    = m_basicBlocks[i];
                        BitVector  LVout = m_state_LVout[i];

                        foreach(BasicBlockEdge edge in bb.Successors)
                        {
                            foreach(Operator op in edge.Successor.Operators)
                            {
                                PhiOperator opPhi = op as PhiOperator;

                                if(opPhi != null)
                                {
                                    Expression[] rhs     = opPhi.Arguments;
                                    BasicBlock[] origins = opPhi.Origins;

                                    for(int pos = 0; pos < rhs.Length; pos++)
                                    {
                                        if(origins[pos] == bb)
                                        {
                                            LVout.Set( rhs[pos].SpanningTreeIndex );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //
                // LVin = (LVout - DEF) Or USE
                //
                for(int i = 0; i < m_bbNum; i++)
                {
                    tmp.Difference( m_state_LVout[i], m_state_DEF[i] );

                    tmp.OrInPlace( m_state_USE[i] );

                    BitVector LVin = m_state_LVin[i];

                    if(LVin != tmp)
                    {
                        LVin.Assign( tmp );

                        fDone = false;
                    }
                }

                if(fDone)
                {
                    break;
                }

                //
                // LVout = Or { LVin(<successors>) }
                //
                for(int i = 0; i < m_bbNum; i++)
                {
                    BitVector b = m_state_LVout[i];

                    b.ClearAll();

                    foreach(BasicBlockEdge edge in m_basicBlocks[i].Successors)
                    {
                        b.OrInPlace( m_state_LVin[ edge.Successor.SpanningTreeIndex ] );
                    }
                }
            }
        }

        //--//

        private BitVector[] ConvertToOperatorGranularity()
        {
            int numOp = m_operators.Length;

            BitVector[] res = new BitVector[numOp];

            for(int i = 0; i < m_bbNum; i++)
            {
                BitVector  live = m_state_LVout[i].Clone();
                BasicBlock bb   = m_basicBlocks[i];
                Operator[] ops  = bb.Operators;

                for(int pos = ops.Length; --pos >= 0;)
                {
                    Operator op = ops[pos];

                    foreach(var an in op.FilterAnnotations< PostInvalidationAnnotation >())
                    {
                        if(m_variablesByAggregate != null)
                        {
                            if(ShouldSkip( an ))
                            {
                                continue;
                            }
                        }

                        live.Clear( an.Target.SpanningTreeIndex );
                    }

                    foreach(var lhs in op.Results)
                    {
                        live.Clear( lhs.SpanningTreeIndex );
                    }

                    if(op is PhiOperator)
                    {
                        //
                        // It would be an error to mark the inputs of a phi operator as alive.
                        // The semantic of the phi operator is that "only one of the input is valid at each invocation".
                        // We already take care of the contribution of phi operators in the computation of LVout.
                        //
                    }
                    else
                    {
                        foreach(Expression ex in op.Arguments)
                        {
                            var var = ex as VariableExpression;
                            if(var != null)
                            {
                                live.Set( var.SpanningTreeIndex );
                            }
                        }

                        if(m_variablesByAggregate != null)
                        {
                            if(op is AddressAssignmentOperator)
                            {
                                var rhs = op.FirstArgument as VariableExpression;

                                if(rhs != null)
                                {
                                    foreach(var rhsAggregate in m_variablesByAggregate[rhs.SpanningTreeIndex])
                                    {
                                        live.Set( rhsAggregate.SpanningTreeIndex );
                                    }
                                }
                            }
                        }
                    }

                    foreach(var an in op.FilterAnnotations< PreInvalidationAnnotation >())
                    {
                        if(m_variablesByAggregate != null)
                        {
                            if(ShouldSkip( an ))
                            {
                                continue;
                            }
                        }

                        live.Clear( an.Target.SpanningTreeIndex );
                    }

                    res[op.SpanningTreeIndex] = live.Clone();
                }
            }

            return res;
        }

        private BitVector[] ConvertToVariableGranularity()
        {
            int numOp = m_operators.Length;

            BitVector[] res                 = BitVector.AllocateBitVectors( m_varNum, numOp );
            int[]       variableStateOrigin = new int[m_varNum];

            for(int i = 0; i < m_bbNum; i++)
            {
                BitVector  live       = m_state_LVout[i].Clone();
                BasicBlock bb         = m_basicBlocks[i];
                Operator[] ops        = bb.Operators;
                int        opsNum     = ops.Length;
                int        opEntryIdx = ops[0].SpanningTreeIndex;
                int        opEndIdx   = opEntryIdx + opsNum;

                //
                // For each basic block, we initialize the state of each variable liveness based on the "LVout" values.
                //
                // Then, for each change of a variable, we set or clear the whole range since the last update.
                // This should touch way less bits than computing the liveness at the operator granularity
                // and then pivoting it to variable granularity.
                //
                for(int varIdx = m_varNum; --varIdx >= 0;)
                {
                    variableStateOrigin[varIdx] = opEndIdx;
                }

                for(int pos = opsNum; --pos >= 0;)
                {
                    Operator op     = ops[pos];
                    int      opIdx  = op.SpanningTreeIndex;

                    foreach(var an in op.FilterAnnotations< PostInvalidationAnnotation >())
                    {
                        if(m_variablesByAggregate != null)
                        {
                            if(ShouldSkip( an ))
                            {
                                continue;
                            }
                        }

                        int varIdx = an.Target.SpanningTreeIndex;

                        if(live.Clear( varIdx )) // State change, propagate the previous state up to this point.
                        {
                            int opStartIdx = opIdx + 1;

                            res[varIdx].SetRange( opStartIdx, variableStateOrigin[varIdx] - opStartIdx );

                            variableStateOrigin[varIdx] = opStartIdx;
                        }
                    }

                    foreach(var lhs in op.Results)
                    {
                        int varIdx = lhs.SpanningTreeIndex;

                        if(live.Clear( varIdx )) // State change, propagate the previous state up to this point.
                        {
                            int opStartIdx = opIdx + 1;

                            res[varIdx].SetRange( opStartIdx, variableStateOrigin[varIdx] - opStartIdx );

                            variableStateOrigin[varIdx] = opStartIdx;
                        }
                    }

                    if(op is PhiOperator)
                    {
                        //
                        // It would be an error to mark the inputs of a phi operator as alive.
                        // The semantic of the phi operator is that "only one of the input is valid at each invocation".
                        // We already take care of the contribution of phi operators in the computation of LVout.
                        //
                    }
                    else
                    {
                        foreach(Expression ex in op.Arguments)
                        {
                            var var = ex as VariableExpression;
                            if(var != null)
                            {
                                int varIdx = var.SpanningTreeIndex;

                                if(live.Set( varIdx )) // State change, propagate the previous state up to this point.
                                {
                                    int opStartIdx = opIdx + 1;

                                    // No need to clear the range, it's already cleared.

                                    variableStateOrigin[varIdx] = opStartIdx;
                                }
                            }
                        }

                        if(m_variablesByAggregate != null)
                        {
                            if(op is AddressAssignmentOperator)
                            {
                                var rhs = op.FirstArgument as VariableExpression;

                                if(rhs != null)
                                {
                                    foreach(var rhsAggregate in m_variablesByAggregate[rhs.SpanningTreeIndex])
                                    {
                                        var varIdx = rhsAggregate.SpanningTreeIndex;

                                        if(live.Set( varIdx )) // State change, propagate the previous state up to this point.
                                        {
                                            int opStartIdx = opIdx + 1;

                                            // No need to clear the range, it's already cleared.

                                            variableStateOrigin[varIdx] = opStartIdx;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach(var an in op.FilterAnnotations< PreInvalidationAnnotation >())
                    {
                        if(m_variablesByAggregate != null)
                        {
                            if(ShouldSkip( an ))
                            {
                                continue;
                            }
                        }

                        int varIdx = an.Target.SpanningTreeIndex;

                        if(live.Clear( varIdx )) // State change, propagate the previous state up to this point.
                        {
                            int opStartIdx = opIdx + 1;

                            res[varIdx].SetRange( opStartIdx, variableStateOrigin[varIdx] - opStartIdx );

                            variableStateOrigin[varIdx] = opStartIdx;
                        }
                    }
                }

                //
                // Finally, if the current state is "live", propagate through the entry of the basic block.
                //
                for(int varIdx = m_varNum; --varIdx >= 0;)
                {
                    if(live[varIdx])
                    {
                        res[varIdx].SetRange( opEntryIdx, variableStateOrigin[varIdx] - opEntryIdx );
                    }
                }
            }

            return res;
        }

        private static bool ShouldSkip( InvalidationAnnotation an )
        {
            var stackVar = an.Target.AliasedVariable as StackLocationExpression;
            if(stackVar != null)
            {
                switch(stackVar.StackPlacement)
                {
                    case StackLocationExpression.Placement.In   :
                    case StackLocationExpression.Placement.Local:
                        return true;
                }
            }

            return false;
        }

        //--//

        //
        // Access Methods
        //

        public BasicBlock[] BasicBlocks
        {
            get
            {
                return m_basicBlocks;
            }
        }

        public Operator[] Operators
        {
            get
            {
                return m_operators;
            }
        }

        public VariableExpression[] Variables
        {
            get
            {
                return m_variables;
            }
        }

        public BitVector[] LivenessAtBasicBlockEntry // It's indexed as [<basic block index>][<variable index>]
        {
            get
            {
                return m_state_LVin;
            }
        }

        public BitVector[] LivenessAtBasicBlockExit // It's indexed as [<basic block index>][<variable index>]
        {
            get
            {
                return m_state_LVout;
            }
        }

        public BitVector[] LivenessAtOperator // It's indexed as [<operator index>][<variable index>]
        {
            get
            {
                if(m_state_LVinAtOperator == null)
                {
                    m_state_LVinAtOperator = ConvertToOperatorGranularity();
                }

                return m_state_LVinAtOperator;
            }
        }

        public BitVector[] VariableLivenessMap // It's indexed as [<variable index>][<operator index>]
        {
            get
            {
                if(m_variableLivenessMap == null)
                {
                    m_variableLivenessMap = ConvertToVariableGranularity();;
                }

                return m_variableLivenessMap;
            }
        }
    }
}
