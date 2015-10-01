// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System
{

    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    ////using System.Runtime.Versioning;

    // The Number class implements methods for formatting and parsing
    // numeric values. To format and parse numeric values, applications should
    // use the Format and Parse methods provided by the numeric
    // classes (Byte, Int16, Int32, Int64,
    // Single, Double, Currency, and Decimal). Those
    // Format and Parse methods share a common implementation
    // provided by this class, and are thus documented in detail here.
    //
    // Formatting
    //
    // The Format methods provided by the numeric classes are all of the
    // form
    //
    //  public static String Format(XXX value, String format);
    //  public static String Format(XXX value, String format, NumberFormatInfo info);
    //
    // where XXX is the name of the particular numeric class. The methods convert
    // the numeric value to a string using the format string given by the
    // format parameter. If the format parameter is null or
    // an empty string, the number is formatted as if the string "G" (general
    // format) was specified. The info parameter specifies the
    // NumberFormatInfo instance to use when formatting the number. If the
    // info parameter is null or omitted, the numeric formatting information
    // is obtained from the current culture. The NumberFormatInfo supplies
    // such information as the characters to use for decimal and thousand
    // separators, and the spelling and placement of currency symbols in monetary
    // values.
    //
    // Format strings fall into two categories: Standard format strings and
    // user-defined format strings. A format string consisting of a single
    // alphabetic character (A-Z or a-z), optionally followed by a sequence of
    // digits (0-9), is a standard format string. All other format strings are
    // used-defined format strings.
    //
    // A standard format string takes the form Axx, where A is an
    // alphabetic character called the format specifier and xx is a
    // sequence of digits called the precision specifier. The format
    // specifier controls the type of formatting applied to the number and the
    // precision specifier controls the number of significant digits or decimal
    // places of the formatting operation. The following table describes the
    // supported standard formats.
    //
    // C c - Currency format. The number is
    // converted to a string that represents a currency amount. The conversion is
    // controlled by the currency format information of the NumberFormatInfo
    // used to format the number. The precision specifier indicates the desired
    // number of decimal places. If the precision specifier is omitted, the default
    // currency precision given by the NumberFormatInfo is used.
    //
    // D d - Decimal format. This format is
    // supported for integral types only. The number is converted to a string of
    // decimal digits, prefixed by a minus sign if the number is negative. The
    // precision specifier indicates the minimum number of digits desired in the
    // resulting string. If required, the number will be left-padded with zeros to
    // produce the number of digits given by the precision specifier.
    //
    // E e Engineering (scientific) format.
    // The number is converted to a string of the form
    // "-d.ddd...E+ddd" or "-d.ddd...e+ddd", where each
    // 'd' indicates a digit (0-9). The string starts with a minus sign if the
    // number is negative, and one digit always precedes the decimal point. The
    // precision specifier indicates the desired number of digits after the decimal
    // point. If the precision specifier is omitted, a default of 6 digits after
    // the decimal point is used. The format specifier indicates whether to prefix
    // the exponent with an 'E' or an 'e'. The exponent is always consists of a
    // plus or minus sign and three digits.
    //
    // F f Fixed point format. The number is
    // converted to a string of the form "-ddd.ddd....", where each
    // 'd' indicates a digit (0-9). The string starts with a minus sign if the
    // number is negative. The precision specifier indicates the desired number of
    // decimal places. If the precision specifier is omitted, the default numeric
    // precision given by the NumberFormatInfo is used.
    //
    // G g - General format. The number is
    // converted to the shortest possible decimal representation using fixed point
    // or scientific format. The precision specifier determines the number of
    // significant digits in the resulting string. If the precision specifier is
    // omitted, the number of significant digits is determined by the type of the
    // number being converted (10 for int, 19 for long, 7 for
    // float, 15 for double, 19 for Currency, and 29 for
    // Decimal). Trailing zeros after the decimal point are removed, and the
    // resulting string contains a decimal point only if required. The resulting
    // string uses fixed point format if the exponent of the number is less than
    // the number of significant digits and greater than or equal to -4. Otherwise,
    // the resulting string uses scientific format, and the case of the format
    // specifier controls whether the exponent is prefixed with an 'E' or an
    // 'e'.
    //
    // N n Number format. The number is
    // converted to a string of the form "-d,ddd,ddd.ddd....", where
    // each 'd' indicates a digit (0-9). The string starts with a minus sign if the
    // number is negative. Thousand separators are inserted between each group of
    // three digits to the left of the decimal point. The precision specifier
    // indicates the desired number of decimal places. If the precision specifier
    // is omitted, the default numeric precision given by the
    // NumberFormatInfo is used.
    //
    // X x - Hexadecimal format. This format is
    // supported for integral types only. The number is converted to a string of
    // hexadecimal digits. The format specifier indicates whether to use upper or
    // lower case characters for the hexadecimal digits above 9 ('X' for 'ABCDEF',
    // and 'x' for 'abcdef'). The precision specifier indicates the minimum number
    // of digits desired in the resulting string. If required, the number will be
    // left-padded with zeros to produce the number of digits given by the
    // precision specifier.
    //
    // Some examples of standard format strings and their results are shown in the
    // table below. (The examples all assume a default NumberFormatInfo.)
    //
    // Value        Format  Result
    // 12345.6789   C       $12,345.68
    // -12345.6789  C       ($12,345.68)
    // 12345        D       12345
    // 12345        D8      00012345
    // 12345.6789   E       1.234568E+004
    // 12345.6789   E10     1.2345678900E+004
    // 12345.6789   e4      1.2346e+004
    // 12345.6789   F       12345.68
    // 12345.6789   F0      12346
    // 12345.6789   F6      12345.678900
    // 12345.6789   G       12345.6789
    // 12345.6789   G7      12345.68
    // 123456789    G7      1.234568E8
    // 12345.6789   N       12,345.68
    // 123456789    N4      123,456,789.0000
    // 0x2c45e      x       2c45e
    // 0x2c45e      X       2C45E
    // 0x2c45e      X8      0002C45E
    //
    // Format strings that do not start with an alphabetic character, or that start
    // with an alphabetic character followed by a non-digit, are called
    // user-defined format strings. The following table describes the formatting
    // characters that are supported in user defined format strings.
    //
    // 
    // 0 - Digit placeholder. If the value being
    // formatted has a digit in the position where the '0' appears in the format
    // string, then that digit is copied to the output string. Otherwise, a '0' is
    // stored in that position in the output string. The position of the leftmost
    // '0' before the decimal point and the rightmost '0' after the decimal point
    // determines the range of digits that are always present in the output
    // string.
    //
    // # - Digit placeholder. If the value being
    // formatted has a digit in the position where the '#' appears in the format
    // string, then that digit is copied to the output string. Otherwise, nothing
    // is stored in that position in the output string.
    //
    // . - Decimal point. The first '.' character
    // in the format string determines the location of the decimal separator in the
    // formatted value; any additional '.' characters are ignored. The actual
    // character used as a the decimal separator in the output string is given by
    // the NumberFormatInfo used to format the number.
    //
    // , - Thousand separator and number scaling.
    // The ',' character serves two purposes. First, if the format string contains
    // a ',' character between two digit placeholders (0 or #) and to the left of
    // the decimal point if one is present, then the output will have thousand
    // separators inserted between each group of three digits to the left of the
    // decimal separator. The actual character used as a the decimal separator in
    // the output string is given by the NumberFormatInfo used to format the
    // number. Second, if the format string contains one or more ',' characters
    // immediately to the left of the decimal point, or after the last digit
    // placeholder if there is no decimal point, then the number will be divided by
    // 1000 times the number of ',' characters before it is formatted. For example,
    // the format string '0,,' will represent 100 million as just 100. Use of the
    // ',' character to indicate scaling does not also cause the formatted number
    // to have thousand separators. Thus, to scale a number by 1 million and insert
    // thousand separators you would use the format string '#,##0,,'.
    //
    // % - Percentage placeholder. The presence of
    // a '%' character in the format string causes the number to be multiplied by
    // 100 before it is formatted. The '%' character itself is inserted in the
    // output string where it appears in the format string.
    //
    // E+ E- e+ e-   - Scientific notation.
    // If any of the strings 'E+', 'E-', 'e+', or 'e-' are present in the format
    // string and are immediately followed by at least one '0' character, then the
    // number is formatted using scientific notation with an 'E' or 'e' inserted
    // between the number and the exponent. The number of '0' characters following
    // the scientific notation indicator determines the minimum number of digits to
    // output for the exponent. The 'E+' and 'e+' formats indicate that a sign
    // character (plus or minus) should always precede the exponent. The 'E-' and
    // 'e-' formats indicate that a sign character should only precede negative
    // exponents.
    //
    // \ - Literal character. A backslash character
    // causes the next character in the format string to be copied to the output
    // string as-is. The backslash itself isn't copied, so to place a backslash
    // character in the output string, use two backslashes (\\) in the format
    // string.
    //
    // 'ABC' "ABC" - Literal string. Characters
    // enclosed in single or double quotation marks are copied to the output string
    // as-is and do not affect formatting.
    //
    // ; - Section separator. The ';' character is
    // used to separate sections for positive, negative, and zero numbers in the
    // format string.
    //
    // Other - All other characters are copied to
    // the output string in the position they appear.
    //
    // For fixed point formats (formats not containing an 'E+', 'E-', 'e+', or
    // 'e-'), the number is rounded to as many decimal places as there are digit
    // placeholders to the right of the decimal point. If the format string does
    // not contain a decimal point, the number is rounded to the nearest
    // integer. If the number has more digits than there are digit placeholders to
    // the left of the decimal point, the extra digits are copied to the output
    // string immediately before the first digit placeholder.
    //
    // For scientific formats, the number is rounded to as many significant digits
    // as there are digit placeholders in the format string.
    //
    // To allow for different formatting of positive, negative, and zero values, a
    // user-defined format string may contain up to three sections separated by
    // semicolons. The results of having one, two, or three sections in the format
    // string are described in the table below.
    //
    // Sections:
    //
    // One - The format string applies to all values.
    //
    // Two - The first section applies to positive values
    // and zeros, and the second section applies to negative values. If the number
    // to be formatted is negative, but becomes zero after rounding according to
    // the format in the second section, then the resulting zero is formatted
    // according to the first section.
    //
    // Three - The first section applies to positive
    // values, the second section applies to negative values, and the third section
    // applies to zeros. The second section may be left empty (by having no
    // characters between the semicolons), in which case the first section applies
    // to all non-zero values. If the number to be formatted is non-zero, but
    // becomes zero after rounding according to the format in the first or second
    // section, then the resulting zero is formatted according to the third
    // section.
    //
    // For both standard and user-defined formatting operations on values of type
    // float and double, if the value being formatted is a NaN (Not
    // a Number) or a positive or negative infinity, then regardless of the format
    // string, the resulting string is given by the NaNSymbol,
    // PositiveInfinitySymbol, or NegativeInfinitySymbol property of
    // the NumberFormatInfo used to format the number.
    //
    // Parsing
    //
    // The Parse methods provided by the numeric classes are all of the form
    //
    //  public static XXX Parse(String s);
    //  public static XXX Parse(String s, int style);
    //  public static XXX Parse(String s, int style, NumberFormatInfo info);
    //
    // where XXX is the name of the particular numeric class. The methods convert a
    // string to a numeric value. The optional style parameter specifies the
    // permitted style of the numeric string. It must be a combination of bit flags
    // from the NumberStyles enumeration. The optional info parameter
    // specifies the NumberFormatInfo instance to use when parsing the
    // string. If the info parameter is null or omitted, the numeric
    // formatting information is obtained from the current culture.
    //
    // Numeric strings produced by the Format methods using the Currency,
    // Decimal, Engineering, Fixed point, General, or Number standard formats
    // (the C, D, E, F, G, and N format specifiers) are guaranteed to be parseable
    // by the Parse methods if the NumberStyles.Any style is
    // specified. Note, however, that the Parse methods do not accept
    // NaNs or Infinities.
    //
    //This class contains only static members and does not need to be serializable 
    internal class Number
    {
        private int precision;
        private int scale;
        private bool negative;
        private char[] digits = new char[NumberMaxDigits + 1];

        //--//

        // Constants used by number parsing
        private const Int32 NumberMaxDigits = 50;

        private const Int32 Int32Precision  = 10;
        private const Int32 UInt32Precision = Int32Precision;
        private const Int32 Int64Precision  = 19;
        private const Int32 UInt64Precision = 20;
        private const Int32 FloatPrecision  = 7;
        private const Int32 DoublePrecision = 15;

////    // NumberBuffer is a partial wrapper around a stack pointer that maps on to
////    // the native NUMBER struct so that it can be passed to native directly. It 
////    // must be initialized with a stack Byte * of size NumberBufferBytes.
////    // For performance, this structure should attempt to be completely inlined.
////    // 
////    // It should always be initialized like so:
////    //
////    // Byte * numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
////    // NumberBuffer number = new NumberBuffer(numberBufferBytes);
////    //
////    // For performance, when working on the buffer in managed we use the values in this
////    // structure, except for the digits, and pack those values into the byte buffer
////    // if called out to managed.
////    unsafe struct NumberBuffer
////    {
////
////        // Enough space for NumberMaxDigit characters plus null and 3 32 bit integers
////        public const Int32 NumberBufferBytes = 12 + ((NumberMaxDigits + 1) * 2);
////        private Byte* baseAddress;
////        public Char* digits;
////        public Int32 precision;
////        public Int32 scale;
////        public Boolean sign;
////
////        public NumberBuffer( Byte* stackBuffer )
////        {
////            this.baseAddress = stackBuffer;
////            this.digits = (((Char*)stackBuffer) + 6);
////            this.precision = 0;
////            this.scale = 0;
////            this.sign = false;
////        }
////
////        public Byte* PackForNative()
////        {
////            Int32* baseInteger = (Int32*)baseAddress;
////            baseInteger[0] = precision;
////            baseInteger[1] = scale;
////            baseInteger[2] = sign ? 1 : 0;
////            return baseAddress;
////        }
////    }
////
////    private static Boolean HexNumberToInt32( ref NumberBuffer number, ref Int32 value )
////    {
////        UInt32 passedValue = 0;
////        Boolean returnValue = HexNumberToUInt32( ref number, ref passedValue );
////        value = (Int32)passedValue;
////        return returnValue;
////    }
////
////    private static Boolean HexNumberToInt64( ref NumberBuffer number, ref Int64 value )
////    {
////        UInt64 passedValue = 0;
////        Boolean returnValue = HexNumberToUInt64( ref number, ref passedValue );
////        value = (Int64)passedValue;
////        return returnValue;
////    }
////
////    private unsafe static Boolean HexNumberToUInt32( ref NumberBuffer number, ref UInt32 value )
////    {
////
////        Int32 i = number.scale;
////        if(i > UInt32Precision || i < number.precision)
////        {
////            return false;
////        }
////        Char* p = number.digits;
////        BCLDebug.Assert( p != null, "" );
////
////        UInt32 n = 0;
////        while(--i >= 0)
////        {
////            if(n > ((UInt32)0xFFFFFFFF / 16))
////            {
////                return false;
////            }
////            n *= 16;
////            if(*p != '\0')
////            {
////                UInt32 newN = n;
////                if(*p != '\0')
////                {
////                    if(*p >= '0' && *p <= '9')
////                    {
////                        newN += (UInt32)(*p - '0');
////                    }
////                    else
////                    {
////                        if(*p >= 'A' && *p <= 'F')
////                        {
////                            newN += (UInt32)((*p - 'A') + 10);
////                        }
////                        else
////                        {
////                            BCLDebug.Assert( *p >= 'a' && *p <= 'f', "" );
////                            newN += (UInt32)((*p - 'a') + 10);
////                        }
////                    }
////                    p++;
////                }
////
////                // Detect an overflow here...
////                if(newN < n)
////                {
////                    return false;
////                }
////                n = newN;
////            }
////        }
////        value = n;
////        return true;
////    }
////
////    private unsafe static Boolean HexNumberToUInt64( ref NumberBuffer number, ref UInt64 value )
////    {
////
////        Int32 i = number.scale;
////        if(i > UInt64Precision || i < number.precision)
////        {
////            return false;
////        }
////        Char* p = number.digits;
////        BCLDebug.Assert( p != null, "" );
////
////        UInt64 n = 0;
////        while(--i >= 0)
////        {
////            if(n > (0xFFFFFFFFFFFFFFFF / 16))
////            {
////                return false;
////            }
////            n *= 16;
////            if(*p != '\0')
////            {
////                UInt64 newN = n;
////                if(*p != '\0')
////                {
////                    if(*p >= '0' && *p <= '9')
////                    {
////                        newN += (UInt64)(*p - '0');
////                    }
////                    else
////                    {
////                        if(*p >= 'A' && *p <= 'F')
////                        {
////                            newN += (UInt64)((*p - 'A') + 10);
////                        }
////                        else
////                        {
////                            BCLDebug.Assert( *p >= 'a' && *p <= 'f', "" );
////                            newN += (UInt64)((*p - 'a') + 10);
////                        }
////                    }
////                    p++;
////                }
////
////                // Detect an overflow here...
////                if(newN < n)
////                {
////                    return false;
////                }
////                n = newN;
////            }
////        }
////        value = n;
////        return true;
////    }
    
        private static Boolean IsWhite( char ch )
        {
            return (((ch) == 0x20) || ((ch) >= 0x09 && (ch) <= 0x0D));
        }
    
////    private unsafe static Boolean NumberToInt32( ref NumberBuffer number, ref Int32 value )
////    {
////
////        Int32 i = number.scale;
////        if(i > Int32Precision || i < number.precision)
////        {
////            return false;
////        }
////        char* p = number.digits;
////        BCLDebug.Assert( p != null, "" );
////        Int32 n = 0;
////        while(--i >= 0)
////        {
////            if((UInt32)n > (0x7FFFFFFF / 10))
////            {
////                return false;
////            }
////            n *= 10;
////            if(*p != '\0')
////            {
////                n += (Int32)(*p++ - '0');
////            }
////        }
////        if(number.sign)
////        {
////            n = -n;
////            if(n > 0)
////            {
////                return false;
////            }
////        }
////        else
////        {
////            if(n < 0)
////            {
////                return false;
////            }
////        }
////        value = n;
////        return true;
////    }
////
////    private unsafe static Boolean NumberToInt64( ref NumberBuffer number, ref Int64 value )
////    {
////
////        Int32 i = number.scale;
////        if(i > Int64Precision || i < number.precision)
////        {
////            return false;
////        }
////        char* p = number.digits;
////        BCLDebug.Assert( p != null, "" );
////        Int64 n = 0;
////        while(--i >= 0)
////        {
////            if((UInt64)n > (0x7FFFFFFFFFFFFFFF / 10))
////            {
////                return false;
////            }
////            n *= 10;
////            if(*p != '\0')
////            {
////                n += (Int32)(*p++ - '0');
////            }
////        }
////        if(number.sign)
////        {
////            n = -n;
////            if(n > 0)
////            {
////                return false;
////            }
////        }
////        else
////        {
////            if(n < 0)
////            {
////                return false;
////            }
////        }
////        value = n;
////        return true;
////    }
////
////    private unsafe static Boolean NumberToUInt32( ref NumberBuffer number, ref UInt32 value )
////    {
////
////        Int32 i = number.scale;
////        if(i > UInt32Precision || i < number.precision || number.sign)
////        {
////            return false;
////        }
////        char* p = number.digits;
////        BCLDebug.Assert( p != null, "" );
////        UInt32 n = 0;
////        while(--i >= 0)
////        {
////            if(n > (0xFFFFFFFF / 10))
////            {
////                return false;
////            }
////            n *= 10;
////            if(*p != '\0')
////            {
////                UInt32 newN = n + (UInt32)(*p++ - '0');
////                // Detect an overflow here...
////                if(newN < n)
////                {
////                    return false;
////                }
////                n = newN;
////            }
////        }
////        value = n;
////        return true;
////    }
////
////    private unsafe static Boolean NumberToUInt64( ref NumberBuffer number, ref UInt64 value )
////    {
////
////        Int32 i = number.scale;
////        if(i > UInt64Precision || i < number.precision || number.sign)
////        {
////            return false;
////        }
////        char* p = number.digits;
////        BCLDebug.Assert( p != null, "" );
////        UInt64 n = 0;
////        while(--i >= 0)
////        {
////            if(n > (0xFFFFFFFFFFFFFFFF / 10))
////            {
////                return false;
////            }
////            n *= 10;
////            if(*p != '\0')
////            {
////                UInt64 newN = n + (UInt64)(*p++ - '0');
////                // Detect an overflow here...
////                if(newN < n)
////                {
////                    return false;
////                }
////                n = newN;
////            }
////        }
////        value = n;
////        return true;
////    }
////
////    private unsafe static char* MatchChars( char* p, string str )
////    {
////        fixed(char* stringPointer = str)
////        {
////            return MatchChars( p, stringPointer );
////        }
////    }
////    private unsafe static char* MatchChars( char* p, char* str )
////    {
////        BCLDebug.Assert( p != null && str != null, "" );
////
////        if(*str == '\0')
////        {
////            return null;
////        }
////        for(; (*str != '\0'); p++, str++)
////        {
////            if(*p != *str)
////            { //We only hurt the failure case
////                if((*str == '\u00A0') && (*p == '\u0020'))
////                {// This fix is for French or Kazakh cultures. Since a user cannot type 0xA0 as a 
////                    // space character we use 0x20 space character instead to mean the same.
////                    continue;
////                }
////                return null;
////            }
////        }
////        return p;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal unsafe static extern Decimal ParseDecimal( String           value   ,
                                                            NumberStyles     options ,
                                                            NumberFormatInfo numfmt  );
////    {
////
////        Byte* numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
////        NumberBuffer number = new NumberBuffer( numberBufferBytes );
////        Decimal result = 0;
////
////        StringToNumber( value, options, ref number, numfmt, true );
////
////        if(!NumberBufferToDecimal( number.PackForNative(), ref result ))
////        {
////            throw new OverflowException( Environment.GetResourceString( "Overflow_Decimal" ) );
////        }
////        return result;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal unsafe static extern Double ParseDouble( String           value   ,
                                                          NumberStyles     options ,
                                                          NumberFormatInfo numfmt  );
