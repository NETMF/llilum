//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    //[PhaseDisabled()]
    [PhaseOrdering( ExecuteAfter = typeof(ReduceTypeSystem), ExecuteBefore = typeof( DetectNonImplementedInternalCalls ) )]
    public sealed class PrepareExternalMethods : PhaseDriver
    {
        //
        // State
        //

        internal ImplementExternalMethods m_state;

        //
        // Constructor Methods
        //

        public PrepareExternalMethods( Controller context )
            : base( context )
        {
            ExternalCallOperator.NativeImportDirectories = context.TypeSystem.NativeImportDirectories;
            ExternalCallOperator.NativeImportLibraries   = context.TypeSystem.NativeImportLibraries;

            m_state = new ImplementExternalMethods( this.TypeSystem );
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            //this.TypeSystem.BuildCrossReference();

            m_state.Prepare();

            //DetectFieldInvariants.Execute( this.TypeSystem );

            return this.NextPhase;
        }
    }
}
