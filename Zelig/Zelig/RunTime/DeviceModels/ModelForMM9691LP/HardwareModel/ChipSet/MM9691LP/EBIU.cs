//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x30010000U,Length=0x00000060U)]
    public class EBIU
    {
        [MemoryMappedPeripheral(Base=0x0000U,Length=0x0010U)]
        public class DEVICE
        {
            public enum Size
            {
                SZ8  = 1,
                SZ16 = 2,
                SZ32 = 3,
            }

            [BitFieldPeripheral(PhysicalType=typeof(uint))]
            public struct ControlBitField
            {
                [BitFieldRegister(Position= 0, Size=6)] public uint WS;
                [BitFieldRegister(Position= 8, Size=2)] public uint RCNT;
                [BitFieldRegister(Position=12, Size=2)] public Size SZ;
                [BitFieldRegister(Position=15        )] public bool RDY;
                [BitFieldRegister(Position=16        )] public bool WD;
                [BitFieldRegister(Position=17        )] public bool BRD;
                [BitFieldRegister(Position=25        )] public bool BFEN;
            }

            //--//

            [Register(Offset=0x00000000U)] public uint            LowAddress;
            [Register(Offset=0x00000004U)] public uint            HighAddress;
            [Register(Offset=0x00000008U)] public ControlBitField Control;
        }

        [Register(Offset=0x00000000U,Size=0x00000010U)] public DEVICE Device0;
        [Register(Offset=0x00000010U,Size=0x00000010U)] public DEVICE Device1;
        [Register(Offset=0x00000020U,Size=0x00000010U)] public DEVICE Device2;
        [Register(Offset=0x00000030U,Size=0x00000010U)] public DEVICE Device3;
        [Register(Offset=0x00000040U,Size=0x00000010U)] public DEVICE Device4;
        [Register(Offset=0x00000050U,Size=0x00000010U)] public DEVICE Device5;

        //
        // Access Methods
        //

        public static extern EBIU Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}