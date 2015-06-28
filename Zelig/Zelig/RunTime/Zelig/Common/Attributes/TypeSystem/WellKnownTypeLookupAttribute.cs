//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;

    [AttributeUsage(AttributeTargets.Field)]
    public class WellKnownTypeLookupAttribute : Attribute
    {
        //
        // State
        //

        public readonly string AssemblyName;
        public readonly string Namespace;
        public readonly string Name;

        //
        // Constructor Methods
        //

        public WellKnownTypeLookupAttribute( string assemblyName ,
                                             string nameSpace    ,
                                             string name         )
        {
            this.AssemblyName = assemblyName;
            this.Namespace    = nameSpace;
            this.Name         = name;
        }
    }
}
