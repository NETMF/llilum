//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    //[PhaseDisabled()]
    [PhaseOrdering( ExecuteAfter=typeof(ApplyClassExtensions) )]
    public sealed class PrepareImplementationOfInternalMethods : PhaseDriver
    {
        //
        // State
        //

        internal ImplementInternalMethods m_state;

        //
        // Constructor Methods
        //

        public PrepareImplementationOfInternalMethods( Controller context ) : base ( context )
        {
            m_state = new ImplementInternalMethods( this.TypeSystem );
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            this.TypeSystem.BuildCrossReference();

            m_state.Prepare();

            DetectFieldInvariants.Execute( this.TypeSystem );

            return this.NextPhase;
        }
    }
}
