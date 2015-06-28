//
// Copyright (c) 2001 Microsoft Corporation.   All rights reserved.
//

namespace GenericInstantiationClosure.Fail_Field
{
    public class Fail_Field_A<T>
    {
        public Fail_Field_B< Fail_Field_A< T > > value = new Fail_Field_B< Fail_Field_A< T > >();
    }

    public class Fail_Field_B<T>
    {
        public Fail_Field_A< Fail_Field_B< T > > value = new Fail_Field_A< Fail_Field_B< T > >();
    }

    public class Bench
    {
        public static void Main()
        {
            Fail_Field_A<int> t = new Fail_Field_A<int>();
        }
    }
}
