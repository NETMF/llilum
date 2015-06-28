//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    [AttributeUsage( AttributeTargets.Field, AllowMultiple=false )]
    public sealed class MemorySectionAttribute : Attribute
    {

        //
        // State
        //

        public Runtime.MemoryUsage Usage;
        public string              Name;
        public Type                ExtensionHandler;

        //
        // Constructor Methods
        //

        public MemorySectionAttribute( Runtime.MemoryUsage usage )
        {
            this.Usage = usage;
        }
    }
}
