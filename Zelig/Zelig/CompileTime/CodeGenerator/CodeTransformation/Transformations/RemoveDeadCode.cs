//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Transformations
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    public static class RemoveDeadCode
    {
        //
        // Helper Methods
        //

        public static bool Execute( ControlFlowGraphStateForCodeTransformation cfg                     ,
                                    bool                                       fLookForDeadAssignments )
        {
            cfg.TraceToFile( "RemoveDeadCode" );

            using(new PerformanceCounters.ContextualTiming( cfg, "RemoveDeadCode" ))
            {
                bool fModified = false;

                while(true)
                {
                    VariableExpression[]              vars         = cfg.DataFlow_SpanningTree_Variables;
                    VariableExpression.Property[]     varProp      = cfg.DataFlow_PropertiesOfVariables;
                    Operator[]                        operators    = cfg.DataFlow_SpanningTree_Operators;
                    Operator[][]                      defChains    = cfg.DataFlow_DefinitionChains;
                    Operator[][]                      useChains    = cfg.DataFlow_UseChains;
                    Abstractions.Platform             pa           = cfg.TypeSystem.PlatformAbstraction;
                    Abstractions.RegisterDescriptor[] registers    = pa.GetRegisters();
                    bool[]                            unneededVars = new bool[vars.Length];
                    bool                              fDone        = true;

                    for(int i = 0; i < vars.Length; i++)
                    {
                        VariableExpression.Property prop = varProp[i];

                        if((prop & VariableExpression.Property.PhysicalRegister) != 0)
                        {
                            PhysicalRegisterExpression reg = (PhysicalRegisterExpression)vars[i].AliasedVariable;

                            if(registers[ reg.Number ].IsSpecial)
                            {
                                continue;
                            }
                        }

                        {
                            VariableExpression var   = vars[i];
                            bool               fKeep = false;

                            if(ShouldKeep( cfg, var, defChains, useChains ))
                            {
                                fKeep = true;
                            }
                            else if(var.AliasedVariable is StackLocationExpression)
                            {
                                //
                                // Check if the variable is a fragment of an aggregate type, keep it around if any of the fragments are used.
                                //
                                StackLocationExpression stackVar = (StackLocationExpression)var.AliasedVariable;

                                foreach(VariableExpression var2 in vars)
                                {
                                    StackLocationExpression stackVar2 = var2.AliasedVariable as StackLocationExpression;

                                    if(stackVar2 != null && stackVar.SourceVariable == stackVar2.SourceVariable)
                                    {
                                        if(useChains[var2.SpanningTreeIndex].Length > 0)
                                        {
                                            fKeep = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if(fKeep == false)
                            {
                                CHECKS.ASSERT( (prop & VariableExpression.Property.AddressTaken) == 0, "Variable {0} cannot be unused and its address taken", vars[i] );

                                unneededVars[i] = true;
                            }
                        }
                    }

                    //
                    // Only remove an operator if ALL the results are dead.
                    //
                    foreach(var op in operators)
                    {
                        if(op.HasAnnotation< DontRemoveAnnotation >() == false)
                        {
                            bool fDelete = false;

                            foreach(var lhs in op.Results)
                            {
                                if(unneededVars[lhs.SpanningTreeIndex])
                                {
                                    fDelete = true;
                                }
                                else
                                {
                                    fDelete = false;
                                    break;
                                }
                            }

                            if(fDelete)
                            {
                                op.Delete();
                                fDone = false;
                            }
                        }
                    }

                    //
                    // Refresh the u/d chains.
                    //
                    if(fDone == false)
                    {
                        varProp   = cfg.DataFlow_PropertiesOfVariables;
                        operators = cfg.DataFlow_SpanningTree_Operators;
                        defChains = cfg.DataFlow_DefinitionChains;
                        useChains = cfg.DataFlow_UseChains;
                    }

                    //
                    // Get rid of all the operators that do nothing, evaluate constant operations, etc.
                    //
                    BitVector[] livenessMap = fLookForDeadAssignments ? cfg.DataFlow_LivenessAtOperator : null; // It's indexed as [<operator index>][<variable index>]

                    foreach(Operator op in operators)
                    {
                        if(op.PerformsNoActions)
                        {
                            op.Delete();
                            fDone = false;
                            continue;
                        }

                        op.EnsureConstantToTheRight();

                        if(op.Simplify( defChains, useChains, varProp ))
                        {
                            fDone = false;
                            continue;
                        }

                        if(fLookForDeadAssignments)
                        {
                            //
                            // All the variables have to be initialized with their default values,
                            // but most of the times these assignments are overwritten by other assignments.
                            // Look for these pattern and remove the duplicate code.
                            //
                            if(op is SingleAssignmentOperator && op.HasAnnotation< InvalidationAnnotation >() == false)
                            {
                                int idxVar = op.FirstResult.SpanningTreeIndex;

                                if((varProp[idxVar] & VariableExpression.Property.AddressTaken) == 0)
                                {
                                    int idxOp = op.SpanningTreeIndex;

                                    if(livenessMap[idxOp+1][idxVar] == false)
                                    {
                                        //
                                        // The variable is dead at the next operator, thus it's a useless assignment.
                                        //
                                        op.Delete();
                                        fDone = false;
                                        continue;
                                    }
                                }
                            }
                        }
                    }

                    if(fDone)
                    {
                        break;
                    }

                    fModified = true;
                }

                return fModified;
            }
        }

        private static bool ShouldKeep( ControlFlowGraphStateForCodeTransformation cfg       ,
                                        VariableExpression                         var       ,
                                        Operator[][]                               defChains ,
                                        Operator[][]                               useChains )
        {
            if(ShouldKeep( var, defChains, useChains ))
            {
                return true;
            }

            Expression[] fragments = cfg.GetFragmentsForExpression( var );
            if(fragments != null)
            {
                foreach(Expression ex in fragments)
                {
                    if(ShouldKeep( ex, defChains, useChains ))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool ShouldKeep( Expression   var       ,
                                        Operator[][] defChains ,
                                        Operator[][] useChains )
        {
            if(useChains[var.SpanningTreeIndex].Length > 0)
            {
                return true;
            }

            foreach(Operator op in defChains[var.SpanningTreeIndex])
            {
                if(op.ShouldNotBeRemoved       ||
                   op.MayMutateExistingStorage ||
                   op.MayThrow                  )
                {
                    return true;
                }

                foreach(var an in op.FilterAnnotations< InvalidationAnnotation >())
                {
                    if(useChains[an.Target.SpanningTreeIndex].Length > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
