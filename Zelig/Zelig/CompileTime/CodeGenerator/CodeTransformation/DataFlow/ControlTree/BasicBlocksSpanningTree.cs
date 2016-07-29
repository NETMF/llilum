//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.DataFlow.ControlTree
{
    using System.Collections.Generic;
    using Microsoft.Zelig;

    public class BasicBlocksSpanningTree : SpanningTree< BasicBlock, ControlOperator >
    {
        //
        // State
        //

        ControlFlowGraphStateForCodeTransformation  m_cfg;
        List< Operator           >                  m_operators;
        List< VariableExpression >                  m_variables;

        //
        // Constructor Methods
        //

        private BasicBlocksSpanningTree( ControlFlowGraphStateForCodeTransformation cfg ) : base( )
        {
            m_cfg       = cfg;
            m_operators = new List< Operator           >();
            m_variables = new List< VariableExpression >();

            Visit( cfg.EntryBasicBlock );
        }

        public static void Compute(     ControlFlowGraphStateForCodeTransformation cfg         ,
                                    out BasicBlock[]                               basicBlocks ,
                                    out Operator[]                                 operators   ,
                                    out VariableExpression[]                       variables   )
        {
            BasicBlocksSpanningTree tree = new BasicBlocksSpanningTree( cfg );

            basicBlocks = tree.m_nodes    .ToArray();
            operators   = tree.m_operators.ToArray();
            variables   = tree.m_variables.ToArray();
        }

        //--//
        
        protected override void ProcessBefore( ITreeNode<ControlOperator> node )
        {
            var bb = (BasicBlock)node;

            bb.SpanningTreeIndex = m_nodes.Count; m_nodes.Add( bb );

            var bb1 = node as BasicBlock;

            foreach(Operator op in bb1.Operators)
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
    }
}
