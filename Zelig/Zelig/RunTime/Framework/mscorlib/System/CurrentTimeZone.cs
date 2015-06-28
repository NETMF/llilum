// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: CurrentTimeZone
**
**
** Purpose: 
** This class represents the current system timezone.  It is
** the only meaningful implementation of the TimeZone class 
** available in this version.
**
** The only TimeZone that we support in version 1 is the 
** CurrentTimeZone as determined by the system timezone.
**
**
============================================================*/
namespace System
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
////using System.Runtime.Versioning;

    //
    // Currently, this is the only supported timezone.
    // The values of the timezone is from the current system timezone setting in the
    // control panel.
    //
    [Microsoft.Zelig.Internals.WellKnownType( "System_CurrentSystemTimeZone" )]
    [Serializable()]
    internal class CurrentSystemTimeZone : TimeZone
    {
        // <BUGBUG>BUGBUG :
        // One problem is when user changes the current timezone.  We 
        // are not able to update currentStandardName/currentDaylightName/
        // currentDaylightChanges.
        // We need WM_TIMECHANGE to do this or use
        // RegNotifyChangeKeyValue() to monitor </BUGBUG>
        //    
        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const long TicksPerMinute = TicksPerSecond * 60;

////    // The per-year information is cached in in this instance value. As a result it can
////    // be cleaned up by CultureInfo.ClearCachedData, which will clear the instance of this object
////    private Dictionary<Int32, Object> m_CachedDaylightChanges = new Dictionary<Int32, Object>();
        private DaylightTime m_CachedDaylightChanges;
        private int          m_CachedDaylightChangesYear;

        // Standard offset in ticks to the Universal time if
        // no daylight saving is in used.
        // E.g. the offset for PST (Pacific Standard time) should be -8 * 60 * 60 * 1000 * 10000.
        // (1 millisecond = 10000 ticks)
        private long m_ticksOffset;
        private String m_standardName;
        private String m_daylightName;

        internal CurrentSystemTimeZone()
        {
            m_ticksOffset = nativeGetTimeZoneMinuteOffset() * TicksPerMinute;
            m_standardName = null;
            m_daylightName = null;
        }

        public override String StandardName
        {
            get
            {
                if(m_standardName == null)
                {
                    m_standardName = nativeGetStandardName();
                }
                return (m_standardName);
            }
        }

        public override String DaylightName
        {
            get
            {
                if(m_daylightName == null)
                {
                    m_daylightName = nativeGetDaylightName();
                    if(m_daylightName == null)
                    {
                        m_daylightName = this.StandardName;
                    }
                }
                return (m_daylightName);
            }
        }

        internal long GetUtcOffsetFromUniversalTime( DateTime time, ref Boolean isAmbiguousLocalDst )
        {
            // Get the daylight changes for the year of the specified time.
            TimeSpan offset = new TimeSpan( m_ticksOffset );
            DaylightTime daylightTime = GetDaylightChanges( time.Year );
            isAmbiguousLocalDst = false;

            if(daylightTime == null || daylightTime.Delta.Ticks == 0)
            {
                return offset.Ticks;
            }

            // The start and end times represent the range of universal times that are in DST for that year.                
            // Within that there is an ambiguous hour, usually right at the end, but at the beginning in
            // the unusual case of a negative daylight savings delta.
            DateTime startTime = daylightTime.Start - offset;
            DateTime endTime = daylightTime.End - offset - daylightTime.Delta;
            DateTime ambiguousStart;
            DateTime ambiguousEnd;
            if(daylightTime.Delta.Ticks > 0)
            {
                ambiguousStart = endTime - daylightTime.Delta;
                ambiguousEnd = endTime;
            }
            else
            {
                ambiguousStart = startTime;
                ambiguousEnd = startTime - daylightTime.Delta;
            }

            Boolean isDst = false;
            if(startTime > endTime)
            {
                // In southern hemisphere, the daylight saving time starts later in the year, and ends in the beginning of next year.
                // Note, the summer in the southern hemisphere begins late in the year.
                isDst = (time < endTime || time >= startTime);
            }
            else
            {
                // In northern hemisphere, the daylight saving time starts in the middle of the year.
                isDst = (time >= startTime && time < endTime);
            }
            if(isDst)
            {
                offset += daylightTime.Delta;

                // See if the resulting local time becomes ambiguous. This must be captured here or the
                // DateTime will not be able to round-trip back to UTC accurately.
                if(time >= ambiguousStart && time < ambiguousEnd)
                {
                    isAmbiguousLocalDst = true;
                }
            }
            return offset.Ticks;
        }

        public override DateTime ToLocalTime( DateTime time )
        {
            if(time.Kind == DateTimeKind.Local)
            {
                return time;
            }
            Boolean isAmbiguousLocalDst = false;
            Int64 offset = GetUtcOffsetFromUniversalTime( time, ref isAmbiguousLocalDst );
            long tick = time.Ticks + offset;
            if(tick > DateTime.MaxTicks)
            {
                return new DateTime( DateTime.MaxTicks, DateTimeKind.Local );
            }
            if(tick < DateTime.MinTicks)
            {
                return new DateTime( DateTime.MinTicks, DateTimeKind.Local );
            }
            return new DateTime( tick, DateTimeKind.Local, isAmbiguousLocalDst );
        }

        // Private object for locking instead of locking on a public type for SQL reliability work.
        private static Object s_InternalSyncObject;
        private static Object InternalSyncObject
        {
            get
            {
                if(s_InternalSyncObject == null)
                {
                    Object o = new Object();
                    Interlocked.CompareExchange( ref s_InternalSyncObject, o, null );
                }
                return s_InternalSyncObject;
            }
        }


        public override DaylightTime GetDaylightChanges( int year )
        {
            if(year < 1 || year > 9999)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "year", String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "ArgumentOutOfRange_Range" ), 1, 9999 ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            lock(InternalSyncObject)
            {

////            Object cachedObject;
////            if(m_CachedDaylightChanges.TryGetValue( year, out cachedObject ))
////            {
////                return (DaylightTime)cachedObject;
////            }
////            else
                if(m_CachedDaylightChangesYear == year && m_CachedDaylightChanges != null)
                {
                    return m_CachedDaylightChanges;
                }
                else
                {
                    //
                    // rawData is an array of 17 short (16 bit) numbers.
                    // The first 8 numbers contains the 
                    // year/month/day/dayOfWeek/hour/minute/second/millisecond for the starting time of daylight saving time.
                    // The next 8 numbers contains the
                    // year/month/day/dayOfWeek/hour/minute/second/millisecond for the ending time of daylight saving time.
                    // The last short number is the delta to the standard offset in minutes.
                    //
                    short[] rawData = nativeGetDaylightChanges();

                    DaylightTime currentDaylightChanges;
                    if(rawData == null)
                    {
                        //
                        // If rawData is null, it means that daylight saving time is not used
                        // in this timezone. So keep currentDaylightChanges as the empty array.
                        //
                        currentDaylightChanges = new DaylightTime( DateTime.MinValue, DateTime.MinValue, TimeSpan.Zero );
                    }
                    else
                    {
                        DateTime start;
                        DateTime end;
                        TimeSpan delta;

                        //
                        // Store the start of daylight saving time.
                        //

                        start = GetDayOfWeek( year, rawData[1], rawData[2],
                                          rawData[3],
                                          rawData[4], rawData[5], rawData[6], rawData[7] );

                        //
                        // Store the end of daylight saving time.
                        //
                        end = GetDayOfWeek( year, rawData[9], rawData[10],
                                        rawData[11],
                                        rawData[12], rawData[13], rawData[14], rawData[15] );

                        delta = new TimeSpan( rawData[16] * TicksPerMinute );
                        currentDaylightChanges = new DaylightTime( start, end, delta );
                    }
////                m_CachedDaylightChanges.Add( year, currentDaylightChanges );
                    m_CachedDaylightChangesYear = year;
                    m_CachedDaylightChanges     = currentDaylightChanges;

                    return currentDaylightChanges;
                }
            }
        }

        public override TimeSpan GetUtcOffset( DateTime time )
        {
            if(time.Kind == DateTimeKind.Utc)
            {
                return TimeSpan.Zero;
            }
            else
            {
                return new TimeSpan( TimeZone.CalculateUtcOffset( time, GetDaylightChanges( time.Year ) ).Ticks + m_ticksOffset );
            }
        }

        //
        // Return the (numberOfSunday)th day of week in a particular year/month.
        //
        private static DateTime GetDayOfWeek( int year, int month, int targetDayOfWeek, int numberOfSunday, int hour, int minute, int second, int millisecond )
        {
            DateTime time;

            if(numberOfSunday <= 4)
            {
                //
                // Get the (numberOfSunday)th Sunday.
                //

                time = new DateTime( year, month, 1, hour, minute, second, millisecond, DateTimeKind.Local );

                int dayOfWeek = (int)time.DayOfWeek;
                int delta = targetDayOfWeek - dayOfWeek;
                if(delta < 0)
                {
                    delta += 7;
                }
                delta += 7 * (numberOfSunday - 1);

                if(delta > 0)
                {
                    time = time.AddDays( delta );
                }
            }
            else
            {
                //
                // If numberOfSunday is greater than 4, we will get the last sunday.
                //
                Calendar cal = GregorianCalendar.GetDefaultInstance();
                time = new DateTime( year, month, cal.GetDaysInMonth( year, month ), hour, minute, second, millisecond, DateTimeKind.Local );
                // This is the day of week for the last day of the month.
                int dayOfWeek = (int)time.DayOfWeek;
                int delta = dayOfWeek - targetDayOfWeek;
                if(delta < 0)
                {
                    delta += 7;
                }

                if(delta > 0)
                {
                    time = time.AddDays( -delta );
                }
            }
            return (time);
        }

////    [ResourceExposure( ResourceScope.None )]
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        internal extern static int nativeGetTimeZoneMinuteOffset();

////    [ResourceExposure( ResourceScope.None )]
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        internal extern static String nativeGetDaylightName();

////    [ResourceExposure( ResourceScope.None )]
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        internal extern static String nativeGetStandardName();

////    [ResourceExposure( ResourceScope.None )]
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        internal extern static short[] nativeGetDaylightChanges();
    } // class CurrentSystemTimeZone
}
