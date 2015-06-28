//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.LPC3180
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x40008000U,Length=0x00014000U)]
    public class INTC
    {
        public const int  IRQ_INDEX_Sub2FIQn          =  0 + 31; // High priority (FIQ) interrupts from SIC2. Active LOW.
        public const int  IRQ_INDEX_Sub1FIQn          =  0 + 30; // High priority (FIQ) interrupts from SIC1. Active LOW.
        public const int  IRQ_INDEX_DMAINT            =  0 + 28; // General Purpose DMA Controller interrupt.
        public const int  IRQ_INDEX_MSTIMER_INT       =  0 + 27; // Match interrupt 0 or 1 from the Millisecond Timer.
        public const int  IRQ_INDEX_IIR1              =  0 + 26; // UART1 interrupt.
        public const int  IRQ_INDEX_IIR2              =  0 + 25; // UART2 interrupt.
        public const int  IRQ_INDEX_IIR7              =  0 + 24; // UART7 interrupt.
        public const int  IRQ_INDEX_SD0_INT           =  0 + 15; // Interrupt 0 from the SD Card interface.
        public const int  IRQ_INDEX_SD1_INT           =  0 + 13; // Interrupt 1 from the SD Card interface.
        public const int  IRQ_INDEX_FLASH_INT         =  0 + 11; // Interrupt from the NAND Flash controller.
        public const int  IRQ_INDEX_IIR6              =  0 + 10; // UART6 interrupt.
        public const int  IRQ_INDEX_IIR5              =  0 +  9; // UART5 interrupt.
        public const int  IRQ_INDEX_IIR4              =  0 +  8; // UART4 interrupt.
        public const int  IRQ_INDEX_IIR3              =  0 +  7; // UART3 interrupt.
        public const int  IRQ_INDEX_WATCH_INT         =  0 +  6; // Watchdog Timer interrupt.
        public const int  IRQ_INDEX_HSTIMER_INT       =  0 +  5; // Match interrupt from the High Speed Timer.
        public const int  IRQ_INDEX_Sub2IRQn          =  0 +  1; // Low priority (FIQ) interrupts from SIC2. Active LOW.
        public const int  IRQ_INDEX_Sub1IRQn          =  0 +  0; // Low priority (FIQ) interrupts from SIC1. Active LOW.

        public const int  IRQ_INDEX_USB_i2c_int       = 32 + 31; // Interrupt from the USB I2C interface.
        public const int  IRQ_INDEX_USB_dev_hp_int    = 32 + 30; // USB high priority interrupt.
        public const int  IRQ_INDEX_USB_dev_lp_int    = 32 + 29; // USB low priority interrupt.
        public const int  IRQ_INDEX_USB_dev_dma_int   = 32 + 28; // USB DMA interrupt.
        public const int  IRQ_INDEX_USB_host_int      = 32 + 27; // USB host interrupt.
        public const int  IRQ_INDEX_USB_otg_atx_int_n = 32 + 26; // External USB transceiver interrupt. Active LOW.
        public const int  IRQ_INDEX_USB_otg_timer_int = 32 + 25; // USB timer interrupt.
        public const int  IRQ_INDEX_SW_INT            = 32 + 24; // Software interrupt (caused by bit 0 of the SW_INT register).
        public const int  IRQ_INDEX_SPI1_INT          = 32 + 23; // Interrupt from the SPI1 interface.
        public const int  IRQ_INDEX_KEY_IRQ           = 32 + 22; // Keyboard scanner interrupt.
        public const int  IRQ_INDEX_RTC_INT           = 32 + 20; // Match interrupt 0 or 1 from the RTC.
        public const int  IRQ_INDEX_I2C_1_INT         = 32 + 19; // Interrupt from the I2C1 interface.
        public const int  IRQ_INDEX_I2C_2_INT         = 32 + 18; // Interrupt from the I2C2 interface.
        public const int  IRQ_INDEX_PLL397_INT        = 32 + 17; // Lock interrupt from the 397x PLL.
        public const int  IRQ_INDEX_PLLHCLK_INT       = 32 + 14; // Lock interrupt from the HCLK PLL.
        public const int  IRQ_INDEX_PLLUSB_INT        = 32 + 13; // Lock interrupt from the USB PLL.
        public const int  IRQ_INDEX_SPI2_INT          = 32 + 12; // Interrupt from the SPI2 interface.
        public const int  IRQ_INDEX_ADC_INT           = 32 +  7; // A/D Converter interrupt.
        public const int  IRQ_INDEX_GPI_11            = 32 +  4; // Interrupt from the GPI_11 pin.
        public const int  IRQ_INDEX_JTAG_COMM_RX      = 32 +  2; // Receiver full interrupt from the JTAG Communication Channel.
        public const int  IRQ_INDEX_JTAG_COMM_TX      = 32 +  1; // Transmitter empty interrupt from the JTAG Communication Channel.

        public const int  IRQ_INDEX_SYSCLK_mux        = 64 + 31; // Status of the SYSCLK Mux (SYSCLK_CTRL[0]). May be used to begin operations that require a change to the alternate clock source.
        public const int  IRQ_INDEX_GPI_06            = 64 + 28; // Interrupt from the GPI_06 (HSTIM_CAP) pin.
        public const int  IRQ_INDEX_GPI_05            = 64 + 27; // Interrupt from the GPI_05 pin.
        public const int  IRQ_INDEX_GPI_04            = 64 + 26; // Interrupt from the GPI_04 (SPI1_BUSY) pin.
        public const int  IRQ_INDEX_GPI_03            = 64 + 25; // Interrupt from the GPI_03 pin.
        public const int  IRQ_INDEX_GPI_02            = 64 + 24; // Interrupt from the GPI_02 pin.
        public const int  IRQ_INDEX_GPI_01            = 64 + 23; // Interrupt from the GPI_01 (SERVICE_N) pin.
        public const int  IRQ_INDEX_GPI_00            = 64 + 22; // Interrupt from the GPI_00 pin.
        public const int  IRQ_INDEX_SPI1_DATIN        = 64 + 20; // Interrupt from the SPI1_DATIN pin.
        public const int  IRQ_INDEX_U5_RX             = 64 + 19; // Interrupt from the UART5 RX pin.
        public const int  IRQ_INDEX_SDIO_INT_N        = 64 + 18; // Interrupt from the MS_DIO1 pin. Active LOW.
        public const int  IRQ_INDEX_GPI_07            = 64 + 15; // Interrupt from the GPI_07 pin.
        public const int  IRQ_INDEX_U7_HCTS           = 64 + 12; // Interrupt from the UART7 HCTS pin.
        public const int  IRQ_INDEX_GPI_10            = 64 + 11; // Interrupt from the GPI_10 (U4_RX) pin.
        public const int  IRQ_INDEX_GPI_09            = 64 + 10; // Interrupt from the GPI_09 (KEY_COL7) pin.
        public const int  IRQ_INDEX_GPI_08            = 64 +  9; // Interrupt from the GPI_08 (KEY_COL6, SPI2_BUSY) pin.
        public const int  IRQ_INDEX_U2_HCTS           = 64 +  7; // Interrupt from the UART2 HCTS pin.
        public const int  IRQ_INDEX_SPI2_DATIN        = 64 +  6; // Interrupt from the SPI1_DATIN) pin.
        public const int  IRQ_INDEX_GPIO_05           = 64 +  5; // Interrupt from the GPI_05 pin.
        public const int  IRQ_INDEX_GPIO_04           = 64 +  4; // Interrupt from the GPI_04 pin.
        public const int  IRQ_INDEX_GPIO_03           = 64 +  3; // Interrupt from the GPI_03 (KEY_ROW7) pin.
        public const int  IRQ_INDEX_GPIO_02           = 64 +  2; // Interrupt from the GPI_02 (KEY_ROW6) pin.
        public const int  IRQ_INDEX_GPIO_01           = 64 +  1; // Interrupt from the GPI_01 pin.
        public const int  IRQ_INDEX_GPIO_00           = 64 +  0; // Interrupt from the GPI_00 pin.

        [MemoryMappedPeripheral(Base=0x0000U,Length=0x4000U)]
        public class Section
        {
            [Register(Offset=0x00000000U)] public uint ER;  // Enable Register for the Main Interrupt Controller 0 R/W
            [Register(Offset=0x00000004U)] public uint RSR; // Raw Status Register for the Main Interrupt Controller x R/W
            [Register(Offset=0x00000008U)] public uint SR;  // Status Register for the Main Interrupt Controller 0 RO
            [Register(Offset=0x0000000CU)] public uint APR; // Activation Polarity select Register for the Main Interrupt Controller 0 R/W
            [Register(Offset=0x00000010U)] public uint ATR; // Activation Type select Register for the Main Interrupt Controller 0 R/W
            [Register(Offset=0x00000014U)] public uint ITR; // Interrupt Type select Register for the Main Interrupt Controller 0 R/W
        }

        //--//

        [Register(Offset=0x00000000U,Instances=3)] public Section[] Sections;

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