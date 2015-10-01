// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  Char
**
**
** Purpose: This is the value class representing a Unicode character
** Char methods until we create this functionality.
**
**
===========================================================*/
namespace System
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;

    [Microsoft.Zelig.Internals.WellKnownType( "System_Char" )]
    [Serializable]
    [StructLayout( LayoutKind.Sequential )]
    public struct Char : IComparable, IConvertible, IComparable<Char>, IEquatable<Char>
    {
        public const char MaxValue = (char)0xFFFF;
        public const char MinValue = (char)0x0000;

        //
        // Member Variables
        //
        internal char m_value;


////    // Unicode category values from Unicode U+0000 ~ U+00FF. Store them in byte[] array to save space.
////    private const byte CTRL        = (byte)UnicodeCategory.Control;
////    private const byte SPACE       = (byte)UnicodeCategory.SpaceSeparator;
////    private const byte DIGIT       = (byte)UnicodeCategory.DecimalDigitNumber;
////    private const byte DIGIT_OTHER = (byte)UnicodeCategory.OtherNumber;
////    private const byte LETTER_UC   = (byte)UnicodeCategory.UppercaseLetter;
////    private const byte LETTER_LC   = (byte)UnicodeCategory.LowercaseLetter;
////    private const byte PUNCT_OPEN  = (byte)UnicodeCategory.OpenPunctuation;
////    private const byte PUNCT_CLOSE = (byte)UnicodeCategory.ClosePunctuation;
////    private const byte PUNCT_DASH  = (byte)UnicodeCategory.DashPunctuation;
////    private const byte PUNCT_CONN  = (byte)UnicodeCategory.ConnectorPunctuation;
////    private const byte PUNCT_IQ    = (byte)UnicodeCategory.InitialQuotePunctuation;
////    private const byte PUNCT_FQ    = (byte)UnicodeCategory.FinalQuotePunctuation;
////    private const byte PUNCT_OTHER = (byte)UnicodeCategory.OtherPunctuation;
////    private const byte CURRENCY    = (byte)UnicodeCategory.CurrencySymbol;
////    private const byte MATH        = (byte)UnicodeCategory.MathSymbol;
////    private const byte MODIFIER    = (byte)UnicodeCategory.ModifierSymbol;
////    private const byte OTHER       = (byte)UnicodeCategory.OtherSymbol;
////
////    private readonly static byte[] categoryForLatin1 =
////    {
////        CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , // 0000 - 0007
////        CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , // 0008 - 000F
////        CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , // 0010 - 0017
////        CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , // 0018 - 001F
////        SPACE      , PUNCT_OTHER, PUNCT_OTHER, PUNCT_OTHER, CURRENCY   , PUNCT_OTHER, PUNCT_OTHER, PUNCT_OTHER, // 0020 - 0027
////        PUNCT_OPEN , PUNCT_CLOSE, PUNCT_OTHER, MATH       , PUNCT_OTHER, PUNCT_DASH , PUNCT_OTHER, PUNCT_OTHER, // 0028 - 002F
////        DIGIT      , DIGIT      , DIGIT      , DIGIT      , DIGIT      , DIGIT      , DIGIT      , DIGIT      , // 0030 - 0037
////        DIGIT      , DIGIT      , PUNCT_OTHER, PUNCT_OTHER, MATH       , MATH       , MATH       , PUNCT_OTHER, // 0038 - 003F
////        PUNCT_OTHER, LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , // 0040 - 0047
////        LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , // 0048 - 004F
////        LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , // 0050 - 0057
////        LETTER_UC  , LETTER_UC  , LETTER_UC  , PUNCT_OPEN , PUNCT_OTHER, PUNCT_CLOSE, MODIFIER   , PUNCT_CONN , // 0058 - 005F
////        MODIFIER   , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , // 0060 - 0067
////        LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , // 0068 - 006F
////        LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , // 0070 - 0077
////        LETTER_LC  , LETTER_LC  , LETTER_LC  , PUNCT_OPEN , MATH       , PUNCT_CLOSE, MATH       , CTRL       , // 0078 - 007F
////        CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , // 0080 - 0087
////        CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , // 0088 - 008F
////        CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , // 0090 - 0097
////        CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , CTRL       , // 0098 - 009F
////        SPACE      , PUNCT_OTHER, CURRENCY   , CURRENCY   , CURRENCY   , CURRENCY   , OTHER      , OTHER      , // 00A0 - 00A7
////        MODIFIER   , OTHER      , LETTER_LC  , PUNCT_IQ   , MATH       , PUNCT_DASH , OTHER      , MODIFIER   , // 00A8 - 00AF
////        OTHER      , MATH       , DIGIT_OTHER, DIGIT_OTHER, MODIFIER   , LETTER_LC  , OTHER      , PUNCT_OTHER, // 00B0 - 00B7
////        MODIFIER   , DIGIT_OTHER, LETTER_LC  , PUNCT_FQ   , DIGIT_OTHER, DIGIT_OTHER, DIGIT_OTHER, PUNCT_OTHER, // 00B8 - 00BF
////        LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , // 00C0 - 00C7
////        LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , // 00C8 - 00CF
////        LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , MATH       , // 00D0 - 00D7
////        LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_UC  , LETTER_LC  , // 00D8 - 00DF
////        LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , // 00E0 - 00E7
////        LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , // 00E8 - 00EF
////        LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , MATH       , // 00F0 - 00F7
////        LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , LETTER_LC  , // 00F8 - 00FF
////    };
    
        // Return true for all characters below or equal U+00ff, which is ASCII + Latin-1 Supplement.
        private static bool IsLatin1( char ch )
        {
            return (ch <= '\x00ff');
        }
    
////    // Return true for all characters below or equal U+007f, which is ASCII.
////    private static bool IsAscii( char ch )
////    {
////        return (ch <= '\x007f');
////    }
////
////    // Return the Unicode category for Unicode character <= 0x00ff.
////    private static UnicodeCategory GetLatin1UnicodeCategory( char ch )
////    {
////
////        BCLDebug.Assert( IsLatin1( ch ), "Char.GetLatin1UnicodeCategory(): ch should be <= 007f" );
////        return (UnicodeCategory)(categoryForLatin1[(int)ch]);
////    }


        //
        // Overriden Instance Methods
        //

        // Calculate a hashcode for a 2 byte Unicode character.
        public override int GetHashCode()
        {
            return (int)m_value | ((int)m_value << 16);
        }

        // Used for comparing two boxed Char objects.
        //
        public override bool Equals( Object obj )
        {
            if(!(obj is Char))
            {
                return false;
            }

            return (m_value == ((Char)obj).m_value);
        }

        public bool Equals( Char obj )
        {
            return m_value == obj;
        }

        // Compares this object to another object, returning an integer that
        // indicates the relationship.
        // Returns a value less than zero if this  object
        // null is considered to be less than any instance.
        // If object is not of type Char, this method throws an ArgumentException.
        //
        public int CompareTo( Object value )
        {
            if(value == null)
            {
                return 1;
            }

            if(!(value is Char))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeChar" ) );
#else
                throw new ArgumentException();
#endif
            }

            return (m_value - ((Char)value).m_value);
        }


        public int CompareTo( Char value )
        {
            return (m_value - value);
        }

        // Overrides System.Object.ToString.
        public override String ToString()
        {
            return Char.ToString( m_value );
        }

        public String ToString( IFormatProvider provider )
        {
            return Char.ToString( m_value );
        }

        //
        // Formatting Methods
        //

        /*===================================ToString===================================
        **This static methods takes a character and returns the String representation of it.
        ==============================================================================*/
        // Provides a string representation of a character.
        public static String ToString( char c )
        {
            return new String( c, 1 );
        }

        public static char Parse( String s )
        {
            if(s == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "s" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(s.Length != 1)
            {
#if EXCEPTION_STRINGS
                throw new FormatException( Environment.GetResourceString( "Format_NeedSingleChar" ) );
#else
                throw new FormatException();
#endif
            }
            return s[0];
        }
    
        public static bool TryParse( String s, out Char result )
        {
            result = '\0';
            if(s == null)
            {
                return false;
            }
            if(s.Length != 1)
            {
                return false;
            }
            result = s[0];
            return true;
        }
    
////    //
////    // Static Methods
////    //
////    /*=================================ISDIGIT======================================
////    **A wrapper for Char.  Returns a boolean indicating whether    **
////    **character c is considered to be a digit.                                    **
////    ==============================================================================*/
////    // Determines whether a character is a digit.
////    public static bool IsDigit( char c )
////    {
////        if(IsLatin1( c ))
////        {
////            return (c >= '0' && c <= '9');
////        }
////        return (CharUnicodeInfo.GetUnicodeCategory( c ) == UnicodeCategory.DecimalDigitNumber);
////    }
////
////
////    /*=================================CheckLetter=====================================
////    ** Check if the specified UnicodeCategory belongs to the letter categories.
////    ==============================================================================*/
////    internal static bool CheckLetter( UnicodeCategory uc )
////    {
////        switch(uc)
////        {
////            case UnicodeCategory.UppercaseLetter:
////            case UnicodeCategory.LowercaseLetter:
////            case UnicodeCategory.TitlecaseLetter:
////            case UnicodeCategory.ModifierLetter :
////            case UnicodeCategory.OtherLetter    :
////                return true;
////        }
////        return false;
////    }
////
////    /*=================================ISLETTER=====================================
////    **A wrapper for Char.  Returns a boolean indicating whether    **
////    **character c is considered to be a letter.                                   **
////    ==============================================================================*/
////    // Determines whether a character is a letter.
////    public static bool IsLetter( char c )
////    {
////        if(IsLatin1( c ))
////        {
////            if(IsAscii( c ))
////            {
////                c |= (char)0x20;
////
////                return ((c >= 'a' && c <= 'z'));
////            }
////
////            return (CheckLetter( GetLatin1UnicodeCategory( c ) ));
////        }
////
////        return (CheckLetter( CharUnicodeInfo.GetUnicodeCategory( c ) ));
////    }
    
        private static bool IsWhiteSpaceLatin1( char c )
        {
    
            // There are characters which belong to UnicodeCategory.Control but are considered as white spaces.
            // We use code point comparisons for these characters here as a temporary fix.
    
            // U+0009 = <control> HORIZONTAL TAB
            // U+000a = <control> LINE FEED
            // U+000b = <control> VERTICAL TAB
            // U+000c = <contorl> FORM FEED
            // U+000d = <control> CARRIAGE RETURN
            // U+0085 = <control> NEXT LINE
            // U+00a0 = NO-BREAK SPACE
            if((c == ' ') || (c >= '\x0009' && c <= '\x000d') || c == '\x00a0' || c == '\x0085')
            {
                return (true);
            }
            return (false);
        }
    
        /*===============================ISWHITESPACE===================================
        **A wrapper for Char.  Returns a boolean indicating whether    **
        **character c is considered to be a whitespace character.                     **
        ==============================================================================*/
        // Determines whether a character is whitespace.
        public static bool IsWhiteSpace( char c )
        {
            if(IsLatin1( c ))
            {
                return (IsWhiteSpaceLatin1( c ));
            }
    
////        return CharUnicodeInfo.IsWhiteSpace( c );
            return false;
        }
    
    
////    /*===================================IsUpper====================================
////    **Arguments: c -- the characater to be checked.
////    **Returns:  True if c is an uppercase character.
////    ==============================================================================*/
////    // Determines whether a character is upper-case.
////    public static bool IsUpper( char c )
////    {
////        if(IsLatin1( c ))
////        {
////            if(IsAscii( c ))
////            {
////                return (c >= 'A' && c <= 'Z');
////            }
////            return (GetLatin1UnicodeCategory( c ) == UnicodeCategory.UppercaseLetter);
////        }
////
////        return (CharUnicodeInfo.GetUnicodeCategory( c ) == UnicodeCategory.UppercaseLetter);
////    }
////
////    /*===================================IsLower====================================
////    **Arguments: c -- the characater to be checked.
////    **Returns:  True if c is an lowercase character.
////    ==============================================================================*/
////    // Determines whether a character is lower-case.
////    public static bool IsLower( char c )
////    {
////        if(IsLatin1( c ))
////        {
////            if(IsAscii( c ))
////            {
////                return (c >= 'a' && c <= 'z');
////            }
////            return (GetLatin1UnicodeCategory( c ) == UnicodeCategory.LowercaseLetter);
////        }
////
////        return (CharUnicodeInfo.GetUnicodeCategory( c ) == UnicodeCategory.LowercaseLetter);
////    }
////
////    internal static bool CheckPunctuation( UnicodeCategory uc )
////    {
////        switch(uc)
////        {
////            case UnicodeCategory.ConnectorPunctuation   :
////            case UnicodeCategory.DashPunctuation        :
////            case UnicodeCategory.OpenPunctuation        :
////            case UnicodeCategory.ClosePunctuation       :
////            case UnicodeCategory.InitialQuotePunctuation:
////            case UnicodeCategory.FinalQuotePunctuation  :
////            case UnicodeCategory.OtherPunctuation       :
////                return true;
////        }
////
////        return false;
////    }
////
////
////    /*================================IsPunctuation=================================
////    **Arguments: c -- the characater to be checked.
////    **Returns:  True if c is an punctuation mark
////    ==============================================================================*/
////    // Determines whether a character is a punctuation mark.
////    public static bool IsPunctuation( char c )
////    {
////        if(IsLatin1( c ))
////        {
////            return (CheckPunctuation( GetLatin1UnicodeCategory( c ) ));
////        }
////
////        return (CheckPunctuation( CharUnicodeInfo.GetUnicodeCategory( c ) ));
////    }
////
////    /*=================================CheckLetterOrDigit=====================================
////    ** Check if the specified UnicodeCategory belongs to the letter or digit categories.
////    ==============================================================================*/
////    internal static bool CheckLetterOrDigit( UnicodeCategory uc )
////    {
////        switch(uc)
////        {
////            case UnicodeCategory.UppercaseLetter   :
////            case UnicodeCategory.LowercaseLetter   :
////            case UnicodeCategory.TitlecaseLetter   :
////            case UnicodeCategory.ModifierLetter    :
////            case UnicodeCategory.OtherLetter       :
////            case UnicodeCategory.DecimalDigitNumber:
////                return true;
////        }
////        return false;
////    }
////
////    // Determines whether a character is a letter or a digit.
////    public static bool IsLetterOrDigit( char c )
////    {
////        if(IsLatin1( c ))
////        {
////            return (CheckLetterOrDigit( GetLatin1UnicodeCategory( c ) ));
////        }
////
////        return (CheckLetterOrDigit( CharUnicodeInfo.GetUnicodeCategory( c ) ));
////    }
////
////    /*===================================ToUpper====================================
////    **
////    ==============================================================================*/
////    // Converts a character to upper-case for the specified culture.
////    // <;<;Not fully implemented>;>;
////    public static char ToUpper( char c, CultureInfo culture )
////    {
////        if(culture == null)
////        {
////            throw new ArgumentNullException( "culture" );
////        }
////
////        return culture.TextInfo.ToUpper( c );
////    }
////
////    /*=================================TOUPPER======================================
////    **A wrapper for Char.toUpperCase.  Converts character c to its **
////    **uppercase equivalent.  If c is already an uppercase character or is not an  **
////    **alphabetic, nothing happens.                                                **
////    ==============================================================================*/
////    // Converts a character to upper-case for the default culture.
////    //
////    public static char ToUpper( char c )
////    {
////        return ToUpper( c, CultureInfo.CurrentCulture );
////    }
////
////    // Converts a character to upper-case for invariant culture.
////    public static char ToUpperInvariant( char c )
////    {
////        return ToUpper( c, CultureInfo.InvariantCulture );
////    }
////
////    /*===================================ToLower====================================
////    **
////    ==============================================================================*/
////    // Converts a character to lower-case for the specified culture.
////    // <;<;Not fully implemented>;>;
////    public static char ToLower( char c, CultureInfo culture )
////    {
////        if(culture == null)
////        {
////            throw new ArgumentNullException( "culture" );
////        }
////
////        return culture.TextInfo.ToLower( c );
////    }
////
////    /*=================================TOLOWER======================================
////    **A wrapper for Char.toLowerCase.  Converts character c to its **
////    **lowercase equivalent.  If c is already a lowercase character or is not an   **
////    **alphabetic, nothing happens.                                                **
////    ==============================================================================*/
////    // Converts a character to lower-case for the default culture.
////    public static char ToLower( char c )
////    {
////        return ToLower( c, CultureInfo.CurrentCulture );
////    }
////
////    // Converts a character to lower-case for invariant culture.
////    public static char ToLowerInvariant( char c )
////    {
////        return ToLower( c, CultureInfo.InvariantCulture );
////    }

        #region IConvertible
    
        public TypeCode GetTypeCode()
        {
            return TypeCode.Char;
        }
    
    
        /// <internalonly/>
        bool IConvertible.ToBoolean( IFormatProvider provider )
        {
#if EXCEPTION_STRINGS
            throw new InvalidCastException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "InvalidCast_FromTo" ), "Char", "Boolean" ) );
