// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  ArrayList
**
**
** Purpose: Implements a dynamically sized List as an array,
**          and provides many convenience methods for treating
**          an array as an IList.
**
**
===========================================================*/
namespace System.Collections
{
    using System;
////using System.Security.Permissions;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    // Implements a variable-size List that uses an array of objects to store the
    // elements. A ArrayList has a capacity, which is the allocated length
    // of the internal array. As elements are added to a ArrayList, the capacity
    // of the ArrayList is automatically increased as required by reallocating the
    // internal array.
    //
////[DebuggerTypeProxy( typeof( System.Collections.ArrayList.ArrayListDebugView ) )]
////[DebuggerDisplay( "Count = {Count}" )]
    [Serializable]
    public class ArrayList : IList, ICloneable
    {
        private const int                cDefaultCapacity = 4;

        private static readonly Object[] emptyArray = new Object[0];

        private Object[] m_items;
        private int      m_size;
        private int      m_version;
        [NonSerialized]
        private Object   m_syncRoot;

        // Note: this constructor is a bogus constructor that does nothing
        // and is for use only with SyncArrayList.
        internal ArrayList( bool trash )
        {
        }

        // Constructs a ArrayList. The list is initially empty and has a capacity
        // of zero. Upon adding the first element to the list the capacity is
        // increased to cDefaultCapacity, and then increased in multiples of two as required.
        public ArrayList()
        {
            m_items = emptyArray;
        }

        // Constructs a ArrayList with a given initial capacity. The list is
        // initially empty, but will have room for the given number of elements
        // before any reallocations are required.
        //
        public ArrayList( int capacity )
        {
            if(capacity < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "capacity", Environment.GetResourceString( "ArgumentOutOfRange_MustBeNonNegNum", "capacity" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            m_items = new Object[capacity];
        }

        // Constructs a ArrayList, copying the contents of the given collection. The
        // size and capacity of the new list will both be equal to the size of the
        // given collection.
        //
        public ArrayList( ICollection c )
        {
            if(c == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "c", Environment.GetResourceString( "ArgumentNull_Collection" ) );
#else
                throw new ArgumentNullException();
#endif
            }

            m_items = new Object[c.Count];
            AddRange( c );
        }

        // Gets and sets the capacity of this list.  The capacity is the size of
        // the internal array used to hold items.  When set, the internal
        // array of the list is reallocated to the given capacity.
        //
        public virtual int Capacity
        {
            get
            {
                return m_items.Length;
            }

            set
            {
                // We don't want to update the version number when we change the capacity.
                // Some existing applications have dependency on this.
                if(value != m_items.Length)
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
                        Object[] newItems = new Object[value];
                        if(m_size > 0)
                        {
                            Array.Copy( m_items, 0, newItems, 0, m_size );
                        }

                        m_items = newItems;
                    }
                    else
                    {
                        m_items = new Object[cDefaultCapacity];
                    }
                }
            }
        }

        // Read-only property describing how many elements are in the List.
        public virtual int Count
        {
            get
            {
                return m_size;
            }
        }

        public virtual bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        // Is this ArrayList read-only?
        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        // Is this ArrayList synchronized (thread-safe)?
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

        // Sets or Gets the element at the given index.
        //
        public virtual Object this[int index]
        {
            get
            {
                if(index < 0 || index >= m_size)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                    throw new ArgumentOutOfRangeException();
#endif
                }

                return m_items[index];
            }
            set
            {
                if(index < 0 || index >= m_size)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                    throw new ArgumentOutOfRangeException();
#endif
                }

                m_items[index] = value;
                m_version++;
            }
        }

