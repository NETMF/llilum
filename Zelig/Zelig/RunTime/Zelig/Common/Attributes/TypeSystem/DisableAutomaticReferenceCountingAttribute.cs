//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;

    /// <summary>
    /// Attribute for methods that should be excluded from automatically injecting reference
    /// counting code (i.e. initialize reference count when allocating an object, add and release
    /// reference when needed).
    /// The attribute can also be apply to a class or a struct, in which case, all methods in the
    /// class / sturct will be excluded from the automatic reference counting code injection.
    /// </summary>
    [WellKnownType( "Microsoft_Zelig_Runtime_TypeSystem_DisableAutomaticReferenceCountingAttribute" )]
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property )]
    public class DisableAutomaticReferenceCountingAttribute : Attribute
    {
    }
}
