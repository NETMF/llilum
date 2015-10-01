//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x
{
    /*
        Table 28-1. Coprocessor Register Summary

            | CRn | CRm | Opcode2 | Register Symbol | Register Description

            Coprocessor 6—Interrupt Controller
            | 0   | 0   | 0       | ICIP†           | Interrupt Controller IRQ Pending register
            | 1   | 0   | 0       | ICMR†           | Interrupt Controller Mask register
            | 2   | 0   | 0       | ICLR†           | Interrupt Controller Level register
            | 3   | 0   | 0       | ICFR†           | Interrupt Controller FIQ Pending register
            | 4   | 0   | 0       | ICPR†           | Interrupt Pending register
            | 5   | 0   | 0       | ICHP†           | Interrupt Highest Priority register

            Coprocessor 14—Clock, Power Management, Debug, and Trace Registers
            | 0   | 0   | 0       | PMNC            | Performance Monitoring Control register
            | 1   | 0   | 0       | CCNT            | Performance Monitoring Clock Counter
            | 2   | 0   | 0       | PMN0            | Performance Monitoring Event Counter 0
            | 3   | 0   | 0       | PMN1            | Performance Monitoring Event Counter 1
            | 4   | —   | —       | —               | reserved
            | 5   | —   | —       | —               | reserved
            | 6   | 0   | 0       | CCLKCFG         | Core Clock Configuration register
            | 7   | 0   | 0       | PWRMODE         | Power Management register
            | 8   | 0   | 0       | TX              | Access Transmit Debug register
            | 9   | 0   | 0       | RX              | Access Receive Debug register
            | 10  | 0   | 0       | DBGCSR          | Access Debug Control and Status register
            | 11  | 0   | 0       | TBREG           | Access Trace Buffer register
            | 12  | 0   | 0       | CHKPT0          | Access Checkpoint0 register
            | 13  | 0   | 0       | CHKPT1          | Access Checkpoint1 register
            | 14  | 0   | 0       | TXRXCTRL        | Access Transmit and Receive Debug Control
            | 15  | —   | —       | —               | reserved

            Coprocessor 15—Intel XScale® Core System Control

            ID and Cache Type Registers
            | 0   | 0   | 0       | ID Identification register
            | 0   | 0   | 1       | Cache Type

            Control and Auxiliary Registers
            | 1   | 0   | 0       | ARM* Control register
            | 1   | 0   | 1       | Auxiliary Control register
            | 2   | 0   | 0       | Translation Table Base register
            | 3   | 0   | 0       | Domain Access Control register
            | 4   | —   | —       | — reserved
            | 5   | 0   | 0       | Fault Status register
            | 6   | 0   | 0       | Fault Address register

            Cache Operations
            | 7   | 7   | 0       | Invalidate I&D cache and BTB
            | 7   | 5   | 0       | Invalidate I cache and BTB
            | 7   | 5   | 1       | Invalidate I-cache Line
            | 7   | 6   | 0       | Invalidate D-cache
            | 7   | 6   | 1       | Invalidate D-cache Line
            | 7   | 10  | 1       | Clean D-cache Line
            | 7   | 10  | 4       | Drain Write (&Fill) Buffer
            | 7   | 5   | 6       | Invalidate Branch Target Buffer
            | 7   | 2   | 5       | Allocate Line in the Data Cache
            | 8   | 7   | 0       | Invalidate I&D TLB
            | 8   | 5   | 0       | Invalidate I TLB
            | 8   | 5   | 1       | Invalidate I TLB Entry
            | 8   | 6   | 0       | Invalidate D TLB
            | 8   | 6   | 1       | Invalidate D TLB Entry
            Cache Lock Down
            | 9   | 1   | 0       | Fetch and Lock I-Cache Line

            TLB Operations
            | 9   | 1   | 1       | Unlock I-Cache
            | 9   | 2   | 0       | Read Data Cache Lock register
            | 9   | 2   | 0       | Write Data Cache Lock register
            | 9   | 2   | 1       | Unlock Data Cache
            TLB Lock Down
            | 10  | 4   | 0       | Translate and Lock Instruction TLB Entry
            | 10  | 8   | 0       | Translate and Lock Data TLB Entry
            | 10  | 4   | 1       | Unlock Instruction TLB
            | 10  | 8   | 1       | Unlock Data TLB
            | 11  | —   | —       | — reserved
            | 12  | —   | —       | — reserved
            | 13  | 0   | 0       | PID Processor ID
            Breakpoint Registers
            | 14  | 0   | 0       | DBCR0 Data Breakpoint register 0
            | 14  | 3   | 0       | DBCR1 Data Breakpoint register 1
            | 14  | 4   | 0       | DBCON Data Breakpoint Control register
            | 14  | 8   | 0       | IBCR0 Instruction Breakpoint register 0
            | 14  | 9   | 0       | IBCR1 Instruction Breakpoint register 1
            | 15  | 1   | 0       | CPAR Coprocessor Access
            | 15  | 1   | 0       | CPAR Coprocessor Access

        Table 28-2. Memory Controller Register Summary

        | 0x4800_0000 | MDCNFG    | SDRAM Configuration register                                                 | 6-43
        | 0x4800_0004 | MDREFR    | SDRAM Refresh Control register                                               | 6-53
        | 0x4800_0008 | MSC0      | Static Memory Control register 0                                             | 6-63
        | 0x4800_000C | MSC1      | Static Memory Control register 1                                             | 6-63
        | 0x4800_0010 | MSC2      | Static Memory Control register 2                                             | 6-63
        | 0x4800_0014 | MECR      | Expansion Memory (PC Card/CompactFlash) Bus Configuration register           | 6-79
        | 0x4800_001C | SXCNFG    | Synchronous Static Memory Configuration register                             | 6-58
        | 0x4800_0020 | FLYCNFG   | Fly-by DMA DVAL<1:0> polarities                                              | 5-39
        | 0x4800_0028 | MCMEM0    | PC Card Interface Common Memory Space Socket 0 Timing Configuration register | 6-77
        | 0x4800_002C | MCMEM1    | PC Card Interface Common Memory Space Socket 1 Timing Configuration register | 6-77
        | 0x4800_0030 | MCATT0    | PC Card Interface Attribute Space Socket 0 Timing Configuration register     | 6-77
        | 0x4800_0034 | MCATT1    | PC Card Interface Attribute Space Socket 1 Timing Configuration register     | 6-77
        | 0x4800_0038 | MCIO0     | PC Card Interface I/o Space Socket 0 Timing Configuration register           | 6-78
        | 0x4800_003C | MCIO1     | PC Card Interface I/o Space Socket 1 Timing Configuration register           | 6-78
        | 0x4800_0040 | MDMRS     | SDRAM Mode Register Set Configuration register                               | 6-49
        | 0x4800_0044 | BOOT_DEF  | Boot Time Default Configuration register                                     | 6-75
        | 0x4800_0048 | ARB_CNTL  | Arbiter Control register                                                     | 29-2
        | 0x4800_004C | BSCNTR0   | System Memory Buffer Strength Control register 0                             | 6-81
        | 0x4800_0050 | BSCNTR1   | System Memory Buffer Strength Control register 1                             | 6-82
        | 0x4800_0054 | LCDBSCNTR | LCD Buffer Strength Control register                                         | 7-102
        | 0x4800_0058 | MDMRSLP   | Special Low Power SDRAM Mode Register Set Configuration register             | 6-51
        | 0x4800_005C | BSCNTR2   | System Memory Buffer Strength Control register 2                             | 6-83
        | 0x4800_0060 | BSCNTR3   | System Memory Buffer Strength Control register 3                             | 6-84
        | 0x4800_0064 | SA1110    | SA-1110 Compatibility Mode for Static Memory register                        | 6-70

        Table 28-3. LCD Controller Register Summary

        | 0x4400_0000 | LCCR0     | LCD Controller Control register 0                                            | 7-56
        | 0x4400_0004 | LCCR1     | LCD Controller Control register 1                                            | 7-64
        | 0x4400_0008 | LCCR2     | LCD Controller Control register 2                                            | 7-66
        | 0x4400_000C | LCCR3     | LCD Controller Control register 3                                            | 7-69
        | 0x4400_0010 | LCCR4     | LCD Controller Control register 4                                            | 7-74
        | 0x4400_0014 | LCCR5     | LCD Controller Control register 5                                            | 7-77
        | 0x4400_0020 | FBR0      | DMA Channel 0 Frame Branch register                                          | 7-101
        | 0x4400_0024 | FBR1      | DMA Channel 1 Frame Branch register                                          | 7-101
        | 0x4400_0028 | FBR2      | DMA Channel 2 Frame Branch register                                          | 7-101
        | 0x4400_002C | FBR3      | DMA Channel 3 Frame Branch register                                          | 7-101
        | 0x4400_0030 | FBR4      | DMA Channel 4 Frame Branch register                                          | 7-101
        | 0x4400_0034 | LCSR1     | LCD Controller Status register 1                                             | 7-109
        | 0x4400_0038 | LCSR0     | LCD Controller Status register 0                                             | 7-104
        | 0x4400_003C | LIIDR     | LCD Controller Interrupt ID register                                         | 7-116
        | 0x4400_0040 | TRGBR     | TMED RGB Seed register                                                       | 7-97
        | 0x4400_0044 | TCR       | TMED Control register                                                        | 7-98
        | 0x4400_0050 | OVL1 C1   | Overlay 1 Control register 1                                                 | 7-90
        | 0x4400_0060 | OVL1 C2   | Overlay 1 Control register 2                                                 | 7-91
        | 0x4400_0070 | OVL2 C1   | Overlay 2 Control register 1                                                 | 7-92
        | 0x4400_0080 | OVL2C2    | Overlay 2 Control register 2                                                 | 7-94
        | 0x4400_0090 | CCR       | Cursor Control register                                                      | 7-95
        | 0x4400_0100 | CMDCR     | Command Control register                                                     | 7-96
        | 0x4400_0104 | PRSR      | Panel Read Status register                                                   | 7-103
        | 0x4400_0110 | FBR5      | DMA Channel 5 Frame Branch register                                          | 7-101
        | 0x4400_0114 | FBR6      | DMA Channel 6 Frame Branch register                                          | 7-101
        | 0x4400_0200 | FDADR0    | DMA Channel 0 Frame Descriptor Address register                              | 7-100
        | 0x4400_0204 | FSADR0    | DMA Channel 0 Frame Source Address register                                  | 7-117
        | 0x4400_0208 | FIDR0     | DMA Channel 0 Frame ID register                                              | 7-117
        | 0x4400_020C | LDCMD0    | LCD DMA Channel 0 Command register                                           | 7-118
        | 0x4400_0210 | FDADR1    | DMA Channel 1 Frame Descriptor Address register                              | 7-100
        | 0x4400_0214 | FSADR1    | DMA Channel 1 Frame Source Address register                                  | 7-117
        | 0x4400_0218 | FIDR1     | DMA Channel 1 Frame ID register                                              | 7-117
        | 0x4400_021C | LDCMD1    | LCD DMA Channel 1 Command register                                           | 7-118
        | 0x4400_0220 | FDADR2    | DMA Channel 2 Frame Descriptor Address register                              | 7-100
        | 0x4400_0224 | FSADR2    | DMA Channel 2 Frame Source Address register                                  | 7-117
        | 0x4400_0228 | FIDR2     | DMA Channel 2 Frame ID register                                              | 7-117
        | 0x4400_022C | LDCMD2    | LCD DMA Channel 2 Command register                                           | 7-118
        | 0x4400_0230 | FDADR3    | DMA Channel 3 Frame Descriptor Address register                              | 7-100
        | 0x4400_0234 | FSADR3    | DMA Channel 3 Frame Source Address register                                  | 7-117
        | 0x4400_0238 | FIDR3     | DMA Channel 3 Frame ID register                                              | 7-117
        | 0x4400_023C | LDCMD3    | LCD DMA Channel 3 Command register                                           | 7-118
        | 0x4400_0240 | FDADR4    | DMA Channel 4 Frame Descriptor Address register                              | 7-100
        | 0x4400_0244 | FSADR4    | DMA Channel 4 Frame Source Address register                                  | 7-117
        | 0x4400_0248 | FIDR4     | DMA Channel 4 Frame ID register                                              | 7-117
        | 0x4400_024C | LDCMD4    | LCD DMA Channel 4 Command register                                           | 7-118
        | 0x4400_0250 | FDADR5    | DMA Channel 5 Frame Descriptor Address register                              | 7-100
        | 0x4400_0254 | FSADR5    | DMA Channel 5 Frame Source Address register                                  | 7-117
        | 0x4400_0258 | FIDR5     | DMA Channel 5 Frame ID register                                              | 7-117
        | 0x4400_025C | LDCMD5    | LCD DMA Channel 5 Command register                                           | 7-118
        | 0x4400_0260 | FDADR6    | DMA Channel 6 Frame Descriptor Address register                              | 7-100
        | 0x4400_0264 | FSADR6    | DMA Channel 6 Frame Source Address register                                  | 7-117
        | 0x4400_0268 | FIDR6     | DMA Channel 6 Frame ID register                                              | 7-117
        | 0x4400_026C | LDCMD6    | LCD DMA Channel 6 Command register                                           | 7-118
        | 0x4800_0054 | LCDBSCNTR | LCD Buffer Strength Control register                                         | 7-102

        28.2.4 USB Host Controller Registers

        | 0x4C00 0000 | UHCREV    | UHC HCI Spec Revision register                                               | 20-10
        | 0x4C00 0004 | UHCHCON   | UHC Host Control register                                                    | 20-10
        | 0x4C00 0008 | UHCCOMS   | UHC Command Status register                                                  | 20-14
        | 0x4C00 000C | UHCINTS   | UHC Interrupt Status register                                                | 20-16
        | 0x4C00 0010 | UHCINTE   | UHC Interrupt Enable register                                                | 20-18
        | 0x4C00 0014 | UHCINTD   | UHC Interrupt Disable register                                               | 20-20
        | 0x4C00 0018 | UHCHCCA   | UHC Host Controller Communication Area register                              | 20-21
        | 0x4C00 001C | UHCPCED   | UHC Period Current Endpoint Descriptor register                              | 20-21
        | 0x4C00 0020 | UHCCHED   | UHC Control Head Endpoint Descriptor register                                | 20-22
        | 0x4C00 0024 | UHCCCED   | UHC Control Current Endpoint Descriptor register                             | 20-22
        | 0x4C00 0028 | UHCBHED   | UHC Bulk Head Endpoint Descriptor register                                   | 20-23
        | 0x4C00 002C | UHCBCED   | UHC Bulk Current Endpoint Descriptor register                                | 20-24
        | 0x4C00 0030 | UHCDHEAD  | UHC Done Head register                                                       | 20-25
        | 0x4C00 0034 | UHCFMI    | UHC Frame Interval register                                                  | 20-26
        | 0x4C00 0038 | UHCFMR    | UHC Frame Remaining register                                                 | 20-27
        | 0x4C00 003C | UHCFMN    | UHC Frame Number register                                                    | 20-28
        | 0x4C00 0040 | UHCPERS   | UHC Periodic Start register                                                  | 20-29
        | 0x4C00 0044 | UHCLST    | UHC Low-Speed Threshold register                                             | 20-30
        | 0x4C00 0048 | UHCRHDA   | UHC Root Hub Descriptor A register                                           | 20-31
        | 0x4C00 004C | UHCRHDB   | UHC Root Hub Descriptor B register                                           | 20-33
        | 0x4C00 0050 | UHCRHS    | UHC Root Hub Status register                                                 | 20-34
        | 0x4C00 0054 | UHCRHPS1  | UHC Root Hub Port 1 Status register                                          | 20-35
        | 0x4C00 0058 | UHCRHPS2  | UHC Root Hub Port 2 Status register                                          | 20-35
        | 0x4C00 005C | UHCRHPS3  | UHC Root Hub Port 3 Status register                                          | 20-35
        | 0x4C00 0060 | UHCSTAT   | UHC Status register                                                          | 20-39
        | 0x4C00 0064 | UHCHR     | UHC Reset register                                                           | 20-41
        | 0x4C00 0068 | UHCHIE    | UHC Interrupt Enable register                                                | 20-44
        | 0x4C00 006C | UHCHIT    | UHC Interrupt Test register                                                  | 20-45

        Table 28-5. Internal Memory Register Summary

        0x5C00_0000–0x5C00_FFFC Memory Bank 0 64-Kbyte SRAM —
        0x5C01_0000–0x5C01_FFFC Memory Bank 1 6-4Kbyte SRAM —
        0x5C02_0000–0x5C02_FFFC Memory Bank 2 64-Kbyte SRAM —
        0x5C03_0000–0x5C03_FFFC Memory Bank 3 64-Kbyte SRAM —

        Table 28-6. Quick Capture Interface Register Summary

        | 0x5000_0000 | CICR0     | Quick Capture Interface Control register 0                                   | 27-24
        | 0x5000_0004 | CICR1     | Quick Capture Interface Control register 1                                   | 27-28
        | 0x5000_0008 | CICR2     | Quick Capture Interface Control register 2                                   | 27-32
        | 0x5000_000C | CICR3     | Quick Capture Interface Control register 3                                   | 27-33
        | 0x5000_0010 | CICR4     | Quick Capture Interface Control register 4                                   | 27-34
        | 0x5000_0014 | CISR      | Quick Capture Interface Status register                                      | 27-37
        | 0x5000_0018 | CIFR      | Quick Capture Interface FIFO Control register                                | 27-40
        | 0x5000_001C | CITOR     | Quick Capture Interface Time-Out register                                    | 27-37
        | 0x5000_0028 | CIBR0     | Quick Capture Interface Receive Buffer register 0 (Channel 0)                | 27-42
        | 0x5000_0030 | CIBR1     | Quick Capture Interface Receive Buffer register 1 (Channel 1)                | 27-42
        | 0x5000_0038 | CIBR2     | Quick Capture Interface Receive Buffer register 2 (Channel 2)                | 27-42

        28.3 Peripheral Module Registers

        DMA Controller                              0x4000_0000
        UART1—Full Function UART                    0x4010_0000
        UART2—Bluetooth UART                        0x4020_0000
        Standard I2C Bus Interface Unit             0x4030_0000
        I2S Controller                              0x4040_0000
        AC ’97 Controller                           0x4050_0000
        USB Client Controller                       0x4060_0000
        UART 3—Standard UART                        0x4070_0000
        Fast Infrared Communications Port           0x4080_0000
        RTC                                         0x4090_0000
        OS Timers                                   0x40A0_0000
        PWM0 and 2                                  0x40B0_0000
        PWM1 and 3                                  0x40C0_0000
        Interrupt Controller                        0x40D0_0000
        GPIO Controller                             0x40E0_0000
        Power Manager                               0x40F0_0000
        Reset Controller                            0x40F0_0000
        Power Manager I2C                           0x40F0_0180
        Synchronous Serial Port 1                   0x4100_0000
        MultiMediaCard/SD/SDIO Controller           0x4110_0000
        reserved                                    0x4120_0000
        Clocks Manager                              0x4130_0000
        Mobile Scalable Link (MSL)                  0x4140_0000
        Keypad Interface                            0x4150_0000
        Universal Subscriber ID (USIM) Interface    0x4160_0000
        Synchronous Serial Port 2                   0x4170_0000
        Memory Stick Host Controller                0x4180_0000
        Synchronous Serial Port 3                   0x4190_0000


        DMA Controller
        | 0x4000_0000 | DCSR0     | DMA Control/Status register for Channel 0                                     | 5-41
        | 0x4000_0004 | DCSR1     | DMA Control/Status register for Channel 1                                     | 5-41
        | 0x4000_0008 | DCSR2     | DMA Control/Status register for Channel 2                                     | 5-41
        | 0x4000_000C | DCSR3     | DMA Control/Status register for Channel 3                                     | 5-41
        | 0x4000_0010 | DCSR4     | DMA Control/Status register for Channel 4                                     | 5-41
        | 0x4000_0014 | DCSR5     | DMA Control/Status register for Channel 5                                     | 5-41
        | 0x4000_0018 | DCSR6     | DMA Control/Status register for Channel 6                                     | 5-41
        | 0x4000_001C | DCSR7     | DMA Control/Status register for Channel 7                                     | 5-41
        | 0x4000_0020 | DCSR8     | DMA Control/Status register for Channel 8                                     | 5-41
        | 0x4000_0024 | DCSR9     | DMA Control/Status register for Channel 9                                     | 5-41
        | 0x4000_0028 | DCSR10    | DMA Control/Status register for Channel 10                                    | 5-41
        | 0x4000_002C | DCSR11    | DMA Control/Status register for Channel 11                                    | 5-41
        | 0x4000_0030 | DCSR12    | DMA Control/Status register for Channel 12                                    | 5-41
        | 0x4000_0034 | DCSR13    | DMA Control/Status register for Channel 13                                    | 5-41
        | 0x4000_0038 | DCSR14    | DMA Control/Status register for Channel 14                                    | 5-41
        | 0x4000_003C | DCSR15    | DMA Control/Status register for Channel 15                                    | 5-41
        | 0x4000_0040 | DCSR16    | DMA Control/Status register for Channel 16                                    | 5-41
        | 0x4000_0044 | DCSR17    | DMA Control/Status register for Channel 17                                    | 5-41
        | 0x4000_0048 | DCSR18    | DMA Control/Status register for Channel 18                                    | 5-41
        | 0x4000_004C | DCSR19    | DMA Control/Status register for Channel 19                                    | 5-41
        | 0x4000_0050 | DCSR20    | DMA Control/Status register for Channel 20                                    | 5-41
        | 0x4000_0054 | DCSR21    | DMA Control/Status register for Channel 21                                    | 5-41
        | 0x4000_0058 | DCSR22    | DMA Control/Status register for Channel 22                                    | 5-41
        | 0x4000_005C | DCSR23    | DMA Control/Status register for Channel 23                                    | 5-41
        | 0x4000_0060 | DCSR24    | DMA Control/Status register for Channel 24                                    | 5-41
        | 0x4000_0064 | DCSR25    | DMA Control/Status register for Channel 25                                    | 5-41
        | 0x4000_0068 | DCSR26    | DMA Control/Status register for Channel 26                                    | 5-41
        | 0x4000_006C | DCSR27    | DMA Control/Status register for Channel 27                                    | 5-41
        | 0x4000_0070 | DCSR28    | DMA Control/Status register for Channel 28                                    | 5-41
        | 0x4000_0074 | DCSR29    | DMA Control/Status register for Channel 29                                    | 5-41
        | 0x4000_0078 | DCSR30    | DMA Control/Status register for Channel 30                                    | 5-41
        | 0x4000_007C | DCSR31    | DMA Control/Status register for Channel 31                                    | 5-41
        | 0x4000_00A0 | DALGN     | DMA Alignment register                                                        | 5-49
        | 0x4000_00A4 | DPCSR     | DMA Programmed I/O Control Status register                                    | 5-51
        | 0x4000_00E0 | DRQSR0    | DMA DREQ<0> Status register                                                   | 5-40
        | 0x4000_00E4 | DRQSR1    | DMA DREQ<1> Status register                                                   | 5-40
        | 0x4000_00E8 | DRQSR2    | DMA DREQ<2> Status register                                                   | 5-40
        | 0x4000_00F0 | DINT      | DMA Interrupt register                                                        | 5-48
        | 0x4000_0100 | DRCMR0    | Request to Channel Map register for DREQ<0> (companion chip request 0)        | 5-31
        | 0x4000_0104 | DRCMR1    | Request to Channel Map register for DREQ<1> (companion chip request 1)        | 5-31
        | 0x4000_0108 | DRCMR2    | Request to Channel Map register for I2S receive request                       | 5-31
        | 0x4000_010C | DRCMR3    | Request to Channel Map register for I2S transmit request                      | 5-31
        | 0x4000_0110 | DRCMR4    | Request to Channel Map register for BTUART receive request                    | 5-31
        | 0x4000_0114 | DRCMR5    | Request to Channel Map register for BTUART transmit request.                  | 5-31
        | 0x4000_0118 | DRCMR6    | Request to Channel Map register for FFUART receive request                    | 5-31
        | 0x4000_011C | DRCMR7    | Request to Channel Map register for FFUART transmit request                   | 5-31
        | 0x4000_0120 | DRCMR8    | Request to Channel Map register for AC ’97 microphone request                 | 5-31
        | 0x4000_0124 | DRCMR9    | Request to Channel Map register for AC ’97 modem receive request              | 5-31
        | 0x4000_0128 | DRCMR10   | Request to Channel Map register for AC ’97 modem transmit request             | 5-31
        | 0x4000_012C | DRCMR11   | Request to Channel Map register for AC ’97 audio receive request              | 5-31
        | 0x4000_0130 | DRCMR12   | Request to Channel Map register for AC ’97 audio transmit request             | 5-31
        | 0x4000_0134 | DRCMR13   | Request to Channel Map register for SSP1 receive request                      | 5-31
        | 0x4000_0138 | DRCMR14   | Request to Channel Map register for SSP1 transmit request                     | 5-31
        | 0x4000_013C | DRCMR15   | Request to Channel Map register for SSP2 receive request                      | 5-31
        | 0x4000_0140 | DRCMR16   | Request to Channel Map register for SSP2 transmit request                     | 5-31
        | 0x4000_0144 | DRCMR17   | Request to Channel Map register for ICP receive request                       | 5-31
        | 0x4000_0148 | DRCMR18   | Request to Channel Map register for ICP transmit request                      | 5-31
        | 0x4000_014C | DRCMR19   | Request to Channel Map register for STUART receive request                    | 5-31
        | 0x4000_0150 | DRCMR20   | Request to Channel Map register for STUART transmit request                   | 5-31
        | 0x4000_0154 | DRCMR21   | Request to Channel Map register for MMC/SDIO receive request                  | 5-31
        | 0x4000_0158 | DRCMR22   | Request to Channel Map register for MMC/SDIO transmit request                 | 5-31
        | 0x4000_0160 | DRCMR24   | Request to Channel Map register for USB endpoint 0 request                    | 5-31
        | 0x4000_0164 | DRCMR25   | Request to Channel Map register for USB endpoint A request                    | 5-31
        | 0x4000_0168 | DRCMR26   | Request to Channel Map register for USB endpoint B request                    | 5-31
        | 0x4000_016C | DRCMR27   | Request to Channel Map register for USB endpoint C request                    | 5-31
        | 0x4000_0170 | DRCMR28   | Request to Channel Map register for USB endpoint D request                    | 5-31
        | 0x4000_0174 | DRCMR29   | Request to Channel Map register for USB endpoint E request                    | 5-31
        | 0x4000_0178 | DRCMR30   | Request to Channel Map register for USB endpoint F request                    | 5-31
        | 0x4000_017C | DRCMR31   | Request to Channel Map register for USB endpoint G request                    | 5-31
        | 0x4000_0180 | DRCMR32   | Request to Channel Map register for USB endpoint H request                    | 5-31
        | 0x4000_0184 | DRCMR33   | Request to Channel Map register for USB endpoint I request                    | 5-31
        | 0x4000_0188 | DRCMR34   | Request to Channel Map register for USB endpoint J request                    | 5-31
        | 0x4000_018C | DRCMR35   | Request to Channel Map register for USB endpoint K request                    | 5-31
        | 0x4000_0190 | DRCMR36   | Request to Channel Map register for USB endpoint L request                    | 5-31
        | 0x4000_0194 | DRCMR37   | Request to Channel Map register for USB endpoint M request                    | 5-31
        | 0x4000_0198 | DRCMR38   | Request to Channel Map register for USB endpoint N request                    | 5-31
        | 0x4000_019C | DRCMR39   | Request to Channel Map register for USB endpoint P request                    | 5-31
        | 0x4000_01A0 | DRCMR40   | Request to Channel Map register for USB endpoint Q request                    | 5-31
        | 0x4000_01A4 | DRCMR41   | Request to Channel Map register for USB endpoint R request                    | 5-31
        | 0x4000_01A8 | DRCMR42   | Request to Channel Map register for USB endpoint S request                    | 5-31
        | 0x4000_01AC | DRCMR43   | Request to Channel Map register for USB endpoint T request                    | 5-31
        | 0x4000_01B0 | DRCMR44   | Request to Channel Map register for USB endpoint U request                    | 5-31
        | 0x4000_01B4 | DRCMR45   | Request to Channel Map register for USB endpoint V request                    | 5-31
        | 0x4000_01B8 | DRCMR46   | Request to Channel Map register for USB endpoint W request                    | 5-31
        | 0x4000_01BC | DRCMR47   | Request to Channel Map register for USB endpoint X request                    | 5-31
        | 0x4000_01C0 | DRCMR48   | Request to Channel Map register for MSL receive request 1                     | 5-31
        | 0x4000_01C4 | DRCMR49   | Request to Channel Map register for MSL transmit request 1                    | 5-31
        | 0x4000_01C8 | DRCMR50   | Request to Channel Map register for MSL receive request 2                     | 5-31
        | 0x4000_01CC | DRCMR51   | Request to Channel Map register for MSL transmit request 2                    | 5-31
        | 0x4000_01D0 | DRCMR52   | Request to Channel Map register for MSL receive request 3                     | 5-31
        | 0x4000_01D4 | DRCMR53   | Request to Channel Map register for MSL transmit request 3                    | 5-31
        | 0x4000_01D8 | DRCMR54   | Request to Channel Map register for MSL receive request 4                     | 5-31
        | 0x4000_01DC | DRCMR55   | Request to Channel Map register for MSL transmit request 4                    | 5-31
        | 0x4000_01E0 | DRCMR56   | Request to Channel Map register for MSL receive request 5                     | 5-31
        | 0x4000_01E4 | DRCMR57   | Request to Channel Map register for MSL transmit request 5                    | 5-31
        | 0x4000_01E8 | DRCMR58   | Request to Channel Map register for MSL receive request 6                     | 5-31
        | 0x4000_01EC | DRCMR59   | Request to Channel Map register for MSL transmit request 6                    | 5-31
        | 0x4000_01F0 | DRCMR60   | Request to Channel Map register for MSL receive request 7                     | 5-31
        | 0x4000_01F4 | DRCMR61   | Request to Channel Map register for MSL transmit request 7                    | 5-31
        | 0x4000_01F8 | DRCMR62   | Request to Channel Map register for USIM receive request                      | 5-31
        | 0x4000_01FC | DRCMR63   | Request to Channel Map register for USIM transmit request                     | 5-31
        | 0x4000_0200 | DDADR0    | DMA Descriptor Address register for Channel 0                                 | 5-32
        | 0x4000_0204 | DSADR0    | DMA Source Address register for Channel 0                                     | 5-33
        | 0x4000_0208 | DTADR0    | DMA Target Address register for Channel 0                                     | 5-34
        | 0x4000_020C | DCMD0     | DMA Command Address register for Channel 0                                    | 5-35
        | 0x4000_0210 | DDADR1    | DMA Descriptor Address register for Channel 1                                 | 5-32
        | 0x4000_0214 | DSADR1    | DMA Source Address register for Channel 1                                     | 5-33
        | 0x4000_0218 | DTADR1    | DMA Target Address register for Channel 1                                     | 5-34
        | 0x4000_021C | DCMD1     | DMA Command Address register for Channel 1                                    | 5-35
        | 0x4000_0220 | DDADR2    | DMA Descriptor Address register for Channel 2                                 | 5-32
        | 0x4000_0224 | DSADR2    | DMA Source Address register for Channel 2                                     | 5-33
        | 0x4000_0228 | DTADR2    | DMA Target Address register for Channel 2                                     | 5-34
        | 0x4000_022C | DCMD2     | DMA Command Address register for Channel 2                                    | 5-35
        | 0x4000_0230 | DDADR3    | DMA Descriptor Address register for Channel 3                                 | 5-32
        | 0x4000_0234 | DSADR3    | DMA Source Address register for Channel 3                                     | 5-33
        | 0x4000_0238 | DTADR3    | DMA Target Address register for Channel 3                                     | 5-34
        | 0x4000_023C | DCMD3     | DMA Command Address register for Channel 3                                    | 5-35
        | 0x4000_0240 | DDADR4    | DMA Descriptor Address register for Channel 4                                 | 5-32
        | 0x4000_0244 | DSADR4    | DMA Source Address register for Channel 4                                     | 5-33
        | 0x4000_0248 | DTADR4    | DMA Target Address register for Channel 4                                     | 5-34
        | 0x4000_024C | DCMD4     | DMA Command Address register for Channel 4                                    | 5-35
        | 0x4000_0250 | DDADR5    | DMA Descriptor Address register for Channel 5                                 | 5-32
        | 0x4000_0254 | DSADR5    | DMA Source Address register for Channel 5                                     | 5-33
        | 0x4000_0258 | DTADR5    | DMA Target Address register for Channel 5                                     | 5-34
        | 0x4000_025C | DCMD5     | DMA Command Address register for Channel 5                                    | 5-35
        | 0x4000_0260 | DDADR6    | DMA Descriptor Address register for Channel 6                                 | 5-32
        | 0x4000_0264 | DSADR6    | DMA Source Address register for Channel 6                                     | 5-33
        | 0x4000_0268 | DTADR6    | DMA Target Address register for Channel 6                                     | 5-34
        | 0x4000_026C | DCMD6     | DMA Command Address register for Channel 6                                    | 5-35
        | 0x4000_0270 | DDADR7    | DMA Descriptor Address register for Channel 7                                 | 5-32
        | 0x4000_0274 | DSADR7    | DMA Source Address register for Channel 7                                     | 5-33
        | 0x4000_0278 | DTADR7    | DMA Target Address register for Channel 7                                     | 5-34
        | 0x4000_027C | DCMD7     | DMA Command Address register for Channel 7                                    | 5-35
        | 0x4000_0280 | DDADR8    | DMA Descriptor Address register for Channel 8                                 | 5-32
        | 0x4000_0284 | DSADR8    | DMA Source Address register for Channel 8                                     | 5-33
        | 0x4000_0288 | DTADR8    | DMA Target Address register for Channel 8                                     | 5-34
        | 0x4000_028C | DCMD8     | DMA Command Address register for Channel 8                                    | 5-35
        | 0x4000_0290 | DDADR9    | DMA Descriptor Address register for Channel 9                                 | 5-32
        | 0x4000_0294 | DSADR9    | DMA Source Address register for Channel 9                                     | 5-33
        | 0x4000_0298 | DTADR9    | DMA Target Address register for Channel 9                                     | 5-34
        | 0x4000_029C | DCMD9     | DMA Command Address register for Channel 9                                    | 5-35
        | 0x4000_02A0 | DDADR10   | DMA Descriptor Address register for Channel 10                                | 5-32
        | 0x4000_02A4 | DSADR10   | DMA Source Address register for Channel 10                                    | 5-33
        | 0x4000_02A8 | DTADR10   | DMA Target Address register for Channel 10                                    | 5-34
        | 0x4000_02AC | DCMD10    | DMA Command Address register for Channel 10                                   | 5-35
        | 0x4000_02B0 | DDADR11   | DMA Descriptor Address register for Channel 11                                | 5-32
        | 0x4000_02B4 | DSADR11   | DMA Source Address register for Channel 11                                    | 5-33
        | 0x4000_02B8 | DTADR11   | DMA Target Address register for Channel 11                                    | 5-34
        | 0x4000_02BC | DCMD11    | DMA Command Address register for Channel 11                                   | 5-35
        | 0x4000_02C0 | DDADR12   | DMA Descriptor Address register for Channel 12                                | 5-32
        | 0x4000_02C4 | DSADR12   | DMA Source Address register for Channel 12                                    | 5-33
        | 0x4000_02C8 | DTADR12   | DMA Target Address register for Channel 12                                    | 5-34
        | 0x4000_02CC | DCMD12    | DMA Command Address register for Channel 12                                   | 5-35
        | 0x4000_02D0 | DDADR13   | DMA Descriptor Address register for Channel 13                                | 5-32
        | 0x4000_02D4 | DSADR13   | DMA Source Address register for Channel 13                                    | 5-33
        | 0x4000_02D8 | DTADR13   | DMA Target Address register for Channel 13                                    | 5-34
        | 0x4000_02DC | DCMD13    | DMA Command Address register for Channel 13                                   | 5-35
        | 0x4000_02E0 | DDADR14   | DMA Descriptor Address register for Channel 14                                | 5-32
        | 0x4000_02E4 | DSADR14   | DMA Source Address register for Channel 14                                    | 5-33
        | 0x4000_02E8 | DTADR14   | DMA Target Address register for Channel 14                                    | 5-34
        | 0x4000_02EC | DCMD14    | DMA Command Address register for Channel 14                                   | 5-35
        | 0x4000_02F0 | DDADR15   | DMA Descriptor Address register for Channel 15                                | 5-32
        | 0x4000_02F4 | DSADR15   | DMA Source Address register for Channel 15                                    | 5-33
        | 0x4000_02F8 | DTADR15   | DMA Target Address register for Channel 15                                    | 5-34
        | 0x4000_02FC | DCMD15    | DMA Command Address register for Channel 15                                   | 5-35
        | 0x4000_0300 | DDADR16   | DMA Descriptor Address register for Channel 16                                | 5-32
        | 0x4000_0304 | DSADR16   | DMA Source Address register for Channel 16                                    | 5-33
        | 0x4000_0308 | DTADR16   | DMA Target Address register for Channel 16                                    | 5-34
        | 0x4000_030C | DCMD16    | DMA Command Address register for Channel 16                                   | 5-35
        | 0x4000_0310 | DDADR17   | DMA Descriptor Address register for Channel 17                                | 5-32
        | 0x4000_0314 | DSADR17   | DMA Source Address register for Channel 17                                    | 5-33
        | 0x4000_0318 | DTADR17   | DMA Target Address register for Channel 17                                    | 5-34
        | 0x4000_031C | DCMD17    | DMA Command Address register for Channel 17                                   | 5-35
        | 0x4000_0320 | DDADR18   | DMA Descriptor Address register for Channel 18                                | 5-32
        | 0x4000_0324 | DSADR18   | DMA Source Address register for Channel 18                                    | 5-33
        | 0x4000_0328 | DTADR18   | DMA Target Address register for Channel 18                                    | 5-34
        | 0x4000_032C | DCMD18    | DMA Command Address register for Channel 18                                   | 5-35
        | 0x4000_0330 | DDADR19   | DMA Descriptor Address register for Channel 19                                | 5-32
        | 0x4000_0334 | DSADR19   | DMA Source Address register for Channel 19                                    | 5-33
        | 0x4000_0338 | DTADR19   | DMA Target Address register for Channel 19                                    | 5-34
        | 0x4000_033C | DCMD19    | DMA Command Address register for Channel 19                                   | 5-35
        | 0x4000_0340 | DDADR20   | DMA Descriptor Address register for Channel 20                                | 5-32
        | 0x4000_0344 | DSADR20   | DMA Source Address register for Channel 20                                    | 5-33
        | 0x4000_0348 | DTADR20   | DMA Target Address register for Channel 20                                    | 5-34
        | 0x4000_034C | DCMD20    | DMA Command Address register for Channel 20                                   | 5-35
        | 0x4000_0350 | DDADR21   | DMA Descriptor Address register for Channel 21                                | 5-32
        | 0x4000_0354 | DSADR21   | DMA Source Address register for Channel 21                                    | 5-33
        | 0x4000_0358 | DTADR21   | DMA Target Address register for Channel 21                                    | 5-34
        | 0x4000_035C | DCMD21    | DMA Command Address register for Channel 21                                   | 5-35
        | 0x4000_0360 | DDADR22   | DMA Descriptor Address register for Channel 22                                | 5-32
        | 0x4000_0364 | DSADR22   | DMA Source Address register for Channel 22                                    | 5-33
        | 0x4000_0368 | DTADR22   | DMA Target Address register for Channel 22                                    | 5-34
        | 0x4000_036C | DCMD22    | DMA Command Address register for Channel 22                                   | 5-35
        | 0x4000_0370 | DDADR23   | DMA Descriptor Address register for Channel 23                                | 5-32
        | 0x4000_0374 | DSADR23   | DMA Source Address register for Channel 23                                    | 5-33
        | 0x4000_0378 | DTADR23   | DMA Target Address register for Channel 23                                    | 5-34
        | 0x4000_037C | DCMD23    | DMA Command Address register for Channel 23                                   | 5-35
        | 0x4000_0380 | DDADR24   | DMA Descriptor Address register for Channel 24                                | 5-32
        | 0x4000_0384 | DSADR24   | DMA Source Address register for Channel 24                                    | 5-33
        | 0x4000_0388 | DTADR24   | DMA Target Address register for Channel 24                                    | 5-34
        | 0x4000_038C | DCMD24    | DMA Command Address register for Channel 24                                   | 5-35
        | 0x4000_0390 | DDADR25   | DMA Descriptor Address register for Channel 25                                | 5-32
        | 0x4000_0394 | DSADR25   | DMA Source Address register for Channel 25                                    | 5-33
        | 0x4000_0398 | DTADR25   | DMA Target Address register for Channel 25                                    | 5-34
        | 0x4000_039C | DCMD25    | DMA Command Address register for Channel 25                                   | 5-35
        | 0x4000_03A0 | DDADR26   | DMA Descriptor Address register for Channel 26                                | 5-32
        | 0x4000_03A4 | DSADR26   | DMA Source Address register for Channel 26                                    | 5-33
        | 0x4000_03A8 | DTADR26   | DMA Target Address register for Channel 26                                    | 5-34
        | 0x4000_03AC | DCMD26    | DMA Command Address register for Channel 26                                   | 5-35
        | 0x4000_03B0 | DDADR27   | DMA Descriptor Address register for Channel 27                                | 5-32
        | 0x4000_03B4 | DSADR27   | DMA Source Address register for Channel 27                                    | 5-33
        | 0x4000_03B8 | DTADR27   | DMA Target Address register for Channel 27                                    | 5-34
        | 0x4000_03BC | DCMD27    | DMA Command Address register for Channel 27                                   | 5-35
        | 0x4000_03C0 | DDADR28   | DMA Descriptor Address register for Channel 28                                | 5-32
        | 0x4000_03C4 | DSADR28   | DMA Source Address register for Channel 28                                    | 5-33
        | 0x4000_03C8 | DTADR28   | DMA Target Address register for Channel 28                                    | 5-34
        | 0x4000_03CC | DCMD28    | DMA Command Address register for Channel 28                                   | 5-35
        | 0x4000_03D0 | DDADR29   | DMA Descriptor Address register for Channel 29                                | 5-32
        | 0x4000_03D4 | DSADR29   | DMA Source Address register for Channel 29                                    | 5-33
        | 0x4000_03D8 | DTADR29   | DMA Target Address register for Channel 29                                    | 5-34
        | 0x4000_03DC | DCMD29    | DMA Command Address register for Channel 29                                   | 5-35
        | 0x4000_03E0 | DDADR30   | DMA Descriptor Address register for Channel 30                                | 5-32
        | 0x4000_03E4 | DSADR30   | DMA Source Address register for Channel 30                                    | 5-33
        | 0x4000_03E8 | DTADR30   | DMA Target Address register for Channel 30                                    | 5-34
        | 0x4000_03EC | DCMD30    | DMA Command Address register for Channel 30                                   | 5-35
        | 0x4000_03F0 | DDADR31   | DMA Descriptor Address register for Channel 31                                | 5-32
        | 0x4000_03F4 | DSADR31   | DMA Source Address register for Channel 31                                    | 5-33
        | 0x4000_03F8 | DTADR31   | DMA Target Address register for Channel 31                                    | 5-34
        | 0x4000_03FC | DCMD31    | DMA Command Address register for Channel 31                                   | 5-35
        | 0x4000_1100 | DRCMR64   | Request to Channel Map register for Memory Stick receive request              | 5-31
        | 0x4000_1104 | DRCMR65   | Request to Channel Map register for Memory Stick transmit request             | 5-31
        | 0x4000_1108 | DRCMR66   | Request to Channel Map register for SSP3 receive request                      | 5-31
        | 0x4000_110C | DRCMR67   | Request to Channel Map register for SSP3 transmit request                     | 5-31
        | 0x4000_1110 | DRCMR68   | Request to Channel Map register for Quick Capture Interface Receive Request 0 | 5-31
        | 0x4000_1114 | DRCMR69   | Request to Channel Map register for Quick Capture Interface Receive Request 1 | 5-31
        | 0x4000_1118 | DRCMR70   | Request to Channel Map register for Quick Capture Interface Receive Request 2 | 5-31
        | 0x4000_1128 | DRCMR74   | Request to Channel Map register for DREQ<2> (companion chip request 2)        | 5-31
        | 0x4800_0020 | FLYCNFG   | Fly-by DMA DVAL<1:0> polarities                                               | 5-39

        Full-Function UART
        | 0x4010_0000 | FFRBR     | Receive Buffer register                                                       | 10-13
        | 0x4010_0000 | FFTHR     | Transmit Holding register                                                     | 10-14
        | 0x4010_0000 | FFDLL     | Divisor Latch register, low byte                                              | 10-14
        | 0x4010_0004 | FFIER     | Interrupt Enable register                                                     | 10-15
        | 0x4010_0004 | FFDLH     | Divisor Latch register, high byte                                             | 10-14
        | 0x4010_0008 | FFIIR     | Interrupt ID register                                                         | 10-17
        | 0x4010_0008 | FFFCR     | FIFO Control register                                                         | 10-19
        | 0x4010_000C | FFLCR     | Line Control register                                                         | 10-25
        | 0x4010_0010 | FFMCR     | Modem Control register                                                        | 10-29
        | 0x4010_0014 | FFLSR     | Line Status register                                                          | 10-26
        | 0x4010_0018 | FFMSR     | Modem Status register                                                         | 10-31
        | 0x4010_001C | FFSPR     | Scratch Pad register                                                          | 10-33
        | 0x4010_0020 | FFISR     | Infrared Select register                                                      | 10-33
        | 0x4010_0024 | FFFOR     | Receive FIFO Occupancy register                                               | 10-22
        | 0x4010_0028 | FFABR     | Auto-baud Control register                                                    | 10-23
        | 0x4010_002C | FFACR     | Auto-baud Count register                                                      | 10-24

        Bluetooth UART
        | 0x4020_0000 | BTRBR     | Receive Buffer register                                                       | 10-13
        | 0x4020_0000 | BTTHR     | Transmit Holding register                                                     | 10-14
        | 0x4020_0000 | BTDLL     | Divisor Latch register, low byte                                              | 10-14
        | 0x4020_0004 | BTIER     | Interrupt Enable register                                                     | 10-15
        | 0x4020_0004 | BTDLH     | Divisor Latch register, high byte                                             | 10-14
        | 0x4020_0008 | BTIIR     | Interrupt ID register                                                         | 10-17
        | 0x4020_0008 | BTFCR     | FIFO Control register                                                         | 10-19
        | 0x4020_000C | BTLCR     | Line Control register                                                         | 10-25
        | 0x4020_0010 | BTMCR     | Modem Control register                                                        | 10-29
        | 0x4020_0014 | BTLSR     | Line Status register                                                          | 10-26
        | 0x4020_0018 | BTMSR     | Modem Status register                                                         | 10-31
        | 0x4020_001C | BTSPR     | Scratch Pad register                                                          | 10-33
        | 0x4020_0020 | BTISR     | Infrared Select register                                                      | 10-33
        | 0x4020_0024 | BTFOR     | Receive FIFO Occupancy register                                               | 10-22
        | 0x4020_0028 | BTABR     | Auto-Baud Control register                                                    | 10-23
        | 0x4020_002C | BTACR     | Auto-Baud Count register                                                      | 10-24

        Standard I2C
        | 0x4030_1680 | IBMR      | I2C Bus Monitor register                                                      | 9-30
        | 0x4030_1688 | IDBR      | I2C Data Buffer register                                                      | 9-29
        | 0x4030_1690 | ICR       | I2C Control register                                                          | 9-23
        | 0x4030_1698 | ISR       | I2C Status register                                                           | 9-26
        | 0x4030_16A0 | ISAR      | I2C Slave Address register                                                    | 9-28

        I2S Controller
        | 0x4040_0000 | SACR0     | Serial Audio Global Control register                                          | 14-10
        | 0x4040_0004 | SACR1     | Serial Audio I2S/MSB-Justified Control register                               | 14-13
        | 0x4040_000C | SASR0     | Serial Audio I2S/MSB-Justified Interface and FIFO Status register             | 14-14
        | 0x4040_0014 | SAIMR     | Serial Audio Interrupt Mask register                                          | 14-18
        | 0x4040_0018 | SAICR     | Serial Audio Interrupt Clear register                                         | 14-17
        | 0x4040_0060 | SADIV     | Audio Clock Divider register                                                  | 14-16
        | 0x4040_0080 | SADR      | Serial Audio Data register (TX and RX FIFO access register).                  | 14-18

        AC ’97 Controller
        | 0x4050_0000 | POCR      | PCM Out Control register                                                      | 13-27
        | 0x4050_0004 | PCMICR    | PCM In Control register                                                       | 13-28
        | 0x4050_0008 | MCCR      | Microphone In Control register                                                | 13-33
        | 0x4050_000C | GCR       | Global Control register                                                       | 13-22
        | 0x4050_0010 | POSR      | PCM Out Status register                                                       | 13-29
        | 0x4050_0014 | PCMISR    | PCM In Status register                                                        | 13-30
        | 0x4050_0018 | MCSR      | MIC In Status register                                                        | 13-34
        | 0x4050_001C | GSR       | Global Status register                                                        | 13-24
        | 0x4050_0020 | CAR       | Codec Access register                                                         | 13-31
        | 0x4050_0040 | PCDR      | PCM Data register                                                             | 13-32
        | 0x4050_0060 | MCDR      | MIC In Data register                                                          | 13-35
        | 0x4050_0100 | MOCR      | Modem Out Control register                                                    | 13-36
        | 0x4050_0108 | MICR      | Modem In Control register                                                     | 13-37
        | 0x4050_0110 | MOSR      | Modem Out Status register                                                     | 13-38
        | 0x4050_0118 | MISR      | Modem In Status register                                                      | 13-39
        | 0x4050_0140 | MODR      | Modem Data register                                                           | 13-40
        | 0x4050_0200–
          0x4050_02FC | with all in increments of 0x00004 — Primary Audio Codec registers                         | 13-41
        | 0x4050_0300–
          0x4050_03FC | with all in increments of 0x00004 — Secondary Audio Codec registers                       | 13-41
        | 0x4050_0400–
          0x4050_04FC | with all in increments of 0x0000_0004 — Primary Modem Codec registers                     | 13-41
        | 0x4050_0500–
          0x4050_05FC | with all in increments of 0x00004 — Secondary Modem Codec registers                       | 13-41

        USB Client Controller
        | 0x4060_0000 | UDCCR     | UDC Control register                                                          | 12-31
        | 0x4060_0004 | UDCICR0   | UDC Interrupt Control register 0                                              | 12-35
        | 0x4060_0008 | UDCCIR1   | UDC Interrupt Control register 1                                              | 12-35
        | 0x4060_000C | UDCISR0   | UDC Interrupt Status register 0                                               | 12-49
        | 0x4060_0010 | UDCSIR1   | UDC Interrupt Status register 1                                               | 12-49
        | 0x4060_0014 | UDCFNR    | UDC Frame Number register                                                     | 12-52
        | 0x4060_0018 | UDCOTGICR | UDC OTG Interrupt Control register                                            | 12-35
        | 0x4060_001C | UDCOTGISR | UDC OTG Interrupt Status register                                             | 12-49
        | 0x4060_0020 | UP2OCR    | USB Port 2 Output Control register                                            | 12-41
        | 0x4060_0024 | UP3OCR    | USB Port 3 Output Control register                                            | 12-47
        | 0x4060_0100 | UDCCSR0   | UDC Control/Status register—Endpoint 0                                        | 12-53
        | 0x4060_0104 | UDCCSRA   | UDC Control/Status register—Endpoint A                                        | 12-56
        | 0x4060_0108 | UDCCSRB   | UDC Control/Status register—Endpoint B                                        | 12-56
        | 0x4060_010C | UDCCSRC   | UDC Control/Status register—Endpoint C                                        | 12-56
        | 0x4060_0110 | UDCCSRD   | UDC Control/Status register—Endpoint D                                        | 12-56
        | 0x4060_0114 | UDCCSRE   | UDC Control/Status register—Endpoint E                                        | 12-56
        | 0x4060_0118 | UDCCSRF   | UDC Control/Status register—Endpoint F                                        | 12-56
        | 0x4060_011C | UDCCSRG   | UDC Control/Status register—Endpoint G                                        | 12-56
        | 0x4060_0120 | UDCCSRH   | UDC Control/Status register—Endpoint H                                        | 12-56
        | 0x4060_0124 | UDCCSRI   | UDC Control/Status register—Endpoint I                                        | 12-56
        | 0x4060_0128 | UDCCSRJ   | UDC Control/Status register—Endpoint J                                        | 12-56
        | 0x4060_012C | UDCCSRK   | UDC Control/Status register—Endpoint K                                        | 12-56
        | 0x4060_0130 | UDCCSRL   | UDC Control/Status register—Endpoint L                                        | 12-56
        | 0x4060_0134 | UDCCSRM   | UDC Control/Status register—Endpoint M                                        | 12-56
        | 0x4060_0138 | UDCCSRN   | UDC Control/Status register—Endpoint N                                        | 12-56
        | 0x4060_013C | UDCCSRP   | UDC Control/Status register—Endpoint P                                        | 12-56
        | 0x4060_0140 | UDCCSRQ   | UDC Control/Status register—Endpoint Q                                        | 12-56
        | 0x4060_0144 | UDCCSRR   | UDC Control/Status register—Endpoint R                                        | 12-56
        | 0x4060_0148 | UDCCSRS   | UDC Control/Status register—Endpoint S                                        | 12-56
        | 0x4060_014C | UDCCSRT   | UDC Control/Status register—Endpoint T                                        | 12-56
        | 0x4060_0150 | UDCCSRU   | UDC Control/Status register—Endpoint U                                        | 12-56
        | 0x4060_0154 | UDCCSRV   | UDC Control/Status register—Endpoint V                                        | 12-56
        | 0x4060_0158 | UDCCSRW   | UDC Control/Status register—Endpoint W                                        | 12-56
        | 0x4060_015C | UDCCSRX   | UDC Control/Status register—Endpoint X                                        | 12-56
        | 0x4060_0200 | UDCBCR0   | UDC Byte Count register—Endpoint 0                                            | 12-62
        | 0x4060_0204 | UDCBCRA   | UDC Byte Count register—Endpoint A                                            | 12-62
        | 0x4060_0208 | UDCBCRB   | UDC Byte Count register—Endpoint B                                            | 12-62
        | 0x4060_020C | UDCBCRC   | UDC Byte Count register—Endpoint C                                            | 12-62
        | 0x4060_0210 | UDCBCRD   | UDC Byte Count register—Endpoint D                                            | 12-62
        | 0x4060_0214 | UDCBCRE   | UDC Byte Count register—Endpoint E                                            | 12-62
        | 0x4060_0218 | UDCBCRF   | UDC Byte Count register—Endpoint F                                            | 12-62
        | 0x4060_021C | UDCBCRG   | UDC Byte Count register—Endpoint G                                            | 12-62
        | 0x4060_0220 | UDCBCRH   | UDC Byte Count register—Endpoint H                                            | 12-62
        | 0x4060_0224 | UDCBCRI   | UDC Byte Count register—Endpoint I                                            | 12-62
        | 0x4060_0228 | UDCBCRJ   | UDC Byte Count register—Endpoint J                                            | 12-62
        | 0x4060_022C | UDCBCRK   | UDC Byte Count register—Endpoint K                                            | 12-62
        | 0x4060_0230 | UDCBCRL   | UDC Byte Count register—Endpoint L                                            | 12-62
        | 0x4060_0234 | UDCBCRM   | UDC Byte Count register—Endpoint M                                            | 12-62
        | 0x4060_0238 | UDCBCRN   | UDC Byte Count register—Endpoint N                                            | 12-62
        | 0x4060_023C | UDCBCRP   | UDC Byte Count register—Endpoint P                                            | 12-62
        | 0x4060_0240 | UDCBCRQ   | UDC Byte Count register—Endpoint Q                                            | 12-62
        | 0x4060_0244 | UDCBCRR   | UDC Byte Count register—Endpoint R                                            | 12-62
        | 0x4060_0248 | UDCBCRS   | UDC Byte Count register—Endpoint S                                            | 12-62
        | 0x4060_024C | UDCBCRT   | UDC Byte Count register—Endpoint T                                            | 12-62
        | 0x4060_0250 | UDCBCRU   | UDC Byte Count register—Endpoint U                                            | 12-62
        | 0x4060_0254 | UDCBCRV   | UDC Byte Count register—Endpoint V                                            | 12-62
        | 0x4060_0258 | UDCBCRW   | UDC Byte Count register—Endpoint W                                            | 12-62
        | 0x4060_025C | UDCBCRX   | UDC Byte Count register—Endpoint X                                            | 12-62
        | 0x4060_0300 | UDCDR0    | UDC Data register—Endpoint 0                                                  | 12-62
        | 0x4060_0304 | UDCDRA    | UDC Data register—Endpoint A                                                  | 12-62
        | 0x4060_0308 | UDCDRB    | UDC Data register—Endpoint B                                                  | 12-62
        | 0x4060_030C | UDCDRC    | UDC Data register—Endpoint C                                                  | 12-62
        | 0x4060_0310 | UDCDRD    | UDC Data register—Endpoint D                                                  | 12-62
        | 0x4060_0314 | UDCDRE    | UDC Data register—Endpoint E                                                  | 12-62
        | 0x4060_0318 | UDCDRF    | UDC Data register—Endpoint F                                                  | 12-62
        | 0x4060_031C | UDCDRG    | UDC Data register—Endpoint G                                                  | 12-62
        | 0x4060_0320 | UDCDRH    | UDC Data register—Endpoint H                                                  | 12-62
        | 0x4060_0324 | UDCDRI    | UDC Data register—Endpoint I                                                  | 12-62
        | 0x4060_0328 | UDCDRJ    | UDC Data register—Endpoint J                                                  | 12-62
        | 0x4060_032C | UDCDRK    | UDC Data register—Endpoint K                                                  | 12-62
        | 0x4060_0330 | UDCDRL    | UDC Data register—Endpoint L                                                  | 12-62
        | 0x4060_0334 | UDCDRM    | UDC Data register—Endpoint M                                                  | 12-62
        | 0x4060_0338 | UDCDRN    | UDC Data register—Endpoint N                                                  | 12-62
        | 0x4060_033C | UDCDRP    | UDC Data register—Endpoint P                                                  | 12-62
        | 0x4060_0340 | UDCDRQ    | UDC Data register—Endpoint Q                                                  | 12-62
        | 0x4060_0344 | UDCDRR    | UDC Data register—Endpoint R                                                  | 12-62
        | 0x4060_0348 | UDCDRS    | UDC Data register—Endpoint S                                                  | 12-62
        | 0x4060_034C | UDCDRT    | UDC Data register—Endpoint T                                                  | 12-62
        | 0x4060_0350 | UDCDRU    | UDC Data register—Endpoint U                                                  | 12-62
        | 0x4060_0354 | UDCDRV    | UDC Data register—Endpoint V                                                  | 12-62
        | 0x4060_0358 | UDCDRW    | UDC Data register—Endpoint W                                                  | 12-62
        | 0x4060_035C | UDCDRX    | UDC Data register—Endpoint X                                                  | 12-62
        | 0x4060_0404 | UDCCRA    | UDC Configuration register—Endpoint A                                         | 12-64
        | 0x4060_0408 | UDCCRB    | UDC Configuration register—Endpoint B                                         | 12-64
        | 0x4060_040C | UDCCRC    | UDC Configuration register—Endpoint C                                         | 12-64
        | 0x4060_0410 | UDCCRD    | UDC Configuration register—Endpoint D                                         | 12-64
        | 0x4060_0414 | UDCCRE    | UDC Configuration register—Endpoint E                                         | 12-64
        | 0x4060_0418 | UDCCRF    | UDC Configuration register—Endpoint F                                         | 12-64
        | 0x4060_041C | UDCCRG    | UDC Configuration register—Endpoint G                                         | 12-64
        | 0x4060_0420 | UDCCRH    | UDC Configuration register—Endpoint H                                         | 12-64
        | 0x4060_0424 | UDCCRI    | UDC Configuration register—Endpoint I                                         | 12-64
        | 0x4060_0428 | UDCCRJ    | UDC Configuration register—Endpoint J                                         | 12-64
        | 0x4060_042C | UDCCRK    | UDC Configuration register—Endpoint K                                         | 12-64
        | 0x4060_0430 | UDCCRL    | UDC Configuration register—Endpoint L                                         | 12-64
        | 0x4060_0434 | UDCCRM    | UDC Configuration register—Endpoint M                                         | 12-64
        | 0x4060_0438 | UDCCRN    | UDC Configuration register—Endpoint N                                         | 12-64
        | 0x4060_043C | UDCCRP    | UDC Configuration register—Endpoint P                                         | 12-64
        | 0x4060_0440 | UDCCRQ    | UDC Configuration register—Endpoint Q                                         | 12-64
        | 0x4060_0444 | UDCCRR    | UDC Configuration register—Endpoint R                                         | 12-64
        | 0x4060_0448 | UDCCRS    | UDC Configuration register—Endpoint S                                         | 12-64
        | 0x4060_044C | UDCCRT    | UDC Configuration register—Endpoint T                                         | 12-64
        | 0x4060_0450 | UDCCRU    | UDC Configuration register—Endpoint U                                         | 12-64
        | 0x4060_0454 | UDCCRV    | UDC Configuration register—Endpoint V                                         | 12-64
        | 0x4060_0458 | UDCCRW    | UDC Configuration register—Endpoint W                                         | 12-64
        | 0x4060_045C | UDCCRX    | UDC Configuration register—Endpoint X                                         | 12-64

        Standard UART
        | 0x4070_0000 | STRBR     | Receive Buffer register                                                       | 10-13
        | 0x4070_0000 | STTHR     | Transmit Holding register                                                     | 10-14
        | 0x4070_0000 | STDLL     | Divisor Latch register, low byte                                              | 10-14
        | 0x4070_0004 | STIER     | Interrupt Enable register                                                     | 10-15
        | 0x4070_0004 | STDLH     | Divisor Latch register, high byte                                             | 10-14
        | 0x4070_0008 | STIIR     | Interrupt ID register                                                         | 10-17
        | 0x4070_0008 | STFCR     | FIFO Control register                                                         | 10-19
        | 0x4070_000C | STLCR     | Line Control register                                                         | 10-25
        | 0x4070_0010 | STMCR     | Modem Control register                                                        | 10-29
        | 0x4070_0014 | STLSR     | Line Status register                                                          | 10-26
        | 0x4070_0018 | STMSR     | Modem Status register                                                         | 10-31
        | 0x4070_001C | STSPR     | Scratch Pad register                                                          | 10-33
        | 0x4070_0020 | STISR     | Infrared Select register                                                      | 10-33
        | 0x4070_0024 | STFOR     | Receive FIFO Occupancy register                                               | 10-22
        | 0x4070_0028 | STABR     | Auto-Baud Control register                                                    | 10-23
        | 0x4070_002C | STACR     | Auto-Baud Count register                                                      | 10-24
        
        Infrared Communications Port
        | 0x4080_0000 | ICCR0     | FICP Control register 0                                                       | 11-10
        | 0x4080_0004 | ICCR1     | FICP Control register 1                                                       | 11-13
        | 0x4080_0008 | ICCR2     | FICP Control register 2                                                       | 11-14
        | 0x4080_000C | ICDR      | FICP Data register                                                            | 11-15
        | 0x4080_0014 | ICSR0     | FICP Status register 0                                                        | 11-16
        | 0x4080_0018 | ICSR1     | FICP Status register 1                                                        | 11-18
        | 0x4080_001C | ICFOR     | FICP FIFO Occupancy Status register                                           | 11-19
        
        Real-Time Clock
        | 0x4090_0000 | RCNR      | RTC Counter register                                                          | 21-24
        | 0x4090_0004 | RTAR      | RTC Alarm register                                                            | 21-19
        | 0x4090_0008 | RTSR      | RTC Status register                                                           | 21-17
        | 0x4090_000C | RTTR      | RTC Timer Trim register                                                       | 21-16
        | 0x4090_0010 | RDCR      | RTC Day Counter register                                                      | 21-24
        | 0x4090_0014 | RYCR      | RTC Year Counter register                                                     | 21-25
        | 0x4090_0018 | RDAR1     | RTC Wristwatch Day Alarm register 1                                           | 21-20
        | 0x4090_001C | RYAR1     | RTC Wristwatch Year Alarm register 1                                          | 21-21
        | 0x4090_0020 | RDAR2     | RTC Wristwatch Day Alarm register 2                                           | 21-20
        | 0x4090_0024 | RYAR2     | RTC Wristwatch Year Alarm register 2                                          | 21-21
        | 0x4090_0028 | SWCR      | RTC Stopwatch Counter register                                                | 21-26
        | 0x4090_002C | SWAR1     | RTC Stopwatch Alarm register 1                                                | 21-22
        | 0x4090_0030 | SWAR2     | RTC Stopwatch Alarm register 2                                                | 21-22
        | 0x4090_0034 | RTCPICR   | RTC Periodic Interrupt Counter register                                       | 21-27
        | 0x4090_0038 | PIAR      | RTC Periodic Interrupt Alarm register                                         | 21-23

        OS Timers
        | 0x40A0_0000 | OSMR0     | OS Timer Match 0 register                                                     | 22-15
        | 0x40A0_0004 | OSMR1     | OS Timer Match 1 register                                                     | 22-15
        | 0x40A0_0008 | OSMR2     | OS Timer Match 2 register                                                     | 22-15
        | 0x40A0_000C | OSMR3     | OS Timer Match 3 register                                                     | 22-15
        | 0x40A0_0010 | OSCR0     | OS Timer Counter 0 register                                                   | 22-17
        | 0x40A0_0014 | OSSR      | OS Timer Status register (used for all counters)                              | 22-18
        | 0x40A0_0018 | OWER      | OS Timer Watchdog Enable register                                             | 22-16
        | 0x40A0_001C | OIER      | OS Timer Interrupt Enable register (used for all counters)                    | 22-16
        | 0x40A0_0020 | OSNR      | OS Timer Snapshot register                                                    | 22-19
        | 0x40A0_0040 | OSCR4     | OS Timer Counter 4 register                                                   | 22-17
        | 0x40A0_0044 | OSCR5     | OS Timer Counter 5 register                                                   | 22-17
        | 0x40A0_0048 | OSCR6     | OS Timer Counter 6 register                                                   | 22-17
        | 0x40A0_004C | OSCR7     | OS Timer Counter 7 register                                                   | 22-17
        | 0x40A0_0050 | OSCR8     | OS Timer Counter 8 register                                                   | 22-17
        | 0x40A0_0054 | OSCR9     | OS Timer Counter 9 register                                                   | 22-17
        | 0x40A0_0058 | OSCR10    | OS Timer Counter 10 register                                                  | 22-17
        | 0x40A0_005C | OSCR11    | OS Timer Counter 11 register                                                  | 22-17
        | 0x40A0_0080 | OSMR4     | OS Timer Match 4 register                                                     | 22-15
        | 0x40A0_0084 | OSMR5     | OS Timer Match 5 register                                                     | 22-15
        | 0x40A0_0088 | OSMR6     | OS Timer Match 6 register                                                     | 22-15
        | 0x40A0_008C | OSMR7     | OS Timer Match 7 register                                                     | 22-15
        | 0x40A0_0090 | OSMR8     | OS Timer Match 8 register                                                     | 22-15
        | 0x40A0_0094 | OSMR9     | OS Timer Match 9 register                                                     | 22-15
        | 0x40A0_0098 | OSMR10    | OS Timer Match 10 register                                                    | 22-15
        | 0x40A0_009C | OSMR11    | OS Timer Match 11 register                                                    | 22-15
        | 0x40A0_00C0 | OMCR4     | OS Match Control 4 register                                                   | 22-9
        | 0x40A0_00C4 | OMCR5     | OS Match Control 5 register                                                   | 22-9
        | 0x40A0_00C8 | OMCR6     | OS Match Control 6 register                                                   | 22-9
        | 0x40A0_00CC | OMCR7     | OS Match Control 7 register                                                   | 22-9
        | 0x40A0_00D0 | OMCR8     | OS Match Control 8 register                                                   | 22-11
        | 0x40A0_00D4 | OMCR9     | OS Match Control 9 register                                                   | 22-13
        | 0x40A0_00D8 | OMCR10    | OS Match Control 10 register                                                  | 22-11
        | 0x40A0_00DC | OMCR11    | OS Match Control 11 register                                                  | 22-13

        Pulse-Width Modulation
        | 0x40B0_0000 | PWMCR0    | PWM 0 Control register                                                        | 23-7
        | 0x40B0_0004 | PWMDCR0   | PWM 0 Duty Cycle register                                                     | 23-8
        | 0x40B0_0008 | PWMPCR0   | PWM 0 Period register                                                         | 23-9
        | 0x40B0_0010 | PWMCR2    | PWM 2 Control register                                                        | 23-7
        | 0x40B0_0014 | PWMDCR2   | PWM 2 Duty Cycle register                                                     | 23-8
        | 0x40B0_0018 | PWMPCR2   | PWM 2 Period register                                                         | 23-9
        | 0x40C0_0000 | PWMCR1    | PWM 1 Control register                                                        | 23-7
        | 0x40C0_0004 | PWMDCR1   | PWM 1 Duty Cycle register                                                     | 23-8
        | 0x40C0_0008 | PWMPCR1   | PWM 1 Period register                                                         | 23-9
        | 0x40C0_0010 | PWMCR3    | PWM 3 Control register                                                        | 23-7
        | 0x40C0_0014 | PWMDCR3   | PWM 3 Duty Cycle register                                                     | 23-8
        | 0x40C0_0018 | PWMPCR3   | PWM 3 Period register                                                         | 23-9

        Interrupt Controller
        | 0x40D0_0000 | ICIP      | Interrupt Controller IRQ Pending register                                     | 25-11
        | 0x40D0_0004 | ICMR      | Interrupt Controller Mask register                                            | 25-20
        | 0x40D0_0008 | ICLR      | Interrupt Controller Level register                                           | 25-24
        | 0x40D0_000C | ICFP      | Interrupt Controller FIQ Pending register                                     | 25-15
        | 0x40D0_0010 | ICPR      | Interrupt Controller Pending register                                         | 25-6
        | 0x40D0_0014 | ICCR      | Interrupt Controller Control register                                         | 25-27
        | 0x40D0_0018 | ICHP      | Interrupt Controller Highest Priority register                                | 25-30
        | 0x40D0_001C–  IPR0–
          0x40D0_0098 | IPR31     | Interrupt Priority registers for Priorities 0–31                              | 25-29
        | 0x40D0_009C | ICIP2     | Interrupt Controller IRQ Pending register 2                                   | 25-10
        | 0x40D0_00A0 | ICMR2     | Interrupt Controller Mask register 2                                          | 25-23
        | 0x40D0_00A4 | ICLR2     | Interrupt Controller Level register 2                                         | 25-27
        | 0x40D0_00A8 | ICFP2     | Interrupt Controller FIQ Pending register 2                                   | 25-19
        | 0x40D0_00AC | ICPR2     | Interrupt Controller Pending register 2                                       | 25-6
        | 0x40D0_00B0–  IPR32–
          0x40D0_00CC | IPR39     |Interrupt Priority registers for Priorities 32–39                             | 25-29

        General-Purpose I/O (GPIO) Controller
        | 0x40E0_0000 | GPLR0     | GPIO Pin-Level register GPIO<31:0>                                            | 24-28
        | 0x40E0_0004 | GPLR1     | GPIO Pin-Level register GPIO<63:32>                                           | 24-28
        | 0x40E0_0008 | GPLR2     | GPIO Pin-Level register GPIO<95:64>                                           | 24-28
        | 0x40E0_000C | GPDR0     | GPIO Pin Direction register GPIO<31:0>                                        | 24-11
        | 0x40E0_0010 | GPDR1     | GPIO Pin Direction register GPIO<63:32>                                       | 24-11
        | 0x40E0_0014 | GPDR2     | GPIO Pin Direction register GPIO<95:64>                                       | 24-11
        | 0x40E0_0018 | GPSR0     | GPIO Pin Output Set register GPIO<31:0>                                       | 24-14
        | 0x40E0_001C | GPSR1     | GPIO Pin Output Set register GPIO<63:32>                                      | 24-14
        | 0x40E0_0020 | GPSR2     | GPIO Pin Output Set register GPIO<95:64>                                      | 24-14
        | 0x40E0_0024 | GPCR0     | GPIO Pin Output Clear register GPIO<31:0>                                     | 24-14
        | 0x40E0_0028 | GPCR1     | GPIO Pin Output Clear register GPIO <63:32>                                   | 24-14
        | 0x40E0_002C | GPCR2     | GPIO pin Output Clear register GPIO <95:64>                                   | 24-14
        | 0x40E0_0030 | GRER0     | GPIO Rising-Edge Detect Enable register GPIO<31:0>                            | 24-18
        | 0x40E0_0034 | GRER1     | GPIO Rising-Edge Detect Enable register GPIO<63:32>                           | 24-18
        | 0x40E0_0038 | GRER2     | GPIO Rising-Edge Detect Enable register GPIO<95:64>                           | 24-18
        | 0x40E0_003C | GFER0     | GPIO Falling-Edge Detect Enable register GPIO<31:0>                           | 24-18
        | 0x40E0_0040 | GFER1     | GPIO Falling-Edge Detect Enable register GPIO<63:32>                          | 24-18
        | 0x40E0_0044 | GFER2     | GPIO Falling-Edge Detect Enable register GPIO<95:64>                          | 24-18
        | 0x40E0_0048 | GEDR0     | GPIO Edge Detect Status register GPIO<31:0>                                   | 24-30
        | 0x40E0_004C | GEDR1     | GPIO Edge Detect Status register GPIO<63:32>                                  | 24-30
        | 0x40E0_0050 | GEDR2     | GPIO Edge Detect Status register GPIO<95:64>                                  | 24-30
        | 0x40E0_0054 | GAFR0_L   | GPIO Alternate Function register GPIO<15:0>                                   | 24-23
        | 0x40E0_0058 | GAFR0_U   | GPIO Alternate Function register GPIO<31:16>                                  | 24-23
        | 0x40E0_005C | GAFR1_L   | GPIO Alternate Function register GPIO<47:32>                                  | 24-23
        | 0x40E0_0060 | GAFR1_U   | GPIO Alternate Function register GPIO<63:48>                                  | 24-23
        | 0x40E0_0064 | GAFR2_L   | GPIO Alternate Function register GPIO<79:64>                                  | 24-23
        | 0x40E0_0068 | GAFR2_U   | GPIO Alternate Function register GPIO <95:80>                                 | 24-23
        | 0x40E0_006C | GAFR3_L   | GPIO Alternate Function register GPIO<111:96>                                 | 24-23
        | 0x40E0_0070 | GAFR3_U   | GPIO Alternate Function register GPIO<120:112>                                | 24-23
        | 0x40E0_0100 | GPLR3     | GPIO Pin-Level register GPIO<120:96>                                          | 24-28
        | 0x40E0_010C | GPDR3     | GPIO Pin Direction register GPIO<120:96>                                      | 24-11
        | 0x40E0_0118 | GPSR3     | GPIO Pin Output Set register GPIO<120:96>                                     | 24-14
        | 0x40E0_0124 | GPCR3     | GPIO Pin Output Clear register GPIO<120:96>                                   | 24-14
        | 0x40E0_0130 | GRER3     | GPIO Rising-Edge Detect Enable register GPIO<120:96>                          | 24-18
        | 0x40E0_013C | GFER3     | GPIO Falling-Edge Detect Enable register GPIO<120:96>                         | 24-18
        | 0x40E0_0148 | GEDR3     | GPIO Edge Detect Status register GPIO<120:96>                                 | 24-18

        Power Manager and Reset Control
        | 0x40F0_0000 | PMCR      | Power Manager Control register                                                | 3-67
        | 0x40F0_0004 | PSSR      | Power Manager Sleep Status register                                           | 3-69
        | 0x40F0_0008 | PSPR      | Power Manager Scratch Pad register                                            | 3-72
        | 0x40F0_000C | PWER      | Power Manager Wake-Up Enable register                                         | 3-73
        | 0x40F0_0010 | PRER      | Power Manager Rising-Edge Detect Enable register                              | 3-77
        | 0x40F0_0014 | PFER      | Power Manager Falling-Edge Detect Enable register                             | 3-78
        | 0x40F0_0018 | PEDR      | Power Manager Edge-Detect Status register                                     | 3-79
        | 0x40F0_001C | PCFR      | Power Manager General Configuration register                                  | 3-80
        | 0x40F0_0020 | PGSR0     | Power Manager GPIO Sleep State register for GPIO<31:0>                        | 3-83
        | 0x40F0_0024 | PGSR1     | Power Manager GPIO Sleep State register for GPIO<63:32>                       | 3-83
        | 0x40F0_0028 | PGSR2     | Power Manager GPIO Sleep State register for GPIO<95:64>                       | 3-83
        | 0x40F0_002C | PGSR3     | Power Manager GPIO Sleep State register for GPIO<120:96>                      | 3-83
        | 0x40F0_0030 | RCSR      | Reset Controller Status register                                              | 3-84
        | 0x40F0_0034 | PSLR      | Power Manager Sleep Configuration register                                    | 3-85
        | 0x40F0_0038 | PSTR      | Power Manager Standby Configuration register                                  | 3-88
        | 0x40F0_0040 | PVCR      | Power Manager Voltage Change Control register                                 | 3-89
        | 0x40F0_004C | PUCR      | Power Manager USIM Card Control/Status register                               | 3-90
        | 0x40F0_0050 | PKWR      | Power Manager Keyboard Wake-Up Enable register                                | 3-92
        | 0x40F0_0054 | PKSR      | Power Manager Keyboard Level-Detect Status register                           | 3-93
        | 0x40F0_0080–  PCMD0–
          0x40F0_00FC | PCMD31    | Power Manager I2C Command register File                                       | 3-94

        Power Manager I2C
        | 0x40F0_0180 | PIBMR     | Power Manager I2C Bus Monitor register                                        | 9-30
        | 0x40F0_0188 | PIDBR     | Power Manager I2C Data Buffer register                                        | 9-29
        | 0x40F0_0190 | PICR      | Power Manager I2C Control register                                            | 9-23
        | 0x40F0_0198 | PISR      | Power Manager I2C Status register                                             | 9-26
        | 0x40F0_01A0 | PISAR     | Power Manager I2C Slave Address register                                      | 9-28
        
        Synchronous Serial Port 1
        | 0x4100_0000 | SSCR0_1   | SSP 1 Control register 0                                                      | 8-25
        | 0x4100_0004 | SSCR1_1   | SSP 1 Control register 1                                                      | 8-30
        | 0x4100_0008 | SSSR_1    | SSP 1 Status register                                                         | 8-43
        | 0x4100_000C | SSITR_1   | SSP 1 Interrupt Test register                                                 | 8-42
        | 0x4100_0010 | SSDR_1    | SSP 1 Data Write register/Data Read register                                  | 8-48
        | 0x4100_0028 | SSTO_1    | SSP 1 Time-Out register                                                       | 8-41
        | 0x4100_002C | SSPSP_1   | SSP 1 Programmable Serial Protocol                                            | 8-39
        | 0x4100_0030 | SSTSA_1   | SSP1 TX Timeslot Active register                                              | 8-48
        | 0x4100_0034 | SSRSA_1   | SSP1 RX Timeslot Active register                                              | 8-49
        | 0x4100_0038 | SSTSS_1   | SSP1 Timeslot Status register                                                 | 8-50
        | 0x4100_003C | SSACD_1   | SSP1 Audio Clock Divider register                                             | 8-51
        
        MultiMediaCard/SD/SDIO Controller
        | 0x4110_0000 | MMC_STRPCL| MMC Clock Start/Stop register                                                 | 15-29
        | 0x4110_0004 | MMC_STAT  | MMC Status register                                                           | 15-29
        | 0x4110_0008 | MMC_CLKRT | MMC Clock Rate register                                                       | 15-31
        | 0x4110_000C | MMC_SPI   | MMC SPI Mode register                                                         | 15-31
        | 0x4110_0010 | MMC_CMDAT | MMC Command/Data register                                                     | 15-32
        | 0x4110_0014 | MMC_RESTO | MMC Response Time-Out register                                                | 15-34
        | 0x4110_0018 | MMC_RDTO  | MMC Read Time-Out register                                                    | 15-34
        | 0x4110_001C | MMC_BLKLEN| MMC Block Length register                                                     | 15-35
        | 0x4110_0020 | MMC_NUMBLK| MMC Number of Blocks register                                                 | 15-35
        | 0x4110_0024 | MMC_PRTBUF| MMC Buffer Partly Full register                                               | 15-36
        | 0x4110_0028 | MMC_I_MASK| MMC Interrupt Mask register                                                   | 15-36
        | 0x4110_002C | MMC_I_REG | MMC Interrupt Request register                                                | 15-38
        | 0x4110_0030 | MMC_CMD   | MMC Command register                                                          | 15-41
        | 0x4110_0034 | MMC_ARGH  | MMC Argument High register                                                    | 15-41
        | 0x4110_0038 | MMC_ARGL  | MMC Argument Low register                                                     | 15-42
        | 0x4110_003C | MMC_RES   | MMC Response FIFO                                                             | 15-42
        | 0x4110_0040 | MMC_RXFIFO| MMC Receive FIFO                                                              | 15-42
        | 0x4110_0044 | MMC_TXFIFO| MMC Transmit FIFO                                                             | 15-43
        | 0x4110_0048 | MMC_RDWAIT| MMC RD_WAIT register                                                          | 15-43
        | 0x4110_004C | MMC_BLKS_REM | MMC Blocks Remaining register                                              | 15-44
        
        Clocks Manager
        | 0x4130_0000 | CCCR      | Core Clock Configuration register                                             | 3-95
        | 0x4130_0004 | CKEN      | Clock Enable register                                                         | 3-98
        | 0x4130_0008 | OSCC      | Oscillator Configuration register                                             | 3-99
        | 0x4130_000C | CCSR      | Core Clock Status register                                                    | 3-101
        
        Mobile Scalable Link (MSL) Interface
        | 0x4140_0004 | BBFIFO1   | MSL Channel 1 Receive/Transmit FIFO register                                  | 16-13
        | 0x4140_0008 | BBFIFO2   | MSL Channel 2 Receive/Transmit FIFO register                                  | 16-13
        | 0x4140_000C | BBFIFO3   | MSL Channel 3 Receive/Transmit FIFO register                                  | 16-13
        | 0x4140_0010 | BBFIFO4   | MSL Channel 4 Receive/Transmit FIFO register                                  | 16-13
        | 0x4140_0014 | BBFIFO5   | MSL Channel 5 Receive/Transmit FIFO register                                  | 16-13
        | 0x4140_0018 | BBFIFO6   | MSL Channel 6 Receive/Transmit FIFO register                                  | 16-13
        | 0x4140_001C | BBFIFO7   | MSL Channel 7 Receive/Transmit FIFO register                                  | 16-13
        | 0x4140_0044 | BBCFG1    | MSL Channel 1 Configuration register                                          | 16-15
        | 0x4140_0048 | BBCFG2    | MSL Channel 2 Configuration register                                          | 16-15
        | 0x4140_004C | BBCFG3    | MSL Channel 3 Configuration register                                          | 16-15
        | 0x4140_0050 | BBCFG4    | MSL Channel 4 Configuration register                                          | 16-15
        | 0x4140_0054 | BBCFG5    | MSL Channel 5 Configuration register                                          | 16-15
        | 0x4140_0058 | BBCFG6    | MSL Channel 6 Configuration register                                          | 16-15
        | 0x4140_005C | BBCFG7    | MSL Channel 7 Configuration register                                          | 16-15
        | 0x4140_0084 | BBSTAT1   | MSL Channel 1 Status register                                                 | 16-19
        | 0x4140_0088 | BBSTAT2   | MSL Channel 2 Status register                                                 | 16-19
        | 0x4140_008C | BBSTAT3   | MSL Channel 3 Status register                                                 | 16-19
        | 0x4140_0090 | BBSTAT4   | MSL Channel 4 Status register                                                 | 16-19
        | 0x4140_0094 | BBSTAT5   | MSL Channel 5 Status register                                                 | 16-19
        | 0x4140_0098 | BBSTAT6   | MSL Channel 6 Status register                                                 | 16-19
        | 0x4140_009C | BBSTAT7   | MSL Channel 7 Status register                                                 | 16-19
        | 0x4140_00C4 | BBEOM1    | MSL Channel 1 EOM register                                                    | 16-22
        | 0x4140_00C8 | BBEOM2    | MSL Channel 2 EOM register                                                    | 16-22
        | 0x4140_00CC | BBEOM3    | MSL Channel 3 EOM register                                                    | 16-22
        | 0x4140_00D0 | BBEOM4    | MSL Channel 4 EOM register                                                    | 16-22
        | 0x4140_00D4 | BBEOM5    | MSL Channel 5 EOM register                                                    | 16-22
        | 0x4140_00D8 | BBEOM6    | MSL Channel 6 EOM register                                                    | 16-22
        | 0x4140_00DC | BBEOM7    | MSL Channel 7 EOM register                                                    | 16-22
        | 0x4140_0108 | BBIID     | MSL Interrupt ID register                                                     | 16-23
        | 0x4140_0110 | BBFREQ    | MSL Transmit Frequency Select register                                        | 10-6
        | 0x4140_0114 | BBWAIT    | MSL Wait Count register                                                       | 16-24
        | 0x4140_0118 | BBCST     | MSL Clock Stop Time register                                                  | 16-25
        | 0x4140_0140 | BBWAKE    | MSL Wake-Up register                                                          | 16-26
        | 0x4140_0144 | BBITFC    | MSL Interface Width register                                                  | 10-6

        Keypad Interface
        | 0x4150_0000 | KPC       | Keypad Interface Control register                                             | 18-12
        | 0x4150_0008 | KPDK      | Keypad Interface Direct Key register                                          | 18-16
        | 0x4150_0010 | KPREC     | Keypad Interface Rotary Encoder Count register                                | 18-17
        | 0x4150_0018 | KPMK      | Keypad Interface Matrix Key register                                          | 18-18
        | 0x4150_0020 | KPAS      | Keypad Interface Automatic Scan register                                      | 18-18
        | 0x4150_0028 | KPASMKP0  | Keypad Interface Automatic Scan Multiple Keypress register 0                  | 18-20
        | 0x4150_0030 | KPASMKP1  | Keypad Interface Automatic Scan Multiple Keypress register 1                  | 18-20
        | 0x4150_0038 | KPASMKP2  | Keypad Interface Automatic Scan Multiple Keypress register 2                  | 18-20
        | 0x4150_0040 | KPASMKP3  | Keypad Interface Automatic Scan Multiple Keypress register 3                  | 18-20
        | 0x4150_0048 | KPKDI     | Keypad Interface Key Debounce Interval register                               | 18-23
        
        Universal Subscriber ID (USIM) Interface
        | 0x4160_0000 | RBR       | USIM Receive Buffer register                                                  | 19-18
        | 0x4160_0004 | THR       | USIM Transmit Holding register                                                | 19-19
        | 0x4160_0008 | IER       | USIM Interrupt Enable register                                                | 19-20
        | 0x4160_000C | IIR       | USIM Interrupt Identification register                                        | 19-22
        | 0x4160_0010 | FCR       | USIM FIFO Control register                                                    | 19-24
        | 0x4160_0014 | FSR       | USIM FIFO Status register                                                     | 19-26
        | 0x4160_0018 | ECR       | USIM Error Control register                                                   | 19-27
        | 0x4160_001C | LCR       | USIM Line Control register                                                    | 19-29
        | 0x4160_0020 | USCCR     | USIM Card Control register                                                    | 19-31
        | 0x4160_0024 | LSR       | USIM Line Status register                                                     | 19-32
        | 0x4160_0028 | EGTR      | USIM Extra Guard Time register                                                | 19-34
        | 0x4160_002C | BGTR      | USIM Block Guard Time register                                                | 19-34
        | 0x4160_0030 | TOR       | USIM Time-Out register                                                        | 19-35
        | 0x4160_0034 | CLKR      | USIM Clock register                                                           | 19-36
        | 0x4160_0038 | DLR       | USIM Divisor Latch register                                                   | 19-37
        | 0x4160_003C | FLR       | USIM Factor Latch register                                                    | 19-37
        | 0x4160_0040 | CWTR      | USIM Character Waiting Time register                                          | 19-38
        | 0x4160_0044 | BWTR      | USIM Block Waiting Time register                                              | 19-39
        
        Synchronous Serial Port 2
        | 0x4170_0000 | SSCR0_2   | SSP2 Control register 0                                                       | 8-25
        | 0x4170_0004 | SSCR1_2   | SSP 2 Control register 1                                                      | 8-30
        | 0x4170_0008 | SSSR_2    | SSP 2 Status register                                                         | 8-43
        | 0x4170_000C | SSITR_2   | SSP 2 Interrupt Test register                                                 | 8-42
        | 0x4170_0010 | SSDR_2    | SSP 2 Data Write register/Data Read register                                  | 8-48
        | 0x4170_0028 | SSTO_2    | SSP 2 Time-Out register                                                       | 8-41
        | 0x4170_002C | SSPSP_2   | SSP 2 Programmable Serial Protocol                                            | 8-39
        | 0x4170_0030 | SSTSA_2   | SSP2 TX Timeslot Active register                                              | 8-48
        | 0x4170_0034 | SSRSA_2   | SSP2 RX Timeslot Active register                                              | 8-49
        | 0x4170_0038 | SSTSS_2   | SSP2 Timeslot Status register                                                 | 8-50
        | 0x4170_003C | SSACD_2   | SSP2 Audio Clock Divider register                                             | 8-51

        Memory Stick Host Controller
        | 0x4180_0000 | MSCMR     | MSHC Command register                                                         | 17-8
        | 0x4180_0004 | MSCRSR    | MSHC Control and Status register                                              | 17-9
        | 0x4180_0008 | MSINT     | MSHC Interrupt and Status register                                            | 17-10
        | 0x4180_000C | MSINTEN   | MSHC Interrupt Enable register                                                | 17-11
        | 0x4180_0010 | MSCR2     | MSHC Control register 2                                                       | 17-12
        | 0x4180_0014 | MSACD     | MSHC ACD Command register                                                     | 17-13
        | 0x4180_0018 | MSRXFIFO  | MSHC Receive FIFO register                                                    | 17-14
        | 0x4180_001C | MSTXFIFO  | MSHC Transmit FIFO register                                                   | 17-15
        
        Synchronous Serial Port 3
        | 0x4190_0000 | SSCR0_3   | SSP 3 Control register 0                                                      | 8-25
        | 0x4190_0004 | SSCR1_3   | SSP 3 Control register 1                                                      | 8-30
        | 0x4190_0008 | SSSR_3    | SSP 3 Status register                                                         | 8-43
        | 0x4190_000C | SSITR_3   | SSP 3 Interrupt Test register                                                 | 8-42
        | 0x4190_0010 | SSDR_3    | SSP 3 Data Write register/Data Read register                                  | 8-48
        | 0x4190_0028 | SSTO_3    | SSP 3 Time-Out register                                                       | 8-41
        | 0x4190_002C | SSPSP_3   | SSP 3 Programmable Serial Protocol                                            | 8-39
        | 0x4190_0030 | SSTSA_3   | SSP TX Timeslot Active register                                               | 8-48
        | 0x4190_0034 | SSRSA_3   | SSP RX Timeslot Active register                                               | 8-49
        | 0x4190_0038 | SSTSS_3   | SSP Timeslot Status register                                                  | 8-50
        | 0x4190_003C | SSACD_3   | SSP Audio Clock Divider register                                              | 8-51

    */
}
