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

            if (this.TypeSystem.EnableReferenceCountingGarbageCollection)
            {
                // Keep the reference counting methods around so they stay alive when we inject 
                // auto reference counting code in the ReferenceCountingGarbageCollection phase
                var wkm = this.TypeSystem.WellKnownMethods;
                m_state.Execute( wkm.ObjectHeader_AddReference );
                m_state.Execute( wkm.ObjectHeader_ReleaseReference );
                m_state.Execute( wkm.ThreadImpl_AllocateReleaseReferenceHelper );
                m_state.Execute( wkm.ReferenceCountingCollector_Swap );
                m_state.Execute( wkm.ReferenceCountingCollector_LoadAndAddReference );
                m_state.Execute( wkm.StringImpl_FastAllocateReferenceCountingString );
                m_state.Execute( wkm.Bootstrap_ReferenceCountingInitialization );
                m_state.Execute( wkm.InterlockedImpl_ReferenceCountingExchange );
                m_state.Execute( wkm.InterlockedImpl_ReferenceCountingCompareExchange );
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