////    {
////        Byte* numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
////        NumberBuffer number = new NumberBuffer( numberBufferBytes );
////        Double d = 0;
////
////        StringToNumber( value, options, ref number, numfmt, false );
////
////        if(!NumberBufferToDouble( number.PackForNative(), ref d ))
////        {
////            throw new OverflowException( Environment.GetResourceString( "Overflow_Double" ) );
////        }
////
////        return d;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal unsafe static extern Int32 ParseInt32( String           s     ,
                                                        NumberStyles     style ,
                                                        NumberFormatInfo info  );
////    {
////
////        Byte* numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
////        NumberBuffer number = new NumberBuffer( numberBufferBytes );
////        Int32 i = 0;
////
////        StringToNumber( s, style, ref number, info, false );
////
////        if((style & NumberStyles.AllowHexSpecifier) != 0)
////        {
////            if(!HexNumberToInt32( ref number, ref i ))
////            {
////                throw new OverflowException( Environment.GetResourceString( "Overflow_Int32" ) );
////            }
////        }
////        else
////        {
////            if(!NumberToInt32( ref number, ref i ))
////            {
////                throw new OverflowException( Environment.GetResourceString( "Overflow_Int32" ) );
////            }
////        }
////        return i;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal unsafe static extern Int64 ParseInt64( String           value   ,
                                                        NumberStyles     options ,
                                                        NumberFormatInfo numfmt  );
