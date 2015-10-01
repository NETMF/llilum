// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
namespace System
{
    using System;
    using System.Globalization;
    using System.Text;
////using Microsoft.Win32;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;

    // Represents a Globally Unique Identifier.
    [Serializable]
    [StructLayout( LayoutKind.Sequential )]
    public struct Guid : /*IFormattable,*/ IComparable, IComparable< Guid >, IEquatable< Guid >
    {
        public static readonly Guid Empty = new Guid();

        ////////////////////////////////////////////////////////////////////////////////
        //  Member variables
        ////////////////////////////////////////////////////////////////////////////////
        private int   _a;
        private short _b;
        private short _c;
        private byte  _d;
        private byte  _e;
        private byte  _f;
        private byte  _g;
        private byte  _h;
        private byte  _i;
        private byte  _j;
        private byte  _k;
    
    
    
        ////////////////////////////////////////////////////////////////////////////////
        //  Constructors
        ////////////////////////////////////////////////////////////////////////////////
    
        // Creates a new guid from an array of bytes.
        //
        public Guid( byte[] b )
        {
            if(b == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "b" );
#else
                throw new ArgumentNullException();
#endif
            }

            if(b.Length != 16)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_GuidArrayCtor" ), "16" ) );
#else
                throw new ArgumentException();
#endif
            }
    
            _a = ((int)b[3] << 24) | ((int)b[2] << 16) | ((int)b[1] << 8) | b[0];
            _b = (short)(((int)b[5] << 8) | b[4]);
            _c = (short)(((int)b[7] << 8) | b[6]);
            _d = b[8];
            _e = b[9];
            _f = b[10];
            _g = b[11];
            _h = b[12];
            _i = b[13];
            _j = b[14];
            _k = b[15];
        }
    
        [CLSCompliant( false )]
        public Guid( uint   a ,
                     ushort b ,
                     ushort c ,
                     byte   d ,
                     byte   e ,
                     byte   f ,
                     byte   g ,
                     byte   h ,
                     byte   i ,
                     byte   j ,
                     byte   k )
        {
            _a = (int)a;
            _b = (short)b;
            _c = (short)c;
            _d = d;
            _e = e;
            _f = f;
            _g = g;
            _h = h;
            _i = i;
            _j = j;
            _k = k;
        }
    
