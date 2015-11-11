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

        //
        // Constructor Methods
        //

        public ReferenceCountingGarbageCollection( Controller context ) : base( context )
        {
            m_modifiedOperators = SetFactory.New<Operator>( );
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
            }

            return this.NextPhase;
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
