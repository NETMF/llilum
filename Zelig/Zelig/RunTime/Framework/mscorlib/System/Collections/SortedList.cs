// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  SortedList
**
** Purpose: A sorted dictionary.
**
**
===========================================================*/
namespace System.Collections
{
    using System;
////using System.Security.Permissions;
    using System.Diagnostics;
    using System.Globalization;

    // The SortedList class implements a sorted list of keys and values. Entries in
    // a sorted list are sorted by their keys and are accessible both by key and by
    // index. The keys of a sorted list can be ordered either according to a
    // specific IComparer implementation given when the sorted list is
    // instantiated, or according to the IComparable implementation provided
    // by the keys themselves. In either case, a sorted list does not allow entries
    // with duplicate keys.
    //
    // A sorted list internally maintains two arrays that store the keys and
    // values of the entries. The capacity of a sorted list is the allocated
    // length of these internal arrays. As elements are added to a sorted list, the
    // capacity of the sorted list is automatically increased as required by
    // reallocating the internal arrays.  The capacity is never automatically
    // decreased, but users can call either TrimToSize or
    // Capacity explicitly.
    //
    // The GetKeyList and GetValueList methods of a sorted list
    // provides access to the keys and values of the sorted list in the form of
    // List implementations. The List objects returned by these
    // methods are aliases for the underlying sorted list, so modifications
    // made to those lists are directly reflected in the sorted list, and vice
    // versa.
    //
    // The SortedList class provides a convenient way to create a sorted
    // copy of another dictionary, such as a Hashtable. For example:
    //
    // Hashtable h = new Hashtable();
    // h.Add(...);
    // h.Add(...);
    // ...
    // SortedList s = new SortedList(h);
    //
    // The last line above creates a sorted list that contains a copy of the keys
    // and values stored in the hashtable. In this particular example, the keys
    // will be ordered according to the IComparable interface, which they
    // all must implement. To impose a different ordering, SortedList also
    // has a constructor that allows a specific IComparer implementation to
    // be specified.
    //
////[DebuggerTypeProxy( typeof( System.Collections.SortedList.SortedListDebugView ) )]
////[DebuggerDisplay( "Count = {Count}" )]
    [Serializable]
    public class SortedList : IDictionary, ICloneable
    {
        private const int       cDefaultCapacity = 16;

        private static Object[] emptyArray = new Object[0];

        private Object[]        m_keys;
        private Object[]        m_values;
        private int             m_size;
        private int             m_version;
        private IComparer       m_comparer;
        private KeyList         m_keyList;
        private ValueList       m_valueList;
        [NonSerialized]
        private Object          m_syncRoot;

        // Constructs a new sorted list. The sorted list is initially empty and has
        // a capacity of zero. Upon adding the first element to the sorted list the
        // capacity is increased to 16, and then increased in multiples of two as
        // required. The elements of the sorted list are ordered according to the
        // IComparable interface, which must be implemented by the keys of
        // all entries added to the sorted list.
        public SortedList()
        {
            m_keys     = emptyArray;
            m_values   = emptyArray;
            m_size     = 0;
            m_comparer = new Comparer( CultureInfo.CurrentCulture );
        }

