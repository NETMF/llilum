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
        class CacheInfo_Dominance : CachedInfo
        {
            //
            // State
            //

            internal DataFlow.ControlTree.Dominance m_dominance;

            //
            // Helper Methods
            //

            protected override void Update()
            {
                ControlFlowGraphStateForCodeTransformation cfg = (ControlFlowGraphStateForCodeTransformation)m_owner;

                using(new PerformanceCounters.ContextualTiming( cfg, "Dominance" ))
                {
                    m_dominance = new DataFlow.ControlTree.Dominance( cfg );
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
