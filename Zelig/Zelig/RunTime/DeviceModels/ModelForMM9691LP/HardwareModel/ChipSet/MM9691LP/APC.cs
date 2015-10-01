//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using Microsoft.Zelig.Runtime;


    //
    // Advanced Power Control
    //
    [MemoryMappedPeripheral(Base=0x380F0000U,Length=0x00000020U)]
    public class APC
    {
        [Register(Offset=0x00000000U)] public byte APC_PWICMD;
        [Register(Offset=0x00000004U)] public byte APC_PWIDATAWR;
        [Register(Offset=0x00000008U)] public byte APC_PWIDATAWD;
        [Register(Offset=0x00000010U)] public byte APC_CONTROL;
        [Register(Offset=0x00000014U)] public byte APC_STATUS;
        [Register(Offset=0x00000018U)] public byte APC_MINVDD_LIMIT;
        [Register(Offset=0x0000001CU)] public byte APC_VDDCHK;
        [Register(Offset=0x00000020U)] public byte APC_VDDCHKD;
        [Register(Offset=0x00000024U)] public byte APC_PREDLYSEL;
        [Register(Offset=0x00000028U)] public byte APC_IMASK;
        [Register(Offset=0x0000002CU)] public byte APC_ISTATUS;
        [Register(Offset=0x00000030U)] public byte APC_ICLEAR;
        [Register(Offset=0x00000034U)] public byte APC_UNSH_NOISE;
        [Register(Offset=0x00000038U)] public byte APC_WKUP_DLY;
        [Register(Offset=0x0000003CU)] public byte APC_SLK_SMP;
        [Register(Offset=0x00000040U)] public byte APC_CLKDIV_PWICLK;
        [Register(Offset=0x00000050U)] public byte APC_OVSHT_LMT;
        [Register(Offset=0x00000054U)] public byte APC_CLP_CTRL;
        [Register(Offset=0x00000058U)] public byte APC_SS_SRATE;
        [Register(Offset=0x0000005CU)] public byte APC_IGAIN4;
        [Register(Offset=0x00000060U)] public byte APC_IGAIN1;
        [Register(Offset=0x00000064U)] public byte APC_IGAIN2;
        [Register(Offset=0x00000068U)] public byte APC_IGAIN3;
        [Register(Offset=0x0000006CU)] public byte APC_ITSTCTRL;
        [Register(Offset=0x00000070U)] public byte APC_ITSTIP1;
        [Register(Offset=0x00000074U)] public byte APC_ITSTIP2;
        [Register(Offset=0x00000078U)] public byte APC_ITSTOP1;
        [Register(Offset=0x0000007CU)] public byte APC_ITSTOP2;
        [Register(Offset=0x00000080U)] public byte APC_PL1_CALCODE;
        [Register(Offset=0x00000084U)] public byte APC_PL2_CALCODE;
        [Register(Offset=0x00000088U)] public byte APC_PL3_CALCODE;
        [Register(Offset=0x0000008CU)] public byte APC_PL4_CALCODE;
        [Register(Offset=0x00000090U)] public byte APC_PL5_CALCODE;
        [Register(Offset=0x00000094U)] public byte APC_PL6_CALCODE;
        [Register(Offset=0x00000098U)] public byte APC_PL7_CALCODE;
        [Register(Offset=0x0000009CU)] public byte APC_PL8_CALCODE;
        [Register(Offset=0x000000A0U)] public byte APC_PL1_COREVDD;
        [Register(Offset=0x000000A4U)] public byte APC_PL2_COREVDD;
        [Register(Offset=0x000000A8U)] public byte APC_PL3_COREVDD;
        [Register(Offset=0x000000ACU)] public byte APC_PL4_COREVDD;
        [Register(Offset=0x000000B0U)] public byte APC_PL5_COREVDD;
        [Register(Offset=0x000000B4U)] public byte APC_PL6_COREVDD;
        [Register(Offset=0x000000B8U)] public byte APC_PL7_COREVDD;
        [Register(Offset=0x000000BCU)] public byte APC_PL8_COREVDD;
        [Register(Offset=0x000000C0U)] public byte APC_RET_VDD;
        [Register(Offset=0x000000C4U)] public byte APC_INTEGRATION_TEST_REG;
        [Register(Offset=0x000000E0U)] public byte APC_DBG_DLYCODE;
        [Register(Offset=0x000000FCU)] public byte APC_REV;
    }
}
