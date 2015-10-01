//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    [AttributeUsage( AttributeTargets.Class, AllowMultiple=true )]
    public sealed class DependsOnAttribute : Attribute
    {
        //
        // State
        //

        public Type Target;

        //
        // Constructor Methods
        //

        public DependsOnAttribute( Type target )
        {
            this.Target = target;
        }
    }
}