////    {
////        Byte* numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
////        NumberBuffer number = new NumberBuffer( numberBufferBytes );
////        Int64 i = 0;
////
////        StringToNumber( value, options, ref number, numfmt, false );
////
////        if((options & NumberStyles.AllowHexSpecifier) != 0)
////        {
////            if(!HexNumberToInt64( ref number, ref i ))
////            {
////                throw new OverflowException( Environment.GetResourceString( "Overflow_Int64" ) );
////            }
////        }
////        else
////        {
////            if(!NumberToInt64( ref number, ref i ))
////            {
////                throw new OverflowException( Environment.GetResourceString( "Overflow_Int64" ) );
////            }
////        }
////        return i;
////    }
////
////    private unsafe static Boolean ParseNumber( ref char* str, NumberStyles options, ref NumberBuffer number, NumberFormatInfo numfmt, Boolean parseDecimal )
////    {
////
////        const Int32 StateSign = 0x0001;
////        const Int32 StateParens = 0x0002;
////        const Int32 StateDigits = 0x0004;
////        const Int32 StateNonZero = 0x0008;
////        const Int32 StateDecimal = 0x0010;
////        const Int32 StateCurrency = 0x0020;
////
////        number.scale = 0;
////        number.sign = false;
////        string decSep;                  // decimal separator from NumberFormatInfo.
////        string groupSep;                // group separator from NumberFormatInfo.
////        string currSymbol = null;       // currency symbol from NumberFormatInfo.
////
////        // The alternative currency symbol used in ANSI codepage, that can not roundtrip between ANSI and Unicode.
////        // Currently, only ja-JP and ko-KR has non-null values (which is U+005c, backslash)
////        string ansicurrSymbol = null;   // currency symbol from NumberFormatInfo.
////        string altdecSep = null;        // decimal separator from NumberFormatInfo as a decimal
////        string altgroupSep = null;      // group separator from NumberFormatInfo as a decimal
////
////        Boolean parsingCurrency = false;
////        if((options & NumberStyles.AllowCurrencySymbol) != 0)
////        {
////            currSymbol = numfmt.CurrencySymbol;
////            if(numfmt.ansiCurrencySymbol != null)
////            {
////                ansicurrSymbol = numfmt.ansiCurrencySymbol;
////            }
////            // The idea here is to match the currency separators and on failure match the number separators to keep the perf of VB's IsNumeric fast.
////            // The values of decSep are setup to use the correct relevant separator (currency in the if part and decimal in the else part).
////            altdecSep = numfmt.NumberDecimalSeparator;
////            altgroupSep = numfmt.NumberGroupSeparator;
////            decSep = numfmt.CurrencyDecimalSeparator;
////            groupSep = numfmt.CurrencyGroupSeparator;
////            parsingCurrency = true;
////        }
////        else
////        {
////            decSep = numfmt.NumberDecimalSeparator;
////            groupSep = numfmt.NumberGroupSeparator;
////        }
////
////        Int32 state = 0;
////        Boolean signflag = false; // Cache the results of "options & PARSE_LEADINGSIGN && !(state & STATE_SIGN)" to avoid doing this twice
////
////        char* p = str;
////        char ch = *p;
////        char* next;
////
////        while(true)
////        {
////            // Eat whitespace unless we've found a sign which isn't followed by a currency symbol.
////            // "-Kr 1231.47" is legal but "- 1231.47" is not.
////            if(IsWhite( ch ) && ((options & NumberStyles.AllowLeadingWhite) != 0) && (((state & StateSign) == 0) || (((state & StateSign) != 0) && (((state & StateCurrency) != 0) || numfmt.numberNegativePattern == 2))))
////            {
////                // Do nothing here. We will increase p at the end of the loop.
////            }
////            else if((signflag = (((options & NumberStyles.AllowLeadingSign) != 0) && ((state & StateSign) == 0))) && ((next = MatchChars( p, numfmt.positiveSign )) != null))
////            {
////                state |= StateSign;
////                p = next - 1;
////            }
////            else if(signflag && (next = MatchChars( p, numfmt.negativeSign )) != null)
////            {
////                state |= StateSign;
////                number.sign = true;
////                p = next - 1;
////            }
////            else if(ch == '(' && ((options & NumberStyles.AllowParentheses) != 0) && ((state & StateSign) == 0))
////            {
////                state |= StateSign | StateParens;
////                number.sign = true;
////            }
////            else if((currSymbol != null && (next = MatchChars( p, currSymbol )) != null) || (ansicurrSymbol != null && (next = MatchChars( p, ansicurrSymbol )) != null))
////            {
////                state |= StateCurrency;
////                currSymbol = null;
////                ansicurrSymbol = null;
////                // We already found the currency symbol. There should not be more currency symbols. Set
////                // currSymbol to NULL so that we won't search it again in the later code path.
////                p = next - 1;
////            }
////            else
////            {
////                break;
////            }
////            ch = *++p;
////        }
////        Int32 digCount = 0;
////        Int32 digEnd = 0;
////        while(true)
////        {
////            if((ch >= '0' && ch <= '9') || (((options & NumberStyles.AllowHexSpecifier) != 0) && ((ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F'))))
////            {
////                state |= StateDigits;
////                if(ch != '0' || (state & StateNonZero) != 0)
////                {
////                    if(digCount < NumberMaxDigits)
////                    {
////                        number.digits[digCount++] = ch;
////                        if(ch != '0' || parseDecimal)
////                        {
////                            digEnd = digCount;
////                        }
////                    }
////                    if((state & StateDecimal) == 0)
////                    {
////                        number.scale++;
////                    }
////                    state |= StateNonZero;
////                }
////                else if((state & StateDecimal) != 0)
////                {
////                    number.scale--;
////                }
////            }
////            else if(((options & NumberStyles.AllowDecimalPoint) != 0) && ((state & StateDecimal) == 0) && ((next = MatchChars( p, decSep )) != null || ((parsingCurrency) && (state & StateCurrency) == 0) && (next = MatchChars( p, altdecSep )) != null))
////            {
////                state |= StateDecimal;
////                p = next - 1;
////            }
////            else if(((options & NumberStyles.AllowThousands) != 0) && ((state & StateDigits) != 0) && ((state & StateDecimal) == 0) && ((next = MatchChars( p, groupSep )) != null || ((parsingCurrency) && (state & StateCurrency) == 0) && (next = MatchChars( p, altgroupSep )) != null))
////            {
////                p = next - 1;
////            }
////            else
////            {
////                break;
////            }
////            ch = *++p;
////        }
////
////        Boolean negExp = false;
////        number.precision = digEnd;
////        number.digits[digEnd] = '\0';
////        if((state & StateDigits) != 0)
////        {
////            if((ch == 'E' || ch == 'e') && ((options & NumberStyles.AllowExponent) != 0))
////            {
////                char* temp = p;
////                ch = *++p;
////                if((next = MatchChars( p, numfmt.positiveSign )) != null)
////                {
////                    ch = *(p = next);
////                }
////                else if((next = MatchChars( p, numfmt.negativeSign )) != null)
////                {
////                    ch = *(p = next);
////                    negExp = true;
////                }
////                if(ch >= '0' && ch <= '9')
////                {
////                    Int32 exp = 0;
////                    do
////                    {
////                        exp = exp * 10 + (ch - '0');
////                        ch = *++p;
////                        if(exp > 1000)
////                        {
////                            exp = 9999;
////                            while(ch >= '0' && ch <= '9')
////                            {
////                                ch = *++p;
////                            }
////                        }
////                    } while(ch >= '0' && ch <= '9');
////                    if(negExp)
////                    {
////                        exp = -exp;
////                    }
////                    number.scale += exp;
////                }
////                else
////                {
////                    p = temp;
////                    ch = *p;
////                }
////            }
////            while(true)
////            {
////                if(IsWhite( ch ) && ((options & NumberStyles.AllowTrailingWhite) != 0))
////                {
////                }
////                else if((signflag = (((options & NumberStyles.AllowTrailingSign) != 0) && ((state & StateSign) == 0))) && (next = MatchChars( p, numfmt.positiveSign )) != null)
////                {
////                    state |= StateSign;
////                    p = next - 1;
////                }
////                else if(signflag && (next = MatchChars( p, numfmt.negativeSign )) != null)
////                {
////                    state |= StateSign;
////                    number.sign = true;
////                    p = next - 1;
////                }
////                else if(ch == ')' && ((state & StateParens) != 0))
////                {
////                    state &= ~StateParens;
////                }
////                else if((currSymbol != null && (next = MatchChars( p, currSymbol )) != null) || (ansicurrSymbol != null && (next = MatchChars( p, ansicurrSymbol )) != null))
////                {
////                    currSymbol = null;
////                    ansicurrSymbol = null;
////                    p = next - 1;
////                }
////                else
////                {
////                    break;
////                }
////                ch = *++p;
////            }
////            if((state & StateParens) == 0)
////            {
////                if((state & StateNonZero) == 0)
////                {
////                    if(!parseDecimal)
////                    {
////                        number.scale = 0;
////                    }
////                    if((state & StateDecimal) == 0)
////                    {
////                        number.sign = false;
////                    }
////                }
////                str = p;
////                return true;
////            }
////        }
////        str = p;
////        return false;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal unsafe static extern Single ParseSingle( String           value   ,
                                                          NumberStyles     options ,
                                                          NumberFormatInfo numfmt  );
////    {
////
////        Byte* numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
////        NumberBuffer number = new NumberBuffer( numberBufferBytes );
////        Double d = 0;
////
////        StringToNumber( value, options, ref number, numfmt, false );
////
////        if(!NumberBufferToDouble( number.PackForNative(), ref d ))
////        {
////            throw new OverflowException( Environment.GetResourceString( "Overflow_Single" ) );
////        }
////        Single castSingle = (Single)d;
////        if(Single.IsInfinity( castSingle ))
////        {
////            throw new OverflowException( Environment.GetResourceString( "Overflow_Single" ) );
////        }
////        return castSingle;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal unsafe static extern UInt32 ParseUInt32( String           value   ,
                                                          NumberStyles     options ,
                                                          NumberFormatInfo numfmt  );
////    {
////
////        Byte* numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
////        NumberBuffer number = new NumberBuffer( numberBufferBytes );
////        UInt32 i = 0;
////
////        StringToNumber( value, options, ref number, numfmt, false );
////
////        if((options & NumberStyles.AllowHexSpecifier) != 0)
////        {
////            if(!HexNumberToUInt32( ref number, ref i ))
////            {
////                throw new OverflowException( Environment.GetResourceString( "Overflow_UInt32" ) );
////            }
////        }
////        else
////        {
////            if(!NumberToUInt32( ref number, ref i ))
////            {
////                throw new OverflowException( Environment.GetResourceString( "Overflow_UInt32" ) );
////            }
////        }
////
////        return i;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal unsafe static extern UInt64 ParseUInt64( String           value   ,
                                                          NumberStyles     options ,
                                                          NumberFormatInfo numfmt  );
////    {
////        Byte* numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
////        NumberBuffer number = new NumberBuffer( numberBufferBytes );
////        UInt64 i = 0;
////
////        StringToNumber( value, options, ref number, numfmt, false );
////        if((options & NumberStyles.AllowHexSpecifier) != 0)
////        {
////            if(!HexNumberToUInt64( ref number, ref i ))
////            {
////                throw new OverflowException( Environment.GetResourceString( "Overflow_UInt64" ) );
////            }
////        }
////        else
////        {
////            if(!NumberToUInt64( ref number, ref i ))
////            {
////                throw new OverflowException( Environment.GetResourceString( "Overflow_UInt64" ) );
////            }
////        }
////        return i;
////    }
////
////    private unsafe static void StringToNumber( String str, NumberStyles options, ref NumberBuffer number, NumberFormatInfo info, Boolean parseDecimal )
////    {
////
////        if(str == null)
////        {
////            throw new ArgumentNullException( "String" );
////        }
////        BCLDebug.Assert( info != null, "" );
////        fixed(char* stringPointer = str)
////        {
////            char* p = stringPointer;
////            if(!ParseNumber( ref p, options, ref number, info, parseDecimal )
////                || (p - stringPointer < str.Length && !TrailingZeros( str, (int)(p - stringPointer) )))
////            {
////                throw new FormatException( Environment.GetResourceString( "Format_InvalidString" ) );
////            }
////        }
////    }
////
////    private static Boolean TrailingZeros( String s, Int32 index )
////    {
////        // For compatability, we need to allow trailing zeros at the end of a number string
////        for(int i = index; i < s.Length; i++)
////        {
////            if(s[i] != '\0')
////            {
////                return false;
////            }
////        }
////        return true;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal unsafe static extern Boolean TryParseDecimal(     String           value   ,
                                                                   NumberStyles     options ,
                                                                   NumberFormatInfo numfmt  ,
                                                               out Decimal          result  );
////    {
////
////        Byte* numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
////        NumberBuffer number = new NumberBuffer( numberBufferBytes );
////        result = 0;
////
////        if(!TryStringToNumber( value, options, ref number, numfmt, true ))
////        {
////            return false;
////        }
////
////        if(!NumberBufferToDecimal( number.PackForNative(), ref result ))
////        {
////            return false;
////        }
////        return true;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal unsafe static extern Boolean TryParseDouble(     String           value   ,
                                                                  NumberStyles     options ,
                                                                  NumberFormatInfo numfmt  ,
                                                              out Double           result  );
////    {
////        Byte* numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
////        NumberBuffer number = new NumberBuffer( numberBufferBytes );
////        result = 0;
////
////
////        if(!TryStringToNumber( value, options, ref number, numfmt, false ))
////        {
////            return false;
////        }
////        if(!NumberBufferToDouble( number.PackForNative(), ref result ))
////        {
////            return false;
////        }
////        return true;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal unsafe static extern Boolean TryParseInt32(     String           s      ,
                                                                 NumberStyles     style  ,
                                                                 NumberFormatInfo info   ,
                                                             out Int32            result );
////    {
////
////        Byte* numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
////        NumberBuffer number = new NumberBuffer( numberBufferBytes );
////        result = 0;
////
////        if(!TryStringToNumber( s, style, ref number, info, false ))
////        {
////            return false;
////        }
////
////        if((style & NumberStyles.AllowHexSpecifier) != 0)
////        {
////            if(!HexNumberToInt32( ref number, ref result ))
////            {
////                return false;
////            }
////        }
////        else
////        {
////            if(!NumberToInt32( ref number, ref result ))
////            {
////                return false;
////            }
////        }
////        return true;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal unsafe static extern Boolean TryParseInt64(     String           s      ,
                                                                 NumberStyles     style  ,
                                                                 NumberFormatInfo info   ,
                                                             out Int64            result );
////    {
////
////        Byte* numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
////        NumberBuffer number = new NumberBuffer( numberBufferBytes );
////        result = 0;
////
////        if(!TryStringToNumber( s, style, ref number, info, false ))
////        {
////            return false;
////        }
////
////        if((style & NumberStyles.AllowHexSpecifier) != 0)
////        {
////            if(!HexNumberToInt64( ref number, ref result ))
////            {
////                return false;
////            }
////        }
////        else
////        {
////            if(!NumberToInt64( ref number, ref result ))
////            {
////                return false;
////            }
////        }
////        return true;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal unsafe static extern Boolean TryParseSingle(     String           value   ,
                                                                  NumberStyles     options ,
                                                                  NumberFormatInfo numfmt  ,
                                                              out Single           result  );
////    {
////        Byte* numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
////        NumberBuffer number = new NumberBuffer( numberBufferBytes );
////        result = 0;
////        Double d = 0;
////
////        if(!TryStringToNumber( value, options, ref number, numfmt, false ))
////        {
////            return false;
////        }
////        if(!NumberBufferToDouble( number.PackForNative(), ref d ))
////        {
////            return false;
////        }
////        Single castSingle = (Single)d;
////        if(Single.IsInfinity( castSingle ))
////        {
////            return false;
////        }
////
////        result = castSingle;
////        return true;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal unsafe static extern Boolean TryParseUInt32(     String           s      ,
                                                                  NumberStyles     style  ,
                                                                  NumberFormatInfo info   ,
                                                              out UInt32           result );
////    {
////
////        Byte* numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
////        NumberBuffer number = new NumberBuffer( numberBufferBytes );
////        result = 0;
////
////        if(!TryStringToNumber( s, style, ref number, info, false ))
////        {
////            return false;
////        }
////
////        if((style & NumberStyles.AllowHexSpecifier) != 0)
////        {
////            if(!HexNumberToUInt32( ref number, ref result ))
////            {
////                return false;
////            }
////        }
////        else
////        {
////            if(!NumberToUInt32( ref number, ref result ))
////            {
////                return false;
////            }
////        }
////        return true;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal unsafe static extern Boolean TryParseUInt64(     String           s      ,
                                                                  NumberStyles     style  ,
                                                                  NumberFormatInfo info   ,
                                                              out UInt64           result );
////    {
////
////        Byte* numberBufferBytes = stackalloc Byte[NumberBuffer.NumberBufferBytes];
////        NumberBuffer number = new NumberBuffer( numberBufferBytes );
////        result = 0;
////
////        if(!TryStringToNumber( s, style, ref number, info, false ))
////        {
////            return false;
////        }
////
////        if((style & NumberStyles.AllowHexSpecifier) != 0)
////        {
////            if(!HexNumberToUInt64( ref number, ref result ))
////            {
////                return false;
////            }
////        }
////        else
////        {
////            if(!NumberToUInt64( ref number, ref result ))
////            {
////                return false;
////            }
////        }
////        return true;
////    }
////
////    private unsafe static Boolean TryStringToNumber( String str, NumberStyles options, ref NumberBuffer number, NumberFormatInfo numfmt, Boolean parseDecimal )
////    {
////
////        if(str == null)
////        {
////            return false;
////        }
////        BCLDebug.Assert( numfmt != null, "" );
////
////        fixed(char* stringPointer = str)
////        {
////            char* p = stringPointer;
////            if(!ParseNumber( ref p, options, ref number, numfmt, parseDecimal )
////                || (p - stringPointer < str.Length && !TrailingZeros( str, (int)(p - stringPointer) )))
////            {
////                return false;
////            }
////        }
////
////        return true;
////    }

