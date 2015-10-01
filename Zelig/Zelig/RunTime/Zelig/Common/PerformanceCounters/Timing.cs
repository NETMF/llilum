//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.PerformanceCounters
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;


    public struct Timing
    {
        public class Nested
        {
            //
            // State
            //

            long m_exclusiveTime;
            int  m_hits;

            //
            // Helper Methods
            //

            public void Sum( long exclusiveTime )
            {
                m_exclusiveTime += exclusiveTime;
                m_hits          += 1;
            }

            //--//

            //
            // Access Methods
            //

            public long TotalExclusiveTicks
            {
                get
                {
                    return m_exclusiveTime;
                }
            }

            public long TotalExclusiveMicroSeconds
            {
                get
                {
                    return (long)(m_exclusiveTime * s_toMicroSeconds);
                }
            }

            public int Hits
            {
                get
                {
                    return m_hits;
                }
            }
        }

        class ThreadState
        {
            //
            // State
            //

            internal long m_overheadTotal;
            internal long m_overheadExclusive;

            internal int  m_suspendCount;
            internal long m_suspendStart;

            internal int  m_overheadQuery;

            //
            // Helper Methods
            //

            internal void Calibrate()
            {
                const int averagingRuns      = 16 * 1024;
                const int averagingThreshold = 20;
                int       averagingCount     = 0;
                int       averagingBestCount = 0;
                int       averagingBestValue = 0;
        
                for(int i = 0; i < averagingRuns; i++)
                {
                    Timing sampler1 = new Timing();
                    Timing sampler2 = new Timing();
                    Timing sampler3 = new Timing();
        
                    sampler1.Start();
                    sampler2.Start();
                    sampler3.Start();
                    sampler3.Stop ( false );
                    sampler2.Stop ( false );
                    sampler1.Stop ( false );
        
                    int diff = (int)sampler1.TotalInclusiveTicks;
        
                    if(-10 < diff && diff < 100)
                    {
                        averagingCount++;
        
                        if(averagingCount == averagingThreshold)
                        {
                            return;
                        }
                    }
                    else
                    {
                        if(averagingBestCount < averagingCount)
                        {
                            averagingBestCount = averagingCount;
                            averagingBestValue = m_overheadQuery;
                        }
        
                        averagingCount = 1;
                    }
                    
                    if(m_overheadQuery == 0)
                    {
                        m_overheadQuery = diff;
                    }
                    else
                    {
                        m_overheadQuery += diff / 8;
                    }
                }
        
                m_overheadQuery = averagingBestValue;
            }

            internal long GetTimestamp()
            {
                long res = System.Diagnostics.Stopwatch.GetTimestamp();

                m_overheadTotal += m_overheadQuery;
                
                return res - m_overheadTotal;
            }
        }

        //
        // State
        //

        /************/ static double      s_toMicroSeconds;
        [ThreadStatic] static ThreadState s_threadState;

        long m_startInclusive;
        long m_startExclusive;
        int  m_gcCount;

        long m_totalInclusive;
        long m_totalExclusive;
        int  m_hits;
        bool m_active;

        //
        // Helper Methods
        //

        private static ThreadState GetThreadState()
        {
            ThreadState ts = s_threadState;
            
            if(ts == null)
            {
                ts = new ThreadState();

                s_threadState = ts;

                ts.Calibrate();
            }

            return ts;
        }

        public static void Suspend()
        {
            ThreadState ts = GetThreadState();
            
            if(ts.m_suspendCount++ == 0)
            {
                ts.m_suspendStart = ts.GetTimestamp();
            }
        }

        public static void Resume()
        {
            ThreadState ts = GetThreadState();
            
            if(--ts.m_suspendCount == 0)
            {
                long end = ts.GetTimestamp();

                ts.m_overheadTotal += end - ts.m_suspendStart;
            }
        }

        public void Start()
        {
            ThreadState ts = GetThreadState();
            
            CHECKS.ASSERT( ts.m_suspendCount == 0, "Cannot start performance timer while it's suspended." );

            CHECKS.ASSERT( m_active == false, "Found recursive use of performance counter" );

            m_startExclusive = ts.m_overheadExclusive;
            m_gcCount        = GC.CollectionCount( 0 );
            m_active         = true;
            m_startInclusive = ts.GetTimestamp();
        }

        public long Sample()
        {
            ThreadState ts = s_threadState;

            CHECKS.ASSERT( ts.m_suspendCount == 0, "Cannot stop performance timer while it's suspended." );

            long end = ts.GetTimestamp();

            return end - m_startInclusive;
        }

        public long Stop( bool fIgnoreGC )
        {
            ThreadState ts = s_threadState;

            CHECKS.ASSERT( ts.m_suspendCount == 0, "Cannot stop performance timer while it's suspended." );

            long end = ts.GetTimestamp();

            if(fIgnoreGC == false && m_gcCount != GC.CollectionCount( 0 ))
            {
                //
                // A GC happened between Start and Stop, throw away the whole sample.
                //
                ts.m_overheadTotal += end - m_startInclusive;
    
                m_active = false;
    
                return 0;
            }
            else
            {
                long diff          = end                    - m_startInclusive;
                long diffOverhead  = ts.m_overheadExclusive - m_startExclusive;
                long diffExclusive = diff                   - diffOverhead;

                m_totalInclusive += diff;
                m_totalExclusive += diffExclusive;
                m_hits           += 1;
                m_active          = false;

                ts.m_overheadExclusive += diffExclusive;

                return diffExclusive;
            }
        }

        public void SetGcCount( int gcCount )
        {
            m_gcCount = gcCount;
        }

        //--//

        static Timing()
        {
            s_toMicroSeconds = 1000000.0 / System.Diagnostics.Stopwatch.Frequency;
        }

        //
        // Access Methods
        //

        public static long ToMicroSeconds( long ticks )
        {
            return (long)(ticks * s_toMicroSeconds);
        }

        public long TotalInclusiveTicks
        {
            get
            {
                return m_totalInclusive;
            }
        }

        public long TotalInclusiveMicroSeconds
        {
            get
            {
                return ToMicroSeconds( m_totalInclusive );
            }
        }

        public float InclusiveTicksPerHit
        {
            get
            {
                return m_hits > 0 ? (float)m_totalInclusive / (float)m_hits : 0;
            }
        }

        public long TotalExclusiveTicks
        {
            get
            {
                return m_totalExclusive;
            }
        }

        public long TotalExclusiveMicroSeconds
        {
            get
            {
                return ToMicroSeconds( m_totalExclusive );
            }
        }

        public float ExclusiveTicksPerHit
        {
            get
            {
                return m_hits > 0 ? (float)m_totalExclusive / (float)m_hits : 0;
            }
        }

        public int Hits
        {
            get
            {
                return m_hits;
            }
        }

        //
        // Debug Methods
        //

        public override string ToString()
        {
            return string.Format( "{0} - {1}", this.ExclusiveTicksPerHit, this.InclusiveTicksPerHit );
        }
    }
}
