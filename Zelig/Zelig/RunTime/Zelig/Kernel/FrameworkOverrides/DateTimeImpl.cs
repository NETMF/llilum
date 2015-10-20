//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.DateTime), NoConstructors=true)]
    public struct DateTimeImpl
    {
        //
        // State
        //

        static SchedulerTimeSpan s_reference = (SchedulerTime)new DateTime( 2007, 1, 1 ) - SchedulerTime.MinValue;

        //
        // Helper Methods
        //

        internal static ulong GetSystemTimeAsDateTimeTicks()
        {
            SchedulerTime now  = SchedulerTime.Now + s_reference;
            DateTime      now2 = (DateTime)now;

            return (ulong)now2.Ticks;
        }

        public static void SetUtcTime( DateTime newTime )
        {
            SchedulerTime now  = SchedulerTime.Now;
            TimeSpan      diff = newTime - (DateTime)now;

            s_reference = (SchedulerTimeSpan)diff;
        }
    }
}
