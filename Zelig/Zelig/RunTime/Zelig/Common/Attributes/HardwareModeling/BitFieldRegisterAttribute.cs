//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;


    [Flags]
    [TypeSystem.AllowCompileTimeIntrospection]
    public enum BitFieldModifier : uint
    {
        ReadOnly     = 0x00000001,
        WriteOnly    = 0x00000002,
        UseReadMask  = 0x00000004,
        UseWriteMask = 0x00000008,
    }

    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_BitFieldRegisterAttribute" )]
    [AttributeUsage(AttributeTargets.Field,AllowMultiple=false)]
    public sealed class BitFieldRegisterAttribute : Attribute
    {
        //
        // State
        //

        public uint             Position;
        public uint             Size;
        public BitFieldModifier Modifiers;
        public uint             ReadsAs;
        public uint             WritesAs;
    }
}
