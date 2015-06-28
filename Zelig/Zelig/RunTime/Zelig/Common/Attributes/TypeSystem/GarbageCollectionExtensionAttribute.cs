//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;

    [WellKnownType( "Microsoft_Zelig_Runtime_TypeSystem_GarbageCollectionExtensionAttribute" )]
    [AttributeUsage(AttributeTargets.Class)]
    public class GarbageCollectionExtensionAttribute : Attribute
    {
        //
        // State
        //

        public readonly Type Target;

        //
        // Constructor Methods
        //

        public GarbageCollectionExtensionAttribute( Type target )
        {
            this.Target = target;
        }
    }
}
