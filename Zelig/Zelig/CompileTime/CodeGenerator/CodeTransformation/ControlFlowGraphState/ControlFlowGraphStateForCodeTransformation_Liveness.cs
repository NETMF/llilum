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
        class CacheInfo_Liveness : CachedInfo
        {
            //
            // State
            //

            internal DataFlow.LivenessAnalysis m_liveness;

            //
            // Helper Methods
            //

            protected override void Update()
            {
                ControlFlowGraphStateForCodeTransformation cfg = (ControlFlowGraphStateForCodeTransformation)m_owner;

                using(new PerformanceCounters.ContextualTiming( cfg, "Liveness" ))
                {
                    m_liveness = DataFlow.LivenessAnalysis.Compute( cfg, false );
                }
            }
        }

        //
        // Helper Methods
        //

        public IDisposable LockLiveness()
        {
            var ci = GetCachedInfo< CacheInfo_Liveness >();

            ci.Lock();

            return ci;
        }

        //
        // Access Methods
        //

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BitVector[] DataFlow_LivenessAtBasicBlockEntry // It's indexed as [<basic block index>][<variable index>]
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_Liveness >();

                return ci.m_liveness.LivenessAtBasicBlockEntry;
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BitVector[] DataFlow_LivenessAtBasicBlockExit // It's indexed as [<basic block index>][<variable index>]
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_Liveness >();

                return ci.m_liveness.LivenessAtBasicBlockExit;
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BitVector[] DataFlow_LivenessAtOperator // It's indexed as [<operator index>][<variable index>]
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_Liveness >();

                return ci.m_liveness.LivenessAtOperator;
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BitVector[] DataFlow_VariableLivenessMap // It's indexed as [<variable index>][<operator index>]
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_Liveness >();

                return ci.m_liveness.VariableLivenessMap;
            }
        }
    }
}
