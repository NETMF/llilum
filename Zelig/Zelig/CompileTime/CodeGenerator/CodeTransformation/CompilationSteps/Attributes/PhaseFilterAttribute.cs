//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
    public sealed class PhaseFilterAttribute : Attribute
    {
        //
        // State
        //

        public readonly Type Target;

        //
        // Constructor Methods
        //

        public PhaseFilterAttribute( Type target )
        {
            this.Target = target;
        }
    }
}
