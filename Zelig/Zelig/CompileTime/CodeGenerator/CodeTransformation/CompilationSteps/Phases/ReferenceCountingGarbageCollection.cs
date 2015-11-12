//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


using Microsoft.Zelig.Runtime.TypeSystem;
using System;

namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    [PhaseOrdering( ExecuteAfter = typeof( FromImplicitToExplicitExceptions ) )]
    public sealed class ReferenceCountingGarbageCollection : PhaseDriver
    {
        private GrowOnlySet<Operator> m_modifiedOperators;
        private GrowOnlyHashTable<MethodRepresentation, int> m_stats;

        //
        // Constructor Methods
        //

        public ReferenceCountingGarbageCollection( Controller context ) : base( context )
        {
            m_modifiedOperators = SetFactory.New<Operator>( );
            m_stats = HashTableFactory.New<MethodRepresentation, int>( );
        }

        public bool IsOperatorModified( Operator op )
        {
            lock (m_modifiedOperators)
            {
                return m_modifiedOperators.Contains( op );
            }
        }

        public void AddToModifiedOperator( Operator op )
        {
            lock (m_modifiedOperators)
            {
                m_modifiedOperators.Insert( op );
            }
        }

        public void IncrementInjectionCount( MethodRepresentation md )
        {
            lock (m_stats)
            {
                if(m_stats.ContainsKey( md ))
                {
                    m_stats.Update( md, m_stats.GetValue( md ) + 1 );
                }
                else
                {
                    m_stats.Add( md, 1 );
                }
            }
        }

        public void DecrementInjectionCount( MethodRepresentation md )
        {
            lock (m_stats)
            {
                CHECKS.ASSERT( m_stats.ContainsKey( md ), "DecrementInjectionCount error" );
                m_stats.Update( md, m_stats.GetValue( md ) - 1 );
            }
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run( )
        {
            if(this.TypeSystem.EnableReferenceCountingGarbageCollection)
            {
                // 1. Inject reference counting specific setup / helpers 
                InjectReferenceCountingHelpers( );

                // 2. Inject calls to AddReference and ReleaseReference for operators that copy references
                // 3. Inject calls to ReleaseReference at the end of each function to for each local / temp variables.
                PerformParallelExecutionOfPhase( true, false );

                // 4. Reduce number of temporaries again to clean up the injected code
                ParallelTransformationsHandler.EnumerateFlowGraphs( this.TypeSystem, delegate ( ControlFlowGraphStateForCodeTransformation cfg )
                {
                    Transformations.ReduceNumberOfTemporaries.Execute( cfg );
                } );

                // 5: Dump out the counts of each injection for informational purpose
                DumpInjectionStats( );
            }

            return this.NextPhase;
        }

        private void DumpInjectionStats()
        {
            Console.WriteLine( );
            Console.WriteLine( "    Reference Counting GC Code Injection Stats" );
            Console.WriteLine( " ================================================" );

            int total = 0;
            foreach(var md in m_stats.Keys)
            {
                var count = m_stats.GetValue( md );
                Console.WriteLine( "    {0,-20} :{1, 6}", md.Name, count );
                total += count;
            }
            Console.WriteLine( " ------------------------------------------------" );
            Console.WriteLine( "    {0,-20} :{1, 6}", "Total", total );
            Console.WriteLine( " ================================================" );
            Console.WriteLine( );
        }

        private struct Injection
        {
            public String                                                         method;
            public Func<ControlFlowGraphStateForCodeTransformation, Expression[]> arguments;
            public String                                                         target;
            public BasicBlock.Qualifier                                           injectionPoint;
        }

        private void InjectReferenceCountingHelpers( )
        {
            Injection[] injections = new Injection[] {
                new Injection {
                    method         = "ThreadImpl_AllocateReleaseReferenceHelper",
                    arguments      = cfg => new Expression[] { cfg.Arguments[ 0 ] },
                    target         = "ThreadImpl_ctor",
                    injectionPoint = BasicBlock.Qualifier.EntryInjectionStart
                },

                new Injection {
                    method         = "Bootstrap_ReferenceCountingInitialization",
                    arguments      = cfg => Expression.SharedEmptyArray,
                    target         = "Bootstrap_HeapInitialization",
                    injectionPoint = BasicBlock.Qualifier.ExitInjectionStart
                },

                new Injection {
                    method         = "ThreadManager_CleanupBootstrapThread",
                    arguments      = cfg => Expression.SharedEmptyArray,
                    target         = "ThreadManager_CleanupBootstrapThreadIfNeeded",
                    injectionPoint = BasicBlock.Qualifier.EntryInjectionStart
                },
            };

            var ts = this.TypeSystem;

            foreach (var injection in injections)
            {
                var target = ts.GetWellKnownMethod( injection.target );
                var cfg = TypeSystemForCodeTransformation.GetCodeForMethod( target );
                var injectionPoint = cfg.GetInjectionPoint( injection.injectionPoint );

                var method = ts.GetWellKnownMethod( injection.method );
                var arguments = injection.arguments( cfg );

                Operator call;
                if (method is StaticMethodRepresentation)
                {
                    var rhs = ts.AddTypePointerToArgumentsOfStaticMethod( method, arguments );
                    call = StaticCallOperator.New( null, CallOperator.CallKind.Direct, method, rhs );
                }
                else
                {
                    call = InstanceCallOperator.New( null, CallOperator.CallKind.Direct, method, arguments, false );
                }

                injectionPoint.AddOperator( call );
            }
        }
    }
}
