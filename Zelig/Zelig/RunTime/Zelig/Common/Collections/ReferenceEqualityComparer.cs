//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;
    using System.Collections.Generic;

    public class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        public bool Equals( T x ,
                            T y )
        {
            return Object.ReferenceEquals( x, y );
        }

        public int GetHashCode( T obj )
        {
            return obj != null ? obj.GetHashCode() : 0;
        }
    }
}
