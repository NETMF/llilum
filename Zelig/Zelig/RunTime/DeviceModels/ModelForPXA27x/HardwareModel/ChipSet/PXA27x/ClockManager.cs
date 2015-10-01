//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x41300000U,Length=0x00000010U)]
    public class ClockManager
    {
        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct CCCR_bitfield
        {
            [BitFieldRegister(Position=31      )] public bool CPDIS;        // Core PLL Output Disable
                                                                            //
                                                                            //   0 = Core PLL is enabled after frequency change.
                                                                            //   1 = Core PLL is disabled after frequency change if not selected as a clock source.
                                                                            //
            [BitFieldRegister(Position=30      )] public bool PPDIS;        // Peripheral PLL Output Disable
                                                                            //
                                                                            //  0 = Peripheral PLL is enabled after frequency change.
                                                                            //  1 = Peripheral PLL is disabled after frequency change if not selected as a clock source.
                                                                            //
            [BitFieldRegister(Position=27      )] public bool LCD_26;       // LCD Clock Frequency in Deep-Idle or 13M Mode
                                                                            //
                                                                            //  0 = LCD clock frequency is 13 MHz.
                                                                            //  1 = LCD clock frequency is 26 MHz.
                                                                            //
            [BitFieldRegister(Position=26      )] public bool PLL_EARLY_EN; // Early PLL Enable
                                                                            //
                                                                            // In 13M mode, allows software to enable the PLLs ahead of time, while
                                                                            // remaining fully operational in 13M mode. When the core and peripheral
                                                                            // PLLs have locked (CCSR[CPLK] and CCSR[PPLK] set), software can
                                                                            // perform the frequency change. This bit is automatically cleared after a
                                                                            // frequency change exiting from 13M mode.
                                                                            //
                                                                            //  0 = Do not enable the core and peripheral PLLs early.
                                                                            //  1 = Enable the core and peripheral PLLs early (all units and the core remain operational at 13 MHz).
                                                                            //
                                                                            // NOTE: Write to this bit only when the processor is in 13M mode and the
                                                                            //       core PLL has been disabled by setting CPDIS. In normal run mode,
                                                                            //       writing to this bit causes unpredictable results.
                                                                            //
            [BitFieldRegister(Position=25      )] public bool A;            // Alternate Setting for Memory Controller Clock
                                                                            //
                                                                            //  0 = Memory controller clock (CLK_MEM) clock frequency is as specified in Table 3-7.
                                                                            //  1 = Memory controller clock (CLK_MEM) clock frequency is the same as the system bus frequency.
                                                                            //
                                                                            // NOTE: Refer to section 6.5.1.4 for changing CLK_MEM while SDCLK <1> or <2> is at 104 MHz
                                                                            //
            [BitFieldRegister(Position=7,Size=3)] public uint N;            // Turbo-Mode-to-Run-Mode Ratio, N (Reset value 0b010 for N = 1)
                                                                            //
                                                                            //  0b000–0b010 = ratio (N) = 1
                                                                            //  0b011–0b110 = ratio (N) = (2N / 2)
                                                                            //  0b111 = reserved
                                                                            //
                                                                            // NOTE: Program this field with twice the value of N.
                                                                            //
            [BitFieldRegister(Position=0,Size=5)] public uint L;            // Run-Mode-to-Oscillator Ratio (Reset value 0b00111 for L=7)
                                                                            //
                                                                            //  0b00000–0b00010 = ratio = 2
                                                                            //  0b00011–0b11110 = ratio = L
                                                                            //  0b11111 = reserved
                                                                            //
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct CKEN_bitfield
        {
            [BitFieldRegister(Position=31)] public bool EnAC97b;            // AC ’97 Controller Clock Enable
            [BitFieldRegister(Position=25)] public bool EnTPM;              // TPM Unit Clock Enable
            [BitFieldRegister(Position=24)] public bool EnQuickCapture;     // Quick Capture Interface Clock Enable
            [BitFieldRegister(Position=23)] public bool EnSSP1;             // SSP1 Unit Clock Enable
            [BitFieldRegister(Position=22)] public bool EnMemoryController; // Memory Controller
            [BitFieldRegister(Position=21)] public bool EnMemoryStick;      // Memory Stick Host Controller
            [BitFieldRegister(Position=20)] public bool EnInternalMemory;   // Internal Memory Clock Enable
            [BitFieldRegister(Position=19)] public bool EnKeypad;           // Keypad Interface Clock Enable
            [BitFieldRegister(Position=18)] public bool EnUSIM;             // USIM Unit Clock Enable
            [BitFieldRegister(Position=17)] public bool EnMSL;              // MSL Interface Unit Clock Enable
            [BitFieldRegister(Position=16)] public bool EnLCD;              // LCD Controller Clock Enable
            [BitFieldRegister(Position=15)] public bool EnPMI2C;            // Power Manager I2C Unit Clock Enable
            [BitFieldRegister(Position=14)] public bool EnI2C;              // I2C Unit Clock Enable
            [BitFieldRegister(Position=13)] public bool EnIRDA;             // Infrared Port Clock Enable
            [BitFieldRegister(Position=12)] public bool EnMMC;              // MMC Controller Clock Enable
            [BitFieldRegister(Position=11)] public bool EnUSBClient;        // USB Client Unit Clock Enable
            [BitFieldRegister(Position=10)] public bool EnUSBHost;          // USB Host Unit Clock Enable
            [BitFieldRegister(Position=9 )] public bool EnOsTimer;          // OS Timer Unit Clock Enable
            [BitFieldRegister(Position=8 )] public bool EnI2S;              // I2S Unit Clock Enable
            [BitFieldRegister(Position=7 )] public bool EnBTUART;           // BTUART Unit Clock Enable
            [BitFieldRegister(Position=6 )] public bool EnFFUART;           // FFUART Unit Clock Enable
            [BitFieldRegister(Position=5 )] public bool EnSTUART;           // STUART Unit Clock Enable
            [BitFieldRegister(Position=4 )] public bool EnSSP3;             // SSP3 Unit Clock Enable
            [BitFieldRegister(Position=3 )] public bool EnSSP2;             // SSP2 Unit Clock Enable
            [BitFieldRegister(Position=2 )] public bool EnAC97a;            // AC ’97 Controller Clock Enable
            [BitFieldRegister(Position=0 )] public bool EnPWM;              // PWM Clock Enable
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct OSCC_bitfield
        {
            public enum Stabilization
            {
                Delay5ms   = 0, // 00 = Approximately 5 ms
                Delay3ms   = 1, // 01 = Approximately 3 ms
                Delay375us = 2, // 10 = Approximately 375 us
                Delay1us   = 3, // 11 = Approximately 1 us
            }

            [BitFieldRegister(Position=5,Size=2)] public Stabilization OSD;     // Processor (13-MHz) Oscillator Stabilization Delay
                                                                                //
            [BitFieldRegister(Position=4       )] public bool          CRI;     // Clock Request Input (External Processor Oscillator) Status
                                                                                //
                                                                                //  0 = The processor oscillator must be supplied using PXTAL_IN and PXTAL_OUT (the CLK_REQ pin was driven low during hardware or watchdog reset).
                                                                                //  1 = The processor oscillator is supplied externally (the CLK_REQ pin was floated during hardware or watchdog reset).
                                                                                //
                                                                                // NOTE: See the requirements for each of these conditions (listed in Section 3.8.2.3).
                                                                                //
            [BitFieldRegister(Position=3       )] public bool          PIO_EN;  // 13-MHz Processor Oscillator Output Enable
                                                                                //
                                                                                //  0 = CLK_PIO is not used; pin can be used as GPIO.
                                                                                //  1 = CLK_PIO is driven at the same frequency as the processor oscillator, regardless of the GPIO configuration. This configuration is ignored if CRI is set.
                                                                                //
            [BitFieldRegister(Position=2       )] public bool          TOUT_EN; // Timekeeping (32.768 kHz) Oscillator Output Enable
                                                                                //
                                                                                //  0 = CLK_TOUT is not used; pin can be used as GPIO.
                                                                                //  1 = CLK_TOUT is driven at the same frequency as the timekeeping oscillator, regardless of the GPIO configuration. If CRI is set,
                                                                                //
                                                                                // TOUT_EN is set out of power-on or hardware reset but can be set or cleared at any time.
                                                                                //
            [BitFieldRegister(Position=1       )] public bool          OON;     // Timekeeping (32.768 kHz) Oscillator On (Write once only)
                                                                                //
                                                                                //  0 = The timekeeping oscillator is disabled.
                                                                                //  1 = The timekeeping oscillator is enabled. Once written, OON cannot be cleared except by power-on or hardware reset.
                                                                                //      If CRI is set, OON is set out of power-on or hardware reset and cannot be cleared.
                                                                                //
            [BitFieldRegister(Position=0       )] public bool          OOK;     // Timekeeping (32.768 kHz) Oscillator OK
                                                                                //
                                                                                //  0 = The timekeeping oscillator is disabled or not stable, and clocks (divided by 112) the RTC and power manager.
                                                                                //  1 = The timekeeping oscillator has been enabled (OON = 1) and stabilized; it clocks the RTC and power manager.
                                                                                //      If CRI is set, OOK is set out of power-on or hardware reset.
                                                                                //
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct CCSR_bitfield
        {
            [BitFieldRegister(Position=31      )] public bool CPDIS_S; // Core PLL Output Disable (CPDIS) Status
                                                                       //
                                                                       // Updated from CCCR at frequency change and standby, sleep, or deepsleep exit.
                                                                       //
            [BitFieldRegister(Position=30      )] public bool PPDIS_S; // Peripheral PLL Output Disable (PPDIS) Status
                                                                       //
                                                                       // Updated from CCCR at frequency change and standby, sleep, or deepsleep exit.
                                                                       //
            [BitFieldRegister(Position=29      )] public bool CPLCK;   // Core PLL Lock
                                                                       //
                                                                       //  0 = Core PLL is not locked. Using it as a clock source incurs a time penalty to make the switch while it locks.
                                                                       //  1 = Core PLL is locked and ready to use.
                                                                       //
            [BitFieldRegister(Position=28      )] public bool PPLCK;   // Peripheral PLL Lock
                                                                       //
                                                                       //  0 = Peripheral PLL is not locked. Using it as a clock source incurs a time penalty to make the switch while it locks.
                                                                       //  1 = Peripheral PLL is locked and ready to use.
                                                                       //
            [BitFieldRegister(Position=7,Size=3)] public uint N_S;     // Turbo-Mode-to-Run-Mode Ratio (N) Status
                                                                       //
                                                                       // Updated from CCCR at frequency change and standby, sleep, or deepsleep exit. (Reset value = 0b010 for N = 1)
                                                                       //
                                                                       // NOTE: The value in this field reflects twice the value of N.
                                                                       //
            [BitFieldRegister(Position=0,Size=5)] public uint L_S;     // Run-Mode to 13-MHz Processor Oscillator Ratio (L) Status
                                                                       //
                                                                       // Updated from CCCR at frequency change and standby, sleep, or deepsleep exit. (Reset value = 0b0_0111 for L = 7)
                                                                       //
        }

        [Register(Offset=0x00)] public CCCR_bitfield CCCR; // Core Clock Configuration register                                             | 3-95
        [Register(Offset=0x04)] public CKEN_bitfield CKEN; // Clock Enable register                                                         | 3-98
        [Register(Offset=0x08)] public OSCC_bitfield OSCC; // Oscillator Configuration register                                             | 3-99
        [Register(Offset=0x0C)] public CCSR_bitfield CCSR; // Core Clock Status register                                                    | 3-101


        public void InitializeClocks()
        {
            //
            // Turn off all the clock,except internal memory, let each unit to turn on at its code
            //
            this.CKEN = new CKEN_bitfield
            {
                EnInternalMemory   = true,
                EnMemoryController = true,
                EnPMI2C            = true, 
            };

            // enable TOUT for debugging.
            this.OSCC.TOUT_EN = true;

            //
            // Run freq = 208MHz, turbo = 416MHz, core PLLs must on, CLK_MEM = 208MHz
            //
            this.CCCR = new CCCR_bitfield
            {
                A = true,
                N = 4,
                L = 16,
            };

            // Start frequency change + enable Turbo mode (416 MHz clock)
            Processor.MoveToCoprocessor  ( 14, 0, 6, 0, 0, 3 );
////        CPU_CPWAIT();
            Processor.MoveFromCoprocessor( 15, 0, 2, 0, 0    );

            //
            // Wait until clk stable
            //
            while(this.CCSR.CPLCK == false)
            {
            }

            while(this.CCSR.PPLCK == false)
            {
            }
        }

        //
        // Access Methods
        //

        public static extern ClockManager Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}