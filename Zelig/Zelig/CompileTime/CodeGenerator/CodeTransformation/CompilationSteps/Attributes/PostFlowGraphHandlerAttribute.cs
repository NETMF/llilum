//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    //
    // Methods with this attribute get a chance to look at the ControlFlowGraphState after everyone else.
    //
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PostFlowGraphHandlerAttribute : AbstractHandlerAttribute
    {
        //
        // State
        //

        //
        // Constructor Methods
        //
    }
}
