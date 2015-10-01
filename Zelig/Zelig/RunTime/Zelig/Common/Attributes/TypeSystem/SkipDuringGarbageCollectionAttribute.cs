//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;

    //
    // Use this attribute on a field if you need to treat it specially during garbage collection.
    //
    [WellKnownType( "Microsoft_Zelig_Runtime_TypeSystem_SkipDuringGarbageCollectionAttribute" )]
    [AttributeUsage( AttributeTargets.Field )]
    public class SkipDuringGarbageCollectionAttribute : Attribute
    {
    }
}
