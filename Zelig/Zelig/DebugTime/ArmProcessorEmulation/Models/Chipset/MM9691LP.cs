//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


//#define TRACE_INTERRUPT_WITH_ARMTIMER
#define OLLIE_USE_CACHE_PSEUDO_LRU


namespace Microsoft.Zelig.Emulation.ArmProcessor.Chipset
{
    using System;
    using System.Threading;
    using System.Collections.Generic;

    using EncDef       = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    using ElementTypes = Microsoft.Zelig.MetaData.ElementTypes;
    using Cfg          = Microsoft.Zelig.Configuration.Environment;

    //--//

    public static partial class MM9691LP
    {
        [Simulator.PeripheralRange(Base=0x30000000U,Length=0x0000000CU,ReadLatency=1,WriteLatency=2)]
        [Simulator.PeripheralRange(Base=0xB0000000U,Length=0x0000000CU,ReadLatency=1,WriteLatency=2)]
        public class BUSWATCHER : Simulator.Peripheral
        {
            public const uint AHB_Abort_Control__ERR       = 0x00000001;
            public const uint AHB_Abort_Control__HWRITE    = 0x00000002;
            public const uint AHB_Abort_Control__HTRANS    = 0x0000000C;
            public const uint AHB_Abort_Control__HSIZE     = 0x00000030;
            public const uint AHB_Abort_Control__HRESP     = 0x000000C0;
            public const uint AHB_Abort_Control__HPROT     = 0x00000F00;
            public const uint AHB_Abort_Control__HBURST    = 0x00007000;
            public const uint AHB_Abort_Control__HMASTLOCK = 0x00008000;
            public const uint AHB_Abort_Control__HMASTER   = 0x000F0000;

            public const uint AHB_Abort_Enable__ABEN       = 0x00000001;

            //--//

            [Simulator.Register(Offset=0x00000000U)] public uint AHB_Abort_Address;
            [Simulator.Register(Offset=0x00000004U)] public uint AHB_Abort_Control;
            [Simulator.Register(Offset=0x00000008U)] public uint AHB_Abort_Enable
            {
                get
                {
                    return 0;
                }
            }

            [Simulator.Register(Offset=0x00000010U)] public uint AHB_Counter_Enable;
            [Simulator.Register(Offset=0x00000014U)] public uint AHB_Counter_Restart;
            [Simulator.Register(Offset=0x00000018U)] public uint AHB_Valid_Count;
            [Simulator.Register(Offset=0x0000001CU)] public uint AHB_Idle_Count;
            [Simulator.Register(Offset=0x00000020U)] public uint AHB_Cache_Valid_Count;
            [Simulator.Register(Offset=0x00000024U)] public uint AHB_NonCache_Count;
            [Simulator.Register(Offset=0x00000028U)] public uint AHB_Cache_Miss;
            [Simulator.Register(Offset=0x0000002CU)] public uint AHB_CPU_Idle_Count;
        }

        [Simulator.PeripheralRange(Base=0x30010000U,Length=0x00000060U,ReadLatency=1,WriteLatency=2)]
        [Simulator.PeripheralRange(Base=0xB0010000U,Length=0x00000060U,ReadLatency=1,WriteLatency=2)]
        public class EBIU : Simulator.Peripheral
        {
            public class DEVICE : Simulator.Peripheral
            {
                public const uint Control__WS__mask    = 0x0000003F;
                public const  int Control__WS__shift   =          0;
                public const uint Control__RCNT__mask  = 0x00000300;
                public const  int Control__RCNT__shift =          8;
                public const uint Control__SZ8         = 0x00001000;
                public const uint Control__SZ16        = 0x00002000;
                public const uint Control__SZ32        = 0x00003000;
                public const uint Control__RDY         = 0x00008000;
                public const uint Control__WD          = 0x00010000;
                public const uint Control__BRD         = 0x00020000;
                public const uint Control__BFEN        = 0x02000000;
                public const uint Control__mask        = 0x0203B73F;

                public static uint Control__WS__set  ( uint w ) { return (w << Control__WS__shift  ) & Control__WS__mask  ; }
                public static uint Control__RCNT__set( uint w ) { return (w << Control__RCNT__shift) & Control__RCNT__mask; }

                //--//

                [Simulator.LinkToContainer()           ] public EBIU Parent;
                [Simulator.Register(Offset=0x00000000U)] public uint LowAddress;
                [Simulator.Register(Offset=0x00000004U)] public uint HighAddress;
                [Simulator.Register(Offset=0x00000008U)] public uint Control;
            }

            [Simulator.Register(Offset=0x00000000U,Size=0x00000010U)] public DEVICE Device0;
            [Simulator.Register(Offset=0x00000010U,Size=0x00000010U)] public DEVICE Device1;
            [Simulator.Register(Offset=0x00000020U,Size=0x00000010U)] public DEVICE Device2;
            [Simulator.Register(Offset=0x00000030U,Size=0x00000010U)] public DEVICE Device3;
            [Simulator.Register(Offset=0x00000040U,Size=0x00000010U)] public DEVICE Device4;
            [Simulator.Register(Offset=0x00000050U,Size=0x00000010U)] public DEVICE Device5;
        }

        [Simulator.PeripheralRange(Base=0x30020000U,Length=0x00000114U,ReadLatency=1,WriteLatency=2)]
        [Simulator.PeripheralRange(Base=0xB0020000U,Length=0x00000114U,ReadLatency=1,WriteLatency=2)]
        public class DMAC : Simulator.Peripheral
        {
            //
            //    static const UINT32 c_DMAC_ENABLE             0x00000001
            //    static const UINT32 c_DMAC_ERROR              0x00000002         
            //    static const UINT32 c_DMAC_DONE               0x00000004
            //    static const UINT32 c_DMAC_HALFDONE           0x00000008
            //
            //    static const UINT32 c_DMAC_SRC_INCR_UNCH      0x00000000
            //    static const UINT32 c_DMAC_SRC_INCR_1         0x00000010
            //    static const UINT32 c_DMAC_SRC_INCR_2         0x00000020
            //    static const UINT32 c_DMAC_SRC_INCR_4         0x00000030
            //
            //    static const UINT32 c_DMAC_SRC_XFER_SZ_1      0x00000000
            //    static const UINT32 c_DMAC_SRC_XFER_SZ_2      0x00000040
            //    static const UINT32 c_DMAC_SRC_XFER_SZ_4      0x00000080
            //    static const UINT32 c_DMAC_SRC_XFER_SZ_RES    0x000000C0
            //
            //    static const UINT32 c_DMAC_SRC_BURST_SING     0x00000000
            //    static const UINT32 c_DMAC_SRC_BURST_I2       0x00000100
            //    static const UINT32 c_DMAC_SRC_BURST_W4       0x00000200
            //    static const UINT32 c_DMAC_SRC_BURST_I4       0x00000300
            //    static const UINT32 c_DMAC_SRC_BURST_W8       0x00000400
            //    static const UINT32 c_DMAC_SRC_BURST_I8       0x00000500
            //    static const UINT32 c_DMAC_SRC_BURST_W16      0x00000600
            //    static const UINT32 c_DMAC_SRC_BURST_I16      0x00000700
            //
            //    static const UINT32 c_DMAC_DMA_MODE_NORM      0x00000000
            //    static const UINT32 c_DMAC_DMA_MODE_CONT      0x00000800
            //    static const UINT32 c_DMAC_DMA_MODE_LINK      0x00001000
            //    static const UINT32 c_DMAC_DMA_MODE_RES       0x00001800
            //
            //    static const UINT32 c_DMAC_SRC_BPR_1          0x00000000
            //    static const UINT32 c_DMAC_SRC_BPR_2          0x00002000
            //    static const UINT32 c_DMAC_SRC_BPR_4          0x00004000
            //    static const UINT32 c_DMAC_SRC_BPR_8          0x00006000
            //    static const UINT32 c_DMAC_SRC_BPR_16         0x00008000
            //    static const UINT32 c_DMAC_SRC_BPR_32         0x0000A000
            //    static const UINT32 c_DMAC_SRC_BPR_64         0x0000C000
            //    static const UINT32 c_DMAC_SRC_BPR_128        0x0000E000
            //
            //    static const UINT32 c_DMAC_DEST_INC_UNCH      0x00000000
            //    static const UINT32 c_DMAC_DEST_INC_1         0x00010000
            //    static const UINT32 c_DMAC_DEST_INC_2         0x00020000
            //    static const UINT32 c_DMAC_DEST_INC_4         0x00030000
            //
            //    static const UINT32 c_DMAC_DEST_XFER_SZ_1     0x00000000
            //    static const UINT32 c_DMAC_DEST_XFER_SZ_2     0x00040000
            //    static const UINT32 c_DMAC_DEST_XFER_SZ_4     0x00080000
            //    static const UINT32 c_DMAC_DEST_XFER_RES      0x000C0000
            //
            //    static const UINT32 c_DMAC_DEST_BURST_SING    0x00000000
            //    static const UINT32 c_DMAC_DEST_BURST_I2      0x00100000
            //    static const UINT32 c_DMAC_DEST_BURST_W4      0x00200000
            //    static const UINT32 c_DMAC_DEST_BURST_I4      0x00300000
            //    static const UINT32 c_DMAC_DEST_BURST_W8      0x00400000
            //    static const UINT32 c_DMAC_DEST_BURST_I8      0x00500000
            //    static const UINT32 c_DMAC_DEST_BURST_W16     0x00600000
            //    static const UINT32 c_DMAC_DEST_BURST_I16     0x00700000
            //
            //    static const UINT32 c_DMAC_DONE_2_SRC         0x00000000
            //    static const UINT32 c_DMAC_DONE_2_DEST        0x00800000
            //
            //    static const UINT32 c_DMAC_DEV_SEL_MASK       0x07000000
            //    static const UINT32 c_DMAC_DEV_SEL_SW         0x00000000
            //    static const UINT32 c_DMAC_DEV_SEL_USB        0x01000000
            //    static const UINT32 c_DMAC_DEV_SEL_MW_TX      0x02000000
            //    static const UINT32 c_DMAC_DEV_SEL_MW_RX      0x03000000
            //    static const UINT32 c_DMAC_DEV_SEL_SER1_TRX   0x04000000 // CHAN 7-4=RX CHAN 3-0=TX
            //    static const UINT32 c_DMAC_DEV_SEL_SER2_TRX   0x05000000 // CHAN 7-4=RX CHAN 3-0=TX
            //    static const UINT32 c_DMAC_DEV_SEL_VITERBI    0x06000000
            //    static const UINT32 c_DMAC_DEV_SEL_ACRTAN     0x07000000
            //
            //    static const UINT32 c_DMAC_IDL_CYCLE_INDEX    27
            //    static const UINT32 c_DMAC_IDL_CYCLE_MASK     0xf8000000
            //

            public class CHANNEL : Simulator.Peripheral
            {
                [Simulator.LinkToContainer()           ] public DMAC Parent;
                [Simulator.Register(Offset=0x00000000U)] public uint SourceStartAddress;
                [Simulator.Register(Offset=0x00000004U)] public uint DestinationStartAddress;
                [Simulator.Register(Offset=0x00000008U)] public uint SourceBeat;
                [Simulator.Register(Offset=0x0000000CU)] public uint ControlWord;
            }

            public class CHANNEL_STATE : Simulator.Peripheral
            {
                [Simulator.LinkToContainer()           ] public DMAC Parent;
                [Simulator.Register(Offset=0x00000000U)] public uint SourceCurrentAddress;       // read only
                [Simulator.Register(Offset=0x00000004U)] public uint DestinationCurrentAddress;  // read only
                [Simulator.Register(Offset=0x00000008U)] public uint SourceCurrentBeatCount;     // read only
            }

            //--//

            [Simulator.Register(Offset=0x00000000U,Size=0x00000010U,Instances=8)] public CHANNEL[]       Channel;
            [Simulator.Register(Offset=0x00000080U,Size=0x00000010U,Instances=8)] public CHANNEL_STATE[] ChannelState;
            [Simulator.Register(Offset=0x00000100U                             )] public uint            InterruptStatus;
            [Simulator.Register(Offset=0x00000104U                             )] public uint            InterruptRawStatus;
            [Simulator.Register(Offset=0x00000108U                             )] public uint            InterruptEnable;
            [Simulator.Register(Offset=0x0000010CU                             )] public uint            InterruptEnableClear;
            [Simulator.Register(Offset=0x00000110U                             )] public uint            InternalDMACTestMode;
            [Simulator.Register(Offset=0x00000114U                             )] public uint            InternalTestRequest;
        }

        [Simulator.PeripheralRange(Base=0x30030000U,Length=0x00008008U,ReadLatency=1,WriteLatency=2)]
        [Simulator.PeripheralRange(Base=0xB0030000U,Length=0x00008008U,ReadLatency=1,WriteLatency=2)]
        public class VITERBI : Simulator.Peripheral
        {
////        //--//
////
////        CPU_MAC_INT16_ON_32    InputData[c_Input_Array_Elements];            // 0x0000      +0x0A00
////        /*************/ UINT32 Padding1 [                   128];            // 0x0A00      +0x0200
////
////        CPU_MAC_UINT16_ON_32   OutputData[c_Output_Array_Elements];          // 0x0C00      +0x0050
////        /*************/ UINT32 Padding2  [                    236];          // 0x0C50      +0x03B0
////
////        CPU_MAC_UINT16_ON_32   PathMetrics[c_PathMetrics_Array_Elements];    // 0x1000      +0x0800
////        /*************/ UINT32 Padding3   [                         512];    // 0x1800      +0x0800
////
////        CPU_MAC_UINT16_ON_32   TracebackArray[c_Traceback_Array_Elements];   // 0x2000      +0x5000
////        /*************/ UINT32 Padding4      [                      1024];   // 0x7000      +0x1000
////
////        /****/ volatile UINT32 RamControl;                                   // 0x8000      +0x0004
////        static const    UINT32 RamControl__CPU     = 0x00000000;
////        static const    UINT32 RamControl__VITERBI = 0x00000001;
////
////        /****/ volatile UINT32 Control;                                      // 0x8004      +0x0004
////        static const    UINT32 Control__DONE_ACS   = 0x00000001;
////        static const    UINT32 Control__DONE_TRC   = 0x00000002;
////        static const    UINT32 Control__IEN_ACS    = 0x00000004;
////        static const    UINT32 Control__IEN_TRC    = 0x00000008;
////        static const    UINT32 Control__DEN_ACS    = 0x00000010;
////        static const    UINT32 Control__DEN_TRC    = 0x00000020;
////        static const    UINT32 Control__GO_ACS     = 0x00000040;
////        static const    UINT32 Control__GO_TRC     = 0x00000080;
////        static const    UINT32 Control__GO_BOTH    = 0x00000100;
        }

        [Simulator.PeripheralRange(Base=0x30040000U,Length=0x00008020U,ReadLatency=1,WriteLatency=2)]
        [Simulator.PeripheralRange(Base=0xB0040000U,Length=0x00008020U,ReadLatency=1,WriteLatency=2)]
        public class FILTERARCTAN : Simulator.Peripheral
        {
////        static const UINT32 c_Base = 0xB0040000;
////
////        static const UINT32 c_Arctan_Block_Size          = 128;
////        static const UINT32 c_Filter_Coefficient_Size    = 512;
////        static const UINT32 c_Filter_Input_Size          = 2048;
////        static const UINT32 c_Filter_Output_Size         = 1024;
////
////        static const UINT32 c_Arctan_Max                 = c_Arctan_Block_Size;
////        static const UINT32 c_Complex_Max                = c_Filter_Coefficient_Size / 2;
////        static const UINT32 c_Vector_Max                 = c_Filter_Coefficient_Size;
////        static const UINT32 c_Filter_Max                 = c_Filter_Output_Size;
////        static const UINT32 c_Filter_Coefficient_Max     = 31;
////
////        //--//
////
////        CPU_MAC_IQPAIR_ON_64   Arctan_InputData   [c_Arctan_Block_Size      ];
////        CPU_MAC_ANGLEMAG_ON_64 Arctan_OutputData  [c_Arctan_Block_Size      ];
////        CPU_MAC_INT16_ON_32    Filter_Coefficients[c_Filter_Coefficient_Size];
////        CPU_MAC_INT16_ON_32    Filter_InputData   [c_Filter_Input_Size      ];
////        CPU_MAC_INT32_ON_64    Filter_OutputData  [c_Filter_Output_Size     ];
////        /*************/ UINT32 Padding1           [                     3072];
////
////        /****/ volatile UINT32 RPT_CNT;
////        static const    UINT32 RPT_CNT__mask             = 0x0000007F;
////
////        /****/ volatile UINT32 reserved;
////
////        /****/ volatile UINT32 NCOEF;
////        static const    UINT32 NCOEF__mask               = 0x000001FF;
////        static const    UINT32 NCOEF__shift              =          0;
////        static const    UINT32 NCOEF__CMPLX              = 0x00000200;
////        static const    UINT32 NCOEF__LOW_N              = 0x00000400;
////
////        __inline static UINT32 NCOEF__set( UINT32 w ) { return (w << NCOEF__shift) & NCOEF__mask; }
////
////        /****/ volatile UINT32 NOUT;
////        static const    UINT32 NOUT__mask                = 0x000007FF;
////        static const    UINT32 NOUT__shift               =          0;
////
////        __inline static UINT32 NOUT__set( UINT32 w ) { return (w << NOUT__shift) & NOUT__mask; }
////
////        /****/ volatile UINT32 IN_PTR;
////        static const    UINT32 IN_PTR__mask              = 0x000007FF;
////        static const    UINT32 IN_PTR__shift             =          0;
////
////        __inline static UINT32 IN_PTR__set( UINT32 w ) { return (w << IN_PTR__shift) & IN_PTR__mask; }
////
////        /****/ volatile UINT32 OUT_PTR;
////        static const    UINT32 OUT_PTR__mask             = 0x000007FF;
////        static const    UINT32 OUT_PTR__shift            =          0;
////
////        __inline static UINT32 OUT_PTR__set( UINT32 w ) { return (w << OUT_PTR__shift) & OUT_PTR__mask; }
////
////        /****/ volatile UINT32 RAM_CONT;
////        static const    UINT32 RAM_CONT__CPU             = 0x00000000;
////        static const    UINT32 RAM_CONT__MAC             = 0x00000001;
////
////        /****/ volatile UINT32 CONT;
////        static const    UINT32 CONT__DONE_ATN      = 0x00000001;
////        static const    UINT32 CONT__DONE_FILT     = 0x00000002;
////        static const    UINT32 CONT__IEN_ATN       = 0x00000004;
////        static const    UINT32 CONT__IEN_FILT      = 0x00000008;
////        static const    UINT32 CONT__DEN_ATN       = 0x00000010;
////        static const    UINT32 CONT__DEN_FILT      = 0x00000020;
////        static const    UINT32 CONT__GO_ATN        = 0x00000040;
////        static const    UINT32 CONT__GO_FILT       = 0x00000080;
        }

