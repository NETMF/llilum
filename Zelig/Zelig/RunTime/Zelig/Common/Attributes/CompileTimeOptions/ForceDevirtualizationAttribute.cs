//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    //
    // At times, we use abstract classes to model extensibility points in the system,
    // to supply a nice abstraction for things like processor initialization, memory subsystems, etc.
    //
    // However we don't really expect to have multiple subclasses in active use at the same time.
    // It wouldn't make sense to have two processor models, for example.
    //
    // Use this attribute to tell the system that a single subclass should be present.
    //
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_ForceDevirtualizationAttribute" )]
    [AttributeUsage(AttributeTargets.Class     |
                    AttributeTargets.Struct    |
                    AttributeTargets.Interface )]
    public sealed class ForceDevirtualizationAttribute : Attribute
    {
    }
}
