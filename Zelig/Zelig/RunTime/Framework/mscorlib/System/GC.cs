// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  GC
**
**
** Purpose: Exposes features of the Garbage Collector through
** the class libraries.  This is a class which cannot be
** instantiated.
**
**
===========================================================*/
namespace System
{
    //This class only static members and doesn't require the serializable keyword.

    using System;
////using System.Security.Permissions;
    using System.Reflection;
////using System.Security;
    using System.Threading;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
////using System.Reflection.Cache;
////using System.Runtime.Versioning;

    public static class GC
    {
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern int GetGenerationWR( IntPtr handle );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern long nativeGetTotalMemory();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern void nativeCollectGeneration( int generation );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern int nativeGetMaxGeneration();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    private static extern int nativeCollectionCount( int generation );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern bool nativeIsServerGC();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern void nativeAddMemoryPressure( UInt64 bytesAllocated );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern void nativeRemoveMemoryPressure( UInt64 bytesAllocated );
////
////    [SecurityPermission( SecurityAction.LinkDemand, UnmanagedCode = true )]
////    public static void AddMemoryPressure( long bytesAllocated )
////    {
////        if(bytesAllocated <= 0)
////        {
////            throw new ArgumentOutOfRangeException( "bytesAllocated", Environment.GetResourceString( "ArgumentOutOfRange_NeedPosNum" ) );
////        }
////
////        if((IntPtr.Size == 4) && (bytesAllocated > Int32.MaxValue))
////        {
////            throw new ArgumentOutOfRangeException( "pressure", Environment.GetResourceString( "ArgumentOutOfRange_MustBeNonNegInt32" ) );
////        }
////
////        nativeAddMemoryPressure( (ulong)bytesAllocated );
////    }
////
////    [SecurityPermission( SecurityAction.LinkDemand, UnmanagedCode = true )]
////    public static void RemoveMemoryPressure( long bytesAllocated )
////    {
////        if(bytesAllocated <= 0)
////        {
////            throw new ArgumentOutOfRangeException( "bytesAllocated", Environment.GetResourceString( "ArgumentOutOfRange_NeedPosNum" ) );
////        }
////
////        if((IntPtr.Size == 4) && (bytesAllocated > Int32.MaxValue))
////        {
////            throw new ArgumentOutOfRangeException( "bytesAllocated", Environment.GetResourceString( "ArgumentOutOfRange_MustBeNonNegInt32" ) );
////        }
////
////        nativeRemoveMemoryPressure( (ulong)bytesAllocated );
////    }
////
////
////    // Returns the generation that obj is currently in.
////    //
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    public static extern int GetGeneration( Object obj );
////
////
////    // Forces a collection of all generations from 0 through Generation.
////    //
////    public static void Collect( int generation )
////    {
////        if(generation < 0)
////        {
////            throw new ArgumentOutOfRangeException( "generation", Environment.GetResourceString( "ArgumentOutOfRange_GenericPositive" ) );
////        }
////
////        nativeCollectGeneration( generation );
////    }
    