        // Constructs a new sorted list. The sorted list is initially empty and has
        // a capacity of zero. Upon adding the first element to the sorted list the
        // capacity is increased to 16, and then increased in multiples of two as
        // required. The elements of the sorted list are ordered according to the
        // IComparable interface, which must be implemented by the keys of
        // all entries added to the sorted list.
        //
        public SortedList( int initialCapacity )
        {
            if(initialCapacity < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "initialCapacity", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            m_keys     = new Object[initialCapacity];
            m_values   = new Object[initialCapacity];
            m_comparer = new Comparer( CultureInfo.CurrentCulture );
        }

        // Constructs a new sorted list with a given IComparer
        // implementation. The sorted list is initially empty and has a capacity of
        // zero. Upon adding the first element to the sorted list the capacity is
        // increased to 16, and then increased in multiples of two as required. The
        // elements of the sorted list are ordered according to the given
        // IComparer implementation. If comparer is null, the
        // elements are compared to each other using the IComparable
        // interface, which in that case must be implemented by the keys of all
        // entries added to the sorted list.
        //
        public SortedList( IComparer comparer ) : this()
        {
            if(comparer != null)
            {
                m_comparer = comparer;
            }
        }

        // Constructs a new sorted list with a given IComparer
        // implementation and a given initial capacity. The sorted list is
        // initially empty, but will have room for the given number of elements
        // before any reallocations are required. The elements of the sorted list
        // are ordered according to the given IComparer implementation. If
        // comparer is null, the elements are compared to each other using
        // the IComparable interface, which in that case must be implemented
        // by the keys of all entries added to the sorted list.
        //
        public SortedList( IComparer comparer, int capacity ) : this( comparer )
        {
            this.Capacity = capacity;
        }

        // Constructs a new sorted list containing a copy of the entries in the
        // given dictionary. The elements of the sorted list are ordered according
        // to the IComparable interface, which must be implemented by the
        // keys of all entries in the the given dictionary as well as keys
        // subsequently added to the sorted list.
        //
        public SortedList( IDictionary d ) : this( d, null )
        {
        }

        // Constructs a new sorted list containing a copy of the entries in the
        // given dictionary. The elements of the sorted list are ordered according
        // to the given IComparer implementation. If comparer is
        // null, the elements are compared to each other using the
        // IComparable interface, which in that case must be implemented
        // by the keys of all entries in the the given dictionary as well as keys
        // subsequently added to the sorted list.
        //
        public SortedList( IDictionary d, IComparer comparer ) : this( comparer, (d != null ? d.Count : 0) )
        {
            if(d == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "d", Environment.GetResourceString( "ArgumentNull_Dictionary" ) );
#else
                throw new ArgumentNullException();
#endif
            }

            d.Keys  .CopyTo( m_keys  , 0 );
            d.Values.CopyTo( m_values, 0 );

            Array.Sort( m_keys, m_values, comparer );

            m_size = d.Count;
        }

        // Adds an entry with the given key and value to this sorted list. An
        // ArgumentException is thrown if the key is already present in the sorted list.
        //
        public virtual void Add( Object key, Object value )
        {
            if(key == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "key", Environment.GetResourceString( "ArgumentNull_Key" ) );
#else
                throw new ArgumentNullException();
#endif
            }

            int i = Array.BinarySearch( m_keys, 0, m_size, key, m_comparer );
            if(i >= 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_AddingDuplicate__", GetKey( i ), key ) );
#else
                throw new ArgumentException();
#endif
            }

            Insert( ~i, key, value );
        }

        // Returns the capacity of this sorted list. The capacity of a sorted list
        // represents the allocated length of the internal arrays used to store the
        // keys and values of the list, and thus also indicates the maximum number
        // of entries the list can contain before a reallocation of the internal
        // arrays is required.
        //
        public virtual int Capacity
        {
            get
            {
                return m_keys.Length;
            }

            set
            {
                if(value != m_keys.Length)
                {
                    if(value < m_size)
                    {
#if EXCEPTION_STRINGS
                        throw new ArgumentOutOfRangeException( "value", Environment.GetResourceString( "ArgumentOutOfRange_SmallCapacity" ) );
#else
                        throw new ArgumentOutOfRangeException();
#endif
                    }

                    if(value > 0)
                    {
                        Object[] newKeys   = new Object[value];
                        Object[] newValues = new Object[value];

                        if(m_size > 0)
                        {
                            Array.Copy( m_keys  , 0, newKeys  , 0, m_size );
                            Array.Copy( m_values, 0, newValues, 0, m_size );
                        }

                        m_keys   = newKeys;
                        m_values = newValues;
                    }
                    else
                    {
                        // size can only be zero here.
                        BCLDebug.Assert( m_size == 0, "Size is not zero" );

                        m_keys   = emptyArray;
                        m_values = emptyArray;
                    }
                }
            }
        }

