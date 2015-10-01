// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: WaitHandle    (this name is NOT definitive)
**
**
** Purpose: Class to represent all synchronization objects in the runtime (that allow multiple wait)
**
**
=============================================================================*/

namespace System.Threading
{
    using System;
    using System.Threading;
////using System.Runtime.Remoting;
////using System.Security.Permissions;
    using System.Runtime.CompilerServices;
////using Microsoft.Win32.SafeHandles;
////using System.Runtime.Versioning;
    using System.Runtime.ConstrainedExecution;
////using Win32Native = Microsoft.Win32.Win32Native;

    public abstract class WaitHandle : MarshalByRefObject, IDisposable
    {
////    public const int WaitTimeout = 0x102;
////
////    private const int MAX_WAITHANDLES      = 64;
////
////    private const int WAIT_OBJECT_0        = 0;
////    private const int WAIT_ABANDONED       = 0x80;
////    private const int WAIT_FAILED          = 0x7FFFFFFF;
////    private const int ERROR_TOO_MANY_POSTS = 0x12A;
////
////
////    protected static readonly IntPtr InvalidHandle = Win32Native.INVALID_HANDLE_VALUE;
////
////
////#pragma warning disable 414  // Field is not used from managed.
////    private IntPtr          waitHandle;  // !!! DO NOT MOVE THIS FIELD. (See defn of WAITHANDLEREF in object.h - has hardcoded access to this field.)
////#pragma warning restore 414
////
////    internal SafeWaitHandle safeWaitHandle;
////
////    internal bool           hasThreadAffinity;
////
////    protected WaitHandle()
////    {
////        safeWaitHandle    = null;
////        waitHandle        = InvalidHandle;
////        hasThreadAffinity = false;
////    }
////
////    public SafeWaitHandle SafeWaitHandle
////    {
////        [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////        [SecurityPermissionAttribute( SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////        [ResourceExposure( ResourceScope.Machine )]
////        [ResourceConsumption( ResourceScope.Machine )]
////        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////        get
////        {
////            if(safeWaitHandle == null)
////            {
////                safeWaitHandle = new SafeWaitHandle( InvalidHandle, false );
////            }
////
////            return safeWaitHandle;
////        }
////
////        [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////        [SecurityPermissionAttribute( SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////        [ResourceExposure( ResourceScope.Machine )]
////        [ResourceConsumption( ResourceScope.Machine )]
////        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////        set
////        {
////            // Set safeWaitHandle and waitHandle in a CER so we won't take
////            // a thread abort between the statements and leave the wait
////            // handle in an invalid state. Note this routine is not thread
////            // safe however.
////            RuntimeHelpers.PrepareConstrainedRegions();
////            try
////            {
////            }
////            finally
////            {
////                if(value == null)
////                {
////                    safeWaitHandle = null;
////                    waitHandle     = InvalidHandle;
////                }
////                else
////                {
////                    safeWaitHandle = value;
////                    waitHandle     = safeWaitHandle.DangerousGetHandle();
////                }
////            }
////        }
////    }
////
////    // Assembly-private version that doesn't do a security check.  Reduces the
////    // number of link-time security checks when reading & writing to a file,
////    // and helps avoid a link time check while initializing security (If you
////    // call a Serialization method that requires security before security
////    // has started up, the link time check will start up security, run
////    // serialization code for some security attribute stuff, call into
////    // FileStream, which will then call Sethandle, which requires a link time
////    // security check.).  While security has fixed that problem, we still
////    // don't need to do a linktime check here.
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    internal void SetHandleInternal( SafeWaitHandle handle )
////    {
////        safeWaitHandle = handle;
////        waitHandle     = handle.DangerousGetHandle();
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public virtual extern bool WaitOne( int millisecondsTimeout, bool exitContext );
////    {
////        if(millisecondsTimeout < Timeout.Infinite)
////        {
////            throw new ArgumentOutOfRangeException( "millisecondsTimeout", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        return WaitOne( (long)millisecondsTimeout, exitContext );
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public virtual extern bool WaitOne( TimeSpan timeout, bool exitContext );
////    {
////        long tm = (long)timeout.TotalMilliseconds;
////        if(Timeout.Infinite > tm || (long)Int32.MaxValue < tm)
////        {
////            throw new ArgumentOutOfRangeException( "timeout", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        return WaitOne( tm, exitContext );
////    }
    
