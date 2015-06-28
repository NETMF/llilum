// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  Dictionary
**
** Purpose: Generic hash table implementation
**
**
===========================================================*/
namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime.Serialization;
////using System.Security.Permissions;

////[DebuggerTypeProxy(typeof(Mscorlib_DictionaryDebugView<,>))]
////[DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary /*, ISerializable, IDeserializationCallback*/
    {
        private struct Entry
        {
            public int    hashCode;    // Lower 31 bits of hash code, -1 if unused
            public int    next;        // Index of next entry, -1 if last
            public TKey   key;         // Key of entry
            public TValue value;       // Value of entry
        }

        // constants for serialization
        private const String VersionName       = "Version";
        private const String HashSizeName      = "HashSize";  // Must save buckets.Length
        private const String KeyValuePairsName = "KeyValuePairs";
        private const String ComparerName      = "Comparer";

        private int[]                   m_buckets;
        private Entry[]                 m_entries;
        private int                     m_count;
        private int                     m_version;
        private int                     m_freeList;
        private int                     m_freeCount;
        private IEqualityComparer<TKey> m_comparer;
        private KeyCollection           m_keys;
        private ValueCollection         m_values;
        private Object                  m_syncRoot;

////    private SerializationInfo       m_siInfo; //A temporary variable which we need during deserialization.

        public Dictionary() : this( 0, null )
        {
        }

        public Dictionary( int capacity ) : this( capacity, null )
        {
        }

        public Dictionary( IEqualityComparer<TKey> comparer ) : this( 0, comparer )
        {
        }

        public Dictionary( int capacity, IEqualityComparer<TKey> comparer )
        {
            if(capacity < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.capacity );
            }

            if(capacity > 0)
            {
                Initialize( capacity );
            }

            if(comparer == null)
            {
                comparer = EqualityComparer<TKey>.Default;
            }

            m_comparer = comparer;
        }

        public Dictionary( IDictionary<TKey, TValue> dictionary ) : this( dictionary, null )
        {
        }

        public Dictionary( IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer ) : this( dictionary != null ? dictionary.Count : 0, comparer )
        {
            if(dictionary == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.dictionary );
            }

            foreach(KeyValuePair<TKey, TValue> pair in dictionary)
            {
                Add( pair.Key, pair.Value );
            }
        }

