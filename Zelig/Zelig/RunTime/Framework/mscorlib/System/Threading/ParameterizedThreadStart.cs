// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: ParameterizedThreadStart
**
**
** Purpose: This class is a Delegate which defines the start method
**  for starting a thread.  That method must match this delegate.
**
**
=============================================================================*/


namespace System.Threading
{
    using System.Threading;
////using System.Security.Permissions;
    using System.Runtime.InteropServices;

    public delegate void ParameterizedThreadStart( object obj );
}
