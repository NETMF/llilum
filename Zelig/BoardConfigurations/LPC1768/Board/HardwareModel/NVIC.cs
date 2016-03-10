//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.LPC1768
{
    enum IRQn
    {
        /******  Cortex-M3 Processor Exceptions Numbers ***************************************************/
        NonMaskableInt_IRQn     = -14,      /*!< 2 Non Maskable Interrupt                         */
        MemoryManagement_IRQn   = -12,      /*!< 4 Cortex-M3 Memory Management Interrupt          */
        BusFault_IRQn           = -11,      /*!< 5 Cortex-M3 Bus Fault Interrupt                  */
        UsageFault_IRQn         = -10,      /*!< 6 Cortex-M3 Usage Fault Interrupt                */
        SVCall_IRQn             = -5,       /*!< 11 Cortex-M3 SV Call Interrupt                   */
        DebugMonitor_IRQn       = -4,       /*!< 12 Cortex-M3 Debug Monitor Interrupt             */
        PendSV_IRQn             = -2,       /*!< 14 Cortex-M3 Pend SV Interrupt                   */
        SysTick_IRQn            = -1,       /*!< 15 Cortex-M3 System Tick Interrupt               */

        /******  LPC17xx Specific Interrupt Numbers *******************************************************/
        WDT_IRQn                = 0,        /*!< Watchdog Timer Interrupt                         */
        TIMER0_IRQn             = 1,        /*!< Timer0 Interrupt                                 */
        TIMER1_IRQn             = 2,        /*!< Timer1 Interrupt                                 */
        TIMER2_IRQn             = 3,        /*!< Timer2 Interrupt                                 */
        TIMER3_IRQn             = 4,        /*!< Timer3 Interrupt                                 */
        UART0_IRQn              = 5,        /*!< UART0 Interrupt                                  */
        UART1_IRQn              = 6,        /*!< UART1 Interrupt                                  */
        UART2_IRQn              = 7,        /*!< UART2 Interrupt                                  */
        UART3_IRQn              = 8,        /*!< UART3 Interrupt                                  */
        PWM1_IRQn               = 9,        /*!< PWM1 Interrupt                                   */
        I2C0_IRQn               = 10,       /*!< I2C0 Interrupt                                   */
        I2C1_IRQn               = 11,       /*!< I2C1 Interrupt                                   */
        I2C2_IRQn               = 12,       /*!< I2C2 Interrupt                                   */
        SPI_IRQn                = 13,       /*!< SPI Interrupt                                    */
        SSP0_IRQn               = 14,       /*!< SSP0 Interrupt                                   */
        SSP1_IRQn               = 15,       /*!< SSP1 Interrupt                                   */
        PLL0_IRQn               = 16,       /*!< PLL0 Lock (Main PLL) Interrupt                   */
        RTC_IRQn                = 17,       /*!< Real Time Clock Interrupt                        */
        EINT0_IRQn              = 18,       /*!< External Interrupt 0 Interrupt                   */
        EINT1_IRQn              = 19,       /*!< External Interrupt 1 Interrupt                   */
        EINT2_IRQn              = 20,       /*!< External Interrupt 2 Interrupt                   */
        EINT3_IRQn              = 21,       /*!< External Interrupt 3 Interrupt                   */
        ADC_IRQn                = 22,       /*!< A/D Converter Interrupt                          */
        BOD_IRQn                = 23,       /*!< Brown-Out Detect Interrupt                       */
        USB_IRQn                = 24,       /*!< USB Interrupt                                    */
        CAN_IRQn                = 25,       /*!< CAN Interrupt                                    */
        DMA_IRQn                = 26,       /*!< General Purpose DMA Interrupt                    */
        I2S_IRQn                = 27,       /*!< I2S Interrupt                                    */
        ENET_IRQn               = 28,       /*!< Ethernet Interrupt                               */
        RIT_IRQn                = 29,       /*!< Repetitive Interrupt Timer Interrupt             */
        MCPWM_IRQn              = 30,       /*!< Motor Control PWM Interrupt                      */
        QEI_IRQn                = 31,       /*!< Quadrature Encoder Interface Interrupt           */
        PLL1_IRQn               = 32,       /*!< PLL1 Lock (USB PLL) Interrupt                    */
        USBActivity_IRQn        = 33,       /* USB Activity interrupt                             */
        CANActivity_IRQn        = 34,       /* CAN Activity interrupt                             */
        LAST                    = 35, 
    }
}
