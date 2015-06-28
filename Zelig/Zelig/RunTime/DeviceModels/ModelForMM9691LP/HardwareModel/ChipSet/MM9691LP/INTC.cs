//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x38000000U,Length=0x00000210U)]
    public class INTC
    {
        public const int  IRQ_INDEX_unused0                  =  0;
        public const int  IRQ_INDEX_Programmed_Interrupt     =  1;
        public const int  IRQ_INDEX_Debug_Channel_Comms_Rx   =  2;
        public const int  IRQ_INDEX_Debug_Channel_Comms_Tx   =  3;
        public const int  IRQ_INDEX_ARM_Timer_1              =  4;
        public const int  IRQ_INDEX_ARM_Timer_2              =  5;
        public const int  IRQ_INDEX_Versatile_Timer_1        =  6;
        public const int  IRQ_INDEX_Versatile_Timer_2        =  7;
        public const int  IRQ_INDEX_Versatile_Timer_3        =  8;
        public const int  IRQ_INDEX_Versatile_Timer_4        =  9;
        public const int  IRQ_INDEX_Real_Time_Clock          = 10;
        public const int  IRQ_INDEX_USB                      = 11;
        public const int  IRQ_INDEX_USART0_Tx                = 12;
        public const int  IRQ_INDEX_USART0_Rx                = 13;
        public const int  IRQ_INDEX_USART1_Tx                = 14;
        public const int  IRQ_INDEX_USART1_Rx                = 15;
        public const int  IRQ_INDEX_GPIO_00_07               = 16;
        public const int  IRQ_INDEX_GPIO_08_15               = 17;
        public const int  IRQ_INDEX_GPIO_16_23               = 18;
        public const int  IRQ_INDEX_GPIO_24_31               = 19;
        public const int  IRQ_INDEX_GPIO_32_39               = 20;
        public const int  IRQ_INDEX_Edge_Detected_Interrupts = 21;
        public const int  IRQ_INDEX_MicroWire                = 22;
        public const int  IRQ_INDEX_Watchdog                 = 23;
        public const int  IRQ_INDEX_USART0_Flow_Control      = 24;
        public const int  IRQ_INDEX_USART1_Flow_Control      = 25;
        public const int  IRQ_INDEX_unused1                  = 26;
        public const int  IRQ_INDEX_APC                      = 27;
        public const int  IRQ_INDEX_DMA_ALL_Channels         = 28;
        public const int  IRQ_INDEX_Viterbi_Processor        = 29;
        public const int  IRQ_INDEX_Filter_Processor         = 30;
        public const int  IRQ_INDEX_AHB_Write_Error          = 31;

        public const uint IRQ_MASK_unused0                   = 1U << IRQ_INDEX_unused0                 ;
        public const uint IRQ_MASK_Programmed_Interrupt      = 1U << IRQ_INDEX_Programmed_Interrupt    ;
        public const uint IRQ_MASK_Debug_Channel_Comms_Rx    = 1U << IRQ_INDEX_Debug_Channel_Comms_Rx  ;
        public const uint IRQ_MASK_Debug_Channel_Comms_Tx    = 1U << IRQ_INDEX_Debug_Channel_Comms_Tx  ;
        public const uint IRQ_MASK_ARM_Timer_1               = 1U << IRQ_INDEX_ARM_Timer_1             ;
        public const uint IRQ_MASK_ARM_Timer_2               = 1U << IRQ_INDEX_ARM_Timer_2             ;
        public const uint IRQ_MASK_Versatile_Timer_1         = 1U << IRQ_INDEX_Versatile_Timer_1       ;
        public const uint IRQ_MASK_Versatile_Timer_2         = 1U << IRQ_INDEX_Versatile_Timer_2       ;
        public const uint IRQ_MASK_Versatile_Timer_3         = 1U << IRQ_INDEX_Versatile_Timer_3       ;
        public const uint IRQ_MASK_Versatile_Timer_4         = 1U << IRQ_INDEX_Versatile_Timer_4       ;
        public const uint IRQ_MASK_Real_Time_Clock           = 1U << IRQ_INDEX_Real_Time_Clock         ;
        public const uint IRQ_MASK_USB                       = 1U << IRQ_INDEX_USB                     ;
        public const uint IRQ_MASK_USART0_Tx                 = 1U << IRQ_INDEX_USART0_Tx               ;
        public const uint IRQ_MASK_USART0_Rx                 = 1U << IRQ_INDEX_USART0_Rx               ;
        public const uint IRQ_MASK_USART1_Tx                 = 1U << IRQ_INDEX_USART1_Tx               ;
        public const uint IRQ_MASK_USART1_Rx                 = 1U << IRQ_INDEX_USART1_Rx               ;
        public const uint IRQ_MASK_GPIO_00_07                = 1U << IRQ_INDEX_GPIO_00_07              ;
        public const uint IRQ_MASK_GPIO_08_15                = 1U << IRQ_INDEX_GPIO_08_15              ;
        public const uint IRQ_MASK_GPIO_16_23                = 1U << IRQ_INDEX_GPIO_16_23              ;
        public const uint IRQ_MASK_GPIO_24_31                = 1U << IRQ_INDEX_GPIO_24_31              ;
        public const uint IRQ_MASK_GPIO_32_39                = 1U << IRQ_INDEX_GPIO_32_39              ;
        public const uint IRQ_MASK_Edge_Detected_Interrupts  = 1U << IRQ_INDEX_Edge_Detected_Interrupts;
        public const uint IRQ_MASK_MicroWire                 = 1U << IRQ_INDEX_MicroWire               ;
        public const uint IRQ_MASK_Watchdog                  = 1U << IRQ_INDEX_Watchdog                ;
        public const uint IRQ_MASK_USART0_Flow_Control       = 1U << IRQ_INDEX_USART0_Flow_Control     ;
        public const uint IRQ_MASK_USART1_Flow_Control       = 1U << IRQ_INDEX_USART1_Flow_Control     ;
        public const uint IRQ_MASK_unused1                   = 1U << IRQ_INDEX_unused1                 ;
        public const uint IRQ_MASK_APC                       = 1U << IRQ_INDEX_APC                     ;
        public const uint IRQ_MASK_DMA_ALL_Channels          = 1U << IRQ_INDEX_DMA_ALL_Channels        ;
        public const uint IRQ_MASK_Viterbi_Processor         = 1U << IRQ_INDEX_Viterbi_Processor       ;
        public const uint IRQ_MASK_Filter_Processor          = 1U << IRQ_INDEX_Filter_Processor        ;
        public const uint IRQ_MASK_AHB_Write_Error           = 1U << IRQ_INDEX_AHB_Write_Error         ;
        public const uint IRQ_MASK_All                       = 0xFFFFFFFFU;     

        //--//

        [MemoryMappedPeripheral(Base=0x0000U,Length=0x0020U)]
        public class IRQ
        {
            [Register(Offset=0x00000000U)] public uint Status;
            [Register(Offset=0x00000004U)] public uint RawStatus;
            [Register(Offset=0x00000008U)] public uint EnableSet;
            [Register(Offset=0x0000000CU)] public uint EnableClear;
            [Register(Offset=0x00000010U)] public uint Soft;
            [Register(Offset=0x00000014U)] public uint TestSource;
            [Register(Offset=0x00000018U)] public uint SourceSelect;
        }

        [MemoryMappedPeripheral(Base=0x0000U,Length=0x0020U)]
        public class FIRQ
        {
            [Register(Offset=0x00000000U)] public uint Status;
            [Register(Offset=0x00000004U)] public uint RawStatus;
            [Register(Offset=0x00000008U)] public uint EnableSet;
            [Register(Offset=0x0000000CU)] public uint EnableClear;
            [Register(Offset=0x00000014U)] public uint TestSource;
            [Register(Offset=0x00000018U)] public uint SourceSelect;
            [Register(Offset=0x0000001CU)] public int  Select;
        }

        [Register(Offset=0x00000000U)] public IRQ  Irq;
        [Register(Offset=0x00000020U)] public uint INTOUT_L_EnableSet;
        [Register(Offset=0x00000024U)] public uint INTOUT_L_EnableClear;
        [Register(Offset=0x00000100U)] public FIRQ Fiq;
        [Register(Offset=0x00000200U)] public uint EdgeStatus;
        [Register(Offset=0x00000204U)] public uint EdgeRawStatus;
        [Register(Offset=0x00000208U)] public uint EdgeEnable;
        [Register(Offset=0x0000020CU)] public uint EdgeEnableClear;
        [Register(Offset=0x00000210U)] public uint EdgeClear;

        //
        // Access Methods
        //

        public static extern INTC Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}