//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    public sealed class ReduceNumberOfTemporaries : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public ReduceNumberOfTemporaries( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            ParallelTransformationsHandler.EnumerateFlowGraphs( this.TypeSystem, delegate( ControlFlowGraphStateForCodeTransformation cfg )
            {
                Transformations.ReduceNumberOfTemporaries.Execute( cfg );
            } );

            return this.NextPhase;
        }
    }
}
