using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Windows.Foundation;
using Windows.Internal;

namespace Windows.Storage.Streams
{
    public sealed class DataReader : IDisposable
    {
        private IInputStream m_inputStream;
        private ByteBuffer m_buffer;
        private ByteBuffer m_tempDataBuffer;
        private int m_readOffset;

        private const int defaultBufferSize = 512;

        /// <summary>Gets or sets the Unicode character encoding for the input stream.</summary>
        /// <returns>One of the enumeration values.</returns>
        public UnicodeEncoding UnicodeEncoding
        {
            get;
            set;
        }

        /// <summary>Gets or sets the read options for the input stream.</summary>
        /// <returns>One of the enumeration values.</returns>
        public InputStreamOptions InputStreamOptions
        {
            get;
            set;
        }

        public ByteOrder ByteOrder
        {
            get;
            set;
        }

        public uint UnconsumedBufferLength
        {
            get
            {
                return (uint)(m_buffer.Capacity - m_buffer.Length);
            }
        }

        /// <summary>Creates and initializes a new instance of the data reader.</summary>
        /// <param name="inputStream">The input stream.</param>
        public DataReader(IInputStream inputStream)
        {
            m_inputStream = inputStream;
            m_buffer = new ByteBuffer(defaultBufferSize);
            m_tempDataBuffer = new ByteBuffer(defaultBufferSize);

            // 16 to accomodate reading Guids
            m_tempDataBuffer.Data = new byte[16];
        }

        private DataReader(IBuffer buffer)
        {
            m_buffer = (ByteBuffer)buffer;

            // 16 to accomodate reading Guids
            m_tempDataBuffer.Data = new byte[16];
        }

        /// <summary>Reads a byte value from the input stream.</summary>
        /// <returns>The value.</returns>
        public byte ReadByte()
        {
            ThrowIfBytesUnavailable(1);

            byte value;
            lock (m_tempDataBuffer)
            {
                value = m_buffer.Data[m_readOffset];
                m_readOffset += 1;

                ResetWriteReadIndexIfNeeded();
            }
            return value;
        }

        public void ReadBytes(byte[] value)
        {
            lock(m_tempDataBuffer)
            {
                ReadBytes(value, value.Length);
            }
        }

        private void ReadBytes(byte[] value, int count)
        {
            ThrowIfBytesUnavailable(count);

            Buffer.BlockCopy(m_buffer.Data, m_readOffset, value, 0, count);
            m_readOffset += count;

            ResetWriteReadIndexIfNeeded();
        }

        public IBuffer ReadBuffer(uint length)
        {
            ByteBuffer buffer = new ByteBuffer(length);

            lock(m_tempDataBuffer)
            {
                ReadBytes(buffer.Data, (int)length);
            }

            return buffer;
        }

        /// <summary>Reads a Boolean value from the input stream.</summary>
        /// <returns>The value.</returns>
        public bool ReadBoolean()
        {
            byte value;
            value = ReadByte();

            return (value != 0x0);
        }

        /// <summary>Reads a GUID value from the input stream.</summary>
        /// <returns>The value.</returns>
        public Guid ReadGuid()
        {
            lock(m_tempDataBuffer)
            {
                ReadBytes(m_tempDataBuffer.Data, 16);
                return new Guid(m_tempDataBuffer.Data);
            }
        }

        public short ReadInt16()
        {
            lock (m_tempDataBuffer)
            {
                ReadBytes(m_tempDataBuffer.Data, 2);
                return BitConverter.ToInt16(m_tempDataBuffer.Data, 0);
            }
        }

        /// <summary>Reads a 32-bit integer value from the input stream.</summary>
        /// <returns>The value.</returns>
        public int ReadInt32()
        {
            lock (m_tempDataBuffer)
            {
                ReadBytes(m_tempDataBuffer.Data, 4);
                return BitConverter.ToInt32(m_tempDataBuffer.Data, 0);
            }
        }

        public long ReadInt64()
        {
            lock (m_tempDataBuffer)
            {
                ReadBytes(m_tempDataBuffer.Data, 8);
                return BitConverter.ToInt64(m_tempDataBuffer.Data, 0);
            }
        }

