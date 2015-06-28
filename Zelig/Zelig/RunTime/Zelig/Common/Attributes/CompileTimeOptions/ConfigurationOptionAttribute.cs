//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    //
    // Use this attribute on a field if you need to guaranteed that
    // it will be included in an image when its declaring type is,
    // even if the field is not explicitly referenced by the application.
    //
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_ConfigurationOptionAttribute" )]
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Method )]
    public class ConfigurationOptionAttribute : Attribute
    {
        //
        // State
        //

        public readonly string Name;

        //
        // Constructor Methods
        //

        public ConfigurationOptionAttribute( string name )
        {
            this.Name = name;
        }
    }
}
