//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CustomAttributeHandlerAttribute : AbstractHandlerAttribute
    {
        //
        // State
        //

        public string FieldName;

        //
        // Constructor Methods
        //

        public CustomAttributeHandlerAttribute( string fieldName )
        {
            this.FieldName = fieldName;
        }
    }
}
