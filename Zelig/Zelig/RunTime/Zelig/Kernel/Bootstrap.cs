//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

// Enables basic low-level self-tests for arithmetic and method calls.
//#define SELF_TEST_BASIC

// Enables memory self-tests to validate garbage collection techniques.
//#define SELF_TEST_MEMORY

// Enables self-tests covering null/bounds/overflow checks.
//#define SELF_TEST_CHECKS

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    // 
    // During bootstrap there's the problem of where to put the machine stack and what to do with it afterwards.
    // Instead of statically allocating the stack, we change the memory initialization order.
    // 
    // 1) All the available memory is assigned to the heap manager. This includes also the memory currently used for the stack.
    // 2) The heap is constructed, but the memory is not cleared.
    // 3) An array of words is allocated. This overlaps the current stack, by virtue of the way the memory is allocated
    //    (from the top of the memory, instead of from the bottom).
    //    [We wrap all free memory into a managed object (a uint[]), an allocation that is less than the full free block is
    //     just a decrement of the array length. This is cheaper than moving the free block and relinking it].
    // 4) All free memory is cleared, so that future allocations already have the correct memory state (all zeros).
    // 5) We pass the previously allocated array to the ThreadManager, that will use it for the stack of the Idle thread.
    // 6) We create the Idle Thread and context switch to it.
    //    This is the trick that allows to reclaim all memory: we'll never go back to the original context,
    //    so all its state can be discarded.
    //    The new thread will overwrite the same location but that's OK. And all memory is managed (AKA: it has a valid ObjectHeader to describe it).
    //

    public static class Bootstrap
    {
        [NoReturn]
        [CannotAllocate]
        [StackNotAvailable]
        [BottomOfCallStack]
        [HardwareExceptionHandler( HardwareException.Reset )]
        public static void EntryPoint()
        {
            PreInitialization();

            Initialization();
        }

        [NoInline]
        [NoReturn]
        [TS.WellKnownMethod( "Bootstrap_Initialization" )]
        public static unsafe void Initialization()
        {
#if SELF_TEST_BASIC
            SelfTest.SelfTest__Bootstrap( );
#endif // SELF_TEST_BASIC

            //
            // This should only minimally setup hardware so that the system is functional.
            // For example, all the peripherals have been added to the address space,
            // memory has been initialized and operated at the nominal rate, the CPU runs at
            // the correct clock frequency, etc.
            //
            HardwareInitialization();
            
            //
            // This only initializes the heap.
            //
            HeapInitialization();

            //
            // This initializes the main software services, like object allocation, type system, thread manager, etc.
            //
            SoftwareInitialization( Device.Instance.BootstrapStack );

#if SELF_TEST_CHECKS
            SelfTest.SelfTest__Checks();
#endif // SELF_TEST_CHECKS
#if SELF_TEST_MEMORY
            SelfTest.SelfTest__Memory();
#endif // SELF_TEST_MEMORY

            //
            // Once all the software services have been initialized, we can activate the hardware.
            // Activating the hardware might require starting threads, associated delegate with callbacks, etc.
            //
            HardwareActivation();

            //
            // After the hardware is ready, we can start the software services, which will use the hardware one.
            //
            SoftwareActivation();

            // 
            // Time to start execution of user app by delegating to the thread manager
            ThreadManager.Instance.StartThreads();
        }

        //--//

        [Inline]
        [StackAvailableOnReturn]
        private static void PreInitialization()
        {
            Device dev = Device.Instance;
            dev.PreInitializeProcessorAndMemory();
            dev.MoveCodeToProperLocation();
        }

        [NoInline]
        private static void HardwareInitialization()
        {
            Processor.Instance.InitializeProcessor();

            Memory.Instance.InitializeMemory();

            Peripherals.Instance.Initialize();
        }

        [NoInline]
        [CanAllocateOnReturn]
        [TS.WellKnownMethod( "Bootstrap_HeapInitialization" )]
        private static void HeapInitialization()
        {
            MemoryManager mm = MemoryManager.Instance;
            mm.InitializeMemoryManager();
            mm.InitializationComplete();
        }

        [TS.WellKnownMethod( "Bootstrap_ReferenceCountingInitialization" )]
        private static void ReferenceCountingInitialization()
        {
            ThreadManager.InitializeForReferenceCounting( );
        }

        [NoInline]
        [CanAllocateOnReturn]
        private static void SoftwareInitialization( uint[] systemStack )
        {
            ThreadManager.Instance.InitializeBeforeStaticConstructors();

            TypeSystemManager.Instance.InitializeTypeSystemManager();
            
            ThreadManager.Instance.InitializeAfterStaticConstructors( systemStack );

            GarbageCollectionManager.Instance.InitializeGarbageCollectionManager();
        }

        [NoInline]
        private static void HardwareActivation()
        {
            Storage.Instance.InitializeStorage();

            Peripherals.Instance.Activate();
        }

        [NoInline]
        private static void SoftwareActivation()
        {
            ThreadManager.Instance.Activate();
        }
    }
}
