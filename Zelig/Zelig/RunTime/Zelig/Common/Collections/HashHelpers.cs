//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;

    internal static class HashHelpers
    {
        // Table of prime numbers to use as hash table sizes.
        // The entry used for capacity is the smallest prime number in this array
        // that is larger than twice the previous capacity.

        internal static readonly int[] primes =
        {
                  3,       7,      11,      17,
                 23,      29,      37,      47,
                 59,      71,      89,     107,
                131,     163,     197,     239,
                293,     353,     431,     521,
                631,     761,     919,    1103,
               1327,    1597,    1931,    2333,
               2801,    3371,    4049,    4861,
               5839,    7013,    8419,   10103,
              12143,   14591,   17519,   21023,
              25229,   30293,   36353,   43627,
              52361,   62851,   75431,   90523,
             108631,  130363,  156437,  187751,
             225307,  270371,  324449,  389357,
             467237,  560689,  672827,  807403,
             968897, 1162687, 1395263, 1674319,
            2009191, 2411033, 2893249, 3471899,
            4166287, 4999559, 5999471, 7199369
        };

        internal static int GetPrime( int min )
        {
            if(min < 0)
            {
                throw new ArgumentException( "Wrong min value" );
            }

            for(int i = 0; i < primes.Length; i++)
            {
                int prime = primes[i];

                if(prime >= min) return prime;
            }

            //
            // Well, if your hashtable has 7 million entries,
            // using a non-prime number for the bucket size is the least of your problems...
            //
            return min;
        }
    }
}
