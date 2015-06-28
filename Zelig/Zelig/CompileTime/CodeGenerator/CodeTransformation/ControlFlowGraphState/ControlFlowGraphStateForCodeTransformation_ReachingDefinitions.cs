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
        class CacheInfo_ReachingDefinitions : CachedInfo
        {
            //
            // State
            //

            internal BitVector[] m_reachingDefinitions;

            //
            // Helper Methods
            //

            protected override void Update()
            {
                ControlFlowGraphStateForCodeTransformation cfg = (ControlFlowGraphStateForCodeTransformation)m_owner;

                using(new PerformanceCounters.ContextualTiming( cfg, "ReachingDefinitions" ))
                {
                    DataFlow.ReachingDefinitions.Compute( cfg.DataFlow_SpanningTree_BasicBlocks, cfg.DataFlow_SpanningTree_Operators, true, out m_reachingDefinitions );
                }
            }
        }

        //
        // Helper Methods
        //

        public IDisposable LockReachingDefinitions()
        {
            var ci = GetCachedInfo< CacheInfo_ReachingDefinitions >();

            ci.Lock();

            return ci;
        }

        //
        // Access Methods
        //

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BitVector[] DataFlow_ReachingDefinitions
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_ReachingDefinitions >();

                return ci.m_reachingDefinitions;
            }
        }
    }
}
