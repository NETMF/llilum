//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.HAL
{
    using Runtime;
    using System;
    using System.Runtime.InteropServices;

    public unsafe delegate void TimerCallback( UIntPtr context, ulong time );

    public static class Timer
    {
        public static unsafe uint LLOS_SYSTEM_TIMER_AllocateTimer( TimerCallback callback, UIntPtr callbackContext, ulong microsecondsFromNow, TimerContext** pTimer )
        {
            UIntPtr callbackPtr = UIntPtr.Zero;
            UIntPtr callbackCtx = UIntPtr.Zero;

            if(callback != null)
            {
                DelegateImpl dlg = (DelegateImpl)(object)callback;

                callbackPtr = new UIntPtr( dlg.InnerGetCodePointer( ).Target.ToPointer( ) );

                callbackCtx = ( (ObjectImpl)callback.Target ).ToPointer( );
            }

            return LLOS_SYSTEM_TIMER_AllocateTimer( callbackPtr, callbackContext != UIntPtr.Zero ? callbackContext : callbackCtx, microsecondsFromNow, pTimer );
        }

        [DllImport( "C" )]
        public static unsafe extern ulong LLOS_SYSTEM_TIMER_GetTicks( TimerContext* pTimer );


        [DllImport( "C" )]
        public static unsafe extern ulong LLOS_SYSTEM_TIMER_GetTimerFrequency( TimerContext* pTimer );

        [DllImport( "C" )]
        private static unsafe extern uint LLOS_SYSTEM_TIMER_AllocateTimer( UIntPtr callback, UIntPtr callbackContext, ulong microsecondsFromNow, TimerContext** pTimer );

        [DllImport( "C" )]
        public static unsafe extern void LLOS_SYSTEM_TIMER_FreeTimer( TimerContext* pTimer );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SYSTEM_TIMER_ScheduleTimer( TimerContext* pTimer, ulong microsecondsFromNow );
    }

    public unsafe struct TimerContext
    {
    };
}
