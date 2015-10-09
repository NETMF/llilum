//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public struct SchedulerTime : IEquatable< SchedulerTime >
    {
        //
        // State
        //

        public static readonly SchedulerTime MinValue = new SchedulerTime( ulong.MinValue );
        public static readonly SchedulerTime MaxValue = new SchedulerTime( ulong.MaxValue );

        private ulong m_units;

        //
        // Constructor Methods
        //

        private SchedulerTime( ulong units )
        {
            m_units = units;
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            if(obj is SchedulerTime)
            {
                SchedulerTime other = (SchedulerTime)obj;

                return this.Equals( other );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_units.GetHashCode();
        }

        public bool Equals( SchedulerTime other )
        {
            return m_units == other.m_units;
        }

        public static bool Equals( SchedulerTime t1 ,
                                   SchedulerTime t2 )
        {
            return t1.Equals( t2 );
        }

        //
        // Helper Methods
        //

        public static int Compare( SchedulerTime t1 ,
                                   SchedulerTime t2 )
        {
            return t1.m_units.CompareTo( t1.m_units );
        }

        public SchedulerTime Add( SchedulerTimeSpan value )
        {
            if(value == SchedulerTimeSpan.MaxValue)
            {
                return MaxValue;
            }
            else if(value == SchedulerTimeSpan.MinValue)
            {
                return MinValue;
            }

            return new SchedulerTime( m_units + (ulong)value.DeltaUnits );
        }

        public SchedulerTime Add( TimeSpan value )
        {
            return Add( (SchedulerTimeSpan)value );
        }

        public SchedulerTime Subtract( SchedulerTimeSpan value )
        {
            if(value == SchedulerTimeSpan.MaxValue)
            {
                return MinValue;
            }
            else if(value == SchedulerTimeSpan.MinValue)
            {
                return MaxValue;
            }

            return new SchedulerTime( m_units - (ulong)value.DeltaUnits );
        }

        public SchedulerTime Subtract( TimeSpan value )
        {
            return Subtract( (SchedulerTimeSpan)value );
        }

        //--//

        public static explicit operator SchedulerTime ( int milliseconds )
        {
            return new SchedulerTime( ConvertFromMillisecondsToUnits( milliseconds ) );
        }

        public static explicit operator SchedulerTime ( TimeSpan ts )
        {
            return new SchedulerTime( ConvertFromTimeSpanTicksToUnits( ts.Ticks ) );
        }

        public static explicit operator SchedulerTime ( DateTime dt )
        {
            return new SchedulerTime( ConvertFromDateTimeTicksToUnits( dt.Ticks ) );
        }

        public static explicit operator DateTime ( SchedulerTime t )
        {
            return new DateTime( ConvertFromUnitsToDateTimeTicks( t.m_units ) );
        }

        public static SchedulerTime operator +( SchedulerTime     t  ,
                                                SchedulerTimeSpan ts )
        {
            return t.Add( ts );
        }

        public static SchedulerTime operator +( SchedulerTime t  ,
                                                TimeSpan      ts )
        {
            return t.Add( ts );
        }

        public static SchedulerTime operator -( SchedulerTime     t  ,
                                                SchedulerTimeSpan ts )
        {
            return t.Subtract( ts );
        }
        public static SchedulerTime operator -( SchedulerTime t ,
                                                TimeSpan      ts )
        {
            return t.Subtract( ts );
        }

        public static SchedulerTimeSpan operator -( SchedulerTime t1 ,
                                                    SchedulerTime t2 )
        {
            return new SchedulerTimeSpan( (long)t1.m_units - (long)t2.m_units );
        }

        [Inline]
        public static bool operator ==( SchedulerTime t1 ,
                                        SchedulerTime t2 )
        {
            return t1.m_units == t2.m_units;
        }

        [Inline]
        public static bool operator !=( SchedulerTime t1 ,
                                        SchedulerTime t2 )
        {
            return t1.m_units != t2.m_units;
        }

        [Inline]
        public static bool operator <( SchedulerTime t1 ,
                                       SchedulerTime t2 )
        {
            return t1.m_units < t2.m_units;
        }

        [Inline]
        public static bool operator <=( SchedulerTime t1 ,
                                        SchedulerTime t2 )
        {
            return t1.m_units <= t2.m_units;
        }

        [Inline]
        public static bool operator >( SchedulerTime t1 ,
                                       SchedulerTime t2 )
        {
            return t1.m_units > t2.m_units;
        }

        [Inline]
        public static bool operator >=( SchedulerTime t1 ,
                                        SchedulerTime t2 )
        {
            return t1.m_units >= t2.m_units;
        }

        //--//

        public static SchedulerTime FromUnits( ulong units )
        {
            return new SchedulerTime( units );
        }

        //--//

        //
        // These have to be implemented by the overriding class extension.
        //
        [MethodImpl( MethodImplOptions.InternalCall )]
        private static extern ulong ConvertFromMillisecondsToUnits( int milliSeconds );

        [MethodImpl( MethodImplOptions.InternalCall )]
        private static extern ulong ConvertFromTimeSpanTicksToUnits( long ticks );

        [MethodImpl( MethodImplOptions.InternalCall )]
        private static extern ulong ConvertFromDateTimeTicksToUnits( long ticks );

        [MethodImpl( MethodImplOptions.InternalCall )]
        private static extern long ConvertFromUnitsToDateTimeTicks( ulong units );

        [MethodImpl( MethodImplOptions.InternalCall )]
        private extern static ulong GetCurrentTime();

        //--//

        //
        // Access Methods
        //

        public static SchedulerTime Now
        {
            get
            {
                return new SchedulerTime( GetCurrentTime() );
            }
        }

        public ulong Units
        {
            get
            {
                return m_units;
            }
        }
    }
}
