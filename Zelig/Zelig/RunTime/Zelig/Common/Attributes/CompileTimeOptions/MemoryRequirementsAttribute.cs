//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;


    [Flags]
    [TypeSystem.AllowCompileTimeIntrospection]
    public enum MemoryAttributes : uint
    {
        RAM                    = 0x00000001,
        FLASH                  = 0x00000002,
        ROM                    = 0x00000004,
        Peripheral             = 0x00000008,
        LocationMask           = 0x0000000F,

        Static                 = 0x00000010,
        Dynamic                = 0x00000020,
        Stack                  = 0x00000040,
        InternalMemory         = 0x00000080,
        ExternalMemory         = 0x00000100,
        RandomAccessMemory     = 0x00000200,
        BlockBasedMemory       = 0x00000400,
        ConfiguredAtEntryPoint = 0x00000800,
        LoadedAtEntrypoint     = 0x00001000,
                  
        Allocated              = 0x80000000u,
    }

    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_MemoryRequirementsAttribute" )]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
    public sealed class MemoryRequirementsAttribute : Attribute
    {
        //
        // State
        //

        public readonly MemoryAttributes Requirements;

        //
        // Constructor Methods
        //

        public MemoryRequirementsAttribute( MemoryAttributes requirements )
        {
            this.Requirements = requirements;
        }
    }
}
