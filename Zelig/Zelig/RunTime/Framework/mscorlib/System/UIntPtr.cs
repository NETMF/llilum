// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  UIntPtr
**
**
** Purpose: Platform independent integer
**
**
===========================================================*/

namespace System
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;

    [Microsoft.Zelig.Internals.WellKnownType( "System_UIntPtr" )]
    [Serializable]
    [CLSCompliant( false )]
    public struct UIntPtr /*: ISerializable*/
    {
        public static readonly UIntPtr Zero;

        unsafe private void* m_value;


        public unsafe UIntPtr( uint value )
        {
            m_value = (void*)value;
        }

        public unsafe UIntPtr(ulong value)
        {
            m_value = (void*)checked((uint)value);
        }

        [CLSCompliant( false )]
        public unsafe UIntPtr( void* value )
        {
            m_value = value;
        }

////    private unsafe UIntPtr( SerializationInfo info, StreamingContext context )
////    {
////        ulong l = info.GetUInt64( "value" );
////
////        if(l > UInt32.MaxValue)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Serialization_InvalidPtrValue" ) );
////        }
////
////        m_value = (void*)l;
////    }
////
////    unsafe void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////
////        info.AddValue( "value", (ulong)m_value );
////    }

        public unsafe override bool Equals( Object obj )
        {
            if(obj is UIntPtr)
            {
                return (m_value == ((UIntPtr)obj).m_value);
            }

            return false;
        }

        public unsafe override int GetHashCode()
        {
            return unchecked( (int)((long)m_value) ) & 0x7fffffff;
        }

        public unsafe uint ToUInt32()
        {
            return (uint)m_value;
        }

        public unsafe ulong ToUInt64() {
            return (ulong)m_value;
        }
////    public unsafe override String ToString()
////    {
////        return ((uint)m_value).ToString( CultureInfo.InvariantCulture );
////    }

        public static explicit operator UIntPtr( uint value )
        {
            return new UIntPtr( value );
        }

        public static explicit operator UIntPtr (ulong value) 
        {
            return new UIntPtr(value);
        }

        public unsafe static explicit operator uint( UIntPtr value )
        {
            return (uint)value.m_value;
        }   

        public unsafe static explicit operator ulong (UIntPtr  value) 
        {
            return (ulong)value.m_value;
        }

        [CLSCompliant( false )]
        public static unsafe explicit operator UIntPtr( void* value )
        {
            return new UIntPtr( value );
        }

        [CLSCompliant( false )]
        public static unsafe explicit operator void*( UIntPtr value )
        {
            return value.ToPointer();
        }


        public unsafe static bool operator ==( UIntPtr value1, UIntPtr value2 )
        {
            return value1.m_value == value2.m_value;
        }

        public unsafe static bool operator !=( UIntPtr value1, UIntPtr value2 )
        {
            return value1.m_value != value2.m_value;
        }

        public static int Size
        {
            get
            {
                return 4;
            }
        }

        [CLSCompliant( false )]
        public unsafe void* ToPointer()
        {
            return m_value;
        }
    }
}


