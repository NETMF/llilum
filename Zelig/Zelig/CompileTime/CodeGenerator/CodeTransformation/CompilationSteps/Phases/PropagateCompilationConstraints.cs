//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    [PhaseOrdering( ExecuteAfter=typeof(HighLevelTransformations) )]
    public sealed class PropagateCompilationConstraints : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public PropagateCompilationConstraints( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            var callsToInline = new Queue< CallOperator >();

            ParallelTransformationsHandler.EnumerateFlowGraphs( this.TypeSystem, delegate( ControlFlowGraphStateForCodeTransformation cfg )
            {
                if(cfg.Method.IsOpenMethod == false)
                {
                    EnforceCompilationConstraints( callsToInline, cfg );
                }
            } );

            while(callsToInline.Count > 0)
            {
                CallOperator op = callsToInline.Dequeue();

                var cfg = (ControlFlowGraphStateForCodeTransformation)op.BasicBlock.Owner;

                cfg.TraceToFile( "InlineCall" );

                var fSuccess = Transformations.InlineCall.Execute( op, null );

                cfg.TraceToFile( "InlineCall-Post" );

                if(fSuccess)
                {
                    EnforceCompilationConstraints( callsToInline, cfg );
                }
                else
                {
                    //
                    // Inling can fail if the target method doesn't have an implementation.
                    // For now, just proceed, it will be caught at a later stage.
                    //
                    //CHECKS.ASSERT( false, "Unexpected failure to force inlining" );
                }
            }

            return this.NextPhase;
        }

        private static void EnforceCompilationConstraints( Queue< CallOperator >                      callsToInline ,
                                                           ControlFlowGraphStateForCodeTransformation cfg           )
        {
            var toBeInlined = new List< CallOperator >();

            cfg.EnforceCompilationConstraints( toBeInlined );

            if(toBeInlined.Count > 0)
            {
                lock(callsToInline)
                {
                    foreach(CallOperator op in toBeInlined)
                    {
                        callsToInline.Enqueue( op );
                    }
                }
            }
        }
    }
}
