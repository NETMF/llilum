// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  UInt64
**
** Purpose: This class will encapsulate an unsigned long and
**          provide an Object representation of it.
**
**
===========================================================*/
namespace System
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;

    // Wrapper for unsigned 64 bit integers.
    [Microsoft.Zelig.Internals.WellKnownType( "System_UInt64" )]
    [Serializable]
    [CLSCompliant( false )]
    [StructLayout( LayoutKind.Sequential )]
    public struct UInt64 : IComparable, IFormattable, IConvertible, IComparable<UInt64>, IEquatable<UInt64>
    {
        public const ulong MaxValue = (ulong)0xFFFFFFFFFFFFFFFFL;
        public const ulong MinValue =        0x0;

        private ulong m_value;


        // Compares this object to another object, returning an integer that
        // indicates the relationship.
        // Returns a value less than zero if this  object
        // null is considered to be less than any instance.
        // If object is not of type UInt64, this method throws an ArgumentException.
        //
        public int CompareTo( Object value )
        {
            if(value == null)
            {
                return 1;
            }

            if(value is UInt64)
            {
                return CompareTo( (UInt64)value );
            }

#if EXCEPTION_STRINGS
            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeUInt64" ) );
#else
            throw new ArgumentException();
#endif
        }

        public int CompareTo( UInt64 value )
        {
            // Need to use compare because subtraction will wrap
            // to positive for very large neg numbers, etc.
            if(m_value < value) return -1;
            if(m_value > value) return  1;
            return 0;
        }

        public override bool Equals( Object obj )
        {
            if(!(obj is UInt64))
            {
                return false;
            }
            return Equals( (UInt64)obj );
        }

        public bool Equals( UInt64 obj )
        {
            return m_value == obj;
        }

        // The value of the lower 32 bits XORed with the uppper 32 bits.
        public override int GetHashCode()
        {
            return ((int)m_value) ^ (int)(m_value >> 32);
        }

        public override String ToString()
        {
            return Number.FormatUInt64( m_value, /*null,*/ NumberFormatInfo.CurrentInfo );
        }

        public String ToString( IFormatProvider provider )
        {
            return Number.FormatUInt64( m_value, /*null,*/ NumberFormatInfo.GetInstance( provider ) );
        }
    
        public String ToString( String format )
        {
            return Number.FormatUInt64( m_value, format, NumberFormatInfo.CurrentInfo );
        }
    
        public String ToString( String          format   ,
                                IFormatProvider provider )
        {
            return Number.FormatUInt64( m_value, format, NumberFormatInfo.GetInstance( provider ) );
        }
    
        [CLSCompliant( false )]
        public static ulong Parse( String s )
        {
            return Number.ParseUInt64( s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo );
        }
    
        [CLSCompliant( false )]
        public static ulong Parse( String       s     ,
                                   NumberStyles style )
        {
            NumberFormatInfo.ValidateParseStyleInteger( style );
    
            return Number.ParseUInt64( s, style, NumberFormatInfo.CurrentInfo );
        }
    
        [CLSCompliant( false )]
        public static ulong Parse( string          s        ,
                                   IFormatProvider provider )
        {
            return Number.ParseUInt64( s, NumberStyles.Integer, NumberFormatInfo.GetInstance( provider ) );
        }
    
        [CLSCompliant( false )]
        public static ulong Parse( String          s        ,
                                   NumberStyles    style    ,
                                   IFormatProvider provider )
        {
            NumberFormatInfo.ValidateParseStyleInteger( style );
    
            return Number.ParseUInt64( s, style, NumberFormatInfo.GetInstance( provider ) );
        }
    
        [CLSCompliant( false )]
        public static Boolean TryParse(     String s      ,
                                        out UInt64 result )
        {
            return Number.TryParseUInt64( s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result );
        }
    
        [CLSCompliant( false )]
        public static Boolean TryParse(     String          s        ,
                                            NumberStyles    style    ,
                                            IFormatProvider provider ,
                                        out UInt64          result   )
        {
            NumberFormatInfo.ValidateParseStyleInteger( style );
    
            return Number.TryParseUInt64( s, style, NumberFormatInfo.GetInstance( provider ), out result );
        }

        #region IConvertible

        public TypeCode GetTypeCode()
        {
            return TypeCode.UInt64;
        }
    
        /// <internalonly/>
        bool IConvertible.ToBoolean( IFormatProvider provider )
        {
            return Convert.ToBoolean( m_value );
        }
    
        /// <internalonly/>
        char IConvertible.ToChar( IFormatProvider provider )
        {
            return Convert.ToChar( m_value );
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
            return m_value;
        }
    
        /// <internalonly/>
        float IConvertible.ToSingle( IFormatProvider provider )
        {
            return Convert.ToSingle( m_value );
        }
    
        /// <internalonly/>
        double IConvertible.ToDouble( IFormatProvider provider )
        {
            return Convert.ToDouble( m_value );
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
            throw new InvalidCastException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "InvalidCast_FromTo" ), "UInt64", "DateTime" ) );
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