////    private Number( ref Decimal value )
////    {
////        this.precision = DecimalPrecision;
////        this.negative = value.negative;
////        int index = NumberMaxDigits;
////        while(value.mid32 != 0 || value.hi32 != 0)
////        {
////            Int32ToDecChars( this.digits, ref index, DecDivMod1E9( ref value ), 9 );
////        }
////        Int32ToDecChars( this.digits, ref index, value.lo32, 0 );
////        int digitCount = NumberMaxDigits - index;
////        this.scale = digitCount - value.scale;
////        int destIndex = 0;
////        while(digitCount > 0)
////        {
////            this.digits[destIndex] = this.digits[index];
////            destIndex++;
////            index++;
////            digitCount--;
////        }
////        this.digits[destIndex] = '\0';
////    }

        private Number( int value )
        {
            this.precision = Int32Precision;
            if(value >= 0)
            {
                this.negative = false;
            }
            else
            {
                this.negative = true;
                value = -value;
            }

            int index = Int32ToDecChars( this.digits, Int32Precision, unchecked( (uint)value ), 0 );
            int digitCount = Int32Precision - index;
            int destIndex = 0;
            this.scale = digitCount;
            while(digitCount > 0)
            {
                this.digits[destIndex] = this.digits[index];
                destIndex++;
                index++;
                digitCount--;
            }
            this.digits[destIndex] = '\0';
        }

        private Number( uint value )
        {
            this.precision = Int32Precision;
            this.negative = false;
            int index = Int32ToDecChars( this.digits, Int32Precision, value, 0 );
            int digitCount = Int32Precision - index;
            int destIndex = 0;
            this.scale = digitCount;
            while(digitCount > 0)
            {
                this.digits[destIndex] = this.digits[index];
                destIndex++;
                index++;
                digitCount--;
            }
            this.digits[destIndex] = '\0';
        }

        private Number( long value )
        {
            this.precision = Int64Precision;
            if(value >= 0)
            {
                this.negative = false;
            }
            else
            {
                this.negative = true;
                value = -value;
            }
            int index = Int64ToDecChars( this.digits, Int64Precision, unchecked( (ulong)value ), 0 );
            int digitCount = Int64Precision - index;
            int destIndex = 0;
            this.scale = digitCount;
            while(digitCount > 0)
            {
                this.digits[destIndex] = this.digits[index];
                destIndex++;
                index++;
                digitCount--;
            }
            this.digits[destIndex] = '\0';
        }

        private Number( ulong value )
        {
            this.precision = Int64Precision;
            this.negative = false;
            int index = Int64ToDecChars( this.digits, Int64Precision, value, 0 );
            int digitCount = Int64Precision - index;
            int destIndex = 0;
            this.scale = digitCount;
            while(digitCount > 0)
            {
                this.digits[destIndex] = this.digits[index];
                destIndex++;
                index++;
                digitCount--;
            }
            this.digits[destIndex] = '\0';
        }

        // inline this!!!
        private static bool EndString( String s, int p )
        {
            return p == s.Length || s[p] == '\0';
        }

        // markples: see also Lightning\Src\VM\COMNumber.cpp::
        // wchar* MatchChars(wchar* p, wchar* str)
        // will now return -1 instead of NULL on failure
        private static int MatchChars( String str1, int p, String str )
        {
            int str_i = 0;
            if(EndString( str, str_i )) return -1;
            for(; !EndString( str, str_i ); p++, str_i++)
            {
                if(str1[p] != str[str_i]) //We only hurt the failure case
                {
                    if((str[str_i] == 0xA0) && (str1[p] == 0x20))
                        // This fix is for French or Kazakh cultures.
                        // Since a user cannot type 0xA0 as a space
                        // character we use 0x20 space character
                        // instead to mean the same.
                        continue;
                    return -1;
                }
            }
            return p;
        }

        private const int STATE_SIGN = 0x0001;
        private const int STATE_PARENS = 0x0002;
        private const int STATE_DIGITS = 0x0004;
        private const int STATE_NONZERO = 0x0008;
        private const int STATE_DECIMAL = 0x0010;
        private const int STATE_HEXLEAD = 0x0020;

        // #defines in Lightning\Src\VM\COMNumber.cpp
        private const int FLOAT_PRECISION = 7;
        private const int DOUBLE_PRECISION = 15;
        private const int MIN_BUFFER_SIZE = 105;
        private const int SCALE_NAN = unchecked( (int)0x80000000 );
        private const int SCALE_INF = 0x7FFFFFFF;

        private static String[] posPercentFormats = {
            "# %", "#%", "%#"
        };

        // BUGBUG yslin: have to verify on the negative Percent
        // format for real format.
        private static String[] negPercentFormats = {
            "-# %", "-#%", "-%#"
        };
        private static String[] negNumberFormats = {
            "(#)", "-#", "- #", "#-", "# -"
        };
        private static String posNumberFormat = "#";

        // code below depends on seeing the null terminator...
        private static char Get( String str, int i )
        {
            return i < str.Length ? str[i] : '\0';
        }

        private Number( double value     ,
                        int    precision )
        {
            this.precision = precision;
            if(value >= 0)
            {
                this.negative = false;
            }
            else
            {
                this.negative = true;
                value = -value;
            }

            double log      = Math.Floor( Math.Log10( value ) );
            int    digitPos = (int)log;

            //
            // Limit to something slightly less than the maximum exponent or we might overflow/underflow.
            //
            if(digitPos >=  308) digitPos =  307;
            if(digitPos <= -308) digitPos = -307;

            double value2 = value * PowerOfTen( -digitPos );

            //
            // Log10 returns a value that is almost the correct one, but due to the integer truncation, we might be off by an order of magnitude.
            // Make sure the number to convert is within the range [0.1,1)
            //
            if(value2 < 0.01)
            {
                value2   *= 1E2;
                digitPos -= 2;
            }
            else if(value2 < 0.1)
            {
                value2   *= 10;
                digitPos -= 1;
            }
            else if(value2 >= 10)
            {
                value2   *= 1E-2;
                digitPos += 2;
            }
            else if(value2 >= 1)
            {
                value2   *= 1E-1;
                digitPos += 1;
            }

            this.scale = digitPos;

            value2 *= PowerOfTen( precision );

            ulong valInt = unchecked( (ulong)(long)value2 );

            int index      = Int64ToDecChars( this.digits, UInt64Precision, valInt, 0 );
            int digitCount = UInt64Precision - index;
            int destIndex  = 0;
            while(digitCount > 0)
            {
                this.digits[destIndex] = this.digits[index];

                destIndex++;
                index++;
                digitCount--;
            }
        }

        static double PowerOfTen( int scale )
        {
            double res = 1;

            if(scale > 0)
            {
                switch(scale % 32)
                {
                    case 0 : res *= 1E0 ; break;
                    case 1 : res *= 1E1 ; break;
                    case 2 : res *= 1E2 ; break;
                    case 3 : res *= 1E3 ; break;
                    case 4 : res *= 1E4 ; break;
                    case 5 : res *= 1E5 ; break;
                    case 6 : res *= 1E6 ; break;
                    case 7 : res *= 1E7 ; break;
                    case 8 : res *= 1E8 ; break;
                    case 9 : res *= 1E9 ; break;
                    case 10: res *= 1E10; break;
                    case 11: res *= 1E11; break;
                    case 12: res *= 1E12; break;
                    case 13: res *= 1E13; break;
                    case 14: res *= 1E14; break;
                    case 15: res *= 1E15; break;
                    case 16: res *= 1E16; break;
                    case 17: res *= 1E17; break;
                    case 18: res *= 1E18; break;
                    case 19: res *= 1E19; break;
                    case 20: res *= 1E20; break;
                    case 21: res *= 1E21; break;
                    case 22: res *= 1E22; break;
                    case 23: res *= 1E23; break;
                    case 24: res *= 1E24; break;
                    case 25: res *= 1E25; break;
                    case 26: res *= 1E26; break;
                    case 27: res *= 1E27; break;
                    case 28: res *= 1E28; break;
                    case 29: res *= 1E29; break;
                    case 30: res *= 1E30; break;
                    case 31: res *= 1E31; break;
                }

                while(scale >= 32)
                {
                    res   *= 1E32;
                    scale -= 32;
                }
            }
            else if(scale < 0)
            {
                scale = -scale;

                switch(scale % 32)
                {
                    case 0 : res *= 1E-0 ; break;
                    case 1 : res *= 1E-1 ; break;
                    case 2 : res *= 1E-2 ; break;
                    case 3 : res *= 1E-3 ; break;
                    case 4 : res *= 1E-4 ; break;
                    case 5 : res *= 1E-5 ; break;
                    case 6 : res *= 1E-6 ; break;
                    case 7 : res *= 1E-7 ; break;
                    case 8 : res *= 1E-8 ; break;
                    case 9 : res *= 1E-9 ; break;
                    case 10: res *= 1E-10; break;
                    case 11: res *= 1E-11; break;
                    case 12: res *= 1E-12; break;
                    case 13: res *= 1E-13; break;
                    case 14: res *= 1E-14; break;
                    case 15: res *= 1E-15; break;
                    case 16: res *= 1E-16; break;
                    case 17: res *= 1E-17; break;
                    case 18: res *= 1E-18; break;
                    case 19: res *= 1E-19; break;
                    case 20: res *= 1E-20; break;
                    case 21: res *= 1E-21; break;
                    case 22: res *= 1E-22; break;
                    case 23: res *= 1E-23; break;
                    case 24: res *= 1E-24; break;
                    case 25: res *= 1E-25; break;
                    case 26: res *= 1E-26; break;
                    case 27: res *= 1E-27; break;
                    case 28: res *= 1E-28; break;
                    case 29: res *= 1E-29; break;
                    case 30: res *= 1E-30; break;
                    case 31: res *= 1E-31; break;
                }

                while(scale >= 32)
                {
                    res   *= 1E-32;
                    scale -= 32;
                }
            }

            return res;
        }

////    // markples: see also Lightning\Src\VM\COMNumber.cpp::
////    //  int ParseNumber(wchar** str, int options,
////    //                  NUMBER* number, NUMFMTREF numfmt)
////    // clr behavior: return nonzero on failure and update *str
////    // new behavior: return false iff broken (error or *str != \0 at end)
////    private bool ParseNumber( String str, NumberStyles style )
////    {
////        this.scale = 0;
////        this.negative = false;
////        String decSep = info.numberDecimalSeparator;
////        String groupSep = info.numberGroupSeparator;
////
////        int state = 0;
////        bool signflag = false; // Cache the results of
////        // "style & NumberStyles.AllowLeadingSign &&
////        // !(state & STATE_SIGN)"
////        // to avoid doing this twice
////        int p = 0;
////        int next;
////
////        if((style & NumberStyles.AllowHexSpecifier) != 0 &&
////            str.Length >= 2)
////        {
////            if(str[0] == '0' && (str[1] == 'x' || str[1] == 'X'))
////            {
////                p = 2;
////            }
////        }
////
////        char ch = Get( str, p );
////
////        while(true)
////        {
////            //Eat whitespace unless we've found a sign.
////            if(IsWhite( ch )
////                && ((style & NumberStyles.AllowLeadingWhite) != 0)
////                && (!((state & STATE_SIGN) != 0) ||
////                    (((state & STATE_SIGN) != 0) &&
////                     (info.numberNegativePattern == 2))))
////            {
////                // Do nothing here. We will increase p at the end of the loop
////            }
////            else if((signflag =
////                        (((style & NumberStyles.AllowLeadingSign) != 0) &&
////                         !((state & STATE_SIGN) != 0))) &&
////                       (next = MatchChars( str, p, info.positiveSign )) != -1)
////            {
////                state |= STATE_SIGN;
////                p = next - 1;
////            }
////            else if(signflag &&
////                       (next = MatchChars( str, p, info.negativeSign )) != -1)
////            {
////                state |= STATE_SIGN;
////                this.negative = true;
////                p = next - 1;
////            }
////            else if(ch == '(' &&
////                       ((style & NumberStyles.AllowParentheses) != 0) &&
////                       !((state & STATE_SIGN) != 0))
////            {
////                state |= STATE_SIGN | STATE_PARENS;
////                this.negative = true;
////            }
////            else
////            {
////                break;
////            }
////            ch = Get( str, ++p );
////        }
////        int digCount = 0;
////        int digEnd = 0;
////        while(true)
////        {
////            if((ch >= '0' && ch <= '9') ||
////                (((style & NumberStyles.AllowHexSpecifier) != 0) &&
////                  ((ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F'))))
////            {
////                state |= STATE_DIGITS;
////                if(ch != '0' || ((state & STATE_NONZERO) != 0))
////                {
////                    if(digCount < NUMBER_MAXDIGITS)
////                    {
////                        this.digits[digCount++] = ch;
////                        if(ch != '0') digEnd = digCount;
////                    }
////                    if(!((state & STATE_DECIMAL) != 0)) this.scale++;
////                    state |= STATE_NONZERO;
////                }
////                else if((state & STATE_DECIMAL) != 0) this.scale--;
////            }
////            else if(((style & NumberStyles.AllowDecimalPoint) != 0) &&
////                       !((state & STATE_DECIMAL) != 0) &&
////                       ((next = MatchChars( str, p, decSep )) != -1))
////            {
////                state |= STATE_DECIMAL;
////                p = next - 1;
////            }
////            else if(((style & NumberStyles.AllowThousands) != 0) &&
////                       ((state & STATE_DIGITS) != 0) &&
////                       !((state & STATE_DECIMAL) != 0) &&
////                       ((next = MatchChars( str, p, groupSep )) != -1))
////            {
////                p = next - 1;
////            }
////            else
////            {
////                break;
////            }
////            ch = Get( str, ++p );
////        }
////
////        bool negExp = false;
////        this.precision = digEnd;
////        this.digits[digEnd] = '\0';
////        if((state & STATE_DIGITS) != 0)
////        {
////            if((ch == 'E' || ch == 'e') &&
////                ((style & NumberStyles.AllowExponent) != 0))
////            {
////                int temp = p;
////                ch = Get( str, ++p );
////                if((next = MatchChars( str, p, info.positiveSign )) != -1)
////                {
////                    ch = Get( str, p = next );
////                }
////                else if((next = MatchChars( str, p, info.negativeSign )) != -1)
////                {
////                    ch = Get( str, p = next );
////                    negExp = true;
////                }
////                if(ch >= '0' && ch <= '9')
////                {
////                    int exp = 0;
////                    do
////                    {
////                        exp = exp * 10 + (ch - '0');
////                        ch = Get( str, ++p );
////                        if(exp > 1000)
////                        {
////                            exp = 9999;
////                            while(ch >= '0' && ch <= '9')
////                            {
////                                ch = Get( str, ++p );
////                            }
////                        }
////                    } while(ch >= '0' && ch <= '9');
////                    if(negExp) exp = -exp;
////                    this.scale += exp;
////                }
////                else
////                {
////                    p = temp;
////                    ch = Get( str, p );
////                }
////            }
////            while(true)
////            {
////                if(IsWhite( ch ) &&
////                    ((style & NumberStyles.AllowTrailingWhite) != 0))
////                {
////                    // do nothing
////                }
////                else if((signflag =
////                            (((style & NumberStyles.AllowTrailingSign) != 0)
////                             &&
////                             !((state & STATE_SIGN) != 0))) &&
////                           (next =
////                            MatchChars( str, p, info.positiveSign )) != -1)
////                {
////                    state |= STATE_SIGN;
////                    p = next - 1;
////                }
////                else if(signflag &&
////                           (next = MatchChars( str, p,
////                                              info.negativeSign )) != -1)
////                {
////                    state |= STATE_SIGN;
////                    this.negative = true;
////                    p = next - 1;
////                }
////                else if(ch == ')' && ((state & STATE_PARENS) != 0))
////                {
////                    state &= ~STATE_PARENS;
////                }
////                else
////                {
////                    break;
////                }
////                ch = Get( str, ++p );
////            }
////            if(!((state & STATE_PARENS) != 0))
////            {
////                if(!((state & STATE_NONZERO) != 0))
////                {
////                    this.scale = 0;
////                    if(!((state & STATE_DECIMAL) != 0))
////                    {
////                        this.negative = false;
////                    }
////                }
////                // *str = p;
////                // return 1;
////                return EndString( str, p );
////            }
////        }
////        // *str = p;
////        // return 0;
////        return false;
////    }
////
////
////    // markples: see also Lightning\Src\VM\COMNumber.cpp::
////    //void StringToNumber(STRINGREF str, int options,
////    //                    NUMBER* number, NUMFMTREF numfmt)
////    private Number( String str, NumberStyles style )
////    {
////        {
////            //THROWSCOMPLUSEXCEPTION();
////
////            if(str == null)
////            {
////                throw new NullReferenceException();
////            }
////
////            if(!info.validForParseAsNumber)
////            {
////                throw new ArgumentException( "Argument_AmbiguousNumberInfo" );
////            }
////            //wchar* p = str->GetBuffer();
////            if(!ParseNumber( str, style ))
////            {
////                throw new FormatException( "Format_InvalidString" );
////            }
////        }
////    }

        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern String FormatDecimal( Decimal          value  ,
                                                   String           format ,
                                                   NumberFormatInfo info   );
