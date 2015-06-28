// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
// EncoderExceptionFallback.cs
namespace System.Text
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class EncoderExceptionFallback : EncoderFallback
    {
        // Construction
        public EncoderExceptionFallback()
        {
        }

        public override EncoderFallbackBuffer CreateFallbackBuffer()
        {
            return new EncoderExceptionFallbackBuffer();
        }

        // Maximum number of characters that this instance of this fallback could return
        public override int MaxCharCount
        {
            get
            {
                return 0;
            }
        }

        public override bool Equals( Object value )
        {
            EncoderExceptionFallback that = value as EncoderExceptionFallback;
            if(that != null)
            {
                return (true);
            }
            return (false);
        }

        public override int GetHashCode()
        {
            return 654;
        }
    }


    public sealed class EncoderExceptionFallbackBuffer : EncoderFallbackBuffer
    {
        public override bool Fallback( char charUnknown, int index )
        {
            // Fall back our char
#if EXCEPTION_STRINGS
            throw new EncoderFallbackException( Environment.GetResourceString( "Argument_InvalidCodePageConversionIndex", (int)charUnknown, index ), charUnknown, index );
#else
            throw new EncoderFallbackException();
#endif
        }

        public override bool Fallback( char charUnknownHigh, char charUnknownLow, int index )
        {
            if(!Char.IsHighSurrogate( charUnknownHigh ))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "charUnknownHigh", Environment.GetResourceString( "ArgumentOutOfRange_Range", 0xD800, 0xDBFF ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(!Char.IsLowSurrogate( charUnknownLow ))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "CharUnknownLow", Environment.GetResourceString( "ArgumentOutOfRange_Range", 0xDC00, 0xDFFF ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            int iTemp = Char.ConvertToUtf32( charUnknownHigh, charUnknownLow );

            // Fall back our char
#if EXCEPTION_STRINGS
            throw new EncoderFallbackException( Environment.GetResourceString( "Argument_InvalidCodePageConversionIndex", iTemp, index ), charUnknownHigh, charUnknownLow, index );
#else
            throw new EncoderFallbackException();
#endif
        }

        public override char GetNextChar()
        {
            return (char)0;
        }

        public override bool MovePrevious()
        {
            // Exception fallback doesn't have anywhere to back up to.
            return false;
        }

        // Exceptions are always empty
        public override int Remaining
        {
            get
            {
                return 0;
            }
        }
    }

    [Serializable]
    public sealed class EncoderFallbackException : ArgumentException
    {
        char charUnknown;
        char charUnknownHigh;
        char charUnknownLow;
        int index;

        public EncoderFallbackException() : base( Environment.GetResourceString( "Arg_ArgumentException" ) )
        {
////        SetErrorCode( __HResults.COR_E_ARGUMENT );
        }

        public EncoderFallbackException( String message ) : base( message )
        {
////        SetErrorCode( __HResults.COR_E_ARGUMENT );
        }

        public EncoderFallbackException( String message, Exception innerException ) : base( message, innerException )
        {
////        SetErrorCode( __HResults.COR_E_ARGUMENT );
        }

////    internal EncoderFallbackException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }

        internal EncoderFallbackException( String message, char charUnknown, int index ) : base( message )
        {
            this.charUnknown = charUnknown;
            this.index = index;
        }

        internal EncoderFallbackException( String message, char charUnknownHigh, char charUnknownLow, int index ) : base( message )
        {
            if(!Char.IsHighSurrogate( charUnknownHigh ))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "charUnknownHigh", Environment.GetResourceString( "ArgumentOutOfRange_Range", 0xD800, 0xDBFF ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
            if(!Char.IsLowSurrogate( charUnknownLow ))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "CharUnknownLow", Environment.GetResourceString( "ArgumentOutOfRange_Range", 0xDC00, 0xDFFF ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            this.charUnknownHigh = charUnknownHigh;
            this.charUnknownLow = charUnknownLow;
            this.index = index;
        }

        public char CharUnknown
        {
            get
            {
                return (charUnknown);
            }
        }

        public char CharUnknownHigh
        {
            get
            {
                return (charUnknownHigh);
            }
        }

        public char CharUnknownLow
        {
            get
            {
                return (charUnknownLow);
            }
        }

        public int Index
        {
            get
            {
                return index;
            }
        }

        // Return true if the unknown character is a surrogate pair.
        public bool IsUnknownSurrogate()
        {
            return (this.charUnknownHigh != '\0');
        }
    }
}
