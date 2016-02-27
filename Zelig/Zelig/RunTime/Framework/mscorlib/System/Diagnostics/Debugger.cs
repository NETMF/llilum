// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// The Debugger class is a part of the System.Diagnostics package
// and is used for communicating with a debugger.

namespace System.Diagnostics
{
    using System;
////using System.IO;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.CompilerServices;
////using System.Security;
////using System.Security.Permissions;
////using System.Runtime.Versioning;


    // No data, does not need to be marked with the serializable attribute
    public sealed class Debugger
    {

        // Break causes a breakpoint to be signalled to an attached debugger.  If no debugger
        // is attached, the user is asked if he wants to attach a debugger. If yes, then the 
        // debugger is launched.
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern static void Break();
////    {
////        if(!IsDebuggerAttached())
////        {
////            // Try and demand UnmanagedCodePermission.  This is done in a try block because if this
////            // fails we want to be able to silently eat the exception and just return so
////            // that the call to Break does not possibly cause an unhandled exception.
////            // The idea here is that partially trusted code shouldn't be able to launch a debugger 
////            // without the user going through Watson.
////            try
////            {
////                new SecurityPermission( SecurityPermissionFlag.UnmanagedCode ).Demand();
////            }
////
////            // If we enter this block, we do not have permission to break into the debugger
////            // and so we just return.
////            catch(SecurityException)
////            {
////                return;
////            }
////        }
////
////        // Causing a break is now allowed.
////        BreakInternal();
////    }

////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    static void BreakCanThrow()
////    {
////        if(!IsDebuggerAttached())
////        {
////            new SecurityPermission( SecurityPermissionFlag.UnmanagedCode ).Demand();
////        }
////
////        // Causing a break is now allowed.
////        BreakInternal();
////    }
////
////    [ResourceExposure( ResourceScope.Process )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern void BreakInternal();

////    // Launch launches & attaches a debugger to the process. If a debugger is already attached,
////    // nothing happens.  
////    //
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static bool Launch()
////    {
////        if(IsDebuggerAttached())
////            return (true);
////
////        // Try and demand UnmanagedCodePermission.  This is done in a try block because if this
////        // fails we want to be able to silently eat the exception and just return so
////        // that the call to Break does not possibly cause an unhandled exception.
////        // The idea here is that partially trusted code shouldn't be able to launch a debugger 
////        // without the user going through Watson.
////        try
////        {
////            new SecurityPermission( SecurityPermissionFlag.UnmanagedCode ).Demand();
////        }
////
////        // If we enter this block, we do not have permission to break into the debugger
////        // and so we just return.
////        catch(SecurityException)
////        {
////            return (false);
////        }
////
////        // Causing the debugger to launch is now allowed.
////        return (LaunchInternal());
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static extern bool LaunchInternal();

        // Returns whether or not a debugger is attached to the process.
        //
        public static bool IsAttached
        {
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
            get
            {
                return IsDebuggerAttached();
            }
        }

////    [ResourceExposure( ResourceScope.Process )]
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        private static extern bool IsDebuggerAttached();

        // Constants representing the importance level of messages to be logged.
        //
        // An attached debugger can enable or disable which messages will
        // actually be reported to the user through the COM+ debugger
        // services API.  This info is communicated to the runtime so only
        // desired events are actually reported to the debugger.  
        //
        // Constant representing the default category
        public static readonly String DefaultCategory = null;

        // Posts a message for the attached debugger.  If there is no
        // debugger attached, has no effect.  The debugger may or may not
        // report the message depending on its settings. 
////    [ResourceExposure( ResourceScope.None )]
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        public static extern void Log( int level, String category, String message );
    }
}
