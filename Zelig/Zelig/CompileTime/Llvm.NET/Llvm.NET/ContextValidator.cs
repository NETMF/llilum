using System;

namespace Llvm.NET
{
    /// <summary>Provides validation extensions to the <see cref="Context"/> class</summary>
    /// <remarks>
    /// These are done as extensions deliberately to allow for the possibility that the provided
    /// context may actually be <see langword="null"/>, which wouldn't be possible as member methods.
    /// </remarks>
    public static class ContextValidator
    {
        /// <summary>Throw an <see cref="ArgumentException"/> if the <see cref="Context"/> is <see langword="null"/> or is disposed</summary>
        /// <param name="context"><see cref="Context"/>to test</param>
        /// <param name="name">Argument name</param>
        /// <param name="message">Error message for the exception</param>
        public static Context VerifyAsArg( this Context context, string name, string message )
        {
            Verify( context, name, message, ThrowArgException );
            return context;
        }

        /// <summary>Throw an <see cref="ArgumentException"/> if the <see cref="Context"/> is <see langword="null"/> or is disposed</summary>
        /// <param name="context"><see cref="Context"/>to test</param>
        /// <param name="name">Argument name</param>
        public static Context VerifyAsArg( this Context context, string name ) => VerifyAsArg( context, name, InvalidContextArgMsg );

        /// <summary>Throw an <see cref="InvalidOperationException"/> if the <see cref="Context"/> is <see langword="null"/> or is disposed</summary>
        /// <param name="context"><see cref="Context"/>to test</param>
        /// <param name="message">Error message for the exception</param>
        public static Context VerifyOperation( this Context context, string message )
        {
            return Verify( context, null, message, ThrowInvalidOperationException );
        }

        /// <summary>Throw an <see cref="InvalidOperationException"/> if the <see cref="Context"/> is <see langword="null"/> or is disposed</summary>
        /// <param name="context"><see cref="Context"/>to test</param>
        public static Context VerifyOperation( this Context context ) => VerifyOperation( context, InvalidContextOperationMsg );

        /// <summary>Execute a given action if the <see cref="Context"/> is <see langword="null"/> or is disposed</summary>
        /// <param name="context"><see cref="Context"/>to test</param>
        /// <param name="name">Argument name or <see langword="null"/> if name is not needed by the supplied <paramref name="failAction"/></param>
        /// <param name="message">Error message for the <paramref name="failAction"/></param>
        /// <param name="failAction">Action to perform if the verification fails</param>
        public static Context Verify( this Context context, string name, string message, Action<string, string> failAction )
        {
            if( failAction == null )
                throw new ArgumentNullException( nameof( failAction ) );

            if( context == null || context.IsDisposed )
            {
                failAction( name, message );
            }
            return context;
        }

        private static void ThrowArgException( string name, string message )
        {
            throw new ArgumentException( message, name );
        }

        private static void ThrowInvalidOperationException( string name, string message )
        {
            throw new InvalidOperationException( message );
        }

        private const string InvalidContextArgMsg = "Provided context cannot be null or disposed";
        private const string InvalidContextOperationMsg = "Cannot use a null or disposed Context";
    }
}
