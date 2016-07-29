//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    //[PhaseDisabled()]
    [PhaseOrdering( ExecuteAfter = typeof( DetectNonImplementedInternalCalls ) )]
    public sealed class OrderStaticConstructors : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public OrderStaticConstructors( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            var mdTarget = this.TypeSystem.WellKnownMethods.TypeSystemManager_InvokeStaticConstructors;
            if(mdTarget != null)
            {
                //
                // 1) Build the call graph for the whole system.
                //
                var callGraph = new CallGraph( this.TypeSystem );

                //
                // 2) Walk through the methods that use field that require static constructors, collecting the cctors.
                //
                var constructors = HashTableFactory.NewWithReferenceEquality< MethodRepresentation, GrowOnlySet< MethodRepresentation > >();

                foreach(var lst in callGraph.MethodsThatRequireStaticConstructors.Values)
                {
                    foreach(var md in lst)
                    {
                        if(constructors.ContainsKey( md ) == false)
                        {
                            //
                            // 3) Create the call closure for each cctor, stopping at other static cctors.
                            //
                            GrowOnlySet< MethodRepresentation > setClosure = callGraph.ComputeClosure( md, FilterClosure );

                            //
                            // 4) Build a map from each static constructor to all the ones it depends on.
                            //
                            GrowOnlySet< MethodRepresentation > set = SetFactory.NewWithReferenceEquality< MethodRepresentation >();;

                            foreach(MethodRepresentation mdInClosure in setClosure)
                            {
                                if(mdInClosure != md)
                                {
                                    if(mdInClosure is StaticConstructorMethodRepresentation)
                                    {
                                        set.Insert( mdInClosure );
                                    }
                                }
                            }

                            constructors[md] = set;
                        }
                    }
                }

                //
                // 5) Create an ordered list of static constructors to invoke, breaking loops if needed.
                //
                var invocationOrder = new List< MethodRepresentation >();
                var processed       = SetFactory.NewWithReferenceEquality< MethodRepresentation >();

                //
                // 6) Iterate until all the static constructors have been ordered.
                //
                while(processed.Count < constructors.Count)
                {
                    bool fStuckInLoop = true;

                    foreach(var md in constructors.Keys)
                    {
                        if(processed.Contains( md ) == false)
                        {
                            bool fAdd = true;

                            foreach(var mdDepends in constructors[md])
                            {
                                if(processed.Contains( mdDepends ) == false)
                                {
                                    fAdd = false;
                                    break;
                                }
                            }

                            if(fAdd)
                            {
                                invocationOrder.Add   ( md );
                                processed      .Insert( md );

                                fStuckInLoop = false;
                            }
                        }
                    }

                    if(fStuckInLoop)
                    {
                        //
                        // Just pick one to break the loop.
                        //
                        MethodRepresentation mdBest = null;

                        Console.WriteLine( "WARNING: Detected loop between static constructors:" );

                        foreach(var md in constructors.Keys)
                        {
                            if(processed.Contains( md ) == false)
                            {
                                Console.WriteLine( "    {0}", md.OwnerType.FullNameWithAbbreviation );

                                if(mdBest == null)
                                {
                                    mdBest = md;
                                }
                                else if(constructors[md].Count < constructors[mdBest].Count)
                                {
                                    mdBest = md;
                                }
                            }
                        }

                        CHECKS.ASSERT( mdBest != null, "Cannot be stuck in loop between static constructors when no items in the loop" );

                        invocationOrder.Add   ( mdBest );
                        processed      .Insert( mdBest );
                    }
                }

                //
                // 7) Inject calls to static constructors into target method.
                //
                var cfg = TypeSystemForCodeTransformation.GetCodeForMethod( mdTarget );
                var bb  = cfg.NormalizedEntryBasicBlock;

                foreach(var md in invocationOrder)
                {
                    var rhs  = this.TypeSystem.AddTypePointerToArgumentsOfStaticMethod( md );
                    var call = StaticCallOperator.New( null, CallOperator.CallKind.Direct, md, rhs );

                    bb.AddOperator( call );
                }
            }

            return this.NextPhase;
        }

        private bool FilterClosure( MethodRepresentation mdNext )
        {
            if(mdNext is StaticConstructorMethodRepresentation)
            {
                return false;
            }

            var wkm = this.TypeSystem.WellKnownMethods;

            if(MethodRepresentation.SameMethodOrOverride( mdNext, wkm.TypeSystemManager_AllocateObject                  ) ||
               MethodRepresentation.SameMethodOrOverride( mdNext, wkm.TypeSystemManager_AllocateArray                   ) ||
               MethodRepresentation.SameMethodOrOverride( mdNext, wkm.TypeSystemManager_AllocateArrayNoClear            ) ||
               MethodRepresentation.SameMethodOrOverride( mdNext, wkm.TypeSystemManager_AllocateString                  ) ||
               MethodRepresentation.SameMethodOrOverride( mdNext, wkm.TypeSystemManager_AllocateReferenceCountingObject ) ||
               MethodRepresentation.SameMethodOrOverride( mdNext, wkm.TypeSystemManager_AllocateReferenceCountingArray  ) ||
               MethodRepresentation.SameMethodOrOverride( mdNext, wkm.TypeSystemManager_AllocateObjectWithExtensions    ) ) 
            {
                //
                // These methods are already working by the time we get to class constructors, so let's stop the closure here.
                //
                return false;
            }

            if (MethodRepresentation.SameMethodOrOverride(mdNext, wkm.TypeSystemManager_Throw) ||
                MethodRepresentation.SameMethodOrOverride(mdNext, wkm.TypeSystemManager_Rethrow))
            {
                //
                // If a class constructor throws an exception, bad things are going to happen, so no need to expand the closure any further.
                //
                return false;
            }

            return true;
        }
    }
}
