//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    //
    // For methods marked with this attribute, the stack pointer is undefined on entry,
    // so the system should generate code to does not access the stack.
    //
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_StackNotAvailableAttribute" )]
    [AttributeUsage(AttributeTargets.Constructor |
                    AttributeTargets.Method      )]
    public sealed class StackNotAvailableAttribute : Attribute
    {
    }
}
