// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: Thread
**
**
** Purpose: Class for creating and managing a thread.
**
**
=============================================================================*/

namespace System.Threading
{
    using System;
    using System.Threading;
    using System.Runtime.InteropServices;
////using System.Runtime.Remoting.Contexts;
////using System.Runtime.Remoting.Messaging;
    using System.Diagnostics;
    using System.Security.Permissions;
////using System.Security.Principal;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
////using System.Security;
////using System.Runtime.Versioning;

////internal delegate Object InternalCrossContextDelegate( Object[] args );

////internal class ThreadHelper
////{
////    Delegate         m_start;
////    Object           m_startArg         = null;
////    ExecutionContext m_executionContext = null;
////
////    internal ThreadHelper( Delegate start )
////    {
////        m_start = start;
////    }
////
////    internal void SetExecutionContextHelper( ExecutionContext ec )
////    {
////        m_executionContext = ec;
////    }
////
////    static internal ContextCallback s_ccb = new ContextCallback( ThreadStart_Context );
////
////    static internal void ThreadStart_Context( Object state )
////    {
////        ThreadHelper t = (ThreadHelper)state;
////        if(t.m_start is ThreadStart)
////        {
////            ((ThreadStart)t.m_start)();
////        }
////        else
////        {
////            ((ParameterizedThreadStart)t.m_start)( t.m_startArg );
////        }
////    }
////
////    // call back helper
////    internal void ThreadStart( object obj )
////    {
////        m_startArg = obj;
////
////        if(m_executionContext != null)
////        {
////            ExecutionContext.Run( m_executionContext, s_ccb, (Object)this );
////        }
////        else
////        {
////            ((ParameterizedThreadStart)m_start)( obj );
////        }
////    }
////
////    // call back helper
////    internal void ThreadStart()
////    {
////        if(m_executionContext != null)
////        {
////            ExecutionContext.Run( m_executionContext, s_ccb, (Object)this );
////        }
////        else
////        {
////            ((ThreadStart)m_start)();
////        }
////    }
////}

    // deliberately not [serializable]
    [Microsoft.Zelig.Internals.WellKnownType( "System_Threading_Thread" )]
    public sealed class Thread : CriticalFinalizerObject
    {
////    /*=========================================================================
////    ** Data accessed from managed code that needs to be defined in
////    ** ThreadBaseObject to maintain alignment between the two classes.
////    ** DON'T CHANGE THESE UNLESS YOU MODIFY ThreadBaseObject in vm\object.h
////    =========================================================================*/
////    private Context          m_Context;
////
////    private ExecutionContext m_ExecutionContext;    // this call context follows the logical thread
        private String           m_Name;
////    private Delegate         m_Delegate;             // Delegate
////
////    private Object[][]       m_ThreadStaticsBuckets; // Holder for thread statics
////    private int[]            m_ThreadStaticsBits;    // Bit-markers for slot availability
////    private CultureInfo      m_CurrentCulture;
////    private CultureInfo      m_CurrentUICulture;
////    private Object           m_ThreadStartArg;
////
////    /*=========================================================================
////    ** The base implementation of Thread is all native.  The following fields
////    ** should never be used in the C# code.  They are here to define the proper
////    ** space so the thread object may be allocated.  DON'T CHANGE THESE UNLESS
////    ** YOU MODIFY ThreadBaseObject in vm\object.h
////    =========================================================================*/
////#pragma warning disable 169
////#pragma warning disable 414  // These fields are not used from managed.
////    // IntPtrs need to be together, and before ints, because IntPtrs are 64-bit
////    //  fields on 64-bit platforms, where they will be sorted together.
////
////    private IntPtr           DONT_USE_InternalThread;        // Pointer
////    private int              m_Priority;                     // INT32
////#pragma warning restore 414
////#pragma warning restore 169
////
////    /*=========================================================================
////    ** This manager is responsible for storing the global data that is
////    ** shared amongst all the thread local stores.
////    =========================================================================*/
////    static private LocalDataStoreMgr s_LocalDataStoreMgr;
////
////    /*=========================================================================
////    ** Thread-local data store
////    =========================================================================*/
////    [ThreadStatic]
////    static private LocalDataStoreHolder s_LocalDataStore;
////
////    // Has to be in sync with THREAD_STATICS_BUCKET_SIZE in vm/threads.cpp
////    private const int STATICS_BUCKET_SIZE = 32;
    
        /*=========================================================================
        ** Creates a new Thread object which will begin execution at
        ** start.ThreadStart on a new thread when the Start method is called.
        **
        ** Exceptions: ArgumentNullException if start == null.
        =========================================================================*/
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern Thread( ThreadStart start );
////    {
////        if(start == null)
////        {
////            throw new ArgumentNullException( "start" );
////        }
////
////        SetStartHelper( (Delegate)start, 0 );  //0 will setup Thread with default stackSize
////    }
    
////    public Thread( ThreadStart start, int maxStackSize )
////    {
////        if(start == null)
////        {
////            throw new ArgumentNullException( "start" );
////        }
////
////        if(0 > maxStackSize)
////        {
////            throw new ArgumentOutOfRangeException( "maxStackSize", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////        }
////
////        SetStartHelper( (Delegate)start, maxStackSize );
////    }
////
////    public Thread( ParameterizedThreadStart start )
////    {
////        if(start == null)
////        {
////            throw new ArgumentNullException( "start" );
////        }
////
////        SetStartHelper( (Delegate)start, 0 );
////    }
////
////    public Thread( ParameterizedThreadStart start, int maxStackSize )
////    {
////        if(start == null)
////        {
////            throw new ArgumentNullException( "start" );
////        }
////
////        if(0 > maxStackSize)
////        {
////            throw new ArgumentOutOfRangeException( "maxStackSize", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////        }
////
////        SetStartHelper( (Delegate)start, maxStackSize );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    public extern override int GetHashCode();
    
