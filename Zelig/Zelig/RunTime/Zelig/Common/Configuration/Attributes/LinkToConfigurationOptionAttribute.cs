//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    [AttributeUsage( AttributeTargets.Field, AllowMultiple=false)]
    public sealed class LinkToConfigurationOptionAttribute : Attribute
    {
        //
        // State
        //

        public string Name;

        //
        // Constructor Methods
        //

        public LinkToConfigurationOptionAttribute( string name )
        {
            this.Name = name;
        }
    }
}
