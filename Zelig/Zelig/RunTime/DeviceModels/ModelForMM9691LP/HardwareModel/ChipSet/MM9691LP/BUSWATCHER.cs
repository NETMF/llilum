//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x30000000U,Length=0x0000000CU)]
    public class BUSWATCHER
    {
        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct AbortBitField
        {
            [BitFieldRegister(Position=0)] public bool ABEN;
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct ControlBitField
        {
            [BitFieldRegister(Position= 0       )] public bool ERR;
            [BitFieldRegister(Position= 1       )] public bool HWRITE;
            [BitFieldRegister(Position= 2,Size=2)] public uint HTRANS;
            [BitFieldRegister(Position= 4,Size=2)] public uint HSIZE;
            [BitFieldRegister(Position= 6,Size=2)] public uint HRESP;
            [BitFieldRegister(Position= 8,Size=4)] public uint HPROT;
            [BitFieldRegister(Position=12,Size=3)] public uint HBURST;
            [BitFieldRegister(Position=15       )] public bool HMASTLOCK;
            [BitFieldRegister(Position=16,Size=4)] public uint HMASTER;
        }

        //--//

        [Register(Offset=0x00000000U)] public uint            AHB_Abort_Address;
        [Register(Offset=0x00000004U)] public ControlBitField AHB_Abort_Control;
        [Register(Offset=0x00000008U)] public AbortBitField   AHB_Abort_Enable;

        [Register(Offset=0x00000010U)] public uint            AHB_Counter_Enable;
        [Register(Offset=0x00000014U)] public uint            AHB_Counter_Restart;
        [Register(Offset=0x00000018U)] public uint            AHB_Valid_Count;
        [Register(Offset=0x0000001CU)] public uint            AHB_Idle_Count;
        [Register(Offset=0x00000020U)] public uint            AHB_Cache_Valid_Count;
        [Register(Offset=0x00000024U)] public uint            AHB_NonCache_Count;
        [Register(Offset=0x00000028U)] public uint            AHB_Cache_Miss;
        [Register(Offset=0x0000002CU)] public uint            AHB_CPU_Idle_Count;

        //
        // Access Methods
        //

        public static extern BUSWATCHER Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}