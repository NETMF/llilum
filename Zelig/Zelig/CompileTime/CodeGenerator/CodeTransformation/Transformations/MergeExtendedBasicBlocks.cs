//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public static class MergeExtendedBasicBlocks
    {
        //
        // Helper Methods
        //

        public static bool Execute( ControlFlowGraphStateForCodeTransformation cfg )
        {
            cfg.TraceToFile( "MergeExtendedBasicBlocks" );

            using(new PerformanceCounters.ContextualTiming( cfg, "MergeExtendedBasicBlocks" ))
            {
                bool fModified = false;

                while(true)
                {
                    BasicBlock[] basicBlocks = cfg.DataFlow_SpanningTree_BasicBlocks;
                    bool         fDone       = true;

                    foreach(BasicBlock bb in basicBlocks)
                    {
                        if(bb is NormalBasicBlock)
                        {
                            UnconditionalControlOperator ctrl = bb.FlowControl as UnconditionalControlOperator;

                            if(ctrl != null)
                            {
                                BasicBlock bbNext = ctrl.TargetBranch;
                                if(bbNext is NormalBasicBlock                  &&
                                   bbNext.Predecessors.Length == 1             &&
                                   bbNext.Annotation          == bb.Annotation  )
                                {
                                    //
                                    // Same exception handling?
                                    //
                                    bool fOkToRemove = ArrayUtility.ArrayEqualsNotNull( bb.ProtectedBy, bbNext.ProtectedBy, 0 );

                                    if(fOkToRemove)
                                    {
                                        foreach(Operator op in bbNext.Operators)
                                        {
                                            if(op is PhiOperator ||
                                               op is PiOperator   )
                                            {
                                                fOkToRemove = false;
                                                break;
                                            }
                                        }
                                    }

                                    if(fOkToRemove)
                                    {
                                        //
                                        // The two blocks have the same exception handling settings, so we can merge them.
                                        //
                                        bb.Merge( bbNext );

                                        fModified = true;
                                        fDone     = false;
                                        break;
                                    }
                                }

                                //
                                // Empty block, not looping to itself.
                                //
                                if(bb.Operators.Length == 1                 &&
                                   bb                  != bbNext            &&
                                   bb.Annotation       == bbNext.Annotation  )
                                {
                                    bool fProceed = true;

                                    //
                                    // It's too complicated to remove a basic block if the successor contains phi operators.
                                    // It's also useless to try, since we'll get a chance later when transitioning out of SSA.
                                    //
                                    foreach(Operator op in bbNext.Operators)
                                    {
                                        if(op is PhiOperator)
                                        {
                                            fProceed = false;
                                            break;
                                        }
                                    }

                                    if(fProceed)
                                    {
                                        foreach(BasicBlockEdge edge in bb.Predecessors)
                                        {
                                            BasicBlock bbPrev = edge.Predecessor;

                                            if(bbPrev is NormalBasicBlock)
                                            {
                                                if(ArrayUtility.ArrayEqualsNotNull( bb.ProtectedBy, bbPrev.ProtectedBy, 0 ))
                                                {
                                                    //
                                                    // The two blocks have the same exception handling settings, so we can redirect.
                                                    //
                                                    if(bbPrev.FlowControl.SubstituteTarget( bb, bbNext ))
                                                    {
                                                        fModified = true;
                                                        fDone     = false;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if(fDone) break;
                }

                return fModified;
            }
        }
    }
}