#else
            throw new InvalidCastException();
#endif
        }
    
        /// <internalonly/>
        char IConvertible.ToChar( IFormatProvider provider )
        {
            return m_value;
        }
    
        /// <internalonly/>
        sbyte IConvertible.ToSByte( IFormatProvider provider )
        {
            return Convert.ToSByte( m_value );
        }
    
        /// <internalonly/>
        byte IConvertible.ToByte( IFormatProvider provider )
        {
            return Convert.ToByte( m_value );
        }
    
        /// <internalonly/>
        short IConvertible.ToInt16( IFormatProvider provider )
        {
            return Convert.ToInt16( m_value );
        }
    
        /// <internalonly/>
        ushort IConvertible.ToUInt16( IFormatProvider provider )
        {
            return Convert.ToUInt16( m_value );
        }
    
        /// <internalonly/>
        int IConvertible.ToInt32( IFormatProvider provider )
        {
            return Convert.ToInt32( m_value );
        }
    
        /// <internalonly/>
        uint IConvertible.ToUInt32( IFormatProvider provider )
        {
            return Convert.ToUInt32( m_value );
        }
    
        /// <internalonly/>
        long IConvertible.ToInt64( IFormatProvider provider )
        {
            return Convert.ToInt64( m_value );
        }
    
        /// <internalonly/>
        ulong IConvertible.ToUInt64( IFormatProvider provider )
        {
            return Convert.ToUInt64( m_value );
        }
    
        /// <internalonly/>
        float IConvertible.ToSingle( IFormatProvider provider )
        {
#if EXCEPTION_STRINGS
            throw new InvalidCastException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "InvalidCast_FromTo" ), "Char", "Single" ) );
