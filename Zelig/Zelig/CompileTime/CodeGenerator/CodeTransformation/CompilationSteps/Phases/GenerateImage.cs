//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    [PhaseOrdering( ExecuteAfter=typeof(AllocateRegisters) )]
    public sealed class GenerateImage : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public GenerateImage( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            ParallelTransformationsHandler.EnumerateFlowGraphs(
                this.TypeSystem,
                delegate( ControlFlowGraphStateForCodeTransformation cfg )
                {
                    Transformations.MergeExtendedBasicBlocks.Execute( cfg, preserveInjectionSites: false );
                } );

            this.TypeSystem.GenerateImage( this );

            return this.NextPhase;
        }
    }
}
