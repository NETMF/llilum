//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

// Enable this macro to run LLVM-specific transforms prior to generating the image.
#define LLVM

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
#if LLVM
            ParallelTransformationsHandler.EnumerateFlowGraphs(
                this.TypeSystem,
                delegate( ControlFlowGraphStateForCodeTransformation cfg )
                {
                    Transformations.MergeExtendedBasicBlocks.Execute( cfg, preserveInjectionSites: false );
                } );
#endif // LLVM

            this.TypeSystem.GenerateImage( this );

            return this.NextPhase;
        }
    }
}
