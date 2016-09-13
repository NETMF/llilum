//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define THREADING_RTOS

namespace Microsoft.CortexM4OnMBED
{
    using System;
    
    using RT            = Microsoft.Zelig.Runtime;
    using RTOS          = Microsoft.Zelig.Support.mbed;
    using ChipsetModel  = Microsoft.CortexM4OnCMSISCore;
    

    public abstract class Processor : ChipsetModel.Processor 
    {

        public new abstract class Context : ChipsetModel.Processor.Context
        {
#if THREADING_RTOS
            //
            // State
            //
            protected UIntPtr m_nativeContext;
#endif

            //--//

            public Context(RT.ThreadImpl owner) : base(owner)
            {
            }

            //
            // Extensibility 
            //
#if THREADING_RTOS
            public override void SwitchTo()
            {
                //
                // When running on a RTOS, we will request the underlying system to choose the next thread
                // based on our indications. Need to solve priority and lock contention issues, e.g. priority inversion
                // issues. 
                //
         
                RTOS.Threading.SwitchToContext( m_nativeContext );
            }
#endif

            //
            // RTOS Extensibility
            // 

            protected override UIntPtr CreateNativeContext( UIntPtr entryPoint, UIntPtr stack, int stackSize )
            {
                return RTOS.Threading.CreateNativeContext( entryPoint, stack, stackSize );
            }

            protected override void SwitchToContext( UIntPtr nativeContext )
            {
                RTOS.Threading.SwitchToContext( nativeContext );
            }
            
            protected override void Yield( UIntPtr nativeContext )
            {
                RTOS.Threading.Yield( nativeContext );
            }
        
            protected override void Retire( UIntPtr nativeContext )
            {
                RTOS.Threading.Retire( nativeContext );
            }
        }

        //--//

#if THREADING_RTOS
        [RT.Inline]
        public override Microsoft.Zelig.Runtime.Processor.Context AllocateProcessorContext()
        {
            return new Context();
        }        
#endif
        
        //
        // Access methods
        //
        
        public static ulong CoreClockFrequency
        {
            [RT.ConfigurationOption("System__CoreClockFrequency")]
            get
            {
                RT.BugCheck.Assert( false, RT.BugCheck.StopCode.IllegalConfiguration );
                return 0;
            }
        }

        public static ulong RealTimeClockFrequency
        {
            [RT.ConfigurationOption("System__RealTimeClockFrequency")]
            get
            { 
                RT.BugCheck.Assert( false, RT.BugCheck.StopCode.IllegalConfiguration );
                return 0;
            }
        }
            
        public static uint DefaultThreadPoolThreads
        {
            [RT.ConfigurationOption( "System__Runtime_DefaultThreadPoolThreads" )]
            get
            {
                RT.BugCheck.Assert( false, RT.BugCheck.StopCode.IllegalConfiguration );
                return 3;
            }
        }

        public static uint DefaultTimerPooThreads
        {
            [RT.ConfigurationOption( "System__Runtime_DefaultTimerPooThreads" )]
            get
            {
                RT.BugCheck.Assert( false, RT.BugCheck.StopCode.IllegalConfiguration );
                return 2;
            }
        }
        
        
        //--//

        //////[RT.BottomOfCallStack()]
        //////[RT.HardwareExceptionHandler(RT.HardwareException.Interrupt)]
        //////private static void InterruptHandler( UIntPtr stackPtr )
        //////{
        //////    s_repeatedAbort = false;
        //////    Context.InterruptHandlerWithContextSwitch( ref stackPtr );
        //////}

        //////[RT.BottomOfCallStack()]
        //////[RT.HardwareExceptionHandler(RT.HardwareException.FastInterrupt)]
        //////[RT.MemoryRequirements( RT.MemoryAttributes.RAM )]
        //////private static void FastInterruptHandler()
        //////{
        //////    s_repeatedAbort = false;
        //////    Context.FastInterruptHandlerWithoutContextSwitch();
        //////}

        //////[RT.BottomOfCallStack()]
        //////[RT.HardwareExceptionHandler(RT.HardwareException.SoftwareInterrupt)]
        //////private static void SoftwareInterruptHandler( ref Context.RegistersOnStackNoFPContext registers )
        //////{
        //////    s_repeatedAbort = false;
        //////    Context.GenericSoftwareInterruptHandler( ref registers );
        //////}

        //--//
        
        [RT.NoInline]
        [RT.NoReturn()]
        [RT.HardwareExceptionHandler(RT.HardwareException.UndefinedInstruction)]
        [RT.MemoryUsage(RT.MemoryUsage.Bootstrap)]
        static void UndefinedInstruction()
        {
            //////RT.Processor.Instance.Breakpoint();
            while(true) { }
        }

        [RT.NoInline]
        [RT.NoReturn()]
        [RT.HardwareExceptionHandler(RT.HardwareException.PrefetchAbort)]
        [RT.MemoryUsage(RT.MemoryUsage.Bootstrap)]
        static void PrefetchAbort()
        {
            //////RT.Processor.Instance.Breakpoint();
            while(true) { }
        }


        private static bool s_repeatedAbort = false;
        private static int s_abortCount = 0;

        [RT.NoInline]
        [RT.NoReturn()]
        [RT.HardwareExceptionHandler(RT.HardwareException.DataAbort)]
        [RT.MemoryUsage(RT.MemoryUsage.Bootstrap)]
        static void DataAbort()
        {
            //////bool repeatedAbort = s_repeatedAbort;
            //////s_repeatedAbort = true;
            //////s_abortCount++;
            //////if (repeatedAbort)
            //////RT.Processor.Instance.Breakpoint();
            while(true) { }
        }
    }
}
