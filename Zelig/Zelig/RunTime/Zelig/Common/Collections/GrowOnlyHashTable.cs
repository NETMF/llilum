//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DETECT_HIGH_COLLISION_RATES

namespace Microsoft.Zelig
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class GrowOnlyHashTable< TKey, TValue >
    {
        //
        // State
        //

        private int[]                     m_buckets;
        private int[]                     m_entries_HashCode; // Lower 31 bits of hash code, -1 if unused
        private int[]                     m_entries_Next;     // Index of next entry, -1 if last
        private TKey[]                    m_entries_Key;      // Key of entry
        private TValue[]                  m_entries_Value;    // Value of entry

        private int                       m_bucketThreshold;
                                       
        private int                       m_count;
        private int                       m_version;

        private IEqualityComparer< TKey > m_comparer;
        private KeyEnumerable             m_keys;
        private ValueEnumerable           m_values;

        //
        // Constructor Methods
        //

        protected GrowOnlyHashTable() : this( EqualityComparer< TKey >.Default )
        {
        }

        internal GrowOnlyHashTable( IEqualityComparer< TKey > comparer )
        {
            m_comparer = comparer;
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

        public TValue this[TKey key]
        {
            get
            {
                int i = FindEntry( key );
                if(i >= 0)
                {
                    return m_entries_Value[i];
                }

                throw new Exception( "Key Not Found" );
            }

            set
            {
                Insert( key, value, false );
            }
        }

        public KeyEnumerable Keys
        {
            get
            {
                if(m_keys == null)
                {
                    m_keys = new KeyEnumerable( this );
                }

                return m_keys;
            }
        }

        public ValueEnumerable Values
        {
            get
            {
                if(m_values == null)
                {
                    m_values = new ValueEnumerable( this );
                }

                return m_values;
            }
        }

        public GrowOnlyHashTable< TKey, TValue > CloneSettings()
        {
            return new GrowOnlyHashTable< TKey, TValue >( m_comparer );
        }

        public GrowOnlyHashTable<TKey, TValue> CloneSettingsAndSize()
        {
            GrowOnlyHashTable<TKey, TValue> res = new GrowOnlyHashTable<TKey, TValue>( m_comparer );

            res.Initialize( this.Count );

            return res;
        }

        public GrowOnlyHashTable< TKey, TValue > Clone()
        {
            GrowOnlyHashTable< TKey, TValue > copy = CloneSettings();

            if(m_count > 0)
            {
                copy.m_buckets          = ArrayUtility.CopyNotNullArray( m_buckets          );
                copy.m_entries_HashCode = ArrayUtility.CopyNotNullArray( m_entries_HashCode );
                copy.m_entries_Next     = ArrayUtility.CopyNotNullArray( m_entries_Next     );
                copy.m_entries_Key      = ArrayUtility.CopyNotNullArray( m_entries_Key      );
                copy.m_entries_Value    = ArrayUtility.CopyNotNullArray( m_entries_Value    );

                copy.m_bucketThreshold = m_bucketThreshold;
                                    
                copy.m_count = m_count;
            }

            return copy;
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
            m_entries_Value    = null;

            m_bucketThreshold = 0;
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

        public bool TryGetKey(     TKey key   ,
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

        public bool TryGetValue(     TKey   key   ,
                                 out TValue value )
        {
            int i = FindEntry( key );
            if(i >= 0)
            {
                value = m_entries_Value[i];
                return true;
            }

            value = default( TValue );
            return false;
        }

        public TKey GetKey( TKey key )
        {
            int i = FindEntry( key );
            if(i >= 0)
            {
                return m_entries_Key[i];
            }

            return default( TKey );
        }

        public TValue GetValue( TKey key )
        {
            int i = FindEntry( key );
            if(i >= 0)
            {
                return m_entries_Value[i];
            }

            return default( TValue );
        }

        public void Add( TKey   key   ,
                         TValue value )
        {
            Insert( key, value, true );
        }

        public bool Update( TKey   key   ,
                            TValue value )
        {
            return Insert( key, value, false );
        }

        public bool ContainsKey( TKey key )
        {
            return FindEntry( key ) >= 0;
        }

        public bool ContainsValue( TValue value )
        {
            return FindValue( value ) >= 0;
        }

        public bool ContainsValue(     TValue value ,
                                   out TKey   key   )
        {
            int pos = FindValue( value );

            if(pos >= 0)
            {
                key = m_entries_Key[pos];
                return true;
            }
            else
            {
                key = default( TKey );
                return false;
            }
        }

        public void Merge( GrowOnlyHashTable< TKey, TValue > target )
        {
            for(int i = 0; i < target.m_count; i++)
            {
                this[ target.m_entries_Key[i] ] = target.m_entries_Value[i];
            }
        }

        public KeyValuePair< TKey, TValue >[] ToArray()
        {
            KeyValuePair< TKey, TValue >[] res = new KeyValuePair< TKey, TValue >[m_count];

            for(int i = 0; i < m_count; i++)
            {
                res[i] = new KeyValuePair< TKey, TValue >( m_entries_Key[i], m_entries_Value[i] );
            }

            return res;
        }

        public TKey[] KeysToArray()
        {
            TKey[] res = new TKey[m_count];

            if(m_count > 0)
            {
                Array.Copy( m_entries_Key, res, m_count );
            }

            return res;
        }

        public TValue[] ValuesToArray()
        {
            TValue[] res = new TValue[m_count];

            if(m_count > 0)
            {
                Array.Copy( m_entries_Value, res, m_count );
            }

            return res;
        }

        public void Load( TKey[]   keys   ,
                          TValue[] values )
        {
            int size = keys.Length;

            Initialize( size );

            m_count = size;
            m_version++;

            Array.Copy( keys  , m_entries_Key  , size );
            Array.Copy( values, m_entries_Value, size );

            RefreshHashCodes();
        }

        //--//

        private void Initialize( int capacity )
        {
            int size = HashHelpers.GetPrime( capacity );

            m_buckets          = new int   [size];
            m_entries_HashCode = new int   [size];
            m_entries_Next     = new int   [size];
            m_entries_Key      = new TKey  [size];
            m_entries_Value    = new TValue[size];

            m_bucketThreshold = 3 * size / 4;
        }

        private void Resize()
        {
            int      newSize             = HashHelpers.GetPrime( m_count * 2 );
            int[]    newBuckets          = new int   [newSize];
            int[]    newEntries_HashCode = new int   [newSize];
            int[]    newEntries_Next     = new int   [newSize];
            TKey[]   newEntries_Key      = new TKey  [newSize];
            TValue[] newEntries_Value    = new TValue[newSize];

            Array.Copy( m_entries_HashCode, 0, newEntries_HashCode, 0, m_count );
            Array.Copy( m_entries_Key     , 0, newEntries_Key     , 0, m_count );
            Array.Copy( m_entries_Value   , 0, newEntries_Value   , 0, m_count );

            m_buckets          = newBuckets;
            m_entries_HashCode = newEntries_HashCode;
            m_entries_Next     = newEntries_Next;
            m_entries_Key      = newEntries_Key;
            m_entries_Value    = newEntries_Value;

            m_bucketThreshold = 3 * newSize / 4;

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

        private int FindValue( TValue value )
        {
            if(value == null)
            {
                for(int i = 0; i < m_count; i++)
                {
                    if(m_entries_Value[i] == null)
                    {
                        return i;
                    }
                }
            }
            else
            {
                EqualityComparer<TValue> c = EqualityComparer<TValue>.Default;

                for(int i = 0; i < m_count; i++)
                {
                    if(c.Equals( m_entries_Value[i], value ))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

#if DETECT_HIGH_COLLISION_RATES
        uint m_collisions = 0;
#endif

        private bool Insert( TKey   key    ,
                             TValue value  ,
                             bool   unique )
        {
            if(m_buckets == null)
            {
                Initialize( 32 );
            }

            int hashCode     = m_comparer.GetHashCode( key ) & 0x7FFFFFFF;
            int targetBucket = hashCode % m_buckets.Length;

            for(int i = m_buckets[targetBucket]; i-- > 0; i = m_entries_Next[i])
            {
                if(m_entries_HashCode[i] == hashCode && m_comparer.Equals( m_entries_Key[i], key ))
                {
                    if(unique)
                    {
                        throw new Exception( "Duplicate Key" );
                    }

                    m_entries_Value[i] = value;
                    m_version++;
                    return true;
                }
            }

            if(m_count >= m_bucketThreshold)
            {
#if DETECT_HIGH_COLLISION_RATES
                double collRate = (double)m_collisions / (double)m_count;

                if(collRate > 0.40 && m_count > 1000)
                {
                    Console.WriteLine( "***** HIGH COLLISION RATE" );
                    Console.WriteLine( "***** Collision Rate: {0}", collRate );
                    Console.WriteLine( "***** KEY TYPE: " + key.GetType() );
                    Console.WriteLine( "***** Total Hash Count: {0}", m_count );
                }

                m_collisions = 0;
#endif

                Resize();

                targetBucket = hashCode % m_buckets.Length;
            }

            int index = m_count++;

#if DETECT_HIGH_COLLISION_RATES
            if(m_buckets[targetBucket] != 0) m_collisions++;
#endif

            m_entries_HashCode[index] = hashCode;
            m_entries_Next    [index] = m_buckets[targetBucket];
            m_entries_Key     [index] = key;
            m_entries_Value   [index] = value;

            m_buckets[targetBucket] = index + 1;
            m_version++;
            return false;
        }

        //--//

        //
        // Debug Methods
        //

        public void Dump()
        {
            for(int i = 0; i < m_count; i++)
            {
                Console.WriteLine( "{0} = {1} => {2}", i, m_entries_Key[i], m_entries_Value[i] );
            }
        }

        //--//--//--//--//--//--//--//--//

        public sealed class KeyEnumerable
        {
            //
            // State
            //

            private GrowOnlyHashTable< TKey, TValue > m_owner;

            //
            // Constructor Methods
            //

            public KeyEnumerable( GrowOnlyHashTable< TKey, TValue > owner )
            {
                m_owner = owner;
            }

            public Enumerator< TKey > GetEnumerator()
            {
                return new Enumerator< TKey >( m_owner, m_owner.m_entries_Key );
            }
        }

        public sealed class ValueEnumerable
        {
            //
            // State
            //

            private GrowOnlyHashTable< TKey, TValue > m_owner;

            //
            // Constructor Methods
            //

            public ValueEnumerable( GrowOnlyHashTable< TKey, TValue > owner )
            {
                m_owner = owner;
            }

            public Enumerator< TValue > GetEnumerator()
            {
                return new Enumerator< TValue >( m_owner, m_owner.m_entries_Value );
            }
        }

        public struct Enumerator<T>
        {
            //
            // State
            //

            private GrowOnlyHashTable< TKey, TValue > m_owner;
            private int                               m_index;
            private int                               m_version;
            private T[]                               m_values;

            //
            // Constructor Methods
            //

            internal Enumerator( GrowOnlyHashTable< TKey, TValue > owner  ,
                                 T[]                               values )
            {
                m_owner   = owner;
                m_version = owner.m_version;
                m_index   = 0;
                m_values  = values;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if(m_version != m_owner.m_version)
                {
                    throw new Exception( "Dictionary changed" );
                }

                if(m_index < m_owner.m_count)
                {
                    m_index++;
                    return true;
                }

                return false;
            }

            public T Current
            {
                get
                {
                    return m_values[m_index-1];
                }
            }

            void Reset()
            {
                if(m_version != m_owner.m_version)
                {
                    throw new Exception( "Dictionary changed" );
                }

                m_index = 0;
            }
        }
    }
}
