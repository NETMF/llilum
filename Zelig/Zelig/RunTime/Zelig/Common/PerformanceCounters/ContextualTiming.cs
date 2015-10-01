//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define COLLECT_PERFORMANCE_DATA


namespace Microsoft.Zelig.PerformanceCounters
{
    using System;
    using System.Collections.Generic;

#if COLLECT_PERFORMANCE_DATA
    public struct ContextualTiming : IDisposable
    {
        class State
        {
            //
            // State
            //

            internal object                                    m_context;
            internal string                                    m_reason;
            internal Timing                                    m_counter;
            internal int                                       m_recursionCount;
            internal GrowOnlyHashTable< State, Timing.Nested > m_children;

            //
            // Constructor Methods
            //

            internal State( object context ,
                            string reason  )
            {
                m_context = context;
                m_reason  = reason;
            }

            //
            // Equality Methods
            //

            public override bool Equals( object obj )
            {
                if(obj is State)
                {
                    State other = (State)obj;

                    if(m_reason == other.m_reason)
                    {
                        return Object.ReferenceEquals( m_context, other.m_context );
                    }
                }

                return false;
            }

            public override int GetHashCode()
            {
                return m_context.GetHashCode();
            }

            //
            // Helper Methods
            //

            internal void Include( State other         ,
                                   long  exclusiveTime )
            {
                if(m_children == null)
                {
                    m_children = HashTableFactory.New< State, Timing.Nested >();
                }

                Timing.Nested child;

                if(m_children.TryGetValue( other, out child ) == false)
                {
                    child = new Timing.Nested();

                    m_children[other] = child;
                }

                child.Sum( exclusiveTime );
            }
        }

        //
        // State
        //

        /************/ static List       < GrowOnlySet< State > > s_threads;
        [ThreadStatic] static GrowOnlySet<              State   > s_states;
        [ThreadStatic] static List       <              State   > s_activeStates;

        State m_state;
        int   m_gcCount;

        //
        // Constructor Methods
        //

        public ContextualTiming( object context ,
                                 string reason  )
        {
            if(context == null)
            {
                m_state   = null;
                m_gcCount = 0;
                return;
            }

            Timing.Suspend();

            if(s_states == null)
            {
                s_states       = SetFactory.New< State >();
                s_activeStates = new       List< State >();

                lock(typeof(State))
                {
                    if(s_threads == null)
                    {
                        s_threads = new List< GrowOnlySet< State > >();
                    }

                    s_threads.Add( s_states );
                }
            }

            State state = new State( context, reason );

            if(s_states.Contains( state, out m_state ) == false)
            {
                m_state = state;

                s_states.Insert( state );
            }

            if(m_state.m_recursionCount++ == 0)
            {
                s_activeStates.Add( m_state );

                Timing.Resume();

                m_state.m_counter.Start();
                m_gcCount = GC.CollectionCount( 0 );
            }
            else
            {
                Timing.Resume();

                m_gcCount = 0;
            }
        }

        //
        // Helper Methods
        //

        public void Dispose()
        {
            if(m_state != null)
            {
                if(m_state.m_recursionCount-- == 1)
                {
                    long exclusiveTime = m_state.m_counter.Stop( false );
                    int  gcCount       = GC.CollectionCount( 0 );

                    Timing.Suspend();

                    int top = s_activeStates.Count - 1;

                    CHECKS.ASSERT( s_activeStates[top].Equals( m_state ), "Incorrect nesting of contextual timing" );

                    s_activeStates.RemoveAt( top );

                    while(--top >= 0)
                    {
                        State other = s_activeStates[top];

                        other.Include( m_state, exclusiveTime );

                        if(m_gcCount == gcCount)
                        {
                            other.m_counter.SetGcCount( gcCount );
                        }
                    }

                    Timing.Resume();
                }
            }
        }

        //
        // Access Methods
        //

        public long TotalInclusiveTicks
        {
            get
            {
                return m_state.m_counter.TotalInclusiveTicks;
            }
        }

        public long TotalInclusiveMicroSeconds
        {
            get
            {
                return m_state.m_counter.TotalInclusiveMicroSeconds;
            }
        }

        public long TotalExclusiveTicks
        {
            get
            {
                return m_state.m_counter.TotalExclusiveTicks;
            }
        }

        public long TotalExclusiveMicroSeconds
        {
            get
            {
                return m_state.m_counter.TotalExclusiveMicroSeconds;
            }
        }

        public int Hits
        {
            get
            {
                return m_state.m_counter.Hits;
            }
        }

        //
        // Debug Methods
        //

        public static bool IsEnabled()
        {
            return true;
        }

