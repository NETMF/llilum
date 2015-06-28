//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class SingleMethodPhaseExecution : PhaseExecution
    {
        //
        // State
        //

        MethodRepresentation m_currentMethod;

        //
        // Constructor Methods
        //

        public SingleMethodPhaseExecution( TypeSystemForCodeTransformation typeSystem    ,
                                           DelegationCache                 cache         ,
                                           CallsDataBase                   callsDatabase ,
                                           PhaseDriver                     phase         ) : base( typeSystem, cache, callsDatabase, phase )
        {
        }

        //
        // Helper Methods
        //

        public virtual void Analyze( MethodRepresentation md )
        {
            m_currentMethod = md;

            AnalyzeMethod( md );
        }
    }
}
