//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.LPC3180
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x200A8000U,Length=0x00010050U)]
    public class MultiLevelNANDController
    {
        public const uint MLC_LOCK_PR__UnlockValue = 0x0000A25E;

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MLC_ICR_bitfield
        {
            public enum Addressing
            {
                ThreeWords = 0,
                FourWords  = 1,
            }

            [BitFieldRegister(Position=3       )] public bool       SoftwareWriteProtection; // 0: Software Write protection disabled.
                                                                                             // 1: Software Write protection enabled.
                                                                                             //
            [BitFieldRegister(Position=2       )] public bool       LargeBlockDevice;        // 0: small block flash device (512 +16 byte pages).
                                                                                             // 1: large block flash device (2k + 64 byte pages).
                                                                                             //
            [BitFieldRegister(Position=1,Size=1)] public Addressing ChipWordAddress;         // 0: NAND flash address word count 3.
                                                                                             // 1: NAND flash address word count 4.
                                                                                             //
            [BitFieldRegister(Position=0       )] public bool       Bus16bit;                // 0: NAND flash I/O bus with 8-bit.
                                                                                             // 1: NAND flash I/O bus with 16-bit (Not supported).
        }
                                                              
        //--//                                                
                                                              
        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MLC_TIME_REG_bitfield
        {
            [BitFieldRegister(Position=24,Size=2)] public uint TCEA_DELAY; // nCE low to dout valid (tCEA)
                                                                           // Default: 0x00
                                                                           //
            [BitFieldRegister(Position=19,Size=5)] public uint BUSY_DELAY; // Read/Write high to busy (tWB/tRB)
                                                                           // Default: 0x00
                                                                           // 
            [BitFieldRegister(Position=16,Size=3)] public uint NAND_TA;    // Read high to high impedance (tRHZ)
                                                                           // Default: 0x00
                                                                           // 
            [BitFieldRegister(Position=12,Size=4)] public uint RD_HIGH;    // Read high hold time (tREH)
                                                                           // Default: 0x00
                                                                           // 
            [BitFieldRegister(Position= 8,Size=4)] public uint RD_LOW;     // Read pulse width (tRP)
                                                                           // Default: 0x00
                                                                           //
            [BitFieldRegister(Position= 4,Size=4)] public uint WR_HIGH;    // Write high hold time (tWH)
                                                                           // Default: 0x00
                                                                           //
            [BitFieldRegister(Position= 0,Size=4)] public uint WR_LOW;     // Write pulse width (tWP) 
                                                                           // Default: 0x07
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MLC_IRQ_MR_bitfield
        {
            [BitFieldRegister(Position=5)] public bool NandReady;                    // NAND Ready (0: Disabled, 1: Enabled)
                                                                                     // This interrupt occurs when the NAND flash’s Ready/nBusy signal transitions from the Busy state to the Ready state.
                                                                                     // This interrupt is delayed by the NAND flash’s tWB/tRB parameters.
                                                                                     //
            [BitFieldRegister(Position=4)] public bool ControllerReady;              // Controller Ready (0: Disabled, 1: Enabled)
                                                                                     // This interrupt indicates that the controller has completed one of the following actions:
                                                                                     //   1) Parity read complete
                                                                                     //   2) Parity write complete
                                                                                     //   3) Auto decode complete
                                                                                     //   4) Auto encode complete
                                                                                     // 
            [BitFieldRegister(Position=3)] public bool DecodeFailure;                // Decode failure (0: Disabled, 1: Enabled)
                                                                                     // This interrupt indicates that the R/S ECC decoder has detected errors present in the last decode cycle that cannot be properly corrected
                                                                                     // (this indicates that the severity of the error exceeds the correction capability of the decoder).
                                                                                     //
            [BitFieldRegister(Position=2)] public bool DecodeErrorDetected;          // Decode error detected (0: Disabled, 1: Enabled)
                                                                                     // This interrupt indicates that the R/S ECC decoder has detected (and possibly corrected) errors present in the last decode cycle.
                                                                                     // The CPU should read the controller’s Status register to determine the severity of the error.
                                                                                     // The CPU should also discard the data and read the corrected data from the controller’s serial Data Buffer.
                                                                                     //
            [BitFieldRegister(Position=1)] public bool EccReady;                     // ECC Encode/Decode ready (0: Disabled, 1: Enabled)
                                                                                     // This interrupt indicates that the ECC Encoder or Decoder has completed the encoding or decoding process.
                                                                                     // For an encode cycle this interrupt occurs after the following actions:
                                                                                     //   1) Host begins encoding cycle by accessing the ECC Encode register,
                                                                                     //   2) Host writes 518 bytes of NAND data, and
                                                                                     //   3) R/S ECC encoding completes.
                                                                                     // For a decode cycle this interrupt occurs after the following actions:
                                                                                     //   1) Host begins decoding cycle by accessing the ECC Decode register,
                                                                                     //   2) Host reads 518/528 bytes of NAND data, and
                                                                                     //   3) R/S ECC decoding completes.
                                                                                     //
            [BitFieldRegister(Position=0)] public bool SoftwareWriteProtectionFault; // Software write protection fault (0: Disabled, 1: Enabled)
                                                                                     // This interrupt indicates that the last NAND write operation was aborted due to a write protection fault.
                                                                                     // This interrupt can occur after the Erase Start (0x60) command or any Auto Program (0x10, 0x11, 0x15) command
                                                                                     // is written to the NAND after the previous address data following the Serial Input (0x80) or Auto Erase (0x60) commands
                                                                                     // falls within the software protection address range and software write protection is enabled.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MLC_IRQ_SR_bitfield
        {
            [BitFieldRegister(Position=5)] public bool NandReady;                    // NAND Ready (0: Inactive, 1: Active)
                                                                                     // This interrupt occurs when the NAND flash’s Ready/nBusy signal transitions from the Busy state to the Ready state.
                                                                                     // This interrupt is delayed by the NAND flash’s tWB/tRB parameters.
                                                                                     //
            [BitFieldRegister(Position=4)] public bool ControllerReady;              // Controller Ready (0: Inactive, 1: Active)
                                                                                     // This interrupt indicates that the controller has completed one of the following actions:
                                                                                     //   1) Parity read complete
                                                                                     //   2) Parity write complete
                                                                                     //   3) Auto decode complete
                                                                                     //   4) Auto encode complete
                                                                                     // 
            [BitFieldRegister(Position=3)] public bool DecodeFailure;                // Decode failure (0: Inactive, 1: Active)
                                                                                     // This interrupt indicates that the R/S ECC decoder has detected errors present in the last decode cycle that cannot be properly corrected
                                                                                     // (this indicates that the severity of the error exceeds the correction capability of the decoder).
                                                                                     //
            [BitFieldRegister(Position=2)] public bool DecodeErrorDetected;          // Decode error detected (0: Inactive, 1: Active)
                                                                                     // This interrupt indicates that the R/S ECC decoder has detected (and possibly corrected) errors present in the last decode cycle.
                                                                                     // The CPU should read the controller’s Status register to determine the severity of the error.
                                                                                     // The CPU should also discard the data and read the corrected data from the controller’s serial Data Buffer.
                                                                                     //
            [BitFieldRegister(Position=1)] public bool EccReady;                     // ECC Encode/Decode ready (0: Inactive, 1: Active)
                                                                                     // This interrupt indicates that the ECC Encoder or Decoder has completed the encoding or decoding process.
                                                                                     // For an encode cycle this interrupt occurs after the following actions:
                                                                                     //   1) Host begins encoding cycle by accessing the ECC Encode register,
                                                                                     //   2) Host writes 518 bytes of NAND data, and
                                                                                     //   3) R/S ECC encoding completes.
                                                                                     // For a decode cycle this interrupt occurs after the following actions:
                                                                                     //   1) Host begins decoding cycle by accessing the ECC Decode register,
                                                                                     //   2) Host reads 518/528 bytes of NAND data, and
                                                                                     //   3) R/S ECC decoding completes.
                                                                                     //
            [BitFieldRegister(Position=0)] public bool SoftwareWriteProtectionFault; // Software write protection fault (0: Inactive, 1: Active)
                                                                                     // This interrupt indicates that the last NAND write operation was aborted due to a write protection fault.
                                                                                     // This interrupt can occur after the Erase Start (0x60) command or any Auto Program (0x10, 0x11, 0x15) command
                                                                                     // is written to the NAND after the previous address data following the Serial Input (0x80) or Auto Erase (0x60) commands
                                                                                     // falls within the software protection address range and software write protection is enabled.
                                                                                     //
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MLC_ISR_bitfield
        {
            [BitFieldRegister(Position=6       )] public bool DecoderFailure;  // Decoder Failure
                                                                               // This flag indicates that the last R/S Decoding cycle was unsuccessful at correcting errors present in the data.
                                                                               // This indicates that the number of errors in the data exceeds the decoder’s correction ability (more than 4 symbols).
                                                                               // The host should inspect this flag prior to validating the data read during the last decoding cycle.
                                                                               //
            [BitFieldRegister(Position=4,Size=2)] public uint SymbolErrors;    // Number of R/S symbols errors
                                                                               // This 2-bit field indicates the number of symbol errors detected by the last R/S decoding cycle.
                                                                               // Note that this field is only valid when both the following conditions are met:
                                                                               //   1) Errors Detected flag is set and
                                                                               //   2) Decoder Failure flag is clear.
                                                                               // 
                                                                               // 00: One symbol-error detected.
                                                                               // 01: Two symbol-error detected.
                                                                               // 10: Three symbol-error detected.
                                                                               // 11: Four symbol-error detected.
                                                                               //
            [BitFieldRegister(Position=3       )] public bool ErrorsDetected;  // Errors Detected
                                                                               // This flag indicates that the last R/S Decode cycle has detected errors in the page data.
                                                                               // This flag does not indicate error severity but merely indicates that errors have been detected.
                                                                               //
            [BitFieldRegister(Position=2       )] public bool EccReady;        // ECC Ready
                                                                               // This flag indicates the R/S ECC encoding/decoding process has been completed.
                                                                               // The Host must check this flag prior to using data read during a decode cycle.
                                                                               // The CPU can also check the status of an encode cycle prior to accessing the Write Parity register
                                                                               // (this in not necessary since the controller ensures that the R/S encoding has completed before writing any data)
                                                                               //
            [BitFieldRegister(Position=1       )] public bool ControllerReady; // Controller Ready
                                                                               // This flag indicates that the controller has completed any of the following:
                                                                               //   1) Read parity cycle,
                                                                               //   2) Write parity cycle,
                                                                               //   3) Auto Encode cycle and
                                                                               //   4) Auto Decode cycle.
                                                                               // The flag is cleared when any of the above operations are started.
                                                                               // The flag must be checked by the CPU prior to attempting an access to the corresponding NAND flash device.
                                                                               // Failure to perform the check may result in unexpected operation and/or data loss.
                                                                               //
            [BitFieldRegister(Position=0       )] public bool NandReady;       // NAND Ready
                                                                               // This flag reflects the status of the NAND flash’s Ready/nBusy signal.
                                                                               // Note that the CPU need not consider the NAND flash’s tWB, tRB timing parameters.
                                                                               // The controller delays the update of the NAND ready flag when data, address, or commands are sent to the NAND flash. 
                                                                               // This ensures that the NAND ready flag remains clear until the tWB, tRB time has passed and
                                                                               // the true status of the NAND flash’s Ready/nBusy signal can be reported.
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MLC_CEH_bitfield
        {
            [BitFieldRegister(Position=0)] public bool ForceNotCEAssert; // 0: Force nCE assert
                                                                         // 1: Normal nCE operation (nCE controlled by controller).
        }

        //--//

        [Register(Offset=0x00000000,Instances=32768/sizeof(byte  ))] public byte  []              MLC_BUFF_8bits;       // MLC NAND Data Buffer. - R/W
        [Register(Offset=0x00000000,Instances=32768/sizeof(ushort))] public ushort[]              MLC_BUFF_16bits;      // MLC NAND Data Buffer. - R/W
        [Register(Offset=0x00000000,Instances=32768/sizeof(uint  ))] public uint  []              MLC_BUFF_32bits;      // MLC NAND Data Buffer. - R/W
                                                                                                                      
        [Register(Offset=0x00008000,Instances=32768/sizeof(byte  ))] public byte  []              MLC_DATA_8bits;       // Start of MLC data buffer - R/W
        [Register(Offset=0x00008000,Instances=32768/sizeof(ushort))] public ushort[]              MLC_DATA_16bits;      // Start of MLC data buffer - R/W
        [Register(Offset=0x00008000,Instances=32768/sizeof(uint  ))] public uint  []              MLC_DATA_32bits;      // Start of MLC data buffer - R/W
                                                                                                                      
        [Register(Offset=0x00010000                               )] public uint                  MLC_CMD;              // MLC NAND Flash Command Register. 0x0 WO
        [Register(Offset=0x00010004                               )] public uint                  MLC_ADDR;             // MLC NAND Flash Address Register. 0x0 WO
                                                                                    
        [Register(Offset=0x00010008                               )] public uint                  MLC_ECC_ENC_REG;      // MLC NAND ECC Encode Register. 0x0 WO
        [Register(Offset=0x0001000C                               )] public uint                  MLC_ECC_DEC_REG;      // MLC NAND ECC Decode Register. 0x0 WO
        [Register(Offset=0x00010010                               )] public uint                  MLC_ECC_AUTO_ENC_REG; // MLC NAND ECC Auto Encode Register. 0x0 WO
        [Register(Offset=0x00010014                               )] public uint                  MLC_ECC_AUTO_DEC_REG; // MLC NAND ECC Auto Decode Register. 0x0 WO
                                                                                    
        [Register(Offset=0x00010018                               )] public uint                  MLC_RPR;              // MLC NAND Read Parity Register. 0x0 WO
        [Register(Offset=0x0001001C                               )] public uint                  MLC_WPR;              // MLC NAND Write Parity Register. 0x0 WO
                                                                                    
        [Register(Offset=0x00010020                               )] public uint                  MLC_RUBP;             // MLC NAND Reset User Buffer Pointer Register. 0x0 WO
        [Register(Offset=0x00010024                               )] public uint                  MLC_ROBP;             // MLC NAND Reset Overhead Buffer Pointer Register. 0x0 WO
                                                                                    
        [Register(Offset=0x00010028                               )] public uint                  MLC_SW_WP_ADD_LOW;    // MLC NAND Software Write Protection Address Low Register. 0x0 WO
        [Register(Offset=0x0001002C                               )] public uint                  MLC_SW_WP_ADD_HIG;    // MLC NAND Software Write Protection Address High Register. 0x0 WO

        [Register(Offset=0x00010030                               )] public MLC_ICR_bitfield      MLC_ICR;              // MLC NAND controller Configuration Register. 0x0 WO
        [Register(Offset=0x00010034                               )] public MLC_TIME_REG_bitfield MLC_TIME_REG;         // MLC NAND Timing Register. 0x37 WO
        [Register(Offset=0x00010038                               )] public MLC_IRQ_MR_bitfield   MLC_IRQ_MR;           // MLC NAND Interrupt Mask Register. 0x0 WO
        [Register(Offset=0x0001003C                               )] public MLC_IRQ_SR_bitfield   MLC_IRQ_SR;           // MLC NAND Interrupt Status Register. 0x0 RO
        [Register(Offset=0x00010044                               )] public uint                  MLC_LOCK_PR;          // MLC NAND Lock Protection Register. 0x0 WO
        [Register(Offset=0x00010048                               )] public MLC_ISR_bitfield      MLC_ISR;              // MLC NAND Status Register. 0x0 RO
        [Register(Offset=0x0001004C                               )] public MLC_CEH_bitfield      MLC_CEH;              // MLC NAND Chip-Enable Host Control Register. 0x0 WO

        //
        // Helper Methods
        //

        [Inline]
        public void SetTimingRegister( uint hclkMHz ,
                                       uint tCEA    ,
                                       uint tWB_RB  ,
                                       uint tRHZ    ,
                                       uint tREH    ,
                                       uint tRP     ,
                                       uint tWH     ,
                                       uint tWP     )
        {
            double scale = (hclkMHz * 1E6) / 1E9;

            var val = new MLC_TIME_REG_bitfield();

            val.TCEA_DELAY = Math.Min( (uint)(tCEA   * scale + 0.5),  3 );
            val.BUSY_DELAY = Math.Min( (uint)(tWB_RB * scale + 0.5), 31 );
            val.NAND_TA    = Math.Min( (uint)(tRHZ   * scale + 0.5),  7 );
            val.RD_HIGH    = Math.Min( (uint)(tREH   * scale + 0.5), 15 );
            val.RD_LOW     = Math.Min( (uint)(tRP    * scale + 0.5), 15 );
            val.WR_HIGH    = Math.Min( (uint)(tWH    * scale + 0.5), 15 );
            val.WR_LOW     = Math.Min( (uint)(tWP    * scale + 0.5), 15 );

            this.MLC_LOCK_PR  = MLC_LOCK_PR__UnlockValue;
            this.MLC_TIME_REG = val;
        }

        //
        // Access Methods
        //

        public static extern MultiLevelNANDController Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}