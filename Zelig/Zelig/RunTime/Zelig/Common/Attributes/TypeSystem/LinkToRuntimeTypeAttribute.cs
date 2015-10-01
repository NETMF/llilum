//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;

    [AttributeUsage(AttributeTargets.Field)]
    public class LinkToRuntimeTypeAttribute : Attribute
    {
        //
        // State
        //

        public readonly Type Target;

        //
        // Constructor Methods
        //

        public LinkToRuntimeTypeAttribute( Type target )
        {
            this.Target = target;
        }
    }
}
