// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  String
**
**
** Purpose: Contains headers for the String class.  Actual implementations
** are in String.cpp
**
**
===========================================================*/
namespace System
{
    using System;
    using System.Text;
    using System.Runtime.ConstrainedExecution;
    using System.Globalization;
    using System.Threading;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
////using System.Runtime.Versioning;
////using Microsoft.Win32;
    using System.Runtime.InteropServices;
////using va_list = System.ArgIterator;

    //
    // For Information on these methods, please see COMString.cpp
    //
    // The String class represents a static string of characters.  Many of
    // the String methods perform some type of transformation on the current
    // instance and return the result as a new String. All comparison methods are
    // implemented as a part of String.  As with arrays, character positions
    // (indices) are zero-based.
    //
    // When passing a null string into a constructor in VJ and VC, the null should be
    // explicitly type cast to a String.
    // For Example:
    // String s = new String((String)null);
    // Text.Out.WriteLine(s);
    //
    [Microsoft.Zelig.Internals.WellKnownType( "System_String" )]
    [Serializable]
    public sealed class String : IComparable, IComparable<String>, ICloneable, IConvertible, /*IEnumerable, IEnumerable<char>,*/ IEquatable<String>
    {
        //These are defined in Com99/src/vm/COMStringCommon.h and must be kept in sync.
        private const int TrimHead = 0;
        private const int TrimTail = 1;
        private const int TrimBoth = 2;

        // The Empty constant holds the empty string value.
        //We need to call the String constructor so that the compiler doesn't mark this as a literal.
        //Marking this as a literal would mean that it doesn't show up as a field which we can access
        //from native.
        public static readonly String Empty = "";

        //
        //NOTE NOTE NOTE NOTE
        //These fields map directly onto the fields in an EE StringObject.  See object.h for the layout.
        //
        [NonSerialized] private int  m_arrayLength;
        [NonSerialized] private int  m_stringLength;
        [NonSerialized] private char m_firstChar;

        //
        //Native Static Methods
        //

////    // Joins an array of strings together as one string with a separator between each original string.
////    //
////    public static String Join( String separator, String[] value )
////    {
////        if(value == null)
////        {
////            throw new ArgumentNullException( "value" );
////        }
////
////        return Join( separator, value, 0, value.Length );
////    }

#if WIN64
        private const int charPtrAlignConst = 3;
        private const int alignConst        = 7;
#else
        private const int charPtrAlignConst = 1;
        private const int alignConst        = 3;
#endif

////    internal char FirstChar
////    {
////        get
////        {
////            return m_firstChar;
////        }
////    }
////
////    // Joins an array of strings together as one string with a separator between each original string.
////    //
////    public unsafe static String Join( String separator, String[] value, int startIndex, int count )
////    {
////        //Treat null as empty string.
////        if(separator == null)
////        {
////            separator = String.Empty;
////        }
////
////        //Range check the array
////        if(value == null)
////        {
////            throw new ArgumentNullException( "value" );
////        }
////
////        if(startIndex < 0)
////        {
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_StartIndex" ) );
////        }
////        if(count < 0)
////        {
////            throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_NegativeCount" ) );
////        }
////
////        if(startIndex > value.Length - count)
////        {
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_IndexCountBuffer" ) );
////        }
////
////        //If count is 0, that skews a whole bunch of the calculations below, so just special case that.
////        if(count == 0)
////        {
////            return String.Empty;
////        }
////
////        //Figure out the total length of the strings in value
////        int jointLength = 0;
////        int endIndex    = startIndex + count - 1;
////        for(int stringToJoinIndex = startIndex; stringToJoinIndex <= endIndex; stringToJoinIndex++)
////        {
////            if(value[stringToJoinIndex] != null)
////            {
////                jointLength += value[stringToJoinIndex].Length;
////            }
////        }
////
////        //Add enough room for the separator.
////        jointLength += (count - 1) * separator.Length;
////
////        // Note that we may not catch all overflows with this check (since we could have wrapped around the 4gb range any number of times
////        // and landed back in the positive range.) The input array might be modifed from other threads,
////        // so we have to do an overflow check before each append below anyway. Those overflows will get caught down there.
////        if((jointLength < 0) || ((jointLength + 1) < 0))
////        {
////            throw new OutOfMemoryException();
////        }
////
////        //If this is an empty string, just return.
////        if(jointLength == 0)
////        {
////            return String.Empty;
////        }
////
////        string jointString = FastAllocateString( jointLength );
////
////        fixed(char* pointerToJointString = &jointString.m_firstChar)
////        {
////            UnSafeCharBuffer charBuffer = new UnSafeCharBuffer( pointerToJointString, jointLength );
////
////            // Append the first string first and then append each following string prefixed by the separator.
////            charBuffer.AppendString( value[startIndex] );
////            for(int stringToJoinIndex = startIndex + 1; stringToJoinIndex <= endIndex; stringToJoinIndex++)
////            {
////                charBuffer.AppendString( separator );
////                charBuffer.AppendString( value[stringToJoinIndex] );
////            }
////            BCLDebug.Assert( *(pointerToJointString + charBuffer.Length) == '\0', "String must be null-terminated!" );
////        }
////
////        return jointString;
////    }


////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal static extern int nativeCompareOrdinal( String strA, String strB, bool bIgnoreCase );

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal static extern int nativeCompareOrdinalEx( String strA, int indexA, String strB, int indexB, int count );

////    //This will not work in case-insensitive mode for any character greater than 0x80.
////    //We'll throw an ArgumentException.
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    unsafe internal static extern int nativeCompareOrdinalWC( String strA, char* strBChars, bool bIgnoreCase, out bool success );

////    //
////    // This is a helper method for the security team.  They need to uppercase some strings (guaranteed to be less
////    // than 0x80) before security is fully initialized.  Without security initialized, we can't grab resources (the nlp's)
////    // from the assembly.  This provides a workaround for that problem and should NOT be used anywhere else.
////    //
////    internal unsafe static string SmallCharToUpper( string strIn )
////    {
////        BCLDebug.Assert( strIn != null, "strIn" );
////        //
////        // Get the length and pointers to each of the buffers.  Walk the length
////        // of the string and copy the characters from the inBuffer to the outBuffer,
////        // capitalizing it if necessary.  We assert that all of our characters are
////        // less than 0x80.
////        //
////        int    length = strIn.Length;
////        String strOut = FastAllocateString( length );
////
////        fixed(char* inBuff = &strIn.m_firstChar, outBuff = &strOut.m_firstChar)
////        {
////            char c;
////
////            int upMask = ~0x20;
////            for(int i = 0; i < length; i++)
////            {
////                c = inBuff[i];
////
////                BCLDebug.Assert( (int)c < 0x80, "(int)c < 0x80" );
////
////                //
////                // 0x20 is the difference between upper and lower characters in the lower
////                // 128 ASCII characters. And this bit off to make the chars uppercase.
////                //
////                if(c >= 'a' && c <= 'z')
////                {
////                    c = (char)((int)c & upMask);
////                }
////
////                outBuff[i] = c;
////            }
////
////            BCLDebug.Assert( outBuff[length] == '\0', "outBuff[length]=='\0'" );
////        }
////
////        return strOut;
////    }

        //
        //
        // NATIVE INSTANCE METHODS
        //
        //

        //
        // Search/Query methods
        //

////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        private unsafe static bool EqualsHelper( String strA, String strB )
        {
            int length = strA.Length;
            if(length != strB.Length) return false;

            fixed(char* ap = strA) fixed(char* bp = strB)
            {
                char* a = ap;
                char* b = bp;

                // unroll the loop

                while(length >= 8)
                {
                    if(*(int*) a      != *(int*) b     ) break;
                    if(*(int*)(a + 2) != *(int*)(b + 2)) break;
                    if(*(int*)(a + 4) != *(int*)(b + 4)) break;
                    if(*(int*)(a + 6) != *(int*)(b + 6)) break;
                    a += 8; b += 8; length -= 8;
                }

                // This depends on the fact that the String objects are
                // always zero terminated and that the terminating zero is not included
                // in the length. For odd string sizes, the last compare will include
                // the zero terminator.
                while(length > 0)
                {
                    if(*(int*)a != *(int*)b) break;

                    a += 2; b += 2; length -= 2;
                }

                return (length <= 0);
            }
        }

        private unsafe static int CompareOrdinalHelper( String strA, String strB )
        {
            BCLDebug.Assert( strA != null && strB != null, "strings cannot be null!" );

            int length     = Math.Min( strA.Length, strB.Length );
            int diffOffset = -1;

            fixed(char* ap = strA) fixed(char* bp = strB)
            {
                char* a = ap;
                char* b = bp;

                // unroll the loop
                while(length >= 8)
                {
                    if(*(int*)a != *(int*)b)
                    {
                        diffOffset = 0;
                        break;
                    }

                    if(*(int*)(a + 2) != *(int*)(b + 2))
                    {
                        diffOffset = 2;
                        break;
                    }

                    if(*(int*)(a + 4) != *(int*)(b + 4))
                    {
                        diffOffset = 4;
                        break;
                    }

                    if(*(int*)(a + 6) != *(int*)(b + 6))
                    {
                        diffOffset = 6;
                        break;
                    }

                    a      += 8;
                    b      += 8;
                    length -= 8;
                }

                if(diffOffset != -1)
                {
                    // we already see a difference in the unrolled loop above
                    a += diffOffset;
                    b += diffOffset;

                    int order;

                    if((order = (int)*a - (int)*b) != 0)
                    {
                        return order;
                    }

                    BCLDebug.Assert( *(a + 1) != *(b + 1), "This byte must be different if we reach here!" );
                    return ((int)*(a + 1) - (int)*(b + 1));
                }

                // now go back to slower code path and do comparison on 4 bytes one time.
                // Following code also take advantage of the fact strings will
                // use even numbers of characters (runtime will have a extra zero at the end.)
                // so even if length is 1 here, we can still do the comparsion.
                while(length > 0)
                {
                    if(*(int*)a != *(int*)b)
                    {
                        break;
                    }

                    a      += 2;
                    b      += 2;
                    length -= 2;
                }

                if(length > 0)
                {
                    int c;

                    // found a different int on above loop
                    if((c = (int)*a - (int)*b) != 0)
                    {
                        return c;
                    }

                    BCLDebug.Assert( *(a + 1) != *(b + 1), "This byte must be different if we reach here!" );
                    return ((int)*(a + 1) - (int)*(b + 1));
                }

                // At this point, we have compared all the characters in at least one string.
                // The longer string will be larger.
                return strA.Length - strB.Length;
            }
        }

        // Determines whether two strings match.
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public override bool Equals( Object obj )
        {
            String str = obj as String;
            if(str == null)
            {
                // exception will be thrown later for null this
                if(this != null) return false;
            }

            return EqualsHelper( this, str );
        }

        // Determines whether two strings match.
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public bool Equals( String value )
        {
            if(value == null)
            {
                // exception will be thrown later for null this
                if(this != null) return false;
            }

            return EqualsHelper( this, value );
        }

        public bool Equals( String value, StringComparison comparisonType )
        {
            if(value == null || m_stringLength != value.Length) return false;

            return 0 == string.Compare( this, value, comparisonType );
        }


        // Determines whether two Strings match.
        public static bool Equals( String a, String b )
        {
            if((Object)a == (Object)b)
            {
                return true;
            }

            if((Object)a == null || (Object)b == null)
            {
                return false;
            }

            return EqualsHelper( a, b );
        }

        public static bool Equals( String a, String b, StringComparison comparisonType )
        {
            if(a != null && b != null && a.Length != b.Length) return false;

            return 0 == string.Compare( a, b, comparisonType );
        }

        public static bool operator ==( String a, String b )
        {
            return String.Equals( a, b );
        }

        public static bool operator !=( String a, String b )
        {
            return !String.Equals( a, b );
        }

