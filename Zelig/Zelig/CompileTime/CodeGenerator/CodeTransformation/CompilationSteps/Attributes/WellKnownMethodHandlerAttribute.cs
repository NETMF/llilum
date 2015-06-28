//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
    public sealed class WellKnownMethodHandlerAttribute : AbstractHandlerAttribute
    {
        //
        // State
        //

        public string Target;

        //
        // Constructor Methods
        //

        public WellKnownMethodHandlerAttribute( string target )
        {
            this.Target = target;
        }
    }
}