        public extern int ManagedThreadId
        {
////        [ResourceExposure( ResourceScope.None )]
////        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    
        /*=========================================================================
        ** Spawns off a new thread which will begin executing at the ThreadStart
        ** method on the IThreadable interface passed in the constructor. Once the
        ** thread is dead, it cannot be restarted with another call to Start.
        **
        ** Exceptions: ThreadStateException if the thread has already been started.
        =========================================================================*/
        [HostProtection( Synchronization = true, ExternalThreading = true )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern void Start();
////    {
////        // Attach current thread's security principal object to the new
////        // thread. Be careful not to bind the current thread to a principal
////        // if it's not already bound.
////        if(m_Delegate != null)
////        {
////            // If we reach here with a null delegate, something is broken. But we'll let the StartInternal method take care of
////            // reporting an error. Just make sure we dont try to dereference a null delegate.
////            ThreadHelper t = (ThreadHelper)(m_Delegate.Target);
////
////            ExecutionContext ec = ExecutionContext.Capture();
////
////            ExecutionContext.ClearSyncContext( ec );
////
////            t.SetExecutionContextHelper( ec );
////        }
////
////        IPrincipal principal = (IPrincipal)CallContext.Principal;
////
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        StartInternal( principal, ref stackMark );
////    }
    
////    [HostProtection( Synchronization = true, ExternalThreading = true )]
////    public void Start( object parameter )
////    {
////        //In the case of a null delegate (second call to start on same thread)
////        //    StartInternal method will take care of the error reporting
////        if(m_Delegate is ThreadStart)
////        {
////            //We expect the thread to be setup with a ParameterizedThreadStart
////            //    if this constructor is called.
////            //If we got here then that wasn't the case
////            throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_ThreadWrongThreadStart" ) );
////        }
////
////        m_ThreadStartArg = parameter;
////        Start();
////    }
////
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    internal ExecutionContext GetExecutionContextNoCreate()
////    {
////        return m_ExecutionContext;
////    }
////
////
////    public ExecutionContext ExecutionContext
////    {
////        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////        get
////        {
////            if(m_ExecutionContext == null && this == Thread.CurrentThread)
////            {
////                m_ExecutionContext = new ExecutionContext();
////                m_ExecutionContext.Thread = this;
////            }
////
////            return m_ExecutionContext;
////        }
////    }
////
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    internal void SetExecutionContext( ExecutionContext value )
////    {
////        m_ExecutionContext = value;
////        if(value != null)
////        {
////            m_ExecutionContext.Thread = this;
////        }
////    }
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void StartInternal( IPrincipal principal, ref StackCrawlMark stackMark );
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    internal extern IntPtr SetAppDomainStack( SafeCompressedStackHandle csHandle );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    internal extern void RestoreAppDomainStack( IntPtr appDomainStack );
////
////
////    // Helper method to get a logical thread ID for StringBuilder (for
////    // correctness) and for FileStream's async code path (for perf, to
////    // avoid creating a Thread instance).
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern static IntPtr InternalGetCurrentThread();
////
////    /*=========================================================================
////    ** Raises a ThreadAbortException in the thread, which usually
////    ** results in the thread's death. The ThreadAbortException is a special
////    ** exception that is not catchable. The finally clauses of all try
////    ** statements will be executed before the thread dies. This includes the
////    ** finally that a thread might be executing at the moment the Abort is raised.
////    ** The thread is not stopped immediately--you must Join on the
////    ** thread to guarantee it has stopped.
////    ** It is possible for a thread to do an unbounded amount of computation in
////    ** the finally's and thus indefinitely delay the threads death.
////    ** If Abort() is called on a thread that has not been started, the thread
////    ** will abort when Start() is called.
////    ** If Abort is called twice on the same thread, a DuplicateThreadAbort
////    ** exception is thrown.
////    =========================================================================*/
////
////    [SecurityPermissionAttribute( SecurityAction.Demand, ControlThread = true )]
////    public void Abort( Object stateInfo )
////    {
////        // If two aborts come at the same time, it is possible that the state info
////        //  gets set by one, and the actual abort gets delivered by another. But this
////        //  is not distinguishable by an application.
////        // The accessor helper will only set the value if it isn't already set,
////        //  and that particular bit of native code can test much faster than this
////        //  code could, because testing might cause a cross-appdomain marshalling.
////        AbortReason = stateInfo;
////
////        // Note: we demand ControlThread permission, then call AbortInternal directly
////        // rather than delegating to the Abort() function below. We do this to ensure
////        // that only callers with ControlThread are allowed to change the AbortReason
////        // of the thread. We call AbortInternal directly to avoid demanding the same
////        // permission twice.
////        AbortInternal();
////    }
    
////    [SecurityPermissionAttribute( SecurityAction.Demand, ControlThread = true )]
        public void Abort()
        {
            throw new NotImplementedException();
////        AbortInternal();
        }

////    // Internal helper (since we can't place security demands on
////    // ecalls/fcalls).
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void AbortInternal();
////
////    /*=========================================================================
////    ** Resets a thread abort.
////    ** Should be called by trusted code only
////      =========================================================================*/
////    [SecurityPermissionAttribute( SecurityAction.Demand, ControlThread = true )]
////    public static void ResetAbort()
////    {
////        Thread thread = Thread.CurrentThread;
////        if((thread.ThreadState & ThreadState.AbortRequested) == 0)
////        {
////            throw new ThreadStateException( Environment.GetResourceString( "ThreadState_NoAbortRequested" ) );
////        }
////
////        thread.ResetAbortNative();
////        thread.ClearAbortReason();
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void ResetAbortNative();
////
////    /*=========================================================================
////    ** Interrupts a thread that is inside a Wait(), Sleep() or Join().  If that
////    ** thread is not currently blocked in that manner, it will be interrupted
////    ** when it next begins to block.
////    =========================================================================*/
////    [SecurityPermission( SecurityAction.Demand, ControlThread = true )]
////    public void Interrupt()
////    {
////        InterruptInternal();
////    }
////
////    // Internal helper (since we can't place security demands on
////    // ecalls/fcalls).
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void InterruptInternal();
    
        /*=========================================================================
        ** Returns the priority of the thread.
        **
        ** Exceptions: ThreadStateException if the thread is dead.
        =========================================================================*/
        public extern ThreadPriority Priority
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            return (ThreadPriority)GetPriorityNative();
////        }

            [HostProtection( SelfAffectingThreading = true )]
            [MethodImpl( MethodImplOptions.InternalCall )]
            set;
////        {
////            SetPriorityNative( (int)value );
////        }
        }
    
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern int GetPriorityNative();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void SetPriorityNative( int priority );
    
        /*=========================================================================
        ** Returns true if the thread has been started and is not dead.
        =========================================================================*/
        public extern bool IsAlive
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern bool IsAliveNative();
////
////    /*=========================================================================
////    ** Returns true if the thread is a threadpool thread.
////    =========================================================================*/
////    public bool IsThreadPoolThread
////    {
////        get
////        {
////            return IsThreadpoolThreadNative();
////        }
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern bool IsThreadpoolThreadNative();
////
////    /*=========================================================================
////    ** Waits for the thread to die.
////    **
////    ** Exceptions: ThreadInterruptedException if the thread is interrupted while waiting.
////    **             ThreadStateException if the thread has not been started yet.
////    =========================================================================*/
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [HostProtection( Synchronization = true, ExternalThreading = true )]
////    private extern void JoinInternal();

        [HostProtection( Synchronization = true, ExternalThreading = true )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern void Join();
////    {
////        JoinInternal();
////    }
    
////    /*=========================================================================
////    ** Waits for the thread to die or for timeout milliseconds to elapse.
////    ** Returns true if the thread died, or false if the wait timed out. If
////    ** Timeout.Infinite is given as the parameter, no timeout will occur.
////    **
////    ** Exceptions: ArgumentException if timeout < 0.
////    **             ThreadInterruptedException if the thread is interrupted while waiting.
////    **             ThreadStateException if the thread has not been started yet.
////    =========================================================================*/
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [HostProtection( Synchronization = true, ExternalThreading = true )]
////    private extern bool JoinInternal( int millisecondsTimeout );

        [HostProtection( Synchronization = true, ExternalThreading = true )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern bool Join( int millisecondsTimeout );
////    {
////        return JoinInternal( millisecondsTimeout );
////    }

        [HostProtection( Synchronization = true, ExternalThreading = true )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern bool Join( TimeSpan timeout );
////    {
////        long tm = (long)timeout.TotalMilliseconds;
////        if(tm < -1 || tm > (long)Int32.MaxValue)
////        {
////            throw new ArgumentOutOfRangeException( "timeout", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        return Join( (int)tm );
////    }

        /*=========================================================================
        ** Suspends the current thread for timeout milliseconds. If timeout == 0,
        ** forces the thread to give up the remainer of its timeslice.  If timeout
        ** == Timeout.Infinite, no timeout will occur.
        **
        ** Exceptions: ArgumentException if timeout < 0.
        **             ThreadInterruptedException if the thread is interrupted while sleeping.
        =========================================================================*/
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern void SleepInternal( int millisecondsTimeout );

        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void Sleep( int millisecondsTimeout );
////    {
////        SleepInternal( millisecondsTimeout );
////    }

        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void Sleep( TimeSpan timeout );
////    {
////        long tm = (long)timeout.TotalMilliseconds;
////        if(tm < -1 || tm > (long)Int32.MaxValue)
////        {
////            throw new ArgumentOutOfRangeException( "timeout", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        Sleep( (int)tm );
////    }


        /* wait for a length of time proportial to 'iterations'.  Each iteration is should
           only take a few machine instructions.  Calling this API is preferable to coding
           a explict busy loop because the hardware can be informed that it is busy waiting. */

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        [HostProtection(Synchronization=true,ExternalThreading=true)]
////    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern void SpinWaitInternal(int iterations);

        [System.Security.SecuritySafeCritical]  // auto-generated
        [HostProtection(Synchronization=true,ExternalThreading=true)]
////    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void SpinWait(int iterations)
        {
            SpinWaitInternal(iterations);
        }

        [System.Security.SecurityCritical]  // auto-generated
////    [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
////    [SuppressUnmanagedCodeSecurity]
        [HostProtection(Synchronization = true, ExternalThreading = true)]
////    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern bool YieldInternal();

        [System.Security.SecuritySafeCritical]  // auto-generated
        [HostProtection(Synchronization = true, ExternalThreading = true)]
////    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static bool Yield()
        {
            return YieldInternal();
        }
        
        public extern static Thread CurrentThread
        {
////        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            Thread th = GetFastCurrentThreadNative();
////            if(th == null)
////            {
////                th = GetCurrentThreadNative();
////            }
////            return th;
////        }
        }
    
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    private static extern Thread GetCurrentThreadNative();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    private static extern Thread GetFastCurrentThreadNative();
////
////    private void SetStartHelper( Delegate start, int maxStackSize )
////    {
////        ThreadHelper threadStartCallBack = new ThreadHelper( start );
////        if(start is ThreadStart)
////        {
////            SetStart( new ThreadStart( threadStartCallBack.ThreadStart ), maxStackSize );
////        }
////        else
////        {
////            SetStart( new ParameterizedThreadStart( threadStartCallBack.ThreadStart ), maxStackSize );
////        }
////    }
////
////    /*=========================================================================
////    ** PRIVATE Sets the IThreadable interface for the thread. Assumes that
////    ** start != null.
////    =========================================================================*/
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void SetStart( Delegate start, int maxStackSize );
////
////    /*=========================================================================
////    ** Clean up the thread when it goes away.
////    =========================================================================*/
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    ~Thread()
////    {
////        // Delegate to the unmanaged portion.
////        InternalFinalize();
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void InternalFinalize();
    
    
        /*=========================================================================
        ** Return whether or not this thread is a background thread.  Background
        ** threads do not affect when the Execution Engine shuts down.
        **
        ** Exceptions: ThreadStateException if the thread is dead.
        =========================================================================*/
        public extern bool IsBackground
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            return IsBackgroundNative();
////        }

            [HostProtection( SelfAffectingThreading = true )]
            [MethodImpl( MethodImplOptions.InternalCall )]
            set;
////        {
////            SetBackgroundNative( value );
////        }
        }
    
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern bool IsBackgroundNative();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void SetBackgroundNative( bool isBackground );
////
////
////    /*=========================================================================
////    ** Return the thread state as a consistent set of bits.  This is more
////    ** general then IsAlive or IsBackground.
////    =========================================================================*/
////    public ThreadState ThreadState
////    {
////        get
////        {
////            return (ThreadState)GetThreadStateNative();
////        }
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern int GetThreadStateNative();
////
////    /*=========================================================================
////    ** Allocates an un-named data slot. The slot is allocated on ALL the
////    ** threads.
////    =========================================================================*/
////    [HostProtection( SharedState = true, ExternalThreading = true )]
////    public static LocalDataStoreSlot AllocateDataSlot()
////    {
////        return LocalDataStoreManager.AllocateDataSlot();
////    }
////
////    /*=========================================================================
////    ** Allocates a named data slot. The slot is allocated on ALL the
////    ** threads.  Named data slots are "public" and can be manipulated by
////    ** anyone.
////    =========================================================================*/
////    [HostProtection( SharedState = true, ExternalThreading = true )]
////    public static LocalDataStoreSlot AllocateNamedDataSlot( String name )
////    {
////        return LocalDataStoreManager.AllocateNamedDataSlot( name );
////    }
////
////    /*=========================================================================
////    ** Looks up a named data slot. If the name has not been used, a new slot is
////    ** allocated.  Named data slots are "public" and can be manipulated by
////    ** anyone.
////    =========================================================================*/
////    [HostProtection( SharedState = true, ExternalThreading = true )]
////    public static LocalDataStoreSlot GetNamedDataSlot( String name )
////    {
////        return LocalDataStoreManager.GetNamedDataSlot( name );
////    }
////
////    /*=========================================================================
////    ** Frees a named data slot. The slot is allocated on ALL the
////    ** threads.  Named data slots are "public" and can be manipulated by
////    ** anyone.
////    =========================================================================*/
////    [HostProtection( SharedState = true, ExternalThreading = true )]
////    public static void FreeNamedDataSlot( String name )
////    {
////        LocalDataStoreManager.FreeNamedDataSlot( name );
////    }
////
////    /*=========================================================================
////    ** Retrieves the value from the specified slot on the current thread, for that thread's current domain.
////    =========================================================================*/
////    [HostProtection( SharedState = true, ExternalThreading = true )]
////    [ResourceExposure( ResourceScope.AppDomain )]
////    public static Object GetData( LocalDataStoreSlot slot )
////    {
////        LocalDataStoreHolder dls = s_LocalDataStore;
////        if(dls == null)
////        {
////            // Make sure to validate the slot even if we take the quick path
////            LocalDataStoreManager.ValidateSlot( slot );
////            return null;
////        }
////
////        return dls.Store.GetData( slot );
////    }
////
////    /*=========================================================================
////    ** Sets the data in the specified slot on the currently running thread, for that thread's current domain.
////    =========================================================================*/
////    [HostProtection( SharedState = true, ExternalThreading = true )]
////    [ResourceExposure( ResourceScope.AppDomain )]
////    public static void SetData( LocalDataStoreSlot slot, Object data )
////    {
////        LocalDataStoreHolder dls = s_LocalDataStore;
////
////        // Create new DLS if one hasn't been created for this domain for this thread
////        if(dls == null)
////        {
////            dls = LocalDataStoreManager.CreateLocalDataStore();
////
////            s_LocalDataStore = dls;
////        }
////
////        dls.Store.SetData( slot, data );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    static extern private bool nativeGetSafeCulture( Thread t, int appDomainId, bool isUI, ref CultureInfo safeCulture );
////
////    // As the culture can be customized object then we cannot hold any
////    // reference to it before we check if it is safe because the app domain
////    // owning this customized culture may get unloaded while executing this
////    // code. To achieve that we have to do the check using nativeGetSafeCulture
////    // as the thread cannot get interrupted during the FCALL.
////    // If the culture is safe (not customized or created in current app domain)
////    // then the FCALL will return a reference to that culture otherwise the
////    // FCALL will return failure. In case of failure we'll return the default culture.
////    // If the app domain owning a customized culture that is set to teh thread and this
////    // app domain get unloaded there is a code to clean up the culture from the thread
////    // using the code in AppDomain::ReleaseDomainStores.
////
////    public CultureInfo CurrentUICulture
////    {
////        get
////        {
////            // Fetch a local copy of m_CurrentUICulture to
////            // avoid races that malicious user can introduce
////            if(m_CurrentUICulture == null)
////            {
////                return CultureInfo.UserDefaultUICulture;
////            }
////
////            CultureInfo culture = null;
////
////            if(!nativeGetSafeCulture( this, GetDomainID(), true, ref culture ) || culture == null)
////            {
////                return CultureInfo.UserDefaultUICulture;
////            }
////
////            return culture;
////        }
////
////        [HostProtection( ExternalThreading = true )]
////        set
////        {
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value" );
////            }
////
////            //If they're trying to use a Culture with a name that we can't use in resource lookup,
////            //don't even let them set it on the thread.
////            CultureInfo.VerifyCultureName( value, true );
////
////            if(nativeSetThreadUILocale( value.LCID ) == false)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidResourceCultureName", value.Name ) );
////            }
////
////            value.StartCrossDomainTracking();
////
////            m_CurrentUICulture = value;
////        }
////    }
////
////    // This returns the exposed context for a given context ID.
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    static extern private bool nativeSetThreadUILocale( int LCID );
    
        // As the culture can be customized object then we cannot hold any
        // reference to it before we check if it is safe because the app domain
        // owning this customized culture may get unloaded while executing this
        // code. To achieve that we have to do the check using nativeGetSafeCulture
        // as the thread cannot get interrupted during the FCALL.
        // If the culture is safe (not customized or created in current app domain)
        // then the FCALL will return a reference to that culture otherwise the
        // FCALL will return failure. In case of failure we'll return the default culture.
        // If the app domain owning a customized culture that is set to teh thread and this
        // app domain get unloaded there is a code to clean up the culture from the thread
        // using the code in AppDomain::ReleaseDomainStores.
    
        public CultureInfo CurrentCulture
        {
            get
            {
////            // Fetch a local copy of m_CurrentCulture to
////            // avoid races that malicious user can introduce
////            if(m_CurrentCulture == null)
////            {
                    return CultureInfo.UserDefaultCulture;
////            }
////
////            CultureInfo culture = null;
////
////            if(!nativeGetSafeCulture( this, GetDomainID(), false, ref culture ) || culture == null)
////            {
////                return CultureInfo.UserDefaultCulture;
////            }
////
////            return culture;
            }
    
////        [SecurityPermission( SecurityAction.Demand, ControlThread = true )]
////        set
////        {
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value" );
////            }
////
////            CultureInfo.CheckNeutral( value );
////
////            //If we can't set the nativeThreadLocale, we'll just let it stay
////            //at whatever value it had before.  This allows people who use
////            //just managed code not to be limited by the underlying OS.
////            CultureInfo.nativeSetThreadLocale( value.LCID );
////
////            value.StartCrossDomainTracking();
////
////            m_CurrentCulture = value;
////        }
        }
    
////    /*===============================================================
////    ====================== Thread Statics ===========================
////    ===============================================================*/
////    private int ReserveSlot()
////    {
////        // This is called by the thread on itself so no need for locks
////        if(m_ThreadStaticsBuckets == null)
////        {
////            BCLDebug.Assert( STATICS_BUCKET_SIZE % 32 == 0, "STATICS_BUCKET_SIZE should be a multiple of 32" );
////
////            // allocate bucket holder
////            // do not publish the data until all the intialization is done, in case of asynchronized exceptions
////            Object[][] newBuckets = new Object[1][];
////
////            SetIsThreadStaticsArray( newBuckets );
////
////            // allocate the first bucket
////            newBuckets[0] = new Object[STATICS_BUCKET_SIZE];
////            SetIsThreadStaticsArray( newBuckets[0] );
////
////            int[] newBits = new int[newBuckets.Length * STATICS_BUCKET_SIZE / 32];
////
////            // use memset!
////            for(int i = 0; i < newBits.Length; i++)
////            {
////                newBits[i] = unchecked( (int)0xffffffff );
////            }
////
////            // block the 0th position, we don't want any static to have
////            // slot #0 (we use it to indicate invalid slot)
////            newBits[0] &= ~1;
////
////            // also we clear the bit for slot #1, as that is the one we will be returning
////            newBits[0] &= ~2;
////
////            // now publish the arrays
////            // the write order matters here, because we check m_ThreadStaticsBuckets for initialization,
////            // we should write it at the last
////            m_ThreadStaticsBits    = newBits;
////            m_ThreadStaticsBuckets = newBuckets;
////
////            return 1;
////        }
////        int slot = FindSlot();
////
////        // slot == 0 => all slots occupied
////        if(slot == 0)
////        {
////            // We need to expand. Allocate a new bucket
////            int oldLength     = m_ThreadStaticsBuckets.Length;
////            int oldLengthBits = m_ThreadStaticsBits.Length;
////
////            int        newLength  = m_ThreadStaticsBuckets.Length + 1;
////            Object[][] newBuckets = new Object[newLength][];
////            SetIsThreadStaticsArray( newBuckets );
////
////            int   newLengthBits = newLength * STATICS_BUCKET_SIZE / 32;
////            int[] newBits       = new int[newLengthBits];
////
////            // Copy old buckets into new holder
////            Array.Copy( m_ThreadStaticsBuckets, newBuckets, m_ThreadStaticsBuckets.Length );
////
////            // Allocate new buckets
////            for(int i = oldLength; i < newLength; i++)
////            {
////                newBuckets[i] = new Object[STATICS_BUCKET_SIZE];
////                SetIsThreadStaticsArray( newBuckets[i] );
////            }
////
////
////            // Copy old bits into new bit array
////            Array.Copy( m_ThreadStaticsBits, newBits, m_ThreadStaticsBits.Length );
////
////            // Initalize new bits
////            for(int i = oldLengthBits; i < newLengthBits; i++)
////            {
////                newBits[i] = unchecked( (int)0xffffffff );
////            }
////
////            // Return the first slot in the expanded area
////            newBits[oldLengthBits] &= ~1;
////
////            // warning: if exceptions happen between the two writes, we are in a corrupted state
////            // but the chances are very low
////            m_ThreadStaticsBits    = newBits;
////            m_ThreadStaticsBuckets = newBuckets;
////
////            return oldLength * STATICS_BUCKET_SIZE;
////        }
////
////        return slot;
////    }
////
////    int FindSlot()
////    {
#if DEBUG
////        BCLDebug.Assert( m_ThreadStaticsBits != null, "m_ThreadStaticsBits must already be initialized" );
////        if(m_ThreadStaticsBits.Length != 0)
////        {
////            BCLDebug.Assert( m_ThreadStaticsBuckets != null && m_ThreadStaticsBits.Length == m_ThreadStaticsBuckets.Length * STATICS_BUCKET_SIZE / 32,
////                    "m_ThreadStaticsBuckets must already be intialized" );
////
////            for(int j = 0; j < m_ThreadStaticsBuckets.Length; j++)
////            {
////                BCLDebug.Assert( m_ThreadStaticsBuckets[j] != null && m_ThreadStaticsBuckets[j].Length == STATICS_BUCKET_SIZE,
////                                     "m_ThreadStaticsBuckets must already be initialized" );
////            }
////        }
#endif //DEBUG
////
////        int  slot = 0;   // 0 is not a valid slot number
////        int  bits = 0;
////        int  i;
////        bool bFound = false;
////
////        if(m_ThreadStaticsBits.Length != 0 && m_ThreadStaticsBits.Length != m_ThreadStaticsBuckets.Length * STATICS_BUCKET_SIZE / 32)
////        {
////            return 0;
////        }
////
////        for(i = 0; i < m_ThreadStaticsBits.Length; i++)
////        {
////            bits = m_ThreadStaticsBits[i];
////            if(bits != 0)
////            {
////                if((bits & 0xffff) != 0)
////                {
////                    bits = bits & 0xffff;
////                }
////                else
////                {
////                    bits = (bits >> 16) & 0xffff;
////                    slot += 16;
////                }
////
////                if((bits & 0xff) != 0)
////                {
////                    bits = bits & 0xff;
////                }
////                else
////                {
////                    slot += 8;
////                    bits = (bits >> 8) & 0xff;
////                }
////
////                int j;
////                for(j = 0; j < 8; j++)
////                {
////                    if((bits & (1 << j)) != 0)
////                    {
////                        bFound = true;
////                        break;
////                    }
////                }
////
////                BCLDebug.Assert( j < 8, "Bad bits?" );
////
////                slot += j;
////
////                m_ThreadStaticsBits[i] &= ~(1 << slot);
////                break;
////            }
////        }
////
////        if(bFound)
////        {
////            slot = slot + 32 * i;
////        }
////
////        BCLDebug.Assert( bFound || slot == 0, "Bad bits" );
////        return slot;
////    }
////    /*=============================================================*/
////
////    /*======================================================================
////    **  Current thread context is stored in a slot in the thread local store
////    **  CurrentContext gets the Context from the slot.
////    ======================================================================*/
////
////    public static Context CurrentContext
////    {
////        [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure )]
////        get
////        {
////            return CurrentThread.GetCurrentContextInternal();
////        }
////    }
////
////    internal Context GetCurrentContextInternal()
////    {
////        if(m_Context == null)
////        {
////            m_Context = Context.DefaultContext;
////        }
////
////        return m_Context;
////    }
////
////    [HostProtection( SharedState = true, ExternalThreading = true )]
////    internal LogicalCallContext GetLogicalCallContext()
////    {
////        return ExecutionContext.LogicalCallContext;
////    }
////
////    [HostProtection( SharedState = true, ExternalThreading = true )]
////    internal LogicalCallContext SetLogicalCallContext( LogicalCallContext callCtx )
////    {
////        LogicalCallContext oldCtx = ExecutionContext.LogicalCallContext;
////
////        ExecutionContext.LogicalCallContext = callCtx;
////
////        return oldCtx;
////    }
////
////    internal IllogicalCallContext GetIllogicalCallContext()
////    {
////        return ExecutionContext.IllogicalCallContext;
////    }
////
////    // Get and set thread's current principal (for role based security).
////    public static IPrincipal CurrentPrincipal
////    {
////        get
////        {
////            lock(CurrentThread)
////            {
////                IPrincipal principal = (IPrincipal)CallContext.Principal;
////                if(principal == null)
////                {
////                    principal = GetDomain().GetThreadPrincipal();
////
////                    CallContext.Principal = principal;
////                }
////
////                return principal;
////            }
////        }
////
////        [SecurityPermissionAttribute( SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal )]
////        set
////        {
////            CallContext.Principal = value;
////        }
////    }
////
////    // Private routine called from unmanaged code to set an initial
////    // principal for a newly created thread.
////    private void SetPrincipalInternal( IPrincipal principal )
////    {
////        GetLogicalCallContext().SecurityData.Principal = principal;
////    }
////
////    // This returns the exposed context for a given context ID.
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern Context GetContextInternal( IntPtr id );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern Object InternalCrossContextCallback( Context ctx, IntPtr ctxID, Int32 appDomainID, InternalCrossContextDelegate ftnToCall, Object[] args );
////
////    internal Object InternalCrossContextCallback( Context ctx, InternalCrossContextDelegate ftnToCall, Object[] args )
////    {
////        return InternalCrossContextCallback( ctx, ctx.InternalContextID, 0, ftnToCall, args );
////    }
////
////    // CompleteCrossContextCallback is called by the EE after transitioning to the requested context
////    private static Object CompleteCrossContextCallback( InternalCrossContextDelegate ftnToCall, Object[] args )
////    {
////        return ftnToCall( args );
////    }
////
////    /*======================================================================
////    ** Returns the current domain in which current thread is running.
////    ======================================================================*/
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern AppDomain GetDomainInternal();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern AppDomain GetFastDomainInternal();
////
////    public static AppDomain GetDomain()
////    {
////        if(CurrentThread.m_Context == null)
////        {
////            AppDomain ad;
////            ad = GetFastDomainInternal();
////            if(ad == null)
////            {
////                ad = GetDomainInternal();
////            }
////
////            return ad;
////        }
////        else
////        {
////            BCLDebug.Assert( GetDomainInternal() == CurrentThread.m_Context.AppDomain, "AppDomains on the managed & unmanaged threads should match" );
////            return CurrentThread.m_Context.AppDomain;
////        }
////    }
////
////
////    /*
////     *  This returns a unique id to identify an appdomain.
////     */
////    public static int GetDomainID()
////    {
////        return GetDomain().GetId();
////    }
    
    
        // Retrieves the name of the thread.
        //
        public String Name
        {
            get
            {
                return m_Name;
            }

            [HostProtection( ExternalThreading = true )]
            set
            {
                lock(this)
                {
                    if(m_Name != null)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_WriteOnce" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }
    
                    m_Name = value;
    
////                InformThreadNameChangeEx( this, m_Name );
                }
            }
        }
    
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern void InformThreadNameChangeEx( Thread t, String name );
////
////    internal Object AbortReason
////    {
////        get
////        {
////            object result = null;
////
////            try
////            {
////                result = GetAbortReason();
////            }
////            catch(Exception e)
////            {
////                throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_ExceptionStateCrossAppDomain" ), e );
////            }
////
////            return result;
////        }
////
////        set
////        {
////            SetAbortReason( value );
////        }
////    }

        /*
         *  This marks the beginning of a critical code region.
         */
        [HostProtection( Synchronization = true, ExternalThreading = true )]
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static extern void BeginCriticalRegion();

        /*
         *  This marks the end of a critical code region.
         */
        [HostProtection( Synchronization = true, ExternalThreading = true )]
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public static extern void EndCriticalRegion();

////    /*
////     *  This marks the beginning of a code region that requires thread affinity.
////     */
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, ControlThread = true )]
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    public static extern void BeginThreadAffinity();
////
////    /*
////     *  This marks the end of a code region that requires thread affinity.
////     */
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, ControlThread = true )]
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    public static extern void EndThreadAffinity();
    