////    // Creates a ArrayList wrapper for a particular IList.  This does not
////    // copy the contents of the IList, but only wraps the ILIst.  So any
////    // changes to the underlying list will affect the ArrayList.  This would
////    // be useful if you want to Reverse a subrange of an IList, or want to
////    // use a generic BinarySearch or Sort method without implementing one yourself.
////    // However, since these methods are generic, the performance may not be
////    // nearly as good for some operations as they would be on the IList itself.
////    //
////    public static ArrayList Adapter( IList list )
////    {
////        if(list == null)
////        {
////            throw new ArgumentNullException( "list" );
////        }
////
////        return new IListWrapper( list );
////    }

        // Adds the given object to the end of this list. The size of the list is
        // increased by one. If required, the capacity of the list is doubled
        // before adding the new element.
        //
        public virtual int Add( Object value )
        {
            if(m_size == m_items.Length)
            {
                EnsureCapacity( m_size + 1 );
            }

            m_items[m_size] = value;
            m_version++;

            return m_size++;
        }

        // Adds the elements of the given collection to the end of this list. If
        // required, the capacity of the list is increased to twice the previous
        // capacity or the new size, whichever is larger.
        //
        public virtual void AddRange( ICollection c )
        {
            InsertRange( m_size, c );
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
        public virtual int BinarySearch( int index, int count, Object value, IComparer comparer )
        {
            if(index < 0 || count < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(m_size - index < count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                throw new ArgumentException();
#endif
            }

            return Array.BinarySearch( (Array)m_items, index, count, value, comparer );
        }

        public virtual int BinarySearch( Object value )
        {
            return BinarySearch( 0, Count, value, null );
        }

        public virtual int BinarySearch( Object value, IComparer comparer )
        {
            return BinarySearch( 0, Count, value, comparer );
        }


        // Clears the contents of ArrayList.
        public virtual void Clear()
        {
            if(m_size > 0)
            {
                Array.Clear( m_items, 0, m_size ); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
                m_size = 0;
            }

            m_version++;
        }

        // Clones this ArrayList, doing a shallow copy.  (A copy is made of all
        // Object references in the ArrayList, but the Objects pointed to
        // are not cloned).
        public virtual Object Clone()
        {
            ArrayList la = new ArrayList( m_size );

            la.m_size    = m_size;
            la.m_version = m_version;

            Array.Copy( m_items, 0, la.m_items, 0, m_size );

            return la;
        }


        // Contains returns true if the specified element is in the ArrayList.
        // It does a linear, O(n) search.  Equality is determined by calling
        // item.Equals().
        //
        public virtual bool Contains( Object item )
        {
            if(item == null)
            {
                for(int i = 0; i < m_size; i++)
                {
                    if(m_items[i] == null)
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                for(int i = 0; i < m_size; i++)
                {
                    if((m_items[i] != null) && (m_items[i].Equals( item )))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // Copies this ArrayList into array, which must be of a
        // compatible array type.
        //
        public virtual void CopyTo( Array array )
        {
            CopyTo( array, 0 );
        }

        // Copies this ArrayList into array, which must be of a
        // compatible array type.
        //
        public virtual void CopyTo( Array array, int arrayIndex )
        {
            if((array != null) && (array.Rank != 1))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_RankMultiDimNotSupported" ) );
#else
                throw new ArgumentException();
#endif
            }

            // Delegate rest of error checking to Array.Copy.
            Array.Copy( m_items, 0, array, arrayIndex, m_size );
        }

        // Copies a section of this list to the given array at the given index.
        //
        // The method uses the Array.Copy method to copy the elements.
        //
        public virtual void CopyTo( int index, Array array, int arrayIndex, int count )
        {
            if(m_size - index < count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                throw new ArgumentException();
#endif
            }

            if((array != null) && (array.Rank != 1))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_RankMultiDimNotSupported" ) );
#else
                throw new ArgumentException();
#endif
            }

            // Delegate rest of error checking to Array.Copy.
            Array.Copy( m_items, index, array, arrayIndex, count );
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

////    // Returns a list wrapper that is fixed at the current size.  Operations
////    // that add or remove items will fail, however, replacing items is allowed.
////    //
////    public static IList FixedSize( IList list )
////    {
////        if(list == null)
////        {
////            throw new ArgumentNullException( "list" );
////        }
////
////        return new FixedSizeList( list );
////    }

////    // Returns a list wrapper that is fixed at the current size.  Operations
////    // that add or remove items will fail, however, replacing items is allowed.
////    //
////    public static ArrayList FixedSize( ArrayList list )
////    {
////        if(list == null)
////        {
////            throw new ArgumentNullException( "list" );
////        }
////
////        return new FixedSizeArrayList( list );
////    }

        // Returns an enumerator for this list with the given
        // permission for removal of elements. If modifications made to the list
        // while an enumeration is in progress, the MoveNext and
        // GetObject methods of the enumerator will throw an exception.
        //
        public virtual IEnumerator GetEnumerator()
        {
            return new ArrayListEnumeratorSimple( this );
        }

        // Returns an enumerator for a section of this list with the given
        // permission for removal of elements. If modifications made to the list
        // while an enumeration is in progress, the MoveNext and
        // GetObject methods of the enumerator will throw an exception.
        //
        public virtual IEnumerator GetEnumerator( int index, int count )
        {
            if(index < 0 || count < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(m_size - index < count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                throw new ArgumentException();
#endif
            }

            return new ArrayListEnumerator( this, index, count );
        }

        // Returns the index of the first occurrence of a given value in a range of
        // this list. The list is searched forwards from beginning to end.
        // The elements of the list are compared to the given value using the
        // Object.Equals method.
        //
        // This method uses the Array.IndexOf method to perform the
        // search.
        //
        public virtual int IndexOf( Object value )
        {
            return Array.IndexOf( (Array)m_items, value, 0, m_size );
        }

        // Returns the index of the first occurrence of a given value in a range of
        // this list. The list is searched forwards, starting at index
        // startIndex and ending at count number of elements. The
        // elements of the list are compared to the given value using the
        // Object.Equals method.
        //
        // This method uses the Array.IndexOf method to perform the
        // search.
        //
        public virtual int IndexOf( Object value, int startIndex )
        {
            if(startIndex > m_size)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            return Array.IndexOf( (Array)m_items, value, startIndex, m_size - startIndex );
        }

        // Returns the index of the first occurrence of a given value in a range of
        // this list. The list is searched forwards, starting at index
        // startIndex and upto count number of elements. The
        // elements of the list are compared to the given value using the
        // Object.Equals method.
        //
        // This method uses the Array.IndexOf method to perform the
        // search.
        //
        public virtual int IndexOf( Object value, int startIndex, int count )
        {
            if(startIndex > m_size)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(count < 0 || startIndex > m_size - count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            return Array.IndexOf( (Array)m_items, value, startIndex, count );
        }

        // Inserts an element into this list at a given index. The size of the list
        // is increased by one. If required, the capacity of the list is doubled
        // before inserting the new element.
        //
        public virtual void Insert( int index, Object value )
        {
            // Note that insertions at the end are legal.
            if(index < 0 || index > m_size)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_ArrayListInsert" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(m_size == m_items.Length) EnsureCapacity( m_size + 1 );

            if(index < m_size)
            {
                Array.Copy( m_items, index, m_items, index + 1, m_size - index );
            }

            m_items[index] = value;
            m_size++;
            m_version++;
        }

        // Inserts the elements of the given collection at a given index. If
        // required, the capacity of the list is increased to twice the previous
        // capacity or the new size, whichever is larger.  Ranges may be added
        // to the end of the list by setting index to the ArrayList's size.
        //
        public virtual void InsertRange( int index, ICollection c )
        {
            if(c == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "c", Environment.GetResourceString( "ArgumentNull_Collection" ) );
#else
                throw new ArgumentNullException();
#endif
            }

            if(index < 0 || index > m_size)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            int count = c.Count;
            if(count > 0)
            {
                EnsureCapacity( m_size + count );

                // shift existing items
                if(index < m_size)
                {
                    Array.Copy( m_items, index, m_items, index + count, m_size - index );
                }

                Object[] itemsToInsert = new Object[count];

                c.CopyTo( itemsToInsert, 0 );

                itemsToInsert.CopyTo( m_items, index );

                m_size += count;
                m_version++;
            }
        }

        // Returns the index of the last occurrence of a given value in a range of
        // this list. The list is searched backwards, starting at the end
        // and ending at the first element in the list. The elements of the list
        // are compared to the given value using the Object.Equals method.
        //
        // This method uses the Array.LastIndexOf method to perform the
        // search.
        //
        public virtual int LastIndexOf( Object value )
        {
            return LastIndexOf( value, m_size - 1, m_size );
        }

        // Returns the index of the last occurrence of a given value in a range of
        // this list. The list is searched backwards, starting at index
        // startIndex and ending at the first element in the list. The
        // elements of the list are compared to the given value using the
        // Object.Equals method.
        //
        // This method uses the Array.LastIndexOf method to perform the
        // search.
        //
        public virtual int LastIndexOf( Object value, int startIndex )
        {
            if(startIndex >= m_size)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            return LastIndexOf( value, startIndex, startIndex + 1 );
        }

        // Returns the index of the last occurrence of a given value in a range of
        // this list. The list is searched backwards, starting at index
        // startIndex and upto count elements. The elements of
        // the list are compared to the given value using the Object.Equals
        // method.
        //
        // This method uses the Array.LastIndexOf method to perform the
        // search.
        //
        public virtual int LastIndexOf( Object value, int startIndex, int count )
        {
            if(m_size == 0)
            {
                return -1;
            }

            if(startIndex < 0 || count < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( (startIndex < 0 ? "startIndex" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(startIndex >= m_size || count > startIndex + 1)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( (startIndex >= m_size ? "startIndex" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_BiggerThanCollection" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            return Array.LastIndexOf( (Array)m_items, value, startIndex, count );
        }

////    // Returns a read-only IList wrapper for the given IList.
////    //
////    public static IList ReadOnly( IList list )
////    {
////        if(list == null)
////        {
////            throw new ArgumentNullException( "list" );
////        }
////
////        return new ReadOnlyList( list );
////    }

////    // Returns a read-only ArrayList wrapper for the given ArrayList.
////    //
////    public static ArrayList ReadOnly( ArrayList list )
////    {
////        if(list == null)
////        {
////            throw new ArgumentNullException( "list" );
////        }
////
////        return new ReadOnlyArrayList( list );
////    }

        // Removes the element at the given index. The size of the list is
        // decreased by one.
        //
        public virtual void Remove( Object obj )
        {
            int index = IndexOf( obj );

            BCLDebug.Correctness( index >= 0 || !(obj is Int32), "You passed an Int32 to Remove that wasn't in the ArrayList.\r\nDid you mean RemoveAt?  int: " + obj + "  Count: " + Count );
            if(index >= 0)
            {
                RemoveAt( index );
            }
        }

        // Removes the element at the given index. The size of the list is
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
                Array.Copy( m_items, index + 1, m_items, index, m_size - index );
            }

            m_items[m_size] = null;
            m_version++;
        }

        // Removes a range of elements from this list.
        //
        public virtual void RemoveRange( int index, int count )
        {
            if(index < 0 || count < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(m_size - index < count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                throw new ArgumentException();
#endif
            }

            if(count > 0)
            {
                int i = m_size;
                m_size -= count;

                if(index < m_size)
                {
                    Array.Copy( m_items, index + count, m_items, index, m_size - index );
                }

                while(i > m_size)
                {
                    m_items[--i] = null;
                }

                m_version++;
            }
        }

        // Returns an IList that contains count copies of value.
        //
        public static ArrayList Repeat( Object value, int count )
        {
            if(count < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            ArrayList list = new ArrayList( (count > cDefaultCapacity) ? count : cDefaultCapacity );
            for(int i = 0; i < count; i++)
            {
                list.Add( value );
            }

            return list;
        }

        // Reverses the elements in this list.
        public virtual void Reverse()
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
        public virtual void Reverse( int index, int count )
        {
            if(index < 0 || count < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(m_size - index < count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                throw new ArgumentException();
#endif
            }

            Array.Reverse( m_items, index, count );
            m_version++;
        }

        // Sets the elements starting at the given index to the elements of the
        // given collection.
        //
        public virtual void SetRange( int index, ICollection c )
        {
            if(c == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "c", Environment.GetResourceString( "ArgumentNull_Collection" ) );
#else
                throw new ArgumentNullException();
#endif
            }

            int count = c.Count;
            if(index < 0 || index > m_size - count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(count > 0)
            {
                c.CopyTo( m_items, index );
                m_version++;
            }
        }

////    public virtual ArrayList GetRange( int index, int count )
////    {
////        if(index < 0 || count < 0)
////        {
////            throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////        }
////
////        if(m_size - index < count)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////        }
////
////        return new Range( this, index, count );
////    }

        // Sorts the elements in this list.  Uses the default comparer and
        // Array.Sort.
        public virtual void Sort()
        {
            Sort( 0, Count, Comparer.Default );
        }

        // Sorts the elements in this list.  Uses Array.Sort with the
        // provided comparer.
        public virtual void Sort( IComparer comparer )
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
        public virtual void Sort( int index, int count, IComparer comparer )
        {
            if(index < 0 || count < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(m_size - index < count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                throw new ArgumentException();
#endif
            }

            Array.Sort( m_items, index, count, comparer );
            m_version++;
        }

////    // Returns a thread-safe wrapper around an IList.
////    //
////    [HostProtection( Synchronization = true )]
////    public static IList Synchronized( IList list )
////    {
////        if(list == null)
////        {
////            throw new ArgumentNullException( "list" );
////        }
////
////        return new SyncIList( list );
////    }

////    // Returns a thread-safe wrapper around a ArrayList.
////    //
////    [HostProtection( Synchronization = true )]
////    public static ArrayList Synchronized( ArrayList list )
////    {
////        if(list == null)
////        {
////            throw new ArgumentNullException( "list" );
////        }
////
////        return new SyncArrayList( list );
////    }

        // ToArray returns a new Object array containing the contents of the ArrayList.
        // This requires copying the ArrayList, which is an O(n) operation.
        public virtual Object[] ToArray()
        {
            Object[] array = new Object[m_size];

            Array.Copy( m_items, 0, array, 0, m_size );

            return array;
        }

        // ToArray returns a new array of a particular type containing the contents
        // of the ArrayList.  This requires copying the ArrayList and potentially
        // downcasting all elements.  This copy may fail and is an O(n) operation.
        // Internally, this implementation calls Array.Copy.
        //
        public virtual Array ToArray( Type type )
        {
            if(type == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "type" );
#else
                throw new ArgumentNullException();
#endif
            }

            Array array = Array.CreateInstance( type, m_size );

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
        // list.TrimToSize();
        //
        public virtual void TrimToSize()
        {
            Capacity = m_size;
        }

        #region Commented IListWrapper
////    // This class wraps an IList, exposing it as a ArrayList
////    // Note this requires reimplementing half of ArrayList...
////    [Serializable]
////    private class IListWrapper : ArrayList
////    {
////        private IList m_list;
////
////        internal IListWrapper( IList list )
////        {
////            m_list    = list;
////            m_version = 0; // list doesn't not contain a version number
////        }
////
////        public override int Capacity
////        {
////            get
////            {
////                return m_list.Count;
////
////            }
////
////            set
////            {
////                if(value < m_list.Count)
////                {
////                    throw new ArgumentOutOfRangeException( "value", Environment.GetResourceString( "ArgumentOutOfRange_SmallCapacity" ) );
////                }
////            }
////        }
////
////        public override int Count
////        {
////            get
////            {
////                return m_list.Count;
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
////                return m_list.IsSynchronized;
////            }
////        }
////
////        public override Object this[int index]
////        {
////            get
////            {
////                return m_list[index];
////            }
////
////            set
////            {
////                m_list[index] = value;
////                m_version++;
////            }
////        }
////
////        public override Object SyncRoot
////        {
////            get
////            {
////                return m_list.SyncRoot;
////            }
////        }
////
////        public override int Add( Object obj )
////        {
////            int i = m_list.Add( obj );
////
////            m_version++;
////
////            return i;
////        }
////
////        public override void AddRange( ICollection c )
////        {
////            InsertRange( Count, c );
////        }
////
////        // Other overloads with automatically work
////        public override int BinarySearch( int index, int count, Object value, IComparer comparer )
////        {
////            if(index < 0 || count < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(m_list.Count - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            if(comparer == null)
////            {
////                comparer = Comparer.Default;
////            }
////
////            int lo = index;
////            int hi = index + count - 1;
////            int mid;
////            while(lo <= hi)
////            {
////                mid = (lo + hi) / 2;
////                int r = comparer.Compare( value, m_list[mid] );
////                if(r == 0)
////                {
////                    return mid;
////                }
////
////                if(r < 0)
////                {
////                    hi = mid - 1;
////                }
////                else
////                {
////                    lo = mid + 1;
////                }
////            }
////
////            // return bitwise complement of the first element greater than value.
////            // Since hi is less than lo now, ~lo is the correct item.
////            return ~lo;
////        }
////
////        public override void Clear()
////        {
////            // If _list is an array, it will support Clear method.
////            // We shouldn't allow clear operation on a FixedSized ArrayList
////            if(m_list.IsFixedSize)
////            {
////                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////            }
////
////            m_list.Clear();
////            m_version++;
////        }
////
////        public override Object Clone()
////        {
////            // This does not do a shallow copy of _list into a ArrayList!
////            // This clones the IListWrapper, creating another wrapper class!
////            return new IListWrapper( m_list );
////        }
////
////        public override bool Contains( Object obj )
////        {
////            return m_list.Contains( obj );
////        }
////
////        public override void CopyTo( Array array, int index )
////        {
////            m_list.CopyTo( array, index );
////        }
////
////        public override void CopyTo( int index, Array array, int arrayIndex, int count )
////        {
////            if(array == null)
////            {
////                throw new ArgumentNullException( "array" );
////            }
////
////            if(index < 0 || arrayIndex < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0) ? "index" : "arrayIndex", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(count < 0)
////            {
////                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(array.Length - arrayIndex < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            if(m_list.Count - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            if(array.Rank != 1)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Arg_RankMultiDimNotSupported" ) );
////            }
////
////            for(int i = index; i < index + count; i++)
////            {
////                array.SetValue( m_list[i], arrayIndex++ );
////            }
////        }
////
////        public override IEnumerator GetEnumerator()
////        {
////            return m_list.GetEnumerator();
////        }
////
////        public override IEnumerator GetEnumerator( int index, int count )
////        {
////            if(index < 0 || count < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(m_list.Count - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            return new IListWrapperEnumWrapper( this, index, count );
////        }
////
////        public override int IndexOf( Object value )
////        {
////            return m_list.IndexOf( value );
////        }
////
////        public override int IndexOf( Object value, int startIndex )
////        {
////            return IndexOf( value, startIndex, m_list.Count - startIndex );
////        }
////
////        public override int IndexOf( Object value, int startIndex, int count )
////        {
////            if(startIndex < 0 || startIndex > m_list.Count)
////            {
////                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////            }
////
////            if(count < 0 || startIndex > m_list.Count - count)
////            {
////                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
////            }
////
////            int endIndex = startIndex + count;
////            if(value == null)
////            {
////                for(int i = startIndex; i < endIndex; i++)
////                {
////                    if(m_list[i] == null)
////                    {
////                        return i;
////                    }
////                }
////
////                return -1;
////            }
////            else
////            {
////                for(int i = startIndex; i < endIndex; i++)
////                {
////                    if(m_list[i] != null && m_list[i].Equals( value ))
////                    {
////                        return i;
////                    }
////                }
////
////                return -1;
////            }
////        }
////
////        public override void Insert( int index, Object obj )
////        {
////            m_list.Insert( index, obj );
////            m_version++;
////        }
////
////        public override void InsertRange( int index, ICollection c )
////        {
////            if(c == null)
////            {
////                throw new ArgumentNullException( "c", Environment.GetResourceString( "ArgumentNull_Collection" ) );
////            }
////
////            if(index < 0 || index > m_list.Count)
////            {
////                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////            }
////
////            if(c.Count > 0)
////            {
////                ArrayList al = m_list as ArrayList;
////                if(al != null)
////                {
////                    // We need to special case ArrayList.
////                    // When c is a range of _list, we need to handle this in a special way.
////                    // See ArrayList.InsertRange for details.
////                    al.InsertRange( index, c );
////                }
////                else
////                {
////                    IEnumerator en = c.GetEnumerator();
////                    while(en.MoveNext())
////                    {
////                        m_list.Insert( index++, en.Current );
////                    }
////                }
////
////                m_version++;
////            }
////        }
////
////        public override int LastIndexOf( Object value )
////        {
////            return LastIndexOf( value, m_list.Count - 1, m_list.Count );
////        }
////
////        public override int LastIndexOf( Object value, int startIndex )
////        {
////            return LastIndexOf( value, startIndex, startIndex + 1 );
////        }
////
////        public override int LastIndexOf( Object value, int startIndex, int count )
////        {
////            if(m_list.Count == 0)
////            {
////                return -1;
////            }
////
////            if(startIndex < 0 || startIndex >= m_list.Count)
////            {
////                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////            }
////
////            if(count < 0 || count > startIndex + 1)
////            {
////                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
////            }
////
////            int endIndex = startIndex - count + 1;
////            if(value == null)
////            {
////                for(int i = startIndex; i >= endIndex; i--)
////                {
////                    if(m_list[i] == null)
////                    {
////                        return i;
////                    }
////                }
////
////                return -1;
////            }
////            else
////            {
////                for(int i = startIndex; i >= endIndex; i--)
////                {
////                    if(m_list[i] != null && m_list[i].Equals( value ))
////                    {
////                        return i;
////                    }
////                }
////
////                return -1;
////            }
////        }
////
////        public override void Remove( Object value )
////        {
////            int index = IndexOf( value );
////            if(index >= 0)
////            {
////                RemoveAt( index );
////            }
////        }
////
////        public override void RemoveAt( int index )
////        {
////            m_list.RemoveAt( index );
////            m_version++;
////        }
////
////        public override void RemoveRange( int index, int count )
////        {
////            if(index < 0 || count < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(m_list.Count - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            if(count > 0)    // be consistent with ArrayList
////            {
////                m_version++;
////            }
////
////            while(count > 0)
////            {
////                m_list.RemoveAt( index );
////                count--;
////            }
////        }
////
////        public override void Reverse( int index, int count )
////        {
////            if(index < 0 || count < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(m_list.Count - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            int i = index;
////            int j = index + count - 1;
////            while(i < j)
////            {
////                Object tmp = m_list[i];
////                m_list[i++] = m_list[j];
////                m_list[j--] = tmp;
////            }
////
////            m_version++;
////        }
////
////        public override void SetRange( int index, ICollection c )
////        {
////            if(c == null)
////            {
////                throw new ArgumentNullException( "c", Environment.GetResourceString( "ArgumentNull_Collection" ) );
////            }
////
////            if(index < 0 || index > m_list.Count - c.Count)
////            {
////                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////            }
////
////            if(c.Count > 0)
////            {
////                IEnumerator en = c.GetEnumerator();
////                while(en.MoveNext())
////                {
////                    m_list[index++] = en.Current;
////                }
////
////                m_version++;
////            }
////        }
////
////        public override ArrayList GetRange( int index, int count )
////        {
////            if(index < 0 || count < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(m_list.Count - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            return new Range( this, index, count );
////        }
////
////        public override void Sort( int index, int count, IComparer comparer )
////        {
////            if(index < 0 || count < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(m_list.Count - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            Object[] array = new Object[count];
////            CopyTo( index, array, 0, count );
////
////            Array.Sort( array, 0, count, comparer );
////
////            for(int i = 0; i < count; i++)
////            {
////                m_list[i + index] = array[i];
////            }
////
////            m_version++;
////        }
////
////
////        public override Object[] ToArray()
////        {
////            Object[] array = new Object[Count];
////
////            m_list.CopyTo( array, 0 );
////
////            return array;
////        }
////
////        public override Array ToArray( Type type )
////        {
////            if(type == null)
////            {
////                throw new ArgumentNullException( "type" );
////            }
////
////            Array array = Array.CreateInstance( type, m_list.Count );
////
////            m_list.CopyTo( array, 0 );
////
////            return array;
////        }
////
////        public override void TrimToSize()
////        {
////            // Can't really do much here...
////        }
////
////        // This is the enumerator for an IList that's been wrapped in another
////        // class that implements all of ArrayList's methods.
////        [Serializable]
////        private sealed class IListWrapperEnumWrapper : IEnumerator, ICloneable
////        {
////            private IEnumerator m_en;
////            private int         m_remaining;
////            private int         m_initialStartIndex; // for reset
////            private int         m_initialCount;      // for reset
////            private bool        m_firstCall;         // firstCall to MoveNext
////
////            private IListWrapperEnumWrapper()
////            {
////            }
////
////            internal IListWrapperEnumWrapper( IListWrapper listWrapper, int startIndex, int count )
////            {
////                m_en                = listWrapper.GetEnumerator();
////                m_initialStartIndex = startIndex;
////                m_initialCount      = count;
////
////                while(startIndex-- > 0 && m_en.MoveNext());
////
////                m_remaining = count;
////                m_firstCall = true;
////            }
////
////            public Object Clone()
////            {
////                // We must clone the underlying enumerator, I think.
////                IListWrapperEnumWrapper clone = new IListWrapperEnumWrapper();
////
////                clone.m_en                = (IEnumerator)((ICloneable)m_en).Clone();
////                clone.m_initialStartIndex =                           m_initialStartIndex;
////                clone.m_initialCount      =                           m_initialCount;
////                clone.m_remaining         =                           m_remaining;
////                clone.m_firstCall         =                           m_firstCall;
////
////                return clone;
////            }
////
////            public bool MoveNext()
////            {
////                if(m_firstCall)
////                {
////                    m_firstCall = false;
////                    return m_remaining-- > 0 && m_en.MoveNext();
////                }
////
////                if(m_remaining < 0)
////                {
////                    return false;
////                }
////
////                bool r = m_en.MoveNext();
////
////                return r && m_remaining-- > 0;
////            }
////
////            public Object Current
////            {
////                get
////                {
////                    if(m_firstCall)
////                    {
////                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumNotStarted" ) );
////                    }
////
////                    if(m_remaining < 0)
////                    {
////                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumEnded" ) );
////                    }
////
////                    return m_en.Current;
////                }
////            }
////
////            public void Reset()
////            {
////                m_en.Reset();
////
////                int startIndex = m_initialStartIndex;
////                while(startIndex-- > 0 && m_en.MoveNext());
////
////                m_remaining = m_initialCount;
////                m_firstCall = true;
////            }
////        }
////    }
        #endregion

        #region Commented SyncArrayList
////    [Serializable]
////    private class SyncArrayList : ArrayList
////    {
////        private ArrayList m_list;
////        private Object    m_root;
////
////        internal SyncArrayList( ArrayList list ) : base( false )
////        {
////            m_list = list;
////            m_root = list.SyncRoot;
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
////            set
////            {
////                lock(m_root)
////                {
////                    m_list.Capacity = value;
////                }
////            }
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
////        public override Object this[int index]
////        {
////            get
////            {
////                lock(m_root)
////                {
////                    return m_list[index];
////                }
////            }
////            set
////            {
////                lock(m_root)
////                {
////                    m_list[index] = value;
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
////        public override int Add( Object value )
////        {
////            lock(m_root)
////            {
////                return m_list.Add( value );
////            }
////        }
////
////        public override void AddRange( ICollection c )
////        {
////            lock(m_root)
////            {
////                m_list.AddRange( c );
////            }
////        }
////
////        public override int BinarySearch( Object value )
////        {
////            lock(m_root)
////            {
////                return m_list.BinarySearch( value );
////            }
////        }
////
////        public override int BinarySearch( Object value, IComparer comparer )
////        {
////            lock(m_root)
////            {
////                return m_list.BinarySearch( value, comparer );
////            }
////        }
////
////        public override int BinarySearch( int index, int count, Object value, IComparer comparer )
////        {
////            lock(m_root)
////            {
////                return m_list.BinarySearch( index, count, value, comparer );
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
////                return new SyncArrayList( (ArrayList)m_list.Clone() );
////            }
////        }
////
////        public override bool Contains( Object item )
////        {
////            lock(m_root)
////            {
////                return m_list.Contains( item );
////            }
////        }
////
////        public override void CopyTo( Array array )
////        {
////            lock(m_root)
////            {
////                m_list.CopyTo( array );
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
////        public override void CopyTo( int index, Array array, int arrayIndex, int count )
////        {
////            lock(m_root)
////            {
////                m_list.CopyTo( index, array, arrayIndex, count );
////            }
////        }
////
////        public override IEnumerator GetEnumerator()
////        {
////            lock(m_root)
////            {
////                return m_list.GetEnumerator();
////            }
////        }
////
////        public override IEnumerator GetEnumerator( int index, int count )
////        {
////            lock(m_root)
////            {
////                return m_list.GetEnumerator( index, count );
////            }
////        }
////
////        public override int IndexOf( Object value )
////        {
////            lock(m_root)
////            {
////                return m_list.IndexOf( value );
////            }
////        }
////
////        public override int IndexOf( Object value, int startIndex )
////        {
////            lock(m_root)
////            {
////                return m_list.IndexOf( value, startIndex );
////            }
////        }
////
////        public override int IndexOf( Object value, int startIndex, int count )
////        {
////            lock(m_root)
////            {
////                return m_list.IndexOf( value, startIndex, count );
////            }
////        }
////
////        public override void Insert( int index, Object value )
////        {
////            lock(m_root)
////            {
////                m_list.Insert( index, value );
////            }
////        }
////
////        public override void InsertRange( int index, ICollection c )
////        {
////            lock(m_root)
////            {
////                m_list.InsertRange( index, c );
////            }
////        }
////
////        public override int LastIndexOf( Object value )
////        {
////            lock(m_root)
////            {
////                return m_list.LastIndexOf( value );
////            }
////        }
////
////        public override int LastIndexOf( Object value, int startIndex )
////        {
////            lock(m_root)
////            {
////                return m_list.LastIndexOf( value, startIndex );
////            }
////        }
////
////        public override int LastIndexOf( Object value, int startIndex, int count )
////        {
////            lock(m_root)
////            {
////                return m_list.LastIndexOf( value, startIndex, count );
////            }
////        }
////
////        public override void Remove( Object value )
////        {
////            lock(m_root)
////            {
////                m_list.Remove( value );
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
////        public override void RemoveRange( int index, int count )
////        {
////            lock(m_root)
////            {
////                m_list.RemoveRange( index, count );
////            }
////        }
////
////        public override void Reverse( int index, int count )
////        {
////            lock(m_root)
////            {
////                m_list.Reverse( index, count );
////            }
////        }
////
////        public override void SetRange( int index, ICollection c )
////        {
////            lock(m_root)
////            {
////                m_list.SetRange( index, c );
////            }
////        }
////
////        public override ArrayList GetRange( int index, int count )
////        {
////            lock(m_root)
////            {
////                return m_list.GetRange( index, count );
////            }
////        }
////
////        public override void Sort()
////        {
////            lock(m_root)
////            {
////                m_list.Sort();
////            }
////        }
////
////        public override void Sort( IComparer comparer )
////        {
////            lock(m_root)
////            {
////                m_list.Sort( comparer );
////            }
////        }
////
////        public override void Sort( int index, int count, IComparer comparer )
////        {
////            lock(m_root)
////            {
////                m_list.Sort( index, count, comparer );
////            }
////        }
////
////        public override Object[] ToArray()
////        {
////            lock(m_root)
////            {
////                return m_list.ToArray();
////            }
////        }
////
////        public override Array ToArray( Type type )
////        {
////            lock(m_root)
////            {
////                return m_list.ToArray( type );
////            }
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
        #endregion

        #region Commented SyncIList
////    [Serializable]
////    private class SyncIList : IList
////    {
////        private IList  m_list;
////        private Object m_root;
////
////        internal SyncIList( IList list )
////        {
////            m_list = list;
////            m_root = list.SyncRoot;
////        }
////
////        public virtual int Count
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
////        public virtual bool IsReadOnly
////        {
////            get
////            {
////                return m_list.IsReadOnly;
////            }
////        }
////
////        public virtual bool IsFixedSize
////        {
////            get
////            {
////                return m_list.IsFixedSize;
////            }
////        }
////
////        public virtual bool IsSynchronized
////        {
////            get
////            {
////                return true;
////            }
////        }
////
////        public virtual Object this[int index]
////        {
////            get
////            {
////                lock(m_root)
////                {
////                    return m_list[index];
////                }
////            }
////
////            set
////            {
////                lock(m_root)
////                {
////                    m_list[index] = value;
////                }
////            }
////        }
////
////        public virtual Object SyncRoot
////        {
////            get
////            {
////                return m_root;
////            }
////        }
////
////        public virtual int Add( Object value )
////        {
////            lock(m_root)
////            {
////                return m_list.Add( value );
////            }
////        }
////
////
////        public virtual void Clear()
////        {
////            lock(m_root)
////            {
////                m_list.Clear();
////            }
////        }
////
////        public virtual bool Contains( Object item )
////        {
////            lock(m_root)
////            {
////                return m_list.Contains( item );
////            }
////        }
////
////        public virtual void CopyTo( Array array, int index )
////        {
////            lock(m_root)
////            {
////                m_list.CopyTo( array, index );
////            }
////        }
////
////        public virtual IEnumerator GetEnumerator()
////        {
////            lock(m_root)
////            {
////                return m_list.GetEnumerator();
////            }
////        }
////
////        public virtual int IndexOf( Object value )
////        {
////            lock(m_root)
////            {
////                return m_list.IndexOf( value );
////            }
////        }
////
////        public virtual void Insert( int index, Object value )
////        {
////            lock(m_root)
////            {
////                m_list.Insert( index, value );
////            }
////        }
////
////        public virtual void Remove( Object value )
////        {
////            lock(m_root)
////            {
////                m_list.Remove( value );
////            }
////        }
////
////        public virtual void RemoveAt( int index )
////        {
////            lock(m_root)
////            {
////                m_list.RemoveAt( index );
////            }
////        }
////    }
        #endregion

        #region Commented FixedSizeList
////    [Serializable]
////    private class FixedSizeList : IList
////    {
////        private IList m_list;
////
////        internal FixedSizeList( IList l )
////        {
////            m_list = l;
////        }
////
////        public virtual int Count
////        {
////            get
////            {
////                return m_list.Count;
////            }
////        }
////
////        public virtual bool IsReadOnly
////        {
////            get
////            {
////                return m_list.IsReadOnly;
////            }
////        }
////
////        public virtual bool IsFixedSize
////        {
////            get
////            {
////                return true;
////            }
////        }
////
////        public virtual bool IsSynchronized
////        {
////            get
////            {
////                return m_list.IsSynchronized;
////            }
////        }
////
////        public virtual Object this[int index]
////        {
////            get
////            {
////                return m_list[index];
////            }
////
////            set
////            {
////                m_list[index] = value;
////            }
////        }
////
////        public virtual Object SyncRoot
////        {
////            get
////            {
////                return m_list.SyncRoot;
////            }
////        }
////
////        public virtual int Add( Object obj )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////        }
////
////        public virtual void Clear()
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////        }
////
////        public virtual bool Contains( Object obj )
////        {
////            return m_list.Contains( obj );
////        }
////
////        public virtual void CopyTo( Array array, int index )
////        {
////            m_list.CopyTo( array, index );
////        }
////
////        public virtual IEnumerator GetEnumerator()
////        {
////            return m_list.GetEnumerator();
////        }
////
////        public virtual int IndexOf( Object value )
////        {
////            return m_list.IndexOf( value );
////        }
////
////        public virtual void Insert( int index, Object obj )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////        }
////
////        public virtual void Remove( Object value )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////        }
////
////        public virtual void RemoveAt( int index )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////        }
////    }
        #endregion

        #region Commented FixedSizeArrayList
////    [Serializable]
////    private class FixedSizeArrayList : ArrayList
////    {
////        private ArrayList m_list;
////
////        internal FixedSizeArrayList( ArrayList l )
////        {
////            m_list    = l;
////            m_version = m_list.m_version;
////        }
////
////        public override int Count
////        {
////            get
////            {
////                return m_list.Count;
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
////                return true;
////            }
////        }
////
////        public override bool IsSynchronized
////        {
////            get
////            {
////                return m_list.IsSynchronized;
////            }
////        }
////
////        public override Object this[int index]
////        {
////            get
////            {
////                return m_list[index];
////            }
////
////            set
////            {
////                m_list[index] = value;
////
////                m_version = m_list.m_version;
////            }
////        }
////
////        public override Object SyncRoot
////        {
////            get
////            {
////                return m_list.SyncRoot;
////            }
////        }
////
////        public override int Add( Object obj )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////        }
////
////        public override void AddRange( ICollection c )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////        }
////
////        public override int BinarySearch( int index, int count, Object value, IComparer comparer )
////        {
////            return m_list.BinarySearch( index, count, value, comparer );
////        }
////
////        public override int Capacity
////        {
////            get
////            {
////                return m_list.Capacity;
////            }
////
////            set
////            {
////                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////            }
////        }
////
////        public override void Clear()
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////        }
////
////        public override Object Clone()
////        {
////            FixedSizeArrayList arrayList = new FixedSizeArrayList( m_list );
////
////            arrayList.m_list = (ArrayList)m_list.Clone();
////
////            return arrayList;
////        }
////
////        public override bool Contains( Object obj )
////        {
////            return m_list.Contains( obj );
////        }
////
////        public override void CopyTo( Array array, int index )
////        {
////            m_list.CopyTo( array, index );
////        }
////
////        public override void CopyTo( int index, Array array, int arrayIndex, int count )
////        {
////            m_list.CopyTo( index, array, arrayIndex, count );
////        }
////
////        public override IEnumerator GetEnumerator()
////        {
////            return m_list.GetEnumerator();
////        }
////
////        public override IEnumerator GetEnumerator( int index, int count )
////        {
////            return m_list.GetEnumerator( index, count );
////        }
////
////        public override int IndexOf( Object value )
////        {
////            return m_list.IndexOf( value );
////        }
////
////        public override int IndexOf( Object value, int startIndex )
////        {
////            return m_list.IndexOf( value, startIndex );
////        }
////
////        public override int IndexOf( Object value, int startIndex, int count )
////        {
////            return m_list.IndexOf( value, startIndex, count );
////        }
////
////        public override void Insert( int index, Object obj )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////        }
////
////        public override void InsertRange( int index, ICollection c )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////        }
////
////        public override int LastIndexOf( Object value )
////        {
////            return m_list.LastIndexOf( value );
////        }
////
////        public override int LastIndexOf( Object value, int startIndex )
////        {
////            return m_list.LastIndexOf( value, startIndex );
////        }
////
////        public override int LastIndexOf( Object value, int startIndex, int count )
////        {
////            return m_list.LastIndexOf( value, startIndex, count );
////        }
////
////        public override void Remove( Object value )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////        }
////
////        public override void RemoveAt( int index )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////        }
////
////        public override void RemoveRange( int index, int count )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////        }
////
////        public override void SetRange( int index, ICollection c )
////        {
////            m_list.SetRange( index, c );
////
////            m_version = m_list.m_version;
////        }
////
////        public override ArrayList GetRange( int index, int count )
////        {
////            if(index < 0 || count < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(Count - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            return new Range( this, index, count );
////        }
////
////        public override void Reverse( int index, int count )
////        {
////            m_list.Reverse( index, count );
////
////            m_version = m_list.m_version;
////        }
////
////        public override void Sort( int index, int count, IComparer comparer )
////        {
////            m_list.Sort( index, count, comparer );
////
////            m_version = m_list.m_version;
////        }
////
////        public override Object[] ToArray()
////        {
////            return m_list.ToArray();
////        }
////
////        public override Array ToArray( Type type )
////        {
////            return m_list.ToArray( type );
////        }
////
////        public override void TrimToSize()
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_FixedSizeCollection" ) );
////        }
////    }
        #endregion

        #region Commented ReadOnlyList
////    [Serializable]
////    private class ReadOnlyList : IList
////    {
////        private IList m_list;
////
////        internal ReadOnlyList( IList l )
////        {
////            m_list = l;
////        }
////
////        public virtual int Count
////        {
////            get
////            {
////                return m_list.Count;
////            }
////        }
////
////        public virtual bool IsReadOnly
////        {
////            get
////            {
////                return true;
////            }
////        }
////
////        public virtual bool IsFixedSize
////        {
////            get
////            {
////                return true;
////            }
////        }
////
////        public virtual bool IsSynchronized
////        {
////            get
////            {
////                return m_list.IsSynchronized;
////            }
////        }
////
////        public virtual Object this[int index]
////        {
////            get
////            {
////                return m_list[index];
////            }
////
////            set
////            {
////                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////            }
////        }
////
////        public virtual Object SyncRoot
////        {
////            get
////            {
////                return m_list.SyncRoot;
////            }
////        }
////
////        public virtual int Add( Object obj )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////
////        public virtual void Clear()
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////
////        public virtual bool Contains( Object obj )
////        {
////            return m_list.Contains( obj );
////        }
////
////        public virtual void CopyTo( Array array, int index )
////        {
////            m_list.CopyTo( array, index );
////        }
////
////        public virtual IEnumerator GetEnumerator()
////        {
////            return m_list.GetEnumerator();
////        }
////
////        public virtual int IndexOf( Object value )
////        {
////            return m_list.IndexOf( value );
////        }
////
////        public virtual void Insert( int index, Object obj )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////
////        public virtual void Remove( Object value )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////
////        public virtual void RemoveAt( int index )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////    }
        #endregion

        #region Commented ReadOnlyArrayList
////    [Serializable]
////    private class ReadOnlyArrayList : ArrayList
////    {
////        private ArrayList m_list;
////
////        internal ReadOnlyArrayList( ArrayList l )
////        {
////            m_list = l;
////        }
////
////        public override int Count
////        {
////            get
////            {
////                return m_list.Count;
////            }
////        }
////
////        public override bool IsReadOnly
////        {
////            get
////            {
////                return true;
////            }
////        }
////
////        public override bool IsFixedSize
////        {
////            get
////            {
////                return true;
////            }
////        }
////
////        public override bool IsSynchronized
////        {
////            get
////            {
////                return m_list.IsSynchronized;
////            }
////        }
////
////        public override Object this[int index]
////        {
////            get
////            {
////                return m_list[index];
////            }
////
////            set
////            {
////                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////            }
////        }
////
////        public override Object SyncRoot
////        {
////            get
////            {
////                return m_list.SyncRoot;
////            }
////        }
////
////        public override int Add( Object obj )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////
////        public override void AddRange( ICollection c )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////
////        public override int BinarySearch( int index, int count, Object value, IComparer comparer )
////        {
////            return m_list.BinarySearch( index, count, value, comparer );
////        }
////
////
////        public override int Capacity
////        {
////            get
////            {
////                return m_list.Capacity;
////            }
////
////            set
////            {
////                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////            }
////        }
////
////        public override void Clear()
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////
////        public override Object Clone()
////        {
////            ReadOnlyArrayList arrayList = new ReadOnlyArrayList( m_list );
////
////            arrayList.m_list = (ArrayList)m_list.Clone();
////
////            return arrayList;
////        }
////
////        public override bool Contains( Object obj )
////        {
////            return m_list.Contains( obj );
////        }
////
////        public override void CopyTo( Array array, int index )
////        {
////            m_list.CopyTo( array, index );
////        }
////
////        public override void CopyTo( int index, Array array, int arrayIndex, int count )
////        {
////            m_list.CopyTo( index, array, arrayIndex, count );
////        }
////
////        public override IEnumerator GetEnumerator()
////        {
////            return m_list.GetEnumerator();
////        }
////
////        public override IEnumerator GetEnumerator( int index, int count )
////        {
////            return m_list.GetEnumerator( index, count );
////        }
////
////        public override int IndexOf( Object value )
////        {
////            return m_list.IndexOf( value );
////        }
////
////        public override int IndexOf( Object value, int startIndex )
////        {
////            return m_list.IndexOf( value, startIndex );
////        }
////
////        public override int IndexOf( Object value, int startIndex, int count )
////        {
////            return m_list.IndexOf( value, startIndex, count );
////        }
////
////        public override void Insert( int index, Object obj )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////
////        public override void InsertRange( int index, ICollection c )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////
////        public override int LastIndexOf( Object value )
////        {
////            return m_list.LastIndexOf( value );
////        }
////
////        public override int LastIndexOf( Object value, int startIndex )
////        {
////            return m_list.LastIndexOf( value, startIndex );
////        }
////
////        public override int LastIndexOf( Object value, int startIndex, int count )
////        {
////            return m_list.LastIndexOf( value, startIndex, count );
////        }
////
////        public override void Remove( Object value )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////
////        public override void RemoveAt( int index )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////
////        public override void RemoveRange( int index, int count )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////
////        public override void SetRange( int index, ICollection c )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////
////        public override ArrayList GetRange( int index, int count )
////        {
////            if(index < 0 || count < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(Count - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            return new Range( this, index, count );
////        }
////
////        public override void Reverse( int index, int count )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////
////        public override void Sort( int index, int count, IComparer comparer )
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////
////        public override Object[] ToArray()
////        {
////            return m_list.ToArray();
////        }
////
////        public override Array ToArray( Type type )
////        {
////            return m_list.ToArray( type );
////        }
////
////        public override void TrimToSize()
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ReadOnlyCollection" ) );
////        }
////    }
        #endregion

        #region Commented ArrayListEnumerator
        // Implements an enumerator for a ArrayList. The enumerator uses the
        // internal version number of the list to ensure that no modifications are
        // made to the list while an enumeration is in progress.
        [Serializable]
        private sealed class ArrayListEnumerator : IEnumerator, ICloneable
        {
            private ArrayList m_list;
            private int       m_index;
            private int       m_endIndex;       // Where to stop.
            private int       m_version;
            private Object    m_currentElement;
            private int       m_startIndex;     // Save this for Reset.

            internal ArrayListEnumerator( ArrayList list, int index, int count )
            {
                m_list           = list;
                m_startIndex     = index;
                m_index          = index - 1;
                m_endIndex       = m_index + count;  // last valid index
                m_version        = list.m_version;
                m_currentElement = null;
            }

            public Object Clone()
            {
                return MemberwiseClone();
            }

            public bool MoveNext()
            {
                if(m_version != m_list.m_version)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumFailedVersion" ) );
#else
                    throw new InvalidOperationException();
#endif
                }

                if(m_index < m_endIndex)
                {
                    m_currentElement = m_list[++m_index];
                    return true;
                }
                else
                {
                    m_index = m_endIndex + 1;
                    return false;
                }
            }

            public Object Current
            {
                get
                {
                    if(m_index < m_startIndex)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumNotStarted" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }
                    else if(m_index > m_endIndex)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumEnded" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }

                    return m_currentElement;
                }
            }

            public void Reset()
            {
                if(m_version != m_list.m_version)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumFailedVersion" ) );
#else
                    throw new InvalidOperationException();
#endif
                }

                m_index = m_startIndex - 1;
            }
        }
        #endregion

        #region Commented Range
////    // Implementation of a generic list subrange. An instance of this class
////    // is returned by the default implementation of List.GetRange.
////    [Serializable]
////    private class Range : ArrayList
////    {
////        private ArrayList m_baseList;
////        private int       m_baseIndex;
////        private int       m_baseSize;
////        private int       m_baseVersion;
////
////        internal Range( ArrayList list, int index, int count ) : base( false )
////        {
////            m_baseList    = list;
////            m_baseIndex   = index;
////            m_baseSize    = count;
////            m_baseVersion = list.m_version;
////
////            // we also need to update _version field to make Range of Range work
////            m_version = list.m_version;
////        }
////
////        private void InternalUpdateRange()
////        {
////            if(m_baseVersion != m_baseList.m_version)
////            {
////                throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_UnderlyingArrayListChanged" ) );
////            }
////        }
////
////        private void InternalUpdateVersion()
////        {
////            m_baseVersion++;
////            m_version++;
////        }
////
////        public override int Add( Object value )
////        {
////            InternalUpdateRange();
////
////            m_baseList.Insert( m_baseIndex + m_baseSize, value );
////
////            InternalUpdateVersion();
////            return m_baseSize++;
////        }
////
////        public override void AddRange( ICollection c )
////        {
////            InternalUpdateRange();
////            if(c == null)
////            {
////                throw new ArgumentNullException( "c" );
////            }
////
////            int count = c.Count;
////            if(count > 0)
////            {
////                m_baseList.InsertRange( m_baseIndex + m_baseSize, c );
////                InternalUpdateVersion();
////                m_baseSize += count;
////            }
////        }
////
////        // Other overloads with automatically work
////        public override int BinarySearch( int index, int count, Object value, IComparer comparer )
////        {
////            InternalUpdateRange();
////
////            if(index < 0 || count < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(m_baseSize - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            int i = m_baseList.BinarySearch( m_baseIndex + index, count, value, comparer );
////            if(i >= 0) return i - m_baseIndex;
////            return i + m_baseIndex;
////        }
////
////        public override int Capacity
////        {
////            get
////            {
////                return m_baseList.Capacity;
////            }
////
////            set
////            {
////                if(value < Count)
////                {
////                    throw new ArgumentOutOfRangeException( "value", Environment.GetResourceString( "ArgumentOutOfRange_SmallCapacity" ) );
////                }
////            }
////        }
////
////
////        public override void Clear()
////        {
////            InternalUpdateRange();
////
////            if(m_baseSize != 0)
////            {
////                m_baseList.RemoveRange( m_baseIndex, m_baseSize );
////
////                InternalUpdateVersion();
////                m_baseSize = 0;
////            }
////        }
////
////        public override Object Clone()
////        {
////            InternalUpdateRange();
////
////            Range arrayList = new Range( m_baseList, m_baseIndex, m_baseSize );
////
////            arrayList.m_baseList = (ArrayList)m_baseList.Clone();
////
////            return arrayList;
////        }
////
////        public override bool Contains( Object item )
////        {
////            InternalUpdateRange();
////
////            if(item == null)
////            {
////                for(int i = 0; i < m_baseSize; i++)
////                {
////                    if(m_baseList[m_baseIndex + i] == null)
////                    {
////                        return true;
////                    }
////                }
////
////                return false;
////            }
////            else
////            {
////                for(int i = 0; i < m_baseSize; i++)
////                {
////                    if(m_baseList[m_baseIndex + i] != null && m_baseList[m_baseIndex + i].Equals( item ))
////                    {
////                        return true;
////                    }
////                }
////
////                return false;
////            }
////        }
////
////        public override void CopyTo( Array array, int index )
////        {
////            InternalUpdateRange();
////
////            if(array == null)
////            {
////                throw new ArgumentNullException( "array" );
////            }
////
////            if(array.Rank != 1)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Arg_RankMultiDimNotSupported" ) );
////            }
////
////            if(index < 0)
////            {
////                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(array.Length - index < m_baseSize)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            m_baseList.CopyTo( m_baseIndex, array, index, m_baseSize );
////        }
////
////        public override void CopyTo( int index, Array array, int arrayIndex, int count )
////        {
////            InternalUpdateRange();
////
////            if(array == null)
////            {
////                throw new ArgumentNullException( "array" );
////            }
////
////            if(array.Rank != 1)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Arg_RankMultiDimNotSupported" ) );
////            }
////
////            if(index < 0 || count < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(array.Length - arrayIndex < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            if(m_baseSize - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            m_baseList.CopyTo( m_baseIndex + index, array, arrayIndex, count );
////        }
////
////        public override int Count
////        {
////            get
////            {
////                InternalUpdateRange();
////
////                return m_baseSize;
////            }
////        }
////
////        public override bool IsReadOnly
////        {
////            get
////            {
////                return m_baseList.IsReadOnly;
////            }
////        }
////
////        public override bool IsFixedSize
////        {
////            get
////            {
////                return m_baseList.IsFixedSize;
////            }
////        }
////
////        public override bool IsSynchronized
////        {
////            get
////            {
////                return m_baseList.IsSynchronized;
////            }
////        }
////
////        public override IEnumerator GetEnumerator()
////        {
////            return GetEnumerator( 0, m_baseSize );
////        }
////
////        public override IEnumerator GetEnumerator( int index, int count )
////        {
////            InternalUpdateRange();
////
////            if(index < 0 || count < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(m_baseSize - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            return m_baseList.GetEnumerator( m_baseIndex + index, count );
////        }
////
////        public override ArrayList GetRange( int index, int count )
////        {
////            InternalUpdateRange();
////
////            if(index < 0 || count < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(m_baseSize - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            return new Range( this, index, count );
////        }
////
////        public override Object SyncRoot
////        {
////            get
////            {
////                return m_baseList.SyncRoot;
////            }
////        }
////
////
////        public override int IndexOf( Object value )
////        {
////            InternalUpdateRange();
////
////            int i = m_baseList.IndexOf( value, m_baseIndex, m_baseSize );
////
////            if(i >= 0) return i - m_baseIndex;
////
////            return -1;
////        }
////
////        public override int IndexOf( Object value, int startIndex )
////        {
////            InternalUpdateRange();
////
////            if(startIndex < 0)
////            {
////                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(startIndex > m_baseSize)
////            {
////                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////            }
////
////            int i = m_baseList.IndexOf( value, m_baseIndex + startIndex, m_baseSize - startIndex );
////
////            if(i >= 0) return i - m_baseIndex;
////
////            return -1;
////        }
////
////        public override int IndexOf( Object value, int startIndex, int count )
////        {
////            InternalUpdateRange();
////
////            if(startIndex < 0 || startIndex > m_baseSize)
////            {
////                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////            }
////
////            if(count < 0 || (startIndex > m_baseSize - count))
////            {
////                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
////            }
////
////            int i = m_baseList.IndexOf( value, m_baseIndex + startIndex, count );
////
////            if(i >= 0) return i - m_baseIndex;
////
////            return -1;
////        }
////
////        public override void Insert( int index, Object value )
////        {
////            InternalUpdateRange();
////
////            if(index < 0 || index > m_baseSize)
////            {
////                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////            }
////
////            m_baseList.Insert( m_baseIndex + index, value );
////
////            InternalUpdateVersion();
////
////            m_baseSize++;
////        }
////
////        public override void InsertRange( int index, ICollection c )
////        {
////            InternalUpdateRange();
////
////            if(index < 0 || index > m_baseSize)
////            {
////                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////            }
////
////            if(c == null)
////            {
////                throw new ArgumentNullException( "c" );
////            }
////
////            int count = c.Count;
////            if(count > 0)
////            {
////                m_baseList.InsertRange( m_baseIndex + index, c );
////                m_baseSize += count;
////
////                InternalUpdateVersion();
////            }
////        }
////
////        public override int LastIndexOf( Object value )
////        {
////            InternalUpdateRange();
////
////            int i = m_baseList.LastIndexOf( value, m_baseIndex + m_baseSize - 1, m_baseSize );
////
////            if(i >= 0) return i - m_baseIndex;
////
////            return -1;
////        }
////
////        public override int LastIndexOf( Object value, int startIndex )
////        {
////            return LastIndexOf( value, startIndex, startIndex + 1 );
////        }
////
////        public override int LastIndexOf( Object value, int startIndex, int count )
////        {
////            InternalUpdateRange();
////
////            if(m_baseSize == 0)
////            {
////                return -1;
////            }
////
////            if(startIndex >= m_baseSize)
////            {
////                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////            }
////
////            if(startIndex < 0)
////            {
////                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            int i = m_baseList.LastIndexOf( value, m_baseIndex + startIndex, count );
////
////            if(i >= 0) return i - m_baseIndex;
////
////            return -1;
////        }
////
////        // Don't need to override Remove
////
////        public override void RemoveAt( int index )
////        {
////            InternalUpdateRange();
////
////            if(index < 0 || index >= m_baseSize)
////            {
////                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////            }
////
////            m_baseList.RemoveAt( m_baseIndex + index );
////
////            InternalUpdateVersion();
////            m_baseSize--;
////        }
////
////        public override void RemoveRange( int index, int count )
////        {
////            InternalUpdateRange();
////
////            if(index < 0 || count < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(m_baseSize - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            // No need to call _bastList.RemoveRange if count is 0.
////            // In addition, _baseList won't change the vresion number if count is 0.
////            if(count > 0)
////            {
////                m_baseList.RemoveRange( m_baseIndex + index, count );
////
////                InternalUpdateVersion();
////
////                m_baseSize -= count;
////            }
////        }
////
////        public override void Reverse( int index, int count )
////        {
////            InternalUpdateRange();
////
////            if(index < 0 || count < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(m_baseSize - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            m_baseList.Reverse( m_baseIndex + index, count );
////            InternalUpdateVersion();
////        }
////
////
////        public override void SetRange( int index, ICollection c )
////        {
////            InternalUpdateRange();
////
////            if(index < 0 || index >= m_baseSize)
////            {
////                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////            }
////
////            m_baseList.SetRange( m_baseIndex + index, c );
////            if(c.Count > 0)
////            {
////                InternalUpdateVersion();
////            }
////        }
////
////        public override void Sort( int index, int count, IComparer comparer )
////        {
////            InternalUpdateRange();
////
////            if(index < 0 || count < 0)
////            {
////                throw new ArgumentOutOfRangeException( (index < 0 ? "index" : "count"), Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////            }
////
////            if(m_baseSize - index < count)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
////            }
////
////            m_baseList.Sort( m_baseIndex + index, count, comparer );
////            InternalUpdateVersion();
////        }
////
////        public override Object this[int index]
////        {
////            get
////            {
////                InternalUpdateRange();
////
////                if(index < 0 || index >= m_baseSize)
////                {
////                    throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////                }
////
////                return m_baseList[m_baseIndex + index];
////            }
////            set
////            {
////                InternalUpdateRange();
////
////                if(index < 0 || index >= m_baseSize)
////                {
////                    throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////                }
////
////                m_baseList[m_baseIndex + index] = value;
////
////                InternalUpdateVersion();
////            }
////        }
////
////        public override Object[] ToArray()
////        {
////            InternalUpdateRange();
////
////            Object[] array = new Object[m_baseSize];
////
////            Array.Copy( m_baseList.m_items, m_baseIndex, array, 0, m_baseSize );
////
////            return array;
////        }
////
////        public override Array ToArray( Type type )
////        {
////            InternalUpdateRange();
////
////            if(type == null)
////            {
////                throw new ArgumentNullException( "type" );
////            }
////
////            Array array = Array.CreateInstance( type, m_baseSize );
////
////            m_baseList.CopyTo( m_baseIndex, array, 0, m_baseSize );
////
////            return array;
////        }
////
////        public override void TrimToSize()
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_RangeCollection" ) );
////        }
////    }
        #endregion

        #region Commented ArrayListEnumeratorSimple
        [Serializable]
        private sealed class ArrayListEnumeratorSimple : IEnumerator, ICloneable
        {
            // this object is used to indicate enumeration has not started or has terminated
            static Object dummyObject = new Object();

            private ArrayList m_list;
            private int       m_index;
            private int       m_version;
            private Object    m_currentElement;
            [NonSerialized]
            private bool      m_isArrayList;

            internal ArrayListEnumeratorSimple( ArrayList list )
            {
                m_list           = list;
                m_index          = -1;
                m_version        = list.m_version;
                m_isArrayList    = (list.GetType() == typeof( ArrayList ));
                m_currentElement = dummyObject;
            }

            public Object Clone()
            {
                return MemberwiseClone();
            }

            public bool MoveNext()
            {
                if(m_version != m_list.m_version)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumFailedVersion" ) );
#else
                    throw new InvalidOperationException();
#endif
                }

                if(m_isArrayList)
                {
                    // avoid calling virtual methods if we are operating on ArrayList to improve performance
                    if(m_index < m_list.m_size - 1)
                    {
                        m_currentElement = m_list.m_items[++m_index];
                        return true;
                    }
                    else
                    {
                        m_currentElement = dummyObject;
                        m_index          = m_list.m_size;
                        return false;
                    }
                }
                else
                {
                    if(m_index < m_list.Count - 1)
                    {
                        m_currentElement = m_list[++m_index];
                        return true;
                    }
                    else
                    {
                        m_index          = m_list.Count;
                        m_currentElement = dummyObject;
                        return false;
                    }
                }
            }

            public Object Current
            {
                get
                {
                    object temp = m_currentElement;
                    if(dummyObject == temp)
                    {
                        // check if enumeration has not started or has terminated
                        if(m_index == -1)
                        {
#if EXCEPTION_STRINGS
                            throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumNotStarted" ) );
#else
                            throw new InvalidOperationException();
#endif
                        }
                        else
                        {
#if EXCEPTION_STRINGS
                            throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumEnded" ) );
#else
                            throw new InvalidOperationException();
#endif
                        }
                    }

                    return temp;
                }
            }

            public void Reset()
            {
                if(m_version != m_list.m_version)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumFailedVersion" ) );
#else
                    throw new InvalidOperationException();
#endif
                }

                m_currentElement = dummyObject;
                m_index          = -1;
            }
        }
        #endregion

        #region Commented ArrayListDebugView
////
////    internal class ArrayListDebugView
////    {
////        private ArrayList arrayList;
////
////        public ArrayListDebugView( ArrayList arrayList )
////        {
////            if(arrayList == null)
////            {
////                throw new ArgumentNullException( "arrayList" );
////            }
////
////            this.arrayList = arrayList;
////        }
////
////        [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
////        public Object[] Items
////        {
////            get
////            {
////                return arrayList.ToArray();
////            }
////        }
////    }
        #endregion
    }
}
