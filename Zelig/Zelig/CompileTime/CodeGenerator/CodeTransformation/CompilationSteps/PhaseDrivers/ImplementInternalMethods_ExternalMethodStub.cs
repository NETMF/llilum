//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;
    using Microsoft.Zelig.Runtime;
    using Microsoft.Zelig.CodeGeneration.IR.Abstractions;


    public sealed partial class ImplementInternalMethods
    {
        //
        // Helper Methods
        //

        public static void InitializeExternalReferences()
        {
            ExternalCallContext.Reset();
        }

        private void ImplementExternalMethodStub( MethodRepresentation md, TypeSystemForCodeTransformation typeSys )
        {
            if (TypeSystemForCodeTransformation.GetCodeForMethod(md) == null)
            {
                ControlFlowGraphStateForCodeTransformation cfg             = (ControlFlowGraphStateForCodeTransformation)m_typeSystem.CreateControlFlowGraphState( md );

                var                                        bb              = cfg.CreateFirstNormalBasicBlock();

                ///
                /// Leave an empty implemenation that will be filled only after the ComputeCallClosure phase is over.  That way
                /// we do not add imported code that will not be used.
                ///

                //
                // Create proper flow control for exit basic block.
                //
                cfg.AddReturnOperator();
            }
        }
    }
}