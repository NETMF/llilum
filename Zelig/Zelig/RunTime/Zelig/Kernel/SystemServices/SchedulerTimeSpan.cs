//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public struct SchedulerTimeSpan : IEquatable< SchedulerTimeSpan >
    {
        //
        // State
        //

        public static readonly SchedulerTimeSpan MinValue = new SchedulerTimeSpan( long.MinValue );
        public static readonly SchedulerTimeSpan MaxValue = new SchedulerTimeSpan( long.MaxValue );

        private long m_deltaUnits;

        //
        // Constructor Methods
        //

        internal SchedulerTimeSpan( long deltaUnits )
        {
            m_deltaUnits = deltaUnits;
        }

        //
        // Equality Methods
        //

        public override bool Equals( Object obj )
        {
            if(obj is SchedulerTimeSpan)
            {
                SchedulerTimeSpan other = (SchedulerTimeSpan)obj;

                return this.Equals( other );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_deltaUnits.GetHashCode();
        }

        public bool Equals( SchedulerTimeSpan other )
        {
            return m_deltaUnits == other.m_deltaUnits;
        }

        public static bool Equals( SchedulerTimeSpan ts1 ,
                                   SchedulerTimeSpan ts2 )
        {
            return ts1.Equals( ts2 );
        }

        //
        // Helper Methods
        //

        public static int Compare( SchedulerTimeSpan ts1 ,
                                   SchedulerTimeSpan ts2 )
        {
            return ts1.m_deltaUnits.CompareTo( ts1.m_deltaUnits );
        }

        public SchedulerTimeSpan Add( SchedulerTimeSpan value )
        {
            if(value == SchedulerTimeSpan.MaxValue)
            {
                return MaxValue;
            }
            else if(value == SchedulerTimeSpan.MinValue)
            {
                return MinValue;
            }

            return new SchedulerTimeSpan( m_deltaUnits + value.m_deltaUnits );
        }

        public SchedulerTimeSpan Add( TimeSpan value )
        {
            return Add( (SchedulerTimeSpan)value );
        }

        public SchedulerTimeSpan Subtract( SchedulerTimeSpan value )
        {
            if(value == SchedulerTimeSpan.MaxValue)
            {
                return MinValue;
            }
            else if(value == SchedulerTimeSpan.MinValue)
            {
                return MaxValue;
            }

            return new SchedulerTimeSpan( m_deltaUnits - value.m_deltaUnits );
        }

        public SchedulerTimeSpan Subtract( TimeSpan value )
        {
            return Subtract( (SchedulerTimeSpan)value );
        }

        //--//

        public static SchedulerTimeSpan FromMilliseconds( long milliSeconds )
        {
            return new SchedulerTimeSpan( ConvertFromMillisecondsToDeltaUnits( milliSeconds ) );
        }

        public static explicit operator SchedulerTimeSpan ( TimeSpan ts )
        {
            return new SchedulerTimeSpan( ConvertFromTimeSpanTicksToDeltaUnits( ts.Ticks ) );
        }

        public static explicit operator TimeSpan ( SchedulerTimeSpan ts )
        {
            return new TimeSpan( ConvertFromDeltaUnitsToTimeSpanTicks( ts.m_deltaUnits ) );
        }

        public static long ToMilliseconds( SchedulerTimeSpan ts )
        {
            return ConvertFromDeltaUnitsToMilliseconds( ts.m_deltaUnits );
        }

        //--//

        public static SchedulerTimeSpan operator +( SchedulerTimeSpan ts1 ,
                                                    TimeSpan          ts2 )
        {
            return ts1.Add( ts2 );
        }

        public static SchedulerTimeSpan operator -( SchedulerTimeSpan ts1 ,
                                                    TimeSpan          ts2 )
        {
            return ts1.Subtract( ts2 );
        }

        public static SchedulerTimeSpan operator -( SchedulerTimeSpan ts1 ,
                                                    SchedulerTimeSpan ts2 )
        {
            return ts1.Subtract( ts2 );
        }

        [Inline]
        public static bool operator ==( SchedulerTimeSpan ts1 ,
                                        SchedulerTimeSpan ts2 )
        {
            return ts1.m_deltaUnits == ts2.m_deltaUnits;
        }

        [Inline]
        public static bool operator !=( SchedulerTimeSpan ts1 ,
                                        SchedulerTimeSpan ts2 )
        {
            return ts1.m_deltaUnits != ts2.m_deltaUnits;
        }

        [Inline]
        public static bool operator <( SchedulerTimeSpan ts1 ,
                                       SchedulerTimeSpan ts2 )
        {
            return ts1.m_deltaUnits < ts2.m_deltaUnits;
        }

        [Inline]
        public static bool operator <=( SchedulerTimeSpan ts1 ,
                                        SchedulerTimeSpan ts2 )
        {
            return ts1.m_deltaUnits <= ts2.m_deltaUnits;
        }

        [Inline]
        public static bool operator >( SchedulerTimeSpan ts1 ,
                                       SchedulerTimeSpan ts2 )
        {
            return ts1.m_deltaUnits > ts2.m_deltaUnits;
        }

        [Inline]
        public static bool operator >=( SchedulerTimeSpan ts1 ,
                                        SchedulerTimeSpan ts2 )
        {
            return ts1.m_deltaUnits >= ts2.m_deltaUnits;
        }

        //--//

        //
        // These have to be implemented by the overriding class extension.
        //
        [MethodImpl( MethodImplOptions.InternalCall )]
        private static extern long ConvertFromMillisecondsToDeltaUnits( long milliSeconds );

        [MethodImpl( MethodImplOptions.InternalCall )]
        private static extern long ConvertFromTimeSpanTicksToDeltaUnits( long ticks );

        [MethodImpl( MethodImplOptions.InternalCall )]
        private static extern long ConvertFromDeltaUnitsToTimeSpanTicks( long deltaUnits );

        [MethodImpl( MethodImplOptions.InternalCall )]
        private static extern long ConvertFromDeltaUnitsToMilliseconds( long deltaUnits );

        //--//

        //
        // Access Methods
        //

        public long DeltaUnits
        {
            get
            {
                return m_deltaUnits;
            }
        }
    }
}
