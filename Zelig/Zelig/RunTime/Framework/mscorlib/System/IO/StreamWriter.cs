// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  StreamWriter
**
**
** Purpose: For writing text to streams in a particular 
** encoding.
**
**
===========================================================*/
using System;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Runtime.CompilerServices;
//using System.Runtime.Versioning;
//using System.Runtime.Serialization;

namespace System.IO
{
    // This class implements a TextWriter for writing characters to a Stream.
    // This is designed for character output in a particular Encoding, 
    // whereas the Stream class is designed for byte input and output.  
    // 
    [Serializable]
    public class StreamWriter : TextWriter
    {
        // For UTF-8, the values of 1K for the default buffer size and 4K for the
        // file stream buffer size are reasonable & give very reasonable
        // performance for in terms of construction time for the StreamWriter and
        // write perf.  Note that for UTF-8, we end up allocating a 4K byte buffer,
        // which means we take advantage of adaptive buffering code.
        // The performance using UnicodeEncoding is acceptable.  
        private const int DefaultBufferSize = 1024;   // char[]
        private const int DefaultFileStreamBufferSize = 4096;
        private const int MinBufferSize = 128;

////    // Bit bucket - Null has no backing store. Non closable.
////    public new static readonly StreamWriter Null = new StreamWriter( Stream.Null, new UTF8Encoding( false, true ), MinBufferSize, false );

        internal Stream stream;
        private Encoding encoding;
        private Encoder encoder;
        internal byte[] byteBuffer;
        internal char[] charBuffer;
        internal int charPos;
        internal int charLen;
        internal bool autoFlush;
        private bool haveWrittenPreamble;
        private bool closable;  // For Console.Out - should Finalize call Dispose?

////    // The high level goal is to be tolerant of encoding errors when we read and very strict 
////    // when we write. Hence, default StreamWriter encoding will throw on encoding error.   
////    // Note: when StreamWriter throws on invalid encoding chars (for ex, high surrogate character 
////    // D800-DBFF without a following low surrogate character DC00-DFFF), it will cause the 
////    // internal StreamWriter's state to be irrecoverable as it would have buffered the 
////    // illegal chars and any subsequent call to Flush() would hit the encoding error again. 
////    // Even Close() will hit the exception as it would try to flush the unwritten data. 
////    // May be we can add a DiscardBufferedData() method to get out of such situation (like 
////    // StreamRerader though for different reason). Eitherway, the buffered data will be lost!
        private static Encoding _UTF8NoBOM;
    
        internal static Encoding UTF8NoBOM
        {
            get
            {
                if(_UTF8NoBOM == null)
                {
                    // No need for double lock - we just want to avoid extra
                    // allocations in the common case.
                    UTF8Encoding noBOM = new UTF8Encoding( false, true );
                    //Thread.MemoryBarrier();
                    _UTF8NoBOM = noBOM;
                }
                return _UTF8NoBOM;
            }
        }


        internal StreamWriter() : base( null )
        { // Ask for CurrentCulture all the time 
        }

        public StreamWriter( Stream stream ) : this( stream, UTF8NoBOM, DefaultBufferSize )
        {
        }

        public StreamWriter( Stream stream, Encoding encoding ) : this( stream, encoding, DefaultBufferSize )
        {
        }

        // Creates a new StreamWriter for the given stream.  The 
        // character encoding is set by encoding and the buffer size, 
        // in number of 16-bit characters, is set by bufferSize.  
        // 
        public StreamWriter( Stream stream, Encoding encoding, int bufferSize ) : base( null )
        { // Ask for CurrentCulture all the time
            if(stream == null || encoding == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( (stream == null ? "stream" : "encoding") );
#else
                throw new ArgumentNullException();
#endif
            }

            if(!stream.CanWrite)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_StreamNotWritable" ) );
#else
                throw new ArgumentException();
#endif
            }

