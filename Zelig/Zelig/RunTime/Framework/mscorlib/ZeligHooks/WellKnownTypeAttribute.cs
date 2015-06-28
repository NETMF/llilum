//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Internals
{
    using System;

    [AttributeUsage( AttributeTargets.Class     |
                     AttributeTargets.Struct    |
                     AttributeTargets.Enum      |
                     AttributeTargets.Interface )]
    internal class WellKnownTypeAttribute : Attribute
    {
        //
        // State
        //

        internal readonly string TypeName;

        //
        // Constructor Methods
        //

        internal WellKnownTypeAttribute( string typeName )
        {
            this.TypeName = typeName;
        }
    }
}
