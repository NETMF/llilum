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

    [Microsoft.Zelig.Internals.WellKnownType( "System_RuntimeMethodHandle" )]
    [Serializable]
    public unsafe struct RuntimeMethodHandle /*: ISerializable*/
    {
////    internal static RuntimeMethodHandle EmptyHandle
////    {
////        get
////        {
////            return new RuntimeMethodHandle();
////        }
////    }
////
////    private IntPtr m_ptr;
////
////    internal RuntimeMethodHandle( void* pMethod )
////    {
////        m_ptr = new IntPtr( pMethod );
////    }
////
////    private RuntimeMethodHandle()
////    {
////    }
////
////    internal RuntimeMethodHandle( IntPtr pMethod )
////    {
////        m_ptr = pMethod;
////    }
////
////    // ISerializable interface
////    private RuntimeMethodHandle( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////
////        MethodInfo m = (RuntimeMethodInfo)info.GetValue( "MethodObj", typeof( RuntimeMethodInfo ) );
////
////        m_ptr = m.MethodHandle.Value;
////
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
////        RuntimeMethodInfo methodInfo = (RuntimeMethodInfo)RuntimeType.GetMethodBase( this );
////
////        info.AddValue( "MethodObj", methodInfo, typeof( RuntimeMethodInfo ) );
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
        public extern override int GetHashCode();
////    {
////        return m_ptr.GetHashCode();
////    }

////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern override bool Equals( object obj );
////    {
////        if(!(obj is RuntimeMethodHandle))
////        {
////            return false;
////        }
////
////        return Equals( (RuntimeMethodHandle)obj );
////    }

        public static bool operator ==( RuntimeMethodHandle left, RuntimeMethodHandle right )
        {
            return left.Equals( right );
        }

        public static bool operator !=( RuntimeMethodHandle left, RuntimeMethodHandle right )
        {
            return !left.Equals( right );
        }

////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern bool Equals( RuntimeMethodHandle handle );
////    {
////        return handle.m_ptr == m_ptr;
////    }

        [MethodImpl( MethodImplOptions.InternalCall )]
        internal extern bool IsNullHandle();
////    {
////        return m_ptr.ToPointer() == null;
////    }

////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    public extern IntPtr GetFunctionPointer();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private unsafe extern void _CheckLinktimeDemands( void* module, int metadataToken );
////
////    internal void CheckLinktimeDemands( Module module, int metadataToken )
////    {
////        _CheckLinktimeDemands( (void*)module.ModuleHandle.Value, metadataToken );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private unsafe extern bool _IsVisibleFromModule( IntPtr source );
////
////    internal bool IsVisibleFromModule( Module source )
////    {
////        return _IsVisibleFromModule( source.ModuleHandle.Value );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private unsafe extern bool _IsVisibleFromType( IntPtr source );
////
////    internal bool IsVisibleFromType( RuntimeTypeHandle source )
////    {
////        return _IsVisibleFromType( source.Value );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern void* _GetCurrentMethod( ref StackCrawlMark stackMark );
////
////    internal static RuntimeMethodHandle GetCurrentMethod( ref StackCrawlMark stackMark )
////    {
////        return new RuntimeMethodHandle( _GetCurrentMethod( ref stackMark ) );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern MethodAttributes GetAttributes();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern MethodImplAttributes GetImplAttributes();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern string ConstructInstantiation();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern RuntimeTypeHandle GetDeclaringType();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern int GetSlot();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern int GetMethodDef();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern string GetName();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _GetUtf8Name();
////
////    internal Utf8String GetUtf8Name()
////    {
////        return new Utf8String( _GetUtf8Name() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern object _InvokeMethodFast( object target, object[] arguments, ref SignatureStruct sig, MethodAttributes methodAttributes, RuntimeTypeHandle typeOwner );
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    internal object InvokeMethodFast( object target, object[] arguments, Signature sig, MethodAttributes methodAttributes, RuntimeTypeHandle typeOwner )
////    {
////        SignatureStruct _sig = sig.m_signature;
////
////        object obj1 = _InvokeMethodFast( target, arguments, ref _sig, methodAttributes, typeOwner );
////
////        sig.m_signature = _sig;
////
////        return obj1;
////    }
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern object _InvokeConstructor( object[] args, ref SignatureStruct signature, IntPtr declaringType );
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    internal object InvokeConstructor( object[] args, SignatureStruct signature, RuntimeTypeHandle declaringType )
////    {
////        return _InvokeConstructor( args, ref signature, declaringType.Value );
////    }
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void _SerializationInvoke( Object target, ref SignatureStruct declaringTypeSig, SerializationInfo info, StreamingContext context );
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    internal void SerializationInvoke( Object target, SignatureStruct declaringTypeSig, SerializationInfo info, StreamingContext context )
////    {
////        _SerializationInvoke( target, ref declaringTypeSig, info, context );
////    }
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool IsILStub();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern RuntimeTypeHandle[] GetMethodInstantiation();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool HasMethodInstantiation();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern RuntimeMethodHandle GetInstantiatingStub( RuntimeTypeHandle declaringTypeHandle, RuntimeTypeHandle[] methodInstantiation );
////
////    internal RuntimeMethodHandle GetInstantiatingStubIfNeeded( RuntimeTypeHandle declaringTypeHandle )
////    {
////        return GetInstantiatingStub( declaringTypeHandle, null );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern RuntimeMethodHandle GetMethodFromCanonical( RuntimeTypeHandle declaringTypeHandle );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool IsGenericMethodDefinition();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _GetTypicalMethodDefinition();
////
////    internal RuntimeMethodHandle GetTypicalMethodDefinition()
////    {
////        return new RuntimeMethodHandle( _GetTypicalMethodDefinition() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _StripMethodInstantiation();
////
////    internal RuntimeMethodHandle StripMethodInstantiation()
////    {
////        return new RuntimeMethodHandle( _StripMethodInstantiation() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool IsDynamicMethod();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern Resolver GetResolver();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern void Destroy();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern MethodBody _GetMethodBody( IntPtr declaringType );
////
////    internal MethodBody GetMethodBody( RuntimeTypeHandle declaringType )
////    {
////        return _GetMethodBody( declaringType.Value );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool IsConstructor();
    }
}
