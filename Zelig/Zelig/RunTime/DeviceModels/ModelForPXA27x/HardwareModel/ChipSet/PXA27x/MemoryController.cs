//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x48000000U,Length=0x00000068U)]
    public class MemoryController
    {
        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MDCNFG_bitfield
        {
            public enum Timing : uint
            {
                Setting0 = 0, // tRP = 2 clks, CL = 2, tRCD = 1 clks, tRASMIN = 3 clks, tRC = 4 clks
                Setting1 = 1, // tRP = 2 clks, CL = 2, tRCD = 2 clks, tRASMIN = 5 clks, tRC = 8 clks
                Setting2 = 2, // tRP = 3 clks, CL = 3, tRCD = 3 clks, tRASMIN = 7 clks, tRC = 10 clks
                Setting3 = 3, // tRP = 3 clks, CL = 3, tRCD = 3 clks, tRASMIN = 7 clks, tRC = 11 clks
            }

            public enum RowAddress : uint
            {
                Use11Bits = 0, // 0b00 = 11 row address bits
                Use12Bits = 1, // 0b01 = 12 row address bits
                Use13Bits = 2, // 0b10 = 13 row address bits
                               // 0b11 = reserved
            }

            public enum ColumnAddress : uint
            {
                Use8Bits  = 0, // 0b000 = 8 column address bits
                Use9Bits  = 1, // 0b001 = 9 column address bits
                Use10Bits = 2, // 0b010 = 10 column address bits
                Use11Bits = 3, // 0b011 = 11 column address bits
                Use12Bits = 4, // 0b100 = 12 column address bits
            }

            public enum Stack : uint
            {
                None          , // 0b00 = SDRAM address is placed on MA<24:10>. See Table 6-3, Table 6-5 and Table 6-7.
                SDRAMplusFLASH, // 0b01 = SDRAM address is placed on MA<24:23,13:1>. Use when flash internal to the PXA271 processor has 16-bit total data bus width on partition 0. See Table 6-4 and Table 6-6.
                                // 0b10 = reserved
                                // 0b11 = reserved
            }

            [BitFieldRegister     (Position=31                )] public bool          MDENX;     // SDRAM 1 GB Memory Map Enable
                                                                                                 // 
                                                                                                 // 0 = Use normal 256-Mbyte memory map.
                                                                                                 // 1 = Use large 1-Gbyte memory map.
                                                                                                 // 
            [BitFieldRegister     (Position=28                )] public bool          DSA1110_2; // SA-1110 Addressing Mode Compatibility
                                                                                                 //
                                                                                                 // Use SA-1110 addressing multiplexing mode for pair 2/3. Setting this bit
                                                                                                 // overrides the addressing bit programmed in MDCNFG[DADDR2]
                                                                                                 //
                                                                                                 // For an explanation of how the address is driven onto the address bus in SA-
                                                                                                 // 1110 addressing mode, see Table 6-7.
                                                                                                 //
                                                                                                 //   0 = Use addressing mode specified in MDCNFG[DADDR2].
                                                                                                 //   1 = Override the addressing bit programmed in MDCNFG[DADDR2] and use SA-1110 addressing mode.
                                                                                                 // 
            [BitFieldRegister     (Position=27,WritesAs=1     )] public bool          SET2;      // Set Reserved Bit
                                                                                                 // Always set this bit
                                                                                                 // 
            [BitFieldRegister     (Position=26                )] public bool          DADDR2;    // Alternate Addressing Mode
                                                                                                 // 
                                                                                                 // Ignored if MDCNFG[DSA1110_2] is set.
                                                                                                 // For an explanation of how the address is driven onto the address bus for the partition pair 2/3 in alternate addressing mode, see Figure 6-3.
                                                                                                 // 
                                                                                                 //   0 = Use normal addressing mode
                                                                                                 //   1 = Use alternate addressing mode
                                                                                                 //
            [BitFieldRegister     (Position=24,Size=2         )] public Timing        DTC2;      // The AC timing parameters for SDRAM partition pair 2/3 represent the
                                                                                                 // number of SDCLKs (not memory clocks) for the clocks indicated below.
                                                                                                 // See the SDRAM data sheet to determine the optimal timings.
                                                                                                 // See the Intel PXA27x Processor Family EMTS for SDRAM timing diagrams and the effects on AC timings.
                                                                                                 // For all timing categories, write recovery time (tWR) is hard-coded at 2 clocks.
                                                                                                 //
                                                                                                 // NOTE: The CAS latency programmed here is set to 3 if the CAS latency
                                                                                                 // for SDRAM partition pair 0 (MDCNFG[DTC0] CAS latency) is
                                                                                                 // programmed to 3. Otherwise, it is set as programmed here. What
                                                                                                 // this means is that the CAS latency for the two SDRAM partition
                                                                                                 // pairs is the same and equals the greater of the two programmed
                                                                                                 // values. If either partition pair is programmed to CAS latency of 3,
                                                                                                 // then both partition pairs have CAS Latency of 3; otherwise, both
                                                                                                 // partition pairs have CAS latency of 2.
                                                                                                 //
            [BitFieldRegister     (Position=23                )] public bool          DNB2;      // Number of Banks in Partition Pair 2/3
                                                                                                 //
                                                                                                 //   0 = 2 internal SDRAM banks
                                                                                                 //   1 = 4 internal SDRAM banks
                                                                                                 // 
            [BitFieldRegister     (Position=21,Size=2         )] public RowAddress    DRAC2;     // SDRAM Row Address Bit Count for Partition Pair 2/3
                                                                                                 // 
            [BitFieldSplitRegister(Position=30,Size=1,Offset=2)]                                 //
            [BitFieldSplitRegister(Position=19,Size=2,Offset=0)] public ColumnAddress DCAC2;     // SDRAM Column Address Bits for Partition Pair 2/3
                                                                                                 // 
            [BitFieldRegister     (Position=18                )] public bool          DWID2;     // SDRAM Data Bus Width for Partition Pair 2/3
                                                                                                 // 
                                                                                                 //   0 = 32 bits
                                                                                                 //   1 = 16 bits
                                                                                                 // 
            [BitFieldRegister     (Position=17                )] public bool          DE3;       // SDRAM Enable for Partition 3
                                                                                                 //
                                                                                                 // A single (non-burst) 32-bit (or 16-bit if MDCNFG[DWIDx] = 1) access (read or write) to a disabled SDRAM partition triggers a CBR refresh cycle to all
                                                                                                 // partitions. When all partitions are disabled, the refresh counter is disabled.
                                                                                                 //
                                                                                                 //   0 = SDRAM partition disabled
                                                                                                 //   1 = SDRAM partition enabled
                                                                                                 //
            [BitFieldRegister     (Position=16                )] public bool          DE2;       // SDRAM Enable for Partition 2
                                                                                                 //
                                                                                                 // A single (non-burst) 32-bit (or 16-bit if MDCNFG[DWIDx] = 1) access (read or write) to a disabled SDRAM partition triggers a CBR refresh cycle to all
                                                                                                 // partitions. When all partitions are disabled, the refresh counter is disabled.
                                                                                                 //
                                                                                                 //   0 = SDRAM partition disabled
                                                                                                 //   1 = SDRAM partition enabled
                                                                                                 //
            [BitFieldSplitRegister(Position=15,Size=1,Offset=1)]                                 // 
            [BitFieldSplitRegister(Position=13,Size=1,Offset=0)] public Stack         STACK;     // The 2-bit STACK field consists of MDCNFG[STACK1] (MSB) and MDCNFG[STACK0] (LSB) to determine the SDRAM address-multiplexing scheme.
                                                                                                 // This 2-bit setting alters the SDRAM multiplexing scheme for all SDRAM partitions as shown below.
                                                                                                 //
                                                                                                 // NOTE: When MDCNFG[STACKx] equals either 0b01 or 0b10, fly-by DMA transfers are not supported,
                                                                                                 // and SA1110 addressing mode is not supported, DASA1110_0 must be cleared.
                                                                                                 // 
            [BitFieldRegister     (Position=12                )] public bool          DSA1110_0; // SA-1110 Addressing Mode Compatibility
                                                                                                 // 
                                                                                                 // Use SA-1110 addressing multiplexing mode for pair 0/1. Setting this bit overrides the addressing bit programmed in MDCNFG[DADDR0].
                                                                                                 // For an explanation of how the address is driven onto the address bus in SA-1110 addressing mode, see Table 6-7.
                                                                                                 // 
                                                                                                 //   0 = Use addressing mode specified in MDCNFG[DADDR0]
                                                                                                 //   1 = Use SA-1110 addressing mode
                                                                                                 // 
            [BitFieldRegister     (Position=11,WritesAs=1     )] public bool          SET0;      // Set Reserved Bit
                                                                                                 // Always set this bit
                                                                                                 // 
            [BitFieldRegister     (Position=10                )] public bool          DADDR0;    // Alternate Addressing Mode
                                                                                                 // 
                                                                                                 // Ignored if MDCNFG[DSA1110_0] is set.
                                                                                                 // For an explanation of how the address is driven onto the address bus for the partition pair 0/1 in alternate addressing mode, see Figure 6-3.
                                                                                                 // 
                                                                                                 //   0 = Use normal addressing mode
                                                                                                 //   1 = Use alternate addressing mode
                                                                                                 //
            [BitFieldRegister     (Position= 8,Size=2         )] public Timing        DTC0;      // The AC timing parameters for SDRAM partition pair 0/1 represent the
                                                                                                 // number of SDCLKs (not memory clocks) for the clocks indicated below.
                                                                                                 // See the SDRAM data sheet to determine the optimal timings.
                                                                                                 // See the Intel PXA27x Processor Family EMTS for SDRAM timing diagrams and the effects on AC timings.
                                                                                                 // For all timing categories, write recovery time (tWR) is hard-coded at 2 clocks.
                                                                                                 //
                                                                                                 // NOTE: The CAS latency programmed here is set to 3 if the CAS latency
                                                                                                 // for SDRAM partition pair 2 (MDCNFG[DTC2] CAS latency) is
                                                                                                 // programmed to 3. Otherwise, it is set as programmed here. What
                                                                                                 // this means is that the CAS latency for the two SDRAM partition
                                                                                                 // pairs is the same and equals the greater of the two programmed
                                                                                                 // values. If either partition pair is programmed to CAS latency of 3,
                                                                                                 // then both partition pairs have CAS Latency of 3; otherwise, both
                                                                                                 // partition pairs have CAS latency of 2.
                                                                                                 //
            [BitFieldRegister     (Position= 7                )] public bool          DNB0;      // Number of Banks in Partition Pair 0/1
                                                                                                 //
                                                                                                 //   0 = 2 internal SDRAM banks
                                                                                                 //   1 = 4 internal SDRAM banks
                                                                                                 // 
            [BitFieldRegister     (Position= 5,Size=2         )] public RowAddress    DRAC0;     // SDRAM Row Address Bit Count for Partition Pair 0/1
                                                                                                 // 
            [BitFieldSplitRegister(Position=14,Size=1,Offset=2)]                                 //
            [BitFieldSplitRegister(Position= 3,Size=2,Offset=0)] public ColumnAddress DCAC0;     // SDRAM Column Address Bits for Partition Pair 0/1
                                                                                                 // 
            [BitFieldRegister     (Position= 2                )] public bool          DWID0;     // SDRAM Data Bus Width for Partition Pair 0/1
                                                                                                 // 
                                                                                                 //   0 = 32 bits
                                                                                                 //   1 = 16 bits
                                                                                                 //
            [BitFieldRegister     (Position= 1                )] public bool          DE1;       // SDRAM Enable for Partition 1
                                                                                                 //
                                                                                                 // A single (non-burst) 32-bit (or 16-bit if MDCNFG[DWIDx] = 1) access (read or write) to a disabled SDRAM partition triggers a CBR refresh cycle to all
                                                                                                 // partitions. When all partitions are disabled, the refresh counter is disabled.
                                                                                                 //
                                                                                                 //   0 = SDRAM partition disabled
                                                                                                 //   1 = SDRAM partition enabled
                                                                                                 //
            [BitFieldRegister     (Position= 0                )] public bool          DE0;       // SDRAM Enable for Partition 0
                                                                                                 //
                                                                                                 // A single (non-burst) 32-bit (or 16-bit if MDCNFG[DWIDx] = 1) access (read or write) to a disabled SDRAM partition triggers a CBR refresh cycle to all
                                                                                                 // partitions. When all partitions are disabled, the refresh counter is disabled.
                                                                                                 //
                                                                                                 //   0 = SDRAM partition disabled
                                                                                                 //   1 = SDRAM partition enabled
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MDMRS_bitfield
        {
            [BitFieldRegister(Position=23,Size=8)] public uint MDMRS2; // The SDRAM MRS bits represent the value driven onto the SDRAM address
                                                                       // bus from MA<24:17> (or MA<24:23,14:9> or MA<24:23,13,8>, depending
                                                                       // on MDCNFG[STACKx] setting) during the MRS command for partition pair 2/3.
                                                                       // 
            [BitFieldRegister(Position=20,Size=3)] public uint MDCL2;  // SDRAM MRS bits representing CAS Latency
                                                                       // SDRAM partition pair 2/3 CAS latency is derived from MDCNFG[DTC2].
                                                                       // Writes are ignored. This bit field represents the value driven onto
                                                                       // MA<16:14> (or MA<8:6> or MA<7:5>, depending on MDCNFG[STACKx]
                                                                       // setting) during the MRS command.
                                                                       // 
            [BitFieldRegister(Position=19       )] public bool MDADD2; // SDRAM MRS bit representing Burst Type
                                                                       // SDRAM partition pair 2/3 burst type. Fix to sequential addressing. This bit
                                                                       // field represents the value driven onto MA<13> (or MA<5> or MA<4>,
                                                                       // depending on MDCNFG[STACKx] setting) during the MRS command. This
                                                                       // field is always zero.
                                                                       // 
            [BitFieldRegister(Position=16,Size=3)] public uint MDBL2;  // SDRAM MRS bits representing Burst Length
                                                                       // SDRAM partition pair 2/3 burst length. Fixed to a burst length of four. This
                                                                       // bit field represents the value driven onto the MA<12:10> (or MA<4:2> or
                                                                       // MA<3:1>, depending on MDCNFG[STACKx] setting) during the MRS
                                                                       // command. This field is read as 0b010.
                                                                       // 
            [BitFieldRegister(Position= 7,Size=8)] public uint MDMRS0; // The SDRAM MRS bits represent the value driven onto the SDRAM address
                                                                       // bus from MA<24:17> (or MA<24:23,14:9> or MA<24:23,13,8>, depending
                                                                       // on MDCNFG[STACKx] setting) during the MRS command for partition pair 0/1.
                                                                       // 
            [BitFieldRegister(Position= 4,Size=3)] public uint MDCL0;  // SDRAM MRS bits representing CAS Latency
                                                                       // SDRAM partition pair 0/1 CAS latency is derived from MDCNFG[DTC2].
                                                                       // Writes are ignored. This bit field represents the value driven onto
                                                                       // MA<16:14> (or MA<8:6> or MA<7:5>, depending on MDCNFG[STACKx]
                                                                       // setting) during the MRS command.
                                                                       // 
            [BitFieldRegister(Position= 3       )] public bool MDADD0; // SDRAM MRS bit representing Burst Type
                                                                       // SDRAM partition pair 0/1 burst type. Fix to sequential addressing. This bit
                                                                       // field represents the value driven onto MA<13> (or MA<5> or MA<4>,
                                                                       // depending on MDCNFG[STACKx] setting) during the MRS command. This
                                                                       // field is always zero.
                                                                       // 
            [BitFieldRegister(Position= 0,Size=3)] public uint MDBL0;  // SDRAM MRS bits representing Burst Length
                                                                       // SDRAM partition pair 0/1 burst length. Fixed to a burst length of four. This
                                                                       // bit field represents the value driven onto the MA<12:10> (or MA<4:2> or
                                                                       // MA<3:1>, depending on MDCNFG[STACKx] setting) during the MRS
                                                                       // command. This field is read as 0b010.
                                                                       // 
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MDMRSLP_bitfield
        {
            [BitFieldRegister(Position=31        )] public bool MDLPEN2;  // Low-Power Enable for Partition Pair 2/3
                                                                          // Enable bit for low-power MRS value for partition pair 2/3.
                                                                          // 
            [BitFieldRegister(Position=23,Size=15)] public uint MDMRSLP2; // Low-Power MRS Value for Partition Pair 2/3
                                                                          // Low-power MRS value to be written to SDRAM for partition pair 2/3.
                                                                          // 
            [BitFieldRegister(Position=15        )] public bool MDLPEN0;  // Low-Power Enable for Partition Pair 0/1
                                                                          // Enable bit for low-power MRS value for partition pair 0/1.
                                                                          // 
            [BitFieldRegister(Position= 0,Size=15)] public uint MDMRSLP0; // Low-Power MRS Value for Partition Pair 0/1
                                                                          // Low-power MRS value to be written to SDRAM for partition pair 0/1.
                                                                          // 
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MDREFR_bitfield
        {
            [BitFieldRegister(Position=31        )] public bool ALTREFA;  // Exiting Alternate Bus Master Mode Refresh Control
                                                                          // 
                                                                          //   0 = SDRAM refresh is always performed after exiting alternate bus master mode.
                                                                          //   1 = SDRAM refresh is not performed after exiting alternate bus master
                                                                          //       mode unless the SDRAM refresh counter has timed out.
                                                                          //       MDREFR[ALTREFB] must be clear. The alternate master must
                                                                          //       perform a PALL before releasing the external bus. See
                                                                          //       Section 6.4.6.2 for more information.
                                                                          // 
            [BitFieldRegister(Position=30        )] public bool ALTREFB;  // Entering Alternate Bus Master Mode Refresh Control
                                                                          // 
                                                                          //   0 = SDRAM refresh is always performed after entering alternate bus master mode.
                                                                          //   1 = SDRAM refresh is not performed after entering alternate bus master
                                                                          //       mode unless the SDRAM refresh counter has timed out.
                                                                          //       MDREFR[ALTREFA] must be clear. The alternate master must first
                                                                          //       perform a PALL after receiving the bus grant.
                                                                          //       See Section 6.4.6 for more information.
                                                                          // 
            [BitFieldRegister(Position=29        )] public bool K0DB4;    // Synchronous Static Memory Clock Pin 0 (SDCLK<0> and SDCLK<3>) Divide-by-4 Control/Status
                                                                          // 
                                                                          // When set, SDCLK<0> and SDCLK<3> runs at one-fourth of the memory clock frequency, regardless of the value of MDREFR[K0DB2].
                                                                          // When clear, the SDCLK<0> and SDCLK<3> speed depends on the value programmed in MDREFR[K0DB2].
                                                                          // 
                                                                          //   0 = Use the value programmed in MDREFR[K0DB2] to generate SDCLK<0> and SDCLK<3>.
                                                                          //   1 = Divide CLK_MEM by four to generate SDCLK<0> and SDCLK<3>. K0DB2 is ignored.
                                                                          // 
            [BitFieldRegister(Position=25        )] public bool K2FREE;   // SDCLK<2> Free-Running
                                                                          // 
                                                                          // When set, it forces SDCLK<2> to be free-running, regardless of the value of the MDREFR[APD] or MDREFR[K2RUN] bits.
                                                                          // The bit resets to 1 so the clock is driven initially, which provides synchronous memory with a clock for
                                                                          // resetting any internal circuitry.
                                                                          // To disable free-running, clear this bit.
                                                                          // 
                                                                          //   0 = SDCLK<2> Dependent on MDREFR[K2RUN]
                                                                          //   1 = SDCLK<2> Free-running enabled
                                                                          // 
            [BitFieldRegister(Position=24        )] public bool K1FREE;   // SDCLK<1> Free-Running
                                                                          // 
                                                                          // When set, it forces SDCLK<1> to be free-running, regardless of the value of the MDREFR[APD] or MDREFR[K1RUN] bits.
                                                                          // This bit resets to 1 so the clock is driven initially. This provides synchronous memory with a clock for
                                                                          // resetting any internal circuitry.
                                                                          // To disable free-running, clear this bit.
                                                                          // 
                                                                          //   0 = SDCLK<1> Dependent on MDREFR[K1RUN]
                                                                          //   1 = SDCLK<1> Free-running enabled
                                                                          // 
            [BitFieldRegister(Position=23        )] public bool K0FREE;   // SDCLK<0> and SDCLK<3> Free-Running
                                                                          // 
                                                                          // When set, forces SDCLK<0> and SDCLK<3> to be free-running, regardless of the value of the MDREFR[APD] or MDREFR[K0RUN] bits.
                                                                          // This bit resets to 1 so the clock is driven initially. This provides synchronous memory with a
                                                                          // clock for resetting any internal circuitry.
                                                                          // To disable free-running, clear this bit.
                                                                          // 
                                                                          //   0 = SDCLK<0> and SDCLK<3> dependent on MDREFR[K0RUN]
                                                                          //   1 = SDCLK<0> and SDCLK<3> free-running enabled
                                                                          // 
            [BitFieldRegister(Position=22        )] public bool SLFRSH;   // SDRAM Self-Refresh Control/Status
                                                                          // 
                                                                          // Control/status bit for entering and exiting SDRAM self-refresh. It is
                                                                          // automatically set upon a hardware or sleep-exit reset.
                                                                          // 
                                                                          // Setting the SLFRSH bit forces a self-refresh command. E1PIN does not
                                                                          // have to be cleared. The appropriate clock run bits (K1RUN and/or K2RUN)
                                                                          // must remain set until SDRAM has entered self-refresh and must be set
                                                                          // before exiting self-refresh (clearing SLFRSH).
                                                                          // 
                                                                          // NOTE: This capability must be used with extreme caution because the
                                                                          //       resulting state prohibits automatic transitions for any commands.
                                                                          //       See Section 6.4.2.5.
                                                                          // 
                                                                          // Clearing SLFRSH is a part of the hardware or sleep-exit reset procedure for SDRAM. See Section 6.4.10.
                                                                          // 
                                                                          //   0 = Self-refresh disabled
                                                                          //   1 = Self-refresh enabled
                                                                          // 
            [BitFieldRegister(Position=20        )] public bool APD;      // SDRAM/Synchronous Static Memory Auto-Power-Down Enable
                                                                          // 
                                                                          // If APD is set and MDEFR[KxFREE] is cleared, the SDCKE signal and
                                                                          // SDCLK<2:0> clocks are driven low when none of the corresponding
                                                                          // partitions are being accessed.
                                                                          // 
                                                                          // • If no SDRAM partitions are being accessed, the SDRAM is put into
                                                                          //   power-down mode and the SDCKE signal and SDCLK<2:1> clocks are
                                                                          //   driven low.
                                                                          // • If one SDRAM partition pair is being used and the other pair is not, the
                                                                          //   SDCLKx clock to the partition pair that is not being used is driven low.
                                                                          // • If synchronous flash memory is installed and not being accessed, the
                                                                          //   SDCLK<0> and SDCLK<3> clocks corresponding to static partitions
                                                                          //   are driven low and the synchronous flash chips are put into powerdown
                                                                          //   mode. (see part datasheets). See Section 6.4.2.5 andSection 6.5.3
                                                                          // 
                                                                          //   0 = APD disabled
                                                                          //   1 = APD enabled
                                                                          // 
            [BitFieldRegister(Position=19        )] public bool K2DB2;    // SDRAM Clock Pin 2 (SDCLK<2>) Divide-by-2 Control/Status
                                                                          // 
                                                                          // When set, SDCLK<2> runs at one-half the CLK_MEM frequency.
                                                                          // When cleared, SDCLK<2> runs at the memory clock frequency.
                                                                          // This bit is automatically set on hardware or sleep-exit reset.
                                                                          // 
                                                                          //   0 = SDCLK<2> equals CLK_MEM
                                                                          //   1 = SDCLK<2> equals CLK_MEM divided by 2
                                                                          // 
            [BitFieldRegister(Position=18        )] public bool K2RUN;    // SDRAM Clock Pin 2 (SDCLK<2>) Run Control/Status
                                                                          // Automatically cleared on hardware or sleep-exit reset.
                                                                          // 
                                                                          // NOTE: Use extreme caution when clearing the K1RUN bit because the
                                                                          //       resulting state prohibits automatic transitions for any commands.
                                                                          //       See Section 6.4.2.5.
                                                                          // 
                                                                          // Setting K1RUN or K2RUN is a part of the hardware and sleep-exit reset procedure for SDRAM. See Section 6.4.10.
                                                                          // 
                                                                          // 0 = SDCLK<2> Dependent on MDREFR[K2FREE]
                                                                          // 1 = SDCLK<2> Enabled
                                                                          // 
            [BitFieldRegister(Position=17        )] public bool K1DB2;    // SDRAM Clock Pin 1 (SDCLK<1>) Divide-by-2 Control/Status
                                                                          // 
                                                                          // When set, SDCLK<1> runs at one-half the CLK_MEM frequency.
                                                                          // When cleared, SDCLK<1> runs at the memory clock frequency.
                                                                          // This bit is automatically set on hardware or sleep-exit reset.
                                                                          // 
                                                                          //   0 = SDCLK<1> equals CLK_MEM
                                                                          //   1 = SDCLK<1> equals CLK_MEM divided by 2
                                                                          // 
            [BitFieldRegister(Position=16        )] public bool K1RUN;    // SDRAM Clock Pin 1 (SDCLK<1>) Run Control/Status
                                                                          // Automatically cleared on hardware or sleep-exit reset.
                                                                          // 
                                                                          // NOTE: Use extreme caution when clearing the K1RUN bit because the
                                                                          //       resulting state prohibits automatic transitions for any commands.
                                                                          //       See Section 6.4.2.5.
                                                                          // 
                                                                          // Setting K1RUN or K2RUN is a part of the hardware and sleep-exit reset procedure for SDRAM. See Section 6.4.10.
                                                                          // 
                                                                          // 0 = SDCLK<1> Dependent on MDREFR[K1FREE]
                                                                          // 1 = SDCLK<1> Enabled
                                                                          // 
            [BitFieldRegister(Position=15        )] public bool E1PIN;    // SDRAM Clock Enable Pin 1 (SDCKE) Level Control/Status
                                                                          // Automatically cleared on hardware or sleep-exit reset.
                                                                          // 
                                                                          // NOTE: Extreme caution must be taken when clearing the E1PIN bit to
                                                                          //       cause a power-down command (if K1RUN = 1 and/or K2RUN = 1,
                                                                          //       and SLFRSH = 0) because the resulting state prohibits automatic
                                                                          //       transitions for mode register set, read, write, and refresh
                                                                          //       commands. E1PIN must be set to cause a power-down-exit
                                                                          //       command (if K1RUN = 1 and/or K2RUN = 1, and SLFRSH = 0). See Section 6.4.10.
                                                                          // 
                                                                          // Setting E1PIN is a part of the hardware reset or sleep-exit reset procedure for SDRAM. See Section 6.4.10.
                                                                          // 
                                                                          //   0 = SDCKE Disabled
                                                                          //   1 = SDCKE Enabled
                                                                          // 
            [BitFieldRegister(Position=14        )] public bool K0DB2;    // Synchronous Static Memory Clock Pin 0 (SDCLK<0> and SDCLK<3>) Divide-by-2 Control/Status
                                                                          // Second control/status bit for clock divisor of SDCLK<0>. The value
                                                                          // programmed here is only valid when MDREFR[K0DB4] is cleared.
                                                                          // When K0DB2 is set, SDCLK<0> and SDCLK<3> run at one-half the
                                                                          // memory clock frequency. When clear, SDCLK<0> and SDCLK<3> run at
                                                                          // the memory clock frequency. This bit is automatically set on hardware or sleep-exit reset.
                                                                          // 
                                                                          //   0 = SDCLK<0> and SDCLK<3> equals CLK_MEM
                                                                          //   1 = SDCLK<0> and SDCLK<3> equals CLK_MEM divided by two
                                                                          // 
            [BitFieldRegister(Position=13        )] public bool K0RUN;    // Synchronous Static Memory Clock Pin 0 (SDCLK<0> and SDCLK<3>) Run Control/Status
                                                                          // Set on hardware or sleep-exit reset if static memory partition 0 (boot space)
                                                                          // is configured for synchronous static memory (see Section 6.5.4).
                                                                          // Otherwise, it is cleared on reset.
                                                                          // Use extreme caution when clearing the K0RUN bit because the resulting
                                                                          // state prohibits automatic transitions for any commands. See Section 6.4.10.
                                                                          // 
                                                                          //   0 = SDCLK<0> and SDCLK<3> dependent on MDREFR[K0FREE]
                                                                          //   1 = SDCLK<0> and SDCLK<3> enabled
                                                                          // 
            [BitFieldRegister(Position= 0,Size=12)] public uint DRI;      // SDRAM Refresh Interval for All Partitions
                                                                          // 
                                                                          // The number of memory clock cycles (divided by 32) between auto refresh
                                                                          // (CBR) cycles. One row is refreshed in each SDRAM bank during each CBR
                                                                          // refresh cycle. This interval applies to all SDRAM in the four partitions.
                                                                          // To the refresh interval from this programmed number, multiply it by 32 and add 31.
                                                                          // 
                                                                          // The value that must be loaded into this register is calculated as follows:
                                                                          // • DRI = (Number of CLK_MEM cycles – 31) / 32 = (Refresh time / rows x memory clock frequency – 31) / 32.
                                                                          // 
                                                                          // This must be programmed to be shared by both partition pairs. Therefore, the worst case number must be programmed.
                                                                          // This number must be less than the tRASMAX for the SDRAM being accessed.
                                                                          // 
                                                                          // If all four SDRAM partitions are disabled, the refresh counter is disabled
                                                                          // and refreshes are only performed when a single transaction to a disabled SDRAM partition is requested.
                                                                          // 
                                                                          // If the clock frequency is changed, this register must be rewritten, even if the
                                                                          // value has not changed. This causes a refresh and resets the refresh counter to the refresh interval.
                                                                          // 
                                                                          // 0x000 = No refreshes are sent to the SDRAM. However, refresh cycles can occur if VLIO cycles are performed.
                                                                          // 
                                                                          // NOTE: See the refresh rules for programming limitations in Section 6.5.1.4
                                                                          // 
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct SXCNFG_bitfield
        {
            public enum CasLatency : uint
            {
                           // 0b0000 = reserved
                           // 0b0001 = reserved
                CL3  =  2, // 0b0010 = 3 clocks (frequency configuration code = 2)
                CL4  =  3, // 0b0011 = 4 clocks (frequency configuration code = 3)
                CL5  =  4, // 0b0100 = 5 clocks (frequency configuration code = 4)
                CL6  =  5, // 0b0101 = 6 clocks (frequency configuration code = 5)
                CL7  =  6, // 0b0110 = 7 clocks (frequency configuration code = 6)
                CL8  =  7, // 0b0111 = 8 clocks (frequency configuration code = 7)
                CL9  =  8, // 0b1000 = 9 clocks (frequency configuration code = 8)
                CL10 =  9, // 0b1001 = 10 clocks (frequency configuration code = 9)
                CL11 = 10, // 0b1010 = 11 clocks (frequency configuration code = 10)
                CL12 = 11, // 0b1011 = 12 clocks (frequency configuration code = 11)
                CL13 = 12, // 0b1100 = 13 clocks (frequency configuration code = 12)
                CL14 = 13, // 0b1101 = 14 clocks (frequency configuration code = 13)
                CL15 = 14, // 0b1110 = 15 clocks (frequency configuration code = 14)
                CL16 = 15, // 0b1111 = 16 clocks (frequency configuration code = 15)
            }

            public enum MemoryType : uint
            {
                               // 0b00 = reserved
                               // 0b01 = reserved
                BurstOf8  = 2, // 0b10 = Synchronous flash memory in burst-of-8 mode
                BurstOf16 = 3, // 0b11 = Synchronous flash memory in burst-of-16 mode
            }

            [BitFieldRegister(Position=30,WritesAs=1          )] public bool       Set2;      // reserved (set during writes and reads are undefined)
                                                                                              // 
            [BitFieldRegister(Position=28,Size=2              )] public MemoryType SXTP2;     // SX Memory Type for Partition Pair 2/3
                                                                                              // 
            [BitFieldSplitRegister(Position=31,Size=1,Offset=4)] 
            [BitFieldSplitRegister(Position=18,Size=3,Offset=0)] public CasLatency SXCL2;     // CAS Latency for SX Memory Partition Pair 2/3
                                                                                              // 
                                                                                              // SXCL2 is the number of SDCLK cycles between the time that the read
                                                                                              // command is received and the data is latched. The unit size for SXCL2 is the
                                                                                              // external SDCLK cycle; when SX memory is run at half the memory clock
                                                                                              // frequency (MDREFR[K0DB2] is set), the delay is two CLK_MEM cycles.
                                                                                              // 
                                                                                              // Use in conjunction with SXCNFG[SXCLEXT2] to achieve the following CAS
                                                                                              // latencies. The field SXCLEXT2 is added to the beginning of the SXCL2 field
                                                                                              // to create the 4 bit values shown below.
                                                                                              // 
                                                                                              // Any frequency configuration code that inserts wait states in the return of the
                                                                                              // data from flash memory is not valid. See the appropriate flash datasheets:
                                                                                              // 
                                                                                              // The first access latency count configuration tells the device how many
                                                                                              // clocks must elapse from ADV#-inactive (VIH) before the first data word must
                                                                                              // be driven onto its data pins. The input clock frequency determines this value.
                                                                                              // 
            [BitFieldRegister(Position=17                     )] public bool       SXEN3;     // SX Memory Partition 3 Enable
                                                                                              // 
                                                                                              //   0 = Partition 3 is not enabled as SX memory.
                                                                                              //   1 = Partition 3 is enabled as SX memory.
                                                                                              // 
            [BitFieldRegister(Position=16                     )] public bool       SXEN2;     // SX Memory Partition 2 Enable
                                                                                              // 
                                                                                              //   0 = Partition 2 is not enabled as SX memory.
                                                                                              //   1 = Partition 2 is enabled as SX memory.
                                                                                              // 
                                                                                              // 
            [BitFieldRegister(Position=14,WritesAs=1          )] public bool       Set0;      // reserved (set during writes and reads are undefined)
                                                                                              // 
            [BitFieldRegister(Position=12,Size=2              )] public MemoryType SXTP0;     // SX Memory Type for Partition Pair 2/3
                                                                                              // 
            [BitFieldSplitRegister(Position=15,Size=1,Offset=4)] 
            [BitFieldSplitRegister(Position= 2,Size=3,Offset=0)] public CasLatency SXCL0;     // CAS Latency for SX Memory Partition Pair 0/1
                                                                                              // 
                                                                                              // SXCL0 is the number of SDCLK cycles between the time that the read
                                                                                              // command is received and the data is latched. The unit size for SXCL0 is the
                                                                                              // external SDCLK cycle; when SX memory is run at half the memory clock
                                                                                              // frequency (MDREFR[K0DB0] is set), the delay is two CLK_MEM cycles.
                                                                                              // 
                                                                                              // Use in conjunction with SXCNFG[SXCLEXT0] to achieve the following CAS
                                                                                              // latencies. The field SXCLEXT0 is added to the beginning of the SXCL0 field
                                                                                              // to create the 4 bit values shown below.
                                                                                              // 
                                                                                              // Any frequency configuration code that inserts wait states in the return of the
                                                                                              // data from flash memory is not valid. See the appropriate flash datasheets:
                                                                                              // 
                                                                                              // The first access latency count configuration tells the device how many
                                                                                              // clocks must elapse from ADV#-inactive (VIH) before the first data word must
                                                                                              // be driven onto its data pins. The input clock frequency determines this value.
                                                                                              // 
            [BitFieldRegister(Position= 1                     )] public bool       SXEN1;     // SX Memory Partition 1 Enable
                                                                                              // 
                                                                                              //   0 = Partition 1 is not enabled as SX memory.
                                                                                              //   1 = Partition 1 is enabled as SX memory.
                                                                                              // 
            [BitFieldRegister(Position= 0                     )] public bool       SXEN0;     // SX Memory Partition 0 Enable
                                                                                              // 
                                                                                              //   0 = Partition 0 is not enabled as SX memory.
                                                                                              //   1 = Partition 0 is enabled as SX memory.
                                                                                              // 
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MSC_bitfield
        {
            public enum Delay : uint
            {
                Val0  =  0,
                Val1  =  1,
                Val2  =  2,
                Val3  =  3,
                Val4  =  4,
                Val5  =  5,
                Val6  =  6,
                Val7  =  7,
                Val8  =  8,
                Val9  =  9,
                Val10 = 10,
                Val11 = 11,
                Val15 = 12,
                Val20 = 13,
                Val26 = 14,
                Val30 = 15,
            }

            public enum MemType : uint
            {
                FlashOrRomNoBurst  = 0, // 0b000 = Synchronous flash or non-burst ROM or non-burst flash
                SRAM               = 1, // 0b001 = SRAM
                FlashOrRomBurstOf4 = 2, // 0b010 = Burst-of-four ROM or burst-of-four flash (with non-burst writes)
                FlashOrRomBurstOf8 = 3, // 0b011 = Burst-of-eight ROM or burst-of-eight flash (with non-burst writes)
                VariableLatencyIO  = 4, // 0b100 = Variable-latency I/O (VLIO)
                                        // 0b101 = reserved
                                        // 0b110 = reserved
                                        // 0b111 = reserved
            }

            [BitFieldRegister(Position=31        )] public bool RBUFF1;  // Return Data Buffer vs. Streaming Behavior
            [BitFieldRegister(Position=15        )] public bool RBUFF0;  //
                                                                         // 
                                                                         // When slow memory devices are used in the system (VLIO, slow
                                                                         // SRAM/ROM), this bit must be cleared to allow the system to process other
                                                                         // information rather than remain idle while all the data is read from the
                                                                         // device. When this bit is cleared, the system is allowed to process other
                                                                         // information. When the bit is set, the system halts and waits until all data is
                                                                         // returned from the device.
                                                                         // 
                                                                         // When synchronous static memory devices have been enabled for a given
                                                                         // bank, this bit defaults to streaming behavior. The register bit is read as 0
                                                                         // unless it has specifically been programmed to a 1. This behavior cannot
                                                                         // be overridden.
                                                                         // 
                                                                         //   0 = Slower device (return data buffer)
                                                                         //   1 = Faster device (streaming behavior)
                                                                         // 
            [BitFieldRegister(Position=28,Size=3)] public uint RRR1;     // ROM/SRAM Recovery Time
            [BitFieldRegister(Position=12,Size=3)] public uint RRR0;     // 
                                                                         // 
                                                                         // The value of this bit is half the number of memory clock cycles from the
                                                                         // time that chip select is de-asserted after a read or write until the next chip
                                                                         // select (of a different static memory bank) or nSDCS is asserted.
                                                                         // 
                                                                         // This field must be programmed with the highest of the three values: tOFF
                                                                         // divided by two, write pulse high time (flash memory/SRAM), and write
                                                                         // recovery before read (flash memory).
                                                                         // 
                                                                         //     tOFF = RRR1 * 2 + 1
                                                                         // 
                                                                         // NOTE: The MSCx[RRR] value (recovery time after chip select deasserted)
                                                                         //       must be reprogrammed prior to switching the processor
                                                                         //       to deep-idle mode to avoid long times when the MD bus
                                                                         //       (MD<31:0>) is in three-state mode after reads.
                                                                         // 
            [BitFieldRegister(Position=24,Size=4)] public Delay RDN1;    // ROM Delay Next Access 
            [BitFieldRegister(Position= 8,Size=4)] public Delay RDN0;    //
                                                                         // 
                                                                         // The RDN field is encoded as follows:
                                                                         // 
                                                                         //    ENCODED (Programmed) Value -----> DECODED (Actual) Value
                                                                         //    0-11 -----> 0-11
                                                                         //    12 -----> 15
                                                                         //    13 -----> 20
                                                                         //    14 -----> 26
                                                                         //    15 -----> 30
                                                                         // 
                                                                         // Use the DECODED value in the equations below for RDNx instead of the actual RDNx value.
                                                                         // 
                                                                         // For burst ROM or flash memory:
                                                                         // • RDNx + 1 = number of MEM_CLKs from Address to Data Valid for subsequent access.
                                                                         // 
                                                                         // For flash memory or SRAM:
                                                                         // • RDNx + 1 = number of CLK_MEMs nWE is asserted for write accesses
                                                                         // 
                                                                         // For VLIO:
                                                                         // • RDNx * 2 = amount of time nOE or nPWE is deasserted to address hold and address setup to nOE or nPWE assertion time.
                                                                         // 
                                                                         // NOTE: For VLIO, this number must be greater than or equal to 2. The
                                                                         //       memory controller substitutes a default value of 2 for values less than 2.
                                                                         // 
                                                                         // 
            [BitFieldRegister(Position=20,Size=4)] public Delay RDF1;    // ROM Delay First Access
            [BitFieldRegister(Position= 4,Size=4)] public Delay RDF0;    // 
                                                                         // 
                                                                         // The encoding scheme is:
                                                                         // 
                                                                         //    ENCODED (programmed) value ------> DECODED (actual) value:
                                                                         //    0-11 ------> 0-11
                                                                         //    12 ------> 15
                                                                         //    13 ------> 20
                                                                         //    14 -----> 26
                                                                         //    15 ------> 30
                                                                         // 
                                                                         // The DECODED value represents:
                                                                         // • Number of memory-clock cycles (minus 2) from address to data valid
                                                                         //   for first read access from all devices except VLIO
                                                                         // • Number of memory clock cycles (minus 1) from address to data valid
                                                                         //   for subsequent read accesses to non-burst devices except VLIO
                                                                         // • Number of memory clock cycles (minus 1) of nWE assertion for write
                                                                         //   accesses to all types of flash memory
                                                                         // 
                                                                         // For variable-latency I/O, this determines the minimum number of memory
                                                                         // clock cycles (minus 1) of nOE (nPWE) assert time for each beat of read (write).
                                                                         // 
                                                                         // NOTE: For VLIO, this number must be greater than or equal to 3. The
                                                                         //       memory controller substitutes a default value of 3 for values less than 3.
                                                                         // 
            [BitFieldRegister(Position=19        )] public bool RBW1;    // ROM Bus Width 
            [BitFieldRegister(Position= 3        )] public bool RBW0;    //
                                                                         // 
                                                                         // For reset value of RBW0, see Section 6.5.4.
                                                                         // 
                                                                         // This value must be programmed, even when using synchronous static memory in banks 0,1,2, or 3.
                                                                         //   0 = 32 bits
                                                                         //   1 = 16 bits
                                                                         // 
                                                                         // NOTE: This value must not be changed during normal operation.
                                                                         // 
            [BitFieldRegister(Position=16,Size=3)] public MemType RT1;   // ROM Type
            [BitFieldRegister(Position= 0,Size=3)] public MemType RT0;   // 
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct SA1110_bitfield
        {
            [BitFieldRegister(Position=13)] public bool SXSTACK_CS1;   // If set, nCS<1> contains stacked flash.
            [BitFieldRegister(Position=12)] public bool SXSTACK_CS0;   // If set, nCS<0> contains stacked flash 
                                                                       //
            [BitFieldRegister(Position= 8)] public bool SXENX;         // Large Memory Support
                                                                       // 
                                                                       // See Figure 6-11 for a diagram.
                                                                       // 
                                                                       //   0 = Use six 64 Mbyte chip selects (nCS<5:0>
                                                                       //   1 = Use two 64 Mbyte chip selects (nCS<5:4>) and two 128 Mbyte chip selects (nCS<1:0>). The corresponding SA1110[3:0] bit fields must be cleared.
                                                                       // 
            [BitFieldRegister(Position= 5)] public bool SA1110_5;      // If set, SA-1110 Compatibility Mode for Static Memory Partition 5
            [BitFieldRegister(Position= 4)] public bool SA1110_4;      // If set, SA-1110 Compatibility Mode for Static Memory Partition 4
            [BitFieldRegister(Position= 3)] public bool SA1110_3;      // If set, SA-1110 Compatibility Mode for Static Memory Partition 3
            [BitFieldRegister(Position= 2)] public bool SA1110_2;      // If set, SA-1110 Compatibility Mode for Static Memory Partition 2
            [BitFieldRegister(Position= 1)] public bool SA1110_1;      // If set, SA-1110 Compatibility Mode for Static Memory Partition 1
            [BitFieldRegister(Position= 0)] public bool SA1110_0;      // If set, SA-1110 Compatibility Mode for Static Memory Partition 0
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct BOOT_DEF_bitfield
        {
            [BitFieldRegister(Position=3)] public bool PKG_TYPE; // Package Type
                                                                 // 
                                                                 //   0 = reserved
                                                                 //   1 = 32-bit package
                                                                 // 
            [BitFieldRegister(Position=0)] public bool BOOT_SEL; // Boot Select
                                                                 // 
                                                                 // Contains the input pin BOOT_SEL for the memory controller. See Section 6.4.7 for pin configuration definitions.
                                                                 // 
                                                                 //   0 = BOOT_SEL is low (boot from 32-bit memory).
                                                                 //   1 = BOOT_SEL is high (boot from 16-bit memory).
                                                                 // 
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct ARB_CNTRL_bitfield
        {
            [BitFieldRegister(Position=31       )] public bool DMA_SLV_park; // 1 = Bus is parked with DMA slave when idle
            [BitFieldRegister(Position=30       )] public bool CI_park;      // 1 = Bus is parked with quick capture interface when idle
            [BitFieldRegister(Position=29       )] public bool EX_MEM_park;  // 1 = Bus is parked with external memory controller when idle
            [BitFieldRegister(Position=28       )] public bool INT_MEM_park; // 1 = Bus is parked with internal memory controller when idle
            [BitFieldRegister(Position=27       )] public bool USB_park;     // 1 = Bus is parked with USB host controller when idle
            [BitFieldRegister(Position=26       )] public bool LCD_park;     // 1 = Bus is parked with LCD controller when idle
            [BitFieldRegister(Position=25       )] public bool DMA_park;     // 1 = Bus is parked with DMA controller when idle
            [BitFieldRegister(Position=24       )] public bool Core_park;    // 1 = Bus is parked with core when idle
            [BitFieldRegister(Position=23       )] public bool LOCK_FLAG;    // 1 = Only locking masters gain access to the bus
                                                                             // 
            [BitFieldRegister(Position= 8,Size=4)] public uint LCD_Wt;       // LCD Priority Value
                                                                             // 
                                                                             // Values in this field determine the relative priority of LCD requests for the bus with core and DMA requests.
                                                                             // 
            [BitFieldRegister(Position= 4,Size=4)] public uint DMA_Wt;       // DMA Priority Value
                                                                             // 
                                                                             // Values in this field determine the relative priority of DMA requests for the bus with core and LCD requests.
                                                                             // 
            [BitFieldRegister(Position= 0,Size=4)] public uint Core_Wt;      // Core Priority Value
                                                                             // 
                                                                             // Values in this field determine the relative priority of core requests for the bus with DMA and LCD requests.
                                                                             // 

        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct BSCNTR0_bitfield
        {
            [BitFieldRegister(Position=28,Size=4)] public uint CKE1BS; // SDCKE Buffer Strength Control register
            [BitFieldRegister(Position=24,Size=4)] public uint CLK2BS; // SDCLK<2> Buffer Strength Control register
            [BitFieldRegister(Position=20,Size=4)] public uint CLK1BS; // SDCLK<1> Buffer Strength Control register
            [BitFieldRegister(Position=16,Size=4)] public uint CLK0BS; // SDCLK<0> Buffer Strength Control register
            [BitFieldRegister(Position=12,Size=4)] public uint RASBS;  // SDRAS Buffer Strength Control register
            [BitFieldRegister(Position= 8,Size=4)] public uint CASBS;  // SDCAS Buffer Strength Control register
            [BitFieldRegister(Position= 4,Size=4)] public uint MDHBS;  // MD<31:16> Buffer Strength Control register
            [BitFieldRegister(Position= 0,Size=4)] public uint MDLBS;  // MD<15:0> Buffer Strength Control register
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct BSCNTR1_bitfield
        {
            [BitFieldRegister(Position=28,Size=4)] public uint DQM32BS;     // DQM<3:2> Buffer Strength Control register
            [BitFieldRegister(Position=24,Size=4)] public uint DQM10BS;     // DQM<1:0> Buffer Strength Control register
            [BitFieldRegister(Position=20,Size=4)] public uint SDCS32BS;    // SDCS<3:2> Buffer Strength Control register
            [BitFieldRegister(Position=16,Size=4)] public uint SDCS10BS;    // SDCS<1:0> Buffer Strength Control register
            [BitFieldRegister(Position=12,Size=4)] public uint WEBS;        // nWE Buffer Strength Control register
            [BitFieldRegister(Position= 8,Size=4)] public uint OEBS;        // nOE Buffer Strength Control register
            [BitFieldRegister(Position= 4,Size=4)] public uint SDCAS_DELAY; // SDCAS Return Signal Timing Delay 
                                                                            // 
                                                                            // 0x5—average timing delay
                                                                            // 
                                                                            // All other values are not valid.
                                                                            // 
                                                                            // NOTE: Do not use this bit field to control the buffer strength for SDCAS.
                                                                            //       This bit field is for signal return clock timing only.
            [BitFieldRegister(Position= 0,Size=4)] public uint RDnWRBS;     // RDnWR Buffer Strength Control register
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct BSCNTR2_bitfield
        {
            [BitFieldRegister(Position=28,Size=4)] public uint CS5BS;  // nCS<5> Buffer Strength Control register
            [BitFieldRegister(Position=24,Size=4)] public uint CS4BS;  // nCS<4> Buffer Strength Control register
            [BitFieldRegister(Position=20,Size=4)] public uint CS3BS;  // nCS<3> Buffer Strength Control register
            [BitFieldRegister(Position=16,Size=4)] public uint CS2BS;  // nCS<2> Buffer Strength Control register
            [BitFieldRegister(Position=12,Size=4)] public uint CS1BS;  // nCS<1> Buffer Strength Control register
            [BitFieldRegister(Position= 8,Size=4)] public uint CS0BS;  // nCS<0> Buffer Strength Control register
            [BitFieldRegister(Position= 4,Size=4)] public uint CLK3BS; // SDCLK<3> Buffer Strength Control Bits
            [BitFieldRegister(Position= 0,Size=4)] public uint MA25BS; // MA<25> Buffer Strength Control register
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct BSCNTR3_bitfield
        {
            [BitFieldRegister(Position=28,Size=4)] public uint MA24BS;   // MA<24> Buffer Strength Control register
            [BitFieldRegister(Position=24,Size=4)] public uint MA23BS;   // MA<23> Buffer Strength Control register
            [BitFieldRegister(Position=20,Size=4)] public uint MA22BS;   // MA<22> Buffer Strength Control register
            [BitFieldRegister(Position=16,Size=4)] public uint MA21BS;   // MA<21> Buffer Strength Control register
            [BitFieldRegister(Position=12,Size=4)] public uint MA2010BS; // MA<20:10> Buffer Strength Control register
            [BitFieldRegister(Position= 8,Size=4)] public uint MA92BS;   // MA<9:2> Buffer Strength Control register
            [BitFieldRegister(Position= 4,Size=4)] public uint MA1BS;    // MA<1> Buffer Strength Control register
            [BitFieldRegister(Position= 0,Size=4)] public uint MA0BS;    // MA<0> Buffer Strength Control register
        }
        
        [Register(Offset=0x00U)] public MDCNFG_bitfield    MDCNFG;    // SDRAM Configuration register                                                 | 6-43
        [Register(Offset=0x04U)] public MDREFR_bitfield    MDREFR;    // SDRAM Refresh Control register                                               | 6-53
        [Register(Offset=0x08U)] public MSC_bitfield       MSC0;      // Static Memory Control register 0                                             | 6-63
        [Register(Offset=0x0CU)] public MSC_bitfield       MSC1;      // Static Memory Control register 1                                             | 6-63
        [Register(Offset=0x10U)] public MSC_bitfield       MSC2;      // Static Memory Control register 2                                             | 6-63
////    [Register(Offset=0x14U)] public uint               MECR;      // Expansion Memory (PC Card/CompactFlash) Bus Configuration register           | 6-79
        [Register(Offset=0x1CU)] public SXCNFG_bitfield    SXCNFG;    // Synchronous Static Memory Configuration register                             | 6-58
        [Register(Offset=0x20U)] public uint               FLYCNFG;   // Fly-by DMA DVAL<1:0> polarities                                              | 5-39
////    [Register(Offset=0x28U)] public uint               MCMEM0;    // PC Card Interface Common Memory Space Socket 0 Timing Configuration register | 6-77
////    [Register(Offset=0x2CU)] public uint               MCMEM1;    // PC Card Interface Common Memory Space Socket 1 Timing Configuration register | 6-77
////    [Register(Offset=0x30U)] public uint               MCATT0;    // PC Card Interface Attribute Space Socket 0 Timing Configuration register     | 6-77
////    [Register(Offset=0x34U)] public uint               MCATT1;    // PC Card Interface Attribute Space Socket 1 Timing Configuration register     | 6-77
////    [Register(Offset=0x38U)] public uint               MCIO0;     // PC Card Interface I/o Space Socket 0 Timing Configuration register           | 6-78
////    [Register(Offset=0x3CU)] public uint               MCIO1;     // PC Card Interface I/o Space Socket 1 Timing Configuration register           | 6-78
        [Register(Offset=0x40U)] public MDMRS_bitfield     MDMRS;     // SDRAM Mode Register Set Configuration register                               | 6-49
        [Register(Offset=0x44U)] public BOOT_DEF_bitfield  BOOT_DEF;  // Boot Time Default Configuration register                                     | 6-75
        [Register(Offset=0x48U)] public ARB_CNTRL_bitfield ARB_CNTL;  // Arbiter Control register                                                     | 29-2
        [Register(Offset=0x4CU)] public BSCNTR0_bitfield   BSCNTR0;   // System Memory Buffer Strength Control register 0                             | 6-81
        [Register(Offset=0x50U)] public BSCNTR1_bitfield   BSCNTR1;   // System Memory Buffer Strength Control register 1                             | 6-82
////    [Register(Offset=0x54U)] public uint               LCDBSCNTR; // LCD Buffer Strength Control register                                         | 7-102
        [Register(Offset=0x58U)] public MDMRSLP_bitfield   MDMRSLP;   // Special Low Power SDRAM Mode Register Set Configuration register             | 6-51
        [Register(Offset=0x5CU)] public BSCNTR2_bitfield   BSCNTR2;   // System Memory Buffer Strength Control register 2                             | 6-83
        [Register(Offset=0x60U)] public BSCNTR3_bitfield   BSCNTR3;   // System Memory Buffer Strength Control register 3                             | 6-84
        [Register(Offset=0x64U)] public SA1110_bitfield    SA1110;    // SA-1110 Compatibility Mode for Static Memory register                        | 6-70

        //
        // Helper Methods
        //

        [DisableNullChecks]
        public unsafe void InitializeStackedSDRAM()
        {
            //
            // Set SDRAM configuration
            //
            this.MDCNFG = new MDCNFG_bitfield
            {
                SET0  = true,
                SET2  = true,
                MDENX = false, // normal SDRAM memory map
                STACK = MDCNFG_bitfield.Stack.SDRAMplusFLASH,
                DTC0  = MDCNFG_bitfield.Timing.Setting3, // use normal addressing mode, 3CLK cycle, tRP=3, CL=3,tRAS=7,tRC=11
                DNB0  = true, // 4 internal bank
                DRAC0 = MDCNFG_bitfield.RowAddress.Use13Bits, // 13 row address
                DCAC0 = MDCNFG_bitfield.ColumnAddress.Use9Bits, // 9 col address
                DWID0 = true, // 16 bit bus
            };

            //
            // Enable partition 0 clock for synchronous memory
            //
            {
                var mdrefr = this.MDREFR;

                mdrefr.DRI    = 25; // (refresh interval in SDRAM clocks)/32 = ((64mS/8192 rows) * 104 MHz)/32 (rounded down) 
                mdrefr.K0FREE = false;
                mdrefr.K1FREE = false;
                mdrefr.K2FREE = false;
                mdrefr.APD    = false;
                mdrefr.K0DB4  = true;
                mdrefr.SLFRSH = true;
                mdrefr.K0RUN  = true;

                this.MDREFR = mdrefr;
            }

            {
                var mdrefr = this.MDREFR;

                mdrefr.K1DB2 = true;
                mdrefr.K1RUN = true;

                this.MDREFR = mdrefr;
            }

            this.MDREFR.SLFRSH = false;
            this.MDREFR.E1PIN  = true;

            //
            // dummy read for entering NOP state, A20=0
            //
            uint sdramBaseAddress = 0xA0000000u;

            StackedFlashChip.InitializeSDRAM( sdramBaseAddress );

            // delay for 200usec
            Processor.DelayMicroseconds( 200 );

            this.MDCNFG.DE0 = true; // Enable SDRAM

            // Issue precharge all command. MA10 ( or  A20) bit set to high for precharge ALL
            StackedFlashChip.InitializeSDRAM( sdramBaseAddress + 0x10000 );

            // Issue 2 AutoRefresh Command
            StackedFlashChip.InitializeSDRAM( sdramBaseAddress );
            StackedFlashChip.InitializeSDRAM( sdramBaseAddress );

            //
            // Set Mode Register
            //
            this.MDMRS.MDMRS0 = 0;

            //
            // Issue Extended Mode Register Command, enable Low power, Amibent temp = 70deg cel, Partial array self refresh = four banks.
            //
            this.MDMRSLP = new MDMRSLP_bitfield
            {
                MDLPEN0 = true,
            };

            //
            // Set Mode Register, back to NOP
            //
            this.MDMRS.MDMRS0 = 0;


            //
            // Turn on Auto Power Down
            //
            this.MDREFR.APD = true;
        }

        [DisableNullChecks]
        [MemoryRequirements(MemoryAttributes.RAM)]
        public unsafe void InitializeStackedFLASH()
        {
            //
            // Flash at CS0, no large memory, no byte addressing
            //
            this.SA1110 = new SA1110_bitfield
            {
                SXSTACK_CS0 = true,
            };

            this.MSC0 = new MSC_bitfield
            {
                RT0    = MSC_bitfield.MemType.FlashOrRomBurstOf4,
                RBW0   = true, // 16 bits
                RBUFF0 = true, // streaming behavior
                RDF0   = MSC_bitfield.Delay.Val15,
                RDN0   = MSC_bitfield.Delay.Val7,
                RRR0   = 2,

                //
                // Keep the other section uninitialized.
                //
                RT1    = MSC_bitfield.MemType.FlashOrRomNoBurst,
                RBW1   = false,
                RBUFF1 = false,
                RDF1   = MSC_bitfield.Delay.Val30,
                RDN1   = MSC_bitfield.Delay.Val30,
                RRR1   = 7,
            };

            //
            // Initialize Flash here instead of flash driver, as it must be done from RAM
            //
            {
                ushort flashConfig;

                flashConfig  =      StackedFlashChip.FLASH_CONFIG_RESERVED;
                flashConfig |=      StackedFlashChip.FLASH_CONFIG_SYNC;
                flashConfig |=      StackedFlashChip.FLASH_CONFIG_CLK_HI_EDGE;
                flashConfig |=      StackedFlashChip.FLASH_CONFIG_DATA_HOLD_1;
                flashConfig |= 4 << StackedFlashChip.FLASH_CONFIG_LAT_SHIFT;
                flashConfig |=      StackedFlashChip.FLASH_CONFIG_BURST_16;
                flashConfig |=      StackedFlashChip.FLASH_CONFIG_BURST_WRAP;

                StackedFlashChip.InitializeFLASH( 0, flashConfig );
            }

            this.SXCNFG = new SXCNFG_bitfield
            {
                Set0 = true,
                Set2 = true,
                SXTP0 = SXCNFG_bitfield.MemoryType.BurstOf16,
                SXCL0 = SXCNFG_bitfield.CasLatency.CL5,
                SXEN0 = true,
            };
        }

        //
        // Access Methods
        //

        public static extern MemoryController Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}