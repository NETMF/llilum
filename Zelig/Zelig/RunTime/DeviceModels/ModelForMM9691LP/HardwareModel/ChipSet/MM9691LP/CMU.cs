//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x380B0000U,Length=0x00000010U)]
    public class CMU
    {
        public enum PERF_LEVEL : uint
        {
            CLK_SEL__DIV_FAST    = 0xFF,
            CLK_SEL__DIV_1       = 0xFF,
            CLK_SEL__DIV_2       = 0x7F,
            CLK_SEL__DIV_3       = 0x3F,
            CLK_SEL__DIV_4       = 0x1F,
            CLK_SEL__DIV_6       = 0x0F,
            CLK_SEL__DIV_12      = 0x07,
            CLK_SEL__DIV_24      = 0x03,
            CLK_SEL__DIV_SLOW    = 0x03,
            CLK_SEL__CPU_CK_SLOW = 0x01,
            CLK_SEL__OFF         = 0x00,
        }

        //--//

        public const uint CLK_SEL__APC_EN       = 0x00000001;
        public const uint CLK_SEL__PU_DIS       = 0x00000002;
        public const uint CLK_SEL__CKOUTEN      = 0x00000020;
        public const uint CLK_SEL__EXTSLOW      = 0x00000040;
        public const uint CLK_SEL__XTALDIS      = 0x00000080;
        public const uint CLK_SEL__48M_EN       = 0x00000100;
        public const uint CLK_SEL__NOPCU        = 0x00000200;
        public const uint CLK_SEL__CLKSEL_RO    = 0x00003000;
        public const uint CLK_SEL__BOOT         = 0x0000C000;
        public const uint CLK_SEL__MASK         = 0x0000F3FF;

        public const uint MCLK_EN__DMAC         = 0x00000001;
        public const uint MCLK_EN__VITERBI      = 0x00000002;
        public const uint MCLK_EN__FILTER       = 0x00000004;
        public const uint MCLK_EN__APC          = 0x00000008;
        public const uint MCLK_EN__ARMTIM       = 0x00000010;
        public const uint MCLK_EN__VTU32        = 0x00000020;
        public const uint MCLK_EN__USART0       = 0x00000040;
        public const uint MCLK_EN__USART1       = 0x00000080;
        public const uint MCLK_EN__RESERVED2    = 0x00000100;
        public const uint MCLK_EN__GPIO         = 0x00000200;
        public const uint MCLK_EN__UWIRE        = 0x00000400;
        public const uint MCLK_EN__USB          = 0x00000800;
        public const uint MCLK_EN__ALL          = 0x0000FFFF;

        public static uint PLLNMP__set( uint N ,
                                        uint M ,
                                        uint P )
        {
            return ((P & 0x03) << 13) | ((M & 0x1F) << 8) | (N & 0x7F);
        }

        //--//

        [Register(Offset=0x00000000U)] public uint PERF_LVL;
        [Register(Offset=0x00000004U)] public uint CLK_SEL;
        [Register(Offset=0x00000008U)] public byte REF_REG;
        [Register(Offset=0x0000000CU)] public uint MCLK_EN;
        [Register(Offset=0x00000010U)] public uint CLK_EN_REG;
        [Register(Offset=0x00000014U)] public uint PLLNMP;

        //
        // Helper Methods
        //

        public void WaitForClockEnableToPropagate()
        {
            while(this.CLK_EN_REG != this.MCLK_EN);
        }

        public void EnableClockWithoutWait( uint mask )
        {
            WaitForClockEnableToPropagate();

            this.MCLK_EN |= mask;
        }

        public void EnableClock( uint mask )
        {
            WaitForClockEnableToPropagate();

            this.MCLK_EN |= mask;

            WaitForClockEnableToPropagate();
        }

        public void DisableClockWithoutWait( uint mask )
        {
            WaitForClockEnableToPropagate();

            this.MCLK_EN &= ~mask;
        }

        public void DisableClock( uint mask )
        {
            WaitForClockEnableToPropagate();

            this.MCLK_EN &= ~mask;

            WaitForClockEnableToPropagate();
        }

        //
        // Access Methods
        //

        public static extern CMU Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}