//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;

    [AttributeUsage(AttributeTargets.Class  | 
                    AttributeTargets.Struct |
                    AttributeTargets.Enum   )]
    public class AllowCompileTimeIntrospectionAttribute : Attribute
    {
    }
}
