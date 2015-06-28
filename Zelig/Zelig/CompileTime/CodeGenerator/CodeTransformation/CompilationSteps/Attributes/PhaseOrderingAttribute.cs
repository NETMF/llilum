//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    //
    // Allows to position phase X in relation to other phases.
    //
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class PhaseOrderingAttribute : Attribute
    {
        //
        // State
        //

        public Type ExecuteBefore;
        public Type ExecuteAfter;

        public bool IsPipelineBlock;
    }
}