#else
            throw new InvalidCastException();
#endif
        }
    
        /// <internalonly/>
        double IConvertible.ToDouble( IFormatProvider provider )
        {
#if EXCEPTION_STRINGS
            throw new InvalidCastException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "InvalidCast_FromTo" ), "Char", "Double" ) );
#else
            throw new InvalidCastException();
#endif
        }
    
        /// <internalonly/>
        Decimal IConvertible.ToDecimal( IFormatProvider provider )
        {
#if EXCEPTION_STRINGS
            throw new InvalidCastException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "InvalidCast_FromTo" ), "Char", "Decimal" ) );
#else
            throw new InvalidCastException();
#endif
        }
    
        /// <internalonly/>
        DateTime IConvertible.ToDateTime( IFormatProvider provider )
        {
#if EXCEPTION_STRINGS
            throw new InvalidCastException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "InvalidCast_FromTo" ), "Char", "DateTime" ) );
#else
            throw new InvalidCastException();
#endif
        }
    
        /// <internalonly/>
        Object IConvertible.ToType( Type type, IFormatProvider provider )
        {
            return Convert.DefaultToType( (IConvertible)this, type, provider );
        }

        #endregion

////    public static bool IsControl( char c )
////    {
////        if(IsLatin1( c ))
////        {
////            return (GetLatin1UnicodeCategory( c ) == UnicodeCategory.Control);
////        }
////
////        return (CharUnicodeInfo.GetUnicodeCategory( c ) == UnicodeCategory.Control);
////    }
////
////    public static bool IsControl( String s, int index )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        if(((uint)index) >= ((uint)s.Length))
////        {
////            throw new ArgumentOutOfRangeException( "index" );
////        }
////
////        char c = s[index];
////        if(IsLatin1( c ))
////        {
////            return (GetLatin1UnicodeCategory( c ) == UnicodeCategory.Control);
////        }
////
////        return (CharUnicodeInfo.GetUnicodeCategory( s, index ) == UnicodeCategory.Control);
////    }
////
////
////    public static bool IsDigit( String s, int index )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        if(((uint)index) >= ((uint)s.Length))
////        {
////            throw new ArgumentOutOfRangeException( "index" );
////        }
////
////        char c = s[index];
////        if(IsLatin1( c ))
////        {
////            return (c >= '0' && c <= '9');
////        }
////
////        return (CharUnicodeInfo.GetUnicodeCategory( s, index ) == UnicodeCategory.DecimalDigitNumber);
////    }
////
////    public static bool IsLetter( String s, int index )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////        if(((uint)index) >= ((uint)s.Length))
////        {
////            throw new ArgumentOutOfRangeException( "index" );
////        }
////
////        char c = s[index];
////        if(IsLatin1( c ))
////        {
////            if(IsAscii( c ))
////            {
////                c |= (char)0x20;
////                return ((c >= 'a' && c <= 'z'));
////            }
////
////            return (CheckLetter( GetLatin1UnicodeCategory( c ) ));
////        }
////
////        return (CheckLetter( CharUnicodeInfo.GetUnicodeCategory( s, index ) ));
////    }
////
////    public static bool IsLetterOrDigit( String s, int index )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        if(((uint)index) >= ((uint)s.Length))
////        {
////            throw new ArgumentOutOfRangeException( "index" );
////        }
////
////        char c = s[index];
////        if(IsLatin1( c ))
////        {
////            return CheckLetterOrDigit( GetLatin1UnicodeCategory( c ) );
////        }
////
////        return CheckLetterOrDigit( CharUnicodeInfo.GetUnicodeCategory( s, index ) );
////    }
////
////    public static bool IsLower( String s, int index )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        if(((uint)index) >= ((uint)s.Length))
////        {
////            throw new ArgumentOutOfRangeException( "index" );
////        }
////
////        char c = s[index];
////        if(IsLatin1( c ))
////        {
////            if(IsAscii( c ))
////            {
////                return (c >= 'a' && c <= 'z');
////            }
////
////            return (GetLatin1UnicodeCategory( c ) == UnicodeCategory.LowercaseLetter);
////        }
////
////        return (CharUnicodeInfo.GetUnicodeCategory( s, index ) == UnicodeCategory.LowercaseLetter);
////    }
////
////    /*=================================CheckNumber=====================================
////    ** Check if the specified UnicodeCategory belongs to the number categories.
////    ==============================================================================*/
////
////    internal static bool CheckNumber( UnicodeCategory uc )
////    {
////        switch(uc)
////        {
////            case UnicodeCategory.DecimalDigitNumber:
////            case UnicodeCategory.LetterNumber      :
////            case UnicodeCategory.OtherNumber       :
////                return true;
////        }
////        return false;
////    }
////
////    public static bool IsNumber( char c )
////    {
////        if(IsLatin1( c ))
////        {
////            if(IsAscii( c ))
////            {
////                return (c >= '0' && c <= '9');
////            }
////
////            return (CheckNumber( GetLatin1UnicodeCategory( c ) ));
////        }
////
////        return (CheckNumber( CharUnicodeInfo.GetUnicodeCategory( c ) ));
////    }
////
////    public static bool IsNumber( String s, int index )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        if(((uint)index) >= ((uint)s.Length))
////        {
////            throw new ArgumentOutOfRangeException( "index" );
////        }
////
////        char c = s[index];
////        if(IsLatin1( c ))
////        {
////            if(IsAscii( c ))
////            {
////                return (c >= '0' && c <= '9');
////            }
////
////            return (CheckNumber( GetLatin1UnicodeCategory( c ) ));
////        }
////
////        return (CheckNumber( CharUnicodeInfo.GetUnicodeCategory( s, index ) ));
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  IsPunctuation
////    //
////    //  Determines if the given character is a punctuation character.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////    public static bool IsPunctuation( String s, int index )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        if(((uint)index) >= ((uint)s.Length))
////        {
////            throw new ArgumentOutOfRangeException( "index" );
////        }
////
////        char c = s[index];
////        if(IsLatin1( c ))
////        {
////            return (CheckPunctuation( GetLatin1UnicodeCategory( c ) ));
////        }
////
////        return (CheckPunctuation( CharUnicodeInfo.GetUnicodeCategory( s, index ) ));
////    }
////
////
////    /*================================= CheckSeparator ============================
////    ** Check if the specified UnicodeCategory belongs to the seprator categories.
////    ==============================================================================*/
////
////    internal static bool CheckSeparator( UnicodeCategory uc )
////    {
////        switch(uc)
////        {
////            case UnicodeCategory.SpaceSeparator    :
////            case UnicodeCategory.LineSeparator     :
////            case UnicodeCategory.ParagraphSeparator:
////                return true;
////        }
////        return false;
////    }
////
////    private static bool IsSeparatorLatin1( char c )
////    {
////        // U+00a0 = NO-BREAK SPACE
////        // There is no LineSeparator or ParagraphSeparator in Latin 1 range.
////        return (c == '\x0020' || c == '\x00a0');
////    }
////
////    public static bool IsSeparator( char c )
////    {
////        if(IsLatin1( c ))
////        {
////            return (IsSeparatorLatin1( c ));
////        }
////
////        return (CheckSeparator( CharUnicodeInfo.GetUnicodeCategory( c ) ));
////    }
////
////    public static bool IsSeparator( String s, int index )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        if(((uint)index) >= ((uint)s.Length))
////        {
////            throw new ArgumentOutOfRangeException( "index" );
////        }
////
////        char c = s[index];
////        if(IsLatin1( c ))
////        {
////            return (IsSeparatorLatin1( c ));
////        }
////
////        return (CheckSeparator( CharUnicodeInfo.GetUnicodeCategory( s, index ) ));
////    }
    
        public static bool IsSurrogate( char c )
        {
            return (c >= HIGH_SURROGATE_START && c <= LOW_SURROGATE_END);
        }
    
        public static bool IsSurrogate( String s, int index )
        {
            if(s == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "s" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(((uint)index) >= ((uint)s.Length))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index" );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            return (IsSurrogate( s[index] ));
        }
    
////    /*================================= CheckSymbol ============================
////     ** Check if the specified UnicodeCategory belongs to the symbol categories.
////     ==============================================================================*/
////
////    internal static bool CheckSymbol( UnicodeCategory uc )
////    {
////        switch(uc)
////        {
////            case UnicodeCategory.MathSymbol    :
////            case UnicodeCategory.CurrencySymbol:
////            case UnicodeCategory.ModifierSymbol:
////            case UnicodeCategory.OtherSymbol   :
////                return true;
////        }
////        return false;
////    }
////
////    public static bool IsSymbol( char c )
////    {
////        if(IsLatin1( c ))
////        {
////            return (CheckSymbol( GetLatin1UnicodeCategory( c ) ));
////        }
////
////        return (CheckSymbol( CharUnicodeInfo.GetUnicodeCategory( c ) ));
////    }
////
////    public static bool IsSymbol( String s, int index )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        if(((uint)index) >= ((uint)s.Length))
////        {
////            throw new ArgumentOutOfRangeException( "index" );
////        }
////
////        if(IsLatin1( s[index] ))
////        {
////            return (CheckSymbol( GetLatin1UnicodeCategory( s[index] ) ));
////        }
////
////        return (CheckSymbol( CharUnicodeInfo.GetUnicodeCategory( s, index ) ));
////    }
////
////
////    public static bool IsUpper( String s, int index )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        if(((uint)index) >= ((uint)s.Length))
////        {
////            throw new ArgumentOutOfRangeException( "index" );
////        }
////
////        char c = s[index];
////        if(IsLatin1( c ))
////        {
////            if(IsAscii( c ))
////            {
////                return (c >= 'A' && c <= 'Z');
////            }
////
////            return (GetLatin1UnicodeCategory( c ) == UnicodeCategory.UppercaseLetter);
////        }
////
////        return (CharUnicodeInfo.GetUnicodeCategory( s, index ) == UnicodeCategory.UppercaseLetter);
////    }
////
////    public static bool IsWhiteSpace( String s, int index )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        if(((uint)index) >= ((uint)s.Length))
////        {
////            throw new ArgumentOutOfRangeException( "index" );
////        }
////
////        if(IsLatin1( s[index] ))
////        {
////            return IsWhiteSpaceLatin1( s[index] );
////        }
////
////        return CharUnicodeInfo.IsWhiteSpace( s, index );
////    }
////
////    public static UnicodeCategory GetUnicodeCategory( char c )
////    {
////        if(IsLatin1( c ))
////        {
////            return (GetLatin1UnicodeCategory( c ));
////        }
////
////        return CharUnicodeInfo.InternalGetUnicodeCategory( c );
////    }
////
////    public static UnicodeCategory GetUnicodeCategory( String s, int index )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        if(((uint)index) >= ((uint)s.Length))
////        {
////            throw new ArgumentOutOfRangeException( "index" );
////        }
////
////        if(IsLatin1( s[index] ))
////        {
////            return (GetLatin1UnicodeCategory( s[index] ));
////        }
////
////        return CharUnicodeInfo.InternalGetUnicodeCategory( s, index );
////    }
////
////    public static double GetNumericValue( char c )
////    {
////        return CharUnicodeInfo.GetNumericValue( c );
////    }
////
////    public static double GetNumericValue( String s, int index )
////    {
////        if(s == null)
////        {
////            throw new ArgumentNullException( "s" );
////        }
////
////        if(((uint)index) >= ((uint)s.Length))
////        {
////            throw new ArgumentOutOfRangeException( "index" );
////        }
////
////        return CharUnicodeInfo.GetNumericValue( s, index );
////    }
    
    
        /*================================= IsHighSurrogate ============================
         ** Check if a char is a high surrogate.
         ==============================================================================*/
        public static bool IsHighSurrogate( char c )
        {
            return ((c >= CharUnicodeInfo.HIGH_SURROGATE_START) && (c <= CharUnicodeInfo.HIGH_SURROGATE_END));
        }
    
        public static bool IsHighSurrogate( String s, int index )
        {
            if(s == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "s" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(index < 0 || index >= s.Length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index" );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            return (IsHighSurrogate( s[index] ));
        }
    
        /*================================= IsLowSurrogate ============================
         ** Check if a char is a low surrogate.
         ==============================================================================*/
        public static bool IsLowSurrogate( char c )
        {
            return ((c >= CharUnicodeInfo.LOW_SURROGATE_START) && (c <= CharUnicodeInfo.LOW_SURROGATE_END));
        }
    
        public static bool IsLowSurrogate( String s, int index )
        {
            if(s == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "s" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(index < 0 || index >= s.Length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index" );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            return (IsLowSurrogate( s[index] ));
        }
    
        /*================================= IsSurrogatePair ============================
         ** Check if the string specified by the index starts with a surrogate pair.
         ==============================================================================*/
        public static bool IsSurrogatePair( String s, int index )
        {
            if(s == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "s" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(index < 0 || index >= s.Length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index" );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            if(index + 1 < s.Length)
            {
                return (IsSurrogatePair( s[index], s[index + 1] ));
            }
    
            return false;
        }
    
        public static bool IsSurrogatePair( char highSurrogate, char lowSurrogate )
        {
            return ((highSurrogate >= CharUnicodeInfo.HIGH_SURROGATE_START && highSurrogate <= CharUnicodeInfo.HIGH_SURROGATE_END) &&
                    (lowSurrogate  >= CharUnicodeInfo.LOW_SURROGATE_START  && lowSurrogate  <= CharUnicodeInfo.LOW_SURROGATE_END )  );
        }
    
        internal const int UNICODE_PLANE00_END   = 0x00FFFF;
        internal const int UNICODE_PLANE01_START = 0x010000; // The starting codepoint for Unicode plane 1.  Plane 1 contains 0x010000 ~ 0x01ffff.
        internal const int UNICODE_PLANE16_END   = 0x10FFFF; // The end codepoint for Unicode plane 16.  Plane 16 contains 0x100000 ~ 0x10ffff. This is the maximum code point value allowed for Unicode.
    
        internal const int HIGH_SURROGATE_START  = 0x00D800;
        internal const int LOW_SURROGATE_END     = 0x00DFFF;
    
////    /*================================= ConvertFromUtf32 ============================
////     ** Convert an UTF32 value into a surrogate pair.
////     ==============================================================================*/
////
////    public static String ConvertFromUtf32( int utf32 )
////    {
////        // For UTF32 values from U+00D800 ~ U+00DFFF, we should throw.  They
////        // are considered as irregular code unit sequence, but they are not illegal.
////        if((utf32 < 0 || utf32 > UNICODE_PLANE16_END) || (utf32 >= HIGH_SURROGATE_START && utf32 <= LOW_SURROGATE_END))
////        {
////            throw new ArgumentOutOfRangeException( "utf32", Environment.GetResourceString( "ArgumentOutOfRange_InvalidUTF32" ) );
////        }
////
////        if(utf32 < UNICODE_PLANE01_START)
////        {
////            // This is a BMP character.
////            return (Char.ToString( (char)utf32 ));
////        }
////
////        // This is a sumplementary character.  Convert it to a surrogate pair in UTF-16.
////        utf32 -= UNICODE_PLANE01_START;
////        char[] surrogate = new char[2];
////        surrogate[0] = (char)((utf32 / 0x400) + (int)CharUnicodeInfo.HIGH_SURROGATE_START);
////        surrogate[1] = (char)((utf32 % 0x400) + (int)CharUnicodeInfo.LOW_SURROGATE_START );
////        return (new String( surrogate ));
////    }
    
    
        /*=============================ConvertToUtf32===================================
        ** Convert a surrogate pair to UTF32 value
        ==============================================================================*/
    
        public static int ConvertToUtf32( char highSurrogate, char lowSurrogate )
        {
            if(!IsHighSurrogate( highSurrogate ))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "highSurrogate", Environment.GetResourceString( "ArgumentOutOfRange_InvalidHighSurrogate" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            if(!IsLowSurrogate( lowSurrogate ))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "lowSurrogate", Environment.GetResourceString( "ArgumentOutOfRange_InvalidLowSurrogate" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            return (((highSurrogate - CharUnicodeInfo.HIGH_SURROGATE_START) * 0x400) + (lowSurrogate - CharUnicodeInfo.LOW_SURROGATE_START) + UNICODE_PLANE01_START);
        }
    
        /*=============================ConvertToUtf32===================================
        ** Convert a character or a surrogate pair starting at index of the specified string
        ** to UTF32 value.
        ** The char pointed by index should be a surrogate pair or a BMP character.
        ** This method throws if a high-surrogate is not followed by a low surrogate.
        ** This method throws if a low surrogate is seen without preceding a high-surrogate.
        ==============================================================================*/
    
        public static int ConvertToUtf32( String s, int index )
        {
            if(s == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "s" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(index < 0 || index >= s.Length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            // Check if the character at index is a high surrogate.
            int temp1 = (int)s[index] - CharUnicodeInfo.HIGH_SURROGATE_START;
            if(temp1 >= 0 && temp1 <= 0x7ff)
            {
                // Found a surrogate char.
                if(temp1 <= 0x3ff)
                {
                    // Found a high surrogate.
                    if(index < s.Length - 1)
                    {
                        int temp2 = (int)s[index + 1] - CharUnicodeInfo.LOW_SURROGATE_START;
                        if(temp2 >= 0 && temp2 <= 0x3ff)
                        {
                            // Found a low surrogate.
                            return ((temp1 * 0x400) + temp2 + UNICODE_PLANE01_START);
                        }
                        else
                        {
#if EXCEPTION_STRINGS
                            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidHighSurrogate", index ), "s" );
#else
                            throw new ArgumentException();
#endif
                        }
                    }
                    else
                    {
                        // Found a high surrogate at the end of the string.
#if EXCEPTION_STRINGS
                        throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidHighSurrogate", index ), "s" );
#else
                        throw new ArgumentException();
#endif
                    }
                }
                else
                {
                    // Find a low surrogate at the character pointed by index.
#if EXCEPTION_STRINGS
                    throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidLowSurrogate", index ), "s" );
#else
                    throw new ArgumentException();
#endif
                }
            }
    
            // Not a high-surrogate or low-surrogate. Genereate the UTF32 value for the BMP characters.
            return ((int)s[index]);
        }
    }
}
