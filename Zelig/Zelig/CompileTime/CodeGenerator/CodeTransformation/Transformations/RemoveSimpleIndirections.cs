//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    public static class RemoveSimpleIndirections
    {
        //
        // Helper Methods
        //

        public static bool Execute( ControlFlowGraphStateForCodeTransformation cfg )
        {
            cfg.TraceToFile( "RemoveSimpleIndirections" );

            using(new PerformanceCounters.ContextualTiming( cfg, "RemoveSimpleIndirections" ))
            {
                Operator[][] defChains = cfg.DataFlow_DefinitionChains;
                Operator[][] useChains = cfg.DataFlow_UseChains;
                bool         fModified = false;

                foreach(var opDef in cfg.FilterOperators< AddressAssignmentOperator >())
                {
                    VariableExpression src = opDef.FirstArgument as VariableExpression;
                    VariableExpression ptr = opDef.FirstResult;

                    if(src != null && ControlFlowGraphState.CheckSingleDefinition( defChains, ptr ) != null)
                    {
                        //
                        // Digest local indirections.
                        //

                        foreach(Operator opUse in useChains[ptr.SpanningTreeIndex])
                        {
                            IndirectOperator opUseInd = opUse as IndirectOperator;

                            if(opUseInd != null && opUseInd.Offset == 0)
                            {
                                if(opUseInd is StoreIndirectOperator)
                                {
                                    if(CanReplace( opUseInd, opUseInd.FirstArgument, ptr, (VariableExpression)src, opUseInd.SecondArgument ))
                                    {
                                        Operator op = SingleAssignmentOperator.New( opUseInd.DebugInfo, (VariableExpression)src, opUseInd.SecondArgument );
                                        opUseInd.SubstituteWithOperator( op, Operator.SubstitutionFlags.Default );
                                        fModified = true;
                                    }
                                }
                                else if(opUseInd is LoadIndirectOperator)
                                {
                                    if(CanReplace( opUseInd, opUseInd.FirstArgument, ptr, opUseInd.FirstResult, src ))
                                    {
                                        Operator op = SingleAssignmentOperator.New( opUseInd.DebugInfo, opUseInd.FirstResult, src );
                                        opUseInd.SubstituteWithOperator( op, Operator.SubstitutionFlags.Default );
                                        fModified = true;
                                    }
                                }
                            }
                        }
                    }
                }

                return fModified;
            }
        }

        private static bool CanReplace( IndirectOperator op     ,
                                        Expression       ptrUse ,
                                        Expression       ptrSrc ,
                                        Expression       target ,
                                        Expression       origin )
        {
            FieldRepresentation[] fdPath = op.AccessPath;

            if(fdPath != null)
            {
                TypeRepresentation tdTarget = target.Type;
                TypeRepresentation tdOrigin = origin.Type;

                if(tdTarget.ValidLayout &&
                   tdOrigin.ValidLayout  )
                {
                    if(tdTarget.SizeOfHoldingVariable == tdOrigin.SizeOfHoldingVariable)
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return ptrUse == ptrSrc;
            }
        }
    }
}