        [Simulator.PeripheralRange(Base=0x38000000U,Length=0x00000210U,ReadLatency=1,WriteLatency=2)]
        public class INTC : Simulator.Peripheral
        {
            public const int IRQ_INDEX_unused0                  =  0;
            public const int IRQ_INDEX_Programmed_Interrupt     =  1;
            public const int IRQ_INDEX_Debug_Channel_Comms_Rx   =  2;
            public const int IRQ_INDEX_Debug_Channel_Comms_Tx   =  3;
            public const int IRQ_INDEX_ARM_Timer_1              =  4;
            public const int IRQ_INDEX_ARM_Timer_2              =  5;
            public const int IRQ_INDEX_Versatile_Timer_1        =  6;
            public const int IRQ_INDEX_Versatile_Timer_2        =  7;
            public const int IRQ_INDEX_Versatile_Timer_3        =  8;
            public const int IRQ_INDEX_Versatile_Timer_4        =  9;
            public const int IRQ_INDEX_Real_Time_Clock          = 10;
            public const int IRQ_INDEX_USB                      = 11;
            public const int IRQ_INDEX_USART0_Tx                = 12;
            public const int IRQ_INDEX_USART0_Rx                = 13;
            public const int IRQ_INDEX_USART1_Tx                = 14;
            public const int IRQ_INDEX_USART1_Rx                = 15;
            public const int IRQ_INDEX_GPIO_00_07               = 16;
            public const int IRQ_INDEX_GPIO_08_15               = 17;
            public const int IRQ_INDEX_GPIO_16_23               = 18;
            public const int IRQ_INDEX_GPIO_24_31               = 19;
            public const int IRQ_INDEX_GPIO_32_39               = 20;
            public const int IRQ_INDEX_Edge_Detected_Interrupts = 21;
            public const int IRQ_INDEX_MicroWire                = 22;
            public const int IRQ_INDEX_Watchdog                 = 23;
            public const int IRQ_INDEX_USART0_Flow_Control      = 24;
            public const int IRQ_INDEX_USART1_Flow_Control      = 25;
            public const int IRQ_INDEX_unused1                  = 26;
            public const int IRQ_INDEX_APC                      = 27;
            public const int IRQ_INDEX_DMA_ALL_Channels         = 28;
            public const int IRQ_INDEX_Viterbi_Processor        = 29;
            public const int IRQ_INDEX_Filter_Processor         = 30;
            public const int IRQ_INDEX_AHB_Write_Error          = 31;

            public class IRQ : Simulator.Peripheral
            {
                [Simulator.LinkToContainer] public INTC Parent;

                [Simulator.Register(Offset=0x00000000U)] public uint Status
                {
                    get
                    {
                        return m_state_EnableSet & this.RawStatus;
                    }
                }

                [Simulator.Register(Offset=0x00000004U)] public uint RawStatus
                {
                    get
                    {
                        uint rawStatus;

                        if((this.SourceSelect & 1) == 0)
                        {
                            rawStatus = this.Parent.m_inputStatus | this.Soft;
                        }
                        else
                        {
                            rawStatus = this.TestSource;
                        }

                        return rawStatus;
                    }
                }

                [Simulator.Register(Offset=0x00000008U)] public uint EnableSet
                {
                    get
                    {
                        return m_state_EnableSet;
                    }

                    set
                    {
                        m_state_EnableSet |= value;

                        this.Parent.Evaluate();
                    }
                }

                [Simulator.Register(Offset=0x0000000CU)] public uint EnableClear
                {
                    get
                    {
                        return m_state_EnableSet;
                    }

                    set
                    {
                        m_state_EnableSet &= ~value;

                        this.Parent.Evaluate();
                    }
                }
                [Simulator.Register(Offset=0x00000010U)] public uint Soft
                {
                    get
                    {
                        return m_state_Soft;
                    }

                    set
                    {
                        m_state_Soft = value & (1U << IRQ_INDEX_Programmed_Interrupt);

                        this.Parent.Evaluate();
                    }
                }

                [Simulator.Register(Offset=0x00000014U)] public uint TestSource
                {
                    get
                    {
                        return m_state_TestSource;
                    }

                    set
                    {
                        m_state_TestSource = value;

                        this.Parent.Evaluate();
                    }
                }

                [Simulator.Register(Offset=0x00000018U)] public uint SourceSelect
                {
                    get
                    {
                        return m_state_SourceSelect;
                    }

                    set
                    {
                        m_state_SourceSelect = (value & 1);

                        this.Parent.Evaluate();
                    }
                }

                //--//
                
                uint m_state_EnableSet;
                uint m_state_Soft;
                uint m_state_TestSource;
                uint m_state_SourceSelect;
            }

            public class FIRQ : Simulator.Peripheral
            {
                [Simulator.LinkToContainer] public INTC Parent;

                [Simulator.Register(Offset=0x00000000U)] public uint Status
                {
                    get
                    {
                        return this.EnableSet & this.RawStatus;
                    }
                }

                [Simulator.Register(Offset=0x00000004U)] public uint RawStatus
                {
                    get
                    {
                        return (this.Parent.m_inputStatus >> (int)this.Select) & 1;
                    }
                }

                [Simulator.Register(Offset=0x00000008U)] public uint EnableSet
                {
                    get
                    {
                        return m_state_EnableSet;
                    }

                    set
                    {
                        m_state_EnableSet |= (value & 1);

                        this.Parent.Evaluate();
                    }
                }

                [Simulator.Register(Offset=0x0000000CU)] public uint EnableClear
                {
                    get
                    {
                        return m_state_EnableSet;
                    }

                    set
                    {
                        m_state_EnableSet &= ~(value & 1);

                        this.Parent.Evaluate();
                    }
                }

                [Simulator.Register(Offset=0x00000014U)] public uint TestSource;
                [Simulator.Register(Offset=0x00000018U)] public uint SourceSelect;
                [Simulator.Register(Offset=0x0000001CU)] public uint Select
                {
                    get
                    {
                        return m_state_Select;
                    }

                    set
                    {
                        m_state_Select = (value & 0x1F);

                        this.Parent.Evaluate();
                    }
                }

                //--//
                
                uint m_state_EnableSet;
                uint m_state_Select;
            }

            [Simulator.Register(Offset=0x00000000U,Size=0x00000020U)] public IRQ  Irq;
            [Simulator.Register(Offset=0x00000020U)                 ] public uint INTOUT_L_EnableSet;
            [Simulator.Register(Offset=0x00000024U)                 ] public uint INTOUT_L_EnableClear;
            [Simulator.Register(Offset=0x00000100U,Size=0x00000020U)] public FIRQ Fiq;
            [Simulator.Register(Offset=0x00000200U)                 ] public uint EdgeStatus;
            [Simulator.Register(Offset=0x00000204U)                 ] public uint EdgeRawStatus;
            [Simulator.Register(Offset=0x00000208U)                 ] public uint EdgeEnable;
            [Simulator.Register(Offset=0x0000020CU)                 ] public uint EdgeEnableClear;
            [Simulator.Register(Offset=0x00000210U)                 ] public uint EdgeClear;

            private uint m_inputStatus = 0;

            //--//

            public void Set( int index )
            {
                uint oldStatus = m_inputStatus;

                m_inputStatus |= 1U << index;

                if(oldStatus != m_inputStatus)
                {
                    Evaluate();
                }
            }

            public void Reset( int index )
            {
                uint oldStatus = m_inputStatus;

                m_inputStatus &= ~(1U << index);

                if(oldStatus != m_inputStatus)
                {
                    Evaluate();
                }
            }

            public void Evaluate()
            {
                m_owner.SetIrqStatus( (Irq.Status != 0) );
                m_owner.SetFiqStatus( (Fiq.Status != 0) );
            }
        }

        [Simulator.PeripheralRange(Base=0x38010000U,Length=0x0000005CU,ReadLatency=1,WriteLatency=2)]
        public class REMAP_PAUSE : Simulator.Peripheral
        {
            public const uint ResetStatus__POR = 0x00000001;

            [Simulator.Register(Offset=0x00000000U)] public uint Pause
            {
                set
                {
                    m_owner.SpinUntilInterrupts();
                }
            }

            [Simulator.Register(Offset=0x00000000U)] public uint Pause_CPU_ONLY;
            [Simulator.Register(Offset=0x00000010U)] public uint Identification = 0x4E969101;
            [Simulator.Register(Offset=0x00000020U)] public uint ClearResetMap
            {
                set
                {
                    //
                    // Map RAM to 0x00000000.
                    //
                    Simulator.AddressSpaceHandler hnd = m_owner.FindMemoryAtAddress( 0x08000000u );
                    if(hnd != null)
                    {
                        hnd.LinkAtAddress( 0 );
                    }
                }
            }

            [Simulator.Register(Offset=0x00000030U)] public uint ResetStatus;
            [Simulator.Register(Offset=0x00000034U)] public uint ResetStatusClear;
            [Simulator.Register(Offset=0x00000040U)] public uint SystemConfiguration;

            [Simulator.Register(Offset=0x00000050U)] public uint Cache_Enable
            {
                set
                {
                    CacheMemoryHandler cache; m_owner.FindMemory( out cache );
                    if(cache != null)
                    {
                        cache.Enabled = (value != 0);
                    }
                }
            }

            [Simulator.Register(Offset=0x00000054U)] public uint Cache_Tags_Reset
            {
                set
                {
                    CacheMemoryHandler cache; m_owner.FindMemory( out cache );
                    if(cache != null)
                    {
                        cache.ResettingTags = (value == 0);
                    }
                }
            }

            [Simulator.Register(Offset=0x00000058U)] public uint Cache_Flush_Enable
            {
                set
                {
                    CacheMemoryHandler cache; m_owner.FindMemory( out cache );
                    if(cache != null)
                    {
                        cache.FlushEnabled = (value != 0);
                    }
                }
            }

            //--//

            public override void OnConnected()
            {
                base.OnConnected();

                //
                // Map FLASH to 0x00000000.
                //
                Simulator.AddressSpaceHandler hnd = m_owner.FindMemoryAtAddress( 0x10000000u );
                if(hnd != null)
                {
                    hnd.LinkAtAddress( 0 );
                }
            }
        }

        public abstract class ARMTIMERx : Simulator.Peripheral
        {
            public const byte Control__PRESCALE_1    = 0x00;
            public const byte Control__PRESCALE_16   = 0x04;
            public const byte Control__PRESCALE_256  = 0x08;
            public const byte Control__MODE_FREE     = 0x00;
            public const byte Control__MODE_PERIODIC = 0x40;
            public const byte Control__MODE_ENABLE   = 0x80;

            [Simulator.Register(Offset=0x00000000U)] public ushort Load;
            [Simulator.Register(Offset=0x00000004U)] public ushort Value
            {
                get
                {
#if TRACE_INTERRUPT_WITH_ARMTIMER
                    Hosting.OutputSink sink; m_owner.GetHostingService( out sink );
                    if(sink != null)
                    {
                        sink.OutputLine( "" );
                        sink.OutputLine( "" );
                        sink.OutputLine( "ARMTIMER read at {0}", m_owner.ClockTicks );
                        sink.OutputLine( "" );

                        fActive = !fActive;

                        if(fActive)
                        {
                            start = m_owner.ClockTicks;
                        }
                        else
                        {
                            sink.OutputLine( "Time: {0}", m_owner.ClockTicks - start );
                            sink.OutputLine( "" );
                        }
                    }

////                m_owner.MonitorOpcodes   = !m_owner.MonitorOpcodes;
////                m_owner.MonitorRegisters = !m_owner.MonitorRegisters;
                    m_owner.MonitorCalls     = !m_owner.MonitorCalls;
#endif

                    return (ushort)(0 - m_owner.ClockTicks);
                }
            }
            [Simulator.Register(Offset=0x00000008U)] public byte   Control;
            [Simulator.Register(Offset=0x0000000CU)] public ushort Clear;

#if TRACE_INTERRUPT_WITH_ARMTIMER
            bool  fActive;
            ulong start;
#endif
        }

        [Simulator.PeripheralRange(Base=0x38020000U,Length=0x00000020U,ReadLatency=1,WriteLatency=2)]
        public class ARMTIMER0 : ARMTIMERx
        {
        }

        [Simulator.PeripheralRange(Base=0x38020020U,Length=0x00000020U,ReadLatency=1,WriteLatency=2)]
        public class ARMTIMER1 : ARMTIMERx
        {
        }

        [Simulator.PeripheralRange(Base=0x38030000U,Length=0x00000050U,ReadLatency=1,WriteLatency=2)]
        public class VTU32 : Simulator.Peripheral
        {
            public class IO_CONTROL : Simulator.Peripheral
            {
                public const uint CxyEDG_000   = 0x00000000;
                public const uint CxyEDG_001   = 0x00000001;
                public const uint CxyEDG_010   = 0x00000002;
                public const uint CxyEDG_011   = 0x00000003;
                public const uint CxyEDG_100   = 0x00000004;
                public const uint CxyEDG_101   = 0x00000005;
                public const uint CxyEDG_110   = 0x00000006;
                public const uint CxyEDG_111   = 0x00000007;
                public const uint PxyPOL_RESET = 0x00000000;
                public const uint PxyPOL_SET   = 0x00000008;

                [Simulator.LinkToContainer()           ] public VTU32 Parent;
                [Simulator.Register(Offset=0x00000000U)] public uint  Value;

                //--//

                // this puts the mode bits M for the chosen timer T (0-1), A/B (0-1) of a pair into the correct nibble/byte
                public static uint Set( uint T ,
                                        uint A ,
                                        uint M )
                {
                    return  M << (int)(A*4 + T*8);
                }

                public static uint Get( uint T ,
                                        uint A ,
                                        uint M )
                {
                    return (M >> (int)(A*4 + T*8)) & 0x000F;
                }

                //--//

                internal int FindChannel()
                {
                    return Array.IndexOf( this.Parent.IOControl, this );
                }
            }

            public class CHANNEL_PAIR : Simulator.Peripheral
            {
                public class CHANNEL : Simulator.Peripheral
                {
                    [Simulator.LinkToContainer()           ] public CHANNEL_PAIR Parent;
                    [Simulator.Register(Offset=0x00000000U)] public uint         Counter
                    {
                        get
                        {
                            return GetCurrentCounter();
                        }

                        set
                        {
                            m_state_Counter = value;
                        }
                    }

                    [Simulator.Register(Offset=0x00000004U)] public uint         PeriodCapture;
                    [Simulator.Register(Offset=0x00000008U)] public uint         DutyCycleCapture;

                    internal ulong m_base;
                    uint           m_state_Counter;

                    //--//
       

                    //--//

                    internal uint GetCurrentCounter()
                    {
                        ulong ticks = m_owner.ClockTicks - m_base;

                        return m_state_Counter + (uint)(ticks * GetRatio());
                    }

