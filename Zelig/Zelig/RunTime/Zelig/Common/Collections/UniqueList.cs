//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Collections.Generic
{
    using System;
    using System.Collections.Generic;


    public class UniqueList<T> : List<T> where T : IEquatable< T >
    {
        //
        // Constructor Methods
        //

        public UniqueList() : base()
        {
        }

        public UniqueList( int capacity ) : base( capacity )
        {
        }

        //
        // Helper Methods
        //

        public void AddUnique( T item )
        {
            for(int i = 0; i < this.Count; i++)
            {
                if(this[i].Equals( item ))
                {
                    return;
                }
            }

            Add( item );
        }

        public int UniqueIndexOf( T item )
        {
            for(int i = 0; i < this.Count; i++)
            {
                if(this[i].Equals( item ))
                {
                    return i;
                }
            }

            return -1;
        }

        public bool ContainsUnique( T item )
        {
            for(int i = 0; i < this.Count; i++)
            {
                if(this[i].Equals( item ))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
