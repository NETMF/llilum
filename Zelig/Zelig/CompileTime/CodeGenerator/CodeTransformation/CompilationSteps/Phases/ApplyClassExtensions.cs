//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    [PhaseOrdering( ExecuteAfter=typeof(TransformFinallyBlocksIntoTryBlocks) )]
    public sealed class ApplyClassExtensions : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public ApplyClassExtensions( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            var ace = new CompilationSteps.ApplyClassExtensions( this.TypeSystem );

            ace.Run();

            return this.NextPhase;
        }
    }
}
