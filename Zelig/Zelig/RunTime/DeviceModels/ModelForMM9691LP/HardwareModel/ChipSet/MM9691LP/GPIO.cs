//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x38070000U,Length=0x00000200U)]
    public class GPIO
    {
        [MemoryMappedPeripheral(Base=0x0000U,Length=0x0004U)]
        public class CW
        {
            public const ushort PIN              = 0x0001;
            public const ushort DOUT_IEN         = 0x0002;
            public const ushort RES_DIS          = 0x0000;
            public const ushort RES_EN           = 0x0004;
            public const ushort RES_DIR_PULLDOWN = 0x0000;
            public const ushort RES_DIR_PULLUP   = 0x0008;
            public const ushort RES_mask         = 0x000C;

            public const ushort MODE_mask        = 0x0070;
            public const ushort MODE_GPIN        = 0x0000;
            public const ushort MODE_GPOUT       = 0x0010;
            public const ushort MODE_ALTA        = 0x0020;
            public const ushort MODE_ALTB        = 0x0030;
            public const ushort MODE_INTRL       = 0x0040;
            public const ushort MODE_INTRH       = 0x0050;
            public const ushort MODE_INTRNE      = 0x0060;
            public const ushort MODE_INTRPE      = 0x0070;

            public const ushort DB_EN            = 0x0080;
            public const ushort INTR_STAT        = 0x0100;
            public const ushort INTR_RAW         = 0x0200;

            [Register(Offset=0x00000000U)] public ushort Data;
        }

        [MemoryMappedPeripheral(Base=0x0000U,Length=0x0020U)]
        public class PIN8
        {
            public const byte DBCLK_SEL__SLOWCLK_DIV_00002 = 0x00;
            public const byte DBCLK_SEL__SLOWCLK_DIV_00004 = 0x01;
            public const byte DBCLK_SEL__SLOWCLK_DIV_00008 = 0x02;
            public const byte DBCLK_SEL__SLOWCLK_DIV_00016 = 0x03;
            public const byte DBCLK_SEL__SLOWCLK_DIV_00032 = 0x04;
            public const byte DBCLK_SEL__SLOWCLK_DIV_00064 = 0x05;
            public const byte DBCLK_SEL__SLOWCLK_DIV_00128 = 0x06;
            public const byte DBCLK_SEL__SLOWCLK_DIV_00256 = 0x07;
            public const byte DBCLK_SEL__SLOWCLK_DIV_00512 = 0x08;
            public const byte DBCLK_SEL__SLOWCLK_DIV_01024 = 0x09;
            public const byte DBCLK_SEL__SLOWCLK_DIV_02048 = 0x0A;
            public const byte DBCLK_SEL__SLOWCLK_DIV_04096 = 0x0B;
            public const byte DBCLK_SEL__SLOWCLK_DIV_08192 = 0x0C;
            public const byte DBCLK_SEL__SLOWCLK_DIV_16384 = 0x0D;
            public const byte DBCLK_SEL__SLOWCLK_DIV_32768 = 0x0E;
            public const byte DBCLK_SEL__SLOWCLK_DIV_65536 = 0x0F;

            [Register(Offset=0x00000000U)] public byte PIN_DIN8;
            [Register(Offset=0x00000004U)] public byte DATA_OUT8;
            [Register(Offset=0x00000008U)] public byte INTR_STAT8;
            [Register(Offset=0x0000000CU)] public byte INTR_RAW8;
            [Register(Offset=0x00000010U)] public byte DBCLK_SEL;       // only valid in array offset 0 (+0x0110)
        }

        public const uint c_Pin_None = 0xFFFFFFFF;
        public const uint c_Pin_00 =  0;
        public const uint c_Pin_01 =  1;
        public const uint c_Pin_02 =  2;
        public const uint c_Pin_03 =  3;
        public const uint c_Pin_04 =  4;
        public const uint c_Pin_05 =  5;
        public const uint c_Pin_06 =  6;
        public const uint c_Pin_07 =  7;
        public const uint c_Pin_08 =  8;
        public const uint c_Pin_09 =  9;
        public const uint c_Pin_10 = 10;
        public const uint c_Pin_11 = 11;
        public const uint c_Pin_12 = 12;
        public const uint c_Pin_13 = 13;
        public const uint c_Pin_14 = 14;
        public const uint c_Pin_15 = 15;
        public const uint c_Pin_16 = 16;
        public const uint c_Pin_17 = 17;
        public const uint c_Pin_18 = 18;
        public const uint c_Pin_19 = 19;
        public const uint c_Pin_20 = 20;
        public const uint c_Pin_21 = 21;
        public const uint c_Pin_22 = 22;
        public const uint c_Pin_23 = 23;
        public const uint c_Pin_24 = 24;
        public const uint c_Pin_25 = 25;
        // 26->31 are not available
        public const uint c_Pin_32 = 32;
        public const uint c_Pin_33 = 33;
        public const uint c_Pin_34 = 34;
        public const uint c_Pin_35 = 35;
        public const uint c_Pin_36 = 36;
        public const uint c_Pin_37 = 37;
        public const uint c_Pin_38 = 38;
        public const uint c_Pin_39 = 39;
        // 40->63 are not available

        //--//

        [Register(Offset=0x00000000U,Instances=64)] public CW[]   Control;
        [Register(Offset=0x00000100U,Instances= 8)] public PIN8[] Pin8;

        //
        // Helper Methods
        //

        [Inline]
        public uint PinStatus( uint pin )
        {
            return (uint)this.Control[pin].Data & MM9691LP.GPIO.CW.PIN;
        }

        //
        // Access Methods
        //

        public static extern GPIO Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}