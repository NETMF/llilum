//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.HAL
{
    using Runtime;
    using System;
    using System.Runtime.InteropServices;

    public unsafe delegate void TimerCallback( UIntPtr timerContext, ulong time );

    public static class Timer
    {
        public static unsafe uint LLOS_SYSTEM_TIMER_Enable( TimerCallback callback )
        {
            UIntPtr callbackPtr = UIntPtr.Zero;

            if(callback != null)
            {
                DelegateImpl dlg = (DelegateImpl)(object)callback;

                callbackPtr = new UIntPtr( dlg.InnerGetCodePointer( ).Target.ToPointer( ) );
            }

            return LLOS_SYSTEM_TIMER_Enable( callbackPtr );
        }

        [DllImport( "C" )]
        private static unsafe extern uint LLOS_SYSTEM_TIMER_Enable( UIntPtr callback );

        [DllImport( "C" )]
        public static unsafe extern void LLOS_SYSTEM_TIMER_Disable( );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SYSTEM_TIMER_SetTicks( ulong value );

        [DllImport( "C" )]
        public static unsafe extern ulong LLOS_SYSTEM_TIMER_GetTicks( );

        [DllImport( "C" )]
        public static unsafe extern ulong LLOS_SYSTEM_TIMER_GetTimerFrequency( );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SYSTEM_TIMER_AllocateTimer( UIntPtr timerContext, TimerContext** pTimer );

        [DllImport( "C" )]
        public static unsafe extern void LLOS_SYSTEM_TIMER_FreeTimer( TimerContext* pTimer );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SYSTEM_TIMER_ScheduleTimer( TimerContext* pTimer, ulong microsecondsFromNow );
    }

    public unsafe struct TimerContext
    {
    };
}
