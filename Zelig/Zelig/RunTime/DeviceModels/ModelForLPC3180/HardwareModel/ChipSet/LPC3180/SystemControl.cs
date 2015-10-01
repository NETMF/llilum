//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.LPC3180
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x40004000U,Length=0x000000F0U)]
    public class SystemControl
    {
        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct PWR_CTRL_bitfield
        {
            [BitFieldRegister(Position=10)] public bool HCLK_SavePower;             // Force HCLK and ARMCLK to run from PERIPH_CLK in order to save power.
                                                                                    // *0 = Normal mode.
                                                                                    //  1 = ARM and AHB Matrix (HCLK) runs with PERIPH_CLK frequency.
                                                                                    //  
            [BitFieldRegister(Position= 9)] public bool SDRAM_SelfRefreshRequest;   // MPMCSREFREQ is used by the SDRAM interface, refer to the External Memory Controller chapter for details. 
                                                                                    // This value is not reflected on MPMCSREFREQ before either PWR_CTRL[8] is changed from 0 to 1 or
                                                                                    // the Start Controller brings the system out of STOP mode.
                                                                                    // *0 = No SDRAM self refresh.
                                                                                    //  1 = SDRAM self refresh request.
                                                                                    //  
            [BitFieldRegister(Position= 8)] public bool SDRAM_SelfRefreshReqEnable; // Update MPMCSREFREQ (SDRAM self refresh request).
                                                                                    // *0 = No action.
                                                                                    //  1 = Update MPMCSREFREQ according to PWR_CTRL[9]. Software must clear this bit again.
                                                                                    //  
            [BitFieldRegister(Position= 7)] public bool SDRAM_AutoExitSelfRefresh;  // SDRAM auto exit self refresh enable.
                                                                                    // If enabled, the SDRAM will automatically exit self refresh mode when the CPU exits STOP mode.
                                                                                    // Note: software must always clear this bit after exiting from STOP mode.
                                                                                    // *0 = Disable auto exit self refresh.
                                                                                    //  1 = Enable auto exit self refresh.
                                                                                    //  
            [BitFieldRegister(Position= 6)] public bool USB_HCLK_Disable;           // USB_HCLK control. Writing this bit to 1 will stop HCLK to the USB block.
                                                                                    // The clock can only be stopped when the USB block is idle and no accesses are done to the slave port.
                                                                                    // Note: software must always clear this bit after exiting from STOP mode.
                                                                                    // *0 = HCLK to the USB block is enabled.
                                                                                    //  1 = HCLK to the USB block is disabled. Lower power mode.
                                                                                    //  
            [BitFieldRegister(Position= 5)] public bool HIGHCORE_Pin;               // HIGHCORE pin level. Allows the HIGHCORE pin to be used as a GPO if bit 1 in this register is written with a 1.
                                                                                    // *0 = HIGHCORE will drive low.
                                                                                    //  1 = HIGHCORE will drive high.
                                                                                    //  
            [BitFieldRegister(Position= 4)] public bool SYSCLKEN_Pin;               // SYSCLKEN pin level. Can be used if using SYSCLK_EN pin as GPO. Bit 3 in this register should be set to 1 when using the pin as GPO.
                                                                                    //  0 = SYSCLKEN will drive low.
                                                                                    // *1 = SYSCLKEN will drive high.
                                                                                    //  
            [BitFieldRegister(Position= 3)] public bool SYSCLKEN_PinOverride;       // SYSCLKEN pin drives high when an external input clock on SYSXIN is requested. The pin is in high impedance mode when no external clock is needed.
                                                                                    // *0 = SYSCLKEN will drive high when not in STOP mode and 3-state in STOP mode.
                                                                                    //  1 = SYSCLKEN will always drive the level specified by bit 4.
                                                                                    //  
            [BitFieldRegister(Position= 2)] public bool NormalRunMode;              // RUN mode control. In Direct RUN mode the ARM, HCLK is clocked directly from the SYSCLK mux. This is the default setting.
                                                                                    // After the PLL outputs a stable clock, writing a 1 to this register will switch all the above clock sources to the PLL clock or divided versions of the PLL clock.
                                                                                    // Note: the HCLK PLL clock frequency must be higher than SYSCLK frequency.
                                                                                    // *0 = Direct RUN mode.
                                                                                    //  1 = Normal RUN mode. ARM, HCLK is sourced from the PLL output.
                                                                                    //  
            [BitFieldRegister(Position= 1)] public bool HighCoreForced;             // Core voltage supply level signalling control.
                                                                                    // The output pin HIGHCORE is defined to indicate nominal Core voltage when low and a lowered core voltage when driving high.
                                                                                    // Note: the HCLK PLL clock frequency must be higher than SYSCLK frequency.
                                                                                    //  0 = HIGHCORE pin will drive high during STOP mode and drive low in all other modes.
                                                                                    // *1 = HIGHCORE pin is always driving the level as specified in bit 5.
                                                                                    //  
            [BitFieldRegister(Position= 0)] public bool StopMode;                   // STOP mode control register.
                                                                                    // In STOP mode the two clock sources to the AHB/ARM clock mux is stopped.
                                                                                    // This means that the ARM, the ARM-PLL, and HCLK clocks are stopped.
                                                                                    // The USB clock is not stopped automatically by the STOP mode hardware.
                                                                                    // The USB clock may be left running or stopped by software while the system is in STOP mode.
                                                                                    //
                                                                                    // Read:
                                                                                    // *0 = The Device is not in STOP mode.
                                                                                    //  1 = An active start event has occurred after this bit has been written to a 1,
                                                                                    //      but before STOP mode has actually been entered by the hardware.
                                                                                    //      Software must restore this bit to 0 immediately after exiting STOP mode.
                                                                                    //
                                                                                    // Write:
                                                                                    //  0 = Restore value to 0 if STOP was never entered.
                                                                                    //  1 = Instruct hardware to enter STOP mode.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct OSC_CTRL_bitfield
        {
            [BitFieldRegister(Position=2,Size=7)] public uint Capacitance; //  0000000 = Don’t add any load capacitance to SYSX_IN and SYSX_OUT.
                                                                           //  0000000 = Don’t add any load capacitance to SYSX_IN and SYSX_OUT.
                                                                           //  xxxxxxx = Add (xxxxxxx binary × 0.1) pF load capacitance to SYSX_IN and SYSX_OUT.
                                                                           // *1000000 = Default setting of 6.4 pF added.
                                                                           // In total 12.7 pF (nominal value) can be added to the external load capacitors.
                                                                           // Capacitor value on the two pins is always programmed equal.
                                                                           // Any difference must be on the external capacitors.
                                                                           //  
            [BitFieldRegister(Position=1       )] public bool TestMode;    // Main oscillator test mode.
                                                                           // In test mode the oscillator will not oscillate but pass the external clock supplied at osc_in as the output clock.
                                                                           // In typical applications, this bit should be left at the default value.
                                                                           // *0 = Normal mode. Either oscillation mode or power down mode.
                                                                           //  1 = Test mode.
                                                                           //  
            [BitFieldRegister(Position=0       )] public bool PowerDown;   // Main oscillator enable.
                                                                           // *0 = Main oscillator is enabled.
                                                                           //  1 = Main oscillator is disabled and in power down mode.
        }
                                                              
        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct SYSCLK_CTRL_bitfield
        {
            [BitFieldRegister(Position=2,Size=10)] public uint BadPhaseRejection; // The number in this register is used by the clock switching circuitry to decide how long a bad
                                                                                  // The number in this register is used by the clock switching circuitry to decide how long a bad
                                                                                  // phase must be present before the clock switching is triggered. This register must always be
                                                                                  // written with a value before the clock switch is used in phase detect mode. The recommended
                                                                                  // value is 0x50, max value is 0xA9. (Higher values may result in no switching at all)
                                                                                  // Default: 0x2D2
                                                                                  //  
            [BitFieldRegister(Position=1        )] public bool PLL397_Enable;     // A write access to this bit triggers switching between the 13’ MHz clock source and the Main oscillator.
                                                                                  // Write:
                                                                                  // *0 = Switch to Main oscillator.
                                                                                  //  1 = Switch to 13’ MHz clock source (PLL397 output).
                                                                                  // Read: Returns the last written value.
                                                                                  //  
            [BitFieldRegister(Position=0        )] public bool MuxStatus;         // SYSCLK MUX status (Read only).
                                                                                  // *0 = Main oscillator selected as the clock source. (Default after external reset, not reset by watchdog reset)
                                                                                  //  1 = 13’ MHz PLL397 output selected as the clock source.
        }
                                                              
        //--//                                                

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct PLL397_CTRL_bitfield
        {
            [BitFieldRegister(Position=10      )] public bool LockStatusBackup; // PLL MSLOCK status (Read only)
                                                                                // This is a backup lock signal only to be used if the main lock signal in bit 0 is not functional.
                                                                                // This lock signal comes from a mixed signal lock detect circuit.
                                                                                // *0 = PLL is not locked.
                                                                                //  1 = PLL is locked. This means that the PLL output clock is stable.
                                                                                //  
            [BitFieldRegister(Position=9       )] public bool Bypass;           // PLL397 bypass control. For test only.
                                                                                // *0 = No bypass.
                                                                                //  1 = Bypass. PLL is bypassed and output clock is the input clock.
                                                                                //  
            [BitFieldRegister(Position=6,Size=3)] public uint ChargePumpBias;   // The number in this register is used by the clock switching circuitry to decide how long a bad
                                                                                // PLL397 charge pump bias control.
                                                                                // Note that -12.5 % of resistance means +12.5 % of the current.
                                                                                // *000 = Normal bias setting.
                                                                                //  001 = -12.5 % of resistance.
                                                                                //  010 = -25.0 % of resistance.
                                                                                //  011 = -37.5 % of resistance.
                                                                                //  100 = +12.5 % of resistance.
                                                                                //  101 = +25.0 % of resistance.
                                                                                //  110 = +37.5 % of resistance.
                                                                                //  111 = +50.0 % of resistance.
                                                                                //  
            [BitFieldRegister(Position=1       )] public bool Disable;          // PLL397 operational control.
                                                                                // Generally, most of the LPC3180, including the PLLs, will run from the main oscillator.
                                                                                // In this case the PLL397 should be stopped to save power.
                                                                                // However, it is possible to use the 13’ MHz clock from PLL397 instead. Upon reset,
                                                                                // PLL397 is started by default, but it is the main oscillator clock that is used by the
                                                                                // system. Note that after power-up or being turned on by software, PLL397 needs time
                                                                                // to stabilize and the PLL lock status must go active before the output clock is used.
                                                                                // Software can switch over to the PLL397 clock when it is locked.
                                                                                // *0 = PLL397 is running.
                                                                                //  1 = PLL397 is stopped and is in low power mode.
                                                                                //  
            [BitFieldRegister(Position=0       )] public bool LockStatus;       // PLL LOCK status (Read only)
                                                                                // *0 = PLL is not locked.
                                                                                //  1 = PLL is locked. This means that the PLL output clock is stable.SYSCLK MUX status (Read only).
        }
                                                              
        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct HCLKPLL_CTRL_bitfield
        {
            [BitFieldRegister(Position=16       )] public bool Enable;                  // PLL Power down.
                                                                                        // This bit is used to start/stop the PLL.
                                                                                        // Startup time must be respected from when the PLL is started until the output clock is used.
                                                                                        //  Startup time is indicated by PLL LOCK going high.
                                                                                        // *0 = PLL is in power down mode.
                                                                                        //  1 = PLL is in operating mode.
                                                                                        //  
            [BitFieldRegister(Position=15       )] public bool Bypass;                  // Bypass control.
                                                                                        // *0 = CCO clock is sent to post divider.
                                                                                        //  1 = PLL input clock bypasses the CCO and is sent directly to the post divider.
                                                                                        //  
            [BitFieldRegister(Position=14       )] public bool DirectOutput;            // Direct output control.
                                                                                        // *0 = The output of the post-divider is used as output of the PLL.
                                                                                        //  1 = CCO clock is the direct output of the PLL, bypassing the post divider.
                                                                                        //  
            [BitFieldRegister(Position=13       )] public bool FeedbackClockedByPLL397; // Feedback divider path control.
                                                                                        // *0 = Feedback divider clocked by CCO clock.
                                                                                        //  1 = Feedback divider clocked by FCLKOUT.PLL397 bypass control. For test only.
                                                                                        //  
            [BitFieldRegister(Position=11,Size=2)] public uint PostDivider;             // PLL post-divider (P) setting.
                                                                                        // This divider divides down the output frequency.
                                                                                        // If 50 % duty cycle is needed, the post-divider should always be active.
                                                                                        // *00 = divide by 2 (P=1)
                                                                                        //  01 = divide by 4 (P=2)
                                                                                        //  10 = divide by 8 (P=4)
                                                                                        //  11 = divide by 16 (P=8)
                                                                                        //  
            [BitFieldRegister(Position= 9,Size=2)] public uint PreDivider;              // PLL pre-divider (N) setting.
                                                                                        // This divider divides down the input frequency before going to the phase comparator.
                                                                                        // *00 = 1
                                                                                        //  01 = 2
                                                                                        //  10 = 3
                                                                                        //  11 = 4
                                                                                        //  
            [BitFieldRegister(Position= 1,Size=8)] public uint FeedbackDivider;         // PLL feedback divider (M) setting. 
                                                                                        // This divider divides down the output frequency before being fed back to the phase comparator.
                                                                                        // *00000000 = 1
                                                                                        //  00000001 = 2                                    
                                                                                        //  ...
                                                                                        //  11111110 = 255
                                                                                        //  11111111 = 256
                                                                                        //  
            [BitFieldRegister(Position= 0       )] public bool LockStatus;              // PLL LOCK status (Read only)
                                                                                        // *0 = PLL is not locked.
                                                                                        //  1 = PLL is locked. This means that the PLL output clock is stable.
        }

        //--//                                                
                                                              
        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct HCLKDIV_CTRL_bitfield
        {
            public enum ClockForDDRAM : uint
            {
                Stopped   = 0,
                Nominal   = 1,
                HalfSpeed = 2,
            }

            public enum DividerForHCLK : uint
            {
                Div1 = 0,
                Div2 = 1,
                Div4 = 2,
            }

            [BitFieldRegister(Position=7,Size=2)] public ClockForDDRAM  DDRAM_CLK;  // DDRAM_CLK control.
                                                                                    // Note that the clock architecture does not support using DDR SDRAM in Direct RUN mode.
                                                                                    // DDR SDRAM can only be accessed when in RUN mode and ARM runs twice or 4 times HCLK frequency.
                                                                                    // This divider divides down the output frequency.
                                                                                    // *00 = DDRAM clock stopped. Use this setting if external SDR SDRAM is used.
                                                                                    //  01 = DDRAM nominal speed. DDRAM clock is same speed at ARM. Software needs to make
                                                                                    //       sure that HCLK is half of this frequency. This is the normal setting for DDRAM.
                                                                                    //  10 = DDRAM half speed. DDRAM clock is half the frequency of ARM clock. Can be used if ARM
                                                                                    //       runs 4 times HCLK frequency.
                                                                                    //  11 = Not used.
                                                                                    //  
            [BitFieldRegister(Position=2,Size=5)] public uint           PERIPH_CLK; // PERIPH_CLK divider control.
                                                                                    // PERIPH_CLK is the clock going to APB/FAB slaves
                                                                                    // This setting may be programmed once after power up and may not be changed afterwards.
                                                                                    // This setting does not affect PERIPH_CLK frequency in Direct RUN mode.
                                                                                    // *00000 = PERIPH_CLK is ARM PLL clock in RUN mode.
                                                                                    //  00001 = PERIPH_CLK is ARM PLL clock divided by 2 in RUN mode.
                                                                                    //  ...
                                                                                    //  11110 = PERIPH_CLK is ARM PLL clock divided by 31 in RUN mode.
                                                                                    //  11111 = PERIPH_CLK is ARM PLL clock divided by 32 in RUN mode.
                                                                                    //  
            [BitFieldRegister(Position=0,Size=2)] public DividerForHCLK HCLK;       // HCLK divider control.
                                                                                    // This setting may typically be programmed once after power up and not changed afterwards.
                                                                                    // This setting do not affect HCLK frequency in Direct RUN mode.
                                                                                    // HCLK must not be set to a frequency higher than 104 MHz.
                                                                                    // This divider divides down the output frequency before being fed back to the phase comparator.
                                                                                    // *00 = HCLK is ARM PLL clock in RUN mode.
                                                                                    //  01 = HCLK is ARM PLL clock divided by 2 in RUN mode.
                                                                                    //  10 = HCLK is ARM PLL clock divided by 4 in RUN mode.
                                                                                    //  11 = Not used.
        }
                                                              
        //--//                                                

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct TEST_CLK_bitfield
        {
            [BitFieldRegister(Position=5,Size=2)] public uint SelectCLK1; // 
                                                                          // The selected clock is output on GPO_00 / TEST_CLK1 pin if bit 4 of this register contains a 1.
                                                                          // *00 = PERIPH_CLK. This clock stops in STOP mode.
                                                                          //  01 = RTC clock, un-synchronized version. Available in STOP mode also (32.768 kHz)
                                                                          //  10 = Main oscillator clock. Available in STOP mode as long as the main oscillator is enabled.
                                                                          //  11 = Not used.
                                                                          //  
            [BitFieldRegister(Position=4       )] public bool EnableCLK1; // *0 = GPO_00 / TST_CLK1 output is connected to the GPIO block.
                                                                          //  1 = GPO_00 / TST_CLK1 output is the clock selected by register bits [6:5].
                                                                          //  
            [BitFieldRegister(Position=1,Size=3)] public uint SelectCLK2; // 
                                                                          // The selected clock is output on the TST_CLK2 pin if bit 0 of this register contains a 1.
                                                                          // *000 = HCLK.
                                                                          //  001 = PERIPH_CLK.
                                                                          //  010 = USB clock (48 MHz output from USB PLL).
                                                                          //  011 = reserved.
                                                                          //  100 = reserved.
                                                                          //  101 = Main oscillator clock. Available in STOP mode as long as the main oscillator is enabled.
                                                                          //  110 = reserved.
                                                                          //  111 = PLL397 output clock (13.008896 MHz).
                                                                          //  
            [BitFieldRegister(Position=0       )] public bool EnableCLK2; // *0 = TST_CLK2 is turned off
                                                                          //  1 = TST_CLK2 outputs the clock selected by register bits [3:1]
        }

        //--//                                                

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct SW_INT_bitfield
        {
            [BitFieldRegister(Position=1,Size=7)] public uint Parameter; // Implemented as read/write register bits.
                                                                         // Can be used to pass a parameter to the interrupt service routine.
            [BitFieldRegister(Position=0       )] public bool Raise;     // 0 = SW_INT source inactive.
                                                                         // 1 = SW_INT source active. Software must ensure that this bit is high for
                                                                         // more than one SYSCLK period. This can be accomplished by causing
                                                                         // foreground software to set SW_INT[0] and the software interrupt service
                                                                         // routine to clear the bit
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct AUTOCLK_CTRL_bitfield
        {
            [BitFieldRegister(Position=6)] public bool PowerSave_USBIROM; // *0 = Autoclock enabled on USB Slave HCLK. Stops clocking after 128 HCLK of inactivity.
                                                                          //      There is one clock additional latency to access the USB block if the clock has been stopped.
                                                                          //  1 = Always clocked.
                                                                          //  
            [BitFieldRegister(Position=1)] public bool PowerSave_IRAM;    // *0 = Autoclock enabled on IRAM. Stops clocking after 16 HCLKs of inactivity.
                                                                          //      There is one clock additional latency to access the IRAM if the clock has been stopped.
                                                                          //  1 = Always clocked.
                                                                          //  
            [BitFieldRegister(Position=0)] public bool PowerSave_IROM;    // *0 = Autoclock enabled on IROM. Stops clocking after 8 HCLKs of inactivity.
                                                                          //      There is one clock additional latency to access the IROM if the clock has been stopped.
                                                                          //  1 = Always clocked.
        }

        //--//

        [Flags]
        public enum START_INT : uint
        {
            AD_IRQ              = 1U << 31,
            USB_AHB_NEED_CLK    = 1U << 26,
            MSTIMER_INT         = 1U << 25,
            RTC_INT             = 1U << 24,
            USB_NEED_CLK        = 1U << 23,
            USB_INT             = 1U << 22,
            USB_I2C_INT         = 1U << 21,
            USB_OTG_TIMER_INT   = 1U << 20,
            USB_OTG_ATX_INT_N   = 1U << 19,
            KEY_IRQ             = 1U << 16,
            GPIO_05             = 1U <<  5,
            GPIO_04             = 1U <<  4,
            GPIO_03             = 1U <<  3,
            GPIO_02             = 1U <<  2,
            GPIO_01             = 1U <<  1,
            GPIO_00             = 1U <<  0,
        }

        [Flags]
        public enum START_PIN : uint
        {
            U7_RX               = 1U << 31,
            U7_HCTS             = 1U << 30,
            U6_IRRX             = 1U << 28,
            U5_RX_or_USB_DAT_VP = 1U << 26,
            GPI_11              = 1U << 25,
            U3_RX               = 1U << 24,
            U2_HCTS             = 1U << 23,
            U2_RX               = 1U << 22,
            U1_RX               = 1U << 21,
            SDIO_INT_N          = 1U << 18,
            MSDIO_START         = 1U << 17,
            GPI_06_or_HSTIM_CAP = 1U << 16,
            GPI_05              = 1U << 15,
            GPI_04              = 1U << 14,
            GPI_03              = 1U << 13,
            GPI_02              = 1U << 12,
            GPI_01_or_SERVICE_N = 1U << 11,
            GPI_00              = 1U << 10,
            SYSCLKEN            = 1U <<  9,
            SPI1_DATIN          = 1U <<  8,
            GPI_07              = 1U <<  7,
            SPI2_DATIN          = 1U <<  6,
            GPI_10_or_U4_RX     = 1U <<  5,
            GPI_09              = 1U <<  4,
            GPI_08              = 1U <<  3,
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct DMACLK_CTRL_bitfield
        {
            [BitFieldRegister(Position=0)] public bool Enable; //  0 = All clocks to DMA stopped. No accesses to DMA registers are allowed.
                                                               // *1 = All clocks to DMA enabled.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct UARTCLK_CTRL_bitfield
        {
            [BitFieldRegister(Position=3)] public bool Uart6_Enable; //  0 = Uart6 HCLK disabled and in low power mode. No accesses to UART registers are allowed.
                                                                     // *1 = Uart6 HCLK enabled.
                                                                     //
            [BitFieldRegister(Position=2)] public bool Uart5_Enable; //  0 = Uart5 HCLK disabled and in low power mode. No accesses to UART registers are allowed.
                                                                     // *1 = Uart5 HCLK enabled.
                                                                     //
            [BitFieldRegister(Position=1)] public bool Uart4_Enable; //  0 = Uart4 HCLK disabled and in low power mode. No accesses to UART registers are allowed.
                                                                     // *1 = Uart4 HCLK enabled.
                                                                     //
            [BitFieldRegister(Position=0)] public bool Uart3_Enable; //  0 = Uart3 HCLK disabled and in low power mode. No accesses to UART registers are allowed.
                                                                     // *1 = Uart3 HCLK enabled.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct USB_CTRL_bitfield
        {
            [BitFieldRegister(Position=24       )] public bool HCLK_Enable;              // USB Slave HCLK control.
                                                                                         // *0 = Slave HCLK disabled.  
                                                                                         //  1 = Slave HCLK enabled.
                                                                                         //  
            [BitFieldRegister(Position=23       )] public bool USB_I2C_Enable;           // Control signal for mux. the mux drives a "0" out on USB_OE_TP_N when set.
                                                                                         // This enables “transparent I2C mode” for communication with an external USB transceiver.
                                                                                         // *0 = ip_3506_otg_tx_en_n is fed to OE_TP_N pad.
                                                                                         //  1 = ’0’ is fed to OE_TP_N pad.
                                                                                         //  
            [BitFieldRegister(Position=22       )] public bool USB_Dev_Need_Clk_Enable;  // During initialization the usb_dev_need_clk should not be fed to the clock switch.
                                                                                         // After initializing the external USB transceiver, this bit should be programmed to "1".
                                                                                         // Note that setting this bit to "0" also disables the software request in OTG_CLOCK_CONTROL register.
                                                                                         // *0 = usb_dev_need_clk is not let into the clock switch.
                                                                                         //  1 = usb_dev_need_clk is let into clock switch.
                                                                                         // 
            [BitFieldRegister(Position=21       )] public bool USB_Host_Need_Clk_Enable; // During initialization the usb_host_need_clk_en should not be fed to the clock switch.
                                                                                         // After initializing the external USB transceiver, this bit should be programmed to "1".
                                                                                         // Note that setting this bit to "0" also disables the software request in OTG_CLOCK_CONTROL register.
                                                                                         // *0 = usb_host_need_clk_en is not let into the clock switch.
                                                                                         //  1 = usb_host_need_clk_en is let into clock switch.
                                                                                         // 
            [BitFieldRegister(Position=19,Size=2)] public uint PadControl;               // 
                                                                                         // Pad control for USB_DAT_VP and USB_SE0_VM pads.
                                                                                         //  00 = Pull-up added to pad.
                                                                                         // *01 = Bus keeper. Retains the last driven value.
                                                                                         //  10 = No added function.
                                                                                         //  11 = Pull-down added to pad.
                                                                                         //  
            [BitFieldRegister(Position=18       )] public bool USB_Clken2;               // USB_Clken2 clock control.
                                                                                         // This bit must be written to a 1 after the PLL indicates stable output clock.
                                                                                         // *0 = Stop clock going into USB block.
                                                                                         //  1 = Enable clock going into USB block.
                                                                                         //
            [BitFieldRegister(Position=17       )] public bool USB_Clken1;               // USB_Clken1 clock control.
                                                                                         // This bit should be written to a 0 when USB is not active.
                                                                                         // *0 = Stop clock going into the USB PLL.
                                                                                         //  1 = Enable clock going into the USB PLL.
                                                                                         //
            [BitFieldRegister(Position=16       )] public bool PLL_PowerDown;            // PLL Power down.
                                                                                         // This bit is used to start/stop the PLL.
                                                                                         // Startup time must be respected from when the PLL is started until the output clock is used.
                                                                                         // Startup time is indicated by PLL LOCK going high.
                                                                                         // *0 = PLL is in power down mode.
                                                                                         //  1 = PLL is in operating mode.
                                                                                         //
            [BitFieldRegister(Position=15       )] public bool Bypass;                   // Bypass control.
                                                                                         // *0 = CCO clock is sent to post divider.
                                                                                         //  1 = PLL input clock bypasses the CCO and is sent directly to the post divider.
                                                                                         //
            [BitFieldRegister(Position=14       )] public bool DirectOutput;             // Direct output control.
                                                                                         // *0 = The output of the post-divider is used as output of the PLL.
                                                                                         //  1 = CCO clock is the direct output of the PLL, bypassing the post divider.
                                                                                         //
            [BitFieldRegister(Position=13       )] public bool FeedbackPath;             // Feedback divider path control.
                                                                                         // *0 = Feedback divider clocked by CCO clock.
                                                                                         //  1 = Feedback divider clocked by post FCLKOUT.
                                                                                         // 
            [BitFieldRegister(Position=11,Size=2)] public uint PostDivider;              // PLL post-divider (P) setting.
                                                                                         // This divider divides down the output frequency.
                                                                                         // If 50 % duty cycle is needed, the post-divider should always be active.
                                                                                         // *00 = divide by 2 (P=1)
                                                                                         //  01 = divide by 4 (P=2)
                                                                                         //  10 = divide by 8 (P=4)
                                                                                         //  11 = divide by 16 (P=8)
                                                                                         //  
            [BitFieldRegister(Position= 9,Size=2)] public uint PreDivider;               // PLL pre-divider (N) setting.
                                                                                         // This divider divides down the input frequency before going to the phase comparator.
                                                                                         // *00 = 1
                                                                                         //  01 = 2
                                                                                         //  10 = 3
                                                                                         //  11 = 4
                                                                                         //  
            [BitFieldRegister(Position= 1,Size=8)] public uint FeedbackDivider;          // PLL feedback divider (M) setting. 
                                                                                         // This divider divides down the output frequency before being fed back to the phase comparator.
                                                                                         // *00000000 = 1
                                                                                         //  00000001 = 2                                    
                                                                                         //  ...
                                                                                         //  11111110 = 255
                                                                                         //  11111111 = 256
                                                                                         //  
            [BitFieldRegister(Position= 0       )] public bool LockStatus;               // PLL LOCK status (Read only)
                                                                                         // *0 = PLL is not locked.
                                                                                         //  1 = PLL is locked. This means that the PLL output clock is stable.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct SDRAMCLK_CTRL_bitfield
        {
            [BitFieldRegister(Position=22       )] public bool SDRAM_PIN_SPEED3;   // This signal controls the slew rate of the pin SDRAM pin RAM_CLK. See bit 20 for details.
                                                                                   // *0 = Fast slew rate.
                                                                                   //  1 = Slower slew rate.
                                                                                   // 
            [BitFieldRegister(Position=21       )] public bool SDRAM_PIN_SPEED2;   // This signal controls the slew rate of the pins SDRAM pads RAM_A[14:0], RAM_CKE, RAM_CS_N, RAM_RAS_N, RAM_CAS_N, and RAM_WR_N.
                                                                                   // *0 = Fast slew rate.
                                                                                   //  1 = Slower slew rate.
                                                                                   // 
            [BitFieldRegister(Position=20       )] public bool SDRAM_PIN_SPEED1;   // This signal controls the slew rate of the pins SDRAM pads RAM_D[31:0], and RAM_DQM[3:0]. Normally fast slew rate is used.
                                                                                   // *0 = Fast slew rate.
                                                                                   //  1 = Slower slew rate.
                                                                                   // 
            [BitFieldRegister(Position=19       )] public bool SW_DDR_RESET;       // When writing from 0 to 1 a reset is applied to the SDRAM controller.
                                                                                   // Must be set back to 0.
                                                                                   // This may be used when the SDRAM controller is in DDR mode and the clocks are not properly synchronized when starting and stopping clocks.
                                                                                   // Note: DDRAM_CLK must not be running while resetting the SDRAM controller (HCLKDIV_CTRL[8:7] must be [00])
                                                                                   // *0 = No SDRAM controller reset.
                                                                                   //  1 = Active SDRAM controller reset.
                                                                                   //  
            [BitFieldRegister(Position=14,Size=5)] public uint HCLKDELAY_DELAY;    // 
                                                                                   // These bits control the delay of the HCLKDELAY input from the HCLK.
                                                                                   // The HCLKDELAY clock is used to send command, data and address to SDRAM.
                                                                                   // Note that all timing is for nominal process, temperature, voltage.
                                                                                   // The timing must be calibrated by software using the Ring oscillator.
                                                                                   // Delay = value programmed × 0.25ns.
                                                                                   // Note: All bit combinations can be used. Max delay is 7.75 ns.
                                                                                   // Default = 0
                                                                                   // 
            [BitFieldRegister(Position=13       )] public bool DelayAdderOverflow; // Delay circuitry Adder status.
                                                                                   // Reading a 1 here means that a value too close to min/max has been programmed in DDR_CAL_DELAY or the sensitivity has been programmed too high in SDRAMCLK_CTRL[12:10]
                                                                                   // *0 = No overflow or sign bit.
                                                                                   //  1 = Last calibration produced either an overflow or a negative number (underflow).
                                                                                   // 
            [BitFieldRegister(Position=10,Size=3)] public uint Sensitivity;        // 
                                                                                   // Sensitivity Factor for DDR SDRAM calibration.
                                                                                   // This value controls how much the error value is shifted down.
                                                                                   // More shifting means less sensitivity of the calibration.
                                                                                   // *000 = No right shift.
                                                                                   //  ...
                                                                                   //  111 = Shift right with 7.
                                                                                   //
            [BitFieldRegister(Position= 9       )] public bool CAL_DELAY;          // *0 = Use un-calibrated delay settings for DDR SDRAM.
                                                                                   //  1 = Use calibrated delay settings for DDR SDRAM.
                                                                                   // 
            [BitFieldRegister(Position= 8       )] public bool SW_DDR_CAL;         // When writing from 0 to 1 a DDR calibration is performed. Must be set back to 0.
                                                                                   // *0 = No manual DDR delay calibration.
                                                                                   //  1 = Perform a DDR delay calibration.
                                                                                   // 
            [BitFieldRegister(Position= 7       )] public bool RTC_TICK_EN;        // *0 = No automatic DDR delay calibration.
                                                                                   //  1 = Enable automatic DDR delay calibration on each RTC TICK.
                                                                                   // 
            [BitFieldRegister(Position= 2,Size=5)] public uint DDR_DQSIN_DELAY;    // 
                                                                                   // These bits control the delay of the DQS input from the DDR SDRAM device.
                                                                                   // The DQS signal is used to capture read data from SDRAM. Note that all timing is for nominal process, temperature, voltage.
                                                                                   // The timing must be calibrated by software using the Ring Oscillator.
                                                                                   // Refer to the section on DDR DQS delay calibration in the SDRAM Controller chapter for details.
                                                                                   // Delay = value programmed × 0.25ns.
                                                                                   // Note: All bit combinations can be used. Max delay is 7.75 ns.
                                                                                   // Default = 0
                                                                                   // 
            [BitFieldRegister(Position= 1       )] public bool DDR_SEL;            // This affects the pin multiplexing as described elsewhere in this chapter.
                                                                                   // *0 = SDR SDRAM is used.
                                                                                   //  1 = DDR SDRAM is used. In this mode, the DQS delay circuitry is also enabled.
                                                                                   // 
            [BitFieldRegister(Position= 0       )] public bool PowerDown;          // *0 = SDRAM HCLK and Inverted HCLK enabled.
                                                                                   //  1 = All Clocks to SDRAM block disabled. Note that no masters can access the SDRAM controller in this mode.
        }

        //--//


        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MS_CTRL_bitfield
        {
            [BitFieldRegister(Position=9       )] public bool PullUps_Enable;   // Enables pull-ups to MSSDIO pins.
                                                                                // If the SD Card interface is not used, this bit should be programmed to 0, and bits 6 through 8 should be programmed to 1.
                                                                                // *0 = MSSDIO pull-up disabled.
                                                                                //  1 = MSSDIO pull-up enable.
                                                                                // 
            [BitFieldRegister(Position=8       )] public bool PadControl_2and3; // MSSDIO2 and MSSDIO3 pad control.
                                                                                // *0 = MSSDIO2 and 3 pad has pull-up enabled.
                                                                                //  1 = MSSDIO2 and 3 pad has no pull-up.
                                                                                // 
            [BitFieldRegister(Position=7       )] public bool PadControl_1;     // MSSDIO1 pad control.
                                                                                // *0 = MSSDIO1 pad has pull-up enabled.
                                                                                //  1 = MSSDIO1 pad has no pull-up.
                                                                                // 
            [BitFieldRegister(Position=6       )] public bool PadControl_0;     // MSSDIO0/MSBS pad control.
                                                                                // *0 = MSSDIO0 pad has pull-up enable.
                                                                                //  1 = MSSDIO0 pad has no pull-up.
                                                                                // 
            [BitFieldRegister(Position=5       )] public bool ClockControl;     // SD Card clock control.
                                                                                // This bit controls MSSDCLK to the SD Card block. 
                                                                                // The registers in the peripheral block cannot be accessed if the clock is stopped.
                                                                                // *0 = Clocks disabled.
                                                                                //  1 = Clocks enabled.
                                                                                // 
            [BitFieldRegister(Position=0,Size=4)] public uint Divider;          // These register bits control the divider ratio when generating the clock from the ARM PLL output clock.
                                                                                // Software must insure that the maximum clock frequency of the targeted device is not exceeded.
                                                                                // *0000 = MSSDCLK stopped. Divider in low power mode.
                                                                                //  0001 = MSSDCLK equals ARM PLL output clock divided by 1.
                                                                                //  ...
                                                                                //  1110 = MSSDCLK equals ARM PLL output clock divided by 14.
                                                                                //  1111 = MSSDCLK equals ARM PLL output clock divided by 15.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct I2CCLK_CTRL_bitfield
        {
            [BitFieldRegister(Position=4)] public bool DriverStrength_USB; // Driver strength control for USB_I2C_SCL and USB_I2C_SDA. For 1.8 V operation set this bit to 1.
                                                                           // *0 = USB I2C pins operate in low drive mode.
                                                                           //  1 = USB I2C pins operate in high drive mode.
                                                                           // 
            [BitFieldRegister(Position=3)] public bool DriverStrength_2;   // I2C2_SCL and I2C2_SDA driver strength control. For 1.8 V operation set this bit to 1.
                                                                           // *0 = I2C2 pins operate in low drive mode.
                                                                           //  1 = I2C2 pins operate in high drive mode.
                                                                           // 
            [BitFieldRegister(Position=2)] public bool DriverStrength_1;   // I2C1_SCL and I2C1_SDA driver strength control. For 1.8 V operation set this bit to 1.
                                                                           // *0 = I2C1 pins operate in low drive mode.
                                                                           //  1 = I2C1 pins operate in high drive mode.
                                                                           // 
            [BitFieldRegister(Position=1)] public bool ClockControl_2;     // Software must set this bit before using the I2C2 block.
                                                                           // It can be cleared if the I2C2 block is not in use.
                                                                           // *0 = I2C2 HCLK stopped. No I2C registers are accessible.
                                                                           //  1 = I2C2 HCLK enabled.
                                                                           // 
            [BitFieldRegister(Position=0)] public bool ClockControl_1;     // Software must set this bit before using the I2C1 block.
                                                                           // It can be cleared if the I2C1 block is not in use.
                                                                           // *0 = I2C1 HCLK stopped. No I2C registers are accessible.
                                                                           //  1 = I2C1 HCLK enabled.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct KEYCLK_CTRL_bitfield
        {
            [BitFieldRegister(Position=0)] public bool Enable; //  0 = Disable clock to Keyboard block.
                                                               // *1 = Enable clock.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct ADCLK_CTRL_bitfield
        {
            [BitFieldRegister(Position=0)] public bool Enable; //  0 = Disable 32 kHz clock to ADC block.
                                                               // *1 = Enable clock.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct PWMCLK_CTRL_bitfield
        {
            [BitFieldRegister(Position=8,Size=4)] public uint PWM2_FREQ;         // Controls the clock divider for PWM2.
                                                                                 // *0000: PWM2_CLK = off
                                                                                 //  0001: PWM2_CLK = CLKin
                                                                                 //  ...
                                                                                 //  1111: PWM2_CLK = CLKin / 15
                                                                                 // 
            [BitFieldRegister(Position=4,Size=4)] public uint PWM1_FREQ;         // Controls the clock divider for PWM1.
                                                                                 // *0000: PWM1_CLK = off
                                                                                 //  0001: PWM1_CLK = CLKin
                                                                                 //  ...
                                                                                 //  1111: PWM1_CLK = CLKin / 15
            [BitFieldRegister(Position=3       )] public bool PWM2_ClockSelect;  // PWM2 clock source selection:
                                                                                 // *0: 32 kHz RTC_CLK
                                                                                 //  1: PERIPH_CLK
                                                                                 // 
            [BitFieldRegister(Position=2       )] public bool PWM2_ClockControl; // *0: Disable clock to PWM2 block.
                                                                                 //  1: Enable clock to PWM2 block.
                                                                                 // 
            [BitFieldRegister(Position=1       )] public bool PWM1_ClockSelect;  // PWM1 clock source selection:
                                                                                 // *0: 32 kHz RTC_CLK
                                                                                 //  1: PERIPH_CLK
                                                                                 // 
            [BitFieldRegister(Position=0       )] public bool PWM1_ClockControl; // *0: Disable clock to PWM1 block.
                                                                                 //  1: Enable clock to PWM1 block.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct TIMCLK_CTRL_bitfield
        {
            [BitFieldRegister(Position=1)] public bool HSTimer_Enable;  // HSTimer clock enable control.
                                                                        // *0: Disable clock.
                                                                        //  1: Enable clock.
                                                                        //
            [BitFieldRegister(Position=0)] public bool Watchdog_Enable; // Watchdog clock enable control.
                                                                        // *0: Disable clock.
                                                                        //  1: Enable clock.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct SPI_CTRL_bitfield
        {
            [BitFieldRegister(Position=7)] public bool SPI2_DATIO_Level;  // SPI2_DATIO output level.
                                                                          // *0: The pin drives low if bit 5 is 0.
                                                                          //  1: The pin drives high if bit 5 is 0.
                                                                          //
            [BitFieldRegister(Position=6)] public bool SPI2_CLK_Level;    // SPI2_CLK output level.
                                                                          // *0: The pin drives low if bit 5 is 0.
                                                                          //  1: The pin drives high if bit 5 is 0.
                                                                          //
            [BitFieldRegister(Position=5)] public bool SPI2_PinControl;   // Output pin control.
                                                                          // By default, the SPI2_DATIO and SPI2_CLK pins are driven to the values set in bits 7 and 6.
                                                                          // In order to use the SPI2 block, this bit must be written to a 1.
                                                                          // *0: SPI2_DATIO and SPI2_CLK outputs the level set by bit 6 and 7.
                                                                          //  1: SPI2_DATIO and SPI2_CLK are driven by the SPI2 block.
            [BitFieldRegister(Position=4)] public bool SPI2_ClockControl; // SPI2 clock enable control.
                                                                          // *0: Disable clock.
                                                                          //  1: Enable clock.
                                                                          //
            [BitFieldRegister(Position=3)] public bool SPI1_DATIO_Level;  // SPI1_DATIO output level.
                                                                          // *0: The pin drives low if bit 1 is 0.
                                                                          //  1: The pin drives high if bit 1 is 0.
                                                                          //
            [BitFieldRegister(Position=2)] public bool SPI1_CLK_Level;    // SPI1_CLK output level.
                                                                          // *0: The pin drives low if bit 1 is 0.
                                                                          //  1: The pin drives high if bit 1 is 0.
                                                                          //
            [BitFieldRegister(Position=1)] public bool SPI1_PinControl;   // Output pin control.
                                                                          // By default, the SPI1_DATIO and SPI1_CLK pins are driven to the values set in bits 3 and 2.
                                                                          // In order to use the SPI1 block, this bit must be written to a 1.
                                                                          // *0: SPI1_DATAIO and SPI1_CLK outputs the level set by bit 2 and 3.
                                                                          //  1: SPI1_DATIO and SPI1_CLK are driven by the SPI1 block.
            [BitFieldRegister(Position=0)] public bool SPI1_ClockControl; // SPI1 clock enable control.
                                                                          // *0: Disable clock.
                                                                          //  1: Enable clock.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct FLASHCLK_CTRL_bitfield
        {
            [BitFieldRegister(Position=5)] public bool InterruptFromMLC;    // Determines which NAND Flash controller interrupt is connected to the interrupt controller.
                                                                            // *0: enable the SLC (single level) NAND Flash controller interrupt.
                                                                            //  1: enable the MLC (multi-level) NAND Flash controller interrupt.
                                                                            //
            [BitFieldRegister(Position=4)] public bool NAND_DMA_REQ_on_RnB; // Enable NAND_DMA_REQ on NAND_RnB. This applies only to the MLC.
                                                                            // *0: Disable.
                                                                            //  1: Enable.
                                                                            //
            [BitFieldRegister(Position=3)] public bool NAND_DMA_REQ_on_INT; // Enable NAND_DMA_REQ on NAND_INT. This applies only to the MLC.
                                                                            // *0: Disable.
                                                                            //  1: Enable.
                                                                            //
            [BitFieldRegister(Position=2)] public bool EnableSLC;           // SLC/MLC select.
                                                                            // Selects either the single-level (SLC), or multi-level (MLC) NAND Flash controller.
                                                                            // *0: Select MLC flash controller.
                                                                            //  1: Select SLC flash controller.
                                                                            //
            [BitFieldRegister(Position=1)] public bool MLC_ClockEnable;     // MLC NAND Flash clock enable control.
                                                                            //  0: Disable clocks to the block, including the AHB interface.
                                                                            //  1: Enable clock.
                                                                            //
            [BitFieldRegister(Position=0)] public bool SLC_ClockEnable;     // SLC NAND Flash clock enable control.
                                                                            //  0: Disable clocks to the block, including the AHB interface.
                                                                            //  1: Enable clock.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct UxCLK_bitfield
        {
            [BitFieldRegister(Position=15       )] public bool UseHCLK; // Clock source select
                                                                        // *0: Use PERIPH_CLK as input clock to the X/Y divider.
                                                                        //  1: Use HCLK as input to the X/Y divider.
                                                                        //  
            [BitFieldRegister(Position= 8,Size=7)] public uint X;       // 
                                                                        // X divider value
                                                                        // If this value is set to 0, the output clock is stopped and the divider is put in a low power mode.
                                                                        //  
            [BitFieldRegister(Position= 0,Size=8)] public uint Y;       // 
                                                                        // Y divider value
                                                                        // If this value is set to 0, the output clock is stopped and the divider is put in a low power mode.
                                                                        //  
                                                                        // The X/Y divider divides the selected input clock using an X/Y divider.
                                                                        // The output should be set to either 16 times the UART bit rate to be used, or a
                                                                        // higher frequency if the UART baud rate generator (using the UnDLM and UnDLL registers)
                                                                        // divides further down. Dividing directly down to 16 times the required bit rate is the most power efficient method.
                                                                        // Note that the X/Y divider cannot multiply the clock rate. The X value must be less than or equal to the Y value.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct IRDACLK_bitfield
        {
            [BitFieldRegister(Position= 8,Size=8)] public uint X; // 
                                                                  // X divider value
                                                                  // If this value is set to 0, the output clock is stopped and the divider is put in a low power mode.
                                                                  //  
            [BitFieldRegister(Position= 0,Size=8)] public uint Y; // 
                                                                  // Y divider value
                                                                  // If this value is set to 0, the output clock is stopped and the divider is put in a low power mode.
                                                                  //  
                                                                  // The X/Y divider divides the selected input clock using an X/Y divider.
                                                                  // The output should be set to either 16 times the UART bit rate to be used, or a
                                                                  // higher frequency if the UART baud rate generator (using the UnDLM and UnDLL registers)
                                                                  // divides further down. Dividing directly down to 16 times the required bit rate is the most power efficient method.
                                                                  //  
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct UART_CTRL_bitfield
        {
            [BitFieldRegister(Position=10       )] public bool HDPX_INV;     // *0 = IRRX6 is not inverted.
                                                                             //  1 = IRRX6 is inverted.
                                                                             //  
                                                                             // This inversion comes in addition to the IRRX6_INV controlled inversion.
                                                                             //  
            [BitFieldRegister(Position= 9       )] public bool HDPX_EN;      // *0 = IRRX6 is not disabled by TXD.
                                                                             //  1 = IRRX6 is masked while TXD is low.
                                                                             //  
                                                                             // This is used for stopping IRRXD6 data received from the IrDA transceiver while transmitting (optical reflection suppression).
                                                                             //  
            [BitFieldRegister(Position= 5       )] public bool UART6_IRDA;   // *0 = UART6 uses the IrDA modulator/demodulator.
                                                                             //  1 = UART6 bypasses the IrDA modulator/demodulator.
                                                                             //  
            [BitFieldRegister(Position= 4       )] public bool IRTX6_INV;    // *0 = The IRTX6 pin is not inverted.
                                                                             //  1 = The IRTX6 pin is inverted.
                                                                             //  
            [BitFieldRegister(Position= 3       )] public bool IRRX6_INV;    // *0 = The IRRX6 pin is not inverted.
                                                                             //  1 = The IRRX6 pin is inverted.
                                                                             //  
            [BitFieldRegister(Position= 2       )] public bool IR_RxLength;  // *0 = The IRDA expects Rx pulses 3/16 of the selected bit period.
                                                                             //  1 = The IRDA expects Rx pulses 3/16 of a 115.2 kbps bit period.
                                                                             //  
            [BitFieldRegister(Position= 1       )] public bool IR_TxLength;  // *0 = The IRDA Tx uses 3/16 of the selected bit period.
                                                                             //  1 = The IRDA Tx uses 3/16 of a 115.2 kbps bit period.
                                                                             //  
            [BitFieldRegister(Position= 0       )] public bool UART5_MODE;   // *0 = The UART5 TX/RX function is only routed to the U5_TX and U5_RX pins.
                                                                             //  1 = The UART5 TX/RX function is also routed to the USB D+ and D- pins.
                                                                             //  
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct UART_CLKMODE_bitfield
        {
            public enum Mode : uint
            {
                ClockOff  = 0, // *00: Clock off mode (default)
                ClockOn   = 1, //  01: Clock on mode
                AutoClock = 2, //  10: Auto clock mode
            }
                                                                          //  11: Not used
            [BitFieldRegister(Position=22       )] public bool CLK_STAT7; // If set, UART 7 clock is running.
            [BitFieldRegister(Position=21       )] public bool CLK_STAT6; // If set, UART 6 clock is running.
            [BitFieldRegister(Position=20       )] public bool CLK_STAT5; // If set, UART 5 clock is running.
            [BitFieldRegister(Position=19       )] public bool CLK_STAT4; // If set, UART 4 clock is running.
            [BitFieldRegister(Position=18       )] public bool CLK_STAT3; // If set, UART 3 clock is running.
            [BitFieldRegister(Position=17       )] public bool CLK_STAT2; // If set, UART 2 clock is running.
            [BitFieldRegister(Position=16       )] public bool CLK_STAT1; // If set, UART 1 clock is running.
            [BitFieldRegister(Position=14       )] public bool CLK_STAT ; // If set, one or more UART clocks are running.
                                                                          //  
            [BitFieldRegister(Position=10,Size=2)] public Mode UART6_CLK; // Selects the clock mode for UART6.
            [BitFieldRegister(Position= 8,Size=2)] public Mode UART5_CLK; // Selects the clock mode for UART5.
            [BitFieldRegister(Position= 6,Size=2)] public Mode UART4_CLK; // Selects the clock mode for UART4.
            [BitFieldRegister(Position= 4,Size=2)] public Mode UART3_CLK; // Selects the clock mode for UART3.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct UART_LOOP_bitfield
        {
            [BitFieldRegister(Position=6)] public bool LOOPBACK7; // If set, UART 7 is in loopback mode.
            [BitFieldRegister(Position=5)] public bool LOOPBACK6; // If set, UART 6 is in loopback mode.
            [BitFieldRegister(Position=4)] public bool LOOPBACK5; // If set, UART 5 is in loopback mode.
            [BitFieldRegister(Position=3)] public bool LOOPBACK4; // If set, UART 4 is in loopback mode.
            [BitFieldRegister(Position=2)] public bool LOOPBACK3; // If set, UART 3 is in loopback mode.
            [BitFieldRegister(Position=1)] public bool LOOPBACK2; // If set, UART 2 is in loopback mode.
            [BitFieldRegister(Position=0)] public bool LOOPBACK1; // If set, UART 1 is in loopback mode.
        }

        //--//

        [Register(Offset=0x00000014U)] public uint                   BOOT_MAP; 
        [Register(Offset=0x00000020U)] public START_INT              START_ER_INT;  // Start Enable register - internal sources 0 R/W
        [Register(Offset=0x00000024U)] public START_INT              START_RSR_INT; // Start Raw status register, internal sources 0 R/W
        [Register(Offset=0x00000028U)] public START_INT              START_SR_INT;  // Start status register, internal sources 0 R/-
        [Register(Offset=0x0000002CU)] public START_INT              START_APR_INT; // Start activation Polarity register, internal sources 0 R/W
        [Register(Offset=0x00000030U)] public START_PIN              START_ER_PIN;  // Start Enable register - pin sources 0 R/W
        [Register(Offset=0x00000034U)] public START_PIN              START_RSR_PIN; // Start Raw status register, pin sources 0 R/W
        [Register(Offset=0x00000038U)] public START_PIN              START_SR_PIN;  // Start status register, pin sources 0 R/-
        [Register(Offset=0x0000003CU)] public START_PIN              START_APR_PIN; // Start activation Polarity register, pin sources 0 R/W

        [Register(Offset=0x00000040U)] public HCLKDIV_CTRL_bitfield  HCLKDIV_CTRL;  // HCLK divider settings 0 R/W
        [Register(Offset=0x00000044U)] public PWR_CTRL_bitfield      PWR_CTRL;      // AHB/ARM power control register 0x0000 0012 R/W
        [Register(Offset=0x00000048U)] public PLL397_CTRL_bitfield   PLL397_CTRL;   // PLL397 PLL control register 0 R/W
        [Register(Offset=0x0000004CU)] public OSC_CTRL_bitfield      OSC_CTRL;      // Main oscillator control register 0x0000 0100 R/W
        [Register(Offset=0x00000050U)] public SYSCLK_CTRL_bitfield   SYSCLK_CTRL;   // SYSCLK control register 0x0000 0B48 R/W
        [Register(Offset=0x00000058U)] public HCLKPLL_CTRL_bitfield  HCLKPLL_CTRL;  // ARM and HCLK PLL control register 0 R/W

        [Register(Offset=0x00000064U)] public USB_CTRL_bitfield      USB_CTRL;      // USB PLL and pad control register 0x0008 0000 R/W
        [Register(Offset=0x00000068U)] public SDRAMCLK_CTRL_bitfield SDRAMCLK_CTRL; // Controls various SDRAM configuration details. 0 R/W
        [Register(Offset=0x0000006CU)] public uint                   DDR_LAP_NOM;   // Contains the nominal value for DDR DQS input delay. 0 R/W
        [Register(Offset=0x00000070U)] public uint                   DDR_LAP_COUNT; // Value of the DDR SDRAM ring oscillator counter. 0 RO
        [Register(Offset=0x00000074U)] public uint                   DDR_CAL_DELAY; // Current calibrated value of the DDR DQS input delay. 0 RO
        [Register(Offset=0x00000080U)] public MS_CTRL_bitfield       MS_CTRL;       // SD Card interface clock and pad control 0 R/W
        [Register(Offset=0x000000A4U)] public TEST_CLK_bitfield      TEST_CLK;      // Clock testing control 0 R/W
        [Register(Offset=0x000000A8U)] public SW_INT_bitfield        SW_INT;        // Software Interrupt Register 0 R/W
        [Register(Offset=0x000000ACU)] public I2CCLK_CTRL_bitfield   I2CCLK_CTRL;   // I2C clock control register 0 R/W
        [Register(Offset=0x000000B0U)] public KEYCLK_CTRL_bitfield   KEYCLK_CTRL;   // Keypad clock control 0 R/W
        [Register(Offset=0x000000B4U)] public ADCLK_CTRL_bitfield    ADCLK_CTRL;    // ADC clock control 0 R/W
        [Register(Offset=0x000000B8U)] public PWMCLK_CTRL_bitfield   PWMCLK_CTRL;   // PWM clock control 0 R/W
        [Register(Offset=0x000000BCU)] public TIMCLK_CTRL_bitfield   TIMCLK_CTRL;   // Timer clock control 0 R/W
        [Register(Offset=0x000000C4U)] public SPI_CTRL_bitfield      SPI_CTRL;      // SPI1 and SPI2 clock and pin control 0 R/W
        [Register(Offset=0x000000C8U)] public FLASHCLK_CTRL_bitfield FLASHCLK_CTRL; // Flash clock control 0x0000 0003 R/W

        [Register(Offset=0x000000D0U)] public UxCLK_bitfield         U3CLK;         // UART 3 Clock Control Register 0 R/W
        [Register(Offset=0x000000D4U)] public UxCLK_bitfield         U4CLK;         // UART 4 Clock Control Register 0 R/W
        [Register(Offset=0x000000D8U)] public UxCLK_bitfield         U5CLK;         // UART 5 Clock Control Register 0 R/W
        [Register(Offset=0x000000DCU)] public UxCLK_bitfield         U6CLK;         // UART 6 Clock Control Register 0 R/W
        [Register(Offset=0x000000E0U)] public IRDACLK_bitfield       IRDACLK;       // IrDA Clock Control Register 0 R/W

        [Register(Offset=0x000000E4U)] public UARTCLK_CTRL_bitfield  UARTCLK_CTRL;  // General UART clock control register 0x0000 000F R/W
        [Register(Offset=0x000000E8U)] public DMACLK_CTRL_bitfield   DMACLK_CTRL;   // DMA clock control register 0x0000 0001 R/W
        [Register(Offset=0x000000ECU)] public AUTOCLK_CTRL_bitfield  AUTOCLK_CTRL;  // Auto clock control register 0 R/W
                                                                   
        [Register(Offset=0x00050000U)] public UART_CTRL_bitfield     UART_CTRL;     // UART Clock Control Register 0 R/W
        [Register(Offset=0x00050004U)] public UART_CLKMODE_bitfield  UART_CLKMODE;  // UART Clock Mode Register 0 R/W
        [Register(Offset=0x00050008U)] public UART_LOOP_bitfield     UART_LOOP;     // UART Loopback Control Register 0 R/W


        //
        // Helper Methods
        //

        [Inline]
        public void ConfigureClocks( uint SYSCLK     ,
                                     uint ARM_CLK    ,
                                     uint HCLK       ,
                                     uint PERIPH_CLK )
        {
            ConfigureHCLKPLL( ARM_CLK / SYSCLK );

            LockHCLKPLL();

            HCLKDIV_CTRL_bitfield.DividerForHCLK hclkDivider;

            switch(ARM_CLK / HCLK)
            {
                case 1 : hclkDivider = HCLKDIV_CTRL_bitfield.DividerForHCLK.Div1; break;
                case 2 : hclkDivider = HCLKDIV_CTRL_bitfield.DividerForHCLK.Div2; break;
                default: hclkDivider = HCLKDIV_CTRL_bitfield.DividerForHCLK.Div4; break;
            }

            ConfigureHCLK( ARM_CLK / PERIPH_CLK, hclkDivider );
        }

        [Inline]
        private void ConfigureHCLKPLL( uint feedbackDivider )
        {
            var val = new HCLKPLL_CTRL_bitfield();

            val.Enable          = true;
            val.DirectOutput    = true;
            val.FeedbackDivider = feedbackDivider - 1;

            this.HCLKPLL_CTRL = val;
        }

        [Inline]
        private void LockHCLKPLL()
        {
            while(this.HCLKPLL_CTRL.LockStatus == false)
            {
            }
        }

        [Inline]
        private void ConfigureHCLK( uint                                 peripheralDivider ,
                                    HCLKDIV_CTRL_bitfield.DividerForHCLK hclkDivider       )
        {
            var val = new HCLKDIV_CTRL_bitfield();

            val.DDRAM_CLK  = HCLKDIV_CTRL_bitfield.ClockForDDRAM.Stopped;
            val.PERIPH_CLK = peripheralDivider - 1;
            val.HCLK       = hclkDivider;

            this.HCLKDIV_CTRL = val;
        }

        [Inline]
        public void SwitchToRunMode()
        {
            this.PWR_CTRL.NormalRunMode = true;
        }

        [Inline]
        public void SwitchToDirectRunMode()
        {
            this.PWR_CTRL.NormalRunMode = false;
        }

        [Inline]
        public void SwitchToStopModeWithSDRAM()
        {
            //
            // Putting the SDRAM into Self Refresh mode.
            //

            this.PWR_CTRL.SDRAM_SelfRefreshRequest = true;

            this.PWR_CTRL.SDRAM_SelfRefreshReqEnable = true;
            this.PWR_CTRL.SDRAM_SelfRefreshReqEnable = false;

            //--//

            //
            // Preparing the SDRAM Controller to automatically come out of self refresh when out of stop mode.
            //

            this.PWR_CTRL.SDRAM_SelfRefreshRequest  = false;
            this.PWR_CTRL.SDRAM_AutoExitSelfRefresh = true;

            //--//

            SwitchToStopMode();

            this.PWR_CTRL.SDRAM_AutoExitSelfRefresh = false;
        }

        [Inline]
        public void SwitchToStopMode()
        {
            this.PWR_CTRL.StopMode = true;

            Processor.Nop();

            this.PWR_CTRL.StopMode = false;
        }

        public void SwitchClockToRTCOscillator()
        {
            this.PLL397_CTRL.Disable = false;

            while(this.PLL397_CTRL.LockStatus == false)
            {
            }

            this.SYSCLK_CTRL.PLL397_Enable = true;

            while(this.SYSCLK_CTRL.MuxStatus == false)
            {
            }

            this.OSC_CTRL.PowerDown = true;
        }

        public void SwitchClockToMainOscillator()
        {
            this.OSC_CTRL.PowerDown = false;

            if(this.PWR_CTRL.NormalRunMode)
            {
                Processor.Delay( 16000000, 208000000 );
            }
            else
            {
                Processor.Delay( 16000000, 13000000 );
            }

            this.SYSCLK_CTRL.PLL397_Enable = false;

            while(this.SYSCLK_CTRL.MuxStatus == true)
            {
            }

            this.PLL397_CTRL.Disable = true;
        }

        [Inline]
        public void EnableAutoSwitchToLowVoltageOnStopMode()
        {
            this.PWR_CTRL.HighCoreForced = false;
        }

        [Inline]
        public void DisableAutoSwitchToLowVoltageOnStopMode()
        {
            this.PWR_CTRL.HighCoreForced = true;
        }

        //
        // Access Methods
        //

        public static extern SystemControl Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}