        // Returns the number of entries in this sorted list.
        //
        public virtual int Count
        {
            get
            {
                return m_size;
            }
        }

        // Returns a collection representing the keys of this sorted list. This
        // method returns the same object as GetKeyList, but typed as an
        // ICollection instead of an IList.
        //
        public virtual ICollection Keys
        {
            get
            {
                return GetKeyList();
            }
        }

        // Returns a collection representing the values of this sorted list. This
        // method returns the same object as GetValueList, but typed as an
        // ICollection instead of an IList.
        //
        public virtual ICollection Values
        {
            get
            {
                return GetValueList();
            }
        }

        // Is this SortedList read-only?
        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        // Is this SortedList synchronized (thread-safe)?
        public virtual bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        // Synchronization root for this object.
        public virtual Object SyncRoot
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

        // Removes all entries from this sorted list.
        public virtual void Clear()
        {
            // clear does not change the capacity
            m_version++;

            Array.Clear( m_keys  , 0, m_size ); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
            Array.Clear( m_values, 0, m_size ); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
            m_size = 0;
        }

        // Makes a virtually identical copy of this SortedList.  This is a shallow
        // copy.  IE, the Objects in the SortedList are not cloned - we copy the
        // references to those objects.
        public virtual Object Clone()
        {
            SortedList sl = new SortedList( m_size );

            Array.Copy( m_keys  , 0, sl.m_keys  , 0, m_size );
            Array.Copy( m_values, 0, sl.m_values, 0, m_size );

            sl.m_size     = m_size;
            sl.m_version  = m_version;
            sl.m_comparer = m_comparer;

            // Don't copy keyList nor valueList.
            return sl;
        }


        // Checks if this sorted list contains an entry with the given key.
        //
        public virtual bool Contains( Object key )
        {
            return IndexOfKey( key ) >= 0;
        }

        // Checks if this sorted list contains an entry with the given key.
        //
        public virtual bool ContainsKey( Object key )
        {
            // Yes, this is a SPEC'ed duplicate of Contains().
            return IndexOfKey( key ) >= 0;
        }

        // Checks if this sorted list contains an entry with the given value. The
        // values of the entries of the sorted list are compared to the given value
        // using the Object.Equals method. This method performs a linear
        // search and is substantially slower than the Contains
        // method.
        //
        public virtual bool ContainsValue( Object value )
        {
            return IndexOfValue( value ) >= 0;
        }

        // Copies the values in this SortedList to an array.
        public virtual void CopyTo( Array array, int arrayIndex )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array", Environment.GetResourceString( "ArgumentNull_Array" ) );
#else
                throw new ArgumentNullException();
#endif
            }

            if(array.Rank != 1)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_RankMultiDimNotSupported" ) );
#else
                throw new ArgumentException();
