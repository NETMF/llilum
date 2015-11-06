//
// Copyright (c) 2001 Microsoft Corporation.   All rights reserved.
//

namespace GenericInstantiationClosure.Pass
{
    public class Pass_B<U>
    {
    }

    public class Pass_A<T> : Pass_B< Pass_A<T> >
    {
    }

    //--//

    public class Pass_P<T>
    {
    }

    public class Pass_C<U,V> : Pass_P< Pass_D<V,U> >
    {
    }

    public class Pass_D<W,X> : Pass_P< Pass_C<W,X> >
    {
    }

    public class Bench
    {
        public static void Main()
        {
        }
    }
}
