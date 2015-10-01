//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.Helpers
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    internal struct DoubleImplementation
    {
        const int   c_Mantissa_Size   = 53;
        const int   c_Mantissa_Shift  = 64 - c_Mantissa_Size;
        const int   c_Mantissa_Range  = 63;
        const ulong c_Mantissa_One    = (1ul << c_Mantissa_Range);

        const int   c_Exponent_Size   = 11;
        const uint  c_Exponent_Mask   = (1u << c_Exponent_Size)     - 1;
        const int   c_Exponent_Shift  =  32  - c_Exponent_Size      - 1;
        const int   c_Exponent_Bias   = (1  << c_Exponent_Size) / 2 - 1;

        const int   c_Sign_Shift      = 32 - 1;
        const uint  c_Sign_Mask       = 1u;

        //
        // State
        //

        internal ulong m_mantissa;
        internal int   m_exponent;
        internal uint  m_sign;
////    internal bool  m_fFinite;

        //
        // Constructor Methods
        //

        //[Inline]
        internal unsafe DoubleImplementation( double val )
        {
            ulong rawVal = *(ulong*)&val;

            m_exponent  = (int)((MathImpl.ExtractHighPart( rawVal ) >> c_Exponent_Shift) & c_Exponent_Mask) - c_Exponent_Bias;
            m_sign      =        MathImpl.ExtractHighPart( rawVal ) >> c_Sign_Shift;

            ulong mantissa = rawVal << c_Mantissa_Shift;

            if(m_exponent > -c_Exponent_Bias)
            {
                mantissa |= c_Mantissa_One;
            }

            m_mantissa = mantissa;
        }

        //[Inline]
        internal DoubleImplementation( int  val       ,
                                       bool fOverflow )
        {
            if(val == 0)
            {
                m_mantissa = 0;
                m_exponent = -c_Exponent_Bias;
                m_sign     = 0;
            }
            else
            {
                ulong valU;

                if(val < 0)
                {
                    valU   = (ulong)unchecked( (uint)-val );
                    m_sign = c_Sign_Mask;
                }
                else
                {
                    valU   = (ulong)unchecked( (uint)val );
                    m_sign = 0;
                }

                m_exponent = c_Mantissa_Range;
                m_mantissa = 0;

                //--//

                Renormalize_Range0to2( valU );
            }
        }

        //[Inline]
        internal DoubleImplementation( uint val       ,
                                       bool fOverflow )
        {
            m_sign = 0;

            if(val == 0)
            {
                m_mantissa = 0;
                m_exponent = -c_Exponent_Bias;
            }
            else
            {
                m_exponent = c_Mantissa_Range;
                m_mantissa = 0;

                //--//

                Renormalize_Range0to2( (ulong)val );
            }
        }

        //[Inline]
        internal DoubleImplementation( long val       ,
                                       bool fOverflow )
        {
            if(val == 0)
            {
                m_mantissa = 0;
                m_exponent = -c_Exponent_Bias;
                m_sign     = 0;
            }
            else
            {
                ulong valU;

                if(val < 0)
                {
                    valU   = unchecked( (ulong)-val );
                    m_sign = c_Sign_Mask;
                }
                else
                {
                    valU   = unchecked( (ulong)val );
                    m_sign = 0;
                }

                m_exponent = c_Mantissa_Range;
                m_mantissa = 0;

                //--//

                Renormalize_Range0to2( valU );
            }
        }

        //[Inline]
        internal DoubleImplementation( ulong val       ,
                                       bool  fOverflow )
        {
            m_sign = 0;

            if(val == 0)
            {
                m_mantissa = 0;
                m_exponent = -c_Exponent_Bias;
            }
            else
            {
                m_exponent = c_Mantissa_Range;
                m_mantissa = 0;

                //--//

                Renormalize_Range0to2( val );
            }
        }

        //[Inline]
        internal DoubleImplementation( ref FloatImplementation fi        ,
                                           bool                fOverflow )
        {
            m_mantissa = MathImpl.InsertHighPart( fi.m_mantissa );
            m_exponent =                          fi.m_exponent;
            m_sign     =                          fi.m_sign;
        }

        //
        // Helper Methods
        //

        [Inline]
        internal void Negate()
        {
            m_sign ^= c_Sign_Mask;
        }

        //[Inline]
        internal int Compare( ref DoubleImplementation right )
        {
            int dir = (this.m_sign != 0) ? -1 : 1;

            if(this.m_sign != right.m_sign)
            {
                return dir;
            }
            else
            {
                int diff = (this.m_exponent - right.m_exponent);

                if(diff == 0)
                {
                    return dir * this.m_mantissa.CompareTo( right.m_mantissa );
                }
                else if(diff > 0)
                {
                    return dir;
                }
                else
                {
                    return -dir;
                }
            }
        }

        //[Inline]
        internal void Add( ref DoubleImplementation right )
        {
            if(this.m_sign != right.m_sign)
            {
                DoubleImplementation tmp = right;

                tmp.Negate();

                Sub( ref tmp );
            }
            else
            {
                int   diff = (this.m_exponent - right.m_exponent);
                ulong tmp;

                //
                // Shift down to prevent overflow.
                //
                ulong valLeft  = this .m_mantissa >> 1;
                ulong valRight = right.m_mantissa >> 1;

                if(diff == 0)
                {
                    tmp = valLeft + valRight;
                }
                else if(diff > 0)
                {
                    if(diff <= c_Mantissa_Range)
                    {
                        tmp = valLeft + (valRight >> diff);
                    }
                    else
                    {
                        //
                        // Too small, nothing to do.
                        //
                        tmp = valLeft;
                    }
                }
                else
                {
                    diff = -diff;

                    this.m_exponent = right.m_exponent;

                    if(diff <= c_Mantissa_Range)
                    {
                        tmp  = valRight += (valLeft >> diff);
                    }
                    else
                    {

                        //
                        // Too small, nothing to do.
                        //
                        tmp = valRight;
                    }
                }

                this.m_exponent++; //This accounts for the right shift to avoid overflow.

                Renormalize_Range0to2( tmp );
            }
        }

        //[Inline]
        internal void Sub( ref DoubleImplementation right )
        {
            if(this.m_sign != right.m_sign)
            {
                DoubleImplementation tmp = right;

                tmp.Negate();

                Add( ref tmp );
            }
            else
            {
                int   diff = (this.m_exponent - right.m_exponent);
                ulong tmp;

                ulong valLeft  = this .m_mantissa;
                ulong valRight = right.m_mantissa;

                if(diff == 0)
                {
                    if(valLeft >= valRight)
                    {
                        tmp = valLeft - valRight;
                    }
                    else
                    {
                        this.Negate();

                        tmp = valRight - valLeft;
                    }
                }
                else if(diff > 0)
                {
                    if(diff <= c_Mantissa_Range)
                    {
                        tmp  = valLeft - (valRight >> diff);
                    }
                    else
                    {
                        //
                        // Too small, nothing to do.
                        //
                        tmp = valLeft;
                    }
                }
                else
                {
                    diff = -diff;

                    this.Negate();
                    this.m_exponent = right.m_exponent;

                    if(diff <= c_Mantissa_Range)
                    {
                        tmp = valRight - (valLeft >> diff);
                    }
                    else
                    {
                        //
                        // Too small, nothing to do.
                        //
                        tmp = valRight;
                    }
                }

                Renormalize_Range0to2( tmp );
            }
        }

        //[Inline]
        internal void Mul( ref DoubleImplementation right )
        {
            this.m_sign     ^= right.m_sign;
            this.m_exponent += right.m_exponent;

            ulong tmp;
            
            tmp  = MathImpl.ExtractHighPart( this .m_mantissa );
            tmp *= MathImpl.ExtractHighPart( right.m_mantissa );

            if(MathImpl.ExtractLowPart( this.m_mantissa ) != 0)
            {
                ulong tmp2;

                tmp2  = MathImpl.ExtractLowPart ( this .m_mantissa );
                tmp2 *= MathImpl.ExtractHighPart( right.m_mantissa );

                tmp += (tmp2 >> 32);
            }

            if(MathImpl.ExtractLowPart( right.m_mantissa ) != 0)
            {
                ulong tmp2;

                tmp2  = MathImpl.ExtractHighPart( this .m_mantissa );
                tmp2 *= MathImpl.ExtractLowPart ( right.m_mantissa );

                tmp += (tmp2 >> 32);
            }

            //
            // Result will be between 0x4000_0000_0000_0000 (1.0) and 0xFFFF_FFFE_0000_0001 (<4.0).
            //
            if(MathImpl.IsNegative( tmp )) // It means it's more than 2.0.
            {
                this.m_mantissa  = tmp;
                this.m_exponent += 1;
            }
            else
            {
                this.m_mantissa = tmp << 1;
            }
        }

        //[Inline]
        internal void Div( ref DoubleImplementation right )
        {
            this.m_sign     ^= right.m_sign;
            this.m_exponent -= right.m_exponent;

            ulong tmp;

            //
            // BUGBUG: We need to use 64x64 division, not 64x32.
            //
            tmp  =                           this .m_mantissa  ;
            tmp /= MathImpl.ExtractHighPart( right.m_mantissa );

            //
            // Result will be between 0x4000_0000 (0.5) and 0xFFFF_FFFF (~2.0).
            //
            Renormalize_Range0to2( tmp << 31 );
        }

        //--//

        //[Inline]
        internal unsafe double ToDouble()
        {
            if(m_mantissa == 0)
            {
                m_exponent = -c_Exponent_Bias;
            }

            uint  rawValHigh;
            ulong rawVal;
            
            rawVal      =        (m_mantissa                    & ~c_Mantissa_One ) >> c_Mantissa_Shift;
            rawValHigh  = ((uint)(m_exponent + c_Exponent_Bias) &  c_Exponent_Mask) << c_Exponent_Shift;
            rawValHigh |=         m_sign                                            << c_Sign_Shift;

            rawVal |= MathImpl.InsertHighPart( rawValHigh );

            return *(double*)&rawVal;
        }

        //[Inline]
        internal unsafe int ToInt( bool fOverflow )
        {
            ulong val        = m_mantissa;
            int   rightShift = c_Mantissa_Range - m_exponent;

            if(rightShift < 0)
            {
                if(fOverflow)
                {
                    throw new OverflowException();
                }

                val = 0xFFFFFFFFu;
            }
            else
            {
                if(rightShift > c_Mantissa_Range)
                {
                    val = 0;
                }
                else
                {
                    val >>= rightShift;
                }
            }

            if(m_sign != 0)
            {
                return -(int)val;
            }
            else
            {
                return (int)val;
            }
        }

        //[Inline]
        internal unsafe uint ToUnsignedInt( bool fOverflow )
        {
            ulong val        = m_mantissa;
            int   rightShift = c_Mantissa_Range - m_exponent;

            if(rightShift < 0)
            {
                if(fOverflow)
                {
                    throw new OverflowException();
                }

                val = 0xFFFFFFFFu;
            }
            else
            {
                if(rightShift > c_Mantissa_Range)
                {
                    val = 0;
                }
                else
                {
                    val >>= rightShift;
                }
            }

            if(m_sign != 0)
            {
                return (uint)-(long)val;
            }
            else
            {
                return (uint)val;
            }
        }

        //[Inline]
        internal unsafe long ToLong( bool fOverflow )
        {
            ulong val        = m_mantissa;
            int   rightShift = c_Mantissa_Range - m_exponent;

            if(rightShift < 0)
            {
                int leftShift = -rightShift;

                if(leftShift > c_Mantissa_Range)
                {
                    if(fOverflow)
                    {
                        throw new OverflowException();
                    }

                    val = 0xFFFFFFFFFFFFFFFFu;
                }
                else
                {
                    val <<= leftShift;
                }
            }
            else if(rightShift > 0)
            {
                if(rightShift > c_Mantissa_Range)
                {
                    val = 0;
                }
                else
                {
                    val >>= (rightShift - 1);
                    val  += 1;
                    val >>= 1;
                }
            }

            if(m_sign != 0)
            {
                return -(long)val;
            }
            else
            {
                return (long)val;
            }
        }

        //[Inline]
        internal unsafe ulong ToUnsignedLong( bool fOverflow )
        {
            ulong val        = m_mantissa;
            int   rightShift = c_Mantissa_Range - m_exponent;

            if(rightShift < 0)
            {
                int leftShift = -rightShift;

                if(leftShift > c_Mantissa_Range)
                {
                    if(fOverflow)
                    {
                        throw new OverflowException();
                    }

                    val = 0xFFFFFFFFFFFFFFFFu;
                }
                else
                {
                    val <<= leftShift;
                }
            }
            else if(rightShift > 0)
            {
                if(rightShift > c_Mantissa_Range)
                {
                    val = 0;
                }
                else
                {
                    val >>= (rightShift - 1);
                    val  += 1;
                    val >>= 1;
                }
            }

            if(m_sign != 0)
            {
                return (ulong)-(long)val;
            }
            else
            {
                return val;
            }
        }

        //--//

        [Inline]
        private void Renormalize_Range0to2( ulong val )
        {
            if(val == 0)
            {
                m_mantissa = 0;
                m_exponent = 0;
            }
            else
            {
                while(MathImpl.IsPositive( val )) // Less than 1.
                {
                    val *= 2;
                    m_exponent--;
                }

                m_mantissa = val;
            }
        }

        //
        // Access Methods
        //

        internal bool IsFinite
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
