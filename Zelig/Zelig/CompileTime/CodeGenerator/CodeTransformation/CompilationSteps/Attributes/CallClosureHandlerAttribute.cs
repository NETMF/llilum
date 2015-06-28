//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
    public sealed class CallClosureHandlerAttribute : AbstractHandlerAttribute
    {
        //
        // State
        //

        public Type Target;

        //
        // Constructor Methods
        //

        public CallClosureHandlerAttribute( Type target )
        {
            this.Target = target;
        }
    }
}
