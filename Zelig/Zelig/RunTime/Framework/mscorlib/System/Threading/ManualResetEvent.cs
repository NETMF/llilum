// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: ManualResetEvent
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
    public sealed class ManualResetEvent : EventWaitHandle
    {
        public ManualResetEvent( bool initialState ) : base( initialState, EventResetMode.ManualReset )
        {
        }
    }
}

