// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: BitArray
**
**
** Purpose: The BitArray class manages a compact array of bit values.
**
**
=============================================================================*/
namespace System.Collections
{
    using System;
////using System.Security.Permissions;

    // A vector of bits.  Use this to store bits efficiently, without having to do bit
    // shifting yourself.
    [Serializable]
    public sealed class BitArray : ICollection, ICloneable
    {
        private const int cShrinkThreshold = 256;

        private int[]  m_array;
        private int    m_length;
        private int    m_version;
        [NonSerialized]
        private Object m_syncRoot;


        private BitArray()
        {
        }

        /*=========================================================================
        ** Allocates space to hold length bit values. All of the values in the bit
        ** array are set to false.
        **
        ** Exceptions: ArgumentException if length < 0.
        =========================================================================*/
        public BitArray( int length ) : this( length, false )
        {
        }

        /*=========================================================================
        ** Allocates space to hold length bit values. All of the values in the bit
        ** array are set to defaultValue.
        **
        ** Exceptions: ArgumentOutOfRangeException if length < 0.
        =========================================================================*/
        public BitArray( int length, bool defaultValue )
        {
            if(length < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "length", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            m_array  = new int[(length + 31) / 32];
            m_length =          length;

            int fillValue = defaultValue ? unchecked( ((int)0xffffffff) ) : 0;
            for(int i = 0; i < m_array.Length; i++)
            {
                m_array[i] = fillValue;
            }

            m_version = 0;
        }

        /*=========================================================================
        ** Allocates space to hold the bit values in bytes. bytes[0] represents
        ** bits 0 - 7, bytes[1] represents bits 8 - 15, etc. The LSB of each byte
        ** represents the lowest index value; bytes[0] & 1 represents bit 0,
        ** bytes[0] & 2 represents bit 1, bytes[0] & 4 represents bit 2, etc.
        **
        ** Exceptions: ArgumentException if bytes == null.
        =========================================================================*/
        public BitArray( byte[] bytes )
        {
            if(bytes == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "bytes" );
#else
                throw new ArgumentNullException();
#endif
            }

            m_array  = new int[(bytes.Length + 3) / 4];
            m_length =          bytes.Length * 8;

            int i = 0;
            int j = 0;
            while(bytes.Length - j >= 4)
            {
                m_array[i++] =  (bytes[j    ] & 0xff)        |
                               ((bytes[j + 1] & 0xff) <<  8) |
                               ((bytes[j + 2] & 0xff) << 16) |
                               ((bytes[j + 3] & 0xff) << 24);

                j += 4;
            }

            BCLDebug.Assert( bytes.Length - j >= 0, "BitArray byteLength problem"    );
            BCLDebug.Assert( bytes.Length - j < 4 , "BitArray byteLength problem #2" );

            switch(bytes.Length - j)
            {
                case 3:
                    m_array[i] = ((bytes[j + 2] & 0xff) << 16);
                    goto case 2;

                // fall through
                case 2:
                    m_array[i] |= ((bytes[j + 1] & 0xff) << 8);
                    goto case 1;

                // fall through
                case 1:
                    m_array[i] |= (bytes[j] & 0xff);
                    break;
            }

            m_version = 0;
        }

        public BitArray( bool[] values )
        {
            if(values == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "values" );
#else
                throw new ArgumentNullException();
#endif
            }

            m_array  = new int[(values.Length + 31) / 32];
            m_length =          values.Length;

            for(int i = 0; i < values.Length; i++)
            {
                if(values[i])
                {
                    m_array[i / 32] |= (1 << (i % 32));
                }
            }

            m_version = 0;

        }

        /*=========================================================================
        ** Allocates space to hold the bit values in values. values[0] represents
        ** bits 0 - 31, values[1] represents bits 32 - 63, etc. The LSB of each
        ** integer represents the lowest index value; values[0] & 1 represents bit
        ** 0, values[0] & 2 represents bit 1, values[0] & 4 represents bit 2, etc.
        **
        ** Exceptions: ArgumentException if values == null.
        =========================================================================*/
        public BitArray( int[] values )
        {
            if(values == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "values" );
#else
                throw new ArgumentNullException();
#endif
            }

            m_array  = new int[values.Length];
            m_length =         values.Length * 32;

