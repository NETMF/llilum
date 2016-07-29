//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Diagnostics;
    using Microsoft.Zelig.Runtime.TypeSystem;

    public static class MergeExtendedBasicBlocks
    {
        //
        // Helper Methods
        //

        public static bool Execute( ControlFlowGraphStateForCodeTransformation cfg, bool preserveInjectionSites )
        {
            cfg.TraceToFile( "MergeExtendedBasicBlocks" );

            using(new PerformanceCounters.ContextualTiming( cfg, "MergeExtendedBasicBlocks" ))
            {
                bool fModified = false;
                bool fDone = false;

                while(!fDone)
                {
                    BasicBlock[] basicBlocks = cfg.DataFlow_SpanningTree_BasicBlocks;

                    fDone = true;

                    foreach(BasicBlock bb in basicBlocks)
                    {
                        if(!(bb is NormalBasicBlock))
                        {
                            continue;
                        }

                        UnconditionalControlOperator ctrl = bb.FlowControl as UnconditionalControlOperator;
                        if(ctrl == null)
                        {
                            continue;
                        }

                        BasicBlock bbNext = ctrl.TargetBranch;

                        // Most transforms depend on finding a block with a specific annotation to inject operators
                        // into. We need to avoid removing the last block of a given annotation unless the caller is
                        // looking for an aggressive merge.
                        if(preserveInjectionSites && (bbNext.Annotation != bb.Annotation))
                        {
                            continue;
                        }

                        if(TryMergeWithSuccessor( bb, bbNext ))
                        {
                            fModified = true;
                            fDone = false;

                            // When eliminating a successor, we need to reflow the graph; restart the loop.
                            break;
                        }

                        if(ReplaceBlockIfEmpty( bb, bbNext ))
                        {
                            fModified = true;
                            fDone = false;
                        }
                    }
                }

                return fModified;
            }
        }

        private static bool TryMergeWithSuccessor(BasicBlock block, BasicBlock successor)
        {
            // If the successor is special or this block isn't its only predecessor, we have to keep them separate.
            if (!(successor is NormalBasicBlock) || (successor.Predecessors.Length != 1))
            {
                return false;
            }

            Debug.Assert(successor.Predecessors[0].Predecessor == block, "A block's sole successor must have the block as a predecessor.");

            // If the exception handling for these two blocks is different, they can't be merged.
            if (!ArrayUtility.ArrayEqualsNotNull(block.ProtectedBy, successor.ProtectedBy, 0))
            {
                return false;
            }

            // It's too complicated to remove a basic block if the successor contains phi operators.
            foreach (Operator op in successor.Operators)
            {
                if (op is PhiOperator || op is PiOperator)
                {
                    return false;
                }
            }

            block.Merge(successor);
            return true;
        }

        private static bool ReplaceBlockIfEmpty(BasicBlock block, BasicBlock successor)
        {
            // If the block isn't empty, or it loops back to itself, skip it.
            if ((block.Operators.Length != 1) || (block == successor))
            {
                return false;
            }

            // It's too complicated to remove a basic block if the successor contains phi operators.
            foreach (Operator op in successor.Operators)
            {
                if (op is PhiOperator)
                {
                    return false;
                }
            }

            bool modified = false;

            foreach (BasicBlockEdge edge in block.Predecessors)
            {
                BasicBlock predecessor = (BasicBlock)edge.Predecessor;

                // If this predecessor isn't a special block and it's under the same exception
                // handling scope, retarget its flow control to the successor.
                if ( (predecessor is NormalBasicBlock)                                                 &&
                     ArrayUtility.ArrayEqualsNotNull( block.ProtectedBy, predecessor.ProtectedBy, 0 )  &&
                     predecessor.FlowControl.SubstituteTarget( block, successor )                       )
                {
                    modified = true;
                }
            }

            return modified;
        }
    }
}
