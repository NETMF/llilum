//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;
    using System.Collections;
    using System.Collections.Generic;


    public static class SetFactory
    {
        public static GrowOnlySet< TKey > New< TKey >()
        {
            return new GrowOnlySet< TKey >( EqualityComparer< TKey >.Default );
        }

        public static GrowOnlySet< TKey > NewWithReferenceEquality< TKey >() where TKey : class 
        {
            return new GrowOnlySet< TKey >( new ReferenceEqualityComparer< TKey >() );
        }

        public static GrowOnlySet< TKey > NewWithWeakEquality< TKey >() 
        {
            return new GrowOnlySet< TKey >( new WeakEqualityComparer< TKey >() );
        }

        public static GrowOnlySet< TKey > NewWithComparer< TKey >( IEqualityComparer< TKey > comparer )
        {
            return new GrowOnlySet< TKey >( comparer );
        }
    }
}
