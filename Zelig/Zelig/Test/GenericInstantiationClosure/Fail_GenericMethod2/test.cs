//
// Copyright (c) 2001 Microsoft Corporation.   All rights reserved.
//

namespace GenericInstantiationClosure.Fail_GenericMethod2
{
    public class Fail_GenericMethod_C
    {
        public class Sub<T>
        {
        }

        static public void Compute<U>()
        {
            Compute< Sub<U> >();
        }
    }

    public class Bench
    {
        public static void Main()
        {
            Fail_GenericMethod_C.Compute< int >();
        }
    }
}