            Array.Copy( values, m_array, values.Length );

            m_version = 0;
        }

        /*=========================================================================
        ** Allocates a new BitArray with the same length and bit values as bits.
        **
        ** Exceptions: ArgumentException if bits == null.
        =========================================================================*/
        public BitArray( BitArray bits )
        {
            if(bits == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "bits" );
#else
                throw new ArgumentNullException();
#endif
            }

            m_array  = new int[(bits.m_length + 31) / 32];
            m_length =          bits.m_length;

            Array.Copy( bits.m_array, m_array, (bits.m_length + 31) / 32 );

            m_version = bits.m_version;
        }

        public bool this[int index]
        {
            get
            {
                return Get( index );
            }

            set
            {
                Set( index, value );
            }
        }

        /*=========================================================================
        ** Returns the bit value at position index.
        **
        ** Exceptions: ArgumentOutOfRangeException if index < 0 or
        **             index >= GetLength().
        =========================================================================*/
        public bool Get( int index )
        {
            if(index < 0 || index >= m_length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            return (m_array[index / 32] & (1 << (index % 32))) != 0;
        }

        /*=========================================================================
        ** Sets the bit value at position index to value.
        **
        ** Exceptions: ArgumentOutOfRangeException if index < 0 or
        **             index >= GetLength().
        =========================================================================*/
        public void Set( int index, bool value )
        {
            if(index < 0 || index >= m_length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(value)
            {
                m_array[index / 32] |= (1 << (index % 32));
            }
            else
            {
                m_array[index / 32] &= ~(1 << (index % 32));
            }

            m_version++;
        }

        /*=========================================================================
        ** Sets all the bit values to value.
        =========================================================================*/
        public void SetAll( bool value )
        {
            int fillValue = value ? unchecked( ((int)0xffffffff) ) : 0;
            int ints      = (m_length + 31) / 32;

            for(int i = 0; i < ints; i++)
            {
                m_array[i] = fillValue;
            }

            m_version++;
        }

        /*=========================================================================
        ** Returns a reference to the current instance ANDed with value.
        **
        ** Exceptions: ArgumentException if value == null or
        **             value.Length != this.Length.
        =========================================================================*/
        public BitArray And( BitArray value )
        {
            if(value == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "value" );
#else
                throw new ArgumentNullException();
#endif
            }

            if(m_length != value.m_length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_ArrayLengthsDiffer" ) );
#else
                throw new ArgumentException();
#endif
            }

            int ints = (m_length + 31) / 32;
            for(int i = 0; i < ints; i++)
            {
                m_array[i] &= value.m_array[i];
            }

            m_version++;
            return this;
        }

        /*=========================================================================
        ** Returns a reference to the current instance ORed with value.
        **
        ** Exceptions: ArgumentException if value == null or
        **             value.Length != this.Length.
        =========================================================================*/
        public BitArray Or( BitArray value )
        {
            if(value == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "value" );
#else
                throw new ArgumentNullException();
#endif
            }

            if(m_length != value.m_length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_ArrayLengthsDiffer" ) );
#else
                throw new ArgumentException();
#endif
            }

            int ints = (m_length + 31) / 32;
            for(int i = 0; i < ints; i++)
            {
                m_array[i] |= value.m_array[i];
            }

            m_version++;
            return this;
        }

        /*=========================================================================
        ** Returns a reference to the current instance XORed with value.
        **
        ** Exceptions: ArgumentException if value == null or
        **             value.Length != this.Length.
        =========================================================================*/
        public BitArray Xor( BitArray value )
        {
            if(value == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "value" );
#else
                throw new ArgumentNullException();
#endif
            }

            if(m_length != value.m_length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_ArrayLengthsDiffer" ) );
#else
                throw new ArgumentException();
#endif
            }

            int ints = (m_length + 31) / 32;
            for(int i = 0; i < ints; i++)
            {
                m_array[i] ^= value.m_array[i];
            }

            m_version++;
            return this;
        }

        /*=========================================================================
        ** Inverts all the bit values. On/true bit values are converted to
        ** off/false. Off/false bit values are turned on/true. The current instance
        ** is updated and returned.
        =========================================================================*/
        public BitArray Not()
        {
            int ints = (m_length + 31) / 32;
            for(int i = 0; i < ints; i++)
            {
                m_array[i] = ~m_array[i];
            }

            m_version++;
            return this;
        }


        public int Length
        {
            get
            {
                return m_length;
            }

            set
            {
                if(value < 0)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentOutOfRangeException( "value", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                    throw new ArgumentOutOfRangeException();
#endif
                }

                int newints = (value + 31) / 32;
                if(newints > m_array.Length || newints + cShrinkThreshold < m_array.Length)
                {
                    // grow or shrink (if wasting more than _ShrinkThreshold ints)
                    int[] newarray = new int[newints];

                    Array.Copy( m_array, newarray, newints > m_array.Length ? m_array.Length : newints );

                    m_array = newarray;
                }

                if(value > m_length)
                {
                    // clear high bit values in the last int
                    int last = ((m_length + 31) / 32) - 1;
                    int bits =   m_length % 32;
                    if(bits > 0)
                    {
                        m_array[last] &= (1 << bits) - 1;
                    }

                    // clear remaining int values
                    Array.Clear( m_array, last + 1, newints - last - 1 );
                }

                m_length = value;
                m_version++;
            }
        }

        // ICollection implementation
        public void CopyTo( Array array, int index )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
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

            if(array.Rank != 1)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_RankMultiDimNotSupported" ) );