#endif
            }

            if(arrayIndex < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "arrayIndex", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(array.Length - arrayIndex < Count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_ArrayPlusOffTooSmall" ) );
#else
                throw new ArgumentException();
#endif
            }

            for(int i = 0; i < Count; i++)
            {
                DictionaryEntry entry = new DictionaryEntry( m_keys[i], m_values[i] );

                array.SetValue( entry, i + arrayIndex );
            }
        }

        // Copies the values in this SortedList to an KeyValuePairs array.
        // KeyValuePairs is different from Dictionary Entry in that it has special
        // debugger attributes on its fields.

        internal virtual KeyValuePairs[] ToKeyValuePairsArray()
        {
            KeyValuePairs[] array = new KeyValuePairs[Count];

            for(int i = 0; i < Count; i++)
            {
                array[i] = new KeyValuePairs( m_keys[i], m_values[i] );
            }

            return array;
        }

        // Ensures that the capacity of this sorted list is at least the given
        // minimum value. If the currect capacity of the list is less than
        // min, the capacity is increased to twice the current capacity or
        // to min, whichever is larger.
        private void EnsureCapacity( int min )
        {
            int newCapacity = m_keys.Length == 0 ? cDefaultCapacity : m_keys.Length * 2;

            if(newCapacity < min)
            {
                newCapacity = min;
            }

            this.Capacity = newCapacity;
        }

        // Returns the value of the entry at the given index.
        //
        public virtual Object GetByIndex( int index )
        {
            if(index < 0 || index >= m_size)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            return m_values[index];
        }

        // Returns an IEnumerator for this sorted list.  If modifications
        // made to the sorted list while an enumeration is in progress,
        // the MoveNext and Remove methods
        // of the enumerator will throw an exception.
        //
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SortedListEnumerator( this, 0, m_size, SortedListEnumerator.DictEntry );
        }

        // Returns an IDictionaryEnumerator for this sorted list.  If modifications
        // made to the sorted list while an enumeration is in progress,
        // the MoveNext and Remove methods
        // of the enumerator will throw an exception.
        //
        public virtual IDictionaryEnumerator GetEnumerator()
        {
            return new SortedListEnumerator( this, 0, m_size, SortedListEnumerator.DictEntry );
        }

        // Returns the key of the entry at the given index.
        //
        public virtual Object GetKey( int index )
        {
            if(index < 0 || index >= m_size)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            return m_keys[index];
        }

        // Returns an IList representing the keys of this sorted list. The
        // returned list is an alias for the keys of this sorted list, so
        // modifications made to the returned list are directly reflected in the
        // underlying sorted list, and vice versa. The elements of the returned
        // list are ordered in the same way as the elements of the sorted list. The
        // returned list does not support adding, inserting, or modifying elements
        // (the Add, AddRange, Insert, InsertRange,
        // Reverse, Set, SetRange, and Sort methods
        // throw exceptions), but it does allow removal of elements (through the
        // Remove and RemoveRange methods or through an enumerator).
        // Null is an invalid key value.
        //
        public virtual IList GetKeyList()
        {
            if(m_keyList == null)
            {
                m_keyList = new KeyList( this );
            }

            return m_keyList;
        }

        // Returns an IList representing the values of this sorted list. The
        // returned list is an alias for the values of this sorted list, so
        // modifications made to the returned list are directly reflected in the
        // underlying sorted list, and vice versa. The elements of the returned
        // list are ordered in the same way as the elements of the sorted list. The
        // returned list does not support adding or inserting elements (the
        // Add, AddRange, Insert and InsertRange
        // methods throw exceptions), but it does allow modification and removal of
        // elements (through the Remove, RemoveRange, Set and
        // SetRange methods or through an enumerator).
        //
        public virtual IList GetValueList()
        {
            if(m_valueList == null)
            {
                m_valueList = new ValueList( this );
            }

            return m_valueList;
        }

        // Returns the value associated with the given key. If an entry with the
        // given key is not found, the returned value is null.
        //
        public virtual Object this[Object key]
        {
            get
            {
                int i = IndexOfKey( key );
                if(i >= 0)
                {
                    return m_values[i];
                }

                return null;
            }
            set
            {
                if(key == null)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentNullException( "key", Environment.GetResourceString( "ArgumentNull_Key" ) );
#else
                    throw new ArgumentNullException();
#endif
                }

                int i = Array.BinarySearch( m_keys, 0, m_size, key, m_comparer );
                if(i >= 0)
                {
                    m_values[i] = value;
                    m_version++;
                    return;
                }

                Insert( ~i, key, value );
            }
        }

        // Returns the index of the entry with a given key in this sorted list. The
        // key is located through a binary search, and thus the average execution
        // time of this method is proportional to Log2(size), where
        // size is the size of this sorted list. The returned value is -1 if
        // the given key does not occur in this sorted list. Null is an invalid
        // key value.
        //
        public virtual int IndexOfKey( Object key )
        {
            if(key == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "key", Environment.GetResourceString( "ArgumentNull_Key" ) );
#else
                throw new ArgumentNullException();
#endif
            }

            int ret = Array.BinarySearch( m_keys, 0, m_size, key, m_comparer );

            return ret >= 0 ? ret : -1;
        }

        // Returns the index of the first occurrence of an entry with a given value
        // in this sorted list. The entry is located through a linear search, and
        // thus the average execution time of this method is proportional to the
        // size of this sorted list. The elements of the list are compared to the
        // given value using the Object.Equals method.
        //
        public virtual int IndexOfValue( Object value )
        {
            return Array.IndexOf( m_values, value, 0, m_size );
        }

        // Inserts an entry with a given key and value at a given index.
        private void Insert( int index, Object key, Object value )
        {
            if(m_size == m_keys.Length)
            {
                EnsureCapacity( m_size + 1 );
            }

            if(index < m_size)
            {
                Array.Copy( m_keys  , index, m_keys  , index + 1, m_size - index );
                Array.Copy( m_values, index, m_values, index + 1, m_size - index );
            }

            m_keys  [index] = key;
            m_values[index] = value;
            m_size++;
            m_version++;
        }

        // Removes the entry at the given index. The size of the sorted list is
        // decreased by one.
        //
        public virtual void RemoveAt( int index )
        {
            if(index < 0 || index >= m_size)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            m_size--;
            if(index < m_size)
            {
                Array.Copy( m_keys  , index + 1, m_keys  , index, m_size - index );
                Array.Copy( m_values, index + 1, m_values, index, m_size - index );
            }

            m_keys  [m_size] = null;
            m_values[m_size] = null;
            m_version++;
        }

        // Removes an entry from this sorted list. If an entry with the specified
        // key exists in the sorted list, it is removed. An ArgumentException is
        // thrown if the key is null.
        //
        public virtual void Remove( Object key )
        {
            int i = IndexOfKey( key );
            if(i >= 0)
            {
                RemoveAt( i );
            }
        }

        // Sets the value at an index to a given value.  The previous value of
        // the given entry is overwritten.
        //
        public virtual void SetByIndex( int index, Object value )
        {
            if(index < 0 || index >= m_size)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            m_values[index] = value;
            m_version++;
        }

