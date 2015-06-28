//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    public unsafe class ArrayReader
    {
        private static readonly System.Text.UTF8Encoding s_stringEncoding = new System.Text.UTF8Encoding();

        //
        // State
        //

        byte[]                m_image;
        int                   m_startPosition;
        int                   m_currentPosition;
        int                   m_endPosition;
        GrowOnlySet< string > m_lookup;

        //
        // Constructor Methods
        //

        public ArrayReader( byte[] image )
        {
            m_image           = image;
            m_startPosition   = 0;
            m_currentPosition = 0;
            m_endPosition     = image.Length;
            m_lookup          = SetFactory.New< string >();
        }

        public ArrayReader( ArrayReader scanner ,
                            int         offset  )
        {
            m_image           = scanner.m_image;
            m_startPosition   = scanner.m_currentPosition + offset;
            m_currentPosition =         m_startPosition;
            m_endPosition     = scanner.m_endPosition;
            m_lookup          = scanner.m_lookup;

            if(m_startPosition < 0)
            {
                throw new IndexOutOfRangeException();
            }
        }

        public ArrayReader( ArrayReader scanner ,
                            int         offset  ,
                            int         count   )
        {
            m_image           = scanner.m_image;
            m_startPosition   = scanner.m_currentPosition + offset;
            m_currentPosition =         m_startPosition;
            m_endPosition     =         m_startPosition   + count;
            m_lookup          = scanner.m_lookup;

            if(m_startPosition < 0 || m_endPosition > scanner.m_endPosition)
            {
                throw new IndexOutOfRangeException();
            }
        }

        //--//

        public override bool Equals( Object obj )
        {
            if(obj is ArrayReader)
            {
                ArrayReader other = (ArrayReader)obj;

                if(this.Length == other.Length)
                {
                    int len = this.Length;

                    while(len-- > 0)
                    {
                        if(m_image[m_startPosition+len] != other.m_image[other.m_startPosition+len])
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int)this.Length;
        }

        //--//

        private void CheckRange( int size )
        {
            if(size < 0 || m_currentPosition + size > m_endPosition)
            {
                throw new IndexOutOfRangeException();
            }
        }

        public bool ReadBoolean()
        {
            return (ReadUInt8() != 0);
        }

        public byte PeekUInt8()
        {
            CheckRange( 1 );

            return m_image[m_currentPosition];
        }

        public byte ReadUInt8()
        {
            CheckRange( 1 );

            byte res = m_image[m_currentPosition];

            m_currentPosition += 1;

            return res;
        }

        public ushort ReadUInt16()
        {
            CheckRange( 2 );

            ushort res = (ushort)(((uint)m_image[m_currentPosition  ]     ) |
                                  ((uint)m_image[m_currentPosition+1] << 8) );

            m_currentPosition += 2;

            return res;
        }

        public uint ReadUInt32()
        {
            CheckRange( 4 );

            uint res = (((uint)m_image[m_currentPosition  ]      ) |
                        ((uint)m_image[m_currentPosition+1] <<  8) |
                        ((uint)m_image[m_currentPosition+2] << 16) |
                        ((uint)m_image[m_currentPosition+3] << 24) );

            m_currentPosition += 4;

            return res;
        }

        public ulong ReadUInt64()
        {
            ulong res = (((ulong)ReadUInt32()      ) |
                         ((ulong)ReadUInt32() << 32) );

            return res;
        }

        public sbyte ReadInt8()
        {
            return (sbyte)ReadUInt8();
        }

        public short ReadInt16()
        {
            return (short)ReadUInt16();
        }

        public int ReadInt32()
        {
            return (int)ReadUInt32();
        }

        public long ReadInt64()
        {
            return (long)ReadUInt64();
        }

        public char ReadChar()
        {
            return (char)ReadUInt16();
        }

        public unsafe float ReadSingle()
        {
            uint tmpBuffer = ReadUInt32();

            return *((float*)&tmpBuffer);
        }

        public unsafe double ReadDouble()
        {
            ulong tmpBuffer = ReadUInt64();

            return *((double*)&tmpBuffer);
        }

        //--//

        public uint ReadCompressedUInt32()
        {
            byte firstByte = this.ReadUInt8();

            if((firstByte & 0x80) == 0)
            {
                // encoded in a single byte
                return firstByte;
            }
            else if((firstByte & 0x40) == 0)
            {
                // encoded in two bytes
                firstByte &= 0x3F;

                return (((uint)firstByte        << 8) |
                        ((uint)this.ReadUInt8()     ) );
            }
            else
            {
                // encoded in four bytes
                firstByte &= 0x3F;

                return (((uint)firstByte        << 24) |
                        ((uint)this.ReadUInt8() << 16) |
                        ((uint)this.ReadUInt8() <<  8) |
                        ((uint)this.ReadUInt8()      ) );
            }
        }

        public string ReadCompressedString()
        {
            byte firstByte = this.ReadUInt8();

            if(firstByte == 0xFF)
            {
                return null;
            }

            m_currentPosition--;

            uint len = this.ReadCompressedUInt32();

            byte[] bytes = this.ReadUInt8Array( (int)len );

            return s_stringEncoding.GetString( bytes );
        }

        public int ReadCompressedToken()
        {
            uint codedToken = ReadCompressedUInt32();

            TokenType tokenType;

            switch(codedToken & 0x3)
            {
                case 0x0: tokenType = TokenType.TypeDef ; break;
                case 0x1: tokenType = TokenType.TypeRef ; break;
                case 0x2: tokenType = TokenType.TypeSpec; break;
                default : throw IllegalMetaDataFormatException.Create( "Bad signature token: {0}", codedToken );
            }

            return MetaData.PackToken( tokenType, (int)(codedToken >> 2) );
        }

        public Guid ReadGuid()
        {
            const int sizeOfGuid = 16;

            return new Guid( this.ReadUInt8Array( sizeOfGuid ) );
        }

        //--//

        public void CopyIntoArray( byte[] dst    ,
                                   int    offset ,
                                   int    count  )
        {
            CheckRange( count );

            Array.Copy( m_image, m_currentPosition, dst, offset, count );

            m_currentPosition += count;
        }

        public byte[] ReadUInt8Array( int count )
        {
            byte[] res = new byte[count];

            CopyIntoArray( res, 0, count );

            return res;
        }

        public byte[] ReadUInt8Array( int count      ,
                                      int extraAlloc )
        {
            byte[] res = new byte[count + extraAlloc];

            CopyIntoArray( res, 0, count );

            return res;
        }

        public char[] ReadCharArray( int count )
        {
            char[] res = new char[count];

            for(int i = 0; i < count; i++)
            {
                res[i] = ReadChar();
            }

            return res;
        }

        public int[] ReadInt32Array( int count )
        {
            int[] res = new int[count];

            for(int i = 0; i < count; i++)
            {
                res[i] = ReadInt32();
            }

            return res;
        }

        public uint[] ReadUInt32Array( int count )
        {
            uint[] res = new uint[count];

            for(int i = 0; i < count; i++)
            {
                res[i] = ReadUInt32();
            }

            return res;
        }

        public string ReadZeroTerminatedUInt8String()
        {
            StringBuilder sb = new StringBuilder();

            while(true)
            {
                byte c = ReadUInt8();

                if(c == 0) break;

                sb.Append( (char)c );
            }

            return sb.ToString();
        }

        public string ReadZeroTerminatedUTF8String()
        {
            int start = m_currentPosition;
            int end   = start;

            while(m_image[end] != 0)
            {
                end++;
            }

            CheckRange( end + 1 - start );

            m_currentPosition = end + 1;

            string res = s_stringEncoding.GetString( m_image, start, end - start );
            string res2;

            if(m_lookup.Contains( res, out res2 ))
            {
                return res2;
            }
            else
            {
                m_lookup.Insert( res );

                return res;
            }
        }

        public string ReadUInt8String( int count )
        {
            char[] res = new char[count];

            for(int i = 0; i < count; i++)
            {
                res[i] = (char)ReadUInt8();
            }

            return new String( res );
        }

        public string ReadUInt16String( int count )
        {
            char[] res = ReadCharArray( count );

            return new String( res );
        }

        //--//

        public ArrayReader CreateSubset( int count )
        {
            CheckRange( count );

            return new ArrayReader( this, 0, count );
        }

        public ArrayReader CreateSubsetAndAdvance( int count )
        {
            ArrayReader res = CreateSubset( count );

            m_currentPosition += count;

            return res;
        }

//        public uint ComputeCRC()
//        {
//            return CRC32.Compute( m_image, m_currentPosition, m_endPosition-m_currentPosition, 0 ); 
//        }

        //--//

        public void Seek( int offset )
        {
            SeekAbsolute( m_currentPosition + offset );
        }

        public void SeekAbsolute( int newPosition )
        {
            if(newPosition < m_startPosition || newPosition > m_endPosition)
            {
                throw new IndexOutOfRangeException();
            }

            m_currentPosition = newPosition;
        }

        public void Align( int count )
        {
            int rem = this.Position % count;

            if(rem != 0)
            {
                Seek( count - rem );
            }
        }

        public void AlignAbsolute( int count )
        {
            int rem = m_currentPosition % count;

            if(rem != 0)
            {
                Seek( count - rem );
            }
        }

        public void Rewind()
        {
            m_currentPosition = m_startPosition;
        }

        public bool IsEOF
        {
            get
            {
                return m_currentPosition == m_endPosition;
            }
        }

        public int Length
        {
            get
            {
                return m_endPosition - m_startPosition;
            }
        }

        public int Position
        {
            get
            {
                return m_currentPosition - m_startPosition;
            }

            set
            {
                int newPosition = m_startPosition + value;

                if(newPosition < m_startPosition || newPosition >= m_endPosition)
                {
                    throw new IndexOutOfRangeException();
                }

                m_currentPosition = newPosition;
            }
        }

        //--//

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            AppendAsString( sb, true );

            return sb.ToString();
        }

        public void AppendAsString( StringBuilder sb    ,
                                    bool          whole )
        {
            int pos = whole ? m_startPosition : m_currentPosition;

            AppendAsString( sb, m_image, pos, m_endPosition - pos );
        }

        //--//

        public static void AppendAsString( StringBuilder sb     ,
                                           byte[]        buffer )
        {
            AppendAsString( sb, buffer, 0, buffer.Length );
        }

        public static void AppendAsString( StringBuilder sb     ,
                                           byte[]        buffer ,
                                           int           offset ,
                                           int           count  )
        {
            int  end    = count + offset;
            bool fFirst = true;

            while(offset < end)
            {
                if(fFirst)
                {
                    fFirst = false;
                }
                else
                {
                    sb.Append( "," );
                }

                sb.Append( "0x" );
                sb.Append( buffer[offset++].ToString( "x2" ) );
            }
        }
    }
}
