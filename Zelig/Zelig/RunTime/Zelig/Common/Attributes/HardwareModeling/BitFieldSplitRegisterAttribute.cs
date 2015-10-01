//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;


    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_BitFieldSplitRegisterAttribute" )]
    [AttributeUsage(AttributeTargets.Field,AllowMultiple=true)]
    public sealed class BitFieldSplitRegisterAttribute : Attribute
    {
        //
        // State
        //

        public uint             Position;
        public uint             Size;
        public uint             Offset;
        public BitFieldModifier Modifiers;
        public uint             ReadsAs;
        public uint             WritesAs;
    }
}
