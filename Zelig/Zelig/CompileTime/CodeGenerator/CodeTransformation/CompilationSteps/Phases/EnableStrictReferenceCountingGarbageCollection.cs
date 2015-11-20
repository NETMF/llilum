//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    [PhaseOrdering( ExecuteAfter = typeof( InitializeReferenceCountingGarbageCollection ) )]
    public sealed class EnableStrictReferenceCountingGarbageCollection : PhaseDriver
    {
        //
        // Constructor Methods
        //
        public EnableStrictReferenceCountingGarbageCollection( Controller context ) : base( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run( )
        {
            if(TypeSystem.IsReferenceCountingGarbageCollectionEnabled)
            {
                TypeSystem.ReferenceCountingGarbageCollectionStatus |=
                    TypeSystemForCodeTransformation.ReferenceCountingStatus.Strict;
            }

            return this.NextPhase;
        }   
    }
}