                    internal double GetRatio()
                    {
                        int idxPair = FindPairChannel();
                        int idxChn  = FindChannel    ();

                        VTU32 vtu = this.Parent.Parent;

                        uint mode = VTU32.ModeControl__get( (uint)(idxChn * 2 + idxPair), vtu.ModeControl );

                        switch(mode & VTU32.ModeControl__TMODx_MASK)
                        {
                            case VTU32.ModeControl__TMODx_LOW_POWER:
                                break;

                            case VTU32.ModeControl__TMODx_DUAL_PWM16:
                                break;

                            case VTU32.ModeControl__TMODx_PWM32:
                                if((mode & VTU32.ModeControl__TxARUN) != 0)
                                {
                                    return 1.0 / ((vtu.ChannelPair[idxChn].ClockPrescalar & 0xFF) + 1);
                                }
                                break;

                            case VTU32.ModeControl__TMODx_CAPTURE:
                                break;
                        }

                        return 0;
                    }

                    internal void Start()
                    {
                        m_base = m_owner.ClockTicks;
                    }

                    internal void Stop()
                    {
                        this.Counter = GetCurrentCounter();
                    }

                    internal int FindPairChannel()
                    {
                        return Array.IndexOf( this.Parent.Channel, this );
                    }

                    internal int FindChannel()
                    {
                        return this.Parent.FindChannel();
                    }
                }
                
                [Simulator.LinkToContainer()           ]                        public VTU32     Parent;
                [Simulator.Register(Offset=0x00000000U)]                        public uint      ClockPrescalar;
                [Simulator.Register(Offset=0x00000004U,Size=0x0CU,Instances=2)] public CHANNEL[] Channel;

                //--//

                public uint ClockPrescalar__get( uint T )
                {
                    return (ClockPrescalar >> (int)(T*8)) & 0x00FF;
                }

                public void ClockPrescalar__set( uint T ,
                                                 uint P )
                {
                    uint val = ClockPrescalar;

                    val &= ~(     0x000000FFu  << (int)(T*8));
                    val |=  ((P & 0x000000FFu) << (int)(T*8));

                    ClockPrescalar = val;
                }

                //--//

                internal int FindChannel()
                {
                    return Array.IndexOf( this.Parent.ChannelPair, this );
                }
            }

            //--//

            public const uint c_TIO1A = GPIO.c_Pin_20;
            public const uint c_TIO2A = GPIO.c_Pin_21;
            public const uint c_TIO3A = GPIO.c_Pin_22;
            public const uint c_TIO4A = GPIO.c_Pin_23;

            public const uint c_TIO1B = GPIO.c_Pin_05;
            public const uint c_TIO2B = GPIO.c_Pin_06;
            public const uint c_TIO3B = GPIO.c_Pin_13;
            public const uint c_TIO4B = GPIO.c_Pin_14;

            //--//

            public const uint ModeControl__TxARUN              = 0x00000001;
            public const uint ModeControl__TxBRUN              = 0x00000002;
            public const uint ModeControl__TMODx_LOW_POWER     = 0x00000000;
            public const uint ModeControl__TMODx_DUAL_PWM16    = 0x00000004;
            public const uint ModeControl__TMODx_PWM32         = 0x00000008;
            public const uint ModeControl__TMODx_CAPTURE       = 0x0000000C;
            public const uint ModeControl__TMODx_MASK          = 0x0000000C;

            public const uint Interrupt__Ix_None               = 0x00000000;
            public const uint Interrupt__Ix_1                  = 0x00000001;
            public const uint Interrupt__Ix_2                  = 0x00000002;
            public const uint Interrupt__Ix_3                  = 0x00000004;
            public const uint Interrupt__Ix_4                  = 0x00000008;
            public const uint Interrupt__Ix_ALL                = (Interrupt__Ix_1 | Interrupt__Ix_2 | Interrupt__Ix_3 | Interrupt__Ix_4);

            public const uint ExternalClockSelectRegister__CK1 = 0x00000001;
            public const uint ExternalClockSelectRegister__CK2 = 0x00000002;
            public const uint ExternalClockSelectRegister__CK3 = 0x00000004;
            public const uint ExternalClockSelectRegister__CK4 = 0x00000008;

            //--//

            [Simulator.Register(Offset=0x00000000U)]                        public uint           ModeControl
            {
                get
                {
                    return m_state_ModeControl;
                }

                set
                {
                    for(uint timer = 0; timer < 4; timer++)
                    {
                        uint oldMode = ModeControl__get( timer, this.ModeControl );
                        uint newMode = ModeControl__get( timer, value            );

                        if(oldMode != newMode)
                        {
                            switch(newMode & ModeControl__TMODx_MASK)
                            {
                                case ModeControl__TMODx_LOW_POWER:
                                    break;

                                case ModeControl__TMODx_DUAL_PWM16:
                                    break;

                                case ModeControl__TMODx_PWM32:
                                    CHANNEL_PAIR.CHANNEL chn = ChannelPair[timer/2].Channel[timer%2];

                                    if((newMode & ModeControl__TxARUN) != 0)
                                    {
                                        chn.Start();
                                    }
                                    else
                                    {
                                        chn.Stop();
                                    }
                                    break;

                                case ModeControl__TMODx_CAPTURE:
                                    break;
                            }
                        }
                    }

                    m_state_ModeControl = value;
                }
            }

            [Simulator.Register(Offset=0x00000004U,Size=0x04U,Instances=2)] public IO_CONTROL[]   IOControl;
            [Simulator.Register(Offset=0x0000000CU)]                        public uint           InterruptControl;
            [Simulator.Register(Offset=0x00000010U)]                        public uint           InterruptPending
            {
                get
                {
                    return 0;
                }
            }

            [Simulator.Register(Offset=0x00000014U,Size=0x1CU,Instances=2)] public CHANNEL_PAIR[] ChannelPair;
            [Simulator.Register(Offset=0x0000004CU)]                        public uint           ExternalClockSelectRegister;

            //--//

            uint m_state_ModeControl;

            //--//

            // this puts the mode bits M for the chosen timer T (0-3) into the correct nibble
            public static uint ModeControl__set( uint T ,
                                                 uint M )
            {
                return M << (int)(T*4);
            }

            public static uint ModeControl__get( uint T ,
                                                 uint M )
            {
                return (M >> (int)(T*4)) & 0x000F;
            }

            // this puts the INT bits I for the chosen timer T (0-3) into the correct nibble
            public static uint Interrupt__set( uint T ,
                                               uint I )
            {
                return I << (int)(T*4);
            }

            public static uint Interrupt__get( uint T ,
                                               uint I )
            {
                return (I >> (int)(T*4)) & 0x000F;
            }
        }

        public abstract class USARTx : Simulator.Peripheral, Hosting.IAsynchronousSerialInterface
        {
////        static const UINT32 SER1_CLKX = MM9637A_GPIO::c_Pin_00;
////        static const UINT32 SER1_TDX  = MM9637A_GPIO::c_Pin_01;
////        static const UINT32 SER1_RDX  = MM9637A_GPIO::c_Pin_02;
////        static const UINT32 SER1_RTS  = MM9637A_GPIO::c_Pin_03;
////        static const UINT32 SER1_CTS  = MM9637A_GPIO::c_Pin_04;
////        static const UINT32 SER2_CLKX = MM9637A_GPIO::c_Pin_08;
////        static const UINT32 SER2_TDX  = MM9637A_GPIO::c_Pin_09;
////        static const UINT32 SER2_RDX  = MM9637A_GPIO::c_Pin_10;
////        static const UINT32 SER2_RTS  = MM9637A_GPIO::c_Pin_11;
////        static const UINT32 SER2_CTS  = MM9637A_GPIO::c_Pin_12;

            //--//

            public const byte UnICTRL__TBE           = 0x01;
            public const byte UnICTRL__RBF           = 0x02;
            public const byte UnICTRL__DCTS          = 0x04;
            public const byte UnICTRL__CTS           = 0x08;
            public const byte UnICTRL__EFCI          = 0x10;
            public const byte UnICTRL__ETI           = 0x20;
            public const byte UnICTRL__ERI           = 0x40;
            public const byte UnICTRL__EEI           = 0x80;
            public const byte UnICTRL__mask          = (UnICTRL__EFCI | UnICTRL__ETI | UnICTRL__ERI | UnICTRL__EEI);

            public const byte UnSTAT__PE             = 0x01;
            public const byte UnSTAT__FE             = 0x02;
            public const byte UnSTAT__DOE            = 0x04;
            public const byte UnSTAT__ERR            = 0x08;
            public const byte UnSTAT__BKD            = 0x10;
            public const byte UnSTAT__RB9            = 0x20;
            public const byte UnSTAT__XMIP           = 0x40;

            public const byte UnFRS__CHAR_8          = 0x00;
            public const byte UnFRS__CHAR_7          = 0x01;
            public const byte UnFRS__CHAR_9          = 0x02;
            public const byte UnFRS__CHAR_9_LOOPBACK = 0x03;
            public const byte UnFRS__STP_1           = 0x00;
            public const byte UnFRS__STP_2           = 0x04;
            public const byte UnFRS__XB9_0           = 0x00;
            public const byte UnFRS__XB9_1           = 0x08;
            public const byte UnFRS__PSEL_ODD        = 0x00;
            public const byte UnFRS__PSEL_EVEN       = 0x10;
            public const byte UnFRS__PSEL_MARK       = 0x20;
            public const byte UnFRS__PSEL_SPACE      = 0x30;
            public const byte UnFRS__PEN_DISABLED    = 0x00;
            public const byte UnFRS__PEN_ENABLED     = 0x40;

            public static int UnFRS__LEN_CHAR__get( byte a )
            {
                switch(a & 0x03)
                {
                    case UnFRS__CHAR_7: return 7;
                    case UnFRS__CHAR_8: return 8;
                    default:            return 9;
                }
            }

            public static int UnFRS__LEN_STOPS__get( byte a )
            {
                return ((a & UnFRS__STP_2) != 0) ? 2 : 1;
            }

            public static int UnFRS__LEN_PARITY__get( byte a )
            {
                return ((a & UnFRS__PEN_ENABLED) != 0) ? 1 : 0;
            }

            public const byte UnMDSL1__MOD           = 0x01;
            public const byte UnMDSL1__ATN           = 0x02;
            public const byte UnMDSL1__BRK           = 0x04;
            public const byte UnMDSL1__CKS           = 0x08;
            public const byte UnMDSL1__ETD           = 0x10;
            public const byte UnMDSL1__ERD           = 0x20;
            public const byte UnMDSL1__FCE           = 0x40;
            public const byte UnMDSL1__RTS           = 0x80;

            public static int  UnBAUD__get    ( byte a ) { return   a                ; }
            public static int  UnPSR__DIV__get( byte a ) { return ((a << 8) & 0x0700); }
            public static int  UnPSR__PSC__get( byte a ) { return ((a >> 3) & 0x001F); }

            public const byte UnOVSR__7              = 0x07;
            public const byte UnOVSR__8              = 0x08;
            public const byte UnOVSR__9              = 0x09;
            public const byte UnOVSR__10             = 0x0A;
            public const byte UnOVSR__11             = 0x0B;
            public const byte UnOVSR__12             = 0x0C;
            public const byte UnOVSR__13             = 0x0D;
            public const byte UnOVSR__14             = 0x0E;
            public const byte UnOVSR__15             = 0x0F;
            public const byte UnOVSR__16             = 0x00;

            public static int  UnOVSR__get( byte a ) { return a == UnOVSR__16 ? 16 : (int)a; }

            public const byte UnMDSL2__SMD           = 0x01;

            //--//

            [Simulator.LinkToPeripheral()          ] public INTC InterruptController;
            [Simulator.LinkToPeripheral()          ] public CMU  ClockController;

            [Simulator.Register(Offset=0x00000000U)] public byte UnTBUF
            {
                get
                {
                    if(this.IsClockEnabled)
                    {
                        return m_txBuffer;
                    }
                    else
                    {
                        return 0;
                    }
                }

                set
                {
                    if(this.IsClockEnabled)
                    {
                        if(m_owner.AreTimingUpdatesEnabled)
                        {
                            m_txBuffer = value;

                            if(this.TransmitBufferEmpty)
                            {
                                this.TransmitBufferEmpty = false;
                            }
                        }
                    }
                }
            }

