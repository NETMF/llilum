// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: EventWaitHandle
**
**
** Purpose: Base class for representing Events
**
**
=============================================================================*/

namespace System.Threading
{
    using System;
    using System.Threading;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
////using System.IO;
////using Microsoft.Win32;
////using Microsoft.Win32.SafeHandles;
    using System.Runtime.InteropServices;
////using System.Runtime.Versioning;
////using System.Security.AccessControl;

    [HostProtection( Synchronization = true, ExternalThreading = true )]
    public class EventWaitHandle : WaitHandle
    {
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern EventWaitHandle( bool initialState, EventResetMode mode );
////        : this( initialState, mode, null )
////    {
////    }
    
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public EventWaitHandle( bool initialState, EventResetMode mode, string name )
////    {
////        if(name != null && name.Length > System.IO.Path.MAX_PATH)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_WaitHandleNameTooLong", name ) );
////        }
////
////        SafeWaitHandle handle = null;
////        switch(mode)
////        {
////            case EventResetMode.ManualReset:
////                handle = Win32Native.CreateEvent( null, true, initialState, name );
////                break;
////
////            case EventResetMode.AutoReset:
////                handle = Win32Native.CreateEvent( null, false, initialState, name );
////                break;
////
////            default:
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidFlag", name ) );
////        };
////
////        if(handle.IsInvalid)
////        {
////            int errorCode = Marshal.GetLastWin32Error();
////
////            handle.SetHandleAsInvalid();
////
////            if(name != null && name.Length != 0 && errorCode == Win32Native.ERROR_INVALID_HANDLE)
////            {
////                throw new WaitHandleCannotBeOpenedException( Environment.GetResourceString( "Threading.WaitHandleCannotBeOpenedException_InvalidHandle", name ) );
////            }
////
////            __Error.WinIOError( errorCode, "" );
////        }
////
////        SetHandleInternal( handle );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public EventWaitHandle( bool initialState, EventResetMode mode, string name, out bool createdNew )
////        : this( initialState, mode, name, out createdNew, null )
////    {
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public unsafe EventWaitHandle( bool initialState, EventResetMode mode, string name, out bool createdNew, EventWaitHandleSecurity eventSecurity )
////    {
////        if(name != null && name.Length > System.IO.Path.MAX_PATH)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_WaitHandleNameTooLong", name ) );
////        }
////
////        Win32Native.SECURITY_ATTRIBUTES secAttrs = null;
////
////        // For ACL's, get the security descriptor from the EventWaitHandleSecurity.
////        if(eventSecurity != null)
////        {
////            secAttrs         = new Win32Native.SECURITY_ATTRIBUTES();
////            secAttrs.nLength = (int)Marshal.SizeOf( secAttrs );
////
////            byte[] sd = eventSecurity.GetSecurityDescriptorBinaryForm();
////            byte* pSecDescriptor = stackalloc byte[sd.Length];
////            Buffer.memcpy( sd, 0, pSecDescriptor, 0, sd.Length );
////            secAttrs.pSecurityDescriptor = pSecDescriptor;
////        }
////
////        SafeWaitHandle handle = null;
////        Boolean isManualReset;
////        switch(mode)
////        {
////            case EventResetMode.ManualReset:
////                isManualReset = true;
////                break;
////
////            case EventResetMode.AutoReset:
////                isManualReset = false;
////                break;
////
////            default:
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidFlag", name ) );
////        };
////
////        handle = Win32Native.CreateEvent( secAttrs, isManualReset, initialState, name );
////
////        int errorCode = Marshal.GetLastWin32Error();
////
////        if(handle.IsInvalid)
////        {
////            handle.SetHandleAsInvalid();
////
////            if(name != null && name.Length != 0 && errorCode == Win32Native.ERROR_INVALID_HANDLE)
////            {
////                throw new WaitHandleCannotBeOpenedException( Environment.GetResourceString( "Threading.WaitHandleCannotBeOpenedException_InvalidHandle", name ) );
////            }
////
////            __Error.WinIOError( errorCode, name );
////        }
////
////        createdNew = errorCode != Win32Native.ERROR_ALREADY_EXISTS;
////        SetHandleInternal( handle );
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    private EventWaitHandle( SafeWaitHandle handle )
////    {
////        SetHandleInternal( handle );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static EventWaitHandle OpenExisting( string name )
////    {
////        return OpenExisting( name, EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode )]
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static EventWaitHandle OpenExisting( string name, EventWaitHandleRights rights )
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
////
////        if(name != null && name.Length > System.IO.Path.MAX_PATH)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_WaitHandleNameTooLong", name ) );
////        }
////
////
////        SafeWaitHandle myHandle = Win32Native.OpenEvent( (int)rights, false, name );
////
////        if(myHandle.IsInvalid)
////        {
////            int errorCode = Marshal.GetLastWin32Error();
////
////            if(errorCode == Win32Native.ERROR_FILE_NOT_FOUND || errorCode == Win32Native.ERROR_INVALID_NAME)
////            {
////                throw new WaitHandleCannotBeOpenedException();
////            }
////
////            if(name != null && name.Length != 0 && errorCode == Win32Native.ERROR_INVALID_HANDLE)
////            {
////                throw new WaitHandleCannotBeOpenedException( Environment.GetResourceString( "Threading.WaitHandleCannotBeOpenedException_InvalidHandle", name ) );
////            }
////
////            //this is for passed through Win32Native Errors
////            __Error.WinIOError( errorCode, "" );
////        }
////
////        return new EventWaitHandle( myHandle );
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern bool Reset();
////    {
////        bool res = Win32Native.ResetEvent( safeWaitHandle );
////        if(!res)
////        {
////            __Error.WinIOError();
////        }
////
////        return res;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern bool Set();
////    {
////        bool res = Win32Native.SetEvent( safeWaitHandle );
////        if(!res)
////        {
////            __Error.WinIOError();
////        }
////
////        return res;
////    }
////
////    public EventWaitHandleSecurity GetAccessControl()
////    {
////        return new EventWaitHandleSecurity( safeWaitHandle, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group );
////    }
////
////    public void SetAccessControl( EventWaitHandleSecurity eventSecurity )
////    {
////        if(eventSecurity == null)
////        {
////            throw new ArgumentNullException( "eventSecurity" );
////        }
////
////        eventSecurity.Persist( safeWaitHandle );
////    }
    }
}

