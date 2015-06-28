//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    [PhaseOrdering( ExecuteAfter=typeof(ComputeCallsClosure))]
    public sealed class EstimateTypeSystemReduction : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public EstimateTypeSystemReduction( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            this.TypeSystem.EstimateReduction();

            return this.NextPhase;
        }
    }
}
