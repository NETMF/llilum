//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    //
    // Methods with this attribute get a chance to look at the ControlFlowGraphState during optimization phases.
    //
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OptimizationHandlerAttribute : AbstractHandlerAttribute
    {
        //
        // State
        //

        public   bool                         RunOnce;
        public   bool                         RunInExtendedSSAForm;
        public   bool                         RunInSSAForm;
        public   bool                         RunInNormalForm;
        public   string                       RunBefore;
        public   string                       RunAfter;

        internal OptimizationHandlerAttribute caToRunBeforeThis;
        internal OptimizationHandlerAttribute caToRunAfterThis;

        //
        // Constructor Methods
        //
    }
}
