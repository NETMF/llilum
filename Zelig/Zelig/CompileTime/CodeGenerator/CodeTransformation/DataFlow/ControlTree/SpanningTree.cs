//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.DataFlow.ControlTree
{
    using System;
    using System.Collections.Generic;

    public class SpanningTree : GenericDepthFirst
    {
        //
        // State
        //

        ControlFlowGraphStateForCodeTransformation m_cfg;
        List< BasicBlock         >                 m_basicBlocks;
        List< Operator           >                 m_operators;
        List< VariableExpression >                 m_variables;

        //
        // Constructor Methods
        //

        private SpanningTree( ControlFlowGraphStateForCodeTransformation cfg )
        {
            m_cfg         = cfg;
            m_basicBlocks = new List< BasicBlock         >();
            m_operators   = new List< Operator           >();
            m_variables   = new List< VariableExpression >();

            Visit( cfg.EntryBasicBlock );
        }

        public static void Compute(     ControlFlowGraphStateForCodeTransformation cfg         ,
                                    out BasicBlock[]                               basicBlocks ,
                                    out Operator[]                                 operators   ,
                                    out VariableExpression[]                       variables   )
        {
            SpanningTree tree = new SpanningTree( cfg );

            basicBlocks = tree.m_basicBlocks.ToArray();
            operators   = tree.m_operators  .ToArray();
            variables   = tree.m_variables  .ToArray();
        }

        //--//

        public static BasicBlock[] ComputeAncestors( BasicBlock[] basicBlocks )
        {
            int          bbCount   = basicBlocks.Length;
            BasicBlock[] ancestors = new BasicBlock[bbCount];

            for(int i = 0; i < bbCount; i++)
            {
                BasicBlock bb = basicBlocks[i];

                foreach(BasicBlockEdge edge in bb.Predecessors)
                {
                    if(edge.EdgeClass == BasicBlockEdgeClass.TreeEdge)
                    {
                        ancestors[i] = edge.Predecessor;
                    }
                }
            }

            return ancestors;
        }

        //--//

        protected override void ProcessBefore( BasicBlock bb )
        {
            bb.SpanningTreeIndex = m_basicBlocks.Count; m_basicBlocks.Add( bb );

            foreach(Operator op in bb.Operators)
            {
                op.SpanningTreeIndex = m_operators.Count; m_operators.Add( op );

                foreach(var an in op.FilterAnnotations< InvalidationAnnotation >())
                {
                    AddExpression( an.Target );
                }

                foreach(var ex in op.Results)
                {
                    AddExpression( ex );
                }

                foreach(var ex in op.Arguments)
                {
                    AddExpression( ex );
                }
            }
        }

        private void AddExpression( Expression ex )
        {
            if(ex is VariableExpression)
            {
                AddVariable( (VariableExpression)ex );
            }
        }

        private void AddVariable( VariableExpression var )
        {
            if(var.SpanningTreeIndex < 0)
            {
                var.SpanningTreeIndex = m_variables.Count;

                m_variables.Add( var );

                //
                // If this is a fragment, we need to add all the other fragments to the spanning tree.
                //
                Expression[] fragments = m_cfg.GetFragmentsForExpression( var );

                LowLevelVariableExpression lowVar = var as LowLevelVariableExpression;
                if(lowVar != null)
                {
                    VariableExpression sourceVar = lowVar.SourceVariable;
                    
                    if(sourceVar != null)
                    {
                        fragments = m_cfg.GetFragmentsForExpression( sourceVar );

                        CHECKS.ASSERT( fragments != null, "Found an orphan fragment: {0} not part of {1}", lowVar, sourceVar );
                    }
                }

                if(fragments != null)
                {
                    foreach(Expression exFragment in fragments)
                    {
                        AddVariable( (VariableExpression)exFragment );
                    }
                }

                //
                // Also add any aliased variable.
                //
                AddVariable( var.AliasedVariable );
            }
        }

        //--//

        protected override void ProcessEdgeBefore( BasicBlockEdge edge )
        {
            edge.EdgeClass = BasicBlockEdgeClass.TreeEdge;
        }

        protected override void ProcessEdgeNotTaken( BasicBlockEdge edge )
        {
            BasicBlock predecessor = edge.Predecessor;
            BasicBlock successor   = edge.Successor;

            if(predecessor.SpanningTreeIndex < successor.SpanningTreeIndex)
            {
                if(IsAncestor( predecessor, successor ))
                {
                    edge.EdgeClass = BasicBlockEdgeClass.ForwardEdge;
                }
                else
                {
                    edge.EdgeClass = BasicBlockEdgeClass.CrossEdge;
                }
            }
            else
            {
                if(IsAncestor( successor, predecessor ))
                {
                    edge.EdgeClass = BasicBlockEdgeClass.BackEdge;
                }
                else
                {
                    edge.EdgeClass = BasicBlockEdgeClass.CrossEdge;
                }
            }
        }

        //--//

        public static bool IsAncestor( BasicBlock node  ,
                                       BasicBlock child )
        {
            while(child != null)
            {
                if(node == child)
                {
                    return true;
                }

                BasicBlock nodeNext = null;

                foreach(BasicBlockEdge edge in child.Predecessors)
                {
                    if(edge.EdgeClass == BasicBlockEdgeClass.TreeEdge)
                    {
                        nodeNext = edge.Predecessor;
                        break;
                    }
                }

                CHECKS.ASSERT( nodeNext != null || child.Predecessors.Length == 0, "Child not a member of a spanning tree" );

                child = nodeNext;
            }

            return false;
        }
    }
}