////    // Creates a new guid based on the value in the string.  The value is made up
////    // of hex digits speared by the dash ("-"). The string may begin and end with
////    // brackets ("{", "}").
////    //
////    // The string must be of the form dddddddd-dddd-dddd-dddd-dddddddddddd. where
////    // d is a hex digit. (That is 8 hex digits, followed by 4, then 4, then 4,
////    // then 12) such as: "CA761232-ED42-11CE-BACD-00AA0057B223"
////    //
////    public Guid( String g )
////    {
////        if(g == null)
////        {
////            throw new ArgumentNullException( "g" );
////        }
////
////        int startPos = 0;
////        int temp;
////        long templ;
////        int currentPos = 0;
////
////        try
////        {
////            // Check if it's of the form dddddddd-dddd-dddd-dddd-dddddddddddd
////            if(g.IndexOf( '-', 0 ) >= 0)
////            {
////
////                String guidString = g.Trim();  //Remove Whitespace
////
////                // check to see that it's the proper length
////                if(guidString[0] == '{')
////                {
////                    if(guidString.Length != 38 || guidString[37] != '}')
////                    {
////                        throw new FormatException( Environment.GetResourceString( "Format_GuidInvLen" ) );
////                    }
////                    startPos = 1;
////                }
////                else if(guidString[0] == '(')
////                {
////                    if(guidString.Length != 38 || guidString[37] != ')')
////                    {
////                        throw new FormatException( Environment.GetResourceString( "Format_GuidInvLen" ) );
////                    }
////                    startPos = 1;
////                }
////                else if(guidString.Length != 36)
////                {
////                    throw new FormatException( Environment.GetResourceString( "Format_GuidInvLen" ) );
////                }
////                if(guidString[8 + startPos] != '-' ||
////                    guidString[13 + startPos] != '-' ||
////                    guidString[18 + startPos] != '-' ||
////                    guidString[23 + startPos] != '-')
////                {
////                    throw new FormatException( Environment.GetResourceString( "Format_GuidDashes" ) );
////                }
////
////                currentPos = startPos;
////                _a = TryParse( guidString, ref currentPos, 8 );
////                ++currentPos; //Increment past the '-';
////                _b = (short)TryParse( guidString, ref currentPos, 4 );
////                ++currentPos; //Increment past the '-';
////                _c = (short)TryParse( guidString, ref currentPos, 4 );
////                ++currentPos; //Increment past the '-';
////                temp = TryParse( guidString, ref currentPos, 4 );
////                ++currentPos; //Increment past the '-';
////                startPos = currentPos;
////                templ = ParseNumbers.StringToLong( guidString, 16, ParseNumbers.NoSpace, ref currentPos );
////                if(currentPos - startPos != 12)
////                {
////                    throw new FormatException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Format_GuidInvLen" ) ) );
////                }
////                _d = (byte)(temp >> 8);
////                _e = (byte)(temp);
////                temp = (int)(templ >> 32);
////                _f = (byte)(temp >> 8);
////                _g = (byte)(temp);
////                temp = (int)(templ);
////                _h = (byte)(temp >> 24);
////                _i = (byte)(temp >> 16);
////                _j = (byte)(temp >> 8);
////                _k = (byte)(temp);
////            }
////            // Else check if it is of the form
////            // {0xdddddddd,0xdddd,0xdddd,{0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd}}
////            else if(g.IndexOf( '{', 0 ) >= 0)
////            {
////                int numStart = 0;
////                int numLen = 0;
////
////                // Convert to lower case
////                //g = g.ToLower();
////
////                // Eat all of the whitespace
////                g = EatAllWhitespace( g );
////
////                // Check for leading '{'
////                if(g[0] != '{')
////                    throw new FormatException( Environment.GetResourceString( "Format_GuidBrace" ) );
////
////                // Check for '0x'
////                if(!IsHexPrefix( g, 1 ))
////                    throw new FormatException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Format_GuidHexPrefix" ), "{0xdddddddd, etc}" ) );
////
////                // Find the end of this hex number (since it is not fixed length)
////                numStart = 3;
////                numLen = g.IndexOf( ',', numStart ) - numStart;
////                if(numLen <= 0)
////                    throw new FormatException( Environment.GetResourceString( "Format_GuidComma" ) );
////
////                // Read in the number
////                _a = (int)ParseNumbers.StringToInt( g.Substring( numStart, numLen ), // first DWORD
////                                                    16,                            // hex
////                                                    ParseNumbers.IsTight );         // tight parsing
////
////                // Check for '0x'
////                if(!IsHexPrefix( g, numStart + numLen + 1 ))
////                    throw new FormatException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Format_GuidHexPrefix" ), "{0xdddddddd, 0xdddd, etc}" ) );
////
////                // +3 to get by ',0x'
////                numStart = numStart + numLen + 3;
////                numLen = g.IndexOf( ',', numStart ) - numStart;
////                if(numLen <= 0)
////                    throw new FormatException( Environment.GetResourceString( "Format_GuidComma" ) );
////
////                // Read in the number
////                _b = (short)ParseNumbers.StringToInt(
////                                                      g.Substring( numStart, numLen ), // first DWORD
////                                                      16,                            // hex
////                                                      ParseNumbers.IsTight );         // tight parsing
////
////                // Check for '0x'
////                if(!IsHexPrefix( g, numStart + numLen + 1 ))
////                    throw new FormatException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Format_GuidHexPrefix" ), "{0xdddddddd, 0xdddd, 0xdddd, etc}" ) );
////
////                // +3 to get by ',0x'
////                numStart = numStart + numLen + 3;
////                numLen = g.IndexOf( ',', numStart ) - numStart;
////                if(numLen <= 0)
////                    throw new FormatException( Environment.GetResourceString( "Format_GuidComma" ) );
////
////                // Read in the number
////                _c = (short)ParseNumbers.StringToInt(
////                                                      g.Substring( numStart, numLen ), // first DWORD
////                                                      16,                            // hex
////                                                      ParseNumbers.IsTight );         // tight parsing
////
////                // Check for '{'
////                if(g.Length <= numStart + numLen + 1 || g[numStart + numLen + 1] != '{')
////                    throw new FormatException( Environment.GetResourceString( "Format_GuidBrace" ) );
////
////                // Prepare for loop
////                numLen++;
////                byte[] bytes = new byte[8];
////
////                for(int i = 0; i < 8; i++)
////                {
////                    // Check for '0x'
////                    if(!IsHexPrefix( g, numStart + numLen + 1 ))
////                        throw new FormatException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Format_GuidHexPrefix" ), "{... { ... 0xdd, ...}}" ) );
////
////                    // +3 to get by ',0x' or '{0x' for first case
////                    numStart = numStart + numLen + 3;
////
////                    // Calculate number length
////                    if(i < 7)  // first 7 cases
////                    {
////                        numLen = g.IndexOf( ',', numStart ) - numStart;
////                        if(numLen <= 0)
////                        {
////                            throw new FormatException( Environment.GetResourceString( "Format_GuidComma" ) );
////                        }
////                    }
////                    else       // last case ends with '}', not ','
////                    {
////                        numLen = g.IndexOf( '}', numStart ) - numStart;
////                        if(numLen <= 0)
////                        {
////                            throw new FormatException( Environment.GetResourceString( "Format_GuidBraceAfterLastNumber" ) );
////                        }
////                    }
////
////                    // Read in the number
////                    uint number = (uint)Convert.ToInt32( g.Substring( numStart, numLen ), 16 );
////                    // check for overflow
////                    if(number > 255)
////                    {
////                        throw new FormatException( Environment.GetResourceString( "Overflow_Byte" ) );
////                    }
////                    bytes[i] = (byte)number;
////                }
////
////                _d = bytes[0];
////                _e = bytes[1];
////                _f = bytes[2];
////                _g = bytes[3];
////                _h = bytes[4];
////                _i = bytes[5];
////                _j = bytes[6];
////                _k = bytes[7];
////
////                // Check for last '}'
////                if(numStart + numLen + 1 >= g.Length || g[numStart + numLen + 1] != '}')
////                {
////                    throw new FormatException( Environment.GetResourceString( "Format_GuidEndBrace" ) );
////                }
////
////                // Check if we have extra characters at the end
////                if(numStart + numLen + 1 != g.Length - 1)
////                {
////                    throw new FormatException( Environment.GetResourceString( "Format_ExtraJunkAtEnd" ) );
////                }
////
////                return;
////            }
////            else
////            // Check if it's of the form dddddddddddddddddddddddddddddddd
////            {
////                String guidString = g.Trim();  //Remove Whitespace
////
////                if(guidString.Length != 32)
////                {
////                    throw new FormatException( Environment.GetResourceString( "Format_GuidInvLen" ) );
////                }
////
////                for(int i = 0; i < guidString.Length; i++)
////                {
////                    char ch = guidString[i];
////                    if(ch >= '0' && ch <= '9')
////                    {
////                        continue;
////                    }
////                    else
////                    {
////                        char upperCaseCh = Char.ToUpper( ch, CultureInfo.InvariantCulture );
////                        if(upperCaseCh >= 'A' && upperCaseCh <= 'F')
////                        {
////                            continue;
////                        }
////                    }
////
////                    throw new FormatException( Environment.GetResourceString( "Format_GuidInvalidChar" ) );
////                }
////
////                _a = (int)ParseNumbers.StringToInt( guidString.Substring( startPos, 8 ), // first DWORD
////                                                    16,                            // hex
////                                                    ParseNumbers.IsTight );         // tight parsing
////                startPos += 8;
////                _b = (short)ParseNumbers.StringToInt( guidString.Substring( startPos, 4 ),
////                                                    16,
////                                                    ParseNumbers.IsTight );
////                startPos += 4;
////                _c = (short)ParseNumbers.StringToInt( guidString.Substring( startPos, 4 ),
////                                                    16,
////                                                    ParseNumbers.IsTight );
////
////                startPos += 4;
////                temp = (short)ParseNumbers.StringToInt( guidString.Substring( startPos, 4 ),
////                                                    16,
////                                                    ParseNumbers.IsTight );
////                startPos += 4;
////                currentPos = startPos;
////                templ = ParseNumbers.StringToLong( guidString, 16, startPos, ref currentPos );
////                if(currentPos - startPos != 12)
////                {
////                    throw new FormatException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Format_GuidInvLen" ) ) );
////                }
////                _d = (byte)(temp >> 8);
////                _e = (byte)(temp);
////                temp = (int)(templ >> 32);
////                _f = (byte)(temp >> 8);
////                _g = (byte)(temp);
////                temp = (int)(templ);
////                _h = (byte)(temp >> 24);
////                _i = (byte)(temp >> 16);
////                _j = (byte)(temp >> 8);
////                _k = (byte)(temp);
////            }
////        }
////        catch(IndexOutOfRangeException)
////        {
////            throw new FormatException( Environment.GetResourceString( "Format_GuidUnrecognized" ) );
////        }
////    }
////
////    // Creates a new GUID initialized to the value represented by the arguments.
////    //
////    public Guid( int a, short b, short c, byte[] d )
////    {
////        if(d == null)
////            throw new ArgumentNullException( "d" );
////        // Check that array is not too big
////        if(d.Length != 8)
////            throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_GuidArrayCtor" ), "8" ) );
////
////        _a = a;
////        _b = b;
////        _c = c;
////        _d = d[0];
////        _e = d[1];
////        _f = d[2];
////        _g = d[3];
////        _h = d[4];
////        _i = d[5];
////        _j = d[6];
////        _k = d[7];
////    }
    
        // Creates a new GUID initialized to the value represented by the
        // arguments.  The bytes are specified like this to avoid endianness issues.
        //
        public Guid( int   a ,
                     short b ,
                     short c ,
                     byte  d ,
                     byte  e ,
                     byte  f ,
                     byte  g ,
                     byte  h ,
                     byte  i ,
                     byte  j ,
                     byte  k )
        {
            _a = a;
            _b = b;
            _c = c;
            _d = d;
            _e = e;
            _f = f;
            _g = g;
            _h = h;
            _i = i;
            _j = j;
            _k = k;
        }
    
    