        public static void DumpAllByType( System.IO.TextWriter textWriter )
        {
            GrowOnlyHashTable< string, List< State > > lookup     = HashTableFactory.New< string, List< State > >();
            GrowOnlyHashTable< string, long          > lookupCost = HashTableFactory.New< string, long          >();

            foreach(GrowOnlySet< State > set in s_threads)
            {
                foreach(State st in set)
                {
                    string key = st.m_context.ToString();;

                    if(HashTableWithListFactory.Add( lookup, key, st ).Count == 1)
                    {
                        lookupCost[key] = 0;
                    }

                    lookupCost[key] += st.m_counter.TotalInclusiveMicroSeconds;
                }
            }

            string[] keyArray = lookup.KeysToArray();

            Array.Sort( keyArray, delegate ( string left, string right )
            {
                return lookupCost[right].CompareTo( lookupCost[left] );
            } );

            foreach(string key in keyArray)
            {
                State[] states = SortByMostExpensive( lookup[key].ToArray() );

                //--//

                long totalInclusiveMicroSeconds = 0;
                long totalExclusiveMicroSeconds = 0;
                int  totalHits                  = 0;

                foreach(State st in states)
                {
                    totalInclusiveMicroSeconds += st.m_counter.TotalInclusiveMicroSeconds;
                    totalExclusiveMicroSeconds += st.m_counter.TotalExclusiveMicroSeconds;
                    totalHits                  += st.m_counter.Hits;
                }

                textWriter.Write( "Key = {0}:    ", key );
                Emit( textWriter, "Exclusive", false, totalExclusiveMicroSeconds, totalHits );
                Emit( textWriter, "Inclusive", false, totalInclusiveMicroSeconds, totalHits );
                textWriter.WriteLine();

                foreach(State st in states)
                {
                    long inclusiveMicroSeconds = st.m_counter.TotalInclusiveMicroSeconds;
                    long exclusiveMicroSeconds = st.m_counter.TotalExclusiveMicroSeconds;
                    int  hits                  = st.m_counter.Hits;

                    textWriter.Write( "    " );
                    Emit( textWriter, "Exclusive", true, exclusiveMicroSeconds, hits );
                    Emit( textWriter, "Inclusive", true, inclusiveMicroSeconds, hits );
                    textWriter.WriteLine( " | {0}", st.m_reason );

                    if(st.m_children != null)
                    {
                        GrowOnlyHashTable< Timing.Nested, State > childrenInverted = GetInvertedHashTable( st.m_children );

                        foreach(Timing.Nested childCounter in SortByMostExpensive( childrenInverted.KeysToArray() ))
                        {
                            State child = childrenInverted[childCounter];

                            textWriter.Write( "    " );
                            Emit( textWriter, "Child Exc", true, childCounter.TotalExclusiveMicroSeconds, childCounter.Hits );

                            if(child.m_context != st.m_context)
                            {
                                textWriter.WriteLine( " | {0,-30} / {1}", child.m_reason, child.m_context );
                            }
                            else
                            {
                                textWriter.WriteLine( " | {0}", child.m_reason );
                            }
                        }
                    }

                    textWriter.WriteLine();
                }

                textWriter.WriteLine();
            }
        }

