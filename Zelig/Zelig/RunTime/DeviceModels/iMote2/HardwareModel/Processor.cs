//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.iMote2
{
    using System;

    using RT           = Microsoft.Zelig.Runtime;
    using TS           = Microsoft.Zelig.Runtime.TypeSystem;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.PXA27x;
    using ARMv4        = Microsoft.Zelig.Runtime.TargetPlatform.ARMv4;


    public sealed class Processor : ChipsetModel.Processor
    {
        const uint c_IRAM__BaseAddress         = 0x5C000000;
        const uint c_IRAM__Size                = 256 * 1024;

        const uint c_FLASH__BaseAddress        = 0x02000000;
        const uint c_FLASH__Size               = 32 * 1024 * 1024;
                                       
        const uint c_DRAM__BaseAddress         = 0xA0000000;
        const uint c_DRAM__Size                = 32 * 1024 * 1024;

        const uint c_Peripherals__BaseAddress  = 0x40000000;
        const uint c_Peripherals__EndAddress   = 0x54000000;

        [RT.AlignmentRequirements( RT.TargetPlatform.ARMv4.MMUv4.c_TLB_SecondLevelSize, sizeof(uint) )]
        static uint[] s_TLB_SecondLevel_Vectors = new uint[RT.TargetPlatform.ARMv4.MMUv4.c_TLB_SecondLevelSlots];

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

            RT.TargetPlatform.ARMv4.MMUv4.AddCacheableSection  ( c_IRAM__BaseAddress       , c_IRAM__BaseAddress  + c_IRAM__Size , c_IRAM__BaseAddress        );
            RT.TargetPlatform.ARMv4.MMUv4.AddCacheableSection  ( c_FLASH__BaseAddress      , c_FLASH__BaseAddress + c_FLASH__Size, 0                          );
            RT.TargetPlatform.ARMv4.MMUv4.AddCacheableSection  ( c_DRAM__BaseAddress       , c_DRAM__BaseAddress  + c_DRAM__Size , c_DRAM__BaseAddress        );
            RT.TargetPlatform.ARMv4.MMUv4.AddUncacheableSection( c_Peripherals__BaseAddress, c_Peripherals__EndAddress           , c_Peripherals__BaseAddress );

            RT.TargetPlatform.ARMv4.MMUv4.AddCoarsePages         ( 0x00000000, 0x00100000,    s_TLB_SecondLevel_Vectors );
            RT.TargetPlatform.ARMv4.MMUv4.AddCacheableCoarsePages( 0x00000000, 0x00001000, 0, s_TLB_SecondLevel_Vectors );

////        RT.TargetPlatform.ARMv4.MMUv4.AddCoarsePages         ( 0xFFF00000, 0xFFFFFFFF,    s_TLB_SecondLevel_Vectors );
////        RT.TargetPlatform.ARMv4.MMUv4.AddCacheableCoarsePages( 0xFFFF0000, 0xFFFF1000, 0, s_TLB_SecondLevel_Vectors );

            RT.TargetPlatform.ARMv4.MMUv4.EnableTLB();
        }

        //--//

        [RT.BottomOfCallStack()]
        [RT.HardwareExceptionHandler(RT.HardwareException.Interrupt)]
        private static void InterruptHandler( ref Context.RegistersOnStack registers )
        {
            s_repeatedAbort = false;
            Context.InterruptHandlerWithContextSwitch( ref registers );
        }

        [RT.BottomOfCallStack()]
        [RT.HardwareExceptionHandler(RT.HardwareException.FastInterrupt)]
        [RT.MemoryRequirements( RT.MemoryAttributes.RAM )]
        private static void FastInterruptHandler()
        {
            s_repeatedAbort = false;
            Context.FastInterruptHandlerWithoutContextSwitch();
        }

        [RT.BottomOfCallStack()]
        [RT.HardwareExceptionHandler(RT.HardwareException.SoftwareInterrupt)]
        private static void SoftwareInterruptHandler( ref Context.RegistersOnStack registers )
        {
            s_repeatedAbort = false;
            Context.GenericSoftwareInterruptHandler( ref registers );
        }

        //--//

        static uint fault_DFSR;
        static uint fault_IFSR;
        static uint fault_FAR;

        [RT.NoInline]
        [RT.NoReturn()]
        [RT.HardwareExceptionHandler(RT.HardwareException.UndefinedInstruction)]
        [RT.MemoryUsage(RT.MemoryUsage.Bootstrap)]
        static void UndefinedInstruction()
        {
            fault_DFSR = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 5, 0, 0 );
            fault_IFSR = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 5, 0, 1 );
            fault_FAR  = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 6, 0, 0 );

            Processor.Instance.Breakpoint();
        }

        [RT.NoInline]
        [RT.NoReturn()]
        [RT.HardwareExceptionHandler(RT.HardwareException.PrefetchAbort)]
        [RT.MemoryUsage(RT.MemoryUsage.Bootstrap)]
        static void PrefetchAbort()
        {
            fault_DFSR = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 5, 0, 0 );
            fault_IFSR = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 5, 0, 1 );
            fault_FAR  = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 6, 0, 0 );

            Processor.Instance.Breakpoint();
        }


        private static bool s_repeatedAbort = false;
        private static int s_abortCount = 0;

        [RT.NoInline]
        [RT.NoReturn()]
        [RT.HardwareExceptionHandler(RT.HardwareException.DataAbort)]
        [RT.MemoryUsage(RT.MemoryUsage.Bootstrap)]
        static void DataAbort()
        {
            fault_DFSR = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 5, 0, 0 );
            fault_IFSR = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 5, 0, 1 );
            fault_FAR  = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 6, 0, 0 );

            bool repeatedAbort = s_repeatedAbort;
            s_repeatedAbort = true;
            s_abortCount++;
            if (repeatedAbort)
            Processor.Instance.Breakpoint();
        }

    }
}
