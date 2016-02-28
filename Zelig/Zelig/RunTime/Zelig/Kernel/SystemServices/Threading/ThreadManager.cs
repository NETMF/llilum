//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define ARMv7

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ImplicitInstance]
    [ForceDevirtualization]
    [TS.WellKnownType( "Microsoft_Zelig_Runtime_ThreadManager" )]
    public abstract class ThreadManager
    {
        class EmptyManager : ThreadManager
        {
            //
            // State
            //

            //
            // Helper Methods
            //

            public override void InitializeBeforeStaticConstructors()
            {
            }

            public override void InitializeAfterStaticConstructors( uint[] systemStack )
            {
            }

            public override void Activate()
            {
            }

            public override void Reschedule()
            {
            }

            public override void SetNextWaitTimer( SchedulerTime nextTimeout )
            {
            }

            public override void CancelQuantumTimer()
            {
            }

            public override void SetNextQuantumTimer()
            {
            }

            public override void SetNextQuantumTimer( SchedulerTime nextTimeout )
            {
            }

            public override ThreadImpl InterruptThread
            {
                get
                {
                    return null;
                }
            }

            public override ThreadImpl FastInterruptThread
            {
                get
                {
                    return null;
                }
            }

            public override ThreadImpl AbortThread
            {
                get
                {
                    return null;
                }
            }

            protected override void IdleThread( )
            {
            }
        }

        //
        // State
        //

        protected KernelList< ThreadImpl >        m_allThreads;
        protected KernelList< ThreadImpl >        m_readyThreads;
        protected KernelList< ThreadImpl >        m_waitingThreads;
                                          
        protected ThreadImpl                      m_mainThread;
        protected ThreadImpl                      m_idleThread;
        protected EventWaitHandleImpl             m_neverSignaledEvent;
        protected bool                            m_noInvalidateNextWaitTimerRecursion;
                                          
        //--//                            
                                          
        protected ThreadImpl                      m_runningThread;
        protected ThreadImpl                      m_nextThread;
                        
        protected KernelPerformanceCounter        m_deadThreadsTime;

        //
        // Helper Methods
        //

        public virtual int DefaultStackSize
        {
            get
            {
                return 2048;
            }
        }

        [TS.DisableAutomaticReferenceCounting]
        public static void InitializeForReferenceCounting()
        {
            // Set up a dummy bootstrap thread with a fake release reference helper as part of the
            // heap initialization when the reference counting garbage collection is turned on.
            // This is so that if ReleaseReference needs to call CurrentThread.ReleaseReferenceHelper
            // before the main thread is established, it can behave predictably.
            ThreadImpl.CurrentThread = new ThreadImpl( ThreadImpl.BootstrapThread.BootstrapThread );
        }

        [TS.DisableAutomaticReferenceCounting]
        [TS.WellKnownMethod( "ThreadManager_CleanupBootstrapThread" )]
        private static void CleanupBootstrapThread( )
        {
            var dummyThread = ThreadImpl.CurrentThread;
            var releaseRefHelper = dummyThread.ReleaseReference;
            ThreadImpl.CurrentThread = null;
            MemoryManager.Instance.Release( ObjectHeader.Unpack( dummyThread ).ToPointer( ) );
            MemoryManager.Instance.Release( ObjectHeader.Unpack( releaseRefHelper ).ToPointer( ) );
        }

        [NoInline]
        [TS.WellKnownMethod( "ThreadManager_CleanupBootstrapThreadIfNeeded" )]
        private static void CleanupBootstrapThreadIfNeeded()
        {
            // Injection site for reference counting GC to call CleanupBootstrapThread()
        }

        public virtual void InitializeBeforeStaticConstructors()
        {
            //
            // Create the first active thread.
            //
            m_mainThread = new ThreadImpl( MainThread );

            CleanupBootstrapThreadIfNeeded( );

            //
            // We need to have a current thread during initialization, in case some static constructors try to access it.
            //
            ThreadImpl.CurrentThread = m_mainThread;
        }

        public virtual void InitializeAfterStaticConstructors( uint[] systemStack )
        {
            m_allThreads          = new KernelList< ThreadImpl >();
            m_readyThreads        = new KernelList< ThreadImpl >();
            m_waitingThreads      = new KernelList< ThreadImpl >();

            m_idleThread          = new ThreadImpl( IdleThread, systemStack );
            m_neverSignaledEvent  = new EventWaitHandleImpl( false, System.Threading.EventResetMode.ManualReset );

            //
            // These threads are never started, so we have to manually register them, to enable the debugger to see them.
            //
            RegisterThread( m_idleThread );
        }

        public virtual void Activate()
        {
        }

        [NoReturn]
        public virtual void StartThreads()
        {
            //
            // 'm_runningThread' should never be null once the interrupts have been enabled, so we have to set it here.
            //
            ThreadImpl bootstrapThread = m_idleThread;

            m_runningThread = bootstrapThread;
#if USE_THREAD_PERFORMANCE_COUNTER
            bootstrapThread.AcquiredProcessor();
#endif // USE_THREAD_PERFORMANCE_COUNTER

            //
            // Start the first active thread.
            //
            m_mainThread.Start();

            //
            // Long jump to the idle thread context, which will re-enable interrupts and 
            // cause the first context switch to the process stack of this thread
            //
            bootstrapThread.SwappedOutContext.SwitchTo();
        }

        //--//

        //
        // Helper Methods
        //

        protected void RegisterThread( ThreadImpl thread )
        {
            BugCheck.AssertInterruptsOff();

            m_allThreads.InsertAtTail( thread.RegistrationLink );
        }

        public virtual void AddThread( ThreadImpl thread )
        {
            BugCheck.Assert( thread.SchedulingLink.VerifyUnlinked(), BugCheck.StopCode.KernelNodeStillLinked );

            using(SmartHandles.InterruptState hnd = SmartHandles.InterruptState.Disable())
            {
                RegisterThread( thread );

                InsertInPriorityOrder( thread );

                RescheduleAndRequestContextSwitchIfNeeded( hnd.GetCurrentExceptionMode() );
            }
        }

        public virtual void RemoveThread( ThreadImpl thread )
        {
            using(SmartHandles.InterruptState hnd = SmartHandles.InterruptState.Disable())
            {
                thread.Detach();

                if(thread == m_runningThread)
                {
                    RescheduleAndRequestContextSwitchIfNeeded( hnd.GetCurrentExceptionMode() );
                }
                else
                {
                    //
                    // If the thread is not the running one, it won't get a chance to execute the Stop method.
                    //
                    thread.Stop();
                }
            }
        }

        public virtual void RetireThread( ThreadImpl thread )
        {
            m_deadThreadsTime.Merge( thread.ActiveTime );
        }

        //--//

        public virtual void Yield()
        {
            BugCheck.AssertInterruptsOn();

            ThreadImpl thisThread = ThreadImpl.CurrentThread;

            BugCheck.Assert( thisThread != null, BugCheck.StopCode.NoCurrentThread );

            using (SmartHandles.InterruptState hnd = SmartHandles.InterruptState.Disable())
            {
                InsertInPriorityOrder(thisThread);

                RescheduleAndRequestContextSwitchIfNeeded(HardwareException.None);
            }
        }

        public virtual void SwitchToWait( Synchronization.WaitingRecord wr )
        {
            BugCheck.AssertInterruptsOn();

            using(SmartHandles.InterruptState hnd = SmartHandles.InterruptState.Disable())
            {
                if(wr.Processed == false)
                {
                    ThreadImpl thread = wr.Source;

                    m_waitingThreads.InsertAtTail( thread.SchedulingLink );

                    thread.State |= System.Threading.ThreadState.WaitSleepJoin;

                    InvalidateNextWaitTimer();

                    RescheduleAndRequestContextSwitchIfNeeded( hnd.GetCurrentExceptionMode() );

                    while(thread.IsWaiting)
                    {
                        hnd.Toggle();
                    }
                }
            }
        }

        public virtual void Wakeup( ThreadImpl thread )
        {
            using(SmartHandles.InterruptState hnd = SmartHandles.InterruptState.Disable())
            {
                if(thread.IsWaiting)
                {
                    thread.State &= ~System.Threading.ThreadState.WaitSleepJoin;

                    InsertInPriorityOrder( thread );

                    RescheduleAndRequestContextSwitchIfNeeded( hnd.GetCurrentExceptionMode() );
                }
            }
        }

        public virtual void TimeQuantumExpired( )
        {
//#if !ARMv7
            BugCheck.AssertInterruptsOff( );
//b#endif

            InsertInPriorityOrder( m_runningThread );

            Reschedule( );
        }

        public virtual void SetNextQuantumTimerIfNeeded()
        {
            ThreadImpl nextThread = m_nextThread;

            if(nextThread == m_idleThread)
            {
                CancelQuantumTimer(); // No need to set a timer, we are just idling.
            }
            else
            {
                ThreadImpl lastThread = m_readyThreads.LastTarget();

                //
                // If the next thread is not an idle thread, there has to be a ready thread.
                //
                BugCheck.Assert( lastThread != null, BugCheck.StopCode.ExpectingReadyThread );

                if(lastThread == nextThread)
                {
                    CancelQuantumTimer(); // Only one ready thread, no need to preempt it.
                }
                else
                {
                    SetNextQuantumTimer();
                }
            }
        }

        public void RescheduleAndRequestContextSwitchIfNeeded( HardwareException mode )
        {
            //BugCheck.Log( "Mode: %d", (int)mode ); 

            Reschedule();

//////#if ARMv7
//////            //
//////            // Timer will fire to ths point, and for the time being they are actual interrupts, although they should
//////            // just be user mode handlers from the controller thread
//////            // We therefore need to  pick the case if System timer exception and let it go as if it was a normal thread mode
//////            // handler. When we enable the interrupts controller this case will be automatically take care of and we 
//////            // can remove this #if
//////            // 
            
//////            if(mode == HardwareException.None || mode == HardwareException.Interrupt )
//////#else
            if(mode == HardwareException.None || mode == HardwareException.SysTick || mode == HardwareException.Interrupt)
//////#endif
            {
                if(this.ShouldContextSwitch)
                {
                    Peripherals.Instance.CauseInterrupt( );
                }
            }
        }

        public virtual void Reschedule()
        {
            SelectNextThreadToRun();
        }

        public void SelectNextThreadToRun()
        {
            using(SmartHandles.InterruptState.Disable())
            {
                ThreadImpl thread = m_readyThreads.FirstTarget();

                m_nextThread = thread != null ? thread : m_idleThread;
                
                SetNextQuantumTimerIfNeeded();
            }
        }

        //
        // Extensibility 
        //
        public abstract void SetNextWaitTimer( SchedulerTime nextTimeout );

        public abstract void CancelQuantumTimer();

        public abstract void SetNextQuantumTimer();

        public abstract void SetNextQuantumTimer( SchedulerTime nextTimeout );
        
        public abstract ThreadImpl InterruptThread     { get; }

        public abstract ThreadImpl FastInterruptThread { get; }

        public abstract ThreadImpl AbortThread         { get; }

        //--//

        public void InvalidateNextWaitTimer()
        {
            if(m_noInvalidateNextWaitTimerRecursion == false)
            {
                ComputeNextTimeout();
            }
        }

        protected void WaitExpired( SchedulerTime currentTime )
        {
            m_noInvalidateNextWaitTimerRecursion = true;

            KernelNode< ThreadImpl > node = m_waitingThreads.StartOfForwardWalk;

            while(node.IsValidForForwardMove)
            {
                KernelNode< ThreadImpl > nodeNext = node.Next;

                node.Target.ProcessWaitExpiration( currentTime );

                node = nodeNext;
            }

            m_noInvalidateNextWaitTimerRecursion = false;

            ComputeNextTimeout();
        }

        private void ComputeNextTimeout()
        {
            SchedulerTime            nextTimeout = SchedulerTime.MaxValue;
            KernelNode< ThreadImpl > node        = m_waitingThreads.StartOfForwardWalk;

            while(node.IsValidForForwardMove)
            {
                SchedulerTime threadTimeout = node.Target.GetFirstTimeout();

                if(nextTimeout > threadTimeout)
                {
                    nextTimeout = threadTimeout;
                }

                node = node.Next;
            }

            SetNextWaitTimer( nextTimeout );
        }

        //--//

        public void Sleep( SchedulerTime schedulerTime )
        {
            m_neverSignaledEvent.WaitOne( schedulerTime, false );
        }

        //--//

        [Inline]
        public static SmartHandles.SwapCurrentThreadUnderInterrupt InstallInterruptThread()
        {
            return ThreadImpl.SwapCurrentThreadUnderInterrupt( ThreadManager.Instance.InterruptThread );
        }

        [Inline]
        public static SmartHandles.SwapCurrentThreadUnderInterrupt InstallFastInterruptThread()
        {
            return ThreadImpl.SwapCurrentThreadUnderInterrupt( ThreadManager.Instance.FastInterruptThread );
        }

        [Inline]
        public static SmartHandles.SwapCurrentThreadUnderInterrupt InstallAbortThread()
        {
            return ThreadImpl.SwapCurrentThreadUnderInterrupt( ThreadManager.Instance.AbortThread );
        }

        //--//

        protected void InsertInPriorityOrder( ThreadImpl thread )
        {
            //
            // Idle thread must never enter the set of the ready threads
            //
            if(thread == m_idleThread)
            {
                return;
            }

            //
            // Insert in order.
            //
            var node = m_readyThreads.StartOfForwardWalk;
            var pri  = thread.Priority;

            while(node.IsValidForForwardMove)
            {
                if(node.Target.Priority < pri)
                {
                    break;
                }

                node = node.Next;
            }

            thread.SchedulingLink.InsertBefore( node );

            thread.State &= ~System.Threading.ThreadState.WaitSleepJoin;
        }

        protected abstract void IdleThread( );

        private void MainThread()
        {
            while(true)
            {
                try
                {
                    //BugCheck.Log( "[MainThreads] !!! EXECUTING APP !!!" );
                    //BugCheck.Log( "[MainThreads] !!! EXECUTING APP !!!" );
                    //BugCheck.Log( "[MainThreads] !!! EXECUTING APP !!!" );

                    Configuration.ExecuteApplication();
                }
                catch
                {
                }

                BugCheck.Raise( BugCheck.StopCode.NoCurrentThread ); 
            }
        }

        //
        // Access Methods
        //

        public static extern ThreadManager Instance
        {
            [SingletonFactory(Fallback=typeof(EmptyManager))]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }

        public virtual ThreadImpl CurrentThread
        {
            get
            {
                return m_runningThread;
            }

            set
            {
#if !ARMv7
                //
                // For ARMv7 we are using the async PendSV exception, which is delivered with ISRs enabled
                //
                BugCheck.AssertInterruptsOff();
#endif

                ThreadImpl oldValue = m_runningThread;

                if(oldValue != value)
                {
#if USE_THREAD_PERFORMANCE_COUNTER
                    oldValue.ReleasedProcessor();
#endif // USE_THREAD_PERFORMANCE_COUNTER
                    m_runningThread = value;

#if USE_THREAD_PERFORMANCE_COUNTER
                    value.AcquiredProcessor();
#endif // USE_THREAD_PERFORMANCE_COUNTER

                    SetNextQuantumTimerIfNeeded();
                }
            }
        }

        public ThreadImpl NextThread
        {
            get
            {
                return m_nextThread;
            }
        }

        public virtual bool ShouldContextSwitch
        {
            [Inline]
            get
            {
                return m_runningThread != m_nextThread;
            }
        }

        public KernelNode< ThreadImpl > StartOfForwardWalkThroughAllThreads
        {
            get
            {
                return m_allThreads.StartOfForwardWalk;
            }
        }
    }
}