        public ushort ReadUInt16()
        {
            lock (m_tempDataBuffer)
            {
                ReadBytes(m_tempDataBuffer.Data, 2);
                return BitConverter.ToUInt16(m_tempDataBuffer.Data, 0);
            }
        }

        /// <summary>Reads a 32-bit unsigned integer from the input stream.</summary>
        /// <returns>The value.</returns>
        public uint ReadUInt32()
        {
            lock (m_tempDataBuffer)
            {
                ReadBytes(m_tempDataBuffer.Data, 4);
                return BitConverter.ToUInt32(m_tempDataBuffer.Data, 0);
            }
        }

        public ulong ReadUInt64()
        {
            lock (m_tempDataBuffer)
            {
                ReadBytes(m_tempDataBuffer.Data, 8);
                return BitConverter.ToUInt64(m_tempDataBuffer.Data, 0);
            }
        }

        /// <summary>Reads a floating-point value from the input stream.</summary>
        /// <returns>The value.</returns>
        public float ReadSingle()
        {
            lock (m_tempDataBuffer)
            {
                ReadBytes(m_tempDataBuffer.Data, 4);
                return BitConverter.ToSingle(m_tempDataBuffer.Data, 0);
            }
        }

        public double ReadDouble()
        {
            lock (m_tempDataBuffer)
            {
                ReadBytes(m_tempDataBuffer.Data, 8);
                return BitConverter.ToDouble(m_tempDataBuffer.Data, 0);
            }
        }

        public string ReadString(uint codeUnitCount)
        {
            ThrowIfBytesUnavailable((int)codeUnitCount);
            
            byte[] data = new byte[codeUnitCount];
            ReadBytes(data);

            return Encoding.Default.GetString(data);
        }

        /// <summary>Reads a date and time value from the input stream.</summary>
        /// <returns>The value.</returns>
        public DateTime ReadDateTime()
        {
            long value = ReadInt64();
            return new DateTime(value);
        }

        /// <summary>Reads a time-interval value from the input stream.</summary>
        /// <returns>The value.</returns>
        public TimeSpan ReadTimeSpan()
        {
            long value = ReadInt64();
            return new TimeSpan(value);
        }

        public DataReaderLoadOperation LoadAsync(uint count)
        {
            if (m_inputStream == null)
            {
                throw new InvalidOperationException();
            }

            ByteBuffer readBuffer = (ByteBuffer)m_inputStream.ReadAsync(m_tempDataBuffer, count, InputStreamOptions).GetResults();

            int bytesRead = (int)readBuffer.Length;
            lock (m_tempDataBuffer)
            {
                ResetWriteReadIndexIfNeeded();

                Buffer.BlockCopy(readBuffer.Data, 0, m_buffer.Data, (int)m_buffer.Length, bytesRead);
                m_buffer.Length += (uint)bytesRead;
            }

            return new DataReaderLoadOperation((uint)bytesRead);
        }

        /// <summary>Detaches the buffer that is associated with the data reader.</summary>
        /// <returns>The detached buffer.</returns>
        public IBuffer DetachBuffer()
        {
            return Interlocked.Exchange(ref m_buffer, null);
        }

        public IInputStream DetachStream()
        {
            return Interlocked.Exchange(ref m_inputStream, null);
        }

        public void Dispose()
        {
            m_inputStream?.Dispose();
        }

        /// <summary>Creates a new instance of the data reader with data from the specified buffer.</summary>
        /// <returns>The data reader.</returns>
        /// <param name="buffer">The buffer.</param>
        public static DataReader FromBuffer(IBuffer buffer)
        {
            return new DataReader(buffer);
        }

        /// <summary>
        /// Throws if user tries to read more bytes than are available
        /// </summary>
        /// <param name="count">Number of bytes that need to be read</param>
        private void ThrowIfBytesUnavailable(int count)
        {
            if (count > (m_buffer.Length - m_readOffset))
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Resets the write/read indices if all bytes have been read
        /// </summary>
        private void ResetWriteReadIndexIfNeeded()
        {
            if (m_buffer.Length == m_readOffset)
            {
                m_buffer.Length = 0;
                m_readOffset = 0;
            }
        }
    }
}
