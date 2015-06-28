//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    //
    // Using this attribute tells the system that the stack pointer is good
    // after a call to the method. This is mainly useful during bootstrap.
    //
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_StackAvailableOnReturnAttribute" )]
    [AttributeUsage(AttributeTargets.Constructor |
                    AttributeTargets.Method      )]
    public sealed class StackAvailableOnReturnAttribute : Attribute
    {
    }
}
