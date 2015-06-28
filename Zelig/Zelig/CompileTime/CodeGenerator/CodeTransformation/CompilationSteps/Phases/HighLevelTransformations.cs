//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    [PhaseOrdering( ExecuteAfter = typeof(ApplyConfigurationSettings))]
    public sealed class HighLevelTransformations : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public HighLevelTransformations( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            PerformParallelExecutionOfPhase( false, false );

            return this.NextPhase;
        }
    }
}