////    {
////        // rusa: see also Lightning\Src\VM\COMNumber.cpp::FormatDecimal
////        if(format == null)
////        {
////            throw new ArgumentNullException( "format" );
////        }
////        Number number = new Number( ref value );
////        return FPNumberToString( number, format, info );
////    }

        public static String FormatDouble( double           value  ,
                                           NumberFormatInfo info   )
        {
            Number number = new Number( value, DOUBLE_PRECISION );

            if(number.scale == SCALE_NAN)
            {
                return info.nanSymbol;
            }

            if(number.scale == SCALE_INF)
            {
                return number.negative ? info.negativeInfinitySymbol : info.positiveInfinitySymbol;
            }

            return number.ToString( 'G', -1, info );
        }

        public static String FormatDouble( double           value  ,
                                           String           format ,
                                           NumberFormatInfo info   )
        {
            // rusa: see also Lightning\Src\VM\COMNumber.cpp::FormatDouble

            Number number;
            int digits;
            char fmt = ParseFormatSpecifier( format, out digits );
            char val = (char)(fmt & 0xFFDF);
            int precision = DOUBLE_PRECISION;
            switch(val)
            {
                case 'R':
                    //In order to give numbers that are both
                    //friendly to display and round-trippable, we
                    //parse the number using 15 digits and then
                    //determine if it round trips to the same value.
                    //If it does, we convert that NUMBER to a
                    //string, otherwise we reparse using 17 digits
                    //and display that.

                    number = new Number( value, DOUBLE_PRECISION );

                    if(number.scale == SCALE_NAN)
                    {
                        return info.nanSymbol;
                    }
                    if(number.scale == SCALE_INF)
                    {
                        return number.negative ? info.negativeInfinitySymbol
                            : info.positiveInfinitySymbol;
                    }

////                double dTest;
////                NumberToDouble( number, out dTest );
////
////                if(dTest == value)
////                {
////                    return number.ToString( 'G', DOUBLE_PRECISION );
////                }

                    number = new Number( value, 17 );
                    return number.ToString( 'G', 17, info );

                case 'E':
                    // Here we round values less than E14 to 15 digits
                    if(digits > 14)
                    {
                        precision = 17;
                    }
                    break;

                case 'G':
                    // Here we round values less than G15 to 15
                    // digits, G16 and G17 will not be touched
                    if(digits > 15)
                    {
                        precision = 17;
                    }
                    break;

            }

            number = new Number( value, precision );
            return FPNumberToString( number, format, info );
        }

        internal static String FormatInt32( int value,
                                            NumberFormatInfo info )
        {
            return FormatInt32( value, 'G', info );
        }

        internal static String FormatInt32( int              value     ,
                                            char             formatChar,
                                            NumberFormatInfo info      )
        {
            Number number = new Number( value );
            return number.ToString( formatChar, -1, info );
        }

        public static String FormatInt32( int              value  ,
                                          String           format ,
                                          NumberFormatInfo info   )
        {
            // rusa: see also Lightning\Src\VM\COMNumber.cpp::FormatInt32
            int digits;
            char fmt = ParseFormatSpecifier( format, out digits );
            switch(fmt)
            {
                case 'g':
                case 'G':
                    {
                        if(digits > 0) break;
                        goto case 'D';
                    }
                case 'd':
                case 'D':
                    {
                        return Int32ToDecString( value, digits, info.negativeSign );
                    }
                case 'x':
                case 'X':
                    {
                        return Int32ToHexString( unchecked( (uint)value ),
                                                (char)(fmt - ('X' - 'A' + 10)),
                                                digits );
                    }
                default:
                    {
                        break;
                    }
            }
            Number number = new Number( value );
            if(fmt == 0)
            {
                return number.ToStringFormat( format, info );
            }
            else
            {
                return number.ToString( fmt, digits, info );
            }
        }

        internal static String FormatUInt32( uint value,
                                    NumberFormatInfo info )
        {
            Number number = new Number( value );
            return number.ToString( 'G', -1, info );
        }

        public static String FormatUInt32( uint             value  ,
                                           String           format ,
                                           NumberFormatInfo info   )
        {
            // rusa: see also Lightning\Src\VM\COMNumber.cpp::FormatUInt32
            int digits;
            char fmt = ParseFormatSpecifier( format, out digits );
            switch(fmt)
            {
                case 'g':
                case 'G':
                    {
                        if(digits > 0) break;
                        goto case 'D';
                    }
                case 'd':
                case 'D':
                    {
                        return UInt32ToDecString( value, digits );
                    }
                case 'x':
                case 'X':
                    {
                        return Int32ToHexString( value,
                                                (char)(fmt - ('X' - 'A' + 10)),
                                                digits );
                    }
                default:
                    {
                        break;
                    }
            }
            Number number = new Number( value );
            if(fmt == 0)
            {
                return number.ToStringFormat( format, info );
            }
            else
            {
                return number.ToString( fmt, digits, info );
            }
        }

        internal static String FormatInt64( Int64 value,
                                    NumberFormatInfo info )
        {
            Number number = new Number( value );
            return number.ToString( 'G', -1, info );
        }

        public static String FormatInt64( long             value  ,
                                          String           format ,
                                          NumberFormatInfo info   )
        {
            // rusa: see also Lightning\Src\VM\COMNumber.cpp::FormatInt64
            int digits;
            char fmt = ParseFormatSpecifier( format, out digits );
            switch(fmt)
            {
                case 'g':
                case 'G':
                    {
                        if(digits > 0) break;
                        goto case 'D';
                    }
                case 'd':
                case 'D':
                    {
                        return Int64ToDecString( value, digits, info.negativeSign );
                    }
                case 'x':
                case 'X':
                    {
                        return Int64ToHexString( unchecked( (ulong)value ),
                                                (char)(fmt - ('X' - 'A' + 10)),
                                                digits );
                    }
                default:
                    {
                        break;
                    }
            }
            Number number = new Number( value );
            if(fmt == 0)
            {
                return number.ToStringFormat( format, info );
            }
            else
            {
                return number.ToString( fmt, digits, info );
            }
        }

        internal static String FormatUInt64( UInt64 value,
                                    NumberFormatInfo info )
        {
            Number number = new Number( value );
            return number.ToString( 'G', -1, info );
        }

        public static String FormatUInt64( ulong            value  ,
                                           String           format ,
                                           NumberFormatInfo info   )
        {
            // rusa: see also Lightning\Src\VM\COMNumber.cpp::FormatUInt64
            int digits;
            char fmt = ParseFormatSpecifier( format, out digits );
            switch(fmt)
            {
                case 'g':
                case 'G':
                    {
                        if(digits > 0) break;
                        goto case 'D';
                    }
                case 'd':
                case 'D':
                    {
                        return UInt64ToDecString( value, digits );
                    }
                case 'x':
                case 'X':
                    {
                        return Int64ToHexString( value, (char)(fmt - ('X' - 'A' + 10)), digits );
                    }
                default:
                    {
                        break;
                    }
            }
            Number number = new Number( value );
            if(fmt == 0)
            {
                return number.ToStringFormat( format, info );
            }
            else
            {
                return number.ToString( fmt, digits, info );
            }
        }

        public static String FormatSingle( float            value,
                                           NumberFormatInfo info )
        {
            Number number = new Number( value, FLOAT_PRECISION );

            if(number.scale == SCALE_NAN)
            {
                return info.nanSymbol;
            }

            if(number.scale == SCALE_INF)
            {
                return number.negative ? info.negativeInfinitySymbol : info.positiveInfinitySymbol;
            }

            return number.ToString( 'G', -1, info );
        }

        public static String FormatSingle( float            value  ,
                                           String           format ,
                                           NumberFormatInfo info   )
        {
            // rusa: see also Lightning\Src\VM\COMNumber.cpp::FormatSingle

            Number number;
            int digits;
            double argsValue = value;
            char fmt = ParseFormatSpecifier( format, out digits );
            char val = (char)(fmt & 0xFFDF);
            int precision = FLOAT_PRECISION;
            switch(val)
            {
                case 'R':
                    //In order to give numbers that are both
                    //friendly to display and round-trippable, we
                    //parse the number using 7 digits and then
                    //determine if it round trips to the same value.
                    //If it does, we convert that NUMBER to a
                    //string, otherwise we reparse using 9 digits
                    //and display that.

                    number = new Number( argsValue, FLOAT_PRECISION );

                    if(number.scale == SCALE_NAN)
                    {
                        return info.nanSymbol;
                    }
                    if(number.scale == SCALE_INF)
                    {
                        return number.negative ? info.negativeInfinitySymbol
                            : info.positiveInfinitySymbol;
                    }

////                double dTest;
////                NumberToDouble( number, out dTest );
////
////                // HACK: force restriction to float
////                float[] hack = new float[1];
////                hack[0] = (float)dTest;
////                float fTest = hack[0];
////
////                if(fTest == value)
////                {
////                    return number.ToString( 'G', FLOAT_PRECISION );
////                }

                    number = new Number( argsValue, 9 );
                    return number.ToString( 'G', 9, info );

                case 'E':
                    // Here we round values less than E14 to 15 digits
                    if(digits > 6)
                    {
                        precision = 9;
                    }
                    break;

                case 'G':
                    // Here we round values less than G15 to 15
                    // digits, G16 and G17 will not be touched
                    if(digits > 7)
                    {
                        precision = 9;
                    }
                    break;
            }

            number = new Number( value, precision );
            return FPNumberToString( number, format, info );

        }