        /*=========================================================================
        ** Volatile Read & Write and MemoryBarrier methods.
        ** Provides the ability to read and write values ensuring that the values
        ** are read/written each time they are accessed.
        =========================================================================*/
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static byte VolatileRead( ref byte address )
        {
            byte ret = address;
    
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            return ret;
        }
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static short VolatileRead( ref short address )
        {
            short ret = address;
    
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            return ret;
        }
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static int VolatileRead( ref int address )
        {
            int ret = address;
    
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            return ret;
        }
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static long VolatileRead( ref long address )
        {
            long ret = address;
    
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            return ret;
        }
    
        [CLSCompliant( false )]
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static sbyte VolatileRead( ref sbyte address )
        {
            sbyte ret = address;
    
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            return ret;
        }
    
        [CLSCompliant( false )]
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static ushort VolatileRead( ref ushort address )
        {
            ushort ret = address;
    
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            return ret;
        }
    
        [CLSCompliant( false )]
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static uint VolatileRead( ref uint address )
        {
            uint ret = address;
    
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            return ret;
        }
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static IntPtr VolatileRead( ref IntPtr address )
        {
            IntPtr ret = address;
    
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            return ret;
        }
    
        [CLSCompliant( false )]
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static UIntPtr VolatileRead( ref UIntPtr address )
        {
            UIntPtr ret = address;
    
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            return ret;
        }
    
