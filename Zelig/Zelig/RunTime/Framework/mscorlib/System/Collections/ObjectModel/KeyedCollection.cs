// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace System.Collections.ObjectModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    [Serializable]
////[DebuggerTypeProxy( typeof( Mscorlib_KeyedCollectionDebugView<,> ) )]
////[DebuggerDisplay( "Count = {Count}" )]
    public abstract class KeyedCollection<TKey, TItem> : Collection<TItem>
    {
        const int defaultThreshold = 0;

        IEqualityComparer<TKey> m_comparer;
        Dictionary<TKey, TItem> m_dict;
        int                     m_keyCount;
        int                     m_threshold;

        protected KeyedCollection() : this( null, defaultThreshold )
        {
        }

        protected KeyedCollection( IEqualityComparer<TKey> comparer ) : this( comparer, defaultThreshold )
        {
        }

        protected KeyedCollection( IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold )
        {
            if(comparer == null)
            {
                comparer = EqualityComparer<TKey>.Default;
            }

            if(dictionaryCreationThreshold == -1)
            {
                dictionaryCreationThreshold = int.MaxValue;
            }

            if(dictionaryCreationThreshold < -1)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.dictionaryCreationThreshold, ExceptionResource.ArgumentOutOfRange_InvalidThreshold );
            }

            m_comparer  = comparer;
            m_threshold = dictionaryCreationThreshold;
        }

        public IEqualityComparer<TKey> Comparer
        {
            get
            {
                return m_comparer;
            }
        }

        public TItem this[TKey key]
        {
            get
            {
                if(key == null)
                {
                    ThrowHelper.ThrowArgumentNullException( ExceptionArgument.key );
                }

                if(m_dict != null)
                {
                    return m_dict[key];
                }

                foreach(TItem item in Items)
                {
                    if(m_comparer.Equals( GetKeyForItem( item ), key ))
                    {
                        return item;
                    }
                }

                ThrowHelper.ThrowKeyNotFoundException();

                return default( TItem );
            }
        }

        public bool Contains( TKey key )
        {
            if(key == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.key );
            }

            if(m_dict != null)
            {
                return m_dict.ContainsKey( key );
            }

            if(key != null)
            {
                foreach(TItem item in Items)
                {
                    if(m_comparer.Equals( GetKeyForItem( item ), key ))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool ContainsItem( TItem item )
        {
            TKey key;

            if((m_dict == null) || ((key = GetKeyForItem( item )) == null))
            {
                return Items.Contains( item );
            }

            TItem itemInDict;
            bool  exist = m_dict.TryGetValue( key, out itemInDict );
            if(exist)
            {
                return EqualityComparer<TItem>.Default.Equals( itemInDict, item );
            }

            return false;
        }

        public bool Remove( TKey key )
        {
            if(key == null)
            {
                ThrowHelper.ThrowArgumentNullException( ExceptionArgument.key );
            }

            if(m_dict != null)
            {
                if(m_dict.ContainsKey( key ))
                {
                    return Remove( m_dict[key] );
                }

                return false;
            }

            if(key != null)
            {
                for(int i = 0; i < Items.Count; i++)
                {
                    if(m_comparer.Equals( GetKeyForItem( Items[i] ), key ))
                    {
                        RemoveItem( i );
                        return true;
                    }
                }
            }

            return false;
        }

        protected IDictionary<TKey, TItem> Dictionary
        {
            get
            {
                return m_dict;
            }
        }

        protected void ChangeItemKey( TItem item, TKey newKey )
        {
            // check if the item exists in the collection
            if(!ContainsItem( item ))
            {
                ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_ItemNotExist );
            }

            TKey oldKey = GetKeyForItem( item );
            if(!m_comparer.Equals( oldKey, newKey ))
            {
                if(newKey != null)
                {
                    AddKey( newKey, item );
                }

                if(oldKey != null)
                {
                    RemoveKey( oldKey );
                }
            }
        }

        protected override void ClearItems()
        {
            base.ClearItems();

            if(m_dict != null)
            {
                m_dict.Clear();
            }

            m_keyCount = 0;
        }

        protected abstract TKey GetKeyForItem( TItem item );

        protected override void InsertItem( int index, TItem item )
        {
            TKey key = GetKeyForItem( item );

            if(key != null)
            {
                AddKey( key, item );
            }

            base.InsertItem( index, item );
        }

        protected override void RemoveItem( int index )
        {
            TKey key = GetKeyForItem( Items[index] );
            if(key != null)
            {
                RemoveKey( key );
            }

            base.RemoveItem( index );
        }

        protected override void SetItem( int index, TItem item )
        {
            TKey newKey = GetKeyForItem( item         );
            TKey oldKey = GetKeyForItem( Items[index] );

            if(m_comparer.Equals( oldKey, newKey ))
            {
                if(newKey != null && m_dict != null)
                {
                    m_dict[newKey] = item;
                }
            }
            else
            {
                if(newKey != null)
                {
                    AddKey( newKey, item );
                }

                if(oldKey != null)
                {
                    RemoveKey( oldKey );
                }
            }

            base.SetItem( index, item );
        }

        private void AddKey( TKey key, TItem item )
        {
            if(m_dict != null)
            {
                m_dict.Add( key, item );
            }
            else if(m_keyCount == m_threshold)
            {
                CreateDictionary();

                m_dict.Add( key, item );
            }
            else
            {
                if(Contains( key ))
                {
                    ThrowHelper.ThrowArgumentException( ExceptionResource.Argument_AddingDuplicate );
                }

                m_keyCount++;
            }
        }

        private void CreateDictionary()
        {
            m_dict = new Dictionary<TKey, TItem>( m_comparer );

            foreach(TItem item in Items)
            {
                TKey key = GetKeyForItem( item );
                if(key != null)
                {
                    m_dict.Add( key, item );
                }
            }
        }

        private void RemoveKey( TKey key )
        {
            BCLDebug.Assert( key != null, "key shouldn't be null!" );
            if(m_dict != null)
            {
                m_dict.Remove( key );
            }
            else
            {
                m_keyCount--;
            }
        }
    }
}
