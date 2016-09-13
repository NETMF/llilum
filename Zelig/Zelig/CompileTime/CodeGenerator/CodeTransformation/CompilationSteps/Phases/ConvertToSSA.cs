//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    //[PhaseDisabled()]
    [PhaseOrdering( ExecuteAfter=typeof(Optimizations) )]
    public sealed class ConvertToSSA : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public ConvertToSSA( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            PerformParallelExecutionOfPhase( false, true );

            return this.NextPhase;
        }
    }
}
