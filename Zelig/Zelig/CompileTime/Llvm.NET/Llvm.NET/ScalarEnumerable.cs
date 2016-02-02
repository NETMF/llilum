using System.Collections.Generic;

namespace Llvm.NET
{
    /// <summary>Static utility class for constructing enumerable sequences using single values</summary>
    public static class ScalarEnumerable
    {
        /// <summary>Create an enumerator that provides a single value</summary>
        /// <typeparam name="T">Type of value to enumerate</typeparam>
        /// <param name="scalar">Value for the enumerator to provide</param>
        /// <returns>Enumerable sequence containing a single value</returns>
        public static IEnumerable<T> From<T>( T scalar )
        {
            yield return scalar;
        }

        /// <summary>Creates a new enumerable that prepends a value to an existing enumerable sequence</summary>
        /// <typeparam name="T">Type of values to enumerate</typeparam>
        /// <param name="scalar">Value to prepend to the sequence</param>
        /// <param name="values">Existing enumerable</param>
        /// <returns>New enumerable sequence starting with <paramref name="scalar"/></returns>
        public static IEnumerable<T> Combine<T>( T scalar, IEnumerable<T> values )
        {
            yield return scalar;
            foreach( T value in values )
                yield return value;
        }

        /// <summary>Creates a new enumerable that appends a value to an existing enumerable sequence</summary>
        /// <typeparam name="T">Type of values to enumerate</typeparam>
        /// <param name="values">Existing enumerable</param>
        /// <param name="scalar">Value to append to the existing sequence</param>
        /// <returns>New enumerable sequence ending with <paramref name="scalar"/></returns>
        public static IEnumerable<T> Combine<T>( IEnumerable<T> values, T scalar )
        {
            foreach( T value in values )
                yield return value;

            yield return scalar;
        }
    }
}
