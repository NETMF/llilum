//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    //
    // At times, we use abstract classes to model extensibility points in the system,
    // to supply a nice abstraction for things like processor initialization, garbage collection, etc.
    //
    // However it's not like we actually have some memory for an instance of these classes.
    // The classes are used only to get access to the virtual method syntax.
    //
    // Use this attribute to tell the system that the "this" parameter doesn't really exist and it should be passed around.
    //
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_ImplicitInstanceAttribute" )]
    [AttributeUsage(AttributeTargets.Class     |
                    AttributeTargets.Struct    |
                    AttributeTargets.Interface )]
    public sealed class ImplicitInstanceAttribute : Attribute
    {
    }
}
