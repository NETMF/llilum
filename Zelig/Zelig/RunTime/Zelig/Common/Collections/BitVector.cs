//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class BitVector
    {
        const int  c_BitsPerWord = 32;
        const int  c_MinWords    = 4;
        const uint c_WordMask    = 0xFFFFFFFFU;

        //--//

        public static readonly BitVector[] SharedEmptyArray = new BitVector[0];

        //--//

        //
        // State
        //

        private  uint[] m_bitArray;
        internal int    m_cardinalityCache;
        internal int    m_version;

        //
        // Constructor Methods
        //

        public BitVector() : this( c_MinWords * c_BitsPerWord )
        {
        }

        public BitVector( int size )
        {
            int arraySize = (size + c_BitsPerWord - 1) / c_BitsPerWord;
            if(arraySize < c_MinWords)
            {
                arraySize = c_MinWords;
            }

            m_bitArray         = new uint[arraySize];
            m_cardinalityCache = 0;
            m_version          = 0;
        }

        public BitVector( uint[] state )
        {
            m_bitArray         = ArrayUtility.CopyNotNullArray( state );
            m_cardinalityCache = -1;
            m_version          = 0;
        }

        //--//

        //
        // Helper Methods
        //

        public static BitVector[] AllocateBitVectors( int num  ,
                                                      int size )
        {
            BitVector[] res = new BitVector[num];

            for(int i = 0; i < num; i++)
            {
                res[i] = new BitVector( size );
            }

            return res;
        }

        public static BitVector[] Pivot( BitVector[] arrayXbyY ,
                                         int         sizeX     ,
                                         int         sizeY     )
        {
            CHECKS.ASSERT( arrayXbyY.Length == sizeX, "Incorrect input array" );

            BitVector[] arrayYbyX = AllocateBitVectors( sizeY, sizeX );

            for(int posX = 0; posX < sizeX; posX++)
            {
                BitVector vecX = arrayXbyY[posX];

                CHECKS.ASSERT( vecX.Size >= sizeY, "Incorrect input array" );

                if(vecX.m_cardinalityCache != 0)
                {
                    for(int posY = 0; posY < sizeY; posY += c_BitsPerWord)
                    {
                        uint word = vecX.m_bitArray[posY / c_BitsPerWord];

                        while(word != 0)
                        {
                            int bitPos = GetPositionOfFirstBitSet( word );

                            word = word & ~(1U << bitPos);

                            BitVector vecY = arrayYbyX[posY + bitPos];

                            vecY.m_bitArray[posX / c_BitsPerWord] |= 1U << (posX % c_BitsPerWord);
                            vecY.m_cardinalityCache++;
                        }
                    }
                }
            }

            return arrayYbyX;
        }

        //--//

        public override bool Equals( object other )
        {
            if(other is BitVector)
            {
                return Equals( (BitVector)other );
            }

            return false;
        }

        public override int GetHashCode()
        {
            uint hash = 1234;

            for(int i = m_bitArray.Length; i >= 0; i--)
            {
                hash = (hash << 4) ^ (hash >> 28) ^ m_bitArray[i];
            }

            return (int)((hash >> 32) ^ hash);
        }

        //--//

        public static bool operator ==( BitVector left  ,
                                        BitVector right )
        {
            if((object)left == null)
            {
                return (object)right == null;
            }

            if((object)right == null)
            {
                return false;
            }

            return left.Equals( right );
        }

        public static bool operator !=( BitVector left  ,
                                        BitVector right )
        {
            return !(left == right);
        }

        public bool Equals( BitVector other )
        {
            //
            // Fast negative check.
            //
            if(m_cardinalityCache != other.m_cardinalityCache)
            {
                if(      m_cardinalityCache >= 0 &&
                   other.m_cardinalityCache >= 0  )
                {
                    return false;
                }
            }

            uint[] myArray    = this      .m_bitArray;
            uint[] otherArray = other     .m_bitArray;
            int    otherSize  = otherArray.Length;
            int    mySize     = myArray.Length;
            int    minimum    = Math.Min( mySize, otherSize );

            for(int i = minimum - 1; i >= 0; i--)
            {
                if(myArray[i] != otherArray[i])
                {
                    return false;
                }
            }

            if(mySize < otherSize)
            {
                for(int i = mySize; i < otherSize; i++)
                {
                    if(otherArray[i] != 0)
                    {
                        return false;
                    }
                }
            }
            else
            {
                for(int i = otherSize; i < mySize; i++)
                {
                    if(myArray[i] != 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public BitVector Clone()
        {
            BitVector res = (BitVector)MemberwiseClone();

            res.m_bitArray = ArrayUtility.CopyNotNullArray( m_bitArray );

            return res;
        }

        public uint[] ToDirectArray()
        {
            m_cardinalityCache = -1;
            m_version++;

            return m_bitArray;
        }

        //--//

        public bool Get( int index )
        {
            int arrayIndex = index / c_BitsPerWord;

            if(arrayIndex < m_bitArray.Length)
            {
                uint mask = 1U << (index % c_BitsPerWord);

                return (m_bitArray[arrayIndex] & mask) != 0;
            }

            return false;
        }

        public bool Set( int index )
        {
            BumpVersion();

            EnsureCapacityInBits( index + 1 );

            int arrayIndex = index / c_BitsPerWord;

            uint old    = m_bitArray[arrayIndex];
            uint mask   = 1U << (index % c_BitsPerWord);
            uint newVal = old | mask;

            bool fChanged = (newVal != old);
            if(fChanged)
            {
                m_bitArray[arrayIndex] = newVal;

                if(m_cardinalityCache != -1)
                {
                    m_cardinalityCache++;
                }
            }

            return fChanged;
        }

        public bool Clear( int index )
        {
            BumpVersion();

            EnsureCapacityInBits( index + 1 );

            int arrayIndex = index / c_BitsPerWord;

            uint old    = m_bitArray[arrayIndex];
            uint mask   = 1U << (index % c_BitsPerWord);
            uint newVal = old & ~mask;

            bool fChanged = (newVal != old);
            if(fChanged)
            {
                m_bitArray[arrayIndex] = newVal;

                if(m_cardinalityCache != -1)
                {
                    m_cardinalityCache--;
                }
            }

            return fChanged;
        }

        //--//

        public void SetRange( int start ,
                              int size  )
        {
            if(size == 0) return;

            EnsureCapacityInBits( start + size );

            if(m_cardinalityCache == 0)
            {
                m_cardinalityCache = size;
            }
            else
            {
                m_cardinalityCache = -1;
            }

            int i = start / c_BitsPerWord;

            start = start % c_BitsPerWord;
            if(start != 0)
            {
                uint mask = c_WordMask;
                
                if(size + start < c_BitsPerWord)
                {
                    mask >>= (c_BitsPerWord - size);
                    size   = 0;
                }
                else
                {
                    size -= (c_BitsPerWord - start);
                }

                m_bitArray[i++] |= mask << start;
            }

            while(size >= c_BitsPerWord)
            {
                m_bitArray[i++] = c_WordMask;

                size -= c_BitsPerWord;
            }

            if(size > 0)
            {
                m_bitArray[i] |= c_WordMask >> (c_BitsPerWord - size);
            }
        }

        public void ClearRange( int start ,
                                int size  )
        {
            if(size == 0) return;

            EnsureCapacityInBits( start + size );

            if(m_cardinalityCache != 0)
            {
                m_cardinalityCache = -1;
            }

            int i = start / c_BitsPerWord;

            start = start % c_BitsPerWord;
            if(start != 0)
            {
                uint mask = c_WordMask;
                
                if(size + start < c_BitsPerWord)
                {
                    mask >>= (c_BitsPerWord - size);
                    size   = 0;
                }
                else
                {
                    size -= (c_BitsPerWord - start);
                }

                m_bitArray[i++] &= ~(mask << start);
            }

            while(size >= c_BitsPerWord)
            {
                m_bitArray[i++] = 0;

                size -= c_BitsPerWord;
            }

            if(size > 0)
            {
                m_bitArray[i] &= ~(c_WordMask >> (c_BitsPerWord - size));
            }
        }

        public bool GetRange( out int low  ,
                              out int high )
        {
            uint[] array = m_bitArray;
            int    size  = array.Length;
            int    pos   = 0;

            while(pos < size)
            {
                uint word = array[pos];

                if(word != 0)
                {
                    low = pos * c_BitsPerWord + GetPositionOfFirstBitSet( word );

                    pos = size;
                    while(--pos >= 0)
                    {
                        word = array[pos];

                        if(word != 0)
                        {
                            high = pos * c_BitsPerWord + GetPositionOfLastBitSet( word );
                            return true;
                        }
                    }
                }

                pos++;
            }

            low  = int.MaxValue;
            high = int.MinValue;

            return false;
        }

        public void ClearAll()
        {
            if(m_cardinalityCache == 0)
            {
                return; // Nothing to do.
            }

            for(int i = 0; i < m_bitArray.Length; i++)
            {
                m_bitArray[i] = 0;
            }

            m_cardinalityCache = 0;
        }

        //--//

        public bool this[int i]
        {
            get
            {
                return Get( i );
            }

            set
            {
                if(value)
                {
                    Set( i );
                }
                else
                {
                    Clear( i );
                }
            }
        }

        public int Size
        {
            get
            {
                return m_bitArray.Length * c_BitsPerWord;
            }
        }

        public int Cardinality
        {
            get
            {
                if(m_cardinalityCache == -1)
                {
                    uint card = 0;

                    for(int i = m_bitArray.Length - 1; i >= 0; i--)
                    {
                        uint value = m_bitArray[i];
                        if(value != 0)
                        {
                            card += CountBits( value );
                        }
                    }

                    m_cardinalityCache = (int)card;
                }

                return m_cardinalityCache;
            }
        }

        public bool IsEmpty
        {
            get
            {
                if(m_cardinalityCache >= 0)
                {
                    return (m_cardinalityCache == 0);
                }

                for(int i = m_bitArray.Length - 1; i >= 0; i--)
                {
                    if(m_bitArray[i] != 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        //--//

        public void Assign( BitVector other )
        {
            BumpVersion();

            uint[] otherArray = other.m_bitArray;
            int    otherSize  = otherArray.Length;
            int    mySize     = m_bitArray.Length;
            int    size       = Math.Max( mySize, otherSize );

            EnsureCapacityInWords( size );

            uint[] myArray = m_bitArray;

            m_cardinalityCache = other.m_cardinalityCache;

            //--//
            
            for(int i = size; --i >= otherSize; ) // Clear excess part.
            {
                myArray[i] = 0;
            }

            for(int i = otherSize; --i >= 0; ) // Copy common part.
            {
                myArray[i] = otherArray[i];
            }
        }

        public void AndInPlace( BitVector other )
        {
            BumpVersion();

            uint[] otherArray = other.m_bitArray;
            int    otherSize  = otherArray.Length;
            int    mySize     = m_bitArray.Length;
            int    size       = Math.Max( mySize, otherSize );

            EnsureCapacityInWords( size );

            uint[] myArray = m_bitArray;

            m_cardinalityCache = -1;

            //--//

            for(int i = size; --i >= otherSize; ) // Clear excess part.
            {
                myArray[i] = 0;
            }

            for(int i = otherSize; --i >= 0; ) // And common part.
            {
                myArray[i] &= otherArray[i];
            }
        }

        public void OrInPlace( BitVector other )
        {
            BumpVersion();

            uint[] otherArray = other.m_bitArray;
            int    otherSize  = otherArray.Length;
            int    mySize     = m_bitArray.Length;
            int    size       = Math.Max( mySize, otherSize );

            EnsureCapacityInWords( size );

            uint[] myArray = m_bitArray;

            m_cardinalityCache = -1;

            //--//
            
            for(int i = otherSize; --i >= 0; ) // Or common part (leave excess part as is).
            {
                myArray[i] |= otherArray[i];
            }
        }

        public void XorInPlace( BitVector other )
        {
            BumpVersion();

            uint[] otherArray = other.m_bitArray;
            int    otherSize  = otherArray.Length;
            int    mySize     = m_bitArray.Length;
            int    size       = Math.Max( mySize, otherSize );

            EnsureCapacityInWords( size );

            uint[] myArray = m_bitArray;

            m_cardinalityCache = -1;

            //--//
            
            for(int i = otherSize; --i >= 0; ) // Xor common part (leave excess part as is).
            {
                myArray[i] ^= otherArray[i];
            }
        }

        public void DifferenceInPlace( BitVector other )
        {
            BumpVersion();

            uint[] otherArray = other.m_bitArray;
            int    otherSize  = otherArray.Length;
            int    mySize     = m_bitArray.Length;
            int    size       = Math.Max( mySize, otherSize );

            EnsureCapacityInWords( size );

            uint[] myArray = m_bitArray;

            m_cardinalityCache = -1;

            //--//

            for(int i = otherSize; --i >= 0; ) // Diff common part (leave excess part as is).
            {
                myArray[i] &= ~otherArray[i];
            }
        }

        public bool IsIntersectionEmpty( BitVector other )
        {
            uint[] myArray    = this      .m_bitArray;
            uint[] otherArray = other     .m_bitArray;
            int    mySize     = myArray   .Length;
            int    otherSize  = otherArray.Length;
            int    minSize    = Math.Min( mySize, otherSize );

            for(int i = minSize; --i >= 0; ) // Check common part.
            {
                if((myArray[i] & otherArray[i]) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsFullyContainedIn( BitVector other )
        {
            uint[] myArray    = this      .m_bitArray;
            uint[] otherArray = other     .m_bitArray;
            int    mySize     = myArray   .Length;
            int    otherSize  = otherArray.Length;
            int    maxSize    = Math.Max( mySize, otherSize );

            for(int i = maxSize; --i >= 0; ) // Check the maximal overlap.
            {
                uint myValue    = (i < mySize   ) ? myArray   [i] : 0;
                uint otherValue = (i < otherSize) ? otherArray[i] : 0;

                if((myValue & otherValue) != myValue)
                {
                    return false;
                }
            }

            return true;
        }

        //--//

        public void And( BitVector left  ,
                         BitVector right )
        {
            BumpVersion();

            uint[] leftArray  = left .m_bitArray;
            uint[] rightArray = right.m_bitArray;
            int    leftSize   = leftArray .Length;
            int    rightSize  = rightArray.Length;
            int    maxSize    = Math.Max( leftSize, rightSize );
            int    minSize    = Math.Min( leftSize, rightSize );

            EnsureCapacityInWords( maxSize );

            uint[] myArray = m_bitArray;
            int    mySize  = myArray.Length;

            m_cardinalityCache = -1;

            //--//

            for(int i = mySize; --i >= minSize; ) // Clear part not in common.
            {
                myArray[i] = 0;
            }

            for(int i = minSize; --i >= 0; ) // And part in common.
            {
                myArray[i] = leftArray[i] & rightArray[i];
            }
        }
    
        public void Or( BitVector left  ,
                        BitVector right )
        {
            BumpVersion();

            uint[] leftArray  = left .m_bitArray;
            uint[] rightArray = right.m_bitArray;
            int    leftSize   = leftArray .Length;
            int    rightSize  = rightArray.Length;
            int    maxSize    = Math.Max( leftSize, rightSize );
            int    minSize    = Math.Min( leftSize, rightSize );

            EnsureCapacityInWords( maxSize );

            uint[] myArray = m_bitArray;
            int    mySize  = myArray.Length;

            m_cardinalityCache = -1;

            //--//

            for(int i = mySize; --i >= maxSize; ) // Clear part not in common.
            {
                myArray[i] = 0;
            }

            if(leftSize < maxSize) // Copy non-overlapping part.
            {
                for(int i = maxSize; --i >= minSize; )
                {
                    myArray[i] = rightArray[i];
                }
            }
            else
            {
                for(int i = maxSize; --i >= minSize; )
                {
                    myArray[i] = leftArray[i];
                }
            }

            for(int i = minSize; --i >= 0; ) // Or part in common.
            {
                myArray[i] = leftArray[i] | rightArray[i];
            }
        }
    
        public void Xor( BitVector left  ,
                         BitVector right )
        {
            BumpVersion();

            uint[] leftArray  = left .m_bitArray;
            uint[] rightArray = right.m_bitArray;
            int    leftSize   = leftArray .Length;
            int    rightSize  = rightArray.Length;
            int    maxSize    = Math.Max( leftSize, rightSize );
            int    minSize    = Math.Min( leftSize, rightSize );

            EnsureCapacityInWords( maxSize );

            uint[] myArray = m_bitArray;
            int    mySize  = myArray.Length;

            m_cardinalityCache = -1;

            //--//
            
            for(int i = mySize; --i >= maxSize; ) // Clear excess part.
            {
                myArray[i] = 0;
            }

            if(leftSize < maxSize) // Copy non-overlapping part.
            {
                for(int i = maxSize; --i >= minSize; )
                {
                    myArray[i] = rightArray[i];
                }
            }
            else
            {
                for(int i = maxSize; --i >= minSize; )
                {
                    myArray[i] = leftArray[i];
                }
            }

            for(int i = minSize; --i >= 0; ) // Xor part in common.
            {
                myArray[i] = leftArray[i] ^ rightArray[i];
            }
        }
    
        public void Difference( BitVector left  ,
                                BitVector right )
        {
            BumpVersion();

            uint[] leftArray  = left .m_bitArray;
            uint[] rightArray = right.m_bitArray;
            int    leftSize   = leftArray .Length;
            int    rightSize  = rightArray.Length;
            int    maxSize    = Math.Max( leftSize, rightSize );
            int    minSize    = Math.Min( leftSize, rightSize );

            EnsureCapacityInWords( maxSize );

            uint[] myArray = m_bitArray;
            int    mySize  = myArray.Length;

            m_cardinalityCache = -1;

            //--//

            for(int i = mySize; --i >= maxSize; ) // Clear excess part.
            {
                myArray[i] = 0;
            }

            if(leftSize < maxSize)
            {
                for(int i = maxSize; --i >= minSize; ) // Clear non-overlapping part (left is smaller, so it's always zero).
                {
                    myArray[i] = 0;
                }
            }
            else
            {
                for(int i = maxSize; --i >= minSize; ) // Copy non-overlapping part (right is smaller, so it's always zero).
                {
                    myArray[i] = leftArray[i];
                }
            }

            for(int i = minSize; --i >= 0; ) // Diff common part.
            {
                myArray[i] = leftArray[i] & ~rightArray[i];
            }
        }

        //
        // BEWARE: Since we don't track the exact number of bits,
        // complement affects more bits than it might be expected.
        //
        // For example, creating a BitVector with all bits set and then complementing it
        // doesn't return zero as the cardinality of the vector, as expected, but 32 - <initializeSize> % 32.
        //
        public void Complement()
        {
            BumpVersion();

            int size = m_bitArray.Length;

            // complement one word at a time
            for(int i = 0; i < size; i++)
            {
                m_bitArray[i] = ~m_bitArray[i];
            }

            if(m_cardinalityCache >= 0)
            {
                m_cardinalityCache = (size * c_BitsPerWord) - m_cardinalityCache;
            }
        }

        //--//

////    public static BitVector operator & ( BitVector left  ,
////                                         BitVector right )
////    {
////        BitVector res = left.Clone();
////
////        res.AndInPlace( right );
////
////        return res;
////    }
////
////    public static BitVector operator | ( BitVector left  ,
////                                         BitVector right )
////    {
////        BitVector res = left.Clone();
////
////        res.OrInPlace( right );
////
////        return res;
////    }
////
////    public static BitVector operator ^ ( BitVector left  ,
////                                         BitVector right )
////    {
////        BitVector res = left.Clone();
////
////        res.XorInPlace( right );
////
////        return res;
////    }
////
////    public static BitVector operator - ( BitVector left  ,
////                                         BitVector right )
////    {
////        BitVector res = left.Clone();
////
////        res.DifferenceInPlace( right );
////
////        return res;
////    }
////
////    public static BitVector operator ~ ( BitVector left )
////    {
////        BitVector res = left.Clone();
////
////        res.Complement();
////
////        return res;
////    }

        //--//

        public Enumerator GetEnumerator()
        {
            return new Enumerator( this );
        }

        //--//

        //
        // Helper Methods
        //

        protected void BumpVersion()
        {
#if DEBUG
            m_version++;
#endif
        }

        protected void EnsureCapacityInBits( int bits )
        {
            int arraySize = (bits + c_BitsPerWord - 1) / c_BitsPerWord;

            EnsureCapacityInWords( arraySize );
        }

        protected void EnsureCapacityInWords( int arraySize )
        {
            int size = m_bitArray.Length;

            if(size < arraySize)
            {
                uint[] oldArray = m_bitArray;

                m_bitArray = new uint[arraySize];

                Array.Copy( oldArray, m_bitArray, size );
            }
        }

        //--//

        public static uint CountBits( uint value )
        {
            //
            // In-place adder tree:
            //
            //      perform 16 1-bit adds,
            //               8 2-bit adds,
            //               4 4-bit adds,
            //               2 8-bit adds, and
            //               1 16-bit add.
            //
            value = ((value >>  1) & 0x55555555) + (value & 0x55555555);
            value = ((value >>  2) & 0x33333333) + (value & 0x33333333);
            value = ((value >>  4) & 0x0F0F0F0F) + (value & 0x0F0F0F0F);
            value = ((value >>  8) & 0x00FF00FF) + (value & 0x00FF00FF);
            value = ((value >> 16) & 0x0000FFFF) + (value & 0x0000FFFF);

            return value;
        }

        public static int GetPositionOfFirstBitSet( uint w )
        {
            int bitPos = 0;

            if((w & 0xFFFF) == 0) { w >>= 16; bitPos += 16; }
            if((w & 0x00FF) == 0) { w >>=  8; bitPos +=  8; }
            if((w & 0x000F) == 0) { w >>=  4; bitPos +=  4; }
            if((w & 0x0003) == 0) { w >>=  2; bitPos +=  2; }
            if((w & 0x0001) == 0) { w >>=  1; bitPos +=  1; }

            return bitPos;
        }

        public static int GetPositionOfFirstBitSet( ulong w )
        {
            uint wHi = (uint)(w >> 32);
            uint wLo = (uint) w;

            if(wLo != 0)
            {
                return GetPositionOfFirstBitSet( wLo );
            }
            else
            {
                return GetPositionOfFirstBitSet( wHi );
            }
        }

        public static int GetPositionOfLastBitSet( uint w )
        {
            int bitPos = 0;

            if((w >> 16) != 0) { w >>= 16; bitPos += 16; }
            if((w >>  8) != 0) { w >>=  8; bitPos +=  8; }
            if((w >>  4) != 0) { w >>=  4; bitPos +=  4; }
            if((w >>  2) != 0) { w >>=  2; bitPos +=  2; }
            if((w >>  1) != 0) { w >>=  1; bitPos +=  1; }

            return bitPos;
        }

        public static int GetPositionOfLastBitSet( ulong w )
        {
            uint wHi = (uint)(w >> 32);
            uint wLo = (uint) w;

            if(wHi != 0)
            {
                return GetPositionOfLastBitSet( wHi ) + 32;
            }
            else
            {
                return GetPositionOfLastBitSet( wLo );
            }
        }

        ///////////////////////////////////////////

        public struct Enumerator
        {
            //
            // State
            //

            private readonly BitVector m_owner;
            private readonly int       m_version;
            private          uint      m_word;
            private          int       m_index;
            private          int       m_current;

            //
            // Constructor Methods
            //

            internal Enumerator( BitVector owner )
            {
                m_owner   = owner;
                m_version = owner.m_version;
                m_word    = m_owner.m_bitArray[0];
                m_index   = 0;
                m_current = -1;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
#if DEBUG
                if(m_version != m_owner.m_version)
                {
                    throw new InvalidOperationException( "Enumerator version check failed" );
                }
#endif

                if(hasMoreElements())
                {
                    m_current = nextInt();
                    return true;
                }
                else
                {
                    m_current = -2;
                    return false;
                }
            }

            public int Current
            {
                get
                {
                    if(m_current == -1)
                    {
                        throw new InvalidOperationException( "Enumerator not started" );
                    }
                    else if(m_current == -2)
                    {
                        throw new InvalidOperationException( "Enumerator is empty" );
                    }
                    else
                    {
                        return m_current;
                    }
                }
            }

            //--//

            private bool hasMoreElements()
            {
                uint[] array = m_owner.m_bitArray;
                int    size  = array.Length;

                while(m_word == 0 && ++m_index < size)
                {
                    m_word = array[m_index];
                }

                return (m_word != 0);
            }

            private int nextInt()
            {
                int bitPos = GetPositionOfFirstBitSet( m_word );

                m_word = m_word & ~(1U << bitPos);

                return bitPos + m_index * c_BitsPerWord;
            }
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            System.Text.StringBuilder result    = new System.Text.StringBuilder( "{" );
            bool                      empty     = true;
            int                       size      = m_bitArray.Length;
            int                       bitNumber = 0;

            for(int arrayIndex = 0; arrayIndex < size; arrayIndex++)
            {
                uint bits = m_bitArray[arrayIndex];

                if(bits != 0)
                {
                    uint mask = 1;

                    while(mask != 0)
                    {
                        if((bits & mask) != 0)
                        {
                            if(empty)
                            {
                                empty = false;
                            }
                            else
                            {
                                result = result.Append( ", " );
                            }

                            result = result.Append( bitNumber );
                        }

                        bitNumber++;
                        mask <<= 1;
                    }
                }
                else
                {
                    bitNumber += 32;
                }
            }

            result = result.Append( "}" );

            return result.ToString();
        }
    }
}
