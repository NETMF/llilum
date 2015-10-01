//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    //
    // This gives an explicit name to the method you want to override, instead of relying on name matching.
    //
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_AliasForTargetMethodAttribute" )]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AliasForTargetMethodAttribute : Attribute
    {
        //
        // State
        //

        public string Target;

        //
        // Constructor Methods
        //

        public AliasForTargetMethodAttribute( string target )
        {
            this.Target = target;
        }
    }
}
