//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;

    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_TypeSystem_NoVTableAttribute" )]
    [AttributeUsage(AttributeTargets.Class)]
    public class NoVTableAttribute : Attribute
    {
    }
}
