//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;


    //
    // After running phase X, all the operators should be at most at level Y.
    //
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class PhaseLimitAttribute : Attribute
    {
        //
        // State
        //

        public readonly Operator.OperatorLevel Level;

        //
        // Constructor Methods
        //

        public PhaseLimitAttribute( Operator.OperatorLevel level )
        {
            this.Level = level;
        }
    }
}
