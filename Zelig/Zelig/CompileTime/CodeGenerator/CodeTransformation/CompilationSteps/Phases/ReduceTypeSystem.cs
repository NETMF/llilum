//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    [PhaseOrdering( ExecuteAfter=typeof(CompleteImplementationOfInternalMethods), IsPipelineBlock=true )]
    [PhaseLimit( Operator.OperatorLevel.ObjectOriented )]
    public sealed class ReduceTypeSystem : PhaseDriver
    {

        private TypeSystem m_typeSystem;

        //
        // Constructor Methods
        //

        public ReduceTypeSystem( Controller context ) : base ( context )
        {
            m_typeSystem = context.TypeSystem;
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            //
            // Flush the call graph, because it could contain reduced methods.
            //
            this.CallsDataBase.ClearCallSites();

            this.TypeSystem.PerformReduction();

            this.TypeSystem.BuildCrossReference();
            
            return this.NextPhase;
        }
    }
}
