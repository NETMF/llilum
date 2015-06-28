// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: TimerQueue
**
**
** Purpose: Class for creating and managing a threadpool
**
**
=============================================================================*/

namespace System.Threading
{
    using System;
    using System.Threading;
////using System.Security;
////using System.Security.Permissions;
////using Microsoft.Win32;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.ConstrainedExecution;
////using System.Runtime.Versioning;

////internal class _TimerCallback
////{
////    TimerCallback    m_timerCallback;
////    ExecutionContext m_executionContext;
////    Object           m_state;
////
////    static internal ContextCallback s_ccb = new ContextCallback( TimerCallback_Context );
////
////    static internal void TimerCallback_Context( Object state )
////    {
////        _TimerCallback helper = (_TimerCallback)state;
////
////        helper.m_timerCallback( helper.m_state );
////    }
////
////    internal _TimerCallback( TimerCallback timerCallback, Object state, ref StackCrawlMark stackMark )
////    {
////        m_timerCallback = timerCallback;
////        m_state         = state;
////
////        if(!ExecutionContext.IsFlowSuppressed())
////        {
////            m_executionContext = ExecutionContext.Capture( ref stackMark );
////
////            ExecutionContext.ClearSyncContext( m_executionContext );
////        }
////    }
////
////    // call back helper
////    static internal void PerformTimerCallback( Object state )
////    {
////        _TimerCallback helper = (_TimerCallback)state;
////
////        BCLDebug.Assert( helper != null, "Null state passed to PerformTimerCallback!" );
////
////        // call directly if EC flow is suppressed
////        if(helper.m_executionContext == null)
////        {
////            TimerCallback callback = helper.m_timerCallback;
////
////            callback( helper.m_state );
////        }
////        else
////        {
////            // From this point on we can use useExecutionContext for this callback
////            ExecutionContext.Run( helper.m_executionContext.CreateCopy(), s_ccb, helper );
////        }
////    }
////}

