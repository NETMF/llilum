//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.DataFlow.ControlTree
{
    using System;
    using System.Collections.Generic;

    public class Dominance
    {
        //
        // State
        //

        BasicBlock[] m_basicBlocksSpanningTree;
        BasicBlock[] m_basicBlocksPostOrder;

        int[]        m_spanningTreeToPostOrder;
        BasicBlock[] m_immediateDominanceSpanningTree;
        BasicBlock[] m_immediateDominancePostOrder;

        //
        // Constructor Methods
        //

        public Dominance( ControlFlowGraphStateForCodeTransformation cfg )
        {
            m_basicBlocksSpanningTree = cfg.DataFlow_SpanningTree_BasicBlocks;
            m_basicBlocksPostOrder    = cfg.DataFlow_PostOrderVisit;

            CHECKS.ASSERT( m_basicBlocksSpanningTree.Length == m_basicBlocksPostOrder.Length, "Mismatch between size of SpanningTree and size of PostOrderVisit for {0}", cfg );

            int num = m_basicBlocksPostOrder.Length;

            m_spanningTreeToPostOrder = new int[num];

            for(int pos = 0; pos < num; pos++)
            {
                BasicBlock bb = m_basicBlocksPostOrder[pos];

                m_spanningTreeToPostOrder[bb.SpanningTreeIndex] = pos;
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

            int num = m_basicBlocksPostOrder.Length;

            m_immediateDominancePostOrder = new BasicBlock[num];

            BasicBlock startNode = m_basicBlocksSpanningTree[0];

            SetIDom( startNode, startNode );

            while(true)
            {
                bool fChanged = false;

                for(int pos = num - 1; --pos >= 0;)
                {
                    BasicBlock nodeB = m_basicBlocksPostOrder[pos];

                    CHECKS.ASSERT( !(nodeB is EntryBasicBlock)  , "Start node should not be processed by ImmediateDominance algorithm" );
                    CHECKS.ASSERT( nodeB.Predecessors.Length > 0, "Node 'b' should have a predecessor: {0}", nodeB                     );

                    BasicBlock first = null;
                    foreach(BasicBlockEdge edge in nodeB.Predecessors)
                    {
                        BasicBlock bb = edge.Predecessor;

                        if(GetIDom( bb ) != null)
                        {
                            first = bb;
                            break;
                        }
                    }

                    CHECKS.ASSERT( first != null, "Cannot find first (processed) predecessor of 'b'" );

                    BasicBlock new_idom = first;

                    foreach(BasicBlockEdge edge in nodeB.Predecessors)
                    {
                        BasicBlock nodeP = edge.Predecessor;

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
            m_immediateDominanceSpanningTree = new BasicBlock[num];
            for(int pos = 0; pos < num; pos++)
            {
                m_immediateDominanceSpanningTree[pos] = GetIDom( m_basicBlocksSpanningTree[pos] );
            }
        }

        private BasicBlock Intersect( BasicBlock b1 ,
                                      BasicBlock b2 )
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

        private int GetPostOrderIndex( BasicBlock bb )
        {
            return m_spanningTreeToPostOrder[ bb.SpanningTreeIndex ];
        }

        private void SetIDom( BasicBlock bb  ,
                              BasicBlock bb2 )
        {
            m_immediateDominancePostOrder[ GetPostOrderIndex( bb ) ] = bb2;
        }

        private BasicBlock GetIDom( BasicBlock bb )
        {
            return m_immediateDominancePostOrder[ GetPostOrderIndex( bb ) ];
        }

        //
        // Access Methods
        //

        public BasicBlock[] GetImmediateDominators()
        {
            if(m_immediateDominanceSpanningTree == null)
            {
                ComputeDominators();
            }

            return m_immediateDominanceSpanningTree;
        }

        public BitVector[] GetDominance()
        {
            BasicBlock[] idom = GetImmediateDominators();
            int          num  = idom.Length;

            BitVector[] res = BitVector.AllocateBitVectors( num, num );

            for(int pos = 0; pos < num; pos++)
            {
                BasicBlock bb  = m_basicBlocksSpanningTree[pos];
                BitVector  vec = res[pos];

                while(true)
                {
                    int idx = bb.SpanningTreeIndex;

                    vec.Set( idx );

                    BasicBlock next = idom[idx];
                    if(next == bb)
                    {
                        break;
                    }

                    bb = next;
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
            BasicBlock[] idom = GetImmediateDominators();
            int          num  = idom.Length;

            BitVector[] res = BitVector.AllocateBitVectors( num, num );

            for(int nodeBidx = 0; nodeBidx < num; nodeBidx++)
            {
                BasicBlock nodeB = m_basicBlocksSpanningTree[nodeBidx];

                if(nodeB.Predecessors.Length >= 2)
                {
                    foreach(BasicBlockEdge edge in nodeB.Predecessors)
                    {
                        BasicBlock nodeP = edge.Predecessor;

                        BasicBlock runner = nodeP;
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