        // Gets the character at a specified position.
        //
        [System.Runtime.CompilerServices.IndexerName( "Chars" )]
        public extern char this[int index]
        {
////        [ResourceExposure( ResourceScope.None )]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }

        // Converts a substring of this string to an array of characters.  Copies the
        // characters of this string beginning at position startIndex and ending at
        // startIndex + length - 1 to the character array buffer, beginning
        // at bufferStartIndex.
        //
        unsafe public void CopyTo( int sourceIndex, char[] destination, int destinationIndex, int count )
        {
            if(destination == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "destination" );
#else
                throw new ArgumentNullException();
#endif
            }

            if(count < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_NegativeCount" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(sourceIndex < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "sourceIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(count > Length - sourceIndex)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "sourceIndex", Environment.GetResourceString( "ArgumentOutOfRange_IndexCount" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(destinationIndex > destination.Length - count || destinationIndex < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "destinationIndex", Environment.GetResourceString( "ArgumentOutOfRange_IndexCount" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            // Note: fixed does not like empty arrays
            if(count > 0)
            {
                fixed(char* src = &this.m_firstChar)
                {
                    fixed(char* dest = destination)
                    {
                        wstrcpy( dest + destinationIndex, src + sourceIndex, count );
                    }
                }
            }
        }

        // Returns the entire string as an array of characters.
        unsafe public char[] ToCharArray()
        {
            // <STRIP> huge performance improvement for short strings by doing this </STRIP>
            int    length = Length;
            char[] chars  = new char[length];

            if(length > 0)
            {
                fixed(char* src = &this.m_firstChar)
                {
                    fixed(char* dest = chars)
                    {
                        wstrcpyPtrAligned( dest, src, length );
                    }
                }
            }

            return chars;
        }

        // Returns a substring of this string as an array of characters.
        //
        unsafe public char[] ToCharArray( int startIndex, int length )
        {
            // Range check everything.
            if(startIndex < 0 || startIndex > Length || startIndex > Length - length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(length < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "length", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            char[] chars = new char[length];
            if(length > 0)
            {
                fixed(char* src = &this.m_firstChar)
                {
                    fixed(char* dest = chars)
                    {
                        wstrcpy( dest, src + startIndex, length );
                    }
                }
            }

            return chars;
        }

        public static bool IsNullOrEmpty( String value )
        {
            return (value == null || value.Length == 0);
        }

        // Gets a hash code for this string.  If strings A and B are such that A.Equals(B), then
        // they will return the same hash code.
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.MayFail )]
        public override int GetHashCode()
        {
            unsafe
            {
                fixed(char* src = this)
                {
                    BCLDebug.Assert( src[this.Length] == '\0', "src[this.Length] == '\\0'" );
                    BCLDebug.Assert( ((int)src) % 4 == 0, "Managed string should start at 4 bytes boundary" );

#if !WIN64
                    int hash1 = (5381<<16) + 5381;
#else
                    int hash1 = 5381;
#endif
                    int hash2 = hash1;

#if !WIN64
                    // 32bit machines.
                    int* pint = (int *)src;
                    int  len  =        this.Length;

                    while(len > 0)
                    {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                        if(len <= 2)
                        {
                            break;
                        }

                        hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
                        pint += 2;
                        len  -= 4;
                    }
#else
                    int   c;
                    char* s = src;
                    while((c = s[0]) != 0)
                    {
                        hash1 = ((hash1 << 5) + hash1) ^ c;

                        c = s[1];
                        if(c == 0)
                        {
                            break;
                        }

                        hash2 = ((hash2 << 5) + hash2) ^ c;
                        s += 2;
                    }
#endif

////#if DEBUG
////                    // We want to ensure we can change our hash function daily.
////                    // This is perfectly fine as long as you don't persist the
////                    // value from GetHashCode to disk or count on String A
////                    // hashing before string B.  Those are bugs in your code.
////                    hash1 ^= ThisAssembly.DailyBuildNumber;
////#endif
                    return hash1 + (hash2 * 1566083941);
                }
            }
        }

        public int Length
        {
            get
            {
                return m_stringLength;
            }
        }

////    // Gets the length of this string
////    //
////    // This is a EE implemented function so that the JIT can recognise is specially
////    // and eliminate checks on character fetchs in a loop like:
////    //        for(int I = 0; I < str.Length; i++) str[i]
////    // The actually code generated for this will be one instruction and will be inlined.
////    //
////    public extern int Length
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [MethodImpl( MethodImplOptions.InternalCall )]
////        get;
////    }

        ///<internalonly/>
        internal int ArrayLength
        {
            get
            {
                return m_arrayLength;
            }
        }

        // Used by StringBuilder
        internal int Capacity
        {
            get
            {
                return m_arrayLength - 1;
            }
        }

        // Creates an array of strings by splitting this string at each
        // occurence of a separator.  The separator is searched for, and if found,
        // the substring preceding the occurence is stored as the first element in
        // the array of strings.  We then continue in this manner by searching
        // the substring that follows the occurence.  On the other hand, if the separator
        // is not found, the array of strings will contain this instance as its only element.
        // If the separator is null
        // whitespace (i.e., Character.IsWhitespace) is used as the separator.
        //
        public String[] Split( params char[] separator )
        {
            return Split( separator, Int32.MaxValue, StringSplitOptions.None );
        }
    
        // Creates an array of strings by splitting this string at each
        // occurence of a separator.  The separator is searched for, and if found,
        // the substring preceding the occurence is stored as the first element in
        // the array of strings.  We then continue in this manner by searching
        // the substring that follows the occurence.  On the other hand, if the separator
        // is not found, the array of strings will contain this instance as its only element.
        // If the spearator is the empty string (i.e., String.Empty), then
        // whitespace (i.e., Character.IsWhitespace) is used as the separator.
        // If there are more than count different strings, the last n-(count-1)
        // elements are concatenated and added as the last String.
        //
        public string[] Split( char[] separator, int count )
        {
            return Split( separator, count, StringSplitOptions.None );
        }
    
        public String[] Split( char[] separator, StringSplitOptions options )
        {
            return Split( separator, Int32.MaxValue, options );
        }
    
        public String[] Split( char[] separator, int count, StringSplitOptions options )
        {
            if(count < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_NegativeCount" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            if(options < StringSplitOptions.None || options > StringSplitOptions.RemoveEmptyEntries)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_EnumIllegalVal", (int)options ) );
#else
                throw new ArgumentException();
#endif
            }
    
            bool omitEmptyEntries = (options == StringSplitOptions.RemoveEmptyEntries);
            if((count == 0) || (omitEmptyEntries && this.Length == 0))
            {
                return new String[0];
            }
    
            int[] sepList = new int[Length];
            int numReplaces = MakeSeparatorList( separator, ref sepList );
    
            //Handle the special case of no replaces and special count.
            if(0 == numReplaces || count == 1)
            {
                String[] stringArray = new String[1];
    
                stringArray[0] = this;
    
                return stringArray;
            }
    
            if(omitEmptyEntries)
            {
                return InternalSplitOmitEmptyEntries( sepList, null, numReplaces, count );
            }
            else
            {
                return InternalSplitKeepEmptyEntries( sepList, null, numReplaces, count );
            }
        }
    
        public String[] Split( String[] separator, StringSplitOptions options )
        {
            return Split( separator, Int32.MaxValue, options );
        }
    
        public String[] Split( String[] separator, Int32 count, StringSplitOptions options )
        {
            if(count < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_NegativeCount" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            if(options < StringSplitOptions.None || options > StringSplitOptions.RemoveEmptyEntries)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_EnumIllegalVal", (int)options ) );
#else
                throw new ArgumentException();
#endif
            }
    
            bool omitEmptyEntries = (options == StringSplitOptions.RemoveEmptyEntries);
    
            if(separator == null || separator.Length == 0)
            {
                return Split( (char[])null, count, options );
            }
    
            if((count == 0) || (omitEmptyEntries && this.Length == 0))
            {
                return new String[0];
            }
    
            int[] sepList    = new int[Length];
            int[] lengthList = new int[Length];
            int numReplaces  = MakeSeparatorList( separator, ref sepList, ref lengthList );
    
            //Handle the special case of no replaces and special count.
            if(0 == numReplaces || count == 1)
            {
                String[] stringArray = new String[1];
    
                stringArray[0] = this;
    
                return stringArray;
            }
    
            if(omitEmptyEntries)
            {
                return InternalSplitOmitEmptyEntries( sepList, lengthList, numReplaces, count );
            }
            else
            {
                return InternalSplitKeepEmptyEntries( sepList, lengthList, numReplaces, count );
            }
        }
    
        // Note a few special case in this function:
        //     If there is no separator in the string, a string array which only contains
        //     the original string will be returned regardless of the count.
        //
    
        private String[] InternalSplitKeepEmptyEntries( Int32[] sepList, Int32[] lengthList, Int32 numReplaces, int count )
        {
            BCLDebug.Assert( count >= 2, "Count>=2" );
    
            int currIndex = 0;
            int arrIndex  = 0;
    
            count--;
            int numActualReplaces = (numReplaces < count) ? numReplaces : count;
    
            //Allocate space for the new array.
            //+1 for the string from the end of the last replace to the end of the String.
            String[] splitStrings = new String[numActualReplaces + 1];
    
            for(int i = 0; i < numActualReplaces && currIndex < Length; i++)
            {
                splitStrings[arrIndex++] = Substring( currIndex, sepList[i] - currIndex );
    
                currIndex = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
            }
    
            //Handle the last string at the end of the array if there is one.
            if(currIndex < Length && numActualReplaces >= 0)
            {
                splitStrings[arrIndex] = Substring( currIndex );
            }
            else if(arrIndex == numActualReplaces)
            {
                //We had a separator character at the end of a string.  Rather than just allowing
                //a null character, we'll replace the last element in the array with an empty string.
                splitStrings[arrIndex] = String.Empty;
            }
    
            return splitStrings;
        }
    
    
        // This function will not keep the Empty String
        private String[] InternalSplitOmitEmptyEntries( Int32[] sepList, Int32[] lengthList, Int32 numReplaces, int count )
        {
            BCLDebug.Assert( count >= 2, "Count>=2" );
    
            // Allocate array to hold items. This array may not be
            // filled completely in this function, we will create a
            // new array and copy string references to that new array.
    
            int      maxItems     = (numReplaces < count) ? (numReplaces + 1) : count;
            String[] splitStrings = new String[maxItems];
    
            int currIndex = 0;
            int arrIndex  = 0;
    
            for(int i = 0; i < numReplaces && currIndex < Length; i++)
            {
                if(sepList[i] - currIndex > 0)
                {
                    splitStrings[arrIndex++] = Substring( currIndex, sepList[i] - currIndex );
                }
    
                currIndex = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
                if(arrIndex == count - 1)
                {
                    // If all the remaining entries at the end are empty, skip them
                    while(i < numReplaces - 1 && currIndex == sepList[++i])
                    {
                        currIndex += ((lengthList == null) ? 1 : lengthList[i]);
                    }
                    break;
                }
            }
    
            // we must have at least one slot left to fill in the last string.
            BCLDebug.Assert( arrIndex < maxItems, "arrIndex < maxItems" );
    
            //Handle the last string at the end of the array if there is one.
            if(currIndex < Length)
            {
                splitStrings[arrIndex++] = Substring( currIndex );
            }
    
            String[] stringArray = splitStrings;
            if(arrIndex != maxItems)
            {
                stringArray = new String[arrIndex];
                for(int j = 0; j < arrIndex; j++)
                {
                    stringArray[j] = splitStrings[j];
                }
            }
            return stringArray;
        }
    
        //--------------------------------------------------------------------
        // This function returns number of the places within baseString where
        // instances of characters in Separator occur.
        // Args: separator  -- A string containing all of the split characters.
        //       sepList    -- an array of ints for split char indicies.
        //--------------------------------------------------------------------
        private unsafe int MakeSeparatorList( char[] separator, ref int[] sepList )
        {
            int foundCount = 0;
    
            if(separator == null || separator.Length == 0)
            {
                fixed(char* pwzChars = &this.m_firstChar)
                {
                    //If they passed null or an empty string, look for whitespace.
                    for(int i = 0; i < Length && foundCount < sepList.Length; i++)
                    {
                        if(Char.IsWhiteSpace( pwzChars[i] ))
                        {
                            sepList[foundCount++] = i;
                        }
                    }
                }
            }
            else
            {
                int sepListCount = sepList  .Length;
                int sepCount     = separator.Length;
    
                //If they passed in a string of chars, actually look for those chars.
                fixed(char* pwzChars = &this.m_firstChar, pSepChars = separator)
                {
                    for(int i = 0; i < Length && foundCount < sepListCount; i++)
                    {
                        char* pSep = pSepChars;
                        for(int j = 0; j < sepCount; j++, pSep++)
                        {
                            if(pwzChars[i] == *pSep)
                            {
                                sepList[foundCount++] = i;
                                break;
                            }
                        }
                    }
                }
            }
            return foundCount;
        }
    
