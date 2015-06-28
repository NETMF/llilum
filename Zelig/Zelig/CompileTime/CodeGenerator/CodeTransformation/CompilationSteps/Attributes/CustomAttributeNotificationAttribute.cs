//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    [AttributeUsage(AttributeTargets.Method,AllowMultiple=true)]
    public sealed class CustomAttributeNotificationAttribute : Attribute
    {
        //
        // State
        //

        public string Target;

        //
        // Constructor Methods
        //

        public CustomAttributeNotificationAttribute( string target )
        {
            this.Target = target;
        }
    }
}
