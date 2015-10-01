//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;


    [Flags]
    [TypeSystem.AllowCompileTimeIntrospection]
    public enum MemoryUsage : uint
    {
        Undefined         = 0x00000000,

        Code              = 0x00000001,
        DataRW            = 0x00000002,
        DataRO            = 0x00000004,
        Relocation        = 0x00000008,

        HotCode           = 0x00000010,
        ColdCode          = 0x00000020,
        VectorsTable      = 0x00000040,
        Bootstrap         = 0x00000080,

        Booter            = 0x00000100, // Hold bootstrap image.
        Stack             = 0x00000200,
        Heap              = 0x00000400,
    }

    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_MemoryUsageAttribute" )]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
    public sealed class MemoryUsageAttribute : Attribute
    {
        //
        // State
        //

        public readonly MemoryUsage Usage;
        public          string      SectionName;
        public          bool        ContentsUninitialized;
        public          bool        AllocateFromHighAddress;

        //
        // Constructor Methods
        //

        public MemoryUsageAttribute( MemoryUsage usage )
        {
            this.Usage = usage;
        }
    }
}
