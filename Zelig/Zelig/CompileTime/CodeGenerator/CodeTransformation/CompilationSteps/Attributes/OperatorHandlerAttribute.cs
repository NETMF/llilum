//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
    public sealed class OperatorHandlerAttribute : AbstractHandlerAttribute
    {
        //
        // State
        //

        public Type Target;

        //
        // Constructor Methods
        //

        public OperatorHandlerAttribute( Type target )
        {
            this.Target = target;
        }
    }
}
