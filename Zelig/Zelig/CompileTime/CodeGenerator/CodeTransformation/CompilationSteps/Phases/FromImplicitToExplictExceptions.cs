//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    //[PhaseDisabled()]
    [PhaseOrdering( ExecuteAfter = typeof( HighLevelToMidLevelConversion ) )]
    public sealed class FromImplicitToExplictExceptions : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public FromImplicitToExplictExceptions( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            PerformParallelExecutionOfPhase( true, false );

            return this.NextPhase;
        }
    }
}
