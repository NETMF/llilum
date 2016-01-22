//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x
{
    using System;

    using RT    = Microsoft.Zelig.Runtime;
    using TS    = Microsoft.Zelig.Runtime.TypeSystem;
    using ARMv4 = Microsoft.Zelig.Runtime.TargetPlatform.ARMv4;


    public abstract class Processor : RT.TargetPlatform.ARMv4.ProcessorARMv4
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

            public static uint DefaultThreadPoolThreads
            {
                [RT.ConfigurationOption( "System__Runtime_DefaultThreadPoolThreads" )]
                get
                {
                    return Int32.MaxValue;
                }
            }

            public static uint DefaultTimerPoolThreads
            {
                [RT.ConfigurationOption( "System__Runtime_DefaultTimerPooThreads" )]
                get
                {
                    return 2;
                }
            }
        }

        public sealed new class Context : RT.TargetPlatform.ARMv4.ProcessorARMv4.Context
        {
            //
            // Constructor Methods
            //

            public Context(RT.ThreadImpl owner) : base(owner)
            {
            }


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
            Chipset.PXA27x.MemoryController.Instance.InitializeStackedFLASH();
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

        public static void EnableCaches()
        {
            ARMv4.Coprocessor15.InvalidateICache();
    
            ARMv4.Coprocessor15.InvalidateDCache();
    
            //
            // Enable ICache/DCache/Branch Target Buffer
            //
            ARMv4.Coprocessor15.SetControlRegisterBits( ARMv4.Coprocessor15.c_ControlRegister__ICache |
                                                        ARMv4.Coprocessor15.c_ControlRegister__DCache |
                                                        ARMv4.Coprocessor15.c_ControlRegister__BTB    );

////        //
////        // Enable ICache/DCache/Branch Target Buffer
////        //
////        // Relocate exception vector table.
////        //
////        ARMv4.Coprocessor15.SetControlRegisterBits( ARMv4.Coprocessor15.c_ControlRegister__ICache |
////                                                    ARMv4.Coprocessor15.c_ControlRegister__DCache |
////                                                    ARMv4.Coprocessor15.c_ControlRegister__BTB    |
////                                                    ARMv4.Coprocessor15.c_ControlRegister__Vector );
        }

        //--//

        [RT.Inline]
        public override RT.Processor.Context AllocateProcessorContext(RT.ThreadImpl owner)
        {
            return new Context(owner);
        }

        //--//

        [RT.NoInline]
        [RT.BottomOfCallStack]
        [RT.MemoryUsage( RT.MemoryUsage.Bootstrap )]
        [RT.DebuggerHookHandler( RT.DebuggerHook.FlushInstructionCache )]
        static void FlushICache()
        {
////        RT.TargetPlatform.ARMv4.Coprocessor15.TestAndCleanDCache();
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