            if(bufferSize <= 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "bufferSize", Environment.GetResourceString( "ArgumentOutOfRange_NeedPosNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            Init( stream, encoding, bufferSize );
        }

        // For non closable streams such as Console.Out
        internal StreamWriter( Stream stream, Encoding encoding, int bufferSize, bool closeable ) : this( stream, encoding, bufferSize )
        {
            closable = closeable;
        }

////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
        public StreamWriter( String path ) : this( path, false, UTF8NoBOM, DefaultBufferSize )
        {
        }

////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
        public StreamWriter( String path, bool append ) : this( path, append, UTF8NoBOM, DefaultBufferSize )
        {
        }

////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
        public StreamWriter( String path, bool append, Encoding encoding ) : this( path, append, encoding, DefaultBufferSize )
        {
        }

////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
        public StreamWriter( String path, bool append, Encoding encoding, int bufferSize ) : base( null )
        { // Ask for CurrentCulture all the time
            if(path == null || encoding == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( (path == null ? "path" : "encoding") );
#else
                throw new ArgumentNullException();
#endif
            }

            if(bufferSize <= 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "bufferSize", Environment.GetResourceString( "ArgumentOutOfRange_NeedPosNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            Stream stream = CreateFile( path, append );
            Init( stream, encoding, bufferSize );
        }

        private void Init( Stream stream, Encoding encoding, int bufferSize )
        {
            this.stream = stream;
            this.encoding = encoding;
            this.encoder = encoding.GetEncoder();
            if(bufferSize < MinBufferSize) bufferSize = MinBufferSize;
            charBuffer = new char[bufferSize];
            byteBuffer = new byte[encoding.GetMaxByteCount( bufferSize )];
            charLen = bufferSize;
            // If we're appending to a Stream that already has data, don't write
            // the preamble.
            if(stream.CanSeek && stream.Position > 0)
            {
                haveWrittenPreamble = true;
            }
            closable = true;
        }

////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
        private static Stream CreateFile( String path, bool append )
        {
            FileMode mode = append ? FileMode.Append : FileMode.Create;
            FileStream f = new FileStream( path, mode, FileAccess.Write, FileShare.Read, DefaultFileStreamBufferSize ); //, FileOptions.SequentialScan );
            return f;
        }

        public override void Close()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected override void Dispose( bool disposing )
        {
            try
            {
                // We need to flush any buffered data if we are being closed/disposed.
                // Also, we never close the handles for stdout & friends.  So we can safely 
                // write any buffered data to those streams even during finalization, which 
                // is generally the right thing to do.
                if(stream != null)
                {
                    // Note: flush on the underlying stream can throw (ex., low disk space)
                    //if(disposing || (!Closable && stream is __ConsoleStream))
                    if(disposing)
                    {
                        Flush( true, true );
                    }
                }
            }
            finally
            {
                // Dispose of our resources if this StreamWriter is closable. 
                // Note: Console.Out and other such non closable streamwriters should be left alone 
                if(Closable && stream != null)
                {
                    try
                    {
                        // Attempt to close the stream even if there was an IO error from Flushing.
                        // Note that Stream.Close() can potentially throw here (may or may not be
                        // due to the same Flush error). In this case, we still need to ensure 
                        // cleaning up internal resources, hence the finally block.  
                        if(disposing)
                        {
                            stream.Close();
                        }
                    }
                    finally
                    {
                        stream = null;
                        byteBuffer = null;
                        charBuffer = null;
                        encoding = null;
                        encoder = null;
                        charLen = 0;
                        base.Dispose( disposing );
                    }
                }
            }
        }

        public override void Flush()
        {
            Flush( true, true );
        }

        private void Flush( bool flushStream, bool flushEncoder )
        {
            // flushEncoder should be true at the end of the file and if
            // the user explicitly calls Flush (though not if AutoFlush is true).
            // This is required to flush any dangling characters from our UTF-7 
            // and UTF-8 encoders.  
            if(stream == null)
            {
                __Error.WriterClosed();
            }

            // Perf boost for Flush on non-dirty writers.
            if(charPos == 0 && !flushStream && !flushEncoder)
            {
                return;
            }

            if(!haveWrittenPreamble)
            {
                haveWrittenPreamble = true;
                byte[] preamble = encoding.GetPreamble();
                if(preamble.Length > 0)
                {
                    stream.Write( preamble, 0, preamble.Length );
                }
            }

            int count = encoder.GetBytes( charBuffer, 0, charPos, byteBuffer, 0, flushEncoder );
            charPos = 0;
            if(count > 0)
            {
                stream.Write( byteBuffer, 0, count );
            }
            // By definition, calling Flush should flush the stream, but this is
            // only necessary if we passed in true for flushStream.  The Web
            // Services guys have some perf tests where flushing needlessly hurts.
            if(flushStream)
            {
                stream.Flush();
            }
        }

        public virtual bool AutoFlush
        {
            get
            {
                return autoFlush;
            }

            set
            {
                autoFlush = value;
                if(value) Flush( true, false );
            }
        }

        public virtual Stream BaseStream
        {
            get { return stream; }
        }

        internal bool Closable
        {
            get { return closable; }
            //set { closable = value; }
        }

        internal bool HaveWrittenPreamble
        {
            set { haveWrittenPreamble = value; }
        }

        public override Encoding Encoding
        {
            get { return encoding; }
        }

        public override void Write( char value )
        {
            if(charPos == charLen) Flush( false, false );
            charBuffer[charPos] = value;
            charPos++;
            if(autoFlush) Flush( true, false );
        }

        public override void Write( char[] buffer )
        {
            // This may be faster than the one with the index & count since it
            // has to do less argument checking.
            if(buffer == null)
            {
                return;
            }
            int index = 0;
            int count = buffer.Length;
            while(count > 0)
            {
                if(charPos == charLen) Flush( false, false );
                int n = charLen - charPos;
                if(n > count) n = count;
                BCLDebug.Assert( n > 0, "StreamWriter::Write(char[]) isn't making progress!  This is most likely a race in user code." );
                Buffer.InternalBlockCopy( buffer, index * 2, charBuffer, charPos * 2, n * 2 );
                charPos += n;
                index += n;
                count -= n;
            }
            if(autoFlush) Flush( true, false );
        }

        public override void Write( char[] buffer, int index, int count )
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

            while(count > 0)
            {
                if(charPos == charLen) Flush( false, false );
                int n = charLen - charPos;
                if(n > count) n = count;
                BCLDebug.Assert( n > 0, "StreamWriter::Write(char[], int, int) isn't making progress!  This is most likely a race condition in user code." );
                Buffer.InternalBlockCopy( buffer, index * 2, charBuffer, charPos * 2, n * 2 );
                charPos += n;
                index += n;
                count -= n;
            }
            if(autoFlush) Flush( true, false );
        }

        public override void Write( String value )
        {
            if(value != null)
            {
                int count = value.Length;
                int index = 0;
                while(count > 0)
                {
                    if(charPos == charLen) Flush( false, false );
                    int n = charLen - charPos;
                    if(n > count) n = count;
                    BCLDebug.Assert( n > 0, "StreamWriter::Write(String) isn't making progress!  This is most likely a race condition in user code." );
                    value.CopyTo( index, charBuffer, charPos, n );
                    charPos += n;
                    index += n;
                    count -= n;
                }
                if(autoFlush) Flush( true, false );
            }
        }
    }
}
