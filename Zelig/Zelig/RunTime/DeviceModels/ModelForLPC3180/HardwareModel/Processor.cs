//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.LPC3180
{
    using System;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class Processor : RT.TargetPlatform.ARMv5.ProcessorARMv5_VFP
    {
        public static class Configuration
        {
            public const uint SYSCLK = 13000000;

            public static ulong CoreClockFrequency
            {
                [RT.ConfigurationOption("System__CoreClockFrequency")]
                get
                {
                    return 0;
                }
            }

            public static ulong AHBClockFrequency
            {
                [RT.ConfigurationOption("LPC3180__AHBClockFrequency")]
                get
                {
                    return 0;
                }
            }

            public static ulong PeripheralsClockFrequency
            {
                [RT.ConfigurationOption("System__PeripheralsClockFrequency")]
                get
                {
                    return 0;
                }
            }

            public static ulong RealTimeClockFrequency
            {
                [RT.ConfigurationOption("System__RealTimeClockFrequency")]
                get
                {
                    return 0;
                }
            }
            
            public static uint DefaultThreadPooThreads
            {
                [RT.ConfigurationOption( "System__Runtime_DefaultThreadPooThreads" )]
                get
                {
                    return Int32.MaxValue;
                }
            }

            public static uint DefaultTimerPooThreads
            {
                [RT.ConfigurationOption( "System__Runtime_DefaultTimerPooThreads" )]
                get
                {
                    return 2;
                }
            }

            public static bool Use32BitBus
            {
                [RT.ConfigurationOption("LPC3180__DRAMWideBus")]
                get
                {
                    return true;
                }
            }
        }

        public sealed new class Context : RT.TargetPlatform.ARMv5.ProcessorARMv5_VFP.Context
        {
            //
            // Constructor Methods
            //

            //
            // Helper Methods
            //

            //
            // Access Methods
            //

        }

        //
        // Helper Methods
        //

        public override void InitializeProcessor()
        {
            EnableRunFastMode();
        }

        public override UIntPtr GetCacheableAddress( UIntPtr ptr )
        {
            return ptr;
        }

        public override UIntPtr GetUncacheableAddress( UIntPtr ptr )
        {
            return ptr;
        }

        //--//

        public static void PreInitializeProcessor( uint SYSCLK     ,
                                                   uint ARM_CLK    ,
                                                   uint HCLK       ,
                                                   uint PERIPH_CLK )
        {
            InitializeClocks( SYSCLK, ARM_CLK, HCLK, PERIPH_CLK );

            InitializeCache();
        }

        private static void InitializeClocks( uint SYSCLK     ,
                                              uint ARM_CLK    ,
                                              uint HCLK       ,
                                              uint PERIPH_CLK )
        {
            var ctrl = Chipset.LPC3180.SystemControl.Instance;

            ctrl.SwitchClockToRTCOscillator();

            ctrl.EnableAutoSwitchToLowVoltageOnStopMode();

            ctrl.ConfigureClocks( SYSCLK, ARM_CLK, HCLK, PERIPH_CLK );

            ctrl.SwitchToRunMode();
        }

        //--//

        [RT.Inline]
        public override Microsoft.Zelig.Runtime.Processor.Context AllocateProcessorContext()
        {
            return new Context();
        }

        //--//

        [RT.NoInline]
        [RT.BottomOfCallStack]
        [RT.MemoryUsage( RT.MemoryUsage.Bootstrap )]
        [RT.DebuggerHookHandler( RT.DebuggerHook.FlushInstructionCache )]
        static void FlushICache()
        {
            RT.TargetPlatform.ARMv4.Coprocessor15.TestAndCleanDCache();
            RT.TargetPlatform.ARMv4.Coprocessor15.DrainWriteBuffer  ();
            RT.TargetPlatform.ARMv4.Coprocessor15.InvalidateICache  ();

            Processor.Instance.Breakpoint();
        }

        [RT.NoInline]
        [RT.BottomOfCallStack]
        [RT.SaveFullProcessorContext]
        [RT.MemoryUsage( RT.MemoryUsage.Bootstrap )]
        [RT.DebuggerHookHandler( RT.DebuggerHook.GetFullProcessorContext )]
        static void GetFullProcessorContext()
        {
            Processor.Instance.Breakpoint();
        }
    }
}
