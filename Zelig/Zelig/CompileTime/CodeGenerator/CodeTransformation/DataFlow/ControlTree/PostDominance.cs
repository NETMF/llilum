//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.DataFlow.ControlTree
{
    using System;
    using System.Collections.Generic;

    //
    // TODO: There's a problem with this algorithm and exceptions.
    //
    // Basically, every exception handler that does not return execution to the method is an exit block.
    // So they must be treated as if they are actual exit blocks.
    //
    public class PostDominance
    {
        //
        // State
        //

        BasicBlock[] m_basicBlocks;
        BitVector[]  m_postDominance;
        BasicBlock[] m_immediatePostDominators;

        //
        // Constructor Methods
        //

        public PostDominance( ControlFlowGraphStateForCodeTransformation cfg )
        {
            m_basicBlocks = cfg.DataFlow_SpanningTree_BasicBlocks;
        }

        //
        // Helper Methods
        //

        private void ComputePostDominators()
        {
            int       len = m_basicBlocks.Length;
            BitVector t   = new BitVector( len );

            m_postDominance = new BitVector[len];

            for(int i = 0; i < len; i++)
            {
                BitVector bv = new BitVector( len );

                if(m_basicBlocks[i].Successors.Length == 0)
                {
                    //
                    // A basic block with no successors is postdominated just by itself.
                    //
                    bv.Set( i );
                }
                else
                {
                    bv.SetRange( 0, len );
                }

                m_postDominance[i] = bv;
            }

            while(true)
            {
                bool fChange = false;

                for(int i = 0; i < len; i++)
                {
                    BasicBlockEdge[] edges = m_basicBlocks[i].Successors;

                    if(edges.Length != 0)
                    {
                        t.SetRange( 0, len );

                        foreach(BasicBlockEdge edge in edges)
                        {
                            t.AndInPlace( m_postDominance[edge.Successor.SpanningTreeIndex] );
                        }

                        t.Set( i );

                        if(t != m_postDominance[i])
                        {
                            fChange = true;

                            m_postDominance[i].Assign( t );
                        }
                    }
                }

                if(!fChange) break;
            }
        }

        private void ComputeImmediatePostDominators()
        {
            int         len = m_basicBlocks.Length;
            BitVector[] tmp = BitVector.AllocateBitVectors( len, len );

            for(int n = 0; n < len; n++)
            {
                // Tmp(n) := PostDomin(n) - {n}
                tmp[n].Assign( m_postDominance[n] );
                tmp[n].Clear (                 n  );
            }

            for(int n = 0; n < len; n++) // Walk the basic blocks in pre-order.
            {
                // for each n in N do

                BitVector tmpN = tmp[n];

                for(int s = 0; s < len; s++)
                {
                    if(tmpN[s])
                    {
                        // for each s in Tmp(n) do

                        BitVector tmpS = tmp[s];

                        for(int t = 0; t < len; t++)
                        {
                            if(t != s && tmpN[t])
                            {
                                // for each t in Tmp(n) - {s} do

                                if(tmpS[t])
                                {
                                    // if t in Tmp(s) then Tmp(n) -= {t}

                                    tmpN.Clear( t );
                                }
                            }
                        }
                    }
                }
            }

            m_immediatePostDominators = new BasicBlock[len];

            for(int n = 0; n < len; n++)
            {
                bool fGot = false;

                foreach(int idom in tmp[n])
                {
                    CHECKS.ASSERT( fGot == false, "Internal failure, found more than one immediate post dominators" );

                    m_immediatePostDominators[n] = m_basicBlocks[idom];

                    fGot = true;
                }
            }
        }

        //
        // Access Methods
        //

        public BitVector[] GetPostDominance()
        {
            if(m_postDominance == null)
            {
                ComputePostDominators();
            }

            return m_postDominance;
        }

        public BasicBlock[] GetImmediatePostDominators()
        {
            if(m_immediatePostDominators == null)
            {
                ComputeImmediatePostDominators();
            }

            return m_immediatePostDominators;
        }
    }
}
