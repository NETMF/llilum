//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WellKnownTypeHandlerAttribute : AbstractHandlerAttribute
    {
        //
        // State
        //

        public string Target;

        //
        // Constructor Methods
        //

        public WellKnownTypeHandlerAttribute( string target )
        {
            this.Target = target;
        }
    }
}
