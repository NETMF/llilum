//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;
    using Microsoft.Zelig;

    public class Dominance< N, E, FC > 
        where N : class, ITreeNode<FC>
        where E : class, ITreeEdge<FC>
    {
        //
        // State
        //

        private readonly N[]   m_nodesSpanningTree;
        private readonly N[]   m_nodesPostOrder;

        private          int[] m_spanningTreeToPostOrder;
        private          N[]   m_immediateDominanceSpanningTree;
        private          N[]   m_immediateDominancePostOrder;

        //
        // Constructor Methods
        //

        public Dominance( N[ ] nodesSpanningTree, N[ ] postOrderVisit )
        {
            if(nodesSpanningTree.Length != postOrderVisit.Length)
            {
                throw new ArgumentException( "Mismatch between size of SpanningTree and size of PostOrderVisit" );
            }

            m_nodesSpanningTree = nodesSpanningTree;
            m_nodesPostOrder    = postOrderVisit;

            int num = m_nodesPostOrder.Length;

            m_spanningTreeToPostOrder = new int[ num ];

            for(int pos = 0; pos < num; pos++)
            {
                N node = m_nodesPostOrder[pos];

                m_spanningTreeToPostOrder[ node.SpanningTreeIndex ] = pos;
            }
        }

        //
        // Helper Methods
        //

        private void ComputeDominators()
        {
            //
            // This is an implementation of the algorithm in "A Simple, Fast Dominance Algorithm", by Keith D. Cooper, Timothy J. Harvey, and Ken Kennedy.
            //
            //  "for all nodes, b /* initialize the dominators array */
            //      doms[b] = Undefined
            //  doms[start_node] = start_node
            //  Changed = true
            //  while (Changed)
            //      Changed = false
            //      for all nodes, b, in reverse postorder (except start node)
            //          new_idom = first (processed) predecessor of b /* (pick one) */
            //          for all other predecessors, p, of b
            //              if doms[p] != Undefined /* i.e., if doms[p] already calculated */
            //                  new_idom = intersect(p, new_idom)
            //          if doms[b] != new_idom
            //              doms[b] = new_idom
            //              Changed = true
            //
            //  function intersect(b1, b2) returns node
            //      finger1 = b1
            //      finger2 = b2
            //      while (finger1 != finger2)
            //          while (finger1 < finger2)
            //              finger1 = doms[finger1]
            //          while (finger2 < finger1)
            //              finger2 = doms[finger2]
            //      return finger1"
            //

            int num = m_nodesPostOrder.Length;

            m_immediateDominancePostOrder = new N[num];

            N startNode = m_nodesSpanningTree[0];

            SetIDom( startNode, startNode );

            while(true)
            {
                bool fChanged = false;

                for(int pos = num - 1; --pos >= 0;)
                {
                    N nodeB = m_nodesPostOrder[pos];

                    CHECKS.ASSERT( !(nodeB is IEntryTreeNode<FC>)  , "Start node should not be processed by ImmediateDominance algorithm" );
                    CHECKS.ASSERT( nodeB.Predecessors.Length > 0, "Node 'b' should have a predecessor: {0}", nodeB                     );

                    N first = null;
                    foreach(ITreeEdge<FC> edge in nodeB.Predecessors)
                    {
                        N node = (N)edge.Predecessor;

                        if(GetIDom( node ) != null)
                        {
                            first = node;
                            break;
                        }
                    }

                    CHECKS.ASSERT( first != null, "Cannot find first (processed) predecessor of 'b'" );

                    N new_idom = first;

                    foreach(ITreeEdge<FC> edge in nodeB.Predecessors)
                    {
                        N nodeP = (N)edge.Predecessor;

                        if(nodeP != first)
                        {
                            if(GetIDom( nodeP ) != null)
                            {
                                new_idom = Intersect( nodeP, new_idom );
                            }
                        }
                    }

                    if(GetIDom( nodeB ) != new_idom)
                    {
                        SetIDom( nodeB, new_idom );

                        fChanged = true;
                    }
                }

                if(!fChanged) break;
            }

            //--//

            //
            // Finally, convert to an array in spanning-tree order.
            //
            m_immediateDominanceSpanningTree = new N[num];
            for(int pos = 0; pos < num; pos++)
            {
                m_immediateDominanceSpanningTree[pos] = GetIDom( m_nodesSpanningTree[pos] );
            }
        }

        private N Intersect( N b1 ,
                                      N b2 )
        {
            int finger1 = GetPostOrderIndex( b1 );
            int finger2 = GetPostOrderIndex( b2 );

            while(finger1 != finger2)
            {
                while(finger1 < finger2)
                {
                    b1      = m_immediateDominancePostOrder[ finger1 ];
                    finger1 = GetPostOrderIndex( b1 );
                }

                while(finger2 < finger1)
                {
                    b2      = m_immediateDominancePostOrder[ finger2 ];
                    finger2 = GetPostOrderIndex( b2 );
                }
            }

            return b1;
        }

        //--//

        private int GetPostOrderIndex( N node )
        {
            return m_spanningTreeToPostOrder[ node.SpanningTreeIndex ];
        }

        private void SetIDom( N node, N node2 )
        {
            m_immediateDominancePostOrder[ GetPostOrderIndex( node ) ] = node2;
        }

        private N GetIDom( N node )
        {
            return m_immediateDominancePostOrder[ GetPostOrderIndex( node ) ];
        }

        //
        // Access Methods
        //

        public N[] GetImmediateDominators()
        {
            if(m_immediateDominanceSpanningTree == null)
            {
                ComputeDominators();
            }

            return m_immediateDominanceSpanningTree;
        }

        public BitVector[] GetDominance()
        {
            N[] idom = GetImmediateDominators();
            int num  = idom.Length;

            BitVector[] res = BitVector.AllocateBitVectors( num, num );

            for(int pos = 0; pos < num; pos++)
            {
                N   node = m_nodesSpanningTree[pos];
                var vec  = res[pos];

                while(true)
                {
                    int idx = node.SpanningTreeIndex;

                    vec.Set( idx );

                    N next = idom[idx];
                    if(next == node)
                    {
                        break;
                    }

                    node = next;
                }
            }

            return res;
        }

        public BitVector[] GetDominanceFrontier()
        {
            //
            // This is an implementation of the algorithm in "A Simple, Fast Dominance Algorithm", by Keith D. Cooper, Timothy J. Harvey, and Ken Kennedy.
            //
            //   "for all nodes, b
            //      if the number of predecessors of b >= 2
            //          for all predecessors, p, of b
            //              runner = p
            //              while runner != doms[b]
            //                  add b to runner’s dominance frontier set
            //                  runner = doms[runner]"
            //
            N[] idom = GetImmediateDominators();
            int num  = idom.Length;

            BitVector[] res = BitVector.AllocateBitVectors( num, num );

            for(int nodeBidx = 0; nodeBidx < num; nodeBidx++)
            {
                N nodeB = m_nodesSpanningTree[nodeBidx];

                if(nodeB.Predecessors.Length >= 2)
                {
                    foreach(var edge in nodeB.Predecessors)
                    {
                        N nodeP = (N)edge.Predecessor;

                        N runner = nodeP;
                        while(runner != idom[nodeBidx])
                        {
                            int idx = runner.SpanningTreeIndex;

                            res[idx].Set( nodeBidx );

                            runner = idom[idx];
                        }
                    }
                }
            }

            return res;
        }
    }
}
