//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System.Collections.Generic;

    public class SpanningTree< N, FC > : GenericDepthFirst< FC > where N : class, ITreeNode<FC>
    {
        //
        // State
        //

        protected List< N > m_nodes;

        //
        // Constructor Methods
        //

        protected SpanningTree( )
        {
            m_nodes = new List< N >();
        }

        //--//

        protected override void ProcessEdgeBefore( ITreeEdge<FC> edge )
        {
            edge.EdgeClass = EdgeClass.TreeEdge;
        }

        protected override void ProcessEdgeNotTaken( ITreeEdge<FC> edge )
        {
            ITreeNode<FC> predecessor = (ITreeNode<FC>)edge.Predecessor;
            ITreeNode<FC> successor   = (ITreeNode<FC>)edge.Successor;

            if(predecessor.SpanningTreeIndex < successor.SpanningTreeIndex)
            {
                if(IsAncestor( predecessor, successor ))
                {
                    edge.EdgeClass = EdgeClass.ForwardEdge;
                }
                else
                {
                    edge.EdgeClass = EdgeClass.CrossEdge;
                }
            }
            else
            {
                if(IsAncestor( successor, predecessor ))
                {
                    edge.EdgeClass = EdgeClass.BackEdge;
                }
                else
                {
                    edge.EdgeClass = EdgeClass.CrossEdge;
                }
            }
        }

        //--//

        public static ITreeNode<FC>[ ] ComputeAncestors( N[ ] nodes )
        {
            int bbCount   = nodes.Length;
            ITreeNode<FC>[] ancestors = new N[bbCount];

            for(int i = 0; i < bbCount; i++)
            {
                var bb = nodes[i];

                foreach(ITreeEdge<FC> edge in bb.Predecessors)
                {
                    if(edge.EdgeClass == EdgeClass.TreeEdge)
                    {
                        ancestors[ i ] = edge.Predecessor;
                    }
                }
            }

            return ancestors;
        }

        //--//

        public static bool IsAncestor( ITreeNode<FC> node  ,
                                       ITreeNode<FC> child )
        {
            while(child != null)
            {
                if(node == child)
                {
                    return true;
                }

                ITreeNode<FC> nodeNext = null;

                foreach(ITreeEdge<FC> edge in child.Predecessors)
                {
                    if(edge.EdgeClass == EdgeClass.TreeEdge)
                    {
                        nodeNext = (ITreeNode<FC>)edge.Predecessor;
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
