//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    //
    // Using this attribute tells the system that the heap is ready for use
    // after a call to the method. This is mainly useful during bootstrap.
    //
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_CanAllocateOnReturnAttribute" )]
    [AttributeUsage(AttributeTargets.Constructor |
                    AttributeTargets.Method      )]
    public sealed class CanAllocateOnReturnAttribute : Attribute
    {
    }
}
