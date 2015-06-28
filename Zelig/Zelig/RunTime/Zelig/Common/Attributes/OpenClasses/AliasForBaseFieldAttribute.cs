//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    //
    // This gives an explicit name to a field you want to use from the target class, but you cannot name.
    //
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_AliasForBaseFieldAttribute" )]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AliasForBaseFieldAttribute : Attribute
    {
        //
        // State
        //

        public string Target;

        //
        // Constructor Methods
        //

        public AliasForBaseFieldAttribute()
        {
        }

        public AliasForBaseFieldAttribute( string target )
        {
            this.Target = target;
        }
    }
}
