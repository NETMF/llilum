//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    //
    // This gives you an opportunity to look at the input values of a method.
    //
    // To inject into a method like this one:
    //
    //   <res> TargetMethod( <parameter list> )
    //
    // you have to use a signature like this one:
    //
    //   void InjectedMethod( <parameter list> )
    //
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_InjectAtEntryPointAttribute" )]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class InjectAtEntryPointAttribute : Attribute
    {
    }
}