        //--------------------------------------------------------------------
        // This function returns number of the places within baseString where
        // instances of separator strings occur.
        // Args: separators -- An array containing all of the split strings.
        //       sepList    -- an array of ints for split string indicies.
        //       lengthList -- an array of ints for split string lengths.
        //--------------------------------------------------------------------
        private unsafe int MakeSeparatorList( String[] separators, ref int[] sepList, ref int[] lengthList )
        {
            BCLDebug.Assert( separators != null && separators.Length > 0, "separators != null && separators.Length > 0" );
    
            int foundCount   = 0;
            int sepListCount = sepList.Length;
            int sepCount     = separators.Length;
    
            fixed(char* pwzChars = &this.m_firstChar)
            {
                for(int i = 0; i < Length && foundCount < sepListCount; i++)
                {
                    for(int j = 0; j < separators.Length; j++)
                    {
                        String separator = separators[j];
                        if(String.IsNullOrEmpty( separator ))
                        {
                            continue;
                        }
    
                        Int32 currentSepLength = separator.Length;
                        if(pwzChars[i] == separator[0] && currentSepLength <= Length - i)
                        {
                            if(currentSepLength == 1 || String.CompareOrdinal( this, i, separator, 0, currentSepLength ) == 0)
                            {
                                sepList   [foundCount] = i;
                                lengthList[foundCount] = currentSepLength;
    
                                foundCount++;
    
                                i += currentSepLength - 1;
                                break;
                            }
                        }
                    }
                }
            }
    
            return foundCount;
        }

        // Returns a substring of this string.
        //
        public String Substring( int startIndex )
        {
            return this.Substring( startIndex, Length - startIndex );
        }

        // Returns a substring of this string.
        //
        public String Substring( int startIndex, int length )
        {
            // okay to not enforce copying in the case of Substring(0, length), since we assume
            // String instances are immutable.
            return InternalSubStringWithChecks( startIndex, length, false );
        }


