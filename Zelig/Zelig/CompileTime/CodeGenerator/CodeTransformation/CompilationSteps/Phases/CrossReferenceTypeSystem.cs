//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    [PhaseOrdering( ExecuteAfter=typeof(PrepareImplementationOfInternalMethods) )]
    public sealed class CrossReferenceTypeSystem : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public CrossReferenceTypeSystem( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            this.TypeSystem.BuildCrossReference();

            return this.NextPhase;
        }
    }
}
