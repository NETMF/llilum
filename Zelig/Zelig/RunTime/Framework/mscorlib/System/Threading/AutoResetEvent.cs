// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: AutoResetEvent
**
**
** Purpose: An example of a WaitHandle class
**
**
=============================================================================*/
namespace System.Threading
{
    using System;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;

    [HostProtection( Synchronization = true, ExternalThreading = true )]
    public sealed class AutoResetEvent : EventWaitHandle
    {
        public AutoResetEvent( bool initialState ) : base( initialState, EventResetMode.AutoReset )
        {
        }
    }
}

