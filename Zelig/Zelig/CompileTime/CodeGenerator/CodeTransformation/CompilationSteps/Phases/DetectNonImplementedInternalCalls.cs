//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DEBUG_MISSING_METHODS_CALLERS
//#define STOP_ON_POSITIVE_DETECTION

namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    [PhaseOrdering( ExecuteAfter=typeof(ReduceTypeSystem) )]
    public sealed class DetectNonImplementedInternalCalls : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public DetectNonImplementedInternalCalls( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
#if DEBUG_MISSING_METHODS_CALLERS
            var missingMethods = new List< MethodRepresentation >();


            foreach(TypeRepresentation td in this.TypeSystem.Types)
            {
                if(td.IsOpenType == false)
                {
                    foreach(MethodRepresentation md in td.Methods)
                    {
                        if((md.Flags & MethodRepresentation.Attributes.Abstract) != 0) continue;

                        if(md.IsOpenMethod == false && TypeSystemForCodeTransformation.GetCodeForMethod( md ) == null)
                        {
                            missingMethods.Add( md );
                        }
                    }
                }
            }
#endif

#if STOP_ON_POSITIVE_DETECTION
            if(missingMethods.Count > 0)
            {
                DumpMissingMethods( missingMethods );
                
                throw TypeConsistencyErrorException.Create( "ERROR: found methods without an implementation" );
            }
#endif

            return this.NextPhase;
        }
        
        private void DumpMissingMethods( List< MethodRepresentation > lst )
        {
            var reverseIndex = new Transformations.ReverseIndexTypeSystem( this.TypeSystem );

            reverseIndex.ProcessTypeSystem();

            foreach(MethodRepresentation md in lst)
            {
                Console.WriteLine( "WARNING: missing implementation for {0}", md.ToShortString() );

#if DEBUG_MISSING_METHODS_CALLERS
                var set = SetFactory.New< MethodRepresentation >();

                DumpCallers( reverseIndex, set, md, 0, 2 );
#endif

                Console.WriteLine();
            }
        }

        private static void DumpCallers( Transformations.ReverseIndexTypeSystem reverseIndex ,
                                         GrowOnlySet< MethodRepresentation >    history      ,
                                         MethodRepresentation                   md           ,
                                         int                                    depth        ,
                                         int                                    maxDepth     )
        {
            if(history.Insert( md ) == true)
            {
                return;
            }

            GrowOnlySet< MethodRepresentation > set = GetCallers( reverseIndex, md );

            foreach(MethodRepresentation caller in set)
            {
                if(history.Contains( caller ) == false)
                {
                    Console.WriteLine( "        {0}called from {1}", new string( ' ', depth * 3 ), caller.ToShortString() );

                    if(depth < maxDepth)
                    {
                        DumpCallers( reverseIndex, history, caller, depth + 1, maxDepth );
                    }
                }
            }
        }

        private static GrowOnlySet< MethodRepresentation > GetCallers( Transformations.ReverseIndexTypeSystem reverseIndex ,
                                                                       MethodRepresentation                   md           )
        {
            GrowOnlySet< MethodRepresentation > res = SetFactory.NewWithReferenceEquality< MethodRepresentation >();

            GrowOnlySet< object > set = reverseIndex[md];
            if(set != null)
            {
                foreach(object obj in set)
                {
                    CallOperator op = obj as CallOperator;

                    if(op != null && op.TargetMethod == md)
                    {
                        MethodRepresentation caller = op.BasicBlock.Owner.Method;

                        res.Insert( caller );
                    }
                }
            }

            if(md is VirtualMethodRepresentation && md.VTableLayoutFlags == MethodRepresentation.Attributes.ReuseSlot)
            {
                TypeRepresentation td   = md.OwnerType;
                string             name = md.Name;

                while((td = td.Extends) != null)
                {
                    foreach(MethodRepresentation md2 in td.Methods)
                    {
                        if(td.GetDeclarationOfMethodImpl( md2 ) != null)
                        {
                            continue; // Skip any method with an explicit MethodImpl.
                        }

                        if(md2.MatchNameAndSignature( name, md, null ))
                        {
                            res.Merge( GetCallers( reverseIndex, md2 ) );
                            break;
                        }
                    }
                }
            }

            return res;
        }
    }
}
