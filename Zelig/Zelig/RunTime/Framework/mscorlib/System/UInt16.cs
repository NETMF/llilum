// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  UInt16
**
** Purpose: This class will encapsulate a short and provide an
**          Object representation of it.
**
**
===========================================================*/
namespace System
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;

    // Wrapper for unsigned 16 bit integers.
    [Microsoft.Zelig.Internals.WellKnownType( "System_UInt16" )]
    [Serializable]
    [CLSCompliant( false )]
    [StructLayout( LayoutKind.Sequential )]
    public struct UInt16 : IComparable, IFormattable, IConvertible, IComparable<UInt16>, IEquatable<UInt16>
    {
        public const ushort MaxValue = (ushort)0xFFFF;
        public const ushort MinValue =         0x0000;

        internal ushort m_value;


        // Compares this object to another object, returning an integer that
        // indicates the relationship.
        // Returns a value less than zero if this  object
        // null is considered to be less than any instance.
        // If object is not of type UInt16, this method throws an ArgumentException.
        //
        public int CompareTo( Object value )
        {
            if(value == null)
            {
                return 1;
            }

            if(value is UInt16)
            {
                return CompareTo( (UInt16)value );
            }

#if EXCEPTION_STRINGS
            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeUInt16" ) );
#else
            throw new ArgumentException();
#endif
        }

        public int CompareTo( UInt16 value )
        {
            return ((int)m_value - (int)value);
        }

        public override bool Equals( Object obj )
        {
            if(!(obj is UInt16))
            {
                return false;
            }

            return Equals( (UInt16)obj );
        }

        public bool Equals( UInt16 obj )
        {
            return m_value == obj;
        }

        // Returns a HashCode for the UInt16
        public override int GetHashCode()
        {
            return (int)m_value;
        }

        // Converts the current value to a String in base-10 with no extra padding.
        public override String ToString()
        {
            return Number.FormatUInt32( m_value, /*null,*/ NumberFormatInfo.CurrentInfo );
        }

        public String ToString( IFormatProvider provider )
        {
            return Number.FormatUInt32( m_value, /*null,*/ NumberFormatInfo.GetInstance( provider ) );
        }
    
        public String ToString( String format )
        {
            return Number.FormatUInt32( m_value, format, NumberFormatInfo.CurrentInfo );
        }
    
        public String ToString( String format, IFormatProvider provider )
        {
            return Number.FormatUInt32( m_value, format, NumberFormatInfo.GetInstance( provider ) );
        }
    
        public static ushort Parse( String s )
        {
            return Parse( s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo );
        }
    
        [CLSCompliant( false )]
        public static ushort Parse( String       s     ,
                                    NumberStyles style )
        {
            NumberFormatInfo.ValidateParseStyleInteger( style );
    
            return Parse( s, style, NumberFormatInfo.CurrentInfo );
        }
    
    
        [CLSCompliant( false )]
        public static ushort Parse( String          s        ,
                                    IFormatProvider provider )
        {
            return Parse( s, NumberStyles.Integer, NumberFormatInfo.GetInstance( provider ) );
        }
    
        [CLSCompliant( false )]
        public static ushort Parse( String          s        ,
                                    NumberStyles    style    ,
                                    IFormatProvider provider )
        {
            NumberFormatInfo.ValidateParseStyleInteger( style );
    
            return Parse( s, style, NumberFormatInfo.GetInstance( provider ) );
        }
    
        private static ushort Parse( String           s     ,
                                     NumberStyles     style ,
                                     NumberFormatInfo info  )
        {
            uint i = 0;
    
            try
            {
                i = Number.ParseUInt32( s, style, info );
            }
            catch(OverflowException e)
            {
#if EXCEPTION_STRINGS
                throw new OverflowException( Environment.GetResourceString( "Overflow_UInt16" ), e );
#else
                throw new OverflowException( null, e);
#endif
            }
    
            if(i > MaxValue)
            {
#if EXCEPTION_STRINGS
                throw new OverflowException( Environment.GetResourceString( "Overflow_UInt16" ) );
#else
                throw new OverflowException();
#endif
            }
    
            return (ushort)i;
        }
    
        [CLSCompliant( false )]
        public static bool TryParse(     String s      ,
                                     out UInt16 result )
        {
            return TryParse( s, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result );
        }
    
        [CLSCompliant( false )]
        public static bool TryParse(     String          s        ,
                                         NumberStyles    style    ,
                                         IFormatProvider provider ,
                                     out UInt16          result   )
        {
            NumberFormatInfo.ValidateParseStyleInteger( style );
    
            return TryParse( s, style, NumberFormatInfo.GetInstance( provider ), out result );
        }
    
        private static bool TryParse(     String           s      ,
                                          NumberStyles     style  ,
                                          NumberFormatInfo info   ,
                                      out UInt16           result )
        {
            result = 0;
    
            UInt32 i;
    
            if(!Number.TryParseUInt32( s, style, info, out i ))
            {
                return false;
            }
    
            if(i > MaxValue)
            {
                return false;
            }
    
            result = (UInt16)i;
            return true;
        }

        #region IConvertible

        public TypeCode GetTypeCode()
        {
            return TypeCode.UInt16;
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
            return m_value;
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
            throw new InvalidCastException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "InvalidCast_FromTo" ), "UInt16", "DateTime" ) );
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