    public delegate void TimerCallback( Object state );

////[HostProtection( Synchronization = true, ExternalThreading = true )]
////internal sealed class TimerBase : CriticalFinalizerObject /*, IDisposable*/
////{
#pragma warning disable 169
////    private IntPtr m_timerHandle;
////    private IntPtr m_delegateInfo;
#pragma warning restore 169
////    private int    m_timerDeleted;
////    private int    m_lock = 0;
////
////    ~TimerBase()
////    {
////        // lock(this) cannot be used reliably in Cer since thin lock could be
////        // promoted to syncblock and that is not a guaranteed operation
////        bool bLockTaken = false;
////
////        do
////        {
////            if(Interlocked.CompareExchange( ref m_lock, 1, 0 ) == 0)
////            {
////                bLockTaken = true;
////
////                try
////                {
////                    DeleteTimerNative( null );
////                }
////                finally
////                {
////                    m_lock = 0;
////                }
////            }
////
////            Thread.SpinWait( 1 );     // yield to processor
////        }
////        while(!bLockTaken);
////    }
////
////    internal void AddTimer( TimerCallback      callback  ,
////                            Object             state     ,
////                            UInt32             dueTime   ,
////                            UInt32             period    ,
////                            ref StackCrawlMark stackMark )
////    {
////        if(callback != null)
////        {
////            _TimerCallback callbackHelper = new _TimerCallback( callback, state, ref stackMark );
////
////            state = (Object)callbackHelper;
////
////            AddTimerNative( state, dueTime, period, ref stackMark );
////
////            m_timerDeleted = 0;
////        }
////        else
////        {
////            throw new ArgumentNullException( "TimerCallback" );
////        }
////    }
////
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    internal bool ChangeTimer( UInt32 dueTime, UInt32 period )
////    {
////        bool status     = false;
////        bool bLockTaken = false;
////
////        // prepare here to prevent threadabort from occuring which could
////        // destroy m_lock state.  lock(this) can't be used due to critical
////        // finalizer and thinlock/syncblock escalation.
////        RuntimeHelpers.PrepareConstrainedRegions();
////        try
////        {
////        }
////        finally
////        {
////            do
////            {
////                if(Interlocked.CompareExchange( ref m_lock, 1, 0 ) == 0)
////                {
////                    bLockTaken = true;
////                    try
////                    {
////                        if(m_timerDeleted != 0)
////                        {
////                            throw new ObjectDisposedException( null, Environment.GetResourceString( "ObjectDisposed_Generic" ) );
////                        }
////
////                        status = ChangeTimerNative( dueTime, period );
////                    }
////                    finally
////                    {
////                        m_lock = 0;
////                    }
////                }
////
////                Thread.SpinWait( 1 );     // yield to processor
////            }
////            while(!bLockTaken);
////        }
////
////        return status;
////
////    }
////
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////    internal bool Dispose( WaitHandle notifyObject )
////    {
////        bool status     = false;
////        bool bLockTaken = false;
////
////        RuntimeHelpers.PrepareConstrainedRegions();
////        try
////        {
////        }
////        finally
////        {
////            do
////            {
////                if(Interlocked.CompareExchange( ref m_lock, 1, 0 ) == 0)
////                {
////                    bLockTaken = true;
////
////                    try
////                    {
////                        status = DeleteTimerNative( notifyObject.SafeWaitHandle );
////                    }
////                    finally
////                    {
////                        m_lock = 0;
////                    }
////                }
////
////                Thread.SpinWait( 1 );     // yield to processor
////            }
////            while(!bLockTaken);
////            GC.SuppressFinalize( this );
////        }
////
////        return status;
////    }
////
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    public void Dispose()
////    {
////        bool bLockTaken = false;
////
////        RuntimeHelpers.PrepareConstrainedRegions();
////        try
////        {
////        }
////        finally
////        {
////            do
////            {
////                if(Interlocked.CompareExchange( ref m_lock, 1, 0 ) == 0)
////                {
////                    bLockTaken = true;
////
////                    try
////                    {
////                        DeleteTimerNative( null );
////                    }
////                    finally
////                    {
////                        m_lock = 0;
////                    }
////                }
////
////                Thread.SpinWait( 1 );     // yield to processor
////            }
////            while(!bLockTaken);
////            GC.SuppressFinalize( this );
////        }
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void AddTimerNative( Object             state     ,
////                                        UInt32             dueTime   ,
////                                        UInt32             period    ,
////                                        ref StackCrawlMark stackMark );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern bool ChangeTimerNative( UInt32 dueTime, UInt32 period );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern bool DeleteTimerNative( SafeHandle notifyObject );
////
////}

////[HostProtection( Synchronization = true, ExternalThreading = true )]
    public sealed class Timer : MarshalByRefObject, IDisposable
    {
////    private const UInt32 MAX_SUPPORTED_TIMEOUT = (uint)0xfffffffe;
////
////    private TimerBase timerBase;
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern Timer( TimerCallback callback ,
                             Object        state    ,
                             int           dueTime  ,
                             int           period   );
////    {
////        if(dueTime < -1)
////        {
////            throw new ArgumentOutOfRangeException( "dueTime", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        if(period < -1)
////        {
////            throw new ArgumentOutOfRangeException( "period", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        TimerSetup( callback, state, (UInt32)dueTime, (UInt32)period, ref stackMark );
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern Timer( TimerCallback callback ,
                             Object        state    ,
                             TimeSpan      dueTime  ,
                             TimeSpan      period   );
////    {
////        long dueTm = (long)dueTime.TotalMilliseconds;
////        if(dueTm < -1)
////        {
////            throw new ArgumentOutOfRangeException( "dueTm", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////        if(dueTm > MAX_SUPPORTED_TIMEOUT)
////        {
////            throw new ArgumentOutOfRangeException( "dueTm", Environment.GetResourceString( "ArgumentOutOfRange_TimeoutTooLarge" ) );
////        }
////
////        long periodTm = (long)period.TotalMilliseconds;
////        if(periodTm < -1)
////        {
////            throw new ArgumentOutOfRangeException( "periodTm", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////        if(periodTm > MAX_SUPPORTED_TIMEOUT)
////        {
////            throw new ArgumentOutOfRangeException( "periodTm", Environment.GetResourceString( "ArgumentOutOfRange_PeriodTooLarge" ) );
////        }
////
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        TimerSetup( callback, state, (UInt32)dueTm, (UInt32)periodTm, ref stackMark );
////    }
    
////    [CLSCompliant( false )]
////    public Timer( TimerCallback callback,
////                  Object        state   ,
////                  UInt32        dueTime ,
////                  UInt32        period  )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        TimerSetup( callback, state, dueTime, period, ref stackMark );
////    }
////
////    public Timer( TimerCallback callback,
////                  Object        state   ,
////                  long          dueTime ,
////                  long          period  )
////    {
////        if(dueTime < -1)
////        {
////            throw new ArgumentOutOfRangeException( "dueTime", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        if(period < -1)
////        {
////            throw new ArgumentOutOfRangeException( "period", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        if(dueTime > MAX_SUPPORTED_TIMEOUT)
////        {
////            throw new ArgumentOutOfRangeException( "dueTime", Environment.GetResourceString( "ArgumentOutOfRange_TimeoutTooLarge" ) );
////        }
////
////        if(period > MAX_SUPPORTED_TIMEOUT)
////        {
////            throw new ArgumentOutOfRangeException( "period", Environment.GetResourceString( "ArgumentOutOfRange_PeriodTooLarge" ) );
////        }
////
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        TimerSetup( callback, state, (UInt32)dueTime, (UInt32)period, ref stackMark );
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern Timer( TimerCallback callback );
////    {
////        int dueTime = -1;   // we want timer to be registered, but not activated.  Requires caller to call
////        int period  = -1;   // Change after a timer instance is created.  This is to avoid the potential
////        // for a timer to be fired before the returned value is assigned to the variable,
////        // potentially causing the callback to reference a bogus value (if passing the timer to the callback).
////
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        TimerSetup( callback, this, (UInt32)dueTime, (UInt32)period, ref stackMark );
////    }
////
////    private void TimerSetup( TimerCallback      callback  ,
////                             Object             state     ,
////                             UInt32             dueTime   ,
////                             UInt32             period    ,
////                             ref StackCrawlMark stackMark )
////    {
////        timerBase = new TimerBase();
////
////        timerBase.AddTimer( callback, state, (UInt32)dueTime, (UInt32)period, ref stackMark );
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern bool Change( int dueTime, int period );
////    {
////        if(dueTime < -1)
////        {
////            throw new ArgumentOutOfRangeException( "dueTime", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        if(period < -1)
////        {
////            throw new ArgumentOutOfRangeException( "period", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        return timerBase.ChangeTimer( (UInt32)dueTime, (UInt32)period );
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern bool Change( TimeSpan dueTime, TimeSpan period );
////    {
////        return Change( (long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds );
////    }
////
////    [CLSCompliant( false )]
////    public bool Change( UInt32 dueTime, UInt32 period )
////    {
////        return timerBase.ChangeTimer( dueTime, period );
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern bool Change( long dueTime, long period );
////    {
////        if(dueTime < -1)
////        {
////            throw new ArgumentOutOfRangeException( "dueTime", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        if(period < -1)
////        {
////            throw new ArgumentOutOfRangeException( "period", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
////        }
////
////        if(dueTime > MAX_SUPPORTED_TIMEOUT)
////        {
////            throw new ArgumentOutOfRangeException( "dueTime", Environment.GetResourceString( "ArgumentOutOfRange_TimeoutTooLarge" ) );
////        }
////
////        if(period > MAX_SUPPORTED_TIMEOUT)
////        {
////            throw new ArgumentOutOfRangeException( "period", Environment.GetResourceString( "ArgumentOutOfRange_PeriodTooLarge" ) );
////        }
////
////        return timerBase.ChangeTimer( (UInt32)dueTime, (UInt32)period );
////    }
////
////    public bool Dispose( WaitHandle notifyObject )
////    {
////        if(notifyObject == null)
////        {
////            throw new ArgumentNullException( "notifyObject" );
////        }
////
////        return timerBase.Dispose( notifyObject );
////    }
    
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern void Dispose();
////    {
////        timerBase.Dispose();
////    }
    }
}
