//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime
{
    using System;

    
    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_CapabilitiesFilterAttribute" )]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
    public sealed class CapabilitiesFilterAttribute : Attribute
    {
        //
        // State
        //

        public String RequiredCapabilities;

        //
        // Constructor Methods
        //

        public CapabilitiesFilterAttribute( )
        {
        }
    }
}
