// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  Double
**
**
** Purpose: A representation of an IEEE double precision
**          floating point number.
**
**
===========================================================*/
namespace System
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
        using System.Runtime.ConstrainedExecution;

    [Microsoft.Zelig.Internals.WellKnownType( "System_Double" )]
    [Serializable]
    [StructLayout( LayoutKind.Sequential )]
    public struct Double : IComparable, IFormattable, IConvertible, IComparable<Double>, IEquatable<Double>
    {
        //
        // Public Constants
        //
        public const double MinValue         =         -1.7976931348623157E+308;
        public const double Epsilon          =          4.9406564584124654E-324;// Note Epsilon should be a double whose hex representation is 0x1 on little endian machines.
        public const double MaxValue         =          1.7976931348623157E+308;
        public const double NegativeInfinity = (double)-1.0 / (double)0.0;
        public const double PositiveInfinity = (double) 1.0 / (double)0.0;
        public const double NaN              = (double) 0.0 / (double)0.0;

////    internal static double NegativeZero = BitConverter.Int64BitsToDouble( unchecked( (long)0x8000000000000000 ) );

        internal double m_value;


        public unsafe static bool IsInfinity( double d )
        {
            return (*(long*)(&d) & 0x7FFFFFFFFFFFFFFF) == 0x7FF0000000000000;
        }

        public static bool IsPositiveInfinity( double d )
        {
            //Jit will generate inlineable code with this
            if(d == double.PositiveInfinity)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsNegativeInfinity( double d )
        {
            //Jit will generate inlineable code with this
            if(d == double.NegativeInfinity)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal unsafe static bool IsNegative( double d )
        {
            return (*(UInt64*)(&d) & 0x8000000000000000) == 0x8000000000000000;
        }

////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public static bool IsNaN( double d )
        {
            //Jit will generate inlineable code with this
            // warning CS1718: comparison to same variable
#pragma warning disable 1718
            if(d != d)
            {
                return true;
            }
            else
            {
                return false;
            }
#pragma warning restore 1718
        }


        // Compares this object to another object, returning an instance of System.Relation.
        // Null is considered less than any instance.
        //
        // If object is not of type Double, this method throws an ArgumentException.
        //
        // Returns a value less than zero if this  object
        //
        public int CompareTo( Object value )
        {
            if(value == null)
            {
                return 1;
            }

            if(value is Double)
            {
                return CompareTo( (Double)value );
            }

#if EXCEPTION_STRINGS
            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeDouble" ) );
#else
            throw new ArgumentException();
#endif
        }

        public int CompareTo( Double value )
        {
            if(m_value <  value) return -1;
            if(m_value >  value) return  1;
            if(m_value == value) return  0;

            // At least one of the values is NaN.
            if(IsNaN( m_value ))
            {
                return (IsNaN( value ) ? 0 : -1);
            }
            else
            {
                return 1;
            }
        }

        // True if obj is another Double with the same value as the current instance.  This is
        // a method of object equality, that only returns true if obj is also a double.
        public override bool Equals( Object obj )
        {
            if(!(obj is Double))
            {
                return false;
            }

            return Equals( (Double)obj );
        }

        public bool Equals( Double obj )
        {
            if(obj == m_value)
            {
                return true;
            }

            return IsNaN( obj ) && IsNaN( m_value );
        }

        //The hashcode for a double is the absolute value of the integer representation
        //of that double.
        //
        public unsafe override int GetHashCode()
        {
            double d = m_value;

            if(d == 0)
            {
                // Ensure that 0 and -0 have the same hash code
                return 0;
            }

            long value = *(long*)(&d);

            return unchecked( (int)value ) ^ ((int)(value >> 32));
        }

        public override String ToString()
        {
            return Number.FormatDouble( m_value, /*null,*/ NumberFormatInfo.CurrentInfo );
        }

        public String ToString( String format )
        {
            return Number.FormatDouble( m_value, format, NumberFormatInfo.CurrentInfo );
        }
    
        public String ToString( IFormatProvider provider )
        {
            return Number.FormatDouble( m_value, /*null,*/ NumberFormatInfo.GetInstance( provider ) );
        }
    
        public String ToString( String format, IFormatProvider provider )
        {
            return Number.FormatDouble( m_value, format, NumberFormatInfo.GetInstance( provider ) );
        }
    
        public static double Parse( String s )
        {
            return Parse( s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo );
        }
    
        public static double Parse( String s, NumberStyles style )
        {
            NumberFormatInfo.ValidateParseStyleFloatingPoint( style );
    
            return Parse( s, style, NumberFormatInfo.CurrentInfo );
        }
    
        public static double Parse( String s, IFormatProvider provider )
        {
            return Parse( s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.GetInstance( provider ) );
        }
    
        public static double Parse( String s, NumberStyles style, IFormatProvider provider )
        {
            NumberFormatInfo.ValidateParseStyleFloatingPoint( style );
    
            return Parse( s, style, NumberFormatInfo.GetInstance( provider ) );
        }
    
        // Parses a double from a String in the given style.  If
        // a NumberFormatInfo isn't specified, the current culture's
        // NumberFormatInfo is assumed.
        //
        // This method will not throw an OverflowException, but will return
        // PositiveInfinity or NegativeInfinity for a number that is too
        // large or too small.
        //
        private static double Parse( String s, NumberStyles style, NumberFormatInfo info )
        {
            try
            {
                return Number.ParseDouble( s, style, info );
            }
            catch(FormatException)
            {
                //If we caught a FormatException, it may be from one of our special strings.
                //Check the three with which we're concerned and rethrow if it's not one of
                //those strings.
                String sTrim = s.Trim();
    
                if(sTrim.Equals( info.PositiveInfinitySymbol ))
                {
                    return PositiveInfinity;
                }
    
                if(sTrim.Equals( info.NegativeInfinitySymbol ))
                {
                    return NegativeInfinity;
                }
    
                if(sTrim.Equals( info.NaNSymbol ))
                {
                    return NaN;
                }
                //Rethrow the previous exception;
                throw;
            }
        }
    
        public static bool TryParse( String s, out double result )
        {
            return TryParse( s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo, out result );
        }
    
        public static bool TryParse( String s, NumberStyles style, IFormatProvider provider, out double result )
        {
            NumberFormatInfo.ValidateParseStyleFloatingPoint( style );
    
            return TryParse( s, style, NumberFormatInfo.GetInstance( provider ), out result );
        }
    
        private static bool TryParse( String s, NumberStyles style, NumberFormatInfo info, out double result )
        {
            if(s == null)
            {
                result = 0;
                return false;
            }
    
            bool success = Number.TryParseDouble( s, style, info, out result );
            if(!success)
            {
                String sTrim = s.Trim();
    
                if(sTrim.Equals( info.PositiveInfinitySymbol ))
                {
                    result = PositiveInfinity;
                }
                else if(sTrim.Equals( info.NegativeInfinitySymbol ))
                {
                    result = NegativeInfinity;
                }
                else if(sTrim.Equals( info.NaNSymbol ))
                {
                    result = NaN;
                }
                else
                {
                    return false; // We really failed
                }
            }
    
            return true;
        }

        #region IConvertible

        public TypeCode GetTypeCode()
        {
            return TypeCode.Double;
        }
    
        /// <internalonly/>
        bool IConvertible.ToBoolean( IFormatProvider provider )
        {
            return Convert.ToBoolean( m_value );
        }
    
        /// <internalonly/>
        char IConvertible.ToChar( IFormatProvider provider )
        {
#if EXCEPTION_STRINGS
            throw new InvalidCastException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "InvalidCast_FromTo" ), "Double", "Char" ) );
#else
            throw new InvalidCastException();
#endif
        }
    
        /// <internalonly/>
        sbyte IConvertible.ToSByte( IFormatProvider provider )
        {
            return Convert.ToSByte( m_value );
        }
    
        /// <internalonly/>
        byte IConvertible.ToByte( IFormatProvider provider )
        {
            return Convert.ToByte( m_value );
        }
    
        /// <internalonly/>
        short IConvertible.ToInt16( IFormatProvider provider )
        {
            return Convert.ToInt16( m_value );
        }
    
        /// <internalonly/>
        ushort IConvertible.ToUInt16( IFormatProvider provider )
        {
            return Convert.ToUInt16( m_value );
        }
    
        /// <internalonly/>
        int IConvertible.ToInt32( IFormatProvider provider )
        {
            return Convert.ToInt32( m_value );
        }
    
        /// <internalonly/>
        uint IConvertible.ToUInt32( IFormatProvider provider )
        {
            return Convert.ToUInt32( m_value );
        }
    
        /// <internalonly/>
        long IConvertible.ToInt64( IFormatProvider provider )
        {
            return Convert.ToInt64( m_value );
        }
    
        /// <internalonly/>
        ulong IConvertible.ToUInt64( IFormatProvider provider )
        {
            return Convert.ToUInt64( m_value );
        }
    
        /// <internalonly/>
        float IConvertible.ToSingle( IFormatProvider provider )
        {
            return Convert.ToSingle( m_value );
        }
    
        /// <internalonly/>
        double IConvertible.ToDouble( IFormatProvider provider )
        {
            return m_value;
        }
    
        /// <internalonly/>
        Decimal IConvertible.ToDecimal( IFormatProvider provider )
        {
            return Convert.ToDecimal( m_value );
        }
    
        /// <internalonly/>
        DateTime IConvertible.ToDateTime( IFormatProvider provider )
        {
#if EXCEPTION_STRINGS
            throw new InvalidCastException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "InvalidCast_FromTo" ), "Double", "DateTime" ) );
#else
            throw new InvalidCastException();
#endif
        }
    
        /// <internalonly/>
        Object IConvertible.ToType( Type type, IFormatProvider provider )
        {
            return Convert.DefaultToType( (IConvertible)this, type, provider );
        }

        #endregion
    }
}
