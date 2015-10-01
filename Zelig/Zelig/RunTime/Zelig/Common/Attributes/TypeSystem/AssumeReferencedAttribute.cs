//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;

    //
    // Use this attribute on a field if you need to guaranteed that
    // it will be included in an image when its declaring type is,
    // even if the field is not explicitly referenced by the application.
    //
    [WellKnownType( "Microsoft_Zelig_Runtime_TypeSystem_AssumeReferencedAttribute" )]
    [AttributeUsage( AttributeTargets.Field )]
    public class AssumeReferencedAttribute : Attribute
    {
    }
}