////    // Returns a thread-safe SortedList.
////    //
////    [HostProtection( Synchronization = true )]
////    public static SortedList Synchronized( SortedList list )
////    {
////        if(list == null)
////        {
////            throw new ArgumentNullException( "list" );
////        }
////
////        return new SyncSortedList( list );
////    }

        // Sets the capacity of this sorted list to the size of the sorted list.
        // This method can be used to minimize a sorted list's memory overhead once
        // it is known that no new elements will be added to the sorted list. To
        // completely clear a sorted list and release all memory referenced by the
        // sorted list, execute the following statements:
        //
        // sortedList.Clear();
        // sortedList.TrimToSize();
        //
        public virtual void TrimToSize()
        {
            Capacity = m_size;
        }

////    [Serializable]
////    private class SyncSortedList : SortedList
////    {
////        private SortedList m_list;
////        private Object     m_root;
////
////        internal SyncSortedList( SortedList list )
////        {
////            m_list = list;
////            m_root = list.SyncRoot;
////        }
////
////        public override int Count
////        {
////            get
////            {
////                lock(m_root)
////                {
////                    return m_list.Count;
////                }
////            }
////        }
////
////        public override Object SyncRoot
////        {
////            get
////            {
////                return m_root;
////            }
////        }
////
////        public override bool IsReadOnly
////        {
////            get
////            {
////                return m_list.IsReadOnly;
////            }
////        }
////
////        public override bool IsFixedSize
////        {
////            get
////            {
////                return m_list.IsFixedSize;
////            }
////        }
////
////
////        public override bool IsSynchronized
////        {
////            get
////            {
////                return true;
////            }
////        }
////
////        public override Object this[Object key]
////        {
////            get
////            {
////                lock(m_root)
////                {
////                    return m_list[key];
////                }
////            }
////
////            set
////            {
////                lock(m_root)
////                {
////                    m_list[key] = value;
////                }
////            }
////        }
////
////        public override void Add( Object key, Object value )
////        {
////            lock(m_root)
////            {
////                m_list.Add( key, value );
////            }
////        }
////
////        public override int Capacity
////        {
////            get
////            {
////                lock(m_root)
////                {
////                    return m_list.Capacity;
////                }
////            }
////        }
////
////        public override void Clear()
////        {
////            lock(m_root)
////            {
////                m_list.Clear();
////            }
////        }
////
////        public override Object Clone()
////        {
////            lock(m_root)
////            {
////                return m_list.Clone();
////            }
////        }
////
////        public override bool Contains( Object key )
////        {
////            lock(m_root)
////            {
////                return m_list.Contains( key );
////            }
////        }
////
////        public override bool ContainsKey( Object key )
////        {
////            lock(m_root)
////            {
////                return m_list.ContainsKey( key );
////            }
////        }
////
////        public override bool ContainsValue( Object key )
////        {
////            lock(m_root)
////            {
////                return m_list.ContainsValue( key );
////            }
////        }
////
////        public override void CopyTo( Array array, int index )
////        {
////            lock(m_root)
////            {
////                m_list.CopyTo( array, index );
////            }
////        }
////
////        public override Object GetByIndex( int index )
////        {
////            lock(m_root)
////            {
////                return m_list.GetByIndex( index );
////            }
////        }
////
////        public override IDictionaryEnumerator GetEnumerator()
////        {
////            lock(m_root)
////            {
////                return m_list.GetEnumerator();
////            }
////        }
////
////        public override Object GetKey( int index )
////        {
////            lock(m_root)
////            {
////                return m_list.GetKey( index );
////            }
////        }
////
////        public override IList GetKeyList()
////        {
////            lock(m_root)
////            {
////                return m_list.GetKeyList();
////            }
////        }
////
////        public override IList GetValueList()
////        {
////            lock(m_root)
////            {
////                return m_list.GetValueList();
////            }
////        }
////
////        public override int IndexOfKey( Object key )
////        {
////            lock(m_root)
////            {
////                return m_list.IndexOfKey( key );
////            }
////        }
////
////        public override int IndexOfValue( Object value )
////        {
////            lock(m_root)
////            {
////                return m_list.IndexOfValue( value );
////            }
////        }
////
////        public override void RemoveAt( int index )
////        {
////            lock(m_root)
////            {
////                m_list.RemoveAt( index );
////            }
////        }
////
////        public override void Remove( Object key )
////        {
////            lock(m_root)
////            {
////                m_list.Remove( key );
////            }
////        }
////
////        public override void SetByIndex( int index, Object value )
////        {
////            lock(m_root)
////            {
////                m_list.SetByIndex( index, value );
////            }
////        }
////
////        internal override KeyValuePairs[] ToKeyValuePairsArray()
////        {
////            return m_list.ToKeyValuePairsArray();
////        }
////
////        public override void TrimToSize()
////        {
////            lock(m_root)
////            {
////                m_list.TrimToSize();
////            }
////        }
////    }


        [Serializable]
        private class SortedListEnumerator : IDictionaryEnumerator, ICloneable
        {
            internal const int Keys      = 1;
            internal const int Values    = 2;
            internal const int DictEntry = 3;

            private SortedList m_sortedList;
            private Object     m_key;
            private Object     m_value;
            private int        m_index;
            private int        m_startIndex;        // Store for Reset.
            private int        m_endIndex;
            private int        m_version;
            private bool       m_current;           // Is the current element valid?
            private int        m_getObjectRetType;  // What should GetObject return?

            internal SortedListEnumerator( SortedList sortedList, int index, int count, int getObjRetType )
            {
                m_sortedList       = sortedList;
                m_index            = index;
                m_startIndex       = index;
                m_endIndex         = index + count;
                m_version          = sortedList.m_version;
                m_current          = false;
                m_getObjectRetType = getObjRetType;
            }

            public Object Clone()
            {
                return MemberwiseClone();
            }

            public virtual Object Key
            {
                get
                {
                    if(m_version != m_sortedList.m_version)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumFailedVersion" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }

                    if(m_current == false)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumOpCantHappen" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }

                    return m_key;
                }
            }

            public virtual bool MoveNext()
            {
                if(m_version != m_sortedList.m_version)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumFailedVersion" ) );
#else
                    throw new InvalidOperationException();
#endif
                }

                if(m_index < m_endIndex)
                {
                    m_key   = m_sortedList.m_keys  [m_index];
                    m_value = m_sortedList.m_values[m_index];
                    m_index++;
                    m_current = true;
                    return true;
                }

                m_key     = null;
                m_value   = null;
                m_current = false;

                return false;
            }

            public virtual DictionaryEntry Entry
            {
                get
                {
                    if(m_version != m_sortedList.m_version)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumFailedVersion" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }

                    if(m_current == false)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumOpCantHappen" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }

                    return new DictionaryEntry( m_key, m_value );
                }
            }

            public virtual Object Current
            {
                get
                {
                    if(m_current == false)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumOpCantHappen" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }

                    if(m_getObjectRetType == Keys)
                    {
                        return m_key;
                    }
                    else if(m_getObjectRetType == Values)
                    {
                        return m_value;
                    }
                    else
                    {
                        return new DictionaryEntry( m_key, m_value );
                    }
                }
            }

            public virtual Object Value
            {
                get
                {
                    if(m_version != m_sortedList.m_version)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumFailedVersion" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }

                    if(m_current == false)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumOpCantHappen" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }

                    return m_value;
                }
            }

            public virtual void Reset()
            {
                if(m_version != m_sortedList.m_version)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumFailedVersion" ) );
#else
                    throw new InvalidOperationException();
#endif
                }

                m_index   = m_startIndex;
                m_current = false;
                m_key     = null;
                m_value   = null;
            }
        }

        [Serializable]
        private class KeyList : IList
        {
            private SortedList m_sortedList;

            internal KeyList( SortedList sortedList )
            {
                this.m_sortedList = sortedList;
            }

            public virtual int Count
            {
                get
                {
                    return m_sortedList.m_size;
                }
            }

            public virtual bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public virtual bool IsFixedSize
            {
                get
                {
                    return true;
                }
            }

            public virtual bool IsSynchronized
            {
                get
                {
                    return m_sortedList.IsSynchronized;
                }
            }

            public virtual Object SyncRoot
            {
                get
                {
                    return m_sortedList.SyncRoot;
                }
            }

            public virtual int Add( Object key )
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SortedListNestedWrite" ) );
#else
                throw new NotSupportedException();
#endif
            }

            public virtual void Clear()
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SortedListNestedWrite" ) );
#else
                throw new NotSupportedException();
