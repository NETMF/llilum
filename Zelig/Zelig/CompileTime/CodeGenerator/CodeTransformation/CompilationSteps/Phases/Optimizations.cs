//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    //[PhaseDisabled()]
    [PhaseOrdering( ExecuteAfter=typeof(FuseOperators) )]
    public sealed class Optimizations : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public Optimizations( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            var maxLevel = Operator.OperatorLevel.Lowest;

            ParallelTransformationsHandler.EnumerateMethods( this.TypeSystem, delegate( ParallelTransformationsHandler.Operation phase, MethodRepresentation md, ref object state )
            {
                var rpe = (OptimizationsPhaseExecution)state;

                switch(phase)
                {
                    case ParallelTransformationsHandler.Operation.Initialize:
                        state = new OptimizationsPhaseExecution( this.TypeSystem, this.DelegationCache, this.CallsDataBase, this );
                        break;

                    case ParallelTransformationsHandler.Operation.Execute:
                        if(md.IsOpenMethod == false && md.OwnerType.IsOpenType == false)
                        {
                            rpe.Analyze( md );
                        }
                        break;

                    case ParallelTransformationsHandler.Operation.Shutdown:
                        if(maxLevel < rpe.MaxAbstractionLevelEncountered)
                        {
                            maxLevel = rpe.MaxAbstractionLevelEncountered;
                        }
                        break;
                }
            } );

            ValidateAbstractionLevel( maxLevel );

            return this.NextPhase;
        }
    }
}
