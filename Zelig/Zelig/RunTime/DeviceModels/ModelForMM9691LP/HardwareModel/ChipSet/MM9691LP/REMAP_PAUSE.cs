//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x38010000U,Length=0x0000005CU)]
    public class REMAP_PAUSE
    {
        public const uint CacheableAddressMask            = 0x80000000;
        public const uint CacheFlushAddressMask           = 0x40000000;

        public const uint ResetStatus__POR                = 0x00000001;

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct SystemConfigurationBitField
        {
            [BitFieldRegister(Position=0)] public bool SSPEN     ; // Short Circuit Protection for USB Transceiver.
            [BitFieldRegister(Position=1)] public bool SW_FRZ_EN ; // Software Freeze Enable.
            [BitFieldRegister(Position=2)] public bool SW_FRZ    ; // Software Freeze.
            [BitFieldRegister(Position=3)] public bool PU_DIS    ; // Pullup Disable.
            [BitFieldRegister(Position=4)] public bool PLL_TST_EN; // PLL Test Enable.
        }

        //--//

        [Register(Offset=0x00000000U)] public uint                        Pause_AHB;
        [Register(Offset=0x00000004U)] public uint                        Pause_CPU;
        [Register(Offset=0x00000010U)] public uint                        Identification;
        [Register(Offset=0x00000020U)] public uint                        ClearResetMap;
        [Register(Offset=0x00000030U)] public uint                        ResetStatus;
        [Register(Offset=0x00000034U)] public uint                        ResetStatusClear;
        [Register(Offset=0x00000040U)] public SystemConfigurationBitField SystemConfiguration;

        [Register(Offset=0x00000050U)] public uint                        Cache_Enable;
        [Register(Offset=0x00000054U)] public uint                        Cache_Tags_Reset;
        [Register(Offset=0x00000058U)] public uint                        Cache_Flush_Enable;

        //
        // Helper Methods
        //

        [Inline]
        [DisableNullChecks]
        public unsafe void InitializeCache()
        {
            //
            // cache initialization (see 2.2.12 usage guidelines pp 68)
            //
            this.Cache_Tags_Reset = 0;

            byte* tagPtr = (byte*)0;
            for(int i = 0; i <= 256; i++, tagPtr += 1 << 4)
            {
                *tagPtr = 0;
            }

            this.Cache_Tags_Reset   = 1;
            this.Cache_Enable       = 1; 
            this.Cache_Flush_Enable = 1;
        }

        //
        // Access Methods
        //

        public static extern REMAP_PAUSE Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}