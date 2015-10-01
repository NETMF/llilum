// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: ThreadStart
**
**
** Purpose: This class is a Delegate which defines the start method
**  for starting a thread.  That method must match this delegate.
**
**
=============================================================================*/

namespace System.Threading
{
////using System.Security.Permissions;
    using System.Threading;

    // Define the delegate
    // NOTE: If you change the signature here, there is code in COMSynchronization
    //  that invokes this delegate in native.
    public delegate void ThreadStart();
}
