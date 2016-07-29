//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public static class SplitBasicBlocksAtExceptionSites
    {
        //
        // Helper Methods
        //

        public static bool Execute( ControlFlowGraphStateForCodeTransformation cfg )
        {
            cfg.TraceToFile( "SplitBasicBlocksAtExceptionSites" );

            using(new PerformanceCounters.ContextualTiming( cfg, "SplitBasicBlocksAtExceptionSites" ))
            {
                Queue< BasicBlock > pending   = new Queue< BasicBlock >();
                bool                fModified = false;

                foreach(BasicBlock bb in cfg.DataFlow_SpanningTree_BasicBlocks)
                {
                    pending.Enqueue( bb );
                }

                while(pending.Count > 0)
                {
                    BasicBlock bb = pending.Dequeue();

                    if(bb.ProtectedBy.Length > 0)
                    {
                        bool fFirst = true;

                        foreach(Operator op in bb.Operators)
                        {
                            if(fFirst)
                            {
                                fFirst = false;
                            }
                            else if(op.MayThrow)
                            {
                                if(op is ThrowControlOperator      ||
                                   op is RethrowControlOperator    ||
                                   op is EndFinallyControlOperator  )
                                {
                                    // Nothing to do, these operators throw an exception, they don't cause an exception.
                                }
                                else
                                {
                                    BasicBlock split = op.BasicBlock.SplitAtOperator( op, false, true );

                                    pending.Enqueue( split );
                                    fModified = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                // Another precondition for converting to SSA is that we cannot put a phi operator on an exception basic block.
                // This means that exception basic blocks should be immediately dominated by the block they protect.
                //
                //    => only one basic block can be protected by an exception handler.
                //
                // Let's split them up!
                //
                // Note: LLVM allows phi operators in exception handling blocks, so we exclude this step.
                if (cfg.TypeSystem.PlatformAbstraction.CodeGenerator != TargetModel.ArmProcessor.InstructionSetVersion.CodeGenerator_LLVM)
                {
                    while (true)
                    {
                        bool fDone = true;

                        foreach (BasicBlock bb in cfg.DataFlow_SpanningTree_BasicBlocks)
                        {
                            ExceptionHandlerBasicBlock ehBB = bb as ExceptionHandlerBasicBlock;

                            if (ehBB != null && ehBB.Predecessors.Length > 1)
                            {
                                CHECKS.ASSERT(ehBB.Successors.Length - ehBB.ProtectedBy.Length == 1, "Unexpected exception basic block with more than one successor: {0}", ehBB);

                                BasicBlock bbNext = ehBB.FirstSuccessor;

                                foreach (BasicBlockEdge edge in (BasicBlockEdge[])ehBB.Predecessors)
                                {
                                    ExceptionHandlerBasicBlock ehBBNew = (ExceptionHandlerBasicBlock)CloneSingleBasicBlock.Execute(ehBB);

                                    ((BasicBlock)edge.Predecessor).SubstituteProtectedBy(ehBB, ehBBNew);
                                }

                                fDone = false;
                                break;
                            }
                        }

                        if (fDone)
                        {
                            break;
                        }
                    }
                }

                return fModified;
            }
        }
    }
}