////    private static int TryParse( String str, ref int parsePos, int requiredLength )
////    {
////        int currStart = parsePos;
////        // the exception message from ParseNumbers is better
////        int retVal = ParseNumbers.StringToInt( str, 16, ParseNumbers.NoSpace, ref parsePos );
////
////        //If we didn't parse enough characters, there's clearly an error.
////        if(parsePos - currStart != requiredLength)
////        {
////            throw new FormatException( Environment.GetResourceString( "Format_GuidInvalidChar" ) );
////        }
////        return retVal;
////    }
////
////    private static String EatAllWhitespace( String str )
////    {
////        int newLength = 0;
////        char[] chArr = new char[str.Length];
////        char curChar;
////
////        // Now get each char from str and if it is not whitespace add it to chArr
////        for(int i = 0; i < str.Length; i++)
////        {
////            curChar = str[i];
////            if(!Char.IsWhiteSpace( curChar ))
////            {
////                chArr[newLength++] = curChar;
////            }
////        }
////
////        // Return a new string based on chArr
////        return new String( chArr, 0, newLength );
////    }
////
////    private static bool IsHexPrefix( String str, int i )
////    {
////        if(str[i] == '0' && (Char.ToLower( str[i + 1], CultureInfo.InvariantCulture ) == 'x'))
////            return true;
////        else
////            return false;
////    }
    
    
        // Returns an unsigned byte array containing the GUID.
        public byte[] ToByteArray()
        {
            byte[] g = new byte[16];
    
            g[0] = (byte)(_a);
            g[1] = (byte)(_a >> 8);
            g[2] = (byte)(_a >> 16);
            g[3] = (byte)(_a >> 24);
            g[4] = (byte)(_b);
            g[5] = (byte)(_b >> 8);
            g[6] = (byte)(_c);
            g[7] = (byte)(_c >> 8);
            g[8] = _d;
            g[9] = _e;
            g[10] = _f;
            g[11] = _g;
            g[12] = _h;
            g[13] = _i;
            g[14] = _j;
            g[15] = _k;
    
            return g;
        }
    
    
