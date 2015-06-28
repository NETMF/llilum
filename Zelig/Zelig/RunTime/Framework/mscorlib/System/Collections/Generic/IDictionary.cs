// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Interface:  IDictionary
**
**
** Purpose: Base interface for all generic dictionaries.
**
**
===========================================================*/
namespace System.Collections.Generic
{
    using System;

    // An IDictionary is a possibly unordered set of key-value pairs.
    // Keys can be any non-null object.  Values can be any object.
    // You can look up a value in an IDictionary via the default indexed
    // property, Items.

    public interface IDictionary<TKey, TValue> : ICollection< KeyValuePair<TKey, TValue> >
    {
        // Interfaces are not serializable
        // The Item property provides methods to read and edit entries
        // in the Dictionary.
        TValue this[TKey key]
        {
            get;
            set;
        }

        // Returns a collections of the keys in this dictionary.
        ICollection<TKey> Keys
        {
            get;
        }

        // Returns a collections of the values in this dictionary.
        ICollection<TValue> Values
        {
            get;
        }

        // Returns whether this dictionary contains a particular key.
        //
        bool ContainsKey( TKey key );

        // Adds a key-value pair to the dictionary.
        //
        void Add( TKey key, TValue value );

        // Removes a particular key from the dictionary.
        //
        bool Remove( TKey key );

        bool TryGetValue( TKey key, out TValue value );
    }
}