        internal String InternalSubStringWithChecks( int startIndex, int length, bool fAlwaysCopy )
        {
            int thisLength = Length;

            //Bounds Checking.
            if(startIndex < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_StartIndex" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(startIndex > thisLength)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_StartIndexLargerThanLength" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(length < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "length", Environment.GetResourceString( "ArgumentOutOfRange_NegativeLength" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(startIndex > thisLength - length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "length", Environment.GetResourceString( "ArgumentOutOfRange_IndexLength" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(length == 0)
            {
                return String.Empty;
            }

            return InternalSubString( startIndex, length, fAlwaysCopy );
        }

        unsafe string InternalSubString( int startIndex, int length, bool fAlwaysCopy )
        {
            BCLDebug.Assert( startIndex >= 0 && startIndex <= this.Length         , "StartIndex is out of range!" );
            BCLDebug.Assert( length     >= 0 && startIndex <= this.Length - length, "length is out of range!"     );

            if(startIndex == 0 && length == this.Length && !fAlwaysCopy)
            {
                return this;
            }

            String result = FastAllocateString( length );

            fixed(char* dest = &result.m_firstChar)
            {
                fixed(char* src = &this.m_firstChar)
                {
                    wstrcpy( dest, src + startIndex, length );
                }
            }

            return result;
        }

        //This should really live on System.Globalization.CharacterInfo.  However,
        //Trim gets called by security while resgen is running, so we can't run
        //CharacterInfo's class initializer (which goes to native and looks for a
        //resource table that hasn't yet been attached to the assembly when resgen
        //runs.
        internal static readonly char[] WhitespaceChars =
            {
                (char)0x9, (char)0xA, (char)0xB, (char)0xC, (char)0xD, (char)0x20, (char)0x85,
                (char)0xA0, (char)0x1680,
                (char)0x2000, (char)0x2001, (char)0x2002, (char)0x2003, (char)0x2004, (char)0x2005,
                (char)0x2006, (char)0x2007, (char)0x2008, (char)0x2009, (char)0x200A, (char)0x200B,
                (char)0x2028, (char)0x2029,
                (char)0x3000, (char)0xFEFF
            };

        // Removes a string of characters from the ends of this string.
        public String Trim( params char[] trimChars )
        {
            if(trimChars == null || trimChars.Length == 0)
            {
                trimChars = WhitespaceChars;
            }
    
            return TrimHelper( trimChars, TrimBoth );
        }
    
        // Removes a string of characters from the beginning of this string.
        public String TrimStart( params char[] trimChars )
        {
            if(trimChars == null || trimChars.Length == 0)
            {
                trimChars = WhitespaceChars;
            }
    
            return TrimHelper( trimChars, TrimHead );
        }
    
    
        // Removes a string of characters from the end of this string.
        public String TrimEnd( params char[] trimChars )
        {
            if(trimChars == null || trimChars.Length == 0)
            {
                trimChars = WhitespaceChars;
            }
    
            return TrimHelper( trimChars, TrimTail );
        }
    
    
////    // Creates a new string with the characters copied in from ptr. If
////    // ptr is null, a string initialized to ";<;No Object>;"; (i.e.,
////    // String.NullString) is created.
////    //
////    [ResourceExposure( ResourceScope.None )]
////    [CLSCompliant( false )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    unsafe public extern String( char* value );
////
////    [ResourceExposure( ResourceScope.None )]
////    [CLSCompliant( false )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    unsafe public extern String( char* value, int startIndex, int length );
////
////    [ResourceExposure( ResourceScope.None )]
////    [CLSCompliant( false )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    unsafe public extern String( sbyte* value );
////
////    [ResourceExposure( ResourceScope.None )]
////    [CLSCompliant( false )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    unsafe public extern String( sbyte* value, int startIndex, int length );
////
////    [ResourceExposure( ResourceScope.None )]
////    [CLSCompliant( false )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    unsafe public extern String( sbyte* value, int startIndex, int length, Encoding enc );
////
////    unsafe static private String CreateString( sbyte* value, int startIndex, int length, Encoding enc )
////    {
////        if(enc == null)
////        {
////            return new String( value, startIndex, length ); // default to ANSI
////        }
////
////        if(length < 0)
////        {
////            throw new ArgumentOutOfRangeException( "length", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
////        }
////
////        if(startIndex < 0)
////        {
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_StartIndex" ) );
////        }
////
////        if((value + startIndex) < value)
////        {
////            // overflow check
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_PartialWCHAR" ) );
////        }
////
////        byte[] b = new byte[length];
////
////        try
////        {
////            Buffer.memcpy( (byte*)value, startIndex, b, 0, length );
////        }
////        catch(NullReferenceException)
////        {
////            // If we got a NullReferencException. It means the pointer or
////            // the index is out of range
////            throw new ArgumentOutOfRangeException( "value", Environment.GetResourceString( "ArgumentOutOfRange_PartialWCHAR" ) );
////        }
////
////        return enc.GetString( b );
////    }
    
        // Helper for encodings so they can talk to our buffer directly
        // stringLength must be the exact size we'll expect
        unsafe static internal String CreateStringFromEncoding( byte*    bytes      ,
                                                                int      byteLength ,
                                                                Encoding encoding   )
        {
            BCLDebug.Assert( bytes != null, "need a byte[]." );
            BCLDebug.Assert( byteLength >= 0, "byteLength >= 0" );
    
            // Get our string length
            int stringLength = encoding.GetCharCount( bytes, byteLength, null );
            BCLDebug.Assert( stringLength >= 0, "stringLength >= 0" );
    
            // They gave us an empty string if they needed one
            // 0 bytelength might be possible if there's something in an encoder
            if(stringLength == 0)
            {
                return String.Empty;
            }
    
            String s = FastAllocateString( stringLength );
            fixed(char* pTempChars = &s.m_firstChar)
            {
                int doubleCheck = encoding.GetChars( bytes, byteLength, pTempChars, stringLength, null );
    
                BCLDebug.Assert( stringLength == doubleCheck, "Expected encoding.GetChars to return same length as encoding.GetCharCount" );
            }
    
            return s;
        }
    
////    unsafe internal byte[] ConvertToAnsi_BestFit_Throw( int iMaxDBCSCharByteSize )
////    {
////        const uint CP_ACP = 0;
////
////        int    nb;
////        int    cbNativeBuffer = (Length + 3) * iMaxDBCSCharByteSize;
////        byte[] bytes          = new byte[cbNativeBuffer];
////
////        uint flgs            = 0;
////        uint DefaultCharUsed = 0;
////
////        fixed(byte* pbNativeBuffer = bytes)
////        {
////            fixed(char* pwzChar = &this.m_firstChar)
////            {
////                nb = Win32Native.WideCharToMultiByte(
////                    CP_ACP,
////                    flgs,
////                    pwzChar,
////                    this.Length,
////                    pbNativeBuffer,
////                    cbNativeBuffer,
////                    IntPtr.Zero,
////                    new IntPtr( &DefaultCharUsed ) );
////            }
////        }
////
////        if(0 != DefaultCharUsed)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Interop_Marshal_Unmappable_Char" ) );
////        }
////
////        bytes[nb] = 0;
////        return bytes;
////    }
////
////    // Normalization Methods
////    // These just wrap calls to Normalization class
////    public bool IsNormalized()
////    {
////        // Default to Form C
////        return IsNormalized( NormalizationForm.FormC );
////    }
////
////    public bool IsNormalized( NormalizationForm normalizationForm )
////    {
////        if(this.IsFastSort())
////        {
////            // If its FastSort && one of the 4 main forms, then its already normalized
////            if(normalizationForm == NormalizationForm.FormC ||
////               normalizationForm == NormalizationForm.FormKC ||
////               normalizationForm == NormalizationForm.FormD ||
////               normalizationForm == NormalizationForm.FormKD)
////            {
////                return true;
////            }
////        }
////
////        return Normalization.IsNormalized( this, normalizationForm );
////    }
////
////    public String Normalize()
////    {
////        // Default to Form C
////        return Normalize( NormalizationForm.FormC );
////    }
////
////    public String Normalize( NormalizationForm normalizationForm )
////    {
////        if(this.IsAscii())
////        {
////            // If its FastSort && one of the 4 main forms, then its already normalized
////            if(normalizationForm == NormalizationForm.FormC ||
////               normalizationForm == NormalizationForm.FormKC ||
////               normalizationForm == NormalizationForm.FormD ||
////               normalizationForm == NormalizationForm.FormKD)
////            {
////                return this;
////            }
////        }
////
////        return Normalization.Normalize( this, normalizationForm );
////    }

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        private extern static String FastAllocateString( int length );

        unsafe private static void FillStringChecked( String dest, int destPos, String src )
        {
            int length = src.Length;

            if(length > dest.Length - destPos)
            {
                throw new IndexOutOfRangeException();
            }

            fixed(char* pDest = &dest.m_firstChar)
            {
                fixed(char* pSrc = &src.m_firstChar)
                {
                    wstrcpy( pDest + destPos, pSrc, length );
                }
            }
        }

        // Creates a new string from the characters in a subarray.  The new string will
        // be created from the characters in value between startIndex and
        // startIndex + length - 1.
        //
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern String( char[] value, int startIndex, int length );
    
        // Creates a new string from the characters in a subarray.  The new string will be
        // created from the characters in value.
        //
    
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern String( char[] value );

        //
        // This handles the case where both smem and dmem pointers are
        //  aligned on a pointer boundary
        //
        private static unsafe void wstrcpyPtrAligned( char* dmem, char* smem, int charCount )
        {
#if _DEBUG
            BCLDebug.Assert(((int)dmem & (IntPtr.Size-1)) == 0, "dmem is pointer size aligned");
            BCLDebug.Assert(((int)smem & (IntPtr.Size-1)) == 0, "smem is pointer size aligned");
#endif

#if !WIN64
            while(charCount >= 8)
            {
                ((uint*)dmem)[0] = ((uint*)smem)[0];
                ((uint*)dmem)[1] = ((uint*)smem)[1];
                ((uint*)dmem)[2] = ((uint*)smem)[2];
                ((uint*)dmem)[3] = ((uint*)smem)[3];

                dmem      += 8;
                smem      += 8;
                charCount -= 8;
            }

            if((charCount & 4) != 0)
            {
                ((uint*)dmem)[0] = ((uint*)smem)[0];
                ((uint*)dmem)[1] = ((uint*)smem)[1];

                dmem += 4;
                smem += 4;
            }
#else
            while (charCount >= 16)
            {
                ((ulong *)dmem)[0] = ((ulong *)smem)[0];
                ((ulong *)dmem)[1] = ((ulong *)smem)[1];
                ((ulong *)dmem)[2] = ((ulong *)smem)[2];
                ((ulong *)dmem)[3] = ((ulong *)smem)[3];

                dmem      += 16;
                smem      += 16;
                charCount -= 16;
            }

            if ((charCount & 8) != 0)
            {
                ((ulong *)dmem)[0] = ((ulong *)smem)[0];
                ((ulong *)dmem)[1] = ((ulong *)smem)[1];

                dmem += 8;
                smem += 8;
            }

            if ((charCount & 4) != 0)
            {
                ((ulong *)dmem)[0] = ((ulong *)smem)[0];

                dmem += 4;
                smem += 4;
            }
#endif
            if((charCount & 2) != 0)
            {
                ((uint*)dmem)[0] = ((uint*)smem)[0];

                dmem += 2;
                smem += 2;
            }

            if((charCount & 1) != 0)
            {
                dmem[0] = smem[0];
            }
        }

        private static unsafe void wstrcpy( char* dmem, char* smem, int charCount )
        {
            if(charCount > 0)
            {
                if((((int)dmem | (int)smem) & (IntPtr.Size - 1)) == 0) // Both pointers word-aligned?
                {
                    while(charCount >= 8)
                    {
                        ((uint*)dmem)[0] = ((uint*)smem)[0];
                        ((uint*)dmem)[1] = ((uint*)smem)[1];
                        ((uint*)dmem)[2] = ((uint*)smem)[2];
                        ((uint*)dmem)[3] = ((uint*)smem)[3];

                        dmem      += 8;
                        smem      += 8;
                        charCount -= 8;
                    }

                    if((charCount & 4) != 0)
                    {
                        ((uint*)dmem)[0] = ((uint*)smem)[0];
                        ((uint*)dmem)[1] = ((uint*)smem)[1];

                        dmem += 4;
                        smem += 4;
                    }

                    if((charCount & 2) != 0)
                    {
                        ((uint*)dmem)[0] = ((uint*)smem)[0];

                        dmem += 2;
                        smem += 2;
                    }
                }
                else
                {
                    while(charCount >= 8)
                    {
                        dmem[0] = smem[0];
                        dmem[1] = smem[1];
                        dmem[2] = smem[2];
                        dmem[3] = smem[3];
                        dmem[4] = smem[4];
                        dmem[5] = smem[5];
                        dmem[6] = smem[6];
                        dmem[7] = smem[7];

                        dmem      += 8;
                        smem      += 8;
                        charCount -= 8;
                    }

                    if((charCount & 4) != 0)
                    {
                        dmem[0] = smem[0];
                        dmem[1] = smem[1];
                        dmem[2] = smem[2];
                        dmem[3] = smem[3];

                        dmem += 4;
                        smem += 4;
                    }

                    if((charCount & 2) != 0)
                    {
                        dmem[0] = smem[0];
                        dmem[1] = smem[1];

                        dmem += 2;
                        smem += 2;
                    }
                }

                if((charCount & 1) != 0)
                {
                    dmem[0] = smem[0];
                }
            }
        }

////    private String CtorCharArray( char[] value )
////    {
////        if(value != null && value.Length != 0)
////        {
////            String result = FastAllocateString( value.Length );
////
////            unsafe
////            {
////                fixed(char* dest = result, source = value)
////                {
////                    wstrcpyPtrAligned( dest, source, value.Length );
////                }
////            }
////            return result;
////        }
////        else
////        {
////            return String.Empty;
////        }
////    }
////
////    private String CtorCharArrayStartLength( char[] value, int startIndex, int length )
////    {
////        if(value == null)
////        {
////            throw new ArgumentNullException( "value" );
////        }
////
////        if(startIndex < 0)
////        {
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_StartIndex" ) );
////        }
////
////        if(length < 0)
////        {
////            throw new ArgumentOutOfRangeException( "length", Environment.GetResourceString( "ArgumentOutOfRange_NegativeLength" ) );
////        }
////
////        if(startIndex > value.Length - length)
////        {
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////        }
////
////        if(length > 0)
////        {
////            String result = FastAllocateString( length );
////
////            unsafe
////            {
////                fixed(char* dest = result, source = value)
////                {
////                    wstrcpy( dest, source + startIndex, length );
////                }
////            }
////            return result;
////        }
////        else
////        {
////            return String.Empty;
////        }
////    }
////
////    private String CtorCharCount( char c, int count )
////    {
////        if(count > 0)
////        {
////            String result = FastAllocateString( count );
////            unsafe
////            {
////                fixed(char* dest = result)
////                {
////                    char* dmem = dest;
////                    while(((uint)dmem & 3) != 0 && count > 0)
////                    {
////                        *dmem++ = c;
////                        count--;
////                    }
////                    uint cc = (uint)((c << 16) | c);
////
////                    if(count >= 4)
////                    {
////                        count -= 4;
////                        do
////                        {
////                            ((uint*)dmem)[0] = cc;
////                            ((uint*)dmem)[1] = cc;
////                            dmem  += 4;
////                            count -= 4;
////                        } while(count >= 0);
////                    }
////
////                    if((count & 2) != 0)
////                    {
////                        ((uint*)dmem)[0] = cc;
////                        dmem += 2;
////                    }
////
////                    if((count & 1) != 0)
////                    {
////                        dmem[0] = c;
////                    }
////                }
////            }
////            return result;
////        }
////        else if(count == 0)
////        {
////            return String.Empty;
////        }
////        else
////        {
////            throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_MustBeNonNegNum", "count" ) );
////        }
////    }
////
////    private static unsafe int wcslen( char* ptr )
////    {
////        char* end = ptr;
////
////        // The following code is (somewhat surprisingly!) significantly faster than a naive loop,
////        // at least on x86 and the current jit.
////
////        // First make sure our pointer is aligned on a dword boundary
////        while(((uint)end & 3) != 0 && *end != 0)
////        {
////            end++;
////        }
////
////        if(*end != 0)
////        {
////            // The loop condition below works because if "end[0] & end[1]" is non-zero, that means
////            // neither operand can have been zero. If is zero, we have to look at the operands individually,
////            // but we hope this going to fairly rare.
////
////            // In general, it would be incorrect to access end[1] if we haven't made sure
////            // end[0] is non-zero. However, we know the ptr has been aligned by the loop above
////            // so end[0] and end[1] must be in the same page, so they're either both accessible, or both not.
////
////            while((end[0] & end[1]) != 0 || (end[0] != 0 && end[1] != 0))
////            {
////                end += 2;
////            }
////        }
////
////        // finish up with the naive loop
////        for(; *end != 0; end++)
////        {
////            ;
////        }
////
////        int count = (int)(end - ptr);
////
////        return count;
////    }
////
////    private unsafe String CtorCharPtr( char* ptr )
////    {
////        BCLDebug.Assert( this == null, "this == null" );        // this is the string constructor, we allocate it
////
////        if(ptr >= (char*)64000)
////        {
////            try
////            {
////                int    count  = wcslen( ptr );
////                String result = FastAllocateString( count );
////
////                fixed(char* dest = result)
////                {
////                    wstrcpy( dest, ptr, count );
////                }
////
////                return result;
////            }
////            catch(NullReferenceException)
////            {
////                throw new ArgumentOutOfRangeException( "ptr", Environment.GetResourceString( "ArgumentOutOfRange_PartialWCHAR" ) );
////            }
////        }
////        else if(ptr == null)
////        {
////            return String.Empty;
////        }
////        else
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeStringPtrNotAtom" ) );
////        }
////    }
////
////    private unsafe String CtorCharPtrStartLength( char* ptr, int startIndex, int length )
////    {
////        BCLDebug.Assert( this == null, "this == null" );        // this is the string constructor, we allocate it
////
////        if(length < 0)
////        {
////            throw new ArgumentOutOfRangeException( "length", Environment.GetResourceString( "ArgumentOutOfRange_NegativeLength" ) );
////        }
////
////        if(startIndex < 0)
////        {
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_StartIndex" ) );
////        }
////
////        char* pFrom = ptr + startIndex;
////        if(pFrom < ptr)
////        {
////            // This means that the pointer operation has had an overflow
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_PartialWCHAR" ) );
////        }
////
////        String result = FastAllocateString( length );
////
////        try
////        {
////            fixed(char* dest = result)
////            {
////                wstrcpy( dest, pFrom, length );
////            }
////
////            return result;
////        }
////        catch(NullReferenceException)
////        {
////            throw new ArgumentOutOfRangeException( "ptr", Environment.GetResourceString( "ArgumentOutOfRange_PartialWCHAR" ) );
////        }
////    }

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern String( char c, int count );

        //
        //
        // INSTANCE METHODS
        //
        //

        // Provides a culture-correct string comparison. StrA is compared to StrB
        // to determine whether it is lexicographically less, equal, or greater, and then returns
        // either a negative integer, 0, or a positive integer; respectively.
        //
        public static int Compare( String strA, String strB )
        {
            return CultureInfo.CurrentCulture.CompareInfo.Compare( strA, strB, CompareOptions.None );
        }

        // Provides a culture-correct string comparison. strA is compared to strB
        // to determine whether it is lexicographically less, equal, or greater, and then a
        // negative integer, 0, or a positive integer is returned; respectively.
        // The case-sensitive option is set by ignoreCase
        //
        public static int Compare( String strA, String strB, bool ignoreCase )
        {
            if(ignoreCase)
            {
                return CultureInfo.CurrentCulture.CompareInfo.Compare( strA, strB, CompareOptions.IgnoreCase );
            }
            else
            {
                return CultureInfo.CurrentCulture.CompareInfo.Compare( strA, strB, CompareOptions.None );
            }
        }

        // Provides a more flexible function for string comparision. See StringComparison
        // for meaning of different comparisonType.
        public static int Compare( String strA, String strB, StringComparison comparisonType )
        {
            if(comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "NotSupported_StringComparison" ), "comparisonType" );
#else
                throw new ArgumentException();
#endif
            }
    
            if((Object)strA == (Object)strB)
            {
                return 0;
            }
    
            //they can't both be null;
            if(strA == null)
            {
                return -1;
            }
    
            if(strB == null)
            {
                return 1;
            }
    
    
            switch(comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare( strA, strB, CompareOptions.None );
    
                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare( strA, strB, CompareOptions.IgnoreCase );
    
                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.Compare( strA, strB, CompareOptions.None );
    
                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.Compare( strA, strB, CompareOptions.IgnoreCase );
    
                case StringComparison.Ordinal:
                    return CompareOrdinalHelper( strA, strB );
    
                case StringComparison.OrdinalIgnoreCase:
                    // If both strings are ASCII strings, we can take the fast path.
                    if(strA.IsAscii() && strB.IsAscii())
                    {
                        return (String.nativeCompareOrdinal( strA, strB, true ));
                    }
#if EXCEPTION_STRINGS
                    throw new NotSupportedException( Environment.GetResourceString( "NotSupported_StringComparison" ) );
#else
                    throw new NotSupportedException();
#endif
////                // Take the slow path.
////                return TextInfo.CompareOrdinalIgnoreCase( strA, strB );
    
                default:
#if EXCEPTION_STRINGS
                    throw new NotSupportedException( Environment.GetResourceString( "NotSupported_StringComparison" ) );
#else
                    throw new NotSupportedException();
#endif
            }
        }

////    // Provides a culture-correct string comparison. strA is compared to strB
////    // to determine whether it is lexicographically less, equal, or greater, and then a
////    // negative integer, 0, or a positive integer is returned; respectively.
////    // The case-sensitive option is set by ignoreCase, and the culture is set
////    // by culture
////    //
////    public static int Compare( String strA, String strB, bool ignoreCase, CultureInfo culture )
////    {
////        if(culture == null)
////        {
////            throw new ArgumentNullException( "culture" );
////        }
////
////        if(ignoreCase)
////        {
////            return culture.CompareInfo.Compare( strA, strB, CompareOptions.IgnoreCase );
////        }
////        else
////        {
////           return culture.CompareInfo.Compare( strA, strB, CompareOptions.None );
////        }
////    }
////
////    // Determines whether two string regions match.  The substring of strA beginning
////    // at indexA of length count is compared with the substring of strB
////    // beginning at indexB of the same length.
////    //
////    public static int Compare( String strA, int indexA, String strB, int indexB, int length )
////    {
////        int lengthA = length;
////        int lengthB = length;
////
////        if(strA != null)
////        {
////            if(strA.Length - indexA < lengthA)
////            {
////                lengthA = (strA.Length - indexA);
////            }
////        }
////
////        if(strB != null)
////        {
////            if(strB.Length - indexB < lengthB)
////            {
////                lengthB = (strB.Length - indexB);
////            }
////        }
////
////        return CultureInfo.CurrentCulture.CompareInfo.Compare( strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.None );
////    }
////
////
////    // Determines whether two string regions match.  The substring of strA beginning
////    // at indexA of length count is compared with the substring of strB
////    // beginning at indexB of the same length.  Case sensitivity is determined by the ignoreCase boolean.
////    //
////    public static int Compare( String strA, int indexA, String strB, int indexB, int length, bool ignoreCase )
////    {
////        int lengthA = length;
////        int lengthB = length;
////
////        if(strA != null)
////        {
////            if(strA.Length - indexA < lengthA)
////            {
////                lengthA = (strA.Length - indexA);
////            }
////        }
////
////        if(strB != null)
////        {
////            if(strB.Length - indexB < lengthB)
////            {
////                lengthB = (strB.Length - indexB);
////            }
////        }
////
////        if(ignoreCase)
////        {
////            return CultureInfo.CurrentCulture.CompareInfo.Compare( strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.IgnoreCase );
////        }
////        else
////        {
////            return CultureInfo.CurrentCulture.CompareInfo.Compare( strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.None );
////        }
////    }
////
////    // Determines whether two string regions match.  The substring of strA beginning
////    // at indexA of length length is compared with the substring of strB
////    // beginning at indexB of the same length.  Case sensitivity is determined by the ignoreCase boolean,
////    // and the culture is set by culture.
////    //
////    public static int Compare( String strA, int indexA, String strB, int indexB, int length, bool ignoreCase, CultureInfo culture )
////    {
////        if(culture == null)
////        {
////            throw new ArgumentNullException( "culture" );
////        }
////
////        int lengthA = length;
////        int lengthB = length;
////
////        if(strA != null)
////        {
////            if(strA.Length - indexA < lengthA)
////            {
////                lengthA = (strA.Length - indexA);
////            }
////        }
////
////        if(strB != null)
////        {
////            if(strB.Length - indexB < lengthB)
////            {
////                lengthB = (strB.Length - indexB);
////            }
////        }
////
////        if(ignoreCase)
////        {
////            return culture.CompareInfo.Compare( strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.IgnoreCase );
////        }
////        else
////        {
////            return culture.CompareInfo.Compare( strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.None );
////        }
////    }
////
////    public static int Compare( String strA, int indexA, String strB, int indexB, int length, StringComparison comparisonType )
////    {
////        if(comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "NotSupported_StringComparison" ), "comparisonType" );
////        }
////
////        if(strA == null || strB == null)
////        {
////            if((Object)strA == (Object)strB)
////            { //they're both null;
////                return 0;
////            }
////
////            return (strA == null) ? -1 : 1; //-1 if A is null, 1 if B is null.
////        }
////
////        if(length < 0)
////        {
////            throw new ArgumentOutOfRangeException( "length", Environment.GetResourceString( "ArgumentOutOfRange_NegativeLength" ) );
////        }
////
////        if(indexA < 0)
////        {
////            throw new ArgumentOutOfRangeException( "indexA", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////        }
////
////        if(indexB < 0)
////        {
////            throw new ArgumentOutOfRangeException( "indexB", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////        }
////
////        if(strA.Length - indexA < 0)
////        {
////            throw new ArgumentOutOfRangeException( "indexA", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////        }
////
////        if(strB.Length - indexB < 0)
////        {
////            throw new ArgumentOutOfRangeException( "indexB", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////        }
////
////        if((length == 0) || ((strA == strB) && (indexA == indexB)))
////        {
////            return 0;
////        }
////
////        int lengthA = length;
////        int lengthB = length;
////
////        if(strA != null)
////        {
////            if(strA.Length - indexA < lengthA)
////            {
////                lengthA = (strA.Length - indexA);
////            }
////        }
////
////        if(strB != null)
////        {
////            if(strB.Length - indexB < lengthB)
////            {
////                lengthB = (strB.Length - indexB);
////            }
////        }
////
////        switch(comparisonType)
////        {
////            case StringComparison.CurrentCulture:
////                return CultureInfo.CurrentCulture.CompareInfo.Compare( strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.None );
////
////            case StringComparison.CurrentCultureIgnoreCase:
////                return CultureInfo.CurrentCulture.CompareInfo.Compare( strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.IgnoreCase );
////
////            case StringComparison.InvariantCulture:
////                return CultureInfo.InvariantCulture.CompareInfo.Compare( strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.None );
////
////            case StringComparison.InvariantCultureIgnoreCase:
////                return CultureInfo.InvariantCulture.CompareInfo.Compare( strA, indexA, lengthA, strB, indexB, lengthB, CompareOptions.IgnoreCase );
////
////            case StringComparison.Ordinal:
////                return nativeCompareOrdinalEx( strA, indexA, strB, indexB, length );
////
////            case StringComparison.OrdinalIgnoreCase:
////                return (TextInfo.CompareOrdinalIgnoreCaseEx( strA, indexA, strB, indexB, length ));
////
////            default:
////                throw new ArgumentException( Environment.GetResourceString( "NotSupported_StringComparison" ) );
////        }
////
////    }

        // Compares this object to another object, returning an integer that
        // indicates the relationship. This method returns a value less than 0 if this is less than value, 0
        // if this is equal to value, or a value greater than 0
        // if this is greater than value.  Strings are considered to be
        // greater than all non-String objects.  Note that this means sorted
        // arrays would contain nulls, other objects, then Strings in that order.
        //
        public int CompareTo( Object value )
        {
            if(value == null)
            {
                return 1;
            }
    
            if(!(value is String))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeString" ) );
#else
                throw new ArgumentException();
#endif
            }

            return CompareTo( (String)value );
        }

        // Determines the sorting relation of StrB to the current instance.
        //
        public int CompareTo( String strB )
        {
            if(strB == null)
            {
                return 1;
            }
    
            return CultureInfo.CurrentCulture.CompareInfo.Compare( this, strB, 0 );
        }

        // Compares strA and strB using an ordinal (code-point) comparison.
        //
        public static int CompareOrdinal( String strA, String strB )
        {
            if((Object)strA == (Object)strB)
            {
                return 0;
            }
    
            //they can't both be null;
            if(strA == null)
            {
                return -1;
            }
    
            if(strB == null)
            {
                return 1;
            }
    
            return CompareOrdinalHelper( strA, strB );
        }
    
    
        // Compares strA and strB using an ordinal (code-point) comparison.
        //
        public static int CompareOrdinal( String strA, int indexA, String strB, int indexB, int length )
        {
            if(strA == null || strB == null)
            {
                if((Object)strA == (Object)strB)
                { //they're both null;
                    return 0;
                }
    
                return (strA == null) ? -1 : 1; //-1 if A is null, 1 if B is null.
            }
    
            return nativeCompareOrdinalEx( strA, indexA, strB, indexB, length );
        }
    
        public bool Contains( string value )
        {
            return (IndexOf( value, StringComparison.Ordinal ) >= 0);
        }
    
    
        // Determines whether a specified string is a suffix of the the current instance.
        //
        // The case-sensitive and culture-sensitive option is set by options,
        // and the default culture is used.
        //
        public Boolean EndsWith( String value )
        {
            return EndsWith( value, false, null );
        }
    
        public Boolean EndsWith( String value, StringComparison comparisonType )
        {
            if((Object)value == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "value" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "NotSupported_StringComparison" ), "comparisonType" );
#else
                throw new ArgumentException();
#endif
            }
    
            if((Object)this == (Object)value)
            {
                return true;
            }
    
            if(value.Length == 0)
            {
                return true;
            }
    
            switch(comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.IsSuffix( this, value, CompareOptions.None );
    
                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.IsSuffix( this, value, CompareOptions.IgnoreCase );
    
                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.IsSuffix( this, value, CompareOptions.None );
    
                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.IsSuffix( this, value, CompareOptions.IgnoreCase );
    
                case StringComparison.Ordinal:
                    return this.Length < value.Length ? false : (nativeCompareOrdinalEx( this, this.Length - value.Length, value, 0, value.Length ) == 0);
    
////            case StringComparison.OrdinalIgnoreCase:
////                return this.Length < value.Length ? false : (TextInfo.CompareOrdinalIgnoreCaseEx( this, this.Length - value.Length, value, 0, value.Length ) == 0);
    
                default:
#if EXCEPTION_STRINGS
                    throw new ArgumentException( Environment.GetResourceString( "NotSupported_StringComparison" ), "comparisonType" );
#else
                    throw new ArgumentException();
#endif
            }
        }
    