////    protected Dictionary( SerializationInfo info, StreamingContext context )
////    {
////        //We can't do anything with the keys and values until the entire graph has been deserialized
////        //and we have a resonable estimate that GetHashCode is not going to fail.  For the time being,
////        //we'll just cache this.  The graph is not valid until OnDeserialization has been called.
////        m_siInfo = info;
////    }

        public IEqualityComparer<TKey> Comparer
        {
            get
            {
                return m_comparer;
            }
        }

        public int Count
        {
            get
            {
                return m_count - m_freeCount;
            }
        }

        public KeyCollection Keys
        {
            get
            {
                if(m_keys == null)
                {
                    m_keys = new KeyCollection( this );
                }

                return m_keys;
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                if(m_keys == null)
                {
                    m_keys = new KeyCollection( this );
                }

                return m_keys;
            }
        }

        public ValueCollection Values
        {
            get
            {
                if(m_values == null)
                {
                    m_values = new ValueCollection( this );
                }

                return m_values;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                if(m_values == null)
                {
                    m_values = new ValueCollection( this );
                }

                return m_values;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                int i = FindEntry( key );
                if(i >= 0)
                {
                    return m_entries[i].value;
                }

                ThrowHelper.ThrowKeyNotFoundException();

                return default( TValue );
            }

            set
            {
                Insert( key, value, false );
            }
        }

        public void Add( TKey key, TValue value )
        {
            Insert( key, value, true );
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add( KeyValuePair<TKey, TValue> keyValuePair )
        {
            Add( keyValuePair.Key, keyValuePair.Value );
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains( KeyValuePair<TKey, TValue> keyValuePair )
        {
            int i = FindEntry( keyValuePair.Key );

            if(i >= 0 && EqualityComparer<TValue>.Default.Equals( m_entries[i].value, keyValuePair.Value ))
            {
                return true;
            }

            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove( KeyValuePair<TKey, TValue> keyValuePair )
        {
            int i = FindEntry( keyValuePair.Key );

            if(i >= 0 && EqualityComparer<TValue>.Default.Equals( m_entries[i].value, keyValuePair.Value ))
            {
                Remove( keyValuePair.Key );
                return true;
            }

            return false;
        }

        public void Clear()
        {
            if(m_count > 0)
            {
                for(int i = 0; i < m_buckets.Length; i++)
                {
                    m_buckets[i] = -1;
                }

                Array.Clear( m_entries, 0, m_count );

                m_freeList  = -1;
                m_count     = 0;
                m_freeCount = 0;
                m_version++;
            }
        }

        public bool ContainsKey( TKey key )
        {
            return FindEntry( key ) >= 0;
        }

        public bool ContainsValue( TValue value )
        {
            if(value == null)
            {
                for(int i = 0; i < m_count; i++)
                {
                    if(m_entries[i].hashCode >= 0 && m_entries[i].value == null)
                    {
                        return true;
                    }
                }
            }
            else
            {
                EqualityComparer<TValue> c = EqualityComparer<TValue>.Default;
                for(int i = 0; i < m_count; i++)
                {
                    if(m_entries[i].hashCode >= 0 && c.Equals( m_entries[i].value, value ))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void CopyTo( KeyValuePair<TKey, TValue>[] array, int index )
        {
            if(array == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.array );
            }

            if(index < 0 || index > array.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum );
            }

            if(array.Length - index < Count)
            {
                ThrowHelper.ThrowArgumentException( ExceptionResource.Arg_ArrayPlusOffTooSmall );
            }

            int     count   = m_count;
            Entry[] entries = m_entries;

            for(int i = 0; i < count; i++)
            {
                if(entries[i].hashCode >= 0)
                {
                    array[index++] = new KeyValuePair<TKey, TValue>( entries[i].key, entries[i].value );
                }
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator( this, Enumerator.KeyValuePair );
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator( this, Enumerator.KeyValuePair );
        }

////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter )]
////    public virtual void GetObjectData( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            ThrowHelper.ThrowArgumentNullException( ExceptionArgument.info );
////        }
////
////        info.AddValue( VersionName , m_version                                                                   );
////        info.AddValue( ComparerName, m_comparer                              , typeof( IEqualityComparer<TKey> ) );
////        info.AddValue( HashSizeName, m_buckets == null ? 0 : m_buckets.Length                                    ); //This is the length of the bucket array.
////
////        if(m_buckets != null)
////        {
////            KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[Count];
////
////            CopyTo( array, 0 );
////
////            info.AddValue( KeyValuePairsName, array, typeof( KeyValuePair<TKey, TValue>[] ) );
////        }
////    }

        private int FindEntry( TKey key )
        {
            if(key == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.key );
            }

            if(m_buckets != null)
            {
                int hashCode = m_comparer.GetHashCode( key ) & 0x7FFFFFFF;

                for(int i = m_buckets[hashCode % m_buckets.Length]; i >= 0; i = m_entries[i].next)
                {
                    if(m_entries[i].hashCode == hashCode && m_comparer.Equals( m_entries[i].key, key ))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private void Initialize( int capacity )
        {
            int size = HashHelpers.GetPrime( capacity );

            m_buckets = new int[size];

            for(int i = 0; i < m_buckets.Length; i++)
            {
                m_buckets[i] = -1;
            }

            m_entries  = new Entry[size];
            m_freeList = -1;
        }

        private void Insert( TKey key, TValue value, bool add )
        {
            if(key == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.key );
            }

            if(m_buckets == null)
            {
                Initialize( 0 );
            }

            int hashCode     = m_comparer.GetHashCode( key ) & 0x7FFFFFFF;
            int targetBucket = hashCode % m_buckets.Length;

            for(int i = m_buckets[targetBucket]; i >= 0; i = m_entries[i].next)
            {
                if(m_entries[i].hashCode == hashCode && m_comparer.Equals( m_entries[i].key, key ))
                {
                    if(add)
                    {
                        ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_AddingDuplicate );
                    }

                    m_entries[i].value = value;
                    m_version++;
                    return;
                }
            }

            int index;
            if(m_freeCount > 0)
            {
                index      = m_freeList;
                m_freeList = m_entries[index].next;
                m_freeCount--;
            }
            else
            {
                if(m_count == m_entries.Length)
                {
                    Resize();
                    targetBucket = hashCode % m_buckets.Length;
                }

                index = m_count;
                m_count++;
            }

            m_entries[index].hashCode = hashCode;
            m_entries[index].next     = m_buckets[targetBucket];
            m_entries[index].key      = key;
            m_entries[index].value    = value;

            m_buckets[targetBucket] = index;
            m_version++;
        }

////    public virtual void OnDeserialization( Object sender )
////    {
////        if(m_siInfo == null)
////        {
////            // It might be necessary to call OnDeserialization from a container if the container object also implements
////            // OnDeserialization. However, remoting will call OnDeserialization again.
////            // We can return immediately if this function is called twice.
////            // Note we set m_siInfo to null at the end of this method.
////            return;
////        }
////
////        int realVersion = m_siInfo.GetInt32( VersionName  );
////        int hashsize    = m_siInfo.GetInt32( HashSizeName );
////
////        m_comparer = (IEqualityComparer<TKey>)m_siInfo.GetValue( ComparerName, typeof( IEqualityComparer<TKey> ) );
////
////        if(hashsize != 0)
////        {
////            m_buckets = new int[hashsize];
////            for(int i = 0; i < m_buckets.Length; i++)
////            {
////                m_buckets[i] = -1;
////            }
////
////            m_entries  = new Entry[hashsize];
////            m_freeList = -1;
////
////            KeyValuePair<TKey, TValue>[] array = (KeyValuePair<TKey, TValue>[])m_siInfo.GetValue( KeyValuePairsName, typeof( KeyValuePair<TKey, TValue>[] ) );
////
////            if(array == null)
////            {
////                ThrowHelper.ThrowSerializationException( ExceptionResource.Serialization_MissingKeys );
////            }
////
////            for(int i = 0; i < array.Length; i++)
////            {
////                if(array[i].Key == null)
////                {
////                    ThrowHelper.ThrowSerializationException( ExceptionResource.Serialization_NullKey );
////                }
////
////                Insert( array[i].Key, array[i].Value, true );
////            }
////        }
////        else
////        {
////            m_buckets = null;
////        }
////
////        m_version = realVersion;
////        m_siInfo  = null;
////    }

        private void Resize()
        {
            int   newSize    = HashHelpers.GetPrime( m_count * 2 );
            int[] newBuckets = new int[newSize];

            for(int i = 0; i < newBuckets.Length; i++)
            {
                newBuckets[i] = -1;
            }

            Entry[] newEntries = new Entry[newSize];

            Array.Copy( m_entries, 0, newEntries, 0, m_count );

            for(int i = 0; i < m_count; i++)
            {
                int bucket = newEntries[i].hashCode % newSize;

                newEntries[i].next = newBuckets[bucket];

                newBuckets[bucket] = i;
            }

            m_buckets = newBuckets;
            m_entries = newEntries;
        }

        public bool Remove( TKey key )
        {
            if(key == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.key );
            }

            if(m_buckets != null)
            {
                int hashCode = m_comparer.GetHashCode( key ) & 0x7FFFFFFF;
                int bucket   = hashCode % m_buckets.Length;
                int last     = -1;

                for(int i = m_buckets[bucket]; i >= 0; last = i, i = m_entries[i].next)
                {
                    if(m_entries[i].hashCode == hashCode && m_comparer.Equals( m_entries[i].key, key ))
                    {
                        if(last < 0)
                        {
                            m_buckets[bucket] = m_entries[i].next;
                        }
                        else
                        {
                            m_entries[last].next = m_entries[i].next;
                        }

                        m_entries[i].hashCode = -1;
                        m_entries[i].next     = m_freeList;
                        m_entries[i].key      = default( TKey );
                        m_entries[i].value    = default( TValue );

                        m_freeList = i;
                        m_freeCount++;
                        m_version++;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TryGetValue( TKey key, out TValue value )
        {
            int i = FindEntry( key );
            if(i >= 0)
            {
                value = m_entries[i].value;
                return true;
            }

            value = default( TValue );
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo( KeyValuePair<TKey, TValue>[] array, int index )
        {
            CopyTo( array, index );
        }

        void ICollection.CopyTo( Array array, int index )
        {
            if(array == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.array );
            }

            if(array.Rank != 1)
            {
                ThrowHelper.ThrowArgumentException( ExceptionResource.Arg_RankMultiDimNotSupported );
            }

            if(array.GetLowerBound( 0 ) != 0)
            {
                ThrowHelper.ThrowArgumentException( ExceptionResource.Arg_NonZeroLowerBound );
            }

            if(index < 0 || index > array.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum );
            }

            if(array.Length - index < Count)
            {
                ThrowHelper.ThrowArgumentException( ExceptionResource.Arg_ArrayPlusOffTooSmall );
            }

            KeyValuePair<TKey, TValue>[] pairs = array as KeyValuePair<TKey, TValue>[];
            if(pairs != null)
            {
                CopyTo( pairs, index );
            }
            else if(array is DictionaryEntry[])
            {
                DictionaryEntry[] dictEntryArray = array as DictionaryEntry[];
                Entry[]           entries        = m_entries;

                for(int i = 0; i < m_count; i++)
                {
                    if(entries[i].hashCode >= 0)
                    {
                        dictEntryArray[index++] = new DictionaryEntry( entries[i].key, entries[i].value );
                    }
                }
            }
            else
            {
                object[] objects = array as object[];
                if(objects == null)
                {
                    ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidArrayType );
                }

                try
                {
                    int     count   = m_count;
                    Entry[] entries = m_entries;

                    for(int i = 0; i < count; i++)
                    {
                        if(entries[i].hashCode >= 0)
                        {
                            objects[index++] = new KeyValuePair<TKey, TValue>( entries[i].key, entries[i].value );
                        }
                    }
                }
                catch(ArrayTypeMismatchException)
                {
                    ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidArrayType );
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator( this, Enumerator.KeyValuePair );
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if(m_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange( ref m_syncRoot, new Object(), null );
                }

                return m_syncRoot;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return (ICollection)Keys;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return (ICollection)Values;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                if(IsCompatibleKey( key ))
                {
                    int i = FindEntry( (TKey)key );
                    if(i >= 0)
                    {
                        return m_entries[i].value;
                    }
                }

                return null;
            }

            set
            {
                if(key == null)
                {
                    ThrowHelper.ThrowArgumentNullException( ExceptionArgument.key );
                }

                ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>( value, ExceptionArgument.value );

                try
                {
                    TKey tempKey = (TKey)key;
                    try
                    {
                        this[tempKey] = (TValue)value;
                    }
                    catch(InvalidCastException)
                    {
                        ThrowHelper.ThrowWrongValueTypeArgumentException( value, typeof( TValue ) );
                    }
                }
                catch(InvalidCastException)
                {
                    ThrowHelper.ThrowWrongKeyTypeArgumentException( key, typeof( TKey ) );
                }
            }
        }

        private static bool IsCompatibleKey( object key )
        {
            if(key == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.key );
            }

            return (key is TKey);
        }

        void IDictionary.Add( object key, object value )
        {
            if(key == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.key );
            }

            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>( value, ExceptionArgument.value );

            try
            {
                TKey tempKey = (TKey)key;

                try
                {
                    Add( tempKey, (TValue)value );
                }
                catch(InvalidCastException)
                {
                    ThrowHelper.ThrowWrongValueTypeArgumentException( value, typeof( TValue ) );
                }
            }
            catch(InvalidCastException)
            {
                ThrowHelper.ThrowWrongKeyTypeArgumentException( key, typeof( TKey ) );
            }
        }

        bool IDictionary.Contains( object key )
        {
            if(IsCompatibleKey( key ))
            {
                return ContainsKey( (TKey)key );
            }

            return false;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new Enumerator( this, Enumerator.DictEntry );
        }

        void IDictionary.Remove( object key )
        {
            if(IsCompatibleKey( key ))
            {
                Remove( (TKey)key );
            }
        }

        [Serializable]
        public struct Enumerator : IEnumerator< KeyValuePair<TKey, TValue> >, IDictionaryEnumerator
        {
            internal const int DictEntry    = 1;
            internal const int KeyValuePair = 2;

            private Dictionary<TKey, TValue>   m_dictionary;
            private int                        m_version;
            private int                        m_index;
            private KeyValuePair<TKey, TValue> m_current;
            private int                        m_getEnumeratorRetType;  // What should Enumerator.Current return?

            internal Enumerator( Dictionary<TKey, TValue> dictionary, int getEnumeratorRetType )
            {
                m_dictionary           = dictionary;
                m_version              = dictionary.m_version;
                m_index                = 0;
                m_getEnumeratorRetType = getEnumeratorRetType;
                m_current              = new KeyValuePair<TKey, TValue>();
            }

            public bool MoveNext()
            {
                if(m_version != m_dictionary.m_version)
                {
                    ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_EnumFailedVersion );
                }

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
                while((uint)m_index < (uint)m_dictionary.m_count)
                {
                    if(m_dictionary.m_entries[m_index].hashCode >= 0)
                    {
                        m_current = new KeyValuePair<TKey, TValue>( m_dictionary.m_entries[m_index].key, m_dictionary.m_entries[m_index].value );
                        m_index++;
                        return true;
                    }

                    m_index++;
                }

                m_index   = m_dictionary.m_count + 1;
                m_current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    return m_current;
                }
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get
                {
                    if(m_index == 0 || (m_index == m_dictionary.m_count + 1))
                    {
                        ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_EnumOpCantHappen );
                    }

                    if(m_getEnumeratorRetType == DictEntry)
                    {
                        return new System.Collections.DictionaryEntry( m_current.Key, m_current.Value );
                    }
                    else
                    {
                        return new KeyValuePair<TKey, TValue>( m_current.Key, m_current.Value );
                    }
                }
            }

            void IEnumerator.Reset()
            {
                if(m_version != m_dictionary.m_version)
                {
                    ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_EnumFailedVersion );
                }

                m_index   = 0;
                m_current = new KeyValuePair<TKey, TValue>();
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if(m_index == 0 || (m_index == m_dictionary.m_count + 1))
                    {
                        ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_EnumOpCantHappen );
                    }

                    return new DictionaryEntry( m_current.Key, m_current.Value );
                }
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    if(m_index == 0 || (m_index == m_dictionary.m_count + 1))
                    {
                        ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_EnumOpCantHappen );
                    }

                    return m_current.Key;
                }
            }

            object IDictionaryEnumerator.Value
            {
                get
                {
                    if(m_index == 0 || (m_index == m_dictionary.m_count + 1))
                    {
                        ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_EnumOpCantHappen );
                    }

                    return m_current.Value;
                }
            }
        }

