//
// Copyright (c) 2001 Microsoft Corporation.   All rights reserved.
//

namespace GenericInstantiationClosure.Fail_Method
{
    public class Fail_Method_A<T>
    {
        static public void Compute()
        {
            Fail_Method_B< Fail_Method_A< T > >.Compute();
        }
    }

    public class Fail_Method_B<T>
    {
        static public void Compute()
        {
            Fail_Method_A< Fail_Method_B< T > >.Compute();
        }
    }

    public class Bench
    {
        public static void Main()
        {
            Fail_Method_A< int >.Compute();
        }
    }
}
