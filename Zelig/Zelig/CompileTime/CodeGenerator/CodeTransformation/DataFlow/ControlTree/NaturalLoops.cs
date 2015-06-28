//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.DataFlow.ControlTree
{
    using System;
    using System.Collections.Generic;

    public class NaturalLoops : IDisposable
    {
        public class Entry
        {
            public static Entry[] SharedEmptyArray = new Entry[0];

            //
            // State
            //

            internal int       m_depth;
            internal int       m_headIdx;
            internal BitVector m_basicBlocks;
            internal BitVector m_exitPoints;

            //
            // Constructor Methods
            //

            internal Entry( BasicBlock bbHead ,
                            BasicBlock bb     ,
                            int        bbNum  )
            {
                m_headIdx     = bbHead.SpanningTreeIndex;
                m_basicBlocks = new BitVector( bbNum );
                m_exitPoints  = new BitVector( bbNum );

                m_basicBlocks.Set( bb.SpanningTreeIndex );

                ExpandInner( bbHead, bb );
            }

            //
            // Helper Methods
            //

            private void ExpandInner( BasicBlock bbHead ,
                                      BasicBlock bb     )
            {
                if(bbHead != bb)
                {
                    foreach(BasicBlockEdge edge in bb.Predecessors)
                    {
                        BasicBlock bbPred    = edge.Predecessor;
                        int        bbPredIdx = bbPred.SpanningTreeIndex;

                        if(m_basicBlocks.Set( bbPredIdx ))
                        {
                            ExpandInner( bbHead, bbPred );
                        }
                    }
                }
            }

            //
            // Access Methods
            //

            public int Depth
            {
                get
                {
                    return m_depth;
                }
            }

            public int IndexOfHead
            {
                get
                {
                    return m_headIdx;
                }
            }

            public BitVector BasicBlocks
            {
                get
                {
                    return m_basicBlocks;
                }
            }

            public BitVector ExitPoints
            {
                get
                {
                    return m_exitPoints;
                }
            }
        }

        //
        // State
        //

        private readonly IDisposable   m_cfgLock;

        private readonly BasicBlock[]  m_basicBlocks;
        private readonly BitVector[]   m_dominance;
        private          List< Entry > m_loops;
        private          Entry[][]     m_loopsLookup;

        //
        // Constructor Methods
        //

        private NaturalLoops( ControlFlowGraphStateForCodeTransformation cfg )
        {
            m_cfgLock     = cfg.GroupLock( cfg.LockSpanningTree() ,
                                           cfg.LockDominance   () );

            m_basicBlocks = cfg.DataFlow_SpanningTree_BasicBlocks;
            m_dominance   = cfg.DataFlow_Dominance;
        }

        //
        // Helper Methods
        //

        public static NaturalLoops Execute( ControlFlowGraphStateForCodeTransformation cfg )
        {
            NaturalLoops nl = new NaturalLoops( cfg );

            nl.Compute();

            return nl;
        }

        public void Dispose()
        {
            m_cfgLock.Dispose();
        }

        private void Compute()
        {
            int bbNum = m_basicBlocks.Length;

            m_loops       = new List< Entry >( bbNum );
            m_loopsLookup = new Entry[bbNum][];
            for(int i = 0; i < bbNum; i++)
            {
                m_loopsLookup[i] = Entry.SharedEmptyArray;
            }

            foreach(BasicBlock bb in m_basicBlocks)
            {
                int bbIdx = bb.SpanningTreeIndex;

                foreach(BasicBlockEdge edge in bb.Successors)
                {
                    BasicBlock bbHead    = edge.Successor;
                    int        bbHeadIdx = bbHead.SpanningTreeIndex;

                    if(m_dominance[bbIdx][bbHeadIdx])
                    {
                        //
                        // Found a back edge bb->bbHead.
                        //
                        bool fNew = true;

                        foreach(var loop in m_loopsLookup[bbHeadIdx])
                        {
                            if(loop.m_headIdx == bbHeadIdx && loop.m_basicBlocks[bbIdx])
                            {
                                fNew = false;
                                break;
                            }
                        }

                        if(fNew)
                        {
                            Entry loopNew = new Entry( bbHead, bb, bbNum );

                            foreach(Entry loop in m_loops)
                            {
                                if(loop.m_basicBlocks == loopNew.m_basicBlocks)
                                {
                                    loopNew = null;
                                    break;
                                }
                            }

                            if(loopNew != null)
                            {
                                m_loops.Add( loopNew );

                                foreach(int idx in loopNew.m_basicBlocks)
                                {
                                    m_loopsLookup[idx] = ArrayUtility.AppendToNotNullArray( m_loopsLookup[idx], loopNew );
                                }
                            }
                        }
                    }
                }
            }

            foreach(var loop in m_loops)
            {
                loop.m_depth = m_loopsLookup[loop.m_headIdx].Length;

                foreach(var idx in loop.m_basicBlocks)
                {
                    foreach(var bbEdge in m_basicBlocks[idx].Successors)
                    {
                        var bbNext = bbEdge.Successor;

                        if(bbNext is ExceptionHandlerBasicBlock)
                        {
                            // Skip these blocks, they are not normal edges.
                        }
                        else
                        {
                            if(loop.m_basicBlocks[bbNext.SpanningTreeIndex] == false)
                            {
                                //
                                // The successor is not part of the same loop, this is the definition of an exit point.
                                //
                                loop.m_exitPoints.Set( idx );
                            }
                        }
                    }
                }
            }
        }

        //--//

        public int GetDepthOfBasicBlock( BasicBlock bb )
        {
            int depth = 0;

            foreach(var en in m_loopsLookup[bb.SpanningTreeIndex])
            {
                depth = Math.Max( depth, en.Depth );
            }

            return depth;
        }

        //
        // Access Methods
        //

        public List< Entry > Loops
        {
            get
            {
                return m_loops;
            }
        }

        public Entry[][] LoopsLookup
        {
            get
            {
                return m_loopsLookup;
            }
        }
    }
}
