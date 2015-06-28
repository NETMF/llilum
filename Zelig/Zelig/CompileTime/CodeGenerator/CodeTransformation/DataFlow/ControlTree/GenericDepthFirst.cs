//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.DataFlow.ControlTree
{
    using System;

    public abstract class GenericDepthFirst
    {
        //
        // State
        //

        GrowOnlySet< BasicBlock > m_visited;

        //
        // Constructor Methods
        //

        protected GenericDepthFirst()
        {
            m_visited = SetFactory.NewWithReferenceEquality< BasicBlock >();
        }

        protected void Visit( BasicBlock bb )
        {
            ProcessBefore( bb );

            m_visited.Insert( bb );

            for(int pass = 0; pass < 2; pass++)
            {
                foreach(BasicBlockEdge edge in bb.Successors)
                {
                    BasicBlock bbSucc = edge.Successor;

                    //
                    // On the first pass, follow only non-dead branches.
                    //
                    // Dead branches are almost always exception-throwing branches,
                    // following them after the other branches ensures that they don't show up in TreeEdges.
                    //
                    if((pass == 0) == (bbSucc.FlowControl is DeadControlOperator))
                    {
                        continue;
                    }


                    if(m_visited.Contains( bbSucc ) == false)
                    {
                        ProcessEdgeBefore( edge );

                        Visit( bbSucc );

                        ProcessEdgeAfter( edge );
                    }
                    else
                    {
                        ProcessEdgeNotTaken( edge );
                    }
                }
            }

            ProcessAfter( bb );
        }

        //--//

        protected virtual void ProcessBefore( BasicBlock bb )
        {
        }

        protected virtual void ProcessAfter( BasicBlock bb )
        {
        }

        //--//

        protected virtual void ProcessEdgeBefore( BasicBlockEdge edge )
        {
        }

        protected virtual void ProcessEdgeAfter( BasicBlockEdge edge )
        {
        }

        protected virtual void ProcessEdgeNotTaken( BasicBlockEdge edge )
        {
        }
    }
}
