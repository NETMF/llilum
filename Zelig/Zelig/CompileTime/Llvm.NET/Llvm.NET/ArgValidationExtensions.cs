using System;

namespace Llvm.NET
{
    internal static class ArgValidationExtensions
    {
        internal static T VerifyArgNotNull<T>( this T self, string name )
            where T : class
        {
            if( self == null )
                throw new ArgumentNullException( name );

            return self;
        }
    }
}
