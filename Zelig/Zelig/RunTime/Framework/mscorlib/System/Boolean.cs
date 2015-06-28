// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  Boolean
**
**
** Purpose: The boolean class serves as a wrapper for the primitive
** type boolean.
**
**
===========================================================*/
namespace System
{
    using System;
    using System.Globalization;

    // The Boolean class provides the
    // object representation of the boolean primitive type.
    [Microsoft.Zelig.Internals.WellKnownType( "System_Boolean" )]
    [Serializable]
    public struct Boolean : IComparable, IConvertible, IComparable<Boolean>,  IEquatable<Boolean>
    {
        internal const int True  = 1; // The true value.
        internal const int False = 0; // The false value.

        internal const String TrueLiteral  = "True";  // The internal string representation of true.
        internal const String FalseLiteral = "False"; // The internal string representation of false.

        private static char[] s_trimmableChars;

        //
        // Member Variables
        //
        private bool m_value;

        //
        // Public Constants
        //

        public static readonly String TrueString  = TrueLiteral;  // The public string representation of true.
        public static readonly String FalseString = FalseLiteral; // The public string representation of false.

        //
        // Overriden Instance Methods
        //
        /*=================================GetHashCode==================================
        **Args:  None
        **Returns: 1 or 0 depending on whether this instance represents true or false.
        **Exceptions: None
        **Overriden From: Value
        ==============================================================================*/
        // Provides a hash code for this instance.
        public override int GetHashCode()
        {
            return (m_value) ? True : False;
        }

        /*===================================ToString===================================
        **Args: None
        **Returns:  "True" or "False" depending on the state of the boolean.
        **Exceptions: None.
        ==============================================================================*/
        // Converts the boolean value of this instance to a String.
        public override String ToString()
        {
            if(false == m_value)
            {
                return FalseLiteral;
            }

            return TrueLiteral;
        }

        public String ToString( IFormatProvider provider )
        {
            if(false == m_value)
            {
                return FalseLiteral;
            }
    
            return TrueLiteral;
        }

        // Determines whether two Boolean objects are equal.
        public override bool Equals( Object obj )
        {
            //If it's not a boolean, we're definitely not equal
            if(!(obj is Boolean))
            {
                return false;
            }

            return Equals( (Boolean)obj );
        }

        public bool Equals( Boolean obj )
        {
            return m_value == obj;
        }

        // Compares this object to another object, returning an integer that
        // indicates the relationship. For booleans, false sorts before true.
        // null is considered to be less than any instance.
        // If object is not of type boolean, this method throws an ArgumentException.
        //
        // Returns a value less than zero if this  object
        //
        public int CompareTo( Object obj )
        {
            if(obj == null)
            {
                return 1;
            }

            if(!(obj is Boolean))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeBoolean" ) );
#else
                throw new ArgumentException();
#endif
            }

            return CompareTo( (Boolean)obj );
        }

        public int CompareTo( Boolean value )
        {
            if(m_value == value)
            {
                return 0;
            }
            else if(m_value == false)
            {
                return -1;
            }

            return 1;
        }

        //
        // Static Methods
        //

        // Determines whether a String represents true or false.
        //
        public static Boolean Parse( String value )
        {
            if(value == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "value" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            Boolean result = false;
    
            if(!TryParse( value, out result ))
            {
#if EXCEPTION_STRINGS
                throw new FormatException( Environment.GetResourceString( "Format_BadBoolean" ) );
#else
                throw new FormatException();
#endif
            }
            else
            {
                return result;
            }
        }
    
        // Determines whether a String represents true or false.
        //
        public static Boolean TryParse( String value, out Boolean result )
        {
            result = false;
    
            if(value == null)
            {
                return false;
            }
    
            // For perf reasons, let's first see if they're equal, then do the
            // trim to get rid of white space, and check again.
            if(TrueLiteral.Equals( value, StringComparison.OrdinalIgnoreCase ))
            {
                result = true;
                return true;
            }
    
            if(FalseLiteral.Equals( value, StringComparison.OrdinalIgnoreCase ))
            {
                result = false;
                return true;
            }
    
            // Special case: Trim whitespace as well as null characters.
            // Solution: Lazily initialize a new character array including 0x0000
            if(s_trimmableChars == null)
            {
                char[] m_trimmableCharsTemp = new char[String.WhitespaceChars.Length + 1];
    
                Array.Copy( String.WhitespaceChars, m_trimmableCharsTemp, String.WhitespaceChars.Length );
    
                m_trimmableCharsTemp[m_trimmableCharsTemp.Length - 1] = (char)0x0000;
    
                s_trimmableChars = m_trimmableCharsTemp;
            }
    
            value = value.Trim( s_trimmableChars );  // Remove leading & trailing white space.
    
            if(TrueLiteral.Equals( value, StringComparison.OrdinalIgnoreCase ))
            {
                result = true;
                return true;
            }
    
            if(FalseLiteral.Equals( value, StringComparison.OrdinalIgnoreCase ))
            {
                result = false;
                return true;
            }
    
            return false;
        }

        #region IConvertible

        public TypeCode GetTypeCode()
        {
            return TypeCode.Boolean;
        }
    
    
        /// <internalonly/>
        bool IConvertible.ToBoolean( IFormatProvider provider )
        {
            return m_value;
        }
    
        /// <internalonly/>
        char IConvertible.ToChar( IFormatProvider provider )
        {
#if EXCEPTION_STRINGS
            throw new InvalidCastException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "InvalidCast_FromTo" ), "Boolean", "Char" ) );
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
            throw new InvalidCastException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "InvalidCast_FromTo" ), "Boolean", "DateTime" ) );
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
