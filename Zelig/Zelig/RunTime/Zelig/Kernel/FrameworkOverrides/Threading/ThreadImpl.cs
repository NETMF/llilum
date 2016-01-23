//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Threading;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;
    
    //[ExtendClass(typeof(System.Threading.Thread), PlatformFilter = "ARM")]
    [ExtendClass(typeof(System.Threading.Thread))]
    public class ThreadImpl
    {
        //
        // State
        //

        [TS.WellKnownField( "ThreadImpl_s_currentThread" )]
        private static   ThreadImpl                                   s_currentThread;
        private static   int                                          s_managedThreadId;

        //--//

        private          int                                          m_managedThreadId;
        private          bool                                         m_fBackground;

        [TS.WellKnownField( "ThreadImpl_m_currentException" )]
        private          Exception                                    m_currentException;
                                     
        private          ThreadPriority                               m_priority;
        private readonly ThreadStart                                  m_start;
        private          uint[]                                       m_stack;
        private readonly Processor.Context                            m_swappedOutContext;
        private readonly Processor.Context                            m_throwContext;
                                                     
        private volatile ThreadState                                  m_state;
        private readonly KernelNode< ThreadImpl >                     m_registrationLink;
        private readonly KernelNode< ThreadImpl >                     m_schedulingLink;
        private          ManualResetEvent                             m_joinEvent;
        private readonly KernelList< Synchronization.WaitableObject > m_ownedObjects;
        private readonly KernelList< Synchronization.WaitingRecord  > m_pendingObjects;

        private          KernelPerformanceCounter                     m_activeTime;

        private          ReleaseReferenceHelper                       m_releaseReferenceHelper;

        //
        // HACK: We have a bug in the liveness of multi-pointer structure. We have to use a class instead.
        //
        internal Synchronization.WaitingRecord.Holder m_holder;
        
        //
        // Constructor Methods
        //

        [DiscardTargetImplementation]
        public ThreadImpl( System.Threading.ThreadStart start ) : this( start, new uint[ ThreadManager.Instance.DefaultStackSize / sizeof( uint ) ] ) // move to configuration??
        {
        }

        [TS.WellKnownMethod( "ThreadImpl_ctor" )]
        [DiscardTargetImplementation]
        public ThreadImpl( System.Threading.ThreadStart start ,
                           uint[]                       stack )
        {
            m_holder = new Synchronization.WaitingRecord.Holder();

            m_managedThreadId   = (int)0x12340000 | s_managedThreadId++;

            m_start             = start;
            m_stack             = stack;
            m_swappedOutContext = Processor.Instance.AllocateProcessorContext(this);
            m_throwContext      = Processor.Instance.AllocateProcessorContext(this);

            m_state             = ThreadState.Unstarted;
            m_registrationLink  = new KernelNode< ThreadImpl                     >( this );
            m_schedulingLink    = new KernelNode< ThreadImpl                     >( this );
            m_ownedObjects      = new KernelList< Synchronization.WaitableObject >();
            m_pendingObjects    = new KernelList< Synchronization.WaitingRecord  >();

            m_priority          = ThreadPriority.Normal;

            ThreadStart entrypoint = Entrypoint;
            
            m_swappedOutContext.PopulateFromDelegate( entrypoint, m_stack );

#if DEBUG_CTX_SWITCH
            unsafe
            {
                BugCheck.Log(
                    "Thread 0x%x, ctx 0x%x: stack 0x%x (0x%x -> 0x%x)",
                    (int)ObjectHeader.Unpack( this ).ToPointer( ),
                    (int)ObjectHeader.Unpack( m_swappedOutContext ).ToPointer( ),
                    (int)m_swappedOutContext.StackPointer,
                    (int)ArrayImpl.CastAsArray( stack ).GetDataPointer( ),
                    (int)m_swappedOutContext.BaseStackPointer );
            }
#endif
        }


        [TS.WellKnownMethod( "ThreadImpl_AllocateReleaseReferenceHelper" )]
        private void AllocateReleaseReferenceHelper()
        {
            m_releaseReferenceHelper = new ReleaseReferenceHelper( );
        }

        internal enum BootstrapThread
        {
            BootstrapThread
        }

        [TS.DisableAutomaticReferenceCounting]
        [DiscardTargetImplementation]
        internal ThreadImpl(BootstrapThread bst)
        {
            if (bst == BootstrapThread.BootstrapThread)
            {
                m_releaseReferenceHelper = new ReleaseReferenceHelper(0, 0);
            }
        }

        //--//

        //
        // Helper Methods
        //

        public void Start()
        {
            if((m_state & ThreadState.Unstarted) == 0)
            {
#if EXCEPTION_STRINGS
                throw new ThreadStateException( "Thread already started" );
#else
                throw new ThreadStateException();
#endif
            }

            m_state &= ~ThreadState.Unstarted;

            ThreadManager.Instance.AddThread( this );
        }

        public void Join()
        {
            Join( Timeout.Infinite );
        }

        public bool Join( int timeout )
        {
            return Join( TimeSpan.FromMilliseconds( timeout ) );
        }

        public bool Join( TimeSpan timeout )
        {
            if((m_state & ThreadState.Unstarted) != 0)
            {
#if EXCEPTION_STRINGS
                throw new ThreadStateException( "Thread not started" );
#else
                throw new ThreadStateException();
#endif
            }

            if((m_state & ThreadState.Stopped) != 0)
            {
                return true;
            }

            ManualResetEvent joinEvent = m_joinEvent;
            if(joinEvent == null)
            {
                joinEvent = new ManualResetEvent( false );

                ManualResetEvent joinEventOld = Interlocked.CompareExchange( ref m_joinEvent, joinEvent, null );
                if(joinEventOld != null)
                {
                    joinEvent = joinEventOld;
                }
            }

            //
            // Recheck after the creation of the event, in case of a race condition.
            //
            if((m_state & ThreadState.Stopped) != 0)
            {
                return true;
            }

            return joinEvent.WaitOne( timeout, false );
        }

        public static void Sleep( int millisecondsTimeout )
        {
            ThreadManager.Instance.Sleep( (SchedulerTime)millisecondsTimeout );
        }

        public static void Sleep( TimeSpan timeout )
        {
            ThreadManager.Instance.Sleep( (SchedulerTime)timeout );
        }

        public static void BeginCriticalRegion()
        {
        }

        public static void EndCriticalRegion()
        {
        }

        //--//

        public void SetupForExceptionHandling( uint mode )
        {
            m_swappedOutContext.SetupForExceptionHandling( mode );
        }

        // TODO: Find out a better implementation of this attribute.  It seems to corrupt the registers for class member functions
        // (in debug builds especially).
        // [BottomOfCallStack()]
        private void Entrypoint()
        {
            try
            {
                m_start();
            }
            catch
            {
            }

            m_state |= ThreadState.StopRequested;

            ThreadManager.Instance.RemoveThread( this );
        }

        //--//

        public void ReleasedProcessor()
        {
            BugCheck.AssertInterruptsOff();

            m_activeTime.Stop();

            if((m_state & ThreadState.StopRequested) != 0)
            {
                Stop();
            }
        }

        public void AcquiredProcessor()
        {
            BugCheck.AssertInterruptsOff();

            m_activeTime.Start();
        }

        //--//

        public void Yield()
        {
            ThreadManager.Instance.Yield();
        }

        public void RegisterWait( KernelNode< Synchronization.WaitingRecord > node )
        {
            BugCheck.AssertInterruptsOff();

            SchedulerTime timeout = node.Target.Timeout;

            if(timeout == SchedulerTime.MaxValue)
            {
                //
                // No timeout, add at end.
                //
                m_pendingObjects.InsertAtTail( node );
            }
            else
            {
                //
                // Insert in order.
                //
                KernelNode< Synchronization.WaitingRecord > node2            = m_pendingObjects.StartOfForwardWalk;
                bool                                        fInvalidateTimer = true;

                while(node2.IsValidForForwardMove)
                {
                    if(node2.Target.Timeout > timeout)
                    {
                        break;
                    }

                    node2            = node2.Next;
                    fInvalidateTimer = false;
                }

                node.InsertBefore(node2);

                if(fInvalidateTimer)
                {
                    ThreadManager.Instance.InvalidateNextWaitTimer();
                }
            }
        }

        public void UnregisterWait( KernelNode< Synchronization.WaitingRecord > node )
        {
            BugCheck.AssertInterruptsOff();

            node.RemoveFromList();

            SchedulerTime timeout = node.Target.Timeout;

            if(timeout != SchedulerTime.MaxValue)
            {
                ThreadManager.Instance.InvalidateNextWaitTimer();
            }
        }

        public SchedulerTime GetFirstTimeout()
        {
            BugCheck.AssertInterruptsOff();

            Synchronization.WaitingRecord wr = m_pendingObjects.FirstTarget();

            return wr != null ? wr.Timeout : SchedulerTime.MaxValue;
        }

        public void ProcessWaitExpiration( SchedulerTime currentTime )
        {
            BugCheck.AssertInterruptsOff();

            KernelNode< Synchronization.WaitingRecord > node    = m_pendingObjects.StartOfForwardWalk;
            bool                                        fWakeup = false;

            while(node.IsValidForForwardMove)
            {
                Synchronization.WaitingRecord wr = node.Target;

                //
                // The items are kept sorted, so we can stop at the first failure.
                //
                if(wr.Timeout > currentTime)
                {
                    break;
                }
                else
                {
                    KernelNode< Synchronization.WaitingRecord > nodeNext = node.Next;

                    wr.RequestFulfilled = false;

                    fWakeup = true;

                    node = nodeNext;
                }
            }

            if(fWakeup)
            {
                Wakeup();
            }
        }

        public void Wakeup()
        {
            ThreadManager.Instance.Wakeup( this );
        }

        //--//

        public void AcquiredWaitableObject( Synchronization.WaitableObject waitableObject )
        {
            using(SmartHandles.InterruptState.Disable())
            {
                m_ownedObjects.InsertAtTail( waitableObject.OwnershipLink );
            }
        }

        public void ReleasedWaitableObject( Synchronization.WaitableObject waitableObject )
        {
            using(SmartHandles.InterruptState.Disable())
            {
                waitableObject.OwnershipLink.RemoveFromList();
            }
        }

        //--//

        public void Stop()
        {
            ThreadManager.Instance.RetireThread( this );

            m_state |= ThreadState.Stopped;

            if(m_joinEvent != null)
            {
                m_joinEvent.Set();
            }
        }

        public void Detach()
        {
            using(SmartHandles.InterruptState.Disable())
            {
                while(true)
                {
                    Synchronization.WaitingRecord wr = m_pendingObjects.FirstTarget();
                    if(wr == null)
                    {
                        break;
                    }

                    wr.RequestFulfilled = false;
                }

                while(true)
                {
                    Synchronization.WaitableObject wo = m_ownedObjects.FirstTarget();
                    if(wo == null)
                    {
                        break;
                    }

                    wo.Release();
                }

                m_schedulingLink  .RemoveFromList();
                m_registrationLink.RemoveFromList();
            }
        }

        //--//

        public bool IsAtSafePoint( Processor.Context ctx )
        {
            if(m_start == null)
            {
                //
                // Interrupt threads don't have an entry point. They are always at a safe point with regard to the garbage collector.
                //
                return true;
            }

            if((m_state & ThreadState.Unstarted) != 0)
            {
                return true;
            }

            if((m_state & ThreadState.StopRequested) != 0)
            {
                return false;
            }

            ctx.Populate( this.SwappedOutContext );

            while(true)
            {
                TS.CodeMap cm = TS.CodeMap.ResolveAddressToCodeMap( ctx.ProgramCounter );

                BugCheck.Assert( cm != null, BugCheck.StopCode.UnwindFailure );

                //
                // TODO: Check to see if the method is marked as a NoGC one.
                //

                if(ctx.Unwind() == false)
                {
                    return true;
                }
            }
        }

        //--//

        [Inline]
        public static SmartHandles.SwapCurrentThread SwapCurrentThread( ThreadImpl newThread )
        {
            return new SmartHandles.SwapCurrentThread( newThread );
        }

        [Inline]
        public static SmartHandles.SwapCurrentThreadUnderInterrupt SwapCurrentThreadUnderInterrupt( ThreadImpl newThread )
        {
            return new SmartHandles.SwapCurrentThreadUnderInterrupt( newThread );
        }

        //--//

        [NoInline]
        [TS.WellKnownMethod( "ThreadImpl_GetCurrentException" )]
        public static Exception GetCurrentException()
        {
            return ThreadImpl.CurrentThread.CurrentException;
        }

        //--//

        [NoInline]
        [NoReturn]
        [TS.WellKnownMethod( "ThreadImpl_ThrowNullException" )]
        internal static void ThrowNullException()
        {
            throw new NullReferenceException();
        }

        [NoInline]
        [NoReturn]
        [TS.WellKnownMethod( "ThreadImpl_ThrowIndexOutOfRangeException" )]
        internal static void ThrowIndexOutOfRangeException()
        {
            throw new IndexOutOfRangeException();
        }

        [NoInline]
        [NoReturn]
        [TS.WellKnownMethod( "ThreadImpl_ThrowOverflowException" )]
        internal static void ThrowOverflowException()
        {
            throw new OverflowException();
        }

        [NoInline]
        [NoReturn]
        [TS.WellKnownMethod( "ThreadImpl_ThrowNotImplementedException" )]
        internal static void ThrowNotImplementedException()
        {
            throw new NotImplementedException();
        }

        //--//

        //
        // Access Methods
        //

        public int ManagedThreadId
        {
            get
            {
                return m_managedThreadId;
            }
        }

        public bool IsBackground
        {
            get
            {
                return m_fBackground;
            }
    
            set
            {
                m_fBackground = value;
            }
        }

        public ThreadPriority Priority
        {
            get
            {
                return m_priority;
            }
    
            set
            {
                m_priority = value;
            }
        }

        public bool IsAlive
        {
            get
            {
                if((m_state & ThreadState.Unstarted) != 0)
                {
                    return false;
                }

                if((m_state & ThreadState.Stopped) != 0)
                {
                    return false;
                }

                return true;
            }
        }

        public Processor.Context SwappedOutContext
        {
            get
            {
                return m_swappedOutContext;
            }
        }

        public Processor.Context ThrowContext
        {
            get
            {
                return m_throwContext;
            }
        }

        public KernelNode< ThreadImpl > RegistrationLink
        {
            get
            {
                return m_registrationLink;
            }
        }

        public KernelNode< ThreadImpl > SchedulingLink
        {
            get
            {
                return m_schedulingLink;
            }
        }

        public ThreadState State
        {
            get
            {
                return m_state;
            }

            set
            {
                m_state = value;
            }
        }

        public bool IsWaiting
        {
            [Inline]
            get
            {
                return (m_state & ThreadState.WaitSleepJoin) != 0;
            }
        }

        public Exception CurrentException
        {
            get
            {
                return m_currentException;
            }

            set
            {
                m_currentException = value;
            }
        }

        public KernelPerformanceCounter ActiveTime
        {
            get
            {
                return m_activeTime;
            }
        }

        public ReleaseReferenceHelper ReleaseReference
        {
            [Inline]
            get
            {
                return m_releaseReferenceHelper;
            }
        }

        public static ThreadImpl CurrentThread
        {
            [Inline]
            [TS.WellKnownMethod( "ThreadImpl_get_CurrentThread" )]
            get
            {
                ThreadImpl curThread = ThreadManager.Instance.CurrentThread;

                if( curThread != null )
                {
                    s_currentThread = curThread;
                }

                return s_currentThread;
            }

            [Inline]
            set
            {
                s_currentThread = value;
            }
        }
    }
}
