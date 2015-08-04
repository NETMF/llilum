// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  IntPtr
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
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;

    [Microsoft.Zelig.Internals.WellKnownType( "System_IntPtr" )]
    [Serializable]
    public struct IntPtr /*: ISerializable*/
    {
        public static readonly IntPtr Zero;

        unsafe private void* m_value; // The compiler treats void* closest to uint hence explicit casts are required to preserve int behavior

        // fast way to compare IntPtr to (IntPtr)0 while IntPtr.Zero doesn't work due to slow statics access
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        internal unsafe bool IsNull()
        {
            return (this.m_value == null);
        }

////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public unsafe IntPtr( int value )
        {
            m_value = (void *)value;
        }

        public unsafe IntPtr( long value )
        {
                m_value = (void *)checked((int)value);
        }

        [CLSCompliant( false )]
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public unsafe IntPtr( void* value )
        {
            m_value = value;
        }

////    private unsafe IntPtr( SerializationInfo info, StreamingContext context )
////    {
////        long l = info.GetInt64( "value" );
////
////        if((l > Int32.MaxValue || l < Int32.MinValue))
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
////        info.AddValue( "value", (long)((int)m_value) );
////    }

        public unsafe override bool Equals( Object obj )
        {
            if(obj is IntPtr)
            {
                return (m_value == ((IntPtr)obj).m_value);
            }

            return false;
        }

        public unsafe override int GetHashCode()
        {
            return unchecked( (int)((long)m_value) );
        }

////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public unsafe int ToInt32()
        {
            return (int)m_value;
        }

        public unsafe long ToInt64() {
                return (long)(int)m_value;
        }
////    public unsafe override String ToString()
////    {
////        return ((int)m_value).ToString( CultureInfo.InvariantCulture );
////    }
////
////    public unsafe String ToString( String format )
////    {
////        return ((int)m_value).ToString( format, CultureInfo.InvariantCulture );
////    }


////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static explicit operator IntPtr( int value )
        {
            return new IntPtr( value );
        }

        public static explicit operator IntPtr( long value )
        {
            return new IntPtr( value );
        }

        [CLSCompliant( false )]
////    [ReliabilityContract( Consistency.MayCorruptInstance, Cer.MayFail )]
        public static unsafe explicit operator IntPtr( void* value )
        {
            return new IntPtr( value );
        }

        [CLSCompliant( false )]
        public static unsafe explicit operator void*( IntPtr value )
        {
            return value.ToPointer();
        }

        public unsafe static explicit operator int( IntPtr value )
        {
            return (int)value.m_value;
        }

        public unsafe static explicit operator long( IntPtr value )
        {
                return (long)(int)value.m_value;
        }

        ////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public unsafe static bool operator ==( IntPtr value1, IntPtr value2 )
        {
            return value1.m_value == value2.m_value;
        }

////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public unsafe static bool operator !=( IntPtr value1, IntPtr value2 )
        {
            return value1.m_value != value2.m_value;
        }

        public static int Size
        {
////        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
            get
            {
                return 4;
            }
        }


        [CLSCompliant( false )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        public unsafe void* ToPointer()
        {
            return m_value;
        }
    }
}


