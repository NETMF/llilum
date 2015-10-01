// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: BinaryReader
**
**
** Purpose: Wraps a stream and provides convenient read functionality
** for strings and primitive types.
**
**
============================================================*/
namespace System.IO
{

    using System;
    using System.Text;
    using System.Globalization;

////[System.Runtime.InteropServices.ComVisible( true )]
    public class BinaryReader : IDisposable
    {
        private const int MaxCharBytesSize = 128;

        private Stream m_stream;
        private byte[] m_buffer;
        private Decoder m_decoder;
        private byte[] m_charBytes;
        private char[] m_singleChar;
        private char[] m_charBuffer;
        private int m_maxCharsSize;  // From MaxCharBytesSize & Encoding

        // Performance optimization for Read() w/ Unicode.  Speeds us up by ~40% 
        private bool m_2BytesPerChar;
        private bool m_isMemoryStream; // "do we sit on MemoryStream?" for Read/ReadInt32 perf

        public BinaryReader( Stream input )
            : this( input, new UTF8Encoding() )
        {
        }

        public BinaryReader( Stream input, Encoding encoding )
        {
            if(input == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "input" );
#else
                throw new ArgumentNullException();
#endif
            }
            if(encoding == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "encoding" );
#else
                throw new ArgumentNullException();
#endif
            }
            if(!input.CanRead)
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_StreamNotReadable" ) );
#else
                throw new ArgumentException();
#endif
            m_stream = input;
            m_decoder = encoding.GetDecoder();
            m_maxCharsSize = encoding.GetMaxCharCount( MaxCharBytesSize );
            int minBufferSize = encoding.GetMaxByteCount( 1 );  // max bytes per one char
            if(minBufferSize < 16)
                minBufferSize = 16;
            m_buffer = new byte[minBufferSize];
            m_charBuffer = null;
            m_charBytes = null;

            // For Encodings that always use 2 bytes per char (or more), 
            // special case them here to make Read() & Peek() faster.
            m_2BytesPerChar = encoding is UnicodeEncoding;
            // check if BinaryReader is based on MemoryStream, and keep this for it's life
            // we cannot use "as" operator, since derived classes are not allowed
            m_isMemoryStream = (m_stream.GetType() == typeof( MemoryStream ));

