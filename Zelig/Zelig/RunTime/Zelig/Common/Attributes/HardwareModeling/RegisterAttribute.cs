//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;


    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_RegisterAttribute" )]
    [AttributeUsage(AttributeTargets.Field,AllowMultiple=false)]
    public sealed class RegisterAttribute : Attribute
    {
        //
        // State
        //

        public uint Offset;
        public uint Size;
        public int  Instances;
    }
}
