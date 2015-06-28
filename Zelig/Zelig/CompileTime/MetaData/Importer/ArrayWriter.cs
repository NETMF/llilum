//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    public unsafe class ArrayWriter
    {
        private static readonly System.Text.UTF8Encoding s_stringEncoding = new System.Text.UTF8Encoding();

        //
        // State
        //

        System.IO.MemoryStream m_image;

        //
        // Constructor Methods
        //

        public ArrayWriter()
        {
            m_image = new System.IO.MemoryStream();
        }

        //
        // Helper Methods
        //

        public void SetLength( int length )
        {
            m_image.SetLength( length );
        }

        public void Seek( int offset )
        {
            if(offset > m_image.Length)
            {
                SetLength( offset );
            }

            m_image.Seek( offset, System.IO.SeekOrigin.Begin );
        }

        public void Write( bool val )
        {
            Write( val ? 1 : 0 );
        }

        public void Write( byte val )
        {
            m_image.WriteByte( val );
        }

        public void Write( ushort val )
        {
            Write( (byte) val       );
            Write( (byte)(val >> 8) );
        }

        public void Write( uint val )
        {
            Write( (byte) val        );
            Write( (byte)(val >>  8) );
            Write( (byte)(val >> 16) );
            Write( (byte)(val >> 24) );
        }

        public void Write( ulong val )
        {
            Write( (uint) val        );
            Write( (uint)(val >> 32) );
        }

        public void Write( sbyte val )
        {
            Write( (byte)val );
        }

        public void Write( short val )
        {
            Write( (ushort)val );
        }

        public void Write( int val )
        {
            Write( (uint)val );
        }

        public void Write( long val )
        {
            Write( (ulong)val );
        }

        public void Write( char val )
        {
            Write( (ushort)val );
        }

        public unsafe void Write( float val )
        {
            Write( *((uint*)&val) );
        }

        public unsafe void Write( double val )
        {
            Write( *((ulong*)&val) );
        }

        public void Write( ArrayWriter sub )
        {
            byte[] buf = sub.m_image.ToArray();

            m_image.Write( buf, 0, buf.Length );
        }

        //--//

        public void WriteCompressedUInt32( uint val )
        {
            if(val < 0x80)
            {
                // encoded in a single byte
                Write( (byte)val );
                return;
            }
            else if(val < 0x4000)
            {
                // encoded in two bytes
                Write( (byte)((val >> 8) & 0x3F | 0x80) );
                Write( (byte)((val     )              ) );
            }
            else
            {
                // encoded in four bytes
                Write( (byte)((val >> 24) & 0x3F | 0xC0) );
                Write( (byte)((val >> 16)              ) );
                Write( (byte)((val >>  8)              ) );
                Write( (byte)((val      )              ) );
            }
        }

        public void WriteCompressedString( string val )
        {
            if(val == null)
            {
                Write( (byte)0xFF );
            }
            else
            {
                uint len = (uint)val.Length;

                WriteCompressedUInt32( len );

                Write( s_stringEncoding.GetBytes( val ) );
            }
        }

        public void WriteCompressedToken( int val )
        {
            TokenType tokenType =       MetaData.UnpackTokenAsType ( val );
            uint      tokenIdx  = (uint)MetaData.UnpackTokenAsIndex( val );
            uint      codedToken;

            switch(tokenType)
            {
                case TokenType.TypeDef : codedToken = 0; break;
                case TokenType.TypeRef : codedToken = 1; break;
                case TokenType.TypeSpec: codedToken = 2; break;
                default : throw IllegalMetaDataFormatException.Create( "Bad signature token: {0}", val );
            }

            WriteCompressedUInt32( tokenIdx << 2 | codedToken );
        }

        public void Write( Guid val )
        {
            Write( val.ToByteArray() );
        }

        //--//--//--//--//

        public void Write( byte[] val )
        {
            Write( val, 0, val.Length );
        }            

        public void Write( byte[] val    ,
                           int    offset ,
                           int    count  )
        {
            m_image.Write( val, offset, count );
        }

        //--//

        public void Write( char[] val )
        {
            Write( val, 0, val.Length );
        }            

        public void Write( char[] val    ,
                           int    offset ,
                           int    count  )
        {
            while(count-- > 0)
            {
                Write( val[offset++] );
            }
        }

        //--//

        public void Write( int[] val )
        {
            Write( val, 0, val.Length );
        }            

        public void Write( int[] val    ,
                           int   offset ,
                           int   count  )
        {
            while(count-- > 0)
            {
                Write( val[offset++] );
            }
        }

        //--//

        public void Write( uint[] val )
        {
            Write( val, 0, val.Length );
        }            

        public void Write( uint[] val    ,
                           int    offset ,
                           int    count  )
        {
            while(count-- > 0)
            {
                Write( val[offset++] );
            }
        }

        //--//

        public void WriteZeroTerminatedUInt8String( string val )
        {
            foreach(char c in val)
            {
                Write( (byte)c );
            }

            Write( (byte)0 );
        }

        public void WriteZeroTerminatedUTF8String( string val )
        {
            foreach(byte b in s_stringEncoding.GetBytes( val ))
            {
                Write( b );
            }

            Write( (byte)0 );
        }

        public void WriteUInt16String( string val )
        {
            foreach(char c in val)
            {
                Write( c );
            }
        }

        //--//

        public void Align( int count )
        {
            int rem = this.Length % count;

            while(rem-- != 0)
            {
                m_image.WriteByte( 0 );
            }
        }

        public byte[] ToArray()
        {
            return m_image.ToArray();
        }

        //
        // Access Methods
        //

        public int Length
        {
            get
            {
                return (int)m_image.Length;
            }
        }
    }
}
