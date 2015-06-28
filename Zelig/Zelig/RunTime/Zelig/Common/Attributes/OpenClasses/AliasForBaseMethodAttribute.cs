//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    //
    // This gives an explicit name to a method you want to call in the target class, but you cannot name.
    //
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_AliasForBaseMethodAttribute" )]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AliasForBaseMethodAttribute : Attribute
    {
        //
        // State
        //

        public string Target;

        //
        // Constructor Methods
        //

        public AliasForBaseMethodAttribute( string target )
        {
            this.Target = target;
        }
    }
}
