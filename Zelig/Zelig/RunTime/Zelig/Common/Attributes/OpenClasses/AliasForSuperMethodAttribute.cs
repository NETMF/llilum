//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    //
    // This gives an explicit name to a method you want to call in the target's super class, but you cannot name.
    //
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_AliasForSuperMethodAttribute" )]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AliasForSuperMethodAttribute : Attribute
    {
        //
        // State
        //

        public string Target;

        //
        // Constructor Methods
        //

        public AliasForSuperMethodAttribute( string target )
        {
            this.Target = target;
        }
    }
}
