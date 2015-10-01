//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;

    [AttributeUsage( AttributeTargets.Class     |
                     AttributeTargets.Struct    |
                     AttributeTargets.Enum      |
                     AttributeTargets.Interface )]
    public class WellKnownTypeAttribute : Attribute
    {
        //
        // State
        //

        public readonly string TypeName;

        //
        // Constructor Methods
        //

        public WellKnownTypeAttribute( string typeName )
        {
            this.TypeName = typeName;
        }
    }
}