        public Boolean EndsWith( String value, Boolean ignoreCase, CultureInfo culture )
        {
            if(value == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "value" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if((object)this == (object)value)
            {
                return true;
            }
    
            CultureInfo referenceCulture = (culture == null) ? CultureInfo.CurrentCulture : culture;
    
            return referenceCulture.CompareInfo.IsSuffix( this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None );
        }
    
////    internal bool EndsWith( char value )
////    {
////        int thisLen = this.Length;
////        if(thisLen != 0)
////        {
////            if(this[thisLen - 1] == value)
////            {
////                return true;
////            }
////        }
////
////        return false;
////    }
    
    
        // Returns the index of the first occurance of value in the current instance.
        // The search starts at startIndex and runs thorough the next count characters.
        //
        public int IndexOf( char value )
        {
            return IndexOf( value, 0, this.Length );
        }
    
        public int IndexOf( char value, int startIndex )
        {
            return IndexOf( value, startIndex, this.Length - startIndex );
        }
    
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern int IndexOf( char value, int startIndex, int count );
    
        // Returns the index of the first occurance of any character in value in the current instance.
        // The search starts at startIndex and runs to endIndex-1. [startIndex,endIndex).
        //
    
        public int IndexOfAny( char[] anyOf )
        {
            return IndexOfAny( anyOf, 0, this.Length );
        }
    
        public int IndexOfAny( char[] anyOf, int startIndex )
        {
            return IndexOfAny( anyOf, startIndex, this.Length - startIndex );
        }
    
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern int IndexOfAny( char[] anyOf, int startIndex, int count );
    
    
        // Determines the position within this string of the first occurence of the specified
        // string, according to the specified search criteria.  The search begins at
        // the first character of this string, it is case-sensitive and culture-sensitive,
        // and the default culture is used.
        //
        public int IndexOf( String value )
        {
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf( this, value );
        }
    
        // Determines the position within this string of the first occurence of the specified
        // string, according to the specified search criteria.  The search begins at
        // startIndex, it is case-sensitive and culture-sensitve, and the default culture is used.
        //
        public int IndexOf( String value, int startIndex )
        {
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf( this, value, startIndex );
        }
    
        // Determines the position within this string of the first occurence of the specified
        // string, according to the specified search criteria.  The search begins at
        // startIndex, ends at endIndex and the default culture is used.
        //
        public int IndexOf( String value, int startIndex, int count )
        {
            if(startIndex < 0 || startIndex > this.Length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            if(count < 0 || count > this.Length - startIndex)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf( this, value, startIndex, count, CompareOptions.None );
        }
    
        public int IndexOf( String value, StringComparison comparisonType )
        {
            return IndexOf( value, 0, this.Length, comparisonType );
        }
    
        public int IndexOf( String value, int startIndex, StringComparison comparisonType )
        {
            return IndexOf( value, startIndex, this.Length - startIndex, comparisonType );
        }
    
        public int IndexOf( String value, int startIndex, int count, StringComparison comparisonType )
        {
            // Validate inputs
            if(value == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "value" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(startIndex < 0 || startIndex > this.Length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            if(count < 0 || startIndex > this.Length - count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
    
            switch(comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.IndexOf( this, value, startIndex, count, CompareOptions.None );
    
                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.IndexOf( this, value, startIndex, count, CompareOptions.IgnoreCase );
    
                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.IndexOf( this, value, startIndex, count, CompareOptions.None );
    
                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.IndexOf( this, value, startIndex, count, CompareOptions.IgnoreCase );
    
                case StringComparison.Ordinal:
                    return CultureInfo.InvariantCulture.CompareInfo.IndexOf( this, value, startIndex, count, CompareOptions.Ordinal );
    
////            case StringComparison.OrdinalIgnoreCase:
////                return TextInfo.IndexOfStringOrdinalIgnoreCase( this, value, startIndex, count );
    
                default:
#if EXCEPTION_STRINGS
                    throw new ArgumentException( Environment.GetResourceString( "NotSupported_StringComparison" ), "comparisonType" );
#else
                    throw new ArgumentException();
#endif
            }
        }
    
        // Returns the index of the last occurance of value in the current instance.
        // The search starts at startIndex and runs to endIndex. [startIndex,endIndex].
        // The character at position startIndex is included in the search.  startIndex is the larger
        // index within the string.
        //
        public int LastIndexOf( char value )
        {
            return LastIndexOf( value, this.Length - 1, this.Length );
        }
    
        public int LastIndexOf( char value, int startIndex )
        {
            return LastIndexOf( value, startIndex, startIndex + 1 );
        }
    
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern int LastIndexOf( char value, int startIndex, int count );
    
////    // Returns the index of the last occurance of any character in value in the current instance.
////    // The search starts at startIndex and runs to endIndex. [startIndex,endIndex].
////    // The character at position startIndex is included in the search.  startIndex is the larger
////    // index within the string.
////    //
////
////    public int LastIndexOfAny( char[] anyOf )
////    {
////        return LastIndexOfAny( anyOf, this.Length - 1, this.Length );
////    }
////
////    public int LastIndexOfAny( char[] anyOf, int startIndex )
////    {
////        return LastIndexOfAny( anyOf, startIndex, startIndex + 1 );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    public extern int LastIndexOfAny( char[] anyOf, int startIndex, int count );
////
////
////    // Returns the index of the last occurance of any character in value in the current instance.
////    // The search starts at startIndex and runs to endIndex. [startIndex,endIndex].
////    // The character at position startIndex is included in the search.  startIndex is the larger
////    // index within the string.
////    //
////    public int LastIndexOf( String value )
////    {
////        return LastIndexOf( value, this.Length - 1, this.Length, StringComparison.CurrentCulture );
////    }
////
////    public int LastIndexOf( String value, int startIndex )
////    {
////        return LastIndexOf( value, startIndex, startIndex + 1, StringComparison.CurrentCulture );
////    }
////
////    public int LastIndexOf( String value, int startIndex, int count )
////    {
////        if(count < 0)
////        {
////            throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
////        }
////
////        return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf( this, value, startIndex, count, CompareOptions.None );
////    }
////
////    public int LastIndexOf( String value, StringComparison comparisonType )
////    {
////        return LastIndexOf( value, this.Length - 1, this.Length, comparisonType );
////    }
////
////    public int LastIndexOf( String value, int startIndex, StringComparison comparisonType )
////    {
////        return LastIndexOf( value, startIndex, startIndex + 1, comparisonType );
////    }
////
////    public int LastIndexOf( String value, int startIndex, int count, StringComparison comparisonType )
////    {
////        if(value == null)
////        {
////            throw new ArgumentNullException( "value" );
////        }
////
////        // Special case for 0 length input strings
////        if(this.Length == 0 && (startIndex == -1 || startIndex == 0))
////        {
////            return (value.Length == 0) ? 0 : -1;
////        }
////
////        // Make sure we're not out of range
////        if(startIndex < 0 || startIndex > this.Length)
////        {
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////        }
////
////        // Make sure that we allow startIndex == this.Length
////        if(startIndex == this.Length)
////        {
////            startIndex--;
////            if(count > 0)
////            {
////                count--;
////            }
////
////            // If we are looking for nothing, just return 0
////            if(value.Length == 0 && count >= 0 && startIndex - count + 1 >= 0)
////            {
////                return startIndex;
////            }
////        }
////
////        // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
////        if(count < 0 || startIndex - count + 1 < 0)
////        {
////            throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
////        }
////
////
////        switch(comparisonType)
////        {
////            case StringComparison.CurrentCulture:
////                return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf( this, value, startIndex, count, CompareOptions.None );
////
////            case StringComparison.CurrentCultureIgnoreCase:
////                return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf( this, value, startIndex, count, CompareOptions.IgnoreCase );
////
////            case StringComparison.InvariantCulture:
////                return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf( this, value, startIndex, count, CompareOptions.None );
////
////            case StringComparison.InvariantCultureIgnoreCase:
////                return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf( this, value, startIndex, count, CompareOptions.IgnoreCase );
////
////            case StringComparison.Ordinal:
////                return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf( this, value, startIndex, count, CompareOptions.Ordinal );
////
////            case StringComparison.OrdinalIgnoreCase:
////                return TextInfo.LastIndexOfStringOrdinalIgnoreCase( this, value, startIndex, count );
////
////            default:
////                throw new ArgumentException( Environment.GetResourceString( "NotSupported_StringComparison" ), "comparisonType" );
////        }
////    }
////
////    //
////    //
////    public String PadLeft( int totalWidth )
////    {
////        return PadHelper( totalWidth, ' ', false );
////    }
////
////    public String PadLeft( int totalWidth, char paddingChar )
////    {
////        return PadHelper( totalWidth, paddingChar, false );
////    }
////
////    public String PadRight( int totalWidth )
////    {
////        return PadHelper( totalWidth, ' ', true );
////    }
////
////    public String PadRight( int totalWidth, char paddingChar )
////    {
////        return PadHelper( totalWidth, paddingChar, true );
////    }
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern String PadHelper( int totalWidth, char paddingChar, bool isRightPadded );
    
        // Determines whether a specified string is a prefix of the current instance
        //
        public Boolean StartsWith( String value )
        {
            return StartsWith( value, false, null );
        }
    
        public Boolean StartsWith( String value, StringComparison comparisonType )
        {
            if((Object)value == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "value" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(comparisonType < StringComparison.CurrentCulture || comparisonType > StringComparison.OrdinalIgnoreCase)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "NotSupported_StringComparison" ), "comparisonType" );
#else
                throw new ArgumentException();
#endif
            }
    
            if((Object)this == (Object)value)
            {
                return true;
            }
    
            if(value.Length == 0)
            {
                return true;
            }
    
            switch(comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.IsPrefix( this, value, CompareOptions.None );
    
                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.IsPrefix( this, value, CompareOptions.IgnoreCase );
    
                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.IsPrefix( this, value, CompareOptions.None );
    
                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.IsPrefix( this, value, CompareOptions.IgnoreCase );
    
                case StringComparison.Ordinal:
                    if(this.Length < value.Length)
                    {
                        return false;
                    }
    
                    return (nativeCompareOrdinalEx( this, 0, value, 0, value.Length ) == 0);
    
////            case StringComparison.OrdinalIgnoreCase:
////                if(this.Length < value.Length)
////                {
////                    return false;
////                }
////
////                return (TextInfo.CompareOrdinalIgnoreCaseEx( this, 0, value, 0, value.Length ) == 0);
    
                default:
#if EXCEPTION_STRINGS
                    throw new ArgumentException( Environment.GetResourceString( "NotSupported_StringComparison" ), "comparisonType" );
#else
                    throw new ArgumentException();
#endif
            }
        }
    
        public Boolean StartsWith( String value, Boolean ignoreCase, CultureInfo culture )
        {
            if(value == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "value" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if((object)this == (object)value)
            {
                return true;
            }
    
            CultureInfo referenceCulture = (culture == null) ? CultureInfo.CurrentCulture : culture;
    
            return referenceCulture.CompareInfo.IsPrefix( this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None );
        }
    
        // Creates a copy of this string in lower case.
        public String ToLower()
        {
            return this.ToLower( CultureInfo.CurrentCulture );
        }
    
        // Creates a copy of this string in lower case.  The culture is set by culture.
        public String ToLower( CultureInfo culture )
        {
            if(culture == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "culture" );
#else
                throw new ArgumentNullException();
#endif
            }
            return culture.TextInfo.ToLower( this );
        }
    
        // Creates a copy of this string in lower case based on invariant culture.
        public String ToLowerInvariant()
        {
            return this.ToLower( CultureInfo.InvariantCulture );
        }
    
        // Creates a copy of this string in upper case.
        public String ToUpper()
        {
            return this.ToUpper( CultureInfo.CurrentCulture );
        }
    
        // Creates a copy of this string in upper case.  The culture is set by culture.
        public String ToUpper( CultureInfo culture )
        {
            if(culture == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "culture" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            return culture.TextInfo.ToUpper( this );
        }
    
        //Creates a copy of this string in upper case based on invariant culture.
        public String ToUpperInvariant()
        {
            return this.ToUpper( CultureInfo.InvariantCulture );
        }

        // Returns this string.
        public override String ToString()
        {
            return this;
        }

        public String ToString( IFormatProvider provider )
        {
            return this;
        }
    
        // Method required for the ICloneable interface.
        // There's no point in cloning a string since they're immutable, so we simply return this.
        public Object Clone()
        {
            return this;
        }


        // Trims the whitespace from both ends of the string.  Whitespace is defined by
        // CharacterInfo.WhitespaceChars.
        //
        public String Trim()
        {
            return TrimHelper( WhitespaceChars, TrimBoth );
        }
    
        private String TrimHelper( char[] trimChars, int trimType )
        {
            //end will point to the first non-trimmed character on the right
            //start will point to the first non-trimmed character on the Left
            int end   = this.Length - 1;
            int start = 0;
    
            //Trim specified characters.
            if(trimType != TrimTail)
            {
                for(start = 0; start < this.Length; start++)
                {
                    int  i  = 0;
                    char ch = this[start];
    
                    for(i = 0; i < trimChars.Length; i++)
                    {
                        if(trimChars[i] == ch) break;
                    }
    
                    if(i == trimChars.Length)
                    { // the character is not white space
                        break;
                    }
                }
            }
    
            if(trimType != TrimHead)
            {
                for(end = Length - 1; end >= start; end--)
                {
                    int  i  = 0;
                    char ch = this[end];
    
                    for(i = 0; i < trimChars.Length; i++)
                    {
                        if(trimChars[i] == ch) break;
                    }
    
                    if(i == trimChars.Length)
                    { // the character is not white space
                        break;
                    }
                }
            }
    
            //Create a new STRINGREF and initialize it from the range determined above.
            int len = end - start + 1;
            if(len == this.Length)
            {
                // Don't allocate a new string is the trimmed string has not changed.
                return this;
            }
            else
            {
                if(len == 0)
                {
                    return String.Empty;
                }
    
                return InternalSubString( start, len, false );
            }
        }
    
////    //
////    //
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    public extern String Insert( int startIndex, String value );
////
////    // Replaces all instances of oldChar with newChar.
////    //
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
        public String Replace( char oldChar, char newChar )
        {
            StringBuilder sb = new StringBuilder( this.Length );

            for(var i = 0; i< Length; ++i)
            {
                char c = this[ i ];
                sb.Append( c == oldChar ? newChar : c );
            }

            return sb.ToString();
        }
    
////    // This method contains the same functionality as StringBuilder Replace. The only difference is that
////    // a new String has to be allocated since Strings are immutable
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    public extern String Replace( String oldValue, String newValue );
////
////    //
////    //
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    public extern String Remove( int startIndex, int count );
////
////
////    // a remove that just takes a startindex.
////    public string Remove( int startIndex )
////    {
////        if(startIndex < 0)
////        {
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_StartIndex" ) );
////        }
////
////        if(startIndex >= Length)
////        {
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_StartIndexLessThanLength" ) );
////        }
////
////        return Substring( 0, startIndex );
////    }

        public static String Format( String format, Object arg0 )
        {
            return Format( null, format, new Object[] { arg0 } );
        }

        public static String Format( String format, Object arg0, Object arg1 )
        {
            return Format( null, format, new Object[] { arg0, arg1 } );
        }

        public static String Format( String format, Object arg0, Object arg1, Object arg2 )
        {
            return Format( null, format, new Object[] { arg0, arg1, arg2 } );
        }


        public static String Format( String format, params Object[] args )
        {
            return Format( null, format, args );
        }

        public static String Format( IFormatProvider provider, String format, params Object[] args )
        {
            if(format == null || args == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( (format == null) ? "format" : "args" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            StringBuilder sb = new StringBuilder( format.Length + args.Length * 8 );
    
            sb.AppendFormat( provider, format, args );
    
            return sb.ToString();
        }

        unsafe public static String Copy( String str )
        {
            if(str == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "str" );
#else
                throw new ArgumentNullException();
#endif
            }

            int length = str.Length;

            String result = FastAllocateString( length );

            fixed(char* dest = &result.m_firstChar)
            {
                fixed(char* src = &str.m_firstChar)
                {
                    wstrcpyPtrAligned( dest, src, length );
                }
            }

            return result;
        }

        // Used by StringBuilder to avoid data corruption
        unsafe internal static String InternalCopy( String str )
        {
            int    length = str.Length;
            String result = FastAllocateString( length );

            // The underlying's String can changed length is StringBuilder
            fixed(char* dest = &result.m_firstChar)
            {
                fixed(char* src = &str.m_firstChar)
                {
                    wstrcpyPtrAligned( dest, src, length );
                }
            }

            return result;
        }

        public static String Concat( Object arg0 )
        {
            if(arg0 == null)
            {
                return String.Empty;
            }

            return arg0.ToString();
        }

        public static String Concat( Object arg0, Object arg1 )
        {
            if(arg0 == null)
            {
                arg0 = String.Empty;
            }

            if(arg1 == null)
            {
                arg1 = String.Empty;
            }

            return Concat( arg0.ToString(), arg1.ToString() );
        }

        public static String Concat( Object arg0, Object arg1, Object arg2 )
        {
            if(arg0 == null)
            {
                arg0 = String.Empty;
            }

            if(arg1 == null)
            {
                arg1 = String.Empty;
            }

            if(arg2 == null)
            {
                arg2 = String.Empty;
            }

            return Concat( arg0.ToString(), arg1.ToString(), arg2.ToString() );
        }

////    [CLSCompliant( false )]
////    public static String Concat( Object arg0, Object arg1, Object arg2, Object arg3, __arglist )
////    {
////        Object[] objArgs;
////        int      argCount;
////
////        ArgIterator args = new ArgIterator( __arglist );
////
////        //+4 to account for the 4 hard-coded arguments at the beginning of the list.
////        argCount = args.GetRemainingCount() + 4;
////
////        objArgs = new Object[argCount];
////
////        //Handle the hard-coded arguments
////        objArgs[0] = arg0;
////        objArgs[1] = arg1;
////        objArgs[2] = arg2;
////        objArgs[3] = arg3;
////
////        //Walk all of the args in the variable part of the argument list.
////        for(int i = 4; i < argCount; i++)
////        {
////            objArgs[i] = TypedReference.ToObject( args.GetNextArg() );
////        }
////
////        return Concat( objArgs );
////    }


        public static String Concat( params Object[] args )
        {
            if(args == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "args" );
#else
                throw new ArgumentNullException();
#endif
            }

            String[] sArgs       = new String[args.Length];
            int      totalLength = 0;

            for(int i = 0; i < args.Length; i++)
            {
                object value = args[i];

                sArgs[i] = ((value == null) ? (String.Empty) : (value.ToString()));

                totalLength += sArgs[i].Length;

                // check for overflow
                if(totalLength < 0)
                {
                    throw new OutOfMemoryException();
                }
            }

            return ConcatArray( sArgs, totalLength );
        }


        public static String Concat( String str0, String str1 )
        {
            if(IsNullOrEmpty( str0 ))
            {
                if(IsNullOrEmpty( str1 ))
                {
                    return String.Empty;
                }
                return str1;
            }

            if(IsNullOrEmpty( str1 ))
            {
                return str0;
            }

            int str0Length = str0.Length;

            String result = FastAllocateString( str0Length + str1.Length );

            FillStringChecked( result, 0         , str0 );
            FillStringChecked( result, str0Length, str1 );

            return result;
        }

        public static String Concat( String str0, String str1, String str2 )
        {
            if(str0 == null && str1 == null && str2 == null)
            {
                return String.Empty;
            }

            if(str0 == null)
            {
                str0 = String.Empty;
            }

            if(str1 == null)
            {
                str1 = String.Empty;
            }

            if(str2 == null)
            {
                str2 = String.Empty;
            }

            int totalLength = str0.Length + str1.Length + str2.Length;

            String result = FastAllocateString( totalLength );

            FillStringChecked( result, 0                        , str0 );
            FillStringChecked( result, str0.Length              , str1 );
            FillStringChecked( result, str0.Length + str1.Length, str2 );

            return result;
        }

        public static String Concat( String str0, String str1, String str2, String str3 )
        {
            if(str0 == null && str1 == null && str2 == null && str3 == null)
            {
                return String.Empty;
            }

            if(str0 == null)
            {
                str0 = String.Empty;
            }

            if(str1 == null)
            {
                str1 = String.Empty;
            }

            if(str2 == null)
            {
                str2 = String.Empty;
            }

            if(str3 == null)
            {
                str3 = String.Empty;
            }

            int totalLength = str0.Length + str1.Length + str2.Length + str3.Length;

            String result = FastAllocateString( totalLength );

            FillStringChecked( result, 0                                      , str0 );
            FillStringChecked( result, str0.Length                            , str1 );
            FillStringChecked( result, str0.Length + str1.Length              , str2 );
            FillStringChecked( result, str0.Length + str1.Length + str2.Length, str3 );

            return result;
        }

        private static String ConcatArray( String[] values, int totalLength )
        {
            String result = FastAllocateString( totalLength );
            int currPos = 0;

            for(int i = 0; i < values.Length; i++)
            {
                BCLDebug.Assert( (currPos <= totalLength - values[i].Length), "[String.ConcatArray](currPos <= totalLength - values[i].Length)" );

                FillStringChecked( result, currPos, values[i] );

                currPos += values[i].Length;
            }

            return result;
        }

        public static String Concat( params String[] values )
        {
            int totalLength = 0;

            if(values == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "values" );
#else
                throw new ArgumentNullException();
#endif
            }

            // Always make a copy to prevent changing the array on another thread.
            String[] internalValues = new String[values.Length];

            for(int i = 0; i < values.Length; i++)
            {
                string value = values[i];

                internalValues[i] = ((value == null) ? (String.Empty) : (value));

                totalLength += internalValues[i].Length;

                // check for overflow
                if(totalLength < 0)
                {
                    throw new OutOfMemoryException();
                }
            }

            return ConcatArray( internalValues, totalLength );
        }

        public static String Intern( String str )
        {
            return str; // BUGBUG: Should we implement interning?
////        if(str == null)
////        {
////            throw new ArgumentNullException( "str" );
////        }
////
////        return Thread.GetDomain().GetOrInternString( str );
        }

        public static String IsInterned( String str )
        {
            return str; // BUGBUG: Should we implement interning?
////        if(str == null)
////        {
////            throw new ArgumentNullException( "str" );
////        }
////        return Thread.GetDomain().IsStringInterned( str );
        }


        #region IConvertible

        public TypeCode GetTypeCode()
        {
            return TypeCode.String;
        }
    
        /// <internalonly/>
        bool IConvertible.ToBoolean( IFormatProvider provider )
        {
            return Convert.ToBoolean( this, provider );
        }
    
        /// <internalonly/>
        char IConvertible.ToChar( IFormatProvider provider )
        {
            return Convert.ToChar( this, provider );
        }
    
        /// <internalonly/>
        sbyte IConvertible.ToSByte( IFormatProvider provider )
        {
            return Convert.ToSByte( this, provider );
        }
    
        /// <internalonly/>
        byte IConvertible.ToByte( IFormatProvider provider )
        {
            return Convert.ToByte( this, provider );
        }
    
        /// <internalonly/>
        short IConvertible.ToInt16( IFormatProvider provider )
        {
            return Convert.ToInt16( this, provider );
        }
    
        /// <internalonly/>
        ushort IConvertible.ToUInt16( IFormatProvider provider )
        {
            return Convert.ToUInt16( this, provider );
        }
    
        /// <internalonly/>
        int IConvertible.ToInt32( IFormatProvider provider )
        {
            return Convert.ToInt32( this, provider );
        }
    
        /// <internalonly/>
        uint IConvertible.ToUInt32( IFormatProvider provider )
        {
            return Convert.ToUInt32( this, provider );
        }
    
        /// <internalonly/>
        long IConvertible.ToInt64( IFormatProvider provider )
        {
            return Convert.ToInt64( this, provider );
        }
    
        /// <internalonly/>
        ulong IConvertible.ToUInt64( IFormatProvider provider )
        {
            return Convert.ToUInt64( this, provider );
        }
    
        /// <internalonly/>
        float IConvertible.ToSingle( IFormatProvider provider )
        {
            return Convert.ToSingle( this, provider );
        }
    
        /// <internalonly/>
        double IConvertible.ToDouble( IFormatProvider provider )
        {
            return Convert.ToDouble( this, provider );
        }
    
        /// <internalonly/>
        Decimal IConvertible.ToDecimal( IFormatProvider provider )
        {
            return Convert.ToDecimal( this, provider );
        }
    
        /// <internalonly/>
        DateTime IConvertible.ToDateTime( IFormatProvider provider )
        {
            return Convert.ToDateTime( this, provider );
        }
    
        /// <internalonly/>
        Object IConvertible.ToType( Type type, IFormatProvider provider )
        {
            return Convert.DefaultToType( (IConvertible)this, type, provider );
        }

        #endregion

////    // Is this a string that can be compared quickly (that is it has only characters > 0x80
////    // and not a - or '
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool IsFastSort();
////
////    // Is this a string that only contains characters < 0x80.
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal extern bool IsAscii();

        ///<internalonly/>
        unsafe internal void SetChar( int index, char value )
        {
#if _DEBUG
            BCLDebug.Assert( ValidModifiableString(), "Modifiable string must not have highChars flags set" );
#endif

            //Bounds check and then set the actual bit.
            if((UInt32)index >= (UInt32)Length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            fixed(char* p = &this.m_firstChar)
            {
                // Set the character.
                p[index] = value;
            }
        }

#if _DEBUG
        // Only used in debug build. Insure that the HighChar state information for a string is not set as known
        [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        private extern bool ValidModifiableString();
#endif

        ///<internalonly/>
        unsafe internal void AppendInPlace( char value, int currentLength )
        {
            BCLDebug.Assert( currentLength < m_arrayLength, "[String.AppendInPlace]currentLength < m_arrayLength" );
#if _DEBUG
            BCLDebug.Assert( ValidModifiableString(), "Modifiable string must not have highChars flags set" );
#endif

            fixed(char* p = &this.m_firstChar)
            {
                // Append the character.
                p[currentLength] = value;
                currentLength++;

                p[currentLength] = '\0';

                m_stringLength = currentLength;
            }
        }


        ///<internalonly/>
        unsafe internal void AppendInPlace( char value, int repeatCount, int currentLength )
        {
            int newLength = currentLength + repeatCount;

            BCLDebug.Assert( newLength < m_arrayLength, "[String.AppendInPlace]currentLength+repeatCount < m_arrayLength" );
#if _DEBUG
            BCLDebug.Assert( ValidModifiableString(), "Modifiable string must not have highChars flags set" );
#endif

            fixed(char* p = &this.m_firstChar)
            {
                int i;

                for(i = currentLength; i < newLength; i++)
                {
                    p[i] = value;
                }
                p[i] = '\0';
            }

            this.m_stringLength = newLength;
        }

        ///<internalonly/>
        internal unsafe void AppendInPlace( String value, int currentLength )
        {
            int count     = value.Length;
            int newLength = currentLength + count;

            BCLDebug.Assert( value != null, "[String.AppendInPlace]value!=null" );
            BCLDebug.Assert( newLength < this.m_arrayLength, "[String.AppendInPlace]Length is wrong." );
#if _DEBUG
            BCLDebug.Assert( ValidModifiableString(), "Modifiable string must not have highChars flags set" );
#endif

            fixed(char* dest = &this.m_firstChar)
            {
                fixed(char* src = &value.m_firstChar)
                {
                    wstrcpy( dest + currentLength, src, count );
                }

                dest[newLength] = '\0';
            }

            this.m_stringLength = newLength;
        }

        internal unsafe void AppendInPlace( String value, int startIndex, int count, int currentLength )
        {
            int newLength = currentLength + count;

            BCLDebug.Assert( value != null                       , "[String.AppendInPlace]value!=null" );
            BCLDebug.Assert( newLength < this.m_arrayLength      , "[String.AppendInPlace]newLength < this.m_arrayLength" );
            BCLDebug.Assert( count >= 0                          , "[String.AppendInPlace]count>=0" );
            BCLDebug.Assert( startIndex >= 0                     , "[String.AppendInPlace]startIndex>=0" );
            BCLDebug.Assert( startIndex <= (value.Length - count), "[String.AppendInPlace]startIndex <= (value.Length - count)" );
#if _DEBUG
            BCLDebug.Assert( ValidModifiableString(), "Modifiable string must not have highChars flags set" );
#endif

            fixed(char* dest = &this.m_firstChar)
            {
                fixed(char* src = &value.m_firstChar)
                {
                    wstrcpy( dest + currentLength, src + startIndex, count );
                }
                dest[newLength] = '\0';
            }

            this.m_stringLength = newLength;
        }

        internal unsafe void AppendInPlace( char* value, int count, int currentLength )
        {
            int newLength = currentLength + count;

            BCLDebug.Assert( value != null                 , "[String.AppendInPlace]value!=null" );
            BCLDebug.Assert( newLength < this.m_arrayLength, "[String.AppendInPlace]newLength < this.m_arrayLength" );
            BCLDebug.Assert( count >= 0                    , "[String.AppendInPlace]count>=0" );
#if _DEBUG
            BCLDebug.Assert(ValidModifiableString(), "Modifiable string must not have highChars flags set");
#endif

            fixed(char* p = &this.m_firstChar)
            {
                wstrcpy( p + currentLength, value, count );

                p[newLength] = '\0';
            }

            this.m_stringLength = newLength;
        }


        ///<internalonly/>
        internal unsafe void AppendInPlace( char[] value, int start, int count, int currentLength )
        {
            int newLength = currentLength + count;

            BCLDebug.Assert( value != null, "[String.AppendInPlace]value!=null" );

            BCLDebug.Assert( newLength < this.m_arrayLength, "[String.AppendInPlace]Length is wrong." );
            BCLDebug.Assert( value.Length - count >= start, "[String.AppendInPlace]value.Length-count>=start" );
#if _DEBUG
            BCLDebug.Assert(ValidModifiableString(), "Modifiable string must not have highChars flags set");
#endif

            fixed(char* dest = &this.m_firstChar)
            {
                // Note: fixed does not like empty arrays
                if(count > 0)
                {
                    fixed(char* src = value)
                    {
                        wstrcpy( dest + currentLength, src + start, count );
                    }
                }

                dest[newLength] = '\0';
            }

            this.m_stringLength = newLength;
        }


        ///<internalonly/>
        unsafe internal void ReplaceCharInPlace( char oldChar, char newChar, int startIndex, int count, int currentLength )
        {
            BCLDebug.Assert( startIndex >= 0                      , "[String.ReplaceCharInPlace]startIndex>0" );
            BCLDebug.Assert( startIndex <= currentLength          , "[String.ReplaceCharInPlace]startIndex>=Length" );
            BCLDebug.Assert( (startIndex <= currentLength - count), "[String.ReplaceCharInPlace]count>0 && startIndex<=currentLength-count" );
#if _DEBUG
            BCLDebug.Assert( ValidModifiableString(), "Modifiable string must not have highChars flags set" );
#endif

            int endIndex = startIndex + count;

            fixed(char* p = &this.m_firstChar)
            {
                for(int i = startIndex; i < endIndex; i++)
                {
                    if(p[i] == oldChar)
                    {
                        p[i] = newChar;
                    }
                }
            }
        }

        ///<internalonly/>
        internal static String GetStringForStringBuilder( String value, int capacity )
        {
            BCLDebug.Assert( value != null, "[String.GetStringForStringBuilder]value!=null" );

            return GetStringForStringBuilder( value, 0, value.Length, capacity );
        }

        ///<internalonly/>
        unsafe internal static String GetStringForStringBuilder( String value, int startIndex, int length, int capacity )
        {
            BCLDebug.Assert( value != null     , "[String.GetStringForStringBuilder]value!=null" );
            BCLDebug.Assert( capacity >= length, "[String.GetStringForStringBuilder]capacity>=length" );

            String newStr = FastAllocateString( capacity );

            if(value.Length == 0)
            {
                newStr.SetLength( 0 );
                // already null terminated
                return newStr;
            }

            fixed(char* dest = &newStr.m_firstChar)
            {
                fixed(char* src = &value.m_firstChar)
                {
                    wstrcpy( dest, src + startIndex, length );
                }
            }
            newStr.SetLength( length );

            // already null terminated
            return newStr;
        }

        ///<internalonly/>
        private unsafe void NullTerminate()
        {
            fixed(char* p = &this.m_firstChar)
            {
                p[m_stringLength] = '\0';
            }
        }

        ///<internalonly/>
        unsafe internal void ClearPostNullChar()
        {
            int newLength = Length + 1;
            if(newLength < m_arrayLength)
            {
                fixed(char* p = &this.m_firstChar)
                {
                    p[newLength] = '\0';
                }
            }
        }

        ///<internalonly/>
        internal void SetLength( int newLength )
        {
            BCLDebug.Assert( newLength <= m_arrayLength, "newLength<=m_arrayLength" );

            m_stringLength = newLength;
        }



////    public CharEnumerator GetEnumerator()
////    {
////        BCLDebug.Perf( false, "Avoid using String's CharEnumerator until C# special cases foreach on String - use the indexed property on String instead." );
////        return new CharEnumerator( this );
////    }
////
////    IEnumerator<char> IEnumerable<char>.GetEnumerator()
////    {
////        BCLDebug.Perf( false, "Avoid using String's CharEnumerator until C# special cases foreach on String - use the indexed property on String instead." );
////        return new CharEnumerator( this );
////    }
////
////    /// <internalonly/>
////    IEnumerator IEnumerable.GetEnumerator()
////    {
////        BCLDebug.Perf( false, "Avoid using String's CharEnumerator until C# special cases foreach on String - use the indexed property on String instead." );
////        return new CharEnumerator( this );
////    }

        internal unsafe void InternalSetCharNoBoundsCheck( int index, char value )
        {
            fixed(char* p = &this.m_firstChar)
            {
                p[index] = value;
            }
        }

////    // Copies the source String (byte buffer) to the destination IntPtr memory allocated with len bytes.
////    internal unsafe static void InternalCopy( String src, IntPtr dest, int len )
////    {
////        if(len == 0)
////        {
////            return;
////        }
////
////        fixed(char* charPtr = &src.m_firstChar)
////        {
////            byte* srcPtr = (byte*)charPtr;
////            byte* dstPtr = (byte*)dest.ToPointer();
////
////            Buffer.memcpyimpl( srcPtr, dstPtr, len );
////        }
////    }

        // memcopies characters inside a String.
        internal unsafe static void InternalMemCpy( String src, int srcOffset, String dst, int destOffset, int len )
        {
            if(len == 0)
            {
                return;
            }

            fixed(char* srcPtr = &src.m_firstChar)
            {
                fixed(char* dstPtr = &dst.m_firstChar)
                {
                    Buffer.InternalMemoryCopy( srcPtr + srcOffset, dstPtr + destOffset, len );
                }
            }
        }

        internal unsafe void InsertInPlace( int index, String value, int repeatCount, int currentLength, int requiredLength )
        {
            BCLDebug.Assert( requiredLength < m_arrayLength                    , "[String.InsertString] requiredLength  < m_arrayLength" );
            BCLDebug.Assert( index >= 0                                        , "index >= 0" );
            BCLDebug.Assert( value.Length * repeatCount < m_arrayLength - index, "[String.InsertString] value.Length * repeatCount < m_arrayLength - index" );
#if _DEBUG
            BCLDebug.Assert( ValidModifiableString(), "Modifiable string must not have highChars flags set" );
#endif
            //Copy the old characters over to make room and then insert the new characters.
            fixed(char* srcPtr = &this.m_firstChar)
            {
                fixed(char* valuePtr = &value.m_firstChar)
                {
                    Buffer.InternalBackwardMemoryCopy( srcPtr + index, srcPtr + index + value.Length * repeatCount, currentLength - index );

                    for(int i = 0; i < repeatCount; i++)
                    {
                        Buffer.InternalMemoryCopy( valuePtr, srcPtr + index + i * value.Length, value.Length );
                    }
                }
            }

            SetLength( requiredLength );

            NullTerminate();
        }

        // Note InsertInPlace char[] has slightly different semantics than the one that takes a String.
        internal unsafe void InsertInPlace( int    index          ,
                                            char[] value          ,
                                            int    startIndex     ,
                                            int    charCount      ,
                                            int    currentLength  ,
                                            int    requiredLength )
        {
            BCLDebug.Assert( requiredLength < m_arrayLength        , "[String.InsertInPlace] requiredLength  < m_arrayLength" );
            BCLDebug.Assert( startIndex >= 0 && index >= 0         , "startIndex >= 0 && index >= 0" );
            BCLDebug.Assert( charCount <= value.Length - startIndex, "[String.InsertInPlace] charCount <= value.Length - startIndex" );
            BCLDebug.Assert( charCount < m_arrayLength - index     , "[String.InsertInPlace]charCount < m_arrayLength - index" );
#if _DEBUG
            BCLDebug.Assert( ValidModifiableString(), "Modifiable string must not have highChars flags set" );
#endif

            //Copy the old characters over to make room and then insert the new characters.
            fixed(char* srcPtr = &this.m_firstChar)
            {
                fixed(char* valuePtr = value)
                {
                    Buffer.InternalBackwardMemoryCopy(                        srcPtr + index, srcPtr + index + charCount, currentLength - index );
                    Buffer.InternalMemoryCopy        ( valuePtr + startIndex, srcPtr + index,                  charCount                        );
                }
            }

            SetLength( requiredLength );

            NullTerminate();
        }

        internal unsafe void RemoveInPlace( int index, int charCount, int currentLength )
        {
            BCLDebug.Assert( index >= 0, "index >= 0" );
            BCLDebug.Assert( charCount < m_arrayLength - index, "[String.InsertInPlace]charCount < m_arrayLength - index" );
#if _DEBUG
            BCLDebug.Assert(ValidModifiableString(), "Modifiable string must not have highChars flags set");
#endif
            //Move the remaining characters to the left and set the string length.
            String.InternalMemCpy( this, index + charCount, this, index, currentLength - charCount - index );

            int newLength = currentLength - charCount;

            SetLength( newLength );
            NullTerminate(); //Null terminate.
        }
    }

    [Flags]
    public enum StringSplitOptions
    {
        None               = 0,
        RemoveEmptyEntries = 1,
    }
}
