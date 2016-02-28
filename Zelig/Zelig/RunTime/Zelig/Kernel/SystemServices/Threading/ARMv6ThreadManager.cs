//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime
{

    using ARMv6 = Microsoft.Zelig.Runtime.TargetPlatform.ARMv6;


    public abstract class ARMv6ThreadManager : ThreadManager
    {
        public const uint c_TimeQuantumMsec = 20;

        //--//

        //
        // State 
        //
        
        protected ThreadImpl m_exceptionThread;

        //--//

        //
        // Helper methods
        //

        public override void InitializeAfterStaticConstructors( uint[] systemStack )
        {
            base.InitializeAfterStaticConstructors( systemStack );

            //
            // Make the stack the frame size + 1, so that we can fit the frame and be aligned to 8 bytes (array as a length member). 
            // Currently hardcoded to 128, see https://github.com/NETMF/llilum/issues/160
            //
            m_exceptionThread = new ThreadImpl( Bootstrap.Initialization, new uint[ 128 ] );

            //
            // The msp thread is never started, so we have to manually register them, to enable the debugger to see them.
            //
            RegisterThread(m_exceptionThread);

            //--//

            m_exceptionThread.SetupForExceptionHandling( unchecked((uint)ARMv6.ProcessorARMv6M.IRQn_Type.Reset_IRQn) );
        }

        public override unsafe void StartThreads( )
        {
            //
            // The standard thread manager will set the current thread to be the 
            // idle thread before letting the scheduler pick up the next target.
            // that means that the Context Switch code will find a current thread and 
            // will try to update the stack pointer of its context to the psp value on
            // the processor. We need to initialize the PSP value to whatever
            // we want the context switch to persist in the current (i.e. idle) thread
            // context. As for the general case, the Idle Thread context stack pointer 
            // is initialized to be the end of the first frame, which though really never 
            // ran. So we will initialized the actual psp register to the base of the 
            // Idle Thread stack pointer at this stage.
            //

            //
            // We willl context switch through SVC call that will fall back into Thread/PSP mode onto 
            // whatever thread the standard thread manager intended to switch into.  We will keep interrupts
            // disabled until we are ready to fire the first SVC request.
            //
            //TargetPlatform.ARMv6.SmartHandles.InterruptStateARMv6M.SetSoftwareExceptionMode( );
            
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
                
            BugCheck.Assert( false, BugCheck.StopCode.CtxSwitchFailed );
        }
        
        public override void RemoveThread( ThreadImpl thread )
        {
            //
            // This should schedule a context switch
            //
            base.RemoveThread( thread ); 
                
            //
            // If context switch was not already performed, we need to jump else where
            //
            ARMv6.ProcessorARMv6M.RaiseSupervisorCall( ARMv6.ProcessorARMv6M.SVC_Code.SupervisorCall__RetireThread );

            //
            // We should never get here
            //
            BugCheck.Assert( false, BugCheck.StopCode.CtxSwitchFailed ); 
        }

        //
        // Access methods 
        // 

        public override ThreadImpl InterruptThread
        {
            get
            {
                return m_exceptionThread;
            }
        }

        public override ThreadImpl FastInterruptThread
        {
            get
            {
                return m_exceptionThread;
            }
        }

        public override ThreadImpl AbortThread
        {
            get
            {
                return m_exceptionThread;
            }
        }

        protected override void IdleThread( )
        {

            ARMv6.ProcessorARMv6M.InitiateContextSwitch( );

            SmartHandles.InterruptState.EnableAll( ); 
             
            while(true)
            {

                Peripherals.Instance.WaitForInterrupt();
            }
        }
    }
}



