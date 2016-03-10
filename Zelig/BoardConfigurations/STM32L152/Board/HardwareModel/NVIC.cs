//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32L152
{
    enum IRQn
    {
        /******  Cortex-M3 Processor Exceptions Numbers ******************************************************/
        NonMaskableInt_IRQn         = -14,    /*!< 2 Non Maskable Interrupt                                */
        MemoryManagement_IRQn       = -12,    /*!< 4 Cortex-M3 Memory Management Interrupt                 */
        BusFault_IRQn               = -11,    /*!< 5 Cortex-M3 Bus Fault Interrupt                         */
        UsageFault_IRQn             = -10,    /*!< 6 Cortex-M3 Usage Fault Interrupt                       */
        SVC_IRQn                    = -5,     /*!< 11 Cortex-M3 SV Call Interrupt                          */
        DebugMonitor_IRQn           = -4,     /*!< 12 Cortex-M3 Debug Monitor Interrupt                    */
        PendSV_IRQn                 = -2,     /*!< 14 Cortex-M3 Pend SV Interrupt                          */
        SysTick_IRQn                = -1,     /*!< 15 Cortex-M3 System Tick Interrupt                      */

        /******  STM32L specific Interrupt Numbers ***********************************************************/
        WWDG_IRQn                   = 0,      /*!< Window WatchDog Interrupt                               */
        PVD_IRQn                    = 1,      /*!< PVD through EXTI Line detection Interrupt               */
        TAMPER_STAMP_IRQn           = 2,      /*!< Tamper and TimeStamp interrupts through the EXTI line   */
        RTC_WKUP_IRQn               = 3,      /*!< RTC Wakeup Timer through EXTI Line Interrupt            */
        FLASH_IRQn                  = 4,      /*!< FLASH global Interrupt                                  */
        RCC_IRQn                    = 5,      /*!< RCC global Interrupt                                    */
        EXTI0_IRQn                  = 6,      /*!< EXTI Line0 Interrupt                                    */
        EXTI1_IRQn                  = 7,      /*!< EXTI Line1 Interrupt                                    */
        EXTI2_IRQn                  = 8,      /*!< EXTI Line2 Interrupt                                    */
        EXTI3_IRQn                  = 9,      /*!< EXTI Line3 Interrupt                                    */
        EXTI4_IRQn                  = 10,     /*!< EXTI Line4 Interrupt                                    */
        DMA1_Channel1_IRQn          = 11,     /*!< DMA1 Channel 1 global Interrupt                         */
        DMA1_Channel2_IRQn          = 12,     /*!< DMA1 Channel 2 global Interrupt                         */
        DMA1_Channel3_IRQn          = 13,     /*!< DMA1 Channel 3 global Interrupt                         */
        DMA1_Channel4_IRQn          = 14,     /*!< DMA1 Channel 4 global Interrupt                         */
        DMA1_Channel5_IRQn          = 15,     /*!< DMA1 Channel 5 global Interrupt                         */
        DMA1_Channel6_IRQn          = 16,     /*!< DMA1 Channel 6 global Interrupt                         */
        DMA1_Channel7_IRQn          = 17,     /*!< DMA1 Channel 7 global Interrupt                         */
        ADC1_IRQn                   = 18,     /*!< ADC1 global Interrupt                                   */
        USB_HP_IRQn                 = 19,     /*!< USB High Priority Interrupt                             */
        USB_LP_IRQn                 = 20,     /*!< USB Low Priority Interrupt                              */
        DAC_IRQn                    = 21,     /*!< DAC Interrupt                                           */
        COMP_IRQn                   = 22,     /*!< Comparator through EXTI Line Interrupt                  */
        EXTI9_5_IRQn                = 23,     /*!< External Line[9:5] Interrupts                           */
        LCD_IRQn                    = 24,     /*!< LCD Interrupt                                           */
        TIM9_IRQn                   = 25,     /*!< TIM9 global Interrupt                                   */
        TIM10_IRQn                  = 26,     /*!< TIM10 global Interrupt                                  */
        TIM11_IRQn                  = 27,     /*!< TIM11 global Interrupt                                  */
        TIM2_IRQn                   = 28,     /*!< TIM2 global Interrupt                                   */
        TIM3_IRQn                   = 29,     /*!< TIM3 global Interrupt                                   */
        TIM4_IRQn                   = 30,     /*!< TIM4 global Interrupt                                   */
        I2C1_EV_IRQn                = 31,     /*!< I2C1 Event Interrupt                                    */
        I2C1_ER_IRQn                = 32,     /*!< I2C1 Error Interrupt                                    */
        I2C2_EV_IRQn                = 33,     /*!< I2C2 Event Interrupt                                    */
        I2C2_ER_IRQn                = 34,     /*!< I2C2 Error Interrupt                                    */
        SPI1_IRQn                   = 35,     /*!< SPI1 global Interrupt                                   */
        SPI2_IRQn                   = 36,     /*!< SPI2 global Interrupt                                   */
        USART1_IRQn                 = 37,     /*!< USART1 global Interrupt                                 */
        USART2_IRQn                 = 38,     /*!< USART2 global Interrupt                                 */
        USART3_IRQn                 = 39,     /*!< USART3 global Interrupt                                 */
        EXTI15_10_IRQn              = 40,     /*!< External Line[15:10] Interrupts                         */
        RTC_Alarm_IRQn              = 41,     /*!< RTC Alarm through EXTI Line Interrupt                   */
        USB_FS_WKUP_IRQn            = 42,     /*!< USB FS WakeUp from suspend through EXTI Line Interrupt  */
        TIM6_IRQn                   = 43,     /*!< TIM6 global Interrupt                                   */
        TIM7_IRQn                   = 44,     /*!< TIM7 global Interrupt                                   */
        TIM5_IRQn                   = 46,     /*!< TIM5 global Interrupt                                   */
        SPI3_IRQn                   = 47,     /*!< SPI3 global Interrupt                                   */
        UART4_IRQn                  = 48,     /*!< UART4 global Interrupt                                  */
        UART5_IRQn                  = 49,     /*!< UART5 global Interrupt                                  */
        DMA2_Channel1_IRQn          = 50,     /*!< DMA2 Channel 1 global Interrupt                         */
        DMA2_Channel2_IRQn          = 51,     /*!< DMA2 Channel 2 global Interrupt                         */
        DMA2_Channel3_IRQn          = 52,     /*!< DMA2 Channel 3 global Interrupt                         */
        DMA2_Channel4_IRQn          = 53,     /*!< DMA2 Channel 4 global Interrupt                         */
        DMA2_Channel5_IRQn          = 54,     /*!< DMA2 Channel 5 global Interrupt                         */
        COMP_ACQ_IRQn               = 56,     /*!< Comparator Channel Acquisition global Interrupt         */
        LAST                        = 57, 
    }
}
