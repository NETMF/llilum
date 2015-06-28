//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    [PhaseOrdering( ExecuteAfter=typeof(GenerateImage) )]
    public sealed class Done : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public Done( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            return this.NextPhase;
        }
    }
}