        public virtual bool WaitOne()
        {
            return WaitOne( Timeout.Infinite, false );
        }
    
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern bool WaitOne( long timeout, bool exitContext );
////    {
////        if(safeWaitHandle == null)
////        {
////            throw new ObjectDisposedException( null, Environment.GetResourceString( "ObjectDisposed_Generic" ) );
////        }
////
////        int ret = WaitOneNative( safeWaitHandle, (uint)timeout, hasThreadAffinity, exitContext );
////        if(ret == WAIT_ABANDONED)
////        {
////            throw new AbandonedMutexException();
////        }
////
////        return (ret != WaitTimeout);
////    }
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern int WaitOneNative( SafeWaitHandle waitHandle, uint millisecondsTimeout, bool hasThreadAffinity, bool exitContext );
////
////    /*========================================================================
////    ** Waits for signal from all the objects.
////    ** timeout indicates how long to wait before the method returns.
////    ** This method will return either when all the object have been pulsed
////    ** or timeout milliseonds have elapsed.
////    ** If exitContext is true then the synchronization domain for the context
////    ** (if in a synchronized context) is exited before the wait and reacquired
////    ========================================================================*/
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    private static extern int WaitMultiple( WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext, bool WaitAll );
    
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern bool WaitAll( WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext );
////    {
////        if(waitHandles == null || waitHandles.Length == 0)
////        {
////            throw new ArgumentNullException( "waitHandles" );
////        }
////
////        if(waitHandles.Length > MAX_WAITHANDLES)
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_MaxWaitHandles" ) );
////        }
////
////        if(millisecondsTimeout < -1)
////        {
////            throw new ArgumentOutOfRangeException( "millisecondsTimeout", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        WaitHandle[] internalWaitHandles = new WaitHandle[waitHandles.Length];
////        for(int i = 0; i < waitHandles.Length; i++)
////        {
////            internalWaitHandles[i] = waitHandles[i];
////            if(internalWaitHandles[i] == null)
////            {
////                throw new ArgumentNullException( Environment.GetResourceString( "ArgumentNull_ArrayElement" ) );
////            }
////        }
////
////        int ret = WaitMultiple( internalWaitHandles, millisecondsTimeout, exitContext, true /* waitall*/ );
////        if((WAIT_ABANDONED <= ret) && (WAIT_ABANDONED + internalWaitHandles.Length > ret))
////        {
////            //In the case of WaitAll the OS will only provide the
////            //    information that mutex was abandoned.
////            //    It won't tell us which one.  So we can't set the Index or provide access to the Mutex
////            throw new AbandonedMutexException();
////        }
////
////        for(int i = 0; i < internalWaitHandles.Length; i++)
////        {
////            GC.KeepAlive( internalWaitHandles[i] );
////        }
////
////        return (ret != WaitTimeout);
////    }
////
////    public static bool WaitAll( WaitHandle[] waitHandles ,
////                                TimeSpan     timeout     ,
////                                bool         exitContext )
////    {
////        long tm = (long)timeout.TotalMilliseconds;
////        if(tm < -1 || tm > (long)Int32.MaxValue)
////        {
////            throw new ArgumentOutOfRangeException( "timeout", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        return WaitAll( waitHandles, (int)tm, exitContext );
////    }
    
    
        /*========================================================================
        ** Shorthand for WaitAll with timeout = Timeout.Infinite and exitContext = true
        ========================================================================*/
        public static bool WaitAll( WaitHandle[] waitHandles )
        {
            return WaitAll( waitHandles, Timeout.Infinite, true );
        }
    
////    /*========================================================================
////    ** Waits for notification from any of the objects.
////    ** timeout indicates how long to wait before the method returns.
////    ** This method will return either when either one of the object have been
////    ** signalled or timeout milliseonds have elapsed.
////    ** If exitContext is true then the synchronization domain for the context
////    ** (if in a synchronized context) is exited before the wait and reacquired
////    ========================================================================*/
////
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern int WaitAny( WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext );
////    {
////        if(waitHandles == null)
////        {
////            throw new ArgumentNullException( "waitHandles" );
////        }
////
////        if(MAX_WAITHANDLES < waitHandles.Length)
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_MaxWaitHandles" ) );
////        }
////
////        if(millisecondsTimeout < -1)
////        {
////            throw new ArgumentOutOfRangeException( "millisecondsTimeout", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        WaitHandle[] internalWaitHandles = new WaitHandle[waitHandles.Length];
////        for(int i = 0; i < waitHandles.Length; i++)
////        {
////            internalWaitHandles[i] = waitHandles[i];
////        }
////
////        int ret = WaitMultiple( internalWaitHandles, millisecondsTimeout, exitContext, false /* waitany*/ );
////        for(int i = 0; i < internalWaitHandles.Length; i++)
////        {
////            GC.KeepAlive( internalWaitHandles[i] );
////        }
////
////        if((WAIT_ABANDONED <= ret) && (WAIT_ABANDONED + internalWaitHandles.Length > ret))
////        {
////            int mutexIndex = ret - WAIT_ABANDONED;
////            if(0 <= mutexIndex && mutexIndex < internalWaitHandles.Length)
////            {
////                throw new AbandonedMutexException( mutexIndex, internalWaitHandles[mutexIndex] );
////            }
////            else
////            {
////                throw new AbandonedMutexException();
////            }
////        }
////        else
////        {
////            return ret;
////        }
////    }
////
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    public static int WaitAny( WaitHandle[] waitHandles ,
////                               TimeSpan     timeout     ,
////                               bool         exitContext )
////    {
////        long tm = (long)timeout.TotalMilliseconds;
////        if(tm < -1 || tm > (long)Int32.MaxValue)
////        {
////            throw new ArgumentOutOfRangeException( "timeout", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        return WaitAny( waitHandles, (int)tm, exitContext );
////    }
    
        /*========================================================================
        ** Shorthand for WaitAny with timeout = Timeout.Infinite and exitContext = true
        ========================================================================*/
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public static int WaitAny( WaitHandle[] waitHandles )
        {
            return WaitAny( waitHandles, Timeout.Infinite, true );
        }
    
////    /*=================================================
////    ==
////    ==  SignalAndWait
////    ==
////    ==================================================*/
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern int SignalAndWaitOne( SafeWaitHandle waitHandleToSignal ,
////                                                SafeWaitHandle waitHandleToWaitOn ,
////                                                int            millisecondsTimeout,
////                                                bool           hasThreadAffinity  ,
////                                                bool           exitContext        );
////
////    public static bool SignalAndWait( WaitHandle toSignal ,
////                                      WaitHandle toWaitOn )
////    {
////        return SignalAndWait( toSignal, toWaitOn, -1, false );
////    }
////
////    public static bool SignalAndWait( WaitHandle toSignal    ,
////                                      WaitHandle toWaitOn    ,
////                                      TimeSpan   timeout     ,
////                                      bool       exitContext )
////    {
////        long tm = (long)timeout.TotalMilliseconds;
////        if(tm < -1 || tm > (long)Int32.MaxValue)
////        {
////            throw new ArgumentOutOfRangeException( "timeout", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        return SignalAndWait( toSignal, toWaitOn, (int)tm, exitContext );
////    }
////
////    public static bool SignalAndWait( WaitHandle toSignal           ,
////                                      WaitHandle toWaitOn           ,
////                                      int        millisecondsTimeout,
////                                      bool       exitContext        )
////    {
////        if(toSignal == null)
////        {
////            throw new ArgumentNullException( "toSignal" );
////        }
////
////        if(toWaitOn == null)
////        {
////            throw new ArgumentNullException( "toWaitOn" );
////        }
////
////        if(millisecondsTimeout < -1)
////        {
////            throw new ArgumentOutOfRangeException( "millisecondsTimeout", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        int ret = SignalAndWaitOne( toSignal.safeWaitHandle, toWaitOn.safeWaitHandle, millisecondsTimeout, toWaitOn.hasThreadAffinity, exitContext );
////
////        if(ret != WAIT_FAILED && toSignal.hasThreadAffinity)
////        {
////            Thread.EndCriticalRegion();
////            Thread.EndThreadAffinity();
////        }
////
////        if(ret == WAIT_ABANDONED)
////        {
////            throw new AbandonedMutexException();
////        }
////
////        if(ret == ERROR_TOO_MANY_POSTS)
////        {
////            throw new InvalidOperationException( Environment.GetResourceString( "Threading.WaitHandleTooManyPosts" ) );
////        }
////
////        //Object was signaled
////        if(ret == WAIT_OBJECT_0)
////        {
////            return true;
////        }
////
////        //Timeout
////        return false;
////    }
    
        public virtual void Close()
        {
            Dispose( true );
    
////        GC.nativeSuppressFinalize( this );
        }
    
        protected virtual void Dispose( bool explicitDisposing )
        {
////        if(safeWaitHandle != null)
////        {
////            safeWaitHandle.Close();
////        }
        }
    
        /// <internalonly/>
        void IDisposable.Dispose()
        {
            Dispose( true );
    
////        GC.nativeSuppressFinalize( this );
        }
    }
}
