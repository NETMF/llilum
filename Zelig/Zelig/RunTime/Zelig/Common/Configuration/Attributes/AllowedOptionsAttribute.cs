//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    [AttributeUsage( AttributeTargets.Field, AllowMultiple=false)]
    public sealed class AllowedOptionsAttribute : Attribute
    {
        //
        // State
        //

        public Type[] Targets;

        //
        // Constructor Methods
        //

        public AllowedOptionsAttribute( params Type[] targets )
        {
            this.Targets = targets;
        }
    }
}
