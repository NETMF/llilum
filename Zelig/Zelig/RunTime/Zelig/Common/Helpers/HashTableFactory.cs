//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;
    using System.Collections;
    using System.Collections.Generic;


    public static class HashTableFactory
    {
        public static GrowOnlyHashTable< TKey, TValue > New< TKey, TValue >()
        {
            return new GrowOnlyHashTable< TKey, TValue >( EqualityComparer< TKey >.Default );
        }

        public static GrowOnlyHashTable< TKey, TValue > NewWithReferenceEquality< TKey, TValue >() where TKey : class 
        {
            return new GrowOnlyHashTable< TKey, TValue >( new ReferenceEqualityComparer< TKey >() );
        }

        public static GrowOnlyHashTable< TKey, TValue > NewWithWeakEquality< TKey, TValue >()
        {
            return new GrowOnlyHashTable< TKey, TValue >( new WeakEqualityComparer< TKey >() );
        }

        public static GrowOnlyHashTable< TKey, TValue > NewWithComparer< TKey, TValue >( IEqualityComparer< TKey > comparer )
        {
            return new GrowOnlyHashTable< TKey, TValue >( comparer );
        }
    }
}
