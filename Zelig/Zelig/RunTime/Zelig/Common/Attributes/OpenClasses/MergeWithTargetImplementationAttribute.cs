//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    //
    // The default behavior is to discard the overridden method, but this doesn't work for constructors.
    // So, instead of picking a default, the code has to explain what to do.
    //
    //
    // With this attribute, the system will try to automatically merge the code for the constructors, based on different cases:
    // 
    // 1) There's no matching constructor
    //    a) Default constructor is missing => Fail, it's not clear what should happen.
    //    b) Default constructor is present => Merge the initialization of the overridden object with the current one.
    // 
    // 2) There's a matching constructor => Merge the initialization of the overridden object with the current one.
    //    
    //
    // Use paired with 'AliasForMethod' to guide how to merge,
    // so you can name otherwise unnameable entities, like constructors.
    //
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_MergeWithTargetImplementationAttribute" )]
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class MergeWithTargetImplementationAttribute : Attribute
    {
    }
}
