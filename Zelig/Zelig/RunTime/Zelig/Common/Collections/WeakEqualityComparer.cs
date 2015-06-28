//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;
    using System.Collections.Generic;

    public class WeakEqualityComparer<T> : IEqualityComparer<T>
    {
        public bool Equals( T x ,
                            T y )
        {
            if(x is ValueType && y is ValueType)
            {
                return Object.Equals( x, y );
            }

            if(x is string && y is string)
            {
                return 0 == string.Compare( x as string, y as string );
            }

            return Object.ReferenceEquals( x, y );
        }

        public int GetHashCode( T obj )
        {
            return obj != null ? obj.GetHashCode() : 0;
        }
    }
}
