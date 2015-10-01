//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.LPC3180
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x40038000U,Length=0x34U)]
    public class HighSpeedTimer
    {
////    0x4003 8028 HSTIM_CCR High Speed timer capture control register 0 R/W
////    0x4003 802C HSTIM_CR0 High Speed timer capture 0 register 0 RO
////    0x4003 8030 HSTIM_CR1 High Speed timer capture 1 register 0 RO

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct HSTIM_INT_bitfield
        {
            [BitFieldRegister(Position=5)] public bool RTC_TICK;   // Reading a 1 indicates an active RTC tick capture status. Write 1 to clear this status.
                                                                   // Note that this status can not generate an ARM interrupt.
                                                                   // The pins can however generate PIO interrupts directly.
                                                                   //
            [BitFieldRegister(Position=4)] public bool GPI_06;     // Reading a 1 indicates an active GPI_06 status. Write 1 to clear this status.
                                                                   // Note that this status can not generate an ARM interrupt.
                                                                   // The pins can however generate PIO interrupts directly.
                                                                   //
            [BitFieldRegister(Position=2)] public bool MATCH2_INT; // Reading a 1 indicates an active MATCH 2 interrupt.
                                                                   // 
                                                                   // Writing a 1 clears the active interrupt status. Writing 0 has no effect.
                                                                   // Note: Remove active match status by writing a new match value before clearing the interrupt.
                                                                   // Otherwise this a new match interrupt may be activated immediately after clearing the match interrupt since the match may still be valid.
                                                                   // 
            [BitFieldRegister(Position=1)] public bool MATCH1_INT; // Reading a 1 indicates an active MATCH 1 interrupt.
                                                                   // 
                                                                   // Writing a 1 clears the active interrupt status. Writing 0 has no effect.
                                                                   // Note: Remove active match status by writing a new match value before clearing the interrupt.
                                                                   // Otherwise this a new match interrupt may be activated immediately after clearing the match interrupt since the match may still be valid.
                                                                   //
            [BitFieldRegister(Position=0)] public bool MATCH0_INT; // Reading a 1 indicates an active MATCH 0 interrupt.
                                                                   // 
                                                                   // Writing a 1 clears the active interrupt status. Writing 0 has no effect.
                                                                   // Note: Remove active match status by writing a new match value before clearing the interrupt.
                                                                   // Otherwise this a new match interrupt may be activated immediately after clearing the match interrupt since the match may still be valid.
                                                                   //
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct HSTIM_CTRL_bitfield
        {
            [BitFieldRegister(Position=2)] public bool PAUSE_EN;    // *0 = Timer counter continues to run if ARM enters debug mode.
                                                                    //  1 = Timer counter is stopped when the core is in debug mode (DBGACK high).
                                                                    //
            [BitFieldRegister(Position=1)] public bool RESET_COUNT; // *0 = Timer counter is not reset.
                                                                    //  1 = Timer counter will be reset on next PERIPH_CLK edge. Software must write this bit back to low to release the reset.
                                                                    //
            [BitFieldRegister(Position=0)] public bool COUNT_ENAB;  // *0 = Timer Counter is stopped.
                                                                    //  1 = Timer Counter is enabled.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct HSTIM_MCTRL_bitfield
        {
            [BitFieldRegister(Position=8)] public bool STOP_COUNT2;  // *0 = Disable the stop functionality on Match 2.
                                                                     //  1 = Enable the Timer Counter to be stopped on Match 2.
                                                                     //
            [BitFieldRegister(Position=7)] public bool RESET_COUNT2; // *0 = Disable reset of Timer Counter on Match 2.
                                                                     //  1 = Enable reset of Timer Counter on Match 2.
                                                                     //
            [BitFieldRegister(Position=6)] public bool MR2_INT;      // *0 = Disable interrupt on the Match 2 register.
                                                                     //  1 = Enable internal interrupt status generation on the Match 2 register.
                                                                     //
            [BitFieldRegister(Position=5)] public bool STOP_COUNT1;  // *0 = Disable the stop functionality on Match 1.
                                                                     //  1 = Enable the Timer Counter to be stopped on Match 1.
                                                                     //
            [BitFieldRegister(Position=4)] public bool RESET_COUNT1; // *0 = Disable reset of Timer Counter on Match 1.
                                                                     //  1 = Enable reset of Timer Counter on Match 1.
                                                                     //
            [BitFieldRegister(Position=3)] public bool MR1_INT;      // *0 = Disable interrupt on the Match 1 register.
                                                                     //  1 = Enable internal interrupt status generation on the Match 1 register.
                                                                     //
            [BitFieldRegister(Position=2)] public bool STOP_COUNT0;  // *0 = Disable the stop functionality on Match 0.
                                                                     //  1 = Enable the Timer Counter to be stopped on Match 0.
                                                                     //
            [BitFieldRegister(Position=1)] public bool RESET_COUNT0; // *0 = Disable reset of Timer Counter on Match 0.
                                                                     //  1 = Enable reset of Timer Counter on Match 0.
                                                                     //
            [BitFieldRegister(Position=0)] public bool MR0_INT;      // *0 = Disable interrupt on the Match 0 register.
                                                                     //  1 = Enable internal interrupt status generation on the Match 0 register.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct HSTIM_CCR_bitfield
        {
            [BitFieldRegister(Position=5)] public bool RTC_TICK_EVENT; // If this bit is set, an interrupt status will be set in the HSTIM_INT register on a capture.
                                                                       //
            [BitFieldRegister(Position=4)] public bool RTC_TICK_FALL;  // If this bit is set, the timer counter will be loaded into the CR1 if a falling edge is detected on RTC_TICK.
                                                                       //
            [BitFieldRegister(Position=3)] public bool RTC_TICK_RISE;  // If this bit is set, the timer counter will be loaded into the CR1 if a rising edge is detected on RTC_TICK.
                                                                       // 
            [BitFieldRegister(Position=2)] public bool GPI_06_EVENT;   // If this bit is set, an interrupt status will be set in the HSTIM_INT register on a capture.
                                                                       //
            [BitFieldRegister(Position=1)] public bool GPI_06_FALL;    // If this bit is set, the timer counter will be loaded into the CR0 if a falling edge is detected on GPI_06.
                                                                       //
            [BitFieldRegister(Position=0)] public bool GPI_06_RISE;    // If this bit is set, the timer counter will be loaded into the CR0 if a rising edge is detected on GPI_06
                                                                       // 
        }

        //--//

        [Register(Offset=0x00000000)] public HSTIM_INT_bitfield   HSTIM_INT;     // High Speed timer interrupt status register 0 R/W
        [Register(Offset=0x00000004)] public HSTIM_CTRL_bitfield  HSTIM_CTRL;    // High Speed timer control register 0 R/W
        [Register(Offset=0x00000008)] public uint                 HSTIM_COUNTER; // High Speed timer counter value register 0 R/W
        [Register(Offset=0x0000000C)] public uint                 HSTIM_PMATCH;  // High Speed timer prescale counter match register 0 R/W
        [Register(Offset=0x00000010)] public uint                 HSTIM_PCOUNT;  // High Speed Timer prescale counter value register 0 R/W
        [Register(Offset=0x00000014)] public HSTIM_MCTRL_bitfield HSTIM_MCTRL;   // High Speed timer match control register 0 R/W
        [Register(Offset=0x00000018)] public uint                 HSTIM_MATCH0;  // High Speed timer match 0 register 0 R/W
        [Register(Offset=0x0000001C)] public uint                 HSTIM_MATCH1;  // High Speed timer match 1 register 0 R/W
        [Register(Offset=0x00000020)] public uint                 HSTIM_MATCH2;  // High Speed timer match 2 register 0 R/W
        [Register(Offset=0x00000028)] public HSTIM_CCR_bitfield   HSTIM_CCR;     // High Speed timer capture control register 0 R/W
        [Register(Offset=0x0000002C)] public uint                 HSTIM_CR0;     // High Speed timer capture 0 register 0 RO
        [Register(Offset=0x00000030)] public uint                 HSTIM_CR1;     // High Speed timer capture 1 register 0 RO

        //
        // Access Methods
        //

        public static extern HighSpeedTimer Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}