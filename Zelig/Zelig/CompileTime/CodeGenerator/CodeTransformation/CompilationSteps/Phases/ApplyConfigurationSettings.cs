//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    [PhaseOrdering( ExecuteAfter=typeof(CrossReferenceTypeSystem) )]
    public sealed class ApplyConfigurationSettings : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public ApplyConfigurationSettings( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            PerformParallelExecutionOfPhase( false, false );

            this.CallsDataBase.ExecuteInlining( this.TypeSystem );

            ApplyGarbageCollectorConfiguration();

            return this.NextPhase;
        }

        private void ApplyGarbageCollectorConfiguration()
        {
            var wkt = TypeSystem.WellKnownTypes;

            var gcImpls = TypeSystem.CollectConcreteImplementations(wkt.Microsoft_Zelig_Runtime_GarbageCollectionManager);

            CHECKS.ASSERT(gcImpls.Count == 1, "There can only be 1 active garbage collection manager");

            var gcImpl = gcImpls[0];

            Console.WriteLine("Garbage collector selected: {0}", gcImpl.FullName);

            // Since ReferenceCountingCollector is done by code injection during compile time
            // (and the ReferenceCountingCollector class itself is only a dummy class with 
            // empty implementations), we need to detect its selection here and enable the 
            // code path for code injection here for the later phases.
            if (gcImpl.IsSubClassOf(wkt.Microsoft_Zelig_Runtime_ReferenceCountingCollector, null))
            {
                TypeSystem.ReferenceCountingGarbageCollectionStatus = TypeSystemForCodeTransformation.ReferenceCountingStatus.Enabled;
            }
            else if (gcImpl.IsSubClassOf(wkt.Microsoft_Zelig_Runtime_StrictReferenceCountingCollector, null))
            {
                TypeSystem.ReferenceCountingGarbageCollectionStatus = TypeSystemForCodeTransformation.ReferenceCountingStatus.EnabledStrict;
            }
        }
    }
}
