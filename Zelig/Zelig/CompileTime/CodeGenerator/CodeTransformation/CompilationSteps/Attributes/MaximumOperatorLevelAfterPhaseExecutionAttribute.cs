//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    //
    // After running phase X, all the operators should be at most at level Y.
    //
    [AttributeUsage(AttributeTargets.Field, AllowMultiple=false)]
    public sealed class MaximumOperatorLevelAfterPhaseExecutionAttribute : Attribute
    {
        //
        // State
        //

        public readonly Operator.OperatorLevel Level;

        //
        // Constructor Methods
        //

        public MaximumOperatorLevelAfterPhaseExecutionAttribute( Operator.OperatorLevel level )
        {
            this.Level = level;
        }
    }
}