        [CLSCompliant( false )]
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static ulong VolatileRead( ref ulong address )
        {
            ulong ret = address;
    
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            return ret;
        }
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static float VolatileRead( ref float address )
        {
            float ret = address;
    
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            return ret;
        }
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static double VolatileRead( ref double address )
        {
            double ret = address;
    
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            return ret;
        }
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static Object VolatileRead( ref Object address )
        {
            Object ret = address;
    
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            return ret;
        }
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static void VolatileWrite( ref byte address, byte value )
        {
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            address = value;
        }
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static void VolatileWrite( ref short address, short value )
        {
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            address = value;
        }
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static void VolatileWrite( ref int address, int value )
        {
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            address = value;
        }
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static void VolatileWrite( ref long address, long value )
        {
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            address = value;
        }
    
        [CLSCompliant( false )]
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static void VolatileWrite( ref sbyte address, sbyte value )
        {
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            address = value;
        }
    
        [CLSCompliant( false )]
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static void VolatileWrite( ref ushort address, ushort value )
        {
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            address = value;
        }
    
        [CLSCompliant( false )]
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static void VolatileWrite( ref uint address, uint value )
        {
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            address = value;
        }
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static void VolatileWrite( ref IntPtr address, IntPtr value )
        {
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            address = value;
        }
    
