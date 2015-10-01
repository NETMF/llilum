using System.Collections.Generic;

namespace Llvm.NET
{
    internal static class ScalarEnumerable
    {
        public static IEnumerable<T> From<T>( T scalar )
        {
            yield return scalar;
        }

        public static IEnumerable<T> Combine<T>( T scalar, IEnumerable<T> values )
        {
            yield return scalar;
            foreach( T value in values )
                yield return value;
        }

        public static IEnumerable<T> Combine<T>( IEnumerable<T> values, T scalar )
        {
            foreach( T value in values )
                yield return value;

            yield return scalar;
        }
    }
}