            BCLDebug.Assert( m_decoder != null, "[BinaryReader.ctor]m_decoder!=null" );
        }

        public virtual Stream BaseStream
        {
            get
            {
                return m_stream;
            }
        }

        public virtual void Close()
        {
            Dispose( true );
        }

        protected virtual void Dispose( bool disposing )
        {
            if(disposing)
            {
                Stream copyOfStream = m_stream;
                m_stream = null;
                if(copyOfStream != null)
                    copyOfStream.Close();
            }
            m_stream = null;
            m_buffer = null;
            m_decoder = null;
            m_charBytes = null;
            m_singleChar = null;
            m_charBuffer = null;
        }

        /// <internalonly/>
        void IDisposable.Dispose()
        {
            Dispose( true );
        }

        public virtual int PeekChar()
        {
            if(m_stream == null) __Error.FileNotOpen();

            if(!m_stream.CanSeek)
                return -1;
            long origPos = m_stream.Position;
            int ch = Read();
            m_stream.Position = origPos;
            return ch;
        }

        public virtual int Read()
        {
            if(m_stream == null)
            {
                __Error.FileNotOpen();
            }
            return InternalReadOneChar();
        }

        public virtual bool ReadBoolean()
        {
            FillBuffer( 1 );
            return (m_buffer[0] != 0);
        }

        public virtual byte ReadByte()
        {
            // Inlined to avoid some method call overhead with FillBuffer.
            if(m_stream == null) __Error.FileNotOpen();

            int b = m_stream.ReadByte();
            if(b == -1)
                __Error.EndOfFile();
            return (byte)b;
        }

        [CLSCompliant( false )]
        public virtual sbyte ReadSByte()
        {
            FillBuffer( 1 );
            return (sbyte)(m_buffer[0]);
        }

        public virtual char ReadChar()
        {
            int value = Read();
            if(value == -1)
            {
                __Error.EndOfFile();
            }
            return (char)value;
        }

        public virtual short ReadInt16()
        {
            FillBuffer( 2 );
            return (short)(m_buffer[0] | m_buffer[1] << 8);
        }

        [CLSCompliant( false )]
        public virtual ushort ReadUInt16()
        {
            FillBuffer( 2 );
            return (ushort)(m_buffer[0] | m_buffer[1] << 8);
        }

        public virtual int ReadInt32()
        {
            if(m_isMemoryStream)
            {
                // read directly from MemoryStream buffer
                MemoryStream mStream = m_stream as MemoryStream;
                BCLDebug.Assert( mStream != null, "m_stream as MemoryStream != null" );

                return mStream.InternalReadInt32();
            }
            else
            {
                FillBuffer( 4 );
                return (int)(m_buffer[0] | m_buffer[1] << 8 | m_buffer[2] << 16 | m_buffer[3] << 24);
            }
        }

        [CLSCompliant( false )]
        public virtual uint ReadUInt32()
        {
            FillBuffer( 4 );
            return (uint)(m_buffer[0] | m_buffer[1] << 8 | m_buffer[2] << 16 | m_buffer[3] << 24);
        }

        public virtual long ReadInt64()
        {
            FillBuffer( 8 );
            uint lo = (uint)(m_buffer[0] | m_buffer[1] << 8 |
                             m_buffer[2] << 16 | m_buffer[3] << 24);
            uint hi = (uint)(m_buffer[4] | m_buffer[5] << 8 |
                             m_buffer[6] << 16 | m_buffer[7] << 24);
            return (long)((ulong)hi) << 32 | lo;
        }

        [CLSCompliant( false )]
        public virtual ulong ReadUInt64()
        {
            FillBuffer( 8 );
            uint lo = (uint)(m_buffer[0] | m_buffer[1] << 8 |
                             m_buffer[2] << 16 | m_buffer[3] << 24);
            uint hi = (uint)(m_buffer[4] | m_buffer[5] << 8 |
                             m_buffer[6] << 16 | m_buffer[7] << 24);
            return ((ulong)hi) << 32 | lo;
        }

        public virtual unsafe float ReadSingle()
        {
            FillBuffer( 4 );
            uint tmpBuffer = (uint)(m_buffer[0] | m_buffer[1] << 8 | m_buffer[2] << 16 | m_buffer[3] << 24);
            return *((float*)&tmpBuffer);
        }

        public virtual unsafe double ReadDouble()
        {
            FillBuffer( 8 );
            uint lo = (uint)(m_buffer[0] | m_buffer[1] << 8 |
                m_buffer[2] << 16 | m_buffer[3] << 24);
            uint hi = (uint)(m_buffer[4] | m_buffer[5] << 8 |
                m_buffer[6] << 16 | m_buffer[7] << 24);

            ulong tmpBuffer = ((ulong)hi) << 32 | lo;
            return *((double*)&tmpBuffer);
        }

        public virtual decimal ReadDecimal()
        {
            FillBuffer( 16 );
            return Decimal.ToDecimal( m_buffer );
        }

        public virtual String ReadString()
        {
            int currPos = 0;
            int n;
            int stringLength;
            int readLength;
            int charsRead;

            if(m_stream == null)
                __Error.FileNotOpen();

            // Length of the string in bytes, not chars
            stringLength = Read7BitEncodedInt();
            if(stringLength < 0)
            {
#if EXCEPTION_STRINGS
                throw new IOException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "IO.IO_InvalidStringLen_Len" ), stringLength ) );
#else
                throw new IOException();
