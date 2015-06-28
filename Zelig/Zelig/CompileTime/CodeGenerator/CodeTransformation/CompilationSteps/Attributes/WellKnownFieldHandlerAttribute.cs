//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WellKnownFieldHandlerAttribute : AbstractHandlerAttribute
    {
        //
        // State
        //

        public string Target;

        //
        // Constructor Methods
        //

        public WellKnownFieldHandlerAttribute( string target )
        {
            this.Target = target;
        }
    }
}