#endif
            }

            public virtual bool Contains( Object key )
            {
                return m_sortedList.Contains( key );
            }

            public virtual void CopyTo( Array array, int arrayIndex )
            {
                if(array != null && array.Rank != 1)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentException( Environment.GetResourceString( "Arg_RankMultiDimNotSupported" ) );
#else
                    throw new ArgumentException();
#endif
                }

                // defer error checking to Array.Copy
                Array.Copy( m_sortedList.m_keys, 0, array, arrayIndex, m_sortedList.Count );
            }

            public virtual void Insert( int index, Object value )
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SortedListNestedWrite" ) );
#else
                throw new NotSupportedException();
#endif
            }

            public virtual Object this[int index]
            {
                get
                {
                    return m_sortedList.GetKey( index );
                }

                set
                {
#if EXCEPTION_STRINGS
                    throw new NotSupportedException( Environment.GetResourceString( "NotSupported_KeyCollectionSet" ) );
#else
                    throw new NotSupportedException();
#endif
                }
            }

            public virtual IEnumerator GetEnumerator()
            {
                return new SortedListEnumerator( m_sortedList, 0, m_sortedList.Count, SortedListEnumerator.Keys );
            }

            public virtual int IndexOf( Object key )
            {
                if(key == null)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentNullException( "key", Environment.GetResourceString( "ArgumentNull_Key" ) );
#else
                    throw new ArgumentNullException();
#endif
                }

                int i = Array.BinarySearch( m_sortedList.m_keys, 0, m_sortedList.Count, key, m_sortedList.m_comparer );

                if(i >= 0) return i;

                return -1;
            }

            public virtual void Remove( Object key )
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SortedListNestedWrite" ) );
#else
                throw new NotSupportedException();
