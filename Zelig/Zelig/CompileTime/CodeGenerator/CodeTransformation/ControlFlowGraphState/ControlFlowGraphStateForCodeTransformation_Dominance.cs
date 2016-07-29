//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig;
    using Microsoft.Zelig.Runtime.TypeSystem;


    public partial class ControlFlowGraphStateForCodeTransformation
    {
        class CacheInfo_Dominance : CachedInfo
        {
            //
            // State
            //

            internal Dominance<BasicBlock, BasicBlockEdge, ControlOperator> m_dominance;

            //
            // Helper Methods
            //

            protected override void Update()
            {
                ControlFlowGraphStateForCodeTransformation cfg = (ControlFlowGraphStateForCodeTransformation)m_owner;

                using(new PerformanceCounters.ContextualTiming( cfg, "Dominance" ))
                {
                    m_dominance = new Dominance<BasicBlock, BasicBlockEdge, ControlOperator>( 
                        cfg.DataFlow_SpanningTree_BasicBlocks, 
                        cfg.DataFlow_PostOrderVisit 
                        );
                }
            }
        }
 
        //
        // Helper Methods
        //

        public IDisposable LockDominance()
        {
            var ci = GetCachedInfo< CacheInfo_Dominance >();

            ci.Lock();

            return ci;
        }

        //
        // Access Methods
        //

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BasicBlock[] DataFlow_ImmediateDominators
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_Dominance >();

                return ci.m_dominance.GetImmediateDominators();
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BitVector[] DataFlow_Dominance
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_Dominance >();

                return ci.m_dominance.GetDominance();
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BitVector[] DataFlow_DominanceFrontier
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_Dominance >();

                return ci.m_dominance.GetDominanceFrontier();
            }
        }
    }
}
