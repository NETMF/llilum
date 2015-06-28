//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    [AttributeUsage( AttributeTargets.Class |
                     AttributeTargets.Field )]
    public sealed class DisplayNameAttribute : Attribute
    {
        //
        // State
        //

        public string Value;

        //
        // Constructor Methods
        //

        public DisplayNameAttribute( string value )
        {
            this.Value = value;
        }
    }
}
