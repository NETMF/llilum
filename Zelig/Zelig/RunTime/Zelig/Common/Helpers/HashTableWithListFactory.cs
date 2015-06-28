//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;
    using System.Collections;
    using System.Collections.Generic;


    public static class HashTableWithListFactory
    {
        public static bool Create< TKey, TValue >(     GrowOnlyHashTable< TKey, List< TValue > > ht  ,
                                                       TKey                                      key ,
                                                   out List< TValue >                            lst )
        {
            if(ht.TryGetValue( key, out lst ) == false)
            {
                lst = new List< TValue >();

                ht[key] = lst;

                return false;
            }

            return true;
        }

        public static List< TValue > Create< TKey, TValue >( GrowOnlyHashTable< TKey, List< TValue > > ht  ,
                                                             TKey                                      key )
        {
            List< TValue > lst;

            Create( ht, key, out lst );

            return lst;
        }

        public static List< TValue > Add< TKey, TValue >( GrowOnlyHashTable< TKey, List< TValue > > ht    ,
                                                          TKey                                      key   ,
                                                          TValue                                    value )
        {
            List< TValue > lst = Create( ht, key );

            lst.Add( value );

            return lst;
        }

        public static List< TValue > AddUnique< TKey, TValue >( GrowOnlyHashTable< TKey, List< TValue > > ht    ,
                                                                TKey                                      key   ,
                                                                TValue                                    value )
        {
            List< TValue > lst = Create( ht, key );

            if(lst.Contains( value ) == false)
            {
                lst.Add( value );
            }

            return lst;
        }
    }
}