            [Simulator.Register(Offset=0x00000004U)] public byte UnRBUF
            {
                get
                {
                    if(this.IsClockEnabled)
                    {
                        byte res = m_rxBuffer;

                        if(this.ReadBufferFull)
                        {
                            this.ReadBufferFull = false;
                        }

                        return res;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            [Simulator.Register(Offset=0x00000008U)] public byte UnICTRL
            {
                get
                {
                    if(this.IsClockEnabled)
                    {
                        return m_state_UnICTRL;
                    }
                    else
                    {
                        return 0;
                    }
                }

                set
                {
                    if(this.IsClockEnabled)
                    {
                        if(m_owner.AreTimingUpdatesEnabled)
                        {
                            MaskedUpdateBitField( ref m_state_UnICTRL, value, UnICTRL__EEI | UnICTRL__ERI | UnICTRL__ETI );

                            AdvanceStateMachine();
                        }
                    }
                }
            }

            [Simulator.Register(Offset=0x0000000CU)] public byte UnSTAT
            {
                get
                {
                    if(this.IsClockEnabled)
                    {
                        return m_state_UnSTAT;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            [Simulator.Register(Offset=0x00000010U)] public byte UnFRS;
            [Simulator.Register(Offset=0x00000014U)] public byte UnMDSL1;
            [Simulator.Register(Offset=0x00000018U)] public byte UnBAUD;
            [Simulator.Register(Offset=0x0000001CU)] public byte UnPSR;
            [Simulator.Register(Offset=0x00000020U)] public byte UnOVSR;
            [Simulator.Register(Offset=0x00000024U)] public byte UnMDSL2;
            [Simulator.Register(Offset=0x00000028U)] public byte UnSPOS;

            //--//

            private   byte           m_state_UnICTRL = UnICTRL__TBE;
            private   byte           m_state_UnSTAT;

            protected int            m_portNumber;
            private   bool           m_fAdvancingStateMachine;
            private   bool           m_fShutdown; 

            private   bool           m_txShiftRegisterActive;
            private   byte           m_txShiftRegister;
            private   byte           m_txBuffer;
            private   Queue< byte >  m_txQueue = new Queue< byte >();
            private   AutoResetEvent m_txWait = new AutoResetEvent( false );

            private   bool           m_rxShiftRegisterActive;
            private   byte           m_rxShiftRegister;
            private   byte           m_rxBuffer;
            private   Queue< byte >  m_rxQueue = new Queue< byte >();

            //--//

            //
            // Interface Methods
            //

            void Hosting.IAsynchronousSerialInterface.Send( byte value )
            {
                HostPushData( value );
            }

            bool Hosting.IAsynchronousSerialInterface.Receive(     int  timeout ,
                                                               out byte value   )
            {
                return HostPullData( timeout, out value );
            }

            int Hosting.IAsynchronousSerialInterface.PortNumber
            {
                get
                {
                    return m_portNumber;
                }
            }

            //
            // Helper Methods
            //

            public override void OnDisconnected()
            {
                m_fShutdown = true;

                m_txWait.Set();

                base.OnDisconnected();
            }

            private void AdvanceStateMachine()
            {
                if(this.IsClockEnabled)
                {
                    //
                    // Prevent recursion.
                    //
                    if(m_fAdvancingStateMachine == false)
                    {
                        m_fAdvancingStateMachine = true;

                        AdvanceStateMachine_RX();
                        AdvanceStateMachine_TX();

                        m_fAdvancingStateMachine = false;
                    }

                    UpdateInterruptStatus();
                }
            }

            private void AdvanceStateMachine_TX()
            {
                if(this.ShiftRegisterTxBusy == false)
                {
                    if(this.TransmitBufferEmpty == false)
                    {
                        m_txShiftRegister = m_txBuffer;

                        this.ShiftRegisterTxBusy = true;
                        this.TransmitBufferEmpty = true;

                        //--//

                        Hosting.DeviceClockTicksTracking svc; m_owner.GetHostingService( out svc );

                        svc.RequestRelativeClockTickCallback( this.CharacterClockCycles, delegate()
                        {
                            this.ShiftRegisterTxBusy = false;

                            if(this.IsClockEnabled)
                            {
                                DevidePushData( m_txShiftRegister );

                                AdvanceStateMachine();
                            }
                        } );
                    }
                }
            }

            private void AdvanceStateMachine_RX()
            {
                if(this.ShiftRegisterRxBusy == false)
                {
                    if(DevicePullData( out m_rxShiftRegister ))
                    {
                        this.ShiftRegisterRxBusy = true;

                        Hosting.DeviceClockTicksTracking svc; m_owner.GetHostingService( out svc );

                        svc.RequestRelativeClockTickCallback( this.CharacterClockCycles, delegate()
                        {
                            m_rxBuffer = m_rxShiftRegister;

                            this.ShiftRegisterRxBusy = false;

                            if(this.IsClockEnabled)
                            {
                                if(this.ReadBufferFull)
                                {
                                    m_state_UnSTAT |= UnSTAT__ERR;
                                }
                                else
                                {
                                    this.ReadBufferFull = true;
                                }
                            }
                        } );
                    }
                }
            }

            private void UpdateInterruptStatus()
            {
                if(this.IsClockEnabled)
                {
                    if(TestBitField( m_state_UnICTRL, UnICTRL__ETI ) && this.TransmitBufferEmpty)
                    {
                        this.InterruptController.Set( m_portNumber == 0 ? INTC.IRQ_INDEX_USART0_Tx : INTC.IRQ_INDEX_USART1_Tx );
                    }
                    else
                    {
                        this.InterruptController.Reset( m_portNumber == 0 ? INTC.IRQ_INDEX_USART0_Tx : INTC.IRQ_INDEX_USART1_Tx );
                    }

                    if((TestBitField( m_state_UnICTRL, UnICTRL__ERI ) && this.ReadBufferFull                        ) ||
                       (TestBitField( m_state_UnICTRL, UnICTRL__EEI ) && TestBitField( m_state_UnSTAT, UnSTAT__ERR ))  )
                    {
                        this.InterruptController.Set( m_portNumber == 0 ? INTC.IRQ_INDEX_USART0_Rx : INTC.IRQ_INDEX_USART1_Rx );
                    }
                    else
                    {
                        this.InterruptController.Reset( m_portNumber == 0 ? INTC.IRQ_INDEX_USART0_Rx : INTC.IRQ_INDEX_USART1_Rx );
                    }
                }
            }

            private bool DevicePullData( out byte val )
            {
                lock(m_rxQueue)
                {
                    if(m_rxQueue.Count > 0)
                    {
                        val = m_rxQueue.Dequeue();
                        return true;
                    }

                    val = 0;
                    return false;
                }
            }

            private void DevidePushData( byte val )
            {
                lock(m_txQueue)
                {
                    m_txQueue.Enqueue( val );
                }

                m_txWait.Set();
            }

            private bool HostPullData( int timeout, out byte val )
            {
                while(true)
                {
                    lock(m_txQueue)
                    {
                        if(m_txQueue.Count > 0)
                        {
                            val = m_txQueue.Dequeue();
                            return true;
                        }
                    }

                    if(m_fShutdown || m_txWait.WaitOne( timeout, false ) == false)
                    {
                        val = 0;
                        return false;
                    }
                }
            }

            private void HostPushData( byte val )
            {
                lock(m_rxQueue)
                {
                    m_rxQueue.Enqueue( val );
                }

                Hosting.DeviceClockTicksTracking svc; m_owner.GetHostingService( out svc );

                svc.RequestRelativeClockTickCallback( this.CharacterClockCycles, delegate()
                {
                    if(this.IsClockEnabled)
                    {
                        AdvanceStateMachine();
                    }
                } );
            }

            //
            // Access Methods
            //

            public bool IsClockEnabled
            {
                get
                {
                    return (this.ClockController.CLK_EN_REG & (m_portNumber == 0 ? CMU.MCLK_EN__USART0 : CMU.MCLK_EN__USART1)) != 0;
                }
            }

            private int CharacterClockCycles
            {
                get
                {
                    int bitsCHAR   = UnFRS__LEN_CHAR__get  ( this.UnFRS );
                    int bitsPARITY = UnFRS__LEN_PARITY__get( this.UnFRS );
                    int bitsSTOP   = UnFRS__LEN_STOPS__get ( this.UnFRS );
                    int bits       = 1 + bitsCHAR + bitsPARITY + bitsSTOP;

                    return (int)(bits * this.BaudRateInClockCycles);
                }
            }

            private float BaudRateInClockCycles
            {
                get
                {
                    int oversample =  UnOVSR__get    ( this.UnOVSR );
                    int divisor    = (UnPSR__DIV__get( this.UnPSR  ) + UnBAUD__get( this.UnBAUD )) + 1;
                    int prescaler  =  UnPSR__PSC__get( this.UnPSR  ) + 1;

                    return (oversample * divisor * prescaler) / 2.0f;
                }
            }

            //--//

            public bool TransmitBufferEmpty
            {
                get
                {
                    return TestBitField( m_state_UnICTRL, UnICTRL__TBE );
                }

                private set
                {
                    UpdateBitField( ref m_state_UnICTRL, UnICTRL__TBE, value );

                    UpdateInterruptStatus();

                    AdvanceStateMachine();
                }
            }

            public bool ReadBufferFull
            {
                get
                {
                    return TestBitField( m_state_UnICTRL, UnICTRL__RBF );
                }

                private set
                {
                    UpdateBitField( ref m_state_UnICTRL, UnICTRL__RBF, value );

                    UpdateInterruptStatus();

                    AdvanceStateMachine();
                }
            }

            public bool ShiftRegisterTxBusy
            {
                get
                {
                    return m_txShiftRegisterActive;
;
                }

                private set
                {
                    UpdateBitField( ref m_state_UnSTAT, UnSTAT__XMIP, value );

                    m_txShiftRegisterActive = value;
                }
            }

            public bool ShiftRegisterRxBusy
            {
                get
                {
                    return m_rxShiftRegisterActive;
                }

                private set
                {
                    m_rxShiftRegisterActive = value;
                }
            }
        }

        [Simulator.PeripheralRange(Base=0x38040000U,Length=0x00010000U,ReadLatency=1,WriteLatency=2)]
        public class USART0 : USARTx
        {
            public override void OnConnected()
            {
                base.OnConnected();

                m_portNumber = 0;
            }
        }

        [Simulator.PeripheralRange(Base=0x38050000U,Length=0x00010000U,ReadLatency=1,WriteLatency=2)]
        public class USART1 : USARTx
        {
            public override void OnConnected()
            {
                base.OnConnected();

                m_portNumber = 1;
            }
        }

        [Simulator.PeripheralRange(Base=0x38060000U,Length=0x00000100U,ReadLatency=1,WriteLatency=2)]
        public class USB : Simulator.Peripheral
        {
////        struct ENDPNT
////        {
////            /****/ volatile UINT8 EPCT;
////            // for EP0
////            static const    UINT8 EPCT__EPC0_MASK               = 0xDF;
////            static const    UINT8 EPCT__EPC0_EP                 = 0x0F;
////            static const    UINT8 USB_SETUP_FIX_DIS             = 0x10;
////
////            static const    UINT8 EPCT__EPC0_DEF                = 0x40;
////            static const    UINT8 EPCT__EPC0_STALL              = 0x80;
////            // for EP1-3
////            static const    UINT8 EPCT__EPC_MASK                = 0xBF;
////            static const    UINT8 EPCT__EPC_EP                  = 0x0F;
////            static const    UINT8 EPCT__EPC_EP_EN               = 0x10;
////            static const    UINT8 EPCT__EPC_ISO                 = 0x20;
////            static const    UINT8 EPCT__EPC_STALL               = 0x80;
////            /*************/ UINT8 padding1[3];
////
////            /****/ volatile UINT8 TXD;
////            static const    UINT8 TXD__mask                     = 0xFF;
////            /*************/ UINT8 padding2[3];
////
////            /****/ volatile UINT8 TXS;
////            // for EP0
////            static const    UINT8 TXS__TXS0_MASK                = 0x6F;
////            static const    UINT8 TXS__TXS0_TCOUNT              = 0x0F;
////            static const    UINT8 TXS__TXS0_TX_DONE             = 0x20;
////            static const    UINT8 TXS__TXS0_ACK_STAT            = 0x40;
////            // for EP1-3
////            static const    UINT8 TXS__TXS_MASK                 = 0xFF;
////            static const    UINT8 TXS__TXS_TCOUNT               = 0x1F;
////            static const    UINT8 TXS__TXS_TX_DONE              = 0x20;
////            static const    UINT8 TXS__TXS_ACK_STAT             = 0x40;
////            static const    UINT8 TXS__TXS_TX_URUN              = 0x80;
////            /*************/ UINT8 padding3[3];
////
////            /****/ volatile UINT8 TXC;
////            // for EP0
////            static const    UINT8 TXC__TXC0_MASK                = 0x14;
////            static const    UINT8 TXC__TXC0_TX_EN               = 0x01;
////            static const    UINT8 TXC__TXC0_TOGGLE              = 0x04;
////            static const    UINT8 TXC__TXC0_FLUSH               = 0x08;
////            static const    UINT8 TXC__TXC0_IGN_IN              = 0x10;
////            // for EP1-3
////            static const    UINT8 TXC__TXC_MASK                 = 0xE4;
////            static const    UINT8 TXC__TXC_TX_EN                = 0x01;
////            static const    UINT8 TXC__TXC_LAST                 = 0x02;
////            static const    UINT8 TXC__TXC_TOGGLE               = 0x04;
////            static const    UINT8 TXC__TXC_FLUSH                = 0x08;
////            static const    UINT8 TXC__TXC_RFF                  = 0x10;
////            static const    UINT8 TXC__TXC_TFWL                 = 0x60;
////            static const    UINT8 TXC__TXC_IGN_ISOMSK           = 0x80;
////            /*************/ UINT8 padding4[3];
////
////            /****/ volatile UINT8 EPCR; // EPCR use EPCT macro
////            /*************/ UINT8 padding5[3];
////
////            /****/ volatile UINT8 RXD;
////            static const    UINT8 RXD__mask                     = 0xFF;
////            /*************/ UINT8 padding6[3];
////
////            /****/ volatile UINT8 RXS;
////            // for EP0
////            static const    UINT8 RXS__RXS0_MASK                = 0x7F;
////            static const    UINT8 RXS__RXS0_RCOUNT              = 0x0F;
////            static const    UINT8 RXS__RXS0_RX_LAST             = 0x10;
////            static const    UINT8 RXS__RXS0_TOGGLE              = 0x20;
////            static const    UINT8 RXS__RXS0_SETUP               = 0x40;
////            // for EP1-3
////            static const    UINT8 RXS__RXS_MASK                 = 0xFF;
////            static const    UINT8 RXS__RXS_RCOUNT               = 0x0F;
////            static const    UINT8 RXS__RXS_RX_LAST              = 0x10;
////            static const    UINT8 RXS__RXS_TOGGLE               = 0x20;
////            static const    UINT8 RXS__RXS_SETUP                = 0x40;
////            static const    UINT8 RXS__RXS_RX_ERR               = 0x80;
////            /*************/ UINT8 padding7[3];
////
////            /****/ volatile UINT8 RXC;
////            // for EP0
////            static const    UINT8 RXC__RXC0_MASK                = 0x06;
////            static const    UINT8 RXC__RXC0_RX_EN               = 0x01;
////            static const    UINT8 RXC__RXC0_IGN_OUT             = 0x02;
////            static const    UINT8 RXC__RXC0_IGN_SETUP           = 0x04;
////            static const    UINT8 RXC__RXC0_FLUSH               = 0x08;
////            // for EP1-3
////            static const    UINT8 RXC__RXC_MASK                 = 0x64;
////            static const    UINT8 RXC__RXC_RX_EN                = 0x01;
////            static const    UINT8 RXC__RXC_IGN_SETUP            = 0x04;
////            static const    UINT8 RXC__RXC_FLUSH                = 0x08;
////            static const    UINT8 RXC__RXC_RFWL                 = 0x60;
////            /*************/ UINT8 padding8[3];
////        };
////
////        //--//
////
////        static const UINT32 c_Base = 0xB8060000;
////
////        //--//
////
////        /****/ volatile UINT8  MCNTRL;
////        static const    UINT8  MCNTRL__MASK             = 0x09;
////        static const    UINT8  MCNTRL__USBEN            = 0x01;
////        static const    UINT8  MCNTRL__DBG              = 0x02;
////        static const    UINT8  MCNTRL__NAT              = 0x08;
////        /*************/ UINT8  Padding1[3];
////
////        /****/ volatile UINT8  XCVRDIAG;
////        /*************/ UINT8  Padding2[3];
////
////        /****/ volatile UINT8  TCR;
////        /*************/ UINT8  Padding3[3];
////
////        /****/ volatile UINT8  UTR;
////        /*************/ UINT8  Padding4[3];
////
////        /****/ volatile UINT8  FAR_;
////        static const    UINT8  FAR__FAR_MASK            = 0xFF;
////        static const    UINT8  FAR__FAR_AD              = 0x7F;
////        static const    UINT8  FAR__FAR_AD_EN           = 0x80;
////        /*************/ UINT8  Padding5[3];
////
////        /****/ volatile UINT8  NFSR;
////        static const    UINT8  NFSR__STATE_NODE_MASK         = 0x03;
////        static const    UINT8  NFSR__STATE_NODE_RESET        = 0x00;
////        static const    UINT8  NFSR__STATE_NODE_RESUME       = 0x01;
////        static const    UINT8  NFSR__STATE_NODE_OPERATIONAL  = 0x02;
////        static const    UINT8  NFSR__STATE_NODE_SUSPEND      = 0x03;
////        /*************/ UINT8  Padding6[3];
////
////        /****/ volatile UINT8  MAEV;
////        static const    UINT8  MAEV__MASK                    = 0xFF;
////        static const    UINT8  MAEV__WARN                    = 0x01;
////        static const    UINT8  MAEV__ALT                     = 0x02;
////        static const    UINT8  MAEV__TX_EV                   = 0x04;
////        static const    UINT8  MAEV__FRAME                   = 0x08;
////        static const    UINT8  MAEV__NAK                     = 0x10;
////        static const    UINT8  MAEV__ULD                     = 0x20;
////        static const    UINT8  MAEV__RX_EV                   = 0x40;
////        static const    UINT8  MAEV__INTR                    = 0x80;
////        /*************/ UINT8  Padding7[3];
////
////        /****/ volatile UINT8  MAMSK;
////        static const    UINT8  MAMSK__MASK                   = 0xFF;
////        static const    UINT8  MAMSK__WARN                   = 0x01;
////        static const    UINT8  MAMSK__ALT                    = 0x02;
////        static const    UINT8  MAMSK__TX_EV                  = 0x04;
////        static const    UINT8  MAMSK__FRAME                  = 0x08;
////        static const    UINT8  MAMSK__NAK                    = 0x10;
////        static const    UINT8  MAMSK__ULD                    = 0x20;
////        static const    UINT8  MAMSK__RX_EV                  = 0x40;
////        static const    UINT8  MAMSK__INTR                   = 0x80;
////        /*************/ UINT8  Padding8[3];
////
////        /****/ volatile UINT8  ALTEV;
////        static const    UINT8  ALTEV__MASK                   = 0xFC;
////        static const    UINT8  ALTEV__DMA                    = 0x04;
////        static const    UINT8  ALTEV__EOP                    = 0x08;
////        static const    UINT8  ALTEV__SD3                    = 0x10;
////        static const    UINT8  ALTEV__SD5                    = 0x20;
////        static const    UINT8  ALTEV__RESET                  = 0x40;
////        static const    UINT8  ALTEV__RESUME                 = 0x80;
////        /*************/ UINT8  Padding9[3];
////
////        /****/ volatile UINT8  ALTMSK;
////        static const    UINT8  ALTMSK__MASK                  = 0xFC;
////        static const    UINT8  ALTMSK__DMA                   = 0x04;
////        static const    UINT8  ALTMSK__EOP                   = 0x08;
////        static const    UINT8  ALTMSK__SD3                   = 0x10;
////        static const    UINT8  ALTMSK__SD5                   = 0x20;
////        static const    UINT8  ALTMSK__RESET                 = 0x40;
////        static const    UINT8  ALTMSK__RESUME                = 0x80;
////        /*************/ UINT8  Padding10[3];
////
////        /****/ volatile UINT8  TXEV;
////        static const    UINT8  TXEV__MASK                    = 0xFF;
////        static const    UINT8  TXEV__FIFO_ALL                = 0x0F;
////        static const    UINT8  TXEV__FIFO_EP0                = 0x01;
////        static const    UINT8  TXEV__UNDERRUN_ALL            = 0xF0;
////        /*************/ UINT8  Padding11[3];
////
////        __inline static UINT8  TXEV__FIFO__set    ( UINT32 n ) { return (0x01 << n); }
////        __inline static UINT8  TXEV__UNDERRUN__set( UINT32 n ) { return (0x10 << n); }
////
////        /****/ volatile UINT8  TXMSK;
////        static const    UINT8  TXMSK__MASK                   = 0xFF;
////        static const    UINT8  TXMSK__FIFO_ALL               = 0x0F;
////        static const    UINT8  TXMSK__FIFO_EP0               = 0x01;
////        static const    UINT8  TXMSK__UNDERRUN_ALL           = 0xF0;
////        /*************/ UINT8  Padding12[3];
////
////        __inline static UINT8  TXMSK__FIFO__set    ( UINT32 n ) { return (0x01 << n); }
////        __inline static UINT8  TXMSK__UNDERRUN__set( UINT32 n ) { return (0x10 << n); }
////
////        /****/ volatile UINT8  RXEV;
////        static const    UINT8  RXEV__MASK                    = 0xFF;
////        static const    UINT8  RXEV__FIFO_ALL                = 0x0F;
////        static const    UINT8  RXEV__FIFO_EP0                = 0x01;
////        static const    UINT8  RXEV__OVERRUN_ALL             = 0xF0;
////        /*************/ UINT8  Padding13[3];
////
////        __inline static UINT8  RXEV__FIFO__set   ( UINT32 n ) { return (0x01 << n); }
////        __inline static UINT8  RXEV__OVERRUN__set( UINT32 n ) { return (0x10 << n); }
////
////        /****/ volatile UINT8  RXMSK;
////        static const    UINT8  RXMSK__MASK                   = 0xFF;
////        static const    UINT8  RXMSK__FIFO_ALL               = 0x0F;
////        static const    UINT8  RXMSK__FIFO_EP0               = 0x01;
////        static const    UINT8  RXMSK__OVERRUN_ALL            = 0xF0;
////        /*************/ UINT8  Padding14[3];
////
////        __inline static UINT8  RXMSK__FIFO__set   ( UINT32 n ) { return (0x01 << n); }
////        __inline static UINT8  RXMSK__OVERRUN__set( UINT32 n ) { return (0x10 << n); }
////
////        /****/ volatile UINT8  NAKEV;
////        static const    UINT8  NAKEV__MASK                   = 0xFF;
////        static const    UINT8  NAKEV__IN_ALL                 = 0x0F;
////        static const    UINT8  NAKEV__OUT_ALL                = 0xF0;
////        /*************/ UINT8  Padding15[3];
////
////        __inline static UINT8  NAKEV__IN__set ( UINT32 n ) { return (0x01 << n); }
////        __inline static UINT8  NAKEV__OUT__set( UINT32 n ) { return (0x10 << n); }
////
////        /****/ volatile UINT8  NAKMSK;
////        static const    UINT8  NAKMSK__MASK                  = 0xFF;
////        static const    UINT8  NAKMSK__IN_ALL                = 0x0F;
////        static const    UINT8  NAKMSK__OUT_ALL               = 0xF0;
////        /*************/ UINT8  Padding16[3];
////
////        __inline static UINT8  NAKMSK__IN__set ( UINT32 n ) { return (0x01 << n); }
////        __inline static UINT8  NAKMSK__OUT__set( UINT32 n ) { return (0x10 << n); }
////
////        /****/ volatile UINT8  FWEV;
////        static const    UINT8  FWEV__MASK                    = 0xEE;
////        static const    UINT8  FWEV__TXWARN                  = 0x0E;
////        static const    UINT8  FWEV__RXWARN                  = 0xE0;
////        /*************/ UINT8  Padding17[3];
////
////        /****/ volatile UINT8  FWMSK;
////        static const    UINT8  FWMSK__MASK                   = 0xEE;
////        static const    UINT8  FWMSK__TXWARN                 = 0x0E;
////        static const    UINT8  FWMSK__RXWARN                 = 0xE0;
////        /*************/ UINT8  Padding18[3];
////
////        /****/ volatile UINT8  FNH;
////        static const    UINT8  FNH__MASK                     = 0xE7;
////        static const    UINT8  FNH__FN                       = 0x07;
////        static const    UINT8  FNH__RFC                      = 0x20;
////        static const    UINT8  FNH__UL                       = 0x40;
////        static const    UINT8  FNH__MF                       = 0x80;
////        /*************/ UINT8  Padding19[3];
////
////        /****/ volatile UINT8  FNL;
////        static const    UINT8  FNL__MASK                     = 0xFF;
////        static const    UINT8  FNL__FN                       = 0xFF;
////        /*************/ UINT8  Padding20[3];
////
////        /****/ volatile UINT8  DMACNTRL;
////        static const    UINT8  DMACNTRL__MASK                = 0xFF;
////        static const    UINT8  DMACNTRL__DSRC                = 0x07;
////        static const    UINT8  DMACNTRL__DMOD                = 0x08;
////        static const    UINT8  DMACNTRL__ADMA                = 0x10;
////        static const    UINT8  DMACNTRL__DTGL                = 0x20;
////        static const    UINT8  DMACNTRL__IGNRXTGL            = 0x40;
////        static const    UINT8  DMACNTRL__DEN                 = 0x80;
////        /*************/ UINT8  Padding21[3];
////
////        /****/ volatile UINT8  DMAEV;
////        static const    UINT8  DMAEV__MASK                   = 0x3F;
////        static const    UINT8  DMAEV__DSHLT                  = 0x01;
////        static const    UINT8  DMAEV__DERR                   = 0x02;
////        static const    UINT8  DMAEV__DCNT                   = 0x04;
////        static const    UINT8  DMAEV__DSIZ                   = 0x08;
////        static const    UINT8  DMAEV__NTGL                   = 0x20;
////        static const    UINT8  USB_DMAEV_ARDY                = 0x10;
////        /*************/ UINT8  Padding22[3];
////
////        /****/ volatile UINT8  DMAMSK;
////        static const    UINT8  DMAMSK__MASK                  = 0x2F;
////        static const    UINT8  DMAMSK__DSHLT                 = 0x01;
////        static const    UINT8  DMAMSK__DERR                  = 0x02;
////        static const    UINT8  DMAMSK__DCNT                  = 0x04;
////        static const    UINT8  DMAMSK__DSIZ                  = 0x08;
////        static const    UINT8  DMAMSK__NTGL                  = 0x20;
////        /*************/ UINT8  Padding23[3];
////
////        /****/ volatile UINT8  MIR;
////        static const    UINT8  MIR__MASK                     = 0xFF;
////        static const    UINT8  MIR__STAT                     = 0xFF;
////        /*************/ UINT8  Padding24[3];
////
////        /****/ volatile UINT8  DMACNT;
////        static const    UINT8  DMACNT__MASK                  = 0xFF;
////        static const    UINT8  DMACNT__DCOUNT                = 0xFF;
////        /*************/ UINT8  Padding25[3];
////
////        /****/ volatile UINT8  DMAERR;
////        static const    UINT8  DMAERR__MASK                  = 0xFF;
////        static const    UINT8  DMAERR__DMAERRCNT             = 0x7F;
////        static const    UINT8  DMAERR__AEH                   = 0x80;
////        /*************/ UINT8  Padding26[3];
////
////        /****/ volatile UINT8  WAKEUP;
////        /*************/ UINT8  Padding27[3];
////
////        /*************/ UINT32 Padding28[5];
////
////        /*************/ ENDPNT EP[4];
        }

        [Simulator.PeripheralRange(Base=0x38070000U,Length=0x00000200U,ReadLatency=1,WriteLatency=2)]
        public class GPIO : Simulator.Peripheral
        {
            public delegate void NotifyOnChange( uint index, bool fSet );

            public class CW : Simulator.Peripheral
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

                public const ushort Config_Mask      = (DOUT_IEN | RES_mask | MODE_mask | DB_EN);

                [Simulator.LinkToContainer()           ] public GPIO   Parent;
                [Simulator.Register(Offset=0x00000000U)] public ushort Data
                {
                    get
                    {
                        return (ushort)this.RawValue;
                    }

                    set
                    {
                        this.RawValue = value;
                    }
                }

                private ushort m_state_Data;
                bool           m_fStatus;

                //--//

                //
                // Helper Methods
                //

                private void UpdateStatus( bool fOldStatus ,
                                           bool fNewStatus )
                {
                    switch(m_state_Data & MODE_mask)
                    {
                        case MODE_INTRL :
                            if(fNewStatus == false)
                            {
                                SetBitField( ref m_state_Data, INTR_RAW );
                            }
                            else
                            {
                                ClearBitField( ref m_state_Data, INTR_RAW );
                            }
                            break;

                        case MODE_INTRH:
                            if(fNewStatus == true)
                            {
                                SetBitField( ref m_state_Data, INTR_RAW );
                            }
                            else
                            {
                                ClearBitField( ref m_state_Data, INTR_RAW );
                            }
                            break;

                        case MODE_INTRNE:
                            if(fOldStatus == true && fNewStatus == false)
                            {
                                SetBitField( ref m_state_Data, INTR_RAW );
                            }
                            break;

                        case MODE_INTRPE:
                            if(fOldStatus == false && fNewStatus == true)
                            {
                                SetBitField( ref m_state_Data, INTR_RAW );
                            }
                            break;

                        default:
                            ClearBitField( ref m_state_Data, INTR_RAW );
                            break;
                    }

                    this.Parent.Notify( this, fOldStatus, fNewStatus );

                    this.Parent.UpdateInterrupts();
                }

                internal void ResetInterrupt()
                {
                    switch(m_state_Data & MODE_mask)
                    {
                        case MODE_INTRNE:
                        case MODE_INTRPE:
                            ClearBitField( ref m_state_Data, INTR_RAW );

                            this.Parent.UpdateInterrupts();
                            break;
                    }
                }

                //
                // Access Methods
                //

                public uint RawValue
                {
                    get
                    {
                        uint res = (uint)m_state_Data & Config_Mask;

                        if(m_fStatus)
                        {
                            res |= PIN;
                        }

                        switch(m_state_Data & MODE_mask)
                        {
                            case MODE_INTRL :
                            case MODE_INTRH :
                            case MODE_INTRNE:
                            case MODE_INTRPE:
                                if((m_state_Data & INTR_RAW) != 0)
                                {
                                    res |= INTR_RAW;

                                    if((res & DOUT_IEN) != 0)
                                    {
                                        res |= INTR_STAT;
                                    }
                                }
                                break;
                        }

                        return res;
                    }

                    set
                    {
                        if(this.RawValue != value)
                        {
                            bool fOldStatus = m_fStatus;

                            //
                            // Update the configuration, leave interrupt status untouched.
                            //
                            ClearBitField( ref m_state_Data,                  Config_Mask  );
                            SetBitField  ( ref m_state_Data, (ushort)(value & Config_Mask) );

                            if(TestBitField( value, INTR_STAT | INTR_RAW ))
                            {
                                this.ResetInterrupt();
                            }

                            switch(m_state_Data & MODE_mask)
                            {
                                case MODE_GPOUT:
                                    m_fStatus = (value & DOUT_IEN) != 0;
                                    break;
                            }

                            UpdateStatus( fOldStatus, m_fStatus );
                        }
                    }
                }

                public bool PinStatus
                {
                    get
                    {
                        return m_fStatus;
                    }

                    set
                    {
                        bool fOldStatus = m_fStatus;

                        if(fOldStatus != value)
                        {
                            switch(m_state_Data & MODE_mask)
                            {
                                case MODE_GPOUT:
                                    break;

                                default:
                                    m_fStatus = value;

                                    UpdateStatus( fOldStatus, value );
                                    break;
                            }
                        }
                    }
                }

                public bool RawInterrupt
                {
                    get
                    {
                        return TestBitField( this.RawValue, INTR_RAW );
                    }
                }

                public bool Interrupt
                {
                    get
                    {
                        return TestBitField( this.RawValue, INTR_STAT );
                    }
                }
            }

            public class PIN8 : Simulator.Peripheral
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

                [Simulator.LinkToContainer()           ] public GPIO Parent;
                [Simulator.Register(Offset=0x00000000U)] public byte PIN_DIN8
                {
                    get
                    {
                        uint res    = 0;
                        uint offset = this.Parent.Index( this );

                        for(int i = 0; i < 8; i++)
                        {
                            if(this.Parent.Control[offset + i].PinStatus)
                            {
                                res = 1u << i;
                            }
                        }

                        return (byte)res;
                    }
                }

                [Simulator.Register(Offset=0x00000004U)] public byte DATA_OUT8
                {
                    get
                    {
                        uint res    = 0;
                        uint offset = this.Parent.Index( this );

                        for(int i = 0; i < 8; i++)
                        {
                            if(this.Parent.Control[offset + i].PinStatus)
                            {
                                res = 1u << i;
                            }
                        }

                        return (byte)res;
                    }

                    set
                    {
                        uint offset = this.Parent.Index( this );

                        for(int i = 0; i < 8; i++)
                        {
                            var ctrl = this.Parent.Control[offset + i];

                            if((value & (1u << i)) != 0)
                            {
                                ctrl.RawValue |= CW.DOUT_IEN;
                            }
                            else
                            {
                                ctrl.RawValue &= ~(uint)CW.DOUT_IEN;
                            }
                        }
                    }
                }

                [Simulator.Register(Offset=0x00000008U)] public byte INTR_STAT8;

                [Simulator.Register(Offset=0x0000000CU)] public byte INTR_RAW8
                {
                    get
                    {
                        uint res    = 0;
                        uint offset = this.Parent.Index( this );

                        for(int i = 0; i < 8; i++)
                        {
                            if(this.Parent.Control[offset + i].Interrupt)
                            {
                                res = 1u << i;
                            }
                        }

                        return (byte)res;
                    }

                    set
                    {
                        ResetInterrupt( value );
                    }
                }

                [Simulator.Register(Offset=0x00000010U)] public byte DBCLK_SEL;       // only valid in array offset 0 (+0x0110)

                //
                // Helper Methods
                //

                private void ResetInterrupt( uint value )
                {
                    uint offset = this.Parent.Index( this );

                    for(int i = 0; i < 8; i++)
                    {
                        if((value & (1u << i)) != 0)
                        {
                            this.Parent.Control[offset + i].ResetInterrupt();
                        }
                    }
                }
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

            [Simulator.LinkToPeripheral()                                  ] public INTC   InterruptController;
            [Simulator.Register(Offset=0x00000000U,Size=0x04U,Instances=64)] public CW[]   Control;
            [Simulator.Register(Offset=0x00000100U,Size=0x20U,Instances= 8)] public PIN8[] Pin8;

            NotifyOnChange[] m_callbacks = new NotifyOnChange[64];

            //--//

            uint Index( PIN8 obj )
            {
                return (uint)(Array.IndexOf( this.Pin8, obj ) * 8);
            }

            uint Index( CW obj )
            {
                return (uint)(Array.IndexOf( this.Control, obj ));
            }

            void Notify( CW   control    ,
                         bool fOldStatus ,
                         bool fNewStatus )
            {
                uint pin = Index( control );

                NotifyOnChange dlg = m_callbacks[pin];
                if(dlg != null)
                {
                    dlg( pin, fNewStatus );
                }
            }

            void UpdateInterrupts()
            {
                for(uint group = 0; group < 8; group++)
                {
                    int index = -1;

                    switch(group)
                    {
                        case 0: index = INTC.IRQ_INDEX_GPIO_00_07; break;
                        case 1: index = INTC.IRQ_INDEX_GPIO_08_15; break;
                        case 2: index = INTC.IRQ_INDEX_GPIO_16_23; break;
                        case 3: index = INTC.IRQ_INDEX_GPIO_24_31; break;
                        case 4: index = INTC.IRQ_INDEX_GPIO_32_39; break;
                    }

                    if(index >= 0)
                    {
                        bool fRaise = false;

                        for(uint pin = 0; pin < 8; pin++)
                        {
                            if(this.Control[group * 8 + pin].Interrupt)
                            {
                                fRaise = true;
                                break;
                            }
                        }

                        if(fRaise)
                        {
                            this.InterruptController.Set( index );
                        }
                        else
                        {
                            this.InterruptController.Reset( index );
                        }
                    }
                }
            }

            //--//

            public void Register( uint           pin ,
                                  NotifyOnChange dlg )
            {
                m_callbacks[pin] += dlg;
            }

            public void Unregister( uint           pin ,
                                    NotifyOnChange dlg )
            {
                m_callbacks[pin] -= dlg;
            }

            public bool ReadPin( uint pin )
            {
                return this.Control[pin].PinStatus;
            }

            public void SetPin( uint pin )
            {
                this.Control[pin].PinStatus = true;
            }

            public void ResetPin( uint pin )
            {
                this.Control[pin].PinStatus = false;
            }
        }

        [Simulator.PeripheralRange(Base=0x38090000U,Length=0x00000080U,ReadLatency=1,WriteLatency=2)]
        public class SECURITYKEY : Simulator.Peripheral
        {
            public class BYTE : Simulator.Peripheral
            {
                [Simulator.LinkToContainer()           ] public SECURITYKEY Parent;
                [Simulator.Register(Offset=0x00000000U)] public byte        Data8;
            }

            //--//

            [Simulator.Register(Offset=0x00000000U,Size=4,Instances=32)] public BYTE[] Key;
        }

        [Simulator.PeripheralRange(Base=0x380A0000U,Length=0x00000014U,ReadLatency=1,WriteLatency=2)]
        public class MWSPI : Simulator.Peripheral, Hosting.ISynchronousSerialInterfaceController
        {
            public const uint c_MS1LE  = GPIO.c_Pin_07;
            public const uint c_MSK    = GPIO.c_Pin_16;
            public const uint c_MDIDO  = GPIO.c_Pin_17;
            public const uint c_MDODI  = GPIO.c_Pin_18;
            public const uint c_MSC0LE = GPIO.c_Pin_19;

            //--//

            public const ushort MWnCTL1__MWEN             = 0x0001;
            public const ushort MWnCTL1__MNS_SLAVE        = 0x0000;
            public const ushort MWnCTL1__MNS_MASTER       = 0x0002;
            public const ushort MWnCTL1__MOD_8            = 0x0000;
            public const ushort MWnCTL1__MOD_16           = 0x0004;
            public const ushort MWnCTL1__ECHO             = 0x0008;
            public const ushort MWnCTL1__EIF              = 0x0010;
            public const ushort MWnCTL1__EIR              = 0x0020;
            public const ushort MWnCTL1__EIW              = 0x0040;
            public const ushort MWnCTL1__SCM_NORMAL       = 0x0000;
            public const ushort MWnCTL1__SCM_ALTERNATE    = 0x0080;
            public const ushort MWnCTL1__SCIDL_MSK0       = 0x0000;
            public const ushort MWnCTL1__SCIDL_MSK1       = 0x0100;

            public static ushort MWnCTL1__SCDV__set( uint   d ) { return (ushort)((d & 0x7F) << 9); }
            public static uint   MWnCTL1__SCDV__get( ushort d ) { return ((uint)d >> 9) & 0x7F; }

            public const ushort MWnSTAT__TBF              = 0x0001;
            public const ushort MWnSTAT__RBF              = 0x0002;
            public const ushort MWnSTAT__OVR              = 0x0004;
            public const ushort MWnSTAT__UDR              = 0x0008;
            public const ushort MWnSTAT__BSY              = 0x0010;

            public const ushort MWnCTL2__EDR              = 0x0001;
            public const ushort MWnCTL2__EDW              = 0x0002;
            public const ushort MWnCTL2__LEE0             = 0x0004;
            public const ushort MWnCTL2__LEE1             = 0x0008;
            public const ushort MWnCTL2__LEMD0            = 0x0010;
            public const ushort MWnCTL2__LEMD1            = 0x0020;
            public const ushort MWnCTL2__LEPL0            = 0x0040;
            public const ushort MWnCTL2__LEPL1            = 0x0080;
            public const ushort MWnCTL2__DTMD_mask        = 0x0300;
            public const ushort MWnCTL2__DTMD_FULL_DUPLEX = 0x0000;
            public const ushort MWnCTL2__DTMD_READ_ONLY   = 0x0100;
            public const ushort MWnCTL2__DTMD_WRITE_ONLY  = 0x0200;
            public const ushort MWnCTL2__FNCLE            = 0x0400;
            public const ushort MWnCTL2__CNTLE            = 0x0800;

            //--//

            [Simulator.LinkToPeripheral()          ] public INTC   InterruptController;
            [Simulator.LinkToPeripheral()          ] public CMU    ClockController;
            [Simulator.LinkToPeripheral()          ] public GPIO   InputOutput;

            [Simulator.Register(Offset=0x00000000U)] public ushort MWnDAT
            {
                get
                {
                    if(this.IsClockEnabled)
                    {
                        ushort res = m_readBuffer;

                        if(m_owner.AreTimingUpdatesEnabled)
                        {
                            this.ReadBufferFull = false;

                            Emulation.Hosting.MonitorExecution svcME;

                            if(m_owner.GetHostingService( out svcME ) && svcME.MonitorOpcodes)
                            {
                                Hosting.OutputSink sink;
                                
                                if(m_owner.GetHostingService( out sink ))
                                {
                                    sink.OutputLine( "SPI READ: {0} {1:X4}", m_owner.ClockTicks, res );
                                }
                            }
                        }

                        return res;
                    }
                    else
                    {
                        return 0;
                    }
                }

                set
                {
                    if(this.IsClockEnabled)
                    {
                        if(m_owner.AreTimingUpdatesEnabled)
                        {
                            if(this.IsEnabled)
                            {
                                Emulation.Hosting.MonitorExecution svcME;

                                if(m_owner.GetHostingService( out svcME ) && svcME.MonitorOpcodes)
                                {
                                    Hosting.OutputSink sink;
                                    
                                    if(m_owner.GetHostingService( out sink ))
                                    {
                                        sink.OutputLine( "SPI WRITE: {0} {1:X4}", m_owner.ClockTicks, value );
                                    }
                                }

                                m_transmitBuffer = value;

                                this.TransmitBufferFull = true;
                            }
                        }
                    }
                }
            }

            [Simulator.Register(Offset=0x00000004U)] public ushort MWnCTL1
            {
                get
                {
                    if(this.IsClockEnabled)
                    {
                        return m_state_MWnCTL1;
                    }
                    else
                    {
                        return 0;
                    }
                }

                set
                {
                    if(this.IsClockEnabled)
                    {
                        if(m_owner.AreTimingUpdatesEnabled)
                        {
                            bool fNotify = false;

                            m_state_MWnCTL1 = value;

                            if(this.IsEnabled == false)
                            {
                                ClearBitField( ref m_state_MWnSTAT, MWnSTAT__BSY | MWnSTAT__UDR | MWnSTAT__OVR | MWnSTAT__RBF | MWnSTAT__TBF );
                            }
                            else
                            {
                                fNotify = (this.IsMasterMode == false);
                            }

                            if(fNotify)
                            {
                                this.InputOutput.Register( c_MSC0LE, NotifyChipSelect );
                            }
                            else
                            {
                                this.InputOutput.Unregister( c_MSC0LE, NotifyChipSelect );
                            }
                        }
                    }
                }
            }
            [Simulator.Register(Offset=0x00000008U)] public ushort MWnSTAT
            {
                get
                {
                    if(this.IsClockEnabled)
                    {
                        return m_state_MWnSTAT;
                    }
                    else
                    {
                        return 0;
                    }
                }

                set
                {
                    if(this.IsClockEnabled)
                    {
                        if(m_owner.AreTimingUpdatesEnabled)
                        {
                            if(TestBitField( value, MWnSTAT__OVR ))
                            {
                                ClearBitField( ref m_state_MWnSTAT, MWnSTAT__OVR );
                            }

                            if(TestBitField( value, MWnSTAT__UDR ))
                            {
                                ClearBitField( ref m_state_MWnSTAT, MWnSTAT__UDR );
                            }

                            UpdateInterruptStatus();
                        }
                    }
                }
            }

            [Simulator.Register(Offset=0x0000000CU)] public ushort MWnCTL2
            {
                get
                {
                    if(this.IsClockEnabled)
                    {
                        return m_state_MWnCTL2;
                    }
                    else
                    {
                        return 0;
                    }
                }

                set
                {
                    if(this.IsClockEnabled)
                    {
                        const ushort unsupported = MWnCTL2__EDR   |
                                                   MWnCTL2__EDW   |
                                                   MWnCTL2__LEE0  |
                                                   MWnCTL2__LEE1  |
                                                   MWnCTL2__LEMD0 |
                                                   MWnCTL2__LEMD1 |
                                                   MWnCTL2__LEPL0 |
                                                   MWnCTL2__LEPL1 |
                                                   MWnCTL2__FNCLE |
                                                   MWnCTL2__CNTLE;

                        if(TestBitField( value, unsupported ))
                        {
                            throw new NotSupportedException( string.Format( "Unsupported use of {0}", this.GetType() ) );
                        }

                        m_state_MWnCTL2 = value;
                    }
                }
            }

            [Simulator.Register(Offset=0x00000010U)] public ushort MWnTEST;

            ushort m_state_MWnCTL1;
            ushort m_state_MWnSTAT;
            ushort m_state_MWnCTL2;

            ushort m_shiftRegister;
            bool   m_shiftRegisterTX;
            bool   m_shiftRegisterRX;
            ushort m_readBuffer;
            ushort m_transmitBuffer;
            bool   m_fAdvancingStateMachine;

            //--//

            //
            // Interface Methods
            //

            uint Hosting.ISynchronousSerialInterfaceController.ShiftData( uint value          ,
                                                                          int  bitSize        ,
                                                                          int  clockFrequency )
            {
                uint res = 0;

                if(this.IsClockEnabled && this.IsEnabled)
                {
                    if(this.IsMasterMode == false)
                    {
                        if(m_shiftRegisterTX == false)
                        {
                            SetBitField( ref m_state_MWnSTAT, MWnSTAT__UDR );
                        }

                        res               = m_shiftRegister;
                        m_shiftRegisterTX = false;

                        this.ShiftRegisterBusy  = true;
                        this.TransmitBufferFull = false;

                        long delay = (long)((double)bitSize * m_owner.ClockFrequency / clockFrequency);

                        Hosting.DeviceClockTicksTracking svc; m_owner.GetHostingService( out svc );

                        svc.RequestRelativeClockTickCallback( delay, delegate()
                        {
                            Emulation.Hosting.MonitorExecution svcME;

                            if(m_owner.GetHostingService( out svcME ) && svcME.MonitorOpcodes)
                            {
                                Hosting.OutputSink sink;
                                
                                if(m_owner.GetHostingService( out sink ))
                                {
                                    sink.OutputLine( "SHIFT END: {0}", m_owner.ClockTicks );
                                }
                            }

                            if(this.ShiftRegisterBusy)
                            {
                                m_shiftRegister   = (ushort)value;
                                m_shiftRegisterRX = true;

                                this.ShiftRegisterBusy = false;

                                AdvanceStateMachine();
                            }
                        } );

                        AdvanceStateMachine();
                    }
                }

                return res;
            }

            void Hosting.ISynchronousSerialInterfaceController.StartTransaction()
            {
                this.InputOutput.ResetPin( c_MSC0LE );
            }

            void Hosting.ISynchronousSerialInterfaceController.EndTransaction()
            {
                this.InputOutput.SetPin( c_MSC0LE );
            }

            //
            // Helper Methods
            //

            private void NotifyChipSelect( uint index ,
                                           bool fSet  )
            {
            }

            private void AdvanceStateMachine()
            {
                if(this.IsClockEnabled && this.IsEnabled)
                {
                    if(this.ShiftRegisterBusy)
                    {
                        //
                        // Wait for the transfer to finish.
                        //
                    }
                    else
                    {
                        //
                        // Prevent recursion.
                        //
                        if(m_fAdvancingStateMachine == false)
                        {
                            m_fAdvancingStateMachine = true;

                            if(this.IsMasterMode)
                            {
                                AdvanceStateMachine_Master();
                            }
                            else
                            {
                                AdvanceStateMachine_Slave();
                            }

                            m_fAdvancingStateMachine = false;
                        }
                    }

                    UpdateInterruptStatus();
                }
            }

            private void AdvanceStateMachine_Master()
            {
                if(m_shiftRegisterRX)
                {
                    if(this.IsReadMode)
                    {
                        m_readBuffer = m_shiftRegister;

                        this.ReadBufferFull = true;
                    }

                    m_shiftRegisterRX = false;
                }

                if(m_shiftRegisterTX == false)
                {
                    if(this.TransmitBufferFull)
                    {
                        if(this.IsReadMode)
                        {
                            if(this.ReadBufferFull)
                            {
                                //
                                // Read buffer is full, we cannot start a new transfer.
                                //
                                return;
                            }
                        }

                        if(this.IsWriteMode)
                        {
                            m_shiftRegister = m_transmitBuffer;
                        }

                        m_shiftRegisterTX = true;

                        this.TransmitBufferFull = false;
                    }
                }

                if(m_shiftRegisterTX)
                {
                    this.ShiftRegisterBusy = true;

                    int  bitSize = TestBitField( m_state_MWnCTL1, MWnCTL1__MOD_16 ) ? 16 : 8;
                    uint delay   = Math.Max( MWnCTL1__SCDV__get( m_state_MWnCTL1 ), 2 );

                    Hosting.DeviceClockTicksTracking svc; m_owner.GetHostingService( out svc );

                    svc.RequestRelativeClockTickCallback( delay * bitSize, delegate()
                    {
                        if(this.IsClockEnabled && this.IsEnabled)
                        {
                            Hosting.ISynchronousSerialInterfaceBus bus;
                            
                            if(m_owner.GetHostingService( out bus ))
                            {
                                m_shiftRegister = (ushort)bus.ShiftData( m_shiftRegister, bitSize, 0 );
                            }

                            m_shiftRegisterTX = false;
                            m_shiftRegisterRX = true;

                            this.ShiftRegisterBusy = false;

                            AdvanceStateMachine();
                        }
                    } );
                }
            }

            private void AdvanceStateMachine_Slave()
            {
                if(m_shiftRegisterRX)
                {
                    if(this.IsReadMode)
                    {
                        if(this.ReadBufferFull)
                        {
                            //
                            // Read buffer is still full, new data is lost.
                            //
                            SetBitField( ref m_state_MWnSTAT, MWnSTAT__OVR );
                        }
                        else
                        {
                            m_readBuffer = m_shiftRegister;

                            this.ReadBufferFull = true;
                        }
                    }

                    m_shiftRegisterRX = false;
                }

                if(m_shiftRegisterTX == false)
                {
                    if(this.IsWriteMode)
                    {
                        if(this.TransmitBufferFull)
                        {
                            m_shiftRegister   = m_transmitBuffer;
                            m_shiftRegisterTX = true;

                            this.ShiftRegisterBusy  = true;
                            // Slave transmits are not double buffered...
                            //this.TransmitBufferFull = false;
                        }
                    }
                }
            }

            private void UpdateInterruptStatus()
            {
                bool fAssertInterrupt = false;

                if(this.IsClockEnabled && this.IsEnabled)
                {
                    if(TestBitField( m_state_MWnCTL1, MWnCTL1__EIW ))
                    {
                        if(this.TransmitBufferFull == false)
                        {
                            fAssertInterrupt = true;
                        }
                    }

                    if(TestBitField( m_state_MWnCTL1, MWnCTL1__EIR ))
                    {
                        if(this.ReadBufferFull)
                        {
                            fAssertInterrupt = true;
                        }
                    }

                    if(TestBitField( m_state_MWnCTL1, MWnCTL1__EIF ))
                    {
                        if(TestBitField( m_state_MWnSTAT, MWnSTAT__OVR ))
                        {
                            fAssertInterrupt = true;
                        }
                    }
                }

                if(fAssertInterrupt)
                {
                    this.InterruptController.Set( INTC.IRQ_INDEX_MicroWire );
                }
                else
                {
                    this.InterruptController.Reset( INTC.IRQ_INDEX_MicroWire );
                }
            }

            //
            // Access Methods
            //

            public bool IsClockEnabled
            {
                get
                {
                    return (this.ClockController.CLK_EN_REG & CMU.MCLK_EN__UWIRE) != 0;
                }
            }

            public bool IsEnabled
            {
                get
                {
                    return TestBitField( m_state_MWnCTL1, MWnCTL1__MWEN );
                }
            }

            public bool IsMasterMode
            {
                get
                {
                    return TestBitField( m_state_MWnCTL1, MWnCTL1__MNS_MASTER );
                }
            }

            public bool IsReadMode
            {
                get
                {
                    switch(m_state_MWnCTL2 & MWnCTL2__DTMD_mask)
                    {
                        case MWnCTL2__DTMD_FULL_DUPLEX:
                        case MWnCTL2__DTMD_READ_ONLY:
                            return true;
                    }

                    return false;
                }
            }

            public bool IsWriteMode
            {
                get
                {
                    switch(m_state_MWnCTL2 & MWnCTL2__DTMD_mask)
                    {
                        case MWnCTL2__DTMD_FULL_DUPLEX:
                        case MWnCTL2__DTMD_WRITE_ONLY:
                            return true;
                    }

                    return false;
                }
            }

            //--//

            public bool TransmitBufferFull
            {
                get
                {
                    return TestBitField( m_state_MWnSTAT, MWnSTAT__TBF );
                }

                private set
                {
                    UpdateBitField( ref m_state_MWnSTAT, MWnSTAT__TBF, value );

                    UpdateInterruptStatus();

                    AdvanceStateMachine();
                }
            }

            public bool ReadBufferFull
            {
                get
                {
                    return TestBitField( m_state_MWnSTAT, MWnSTAT__RBF );
                }

                private set
                {
                    UpdateBitField( ref m_state_MWnSTAT, MWnSTAT__RBF, value );

                    UpdateInterruptStatus();

                    AdvanceStateMachine();
                }
            }

            public bool ShiftRegisterBusy
            {
                get
                {
                    return TestBitField( m_state_MWnSTAT, MWnSTAT__BSY );
                }

                private set
                {
                    UpdateBitField( ref m_state_MWnSTAT, MWnSTAT__BSY, value );
                }
            }
        }

        [Simulator.PeripheralRange(Base=0x380B0000U,Length=0x00000018U,ReadLatency=1,WriteLatency=2)]
        public class CMU : Simulator.Peripheral
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

            [Simulator.Register(Offset=0x00000000U)] public uint PERF_LVL;
            [Simulator.Register(Offset=0x00000004U)] public uint CLK_SEL;
            [Simulator.Register(Offset=0x00000008U)] public byte REF_REG;

            [Simulator.Register(Offset=0x0000000CU)] public uint MCLK_EN
            {
                get
                {
                    return m_state_MCLK_EN;
                }

                set
                {
                    m_state_MCLK_EN = value;

                    //
                    // It takes 256 clock cycles to update CLK_EN_REG register.
                    //
                    Hosting.DeviceClockTicksTracking svc; m_owner.GetHostingService( out svc );

                    svc.RequestRelativeClockTickCallback( 256, delegate()
                    {
                        this.CLK_EN_REG = value;
                    } );
                }
            }

            [Simulator.Register(Offset=0x00000010U)] public uint CLK_EN_REG;
            [Simulator.Register(Offset=0x00000014U)] public uint PLLNMP;

            uint m_state_MCLK_EN;
        }

        [Simulator.PeripheralRange(Base=0x380C0000U,Length=0x00000020U,ReadLatency=1,WriteLatency=2)]
        public class RTC : Simulator.Peripheral
        {
            public const uint WDCTL__DIS                        = 0x00000000;
            public const uint WDCTL__EN                         = 0x00000001;
            public const uint WDCTL__LK                         = 0x00000002;
            public const uint WDCTL__IEN                        = 0x00000004;
            public const uint WDCTL__REN                        = 0x00000008;
            public const uint WDCTL__WDOG_PIN_DISABLED          = 0x00000000;
            public const uint WDCTL__WDOG_PIN_ACTIVE            = 0x00000010;
            public const uint WDCTL__WDOG_PIN_OPEN_DRAIN        = 0x00000020;
            public const uint WDCTL__WDOG_PIN_OPEN_DRAIN_PULLUP = 0x00000030;

            public const uint WDDLY__RESET_CLOCK_DELAY          =        512;
            public const uint WDDLY__READ_HREG                  =         12;

            public const uint WDRST__KEY                        = 0x0000005C;

            //--//

            [Simulator.LinkToPeripheral()          ] public INTC InterruptController;

            [Simulator.Register(Offset=0x00000000U)] public uint HREG_low
            {
                get
                {
                    return m_state_HREG_low;
                }

                set
                {
                    m_state_HREG_low = value;
                }
            }

            [Simulator.Register(Offset=0x00000004U)] public uint HREG_high
            {
                get
                {
                    return m_state_HREG_high;
                }

                set
                {
                    m_state_HREG_high = value;

                    m_base = Get64BitValue( m_state_HREG_low, m_state_HREG_high ) - GetCurrentTime();

                    Evaluate();
                }
            }

            [Simulator.Register(Offset=0x00000008U)] public uint COMP_low
            {
                get
                {
                    return m_state_COMP_low;
                }

                set
                {
                    m_state_COMP_low = value;

                    //
                    // A write to the first part of COMP disables interrupts.
                    //
                    m_fInterruptDisable = true;

                    Evaluate();
                }
            }

            [Simulator.Register(Offset=0x0000000CU)] public uint COMP_high
            {
                get
                {
                    return m_state_COMP_high;
                }

                set
                {
                    m_state_COMP_high = value;

                    //
                    // A write to the second part of COMP triggers the reload of the compare register.
                    //
                    m_compare           = Get64BitValue( m_state_COMP_low, m_state_COMP_high );
                    m_fInterruptDisable = false;

                    Evaluate();
                }
            }

            [Simulator.Register(Offset=0x00000010U)] public uint LD_HREG
            {
                set
                {
                    //
                    // Any value write cause the load to holding register.
                    //

                    Set64BitValue( GetCounterValue(), out m_state_HREG_low, out m_state_HREG_high );
                }
            }

            [Simulator.Register(Offset=0x00000014U)] public uint WDCTL;
            [Simulator.Register(Offset=0x00000018U)] public uint WDDLY;
            [Simulator.Register(Offset=0x0000001CU)] public uint WDRST;

            //--//

            //
            // State
            //

            uint m_state_HREG_low;
            uint m_state_HREG_high;

            uint m_state_COMP_low;
            uint m_state_COMP_high;

            double m_convertFromCpuTicks;
            double m_convertToCpuTicks;

            ulong  m_compare           = 0x0000FFFFFFFFFFFFU;
            ulong  m_base              = 0;
            bool   m_fInterruptDisable = false;

            //
            // Getter/Setter Methods
            //

            public override void OnConnected()
            {
                base.OnConnected();

                Cfg.ProcessorCategory proc = m_owner.Product.SearchValue< Cfg.ProcessorCategory >();

                m_convertFromCpuTicks = (double)proc.RealTimeClockFrequency / (double)proc.CoreClockFrequency;
                m_convertToCpuTicks   = (double)proc.CoreClockFrequency     / (double)proc.RealTimeClockFrequency;
            }

            //--//

            //
            // Helper Methods
            //

            ulong GetCurrentTime()
            {
                return ConvertFromCpuTicks( m_owner.ClockTicks );
            }

            ulong GetCounterValue()
            {
                return GetCurrentTime() + m_base;
            }

            void Evaluate()
            {
                Hosting.DeviceClockTicksTracking svc; m_owner.GetHostingService( out svc );

                svc.CancelClockTickCallback( Callback );

                if(m_fInterruptDisable)
                {
                    this.InterruptController.Reset( INTC.IRQ_INDEX_Real_Time_Clock );
                }
                else
                {
                    long diff = (long)(m_compare - GetCounterValue());

                    if(diff > 0)
                    {
                        this.InterruptController.Reset( INTC.IRQ_INDEX_Real_Time_Clock );

                        svc.RequestRelativeClockTickCallback( ConvertToCpuTicks( diff ), Callback );
                    }
                    else
                    {
                        this.InterruptController.Set( INTC.IRQ_INDEX_Real_Time_Clock );
                    }
                }
            }

            void Callback()
            {
                Evaluate();
            }

            //--//

            private ulong ConvertFromCpuTicks( ulong ticks )
            {
                return (ulong)(ticks * m_convertFromCpuTicks);
            }

            private long ConvertToCpuTicks( long ticks )
            {
                return (long)(ticks * m_convertToCpuTicks);
            }

            //
            // Access Methods
            //

            public ulong CompareTime
            {
                get
                {
                    return m_compare;
                }
            }
        }

        [Simulator.PeripheralRange(Base=0x380D0000U,Length=0x00000004U,ReadLatency=1,WriteLatency=2)]
        public class EDMAIF : Simulator.Peripheral
        {
            [Simulator.Register(Offset=0x00000000U)] public uint EMDAIF_Control;
        }

        [Simulator.PeripheralRange(Base=0x380E0000U,Length=0x00000020U,ReadLatency=1,WriteLatency=2)]
        public class PCU : Simulator.Peripheral
        {
            public const uint PCU_STATUS__LN2   = 0x00000040;
            public const uint PCU_STATUS__LN1   = 0x00000020;
            public const uint PCU_STATUS__SW1   = 0x00000010;
            public const uint PCU_STATUS__OTHER = 0x00000008;
            public const uint PCU_STATUS__RESET = 0x00000004;
            public const uint PCU_STATUS__DEAD  = 0x00000002;
            public const uint PCU_STATUS__OFF   = 0x00000001;

            [Simulator.Register(Offset=0x00000000U)] public uint CLR_SW1;
            [Simulator.Register(Offset=0x00000004U)] public uint SET_SW1;

            [Simulator.Register(Offset=0x00000008U)] public uint CLR_LN1;
            [Simulator.Register(Offset=0x0000000CU)] public uint SET_LN1;

            [Simulator.Register(Offset=0x00000010U)] public uint CLR_LN2;
            [Simulator.Register(Offset=0x00000014U)] public uint SET_LN2;

            [Simulator.Register(Offset=0x00000018U)] public uint SW_RESET;

            [Simulator.Register(Offset=0x0000001CU)] public uint PCU_STATUS;

            [Simulator.Register(Offset=0x00000020U)] public byte CLR_SW1_CNT;
            [Simulator.Register(Offset=0x00000024U)] public byte SET_SW1_CNT;
            [Simulator.Register(Offset=0x00000028U)] public byte CLR_LN1_CNT;
            [Simulator.Register(Offset=0x0000002CU)] public byte SET_LN1_CNT;
            [Simulator.Register(Offset=0x00000030U)] public byte CLR_LN2_CNT;
            [Simulator.Register(Offset=0x00000034U)] public byte SET_LN2_CNT;
        }


        //
        // Advanced Power Control
        //
        [Simulator.PeripheralRange(Base=0x380F0000U,Length=0x00000020U,ReadLatency=1,WriteLatency=2)]
        public class APC
        {
            [Simulator.Register(Offset=0x00000000U)] public byte APC_PWICMD;
            [Simulator.Register(Offset=0x00000004U)] public byte APC_PWIDATAWR;
            [Simulator.Register(Offset=0x00000008U)] public byte APC_PWIDATAWD;
            [Simulator.Register(Offset=0x00000010U)] public byte APC_CONTROL;
            [Simulator.Register(Offset=0x00000014U)] public byte APC_STATUS;
            [Simulator.Register(Offset=0x00000018U)] public byte APC_MINVDD_LIMIT;
            [Simulator.Register(Offset=0x0000001CU)] public byte APC_VDDCHK;
            [Simulator.Register(Offset=0x00000020U)] public byte APC_VDDCHKD;
            [Simulator.Register(Offset=0x00000024U)] public byte APC_PREDLYSEL;
            [Simulator.Register(Offset=0x00000028U)] public byte APC_IMASK;
            [Simulator.Register(Offset=0x0000002CU)] public byte APC_ISTATUS;
            [Simulator.Register(Offset=0x00000030U)] public byte APC_ICLEAR;
            [Simulator.Register(Offset=0x00000034U)] public byte APC_UNSH_NOISE;
            [Simulator.Register(Offset=0x00000038U)] public byte APC_WKUP_DLY;
            [Simulator.Register(Offset=0x0000003CU)] public byte APC_SLK_SMP;
            [Simulator.Register(Offset=0x00000040U)] public byte APC_CLKDIV_PWICLK;
            [Simulator.Register(Offset=0x00000050U)] public byte APC_OVSHT_LMT;
            [Simulator.Register(Offset=0x00000054U)] public byte APC_CLP_CTRL;
            [Simulator.Register(Offset=0x00000058U)] public byte APC_SS_SRATE;
            [Simulator.Register(Offset=0x0000005CU)] public byte APC_IGAIN4;
            [Simulator.Register(Offset=0x00000060U)] public byte APC_IGAIN1;
            [Simulator.Register(Offset=0x00000064U)] public byte APC_IGAIN2;
            [Simulator.Register(Offset=0x00000068U)] public byte APC_IGAIN3;
            [Simulator.Register(Offset=0x0000006CU)] public byte APC_ITSTCTRL;
            [Simulator.Register(Offset=0x00000070U)] public byte APC_ITSTIP1;
            [Simulator.Register(Offset=0x00000074U)] public byte APC_ITSTIP2;
            [Simulator.Register(Offset=0x00000078U)] public byte APC_ITSTOP1;
            [Simulator.Register(Offset=0x0000007CU)] public byte APC_ITSTOP2;
            [Simulator.Register(Offset=0x00000080U)] public byte APC_PL1_CALCODE;
            [Simulator.Register(Offset=0x00000084U)] public byte APC_PL2_CALCODE;
            [Simulator.Register(Offset=0x00000088U)] public byte APC_PL3_CALCODE;
            [Simulator.Register(Offset=0x0000008CU)] public byte APC_PL4_CALCODE;
            [Simulator.Register(Offset=0x00000090U)] public byte APC_PL5_CALCODE;
            [Simulator.Register(Offset=0x00000094U)] public byte APC_PL6_CALCODE;
            [Simulator.Register(Offset=0x00000098U)] public byte APC_PL7_CALCODE;
            [Simulator.Register(Offset=0x0000009CU)] public byte APC_PL8_CALCODE;
            [Simulator.Register(Offset=0x000000A0U)] public byte APC_PL1_COREVDD;
            [Simulator.Register(Offset=0x000000A4U)] public byte APC_PL2_COREVDD;
            [Simulator.Register(Offset=0x000000A8U)] public byte APC_PL3_COREVDD;
            [Simulator.Register(Offset=0x000000ACU)] public byte APC_PL4_COREVDD;
            [Simulator.Register(Offset=0x000000B0U)] public byte APC_PL5_COREVDD;
            [Simulator.Register(Offset=0x000000B4U)] public byte APC_PL6_COREVDD;
            [Simulator.Register(Offset=0x000000B8U)] public byte APC_PL7_COREVDD;
            [Simulator.Register(Offset=0x000000BCU)] public byte APC_PL8_COREVDD;
            [Simulator.Register(Offset=0x000000C0U)] public byte APC_RET_VDD;
            [Simulator.Register(Offset=0x000000C4U)] public byte APC_INTEGRATION_TEST_REG;
            [Simulator.Register(Offset=0x000000E0U)] public byte APC_DBG_DLYCODE;
            [Simulator.Register(Offset=0x000000FCU)] public byte APC_REV;
        }

        //
        // Internal memory.
        //
        public class RamMemoryHandler : Simulator.MemoryHandler
        {
            //
            // State
            //

            private ulong m_lastWrite = 0;

            //
            // Constructor Methods
            //

            public RamMemoryHandler()
            {
            }

            //--//

            //
            // Helper Methods
            //

            public override void Initialize( Simulator owner        ,
                                             ulong     rangeLength  ,
                                             uint      rangeWidth   ,
                                             uint      readLatency  ,
                                             uint      writeLatency )
            {
                base.Initialize( owner, rangeLength, rangeWidth, readLatency, writeLatency );

                for(int i = 0; i < m_target.Length; i++)
                {
                    m_target[i] = 0xDEADBEEF;
                }
            }

            public override void UpdateClockTicksForLoad( uint                                           address ,
                                                          TargetAdapterAbstractionLayer.MemoryAccessType kind    )
            {
                uint latency = m_readLatency; if(m_owner.ClockTicks == m_lastWrite) latency++; // See Ollie Spec on Read-After-Write.

                UpdateClocks( latency, kind );
            }

            public override void UpdateClockTicksForStore( uint                                           address ,
                                                           TargetAdapterAbstractionLayer.MemoryAccessType kind    )
            {
                m_lastWrite = m_owner.ClockTicks + 1;

                UpdateClocks( m_writeLatency, kind );
            }
        }

        //
        // 16KB cache memory
        //
        public class CacheMemoryHandler : Simulator.AddressSpaceBusHandler
        {
            const uint CacheableMask   = 0x80000000u;
            const uint FlushMask       = 0x40000000u;

            const int  c_Ways_Log      = 2;
            const int  c_SetSize_Log   = 8; // 16KB - Use 7 for 8KB.
            const int  c_LineSize_Log  = 2; // 4 words
            const int  c_WordSize_Log  = 2;
            const int  c_LruSize_Log   = 2;

            const int  c_WaySize_Log   =  (c_SetSize_Log + c_LineSize_Log);
            const int  c_Ways          =  (1  << c_Ways_Log    );
            const uint c_WordsInLine   =  (1u << c_LineSize_Log);

            const uint c_WaySize_Mask  = ((1u << c_WaySize_Log ) - 1u);
            const uint c_SetSize_Mask  = ((1u << c_SetSize_Log ) - 1u);
            const uint c_LineSize_Mask = ((1u << c_LineSize_Log) - 1u);
            const uint c_WordSize_Mask = ((1u << c_WordSize_Log) - 1u);
            const uint c_LruSize_Mask  = ((1u << c_LruSize_Log ) - 1u);

            //
            // State
            //

            private bool    m_fEnabled;
            private bool    m_fFlushEnabled;
            private bool    m_fResettingTags;

            private ulong[] m_cacheReady;
            private uint[]  m_cacheData;
            private uint[]  m_cacheTag;
            private uint[]  m_cacheLRU;
            private uint[]  m_cacheMap_LRUxWAY_LRU;
            private uint[]  m_cacheMap_LRU_WAY;

            private ulong   m_cache_LastBusActivity;

            //
            // Constructor Methods
            //

            public CacheMemoryHandler()
            {
                ResetCache();
            }

            //--//

            //
            // Helper Methods
            //

            public override bool CanAccess( uint                                           address         ,
                                            uint                                           relativeAddress ,
                                            TargetAdapterAbstractionLayer.MemoryAccessType kind            )
            {
                uint uncachedAddress = GetUncacheableAddress( address );

                return base.CanAccess( uncachedAddress, uncachedAddress, kind );
            }

            public override uint Read( uint                                           address         ,
                                       uint                                           relativeAddress ,
                                       TargetAdapterAbstractionLayer.MemoryAccessType kind            )
            {
                uint uncachedAddress = GetUncacheableAddress( address );
                uint res;

                if(m_owner.AreTimingUpdatesEnabled)
                {
                    if(m_fEnabled && uncachedAddress != address)
                    {
                        if(m_fFlushEnabled)
                        {
                            if((address & FlushMask) != 0)
                            {
                                return 0;
                            }
                        }

                        uint latency = ComputeCacheLatency( address, out res );

                        //
                        // If there's a detour installed at this spot, read the redirected value!!
                        //
                        if(res == Simulator.TrackDetour.c_DetourOpcode && kind != TargetAdapterAbstractionLayer.MemoryAccessType.FETCH)
                        {
                            res = m_owner.HandleDetour( address        , res );
                            res = m_owner.HandleDetour( uncachedAddress, res );
                        }

                        UpdateClocks( latency, TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );

                        switch(kind)
                        {
                            case TargetAdapterAbstractionLayer.MemoryAccessType.UINT8:
                                {
                                    uint shift = (address % 4) * 8;

                                    return (uint)((byte)(res >> (int)shift));
                                }

                            case TargetAdapterAbstractionLayer.MemoryAccessType.UINT16:
                                {
                                    uint shift = (address % 4) * 8;

                                    return (uint)((ushort)(res >> (int)shift));
                                }

                            case TargetAdapterAbstractionLayer.MemoryAccessType.UINT32:
                            case TargetAdapterAbstractionLayer.MemoryAccessType.SINT32:
                            case TargetAdapterAbstractionLayer.MemoryAccessType.FETCH:
                                {
                                    return res;
                                }

                            case TargetAdapterAbstractionLayer.MemoryAccessType.SINT8:
                                {
                                    uint shift = (address % 4) * 8;

                                    return (uint)(int)((sbyte)(res >> (int)shift));
                                }

                            case TargetAdapterAbstractionLayer.MemoryAccessType.SINT16:
                                {
                                    uint shift = (address % 4) * 8;

                                    return (uint)(int)((short)(res >> (int)shift));
                                }

                            default:
                                throw new NotSupportedException();
                        }
                    }
                }

                res = base.Read( uncachedAddress, uncachedAddress, kind );

                //
                // If there's a detour installed at this spot, read the redirected value!!
                //
                if(res == Simulator.TrackDetour.c_DetourOpcode && kind != TargetAdapterAbstractionLayer.MemoryAccessType.FETCH)
                {
                    res = m_owner.HandleDetour( address        , res );
                    res = m_owner.HandleDetour( uncachedAddress, res );
                }

                return res;
            }

            public override void Write( uint                                           address         ,
                                        uint                                           relativeAddress ,
                                        uint                                           value           ,
                                        TargetAdapterAbstractionLayer.MemoryAccessType kind            )
            {
                uint uncachedAddress = GetUncacheableAddress( address );

                if(m_owner.AreTimingUpdatesEnabled)
                {
                    if(m_fResettingTags)
                    {
                        //
                        // The Cache Tag Reset bit resets to 1. This control bit is active low.
                        // A 1 indicates normal cache tag operation.
                        // When 0, indicates that a cache tag initialization is in progress.
                        // Software should set this bit to ‘0’ to enable resetting of the cache tags and then set this back to a ‘1’ to resume normal operation.
                        // Setting this bit to zero remaps memory byte writes to addresses tarting at 0x0000_0000 to write to the cache tag memory rather
                        // than to normal memory. All four tag lines in a set are reset on carrying out a single write.
                        //
                        switch(kind)
                        {
                            case TargetAdapterAbstractionLayer.MemoryAccessType.UINT8:
                            case TargetAdapterAbstractionLayer.MemoryAccessType.SINT8:
                                //
                                // Absorb byte writes.
                                //
                                UpdateClockTicksForStore( address, kind );
                                return;
                        }
                    }

                    if(m_fEnabled && uncachedAddress != address)
                    {
                        if(m_fFlushEnabled)
                        {
                            if((address & FlushMask) != 0)
                            {
                                FlushAddress( address );
                                return;
                            }
                        }

                        uint data;
                        uint latency = ComputeCacheLatency( address, out data );

                        Simulator.TimingState backup = new Simulator.TimingState();

                        m_owner.SuspendTimingUpdates( ref backup );

                        UpdateCacheValue( address, value );

                        base.Write( uncachedAddress, uncachedAddress, value, kind );

                        m_owner.ResumeTimingUpdates( ref backup );

                        UpdateClocks( latency, TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );

                        return;
                    }
                }

                if(uncachedAddress != address)
                {
                    UpdateCacheValue( address, value );
                }

                base.Write( uncachedAddress, uncachedAddress, value, kind );
            }

            public override uint GetPhysicalAddress( uint address )
            {
                return GetUncacheableAddress( address );
            }

            //--//

            private void ResetCache()
            {
                m_cacheReady           = new ulong[1 << (c_Ways_Log + c_SetSize_Log + c_LineSize_Log)];
                m_cacheData            = new uint [1 << (c_Ways_Log + c_SetSize_Log + c_LineSize_Log)];
                m_cacheTag             = new uint [1 << (c_Ways_Log + c_SetSize_Log                 )];
                m_cacheLRU             = new uint [1 << (             c_SetSize_Log                 )];
                m_cacheMap_LRUxWAY_LRU = new uint [1 << (c_Ways_Log + c_LruSize_Log * c_Ways        )];
                m_cacheMap_LRU_WAY     = new uint [1 << (             c_LruSize_Log * c_Ways        )];

                for(int i = 0 ; i < m_cacheTag.Length; i++)
                {
                    m_cacheTag[i] = 0xFFFFFFFFu;
                }

                for(int i = 0 ; i < m_cacheMap_LRUxWAY_LRU.Length; i++)
                {
                    m_cacheMap_LRUxWAY_LRU[i] = 0xFFFFFFFFu;
                }

                for(int i = 0 ; i < m_cacheMap_LRU_WAY.Length; i++)
                {
                    m_cacheMap_LRU_WAY[i] = 0xFFFFFFFFu;
                }
            }

            private uint ComputeCacheLatency(     uint address ,
                                              out uint result  )
            {
                //
                // Align to word boundary.
                //
                address = address & ~(4u-1u);

                uint wordAddress = address >> c_WordSize_Log;
                uint tagAddress  = ExtractTagAddress( wordAddress );
                uint tag         = ExtractTag       ( wordAddress );
                uint tagIdx      = tagAddress << c_Ways_Log;
                int  way;

                //
                // Do we have a hit?
                //
                for(way = 0; way < c_Ways; way++)
                {
                    if(m_cacheTag[tagIdx + way] == tag)
                    {
                        //
                        // Hit.
                        //

                        uint cacheAddress = ExtractCacheAddress( wordAddress, way );
                        long diff         = (long)m_cacheReady[cacheAddress] - (long)m_owner.ClockTicks;

                        result = m_cacheData[cacheAddress];

                        //
                        // Update LRU
                        //
                        UpdateLRU( ref m_cacheLRU[tagAddress], way );

                        //
                        // Already filled.
                        //
                        if(diff <= 0)
                        {
                            return 1;
                        }
                        else
                        {
                            return (uint)diff;
                        }
                    }
                }

                //--//

                //
                // Select LRU way.
                //
                SelectWay( ref m_cacheLRU[tagAddress], out way );

                //
                // Update memories.
                //
////            if(m_cacheTag[tagIdx + way] != 0xFFFFFFFFu)
////            {
////                Hosting.OutputSink sink; m_owner.GetHostingService( out sink );
////                if(sink != null)
////                {
////                    uint oldLine;
////                    uint newLine;
////
////                    oldLine  = m_cacheTag[tagIdx + way] << (c_WaySize_Log + c_WordSize_Log);
////                    oldLine |= tagAddress << (c_LineSize_Log + c_WordSize_Log);
////
////                    newLine  = tag << (c_WaySize_Log + c_WordSize_Log);
////                    newLine |= tagAddress << (c_LineSize_Log + c_WordSize_Log);
////
////                    sink.OutputLine( "Evicting {0:X8} for {1:X8} at {2}", oldLine, newLine, m_owner.ClockTicks );
////                }
////            }

                m_cacheTag[tagIdx + way] = tag;

                {
                    uint  cacheAddress = ExtractCacheAddress( wordAddress, way );
                    ulong ready        = m_owner.ClockTicks; if(ready < m_cache_LastBusActivity) ready = m_cache_LastBusActivity;
                    uint  waitStates   = 0;

                    Simulator.TimingState backup = new Simulator.TimingState();

                    m_owner.SaveTimingUpdates( ref backup );

                    result = 0;

                    for(int word = 0; word < c_WordsInLine; word++)
                    {
                        ulong clockStart = m_owner.ClockTicks;
                        uint  data       = m_owner.Load( GetUncacheableAddress( address ), TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );
                        uint  accessTime = (uint)(m_owner.ClockTicks - clockStart);

                        if(word == 0)
                        {
                            waitStates = accessTime;
                            result     = data;
                        }

                        ready += accessTime;

                        m_cacheReady[cacheAddress] = ready;
                        m_cacheData [cacheAddress] = data;

                        address      = WrapAddress( address     , 4, 4 * c_WordsInLine );
                        cacheAddress = WrapAddress( cacheAddress, 1,     c_WordsInLine );
                    }

                    m_owner.RestoreTimingUpdates( ref backup );

                    m_cache_LastBusActivity = ready;

                    return waitStates;
                }
            }

            private void FlushAddress( uint address )
            {
                if(m_owner.AreTimingUpdatesEnabled == false)
                {
                    return;
                }

                uint wordAddress = address >> c_WordSize_Log;
                uint tagAddress  = ExtractTagAddress( wordAddress );
                uint tag         = ExtractTag       ( wordAddress );
                uint tagIdx      = tagAddress << c_Ways_Log;
                int  way;

                //
                // Do we have a hit?
                //
                for(way = 0; way < c_Ways; way++)
                {
                    if(m_cacheTag[tagIdx + way] == tag)
                    {
                        //
                        // Invalidate line.
                        //
                        m_cacheTag[tagIdx + way] = 0xFFFFFFFFu;
                        break;
                    }
                }
            }

            private void UpdateCacheValue( uint address ,
                                           uint value   )
            {
                //
                // Align to word boundary.
                //
                address = address & ~(4u-1u);

                uint wordAddress = address >> c_WordSize_Log;
                uint tagAddress  = ExtractTagAddress( wordAddress );
                uint tag         = ExtractTag       ( wordAddress );
                uint tagIdx      = tagAddress << c_Ways_Log;
                int  way;

                //
                // Do we have a hit?
                //
                for(way = 0; way < c_Ways; way++)
                {
                    if(m_cacheTag[tagIdx + way] == tag)
                    {
                        //
                        // Hit.
                        //

                        uint cacheAddress = ExtractCacheAddress( wordAddress, way );

                        m_cacheData[cacheAddress] = value;
                    }
                }
            }

            private void SelectWay( ref uint LRU ,
                                    out int  way )
            {
#if OLLIE_USE_CACHE_PSEUDO_LRU
                SelectWay__PseudoLRU( ref LRU, out way );
#else
                SelectWay__ReadLRU( ref LRU, out way );
#endif
            }

            private void SelectWay__PseudoLRU( ref uint LRU ,
                                               out int  way )
            {
                // bit 5 = A or B
                // bit 4 = A or C
                // bit 3 = A or D
                // bit 2 = B or C
                // bit 1 = B or D
                // bit 0 = C or D

                // If there's an access to A, you write "000xxx".  (fifth bit, fourth bit, ... zeroth bit)
                // If there's an access to B, you write "1xx00x".
                // If there's an access to C, you write "x1x1x0".
                // If there's an access to D, you write "xx1x11".
                //
                // Then at cache-miss, you use sets of 3 bits to determine the LRU
                // there should not be ambiguity, but we don't check that
                //
                uint localLRU = LRU;

                if     ((localLRU & 0x38) == 0x38) way = 0;  // LRU == A
                else if((localLRU & 0x26) == 0x06) way = 1;
                else if((localLRU & 0x15) == 0x01) way = 2;
                else if((localLRU & 0x0B) == 0x00) way = 3;
                else                               way = 0;

                UpdateLRU__PseudoLRU( ref LRU, way );
            }

            private void SelectWay__ReadLRU( ref uint LRU ,
                                             out int  way )
            {
                uint newLRU = m_cacheMap_LRU_WAY[LRU];

                if(newLRU == 0xFFFFFFFF)
                {
                    uint minLRU = c_LruSize_Mask + 1;
                    int  minWay = 0;
                    int  idxWay;

                    newLRU = 0;

                    for(idxWay = 0; idxWay < c_Ways; idxWay++)
                    {
                        uint count = ExtractLRU( LRU, idxWay );

                        if(count < minLRU)
                        {
                            minLRU = count;
                            minWay = idxWay;
                        }
                    }

                    newLRU = (uint)(minWay << (c_LruSize_Log * c_Ways));

                    for(idxWay = 0; idxWay < c_Ways; idxWay++)
                    {
                        uint count = ExtractLRU( LRU, idxWay );

                        if(idxWay == minWay)
                        {
                            count = c_LruSize_Mask;
                        }
                        else if(count != 0 && count >= minLRU)
                        {
                            count--;
                        }

                        newLRU |= InsertLRU( count, idxWay );
                    }

                    m_cacheMap_LRU_WAY[LRU] = newLRU;
                }

                way = (int)(newLRU >> (c_LruSize_Log * c_Ways));

                LRU = newLRU & ((1 << (c_LruSize_Log * c_Ways)) - 1);
            }

            private void UpdateLRU( ref uint LRU ,
                                        int  way )
            {
#if OLLIE_USE_CACHE_PSEUDO_LRU
                UpdateLRU__PseudoLRU( ref LRU, way );
#else
                UpdateLRU__RealLRU( ref LRU, way );
#endif
            }

            private void UpdateLRU__PseudoLRU( ref uint LRU ,
                                                   int  way )
            {
                // bit 5 = A or B
                // bit 4 = A or C
                // bit 3 = A or D
                // bit 2 = B or C
                // bit 1 = B or D
                // bit 0 = C or D

                // If there's an access to A, you write "000xxx".  (fifth bit, fourth bit, ... zeroth bit)
                // If there's an access to B, you write "1xx00x".
                // If there's an access to C, you write "x1x1x0".
                // If there's an access to D, you write "xx1x11".
                //
                uint newLRU = LRU;

                switch(way)
                {
                case 0: newLRU &= ~(0x38u); newLRU |= 0x00; break;
                case 1: newLRU &= ~(0x26u); newLRU |= 0x20; break;
                case 2: newLRU &= ~(0x15u); newLRU |= 0x14; break;
                case 3: newLRU &= ~(0x0Bu); newLRU |= 0x0B; break;
                }

                LRU = newLRU;
            }

            private void UpdateLRU__RealLRU( ref uint LRU ,
                                                 int  way )
            {
                uint newLRU = m_cacheMap_LRUxWAY_LRU[LRU | (uint)(way << (c_LruSize_Log * c_Ways))];

                if(newLRU == 0xFFFFFFFF)
                {
                    uint minLRU = ExtractLRU( LRU, way );
                    int  idxWay;

                    newLRU = 0;

                    for(idxWay = 0; idxWay < c_Ways; idxWay++)
                    {
                        uint count = ExtractLRU( LRU, idxWay );

                        if(idxWay == way)
                        {
                            count = c_LruSize_Mask;
                        }
                        else if(count != 0 && count >= minLRU)
                        {
                            count--;
                        }

                        newLRU |= InsertLRU( count, idxWay );
                    }

                    m_cacheMap_LRUxWAY_LRU[LRU | (uint)(way << (c_LruSize_Log * c_Ways))] = newLRU;
                }

                LRU = newLRU & ((1 << (c_LruSize_Log * c_Ways)) - 1);
            }

            //--//

            public static uint GetUncacheableAddress( uint address )
            {
                return address & ~CacheableMask;
            }

            public static uint GetCacheableAddress( uint address )
            {
                return address | CacheableMask;
            }

            private static uint ExtractTag( uint address )
            {
                return (address >> (c_WaySize_Log));
            }

            private static uint ExtractTagAddress( uint address )
            {
                return ((address >> (c_LineSize_Log)) & c_SetSize_Mask);
            }

            private static uint ExtractCacheAddress( uint address ,
                                                     int  way     )
            {
                return ((address & c_WaySize_Mask) + (uint)(way << c_WaySize_Log));
            }

            private static uint WrapAddress( uint address   ,
                                             uint increment ,
                                             uint maskSize  )
            {
                return (address & ~(maskSize-1)) | ((address + increment) & (maskSize-1));
            }

            private static uint ExtractLRU( uint lru ,
                                            int  way )
            {
                return ((lru >> (way * c_LruSize_Log)) & c_LruSize_Mask);
            }

            private static uint InsertLRU( uint lru ,
                                           int  way )
            {
                return ((lru & c_LruSize_Mask) << (way * c_LruSize_Log));
            }

            //
            // Access Methods
            //

            public bool Enabled
            {
                set
                {
                    m_fEnabled = value;
                }
            }

            public bool ResettingTags 
            {
                set
                {
                    m_fResettingTags = value;

                    if(m_fResettingTags)
                    {
                        ResetCache();
                    }
                }
            }

            public bool FlushEnabled
            {
                set
                {
                    m_fFlushEnabled = value;
                }
            }
        }
    }
}
