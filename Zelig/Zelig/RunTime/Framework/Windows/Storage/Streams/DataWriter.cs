using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Windows.Foundation;
using Windows.Internal;

namespace Windows.Storage.Streams
{
    /// <summary>Writes data to an output stream.</summary>
    public sealed class DataWriter : IDisposable
    {
        private IOutputStream m_outputStream;
        private ByteBuffer m_buffer;

        private const int defaultBufferSize = 512;

        /// <summary>Gets or sets the Unicode character encoding for the output stream.</summary>
        /// <returns>One of the enumeration values.</returns>
        public UnicodeEncoding UnicodeEncoding
        {
            get;
            set;
        }

        public ByteOrder ByteOrder
        {
            get;
            set;
        }

        public uint UnstoredBufferLength
        {
            get
            {
                return m_buffer.Capacity - m_buffer.Length;
            }
        }

        /// <summary>Creates and initializes a new instance of the data writer to an output stream.</summary>
        /// <param name="outputStream">The new output stream instance.</param>
        public DataWriter(IOutputStream outputStream)
        {
            m_outputStream = outputStream;
            m_buffer = new ByteBuffer(defaultBufferSize);
        }

        public DataWriter()
        {
            throw new NotImplementedException();
        }

        public void WriteByte(byte value)
        {
            FlushIfFull(1);

            lock (m_buffer)
            {
                byte[] data = m_buffer.Data;
                data[m_buffer.Length] = value;
                m_buffer.Length++;
            }
        }

        private void WriteBytes(byte[] value, int start, int count)
        {
            if(count > (m_buffer.Capacity - m_buffer.Length))
            {
                count = (int)(m_buffer.Capacity - m_buffer.Length);
            }
            Buffer.BlockCopy(value, start, m_buffer.Data, (int)m_buffer.Length, count);
            
            m_buffer.Length += (uint)count;
        }

        /// <summary>Writes an array of byte values to the output stream.</summary>
        /// <param name="value">The array of values.</param>
        public void WriteBytes(byte[] value)
        {
            FlushIfFull(value.Length);

            lock (m_buffer)
            {
                WriteBytes(value, 0, value.Length);
            }
        }
        
        public void WriteBuffer(IBuffer buffer)
        {
            WriteBuffer(buffer, 0, buffer.Length);
        }

        /// <summary>Writes the specified bytes from a buffer to the output stream.</summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="start">The starting byte.</param>
        /// <param name="count">The number of bytes to write.</param>
        public void WriteBuffer(IBuffer buffer, uint start, uint count)
        {
            byte[] data = ((ByteBuffer)buffer).Data;

            FlushIfFull((int)count);

            lock (m_buffer)
            {
                WriteBytes(data, (int)start, data.Length);
            }
        }

        /// <summary>Writes a Boolean value to the output stream.</summary>
        /// <param name="value">The value.</param>
        public void WriteBoolean(bool value)
        {
            if(value)
            {
                WriteByte(0x1);
            }
            else
            {
                WriteByte(0x0);
            }
        }

        public void WriteGuid(Guid value)
        {
            WriteBytes(value.ToByteArray());
        }

        /// <summary>Writes a 16-bit integer value to the output stream.</summary>
        /// <param name="value">The value.</param>
        public void WriteInt16(short value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteInt32(int value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        /// <summary>Writes a 64-bit integer value to the output stream.</summary>
        /// <param name="value">The value.</param>
        public void WriteInt64(long value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        /// <summary>Writes a 16-bit unsigned integer value to the output stream.</summary>
        /// <param name="value">The value.</param>
        public void WriteUInt16(ushort value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteUInt32(uint value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        /// <summary>Writes a 64-bit unsigned integer value to the output stream.</summary>
        /// <param name="value">The value.</param>
        public void WriteUInt64(ulong value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteSingle(float value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        /// <summary>Writes a floating-point value to the output stream.</summary>
        /// <param name="value">The value.</param>
        public void WriteDouble(double value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        public void WriteDateTime(DateTime value)
        {
            WriteBytes(BitConverter.GetBytes(value.Ticks));
        }

        public void WriteTimeSpan(TimeSpan value)
        {
            WriteBytes(BitConverter.GetBytes(value.Ticks));
        }

        /// <summary>Writes a string value to the output stream.</summary>
        /// <returns>The length of the string, in bytes.</returns>
        /// <param name="value">The value.</param>
        public uint WriteString(string value)
        {
            char[] arrValue = value.ToCharArray();
            int valLength = arrValue.Length;

            FlushIfFull(valLength);

            lock(m_buffer)
            {
                Buffer.BlockCopy(arrValue, 0, m_buffer.Data, (int)m_buffer.Length, valLength);
                m_buffer.Length += (uint)valLength;
            }

            return (uint)valLength;
        }

        /// <summary>Gets the size of a string.</summary>
        /// <returns>The size of the string, in bytes.</returns>
        /// <param name="value">The string.</param>
        public uint MeasureString(string value)
        {
            return (uint)value.Length;
        }

        public DataWriterStoreOperation StoreAsync()
        {
            // NOP - so copy/pasted code works
            return null;
        }

        public IAsyncOperation<bool> FlushAsync()
        {
            if (m_outputStream == null)
            {
                throw new InvalidOperationException();
            }

            IAsyncOperationWithProgress<uint, uint> operation;
            lock (m_buffer)
            {
                operation = m_outputStream.WriteAsync(m_buffer);
                m_buffer.Length = 0;
            }

            if (operation.ErrorCode != null)
            {
                return new SynchronousOperation<bool>(operation.ErrorCode);
            }

            return FlushAsync();
        }

        public IBuffer DetachBuffer()
        {
            return Interlocked.Exchange(ref m_buffer, null);
        }

        /// <summary>Detaches the stream that is associated with the data writer.</summary>
        /// <returns>The detached stream.</returns>
        public IOutputStream DetachStream()
        {
            return Interlocked.Exchange(ref m_outputStream, null);
        }


        public void Dispose()
        {
            m_outputStream?.Dispose();
        }

        private void FlushIfFull(int count)
        {
            if((m_buffer.Length + count) >= m_buffer.Capacity)
            {
                FlushAsync().GetResults();
            }
        }
    }
}
