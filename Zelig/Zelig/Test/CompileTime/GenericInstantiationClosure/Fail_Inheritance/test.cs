//
// Copyright (c) 2001 Microsoft Corporation.   All rights reserved.
//

namespace GenericInstantiationClosure.Fail_Inheritance
{
    public class Fail_B<U>
    {
    }

    public class Fail_A<T> : Fail_B< Fail_A< Fail_A<T> > >
    {
    }

    public class Bench
    {
        public static void Main()
        {
            Fail_A<int> t = new Fail_A<int>();
        }
    }
}
