//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    //[PhaseDisabled()]
    [PhaseOrdering( ExecuteAfter=typeof(MidLevelToLowLevelConversion) )]
    public sealed class ConvertUnsupportedOperatorsToMethodCalls : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public ConvertUnsupportedOperatorsToMethodCalls( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            PerformParallelExecutionOfPhase( true, true );

            return this.NextPhase;
        }
    }
}
