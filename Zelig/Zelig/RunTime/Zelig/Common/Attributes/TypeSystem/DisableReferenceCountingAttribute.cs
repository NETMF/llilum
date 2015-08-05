//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;

    [WellKnownType( "Microsoft_Zelig_Runtime_TypeSystem_DisableReferenceCountingAttribute" )]
    [AttributeUsage( AttributeTargets.Class )]
    public class DisableReferenceCountingAttribute : Attribute
    {
    }
}
