//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System;

namespace Microsoft.Zelig.Runtime
{

    using Microsoft.Zelig.Runtime.TargetPlatform.ARMv7;


    public abstract class ARMv7ThreadManager : ThreadManager
    {
        public const uint c_TimeQuantumMsec = 20;

        //--//

        //
        // State 
        //

        //protected ThreadImpl m_NMI;
        //protected ThreadImpl m_MemManage;
        //protected ThreadImpl m_BusFault;
        protected ThreadImpl m_UsageFault;
        protected ThreadImpl m_SVCCall;
        protected ThreadImpl m_PendSV;
        protected ThreadImpl m_SysTick;
        protected ThreadImpl m_interruptThread;

        //--//

        //
        // Helper methods
        //

        public override void InitializeAfterStaticConstructors( uint[] systemStack )
        {
            base.InitializeAfterStaticConstructors( systemStack );

            //m_NMI         = new ThreadImpl( null, new uint[ 128 ] );
            //m_MemManage   = new ThreadImpl( null, new uint[ 128 ] );
            //m_BusFault    = new ThreadImpl( null, new uint[ 128 ] );
            //m_UsageFault        = new ThreadImpl( null, new uint[ 128 ] );
            //m_SVCCall           = new ThreadImpl( null, new uint[ 128 ] );
            //m_PendSV            = new ThreadImpl( null, new uint[ 128 ] );
            //m_SysTick           = new ThreadImpl( null, new uint[ 256 ] );
            //m_interruptThread   = new ThreadImpl( null, new uint[ 512 ] );

            ////
            //// These threads are never started, so we have to manually register them, to enable the debugger to see them.
            ////
            //RegisterThread( m_NMI        );
            //RegisterThread( m_MemManage  );
            //RegisterThread( m_BusFault   );
            //RegisterThread( m_UsageFault );
            //RegisterThread( m_SVCCall         );
            //RegisterThread( m_PendSV          );
            //RegisterThread( m_SysTick         );
            //RegisterThread( m_interruptThread );

            ////--//

            //m_NMI       .SetupForExceptionHandling( unchecked((uint)TargetPlatform.ARMv7.ProcessorARMv7M.IRQn_Type.NonMaskableInt_IRQn  ) );
            //m_MemManage .SetupForExceptionHandling( unchecked((uint)TargetPlatform.ARMv7.ProcessorARMv7M.IRQn_Type.MemoryManagement_IRQn) );
            //m_BusFault  .SetupForExceptionHandling( unchecked((uint)TargetPlatform.ARMv7.ProcessorARMv7M.IRQn_Type.BusFault_IRQn        ) );
            //m_UsageFault.SetupForExceptionHandling( unchecked((uint)TargetPlatform.ARMv7.ProcessorARMv7M.IRQn_Type.UsageFault_IRQn      ) );
            //m_SVCCall        .SetupForExceptionHandling( unchecked((uint)TargetPlatform.ARMv7.ProcessorARMv7M.IRQn_Type.SVCall_IRQn    ) ); 
            //m_PendSV         .SetupForExceptionHandling( unchecked((uint)TargetPlatform.ARMv7.ProcessorARMv7M.IRQn_Type.PendSV_IRQn    ) );
            //m_SysTick        .SetupForExceptionHandling( unchecked((uint)TargetPlatform.ARMv7.ProcessorARMv7M.IRQn_Type.SysTick_IRQn   ) );
            //m_interruptThread.SetupForExceptionHandling( unchecked((uint)TargetPlatform.ARMv7.ProcessorARMv7M.IRQn_Type.AnyInterrupt16 ) );
        }

        public override unsafe void StartThreads( )
        {
            //
            // The standard thread manager will set the current thread to be the 
            // idle thread before letting the scheduler pick up the next target.
            // that means that the Context Switch code will find a current thread and 
            // will try to update the stack pointer of its context to the psp value on
            // the processor. We need to initailize the PSP value to whatever
            // we want the context switch to persist in the current (i.e. idle) thread
            // context. As for the general case, the Idle Thread context stack pointer 
            // is initialized to be the end of the first frame, which though really never 
            // ran. So we will initailzed the actual psp register to the base of the 
            // Idle Thread stack pointer at this stage.
            //

            //
            // Enable context switch through SVC call that will fall back into Thread/PSP mode onto 
            // whatever thread the standard thread manager intended to switch into 
            //
            ProcessorARMv7M.DisableInterruptsWithPriorityLevelHigherOrEqualTo( ProcessorARMv7M.c_Priority__SVCCall + 1 );

            //
            // Let the standard thread manager set up the next thread to run and request the switch to its context
            // It will be a switch to the idle thread (bootstrap thread)
            //
            base.StartThreads( );

            //
            // Never come back from this!
            //
                
            //BugCheck.Log( "!!!!!!!!!!!!!!!!!!!  ERROR  !!!!!!!!!!!!!!!!!!!!!");
            //BugCheck.Log( "!!! Back in Thread Manager, Ctx Switch Failed !!!");
            //BugCheck.Log( "!!!!!!!!!!!!!!!!!!!  ERROR  !!!!!!!!!!!!!!!!!!!!!");
                
            BugCheck.Assert( false, BugCheck.StopCode.CtxSwtchFailed );
        }
        
        public override void RemoveThread( ThreadImpl thread )
        {
            //
            // This shoudl scheduel a context switch
            //
            base.RemoveThread( thread ); 
                
            //
            // If context switch was not already performed, we need to jump else where
            //
            ProcessorARMv7M.RaiseSupervisorCall( ProcessorARMv7M.SVC_Code.SupervisorCall__RetireThread );

            //
            // We should never get here
            //
            BugCheck.Assert( false, BugCheck.StopCode.CtxSwtchFailed ); 
        }

        //
        // Access methods 
        // 

        public override ThreadImpl InterruptThread
        {
            get
            {
                return m_interruptThread;
            }
        }

        public override ThreadImpl FastInterruptThread
        {
            get
            {
                BugCheck.Assert( false, BugCheck.StopCode.IllegalSchedule );
                return null;
            }
        }

        public override ThreadImpl AbortThread
        {
            get
            {
                return m_UsageFault;
            }
        }

        //--//
        
            //////[Inline]
            //////private unsafe void SetNextThread( ThreadImpl th )
            //////{
                
            //////}
            
            //////[Inline]
            //////private unsafe ThreadImpl GetNextThread( )
            //////{
            //////    return null;
            //////}
            
            //////[Inline]
            //////private unsafe void SetCurrentThread( ThreadImpl th )
            //////{
            //////}
            
            //////[Inline]
            //////private unsafe ThreadImpl GetCurrentThread( )
            //////{
            //////    return null;
            //////}

            ////////--//

            //////[Inline]
            //////private static extern unsafe void CUSTOM_STUB_CTX_SWITCH_SetCurrentThread( void* current );


            //////[Inline]
            //////private static extern unsafe void* CUSTOM_STUB_CTX_SWITCH_GetCurrentThread( );


            //////[Inline]
            //////private static extern unsafe void CUSTOM_STUB_CTX_SWITCH_SetNextThread( void* next );


            //////[Inline]
            //////private static extern unsafe void* CUSTOM_STUB_CTX_SWITCH_GetNextThread( );
    }
}