        public static void DumpAllByReason( System.IO.TextWriter textWriter )
        {
            GrowOnlyHashTable< string, List< State > > lookup     = HashTableFactory.New< string, List< State > >();
            GrowOnlyHashTable< string, long          > lookupCost = HashTableFactory.New< string, long          >();

            foreach(GrowOnlySet< State > set in s_threads)
            {
                foreach(State st in set)
                {
                    string        key = st.m_reason;
                    List< State > lst;

                    if(lookup.TryGetValue( key, out lst ) == false)
                    {
                        lst = new List< State >();

                        lookup    [key] = lst;
                        lookupCost[key] = 0;
                    }

                    lookupCost[key] += st.m_counter.TotalInclusiveMicroSeconds;

                    lst.Add( st );
                }
            }

            string[] keyArray = lookup.KeysToArray();

            Array.Sort( keyArray, delegate ( string left, string right )
            {
                return lookupCost[right].CompareTo( lookupCost[left] );
            } );

            foreach(string key in keyArray)
            {
                State[] states = SortByMostExpensive( lookup[key].ToArray() );

                //--//

                long totalInclusiveMicroSeconds = 0;
                long totalExclusiveMicroSeconds = 0;
                int  totalHits                  = 0;

                foreach(State st in states)
                {
                    totalInclusiveMicroSeconds += st.m_counter.TotalInclusiveMicroSeconds;
                    totalExclusiveMicroSeconds += st.m_counter.TotalExclusiveMicroSeconds;
                    totalHits                  += st.m_counter.Hits;
                }

                textWriter.Write( "Key = {0}:    ", key );
                Emit( textWriter, "Exclusive", false, totalExclusiveMicroSeconds, totalHits );
                Emit( textWriter, "Inclusive", false, totalInclusiveMicroSeconds, totalHits );
                textWriter.WriteLine();

                foreach(State st in states)
                {
                    long inclusiveMicroSeconds = st.m_counter.TotalInclusiveMicroSeconds;
                    long exclusiveMicroSeconds = st.m_counter.TotalExclusiveMicroSeconds;
                    int  hits                  = st.m_counter.Hits;

                    textWriter.Write( "    " );
                    Emit( textWriter, "Exclusive", true, exclusiveMicroSeconds, hits );
                    Emit( textWriter, "Inclusive", true, inclusiveMicroSeconds, hits );
                    textWriter.WriteLine( " | {0}", st.m_context );

                    if(st.m_children != null)
                    {
                        GrowOnlyHashTable< Timing.Nested, State > childrenInverted = GetInvertedHashTable( st.m_children );

                        foreach(Timing.Nested childCounter in SortByMostExpensive( childrenInverted.KeysToArray() ))
                        {
                            State child = childrenInverted[childCounter];

                            textWriter.Write( "    " );
                            Emit( textWriter, "Child Exc", true, childCounter.TotalExclusiveMicroSeconds, childCounter.Hits );

                            if(child.m_context != st.m_context)
                            {
                                textWriter.WriteLine( " | {0,-30} / {1}", child.m_reason, child.m_context );
                            }
                            else
                            {
                                textWriter.WriteLine( " | {0}", child.m_reason );
                            }
                        }
                    }

                    textWriter.WriteLine();
                }

                textWriter.WriteLine();
            }
        }

        private static GrowOnlyHashTable< Timing.Nested, State > GetInvertedHashTable( GrowOnlyHashTable< State, Timing.Nested > ht )
        {
            GrowOnlyHashTable< Timing.Nested, State > htInverted = HashTableFactory.NewWithReferenceEquality< Timing.Nested, State >();

            htInverted.Load( ht.ValuesToArray(), ht.KeysToArray() );

            return htInverted;
        }

        private static Timing.Nested[] SortByMostExpensive( Timing.Nested[] children )
        {
            Array.Sort( children, delegate ( Timing.Nested left, Timing.Nested right )
            {
                long leftUSec  = left .TotalExclusiveMicroSeconds;
                long rightUSec = right.TotalExclusiveMicroSeconds;

                return rightUSec.CompareTo( leftUSec );
            } );

            return children;
        }

        private static State[] SortByMostExpensive( State[] states )
        {
            Array.Sort( states, delegate ( State left, State right )
            {
                long leftUSec  = left .m_counter.TotalInclusiveMicroSeconds;
                long rightUSec = right.m_counter.TotalInclusiveMicroSeconds;

                return rightUSec.CompareTo( leftUSec );
            } );

            return states;
        }

        private static State[] SortByReason( State[] states )
        {
            Array.Sort( states, delegate ( State left, State right )
            {
                return left.m_reason.CompareTo( right.m_reason );
            } );

            return states;
        }

        private static void Emit( System.IO.TextWriter textWriter        ,
                                  string               text              ,
                                  bool                 fAlign            ,
                                  long                 totalMicroSeconds ,
                                  int                  totalHits         )
        {
            string fmt;

            if(fAlign)
            {
               fmt = " {0} => {1,10} uSec/{2,10} [{3,10:F1} uSec per hit]";
            }
            else
            {
               fmt = " {0} => {1} uSec/{2} [{3} uSec per hit]";
            }
            
            textWriter.Write( fmt, text, totalMicroSeconds, totalHits, totalHits > 0 ? (float)totalMicroSeconds / (float)totalHits : 0 );
        }
    }
#else
    public struct ContextualTiming : IDisposable
    {
        public ContextualTiming( object context ,
                                 string reason  )
        {
        }

        public void Dispose()
        {
        }

        //
        // Access Methods
        //

        public long TotalInclusiveTicks
        {
            get
            {
                return 0;
            }
        }

        public long TotalInclusiveMicroSeconds
        {
            get
            {
                return 0;
            }
        }

        public long TotalExclusiveTicks
        {
            get
            {
                return 0;
            }
        }

        public long TotalExclusiveMicroSeconds
        {
            get
            {
                return 0;
            }
        }

        //
        // Debug Methods
        //

        public static bool IsEnabled()
        {
            return false;
        }

        public static void DumpAllByType( System.IO.TextWriter textWriter )
        {
        }

        public static void DumpAllByReason( System.IO.TextWriter textWriter )
        {
        }
    }
#endif
}