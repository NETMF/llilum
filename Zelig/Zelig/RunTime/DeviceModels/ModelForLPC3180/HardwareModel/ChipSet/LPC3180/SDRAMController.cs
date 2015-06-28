//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.LPC3180
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x31080000U,Length=0x0000048CU)]
    public class SDRAMController
    {
        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MPMCControl_bitfield
        {
            [BitFieldRegister(Position=2)] public bool LowPowerMode;            // Indicates normal, or low-power mode:
                                                                                // 
                                                                                //  *0 = normal.
                                                                                //   1 = low-power mode.
                                                                                // 
                                                                                // Entering low-power mode reduces memory controller power consumption.
                                                                                // Dynamic memory is refreshed as necessary.
                                                                                // The memory controller returns to normal functional mode by clearing the low-power mode bit (L), or by Reset.
                                                                                // This bit must only be modified when the SDRAM Controller is in idle state.
                                                                                // 
            [BitFieldRegister(Position=0)] public bool SDRAM_Controller_Enable; // Indicates if the SDRAM Controller is enabled or disabled:
                                                                                // 
                                                                                //   0 = disabled.
                                                                                //  *1 = enabled.
                                                                                // 
                                                                                // Disabling the SDRAM Controller reduces power consumption.
                                                                                // When the memory controller is disabled the memory is not refreshed.
                                                                                // The memory controller is enabled by setting the enable bit, or by reset.
                                                                                // This bit must only be modified when the SDRAM Controller is in idle state.
                                                                                // 
        }

        //--//                                                
                                                              
        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MPMCStatus_bitfield
        {
            [BitFieldRegister(Position=2)] public bool SelfRefreshAcknowledge; // This bit indicates the operating mode of the SDRAM Controller:
                                                                               // 
                                                                               //   0 = normal mode
                                                                               //  *1 = self-refresh mode.
                                                                               // 
            [BitFieldRegister(Position=0)] public bool Busy;                   // This bit is used to ensure that the memory controller enters the low-power or disabled mode cleanly by determining if the memory controller is busy or not:
                                                                               // 
                                                                               //   0 = SDRAM Controller is idle.
                                                                               //  1* = SDRAM Controller is busy performing memory transactions, commands, auto-refresh cycles, or is in self-refresh mode.
                                                                               // 
        }
                                                              
        //--//                                                

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MPMCConfig_bitfield
        {
            [BitFieldRegister(Position=0)] public bool BigEndianMode; // Endian mode:
                                                                      // 
                                                                      //  *0 = little-endian mode.
                                                                      //   1 = big-endian mode.
                                                                      // 
                                                                      // On power-on reset, the value of the endian bit is 0.
                                                                      // All data must be flushed in the SDRAM Controller before switching between little-endian and big-endian modes.
                                                                      // 
        }
                                                              
        //--//                                                
                                                              
        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MPMCDynamicControl_bitfield
        {
            public enum Mode : uint
            {
                NORMAL = 0, //  *00 = issue SDRAM NORMAL operation command.
                MODE   = 1, //   01 = issue SDRAM MODE command.[1]
                PALL   = 2, //   10 = issue SDRAM PALL (precharge all) command.
                NOP    = 3, //   11 = issue SDRAM NOP (no operation) command).
            }

            [BitFieldRegister(Position=13       )] public bool DeepSleep;        // Low-power SDRAM deep-sleep mode:
                                                                                 // 
                                                                                 //  *0 = normal operation.
                                                                                 //   1 = enter deep power down mode.
                                                                                 // 
            [BitFieldRegister(Position= 7,Size=2)] public Mode Initialization;   // SDRAM initialization:
                                                                                 //  
                                                                                 //  *00 = issue SDRAM NORMAL operation command.
                                                                                 //   01 = issue SDRAM MODE command.[1]
                                                                                 //   10 = issue SDRAM PALL (precharge all) command.
                                                                                 //   11 = issue SDRAM NOP (no operation) command).
                                                                                 //
                                                                                 //
            [BitFieldRegister(Position= 5       )] public bool RAM_CLK;          // Memory clock control (MMC):
                                                                                 // 
                                                                                 //  *0 = RAM_CLK enabled (POR reset value).
                                                                                 //   1 = RAM_CLK disabled.
                                                                                 //
            [BitFieldRegister(Position= 4       )] public bool DDR_nCLK_Disable; // Inverted Memory Clock Control (IMCC):
                                                                                 // 
                                                                                 //  *0 = DDR_nCLK enabled.
                                                                                 //   1 = DDR_nCLK disabled.
                                                                                 //
            [BitFieldRegister(Position= 3       )] public bool SR_CLK;           // Self-Refresh Clock Control (SRMCC):
                                                                                 // 
                                                                                 //  *0 = RAM_CLK and DDR_nCLK run continuously during self-refresh mode.
                                                                                 //   1 = RAM_CLK and DDR_nCLK run are stopped during self-refresh mode.
                                                                                 //
            [BitFieldRegister(Position= 2       )] public bool SR_REQ;           // Self-refresh request, MPMCSREFREQ (SR):
                                                                                 // 
                                                                                 //   0 = normal mode.
                                                                                 //  *1 = enter self-refresh mode.
                                                                                 // 
                                                                                 // Note: this bit must be written to 0 by software for correct operation.
                                                                                 //
            [BitFieldRegister(Position= 1       )] public bool CS;               // Dynamic memory clock control (CS):
                                                                                 // 
                                                                                 //   0 = RAM_CLK stops when all SDRAMs are idle and during self-refresh mode.
                                                                                 //  *1 = RAM_CLK runs continuously.
                                                                                 // 
                                                                                 // When clock control is LOW the output clock RAM_CLK is stopped when there are no SDRAM transactions.
                                                                                 // The clock is also stopped during self-refresh mode.
                                                                                 //
            [BitFieldRegister(Position= 0       )] public bool CE;               // Dynamic memory clock enable (CE):
                                                                                 // 
                                                                                 //  *0 = clock enable of idle devices are deasserted to save power.
                                                                                 //   1 = all clock enables are driven HIGH continuously.
                                                                                 //
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MPMCDynamicReadConfig_bitfield
        {
            public enum CommandStrategy : uint
            {
                RAM_CLK           = 0, //  *00 = clock out delayed strategy, using RAM_CLK (command not delayed, clock out delayed).
                MPMCCLKDELAY      = 1, //   01 = command delayed strategy, using MPMCCLKDELAY (command delayed, clock out not delayed).
                MPMCCLKDELAYplus1 = 2, //   10 = command delayed strategy plus one clock cycle, using MPMCCLKDELAY (command delayed, clock out not delayed).
                MPMCCLKDELAYplus2 = 3, //   11 = command delayed strategy plus two clock cycles, using MPMCCLKDELAY (command delayed, clock out not delayed).
            }

            [BitFieldRegister(Position=12       )] public bool            DRP; // DDR SDRAM read data capture polarity (DRP)
                                                                               //
                                                                               //  *0 = data captured on the negative edge of HCLK.
                                                                               //   1 = data captured on the positive edge of HCLK.
                                                                               //
            [BitFieldRegister(Position= 8,Size=2)] public CommandStrategy DRD; // DDR SDRAM read data strategy (DRD):
                                                                               //
                                                                               //  *00 = clock out delayed strategy, using RAM_CLK (command not delayed, clock out delayed).
                                                                               //   01 = command delayed strategy, using MPMCCLKDELAY (command delayed, clock out not delayed).
                                                                               //   10 = command delayed strategy plus one clock cycle, using MPMCCLKDELAY (command delayed, clock out not delayed).
                                                                               //   11 = command delayed strategy plus two clock cycles, using MPMCCLKDELAY (command delayed, clock out not delayed).
                                                                               //
            [BitFieldRegister(Position= 4       )] public bool            SRP; // SDR-SDRAM read data capture polarity (SRP):
                                                                               //
                                                                               //  *0 = data captured on the negative edge of HCLK.
                                                                               //   1 = data captured on the positive edge of HCLK.
                                                                               //
            [BitFieldRegister(Position= 0,Size=2)] public CommandStrategy SRD; // SDR-SDRAM read data strategy (SRD):
                                                                               //
                                                                               //  *00 = clock out delayed strategy, using RAM_CLK (command not delayed, clock out delayed).
                                                                               //   01 = command delayed strategy, using MPMCCLKDELAY (command delayed, clock out not delayed).
                                                                               //   10 = command delayed strategy plus one clock cycle, using MPMCCLKDELAY (command delayed, clock out not delayed).
                                                                               //   11 = command delayed strategy plus two clock cycles, using MPMCCLKDELAY (command delayed, clock out not delayed).
                                                                               //
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MPMCDynamicConfig0_bitfield
        {
            public enum Performance : uint
            {
                HighPerf = 0x00, //  *00 = high-performance.
                LowPower = 0x01, //   01 = low-power.
            }

            public enum SizeMode : uint
            {
                Size_16Mb_2Mx8    = 0x00, //  *000 00 16Mb (2Mx8), 2 banks, row length = 11, column length = 9
                Size_16Mb_1Mx16   = 0x01, //   000 01 16Mb (1Mx16), 2 banks, row length = 11, column length = 8
                Size_64Mb_8Mx8    = 0x04, //   001 00 64Mb (8Mx8), 4 banks, row length = 12, column length = 9
                Size_64Mb_4Mx16   = 0x05, //   001 01 64Mb (4Mx16), 4 banks, row length = 12, column length = 8
                Size_128Mb_16Mx8  = 0x08, //   010 00 128Mb (16Mx8), 4 banks, row length = 12, column length = 10
                Size_128Mb_8Mx16  = 0x09, //   010 01 128Mb (8Mx16), 4 banks, row length = 12, column length = 9
                Size_256Mb_32Mx8  = 0x0C, //   011 00 256Mb (32Mx8), 4 banks, row length = 13, column length = 10
                Size_256Mb_16Mx16 = 0x0D, //   011 01 256Mb (16Mx16), 4 banks, row length = 13, column length = 9
                Size_512Mb_64Mx8  = 0x10, //   100 00 512Mb (64Mx8), 4 banks, row length = 13, column length = 11
                Size_512Mb_32Mx16 = 0x11, //   100 01 512Mb (32Mx16), 4 banks, row length = 13, column length = 10
            }

            public enum DeviceKind : uint
            {
                SDR    = 0x00, //  *000 = SDR SDRAM.
                               //   001 = reserved.
                LP_SDR = 0x02, //   010 = low power SDR SDRAM.
                               //   011 = reserved.
                DDR    = 0x04, //   100 = DDR SDRAM.
                               //   101 = reserved.
                LP_DDR = 0x06, //   110 = low power DDR SDRAM.
                               //   111 = reserved.
            }

            [BitFieldRegister(Position=20       )] public bool        WriteProtect; // Write protect (P):
                                                                                    //
                                                                                    //  *0 = writes not protected.
                                                                                    //   1 = write protected.
                                                                                    //
            [BitFieldRegister(Position=14       )] public bool        Bus32bit;     // Address Mapping (AM): external data bus.
                                                                                    //
                                                                                    //  *0 = 16-bit
                                                                                    //   1 = 32-bit
                                                                                    //
            [BitFieldRegister(Position=12,Size=2)] public Performance LowPower;     // Address Mapping (AM): performance.
                                                                                    //
            [BitFieldRegister(Position= 7,Size=5)] public SizeMode    Size;         // Address Mapping (AM): size.
                                                                                    //
            [BitFieldRegister(Position= 0,Size=3)] public DeviceKind  MemoryDevice; // Memory device (MD):
                                                                                    //
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct MPMCDynamicRasCas0_bitfield
        {
            public enum CasLatency : uint
            {
                                              //   0000 = reserved.
                Latency_HalfClock       =  1, //   0001 = one half clock cycle.
                Latency_1Clock          =  2, //   0010 = one clock cycle.
                Latency_1AndAHalfClocks =  3, //   0011 = one and a half clock cycles.
                Latency_2Clocks         =  4, //   0100 = two clock cycles.
                Latency_2AndAHalfClocks =  5, //   0101 = two and a half clock cycles.
                Latency_3Clocks         =  6, //  *0110 = three clock cycles.
                Latency_3AndAHalfClocks =  7, //   0111 = three and a half clock cycles.
                Latency_4Clocks         =  8, //   1000 = four clock cycles.
                Latency_4AndAHalfClocks =  9, //   1001 = four and a half clock cycles.
                Latency_5Clocks         = 10, //   1010 = five clock cycles.
                Latency_5AndAHalfClocks = 11, //   1011 = five and a half clock cycles.
                Latency_6Clocks         = 12, //   1100 = six clock cycles.
                Latency_6AndAHalfClocks = 13, //   1101 = six and a half clock cycles.
                Latency_7Clocks         = 14, //   1110 = seven clock cycles.
                Latency_7AndAHalfClocks = 15, //   1111 = seven and a half clock cycles.
            }

            public enum RasLatency : uint
            {
                                      //  0000 = reserved.
                Latency_1Clock  =  1, //  0001 to 1110 = n clock cycles.     (Default: 3)
                Latency_2Clock  =  2, //
                Latency_3Clock  =  3, //
                Latency_4Clock  =  4, //
                Latency_5Clock  =  5, //
                Latency_6Clock  =  6, //
                Latency_7Clock  =  7, //
                Latency_8Clock  =  8, //
                Latency_9Clock  =  9, //
                Latency_0Clock  = 10, //
                Latency_11Clock = 11, //
                Latency_12Clock = 12, //
                Latency_13Clock = 13, //
                Latency_14Clock = 14, //
                Latency_15Clock = 15, //  1111 = 15 clock cycles.
            }

            [BitFieldRegister(Position=7,Size=4)] public CasLatency CAS_Latency; // CAS latency (CAS):
                                                                                 //
            [BitFieldRegister(Position=0,Size=4)] public RasLatency RAS_Latency; // RAS latency (active to read/write delay) (RAS):
                                                                                 //
        }

        //--//

        [MemoryMappedPeripheral(Base=0x0000U,Length=0x000CU)]
        public class Port
        {
            [BitFieldPeripheral(PhysicalType=typeof(uint))]
            public struct Control_bitfield
            {
                [BitFieldRegister(Position=0)] public bool BufferEnable; // AHB Port Buffer Enable (E):
                                                                         //
                                                                         //  *0 = disable buffer.
                                                                         //   1 = enable buffer.
                                                                         //
            }

            [BitFieldPeripheral(PhysicalType=typeof(uint))]
            public struct Status_bitfield
            {
                [BitFieldRegister(Position=1)] public bool BufferStatus; // AHB Port Buffer Status (S):
                                                                         //
                                                                         //  0 = buffer empty.
                                                                         //  1 = buffer contains data.
                                                                         //
            }

            //--//

            [Register(Offset=0x00000000U)] public Control_bitfield Control; // Control register for AHB port x. 0 R/W
            [Register(Offset=0x00000004U)] public Status_bitfield  Status;  // Status register for AHB port x. 0 R/W
            [Register(Offset=0x00000008U)] public uint             Timeout; // Timeout register for AHB port x. 0 R/W     0x0 = timeout disabled. 0x001 - 0x1FF = number of AHB cycles before timeout is reached.
        }

        [Register(Offset=0x00000000)] public MPMCControl_bitfield           MPMCControl;           // Controls operation of the memory controller. 0x3 R/W
        [Register(Offset=0x00000004)] public MPMCStatus_bitfield            MPMCStatus;            // Provides SDRAM Controller status information. 0x5 RO
        [Register(Offset=0x00000008)] public MPMCConfig_bitfield            MPMCConfig;            // Configures operation of the memory controller. 0 R/W
        [Register(Offset=0x00000020)] public MPMCDynamicControl_bitfield    MPMCDynamicControl;    // Controls dynamic memory operation. 0x006 R/W
        [Register(Offset=0x00000024)] public uint                           MPMCDynamicRefresh;    // Configures dynamic memory refresh operation. 0 R/W              0x0 = refresh disabled. (Default) //  0x1 - 0x7FF = n × 16 = 16n clocks between SDRAM refresh cycles.
        [Register(Offset=0x00000028)] public MPMCDynamicReadConfig_bitfield MPMCDynamicReadConfig; // Configures the dynamic memory read strategy. 0 R/W
        [Register(Offset=0x00000030)] public uint                           MPMCDynamictRP;        // Selects the precharge command period. 0x0F R/W                  0x0 - 0x0E = n + 1 clock cycles. 0x0F = 16 clock cycles
        [Register(Offset=0x00000034)] public uint                           MPMCDynamictRAS;       // Selects the active to precharge command period. 0xF R/W         0x0 - 0x0E = n + 1 clock cycles. 0x0F = 16 clock cycles
        [Register(Offset=0x00000038)] public uint                           MPMCDynamictSREX;      // Selects the self-refresh exit time. 0xF R/W                     0x0 - 0x7E = n + 1 clock cycles. 0x7F = 128 clock cycles.
        [Register(Offset=0x00000044)] public uint                           MPMCDynamictWR;        // Selects the write recovery time. 0xF R/W                        0x0 - 0x0E = n + 1 clock cycles. 0x0F = 16 clock cycles
        [Register(Offset=0x00000048)] public uint                           MPMCDynamictRC;        // Selects the active to active command period. 0x1F R/W           0x0 - 0x1E = n + 1 clock cycles. 0x1F = 32 clock cycles.
        [Register(Offset=0x0000004C)] public uint                           MPMCDynamictRFC;       // Selects the auto-refresh period. 0x1F R/W                       0x0 - 0x1E = n + 1 clock cycles. 0x1F = 32 clock cycles.
        [Register(Offset=0x00000050)] public uint                           MPMCDynamictXSR;       // Selects the exit self-refresh to active command time 0x1F R/W   0x0 - 0xFE = n + 1 clock cycles. 0xFF = 256 clock cycles.
        [Register(Offset=0x00000054)] public uint                           MPMCDynamictRRD;       // Selects the active bank A to active bank B latency 0xF R/W      0x0 - 0x0E = n + 1 clock cycles. 0x0F = 16 clock cycles
        [Register(Offset=0x00000058)] public uint                           MPMCDynamictMRD;       // Selects the load mode register to active command time 0xF R/W   0x0 - 0x0E = n + 1 clock cycles. 0x0F = 16 clock cycles
        [Register(Offset=0x0000005C)] public uint                           MPMCDynamictCDLR;      // Selects the last data in to read command time. 0xF R/W          0x0 - 0x0E = n + 1 clock cycles. 0x0F = 16 clock cycles
        [Register(Offset=0x00000100)] public MPMCDynamicConfig0_bitfield    MPMCDynamicConfig0;    // Selects the configuration information for the SDRAM. 0 R/W
        [Register(Offset=0x00000104)] public MPMCDynamicRasCas0_bitfield    MPMCDynamicRasCas0;    // Selects the RAS and CAS latencies for the SDRAM. 0x303 R/W
        [Register(Offset=0x00000400)] public Port                           MPMCAHB_Port0;         // AHB port 0.
        [Register(Offset=0x00000440)] public Port                           MPMCAHB_Port2;         // AHB port 2.
        [Register(Offset=0x00000460)] public Port                           MPMCAHB_Port3;         // AHB port 3.
        [Register(Offset=0x00000480)] public Port                           MPMCAHB_Port4;         // AHB port 4.

        //
        // Helper Methods
        //

        [Inline]
        public static unsafe void InitializeLowPowerSDR( uint                                 hclkMHz              ,
                                                         MPMCDynamicConfig0_bitfield.SizeMode sizeDefinition       ,
                                                         bool                                 wideDataBus          ,
                                                         uint                                 modeRegister         ,
                                                         uint                                 extendedModeRegister )
        {
            var valSDRAMCLK_CTRL = new SystemControl.SDRAMCLK_CTRL_bitfield();

            valSDRAMCLK_CTRL.HCLKDELAY_DELAY = 7;

            SystemControl.Instance.SDRAMCLK_CTRL = valSDRAMCLK_CTRL;

            var ctrl = SDRAMController.Instance;

            {
                var val = new SDRAMController.MPMCControl_bitfield();

                val.LowPowerMode            = false;
                val.SDRAM_Controller_Enable = true;

                ctrl.MPMCControl = val;
            }

            {
                var val = new SDRAMController.MPMCConfig_bitfield();

                val.BigEndianMode = false;

                ctrl.MPMCConfig = val;
            }

            {
                var val = new Port.Control_bitfield();

                val.BufferEnable = true;

                ctrl.MPMCAHB_Port0.Control = val;
                ctrl.MPMCAHB_Port2.Control = val;
                ctrl.MPMCAHB_Port3.Control = val;
                ctrl.MPMCAHB_Port4.Control = val;
            }

            {
                var val = new MPMCDynamicConfig0_bitfield();

                val.WriteProtect = false;
                val.Bus32bit     = wideDataBus;
                val.LowPower     = MPMCDynamicConfig0_bitfield.Performance.LowPower;
                val.Size         = sizeDefinition;
                val.MemoryDevice = MPMCDynamicConfig0_bitfield.DeviceKind.LP_SDR;

                ctrl.MPMCDynamicConfig0 = val;
            }

            //--//

            //
            // This value is normally found in SDRAM data sheets as tRCD
            //
            // CAS latency(CAS) -> Three clock cycles
            //
            {
                var val = new MPMCDynamicRasCas0_bitfield();

                val.CAS_Latency = MPMCDynamicRasCas0_bitfield.CasLatency.Latency_3Clocks;

                if(hclkMHz <= 39)
                {
                    val.RAS_Latency = MPMCDynamicRasCas0_bitfield.RasLatency.Latency_1Clock;
                }
                else if(hclkMHz <= 91)
                {
                    val.RAS_Latency = MPMCDynamicRasCas0_bitfield.RasLatency.Latency_2Clock;
                }
                else
                {
                    val.RAS_Latency = MPMCDynamicRasCas0_bitfield.RasLatency.Latency_3Clock;
                }

                ctrl.MPMCDynamicRasCas0 = val;
            }

            {
                var val = new MPMCDynamicReadConfig_bitfield();

                val.SRD = MPMCDynamicReadConfig_bitfield.CommandStrategy.MPMCCLKDELAY;
                val.SRP = true;

                ctrl.MPMCDynamicReadConfig = val;
            }

            //
            // This value is normally found in SDRAM data sheets as tRP
            //
            if(hclkMHz <= 39)
            {
                ctrl.MPMCDynamictRP = 0;
            }
            else if(hclkMHz <= 91)
            {
                ctrl.MPMCDynamictRP = 1;
            }
            else
            {
                ctrl.MPMCDynamictRP = 2;
            }

            //
            // This value is normally found in SDRAM data sheets as tRAS
            //
            if(hclkMHz <= 13)
            {
                ctrl.MPMCDynamictRAS = 0;
            }
            else if(hclkMHz <= 26)
            {
                ctrl.MPMCDynamictRAS = 1;
            }
            else if(hclkMHz <= 52)
            {
                ctrl.MPMCDynamictRAS = 2;
            }
            else if(hclkMHz <= 65)
            {
                ctrl.MPMCDynamictRAS = 3;
            }
            else if(hclkMHz <= 91)
            {
                ctrl.MPMCDynamictRAS = 4;
            }
            else
            {
                ctrl.MPMCDynamictRAS = 5;
            }

            //
            // This value is normally found in SDRAM data sheets as tSREX
            // For devices without this parameter you use the same value as tXSR
            // For some DDR-SDRAM data sheets, this parameter is known as tXSNR
            //
            if(hclkMHz <= 13)
            {
                ctrl.MPMCDynamictSREX = 1;
            }
            else if(hclkMHz <= 26)
            {
                ctrl.MPMCDynamictSREX = 2;
            }
            else if(hclkMHz <= 39)
            {
                ctrl.MPMCDynamictSREX = 3;
            }
            else if(hclkMHz <= 52)
            {
                ctrl.MPMCDynamictSREX = 4;
            }
            else if(hclkMHz <= 65)
            {
                ctrl.MPMCDynamictSREX = 5;
            }
            else if(hclkMHz <= 78)
            {
                ctrl.MPMCDynamictSREX = 6;
            }
            else if(hclkMHz <= 91)
            {
                ctrl.MPMCDynamictSREX = 7;
            }
            else
            {
                ctrl.MPMCDynamictSREX = 8;
            }

            //
            // This value is normally found in SDRAM data sheets as tWR, tDPL, tRWL, or tRDL
            //
            ctrl.MPMCDynamictWR = 1;

            //
            // This value is normally found in SDRAM data sheets as tRC
            //
            if(hclkMHz <= 13)
            {
                ctrl.MPMCDynamictRC = 1;
            }
            else if(hclkMHz <= 26)
            {
                ctrl.MPMCDynamictRC = 2;
            }
            else if(hclkMHz <= 39)
            {
                ctrl.MPMCDynamictRC = 3;
            }
            else if(hclkMHz <= 52)
            {
                ctrl.MPMCDynamictRC = 4;
            }
            else if(hclkMHz <= 65)
            {
                ctrl.MPMCDynamictRC = 5;
            }
            else if(hclkMHz <= 78)
            {
                ctrl.MPMCDynamictRC = 6;
            }
            else if(hclkMHz <= 91)
            {
                ctrl.MPMCDynamictRC = 7;
            }
            else
            {
                ctrl.MPMCDynamictRC = 8;
            }

            //
            // This value is normally found in SDRAM data sheets as tRFC, or sometimes as tRC
            //
            if(hclkMHz <= 13)
            {
                ctrl.MPMCDynamictRFC = 1;
            }
            else if(hclkMHz <= 26)
            {
                ctrl.MPMCDynamictRFC = 2;
            }
            else if(hclkMHz <= 39)
            {
                ctrl.MPMCDynamictRFC = 3;
            }
            else if(hclkMHz <= 52)
            {
                ctrl.MPMCDynamictRFC = 4;
            }
            else if(hclkMHz <= 65)
            {
                ctrl.MPMCDynamictRFC = 5;
            }
            else if(hclkMHz <= 78)
            {
                ctrl.MPMCDynamictRFC = 6;
            }
            else if(hclkMHz <= 91)
            {
                ctrl.MPMCDynamictRFC = 7;
            }
            else
            {
                ctrl.MPMCDynamictRFC = 8;
            }

            //
            // This value is normally found in SDRAM data sheets as tXSR
            // But it is sometimes called tXSNR in some DDR SDRAM data sheets.
            //
            if(hclkMHz <= 91)
            {
                ctrl.MPMCDynamictXSR = 1;
            }
            else if(hclkMHz <= 26)
            {
                ctrl.MPMCDynamictXSR = 2;
            }
            else if(hclkMHz <= 39)
            {
                ctrl.MPMCDynamictXSR = 3;
            }
            else if(hclkMHz <= 52)
            {
                ctrl.MPMCDynamictXSR = 4;
            }
            else if(hclkMHz <= 65)
            {
                ctrl.MPMCDynamictXSR = 5;
            }
            else if(hclkMHz <= 78)
            {
                ctrl.MPMCDynamictXSR = 6;
            }
            else if(hclkMHz <= 91)
            {
                ctrl.MPMCDynamictXSR = 7;
            }
            else
            {
                ctrl.MPMCDynamictXSR = 8;
            }

            // This value is normally found in SDRAM data sheets as tRRD
            if(hclkMHz <= 52)
            {
                ctrl.MPMCDynamictRRD = 0;
            }
            else
            {
                ctrl.MPMCDynamictRRD = 1;
            }

            //
            // This value is normally found in SDRAM data sheets as tMRD, or tRSA
            //
            ctrl.MPMCDynamictMRD = 1;

            //
            // This value is normally found in SDRAM data sheets as tCDLR
            //
            ctrl.MPMCDynamictCDLR = 0;

            //
            // All clock enables are driven HIGH continuously
            // RAM_CLK runs continuously
            //
            {
                var val = new MPMCDynamicControl_bitfield();

                val.CE               = true;
                val.CS               = true;
                val.DDR_nCLK_Disable = true;
                val.Initialization   = MPMCDynamicControl_bitfield.Mode.NOP;

                ctrl.MPMCDynamicControl = val;
            }

            Processor.Delay( 100 * 1000, hclkMHz * 1000000 );

            //
            // All clock enables are driven HIGH continuously
            // RAM_CLK runs continuously
            //
            {
                var val = new MPMCDynamicControl_bitfield();

                val.CE               = true;
                val.CS               = true;
                val.DDR_nCLK_Disable = true;
                val.Initialization   = MPMCDynamicControl_bitfield.Mode.PALL;

                ctrl.MPMCDynamicControl = val;
            }

            // 2 x 16 clocks between SDRAM refresh cycles
            ctrl.MPMCDynamicRefresh = 2;

            Processor.Delay( 2 * 16 * 1000, hclkMHz * 1000000 );

            if(hclkMHz <= 13)
            {
                // 12 x 16 clocks between SDRAM refresh cycles
                ctrl.MPMCDynamicRefresh = 12;
            }
            else if(hclkMHz <= 26)
            {
                // 25 x 16 clocks between SDRAM refresh cycles
                ctrl.MPMCDynamicRefresh = 25;
            }
            else if(hclkMHz <= 39)
            {
                // 35 x 16 clocks between SDRAM refresh cycles
                ctrl.MPMCDynamicRefresh = 35;
            }
            else if(hclkMHz <= 52)
            {
                // 51 x 16 clocks between SDRAM refresh cycles
                ctrl.MPMCDynamicRefresh = 51;
            }
            else if(hclkMHz <= 65)
            {
                // 64 x 16 clocks between SDRAM refresh cycles
                ctrl.MPMCDynamicRefresh = 64;
            }
            else if(hclkMHz <= 78)
            {
                // 77 x 16 clocks between SDRAM refresh cycles
                ctrl.MPMCDynamicRefresh = 77;
            }
            else if(hclkMHz <= 91)
            {
                // 89 x 16 clocks between SDRAM refresh cycles
                ctrl.MPMCDynamicRefresh = 89;
            }
            else
            {
                // 101 x 16 clocks between SDRAM refresh cycles
                ctrl.MPMCDynamicRefresh = 101;
            }

            //--//

            //
            // All clock enables are driven HIGH continuously
            // RAM_CLK runs continuously
            //
            {
                var val = new MPMCDynamicControl_bitfield();

                val.CE               = true;
                val.CS               = true;
                val.DDR_nCLK_Disable = true;
                val.Initialization   = MPMCDynamicControl_bitfield.Mode.MODE;

                ctrl.MPMCDynamicControl = val;
            }

            // Load Mode Register by reading the target location.
            // Refer to Application Note for detail
            SetRegister( modeRegister );

            //
            // All clock enables are driven HIGH continuously
            // RAM_CLK runs continuously
            //
            {
                var val = new MPMCDynamicControl_bitfield();

                val.CE               = true;
                val.CS               = true;
                val.DDR_nCLK_Disable = true;
                val.Initialization   = MPMCDynamicControl_bitfield.Mode.MODE;

                ctrl.MPMCDynamicControl = val;
            }

            // Load Extended Mode Register by reading the target location.
            // Refer to Application Note for detail
            SetRegister( extendedModeRegister );

            // 
            // Clock enable of idle devices are deasserted to save power
            // RAM_CLK stops when all SDRAMs are idle and during self-refresh mode
            //
            {
                var val = new MPMCDynamicControl_bitfield();

                val.CE               = false;
                val.CS               = false;
                val.DDR_nCLK_Disable = true;
                val.Initialization   = MPMCDynamicControl_bitfield.Mode.NORMAL;

                ctrl.MPMCDynamicControl = val;
            }

            //--//

            ctrl.MPMCAHB_Port0.Timeout = 100;
            ctrl.MPMCAHB_Port2.Timeout = 400;
            ctrl.MPMCAHB_Port3.Timeout = 400;
            ctrl.MPMCAHB_Port4.Timeout = 400;
        }

////        uint* mem = (uint*)0x80000000;
////        uint  val = 0xAAAA5555;
////
////        for(uint i = 0; i < 1024; i++)
////        {
////            mem[i] = val + i * 0x01020304;
////        }
////
////        while(true)
////        {
////            for(uint i = 0; i < 1024; i++)
////            {
////                if(mem[i] != val + i * 0x01020304)
////                {
////                    while(true);
////                }
////            }
////        }


        [NoInline]
        [MemoryUsage(MemoryUsage.Bootstrap)]
        private static unsafe uint SetRegister( uint register )
        {
            return *(uint*)(0x80000000u + register);
        }

        //
        // Access Methods
        //

        public static extern SDRAMController Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}