//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.HAL
{
    using Runtime;
    using System;
    using System.Runtime.InteropServices;

    public static class Thread
    {
        public enum ThreadPriority
        {
            Lowest = 0,
            BelowNormal,
            Normal,
            AboveNormal,
            Highest,
        }

        public static unsafe uint LLOS_THREAD_CreateThread(Delegate dlgEntry, ThreadImpl thread, ref UIntPtr threadHandle )
        {
            UIntPtr threadEntry;
            UIntPtr threadParam;
            UIntPtr managedThread;

            DelegateImpl dlg = (DelegateImpl)(object)dlgEntry;
            threadEntry = new UIntPtr( dlg.InnerGetCodePointer( ).Target.ToPointer( ) );
            threadParam = ( (ObjectImpl)dlgEntry.Target ).ToPointer( );
            managedThread = ( (ObjectImpl)(object)thread ).ToPointer( );

            return LLOS_THREAD_CreateThread( threadEntry, threadParam, managedThread, 8*1024, ref threadHandle );
        }

        //--//

        [DllImport( "C" )]
        private static unsafe extern uint LLOS_THREAD_CreateThread( UIntPtr threadEntry, UIntPtr threadParameter, UIntPtr managedThread, uint stackSize, ref UIntPtr threadHandle );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_THREAD_DeleteThread( UIntPtr threadHandle );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_THREAD_Start( UIntPtr threadHandle );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_THREAD_Yield( );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_THREAD_Signal(UIntPtr threadHandle);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_THREAD_Wait(UIntPtr threadHandle, int timeoutMs);

        [DllImport( "C" )]
        public static unsafe extern void LLOS_THREAD_Sleep( int timeoutMs );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_THREAD_GetCurrentThread( ref UIntPtr threadHandle );
        [DllImport( "C" )]
        public static unsafe extern uint LLOS_THREAD_SetPriority( UIntPtr threadHandle, ThreadPriority threadPriority );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_THREAD_GetPriority( UIntPtr threadHandle, out ThreadPriority threadPriority );
        
        [DllImport( "C" )]
        public static unsafe extern uint LLOS_THREAD_GetMainStackAddress( );
        
        [DllImport( "C" )]
        public static unsafe extern uint LLOS_THREAD_GetMainStackSize( );
    }
}
