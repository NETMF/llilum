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
        class CacheInfo_PostOrderVisit : CachedInfo
        {
            //
            // State
            //

            internal BasicBlock[] m_basicBlocks;

            //
            // Helper Methods
            //

            protected override void Update()
            {
                ControlFlowGraphStateForCodeTransformation cfg = (ControlFlowGraphStateForCodeTransformation)m_owner;

                using(new PerformanceCounters.ContextualTiming( cfg, "PostOrderVisit" ))
                {
                    cfg.UpdateFlowInformation();

                    DataFlow.ControlTree.PostOrderVisit.Compute( cfg.m_entryBasicBlock, out m_basicBlocks );
                }
            }
        }

        //
        // Helper Methods
        //

        public IDisposable LockPostOrderVisit()
        {
            var ci = GetCachedInfo< CacheInfo_PostOrderVisit >();

            ci.Lock();

            return ci;
        }
        
        //
        // Access Methods
        //

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BasicBlock[] DataFlow_PostOrderVisit
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_PostOrderVisit >();

                return ci.m_basicBlocks;
            }
        }
    }
}
