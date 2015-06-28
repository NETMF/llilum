// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  List
**
** Purpose: Implements a generic, dynamically sized list as an
**          array.
**
**
===========================================================*/
namespace System.Collections.Generic
{
    using System;
    using System.Diagnostics;
    using System.Collections.ObjectModel;
////using System.Security.Permissions;

    // Implements a variable-size List that uses an array of objects to store the
    // elements. A List has a capacity, which is the allocated length
    // of the internal array. As elements are added to a List, the capacity
    // of the List is automatically increased as required by reallocating the
    // internal array.
    //
////[DebuggerTypeProxy( typeof( Mscorlib_CollectionDebugView<> ) )]
////[DebuggerDisplay( "Count = {Count}" )]
    [Serializable]
    public class List<T> : IList<T>, System.Collections.IList
    {
        private const int cDefaultCapacity = 4;

        static readonly T[] sEmptyArray = new T[0];

        private T[]    m_items;
        private int    m_size;
        private int    m_version;
        [NonSerialized]
        private Object m_syncRoot;

        // Constructs a List. The list is initially empty and has a capacity
        // of zero. Upon adding the first element to the list the capacity is
        // increased to 16, and then increased in multiples of two as required.
        public List()
        {
            m_items = sEmptyArray;
        }

        // Constructs a List with a given initial capacity. The list is
        // initially empty, but will have room for the given number of elements
        // before any reallocations are required.
        //
        public List( int capacity )
        {
            if(capacity < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.capacity, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum );
            }

            m_items = new T[capacity];
        }

        // Constructs a List, copying the contents of the given collection. The
        // size and capacity of the new list will both be equal to the size of the
        // given collection.
        //
        public List( IEnumerable<T> collection )
        {
            if(collection == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.collection );
            }