////    [DebuggerTypeProxy( typeof( Mscorlib_DictionaryKeyCollectionDebugView<,> ) )]
////    [DebuggerDisplay( "Count = {Count}" )]
        [Serializable]
        public sealed class KeyCollection : ICollection<TKey>, ICollection
        {
            private Dictionary<TKey, TValue> m_dictionary;

            public KeyCollection( Dictionary<TKey, TValue> dictionary )
            {
                if(dictionary == null)
                {
                    ThrowHelper.ThrowArgumentNullException( ExceptionArgument.dictionary );
                }

                m_dictionary = dictionary;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator( m_dictionary );
            }

            public void CopyTo( TKey[] array, int index )
            {
                if(array == null)
                {
                    ThrowHelper.ThrowArgumentNullException( ExceptionArgument.array );
                }

                if(index < 0 || index > array.Length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum );
                }

                if(array.Length - index < m_dictionary.Count)
                {
                    ThrowHelper.ThrowArgumentException( ExceptionResource.Arg_ArrayPlusOffTooSmall );
                }

                int     count   = m_dictionary.m_count;
                Entry[] entries = m_dictionary.m_entries;

                for(int i = 0; i < count; i++)
                {
                    if(entries[i].hashCode >= 0) array[index++] = entries[i].key;
                }
            }

            public int Count
            {
                get
                {
                    return m_dictionary.Count;
                }
            }

            bool ICollection<TKey>.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            void ICollection<TKey>.Add( TKey item )
            {
                ThrowHelper.ThrowNotSupportedException( ExceptionResource.NotSupported_KeyCollectionSet );
            }

            void ICollection<TKey>.Clear()
            {
                ThrowHelper.ThrowNotSupportedException( ExceptionResource.NotSupported_KeyCollectionSet );
            }

            bool ICollection<TKey>.Contains( TKey item )
            {
                return m_dictionary.ContainsKey( item );
            }

            bool ICollection<TKey>.Remove( TKey item )
            {
                ThrowHelper.ThrowNotSupportedException( ExceptionResource.NotSupported_KeyCollectionSet );
                return false;
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return new Enumerator( m_dictionary );
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator( m_dictionary );
            }

            void ICollection.CopyTo( Array array, int index )
            {
                if(array == null)
                {
                    ThrowHelper.ThrowArgumentNullException( ExceptionArgument.array );
                }

                if(array.Rank != 1)
                {
                    ThrowHelper.ThrowArgumentException( ExceptionResource.Arg_RankMultiDimNotSupported );
                }

                if(array.GetLowerBound( 0 ) != 0)
                {
                    ThrowHelper.ThrowArgumentException( ExceptionResource.Arg_NonZeroLowerBound );
                }

                if(index < 0 || index > array.Length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum );
                }

                if(array.Length - index < m_dictionary.Count)
                {
                    ThrowHelper.ThrowArgumentException( ExceptionResource.Arg_ArrayPlusOffTooSmall );
                }

                TKey[] keys = array as TKey[];
                if(keys != null)
                {
                    CopyTo( keys, index );
                }
                else
                {
                    object[] objects = array as object[];
                    if(objects == null)
                    {
                        ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidArrayType );
                    }

                    int     count   = m_dictionary.m_count;
                    Entry[] entries = m_dictionary.m_entries;

                    try
                    {
                        for(int i = 0; i < count; i++)
                        {
                            if(entries[i].hashCode >= 0) objects[index++] = entries[i].key;
                        }
                    }
                    catch(ArrayTypeMismatchException)
                    {
                        ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidArrayType );
                    }
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            Object ICollection.SyncRoot
            {
                get
                {
                    return ((ICollection)m_dictionary).SyncRoot;
                }
            }

            [Serializable]
            public struct Enumerator : IEnumerator<TKey>, IEnumerator
            {
                private Dictionary<TKey, TValue> m_dictionary;
                private int                      m_index;
                private int                      m_version;
                private TKey                     m_currentKey;

                internal Enumerator( Dictionary<TKey, TValue> dictionary )
                {
                    m_dictionary = dictionary;
                    m_version    = dictionary.m_version;
                    m_index      = 0;
                    m_currentKey = default( TKey );
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    if(m_version != m_dictionary.m_version)
                    {
                        ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_EnumFailedVersion );
                    }

                    while((uint)m_index < (uint)m_dictionary.m_count)
                    {
                        if(m_dictionary.m_entries[m_index].hashCode >= 0)
                        {
                            m_currentKey = m_dictionary.m_entries[m_index].key;
                            m_index++;
                            return true;
                        }

                        m_index++;
                    }

                    m_index      = m_dictionary.m_count + 1;
                    m_currentKey = default( TKey );

                    return false;
                }

                public TKey Current
                {
                    get
                    {
                        return m_currentKey;
                    }
                }

                Object System.Collections.IEnumerator.Current
                {
                    get
                    {
                        if(m_index == 0 || (m_index == m_dictionary.m_count + 1))
                        {
                            ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_EnumOpCantHappen );
                        }

                        return m_currentKey;
                    }
                }

                void System.Collections.IEnumerator.Reset()
                {
                    if(m_version != m_dictionary.m_version)
                    {
                        ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_EnumFailedVersion );
                    }

                    m_index      = 0;
                    m_currentKey = default( TKey );
                }
            }
        }

