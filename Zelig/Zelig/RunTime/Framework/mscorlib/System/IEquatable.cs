// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
namespace System
{
    using System;

    [Microsoft.Zelig.Internals.WellKnownType( "System_IEquatable_of_T" )]
    public interface IEquatable<T>
    {
        bool Equals( T other );
    }
}

