//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    //
    // Methods with this attribute get a chance to look at the type system before a phase has executed.
    //
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PrePhaseHandlerAttribute : AbstractHandlerAttribute
    {
        //
        // State
        //

        //
        // Constructor Methods
        //
    }
}
