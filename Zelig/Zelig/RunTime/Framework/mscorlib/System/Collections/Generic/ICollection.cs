// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Interface:  ICollection
**
**
** Purpose: Base interface for all generic collections.
**
**
===========================================================*/
namespace System.Collections.Generic
{
    using System;
    using System.Runtime.CompilerServices;

    // Base interface for all collections, defining enumerators, size, and
    // synchronization methods.

    // Note that T[] : IList<T>, and we want to ensure that if you use
    // IList<YourValueType>, we ensure a YourValueType[] can be used
    // without jitting.  Hence the TypeDependencyAttribute on SZArrayHelper.
    // This is a special hack internally though - see VM\compile.cpp.
    // The same attribute is on IEnumerable<T> and ICollection<T>.
    [Microsoft.Zelig.Internals.WellKnownType( "System_Collections_Generic_ICollection_of_T" )]
////[TypeDependencyAttribute( "System.SZArrayHelper" )]
    public interface ICollection<T> : IEnumerable<T>
    {
        // Interfaces are not serialable
        // Number of items in the collections.
        int Count
        {
            get;
        }

        bool IsReadOnly
        {
            get;
        }

        void Add( T item );

        void Clear();

        bool Contains( T item );

        // CopyTo copies a collection into an Array, starting at a particular
        // index into the array.
        //
        void CopyTo( T[] array, int arrayIndex );

        //void CopyTo(int sourceIndex, T[] destinationArray, int destinationIndex, int count);

        bool Remove( T item );
    }
}
