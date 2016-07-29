//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;

    public abstract class GenericDepthFirst< FC >
    {
        //
        // State
        //

        GrowOnlySet< ITreeNode< FC > > m_visited;

        //
        // Constructor Methods
        //

        protected GenericDepthFirst()
        {
            m_visited = SetFactory.NewWithReferenceEquality< ITreeNode<FC>>();
        }

        protected void Visit( ITreeNode<FC> node )
        {
            ProcessBefore( node );

            m_visited.Insert( node );

            for(int pass = 0; pass < 2; pass++)
            {
                foreach(ITreeEdge<FC> edge in node.Successors)
                {
                    var bbSucc = edge.Successor;

                    //
                    // On the first pass, follow only non-dead branches.
                    //
                    // Dead branches are almost always exception-throwing branches,
                    // following them after the other branches ensures that they don't show up in TreeEdges.
                    //
                    if((pass == 0) == (bbSucc.FlowControl is IDeadBranch))
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

            ProcessAfter( node );
        }

        //--//

        protected virtual void ProcessBefore( ITreeNode<FC> node )
        {
        }

        protected virtual void ProcessAfter( ITreeNode<FC> node )
        {
        }

        //--//

        protected virtual void ProcessEdgeBefore( ITreeEdge<FC> edge )
        {
        }

        protected virtual void ProcessEdgeAfter( ITreeEdge<FC> edge )
        {
        }

        protected virtual void ProcessEdgeNotTaken( ITreeEdge<FC> edge )
        {
        }
    }
}
