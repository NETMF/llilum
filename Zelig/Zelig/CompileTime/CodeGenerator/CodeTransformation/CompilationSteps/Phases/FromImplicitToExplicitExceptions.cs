//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

// This macro enables transformation from exception blocks to landing pad form.
#define ENABLE_LANDING_PADS

namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    [PhaseOrdering( ExecuteAfter = typeof( HighLevelToMidLevelConversion ) )]
    public sealed class FromImplicitToExplicitExceptions : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public FromImplicitToExplicitExceptions( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
#if ENABLE_LANDING_PADS
            ParallelTransformationsHandler.EnumerateFlowGraphs(
                this.TypeSystem,
                delegate (ControlFlowGraphStateForCodeTransformation cfg)
                {
                    Transformations.ConvertToLandingPads.Execute(cfg);
                });
#endif // ENABLE_LANDING_PADS

            PerformParallelExecutionOfPhase( true, false );

            return this.NextPhase;
        }
    }
}
