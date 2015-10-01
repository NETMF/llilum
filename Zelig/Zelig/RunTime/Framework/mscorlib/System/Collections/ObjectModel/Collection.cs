// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace System.Collections.ObjectModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    [Serializable]
////[DebuggerTypeProxy( typeof( Mscorlib_CollectionDebugView<> ) )]
////[DebuggerDisplay( "Count = {Count}" )]
    public class Collection<T> : IList<T>, IList
    {
        IList<T>       m_items;
        [NonSerialized]
        private Object m_syncRoot;

        public Collection()
        {
            m_items = new List<T>();
        }

        public Collection( IList<T> list )
        {
            if(list == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.list );
            }

            m_items = list;
        }

        public int Count
        {
            get
            {
                return m_items.Count;
            }
        }

        protected IList<T> Items
        {
            get
            {
                return m_items;
            }
        }

        public T this[int index]
        {
            get
            {
                return m_items[index];
            }

            set
            {
                if(m_items.IsReadOnly)
                {
                    ThrowHelper.ThrowNotSupportedException( ExceptionResource.NotSupported_ReadOnlyCollection );
                }

                if(index < 0 || index >= m_items.Count)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }

                SetItem( index, value );
            }
        }

        public void Add( T item )
        {
            if(m_items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException( ExceptionResource.NotSupported_ReadOnlyCollection );
            }

            int index = m_items.Count;

            InsertItem( index, item );
        }

        public void Clear()
        {
            if(m_items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException( ExceptionResource.NotSupported_ReadOnlyCollection );
            }

            ClearItems();
        }

        public void CopyTo( T[] array, int index )
        {
            m_items.CopyTo( array, index );
        }

        public bool Contains( T item )
        {
            return m_items.Contains( item );
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_items.GetEnumerator();
        }

        public int IndexOf( T item )
        {
            return m_items.IndexOf( item );
        }

        public void Insert( int index, T item )
        {
            if(m_items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException( ExceptionResource.NotSupported_ReadOnlyCollection );
            }

            if(index < 0 || index > m_items.Count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_ListInsert );
            }

            InsertItem( index, item );
        }

        public bool Remove( T item )
        {
            if(m_items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException( ExceptionResource.NotSupported_ReadOnlyCollection );
            }

            int index = m_items.IndexOf( item );
            if(index < 0)
            {
                return false;
            }

            RemoveItem( index );
            return true;
        }

        public void RemoveAt( int index )
        {
            if(m_items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException( ExceptionResource.NotSupported_ReadOnlyCollection );
            }

            if(index < 0 || index >= m_items.Count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            RemoveItem( index );
        }

        protected virtual void ClearItems()
        {
            m_items.Clear();
        }

        protected virtual void InsertItem( int index, T item )
        {
            m_items.Insert( index, item );
        }

        protected virtual void RemoveItem( int index )
        {
            m_items.RemoveAt( index );
        }

        protected virtual void SetItem( int index, T item )
        {
            m_items[index] = item;
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return m_items.IsReadOnly;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_items).GetEnumerator();
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
                    ICollection c = m_items as ICollection;
                    if(c != null)
                    {
                        m_syncRoot = c.SyncRoot;
                    }
                    else
                    {
                        System.Threading.Interlocked.CompareExchange( ref m_syncRoot, new Object(), null );
                    }
                }

                return m_syncRoot;
            }
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

            if(index < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum );
            }

            if(array.Length - index < Count)
            {
                ThrowHelper.ThrowArgumentException( ExceptionResource.Arg_ArrayPlusOffTooSmall );
            }

            T[] tArray = array as T[];
            if(tArray != null)
            {
                m_items.CopyTo( tArray, index );
            }
            else
            {
                //
                // Catch the obvious case assignment will fail.
                // We can found all possible problems by doing the check though.
                // For example, if the element type of the Array is derived from T,
                // we can't figure out if we can successfully copy the element beforehand.
                //
                Type targetType = array.GetType().GetElementType();
                Type sourceType = typeof( T );
                if(!(targetType.IsAssignableFrom( sourceType ) || sourceType.IsAssignableFrom( targetType )))
                {
                    ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidArrayType );
                }

                //
                // We can't cast array of value type to object[], so we don't support
                // widening of primitive types here.
                //
                object[] objects = array as object[];
                if(objects == null)
                {
                    ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidArrayType );
                }

                int count = m_items.Count;
                try
                {
                    for(int i = 0; i < count; i++)
                    {
                        objects[index++] = m_items[i];
                    }
                }
                catch(ArrayTypeMismatchException)
                {
                    ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_InvalidArrayType );
                }
            }
        }

        object IList.this[int index]
        {
            get
            {
                return m_items[index];
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

        bool IList.IsReadOnly
        {
            get
            {
                return m_items.IsReadOnly;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                // There is no IList<T>.IsFixedSize, so we must assume that only
                // readonly collections are fixed size, if our internal item
                // collection does not implement IList.  Note that Array implements
                // IList, and therefore T[] and U[] will be fixed-size.
                IList list = m_items as IList;
                if(list != null)
                {
                    return list.IsFixedSize;
                }

                return m_items.IsReadOnly;
            }
        }

        int IList.Add( object value )
        {
            if(m_items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException( ExceptionResource.NotSupported_ReadOnlyCollection );
            }

            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>( value, ExceptionArgument.value );

            try
            {
                Add( (T)value );
            }
            catch(InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException( value, typeof( T ) );
            }

            return this.Count - 1;
        }

        bool IList.Contains( object value )
        {
            if(IsCompatibleObject( value ))
            {
                return Contains( (T)value );
            }

            return false;
        }

        int IList.IndexOf( object value )
        {
            if(IsCompatibleObject( value ))
            {
                return IndexOf( (T)value );
            }

            return -1;
        }

        void IList.Insert( int index, object value )
        {
            if(m_items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException( ExceptionResource.NotSupported_ReadOnlyCollection );
            }

            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>( value, ExceptionArgument.value );

            try
            {
                Insert( index, (T)value );
            }
            catch(InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException( value, typeof( T ) );
            }

        }

        void IList.Remove( object value )
        {
            if(m_items.IsReadOnly)
            {
                ThrowHelper.ThrowNotSupportedException( ExceptionResource.NotSupported_ReadOnlyCollection );
            }

            if(IsCompatibleObject( value ))
            {
                Remove( (T)value );
            }
        }

        private static bool IsCompatibleObject( object value )
        {
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>.
            return ((value is T) || (value == null && default( T ) == null));
        }
    }
}