#endif
            }

            if(stringLength == 0)
            {
                return String.Empty;
            }

            if(m_charBytes == null)
            {
                m_charBytes = new byte[MaxCharBytesSize];
            }

            if(m_charBuffer == null)
            {
                m_charBuffer = new char[m_maxCharsSize];
            }

            StringBuilder sb = null;
            do
            {
                readLength = ((stringLength - currPos) > MaxCharBytesSize) ? MaxCharBytesSize : (stringLength - currPos);

                n = m_stream.Read( m_charBytes, 0, readLength );
                if(n == 0)
                {
                    __Error.EndOfFile();
                }

                charsRead = m_decoder.GetChars( m_charBytes, 0, n, m_charBuffer, 0 );

                if(currPos == 0 && n == stringLength)
                    return new String( m_charBuffer, 0, charsRead );

                if(sb == null)
                    sb = new StringBuilder( stringLength ); // Actual string length in chars may be smaller.
                sb.Append( m_charBuffer, 0, charsRead );
                currPos += n;

            } while(currPos < stringLength);

            return sb.ToString();
        }

        public virtual int Read( char[] buffer, int index, int count )
        {
            if(buffer == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "buffer", Environment.GetResourceString( "ArgumentNull_Buffer" ) );
#else
                throw new ArgumentNullException();
#endif
            }
            if(index < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
            if(count < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
            if(buffer.Length - index < count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                throw new ArgumentException();
#endif
            }

            if(m_stream == null)
                __Error.FileNotOpen();

            return InternalReadChars( buffer, index, count );
        }

        private int InternalReadChars( char[] buffer, int index, int count )
        {
            int charsRead = 0;
            int numBytes = 0;
            int charsRemaining = count;

            if(m_charBytes == null)
            {
                m_charBytes = new byte[MaxCharBytesSize];
            }

            while(charsRemaining > 0)
            {
                // We really want to know what the minimum number of bytes per char
                // is for our encoding.  Otherwise for UnicodeEncoding we'd have to
                // do ~1+log(n) reads to read n characters.
                numBytes = charsRemaining;
                if(m_2BytesPerChar)
                    numBytes <<= 1;
                if(numBytes > MaxCharBytesSize)
                    numBytes = MaxCharBytesSize;

                if(m_isMemoryStream)
                {
                    MemoryStream mStream = m_stream as MemoryStream;
                    BCLDebug.Assert( mStream != null, "m_stream as MemoryStream != null" );

                    int position = mStream.InternalGetPosition();
                    numBytes = mStream.InternalEmulateRead( numBytes );
                    if(numBytes == 0)
                    {
                        return (count - charsRemaining);
                    }
                    charsRead = m_decoder.GetChars( mStream.InternalGetBuffer(), position, numBytes, buffer, index );
                }
                else
                {
                    numBytes = m_stream.Read( m_charBytes, 0, numBytes );
                    if(numBytes == 0)
                    {
                        //  Console.WriteLine("Found no bytes.  We're outta here.");
                        return (count - charsRemaining);
                    }
                    charsRead = m_decoder.GetChars( m_charBytes, 0, numBytes, buffer, index );
                }

                charsRemaining -= charsRead;
                index += charsRead;
                //                Console.WriteLine("That became: " + charsRead + " characters.");
            }
            BCLDebug.Assert( charsRemaining == 0, "We didn't read all the chars we thought we would." );
            return count;
        }

        private int InternalReadOneChar()
        {
            // I know having a separate InternalReadOneChar method seems a little 
            // redundant, but this makes a scenario like the security parser code
            // 20% faster, in addition to the optimizations for UnicodeEncoding I
            // put in InternalReadChars.   
            int charsRead = 0;
            int numBytes = 0;
            long posSav = posSav = 0;

            if(m_stream.CanSeek)
                posSav = m_stream.Position;

            if(m_charBytes == null)
            {
                m_charBytes = new byte[MaxCharBytesSize]; //REVIEW: We need atmost 2 bytes/char here? 
            }
            if(m_singleChar == null)
            {
                m_singleChar = new char[1];
            }

            while(charsRead == 0)
            {
                // We really want to know what the minimum number of bytes per char
                // is for our encoding.  Otherwise for UnicodeEncoding we'd have to
                // do ~1+log(n) reads to read n characters.
                // Assume 1 byte can be 1 char unless m_2BytesPerChar is true.
                numBytes = m_2BytesPerChar ? 2 : 1;

                int r = m_stream.ReadByte();
                m_charBytes[0] = (byte)r;
                if(r == -1)
                    numBytes = 0;
                if(numBytes == 2)
                {
                    r = m_stream.ReadByte();
                    m_charBytes[1] = (byte)r;
                    if(r == -1)
                        numBytes = 1;
                }

                if(numBytes == 0)
                {
                    // Console.WriteLine("Found no bytes.  We're outta here.");
                    return -1;
                }

                BCLDebug.Assert( numBytes == 1 || numBytes == 2, "BinaryReader::InternalReadOneChar assumes it's reading one or 2 bytes only." );

                try
                {

                    charsRead = m_decoder.GetChars( m_charBytes, 0, numBytes, m_singleChar, 0 );
                }
                catch
                {
                    // Handle surrogate char 

                    if(m_stream.CanSeek)
                        m_stream.Seek( (posSav - m_stream.Position), SeekOrigin.Current );
                    // else - we can't do much here

                    throw;
                }

                BCLDebug.Assert( charsRead < 2, "InternalReadOneChar - assuming we only got 0 or 1 char, not 2!" );
                //                Console.WriteLine("That became: " + charsRead + " characters.");
            }
            if(charsRead == 0)
                return -1;
            return m_singleChar[0];
        }

        public virtual char[] ReadChars( int count )
        {
            if(m_stream == null)
            {
                __Error.FileNotOpen();
            }
            if(count < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
            char[] chars = new char[count];
            int n = InternalReadChars( chars, 0, count );
            if(n != count)
            {
                char[] copy = new char[n];
                Buffer.InternalBlockCopy( chars, 0, copy, 0, 2 * n ); // sizeof(char)
                chars = copy;
            }

            return chars;
        }

        public virtual int Read( byte[] buffer, int index, int count )
        {
            if(buffer == null)
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "buffer", Environment.GetResourceString( "ArgumentNull_Buffer" ) );
#else
                throw new ArgumentNullException();
#endif
            if(index < 0)
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            if(count < 0)
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            if(buffer.Length - index < count)
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                throw new ArgumentException();
#endif

            if(m_stream == null) __Error.FileNotOpen();
            return m_stream.Read( buffer, index, count );
        }

        public virtual byte[] ReadBytes( int count )
        {
            if(count < 0) 
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            if(m_stream == null) __Error.FileNotOpen();

            byte[] result = new byte[count];

            int numRead = 0;
            do
            {
                int n = m_stream.Read( result, numRead, count );
                if(n == 0)
                    break;
                numRead += n;
                count -= n;
            } while(count > 0);

            if(numRead != result.Length)
            {
                // Trim array.  This should happen on EOF & possibly net streams.
                byte[] copy = new byte[numRead];
                Buffer.InternalBlockCopy( result, 0, copy, 0, numRead );
                result = copy;
            }

            return result;
        }

        protected virtual void FillBuffer( int numBytes )
        {
            BCLDebug.Assert( m_buffer == null || (numBytes > 0 && numBytes <= m_buffer.Length), "[FillBuffer]numBytes>0 && numBytes<=m_buffer.Length" );
            int bytesRead = 0;
            int n = 0;

            if(m_stream == null) __Error.FileNotOpen();

            // Need to find a good threshold for calling ReadByte() repeatedly
            // vs. calling Read(byte[], int, int) for both buffered & unbuffered
            // streams.
            if(numBytes == 1)
            {
                n = m_stream.ReadByte();
                if(n == -1)
                    __Error.EndOfFile();
                m_buffer[0] = (byte)n;
                return;
            }

            do
            {
                n = m_stream.Read( m_buffer, bytesRead, numBytes - bytesRead );
                if(n == 0)
                {
                    __Error.EndOfFile();
                }
                bytesRead += n;
            } while(bytesRead < numBytes);
        }

        internal protected int Read7BitEncodedInt()
        {
            // Read out an Int32 7 bits at a time.  The high bit
            // of the byte when on means to continue reading more bytes.
            int count = 0;
            int shift = 0;
            byte b;
            do
            {
                // Check for a corrupted stream.  Read a max of 5 bytes.
                // In a future version, add a DataFormatException.
                if(shift == 5 * 7)  // 5 bytes max per Int32, shift += 7
#if EXCEPTION_STRINGS
                    throw new FormatException( Environment.GetResourceString( "Format_Bad7BitInt32" ) );
#else
                    throw new FormatException();
#endif

                // ReadByte handles end of stream cases for us.
                b = ReadByte();
                count |= (b & 0x7F) << shift;
                shift += 7;
            } while((b & 0x80) != 0);
            return count;
        }
    }
}
