//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DEBUG_INCLUDE_ALL_METHODS

namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;

    using Microsoft.Zelig.Runtime.TypeSystem;

    [PhaseOrdering( ExecuteAfter = typeof( PropagateCompilationConstraints ) )]
    public sealed class ComputeCallsClosure : PhaseDriver
    {
        //
        // State
        //

        private CompilationSteps.ComputeCallsClosure m_state;

        //
        // Constructor Methods
        //

        public ComputeCallsClosure( Controller context )
            : base( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run( )
        {
            this.CallsDataBase.ResetCallSites( );

            m_state = new CompilationSteps.ComputeCallsClosure( this.TypeSystem, this.DelegationCache, this.CallsDataBase, this, false );

            if( this.TypeSystem.PlatformAbstraction.PlatformName.Equals( "LLVM" ) )
            {
                //MethodRepresentation mainThread = null;
                //// HACK: for LLVM, we will use the ThreadManager::MainThread method as the entry point for calls closure
                //this.TypeSystem.EnumerateMethods( delegate( MethodRepresentation md )
                //{
                //    if( md.Name.Equals( "MainThread" ) && md.OwnerType.Equals( this.TypeSystem.WellKnownMethods ) )
                //    {
                //        mainThread = md;
                //    }
                //} );

                //if( mainThread  != null)
                //{
                //    m_state.Execute( mainThread );
                //}
                //else
                //{
                //    CHECKS.ASSERT( false, "No Thread Manager or no MainThread method found!!" ); 
                //}

                //// HACK: for LLVM, we will use the Bootstrap_Initialization method as the entry point for calls closure
                m_state.Execute( this.TypeSystem.GetWellKnownMethod( "Bootstrap_Initialization" ) );
            }
            else
            {
#if DEBUG_INCLUDE_ALL_METHODS
            this.TypeSystem.EnumerateMethods( delegate( MethodRepresentation md )
            {
                m_state.Execute( md );
            } );
#else
                m_state.Execute( this.TypeSystem.GetHandler( Runtime.HardwareException.Reset ) );
#endif

            }
            this.TypeSystem.ExpandCallsClosure( m_state );

            var touched = this.CallsDataBase.ExecuteInlining( this.TypeSystem );
            if( touched.Count > 0 )
            {
                return this;
            }

            return this.NextPhase;
        }

        //
        // Access Methods
        //

        public GrowOnlyHashTable<MethodRepresentation, GrowOnlySet<object>> EntitiesReferencedByMethods
        {
            get
            {
                return m_state.EntitiesReferencedByMethods;
            }
        }
    }
}