        [CLSCompliant( false )]
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static void VolatileWrite( ref UIntPtr address, UIntPtr value )
        {
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            address = value;
        }
    
        [CLSCompliant( false )]
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static void VolatileWrite( ref ulong address, ulong value )
        {
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            address = value;
        }
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static void VolatileWrite( ref float address, float value )
        {
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
            address = value;
        }
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static void VolatileWrite( ref double address, double value )
        {
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            address = value;
        }
    
        [MethodImpl( MethodImplOptions.NoInlining )] // disable optimizations
        public static void VolatileWrite( ref Object address, Object value )
        {
            MemoryBarrier(); // Call MemoryBarrier to ensure the proper semantic in a portable way.
    
            address = value;
        }
    
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void MemoryBarrier();
    
////    //We need to mark thread statics array for AppDomain leak checking purpose
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern void SetIsThreadStaticsArray( Object o );
////
////    private static LocalDataStoreMgr LocalDataStoreManager
////    {
////        get
////        {
////            if(s_LocalDataStoreMgr == null)
////            {
////                Interlocked.CompareExchange( ref s_LocalDataStoreMgr, new LocalDataStoreMgr(), null );
////            }
////
////            return s_LocalDataStoreMgr;
////        }
////    }
////
////    // Helper function to set the AbortReason for a thread abort.
////    //  Checks that they're not alredy set, and then atomically updates
////    //  the reason info (object + ADID).
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern void SetAbortReason( Object o );
////
////    // Helper function to retrieve the AbortReason from a thread
////    //  abort.  Will perform cross-AppDomain marshalling if the object
////    //  lives in a different AppDomain from the requester.
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern Object GetAbortReason();
////
////    // Helper function to clear the AbortReason.  Takes care of
////    //  AppDomain related cleanup if required.
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern void ClearAbortReason();
    } // End of class Thread

    // declaring a local var of this enum type and passing it by ref into a function that needs to do a
    // stack crawl will both prevent inlining of the calle and pass an ESP point to stack crawl to
    // Declaring these in EH clauses is illegal; they must declared in the main method body
    [Serializable]
    internal enum StackCrawlMark
    {
        LookForMe              = 0,
        LookForMyCaller        = 1,
        LookForMyCallersCaller = 2,
        LookForThread          = 3,
    }
}
