//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Length=0x00000020U)]
    public abstract class ARMTIMERx
    {
        public enum Prescale
        {
            Div1   = 0,
            Div16  = 1,
            Div256 = 2,
        }
        
        [BitFieldPeripheral(PhysicalType=typeof(byte))]
        public struct ControlBitField
        {
            [BitFieldRegister(Position=2, Size=2)] public Prescale Prescale;
            [BitFieldRegister(Position=6        )] public bool     Periodic;
            [BitFieldRegister(Position=7        )] public bool     Enable;
        }

        [Register(Offset=0x00000000U)] public ushort          Load;
        [Register(Offset=0x00000004U)] public ushort          Value;
        [Register(Offset=0x00000008U)] public ControlBitField Control;
        [Register(Offset=0x0000000CU)] public ushort          Clear;
    }
}