////    // Returns the guid in "registry" format.
////    public override String ToString()
////    {
////        return ToString( "D", null );
////    }
    
        public override int GetHashCode()
        {
            return _a ^ (((int)_b << 16) | (int)(ushort)_c) ^ (((int)_f << 24) | _k);
        }
    
        // Returns true if and only if the guid represented
        //  by o is the same as this instance.
        public override bool Equals( Object o )
        {
            // Check that o is a Guid first
            if(o == null || !(o is Guid))
            {
                return false;
            }

            return Equals( (Guid)o );
        }
    
        public bool Equals( Guid g )
        {
            // Now compare each of the elements
            if(g._a != _a)
                return false;
            if(g._b != _b)
                return false;
            if(g._c != _c)
                return false;
            if(g._d != _d)
                return false;
            if(g._e != _e)
                return false;
            if(g._f != _f)
                return false;
            if(g._g != _g)
                return false;
            if(g._h != _h)
                return false;
            if(g._i != _i)
                return false;
            if(g._j != _j)
                return false;
            if(g._k != _k)
                return false;
    
            return true;
        }
    
        private int GetResult( uint me, uint them )
        {
            if(me < them)
            {
                return -1;
            }
            return 1;
        }
    
        public int CompareTo( Object value )
        {
            if(value == null)
            {
                return 1;
            }
            if(!(value is Guid))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeGuid" ) );
#else
                throw new ArgumentException();
#endif
            }

            return CompareTo( (Guid)value );
        }
    
        public int CompareTo(Guid value)
        {
            if (value._a!=this._a)
            {
                return GetResult((uint)this._a, (uint)value._a);
            }
    
            if (value._b!=this._b)
            {
                return GetResult((uint)this._b, (uint)value._b);
            }
    
            if (value._c!=this._c)
            {
                return GetResult((uint)this._c, (uint)value._c);
            }
    
            if (value._d!=this._d)
            {
                return GetResult((uint)this._d, (uint)value._d);
            }
    
            if (value._e!=this._e)
            {
                return GetResult((uint)this._e, (uint)value._e);
            }
    
            if (value._f!=this._f)
            {
                return GetResult((uint)this._f, (uint)value._f);
            }
    
            if (value._g!=this._g)
            {
                return GetResult((uint)this._g, (uint)value._g);
            }
    
            if (value._h!=this._h)
            {
                return GetResult((uint)this._h, (uint)value._h);
            }
    
            if (value._i!=this._i)
            {
                return GetResult((uint)this._i, (uint)value._i);
            }
    
            if (value._j!=this._j)
            {
                return GetResult((uint)this._j, (uint)value._j);
            }
    
            if (value._k!=this._k)
            {
                return GetResult((uint)this._k, (uint)value._k);
            }
    
            return 0;
        }
    
        public static bool operator ==( Guid a, Guid b )
        {
            // Now compare each of the elements
            if(a._a != b._a)
                return false;
            if(a._b != b._b)
                return false;
            if(a._c != b._c)
                return false;
            if(a._d != b._d)
                return false;
            if(a._e != b._e)
                return false;
            if(a._f != b._f)
                return false;
            if(a._g != b._g)
                return false;
            if(a._h != b._h)
                return false;
            if(a._i != b._i)
                return false;
            if(a._j != b._j)
                return false;
            if(a._k != b._k)
                return false;
    
            return true;
        }
    
        public static bool operator !=( Guid a, Guid b )
        {
            return !(a == b);
        }
    
        // This will create a new guid.  Since we've now decided that constructors should 0-init,
        // we need a method that allows users to create a guid.
        public static Guid NewGuid()
        {
////        Guid guid;
////        Marshal.ThrowExceptionForHR( Win32Native.CoCreateGuid( out guid ), new IntPtr( -1 ) );
////        return guid;

            byte[] buf = new byte[16];

            Random rnd = new Random();

            rnd.NextBytes( buf );

            return new Guid( buf );
        }
    
