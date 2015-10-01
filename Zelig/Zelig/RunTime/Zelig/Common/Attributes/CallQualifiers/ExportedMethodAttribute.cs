//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_ExportedMethodAttribute" )]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ExportedMethodAttribute : Attribute
    {
    }
}