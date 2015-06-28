// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  CriticalFinalizerObject
**
**
** Deriving from this class will cause any finalizer you define to be critical
** (i.e. the finalizer is guaranteed to run, won't be aborted by the host and is
** run after the finalizers of other objects collected at the same time).
**
** You must possess UnmanagedCode permission in order to derive from this class.
**
**
===========================================================*/

namespace System.Runtime.ConstrainedExecution
{
    using System;
////using System.Security.Permissions;
    using System.Runtime.InteropServices;


////[SecurityPermission( SecurityAction.InheritanceDemand, UnmanagedCode = true )]
    public abstract class CriticalFinalizerObject
    {
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        protected CriticalFinalizerObject()
        {
        }

////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        ~CriticalFinalizerObject()
        {
        }
    }
}