#endif
            }

            public virtual void RemoveAt( int index )
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SortedListNestedWrite" ) );
#else
                throw new NotSupportedException();
#endif
            }
        }

        [Serializable]
        private class ValueList : IList
        {
            private SortedList m_sortedList;

            internal ValueList( SortedList sortedList )
            {
                m_sortedList = sortedList;
            }

            public virtual int Count
            {
                get
                {
                    return m_sortedList.m_size;
                }
            }

            public virtual bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public virtual bool IsFixedSize
            {
                get
                {
                    return true;
                }
            }

            public virtual bool IsSynchronized
            {
                get
                {
                    return m_sortedList.IsSynchronized;
                }
            }

            public virtual Object SyncRoot
            {
                get
                {
                    return m_sortedList.SyncRoot;
                }
            }

            public virtual int Add( Object key )
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SortedListNestedWrite" ) );
#else
                throw new NotSupportedException();
#endif
            }

            public virtual void Clear()
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SortedListNestedWrite" ) );
#else
                throw new NotSupportedException();
#endif
            }

            public virtual bool Contains( Object value )
            {
                return m_sortedList.ContainsValue( value );
            }

            public virtual void CopyTo( Array array, int arrayIndex )
            {
                if(array != null && array.Rank != 1)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentException( Environment.GetResourceString( "Arg_RankMultiDimNotSupported" ) );
#else
                    throw new ArgumentException();
#endif
                }

                // defer error checking to Array.Copy
                Array.Copy( m_sortedList.m_values, 0, array, arrayIndex, m_sortedList.Count );
            }

            public virtual void Insert( int index, Object value )
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SortedListNestedWrite" ) );
#else
                throw new NotSupportedException();
