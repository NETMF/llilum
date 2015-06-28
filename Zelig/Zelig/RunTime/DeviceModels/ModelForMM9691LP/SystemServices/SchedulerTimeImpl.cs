//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    //
    // We use the RTC time resolution for the internal time.
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
                //
                // The RTC clock frequency is 32768.
                //
                //  => 32768 / 1000 == 4096 / 125
                //

                ulong res = (uint)milliSeconds * 4096;
                
                res /= 125;

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
            return Drivers.RealTimeClock.Instance.CurrentTime;
        }

        //
        // Access Methods
        //

        static int HardwareClockFrequency
        {
            [RT.Inline]
            get
            {
                return (int)RT.Configuration.RealTimeClockFrequency;
            }
        }

        static double RatioFromMillisecondsToUnits
        {
            [RT.Inline]
            get
            {
                return (double)HardwareClockFrequency / (double)1000;
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
