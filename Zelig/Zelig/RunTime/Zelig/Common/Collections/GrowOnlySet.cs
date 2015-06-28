//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig
{
    using System;
    using System.Collections;
    using System.Collections.Generic;


    public class GrowOnlySet< TKey >
    {
        //
        // State
        //

        private int[]                     m_buckets;
        private int[]                     m_entries_HashCode; // Lower 31 bits of hash code, -1 if unused
        private int[]                     m_entries_Next;     // Index of next entry, -1 if last
        private TKey[]                    m_entries_Key;      // Key of entry
                                    
        private int                       m_count;
        private int                       m_version;

        private IEqualityComparer< TKey > m_comparer;

        //
        // Constructor Methods
        //

        protected GrowOnlySet() : this( EqualityComparer< TKey >.Default )
        {
        }

        internal GrowOnlySet( IEqualityComparer< TKey > comparer )
        {
            m_comparer = comparer;
        }

        //
        // Helper Methods
        //

        public void Clear()
        {
            m_buckets          = null;
            m_entries_HashCode = null;
            m_entries_Next     = null;
            m_entries_Key      = null;

            m_count = 0;
            m_version++;
        }

        public void RefreshHashCodes()
        {
            for(int i = 0; i < m_count; i++)
            {
                m_entries_HashCode[i] = m_comparer.GetHashCode( m_entries_Key[i] ) & 0x7FFFFFFF;
            }

            RebuildBuckets();
        }

        public bool Insert( TKey key )
        {
            if(m_buckets == null)
            {
                Initialize( 0 );
            }

            int hashCode     = m_comparer.GetHashCode( key ) & 0x7FFFFFFF;
            int targetBucket = hashCode % m_buckets.Length;

            for(int i = m_buckets[targetBucket]; i-- > 0; i = m_entries_Next[i])
            {
                if(m_entries_HashCode[i] == hashCode && m_comparer.Equals( m_entries_Key[i], key ))
                {
                    return true;
                }
            }

            if(m_count == m_entries_HashCode.Length)
            {
                Resize();

                targetBucket = hashCode % m_buckets.Length;
            }

            int index = m_count++;

            m_entries_HashCode[index] = hashCode;
            m_entries_Next    [index] = m_buckets[targetBucket];
            m_entries_Key     [index] = key;

            m_buckets[targetBucket] = index + 1;
            m_version++;
            return false;
        }

        public TKey MakeUnique( TKey key )
        {
            TKey oldKey;

            if(Contains( key, out oldKey ))
            {
                return oldKey;
            }

            Insert( key );

            return key;
        }

        public bool Contains( TKey key )
        {
            return FindEntry( key ) >= 0;
        }

        public bool Contains(     TKey key   ,
                              out TKey value )
        {
            int i = FindEntry( key );
            if(i >= 0)
            {
                value = m_entries_Key[i];
                return true;
            }

            value = default( TKey );
            return false;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator( this, m_entries_Key );
        }

        public void Merge( GrowOnlySet< TKey > target )
        {
            int    num  = target.m_count;
            TKey[] keys = target.m_entries_Key;

            for(int i = 0; i < num; i++)
            {
                this.Insert( keys[i] );
            }
        }

        public TKey[] ToArray()
        {
            TKey[] res = new TKey[m_count];

            if(m_count > 0)
            {
                Array.Copy( m_entries_Key, res, m_count );
            }

            return res;
        }

        public void Load( TKey[] keys )
        {
            int size = keys.Length;

            Initialize( size );

            m_count = size;
            m_version++;

            Array.Copy( keys, m_entries_Key, size );

            RefreshHashCodes();
        }

        //--//

        private void Initialize( int capacity )
        {
            int size = HashHelpers.GetPrime( capacity );

            m_buckets          = new int [size];
            m_entries_HashCode = new int [size];
            m_entries_Next     = new int [size];
            m_entries_Key      = new TKey[size];
        }

        private void Resize()
        {
            int    newSize             = HashHelpers.GetPrime( m_count * 2 );
            int[]  newBuckets          = new int [newSize];
            int[]  newEntries_HashCode = new int [newSize];
            int[]  newEntries_Next     = new int [newSize];
            TKey[] newEntries_Key      = new TKey[newSize];

            Array.Copy( m_entries_HashCode, 0, newEntries_HashCode, 0, m_count );
            Array.Copy( m_entries_Key     , 0, newEntries_Key     , 0, m_count );

            m_buckets          = newBuckets;
            m_entries_HashCode = newEntries_HashCode;
            m_entries_Next     = newEntries_Next;
            m_entries_Key      = newEntries_Key;

            RebuildBuckets();
        }

        private void RebuildBuckets()
        {
            if(m_buckets != null)
            {
                int size = m_buckets.Length;

                Array.Clear( m_buckets, 0, size );

                for(int i = 0; i < m_count; i++)
                {
                    int bucket = m_entries_HashCode[i] % size;

                    m_entries_Next[i] = m_buckets[bucket];

                    m_buckets[bucket] = i + 1;
                }
            }
        }

        private int FindEntry( TKey key )
        {
            if(m_buckets != null)
            {
                int hashCode = m_comparer.GetHashCode( key ) & 0x7FFFFFFF;

                for(int i = m_buckets[hashCode % m_buckets.Length]; i-- > 0; i = m_entries_Next[i])
                {
                    if(m_entries_HashCode[i] == hashCode && m_comparer.Equals( m_entries_Key[i], key ))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        //--//

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

        public GrowOnlySet< TKey > CloneSettings()
        {
            return new GrowOnlySet< TKey >( m_comparer );
        }

        public GrowOnlySet< TKey > Clone()
        {
            GrowOnlySet< TKey > copy = CloneSettings();

            if(m_count > 0)
            {
                copy.m_buckets          = ArrayUtility.CopyNotNullArray( m_buckets          );
                copy.m_entries_HashCode = ArrayUtility.CopyNotNullArray( m_entries_HashCode );
                copy.m_entries_Next     = ArrayUtility.CopyNotNullArray( m_entries_Next     );
                copy.m_entries_Key      = ArrayUtility.CopyNotNullArray( m_entries_Key      );
                                    
                copy.m_count = m_count;
            }

            return copy;
        }

        //--//

        //
        // Debug Methods
        //

        public void Dump()
        {
            for(int i = 0; i < m_count; i++)
            {
                Console.WriteLine( "{0} = {1}", i, m_entries_Key[i] );
            }
        }

        //--//--//--//--//--//--//--//--//

        public struct Enumerator
        {
            //
            // State
            //

            private GrowOnlySet< TKey > m_dictionary;
            private int                 m_index;
            private int                 m_version;
            private TKey[]              m_values;

            //
            // Constructor Methods
            //

            internal Enumerator( GrowOnlySet< TKey > dictionary ,
                                 TKey[]              values     )
            {
                m_dictionary = dictionary;
                m_version    = dictionary.m_version;
                m_index      = 0;
                m_values     = values;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if(m_version != m_dictionary.m_version)
                {
                    throw new Exception( "Dictionary changed" );
                }

                if(m_index < m_dictionary.m_count)
                {
                    m_index++;
                    return true;
                }

                return false;
            }

            public TKey Current
            {
                get
                {
                    return m_values[m_index-1];
                }
            }

            void Reset()
            {
                if(m_version != m_dictionary.m_version)
                {
                    throw new Exception( "Dictionary changed" );
                }

                m_index = 0;
            }
        }
    }
}
