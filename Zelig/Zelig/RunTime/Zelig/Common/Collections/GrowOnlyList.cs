//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;
    using System.Collections;
    using System.Collections.Generic;


    public class GrowOnlyList< T >
    {
        class Cluster
        {
            internal const int DefaultCapacity = 4;
            internal const int MaxCapacity     = 256;

            //
            // State
            //

            internal          Cluster m_next;
            internal readonly T[]     m_elements;
            internal          int     m_pos;

            //
            // Costructor Methods
            //

            internal Cluster( int capacity )
            {
                m_elements = new T[capacity];
                m_pos      = 0;
            }

            //
            // Helper Methods
            //

            internal Cluster Add( T obj )
            {
                int capacity = m_elements.Length;

                if(m_pos == capacity)
                {
                    m_next = new Cluster( Math.Min( capacity * 2, MaxCapacity ) );

                    return m_next.Add( obj );
                }

                m_elements[m_pos++] = obj;

                return this;
            }
        }

        //
        // State
        //

        private Cluster m_first;
        private Cluster m_last;
        private int     m_count;

        //
        // Costructor Methods
        //

        public GrowOnlyList()
        {
            Clear();
        }

        //
        // Helper Methods
        //

        public void Clear()
        {
            m_first = new Cluster( Cluster.DefaultCapacity );
            m_last  = m_first;
            m_count = 0;
        }

        public void Add( T obj )
        {
            m_last = m_last.Add( obj );
            m_count++;
        }

        //--//

        public Enumerator GetEnumerator()
        {
            return new Enumerator( this );
        }

        //
        // Access Methods
        //

        public int Count
        {
            get
            {
                return m_count;
            }
        }

        public T this[int index]
        {
            get
            {
                if(index >= 0 && index < m_count)
                {
                    for(Cluster cluster = m_first; cluster != null; cluster = cluster.m_next)
                    {
                        if(index < cluster.m_pos)
                        {
                            return cluster.m_elements[index];
                        }

                        index -= cluster.m_pos;
                    }
                }

                throw new ArgumentOutOfRangeException();
            }

            set
            {
                if(index >= 0 && index < m_count)
                {
                    for(Cluster cluster = m_first; cluster != null; cluster = cluster.m_next)
                    {
                        if(index < cluster.m_pos)
                        {
                            cluster.m_elements[index] = value;
                            return;
                        }

                        index -= cluster.m_pos;
                    }
                }

                throw new ArgumentOutOfRangeException();
            }
        }

        //--//--//--//--//--//--//--//--//

        public struct Enumerator
        {
            //
            // State
            //

            private Cluster m_first;
            private Cluster m_current;
            private int     m_index;

            //
            // Constructor Methods
            //

            internal Enumerator( GrowOnlyList< T > list )
            {
                m_first   = list.m_first;
                m_current = m_first;
                m_index   = 0;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                while(m_current != null)
                {
                    if(m_index < m_current.m_pos)
                    {
                        m_index++;
                        return true;
                    }

                    m_current = m_current.m_next;
                    m_index   = 0;
                }

                return false;
            }

            public T Current
            {
                get
                {
                    return m_current.m_elements[m_index-1];
                }
            }

            void Reset()
            {
                m_current = m_first;
                m_index   = 0;
            }
        }
    }
}
