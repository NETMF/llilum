// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: Mutex
**
**
** Purpose: synchronization primitive that can also be used for interprocess synchronization
**
**
=============================================================================*/
namespace System.Threading
{
    using System;
    using System.Threading;
    using System.Runtime.CompilerServices;
////using System.Security.Permissions;
////using System.IO;
////using Microsoft.Win32;
////using Microsoft.Win32.SafeHandles;
    using System.Runtime.InteropServices;
    using System.Runtime.ConstrainedExecution;
////using System.Runtime.Versioning;
////using System.Security.Principal;
////using System.Security;
////using System.Security.AccessControl;

////[HostProtection( Synchronization = true, ExternalThreading = true )]
    public sealed class Mutex : WaitHandle
    {
////    static bool dummyBool;
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////    public Mutex( bool initiallyOwned, String name, out bool createdNew )
////        : this( initiallyOwned, name, out createdNew, null )
////    {
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public unsafe Mutex( bool initiallyOwned, String name, out bool createdNew, MutexSecurity mutexSecurity )
////    {
////        if(name != null && name.Length < System.IO.Path.MAX_PATH)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_WaitHandleNameTooLong", name ) );
////        }
////
////        Win32Native.SECURITY_ATTRIBUTES secAttrs = null;
////
////        // For ACL's, get the security descriptor from the MutexSecurity.
////        if(mutexSecurity != null)
////        {
////            secAttrs = new Win32Native.SECURITY_ATTRIBUTES();
////            secAttrs.nLength = (int)Marshal.SizeOf( secAttrs );
////
////            byte[] sd = mutexSecurity.GetSecurityDescriptorBinaryForm();
////            byte* pSecDescriptor = stackalloc byte[sd.Length];
////            Buffer.memcpy( sd, 0, pSecDescriptor, 0, sd.Length );
////            secAttrs.pSecurityDescriptor = pSecDescriptor;
////        }
////
////        SafeWaitHandle mutexHandle = null;
////        bool           newMutex    = false;
////
////        RuntimeHelpers.CleanupCode cleanupCode = new RuntimeHelpers.CleanupCode( MutexCleanupCode );
////        MutexCleanupInfo           cleanupInfo = new MutexCleanupInfo          ( mutexHandle, false );
////
////        RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(
////            delegate( object userData )
////            {  // try block
////                RuntimeHelpers.PrepareConstrainedRegions();
////                try
////                {
////                }
////                finally
////                {
////                    if(initiallyOwned)
////                    {
////                        cleanupInfo.inCriticalRegion = true;
////
////                        Thread.BeginThreadAffinity();
////                        Thread.BeginCriticalRegion();
////                    }
////                }
////
////                int errorCode = 0;
////                RuntimeHelpers.PrepareConstrainedRegions();
////                try
////                {
////                }
////                finally
////                {
////                    errorCode = CreateMutexHandle( initiallyOwned, name, secAttrs, out mutexHandle );
////                }
////
////                if(mutexHandle.IsInvalid)
////                {
////                    mutexHandle.SetHandleAsInvalid();
////                    if(name != null && name.Length != 0 && errorCode == Win32Native.ERROR_INVALID_HANDLE)
////                    {
////                        throw new WaitHandleCannotBeOpenedException( Environment.GetResourceString( "Threading.WaitHandleCannotBeOpenedException_InvalidHandle", name ) );
////                    }
////
////                    __Error.WinIOError( errorCode, name );
////                }
////
////                newMutex = errorCode != Win32Native.ERROR_ALREADY_EXISTS;
////
////                SetHandleInternal( mutexHandle );
////                mutexHandle.SetAsMutex();
////
////                hasThreadAffinity = true;
////
////            },
////            cleanupCode,
////            cleanupInfo );
////
////        createdNew = newMutex;
////    }
////
////    [PrePrepareMethod]
////    private void MutexCleanupCode( Object userData, bool exceptionThrown )
////    {
////        MutexCleanupInfo cleanupInfo = (MutexCleanupInfo)userData;
////
////        // If hasThreadAffinity isn’t true, we’ve thrown an exception in the above try, and we must free the mutex
////        // on this OS thread before ending our thread affninity.
////        if(!hasThreadAffinity)
////        {
////            if(cleanupInfo.mutexHandle != null && !cleanupInfo.mutexHandle.IsInvalid)
////            {
////                if(cleanupInfo.inCriticalRegion)
////                {
////                    Win32Native.ReleaseMutex( cleanupInfo.mutexHandle );
////                }
////
////                cleanupInfo.mutexHandle.Dispose();
////            }
////
////            if(cleanupInfo.inCriticalRegion)
////            {
////                Thread.EndCriticalRegion();
////                Thread.EndThreadAffinity();
////            }
////        }
////    }
////
////    internal class MutexCleanupInfo
////    {
////        internal SafeWaitHandle mutexHandle;
////        internal bool           inCriticalRegion;
////
////        internal MutexCleanupInfo( SafeWaitHandle mutexHandle, bool inCriticalRegion )
////        {
////            this.mutexHandle      = mutexHandle;
////            this.inCriticalRegion = inCriticalRegion;
////        }
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public Mutex( bool initiallyOwned, String name ) : this( initiallyOwned, name, out dummyBool )
////    {
////    }
////
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////    public Mutex( bool initiallyOwned ) : this( initiallyOwned, null, out dummyBool )
////    {
////    }
////
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////    public Mutex() : this( false, null, out dummyBool )
////    {
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    private Mutex( SafeWaitHandle handle )
////    {
////        SetHandleInternal( handle );
////        handle.SetAsMutex();
////        hasThreadAffinity = true;
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static Mutex OpenExisting( string name )
////    {
////        return OpenExisting( name, MutexRights.Modify | MutexRights.Synchronize );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static Mutex OpenExisting( string name, MutexRights rights )
////    {
////        if(name == null)
////        {
////            throw new ArgumentNullException( "name", Environment.GetResourceString( "ArgumentNull_WithParamName" ) );
////        }
////
////        if(name.Length == 0)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_EmptyName" ), "name" );
////        }
////        if(name.Length > System.IO.Path.MAX_PATH)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_WaitHandleNameTooLong", name ) );
////        }
////
////
////        // To allow users to view & edit the ACL's, call OpenMutex
////        // with parameters to allow us to view & edit the ACL.  This will
////        // fail if we don't have permission to view or edit the ACL's.
////        // If that happens, ask for less permissions.
////        SafeWaitHandle myHandle = Win32Native.OpenMutex( (int)rights, false, name );
////
////        int errorCode = 0;
////        if(myHandle.IsInvalid)
////        {
////            errorCode = Marshal.GetLastWin32Error();
////
////            if(Win32Native.ERROR_FILE_NOT_FOUND == errorCode || Win32Native.ERROR_INVALID_NAME == errorCode)
////            {
////                throw new WaitHandleCannotBeOpenedException();
////            }
////
////            if(null != name && 0 != name.Length && Win32Native.ERROR_INVALID_HANDLE == errorCode)
////            {
////                throw new WaitHandleCannotBeOpenedException( Environment.GetResourceString( "Threading.WaitHandleCannotBeOpenedException_InvalidHandle", name ) );
////            }
////
////            // this is for passed through Win32Native Errors
////            __Error.WinIOError( errorCode, name );
////        }
////
////        return new Mutex( myHandle );
////    }
////
////    // Note: To call ReleaseMutex, you must have an ACL granting you
////    // MUTEX_MODIFY_STATE rights (0x0001).  The other interesting value
////    // in a Mutex's ACL is MUTEX_ALL_ACCESS (0x1F0001).
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    public void ReleaseMutex()
////    {
////        if(Win32Native.ReleaseMutex( safeWaitHandle ))
////        {
////            Thread.EndCriticalRegion();
////            Thread.EndThreadAffinity();
////        }
////        else
////        {
////            throw new ApplicationException( Environment.GetResourceString( "Arg_SynchronizationLockException" ) );
////        }
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    static int CreateMutexHandle( bool initiallyOwned, String name, Win32Native.SECURITY_ATTRIBUTES securityAttribute, out SafeWaitHandle mutexHandle )
////    {
////        int  errorCode;
////        bool fReservedMutexObtained = false;
////        bool fAffinity              = false;
////
////        while(true)
////        {
////            mutexHandle = Win32Native.CreateMutex( securityAttribute, initiallyOwned, name );
////            errorCode   = Marshal.GetLastWin32Error();
////            if(!mutexHandle.IsInvalid)
////            {
////                break;
////            }
////
////            if(errorCode == Win32Native.ERROR_ACCESS_DENIED)
////            {
////                // If a mutex with the name already exists, OS will try to open it with FullAccess.
////                // It might fail if we don't have enough access. In that case, we try to open the mutex will modify and synchronize access.
////                //
////
////                RuntimeHelpers.PrepareConstrainedRegions();
////                try
////                {
////                    try
////                    {
////                    }
////                    finally
////                    {
////                        Thread.BeginThreadAffinity();
////                        fAffinity = true;
////                    }
////                    AcquireReservedMutex( ref fReservedMutexObtained );
////                    mutexHandle = Win32Native.OpenMutex( Win32Native.MUTEX_MODIFY_STATE | Win32Native.SYNCHRONIZE, false, name );
////                    if(!mutexHandle.IsInvalid)
////                    {
////                        errorCode = Win32Native.ERROR_ALREADY_EXISTS;
////                    }
////                    else
////                    {
////                        errorCode = Marshal.GetLastWin32Error();
////                    }
////                }
////                finally
////                {
////                    if(fReservedMutexObtained)
////                    {
////                        ReleaseReservedMutex();
////                    }
////                    if(fAffinity)
////                    {
////                        Thread.EndThreadAffinity();
////                    }
////                }
////
////                // There could be a race here, the other owner of the mutex can free the mutex,
////                // We need to retry creation in that case.
////                if(errorCode != Win32Native.ERROR_FILE_NOT_FOUND)
////                {
////                    if(errorCode == Win32Native.ERROR_SUCCESS)
////                    {
////                        errorCode = Win32Native.ERROR_ALREADY_EXISTS;
////                    }
////                    break;
////                }
////            }
////            else
////            {
////                break;
////            }
////        }
////        return errorCode;
////    }
////
////    public MutexSecurity GetAccessControl()
////    {
////        return new MutexSecurity( safeWaitHandle, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group );
////    }
////
////    public void SetAccessControl( MutexSecurity mutexSecurity )
////    {
////        if(mutexSecurity == null)
////            throw new ArgumentNullException( "mutexSecurity" );
////
////        mutexSecurity.Persist( safeWaitHandle );
////    }
////
////
////    // Enables workaround for known OS bug at
////    // http://support.microsoft.com/default.aspx?scid=kb;en-us;889318
////    // One machine-wide mutex serializes all OpenMutex and CloseHandle operations.
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    internal static unsafe void AcquireReservedMutex( ref bool bHandleObtained )
////    {
////        SafeWaitHandle mutexHandle = null;
////        int            errorCode;
////
////        bHandleObtained = false;
////        if(s_ReservedMutex == null)
////        {
////
////            // Create a maximally-permissive security descriptor, to ensure we never get an
////            // ACCESS_DENIED error when calling CreateMutex
////            MutexSecurity sec = new MutexSecurity();
////            SecurityIdentifier everyoneSid = new SecurityIdentifier( WellKnownSidType.WorldSid, null );
////            sec.AddAccessRule( new MutexAccessRule( everyoneSid, MutexRights.FullControl, AccessControlType.Allow ) );
////
////            // For ACL's, get the security descriptor from the MutexSecurity.
////            Win32Native.SECURITY_ATTRIBUTES secAttrs = new Win32Native.SECURITY_ATTRIBUTES();
////            secAttrs.nLength = (int)Marshal.SizeOf( secAttrs );
////
////            byte[] sd = sec.GetSecurityDescriptorBinaryForm();
////            byte* bytesOnStack = stackalloc byte[sd.Length];
////            Buffer.memcpy( sd, 0, bytesOnStack, 0, sd.Length );
////            secAttrs.pSecurityDescriptor = bytesOnStack;
////
////            RuntimeHelpers.PrepareConstrainedRegions();
////            try
////            {
////            }
////            finally
////            {
////                mutexHandle = Win32Native.CreateMutex( secAttrs, false, c_ReservedMutexName );
////
////                // need to set specially, since this mutex cannot lock on itself while closing itself.
////                mutexHandle.SetAsReservedMutex();
////            }
////
////            errorCode = Marshal.GetLastWin32Error();
////            if(mutexHandle.IsInvalid)
////            {
////                mutexHandle.SetHandleAsInvalid();
////                __Error.WinIOError( errorCode, c_ReservedMutexName );
////            }
////
////            Mutex m = new Mutex( mutexHandle );
////
////            Interlocked.CompareExchange( ref s_ReservedMutex, m, null );
////        }
////
////
////        RuntimeHelpers.PrepareConstrainedRegions();
////        try
////        {
////        }
////        finally
////        {
////            try
////            {
////                s_ReservedMutex.WaitOne();
////                bHandleObtained = true;
////            }
////            catch(AbandonedMutexException)
////            {
////                // we don't care if another process holding the Mutex was killed
////                bHandleObtained = true;
////            }
////        }
////    }
////
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
////    internal static void ReleaseReservedMutex()
////    {
////        BCLDebug.Assert( s_ReservedMutex != null, "ReleaseReservedMutex called without prior call to AcquireReservedMutex!" );
////
////        s_ReservedMutex.ReleaseMutex();
////    }
    }
}
