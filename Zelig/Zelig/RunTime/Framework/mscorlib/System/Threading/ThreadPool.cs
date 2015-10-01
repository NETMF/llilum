// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: ThreadPool
**
**
** Purpose: Class for creating and managing a threadpool
**
**
=============================================================================*/

/*
 * Below you'll notice two sets of APIs that are separated by the
 * use of 'Unsafe' in their names.  The unsafe versions are called
 * that because they do not propagate the calling stack onto the
 * worker thread.  This allows code to lose the calling stack and 
 * thereby elevate its security privileges.  Note that this operation
 * is much akin to the combined ability to control security policy
 * and control security evidence.  With these privileges, a person 
 * can gain the right to load assemblies that are fully trusted which
 * then assert full trust and can call any code they want regardless
 * of the previous stack information.
 */

namespace System.Threading
{
    using System;
    using System.Security;
    using System.Threading;
////using System.Runtime.Remoting;
    using System.Security.Permissions;
////using Microsoft.Win32;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
////using System.Runtime.Versioning;

////internal sealed class RegisteredWaitHandleSafe : CriticalFinalizerObject
////{
////    private static readonly IntPtr InvalidHandle = Win32Native.INVALID_HANDLE_VALUE;
////    private IntPtr registeredWaitHandle;
////    private WaitHandle m_internalWaitObject;
////    private bool bReleaseNeeded = false;
////    private int m_lock = 0;
////
////    internal RegisteredWaitHandleSafe()
////    {
////        registeredWaitHandle = InvalidHandle;
////    }
////
////    internal IntPtr GetHandle()
////    {
////        return registeredWaitHandle;
////    }
////
////    internal void SetHandle( IntPtr handle )
////    {
////        registeredWaitHandle = handle;
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    internal void SetWaitObject( WaitHandle waitObject )
////    {
////        // needed for DangerousAddRef
////        RuntimeHelpers.PrepareConstrainedRegions();
////        try
////        {
////        }
////        finally
////        {
////            m_internalWaitObject = waitObject;
////            if(waitObject != null)
////            {
////                m_internalWaitObject.SafeWaitHandle.DangerousAddRef( ref bReleaseNeeded );
////            }
////        }
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    internal bool Unregister(
////         WaitHandle waitObject          // object to be notified when all callbacks to delegates have completed
////         )
////    {
////        bool result = false;
////        // needed for DangerousRelease
////        RuntimeHelpers.PrepareConstrainedRegions();
////        try
////        {
////        }
////        finally
////        {
////            // lock(this) cannot be used reliably in Cer since thin lock could be
////            // promoted to syncblock and that is not a guaranteed operation
////            bool bLockTaken = false;
////            do
////            {
////                if(Interlocked.CompareExchange( ref m_lock, 1, 0 ) == 0)
////                {
////                    bLockTaken = true;
////                    try
////                    {
////                        if(ValidHandle())
////                        {
////                            result = UnregisterWaitNative( GetHandle(), waitObject == null ? null : waitObject.SafeWaitHandle );
////                            if(result == true)
////                            {
////                                if(bReleaseNeeded)
////                                {
////                                    m_internalWaitObject.SafeWaitHandle.DangerousRelease();
////                                    bReleaseNeeded = false;
////                                }
////                                // if result not true don't release/suppress here so finalizer can make another attempt
////                                SetHandle( InvalidHandle );
////                                m_internalWaitObject = null;
////                                GC.SuppressFinalize( this );
////                            }
////                        }
////                    }
////                    finally
////                    {
////                        m_lock = 0;
////                    }
////                }
////                Thread.SpinWait( 1 );     // yield to processor
////            }
////            while(!bLockTaken);
////        }
////        return result;
////    }
////
////    private bool ValidHandle()
////    {
////        return (registeredWaitHandle != InvalidHandle && registeredWaitHandle != IntPtr.Zero);
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////    ~RegisteredWaitHandleSafe()
////    {
////        // if the app has already unregistered the wait, there is nothing to cleanup
////        // we can detect this by checking the handle. Normally, there is no race here
////        // so no need to protect reading of handle. However, if this object gets 
////        // resurrected and then someone does an unregister, it would introduce a race
////
////        // PrepareConstrainedRegions call not needed since finalizer already in Cer
////
////        // lock(this) cannot be used reliably even in Cer since thin lock could be
////        // promoted to syncblock and that is not a guaranteed operation
////
////        bool bLockTaken = false;
////        do
////        {
////            if(Interlocked.CompareExchange( ref m_lock, 1, 0 ) == 0)
////            {
////                bLockTaken = true;
////                try
////                {
////                    if(ValidHandle())
////                    {
////                        WaitHandleCleanupNative( registeredWaitHandle );
////                        if(bReleaseNeeded)
////                        {
////                            m_internalWaitObject.SafeWaitHandle.DangerousRelease();
////                            bReleaseNeeded = false;
////                        }
////                        SetHandle( InvalidHandle );
////                        m_internalWaitObject = null;
////                    }
////                }
////                finally
////                {
////                    m_lock = 0;
////                }
////            }
////            Thread.SpinWait( 1 );     // yield to processor
////        }
////        while(!bLockTaken);
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern void WaitHandleCleanupNative( IntPtr handle );
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern bool UnregisterWaitNative( IntPtr handle, SafeHandle waitObject );
////}
////
////public sealed class RegisteredWaitHandle : MarshalByRefObject
////{
////    private RegisteredWaitHandleSafe internalRegisteredWait;
////
////    internal RegisteredWaitHandle()
////    {
////        internalRegisteredWait = new RegisteredWaitHandleSafe();
////    }
////
////    internal void SetHandle( IntPtr handle )
////    {
////        internalRegisteredWait.SetHandle( handle );
////    }
////
////    internal void SetWaitObject( WaitHandle waitObject )
////    {
////        internalRegisteredWait.SetWaitObject( waitObject );
////    }
////
////
////    // This is the only public method on this class
////    public bool Unregister(
////         WaitHandle waitObject          // object to be notified when all callbacks to delegates have completed
////         )
////    {
////        return internalRegisteredWait.Unregister( waitObject );
////    }
////}

    public delegate void WaitCallback( Object state );

    public delegate void WaitOrTimerCallback( Object state, bool timedOut );  // signalled or timed out

////internal class _ThreadPoolWaitCallback
////{
////    WaitCallback _waitCallback;
////    ExecutionContext _executionContext;
////    Object _state;
////
////    static internal ContextCallback _ccb = new ContextCallback( WaitCallback_Context );
////    static internal void WaitCallback_Context( Object state )
////    {
////        _ThreadPoolWaitCallback obj = (_ThreadPoolWaitCallback)state;
////        obj._waitCallback( obj._state );
////    }
////
////
////    internal _ThreadPoolWaitCallback( WaitCallback waitCallback, Object state, bool compressStack, ref StackCrawlMark stackMark )
////    {
////        _waitCallback = waitCallback;
////        _state = state;
////        if(compressStack && !ExecutionContext.IsFlowSuppressed())
////        {
////            // clone the exection context
////            _executionContext = ExecutionContext.Capture( ref stackMark );
////            ExecutionContext.ClearSyncContext( _executionContext );
////        }
////    }
////
////    // call back helper
////    static internal void PerformWaitCallback( Object state )
////    {
////        _ThreadPoolWaitCallback helper = (_ThreadPoolWaitCallback)state;
////
////        BCLDebug.Assert( helper != null, "Null state passed to PerformWaitCallback!" );
////        // call directly if it is an unsafe call OR EC flow is suppressed
////        if(helper._executionContext == null)
////        {
////            WaitCallback callback = helper._waitCallback;
////            callback( helper._state );
////        }
////        else
////        {
////            ExecutionContext.Run( helper._executionContext, _ccb, helper );
////        }
////    }
////};
////
////internal class _ThreadPoolWaitOrTimerCallback
////{
////    WaitOrTimerCallback _waitOrTimerCallback;
////    ExecutionContext _executionContext;
////    Object _state;
////    static private ContextCallback _ccbt = new ContextCallback( WaitOrTimerCallback_Context_t );
////    static private ContextCallback _ccbf = new ContextCallback( WaitOrTimerCallback_Context_f );
////
////    internal _ThreadPoolWaitOrTimerCallback( WaitOrTimerCallback waitOrTimerCallback, Object state, bool compressStack, ref StackCrawlMark stackMark )
////    {
////        _waitOrTimerCallback = waitOrTimerCallback;
////        _state = state;
////        if(compressStack && !ExecutionContext.IsFlowSuppressed())
////        {
////            // capture the exection context
////            _executionContext = ExecutionContext.Capture( ref stackMark );
////            ExecutionContext.ClearSyncContext( _executionContext );
////        }
////    }
////    static private void WaitOrTimerCallback_Context_t( Object state )
////    {
////        WaitOrTimerCallback_Context( state, true );
////    }
////    static private void WaitOrTimerCallback_Context_f( Object state )
////    {
////        WaitOrTimerCallback_Context( state, false );
////    }
////
////    static private void WaitOrTimerCallback_Context( Object state, bool timedOut )
////    {
////        _ThreadPoolWaitOrTimerCallback helper = (_ThreadPoolWaitOrTimerCallback)state;
////        helper._waitOrTimerCallback( helper._state, timedOut );
////    }
////
////
////    // call back helper
////    static internal void PerformWaitOrTimerCallback( Object state, bool timedOut )
////    {
////        _ThreadPoolWaitOrTimerCallback helper = (_ThreadPoolWaitOrTimerCallback)state;
////        BCLDebug.Assert( helper != null, "Null state passed to PerformWaitOrTimerCallback!" );
////        // call directly if it is an unsafe call OR EC flow is suppressed
////        if(helper._executionContext == null)
////        {
////            WaitOrTimerCallback callback = helper._waitOrTimerCallback;
////            callback( helper._state, timedOut );
////        }
////        else
////        {
////            if(timedOut)
////                ExecutionContext.Run( helper._executionContext.CreateCopy(), _ccbt, helper );
////            else
////                ExecutionContext.Run( helper._executionContext.CreateCopy(), _ccbf, helper );
////        }
////    }
////};
////
////[CLSCompliant( false )]
////unsafe public delegate void IOCompletionCallback( uint errorCode, // Error code
////                                   uint numBytes, // No. of bytes transferred 
////                                   NativeOverlapped* pOVERLAP // ptr to OVERLAP structure
////                                   );

    [HostProtection( Synchronization = true, ExternalThreading = true )]
    public static class ThreadPool
    {
////    [SecurityPermissionAttribute( SecurityAction.Demand, ControlThread = true )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern static bool SetMaxThreads( int workerThreads         ,
                                                 int completionPortThreads );
////    {
////        return SetMaxThreadsNative( workerThreads, completionPortThreads );
////    }
    
////    public static void GetMaxThreads( out int workerThreads         ,
////                                      out int completionPortThreads )
////    {
////        GetMaxThreadsNative( out workerThreads, out completionPortThreads );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.Demand, ControlThread = true )]
////    public static bool SetMinThreads( int workerThreads         ,
////                                      int completionPortThreads )
////    {
////        return SetMinThreadsNative( workerThreads, completionPortThreads );
////    }
////
////    public static void GetMinThreads( out int workerThreads         ,
////                                      out int completionPortThreads )
////    {
////        GetMinThreadsNative( out workerThreads, out completionPortThreads );
////    }
////
////    public static void GetAvailableThreads( out int workerThreads         ,
////                                            out int completionPortThreads )
////    {
////        GetAvailableThreadsNative( out workerThreads, out completionPortThreads );
////    }
////
////    // throws RegisterWaitException
////    [CLSCompliant( false )]
////    public static RegisteredWaitHandle RegisterWaitForSingleObject( WaitHandle          waitObject                  ,
////                                                                    WaitOrTimerCallback callBack                    ,
////                                                                    Object              state                       ,
////                                                                    uint                millisecondsTimeOutInterval ,
////                                                                    bool                executeOnlyOnce             )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return RegisterWaitForSingleObject( waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce, ref stackMark, true );
////    }
////
////    // throws RegisterWaitException
////    [CLSCompliant( false )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy )]
////    public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject( WaitHandle          waitObject                  ,
////                                                                          WaitOrTimerCallback callBack                    ,
////                                                                          Object              state                       ,
////                                                                          uint                millisecondsTimeOutInterval ,
////                                                                          bool                executeOnlyOnce             )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return RegisterWaitForSingleObject( waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce, ref stackMark, false );
////    }
////
////
////    // throws RegisterWaitException
////    private static RegisteredWaitHandle RegisterWaitForSingleObject(     WaitHandle          waitObject                  ,
////                                                                         WaitOrTimerCallback callBack                    ,
////                                                                         Object              state                       ,
////                                                                         uint                millisecondsTimeOutInterval ,
////                                                                         bool                executeOnlyOnce             ,
////                                                                     ref StackCrawlMark      stackMark                   ,
////                                                                         bool                compressStack               )
////    {
////        if(RemotingServices.IsTransparentProxy( waitObject ))
////        {
////            throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_WaitOnTransparentProxy" ) );
////        }
////
////        RegisteredWaitHandle registeredWaitHandle = new RegisteredWaitHandle();
////
////        if(callBack != null)
////        {
////            _ThreadPoolWaitOrTimerCallback callBackHelper = new _ThreadPoolWaitOrTimerCallback( callBack, state, compressStack, ref stackMark );
////            state = (Object)callBackHelper;
////            // call SetWaitObject before native call so that waitObject won't be closed before threadpoolmgr registration
////            // this could occur if callback were to fire before SetWaitObject does its addref
////            registeredWaitHandle.SetWaitObject( waitObject );
////            IntPtr nativeRegisteredWaitHandle = RegisterWaitForSingleObjectNative( waitObject,
////                                                                                   state,
////                                                                                   millisecondsTimeOutInterval,
////                                                                                   executeOnlyOnce,
////                                                                                   registeredWaitHandle,
////                                                                                   ref stackMark,
////                                                                                   compressStack );
////            registeredWaitHandle.SetHandle( nativeRegisteredWaitHandle );
////        }
////        else
////        {
////            throw new ArgumentNullException( "WaitOrTimerCallback" );
////        }
////
////        return registeredWaitHandle;
////    }
////
////
////    // throws RegisterWaitException
////    public static RegisteredWaitHandle RegisterWaitForSingleObject( WaitHandle          waitObject                  ,
////                                                                    WaitOrTimerCallback callBack                    ,
////                                                                    Object              state                       ,
////                                                                    int                 millisecondsTimeOutInterval ,
////                                                                    bool                executeOnlyOnce             )
////    {
////        if(millisecondsTimeOutInterval < -1)
////        {
////            throw new ArgumentOutOfRangeException( "millisecondsTimeOutInterval", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return RegisterWaitForSingleObject( waitObject, callBack, state, (UInt32)millisecondsTimeOutInterval, executeOnlyOnce, ref stackMark, true );
////    }
////
////    // throws RegisterWaitException
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy )]
////    public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject( WaitHandle          waitObject                  ,
////                                                                          WaitOrTimerCallback callBack                    ,
////                                                                          Object              state                       ,
////                                                                          int                 millisecondsTimeOutInterval ,
////                                                                          bool                executeOnlyOnce             )
////    {
////        if(millisecondsTimeOutInterval < -1)
////        {
////            throw new ArgumentOutOfRangeException( "millisecondsTimeOutInterval", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return RegisterWaitForSingleObject( waitObject, callBack, state, (UInt32)millisecondsTimeOutInterval, executeOnlyOnce, ref stackMark, false );
////    }
////
////    // throws RegisterWaitException
////    public static RegisteredWaitHandle RegisterWaitForSingleObject( WaitHandle          waitObject                  ,
////                                                                    WaitOrTimerCallback callBack                    ,
////                                                                    Object              state                       ,
////                                                                    long                millisecondsTimeOutInterval ,
////                                                                    bool                executeOnlyOnce             )
////    {
////        if(millisecondsTimeOutInterval < -1)
////        {
////            throw new ArgumentOutOfRangeException( "millisecondsTimeOutInterval", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return RegisterWaitForSingleObject( waitObject, callBack, state, (UInt32)millisecondsTimeOutInterval, executeOnlyOnce, ref stackMark, true );
////    }
////
////    // throws RegisterWaitException
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy )]
////    public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject( WaitHandle          waitObject                  ,
////                                                                          WaitOrTimerCallback callBack                    ,
////                                                                          Object              state                       ,
////                                                                          long                millisecondsTimeOutInterval ,
////                                                                          bool                executeOnlyOnce             )
////    {
////        if(millisecondsTimeOutInterval < -1)
////        {
////            throw new ArgumentOutOfRangeException( "millisecondsTimeOutInterval", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return RegisterWaitForSingleObject( waitObject, callBack, state, (UInt32)millisecondsTimeOutInterval, executeOnlyOnce, ref stackMark, false );
////    }
////
////
////    public static RegisteredWaitHandle RegisterWaitForSingleObject( WaitHandle          waitObject      ,
////                                                                    WaitOrTimerCallback callBack        ,
////                                                                    Object              state           ,
////                                                                    TimeSpan            timeout         ,
////                                                                    bool                executeOnlyOnce )
////    {
////        long tm = (long)timeout.TotalMilliseconds;
////        if(tm < -1)
////        {
////            throw new ArgumentOutOfRangeException( "timeout", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////        if(tm > (long)Int32.MaxValue)
////        {
////            throw new ArgumentOutOfRangeException( "timeout", Environment.GetResourceString( "ArgumentOutOfRange_LessEqualToIntegerMaxVal" ) );
////        }
////
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return RegisterWaitForSingleObject( waitObject, callBack, state, (UInt32)tm, executeOnlyOnce, ref stackMark, true );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy )]
////    public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject( WaitHandle          waitObject      ,
////                                                                          WaitOrTimerCallback callBack        ,
////                                                                          Object              state           ,
////                                                                          TimeSpan            timeout         ,
////                                                                          bool                executeOnlyOnce )
////    {
////        long tm = (long)timeout.TotalMilliseconds;
////        if(tm < -1)
////        {
////            throw new ArgumentOutOfRangeException( "timeout", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        if(tm > (long)Int32.MaxValue)
////        {
////            throw new ArgumentOutOfRangeException( "timeout", Environment.GetResourceString( "ArgumentOutOfRange_LessEqualToIntegerMaxVal" ) );
////        }
////
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return RegisterWaitForSingleObject( waitObject, callBack, state, (UInt32)tm, executeOnlyOnce, ref stackMark, false );
////    }


        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern static bool QueueUserWorkItem( WaitCallback callBack ,
                                                     Object       state    );
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return QueueUserWorkItemHelper( callBack, state, ref stackMark, true );
////    }

        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern static bool QueueUserWorkItem( WaitCallback callBack );
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return QueueUserWorkItemHelper( callBack, null, ref stackMark, true );
////    }

