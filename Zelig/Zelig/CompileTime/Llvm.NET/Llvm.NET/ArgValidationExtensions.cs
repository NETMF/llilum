using System;

namespace Llvm.NET
{
    internal static class ArgValidationExtensions
    {
        /// <summary>Fluent style method for validating a function argument is not null</summary>
        /// <typeparam name="T">Type of the argument</typeparam>
        /// <param name="self">argument value to test for null</param>
        /// <param name="name">name of the argument to appear in the exception</param>
        /// <returns>
        /// <paramref name="self"/> unless it is null, in which case an <see cref="ArgumentNullException"/>
        /// is thrown.
        /// </returns>
        /// <remarks>
        /// This is useful for validating cases where a parameter must be used to compute
        /// an additional parameter to a base class constructor.
        /// </remarks>
        internal static T VerifyArgNotNull<T>( this T self, string name )
            where T : class
        {
            if( self == null )
                throw new ArgumentNullException( name );

            return self;
        }
    }
}
