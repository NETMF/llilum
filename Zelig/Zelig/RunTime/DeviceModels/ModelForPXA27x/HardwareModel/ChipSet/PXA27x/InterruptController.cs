//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x40D00000U,Length=0x000000D0U)]
    public class InterruptController
    {
        public const int IRQ_INDEX_SSP_3            =  0 +  0;
        public const int IRQ_INDEX_MSL              =  0 +  1;
        public const int IRQ_INDEX_USB_HOST_2       =  0 +  2;
        public const int IRQ_INDEX_USB_HOST_1       =  0 +  3;
        public const int IRQ_INDEX_KEYPAD_CTRL      =  0 +  4;
        public const int IRQ_INDEX_MEMORY_STICK     =  0 +  5;
        public const int IRQ_INDEX_PWR_I2C          =  0 +  6;
        public const int IRQ_INDEX_OS_TIMER         =  0 +  7;
        public const int IRQ_INDEX_GPIO0            =  0 +  8;
        public const int IRQ_INDEX_GPIO1            =  0 +  9;
        public const int IRQ_INDEX_GPIOx            =  0 + 10;
        public const int IRQ_INDEX_USB_CLIENT       =  0 + 11;
        public const int IRQ_INDEX_PMU              =  0 + 12;
        public const int IRQ_INDEX_I2S              =  0 + 13;
        public const int IRQ_INDEX_AC97             =  0 + 14;
        public const int IRQ_INDEX_USIM             =  0 + 15;
        public const int IRQ_INDEX_SSP_2            =  0 + 16;
        public const int IRQ_INDEX_LCD              =  0 + 17;
        public const int IRQ_INDEX_I2C              =  0 + 18;
        public const int IRQ_INDEX_INFRA_RED_COM    =  0 + 19;
        public const int IRQ_INDEX_STUART           =  0 + 20;
        public const int IRQ_INDEX_BTUART           =  0 + 21;
        public const int IRQ_INDEX_FFUART           =  0 + 22;
        public const int IRQ_INDEX_FLASH_CARD       =  0 + 23;
        public const int IRQ_INDEX_SSP_1            =  0 + 24;
        public const int IRQ_INDEX_DMA_CTRL         =  0 + 25;
        public const int IRQ_INDEX_OS_TIMER0        =  0 + 26;
        public const int IRQ_INDEX_OS_TIMER1        =  0 + 27;
        public const int IRQ_INDEX_OS_TIMER2        =  0 + 28;
        public const int IRQ_INDEX_OS_TIMER3        =  0 + 29;
        public const int IRQ_INDEX_RTC_1HZ_TIC      =  0 + 30;
        public const int IRQ_INDEX_RTC_ALARM        =  0 + 31;
        public const int IRQ_INDEX_TRUSTED_PLFM     = 32 +  0;
        public const int IRQ_INDEX_QK_CAP           = 32 +  1;
        public const int IRQ_INDEX_MAX              = 34;
                                                      
        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct ICCR_bitfield
        {
            [BitFieldRegister(Position=0)] public bool DIM; // 0 = Any interrupt in ICPR brings the processor out of idle mode.
                                                            // 1 = Only active, unmasked interrupts (as defined in the ICMR) bring the processor out of idle mode. Cleared during resets.
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct ICHP_bitfield
        {
            [BitFieldRegister(Position=31       )] public bool VAL_IRQ; // Valid IRQ
                                                                        // 
                                                                        // 0 = No valid peripheral ID that is causing an IRQ interrupt.
                                                                        // 1 = Valid peripheral ID is causing an IRQ interrupt.
                                                                        // 
            [BitFieldRegister(Position=16,Size=6)] public uint IRQ;     // IRQ Highest Priority Field
                                                                        // 
                                                                        // Peripheral ID with the highest IRQ priority.
                                                                        // When no interrupt has occurred, this bit is set to –1.
                                                                        // 
            [BitFieldRegister(Position=15       )] public bool VAL_FIQ; // Valid FIQ
                                                                        // 
                                                                        // 0 = No valid peripheral ID that is causing an FIQ interrupt.
                                                                        // 1 = Valid peripheral ID is causing an FIQ interrupt.
                                                                        // 
            [BitFieldRegister(Position= 0,Size=6)] public uint FIQ;     // FIQ Highest Priority Field
                                                                        // 
                                                                        // Peripheral ID with the highest FIQ priority.
                                                                        // When no interrupt has occurred, this bit is set to –1.
                                                                        // 
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct IPR_bitfield
        {
            [BitFieldRegister(Position=31      )] public bool VAL; // Valid Bit
                                                                   // 
                                                                   //   0 = Peripheral ID contained in the register is not valid.
                                                                   //   1 = Peripheral ID contained in the register is valid.
                                                                   // 
            [BitFieldRegister(Position=0,Size=6)] public uint PID; // Peripheral ID for this Priority IPR[n]
                                                                   // 
                                                                   // Peripheral ID of peripheral with priority n (n=IPR number). Valid IDs: 0 through 39
                                                                   // 
        }
        
        [Register(Offset=0x14U             )] public ICCR_bitfield  ICCR;     // Interrupt Controller Control register                                         | 25-27
        [Register(Offset=0x18U             )] public ICHP_bitfield  ICHP;     // Interrupt Controller Highest Priority register                                | 25-30

        [Register(Offset=0x00U             )] public uint           ICIP;     // Interrupt Controller IRQ Pending register                                     | 25-11
        [Register(Offset=0x04U             )] public uint           ICMR;     // Interrupt Controller Mask register                                            | 25-20
        [Register(Offset=0x08U             )] public uint           ICLR;     // Interrupt Controller Level register                                           | 25-24
        [Register(Offset=0x0CU             )] public uint           ICFP;     // Interrupt Controller FIQ Pending register                                     | 25-15
        [Register(Offset=0x10U             )] public uint           ICPR;     // Interrupt Controller Pending register                                         | 25-6
        [Register(Offset=0x1CU,Instances=32)] public IPR_bitfield[] IPR0_31;  // Interrupt Priority registers for Priorities 0–31                              | 25-29

        [Register(Offset=0x9CU             )] public uint           ICIP2;    // Interrupt Controller IRQ Pending register 2                                   | 25-10
        [Register(Offset=0xA0U             )] public uint           ICMR2;    // Interrupt Controller Mask register 2                                          | 25-23
        [Register(Offset=0xA4U             )] public uint           ICLR2;    // Interrupt Controller Level register 2                                         | 25-27
        [Register(Offset=0xA8U             )] public uint           ICFP2;    // Interrupt Controller FIQ Pending register 2                                   | 25-19
        [Register(Offset=0xACU             )] public uint           ICPR2;    // Interrupt Controller Pending register 2                                       | 25-6
        [Register(Offset=0xB0U,Instances= 8)] public IPR_bitfield[] IPR32_39; // Interrupt Priority registers for Priorities 32–39                             | 25-29

        //
        // Helper Methods
        //

        [Inline]
        public void SetPriority( uint irqIndex ,
                                 int  priority )
        {
            if(priority < 32)
            {
                this.IPR0_31[priority] = new IPR_bitfield
                {
                    VAL = true,
                    PID = irqIndex,
                };
            }
            else if(priority < 64)
            {
                this.IPR32_39[priority-32] = new IPR_bitfield
                {
                    VAL = true,
                    PID = irqIndex,
                };
            }
        }

        //
        // Access Methods
        //

        public static extern InterruptController Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}