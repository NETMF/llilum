//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.NohauLPC3180
{
    using System;
    using System.Runtime.CompilerServices;

    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.LPC3180;


    public sealed class Device : RT.Device
    {
        const uint c_PageCount       = 65536;
        const uint c_PageSize        = 512;
        const uint c_SpareSize       = 16;
        const uint c_UsableSpareSize = 6;
        const uint c_BlockSize       = 16384;
        const uint c_ChipSize        = c_PageCount * c_PageSize;

        static readonly uint  [] s_Page  = new uint  [c_PageSize  / sizeof(uint  )];
        static readonly ushort[] s_Spare = new ushort[c_SpareSize / sizeof(ushort)];

        //--//

        public override void PreInitializeProcessorAndMemory()
        {
            //
            // Enter System mode, with interrupts disabled.
            //
            Processor.SetStatusRegister( Processor.c_psr_field_c, Processor.c_psr_I | Processor.c_psr_F | Processor.c_psr_mode_SYS );

            Processor.SetRegister( Processor.Context.RegistersOnStack.StackRegister, this.BootstrapStackPointer );

            Processor.PreInitializeProcessor( Processor.Configuration.SYSCLK, (uint)Processor.Configuration.CoreClockFrequency, (uint)Processor.Configuration.AHBClockFrequency, (uint)Processor.Configuration.PeripheralsClockFrequency );
        }

        public override void MoveCodeToProperLocation()
        {
            uint HCLK = (uint)Processor.Configuration.AHBClockFrequency;

            InitializeNANDController( HCLK );

            InitializeDRAMController( HCLK );

            ExecuteImageRelocation();
        }

        private void InitializeNANDController( uint hclkMHz )
        {
            var flashclk = new ChipsetModel.SystemControl.FLASHCLK_CTRL_bitfield();
 
            flashclk.InterruptFromMLC = true;
            flashclk.MLC_ClockEnable  = true;

            ChipsetModel.SystemControl.Instance.FLASHCLK_CTRL = flashclk;

            var ctrl = ChipsetModel.MultiLevelNANDController.Instance;

            ctrl.SetTimingRegister( hclkMHz, 45, 100, 30, 15, 30, 20, 40 );

            ctrl.MLC_CMD = 0xFF;
    
            while(ctrl.MLC_ISR.NandReady == false)
            {
            }

            {
                var val = new  ChipsetModel.MultiLevelNANDController.MLC_ICR_bitfield();

                val.ChipWordAddress = ChipsetModel.MultiLevelNANDController.MLC_ICR_bitfield.Addressing.ThreeWords;

                ctrl.MLC_LOCK_PR = ChipsetModel.MultiLevelNANDController.MLC_LOCK_PR__UnlockValue;
                ctrl.MLC_ICR     = val;
            }
        }

        private void InitializeDRAMController( uint hclkMHz )
        {
            if(Processor.Configuration.Use32BitBus)
            {
                // Address mapping(AM) -> 32-bit external bus low-power SDRAM address mapping : 
                //                        128Mb (8Mx16), 4 banks, row length = 12, column length = 9 => Cfg Register values should be shifted left by 11 bits.
                ChipsetModel.SDRAMController.InitializeLowPowerSDR( hclkMHz, ChipsetModel.SDRAMController.MPMCDynamicConfig0_bitfield.SizeMode.Size_128Mb_8Mx16, true, 0x30u << 11, 0x2058u << 11 );
            }
            else
            {
                // Address mapping(AM) -> 16-bit external bus low-power SDRAM address mapping : 
                //                        128Mb (8Mx16), 4 banks, row length = 12, column length = 9 => Cfg Register values should be shifted left by 10 bits.
                ChipsetModel.SDRAMController.InitializeLowPowerSDR( hclkMHz, ChipsetModel.SDRAMController.MPMCDynamicConfig0_bitfield.SizeMode.Size_128Mb_8Mx16, false, 0x31u << 10, 0x2058u << 10 );
            }
        }

        [RT.DisableNullChecks( ApplyRecursively=true )]
        [RT.DisableBoundsChecks( ApplyRecursively=true )]
        private unsafe void ExecuteImageRelocation()
        {
            Memory.RelocationInfo[] array = Memory.Instance.RelocationData;

            for(int i = 0; i < array.Length; i++)
            {
                Memory.RelocationInfo ri  = array[i];
                uint*                 dst = (uint*)ri.Destination.ToPointer();

                if(ri.IsEraseBlock)
                {
                    uint count = ri.SizeInWords;

                    while(count != 0)
                    {
                        *dst++ = 0;
                        count--;
                    }
                }
                else
                {
                    uint start     = ri.Start.ToUInt32();
                    uint end       = ri.End  .ToUInt32();
                    uint skipCount = 0;

                    while(start < end)
                    {
                        uint address                =  start & ~(c_PageSize - 1);
                        uint offset                 = (start - address);
                        uint expectedLogicalAddress =  address & (c_ChipSize - 1);

                        skipCount = ReadLogicalPage( expectedLogicalAddress, skipCount );

                        fixed(uint* ptr = &s_Page[0])
                        {
                            uint  count  = Math.Min( (c_PageSize - offset), (end - start) );
                            uint* src    = ptr + offset / sizeof(uint);
                            uint* srcEnd = src + count  / sizeof(uint);

                            while(src < srcEnd)
                            {
                                *dst++  = *src++;
                            }

                            start += count;
                        }
                    }
                }
            }
        }

        [RT.NoInline]
        [RT.MemoryUsage(RT.MemoryUsage.Bootstrap)]
        [RT.DisableNullChecks( ApplyRecursively=true )]
        [RT.DisableBoundsChecks( ApplyRecursively=true )]
        private static unsafe uint ReadLogicalPage( uint expectedLogicalAddress ,
                                                    uint skipCount              )
        {
            while(true)
            {
                uint physicalAddress = expectedLogicalAddress + skipCount * c_BlockSize;

                if(ReadPage( physicalAddress ) == false)
                {
                    //
                    // Bad CRC, try skipping a block.
                    //
                    skipCount++;
                    continue;
                }

                //
                // The first 32bit word of the spare area is the logical address of the page.
                //
                uint logicalAddress = ((uint)s_Spare[0]      ) |
                                      ((uint)s_Spare[1] << 16) ;

                if(expectedLogicalAddress != logicalAddress)
                {
                    if(physicalAddress < logicalAddress)
                    {
                        //
                        // We always move blocks forward, so the physical address will always be higher than the logical one.
                        //
                        RT.BugCheck.Raise( RT.BugCheck.StopCode.FailedBootstrap );
                    }

                    //
                    // Move forward and try again.
                    //
                    skipCount++;
                    continue;
                }

                return skipCount;
            }
        }

        [RT.DisableNullChecks()]
        [RT.MemoryUsage(RT.MemoryUsage.Bootstrap)]
        public static unsafe bool ReadPage( uint address )
        {
            var ctrl = ChipsetModel.MultiLevelNANDController.Instance;

            uint pageAddress = address / 512;

            ctrl.MLC_CMD              = 0x00; // Select Page A
            ctrl.MLC_ADDR             = 0;
            ctrl.MLC_ADDR             = (pageAddress >>  0) & 0xFF;
            ctrl.MLC_ADDR             = (pageAddress >>  8) & 0xFF;
////        ctrl.MLC_ADDR             = (pageAddress >> 16) & 0xFF; // Used for 4-word address chips.
            ctrl.MLC_ECC_AUTO_DEC_REG = 0;

            //--//

            while(true)
            {
                var val = ctrl.MLC_ISR;

                if(val.ControllerReady == false)
                {
                    continue;
                }

                if(val.DecoderFailure)
                {
                    for(int i = 0; i < s_Page.Length; i++)
                    {
                        s_Page[i] = ctrl.MLC_BUFF_32bits[0];
                    }

                    for(int i = 0; i < s_Spare.Length; i++)
                    {
                        s_Spare[i] = ctrl.MLC_BUFF_16bits[0];
                    }

                    return false;
                }

                break;
            }

            for(int i = 0; i < s_Page.Length; i++)
            {
                s_Page[i] = ctrl.MLC_BUFF_32bits[0];
            }

            for(int i = 0; i < s_Spare.Length; i++)
            {
                s_Spare[i] = ctrl.MLC_BUFF_16bits[0];
            }

            return true;
        }
    }
}
