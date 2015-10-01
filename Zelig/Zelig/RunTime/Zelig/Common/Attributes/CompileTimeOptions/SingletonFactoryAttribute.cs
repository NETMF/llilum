//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_SingletonFactoryAttribute" )]
    [AttributeUsage(AttributeTargets.Constructor |
                    AttributeTargets.Method      )]
    public sealed class SingletonFactoryAttribute : Attribute
    {
        public bool ReadOnly;

        public Type Fallback;
    }
}