////    public static Decimal ParseDecimal( String s, NumberStyles style )
////    {
////        // rusa: see also Lightning\Src\VM\COMNumber.cpp::ParseDecimal
////        Number number = new Number( s, style );
////        Decimal result;
////        if(!NumberToDecimal( number, out result ))
////        {
////            throw new OverflowException( "Overflow_Decimal" );
////        }
////        return result;
////    }
////
////    public static double ParseDouble( String s, NumberStyles style )
////    {
////        Number number = new Number( s, style );
////        double result;
////        NumberToDouble( number, out result );
////        return result;
////    }
////
////    public static int ParseInt32( String s, NumberStyles style )
////    {
////        // rusa: see also Lightning\Src\VM\COMNumber.cpp::ParseInt32
////        Number number = new Number( s, style );
////        int result;
////        if((style & NumberStyles.AllowHexSpecifier) != 0)
////        {
////            uint temp;
////            if(!HexNumberToUInt32( number, out temp ))
////            {
////                throw new OverflowException( "Overflow_Int32" );
////            }
////            result = unchecked( (int)temp );
////        }
////        else
////        {
////            if(!NumberToInt32( number, out result ))
////            {
////                throw new OverflowException( "Overflow_Int32" );
////            }
////        }
////        return result;
////    }
////
////    public static uint ParseUInt32( String s, NumberStyles style )
////    {
////        // rusa: see also Lightning\Src\VM\COMNumber.cpp::ParseUInt32
////        Number number = new Number( s, style );
////        uint result;
////        if((style & NumberStyles.AllowHexSpecifier) != 0)
////        {
////            if(!HexNumberToUInt32( number, out result ))
////            {
////                throw new OverflowException( "Overflow_UInt32" );
////            }
////        }
////        else
////        {
////            if(!NumberToUInt32( number, out result ))
////            {
////                throw new OverflowException( "Overflow_UInt32" );
////            }
////        }
////        return result;
////    }
////
////    public static long ParseInt64( String s, NumberStyles style )
////    {
////        // rusa: see also Lightning\Src\VM\COMNumber.cpp::ParseInt64
////        Number number = new Number( s, style );
////        long result;
////        if((style & NumberStyles.AllowHexSpecifier) != 0)
////        {
////            ulong temp;
////            if(!HexNumberToUInt64( number, out temp ))
////            {
////                throw new OverflowException( "Overflow_UInt32" );
////            }
////            result = unchecked( (long)temp );
////        }
////        else
////        {
////            if(!NumberToInt64( number, out result ))
////            {
////                throw new OverflowException( "Overflow_UInt32" );
////            }
////        }
////        return result;
////    }
////
////    public static ulong ParseUInt64( String s, NumberStyles style )
////    {
////        // rusa: see also Lightning\Src\VM\COMNumber.cpp::ParseUInt64
////        Number number = new Number( s, style );
////        ulong result;
////        if((style & NumberStyles.AllowHexSpecifier) != 0)
////        {
////            if(!HexNumberToUInt64( number, out result ))
////            {
////                throw new OverflowException( "Overflow_UInt32" );
////            }
////        }
////        else
////        {
////            if(!NumberToUInt64( number, out result ))
////            {
////                throw new OverflowException( "Overflow_UInt32" );
////            }
////        }
////        return result;
////    }
////
////    public static float ParseSingle( String s, NumberStyles style )
////    {
////        return (float)ParseDouble( s, style );
////    }

        private static String FPNumberToString( Number number, String format, NumberFormatInfo info )
        {
            if(number.scale == SCALE_NAN)
            {
                return info.nanSymbol;
            }

            if(number.scale == SCALE_INF)
            {
                return number.negative ? info.negativeInfinitySymbol : info.positiveInfinitySymbol;
            }

            int  digits;
            char fmt = ParseFormatSpecifier( format, out digits );
            if(fmt != 0)
            {
                return number.ToString( fmt, digits, info );
            }

            return number.ToStringFormat( format, info );
        }

        private String ToString( char             format ,
                                 int              digits ,
                                 NumberFormatInfo info   )
        {
            long                      newBufferLen = MIN_BUFFER_SIZE;
            System.Text.StringBuilder sb           = null;
            int                       digCount     = 0;
            char                      ftype        = (char)(format & 0xFFDF);

            switch(ftype)
            {
                case 'F':
                    if(digits < 0) digits = info.numberDecimalDigits;

                    if(this.scale < 0)
                        digCount = 0;
                    else
                        digCount = this.scale + digits;

                    newBufferLen += digCount;

                    // For number and exponent
                    newBufferLen += info.negativeSign.Length;

                    newBufferLen += info.numberDecimalSeparator.Length;

                    sb = new System.Text.StringBuilder( (int)newBufferLen );

                    RoundNumber( this.scale + digits );
                    if(this.negative)
                    {
                        sb.Append( info.negativeSign );
                    }
                    FormatFixed( sb, digits, null, info.numberDecimalSeparator, null, info );
                    break;

                case 'N':
                    // Since we are using digits in our calculation
                    if(digits < 0) digits = info.numberDecimalDigits;

                    if(this.scale < 0)
                        digCount = 0;
                    else
                        digCount = this.scale + digits;

                    newBufferLen += digCount;

                    // For number and exponent
                    newBufferLen += info.negativeSign.Length;

                    // For all the grouping sizes
                    newBufferLen += info.numberGroupSeparator.Length * digCount;
                    newBufferLen += info.numberDecimalSeparator.Length;

                    sb = new System.Text.StringBuilder( (int)newBufferLen );

                    RoundNumber( this.scale + digits );
                    FormatNumber( sb, digits, info );
                    break;

                case 'E':
                    if(digits < 0) digits = 6;
                    digits++;

                    newBufferLen += digits;

                    // For number and exponent
                    newBufferLen += (info.negativeSign.Length +
                                     info.positiveSign.Length) * 2;
                    newBufferLen += info.numberDecimalSeparator.Length;

                    sb = new System.Text.StringBuilder( (int)newBufferLen );

                    RoundNumber( digits );
                    if(this.negative)
                    {
                        sb.Append( info.negativeSign );
                    }

                    FormatScientific( sb, digits, format, info );
                    break;

                case 'G':
                    if(digits < 1) digits = this.precision;
                    newBufferLen += digits;

                    // For number and exponent
                    newBufferLen += (info.negativeSign.Length +
                                     info.positiveSign.Length) * 2;
                    newBufferLen += info.numberDecimalSeparator.Length;

                    sb = new System.Text.StringBuilder( (int)newBufferLen );

                    RoundNumber( digits );
                    if(this.negative)
                    {
                        sb.Append( info.negativeSign );
                    }

                    FormatGeneral( sb, digits, (char)(format - ('G' - 'E')), info );
                    break;

                case 'P':
                    if(digits < 0) digits = info.percentDecimalDigits;
                    this.scale += 2;

                    if(this.scale < 0)
                        digCount = 0;
                    else
                        digCount = this.scale + digits;

                    newBufferLen += digCount;

                    // For number and exponent
                    newBufferLen += info.negativeSign.Length;

                    // For all the grouping sizes
                    newBufferLen += info.percentGroupSeparator.Length * digCount;
                    newBufferLen += info.percentDecimalSeparator.Length;
                    newBufferLen += info.percentSymbol.Length;

                    sb = new System.Text.StringBuilder( (int)newBufferLen );

                    RoundNumber( this.scale + digits );
                    FormatPercent( sb, digits, info );
                    break;

                default:
#if EXCEPTION_STRINGS
                    throw new FormatException( "Format_BadFormatSpecifier" );
#else
                    throw new FormatException();
#endif
                // COMPlusThrow(kFormatException,L"Format_BadFormatSpecifier");
            }

            return sb.ToString();
        }

