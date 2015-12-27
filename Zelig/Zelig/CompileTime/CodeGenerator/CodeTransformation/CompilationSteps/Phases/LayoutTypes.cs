//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    //[PhaseDisabled()]
    [PhaseOrdering( ExecuteAfter = typeof( OrderStaticConstructors ), IsPipelineBlock = true )]
    [PhaseLimit( Operator.OperatorLevel.ConcreteTypes )]
    public sealed class LayoutTypes : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public LayoutTypes( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            this.TypeSystem.LayoutTypes( this.TypeSystem.PlatformAbstraction.MemoryAlignment );

            this.TypeSystem.DataManagerInstance.RefreshValues( this );
            
            this.TypeSystem.FlattenCallsDatabase( this.CallsDataBase.Analyze( this.TypeSystem ), fCallsTo: true  );
            this.TypeSystem.FlattenCallsDatabase( this.CallsDataBase.Analyze( this.TypeSystem ), fCallsTo: false );

            return this.NextPhase;
        }

        public override void ValidatePhaseMovement( PhaseDriver prevPhase ,
                                                    PhaseDriver nextPhase )
        {
            base.ValidatePhaseMovement( prevPhase, nextPhase );

            if(nextPhase.PhaseIndex < this.PhaseIndex)
            {
                this.TypeSystem.InvalidateLayout();
            }
        }
    }
}