            ICollection<T> c = collection as ICollection<T>;
            if(c != null)
            {
                int count = c.Count;

                m_items = new T[count];

                c.CopyTo( m_items, 0 );

                m_size = count;
            }
            else
            {
                m_size  = 0;
                m_items = new T[cDefaultCapacity];

                using(IEnumerator<T> en = collection.GetEnumerator())
                {
                    while(en.MoveNext())
                    {
                        Add( en.Current );
                    }
                }
            }
        }

        // Gets and sets the capacity of this list.  The capacity is the size of
        // the internal array used to hold items.  When set, the internal
        // array of the list is reallocated to the given capacity.
        //
        public int Capacity
        {
            get
            {
                return m_items.Length;
            }

            set
            {
                if(value != m_items.Length)
                {
                    if(value < m_size)
                    {
                        ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.value, ExceptionResource.ArgumentOutOfRange_SmallCapacity );
                    }

                    if(value > 0)
                    {
                        T[] newItems = new T[value];
                        if(m_size > 0)
                        {
                            Array.Copy( m_items, 0, newItems, 0, m_size );
                        }
                        m_items = newItems;
                    }
                    else
                    {
                        m_items = sEmptyArray;
                    }
                }
            }
        }

        // Read-only property describing how many elements are in the List.
        public int Count
        {
            get
            {
                return m_size;
            }
        }

        bool System.Collections.IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }


        // Is this List read-only?
        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool System.Collections.IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        // Is this List synchronized (thread-safe)?
        bool System.Collections.ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        // Synchronization root for this object.
        Object System.Collections.ICollection.SyncRoot
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

        // Sets or Gets the element at the given index.
        //
        public T this[int index]
        {
            get
            {
                // Fllowing trick can reduce the range check by one
                if((uint)index >= (uint)m_size)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }

                return m_items[index];
            }
            set
            {
                if((uint)index >= (uint)m_size)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }

                m_items[index] = value;
                m_version++;
            }
        }

        private static bool IsCompatibleObject( object value )
        {
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>.
            return ((value is T) || (value == null && default( T ) == null));
        }

        Object System.Collections.IList.this[int index]
        {
            get
            {
                return this[index];
            }

            set
            {
                ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>( value, ExceptionArgument.value );

                try
                {
                    this[index] = (T)value;
                }
                catch(InvalidCastException)
                {
                    ThrowHelper.ThrowWrongValueTypeArgumentException( value, typeof( T ) );
                }
            }
        }

        // Adds the given object to the end of this list. The size of the list is
        // increased by one. If required, the capacity of the list is doubled
        // before adding the new element.
        //
        public void Add( T item )
        {
            if(m_size == m_items.Length)
            {
                EnsureCapacity( m_size + 1 );
            }

            m_items[m_size++] = item;
            m_version++;
        }

        int System.Collections.IList.Add( Object item )
        {
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>( item, ExceptionArgument.item );

            try
            {
                Add( (T)item );
            }
            catch(InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException( item, typeof( T ) );
            }

            return Count - 1;
        }


        // Adds the elements of the given collection to the end of this list. If
        // required, the capacity of the list is increased to twice the previous
        // capacity or the new size, whichever is larger.
        //
        public void AddRange( IEnumerable<T> collection )
        {
            InsertRange( m_size, collection );
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            return new ReadOnlyCollection<T>( this );
        }

        // Searches a section of the list for a given element using a binary search
        // algorithm. Elements of the list are compared to the search value using
        // the given IComparer interface. If comparer is null, elements of
        // the list are compared to the search value using the IComparable
        // interface, which in that case must be implemented by all elements of the
        // list and the given search value. This method assumes that the given
        // section of the list is already sorted; if this is not the case, the
        // result will be incorrect.
        //
        // The method returns the index of the given value in the list. If the
        // list does not contain the given value, the method returns a negative
        // integer. The bitwise complement operator (~) can be applied to a
        // negative result to produce the index of the first element (if any) that
        // is larger than the given search value. This is also the index at which
        // the search value should be inserted into the list in order for the list
        // to remain sorted.
        //
        // The method uses the Array.BinarySearch method to perform the
        // search.
        //
        public int BinarySearch( int index, int count, T item, IComparer<T> comparer )
        {
            if(index < 0 || count < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( (index < 0 ? ExceptionArgument.index : ExceptionArgument.count), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum );
            }

            if(m_size - index < count)
            {
                ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidOffLen );
            }

            return Array.BinarySearch<T>( m_items, index, count, item, comparer );
        }

        public int BinarySearch( T item )
        {
            return BinarySearch( 0, Count, item, null );
        }

        public int BinarySearch( T item, IComparer<T> comparer )
        {
            return BinarySearch( 0, Count, item, comparer );
        }


        // Clears the contents of List.
        public void Clear()
        {
            if(m_size > 0)
            {
                Array.Clear( m_items, 0, m_size ); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.

                m_size = 0;
            }

            m_version++;
        }

        // Contains returns true if the specified element is in the List.
        // It does a linear, O(n) search.  Equality is determined by calling
        // item.Equals().
        //
        public bool Contains( T item )
        {
            if((Object)item == null)
            {
                for(int i = 0; i < m_size; i++)
                {
                    if((Object)m_items[i] == null)
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                EqualityComparer<T> c = EqualityComparer<T>.Default;

                for(int i = 0; i < m_size; i++)
                {
                    if(c.Equals( m_items[i], item )) return true;
                }

                return false;
            }
        }

        bool System.Collections.IList.Contains( Object item )
        {
            if(IsCompatibleObject( item ))
            {
                return Contains( (T)item );
            }

            return false;
        }

        public List<TOutput> ConvertAll<TOutput>( Converter<T, TOutput> converter )
        {
            if(converter == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.converter );
            }

            List<TOutput> list = new List<TOutput>( m_size );

            for(int i = 0; i < m_size; i++)
            {
                list.m_items[i] = converter( m_items[i] );
            }

            list.m_size = m_size;

            return list;
        }

        // Copies this List into array, which must be of a
        // compatible array type.
        //
        public void CopyTo( T[] array )
        {
            CopyTo( array, 0 );
        }

        // Copies this List into array, which must be of a
        // compatible array type.
        //
        void System.Collections.ICollection.CopyTo( Array array, int arrayIndex )
        {
            if((array != null) && (array.Rank != 1))
            {
                ThrowHelper.ThrowArgumentException( ExceptionResource.Arg_RankMultiDimNotSupported );
            }

            try
            {
                // Array.Copy will check for NULL.
                Array.Copy( m_items, 0, array, arrayIndex, m_size );
            }
            catch(ArrayTypeMismatchException)
            {
                ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidArrayType );
            }
        }

        // Copies a section of this list to the given array at the given index.
        //
        // The method uses the Array.Copy method to copy the elements.
        //
        public void CopyTo( int index, T[] array, int arrayIndex, int count )
        {
            if(m_size - index < count)
            {
                ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidOffLen );
            }

            // Delegate rest of error checking to Array.Copy.
            Array.Copy( m_items, index, array, arrayIndex, count );
        }

        public void CopyTo( T[] array, int arrayIndex )
        {
            // Delegate rest of error checking to Array.Copy.
            Array.Copy( m_items, 0, array, arrayIndex, m_size );
        }

        // Ensures that the capacity of this list is at least the given minimum
        // value. If the currect capacity of the list is less than min, the
        // capacity is increased to twice the current capacity or to min,
        // whichever is larger.
        private void EnsureCapacity( int min )
        {
            if(m_items.Length < min)
            {
                int newCapacity = m_items.Length == 0 ? cDefaultCapacity : m_items.Length * 2;

                if(newCapacity < min) newCapacity = min;

                Capacity = newCapacity;
            }
        }

        public bool Exists( Predicate<T> match )
        {
            return FindIndex( match ) != -1;
        }

        public T Find( Predicate<T> match )
        {
            if(match == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.match );
            }

            for(int i = 0; i < m_size; i++)
            {
                if(match( m_items[i] ))
                {
                    return m_items[i];
                }
            }

            return default( T );
        }

        public List<T> FindAll( Predicate<T> match )
        {
            if(match == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.match );
            }

            List<T> list = new List<T>();

            for(int i = 0; i < m_size; i++)
            {
                if(match( m_items[i] ))
                {
                    list.Add( m_items[i] );
                }
            }

            return list;
        }

        public int FindIndex( Predicate<T> match )
        {
            return FindIndex( 0, m_size, match );
        }

        public int FindIndex( int startIndex, Predicate<T> match )
        {
            return FindIndex( startIndex, m_size - startIndex, match );
        }

        public int FindIndex( int startIndex, int count, Predicate<T> match )
        {
            if((uint)startIndex > (uint)m_size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index );
            }

            if(count < 0 || startIndex > m_size - count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count );
            }

            if(match == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.match );
            }

            int endIndex = startIndex + count;

            for(int i = startIndex; i < endIndex; i++)
            {
                if(match( m_items[i] )) return i;
            }

            return -1;
        }

        public T FindLast( Predicate<T> match )
        {
            if(match == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.match );
            }

            for(int i = m_size - 1; i >= 0; i--)
            {
                if(match( m_items[i] ))
                {
                    return m_items[i];
                }
            }

            return default( T );
        }

        public int FindLastIndex( Predicate<T> match )
        {
            return FindLastIndex( m_size - 1, m_size, match );
        }

        public int FindLastIndex( int startIndex, Predicate<T> match )
        {
            return FindLastIndex( startIndex, startIndex + 1, match );
        }

        public int FindLastIndex( int startIndex, int count, Predicate<T> match )
        {
            if(match == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.match );
            }

            if(m_size == 0)
            {
                // Special case for 0 length List
                if(startIndex != -1)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index );
                }
            }
            else
            {
                // Make sure we're not out of range
                if((uint)startIndex >= (uint)m_size)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index );
                }
            }

            // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
            if(count < 0 || startIndex - count + 1 < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count );
            }

            int endIndex = startIndex - count;
            for(int i = startIndex; i > endIndex; i--)
            {
                if(match( m_items[i] ))
                {
                    return i;
                }
            }
            return -1;
        }


        public void ForEach( Action<T> action )
        {
            if(action == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.match );
            }

            for(int i = 0; i < m_size; i++)
            {
                action( m_items[i] );
            }
        }

        // Returns an enumerator for this list with the given
        // permission for removal of elements. If modifications made to the list
        // while an enumeration is in progress, the MoveNext and
        // GetObject methods of the enumerator will throw an exception.
        //
        public Enumerator GetEnumerator()
        {
            return new Enumerator( this );
        }

        /// <internalonly/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator( this );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator( this );
        }

        public List<T> GetRange( int index, int count )
        {
            if(index < 0 || count < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( (index < 0 ? ExceptionArgument.index : ExceptionArgument.count), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum );
            }

            if(m_size - index < count)
            {
                ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidOffLen );
            }

            List<T> list = new List<T>( count );

            Array.Copy( m_items, index, list.m_items, 0, count );

            list.m_size = count;

            return list;
        }


        // Returns the index of the first occurrence of a given value in a range of
        // this list. The list is searched forwards from beginning to end.
        // The elements of the list are compared to the given value using the
        // Object.Equals method.
        //
        // This method uses the Array.IndexOf method to perform the
        // search.
        //
        public int IndexOf( T item )
        {
            return Array.IndexOf( m_items, item, 0, m_size );
        }

        int System.Collections.IList.IndexOf( Object item )
        {
            if(IsCompatibleObject( item ))
            {
                return IndexOf( (T)item );
            }

            return -1;
        }

        // Returns the index of the first occurrence of a given value in a range of
        // this list. The list is searched forwards, starting at index
        // index and ending at count number of elements. The
        // elements of the list are compared to the given value using the
        // Object.Equals method.
        //
        // This method uses the Array.IndexOf method to perform the
        // search.
        //
        public int IndexOf( T item, int index )
        {
            if(index > m_size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index );
            }

            return Array.IndexOf( m_items, item, index, m_size - index );
        }

        // Returns the index of the first occurrence of a given value in a range of
        // this list. The list is searched forwards, starting at index
        // index and upto count number of elements. The
        // elements of the list are compared to the given value using the
        // Object.Equals method.
        //
        // This method uses the Array.IndexOf method to perform the
        // search.
        //
        public int IndexOf( T item, int index, int count )
        {
            if(index > m_size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index );
            }

            if(count < 0 || index > m_size - count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count );
            }

            return Array.IndexOf( m_items, item, index, count );
        }

        // Inserts an element into this list at a given index. The size of the list
        // is increased by one. If required, the capacity of the list is doubled
        // before inserting the new element.
        //
        public void Insert( int index, T item )
        {
            // Note that insertions at the end are legal.
            if((uint)index > (uint)m_size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_ListInsert );
            }

            if(m_size == m_items.Length)
            {
                EnsureCapacity( m_size + 1 );
            }

            if(index < m_size)
            {
                Array.Copy( m_items, index, m_items, index + 1, m_size - index );
            }

            m_items[index] = item;
            m_size++;
            m_version++;
        }

        void System.Collections.IList.Insert( int index, Object item )
        {
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>( item, ExceptionArgument.item );

            try
            {
                Insert( index, (T)item );
            }
            catch(InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException( item, typeof( T ) );
            }
        }

        // Inserts the elements of the given collection at a given index. If
        // required, the capacity of the list is increased to twice the previous
        // capacity or the new size, whichever is larger.  Ranges may be added
        // to the end of the list by setting index to the List's size.
        //
        public void InsertRange( int index, IEnumerable<T> collection )
        {
            if(collection == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.collection );
            }

            if((uint)index > (uint)m_size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index );
            }

            ICollection<T> c = collection as ICollection<T>;
            if(c != null)
            {    // if collection is ICollection<T>
                int count = c.Count;
                if(count > 0)
                {
                    EnsureCapacity( m_size + count );
                    if(index < m_size)
                    {
                        Array.Copy( m_items, index, m_items, index + count, m_size - index );
                    }

                    // If we're inserting a List into itself, we want to be able to deal with that.
                    if(this == c)
                    {
                        // Copy first part of _items to insert location
                        Array.Copy( m_items, 0            , m_items, index    , index          );
                        // Copy last part of _items back to inserted location
                        Array.Copy( m_items, index + count, m_items, index * 2, m_size - index );
                    }
                    else
                    {
                        T[] itemsToInsert = new T[count];

                        c.CopyTo( itemsToInsert, 0 );

                        itemsToInsert.CopyTo( m_items, index );
                    }

                    m_size += count;
                }
            }
            else
            {
                using(IEnumerator<T> en = collection.GetEnumerator())
                {
                    while(en.MoveNext())
                    {
                        Insert( index++, en.Current );
                    }
                }
            }

            m_version++;
        }

        // Returns the index of the last occurrence of a given value in a range of
        // this list. The list is searched backwards, starting at the end
        // and ending at the first element in the list. The elements of the list
        // are compared to the given value using the Object.Equals method.
        //
        // This method uses the Array.LastIndexOf method to perform the
        // search.
        //
        public int LastIndexOf( T item )
        {
            return LastIndexOf( item, m_size - 1, m_size );
        }

        // Returns the index of the last occurrence of a given value in a range of
        // this list. The list is searched backwards, starting at index
        // index and ending at the first element in the list. The
        // elements of the list are compared to the given value using the
        // Object.Equals method.
        //
        // This method uses the Array.LastIndexOf method to perform the
        // search.
        //
        public int LastIndexOf( T item, int index )
        {
            if(index >= m_size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index );
            }

            return LastIndexOf( item, index, index + 1 );
        }

        // Returns the index of the last occurrence of a given value in a range of
        // this list. The list is searched backwards, starting at index
        // index and upto count elements. The elements of
        // the list are compared to the given value using the Object.Equals
        // method.
        //
        // This method uses the Array.LastIndexOf method to perform the
        // search.
        //
        public int LastIndexOf( T item, int index, int count )
        {
            if(m_size == 0)
            {
                return -1;
            }

            if(index < 0 || count < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( (index < 0 ? ExceptionArgument.index : ExceptionArgument.count), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum );
            }

            if(index >= m_size || count > index + 1)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( (index >= m_size ? ExceptionArgument.index : ExceptionArgument.count), ExceptionResource.ArgumentOutOfRange_BiggerThanCollection );
            }

            return Array.LastIndexOf( m_items, item, index, count );
        }

        // Removes the element at the given index. The size of the list is
        // decreased by one.
        //
        public bool Remove( T item )
        {
            int index = IndexOf( item );
            if(index >= 0)
            {
                RemoveAt( index );
                return true;
            }

            return false;
        }

        void System.Collections.IList.Remove( Object item )
        {
            if(IsCompatibleObject( item ))
            {
                Remove( (T)item );
            }
        }

        // This method removes all items which matches the predicate.
        // The complexity is O(n).
        public int RemoveAll( Predicate<T> match )
        {
            if(match == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.match );
            }

            int freeIndex = 0;   // the first free slot in items array

            // Find the first item which needs to be removed.
            while(freeIndex < m_size && !match( m_items[freeIndex] ))
            {
                freeIndex++;
            }

            if(freeIndex >= m_size)
            {
                return 0;
            }

            int current = freeIndex + 1;
            while(current < m_size)
            {
                // Find the first item which needs to be kept.
                while(current < m_size && match( m_items[current] ))
                {
                    current++;
                }

                if(current < m_size)
                {
                    // copy item to the free slot.
                    m_items[freeIndex++] = m_items[current++];
                }
            }

            Array.Clear( m_items, freeIndex, m_size - freeIndex );

            int result = m_size - freeIndex;

            m_size = freeIndex;
            m_version++;

            return result;
        }

        // Removes the element at the given index. The size of the list is
        // decreased by one.
        //
        public void RemoveAt( int index )
        {
            if((uint)index >= (uint)m_size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            m_size--;
            if(index < m_size)
            {
                Array.Copy( m_items, index + 1, m_items, index, m_size - index );
            }

            m_items[m_size] = default( T );
            m_version++;
        }

        // Removes a range of elements from this list.
        //
        public void RemoveRange( int index, int count )
        {
            if(index < 0 || count < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( (index < 0 ? ExceptionArgument.index : ExceptionArgument.count), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum );
            }

            if(m_size - index < count)
            {
                ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidOffLen );
            }

            if(count > 0)
            {
                int i = m_size;

                m_size -= count;
                if(index < m_size)
                {
                    Array.Copy( m_items, index + count, m_items, index, m_size - index );
                }

                Array.Clear( m_items, m_size, count );

                m_version++;
            }
        }

        // Reverses the elements in this list.
        public void Reverse()
        {
            Reverse( 0, Count );
        }

        // Reverses the elements in a range of this list. Following a call to this
        // method, an element in the range given by index and count
        // which was previously located at index i will now be located at
        // index index + (index + count - i - 1).
        //
        // This method uses the Array.Reverse method to reverse the
        // elements.
        //
        public void Reverse( int index, int count )
        {
            if(index < 0 || count < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( (index < 0 ? ExceptionArgument.index : ExceptionArgument.count), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum );
            }

            if(m_size - index < count)
            {
                ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidOffLen );
            }

            Array.Reverse( m_items, index, count );

            m_version++;
        }

        // Sorts the elements in this list.  Uses the default comparer and
        // Array.Sort.
        public void Sort()
        {
            Sort( 0, Count, null );
        }

        // Sorts the elements in this list.  Uses Array.Sort with the
        // provided comparer.
        public void Sort( IComparer<T> comparer )
        {
            Sort( 0, Count, comparer );
        }

        // Sorts the elements in a section of this list. The sort compares the
        // elements to each other using the given IComparer interface. If
        // comparer is null, the elements are compared to each other using
        // the IComparable interface, which in that case must be implemented by all
        // elements of the list.
        //
        // This method uses the Array.Sort method to sort the elements.
        //
        public void Sort( int index, int count, IComparer<T> comparer )
        {
            if(index < 0 || count < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( (index < 0 ? ExceptionArgument.index : ExceptionArgument.count), ExceptionResource.ArgumentOutOfRange_NeedNonNegNum );
            }

            if(m_size - index < count)
            {
                ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidOffLen );
            }

            Array.Sort<T>( m_items, index, count, comparer );

            m_version++;
        }

        public void Sort( Comparison<T> comparison )
        {
            if(comparison == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.match );
            }

            if(m_size > 0)
            {
                IComparer<T> comparer = new Array.FunctorComparer<T>( comparison );

                Array.Sort( m_items, 0, m_size, comparer );
            }
        }

        // ToArray returns a new Object array containing the contents of the List.
        // This requires copying the List, which is an O(n) operation.
        public T[] ToArray()
        {
            T[] array = new T[m_size];

            Array.Copy( m_items, 0, array, 0, m_size );

            return array;
        }

        // Sets the capacity of this list to the size of the list. This method can
        // be used to minimize a list's memory overhead once it is known that no
        // new elements will be added to the list. To completely clear a list and
        // release all memory referenced by the list, execute the following
        // statements:
        //
        // list.Clear();
        // list.TrimExcess();
        //
        public void TrimExcess()
        {
            int threshold = (int)(((double)m_items.Length) * 0.9);

            if(m_size < threshold)
            {
                Capacity = m_size;
            }
        }

        public bool TrueForAll( Predicate<T> match )
        {
            if(match == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.match );
            }

            for(int i = 0; i < m_size; i++)
            {
                if(!match( m_items[i] ))
                {
                    return false;
                }
            }

            return true;
        }

        [Serializable]
        public struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator
        {
            private List<T> m_list;
            private int     m_index;
            private int     m_version;
            private T       m_current;

            internal Enumerator( List<T> list )
            {
                m_list    = list;
                m_index   = 0;
                m_version = list.m_version;
                m_current = default( T );
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if(m_version != m_list.m_version)
                {
                    ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_EnumFailedVersion );
                }

                if((uint)m_index < (uint)m_list.m_size)
                {
                    m_current = m_list.m_items[m_index];
                    m_index++;
                    return true;
                }

                m_index   = m_list.m_size + 1;
                m_current = default( T );
                return false;
            }

            public T Current
            {
                get
                {
                    return m_current;
                }
            }

            Object System.Collections.IEnumerator.Current
            {
                get
                {
                    if(m_index == 0 || m_index == m_list.m_size + 1)
                    {
                        ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_EnumOpCantHappen );
                    }

                    return Current;
                }
            }

            void System.Collections.IEnumerator.Reset()
            {
                if(m_version != m_list.m_version)
                {
                    ThrowHelper.ThrowInvalidOperationException( ExceptionResource.InvalidOperation_EnumFailedVersion );
                }

                m_index   = 0;
                m_current = default( T );
            }
        }
    }
}

