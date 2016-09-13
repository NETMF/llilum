//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    //[PhaseDisabled()]
    [PhaseOrdering( ExecuteAfter=typeof(CollectRegisterAllocationConstraints), IsPipelineBlock=true )]
    [PhaseLimit( Operator.OperatorLevel.StackLocations )]
    public sealed class AllocateRegisters : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public AllocateRegisters( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
////        GrowOnlySet< Type > set = SetFactory.New< Type >();

            ParallelTransformationsHandler.EnumerateFlowGraphs( this.TypeSystem, delegate( ControlFlowGraphStateForCodeTransformation cfg )
            {
                Transformations.GlobalRegisterAllocation.Execute( cfg );

                cfg.DropDeadVariables();

////            lock(set)
////            {
////                foreach(IR.Operator op in cfg.SpanningTreeOperators)
////                {
////                    set.Insert( op.GetType() );
////                }
////            }
            } );

////        foreach(Type t in set)
////        {
////            Console.WriteLine( "{0}", t.FullName );
////        }

            return this.NextPhase;
        }
    }
}