        // Garbage Collect all generations.
        //
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void Collect();
////    {
////        //-1 says to GC all generations.
////        nativeCollectGeneration( -1 );
////    }
    
    
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern int CollectionCount( int generation );
////    {
////        if(generation < 0)
////        {
////            throw new ArgumentOutOfRangeException( "generation", Environment.GetResourceString( "ArgumentOutOfRange_GenericPositive" ) );
////        }
////
////        return nativeCollectionCount( generation );
////
////    }
    
    
        // This method DOES NOT DO ANYTHING in and of itself.  It's used to
        // prevent a finalizable object from losing any outstanding references
        // a touch too early.  The JIT is very aggressive about keeping an
        // object's lifetime to as small a window as possible, to the point
        // where a 'this' pointer isn't considered live in an instance method
        // unless you read a value from the instance.  So for finalizable
        // objects that store a handle or pointer and provide a finalizer that
        // cleans them up, this can cause subtle races with the finalizer
        // thread.  This isn't just about handles - it can happen with just
        // about any finalizable resource.
        //
        // Users should insert a call to this method near the end of a
        // method where they must keep an object alive for the duration of that
        // method, up until this method is called.  Here is an example:
        //
        // "...all you really need is one object with a Finalize method, and a
        // second object with a Close/Dispose/Done method.  Such as the following
        // contrived example:
        //
        // class Foo {
        //    Stream stream = ...;
        //    protected void Finalize() { stream.Close(); }
        //    void Problem() { stream.MethodThatSpansGCs(); }
        //    static void Main() { new Foo().Problem(); }
        // }
        //
        //
        // In this code, Foo will be finalized in the middle of
        // stream.MethodThatSpansGCs, thus closing a stream still in use."
        //
        // If we insert a call to GC.KeepAlive(this) at the end of Problem(), then
        // Foo doesn't get finalized and the stream says open.
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public static extern void KeepAlive( Object obj );
    
////    // Returns the generation in which wo currently resides.
////    //
////    public static int GetGeneration( WeakReference wo )
////    {
////        int result = GetGenerationWR( wo.m_handle );
////
////        KeepAlive( wo );
////
////        return result;
////    }
////
////    // Returns the maximum GC generation.  Currently assumes only 1 heap.
////    //
////    public static int MaxGeneration
////    {
////        get
////        {
////            return nativeGetMaxGeneration();
////        }
////    }
    
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void WaitForPendingFinalizers();
    
////    // Indicates that the system should not call the Finalize() method on
////    // an object that would normally require this call.
////    // Has the DynamicSecurityMethodAttribute custom attribute to prevent
////    // inlining of the caller.
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    internal static extern void nativeSuppressFinalize( Object o );
    
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void SuppressFinalize( Object obj );
////    {
////        if(obj == null)
////        {
////            throw new ArgumentNullException( "obj" );
////        }
////
////        nativeSuppressFinalize( obj );
////    }
    
////    // Indicates that the system should call the Finalize() method on an object
////    // for which SuppressFinalize has already been called. The other situation
////    // where calling ReRegisterForFinalize is useful is inside a finalizer that
////    // needs to resurrect itself or an object that it references.
////    // Has the DynamicSecurityMethodAttribute custom attribute to prevent
////    // inlining of the caller.
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern void nativeReRegisterForFinalize( Object o );
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern void ReRegisterForFinalize( Object obj );
////    {
////        if(obj == null)
////        {
////            throw new ArgumentNullException( "obj" );
////        }
////
////        nativeReRegisterForFinalize( obj );
////    }
    
        // Returns the total number of bytes currently in use by live objects in
        // the GC heap.  This does not return the total size of the GC heap, but
        // only the live objects in the GC heap.
        //
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern long GetTotalMemory( bool forceFullCollection );
////    {
////        long size = nativeGetTotalMemory();
////
////        if(!forceFullCollection)
////        {
////            return size;
////        }
////
////        // If we force a full collection, we will run the finalizers on all
////        // existing objects and do a collection until the value stabilizes.
////        // The value is "stable" when either the value is within 5% of the
////        // previous call to nativeGetTotalMemory, or if we have been sitting
////        // here for more than x times (we don't want to loop forever here).
////        int  reps    = 20;  // Number of iterations
////        long newSize = size;
////        float diff;
////        do
////        {
////            GC.WaitForPendingFinalizers();
////            GC.Collect();
////
////            size    = newSize;
////            newSize = nativeGetTotalMemory();
////            diff = ((float)(newSize - size)) / size;
////        } while(reps-- > 0 && !(-.05 < diff && diff < .05));
////
////        return newSize;
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern void SetCleanupCache();
////
////    private static ClearCacheHandler m_cacheHandler;
////
////    private static readonly Object locker = new Object();
////
////    internal static event ClearCacheHandler ClearCache
////    {
////        add
////        {
////            lock(locker)
////            {
////                m_cacheHandler += value;
////
////                SetCleanupCache();
////            }
////        }
////        remove
////        {
////            lock(locker)
////            {
////                m_cacheHandler -= value;
////            }
////        }
////    }
////
////    //This method is called from native code.  If you update the signature, please also update
////    //mscorlib.h and COMUtilNative.cpp
////    internal static void FireCacheEvent()
////    {
////        BCLDebug.Trace( "CACHE", "Called FileCacheEvent" );
////        ClearCacheHandler handler = Interlocked.Exchange<ClearCacheHandler>( ref m_cacheHandler, null );
////        if(handler != null)
////        {
////            handler( null, null );
////        }
////    }
    }
}
