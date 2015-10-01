//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.CortexM4
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    using Microsoft.Zelig.Runtime.TargetPlatform.ARMv7;

    using RT = Microsoft.Zelig.Runtime;

    //--//

    // TODO: put right addresses, and fix code generation for LLVM that does not understand the attribute's constants
    //[MemoryMappedPeripheral(Base = 0x40D00000U, Length = 0x000000D0U)]
    public sealed class NVIC : Microsoft.DeviceModels.Chipset.CortexM3.NVIC
    {
    }
}