//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Zelig.Runtime.TypeSystem;

    public static class ConvertToLandingPads
    {
        class ExceptionRegion
        {
            public ExceptionRegion(ExceptionHandlerBasicBlock[] protectedBy, ExceptionHandlerBasicBlock landingPad)
            {
                ProtectedBy = protectedBy;
                LandingPad = landingPad;
            }

            public ExceptionHandlerBasicBlock[] ProtectedBy
            {
                get;
            }

            public ExceptionHandlerBasicBlock LandingPad
            {
                get;
            }
        };

        public static void Execute(ControlFlowGraphStateForCodeTransformation cfg)
        {
            WellKnownTypes wkt = cfg.TypeSystem.WellKnownTypes;
            var handledRegions = new List<ExceptionRegion>();

            foreach (BasicBlock block in cfg.DataFlow_SpanningTree_BasicBlocks)
            {
                // Skip blocks that aren't protected.
                if (block.ProtectedBy.Length == 0)
                {
                    continue;
                }

                ExceptionRegion handledRegion = null;
                foreach (var region in handledRegions)
                {
                    if (ArrayUtility.ArrayEqualsNotNull(block.ProtectedBy, region.ProtectedBy, 0))
                    {
                        handledRegion = region;
                        break;
                    }
                }

                // If we haven't handled this region, create a new landing pad block for it.
                if (handledRegion == null)
                {
                    var handledTypes = new Expression[block.ProtectedBy.Length];
                    var handlerBlocks = new BasicBlock[block.ProtectedBy.Length];

                    // TODO: Assert that all ProtectedBy blocks have the same protection.
                    for (int i = 0; i < block.ProtectedBy.Length; ++i)
                    {
                        ExceptionHandlerBasicBlock handler = block.ProtectedBy[i];

                        // All class types for a given handler are required to be the same. We can
                        // therefore inspect only the first one.
                        TypeRepresentation clauseType = handler.HandlerFor[0].ClassObject;

                        // Finally blocks are handled as special clauses with a null vtable. This is
                        // distinct from a catch-all clause, which has an Object vtable.
                        Expression virtualTable;
                        if (clauseType == null)
                        {
                            virtualTable = cfg.TypeSystem.CreateNullPointer(wkt.Microsoft_Zelig_Runtime_TypeSystem_VTable);
                        }
                        else
                        {
                            virtualTable = cfg.TypeSystem.CreateConstantFromObject(clauseType.VirtualTable);
                        }

                        // We set the handler's successor here because we don't want to jump into an
                        // exception handling block with normal flow control. Instead, we'll replace
                        // all exception header blocks for this protected region with a single new
                        // header containing only a landing pad.
                        handledTypes[i] = virtualTable;
                        handlerBlocks[i] = handler.FirstSuccessor;
                    }

                    // Expected form for landing pads: The pad entry serves as the exception "header"
                    // block, and contains only the landing pad instruction itself. This serves as a
                    // single entry point for all handled exception types. The landing pad operator
                    // must be the first operator reached when an exception is thrown.
                    //
                    // Upon entering the landing pad, we select a block based on the result of the
                    // personality function. The handler blocks are ordered and the selector will be
                    // an index into the list of handlers. If the selector doesn't match any block,
                    // we resume unwinding the stack and leave the exception unhandled.

                    TypeRepresentation resultType = wkt.Microsoft_Zelig_Runtime_LandingPadResult;
                    VariableExpression result = cfg.AllocateTemporary(resultType, null);
                    VariableExpression exception = cfg.AllocateTemporary(resultType.Fields[0].FieldType, null);
                    VariableExpression selector = cfg.AllocateTemporary(resultType.Fields[1].FieldType, null);

                    var padEntry = new ExceptionHandlerBasicBlock(cfg);
                    var padBody = NormalBasicBlock.CreateWithSameProtection(handlerBlocks[0]);
                    var resumeBlock = NormalBasicBlock.CreateWithSameProtection(handlerBlocks[0]);

                    // Landing pad entry
                    var landingPadOp = LandingPadOperator.New(null, result, handledTypes, false);
                    landingPadOp.AddAnnotation(DontRemoveAnnotation.Create(cfg.TypeSystemForIR));
                    padEntry.AddOperator(landingPadOp);
                    padEntry.FlowControl = UnconditionalControlOperator.New(null, padBody);

                    // Landing pad body
                    var handlersWithCleanup = ArrayUtility.InsertAtHeadOfNotNullArray(handlerBlocks, resumeBlock);
                    padBody.AddOperator(LoadInstanceFieldOperator.New(null, resultType.Fields[1], selector, result, false));
                    padBody.FlowControl = MultiWayConditionalControlOperator.New(null, selector, resumeBlock, handlersWithCleanup);

                    // Resume block
                    resumeBlock.AddOperator(LoadInstanceFieldOperator.New(null, resultType.Fields[0], exception, result, false));
                    resumeBlock.FlowControl = ResumeUnwindOperator.New(null, exception);

                    handledRegion = new ExceptionRegion(block.ProtectedBy, padEntry);
                    handledRegions.Add(handledRegion);
                }

                // Replace the handler for the current block with the landing pad.
                block.ClearProtectedBy();
                block.SetProtectedBy(handledRegion.LandingPad);
            }
        }
    }
}
