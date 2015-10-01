// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

#define DEBUG_PTRS

namespace System
{
    using System;
    using System.Reflection;
////using System.Reflection.Emit;
    using System.Runtime.ConstrainedExecution;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
////using System.Runtime.Versioning;
////using System.Text;
    using System.Globalization;
////using System.Security;
////using Microsoft.Win32.SafeHandles;
////using StackCrawlMark = System.Threading.StackCrawlMark;

    [Microsoft.Zelig.Internals.WellKnownType( "System_RuntimeFieldHandle" )]
    [Serializable]
    public unsafe struct RuntimeFieldHandle /*: ISerializable*/
    {
////    private IntPtr m_ptr;
////
////    internal RuntimeFieldHandle( void* pFieldHandle )
////    {
////        m_ptr = new IntPtr( pFieldHandle );
////    }
////
////    public IntPtr Value
////    {
////        get
////        {
////            return m_ptr;
////        }
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal extern bool IsNullHandle();
////    {
////        return m_ptr.ToPointer() == null;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern override int GetHashCode();
////    {
////        return m_ptr.GetHashCode();
////    }
    
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern override bool Equals( object obj );
////    {
////        if(!(obj is RuntimeFieldHandle))
////            return false;
////
////        RuntimeFieldHandle handle = (RuntimeFieldHandle)obj;
////
////        return handle.m_ptr == m_ptr;
////    }
    
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public unsafe extern bool Equals( RuntimeFieldHandle handle );
////    {
////        return handle.m_ptr == m_ptr;
////    }
    
        public static bool operator ==( RuntimeFieldHandle left, RuntimeFieldHandle right )
        {
            return left.Equals( right );
        }
    
        public static bool operator !=( RuntimeFieldHandle left, RuntimeFieldHandle right )
        {
            return !left.Equals( right );
        }
    
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern String GetName();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern unsafe void* _GetUtf8Name();
////
////    internal unsafe Utf8String GetUtf8Name() { return new Utf8String( _GetUtf8Name() ); }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern FieldAttributes GetAttributes();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern RuntimeTypeHandle GetApproxDeclaringType();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern int GetToken();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern Object GetValue( Object instance, RuntimeTypeHandle fieldType, RuntimeTypeHandle declaringType, ref bool domainInitialized );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern Object GetValueDirect( RuntimeTypeHandle fieldType, TypedReference obj, RuntimeTypeHandle contextType );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern void SetValue( Object obj, Object value, RuntimeTypeHandle fieldType, FieldAttributes fieldAttr, RuntimeTypeHandle declaringType, ref bool domainInitialized );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern void SetValueDirect( RuntimeTypeHandle fieldType, TypedReference obj, Object value, RuntimeTypeHandle contextType );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern RuntimeFieldHandle GetStaticFieldForGenericType( RuntimeTypeHandle declaringType );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool AcquiresContextFromThis();
////
////    // ISerializable interface
////    private RuntimeFieldHandle( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////
////        FieldInfo f = (RuntimeFieldInfo)info.GetValue( "FieldObj", typeof( RuntimeFieldInfo ) );
////        if(f == null)
////        {
////            throw new SerializationException( Environment.GetResourceString( "Serialization_InsufficientState" ) );
////        }
////
////        m_ptr = f.FieldHandle.Value;
////        if(m_ptr.ToPointer() == null)
////        {
////            throw new SerializationException( Environment.GetResourceString( "Serialization_InsufficientState" ) );
////        }
////    }
////
////    public void GetObjectData( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////
////        if(m_ptr.ToPointer() == null)
////        {
////            throw new SerializationException( Environment.GetResourceString( "Serialization_InvalidFieldState" ) );
////        }
////
////        RuntimeFieldInfo fldInfo = (RuntimeFieldInfo)RuntimeType.GetFieldInfo( this );
////
////        info.AddValue( "FieldObj", fldInfo, typeof( RuntimeFieldInfo ) );
////    }
    }
}
