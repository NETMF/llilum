// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
namespace System
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;

    // TimeSpan represents a duration of time.  A TimeSpan can be negative
    // or positive.
    //
    // TimeSpan is internally represented as a number of milliseconds.  While
    // this maps well into units of time such as hours and days, any
    // periods longer than that aren't representable in a nice fashion.
    // For instance, a month can be between 28 and 31 days, while a year
    // can contain 365 or 364 days.  A decade can have between 1 and 3 leapyears,
    // depending on when you map the TimeSpan into the calendar.  This is why
    // we do not provide Years() or Months().
    //
    [Serializable]
    public struct TimeSpan /*: IComparable, IComparable<TimeSpan>, IEquatable<TimeSpan>*/
    {
        public  const long   TicksPerMillisecond = 10000;
        public  const long   TicksPerSecond      = TicksPerMillisecond * 1000;
        public  const long   TicksPerMinute      = TicksPerSecond      * 60;
        public  const long   TicksPerHour        = TicksPerMinute      * 60;
        public  const long   TicksPerDay         = TicksPerHour        * 24;

        private const double MillisecondsPerTick = 1.0 / TicksPerMillisecond;
        private const double SecondsPerTick      = 1.0 / TicksPerSecond;
        private const double MinutesPerTick      = 1.0 / TicksPerMinute;
        private const double HoursPerTick        = 1.0 / TicksPerHour;
        private const double DaysPerTick         = 1.0 / TicksPerDay;

        private const int    MillisPerSecond     = 1000;
        private const int    MillisPerMinute     = MillisPerSecond * 60;
        private const int    MillisPerHour       = MillisPerMinute * 60;
        private const int    MillisPerDay        = MillisPerHour   * 24;

        private const long   MaxSeconds          = Int64.MaxValue / TicksPerSecond;
        private const long   MinSeconds          = Int64.MinValue / TicksPerSecond;

        private const long   MaxMilliSeconds     = Int64.MaxValue / TicksPerMillisecond;
        private const long   MinMilliSeconds     = Int64.MinValue / TicksPerMillisecond;

        public static readonly TimeSpan Zero     = new TimeSpan( 0 );

        public static readonly TimeSpan MaxValue = new TimeSpan( Int64.MaxValue );
        public static readonly TimeSpan MinValue = new TimeSpan( Int64.MinValue );

        // internal so that DateTime doesn't have to call an extra get
        // method for some arithmetic operations.
        internal long m_ticks;

        public TimeSpan( long ticks )
        {
            m_ticks = ticks;
        }

        public TimeSpan( int hours, int minutes, int seconds )
        {
            m_ticks = TimeToTicks( hours, minutes, seconds );
        }

        public TimeSpan( int days, int hours, int minutes, int seconds ) : this( days, hours, minutes, seconds, 0 )
        {
        }

        public TimeSpan( int days, int hours, int minutes, int seconds, int milliseconds )
        {
            Int64 totalMilliSeconds = ((Int64)days * 3600 * 24 + (Int64)hours * 3600 + (Int64)minutes * 60 + seconds) * 1000 + milliseconds;

            if(totalMilliSeconds > MaxMilliSeconds || totalMilliSeconds < MinMilliSeconds)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( null, Environment.GetResourceString( "Overflow_TimeSpanTooLong" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            m_ticks = (long)totalMilliSeconds * TicksPerMillisecond;
        }

        public long Ticks
        {
            get
            {
                return m_ticks;
            }
        }

        public int Days
        {
            get
            {
                return (int)(m_ticks / TicksPerDay);
            }
        }

        public int Hours
        {
            get
            {
                return (int)((m_ticks / TicksPerHour) % 24);
            }
        }

        public int Milliseconds
        {
            get
            {
                return (int)((m_ticks / TicksPerMillisecond) % 1000);
            }
        }

        public int Minutes
        {
            get
            {
                return (int)((m_ticks / TicksPerMinute) % 60);
            }
        }

        public int Seconds
        {
            get
            {
                return (int)((m_ticks / TicksPerSecond) % 60);
            }
        }

        public double TotalDays
        {
            get
            {
                return ((double)m_ticks) * DaysPerTick;
            }
        }

        public double TotalHours
        {
            get
            {
                return (double)m_ticks * HoursPerTick;
            }
        }

        public double TotalMilliseconds
        {
            get
            {
                double temp = (double)m_ticks * MillisecondsPerTick;
                if(temp > MaxMilliSeconds)
                {
                    return (double)MaxMilliSeconds;
                }

                if(temp < MinMilliSeconds)
                {
                    return (double)MinMilliSeconds;
                }

                return temp;
            }
        }

        public double TotalMinutes
        {
            get
            {
                return (double)m_ticks * MinutesPerTick;
            }
        }

        public double TotalSeconds
        {
            get
            {
                return (double)m_ticks * SecondsPerTick;
            }
        }

        public TimeSpan Add( TimeSpan ts )
        {
            long result = m_ticks + ts.m_ticks;

            // Overflow if signs of operands was identical and result's
            // sign was opposite.
            // >> 63 gives the sign bit (either 64 1's or 64 0's).
            if((m_ticks >> 63 == ts.m_ticks >> 63) && (m_ticks >> 63 != result >> 63))
            {
#if EXCEPTION_STRINGS
                throw new OverflowException( Environment.GetResourceString( "Overflow_TimeSpanTooLong" ) );
#else
                throw new OverflowException();
#endif
            }

            return new TimeSpan( result );
        }


        // Compares two TimeSpan values, returning an integer that indicates their
        // relationship.
        //
        public static int Compare( TimeSpan t1, TimeSpan t2 )
        {
            return t1.CompareTo( t2 );
        }

        // Returns a value less than zero if this  object
        public int CompareTo( Object value )
        {
            if(value == null) return 1;

            if(!(value is TimeSpan))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeTimeSpan" ) );
#else
                throw new ArgumentException();
#endif
            }

            return CompareTo( (TimeSpan)value );
        }

        public int CompareTo( TimeSpan value )
        {
            long t = value.m_ticks;

            if(m_ticks > t) return  1;
            if(m_ticks < t) return -1;

            return 0;
        }

        public static TimeSpan FromDays( double value )
        {
            return Interval( value, MillisPerDay );
        }

        public TimeSpan Duration()
        {
            if(m_ticks == TimeSpan.MinValue.m_ticks)
            {
#if EXCEPTION_STRINGS
                throw new OverflowException( Environment.GetResourceString( "Overflow_Duration" ) );
#else
                throw new OverflowException();
#endif
            }

            return new TimeSpan( m_ticks >= 0 ? m_ticks : -m_ticks );
        }

        public override bool Equals( Object obj )
        {
            if(!(obj is TimeSpan))
            {
                return false;
            }

            return Equals( (TimeSpan)obj );
        }

        public bool Equals( TimeSpan obj )
        {
            return m_ticks == obj.m_ticks;
        }

        public static bool Equals( TimeSpan t1, TimeSpan t2 )
        {
            return t1.Equals( t2 );
        }

        public override int GetHashCode()
        {
            return (int)m_ticks ^ (int)(m_ticks >> 32);
        }

        public static TimeSpan FromHours( double value )
        {
            return Interval( value, MillisPerHour );
        }

        private static TimeSpan Interval( double value, int scale )
        {
            if(Double.IsNaN( value ))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_CannotBeNaN" ) );
#else
                throw new ArgumentException();
#endif
            }

            double tmp    = value * scale;
            double millis = tmp + (value >= 0 ? 0.5 : -0.5);

            if((millis > Int64.MaxValue / TicksPerMillisecond) || (millis < Int64.MinValue / TicksPerMillisecond))
            {
#if EXCEPTION_STRINGS
                throw new OverflowException( Environment.GetResourceString( "Overflow_TimeSpanTooLong" ) );
#else
                throw new OverflowException();
#endif
            }

            return new TimeSpan( (long)millis * TicksPerMillisecond );
        }

        public static TimeSpan FromMilliseconds( double value )
        {
            return Interval( value, 1 );
        }

        public static TimeSpan FromMinutes( double value )
        {
            return Interval( value, MillisPerMinute );
        }

        public TimeSpan Negate()
        {
            if(m_ticks == TimeSpan.MinValue.m_ticks)
            {
#if EXCEPTION_STRINGS
                throw new OverflowException( Environment.GetResourceString( "Overflow_NegateTwosCompNum" ) );
#else
                throw new OverflowException();
#endif
            }

            return new TimeSpan( -m_ticks );
        }

        // Constructs a TimeSpan from a string.  Leading and trailing white
        // space characters are allowed.
        //
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern TimeSpan Parse( String s );
////    {
////        return new TimeSpan( new StringParser().Parse( s ) );
////    }
////
////    public static Boolean TryParse( String s, out TimeSpan result )
////    {
////        long longResult;
////
////        if(new StringParser().TryParse( s, out longResult ))
////        {
////            result = new TimeSpan( longResult );
////            return true;
////        }
////        else
////        {
////            result = TimeSpan.Zero;
////            return false;
////        }
////    }

        public static TimeSpan FromSeconds( double value )
        {
            return Interval( value, MillisPerSecond );
        }

        public TimeSpan Subtract( TimeSpan ts )
        {
            long result = m_ticks - ts.m_ticks;

            // Overflow if signs of operands was different and result's
            // sign was opposite from the first argument's sign.
            // >> 63 gives the sign bit (either 64 1's or 64 0's).
            if((m_ticks >> 63 != ts.m_ticks >> 63) && (m_ticks >> 63 != result >> 63))
            {
#if EXCEPTION_STRINGS
                throw new OverflowException( Environment.GetResourceString( "Overflow_TimeSpanTooLong" ) );
#else
                throw new OverflowException();
#endif
            }

            return new TimeSpan( result );
        }

        public static TimeSpan FromTicks( long value )
        {
            return new TimeSpan( value );
        }

        internal static long TimeToTicks( int hour, int minute, int second )
        {
            // totalSeconds is bounded by 2^31 * 2^12 + 2^31 * 2^8 + 2^31,
            // which is less than 2^44, meaning we won't overflow totalSeconds.
            long totalSeconds = (long)hour * 3600 + (long)minute * 60 + (long)second;

            if(totalSeconds > MaxSeconds || totalSeconds < MinSeconds)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( null, Environment.GetResourceString( "Overflow_TimeSpanTooLong" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            return totalSeconds * TicksPerSecond;
        }

////    private String IntToString( int n, int digits )
////    {
////        return ParseNumbers.IntToString( n, 10, digits, '0', 0 );
////    }
    
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
    
            int  day  = (int)(m_ticks / TicksPerDay);
            long time =       m_ticks % TicksPerDay;
    
            if(m_ticks < 0)
            {
                sb.Append( "-" );
                day  = -day;
                time = -time;
            }
    
            if(day != 0)
            {
                sb.Append( day );
                sb.Append( "." );
            }
    
            sb.AppendFormat( "{0:D2}:{1:D2}:{2:D2}", (int)(time / TicksPerHour   % 24),
                                                     (int)(time / TicksPerMinute % 60),
                                                     (int)(time / TicksPerSecond % 60) );
    
            int t = (int)(time % TicksPerSecond);
            if(t != 0)
            {
                sb.AppendFormat( ".{0:D7}", t );
            }
    
            return sb.ToString();
        }

        public static TimeSpan operator -( TimeSpan t )
        {
            if(t.m_ticks == TimeSpan.MinValue.m_ticks)
            {
#if EXCEPTION_STRINGS
                throw new OverflowException( Environment.GetResourceString( "Overflow_NegateTwosCompNum" ) );
#else
                throw new OverflowException();
#endif
            }

            return new TimeSpan( -t.m_ticks );
        }

        public static TimeSpan operator -( TimeSpan t1, TimeSpan t2 )
        {
            return t1.Subtract( t2 );
        }

        public static TimeSpan operator +( TimeSpan t )
        {
            return t;
        }

        public static TimeSpan operator +( TimeSpan t1, TimeSpan t2 )
        {
            return t1.Add( t2 );
        }

        public static bool operator ==( TimeSpan t1, TimeSpan t2 )
        {
            return t1.m_ticks == t2.m_ticks;
        }

        public static bool operator !=( TimeSpan t1, TimeSpan t2 )
        {
            return t1.m_ticks != t2.m_ticks;
        }

        public static bool operator <( TimeSpan t1, TimeSpan t2 )
        {
            return t1.m_ticks < t2.m_ticks;
        }

        public static bool operator <=( TimeSpan t1, TimeSpan t2 )
        {
            return t1.m_ticks <= t2.m_ticks;
        }

        public static bool operator >( TimeSpan t1, TimeSpan t2 )
        {
            return t1.m_ticks > t2.m_ticks;
        }

        public static bool operator >=( TimeSpan t1, TimeSpan t2 )
        {
            return t1.m_ticks >= t2.m_ticks;
        }

////    private struct StringParser
////    {
////        private enum ParseError
////        {
////            Format                      = 1,
////            Overflow                    = 2,
////            OverflowHoursMinutesSeconds = 3,
////            ArgumentNull                = 4,
////        }
////
////        private String     str;
////        private char       ch;
////        private int        pos;
////        private int        len;
////        private ParseError error;
////
////        internal void NextChar()
////        {
////            if(pos < len) pos++;
////
////            ch = pos < len ? str[pos] : (char)0;
////        }
////
////        internal char NextNonDigit()
////        {
////            for(i = pos; i < len; i++)
////            {
////                char ch = str[i];
////
////                if(ch < '0' || ch > '9') return ch;
////            }
////
////            return (char)0;
////        }
////
////        internal long Parse( String s )
////        {
////            long value;
////
////            if(TryParse( s, out value ))
////            {
////                return value;
////            }
////            else
////            {
////                switch(error)
////                {
////                    case ParseError.ArgumentNull:
////                        throw new ArgumentNullException( "s" );
////
////                    case ParseError.Format:
////                        throw new FormatException( Environment.GetResourceString( "Format_InvalidString" ) );
////
////                    case ParseError.Overflow:
////                        throw new OverflowException( Environment.GetResourceString( "Overflow_TimeSpanTooLong" ) );
////
////                    case ParseError.OverflowHoursMinutesSeconds:
////                        throw new OverflowException( Environment.GetResourceString( "Overflow_TimeSpanElementTooLarge" ) );
////
////                    default:
////                        BCLDebug.Assert( false, "Unknown error: " + error.ToString() );
////                        return 0;
////                }
////            }
////        }
////
////        internal bool TryParse( String s, out long value )
////        {
////            value = 0;
////            if(s == null)
////            {
////                error = ParseError.ArgumentNull;
////                return false;
////            }
////
////            str = s;
////            len = s.Length;
////            pos = -1;
////
////            NextChar();
////            SkipBlanks();
////
////            bool negative = false;
////            if(ch == '-')
////            {
////                negative = true;
////                NextChar();
////            }
////
////            long time;
////
////            if(NextNonDigit() == ':')
////            {
////                if(!ParseTime( out time ))
////                {
////                    return false;
////                }
////            }
////            else
////            {
////                int days;
////
////                if(!ParseInt( (int)(0x7FFFFFFFFFFFFFFFL / TicksPerDay), out days ))
////                {
////                    return false;
////                }
////
////                time = days * TicksPerDay;
////                if(ch == '.')
////                {
////                    NextChar();
////                    long remainingTime;
////                    if(!ParseTime( out remainingTime ))
////                    {
////                        return false;
////                    }
////
////                    time += remainingTime;
////                }
////            }
////
////            if(negative)
////            {
////                time = -time;
////                // Allow -0 as well
////                if(time > 0)
////                {
////                    error = ParseError.Overflow;
////                    return false;
////                }
////            }
////            else
////            {
////                if(time < 0)
////                {
////                    error = ParseError.Overflow;
////                    return false;
////                }
////            }
////
////            SkipBlanks();
////            if(pos < len)
////            {
////                error = ParseError.Format;
////                return false;
////            }
////
////            value = time;
////            return true;
////        }
////
////        internal bool ParseInt( int max, out int i )
////        {
////            i = 0;
////            int p = pos;
////            while(ch >= '0' && ch <= '9')
////            {
////                if((i & 0xF0000000) != 0)
////                {
////                    error = ParseError.Overflow;
////                    return false;
////                }
////
////                i = i * 10 + ch - '0';
////                if(i < 0)
////                {
////                    error = ParseError.Overflow;
////                    return false;
////                }
////                NextChar();
////            }
////
////            if(p == pos)
////            {
////                error = ParseError.Format;
////                return false;
////            }
////
////            if(i > max)
////            {
////                error = ParseError.Overflow;
////                return false;
////            }
////
////            return true;
////        }
////
////        internal bool ParseTime( out long time )
////        {
////            time = 0;
////
////            int unit;
////
////            if(!ParseInt( 23, out unit ))
////            {
////                if(error == ParseError.Overflow)
////                {
////                    error = ParseError.OverflowHoursMinutesSeconds;
////                }
////                return false;
////            }
////
////            time = unit * TicksPerHour;
////            if(ch != ':')
////            {
////                error = ParseError.Format;
////                return false;
////            }
////
////            NextChar();
////            if(!ParseInt( 59, out unit ))
////            {
////                if(error == ParseError.Overflow)
////                {
////                    error = ParseError.OverflowHoursMinutesSeconds;
////                }
////                return false;
////            }
////
////            time += unit * TicksPerMinute;
////            if(ch == ':')
////            {
////                NextChar();
////
////                // allow seconds with the leading zero
////                if(ch != '.')
////                {
////                    if(!ParseInt( 59, out unit ))
////                    {
////                        if(error == ParseError.Overflow)
////                        {
////                            error = ParseError.OverflowHoursMinutesSeconds;
////                        }
////                        return false;
////                    }
////                    time += unit * TicksPerSecond;
////                }
////
////                if(ch == '.')
////                {
////                    NextChar();
////                    int f = (int)TicksPerSecond;
////                    while(f > 1 && ch >= '0' && ch <= '9')
////                    {
////                        f /= 10;
////                        time += (ch - '0') * f;
////                        NextChar();
////                    }
////                }
////            }
////
////            return true;
////        }
////
////        internal void SkipBlanks()
////        {
////            while(ch == ' ' || ch == '\t') NextChar();
////        }
////    }
    }
}
