//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


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
                // 1. Inject AllocateReleaseReferenceHelper() call to Thread::.ctor so each thread has its own 
                //    ReleaseReferenceHelper.
                //    Inject ReferenceCountingInitialization() call to Bootstrap::HeapInitialization
                InjectAllocateReleaseReferenceHelper( );
                InjectReferenceCountingInitialization( );

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

        private void InjectAllocateReleaseReferenceHelper( )
        {
            var ts = this.TypeSystem;
            var wkm = ts.WellKnownMethods;

            var mdThreadCtor = wkm.ThreadImpl_ctor;
            var mdAllocateRRH = wkm.ThreadImpl_AllocateReleaseReferenceHelper;

            var cfg = TypeSystemForCodeTransformation.GetCodeForMethod( mdThreadCtor );
            var injectionPoint = cfg.GetInjectionPoint( BasicBlock.Qualifier.EntryInjectionStart );

            var rhs = new Expression[] { cfg.Arguments[ 0 ] }; // Arguments[0] is this variable
            var call = InstanceCallOperator.New( null, CallOperator.CallKind.Direct, mdAllocateRRH, rhs, false );

            injectionPoint.AddOperator( call );
        }

        private void InjectReferenceCountingInitialization( )
        {
            var ts = this.TypeSystem;
            var wkm = ts.WellKnownMethods;

            var mdRCInit = wkm.Bootstrap_ReferenceCountingInitialization;
            var mdHeapInit = wkm.Bootstrap_HeapInitialization;

            var cfg = TypeSystemForCodeTransformation.GetCodeForMethod( mdHeapInit );
            var injectionPoint = cfg.GetInjectionPoint( BasicBlock.Qualifier.ExitInjectionStart );

            var rhs = ts.AddTypePointerToArgumentsOfStaticMethod( mdRCInit, Expression.SharedEmptyArray );
            var call = StaticCallOperator.New( null, CallOperator.CallKind.Direct, mdRCInit, rhs );

            injectionPoint.AddOperator( call );
        }
    }
}
