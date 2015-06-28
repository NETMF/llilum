// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: Monitor
**
**
** Purpose: Synchronizes access to a shared resource or region of code in a multi-threaded
**             program.
**
**
=============================================================================*/


namespace System.Threading
{
    using System;
////using System.Security.Permissions;
////using System.Runtime.Remoting;
    using System.Threading;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
////using System.Runtime.Versioning;

////[HostProtection( Synchronization = true, ExternalThreading = true )]
    public static class Monitor
    {
        /*=========================================================================
        ** Obtain the monitor lock of obj. Will block if another thread holds the lock
        ** Will not block if the current thread holds the lock,
        ** however the caller must ensure that the same number of Exit
        ** calls are made as there were Enter calls.
        **
        ** Exceptions: ArgumentNullException if object is null.
        =========================================================================*/
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void Enter( Object obj );

////    // This should be made public in a future version.
////    // Use a ref bool instead of out to ensure that unverifiable code must
////    // initialize this value to something.  If we used out, the value
////    // could be uninitialized if we threw an exception in our prolog.
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern void ReliableEnter( Object obj, ref bool tookLock );

        public static void Enter( Object obj, ref bool lockTaken )
        {
            if(obj == null)
            {
                throw new ArgumentNullException();
            }

            //  The input must be false.
            if (lockTaken == true)
            {
                lockTaken = false;
                throw new ArgumentException();
            }
            
            //The output is true if the lock is acquired; otherwise, the output is 
            // false. The output is set even if an exception occurs during the attempt 
            // to acquire the lock. 
            Enter(obj);

            lockTaken = true;
        }

        /*=========================================================================
        ** Release the monitor lock. If one or more threads are waiting to acquire the
        ** lock, and the current thread has executed as many Exits as
        ** Enters, one of the threads will be unblocked and allowed to proceed.
        **
        ** Exceptions: ArgumentNullException if object is null.
        **             SynchronizationLockException if the current thread does not
        **             own the lock.
        =========================================================================*/
////    [ResourceExposure( ResourceScope.None )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void Exit( Object obj );

        /*=========================================================================
        ** Similar to Enter, but will never block. That is, if the current thread can
        ** acquire the monitor lock without blocking, it will do so and TRUE will
        ** be returned. Otherwise FALSE will be returned.
        **
        ** Exceptions: ArgumentNullException if object is null.
        =========================================================================*/
        public static bool TryEnter( Object obj )
        {
            return TryEnterTimeout( obj, 0 );
        }

        /*=========================================================================
        ** Version of TryEnter that will block, but only up to a timeout period
        ** expressed in milliseconds. If timeout == Timeout.Infinite the method
        ** becomes equivalent to Enter.
        **
        ** Exceptions: ArgumentNullException if object is null.
        **             ArgumentException if timeout < 0.
        =========================================================================*/
        public static bool TryEnter( Object obj, int millisecondsTimeout )
        {
            return TryEnterTimeout( obj, millisecondsTimeout );
        }

        public static bool TryEnter( Object obj, TimeSpan timeout )
        {
            long tm = (long)timeout.TotalMilliseconds;
            if(tm < -1 || tm > (long)Int32.MaxValue)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "timeout", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            return TryEnterTimeout( obj, (int)tm );
        }

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        private static extern bool TryEnterTimeout( Object obj, int timeout );

        /*========================================================================
    ** Waits for notification from the object (via a Pulse/PulseAll).
    ** timeout indicates how long to wait before the method returns.
    ** This method acquires the monitor waithandle for the object
    ** If this thread holds the monitor lock for the object, it releases it.
    ** On exit from the method, it obtains the monitor lock back.
    ** If exitContext is true then the synchronization domain for the context
    ** (if in a synchronized context) is exited before the wait and reacquired
    **
        ** Exceptions: ArgumentNullException if object is null.
    ========================================================================*/
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        private static extern bool ObjWait( bool exitContext, int millisecondsTimeout, Object obj );

        public static bool Wait( Object obj, int millisecondsTimeout, bool exitContext )
        {
            if(obj == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "obj" );
#else
                throw new ArgumentNullException();
#endif
            }

            return ObjWait( exitContext, millisecondsTimeout, obj );
        }

        public static bool Wait( Object obj, TimeSpan timeout, bool exitContext )
        {
            long tm = (long)timeout.TotalMilliseconds;
            if(tm < -1 || tm > (long)Int32.MaxValue)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "timeout", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            return Wait( obj, (int)tm, exitContext );
        }

        public static bool Wait( Object obj, int millisecondsTimeout )
        {
            return Wait( obj, millisecondsTimeout, false );
        }

        public static bool Wait( Object obj, TimeSpan timeout )
        {
            long tm = (long)timeout.TotalMilliseconds;
            if(tm < -1 || tm > (long)Int32.MaxValue)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "timeout", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegOrNegative1" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            return Wait( obj, (int)tm, false );
        }

        public static bool Wait( Object obj )
        {
            return Wait( obj, Timeout.Infinite, false );
        }

        /*========================================================================
        ** Sends a notification to a single waiting object.
        * Exceptions: SynchronizationLockException if this method is not called inside
        * a synchronized block of code.
        ========================================================================*/
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        private static extern void ObjPulse( Object obj );

        public static void Pulse( Object obj )
        {
            if(obj == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "obj" );
#else
                throw new ArgumentNullException();
#endif
            }

            ObjPulse( obj );
        }
        /*========================================================================
        ** Sends a notification to all waiting objects.
        ========================================================================*/
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        private static extern void ObjPulseAll( Object obj );

        public static void PulseAll( Object obj )
        {
            if(obj == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "obj" );
#else
                throw new ArgumentNullException();
#endif
            }

            ObjPulseAll( obj );
        }
    }
}
