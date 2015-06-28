//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    [PhaseOrdering( ExecuteAfter=typeof(CrossReferenceTypeSystem) )]
    public sealed class ApplyConfigurationSettings : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public ApplyConfigurationSettings( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            PerformParallelExecutionOfPhase( false, false );

            this.CallsDataBase.ExecuteInlining( this.TypeSystem );

            return this.NextPhase;
        }
    }
}