////    private static bool NumberToDecimal( Number number, out Decimal value )
////    {
////        throw new Exception( "System.Number.NumberToDecimal not implemented in Bartok!" );
////    }
////
////    private static void NumberToDouble( Number number, out double value )
////    {
////        if(number.digits[0] != 0)
////        {
////            byte[] buffer = new byte[64];
////            int index = 0;
////            char[] src = number.digits;
////            if(number.negative) buffer[index++] = (byte)'-';
////            for(int j = 0; j < src.Length; j++)
////            {
////                if(src[j] == '\0')
////                {
////                    break;
////                }
////                else
////                {
////                    buffer[index++] = (byte)src[j];
////                }
////            }
////            int i = number.scale - number.precision;
////            if(i != 0)
////            {
////                buffer[index++] = (byte)'e';
////                if(i < 0)
////                {
////                    buffer[index++] = (byte)'-';
////                    i = -i;
////                }
////                if(i >= 100)
////                {
////                    if(i > 999) i = 999;
////                    buffer[index++] = (byte)(i / 100 + (int)'0');
////                    i %= 100;
////                }
////                buffer[index++] = (byte)(i / 10 + (int)'0');
////                buffer[index++] = (byte)(i % 10 + (int)'0');
////            }
////            buffer[index] = (byte)'\0';
////            value = atof( buffer );
////        }
////        else
////        {
////            value = 0;
////        }
////
////    }
////
////    // int NumberToInt32(NUMBER* number, int* value)
////    // Returns 1 (true) on success, 0 (false) for fail.
////    private static bool NumberToInt32( Number number, out int value )
////    {
////        // markples: see also Lightning\Src\VM\COMNumber.cpp::NumberToInt32
////        int i = number.scale;
////        if(i > Int32Precision || i < number.precision) goto broken;
////        char[] c = number.digits;
////        int p = 0;
////        int n = 0;
////        while(--i >= 0)
////        {
////            if((uint)n > (0x7FFFFFFF / 10)) goto broken;
////            n *= 10;
////            if(c[p] != '\0') n += c[p++] - '0';
////        }
////        if(number.negative)
////        {
////            n = -n;
////            if(n > 0) goto broken;
////        }
////        else
////        {
////            if(n < 0) goto broken;
////        }
////        value = n;
////        return true;
////
////        broken:
////        value = 0;
////        return false;
////    }
////
////    private static bool NumberToUInt32( Number number, out uint value )
////    {
////        // markples: see also Lightning\Src\VM\COMNumber.cpp::NumberToInt32
////        int i = number.scale;
////        if(i > UInt32Precision || i < number.precision) goto broken;
////        if(number.negative) goto broken;
////        char[] c = number.digits;
////        int p = 0;
////        uint n = 0;
////        while(--i >= 0)
////        {
////            if((uint)n > (0xFFFFFFFF / 10)) goto broken;
////            n *= 10;
////            if(c[p] != '\0') n += (uint)(c[p++] - '0');
////        }
////        value = n;
////        return true;
////
////        broken:
////        value = 0;
////        return false;
////    }
////
////    private static bool NumberToInt64( Number number, out long value )
////    {
////        // markples: see also Lightning\Src\VM\COMNumber.cpp::NumberToInt32
////        int i = number.scale;
////        if(i > UInt64Precision || i < number.precision) goto broken;
////        char[] c = number.digits;
////        int p = 0;
////        long n = 0;
////        while(--i >= 0)
////        {
////            if((ulong)n > (0x7FFFFFFFFFFFFFFF / 10)) goto broken;
////            n *= 10;
////            if(c[p] != '\0') n += c[p++] - '0';
////        }
////        if(number.negative)
////        {
////            n = -n;
////            if(n > 0) goto broken;
////        }
////        else
////        {
////            if(n < 0) goto broken;
////        }
////        value = n;
////        return true;
////
////        broken:
////        value = 0;
////        return false;
////    }
////
////    private static bool NumberToUInt64( Number number, out ulong value )
////    {
////        // markples: see also Lightning\Src\VM\COMNumber.cpp::NumberToInt32
////        int i = number.scale;
////        if(i > Int64Precision || i < number.precision) goto broken;
////        if(number.negative) goto broken;
////        char[] c = number.digits;
////        int p = 0;
////        ulong n = 0;
////        while(--i >= 0)
////        {
////            if((ulong)n > (0xFFFFFFFFFFFFFFFF / 10)) goto broken;
////            n *= 10;
////            if(c[p] != '\0') n += (ulong)(c[p++] - '0');
////        }
////        value = n;
////        return true;
////
////        broken:
////        value = 0;
////        return false;
////    }
////
////    private static bool HexNumberToUInt32( Number number, out uint value )
////    {
////        // markples: see also Lightning\Src\VM\COMNumber.cpp::HexNumberToInt32
////        int i = number.scale;
////        if(i > UInt32Precision || i < number.precision)
////        {
////            goto broken;
////        }
////        if(number.negative)
////        {
////            goto broken;
////        }
////        char[] c = number.digits;
////        int p = 0;
////        uint n = 0;
////        while(--i >= 0)
////        {
////            if((uint)n > (0xFFFFFFFF / 16))
////            {
////                goto broken;
////            }
////            n *= 16;
////            if(c[p] >= '0' && c[p] <= '9')
////            {
////                n += (uint)(c[p++] - '0');
////            }
////            else if(c[p] >= 'a' && c[p] <= 'f')
////            {
////                n += 10 + (uint)(c[p++] - 'a');
////            }
////            else if(c[p] >= 'A' && c[p] <= 'F')
////            {
////                n += 10 + (uint)(c[p++] - 'A');
////            }
////        }
////        value = n;
////        return true;
////
////        broken:
////        value = 0;
////        return false;
////    }
////
////    private static bool HexNumberToUInt64( Number number, out ulong value )
////    {
////        // markples: see also Lightning\Src\VM\COMNumber.cpp::HexNumberToInt32
////        int i = number.scale;
////        if(i > Int64Precision || i < number.precision)
////        {
////            goto broken;
////        }
////        if(number.negative)
////        {
////            goto broken;
////        }
////        char[] c = number.digits;
////        int p = 0;
////        ulong n = 0;
////        while(--i >= 0)
////        {
////            if((ulong)n > (0xFFFFFFFFFFFFFFFF / 16))
////            {
////                goto broken;
////            }
////            n *= 16;
////            if(c[p] >= '0' && c[p] <= '9')
////            {
////                n += (uint)(c[p++] - '0');
////            }
////            else if(c[p] >= 'a' && c[p] <= 'f')
////            {
////                n += 10 + (uint)(c[p++] - 'a');
////            }
////            else if(c[p] >= 'A' && c[p] <= 'F')
////            {
////                n += 10 + (uint)(c[p++] - 'A');
////            }
////        }
////        value = n;
////        return true;
////
////        broken:
////        value = 0;
////        return false;
////    }
    
        private static char ParseFormatSpecifier( string format, out int digits )
        {
            if(format != null)
            {
                int index = 0;
                char c = Get( format, index );
                if(c != 0)
                {
                    if(c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z')
                    {
                        index++;
                        int n = -1;
                        c = Get( format, index );
                        if(c >= '0' && c <= '9')
                        {
                            n = (c - '0');
                            index++;
                            c = Get( format, index );
                            while(c >= '0' && c <= '9')
                            {
                                n = n * 10 + (c - '0');
                                index++;
                                c = Get( format, index );
                                if(n >= 10) break;
                            }
                        }
                        if(c == 0)
                        {
                            digits = n;
                            return Get( format, 0 );
                        }
                    }
                    digits = -1;
                    return '\0';
                }
            }
            digits = -1;
            return 'G';
        }


        struct ToStringFormatState
        {
            internal int    section;
            internal int    sectionOffset;
            internal int    firstDigit;
            internal int    lastDigit;
            internal int    digitCount;
            internal int    scaleAdjust;

            internal bool   scientific;
            internal int    decimalPos;
            internal int    percent;
            internal int    permille;

            internal int    thousandSeps;
            internal int    thousandCount;
            internal int    thousandPos;

            internal String format;
            internal int    src;

            //--/

            internal void Reset()
            {
                decimalPos   = -1;
                firstDigit   = 0x7FFFFFFF;
                lastDigit    = 0;
                scientific   = false;
                percent      = 0;
                permille     = 0;
                thousandSeps = 0;

                src          = sectionOffset;
            }

            internal char DecodeFormat( char ch )
            {
                digitCount  = 0;
                thousandPos = -1;
                scaleAdjust = 0;

                while(ch != '\0' && ch != ';')
                {
                    switch(ch)
                    {
                        case '#':
                            digitCount++;
                            break;

                        case '0':
                            if(firstDigit == 0x7FFFFFFF)
                            {
                                firstDigit = digitCount;
                            }
                            digitCount++;
                            lastDigit = digitCount;
                            break;

                        case '.':
                            if(decimalPos < 0)
                            {
                                decimalPos = digitCount;
                            }
                            break;

                        case ',':
                            if(digitCount > 0 && decimalPos < 0)
                            {
                                if(thousandPos >= 0)
                                {
                                    if(thousandPos == digitCount)
                                    {
                                        thousandCount++;
                                        break;
                                    }

                                    thousandSeps = 1;
                                }

                                thousandPos   = digitCount;
                                thousandCount = 1;
                            }
                            break;

                        case '%':
                            percent++;
                            scaleAdjust += 2;
                            break;

                        case '\u2030':
                            permille++;
                            scaleAdjust += 3;
                            break;

                        case '\'':
                        case '"':
                            while(true)
                            {
                                char ch2 = PeekAt( 0 );

                                if(ch2 == 0  ||
                                   ch2 == ch  )
                                {
                                    break;
                                }

                                src++;
                            }
                            break;

                        case '\\':
                            if(PeekAt( 0 ) != 0)
                            {
                                src++;
                            }
                            break;

                        case 'E':
                            {
                                char ch2 = PeekAt( 0 );
                                char ch3 = PeekAt( 1 );

                                if(                                 ch2 == '0'  ||
                                   ((ch2 == '+' || ch2 == '-'  ) && ch3 == '0')  )
                                {
                                    MoveToEnd();
                                    scientific = true;
                                }
                            }
                            break;
                    }

                    ch = Get();
                }

                if(decimalPos < 0)
                {
                    decimalPos = digitCount;
                }

                if(thousandPos >= 0)
                {
                    if(thousandPos == decimalPos)
                    {
                        scaleAdjust -= thousandCount * 3;
                    }
                    else
                    {
                        thousandSeps = 1;
                    }
                }

                return ch;
            }

            internal char Get()
            {
                if(src < format.Length)
                {
                    return format[src++];
                }
                
                return '\0';
            }

            internal char PeekAt( int offset )
            {
                if(src + offset < format.Length)
                {
                    return format[src+offset];
                }
                
                return '\0';
            }

            internal void Advance()
            {
                src++;
            }

            private void MoveToEnd()
            {
                src = format.Length;
            }
        }

        // rusa: see also Lightning\Src\VM\COMNumber.cpp::NumberToStringFormat
        private String ToStringFormat( String           format ,
                                       NumberFormatInfo info   )
        {
            ToStringFormatState state = new ToStringFormatState();

            state.format        = format;
            state.section       = (this.digits[0] == 0 ? 2 : (this.negative ? 1 : 0));
            state.sectionOffset = FindSection( state.format, state.section );

            while(true)
            {
                state.Reset();

                char ch = state.DecodeFormat( state.Get() );

                if(this.digits[0] != 0)
                {
                    this.scale += state.scaleAdjust;

                    int pos = (state.scientific ?
                               state.digitCount :
                               (this.scale + state.digitCount - state.decimalPos));

                    this.RoundNumber( pos );

                    if(this.digits[0] == 0)
                    {
                        state.src = FindSection( state.format, 2 );
                        if(state.src != state.sectionOffset)
                        {
                            state.sectionOffset = state.src;
                            continue;
                        }
                    }
                }
                else
                {
                    this.negative = false;
                }
                break;
            }

            state.firstDigit = (state.firstDigit < state.decimalPos) ? state.decimalPos - state.firstDigit : 0;
            state.lastDigit  = (state.lastDigit  > state.decimalPos) ? state.decimalPos - state. lastDigit : 0;

            int digPos;
            int adjust;

            if(state.scientific)
            {
                digPos = state.decimalPos;
                adjust = 0;
            }
            else
            {
                digPos = (this.scale > state.decimalPos) ? this.scale : state.decimalPos;
                adjust = this.scale - state.decimalPos;
            }

            state.src = state.sectionOffset;

            ulong adjustLength   = (adjust > 0) ? (uint)adjust : 0U;
            int   bufferLength   = 125;
            int[] thousandSepPos = null;
            int   thousandSepCtr = -1;

            if(state.thousandSeps != 0)
            {
                int groupSizeLen = info.numberGroupSizes.Length;
                if(groupSizeLen == 0)
                {
                    state.thousandSeps = 0;
                }
                else
                {
                    thousandSepPos = new int[bufferLength];

                    long groupTotalSizeCount = info.numberGroupSizes[0];
                    int  groupSizeIndex      = 0;
                    int  groupSize           = (int)groupTotalSizeCount;
                    int  totalDigits         = digPos + ((adjust < 0) ? adjust : 0);
                    int  numDigits           = (state.firstDigit > totalDigits) ? state.firstDigit : totalDigits;

                    while(numDigits > groupTotalSizeCount)
                    {
                        if(groupSize == 0)
                        {
                            break;
                        }

                        thousandSepCtr++;
                        thousandSepPos[thousandSepCtr] = (int)groupTotalSizeCount;

                        if(groupSizeIndex < groupSizeLen - 1)
                        {
                            groupSizeIndex++;
                            groupSize = info.numberGroupSizes[groupSizeIndex];
                        }

                        groupTotalSizeCount += groupSize;
                        if(bufferLength - thousandSepCtr < 10)
                        {
                            bufferLength *= 2;
                            int[] oldThousandSepPos = thousandSepPos;
                            thousandSepPos = new int[bufferLength];
                            for(int i = 0; i < thousandSepCtr; i++)
                            {
                                thousandSepPos[i] = oldThousandSepPos[i];
                            }
                        }
                    }

                    adjustLength += (ulong)((thousandSepCtr + 1) * info.numberGroupSeparator.Length);
                }
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder( 128 );

            if(this.negative && state.sectionOffset == 0)
            {
                sb.Append( info.negativeSign );
            }

            {
                char ch          = state.Get();
                int  digitOffset = 0;

                while(ch != 0 && ch != ';')
                {
                    switch(ch)
                    {
                        case '#':
                        case '0':
                            {
                                while(adjust > 0)
                                {
                                    sb.Append( this.digits[digitOffset] != 0 ? this.digits[digitOffset++] : '0' );

                                    if(state.thousandSeps != 0 &&
                                       digPos             >  1 &&
                                       thousandSepCtr     >= 0  )
                                    {
                                        if(digPos == thousandSepPos[thousandSepCtr] + 1)
                                        {
                                            sb.Append( info.numberGroupSeparator );
                                            thousandSepCtr--;
                                        }
                                    }

                                    digPos--;
                                    adjust--;
                                }

                                if(adjust < 0)
                                {
                                    adjust++;
                                    ch = (digPos <= state.firstDigit) ? '0' : '\0';
                                }
                                else
                                {
                                    ch = ((this.digits[digitOffset] != 0) ? this.digits[digitOffset++] : ((digPos > state.lastDigit) ? '0' : '\0'));
                                }

                                if(ch != 0)
                                {
                                    if(digPos == 0)
                                    {
                                        sb.Append( info.numberDecimalSeparator );
                                    }

                                    sb.Append( ch );

                                    if(state.thousandSeps != 0 &&
                                       digPos             >  1 &&
                                       thousandSepCtr     >= 0  )
                                    {
                                        if(digPos == thousandSepPos[thousandSepCtr] + 1)
                                        {
                                            sb.Append( info.numberGroupSeparator );
                                            thousandSepCtr--;
                                        }
                                    }
                                }

                                digPos--;
                                break;
                            }

                        case '.':
                            break;

                        case '\u2030':
                            sb.Append( info.perMilleSymbol );
                            break;

                        case '%':
                            sb.Append( info.percentSymbol );
                            break;

                        case ',':
                            break;

                        case '\'':
                        case '"':
                            while(true)
                            {
                                char ch2 = state.Get();

                                if(ch2 == 0 || ch2 == ch)
                                {
                                    break;
                                }

                                sb.Append( ch2 );
                            }
                            break;

                        case '\\':
                            {
                                char ch2 = state.Get();

                                if(ch2 != 0)
                                {
                                    sb.Append( ch2 );
                                }
                            }
                            break;

                        case 'E':
                        case 'e':
                            {
                                String sign = null;
                                int    i    = 0;

                                if(state.scientific)
                                {
                                    char ch2 = state.PeekAt( 0 );
                                    char ch3 = state.PeekAt( 1 );

                                    if(ch2 == '0')
                                    {
                                        i++;
                                    }
                                    else if(ch2 == '+' &&
                                            ch3 == '0')
                                    {
                                        sign = info.positiveSign;
                                    }
                                    else if(ch2 == '-' &&
                                            ch3 == '0')
                                    {
                                        // Do nothing
                                    }
                                    else
                                    {
                                        sb.Append( ch );
                                        break;
                                    }

                                    state.Advance();

                                    while(state.PeekAt( 0 ) == '0')
                                    {
                                        state.Advance();
                                        i++;
                                    }

                                    if(i > 10)
                                    {
                                        i = 10;
                                    }

                                    int exp = ((this.digits[0] == 0) ? 0 : (this.scale - state.decimalPos));

                                    FormatExponent( sb, exp, ch, sign, info.negativeSign, i );
                                    state.scientific = false;
                                }
                                else
                                {
                                    sb.Append( ch );

                                    while(state.PeekAt( 0 ) != 0)
                                    {
                                        sb.Append( state.Get() );
                                    }
                                }
                                break;
                            }

                        default:
                            sb.Append( ch );
                            break;
                    }

                    ch = state.Get();
                }
            }

            return sb.ToString();
        }

        private static int FindSection( String format, int section )
        {
            if(section == 0)
            {
                return 0;
            }

            int src = 0;
            while(true)
            {
                char ch = Get( format, src );
                src++;
                switch(ch)
                {
                    case '\'':
                    case '"':
                        while(Get( format, src ) != '\0' && Get( format, src ) != ch)
                        {
                            src++;
                        }
                        break;
                    case '\\':
                        if(Get( format, src ) != 0)
                        {
                            src++;
                        }
                        break;
                    case ';':
                        section--;
                        if(section != 0)
                        {
                            break;
                        }
                        if(Get( format, src ) != 0 && Get( format, src ) != ';')
                        {
                            return src;
                        }
                        return 0;
                    case '\0':
                        return 0;
                }
            }
        }

        // markples: see also Lightning\Src\VM\COMNumber.cpp::
        // STRINGREF Int32ToDecStr(int value, int digits, STRINGREF sNegative)
        private static String Int32ToDecString( int value, int digits, String sign )
        {
            //THROWSCOMPLUSEXCEPTION();
            //CQuickBytes buf;

            int bufferLength = 100; // was UINT
            int negLength = 0;
            // wchar* src = NULL;
            if(digits < 1) digits = 1;

            if(value < 0)
            {
                //src = sNegative->GetBuffer();
                negLength = sign.Length;
                if(negLength > 85)
                {
                    bufferLength = negLength + 15; //was implicit C++ cast
                }
            }

            char[] buffer = new char[bufferLength];
            //if (!buffer)
            //    COMPlusThrowOM();

            int p = Int32ToDecChars( buffer, bufferLength, (uint)(value >= 0 ? value : -value), digits );
            if(value < 0)
            {
                for(int i = negLength - 1; i >= 0; i--)
                {
                    buffer[--p] = sign[i];
                    // *(--p) = *(src+i);
                }
            }

            // _ASSERTE( buffer + bufferLength - p >=0 && buffer <= p);
            return new string( buffer, p, bufferLength - p );
            // return COMString::NewString(p, buffer + bufferLength - p);
        }

        private static String Int32ToHexString( uint value,
                                               char hexBase,
                                               int digits )
        {
            char[] buffer = new char[100];
            if(digits < 1)
            {
                digits = 1;
            }
            int start = Int32ToHexChars( buffer, 100, value, hexBase, digits );
            return new string( buffer, start, 100 - start );
        }

        private static int Int32ToHexChars( char[] buffer, int offset, uint value,
                                           char hexBase, int digits )
        {
            while(digits > 0 || value != 0)
            {
                digits--;
                uint digit = value & 0xf;
                offset--;
                buffer[offset] = (char)(digit + (digit < 10 ? '0' : hexBase));
                value >>= 4;
            }
            return offset;
        }

        private static String UInt32ToDecString( uint value, int digits )
        {
            int bufferLength = 100;
            if(digits < 1) digits = 1;
            char[] buffer = new char[bufferLength];
            int p = Int32ToDecChars( buffer, bufferLength, value, digits );
            return new string( buffer, p, bufferLength - p );
        }

        // used to be macros
        private static uint LO32( ulong x ) { return (uint)(x); }
        private static uint HI32( ulong x )
        {
            return (uint)((((ulong)x) & 0xFFFFFFFF00000000L) >> 32);
        }

        // markples: see also Lightning\Src\VM\COMNumber.cpp::
        //STRINGREF Int64ToDecStr(__int64 value, int digits, STRINGREF sNegative)
        private static String Int64ToDecString( long value, int digits, String sign )
        {
            //THROWSCOMPLUSEXCEPTION();
            //CQuickBytes buf;

            if(digits < 1) digits = 1;
            int signNum = (int)HI32( unchecked( (ulong)value ) );
            int bufferLength = 100; // was UINT

            if(signNum < 0)
            {
                value = -value;
                int negLength = sign.Length;
                if(negLength > 75)
                {// Since max is 20 digits
                    bufferLength = negLength + 25;
                }
            }

            char[] buffer = new char[bufferLength];
            //if (!buffer)
            //    COMPlusThrowOM();
            int p = bufferLength;
            while(HI32( unchecked( (ulong)value ) ) != 0)
            {
                uint rem;
                value = (long)Int64DivMod1E9( (ulong)value, out rem );

                p = Int32ToDecChars( buffer, p, rem, 9 );
                digits -= 9;
            }
            p = Int32ToDecChars( buffer, p, LO32( unchecked( (ulong)value ) ), digits );
            if(signNum < 0)
            {
                for(int i = sign.Length - 1; i >= 0; i--)
                {
                    buffer[--p] = sign[i];
                }
            }
            return new string( buffer, p, bufferLength - p );
        }

        private static int Int64ToHexChars( char[] buffer, int offset, ulong value,
                                           char hexBase, int digits )
        {
            while(digits > 0 || value != 0)
            {
                digits--;
                uint digit = ((uint)value) & 0xf;
                offset--;
                buffer[offset] = (char)(digit + (digit < 10 ? '0' : hexBase));
                value >>= 4;
            }
            return offset;
        }

        private static String Int64ToHexString( ulong value,
                                               char hexBase,
                                               int digits )
        {
            char[] buffer = new char[100];
            if(digits < 1)
            {
                digits = 1;
            }
            int start = Int64ToHexChars( buffer, 100, value, hexBase, digits );
            return new string( buffer, start, 100 - start );
        }

        private static String UInt64ToDecString( ulong value, int digits )
        {
            int bufferLength = 100;
            char[] buffer = new char[bufferLength];
            if(digits < 1) digits = 1;
            int p = bufferLength;
            while(HI32( value ) != 0)
            {
                uint rem;
                value = Int64DivMod1E9( value, out rem );
                p = Int32ToDecChars( buffer, p, rem, 9 );
                digits -= 9;
            }
            p = Int32ToDecChars( buffer, p, LO32( value ), digits );
            return new string( buffer, p, bufferLength - p );
        }

        // markples: see also Lightning\Src\VM\COMNumber.cpp::
        // wchar* Int32ToDecChars(wchar* p, unsigned int value, int digits)
        // There's a x86 asm version there too.
        private static int Int32ToDecChars( char[] buffer      ,
                                            int    bufferIndex ,
                                            uint   value       ,
                                            int    digits      )
        {
            while(--digits >= 0 || value != 0)
            {
                buffer[--bufferIndex] = (char)(value % 10 + '0');
                value /= 10;
            }

            return bufferIndex;
        }

        private static int Int64ToDecChars( char[] buffer      ,
                                            int    bufferIndex ,
                                            ulong  value       ,
                                            int    digits      )
        {
            while(--digits >= 0 || value != 0)
            {
                buffer[--bufferIndex] = (char)(value % 10 + '0');
                value /= 10;
            }

            return bufferIndex;
        }

        // markples: see also Lightning\Src\VM\COMNumber.cpp::
        // void RoundNumber(NUMBER* number, int pos)
        private void RoundNumber( int pos )
        {
            int i = 0;
            while(i < pos && this.digits[i] != 0) i++;
            if(i == pos && this.digits[i] >= '5')
            {
                while(i > 0 && this.digits[i - 1] == '9') i--;
                if(i > 0)
                {
                    this.digits[i - 1]++;
                }
                else
                {
                    this.scale++;
                    this.digits[0] = '1';
                    i = 1;
                }
            }
            else
            {
                while(i > 0 && this.digits[i - 1] == '0') i--;
            }
            if(i == 0)
            {
                this.scale = 0;
                this.negative = false;
            }
            this.digits[i] = '\0';
        }

        private void FormatExponent( System.Text.StringBuilder sb         ,
                                     int                       value      ,
                                     char                      expChar    ,
                                     String                    posSignStr ,
                                     String                    negSignStr ,
                                     int                       minDigits  )
        {
            char[] digits = new char[11];

            sb.Append( expChar );

            if(value < 0)
            {
                sb.Append( negSignStr );
                value = -value;
            }
            else
            {
                if(posSignStr != null)
                {
                    sb.Append( posSignStr );
                }
            }

            // REVIEW: (int) was implicit in C++ code
            int p = Int32ToDecChars( digits, 10, checked( (uint)value ), minDigits );

            int i = 10 - p;
            while(--i >= 0)
            {
                sb.Append( digits[p++] );
            }
        }

        private void FormatGeneral( System.Text.StringBuilder sb      ,
                                    int                       digits  ,
                                    char                      expChar ,
                                    NumberFormatInfo          info    )
        {
            int  digPos     = this.scale;
            bool scientific = false;

            if(digPos > digits || digPos < -3)
            {
                digPos = 1;
                scientific = true;
            }

            int dig = 0; // number->digits;
            if(digPos > 0)
            {
                do
                {
                    sb.Append( this.digits[dig] != 0 ? this.digits[dig++] : '0' );
                } while(--digPos > 0);
            }
            else
            {
                sb.Append( '0' );
            }

            if(this.digits[dig] != 0)
            {
                sb.Append( info.numberDecimalSeparator );

                while(digPos < 0)
                {
                    sb.Append( '0' );
                    digPos++;
                }

                do
                {
                    sb.Append( this.digits[dig++] );
                } while(this.digits[dig] != 0);
            }

            if(scientific)
            {
                FormatExponent( sb, this.scale - 1, expChar, info.positiveSign, info.negativeSign, 2 );
            }
        }

        private void FormatScientific( System.Text.StringBuilder sb      ,
                                       int                       digits  ,
                                       char                      expChar ,
                                       NumberFormatInfo          info    )
        {
            int dig = 0;  // number->digits;

            sb.Append( this.digits[dig] != 0 ? this.digits[dig++] : '0' );

            if(digits != 1)
            {
                // For E0 we would like to suppress the decimal point
                sb.Append( info.numberDecimalSeparator );
            }

            while(--digits > 0)
            {
                sb.Append( this.digits[dig] != 0 ? this.digits[dig++] : '0' );
            }

            int e = this.digits[0] == 0 ? 0 : this.scale - 1;

            FormatExponent( sb, e, expChar, info.positiveSign, info.negativeSign, 3 );
        }

        // REVIEW: call the real wcslen?
        private static int wcslen( char[] c, int i )
        {
            int j;
            for(j = i; j < c.Length; ++j)
            {
                if(c[j] == '\0') break;
            }
            return j - i;
        }

        private void FormatFixed( System.Text.StringBuilder sb          ,
                                  int                       digits      ,
                                  int[]                     groupDigits ,
                                  String                    sDecimal    ,
                                  String                    sGroup      ,
                                  NumberFormatInfo          info        )
        {
            //          int bufferSize = 0;   // the length of the result buffer string.
            int digPos = this.scale;
            int dig = 0; // = number->digits;

            if(digPos > 0)
            {
                if(groupDigits != null)
                {
                    // index into the groupDigits array.
                    int groupSizeIndex = 0;
                    // the current total of group size.
                    int groupSizeCount = groupDigits[groupSizeIndex];
                    // the length of groupDigits array.
                    int groupSizeLen = groupDigits.Length;
                    // the length of the result buffer string.
                    int bufferSize = digPos;
                    // the length of the group separator string.
                    int groupSeparatorLen = sGroup.Length;
                    // the current group size.
                    int groupSize = 0;

                    //
                    // Find out the size of the string buffer for the result.
                    //
                    if(groupSizeLen != 0) // You can pass in 0 length arrays
                    {
                        while(digPos > groupSizeCount)
                        {
                            groupSize = groupDigits[groupSizeIndex];
                            if(groupSize == 0)
                            {
                                break;
                            }

                            bufferSize += groupSeparatorLen;
                            if(groupSizeIndex < groupSizeLen - 1)
                            {
                                groupSizeIndex++;
                            }
                            groupSizeCount += groupDigits[groupSizeIndex];
                            if(groupSizeCount < 0 || bufferSize < 0)
                            {
                                throw new ArgumentOutOfRangeException();
                                // if we overflow
                                //COMPlusThrow(kArgumentOutOfRangeException);
                            }
                        }
                        // If you passed in an array with one
                        // entry as 0, groupSizeCount == 0
                        if(groupSizeCount == 0)
                            groupSize = 0;
                        else
                            groupSize = groupDigits[0];
                    }

                    groupSizeIndex = 0;

                    int digitCount = 0;
                    int digLength  = (int)wcslen( this.digits, dig );
                    int digStart   = (digPos < digLength) ? digPos : digLength;

                    char[] buffer = new char[bufferSize];
                    int    p      = bufferSize - 1;

                    for(int i = digPos - 1; i >= 0; i--)
                    {
                        buffer[p--] = (i < digStart) ? this.digits[dig + i] : '0';

                        if(groupSize > 0)
                        {
                            digitCount++;
                            if(digitCount == groupSize && i != 0)
                            {
                                for(int j = groupSeparatorLen - 1; j >= 0; j--)
                                {
                                    buffer[p--] = sGroup[j];
                                }

                                if(groupSizeIndex < groupSizeLen - 1)
                                {
                                    groupSizeIndex++;
                                    groupSize = groupDigits[groupSizeIndex];
                                }
                                digitCount = 0;
                            }
                        }
                    }

                    sb.Append( buffer );

                    dig += digStart;
                }
                else
                {
                    do
                    {
                        sb.Append( this.digits[dig] != 0 ? this.digits[dig++] : '0' );
                    } while(--digPos > 0);
                }
            }
            else
            {
                sb.Append( '0' );
            }

            if(digits > 0)
            {
                sb.Append( sDecimal );

                while(digPos < 0 && digits > 0)
                {
                    sb.Append( '0' );

                    digPos++;
                    digits--;
                }

                while(digits > 0)
                {
                    char ch = this.digits[dig];

                    if(ch == 0)
                    {
                        ch = '0';
                    }
                    else
                    {
                        dig++;
                    }

                    sb.Append( ch );

                    digits--;
                }
            }
        }

        private void FormatNumber( System.Text.StringBuilder sb     ,
                                   int                       digits ,
                                   NumberFormatInfo          info   )
        {
            String fmt      = this.negative ? negNumberFormats[info.numberNegativePattern] : posNumberFormat;
            int    fmtIndex = 0;

            for(; fmtIndex < fmt.Length; fmtIndex++)
            {
                char ch = fmt[fmtIndex];

                switch(ch)
                {
                    case '#':
                        FormatFixed( sb, digits,
                                     info.numberGroupSizes,
                                     info.numberDecimalSeparator,
                                     info.numberGroupSeparator, info );
                        break;

                    case '-':
                        sb.Append( info.negativeSign );
                        break;

                    default:
                        sb.Append( ch );
                        break;
                }
            }
        }

        private void FormatPercent( System.Text.StringBuilder sb     ,
                                    int                       digits ,
                                    NumberFormatInfo          info   )
        {
            String fmt      = this.negative ? negPercentFormats[info.percentNegativePattern] :
                                              posPercentFormats[info.percentPositivePattern];
            int    fmtIndex = 0;
            char   ch;

            while((ch = fmt[fmtIndex++]) != 0)
            {
                switch(ch)
                {
                    case '#':
                        FormatFixed( sb, digits,
                                     info.percentGroupSizes,
                                     info.percentDecimalSeparator,
                                     info.percentGroupSeparator, info );
                        break;

                    case '-':
                        sb.Append( info.negativeSign );
                        break;

                    case '%':
                        sb.Append( info.percentSymbol );
                        break;

                    default:
                        sb.Append( ch );
                        break;
                }
            }
        }

        private uint D32DivMod1E9( uint hi32, uint lo32, out uint newlo32 )
        {
            ulong n = (((ulong)hi32) << 32) | lo32;
            newlo32 = (uint)(n / 1000000000);
            return (uint)(n % 1000000000);
        }

////    private uint DecDivMod1E9( ref Decimal value )
////    {
////        uint newhi, newmid, newlo;
////        uint result = D32DivMod1E9( D32DivMod1E9( D32DivMod1E9( 0,
////                                                             value.hi32,
////                                                             out newhi ),
////                                                value.mid32,
////                                                out newmid ),
////                                   value.lo32,
////                                   out newlo );
////        value.hi32 = newhi;
////        value.mid32 = newmid;
////        value.lo32 = newlo;
////        return result;
////    }

        // markples: see also Lightning\Src\VM\COMNumber.cpp::
        // unsigned int Int64DivMod1E9(unsigned __int64* value)
        // There's a x86 asm version there too.
        // The interface is different because Bartok does not support
        // taking the address of 64 bit values.
        private static ulong Int64DivMod1E9( ulong value, out uint rem )
        {
            rem = (uint)(value % 1000000000);
            value /= 1000000000;
            return value;
        }

        public static bool TryParseDouble( String s, NumberStyles style, out double result )
        {
#if EXCEPTION_STRINGS
            throw new Exception( "System.Number.TryParseDouble not implemented in Bartok!" );
#else
            throw new Exception();
#endif
        }


        private const int DecimalPrecision = 29;
    }
}
