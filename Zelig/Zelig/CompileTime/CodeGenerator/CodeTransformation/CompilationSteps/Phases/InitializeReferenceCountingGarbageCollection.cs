//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    public sealed class InitializeReferenceCountingGarbageCollection : PhaseDriver
    {
        //
        // Constructor Methods
        //
        public InitializeReferenceCountingGarbageCollection( Controller context ) : base( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run( )
        {
            TypeSystem.EnableReferenceCountingGarbageCollection = true;

            return this.NextPhase;
        }
    }
}
