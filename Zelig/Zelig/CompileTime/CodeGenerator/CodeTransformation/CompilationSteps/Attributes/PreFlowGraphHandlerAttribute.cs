//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    //
    // Methods with this attribute get a chance to look at the ControlFlowGraphState before everyone else.
    //
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PreFlowGraphHandlerAttribute : AbstractHandlerAttribute
    {
        //
        // State
        //

        //
        // Constructor Methods
        //
    }
}
