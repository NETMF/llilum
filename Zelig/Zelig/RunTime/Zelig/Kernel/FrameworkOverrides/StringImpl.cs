//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.String))]
    public class StringImpl
    {
        //
        // State
        //

        //
        // Aliasing to mark fields in mscorlib as well-known.
        //

#pragma warning disable 649
        [TS.AssumeReferenced] [TS.WellKnownField( "StringImpl_ArrayLength"  )] [AliasForBaseField] internal int  m_arrayLength;
        [TS.AssumeReferenced] [TS.WellKnownField( "StringImpl_StringLength" )] [AliasForBaseField] internal int  m_stringLength;
        [TS.AssumeReferenced] [TS.WellKnownField( "StringImpl_FirstChar"    )] [AliasForBaseField] internal char m_firstChar;
#pragma warning restore 649

        //
        // Constructor Methods
        //

        [DiscardTargetImplementation]
        [TS.WellKnownMethod( "StringImpl_ctor_charArray_int_int" )]
        public unsafe StringImpl( char[] value      ,
                                  int    startIndex ,
                                  int    length     )
        {
////        BCLDebug.Assert( startIndex >= 0 && startIndex <= this.Length         , "StartIndex is out of range!" );
////        BCLDebug.Assert( length     >= 0 && startIndex <= this.Length - length, "length is out of range!"     );

            if(length     < 0 ||               length  > m_stringLength ||
               startIndex < 0 || (startIndex + length) > value.Length   )
            {
                ThreadImpl.ThrowIndexOutOfRangeException();
            }

            if(length > 0)
            {
                fixed(char* dest = &this.m_firstChar)
                {
                    fixed(char* src = &value[startIndex])
                    {
                        Buffer.InternalMemoryCopy( src, dest, length);
                    }
                }
            }
        }

        [DiscardTargetImplementation]
        [TS.WellKnownMethod( "StringImpl_ctor_charArray" )]
        public StringImpl( char[] value ) : this( value, 0, value.Length )
        {
        }

        [DiscardTargetImplementation]
        [TS.WellKnownMethod( "StringImpl_ctor_char_int" )]
        public unsafe StringImpl( char c     ,
                                  int  count )
        {
            if(count < 0 || count > m_stringLength)
            {
                ThreadImpl.ThrowIndexOutOfRangeException();
            }

            fixed(char* dest = &this.m_firstChar)
            {
                char* ptr = dest;

                while(--count >= 0)
                {
                    *ptr++ = c;
                }
            }
        }

        //--//

        //
        // Helper Methods
        //

        [TS.WellKnownMethod( "StringImpl_FastAllocateString" )]
        private static StringImpl FastAllocateString( int length )
        {
            StringImpl res = (StringImpl)(object)TypeSystemManager.Instance.AllocateString( TS.VTable.GetFromType( typeof(string) ), length + 1 );

            res.m_stringLength = length;

            return res;
        }

        [TS.WellKnownMethod( "StringImpl_FastAllocateReferenceCountingString" )]
        [TS.DisableAutomaticReferenceCounting]
        private static StringImpl FastAllocateReferenceCountingString( int length )
        {
            StringImpl res = (StringImpl)(object)TypeSystemManager.Instance.AllocateReferenceCountingString( TS.VTable.GetFromType( typeof( string ) ), length + 1 );

            res.m_stringLength = length;

            return res;
        }

        //
        // Aliasing to mark methods in mscorlib as well-known.
        //

        public unsafe int LastIndexOf( char value, int startIndex, int count )
        {
            int retVal = -1;

            if(m_stringLength == 0             ) return -1;
            if(startIndex     <  0             ) throw new ArgumentOutOfRangeException();
            if(count          <  0             ) throw new ArgumentOutOfRangeException();
            if(startIndex     >= m_stringLength) throw new ArgumentOutOfRangeException();
            if(count          >  startIndex + 1) throw new ArgumentOutOfRangeException();

            int end = startIndex - count + 1;

            fixed(char* ptr = (string)(object)this)
            {
                for(int index = startIndex; index >= end; index--)
                {
                    if(ptr[index] == value)
                    {
                        retVal = index;
                        break;
                    }
                }
            }

            return retVal;
        }

        public unsafe int IndexOf( char value, int startIndex, int count )
        {
            int retVal = -1;

            if(m_stringLength    == 0             ) return -1;
            if(startIndex         < 0             ) throw new ArgumentOutOfRangeException();
            if(count              < 0             ) throw new ArgumentOutOfRangeException();
            if(startIndex + count > m_stringLength) throw new ArgumentOutOfRangeException();

            int end = count + startIndex;

            fixed(char* ptr = (string)(object)this)
            {
                for(int index = startIndex; index < end; index++)
                {
                    if(ptr[index] == value)
                    {
                        retVal = index;
                        break;
                    }
                }
            }

            return retVal;
        }

        public unsafe int IndexOfAny( char[] anyOf, int startIndex, int count )
        {
            if(startIndex + count > m_stringLength) throw new IndexOutOfRangeException();

            UInt64[] mask = new UInt64[2];

            char minC = char.MaxValue, maxC = char.MinValue;
            int minIdx = startIndex, maxIdx = startIndex + count - 1;
            int cAny = anyOf.Length;

            for(int i = cAny; --i >= 0; )
            {
                char c = anyOf[i];

                if(c < minC) minC = c;
                if(c > maxC) maxC = c;

                if(c < 128)
                {
                    if(c < 64) mask[0] |= 1ul << c;
                    else mask[1] |= 1ul << ( c - 64 );
                }
            }

            fixed(char* pS = (string)(object)this)
            {
                count += startIndex;

                for(int i = startIndex; i < count; i++)
                {
                    char c = pS[i];

                    if(c == minC) return i;
                    if(c == maxC) return i;

                    if(c > minC && c < maxC)
                    {
                        if(c < 128)
                        {
                            if(c < 64) { if(0 != ( mask[0] & 1ul << c )) return i; }
                            else { if(0 != ( mask[1] & 1ul << ( c - 64 ) )) return i; }
                        }
                        else
                        {
                            for(int j = cAny; --j >= 0; )
                            {
                                if(c == anyOf[j])
                                {
                                    return i + startIndex;
                                }
                            }
                        }
                    }
                }
            }

            return -1;
        }

        //
        // This is used to cast between an object and and ArrayImpl, which is not possible in C#.
        //
        [TS.GenerateUnsafeCast]
        internal extern static StringImpl CastAsString( object target );

        //
        // This is used to cast between an object and and ArrayImpl, which is not possible in C#.
        //
        [TS.GenerateUnsafeCast]
        internal extern String CastThisAsString();

        //--//

        //
        // Access Methods
        //

        [System.Runtime.CompilerServices.IndexerName( "Chars" )]
        public unsafe char this[int index]
        {
            get
            {
                if(index >= 0 && index < m_stringLength)
                {
                    fixed(char* ptr = (string)(object)this)
                    {
                        return ptr[index];
                    }
                }

                throw new IndexOutOfRangeException();
            }
        }
    }
}
