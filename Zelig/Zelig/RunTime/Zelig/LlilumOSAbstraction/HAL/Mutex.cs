//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.HAL
{
    using System;
    using System.Runtime.InteropServices;

    public static class Mutex
    {
        [DllImport( "C" )]
        public static unsafe extern uint LLOS_MUTEX_CreateGlobalLock( ref UIntPtr mutexHandle );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_MUTEX_Create( UIntPtr attributes, UIntPtr name, ref UIntPtr mutexHandle );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_MUTEX_Acquire( UIntPtr mutexHandle, int timeout );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_MUTEX_Release( UIntPtr mutexHandle );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_MUTEX_CurrentThreadHasLock( UIntPtr mutexHandle );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_MUTEX_Delete( UIntPtr mutexHandle );
    }
}
