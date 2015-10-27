//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;
    
    [PhaseOrdering( ExecuteAfter=typeof( FromImplicitToExplictExceptions ) )]
    [PhaseLimit( Operator.OperatorLevel.ConcreteTypes_NoExceptions )]
    public sealed class MidLevelToLowLevelConversion : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public MidLevelToLowLevelConversion( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            PerformParallelExecutionOfPhase( true, true );

            return this.NextPhase;
        }
    }
}