#else
                throw new ArgumentException();
#endif
            }


            if(array is int[])
            {
                Array.Copy( m_array, 0, array, index, (m_length + 31) / 32 );
            }
            else if(array is byte[])
            {
                if((array.Length - index) < (m_length + 7) / 8)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                    throw new ArgumentException();
#endif
                }

                byte[] b = (byte[])array;
                for(int i = 0; i < (m_length + 7) / 8; i++)
                {
                    b[index + i] = (byte)((m_array[i / 4] >> ((i % 4) * 8)) & 0x000000FF); // Shift to bring the required byte to LSB, then mask
                }
            }
            else if(array is bool[])
            {
                if(array.Length - index < m_length)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                    throw new ArgumentException();
#endif
                }

                bool[] b = (bool[])array;
                for(int i = 0; i < m_length; i++)
                {
                    b[index + i] = ((m_array[i / 32] >> (i % 32)) & 0x00000001) != 0;
                }
            }
            else
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_BitArrayTypeUnsupported" ) );
#else
                throw new ArgumentException();
#endif
            }
        }

        public int Count
        {
            get
            {
                return m_length;
            }
        }

        public Object Clone()
        {
            BitArray bitArray = new BitArray( m_array );

            bitArray.m_version = m_version;
            bitArray.m_length  = m_length;

            return bitArray;
        }

        public Object SyncRoot
        {
            get
            {
                if(m_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange( ref m_syncRoot, new Object(), null );
                }

                return m_syncRoot;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }


        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new BitArrayEnumeratorSimple( this );
        }

        [Serializable]
        private class BitArrayEnumeratorSimple : IEnumerator, ICloneable
        {
            private BitArray m_bitarray;
            private int      m_index;
            private int      m_version;
            private bool     m_currentElement;

            internal BitArrayEnumeratorSimple( BitArray bitarray )
            {
                m_bitarray = bitarray;
                m_index    = -1;
                m_version  = bitarray.m_version;
            }

            public Object Clone()
            {
                return MemberwiseClone();
            }

            public virtual bool MoveNext()
            {
                if(m_version != m_bitarray.m_version)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumFailedVersion" ) );
#else
                    throw new InvalidOperationException();
#endif
                }

                if(m_index < (m_bitarray.Count - 1))
                {
                    m_index++;
                    m_currentElement = m_bitarray.Get( m_index );
                    return true;
                }
                else
                {
                    m_index = m_bitarray.Count;
                }

                return false;
            }

            public virtual Object Current
            {
                get
                {
                    if(m_index == -1)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumNotStarted" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }

                    if(m_index >= m_bitarray.Count)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumEnded" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }

                    return m_currentElement;
                }
            }

            public void Reset()
            {
                if(m_version != m_bitarray.m_version)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumFailedVersion" ) );
#else
                    throw new InvalidOperationException();
#endif
                }

                m_index = -1;
            }
        }
    }
}
