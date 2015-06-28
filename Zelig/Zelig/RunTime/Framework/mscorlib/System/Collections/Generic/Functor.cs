// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace System
{
    public delegate void Action<T>( T obj );

    public delegate int Comparison<T>( T x, T y );

    public delegate TOutput Converter<TInput, TOutput>( TInput input );

    public delegate bool Predicate<T>( T obj );
}