#endif
            }

            public virtual Object this[int index]
            {
                get
                {
                    return m_sortedList.GetByIndex( index );
                }

                set
                {
#if EXCEPTION_STRINGS
                    throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SortedListNestedWrite" ) );
#else
                    throw new NotSupportedException();
#endif
                }
            }

            public virtual IEnumerator GetEnumerator()
            {
                return new SortedListEnumerator( m_sortedList, 0, m_sortedList.Count, SortedListEnumerator.Values );
            }

            public virtual int IndexOf( Object value )
            {
                return Array.IndexOf( m_sortedList.m_values, value, 0, m_sortedList.Count );
            }

            public virtual void Remove( Object value )
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SortedListNestedWrite" ) );
#else
                throw new NotSupportedException();
#endif
            }

            public virtual void RemoveAt( int index )
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SortedListNestedWrite" ) );
#else
                throw new NotSupportedException();
#endif
            }

        }

////    // internal debug view class for sorted list
////    internal class SortedListDebugView
////    {
////        private SortedList sortedList;
////
////        public SortedListDebugView( SortedList sortedList )
////        {
////            if(sortedList == null)
////            {
////                throw new ArgumentNullException( "sortedList" );
////            }
////
////            this.sortedList = sortedList;
////        }
////
////        [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
////        public KeyValuePairs[] Items
////        {
////            get
////            {
////                return sortedList.ToKeyValuePairsArray();
////            }
////        }
////    }
    }
}
