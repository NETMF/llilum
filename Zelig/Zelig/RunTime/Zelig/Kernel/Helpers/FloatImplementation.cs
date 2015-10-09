//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.Helpers
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    internal struct FloatImplementation
    {
        const int  c_Mantissa_Size   = 24;
        const int  c_Mantissa_Shift  = 32 - c_Mantissa_Size;
        const int  c_Mantissa_Range  = 31;
        const uint c_Mantissa_One    = (1u << c_Mantissa_Range);

        const int  c_Exponent_Size   = 8;
        const uint c_Exponent_Mask   = (1u << c_Exponent_Size)     - 1;
        const int  c_Exponent_Shift  =  32  - c_Exponent_Size      - 1;
        const int  c_Exponent_Bias   = (1  << c_Exponent_Size) / 2 - 1;

        const int  c_Sign_Shift      = 32 - 1;
        const uint c_Sign_Mask       = 1u;

        //
        // State
        //

        internal uint m_mantissa;
        internal int  m_exponent;
        internal uint m_sign;
////    internal bool m_fFinite;

        //
        // Constructor Methods
        //

        //[Inline]
        internal unsafe FloatImplementation( float val )
        {
            uint rawVal = *(uint*)&val;

            m_exponent  = (int)((rawVal >> c_Exponent_Shift) & c_Exponent_Mask) - c_Exponent_Bias;
            m_sign      =        rawVal >> c_Sign_Shift;

            uint mantissa = rawVal << c_Mantissa_Shift;

            if(m_exponent > -c_Exponent_Bias)
            {
                mantissa |= c_Mantissa_One;
            }

            m_mantissa = mantissa;
        }

        //[Inline]
        internal FloatImplementation( int  val       ,
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
                uint valU;

                if(val < 0)
                {
                    valU   = unchecked( (uint)-val );
                    m_sign = c_Sign_Mask;
                }
                else
                {
                    valU   = unchecked( (uint)val );
                    m_sign = 0;
                }

                m_exponent = c_Mantissa_Range;
                m_mantissa = 0;

                //--//

                Renormalize_Range0to2( valU );
            }
        }

        //[Inline]
        internal FloatImplementation( uint val       ,
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

                Renormalize_Range0to2( val );
            }
        }

        //[Inline]
        internal FloatImplementation( long val       ,
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

                while(MathImpl.IsNegative( valU )) // It means it's more than one.
                {
                    valU       >>= 1;
                    m_exponent  += 1;
                }

                Renormalize_Range0to2( (uint)valU );
            }
        }

        //[Inline]
        internal FloatImplementation( ulong val       ,
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

                while(MathImpl.IsNegative( val )) // It means it's more than one.
                {
                    val        >>= 1;
                    m_exponent  += 1;
                }

                Renormalize_Range0to2( (uint)val );
            }
        }

        //[Inline]
        internal FloatImplementation( ref DoubleImplementation di        ,
                                          bool                 fOverflow )
        {
            m_mantissa = (uint)(di.m_mantissa >> 32);
            m_exponent =        di.m_exponent;
            m_sign     =        di.m_sign;

            if(m_exponent > c_Exponent_Bias)
            {
                if(fOverflow)
                {
                    throw new OverflowException();
                }

                m_exponent = c_Exponent_Bias;
                m_mantissa = 0xFFFFFFFFu;
            }
            else if(m_exponent < -c_Exponent_Bias)
            {
                m_exponent = 0;
                m_mantissa = c_Mantissa_One;
            }
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
        internal int Compare( ref FloatImplementation right )
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
        internal void Add( ref FloatImplementation right )
        {
            if(this.m_sign != right.m_sign)
            {
                FloatImplementation tmp = right;

                tmp.Negate();

                Sub( ref tmp );
            }
            else
            {
                int   diff = (this.m_exponent - right.m_exponent);
                ulong tmp;

                if(diff == 0)
                {
                    tmp  = this .m_mantissa;
                    tmp += right.m_mantissa;
                }
                else if(diff > 0)
                {
                    if(diff <= c_Mantissa_Range)
                    {
                        tmp  =  this .m_mantissa;
                        tmp += (right.m_mantissa >> diff);
                    }
                    else
                    {
                        //
                        // Too small, nothing to do.
                        //
                        tmp = this.m_mantissa;
                    }
                }
                else
                {
                    diff = -diff;

                    this.m_exponent = right.m_exponent;

                    if(diff <= c_Mantissa_Range)
                    {
                        tmp  =  right.m_mantissa;
                        tmp += (this .m_mantissa >> diff);
                    }
                    else
                    {

                        //
                        // Too small, nothing to do.
                        //
                        tmp = right.m_mantissa;
                    }
                }

                Renormalize_Range1to4( tmp );
            }
        }

        //[Inline]
        internal void Sub( ref FloatImplementation right )
        {
            if(this.m_sign != right.m_sign)
            {
                FloatImplementation tmp = right;

                tmp.Negate();

                Add( ref tmp );
            }
            else
            {
                int  diff = (this.m_exponent - right.m_exponent);
                uint tmp;

                if(diff == 0)
                {
                    if(this.m_mantissa >= right.m_mantissa)
                    {
                        tmp  = this .m_mantissa;
                        tmp -= right.m_mantissa;
                    }
                    else
                    {
                        this.Negate();

                        tmp  = right.m_mantissa;
                        tmp -= this .m_mantissa;
                    }
                }
                else if(diff > 0)
                {
                    if(diff <= c_Mantissa_Range)
                    {
                        tmp  =  this .m_mantissa;
                        tmp -= (right.m_mantissa >> diff);
                    }
                    else
                    {
                        //
                        // Too small, nothing to do.
                        //
                        tmp = this.m_mantissa;
                    }
                }
                else
                {
                    diff = -diff;

                    this.Negate();
                    this.m_exponent = right.m_exponent;

                    if(diff <= c_Mantissa_Range)
                    {
                        tmp  =  right.m_mantissa;
                        tmp -= (this .m_mantissa >> diff);
                    }
                    else
                    {
                        //
                        // Too small, nothing to do.
                        //
                        tmp = right.m_mantissa;
                    }
                }

                Renormalize_Range0to2( tmp );
            }
        }

        //[Inline]
        internal void Mul( ref FloatImplementation right )
        {
            this.m_sign     ^= right.m_sign;
            this.m_exponent += right.m_exponent;

            ulong tmp;
            
            tmp  = (ulong)this .m_mantissa;
            tmp *= (ulong)right.m_mantissa;

            //
            // Result will be between 0x4000_0000_0000_0000 (1.0) and 0xFFFF_FFFE_0000_0001 (<4.0).
            //
            if(MathImpl.IsNegative( tmp )) // It means it's more than 2.0.
            {
                this.m_mantissa  = (uint)(tmp >> (c_Mantissa_Range+1));
                this.m_exponent += 1;
            }
            else
            {
                this.m_mantissa = (uint)(tmp >> c_Mantissa_Range);
            }
        }

        //[Inline]
        internal void Div( ref FloatImplementation right )
        {
            this.m_sign     ^= right.m_sign;
            this.m_exponent -= right.m_exponent;

            ulong tmp;
            
            tmp  = ((ulong)this .m_mantissa) << 31;
            tmp /=  (ulong)right.m_mantissa;

            //
            // Result will be between 0x4000_0000 (0.5) and 0xFFFF_FFFF (~2.0).
            //
            Renormalize_Range0to2( (uint)tmp );
        }

        //--//

        //[Inline]
        internal unsafe float ToFloat()
        {
            if(m_mantissa == 0)
            {
                m_exponent = -c_Exponent_Bias;
            }

            uint rawVal;

            rawVal  =        (m_mantissa                    & ~c_Mantissa_One ) >> c_Mantissa_Shift;
            rawVal |= ((uint)(m_exponent + c_Exponent_Bias) &  c_Exponent_Mask) << c_Exponent_Shift;
            rawVal |=         m_sign                                            << c_Sign_Shift;

            return *(float*)&rawVal;
        }

        //[Inline]
        internal unsafe int ToInt( bool fOverflow )
        {
            uint val        = m_mantissa;
            int  rightShift = c_Mantissa_Range - m_exponent;

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
            uint val        = m_mantissa;
            int  rightShift = c_Mantissa_Range - m_exponent;

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
                return (uint)(-(long)val);
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
                return (ulong)(-(long)val);
            }
            else
            {
                return val;
            }
        }

        //--//

        [Inline]
        private void Renormalize_Range1to4( ulong val )
        {
            if((uint)(val >> 32) != 0) // It means it's more than one.
            {
                m_mantissa  = (uint)(val >> 1);
                m_exponent += 1;
            }
            else
            {
                m_mantissa = (uint)val;
            }
        }

        
        [Inline]
        private void Renormalize_Range0to2( uint val )
        {
            if(val == 0)
            {
                m_mantissa = 0;
                m_exponent = 0;
            }
            else
            {
                while(val < c_Mantissa_One)
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
