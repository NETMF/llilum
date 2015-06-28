//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OperatorArgumentHandlerAttribute : AbstractHandlerAttribute
    {
        //
        // State
        //

        public Type Target;

        //
        // Constructor Methods
        //

        public OperatorArgumentHandlerAttribute( Type target )
        {
            this.Target = target;
        }
    }
}
