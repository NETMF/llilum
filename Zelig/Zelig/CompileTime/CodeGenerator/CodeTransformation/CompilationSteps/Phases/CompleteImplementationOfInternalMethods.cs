//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    //[PhaseDisabled()]
    [PhaseOrdering( ExecuteAfter=typeof(EstimateTypeSystemReduction) )]
    public sealed class CompleteImplementationOfInternalMethods : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public CompleteImplementationOfInternalMethods( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            var phaseStart = FindPhase< PrepareImplementationOfInternalMethods >();

            if(phaseStart.m_state.Complete() == false)
            {
                this.TypeSystem.BuildCrossReference();

                return FindPhase< HighLevelTransformations >();
            }

            return this.NextPhase;
        }
    }
}
