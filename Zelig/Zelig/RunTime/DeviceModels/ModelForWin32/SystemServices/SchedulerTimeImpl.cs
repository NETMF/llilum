//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED
{
    using System;

    using RT = Microsoft.Zelig.Runtime;
    using LLOS = Zelig.LlilumOSAbstraction;


    //
    // We use the 1MHz timer for the internal time.
    //
    [RT.ExtendClass(typeof(RT.SchedulerTime), NoConstructors=true)]
    public struct SchedulerTimeImpl
    {
        //
        // Helper Methods
        //

        private static ulong ConvertFromMillisecondsToUnits( int milliSeconds )
        {
            if(milliSeconds < 0)
            {
                return ulong.MaxValue;
            }
            else
            {
                ulong res = (uint)milliSeconds * (uint)HardwareClockFrequency / 1000;
                
                return GetCurrentTime() + res;
            }
        }

        private static ulong ConvertFromTimeSpanTicksToUnits( long ticks )
        {
            if(ticks < 0)
            {
                return ulong.MaxValue;
            }
            else
            {
                ulong res = ConvertFromDateTimeTicksToUnits( ticks );

                return GetCurrentTime() + res;
            }
        }

        private static ulong ConvertFromDateTimeTicksToUnits( long ticks )
        {
            return (ulong)((double)ticks * RatioFromDateTimeTicksToUnits);
        }

        private static long ConvertFromUnitsToDateTimeTicks( ulong units )
        {
            return (long)((double)units * RatioFromUnitsToDateTimeTicks);
        }

        private static ulong GetCurrentTime()
        {
            return LLOS.HAL.Clock.LLOS_CLOCK_GetClockTicks();
        }

        //
        // Access Methods
        //

        static unsafe int HardwareClockFrequency
        {
            [RT.Inline]
            get
            {
                return (int)LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_GetTimerFrequency( null );
            }
        }

        static double RatioFromMillisecondsToUnits
        {
            [RT.Inline]
            get
            {
                return (double)HardwareClockFrequency / 1000;
            }
        }

        static double RatioFromDateTimeTicksToUnits
        {
            [RT.Inline]
            get
            {
                return (double)HardwareClockFrequency / (double)TimeSpan.TicksPerSecond;
            }
        }

        static double RatioFromUnitsToMilliseconds
        {
            [RT.Inline]
            get
            {
                return 1.0 / RatioFromMillisecondsToUnits;
            }
        }

        static double RatioFromUnitsToDateTimeTicks
        {
            [RT.Inline]
            get
            {
                return 1.0 / RatioFromDateTimeTicksToUnits;
            }
        }
    }
}
