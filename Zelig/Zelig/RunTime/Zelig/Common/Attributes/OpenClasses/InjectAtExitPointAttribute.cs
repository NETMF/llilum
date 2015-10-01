//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    //
    // This gives you an opportunity to look at the result value of a method.
    //
    // To inject into a method like this one:
    //
    //   <res> TargetMethod( <parameter list> )
    //
    // you have to use a signature like this one:
    //
    //   <res> InjectedMethod( <parameter list>, <res> )
    //
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_InjectAtExitPointAttribute" )]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class InjectAtExitPointAttribute : Attribute
    {
    }
}
