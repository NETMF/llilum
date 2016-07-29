//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public partial class ControlFlowGraphStateForCodeTransformation
    {
        class CacheInfo_PostDominance : CachedInfo
        {
            //
            // State
            //

            internal PostDominance<BasicBlock, BasicBlockEdge, ControlOperator> m_postDominance;

            //
            // Helper Methods
            //

            protected override void Update()
            {
                ControlFlowGraphStateForCodeTransformation cfg = (ControlFlowGraphStateForCodeTransformation)m_owner;

                using(new PerformanceCounters.ContextualTiming( cfg, "PostDominance" ))
                {
                    m_postDominance = new PostDominance<BasicBlock, BasicBlockEdge, ControlOperator>( cfg.DataFlow_SpanningTree_BasicBlocks );
                }
            }
        }

        //
        // Helper Methods
        //

        public IDisposable LockPostDominance()
        {
            var ci = GetCachedInfo< CacheInfo_PostDominance >();

            ci.Lock();

            return ci;
        }

        //
        // Access Methods
        //

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BitVector[] DataFlow_PostDominance
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_PostDominance >();

                return ci.m_postDominance.GetPostDominance();
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BasicBlock[] DataFlow_ImmediatePostDominators
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_PostDominance >();

                return ci.m_postDominance.GetImmediatePostDominators();
            }
        }
    }
}
