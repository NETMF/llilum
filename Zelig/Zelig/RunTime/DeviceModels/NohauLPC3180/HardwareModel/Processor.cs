//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.NohauLPC3180
{
    using System;

    using RT           = Microsoft.Zelig.Runtime;
    using TS           = Microsoft.Zelig.Runtime.TypeSystem;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.LPC3180;


    public sealed class Processor : ChipsetModel.Processor
    {
        const uint c_IRAM__BaseAddress         = 0x08000000;
        const uint c_IRAM__Size                = 64 * 1024;
                                       
        const uint c_DRAM__BaseAddress         = 0x80000000;
        const uint c_DRAM__Size                = 32 * 1024 * 1024;

        const uint c_Peripherals1__BaseAddress = 0x20000000;
        const uint c_Peripherals1__EndAddress  = 0x200C0000;

        const uint c_Peripherals2__BaseAddress = 0x30000000;
        const uint c_Peripherals2__EndAddress  = 0x32000000;

        const uint c_Peripherals3__BaseAddress = 0x40000000;
        const uint c_Peripherals3__EndAddress  = 0x40100000;

        [RT.AlignmentRequirements( RT.TargetPlatform.ARMv4.MMUv4.c_TLB_SecondLevelSize, sizeof(uint) )]
        static uint[] s_TLB_SecondLevel_IRAM = new uint[RT.TargetPlatform.ARMv4.MMUv4.c_TLB_SecondLevelSlots];

        //
        // Helper Methods
        //

        public override void InitializeProcessor()
        {
            base.InitializeProcessor();

            ConfigureMMU();
        }

        private static void ConfigureMMU()
        {
            RT.TargetPlatform.ARMv4.MMUv4.ClearTLB();

            RT.TargetPlatform.ARMv4.MMUv4.AddCoarsePages       (                   0,                   0 + c_IRAM__Size                     , s_TLB_SecondLevel_IRAM );
            RT.TargetPlatform.ARMv4.MMUv4.AddCoarsePages       ( c_IRAM__BaseAddress, c_IRAM__BaseAddress + c_IRAM__Size                     , s_TLB_SecondLevel_IRAM );
            RT.TargetPlatform.ARMv4.MMUv4.AddCacheableSection  ( c_DRAM__BaseAddress, c_DRAM__BaseAddress + c_DRAM__Size, c_DRAM__BaseAddress                         );

            RT.TargetPlatform.ARMv4.MMUv4.AddUncacheableSection( c_Peripherals1__BaseAddress, c_Peripherals1__EndAddress, c_Peripherals1__BaseAddress );
            RT.TargetPlatform.ARMv4.MMUv4.AddUncacheableSection( c_Peripherals2__BaseAddress, c_Peripherals2__EndAddress, c_Peripherals2__BaseAddress );
            RT.TargetPlatform.ARMv4.MMUv4.AddUncacheableSection( c_Peripherals3__BaseAddress, c_Peripherals3__EndAddress, c_Peripherals3__BaseAddress );

            RT.TargetPlatform.ARMv4.MMUv4.AddCacheableCoarsePages( 0, 0 + c_IRAM__Size, c_IRAM__BaseAddress, s_TLB_SecondLevel_IRAM );

            RT.TargetPlatform.ARMv4.MMUv4.EnableTLB();
        }

        //--//

        [RT.BottomOfCallStack()]
        [RT.HardwareExceptionHandler(RT.HardwareException.Interrupt)]
        private static void InterruptHandler( ref Context.RegistersOnStack registers )
        {
            Context.InterruptHandlerWithContextSwitch( ref registers );
        }

        [RT.BottomOfCallStack()]
        [RT.HardwareExceptionHandler(RT.HardwareException.FastInterrupt)]
        [RT.MemoryRequirements( RT.MemoryAttributes.RAM )]
        private static void FastInterruptHandler()
        {
            Context.FastInterruptHandlerWithoutContextSwitch();
        }

        [RT.BottomOfCallStack()]
        [RT.HardwareExceptionHandler(RT.HardwareException.SoftwareInterrupt)]
        private static void SoftwareInterruptHandler( ref Context.RegistersOnStack registers )
        {
            Context.GenericSoftwareInterruptHandler( ref registers );
        }
    }
}
