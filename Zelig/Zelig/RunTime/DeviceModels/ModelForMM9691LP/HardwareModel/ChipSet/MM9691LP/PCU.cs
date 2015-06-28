//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x380E0000U,Length=0x00000020U)]
    public class PCU
    {
        public const uint PCU_STATUS__LN2   = 0x00000040;
        public const uint PCU_STATUS__LN1   = 0x00000020;
        public const uint PCU_STATUS__SW1   = 0x00000010;
        public const uint PCU_STATUS__OTHER = 0x00000008;
        public const uint PCU_STATUS__RESET = 0x00000004;
        public const uint PCU_STATUS__DEAD  = 0x00000002;
        public const uint PCU_STATUS__OFF   = 0x00000001;

        [Register(Offset=0x00000000U)] public uint CLR_SW1;
        [Register(Offset=0x00000004U)] public uint SET_SW1;

        [Register(Offset=0x00000008U)] public uint CLR_LN1;
        [Register(Offset=0x0000000CU)] public uint SET_LN1;

        [Register(Offset=0x00000010U)] public uint CLR_LN2;
        [Register(Offset=0x00000014U)] public uint SET_LN2;

        [Register(Offset=0x00000018U)] public uint SW_RESET;

        [Register(Offset=0x0000001CU)] public uint PCU_STATUS;

        [Register(Offset=0x00000020U)] public byte CLR_SW1_CNT;
        [Register(Offset=0x00000024U)] public byte SET_SW1_CNT;
        [Register(Offset=0x00000028U)] public byte CLR_LN1_CNT;
        [Register(Offset=0x0000002CU)] public byte SET_LN1_CNT;
        [Register(Offset=0x00000030U)] public byte CLR_LN2_CNT;
        [Register(Offset=0x00000034U)] public byte SET_LN2_CNT;

        //
        // Access Methods
        //

        public static extern PCU Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}