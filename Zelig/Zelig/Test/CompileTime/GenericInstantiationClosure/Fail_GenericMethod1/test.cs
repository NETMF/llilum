//
// Copyright (c) 2001 Microsoft Corporation.   All rights reserved.
//

namespace GenericInstantiationClosure.Fail_GenericMethod1
{
    public class Fail_GenericMethod_A<T>
    {
        static public void Compute<U>()
        {
            Fail_GenericMethod_B<U>.Compute< Fail_GenericMethod_A<T> >();
        }
    }

    public class Fail_GenericMethod_B<T>
    {
        static public void Compute<U>()
        {
            Fail_GenericMethod_A<U>.Compute< Fail_GenericMethod_B<T> >();
        }
    }

    public class Bench
    {
        public static void Main()
        {
            Fail_GenericMethod_A<int>.Compute< int >();
        }
    }
}
