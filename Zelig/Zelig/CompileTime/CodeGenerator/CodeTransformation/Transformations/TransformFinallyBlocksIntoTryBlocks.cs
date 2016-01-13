//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DEBUG_REMOVEUNNECESSARYTEMPORARIES

namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public static class TransformFinallyBlocksIntoTryBlocks
    {
        struct FinallyRange
        {
            public ExceptionHandlerBasicBlock Entry;
            public BasicBlock Exit;

            public override bool Equals(Object obj)
            {
                return (obj is FinallyRange) && (this == (FinallyRange)obj);
            }

            public override int GetHashCode()
            {
                return Entry.GetHashCode() ^ Exit.GetHashCode();
            }

            public static bool operator ==(FinallyRange x, FinallyRange y)
            {
                return (x.Entry == y.Entry) && (x.Entry == y.Entry);
            }

            public static bool operator !=(FinallyRange x, FinallyRange y)
            {
                return !(x == y);
            }
        };

        public static void Execute(ControlFlowGraphStateForCodeTransformation cfg)
        {
            // This maps finally block ranges to the entry block of their cloned 'normal' region.
            var clonedRanges = HashTableFactory.New<FinallyRange, BasicBlock>();

            while (true)
            {
                cfg.TraceToFile( "TransformFinallyBlocksIntoTryBlocks" );

                bool fDone = true;

                foreach (BasicBlock block in cfg.DataFlow_SpanningTree_BasicBlocks)
                {
                    if (RemoveFinally(cfg, block, clonedRanges))
                    {
                        fDone = false;
                        break;
                    }
                }

                if(fDone)
                {
                    CHECKS.ASSERT( FindOperation( cfg.DataFlow_SpanningTree_Operators, typeof(EndFinallyControlOperator) ) == null, "Internal error: failed to remove 'endfinally' operators in {0}.", cfg.Method );

                    break;
                }
            }
        }

        private static bool RemoveFinally(
            ControlFlowGraphStateForCodeTransformation cfg,
            BasicBlock block,
            GrowOnlyHashTable<FinallyRange, BasicBlock> clonedRanges)
        {
            ExceptionHandlerBasicBlock ehBB = block as ExceptionHandlerBasicBlock;
            if (ehBB == null)
            {
                return false;
            }

            foreach(ExceptionClause ec in ehBB.HandlerFor)
            {
                if(ec.Flags == ExceptionClause.ExceptionFlag.Finally)
                {
                    GrowOnlySet<BasicBlock> setVisited = SetFactory.NewWithReferenceEquality<BasicBlock>();

                    //
                    // Collect all the basic blocks that are part of the handler.
                    //
                    CollectFinallyHandler(ehBB, setVisited);

                    //
                    // First, see if there's any nested finally clause. If so, bail out.
                    //
                    foreach (BasicBlock bbSub in setVisited)
                    {
                        if (bbSub != ehBB)
                        {
                            if (RemoveFinally(cfg, bbSub, clonedRanges))
                            {
                                return true;
                            }
                        }
                    }

                    GrowOnlySet<ControlOperator> setLeave = SetFactory.NewWithReferenceEquality<ControlOperator>();
                    GrowOnlySet<ControlOperator> setFinally = SetFactory.NewWithReferenceEquality<ControlOperator>();

                    //
                    // Find all the leave operators in the basic blocks protected by this finally handler.
                    //
                    FindAllLeaveOperators(ehBB, setLeave);

                    //
                    // For each of them:
                    //
                    //      1) Clone the finally handler(s) protecting the leaving block.
                    //      2) Convert the leave to a branch to the cloned handler.
                    //      3) Change endfinally to a leave to the same original target.
                    //
                    foreach (LeaveControlOperator leave in setLeave)
                    {
                        CloningContext context = new CloneForwardGraphButLinkToExceptionHandlers(cfg, cfg, null);
                        var finallyHandlers = new Stack<BasicBlock>();

                        FinallyRange currentRange = new FinallyRange{ Entry = null, Exit = leave.TargetBranch };

                        // Unwind all finally blocks that either ehBB itself or protected by it. These are always in
                        // order from innermost to outermost; we can just chain them together until we hit ehBB.
                        foreach (var handler in leave.BasicBlock.ProtectedBy)
                        {
                            foreach (var clause in handler.HandlerFor)
                            {
                                if ((clause.Flags & ExceptionClause.ExceptionFlag.Finally) != 0)
                                {
                                    // Set the range's entry block to the first one we encounter.
                                    if (currentRange.Entry == null)
                                    {
                                        currentRange.Entry = handler;
                                    }

                                    finallyHandlers.Push(handler);
                                    break;
                                }
                            }

                            if (handler == ehBB)
                            {
                                break;
                            }
                        }

                        // If we already cloned this range of finally blocks, just reuse the existing entry block.
                        BasicBlock nextBlock;
                        if (!clonedRanges.TryGetValue(currentRange, out nextBlock))
                        {
                            nextBlock = leave.TargetBranch;

                            // Clone each handler block in reverse order.
                            while (finallyHandlers.Count != 0)
                            {
                                BasicBlock handler = finallyHandlers.Pop();
                                BasicBlock clonedBlock = context.Clone(handler.FirstSuccessor);

                                setFinally.Clear();
                                FindAllFinallyOperators(clonedBlock, setFinally);
                                SubstituteFinallyForBranch(setFinally, nextBlock);

                                nextBlock = clonedBlock;
                            }

                            clonedRanges[currentRange] = nextBlock;
                        }

                        var leaveBranch = UnconditionalControlOperator.New(leave.DebugInfo, nextBlock);
                        leave.SubstituteWithOperator(leaveBranch, Operator.SubstitutionFlags.Default);
                    }

                    // To finish, all the endfinally operators should be converted to simple rethrow.
                    setFinally.Clear();
                    FindAllFinallyOperators(ehBB, setFinally);
                    SubstituteFinallyForRethrow(setFinally);

                    // Convert the finally exception clauses to catch all ones.
                    ExceptionClause ecNew = new ExceptionClause(ExceptionClause.ExceptionFlag.None, null);
                    ehBB.SubstituteHandlerFor(ec, ecNew);

                    return true;
                }
            }

            return false;
        }

        private static Operator FindOperation( Operator[] ops   ,
                                               Type       match )
        {
            foreach(Operator op in ops)
            {
                if(op.GetType() == match)
                {
                    return op;
                }
            }

            return null;
        }

        private static void CollectFinallyHandler( BasicBlock                bb         ,
                                                   GrowOnlySet< BasicBlock > setVisited )
        {
            setVisited.Insert( bb );

            foreach(BasicBlockEdge edge in bb.Successors)
            {
                BasicBlock succ = edge.Successor;

                if(succ is ExceptionHandlerBasicBlock)
                {
                    //
                    // Don't follow exceptional edges.
                    //
                }
                else if(setVisited.Contains( succ ) == false)
                {
                    CollectFinallyHandler( succ, setVisited );
                }
            }
        }

        private static void FindAllLeaveOperators( BasicBlock                   bb       ,
                                                   GrowOnlySet<ControlOperator> setLeave )
        {
            foreach(BasicBlockEdge edge in bb.Predecessors)
            {
                BasicBlock           pred = edge.Predecessor;
                LeaveControlOperator ctrl = pred.FlowControl as LeaveControlOperator;

                if(ctrl != null)
                {
                    //
                    // Only consider operators leaving the area protected by the handler.
                    //
                    if(pred             .IsProtectedBy( bb ) == true  &&
                       ctrl.TargetBranch.IsProtectedBy( bb ) == false  )
                    {
                        setLeave.Insert( ctrl );
                    }
                }
            }
        }
        
        private static void FindAllFinallyOperators( BasicBlock                   bb         ,
                                                     GrowOnlySet<ControlOperator> setFinally )
        {
            GrowOnlySet< BasicBlock > setVisited = SetFactory.NewWithReferenceEquality< BasicBlock >();

            CollectFinallyHandler( bb, setVisited );

            foreach(BasicBlock bb2 in setVisited)
            {
                ControlOperator ctrl = bb2.FlowControl;

                if(ctrl is EndFinallyControlOperator)
                {
                    setFinally.Insert( ctrl );
                }
            }
        }

        private static void SubstituteFinallyForBranch(GrowOnlySet<ControlOperator> setFinally, BasicBlock target)
        {
            foreach (ControlOperator op in setFinally)
            {
                var opNew = UnconditionalControlOperator.New(op.DebugInfo, target);
                op.SubstituteWithOperator(opNew, Operator.SubstitutionFlags.Default);
            }
        }

        private static void SubstituteFinallyForRethrow( GrowOnlySet<ControlOperator> setFinally )
        {
            foreach(ControlOperator op in setFinally)
            {
                RethrowControlOperator opNew = RethrowControlOperator.New( op.DebugInfo );
                op.SubstituteWithOperator( opNew, Operator.SubstitutionFlags.Default );
            }
        }
    }
}
