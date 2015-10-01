//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.LPC3180
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x40034000U,Length=0x20U)]
    public class MilliSecondTimer
    {
        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MSTIM_INT_bitfield
        {
            [BitFieldRegister(Position=1)] public bool MATCH1_INT; // Reading a 1 indicates an active MATCH 1 interrupt.
                                                                   // Writing a 1 clears the active interrupt status. Writing 0 has no effect. 
                                                                   // Note: Remove active match status by writing a new match value before clearing the interrupt.
                                                                   // Otherwise a new match interrupt may be activated immediately after clearing the match interrupt since the match may still be valid.
                                                                   //
            [BitFieldRegister(Position=0)] public bool MATCH0_INT; // Reading a 1 indicates an active MATCH 0 interrupt.
                                                                   // Writing a 1 clears the active interrupt status. Writing 0 has no effect. 
                                                                   // Note: Remove active match status by writing a new match value before clearing the interrupt.
                                                                   // Otherwise a new match interrupt may be activated immediately after clearing the match interrupt since the match may still be valid.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MSTIM_CTRL_bitfield
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
        public struct MSTIM_MCTRL_bitfield
        {
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

        [Register(Offset=0x00000000)] public MSTIM_INT_bitfield   MSTIM_INT;     // Millisecond timer interrupt status register 0 R/W
        [Register(Offset=0x00000004)] public MSTIM_CTRL_bitfield  MSTIM_CTRL;    // Millisecond timer control register 0 R/W
        [Register(Offset=0x00000008)] public uint                 MSTIM_COUNTER; // Millisecond timer counter value register 0 R/W
        [Register(Offset=0x00000014)] public MSTIM_MCTRL_bitfield MSTIM_MCTRL;   // Millisecond timer match control register 0 R/W
        [Register(Offset=0x00000018)] public uint                 MSTIM_MATCH0;  // Millisecond timer match 0 register 0 R/W
        [Register(Offset=0x0000001C)] public uint                 MSTIM_MATCH1;  // Millisecond timer match 1 register 0 R/W

        //
        // Access Methods
        //

        public static extern MilliSecondTimer Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}