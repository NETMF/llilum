//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    [PhaseOrdering( ExecuteAfter=typeof(ReduceNumberOfTemporaries) )]
    public sealed class TransformFinallyBlocksIntoTryBlocks : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public TransformFinallyBlocksIntoTryBlocks( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            ParallelTransformationsHandler.EnumerateFlowGraphs( this.TypeSystem, delegate( ControlFlowGraphStateForCodeTransformation cfg )
            {
                Transformations.TransformFinallyBlocksIntoTryBlocks.Execute( cfg );

                Transformations.MergeExtendedBasicBlocks.Execute( cfg );
            } );

            return this.NextPhase;
        }
    }
}