////    [DebuggerTypeProxy( typeof( Mscorlib_DictionaryValueCollectionDebugView<,> ) )]
////    [DebuggerDisplay( "Count = {Count}" )]
        [Serializable]
        public sealed class ValueCollection : ICollection<TValue>, ICollection
        {
            private Dictionary<TKey, TValue> m_dictionary;

            public ValueCollection( Dictionary<TKey, TValue> dictionary )
            {
                if(dictionary == null)
                {
                    ThrowHelper.ThrowArgumentNullException( ExceptionArgument.dictionary );
                }

                m_dictionary = dictionary;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator( m_dictionary );
            }

            public void CopyTo( TValue[] array, int index )
            {
                if(array == null)
                {
                    ThrowHelper.ThrowArgumentNullException( ExceptionArgument.array );
                }

                if(index < 0 || index > array.Length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum );
                }

                if(array.Length - index < m_dictionary.Count)
                {
                    ThrowHelper.ThrowArgumentException( ExceptionResource.Arg_ArrayPlusOffTooSmall );
                }

                int     count   = m_dictionary.m_count;
                Entry[] entries = m_dictionary.m_entries;

                for(int i = 0; i < count; i++)
                {
                    if(entries[i].hashCode >= 0) array[index++] = entries[i].value;
                }
            }

            public int Count
            {
                get
                {
                    return m_dictionary.Count;
                }
            }

            bool ICollection<TValue>.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            void ICollection<TValue>.Add( TValue item )
            {
                ThrowHelper.ThrowNotSupportedException( ExceptionResource.NotSupported_ValueCollectionSet );
            }

            bool ICollection<TValue>.Remove( TValue item )
            {
                ThrowHelper.ThrowNotSupportedException( ExceptionResource.NotSupported_ValueCollectionSet );
                return false;
            }

            void ICollection<TValue>.Clear()
            {
                ThrowHelper.ThrowNotSupportedException( ExceptionResource.NotSupported_ValueCollectionSet );
            }

            bool ICollection<TValue>.Contains( TValue item )
            {
                return m_dictionary.ContainsValue( item );
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return new Enumerator( m_dictionary );
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator( m_dictionary );
            }

            void ICollection.CopyTo( Array array, int index )
            {
                if(array == null)
                {
                    ThrowHelper.ThrowArgumentNullException( ExceptionArgument.array );
                }

                if(array.Rank != 1)
                {
                    ThrowHelper.ThrowArgumentException( ExceptionResource.Arg_RankMultiDimNotSupported );
                }

                if(array.GetLowerBound( 0 ) != 0)
                {
                    ThrowHelper.ThrowArgumentException( ExceptionResource.Arg_NonZeroLowerBound );
                }

                if(index < 0 || index > array.Length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum );
                }

                if(array.Length - index < m_dictionary.Count)
                {
                    ThrowHelper.ThrowArgumentException( ExceptionResource.Arg_ArrayPlusOffTooSmall );
                }

                TValue[] values = array as TValue[];
                if(values != null)
                {
                    CopyTo( values, index );
                }
                else
                {
                    object[] objects = array as object[];
                    if(objects == null)
                    {
                        ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidArrayType );
                    }

                    int     count   = m_dictionary.m_count;
                    Entry[] entries = m_dictionary.m_entries;
                    try
                    {
                        for(int i = 0; i < count; i++)
                        {
                            if(entries[i].hashCode >= 0) objects[index++] = entries[i].value;
                        }
                    }
                    catch(ArrayTypeMismatchException)
                    {
                        ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidArrayType );
                    }
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            Object ICollection.SyncRoot
            {
                get
                {
                    return ((ICollection)m_dictionary).SyncRoot;
                }
            }

            [Serializable]
            public struct Enumerator : IEnumerator<TValue>, IEnumerator
            {
                private Dictionary<TKey, TValue> m_dictionary;
                private int                      m_index;
                private int                      m_version;
                private TValue                   m_currentValue;

                internal Enumerator( Dictionary<TKey, TValue> dictionary )
                {
                    m_dictionary   = dictionary;
                    m_version      = dictionary.m_version;
                    m_index        = 0;
                    m_currentValue = default( TValue );
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    if(m_version != m_dictionary.m_version)
                    {
                        ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_EnumFailedVersion );
                    }

                    while((uint)m_index < (uint)m_dictionary.m_count)
                    {
                        if(m_dictionary.m_entries[m_index].hashCode >= 0)
                        {
                            m_currentValue = m_dictionary.m_entries[m_index].value;
                            m_index++;
                            return true;
                        }

                        m_index++;
                    }

                    m_index        = m_dictionary.m_count + 1;
                    m_currentValue = default( TValue );
                    return false;
                }

                public TValue Current
                {
                    get
                    {
                        return m_currentValue;
                    }
                }

                Object System.Collections.IEnumerator.Current
                {
                    get
                    {
                        if(m_index == 0 || (m_index == m_dictionary.m_count + 1))
                        {
                            ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_EnumOpCantHappen );
                        }

                        return m_currentValue;
                    }
                }

                void System.Collections.IEnumerator.Reset()
                {
                    if(m_version != m_dictionary.m_version)
                    {
                        ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_EnumFailedVersion );
                    }

                    m_index        = 0;
                    m_currentValue = default( TValue );
                }
            }
        }
    }
}