////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy )]
////    public static bool UnsafeQueueUserWorkItem( WaitCallback callBack ,
////                                                Object       state    )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return QueueUserWorkItemHelper( callBack, state, ref stackMark, false );
////    }
////
////    private static bool QueueUserWorkItemHelper(     WaitCallback   callBack      ,
////                                                     Object         state         ,
////                                                 ref StackCrawlMark stackMark     ,
////                                                     bool           compressStack )
////    {
////        if(callBack != null)
////        {
////            _ThreadPoolWaitCallback callBackHelper = new _ThreadPoolWaitCallback( callBack, state, compressStack, ref stackMark );
////
////            state = (Object)callBackHelper;
////
////            return QueueUserWorkItem( state, ref stackMark, compressStack );
////        }
////        else
////        {
////            throw new ArgumentNullException( "WaitCallback" );
////        }
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern bool QueueUserWorkItem( Object state, ref StackCrawlMark stackMark, bool compressStack );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    unsafe private static extern bool PostQueuedCompletionStatus( NativeOverlapped* overlapped );
////
////    [CLSCompliant( false )]
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.ControlEvidence | SecurityPermissionFlag.ControlPolicy )]
////    unsafe public static bool UnsafeQueueNativeOverlapped( NativeOverlapped* overlapped )
////    {
////        return PostQueuedCompletionStatus( overlapped );
////    }
////
////    // Native methods: 
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern bool SetMinThreadsNative( int workerThreads, int completionPortThreads );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern bool SetMaxThreadsNative( int workerThreads, int completionPortThreads );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern void GetMinThreadsNative( out int workerThreads, out int completionPortThreads );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern void GetMaxThreadsNative( out int workerThreads, out int completionPortThreads );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern void GetAvailableThreadsNative( out int workerThreads, out int completionPortThreads );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern IntPtr RegisterWaitForSingleObjectNative(     WaitHandle           waitHandle           ,
////                                                                        Object               state                ,
////                                                                        uint                 timeOutInterval      ,
////                                                                        bool                 executeOnlyOnce      ,
////                                                                        RegisteredWaitHandle registeredWaitHandle ,
////                                                                    ref StackCrawlMark       stackMark            ,
////                                                                        bool                 compressStack        );
////
////    [Obsolete( "ThreadPool.BindHandle(IntPtr) has been deprecated.  Please use ThreadPool.BindHandle(SafeHandle) instead.", false )]
////    [SecurityPermissionAttribute( SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static bool BindHandle( IntPtr osHandle )
////    {
////        return BindIOCompletionCallbackNative( osHandle );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    public static bool BindHandle( SafeHandle osHandle )
////    {
////        if(osHandle == null)
////        {
////            throw new ArgumentNullException( "osHandle" );
////        }
////
////        bool ret = false;
////        bool mustReleaseSafeHandle = false;
////        RuntimeHelpers.PrepareConstrainedRegions();
////        try
////        {
////            osHandle.DangerousAddRef( ref mustReleaseSafeHandle );
////            ret = BindIOCompletionCallbackNative( osHandle.DangerousGetHandle() );
////        }
////        finally
////        {
////            if(mustReleaseSafeHandle)
////            {
////                osHandle.DangerousRelease();
////            }
////        }
////        return ret;
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    private static extern bool BindIOCompletionCallbackNative( IntPtr fileHandle );
    }
}
