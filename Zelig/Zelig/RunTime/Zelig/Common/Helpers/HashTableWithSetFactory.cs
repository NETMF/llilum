//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;
    using System.Collections;
    using System.Collections.Generic;


    public static class HashTableWithSetFactory
    {
        public static bool Create< TKey, TValue >(     GrowOnlyHashTable< TKey, GrowOnlySet< TValue > > ht  ,
                                                       TKey                                             key ,
                                                   out GrowOnlySet< TValue >                            set )
        {
            if(ht.TryGetValue( key, out set ) == false)
            {
                set = SetFactory.New< TValue >();

                ht[key] = set;

                return false;
            }

            return true;
        }

        public static GrowOnlySet< TValue > Create< TKey, TValue >( GrowOnlyHashTable< TKey, GrowOnlySet< TValue > > ht  ,
                                                                    TKey                                             key )
        {
            GrowOnlySet< TValue > set;

            Create( ht, key, out set );

            return set;
        }

        public static GrowOnlySet< TValue > Add< TKey, TValue >( GrowOnlyHashTable< TKey, GrowOnlySet< TValue > > ht    ,
                                                                 TKey                                             key   ,
                                                                 TValue                                           value )
        {
            GrowOnlySet< TValue > set = Create( ht, key );

            set.Insert( value );

            return set;
        }

        //--//

        public static bool CreateWithReferenceEquality< TKey, TValue >(     GrowOnlyHashTable< TKey, GrowOnlySet< TValue > > ht  ,
                                                                            TKey                                             key ,
                                                                        out GrowOnlySet< TValue >                            set ) where TValue : class
        {
            if(ht.TryGetValue( key, out set ) == false)
            {
                set = SetFactory.NewWithReferenceEquality< TValue >();

                ht[key] = set;

                return false;
            }

            return true;
        }

        public static GrowOnlySet< TValue > CreateWithReferenceEquality< TKey, TValue >( GrowOnlyHashTable< TKey, GrowOnlySet< TValue > > ht  ,
                                                                                         TKey                                             key ) where TValue : class
        {
            GrowOnlySet< TValue > set;

            CreateWithReferenceEquality( ht, key, out set );

            return set;
        }

        public static GrowOnlySet< TValue > AddWithReferenceEquality< TKey, TValue >( GrowOnlyHashTable< TKey, GrowOnlySet< TValue > > ht    ,
                                                                                      TKey                                             key   ,
                                                                                      TValue                                           value ) where TValue : class
        {
            GrowOnlySet< TValue > set = CreateWithReferenceEquality( ht, key );

            set.Insert( value );

            return set;
        }
    }
}
