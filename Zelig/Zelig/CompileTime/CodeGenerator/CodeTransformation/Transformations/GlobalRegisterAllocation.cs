//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


#define REGISTERALLOCATOR_SPILL_ONLY_ONE_VARIABLE
#define REGISTERALLOCATOR_REPORT_SLOW_METHODS


namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    //
    // This class converts PseudoRegisters into PhysicalRegisters or moves the variables to stack locations.
    //
    public class GlobalRegisterAllocation
    {
        internal enum WebFlavor
        {
            Must  ,
            Should,
            May   ,
        }

        internal class WebPair
        {
            //
            // State
            //

            internal InterferenceNode m_node1;
            internal InterferenceNode m_node2;
            internal WebFlavor        m_flavor;

            //
            // Constructor Methods
            //

            internal WebPair( InterferenceNode node1  ,
                              InterferenceNode node2  ,
                              WebFlavor        flavor )
            {
                m_node1  = node1;
                m_node2  = node2;
                m_flavor = flavor;
            }

            //
            // Helper methods
            //

            internal void MaximizeFlavor( WebFlavor flavor )
            {
                if(m_flavor > flavor)
                {
                    m_flavor = flavor;
                }
            }
            
            internal bool IsMatch( InterferenceNode node )
            {
                return m_node1 == node || m_node2 == node;
            }

            internal int SpillCost()
            {
                return m_node1.m_spillCost + m_node2.m_spillCost;
            }

            internal void Remove()
            {
                m_node1.m_coalesceWeb.Remove( this );
                m_node2.m_coalesceWeb.Remove( this );
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                return string.Format( "{0} <-> {1} as {2}", m_node1.m_source, m_node2.m_source, m_flavor );
            }
        }

        internal class InterferenceNode
        {
            //
            // State
            //

            internal int                        m_index;
            internal VariableExpression         m_source;
            internal Operator                   m_definition; // We are in SSA form, at most there's one definition.
            internal Operator[]                 m_uses;
            internal BitVector                  m_compatibleRegs;
            internal List< WebPair >            m_coalesceWeb;

            internal PhysicalRegisterExpression m_fixed;
            internal PhysicalRegisterExpression m_assigned;
            internal int                        m_candidateColor;

            internal BitVector                  m_edges;
            internal BitVector                  m_liveness;
            internal int                        m_livenessLow;
            internal int                        m_livenessHigh;
            internal int                        m_degree;
                                        
            internal InterferenceNode           m_previousStackElement;

            internal int                        m_spillCost;
            internal bool                       m_spillCandidate;
            internal bool                       m_spillWouldCreateNewWebs;

            //
            // Constructor Methods
            //

            internal InterferenceNode() // Used as an End-Of-Stack sentinel.
            {
            }

            internal InterferenceNode( int                index       ,
                                       VariableExpression source      ,
                                       Operator           definition  ,
                                       Operator[]         uses        ,
                                       BitVector[]        livenessMap )
            {
                m_index          = index;
                m_source         = source;
                m_definition     = definition;
                m_uses           = uses;
                m_compatibleRegs = new BitVector();
                m_coalesceWeb    = new List< WebPair >();

                m_liveness = livenessMap[source.SpanningTreeIndex].Clone();

                //
                // This takes care of registers used in calling convention.
                //
                m_fixed    = source.AliasedVariable as PhysicalRegisterExpression;
                m_assigned = m_fixed;
            }

            //
            // Helper methods
            //

            internal Abstractions.RegisterClass ComputeConstraintsForLHS()
            {
                return RegisterAllocationConstraintAnnotation.ComputeConstraintsForLHS( m_definition, m_source );
            }

            internal Abstractions.RegisterClass ComputeConstraintsForRHS()
            {
                Abstractions.RegisterClass constraint = Abstractions.RegisterClass.None;

                foreach(Operator use in m_uses)
                {
                    constraint |= RegisterAllocationConstraintAnnotation.ComputeConstraintsForRHS( use, m_source );
                }

                return constraint;
            }

            internal void SubstituteDefinition( GlobalRegisterAllocation owner )
            {
                owner.SubstituteDefinition( m_definition, m_source, false );
            }

            internal void SubstituteUsage( GlobalRegisterAllocation owner )
            {
                foreach(Operator use in m_uses)
                {
                    owner.SubstituteUsage( use, m_source, false );
                }
            }

            //--//

            internal WebPair FindPair( InterferenceNode node )
            {
                foreach(var pair in m_coalesceWeb)
                {
                    if(pair.IsMatch( this ) && pair.IsMatch( node ))
                    {
                        return pair;
                    }
                }

                return null;
            }

            //--//

            internal void ResetCandidate()
            {
                if(m_assigned != null)
                {
                    m_candidateColor = m_assigned.RegisterDescriptor.Index;
                }
                else
                {
                    m_candidateColor = -1;
                }
            }

            internal void UpdateAdjacencies( InterferenceNode[] interferenceGraph )
            {
                foreach(int idxEdge in m_edges)
                {
                    InterferenceNode nodeEdge = interferenceGraph[idxEdge];

                    nodeEdge.m_degree--;
                }
            }

            internal void Push(     InterferenceNode[] interferenceGraph ,
                                ref InterferenceNode   lastStackElement  ,
                                    bool               spillCandidate    )
            {
                m_spillCandidate       = spillCandidate;

                m_previousStackElement = lastStackElement;
                lastStackElement       = this;

                UpdateAdjacencies( interferenceGraph );
            }

            internal bool HasLowerRelativeCost( InterferenceNode otherNode )
            {
                int relCostThis  = this     .m_spillCost * otherNode.m_edges.Cardinality;
                int relCostOther = otherNode.m_spillCost * this     .m_edges.Cardinality;

                return relCostThis < relCostOther;
            }

            internal bool LivenessOverlapsWith( InterferenceNode otherNode )
            {
                if(this     .m_livenessHigh < otherNode.m_livenessLow ||
                   otherNode.m_livenessHigh < this     .m_livenessLow  )
                {
                    //
                    // Disjoint live ranges for sure.
                    //
                    return false;
                }

                return this.m_liveness.IsIntersectionEmpty( otherNode.m_liveness ) == false;
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.AppendFormat( "N.{0} {1}", m_index, m_source );

                if(m_edges != null)
                {
                    sb.AppendFormat( " => Edges {0}", m_edges );
                }

                if(m_coalesceWeb.Count > 0)
                {
                    sb.Append( " => Web [" );

                    foreach(var pair in m_coalesceWeb)
                    {
                        sb.AppendFormat( " {0} ", pair );
                    }

                    sb.Append( "]" );
                }

                if(m_fixed != null)
                {
                    sb.AppendFormat( " => Fixed as {0}", m_fixed );
                }

                if(m_assigned != null)
                {
                    sb.AppendFormat( " => Assigned to {0}", m_assigned );
                }

                if(m_candidateColor >= 0)
                {
                    sb.AppendFormat( " => Candidate for {0}", m_candidateColor );
                }

                return sb.ToString();
            }
        }

        //
        // State
        //

        private readonly ControlFlowGraphStateForCodeTransformation m_cfg;
        private readonly Abstractions.RegisterDescriptor[]          m_registers;
        private readonly InterferenceNode                           m_stackSentinel;
        private          GrowOnlySet< VariableExpression >          m_spillHistory;

        private          Operator[][]                               m_defChains;
        private          Operator[][]                               m_useChains;

        private          int                                        m_interferenceGraphSize;
        private          InterferenceNode[]                         m_interferenceGraph;
        private          InterferenceNode[]                         m_interferenceGraphLookup;

        private          DataFlow.ControlTree.NaturalLoops          m_naturalLoops;

        //
        // Constructor Methods
        //

        public GlobalRegisterAllocation( ControlFlowGraphStateForCodeTransformation cfg )
        {
            m_cfg = cfg;

            //--//

            //
            // Get list of possible register candidates.
            //
            m_registers = cfg.TypeSystem.PlatformAbstraction.GetRegisters();

            //--//

            m_stackSentinel = new InterferenceNode();
        }

        //
        // Helper Methods
        //

        public static void Execute( ControlFlowGraphStateForCodeTransformation cfg )
        {
            using(new PerformanceCounters.ContextualTiming( cfg, "GlobalRegisterAllocation" ))
            {
                GlobalRegisterAllocation gra = new GlobalRegisterAllocation( cfg );

                gra.PerformAllocation();
            }
        }

        private void PerformAllocation()
        {
#if REGISTERALLOCATOR_REPORT_SLOW_METHODS
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Start();

            int iterations = 0;
#endif

            //--//

            while(true)
            {
#if REGISTERALLOCATOR_REPORT_SLOW_METHODS
                iterations++;
#endif

                DisposeSpillCosts();

                //--//

                m_cfg.TraceToFile( "RegisterAllocation-Loop" );

                CHECKS.ASSERT( Transformations.StaticSingleAssignmentForm.ShouldTransformInto( m_cfg ) == false, "Control Flow Graph no longer in SSA form" );


                //
                // 1) Ensure all the operators with register constraints have pseudo registers as parameters, not stack locations.
                //

                Operator[] operators = m_cfg.DataFlow_SpanningTree_Operators;
                bool       fGot      = false;
                int opLen = operators.Length;

                //foreach(Operator op in operators)
                for(int idxOp = 0; idxOp < opLen; idxOp++)
                {
                    Operator op = operators[idxOp];
                    int opResLen = op.Results.Length;

                    for(int idx = 0; idx < opResLen; idx++)
                    {
                        if(RegisterAllocationConstraintAnnotation.ShouldLhsBeMovedToPseudoRegister( op, idx ))
                        {
                            SubstituteDefinition( op, op.Results[idx], false );
                            fGot = true;
                        }
                    }

                    opResLen = op.Arguments.Length;

                    for(int idx = 0; idx < opResLen; idx++)
                    {
                        if(RegisterAllocationConstraintAnnotation.ShouldRhsBeMovedToPseudoRegister( op, idx ))
                        {
                            SubstituteUsage( op, (VariableExpression)op.Arguments[idx], false );
                            fGot = true;
                        }
                    }

                    //
                    // If an operator has some coupling constraints, we create temporary variables for the expressions involved in the constraint.
                    // This relies some of the pressure from the graph coloring algorithm, since these constraints add 'MUST' web edges.
                    //
                    foreach(var an in op.FilterAnnotations< RegisterCouplingConstraintAnnotation >())
                    {
                        if(an.IsResult1)
                        {
                            var ex = op.Results[an.VarIndex1];

                            if(!(ex is PseudoRegisterExpression))
                            {
                                SubstituteDefinition( op, ex, true );
                                fGot = true;
                            }
                        }
                        else
                        {
                            var ex = (VariableExpression)op.Arguments[an.VarIndex1];

                            if(!(ex is PseudoRegisterExpression))
                            {
                                SubstituteUsage( op, ex, true );
                                fGot = true;
                            }
                        }

                        if(an.IsResult2)
                        {
                            var ex = op.Results[an.VarIndex2];

                            if(!(ex is PseudoRegisterExpression))
                            {
                                SubstituteDefinition( op, ex, true );
                                fGot = true;
                            }
                        }
                        else
                        {
                            var ex = (VariableExpression)op.Arguments[an.VarIndex2];

                            if(!(ex is PseudoRegisterExpression))
                            {
                                SubstituteUsage( op, ex, true );
                                fGot = true;
                            }
                        }
                    }
                }

                if(fGot)
                {
                    continue;
                }

                //
                // 1b) Create Use chains and liveness analysis.
                //

                VariableExpression[] variables    = m_cfg.DataFlow_SpanningTree_Variables;
                BitVector[]          livenessMap  = m_cfg.DataFlow_VariableLivenessMap; // It's indexed as Operator[<variable index>][<operator index>]

                //
                // 2) Build interference graph. Any variables that have overlapping liveness are set to interfere.
                //
                using(new PerformanceCounters.ContextualTiming( m_cfg, "BuildInterferenceGraph" ))
                {
                    m_interferenceGraphSize   = 0;
                    m_interferenceGraph       = new InterferenceNode[variables.Length];
                    m_interferenceGraphLookup = new InterferenceNode[variables.Length];
                    m_defChains               = m_cfg.DataFlow_DefinitionChains;
                    m_useChains               = m_cfg.DataFlow_UseChains;

                    bool fRestart = false;

                    int varLen = variables.Length;

                    //foreach(VariableExpression var in variables)
                    for(int idxVar = 0; idxVar < varLen; idxVar++)
                    {
                        VariableExpression var = variables[idxVar];

                        Operator[] defChain = m_defChains[var.SpanningTreeIndex];

                        CHECKS.ASSERT( defChain.Length <= 1, "Not in SSA form!" );

                        VariableExpression varAliased = var.AliasedVariable;
                        bool               fInclude   = false;

                        if(varAliased is PhysicalRegisterExpression)
                        {
                            fInclude = true;
                        }
                        else if(varAliased is PseudoRegisterExpression)
                        {
                            //
                            // Some variables are not actually defined, because:
                            //   1) they are just aliases for low-level expressions,
                            //   2) they are passed as inputs.
                            //
                            if(defChain.Length == 1 || ((PseudoRegisterExpression)varAliased).SourceVariable.Type.FullName.Equals( "Microsoft.Zelig.Runtime.TypeSystem.CodePointer" ))
                            {
                                fInclude = true;
                            }
                            else
                            {
                                CHECKS.ASSERT( m_useChains[var.SpanningTreeIndex].Length == 0, "Cannot have a use of an undefined non-argument variable." );
                            }
                        }

                        if(fInclude)
                        {
                            InterferenceNode node = new InterferenceNode( m_interferenceGraphSize, var, defChain.Length == 1 ? defChain[0] : null, m_useChains[var.SpanningTreeIndex], livenessMap );

                            //--//

                            Abstractions.RegisterClass constraintLHS = node.ComputeConstraintsForLHS();
                            Abstractions.RegisterClass constraintRHS = node.ComputeConstraintsForRHS();
                            Abstractions.RegisterClass constraint    = constraintLHS | constraintRHS;
                            bool                       fGotLHS       = false;
                            int regLen = m_registers.Length;

                            //foreach(Abstractions.RegisterDescriptor regDesc in m_registers)
                            for(int idxReg = 0; idxReg < regLen; idxReg++)
                            {
                                Abstractions.RegisterDescriptor regDesc = m_registers[idxReg];

                                if(regDesc.CanAllocate && regDesc.PhysicalStorageSize == node.m_source.Type.SizeOfHoldingVariableInWords)
                                {
                                    if((regDesc.ComputeCapabilities & constraintLHS) == constraintLHS)
                                    {
                                        fGotLHS = true;
                                    }

                                    if((regDesc.ComputeCapabilities & constraint) == constraint)
                                    {
                                        node.m_compatibleRegs.Set( regDesc.Index );
                                    }
                                }
                            }

                            //
                            // If no compatible registers, we need to reassign.
                            //
                            if(node.m_compatibleRegs.Cardinality == 0)
                            {
                                fRestart = true;
                            }

                            //
                            // If the variable is already assigned to a physical register,
                            // it has to be either a compatible register or a register that cannot be allocated (special usage registers).
                            //
                            if(node.m_fixed != null)
                            {
                                int idx = node.m_fixed.Number;

                                if(m_registers[idx].CanAllocate)
                                {
                                    if(node.m_compatibleRegs[idx] == false)
                                    {
                                        fRestart = true;
                                    }
                                }
                            }

                            if(fRestart)
                            {
                                if(fGotLHS == false)
                                {
                                    node.SubstituteDefinition( this );
                                }
                                else
                                {
                                    node.SubstituteUsage( this );
                                }

                                break;
                            }

                            m_interferenceGraph[m_interferenceGraphSize++] = node;

                            m_interferenceGraphLookup[var.SpanningTreeIndex] = node;
                        }
                    }

                    if(fRestart)
                    {
                        continue;
                    }

                    //--//

                    if(m_interferenceGraphSize != 0)
                    {
                        for(int idx = 0; idx < m_interferenceGraphSize; idx++)
                        {
                            InterferenceNode node = m_interferenceGraph[idx];

                            node.m_edges = new BitVector( m_interferenceGraphSize );
                        }

                        //--//

                        //
                        // The data from the liveness analysis doesn't work directly for building the interference graph,
                        // because if a register is invalidated as part of the calling convention but not used in the method,
                        // it won't be marked as alive by the liveness analysis (correctly, since it's not used),
                        // although it should be added to the interference set so that we don't assign scratch registers across method calls.
                        //
                        // Same thing for all the variables that are defined but not used, because their definition leads to a side-effect.
                        //
                        BitVector done = new BitVector();
                        BitVector tmp  = new BitVector( opLen );

                        for(int idx = 0; idx < opLen; idx++)
                        {
                            Operator op = operators[idx];

                            foreach(var an in op.FilterAnnotations< PreInvalidationAnnotation >())
                            {
                                var reg = an.Target.AliasedVariable as PhysicalRegisterExpression;
                                if(reg != null && done[reg.Number] == false)
                                {
                                    done[reg.Number] = true;

                                    tmp.ClearAll();


                                    for(int idx2 = idx; idx2 < opLen; idx2++)
                                    {
                                        foreach(var an2 in operators[idx2].FilterAnnotations< InvalidationAnnotation >())
                                        {
                                            var reg2 = an2.Target.AliasedVariable as PhysicalRegisterExpression;
                                            if(reg2 != null)
                                            {
                                                if(reg2.Number == reg.Number)
                                                {
                                                    tmp.Set( idx2 );
                                                }
                                            }
                                        }
                                    }

                                    for(int idx2 = 0; idx2 < m_interferenceGraphSize; idx2++)
                                    {
                                        InterferenceNode node = m_interferenceGraph[idx2];

                                        if(node.m_fixed == reg)
                                        {
                                            node.m_liveness.OrInPlace( tmp );
                                        }
                                    }
                                }
                            }

                            int resLen = op.Results.Length;

                            //foreach(var lhs in op.Results)
                            for(int lIdx = 0; lIdx < resLen; lIdx++)
                            {
                                var lhs = op.Results[lIdx];

                                int lhsIdx = lhs.SpanningTreeIndex;

                                if(m_useChains[lhsIdx].Length == 0)
                                {
                                    InterferenceNode node = m_interferenceGraphLookup[lhsIdx];

                                    if(node != null)
                                    {
                                        node.m_liveness.Set( idx );
                                    }
                                }
                            }

                            foreach(var an in op.FilterAnnotations< PostInvalidationAnnotation >())
                            {
                                var reg = an.Target.AliasedVariable as PhysicalRegisterExpression;
                                if(reg != null && done[reg.Number] == false)
                                {
                                    done[reg.Number] = true;

                                    tmp.ClearAll();

                                    for(int idx2 = idx; idx2 < opLen; idx2++)
                                    {
                                        foreach(var an2 in operators[idx2].FilterAnnotations< InvalidationAnnotation >())
                                        {
                                            var reg2 = an2.Target.AliasedVariable as PhysicalRegisterExpression;
                                            if(reg2 != null)
                                            {
                                                if(reg2.Number == reg.Number)
                                                {
                                                    tmp.Set( idx2 );
                                                }
                                            }
                                        }
                                    }

                                    for(int idx2 = 0; idx2 < m_interferenceGraphSize; idx2++)
                                    {
                                        InterferenceNode node = m_interferenceGraph[idx2];

                                        if(node.m_fixed == reg)
                                        {
                                            node.m_liveness.OrInPlace( tmp );
                                        }
                                    }
                                }
                            }
                        }

                        for(int idx = 0; idx < m_interferenceGraphSize; idx++)
                        {
                            InterferenceNode node = m_interferenceGraph[idx];

                            node.m_liveness.GetRange( out node.m_livenessLow, out node.m_livenessHigh );
                        }
                    }
                }

                if(m_interferenceGraphSize == 0)
                {
                    //
                    // Nothing to do, probably because there's nothing to compute in this method...
                    //
                    break;
                }

                //--//

                //
                // 3) Create adjacency information for all the pseudo registers and physical registers.
                //
                using(new PerformanceCounters.ContextualTiming( m_cfg, "ComputeAdjacencyMatrix" ))
                {
                    for(int idx = 0; idx < m_interferenceGraphSize; idx++)
                    {
                        InterferenceNode node = m_interferenceGraph[idx];

                        for(int idx2 = idx + 1; idx2 < m_interferenceGraphSize; idx2++)
                        {
                            InterferenceNode node2 = m_interferenceGraph[idx2];

                            if(node.LivenessOverlapsWith( node2 ))
                            {
                                //
                                // The two nodes interfere, so create an edge between them.
                                //
                                node .m_edges.Set( node2.m_index );
                                node2.m_edges.Set( node .m_index );
                            }
                        }

                        node.m_degree = node.m_edges.Cardinality;
                    }
                }


                //
                // 4) Is the graph N-colorable? If not, select a candidate for spilling, update the interference graph and repeat.
                //
                // Look for any node interfering with less than N other nodes.
                // Remove it from the graph, put it on a stack, update the edges of the other nodes.
                // Repeat until no more such nodes are left in the graph.
                // If at the end the graph has no nodes, it's colorable.
                //

                InterferenceNode lastStackElement = m_stackSentinel;

                for(int nodesToPush = m_interferenceGraphSize; nodesToPush > 0; )
                {
                    bool fSpilled = false;

                    for(int idx = 0; idx < m_interferenceGraphSize; idx++)
                    {
                        InterferenceNode node = m_interferenceGraph[idx];

                        if(node.m_previousStackElement == null)
                        {
                            if(node.m_degree < node.m_compatibleRegs.Cardinality)
                            {
                                node.Push( m_interferenceGraph, ref lastStackElement, false );

                                fSpilled = true;
                                nodesToPush--;
                            }
                        }
                    }

                    if(fSpilled == false)
                    {
                        //
                        // Coulnd't find any node with degree < N.
                        // Select a node with the lowest spill cost, push it as a candidate and continue.
                        //
                        InterferenceNode nodeToSpill = SelectNodeToSpillOptimistically();
                        
                        nodeToSpill.Push( m_interferenceGraph, ref lastStackElement, true );

                        nodesToPush--;
                    }
                }

                if(ColorGraph( lastStackElement ))
                {
                    //
                    // 5) Assign pseudo registers to physical registers.
                    //

                    //
                    // We have a coloring. Substitute registers for candidate variables.
                    //
                    foreach(Operator op in m_cfg.DataFlow_SpanningTree_Operators)
                    {
                        ConvertToPhysicalRegisters( op );
                    }

                    break;
                }
                else
                {
                    //
                    // Failed to color, spill all interfering nodes, selecting the least expensive ones.
                    //
                    SpillVariables();
                }
            }

            DisposeSpillCosts();

            m_cfg.TraceToFile( "RegisterAllocation-Done" );

            while(Transformations.CommonMethodRedundancyElimination.Execute( m_cfg ))
            {
            }

            Transformations.StaticSingleAssignmentForm.ConvertOut( m_cfg, false );

            m_cfg.TraceToFile( "RegisterAllocation-Post" );

            //
            // Remove temporary copy propagation blocks.
            //
            int stoLen = m_cfg.DataFlow_SpanningTree_Operators.Length;
            //foreach(Operator op in m_cfg.DataFlow_SpanningTree_Operators)
            for(int idxSto = 0; idxSto < stoLen; idxSto++)
            {
                Operator op = m_cfg.DataFlow_SpanningTree_Operators[idxSto];

                foreach(var an in op.FilterAnnotations< BlockCopyPropagationAnnotation >())
                {
                    if(an.IsTemporary)
                    {
                        op.RemoveAnnotation( an );
                    }
                }
            }

#if REGISTERALLOCATOR_REPORT_SLOW_METHODS
            sw.Stop();

            int totalSeconds = (int)(sw.ElapsedMilliseconds / 1000);

            if(totalSeconds >= 2)
            {
                Console.WriteLine( "NOTICE: Register allocation for method '{0}' took {1} seconds and {2} iterations! [{3} variables and {4} operators]", m_cfg.Method.ToShortString(), totalSeconds, iterations, m_cfg.DataFlow_SpanningTree_Variables.Length, m_cfg.DataFlow_SpanningTree_Operators.Length );
                Console.WriteLine( "NOTICE: Interference graph for method '{0}' has {1} nodes!"                                                         , m_cfg.Method.ToShortString(), m_interferenceGraphSize                                                                                              );
                Console.WriteLine();
            }
#endif
        }

        //--//

        private int EstimateCostOfOperator( Operator op )
        {
            if(op == null)
            {
                return 0;
            }

            int depth = m_naturalLoops.GetDepthOfBasicBlock( op.BasicBlock );

            return (int)Math.Pow( 8, depth );
        }

        private void DisposeSpillCosts()
        {
            if(m_naturalLoops != null)
            {
                m_naturalLoops.Dispose();
                m_naturalLoops = null;
            }
        }

        private void ComputeSpillCosts()
        {
            if(m_naturalLoops == null)
            {
                m_naturalLoops = DataFlow.ControlTree.NaturalLoops.Execute( m_cfg );

                //--//

                Abstractions.Platform pa              = m_cfg.TypeSystem.PlatformAbstraction;
                int                   cost_definition = pa.EstimatedCostOfStoreOperation;
                int                   cost_use        = pa.EstimatedCostOfLoadOperation;

                using(new PerformanceCounters.ContextualTiming( m_cfg, "ComputeSpillCosts" ))
                {
                    for(int idx = 0; idx < m_interferenceGraphSize; idx++)
                    {
                        InterferenceNode node = m_interferenceGraph[idx];
                        int              cost = 0;

                        foreach(Operator def in m_defChains[node.m_source.SpanningTreeIndex])
                        {
                            cost += cost_definition * EstimateCostOfOperator( def );
                        }

                        foreach(Operator use in m_useChains[node.m_source.SpanningTreeIndex])
                        {
                            cost += cost_use * EstimateCostOfOperator( use );
                        }

                        node.m_spillCost = cost;

                        //--//

                        //
                        // It makes sense to consider the node as a spill candidate
                        // only if removing it would improve the chances of coloring the graph.
                        //
                        BitVector postLiveness = new BitVector();

                        if(node.m_definition != null)
                        {
                            postLiveness.Set( node.m_definition.SpanningTreeIndex );
                        }

                        foreach(Operator use in node.m_uses)
                        {
                            postLiveness.Set( use.SpanningTreeIndex );
                        }

                        node.m_spillWouldCreateNewWebs = false;
                        if(node.m_fixed == null)
                        {
                            foreach(int idxEdge in node.m_edges)
                            {
                                InterferenceNode otherNode = m_interferenceGraph[idxEdge];

                                if(postLiveness.IsIntersectionEmpty( otherNode.m_liveness ))
                                {
                                    node.m_spillWouldCreateNewWebs = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private InterferenceNode SelectNodeToSpillOptimistically()
        {
            ComputeSpillCosts();

            InterferenceNode lastNode      = null;
            InterferenceNode firstPhysical = null;

            for(int idx = 0; idx < m_interferenceGraphSize; idx++)
            {
                InterferenceNode node = m_interferenceGraph[idx];

                //
                // Only look for nodes not in the stack yet.
                //
                if(node.m_previousStackElement == null)
                {
                    if(node.m_fixed != null)
                    {
                        //
                        // Just remember the first node associated with a physical register, but don't include them in the estimation.
                        //
                        if(firstPhysical == null)
                        {
                            firstPhysical = node;
                        }
                    }
                    else
                    {
                        if(lastNode == null || node.HasLowerRelativeCost( lastNode ))
                        {
                            lastNode = node;
                        }
                    }
                }
            }

            if(lastNode == null)
            {
                //
                // We didn't find a node with a pseudo register, but we need to push something, so we push a physical register.
                //
                lastNode = firstPhysical;
            }

            CHECKS.ASSERT( lastNode != null, "Cannot find a candidate for spilling, even if the graph is not colorable" );

            return lastNode;
        }

        //--//

        private void SpillVariables()
        {
            ComputeSpillCosts();

            if(m_spillHistory == null)
            {
                m_spillHistory = SetFactory.NewWithReferenceEquality< VariableExpression >();
            }

            InterferenceNode[] candidates = new InterferenceNode[m_interferenceGraphSize];

            for(int idx = 0; idx < m_interferenceGraphSize; idx++)
            {
                InterferenceNode node = m_interferenceGraph[idx];

                if(node.m_fixed != null)
                {
                    continue;
                }

                if(m_spillHistory.Contains( node.m_source ))
                {
                    continue;
                }

                if(node.m_edges.Cardinality < node.m_compatibleRegs.Cardinality && node.m_candidateColor >= 0)
                {
                    //
                    // This node can be colored for sure, because it interferes with less nodes than available registers.
                    //
                    continue;
                }

                candidates[idx] = node;
            }

            int iSpilled = 0;

            while(true)
            {
                InterferenceNode nodeToSpill = SelectNodeToSpill( candidates );
                if(nodeToSpill == null)
                {
                    break;
                }

                iSpilled++;

                SpillVariable( candidates, nodeToSpill );

#if REGISTERALLOCATOR_SPILL_ONLY_ONE_VARIABLE
                break;
#endif
            }

            if(iSpilled == 0)
            {
                throw TypeConsistencyErrorException.Create( "Failed to find a pseudo register to spill in {0}", m_cfg );
            }
        }

        private void SpillVariable( InterferenceNode[] candidates ,
                                    InterferenceNode   node       )
        {
            Operator                 opDef    = node.m_definition;
            VariableExpression       var      = node.m_source;
            PseudoRegisterExpression source   = (PseudoRegisterExpression)var.AliasedVariable;
            StackLocationExpression  stackVar = null;
            ConstantExpression       constVar = null;
            Expression               targetVar;

            //--//

            m_spillHistory.Insert( var );

            //
            // Remove this node and all the nodes it interferes with from the set of variables viable for spilling.
            //
            candidates[node.m_index] = null;

            foreach(int idxEdge in node.m_edges)
            {
                InterferenceNode otherNode = m_interferenceGraph[idxEdge];

                candidates[otherNode.m_index] = null;
            }

            //--//

            //
            // Is this an assignment from an already spilled variables?
            // If so, we don't need to create a new variable.
            //
            if(opDef is SingleAssignmentOperator)
            {
                var rhs = opDef.FirstArgument;

                stackVar = rhs as StackLocationExpression;
                constVar = rhs as ConstantExpression;

                if(stackVar != null)
                {
                    if(m_spillHistory.Contains( stackVar ) == false)
                    {
                        stackVar = null;
                    }
                }
            }

            if(constVar != null)
            {
                //
                // No need to spill, it's a constant.
                //
                opDef.Delete();
    
                targetVar = constVar;
            }
            else if(stackVar == null)
            {
                //
                // Don't associate the spill variable with the source variable, it would create a wrong alias for aggregate types.
                //
                stackVar = m_cfg.AllocateLocalStackLocation( source.Type, source.DebugName, null, 0 );

                if(opDef is SingleAssignmentOperator ||
                   opDef is PhiOperator               )
                {
                    opDef.SubstituteDefinition( var, stackVar );
                }
                else
                {
                    opDef.AddOperatorAfter( SingleAssignmentOperator.New( opDef.DebugInfo, stackVar, var ) );
                }

                m_spillHistory.Insert( stackVar );

                targetVar = stackVar;
            }
            else
            {
                opDef.Delete();

                targetVar = stackVar;
            }

            //--//

            foreach(Operator opUse in m_useChains[var.SpanningTreeIndex])
            {
                if(opUse is SingleAssignmentOperator ||
                   opUse is PhiOperator               )
                {
                    //
                    // No need to create a pseudo register and then assign it to the left side. Just substitute the usage.
                    //
                    opUse.SubstituteUsage( var, targetVar );
                }
                else
                {
                    PseudoRegisterExpression tmp = m_cfg.AllocatePseudoRegister( source.Type, source.DebugName, source.SourceVariable, source.SourceOffset );

                    opUse.AddOperatorBefore( SingleAssignmentOperator.New( opUse.DebugInfo, tmp, targetVar ) );

                    opUse.SubstituteUsage( var, tmp );

                    m_spillHistory.Insert( tmp );
                }
            }
        }

        private InterferenceNode SelectNodeToSpill( InterferenceNode[] candidates )
        {
            InterferenceNode lastNode;

            lastNode = SelectNodeToSpill( candidates, true , true , false ); if(lastNode != null) return lastNode;
            lastNode = SelectNodeToSpill( candidates, true , false, true  ); if(lastNode != null) return lastNode;
            lastNode = SelectNodeToSpill( candidates, true , false, false ); if(lastNode != null) return lastNode;
            lastNode = SelectNodeToSpill( candidates, false, true , false ); if(lastNode != null) return lastNode;
            lastNode = SelectNodeToSpill( candidates, false, false, true  ); if(lastNode != null) return lastNode;
            lastNode = SelectNodeToSpill( candidates, false, false, false );
            
            return lastNode;
        }

        private InterferenceNode SelectNodeToSpill( InterferenceNode[] candidates                   ,
                                                    bool               fOnlyConsiderNewWebs         ,
                                                    bool               fOnlyConsiderSpillCandidates ,
                                                    bool               fOnlyConsiderFailures        )
        {
            InterferenceNode lastNode = null;

            for(int idx = 0; idx < candidates.Length; idx++)
            {
                InterferenceNode node = candidates[idx];

                if(node == null)
                {
                    continue;
                }

                //
                // Only look at the nodes that, if spilled, would create a new web?
                //
                if(fOnlyConsiderNewWebs && node.m_spillWouldCreateNewWebs == false)
                {
                    continue;
                }

                //
                // Only look at the nodes that have been optimistically pushed on the stack?
                //
                if(fOnlyConsiderSpillCandidates && node.m_spillCandidate == false)
                {
                    continue;
                }

                //
                // Only look at the nodes that failed to be assigned a color?
                //
                if(fOnlyConsiderFailures && node.m_candidateColor >= 0)
                {
                    continue;
                }

                if(lastNode == null || node.HasLowerRelativeCost( lastNode ))
                {
                    lastNode = node;
                }
            }

            return lastNode;
        }

        //--//

        private void ConvertToPhysicalRegisters( Operator op )
        {
            foreach(var lhs in op.Results)
            {
                var node = GetInterferenceNode( lhs );
                if(node != null)
                {
                    if(lhs != node.m_assigned)
                    {
                        op.SubstituteDefinition( lhs, node.m_assigned );
                    }
                }
            }

            foreach(var rhs in op.Arguments)
            {
                var node = GetInterferenceNode( rhs );
                if(node != null)
                {
                    if(rhs != node.m_assigned)
                    {
                        op.SubstituteUsage( rhs, node.m_assigned );
                    }
                }
            }
        }

        private InterferenceNode GetInterferenceNode( Expression ex )
        {
            if(ex is VariableExpression)
            {
                return m_interferenceGraphLookup[ex.SpanningTreeIndex];
            }

            return null;
        }

        //--//

        private void SubstituteDefinition( Operator           op                    ,
                                           VariableExpression var                   ,
                                           bool               fBlockCopyPropagation )
        {
            LowLevelVariableExpression source = var as LowLevelVariableExpression;
            PseudoRegisterExpression   tmp    = m_cfg.AllocatePseudoRegister( var.Type, var.DebugName, source != null ? source.SourceVariable : null, source != null ? source.SourceOffset : 0 );

            op.SubstituteDefinition( var, tmp );

            var opNew = SingleAssignmentOperator.New( op.DebugInfo, var, tmp );

            if(fBlockCopyPropagation)
            {
                opNew.AddAnnotation( BlockCopyPropagationAnnotation.Create( m_cfg.TypeSystem, 0, false, true ) );
            }

            op.AddOperatorAfter( opNew );
        }

        private void SubstituteUsage( Operator           op                    ,
                                      VariableExpression var                   ,
                                      bool               fBlockCopyPropagation )
        {
            LowLevelVariableExpression source         = var as LowLevelVariableExpression;
            var                        sourceVariable = source != null ? source.SourceVariable : null;
            var                        sourceOffset   = source != null ? source.SourceOffset   : 0;

            if(op is PhiOperator)
            {
                PhiOperator opPhi   = (PhiOperator)op;
                var         rhs     = opPhi.Arguments;
                var         origins = opPhi.Origins;

                for(int i = 0; i < rhs.Length; i++)
                {
                    if(rhs[i] == var)
                    {
                        PseudoRegisterExpression tmp = m_cfg.AllocatePseudoRegister( var.Type, var.DebugName, sourceVariable, sourceOffset );

                        var opNew = SingleAssignmentOperator.New( op.DebugInfo, tmp, var );

                        if(fBlockCopyPropagation)
                        {
                            opNew.AddAnnotation( BlockCopyPropagationAnnotation.Create( m_cfg.TypeSystem, 0, true, true ) );
                        }

                        origins[i].AddOperator( opNew );

                        op.SubstituteUsage( i, tmp );

                    }
                }
            }
            else
            {
                PseudoRegisterExpression tmp = m_cfg.AllocatePseudoRegister( var.Type, var.DebugName, sourceVariable, sourceOffset );

                var opNew = SingleAssignmentOperator.New( op.DebugInfo, tmp, var );

                if(fBlockCopyPropagation)
                {
                    opNew.AddAnnotation( BlockCopyPropagationAnnotation.Create( m_cfg.TypeSystem, 0, true, true ) );
                }

                op.AddOperatorBefore( opNew );

                op.SubstituteUsage( var, tmp );
            }
        }

        //--//

        private bool ColorGraph( InterferenceNode root )
        {
            using(new PerformanceCounters.ContextualTiming( m_cfg, "ColorGraph" ))
            {
                InterferenceNode node;
                bool             fFound;
                
                //
                // Prep candidate coloring.
                //
                for(node = root; node != m_stackSentinel; node = node.m_previousStackElement)
                {
                    node.ResetCandidate();
                }

                //
                // If we cannot find a coloring without the constraints of the coalescing web, we should give up immediately.
                //
                fFound = FindColorCandidates( root );
                if(fFound)
                {
                    //
                    // Build the coalesce web.
                    //
                    for(node = root; node != m_stackSentinel; node = node.m_previousStackElement)
                    {
                        node.m_coalesceWeb.Clear();
                    }

                    for(node = root; node != m_stackSentinel; node = node.m_previousStackElement)
                    {
                        Operator op = node.m_definition;
    
                        if(op is SingleAssignmentOperator ||
                           op is PiOperator                )
                        {
                            MergeCoalesceCandidates( node, op.FirstArgument, WebFlavor.Should );
                        }

                        if(op is PhiOperator)
                        {
                            foreach(Expression ex in op.Arguments)
                            {
                                MergeCoalesceCandidates( node, ex, WebFlavor.Should );
                            }
                        }
    
                        foreach(Operator opUse in node.m_uses)
                        {
                            if(opUse is AbstractAssignmentOperator)
                            {
                                MergeCoalesceCandidates( node, opUse.FirstResult, WebFlavor.Should );
    
                                if(opUse is PhiOperator)
                                {
                                    foreach(Expression ex in opUse.Arguments)
                                    {
                                        MergeCoalesceCandidates( node, ex, WebFlavor.Should );
                                    }
                                }
                            }
                        }

                        if(op != null)
                        {
                            foreach(var an in op.FilterAnnotations< RegisterCouplingConstraintAnnotation >())
                            {
                                var coupledSource = an.FindCoupledExpression( node.m_definition, node.m_source );
                                if(coupledSource != null)
                                {
                                    MergeCoalesceCandidates( node, coupledSource, WebFlavor.Must );
                                }
                            }
                        }
                    }

                    while(true)
                    {
                        //
                        // Reset colors.
                        //
                        for(node = root; node != m_stackSentinel; node = node.m_previousStackElement)
                        {
                            node.ResetCandidate();
                        }

                        fFound = FindColorCandidates( root );

                        if(fFound)
                        {
                            //
                            // Because of the aggressive use of coalesce webs, we might generate an incorrect coloring.
                            // If so, revert the work and try again.
                            //
                            for(node = root; node != m_stackSentinel; node = node.m_previousStackElement)
                            {
                                if(node.m_assigned == null)
                                {
                                    foreach(int idxEdge in node.m_edges)
                                    {
                                        InterferenceNode nodeEdge = m_interferenceGraph[idxEdge];

                                        if(nodeEdge.m_candidateColor == node.m_candidateColor)
                                        {
                                            nodeEdge.ResetCandidate();
                                            node    .ResetCandidate();

                                            fFound = false;
                                        }
                                    }
                                }
                            }
                        }

                        if(fFound)
                        {
                            AllocateRegisters( root );
                            break;
                        }

                        //
                        // Remove one node from a coalesce web, retry.
                        //

                        ComputeSpillCosts();

                        BitVector web        = new BitVector();
                        WebPair   bestVictim = null;

                        bestVictim = SelectWebToBreak( root, bestVictim, web, true , true  );
                        bestVictim = SelectWebToBreak( root, bestVictim, web, true , false );
                        bestVictim = SelectWebToBreak( root, bestVictim, web, false, true  );
                        bestVictim = SelectWebToBreak( root, bestVictim, web, false, false );
                        
                        CHECKS.ASSERT( bestVictim != null, "Cannot find victim for removal from coalesce web" );

                        bestVictim.Remove();
                    }
                }

                return fFound;
            }
        }

        private WebPair SelectWebToBreak( InterferenceNode root                 ,
                                          WebPair          bestVictim           ,
                                          BitVector        web                  ,
                                          bool             fIncludeFailuresOnly ,
                                          bool             fSkipPhysicalWebs    )
        {
            if(bestVictim != null)
            {
                //
                // Exit as soon as we find a victim.
                //
                return bestVictim;
            }

            for(var node = root; node != m_stackSentinel; node = node.m_previousStackElement)
            {
                //
                // On the first pass, skip nodes that have been successfully colored.
                //
                if(fIncludeFailuresOnly && node.m_candidateColor >= 0)
                {
                    continue;
                }

                if(node.m_coalesceWeb.Count > 0)
                {
                    web.ClearAll();

                    ComputeCoalesceSet( node, web );

                    bool fSkip = false;

                    if(fSkipPhysicalWebs)
                    {
                        foreach(int idx in web)
                        {
                            InterferenceNode candidate = m_interferenceGraph[idx];
                        
                            if(candidate.m_fixed != null)
                            {
                                fSkip = true;
                                break; 
                            }
                        }
                    }

                    if(fSkip)
                    {
                        continue;
                    }

                    //--//

                    //
                    // Try to find a victim to remove from the web based on its location,
                    // giving higher score to variables from outside loops.
                    //
                    foreach(int idx in web)
                    {
                        InterferenceNode candidate = m_interferenceGraph[idx];
                        
                        foreach(var pair in candidate.m_coalesceWeb)
                        {
                            if(pair.m_flavor != WebFlavor.Must)
                            {
                                if(bestVictim == null || bestVictim.SpillCost() > pair.SpillCost())
                                {
                                    bestVictim = pair;
                                }
                            }
                        }
                    }
                }
            }

            return bestVictim;
        }

        private bool FindColorCandidates( InterferenceNode root )
        {
            var  web    = new BitVector();
            bool fFound = true;

            for(InterferenceNode node = root; node != m_stackSentinel; node = node.m_previousStackElement)
            {
                if(node.m_candidateColor < 0)
                {
                    BitVector availableRegs = node.m_compatibleRegs.Clone();

                    web.ClearAll();

                    ComputeCoalesceSet( node, web );

                    foreach(int idx in web)
                    {
                        if(UpdateAvailableRegisters( availableRegs, m_interferenceGraph[idx] ) == false)
                        {
                            break;
                        }
                    }

                    bool fGot = false;

                    foreach(int idx in web)
                    {
                        InterferenceNode coalesceNode = m_interferenceGraph[idx];

                        if(coalesceNode.m_candidateColor >= 0)
                        {
                            //
                            // Assign the same color to all the nodes in the coalesce web, otherwise fail.
                            //
                            if(availableRegs[coalesceNode.m_candidateColor])
                            {
                                node.m_candidateColor = coalesceNode.m_candidateColor;
                            }

                            fGot = true;
                            break;
                        }
                    }

                    if(fGot == false)
                    {
                        Abstractions.RegisterClass regClass;
                        var                        td = node.m_source.Type;

                        //
                        // Make sure we select a register that is compatible with the type of variable.
                        //
                        if(td.IsNumeric)
                        {
                            switch(td.BuiltInType)
                            {
                                case TypeRepresentation.BuiltInTypes.R4:
                                    regClass = Abstractions.RegisterClass.SinglePrecision;
                                    break;

                                case TypeRepresentation.BuiltInTypes.R8:
                                    regClass = Abstractions.RegisterClass.DoublePrecision;
                                    break;

                                default:
                                    regClass = Abstractions.RegisterClass.Integer;
                                    break;
                            }
                        }
                        else
                        {
                            regClass = Abstractions.RegisterClass.Address;
                        }

                        foreach(int regIdx in availableRegs)
                        {
                            if((m_registers[regIdx].StorageCapabilities & regClass) == regClass)
                            {
                                node.m_candidateColor = regIdx;
                                break;
                            }
                        }
                    }

                    if(node.m_candidateColor < 0)
                    {
                        fFound = false;
                    }
                    else
                    {
                        foreach(int idx in web)
                        {
                            InterferenceNode coalesceNode = m_interferenceGraph[idx];

                            if(coalesceNode.m_candidateColor < 0)
                            {
                                if(coalesceNode.m_compatibleRegs[node.m_candidateColor])
                                {
                                    coalesceNode.m_candidateColor = node.m_candidateColor;
                                }
                            }
                        }
                    }
                }
            }

            return fFound;
        }

        private void AllocateRegisters( InterferenceNode node )
        {
            for(; node != m_stackSentinel; node = node.m_previousStackElement)
            {
                if(node.m_assigned == null)
                {
                    LowLevelVariableExpression source = (LowLevelVariableExpression)node.m_source.AliasedVariable;

                    node.m_assigned = m_cfg.AllocateTypedPhysicalRegister( source.Type, m_registers[node.m_candidateColor], source.DebugName, source.SourceVariable, source.SourceOffset );
                }
            }
        }

        private void ComputeCoalesceSet( InterferenceNode node        ,
                                         BitVector        coalesceSet )
        {
            if(coalesceSet.Set( node.m_index ))
            {
                var web = node.m_coalesceWeb;
                if(web != null)
                {
                    foreach(var pair in web)
                    {
                        ComputeCoalesceSet( pair.m_node1, coalesceSet );
                        ComputeCoalesceSet( pair.m_node2, coalesceSet );
                    }
                }
            }
        }

        private void MergeCoalesceCandidates( InterferenceNode node      ,
                                              Expression       ex        ,
                                              WebFlavor        webFlavor )
        {
            InterferenceNode nodeSrc = GetInterferenceNode( ex );

            if(nodeSrc != null && nodeSrc != node && nodeSrc.m_assigned == null && nodeSrc.LivenessOverlapsWith( node ) == false)
            {
                WebPair oldPair = node.FindPair( nodeSrc );
                if(oldPair != null)
                {
                    oldPair.MaximizeFlavor( webFlavor );
                }
                else
                {
                    var vec    = new BitVector();
                    var vecSrc = new BitVector();

                    ComputeCoalesceSet( node   , vec    );
                    ComputeCoalesceSet( nodeSrc, vecSrc );

                    if(DoesItInterfereWithCoalesceWeb( node   , vecSrc ) || 
                       DoesItInterfereWithCoalesceWeb( nodeSrc, vec    )  )
                    {
                        return;
                    }

                    var pair = new WebPair( node, nodeSrc, webFlavor );

                    node   .m_coalesceWeb.Add( pair );
                    nodeSrc.m_coalesceWeb.Add( pair );
                }
            }
        }

        private bool DoesItInterfereWithCoalesceWeb( InterferenceNode node ,
                                                     BitVector        vec  )
        {
            foreach(int idx in vec)
            {
                InterferenceNode other = m_interferenceGraph[idx];

                if(other.LivenessOverlapsWith( node ))
                {
                    return true;
                }
            }

            return false;
        }

        private bool UpdateAvailableRegisters( BitVector        availableRegs ,
                                               InterferenceNode node          )
        {
            foreach(int idxEdge in node.m_edges)
            {
                InterferenceNode nodeEdge     = m_interferenceGraph[idxEdge];
                int              candidateIdx = nodeEdge.m_candidateColor;

                if(candidateIdx >= 0)
                {
                    availableRegs.Clear( candidateIdx );

                    Abstractions.RegisterDescriptor regDesc = m_registers[candidateIdx];

                    foreach(Abstractions.RegisterDescriptor regDescInterfere in regDesc.InterfersWith)
                    {
                        availableRegs.Clear( regDescInterfere.Index );
                    }

                    if(availableRegs.Cardinality == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        //
        // Access Methods
        //

    }
}