////    public String ToString( String format )
////    {
////        return ToString( format, null );
////    }
////
////    private static char HexToChar( int a )
////    {
////        a = a & 0xf;
////        return (char)((a > 9) ? a - 10 + 0x61 : a + 0x30);
////    }
////
////    private static int HexsToChars( char[] guidChars, int offset, int a, int b )
////    {
////        guidChars[offset++] = HexToChar( a >> 4 );
////        guidChars[offset++] = HexToChar( a );
////        guidChars[offset++] = HexToChar( b >> 4 );
////        guidChars[offset++] = HexToChar( b );
////        return offset;
////    }
////
////    // IFormattable interface
////    // We currently ignore provider
////    public String ToString( String format, IFormatProvider provider )
////    {
////        if(format == null || format.Length == 0)
////            format = "D";
////
////        char[] guidChars;
////        int offset = 0;
////        int strLength = 38;
////        bool dash = true;
////
////        if(format.Length != 1)
////        {
////            // all acceptable format string are of length 1
////            throw new FormatException( Environment.GetResourceString( "Format_InvalidGuidFormatSpecification" ) );
////        }
////
////        char formatCh = format[0];
////        if(formatCh == 'D' || formatCh == 'd')
////        {
////            guidChars = new char[36];
////            strLength = 36;
////        }
////        else if(formatCh == 'N' || formatCh == 'n')
////        {
////            guidChars = new char[32];
////            strLength = 32;
////            dash = false;
////        }
////        else if(formatCh == 'B' || formatCh == 'b')
////        {
////            guidChars = new char[38];
////            guidChars[offset++] = '{';
////            guidChars[37] = '}';
////        }
////        else if(formatCh == 'P' || formatCh == 'p')
////        {
////            guidChars = new char[38];
////            guidChars[offset++] = '(';
////            guidChars[37] = ')';
////        }
////        else
////        {
////            throw new FormatException( Environment.GetResourceString( "Format_InvalidGuidFormatSpecification" ) );
////        }
////
////        offset = HexsToChars( guidChars, offset, _a >> 24, _a >> 16 );
////        offset = HexsToChars( guidChars, offset, _a >> 8, _a );
////
////        if(dash) guidChars[offset++] = '-';
////
////        offset = HexsToChars( guidChars, offset, _b >> 8, _b );
////
////        if(dash) guidChars[offset++] = '-';
////
////        offset = HexsToChars( guidChars, offset, _c >> 8, _c );
////
////        if(dash) guidChars[offset++] = '-';
////
////        offset = HexsToChars( guidChars, offset, _d, _e );
////
////        if(dash) guidChars[offset++] = '-';
////
////        offset = HexsToChars( guidChars, offset, _f, _g );
////        offset = HexsToChars( guidChars, offset, _h, _i );
////        offset = HexsToChars( guidChars, offset, _j, _k );
////
////        return new String( guidChars, 0, strLength );
////    }
    }
}
