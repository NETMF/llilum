//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR.Abstractions
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.MetaData.Normalized;

    using Microsoft.Zelig.Runtime.TypeSystem;


    [Flags]
    public enum RegisterClass : uint
    {
        None                   = 0x00000000,

        Address                = 0x00000001,
        System                 = 0x00000002,
        Integer                = 0x00000004,
        SinglePrecision        = 0x00000008,
        DoublePrecision        = 0x00000010,
        DoublePrecision_Low    = 0x00000020,
        DoublePrecision_High   = 0x00000040,

        StatusRegister         = 0x04000000,
        LinkAddress            = 0x08000000,
        StackPointer           = 0x10000000,
        ProgramCounter         = 0x20000000,
        AvailableForAllocation = 0x40000000,
        Special                = 0x80000000,
    }
}
