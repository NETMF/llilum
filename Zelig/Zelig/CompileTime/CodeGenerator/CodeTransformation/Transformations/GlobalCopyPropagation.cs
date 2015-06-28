//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    //
    // This class computes, for each program point, the set of identity assignments that reach it.
    //
    public class GlobalCopyPropagation : IDisposable
    {
        internal class SubstitutionScoreBoard
        {
            //
            // State
            //

            internal readonly Operator   m_op;
            internal readonly int        m_idxDst;
            internal readonly Expression m_exSrc;

            //
            // Constructor Methods
            //

            internal SubstitutionScoreBoard( Operator   op     ,
                                             int        idxDst ,
                                             Expression exSrc  )
            {
                m_op     = op;
                m_idxDst = idxDst;
                m_exSrc  = exSrc;
            }
        }

        //
        // State
        //

        private readonly ControlFlowGraphStateForCodeTransformation m_cfg;
        private readonly IDisposable                                m_cfgLock;

        private readonly BasicBlock[]                               m_basicBlocks;          // All the basic blocks, in spanning tree order.
        private readonly Operator[]                                 m_operators;            // All the operators, in spanning tree order.
        private readonly VariableExpression[]                       m_variables;            // All the variables, in spanning tree order.
        private readonly VariableExpression[][]                     m_variablesByStorage;   // Lookup for multi-mapped variables.
        private readonly VariableExpression.Property[]              m_varProps;
        private readonly int                                        m_bbNum;
        private readonly int                                        m_opsNum;
        private readonly int                                        m_varNum;
                                                                    
        private readonly BitVector[]                                m_variableUses;         // For each variable (spanning tree index), which operators use it.
        private readonly BitVector[]                                m_variableDefinitions;  // For each variable (spanning tree index), which operators define it.
                                                                    
        private          BitVector[]                                m_operatorKillPre;      // For each operator (spanning tree index), which variable assignments it kills BEFORE its execution.
        private          BitVector[]                                m_operatorKillPost;     // For each operator (spanning tree index), which variable assignments it kills AFTER its execution.
        private          BitVector[]                                m_operatorCopy;         // For each operator (spanning tree index), which variable assignments are available.
                                                                    
        private          BitVector[]                                m_CPin;
        private          BitVector[]                                m_CPout;
        private          BitVector[]                                m_KILL;                 // For each basic block (spanning tree index), which variable assignments it kills.
        private          BitVector[]                                m_COPY;                 // For each basic block (spanning tree index), which variable assignments are available at the exit.

        //
        // Constructor Methods
        //

        private GlobalCopyPropagation( ControlFlowGraphStateForCodeTransformation cfg )
        {
            m_cfg                 = cfg;
            m_cfgLock             = cfg.GroupLock( cfg.LockSpanningTree         () ,
                                                   cfg.LockPropertiesOfVariables() ,
                                                   cfg.LockUseDefinitionChains  () );

            m_basicBlocks         = cfg.DataFlow_SpanningTree_BasicBlocks;
            m_operators           = cfg.DataFlow_SpanningTree_Operators;
            m_variables           = cfg.DataFlow_SpanningTree_Variables;
            m_variablesByStorage  = cfg.DataFlow_SpanningTree_VariablesByStorage;
            m_varProps            = cfg.DataFlow_PropertiesOfVariables;

            m_bbNum               = m_basicBlocks.Length;
            m_opsNum              = m_operators.Length;
            m_varNum              = m_variables.Length;

            m_variableUses        = cfg.DataFlow_BitVectorsForUseChains;
            m_variableDefinitions = cfg.DataFlow_BitVectorsForDefinitionChains;

            //--//

            ComputeEquationParameters();

            SolveEquations();

            PropagateSolutions();
        }

        //
        // Helper Methods
        //

        public static bool Execute( ControlFlowGraphStateForCodeTransformation cfg )
        {
            cfg.TraceToFile( "GlobalCopyPropagation" );

            using(new PerformanceCounters.ContextualTiming( cfg, "GlobalCopyPropagation" ))
            {
                using(GlobalCopyPropagation gcp = new GlobalCopyPropagation( cfg ))
                {
                    return gcp.Apply();
                }
            }
        }

        public void Dispose()
        {
            m_cfgLock.Dispose();
        }

        private bool Apply()
        {
            TypeSystemForCodeTransformation ts                   = m_cfg.TypeSystem;
            Abstractions.Platform           pa                   = ts.PlatformAbstraction;
            Operator[]                      operators            = m_operators;
            BitVector[]                     availableCopies      = m_operatorCopy;
            BitVector                       unavailableOperators = new BitVector( operators.Length );
            List< SubstitutionScoreBoard >  workList             = null;

            int opsNum = operators.Length;

            for(int opIdx = 0; opIdx < opsNum; opIdx++)
            {
                Operator op = operators[opIdx];

                foreach(int opSrcIdx in availableCopies[opIdx])
                {
                    //
                    // Ignore operators that have been modified in this round.
                    //
                    if(unavailableOperators[opSrcIdx])
                    {
                        continue;
                    }

                    SingleAssignmentOperator opSrc = operators[opSrcIdx] as SingleAssignmentOperator;
                    Expression               exSrc = opSrc.FirstArgument;
                    TypeRepresentation       tdSrc = exSrc.Type;
                    bool                     fSkip = false;

                    foreach(Annotation an in opSrc.Annotations)
                    {
                        if(an is NotNullAnnotation)
                        {
                            if(exSrc is ConstantExpression)
                            {
                                //
                                // A constant is never null, so it's OK to ignore the annotation.
                                //
                                continue;
                            }
                        }

                        //
                        // Don't propagate copies if they carry annotations, they would be lost.
                        //
                        fSkip = true;
                        break;
                    }

                    if(fSkip)
                    {
                        continue;
                    }

                    //--//

                    for(int rhsIndex = 0; rhsIndex < op.Arguments.Length; rhsIndex++)
                    {
                        Expression exDst = op.Arguments[rhsIndex];

                        if(exDst == opSrc.FirstResult                                                                                &&
                           exDst != exSrc                                                                                            &&
                           op.CanPropagateCopy( exDst, exSrc                                                                       ) &&
                           pa.CanPropagateCopy( opSrc, op, rhsIndex, m_variables, m_variableUses, m_variableDefinitions, operators )  )
                        {
                            bool fCopy = false;

                            if(ts.GetLevel( exSrc ) == Operator.OperatorLevel.StackLocations)
                            {
                                // Don't propagate from stack locations to anywhere else, it's inefficient.
                            }
                            else if(ts.GetLevel( exDst ) <= Operator.OperatorLevel.ScalarValues &&
                                    ts.GetLevel( exSrc ) <= Operator.OperatorLevel.ScalarValues  )
                            {
                                fCopy = true;
                            }
                            else
                            {
                                TypeRepresentation tdDst = exDst.Type;

                                if(ShouldSubstitute( tdDst, tdSrc ))
                                {
                                    //
                                    // Rhs and Candidate copy are compatible.
                                    //
                                    fCopy = true;
                                }
                                else if(exSrc is ConstantExpression)
                                {
                                    if(tdDst is ReferenceTypeRepresentation                       ||
                                       tdDst is PointerTypeRepresentation                         ||
                                       tdDst.StackEquivalentType == StackEquivalentType.NativeInt  )
                                    {
                                        //
                                        // We can always assign a constant to a reference, a pointer type, or a native int.
                                        //
                                        fCopy = true;
                                    }
                                    else if(tdSrc is ScalarTypeRepresentation &&
                                            tdDst is ScalarTypeRepresentation  )
                                    {
                                        //
                                        // Convert constant to the target type.
                                        //
                                        var   exConst = (ConstantExpression)exSrc;
                                        ulong rawValue;

                                        if(exConst.GetAsRawUlong( out rawValue ))
                                        {
                                            var obj = ConstantExpression.ConvertToType( tdDst, rawValue );

                                            exSrc = m_cfg.TypeSystem.CreateConstant( tdDst, obj );
                                            fCopy = true;
                                        }
                                    }
                                }

                                if(fCopy == false)
                                {
                                    if(op is SingleAssignmentOperator)
                                    {
                                        VariableExpression lhs = op.FirstResult;

                                        if(ShouldSubstitute( lhs.Type, tdSrc ))
                                        {
                                            //
                                            // Lhs and Candidate copy are compatible, through assignment operator.
                                            //
                                            fCopy = true;
                                        }
                                    }
                                }
                            }

                            if(fCopy)
                            {
                                foreach(var an in op.FilterAnnotations< BlockCopyPropagationAnnotation >())
                                {
                                    if(an.IsResult == false && an.Index == rhsIndex)
                                    {
                                        fCopy = false;
                                        break;
                                    }
                                }
                            }

                            if(fCopy)
                            {
                                if(workList == null)
                                {
                                    workList = new List< SubstitutionScoreBoard >();
                                }

                                workList.Add( new SubstitutionScoreBoard( op, rhsIndex, exSrc ) );

                                unavailableOperators.Set( opIdx );
                            }
                        }
                    }
                }
            }

            if(workList == null)
            {
                return false;
            }

            foreach(SubstitutionScoreBoard sb in workList)
            {
                sb.m_op.SubstituteUsage( sb.m_idxDst, sb.m_exSrc );
            }

            return true;
        }

        private void ComputeEquationParameters()
        {
            //
            // Compute the KILL and COPY values for each operator.
            //
            m_operatorKillPre  = BitVector.AllocateBitVectors( m_opsNum, m_opsNum );
            m_operatorKillPost = BitVector.AllocateBitVectors( m_opsNum, m_opsNum );
            m_operatorCopy     = BitVector.AllocateBitVectors( m_opsNum, m_opsNum );

            for(int opIdx = 0; opIdx < m_opsNum; opIdx++)
            {
                Operator  op         = m_operators       [opIdx];
                BitVector opKillPre  = m_operatorKillPre [opIdx];
                BitVector opKillPost = m_operatorKillPost[opIdx];
                BitVector opCopy     = m_operatorCopy    [opIdx];

                foreach(var an in op.FilterAnnotations< PreInvalidationAnnotation >())
                {
                    KillAllReferences( opKillPre, an.Target );
                }

                if(op is ReturnControlOperator)
                {
                    //
                    // We don't want to change the expression associated with the return control operator,
                    // because it's associated with the m_returnValue field on the ControlFlowGraphState object,
                    // and m_returnValue is associated with registers or stack locations during Phase.ExpandAggregateTypes.
                    //
                    foreach(var ex in op.Arguments)
                    {
                        var ex2 = ex as VariableExpression;
                        if(ex2 != null)
                        {
                            KillAllReferences( opKillPre, ex2 );
                        }
                    }
                }

                if(op.MayWriteThroughPointerOperands)
                {
                    for(int varIdx = 0; varIdx < m_varNum; varIdx++)
                    {
                        if((m_varProps[varIdx] & VariableExpression.Property.AddressTaken) != 0)
                        {
                            KillAllReferences( opKillPost, m_variables[varIdx] );
                        }   
                    }
                }

                foreach(var lhs in op.Results)
                {
                    KillAllReferences( opKillPost, lhs );
                }

                foreach(var an in op.FilterAnnotations< PostInvalidationAnnotation >())
                {
                    KillAllReferences( opKillPost, an.Target );
                }

                if(op is SingleAssignmentOperator)
                {
                    bool fCopy = true;

                    if(op.FirstResult.AliasedVariable is LowLevelVariableExpression &&
                       op.FirstArgument               is ConstantExpression          )
                    {
                        //
                        // Don't propagate constant if the target is a low-level variable.
                        //
                        fCopy = false;
                    }

                    if(fCopy)
                    {
                        if(op.FirstResult.Type.IsFloatingPoint != op.FirstArgument.Type.IsFloatingPoint)
                        {
                            //
                            // Don't mix floating-point and integer numbers.
                            //
                            fCopy = false;
                        }
                    }

                    if(fCopy)
                    {
                        foreach(var an in op.FilterAnnotations< BlockCopyPropagationAnnotation >())
                        {
                            if(an.IsResult && an.Index == 0)
                            {
                                fCopy = false;
                                break;
                            }
                        }
                    }

                    if(fCopy)
                    {
                        opCopy.Set( op.SpanningTreeIndex );
                    }
                }
            }

            //
            // Compute the KILL and COPY values for each basic block.
            //
            m_KILL = BitVector.AllocateBitVectors( m_bbNum, m_opsNum );
            m_COPY = BitVector.AllocateBitVectors( m_bbNum, m_opsNum );

            for(int bbIdx = 0; bbIdx < m_bbNum; bbIdx++)
            {
                BasicBlock bb     = m_basicBlocks[bbIdx];
                BitVector  bbKill = m_KILL       [bbIdx];
                BitVector  bbCopy = m_COPY       [bbIdx];

                int pos = bb.Operators[0].SpanningTreeIndex;
                int len = bb.Operators.Length;

                while(--len >= 0)
                {
                    bbKill.OrInPlace        ( m_operatorKillPre[pos] );
                    bbCopy.DifferenceInPlace( m_operatorKillPre[pos] );

                    bbKill.OrInPlace        ( m_operatorKillPost[pos] );
                    bbCopy.DifferenceInPlace( m_operatorKillPost[pos] );
 
                    bbCopy.OrInPlace        ( m_operatorCopy[pos] );
                    bbKill.DifferenceInPlace( m_operatorCopy[pos] );

                    pos++;
                }
            }
        }

        private void SolveEquations()
        {
            //
            // Prepare initial state for Data Flow computation.
            //

            m_CPin  = BitVector.AllocateBitVectors( m_bbNum, m_opsNum );
            m_CPout = BitVector.AllocateBitVectors( m_bbNum, m_opsNum );

            BitVector CopyTotal = new BitVector( m_opsNum );

            for(int bbIdx = 0; bbIdx < m_bbNum; bbIdx++)
            {
                CopyTotal.OrInPlace( m_COPY[bbIdx] );
            }

            for(int bbIdx = 0; bbIdx < m_bbNum; bbIdx++)
            {
                if(m_basicBlocks[bbIdx] is EntryBasicBlock)
                {
                }
                else
                {
                    m_CPin[bbIdx].Assign( CopyTotal );
                }
            }

            //
            // Solve the Data Flow equations.
            //
            BitVector tmp = new BitVector( m_opsNum );

            while(true)
            {
                bool fDone = true;

                //
                // CPout = (CPin - Kill) Or Copy
                //
                for(int bbIdx = 0; bbIdx < m_bbNum; bbIdx++)
                {
                    tmp.Difference( m_CPin[bbIdx], m_KILL[bbIdx] );

                    tmp.OrInPlace( m_COPY[bbIdx] );

                    if(m_CPout[bbIdx] != tmp)
                    {
                        m_CPout[bbIdx].Assign( tmp );

                        fDone = false;
                    }
                }

                if(fDone)
                {
                    break;
                }

                //
                // CPin = And { CPout(<predecessors>) }
                //
                for(int bbIdx = 0; bbIdx < m_bbNum; bbIdx++)
                {
                    BitVector b      = m_CPin[bbIdx];
                    bool      fFirst = true;

                    foreach(BasicBlockEdge edge in m_basicBlocks[bbIdx].Predecessors)
                    {
                        BitVector CPpred = m_CPout[ edge.Predecessor.SpanningTreeIndex ];

                        if(fFirst)
                        {
                            fFirst = false;

                            b.Assign( CPpred );
                        }
                        else
                        {
                            b.AndInPlace( CPpred );
                        }
                    }

                    if(fFirst)
                    {
                        b.ClearAll();
                    }
                }
            }
        }

        private void PropagateSolutions()
        {
            //
            // Apply the solution to all the operators.
            //
            BitVector tmp  = new BitVector( m_opsNum );
            BitVector tmp2 = new BitVector( m_opsNum );

            for(int bbIdx = 0; bbIdx < m_bbNum; bbIdx++)
            {
                tmp.Assign( m_CPin[bbIdx] );

                BasicBlock bb = m_basicBlocks[bbIdx];

                int pos = bb.Operators[0].SpanningTreeIndex;
                int len = bb.Operators.Length;

                while(--len >= 0)
                {
                    tmp2.Assign( m_operatorCopy[pos] );

                    tmp.DifferenceInPlace( m_operatorKillPre[pos] );

                    m_operatorCopy[pos].Assign( tmp );

                    tmp.DifferenceInPlace( m_operatorKillPost[pos] );
                    tmp.OrInPlace        ( tmp2                    );

                    pos++;
                }
            }
        }

        //--//

        private void KillAllReferences( BitVector          opKill ,
                                        VariableExpression var    )
        {
            int varIdx = var.SpanningTreeIndex;

            //
            // We actually use multiple instances of physical registers and stack locations, each one with a different type.
            // But whenever we encounter one, we need to kill all of them.
            //
            foreach(VariableExpression var2 in m_variablesByStorage[varIdx])
            {
                int varIdx2 = var2.SpanningTreeIndex;

                opKill.OrInPlace( m_variableDefinitions[varIdx2] );
                opKill.OrInPlace( m_variableUses       [varIdx2] );
            }
        }

        private bool ShouldSubstitute( TypeRepresentation tdDst ,
                                       TypeRepresentation tdSrc )
        {
            if(tdDst.CanBeAssignedFrom( tdSrc, null ))
            {
                if(tdDst is PointerTypeRepresentation)
                {
                    tdDst = tdDst.UnderlyingType;
                    if(tdDst == m_cfg.TypeSystem.WellKnownTypes.System_Void)
                    {
                        //
                        // It's always OK to substitute a 'void*' with something more specific.
                        //
                        return true;
                    }

                    if(tdSrc is PointerTypeRepresentation)
                    {
                        tdSrc = tdSrc.UnderlyingType;

                        //
                        // Only substitute the pointer if the source is more specific.
                        //
                        return ShouldSubstitute( tdDst, tdSrc );
                    }
                    else
                    {
                        //
                        // If the source is not a pointer, don't make the substitution.
                        //
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
        
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

        public BitVector[] VariableUses
        {
            get
            {
                return m_variableUses;
            }
        }

        public BitVector[] VariableDefinitions
        {
            get
            {
                return m_variableDefinitions;
            }
        }

        public BitVector[] AvailableCopyAssignments
        {
            get
            {
                return m_operatorCopy;
            }
        }
    }
}
