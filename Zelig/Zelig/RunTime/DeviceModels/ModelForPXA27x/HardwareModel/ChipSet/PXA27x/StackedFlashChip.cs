//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    public static class StackedFlashChip
    {
        // the following are the constant for Intel I28F chips
        public const ushort PROGRAM_WORD             = 0x0040;
        public const ushort BUFFERED_PROGRAM_WORD    = 0x00E8;
        public const ushort BUFFERED_PROGRAM_CONFIRM = 0x00D0;

        public const ushort READ_STATUS_REGISTER     = 0x0070;
        public const ushort SR_PROTECT_ERROR         = 0x0002;
        public const ushort SR_VPP_ERROR             = 0x0008;
        public const ushort SR_PROGRAM_ERROR         = 0x0010;
        public const ushort SR_ERASE_ERROR           = 0x0020;
        public const ushort SR_ERASE_SUSPENDED       = 0x0040;
        public const ushort SR_WSM_READY             = 0x0080;

        public const ushort CLEAR_STATUS_REGISTER    = 0x0050;

        public const ushort ENTER_READ_ARRAY_MODE    = 0x00FF;

        public const ushort READ_ID                  = 0x0090;
        public const ushort LOCK_STATUS_LOCKED       = 0x0001;
        public const ushort LOCK_STATUS_LOCKED_DOWN  = 0x0002;

        public const ushort BLOCK_ERASE_SETUP        = 0x0020;

        public const ushort BLOCK_ERASE_CONFIRM      = 0x00D0;

        public const ushort LOCK_SETUP               = 0x0060;
        public const ushort LOCK_LOCK_BLOCK          = 0x0001;
        public const ushort LOCK_UNLOCK_BLOCK        = 0x00D0;
        public const ushort LOCK_LOCK_DOWN_BLOCK     = 0x002F;

        public const ushort CONFIG_SETUP             = 0x0060;       // Command to set up configuration word write
        public const ushort CONFIG_WRITE             = 0x0003;       // Command to perform configuration word write (from LO 16 address lines)

        // Read Configuration Register definitions
        public const ushort FLASH_CONFIG_RESERVED    = 0x0580;       // Bits which must always be set
        public const ushort FLASH_CONFIG_ASYNC       = 0x8000;       // Asynchronous operation
        public const ushort FLASH_CONFIG_SYNC        = 0x0000;       // Synchronous operation
        public const ushort FLASH_CONFIG_LAT_SHIFT   = 11;           // Shift for read latency (2-7 clocks)
        public const ushort FLASH_CONFIG_DATA_HOLD_1 = 0x0000;       // Data hold for 1 clock
        public const ushort FLASH_CONFIG_DATA_HOLD_2 = 0x0200;       // Data hold for 2 clocks
        public const ushort FLASH_CONFIG_CLK_HI_EDGE = 0x0040;       // Clock active on rising edge
        public const ushort FLASH_CONFIG_CLK_LW_EDGE = 0x0000;       // Clock active on falling edge
        public const ushort FLASH_CONFIG_BURST_WRAP  = 0x0000;       // Data burst wraps
        public const ushort FLASH_CONFIG_NO_WRAP     = 0x0008;       // Data burst does not wrap
        public const ushort FLASH_CONFIG_BURST_8     = 0x0002;       // Data burst is 8 words long
        public const ushort FLASH_CONFIG_BURST_16    = 0x0003;       // Data burst is 16 words long

        //
        // Helper Methods
        //

        [DisableNullChecks]
        [DisableBoundsChecks()]
        [NoInline]
        [MemoryRequirements(MemoryAttributes.RAM)]
        public static unsafe ushort InitializeFLASH( uint address, ushort val )
        {
            ushort* flash = (ushort*)new UIntPtr( address ).ToPointer();

            flash[val] = CONFIG_SETUP;
            flash[val] = CONFIG_WRITE;
            flash[0]   = ENTER_READ_ARRAY_MODE;

            return flash[0];
        }

        [DisableNullChecks]
        [DisableBoundsChecks()]
        [NoInline]
        [MemoryUsage(MemoryUsage.Bootstrap)]
        public static unsafe uint InitializeSDRAM( uint address )
        {
            uint* sdram = (uint*)new UIntPtr( address ).ToPointer();

            return sdram[0];
        }

        [DisableNullChecks]
        [DisableBoundsChecks()]
        [NoInline]
        [MemoryRequirements(MemoryAttributes.RAM)]
        public static unsafe void EnterCFI( ushort* sectorStart )
        {
            sectorStart[0x555] = 0x98;
        }

        [DisableNullChecks]
        [DisableBoundsChecks()]
        [NoInline]
        [MemoryRequirements(MemoryAttributes.RAM)]
        public static unsafe void ExitCFI( ushort* sectorStart )
        {
            *sectorStart = ENTER_READ_ARRAY_MODE;
        }

        [DisableNullChecks]
        [DisableBoundsChecks()]
        [NoInline]
        [MemoryRequirements(MemoryAttributes.RAM)]
        public static unsafe void UnlockFlashSector( ushort* sectorStart )
        {
            *sectorStart = LOCK_SETUP;
            *sectorStart = LOCK_UNLOCK_BLOCK;
            *sectorStart = ENTER_READ_ARRAY_MODE;
        }

        [DisableNullChecks]
        [DisableBoundsChecks()]
        [NoInline]
        [MemoryRequirements(MemoryAttributes.RAM)]
        public static unsafe uint EraseFlashSector( ushort* sectorStart )
        {
            //
            // now setup erasing
            //
            *sectorStart = BLOCK_ERASE_SETUP;
            *sectorStart = BLOCK_ERASE_CONFIRM;

            // wait for device to signal completion
            // break when the device signals completion by looking for Data (0xff erasure value) on I/O 7 (a 0 on I/O 7 indicates erasure in progress)
            while((*sectorStart & SR_WSM_READY) != SR_WSM_READY)
            {
            }

            uint result = *sectorStart;

            //
            // error conditions must be cleared
            //
            *sectorStart = CLEAR_STATUS_REGISTER;
            *sectorStart = ENTER_READ_ARRAY_MODE;

            return result;
        }

        [DisableNullChecks]
        [DisableBoundsChecks()]
        [NoInline]
        [MemoryRequirements(MemoryAttributes.RAM)]
        public static unsafe uint ProgramFlashWord( ushort* address ,
                                                    ushort  value   )
        {
            *address = PROGRAM_WORD;
            *address = value;

            //
            // Wait for device to signal completion
            // break when the device signals completion by looking for Data (0xff erasure value) on I/O 7 (a 0 on I/O 7 indicates erasure in progress)
            //
            while((*address & SR_WSM_READY) != SR_WSM_READY);

            uint result = *address;

            //
            // Exit Status Read Mode.
            //
            *address = CLEAR_STATUS_REGISTER;
            *address = ENTER_READ_ARRAY_MODE;

            return result;
        }

        [DisableNullChecks]
        [DisableBoundsChecks()]
        [NoInline]
        [MemoryRequirements(MemoryAttributes.RAM)]
        public static unsafe void StartBufferedProgramFlashWord( ushort* address ,
                                                                 int     count   )
        {
            *address = BUFFERED_PROGRAM_WORD;
            *address = (ushort)(count - 1);
        }

        [DisableNullChecks]
        [DisableBoundsChecks()]
        [NoInline]
        [MemoryRequirements(MemoryAttributes.RAM)]
        public static unsafe void AddBufferedProgramFlashWord( ushort* address ,
                                                               ushort  value   )
        {
            *address = value;
        }

        [DisableNullChecks]
        [DisableBoundsChecks()]
        [NoInline]
        [MemoryRequirements(MemoryAttributes.RAM)]
        public static unsafe uint ConfirmProgramFlashWord( ushort* address )
        {
            *address = BUFFERED_PROGRAM_CONFIRM;

            //
            // Wait for device to signal completion
            // break when the device signals completion by looking for Data (0xff erasure value) on I/O 7 (a 0 on I/O 7 indicates erasure in progress)
            //
            while((*address & SR_WSM_READY) != SR_WSM_READY);

            uint result = *address;

            //
            // Exit Status Read Mode.
            //
            *address = CLEAR_STATUS_REGISTER;
            *address = ENTER_READ_ARRAY_MODE;

            return result;
        }
    }
}