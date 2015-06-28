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

    [Microsoft.Zelig.Internals.WellKnownType( "System_RuntimeTypeHandle" )]
    [Serializable]
    public unsafe struct RuntimeTypeHandle /*: ISerializable*/
    {
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool IsInstanceOfType( Object o );
////
////    internal unsafe static IntPtr GetTypeHelper( IntPtr th, IntPtr pGenericArgs, int cGenericArgs, IntPtr pModifiers, int cModifiers )
////    {
////        RuntimeTypeHandle typeHandle = new RuntimeTypeHandle( th.ToPointer() );
////
////        Type type = typeHandle.GetRuntimeType();
////        if(type == null)
////        {
////            return th;
////        }
////
////        if(cGenericArgs > 0)
////        {
////            Type[] genericArgs   = new Type[cGenericArgs];
////            void** arGenericArgs = (void**)pGenericArgs.ToPointer();
////
////            for(int i = 0; i < genericArgs.Length; i++)
////            {
////                RuntimeTypeHandle genericArg = new RuntimeTypeHandle( (void*)Marshal.ReadIntPtr( (IntPtr)arGenericArgs, i * sizeof( void* ) ) );
////
////                genericArgs[i] = Type.GetTypeFromHandle( genericArg );
////                if(genericArgs[i] == null)
////                {
////                    return (IntPtr)0;
////                }
////            }
////
////            type = type.MakeGenericType( genericArgs );
////        }
////
////        if(cModifiers > 0)
////        {
////            int* arModifiers = (int*)pModifiers.ToPointer();
////            for(int i = 0; i < cModifiers; i++)
////            {
////                if((CorElementType)Marshal.ReadInt32( (IntPtr)arModifiers, i * sizeof( int ) ) == CorElementType.Ptr)
////                {
////                    type = type.MakePointerType();
////                }
////                else if((CorElementType)Marshal.ReadInt32( (IntPtr)arModifiers, i * sizeof( int ) ) == CorElementType.ByRef)
////                {
////                    type = type.MakeByRefType();
////                }
////                else if((CorElementType)Marshal.ReadInt32( (IntPtr)arModifiers, i * sizeof( int ) ) == CorElementType.SzArray)
////                {
////                    type = type.MakeArrayType();
////                }
////                else
////                {
////                    type = type.MakeArrayType( Marshal.ReadInt32( (IntPtr)arModifiers, ++i * sizeof( int ) ) );
////                }
////            }
////        }
////
////        return type.GetTypeHandleInternal().Value;
////    }

        public static bool operator ==( RuntimeTypeHandle left, object right )
        {
            return left.Equals( right );
        }

        public static bool operator ==( object left, RuntimeTypeHandle right )
        {
            return right.Equals( left );
        }

        public static bool operator !=( RuntimeTypeHandle left, object right )
        {
            return !left.Equals( right );
        }

        public static bool operator !=( object left, RuntimeTypeHandle right )
        {
            return !right.Equals( left );
        }

        /*
                internal new Type GetType()
                {
                    Console.WriteLine("RuntimeTypeHandle.GetType() called");
                    throw new Exception("RuntimeTypeHandle.GetType() called. Chances are you want to call GetRuntimeType()");
                    return base.GetType();
                }
        */
////    private const int MAX_CLASS_NAME = 1024;
////    internal static RuntimeTypeHandle EmptyHandle
////    {
////        get
////        {
////            return new RuntimeTypeHandle();
////        }
////    }
////
////    // this is the TypeHandle/MethodTable for the type
////#pragma warning disable 649 // Field 'field' is never assigned to, and will always have its default value 'value'
////    private IntPtr m_ptr;
////#pragma warning restore 649

        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern override int GetHashCode();
////    {
////        return m_ptr.GetHashCode();
////    }

////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern override bool Equals( object obj );
////    {
////        if(!(obj is RuntimeTypeHandle))
////        {
////            return false;
////        }
////
////        return Equals( (RuntimeTypeHandle)obj );
////    }

////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern bool Equals( RuntimeTypeHandle handle );
////    {
////        return handle.m_ptr == m_ptr;
////    }

////    public IntPtr Value
////    {
////        get
////        {
////            return m_ptr;
////        }
////    }
////
////    internal RuntimeTypeHandle( void* rth )
////    {
////        m_ptr = new IntPtr( rth );
////    }
////
////    internal RuntimeTypeHandle( IntPtr rth )
////    {
////        m_ptr = rth;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        internal extern bool IsNullHandle();
////    {
////        return m_ptr.ToPointer() == null;
////    }
    
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern Object CreateInstance( RuntimeType type, bool publicOnly, bool noCheck, ref bool canBeCached, ref RuntimeMethodHandle ctor, ref bool bNeedSecurityCheck );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern Object CreateCaInstance( RuntimeMethodHandle ctor );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern Object Allocate();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern Object CreateInstanceForAnotherGenericParameter( Type genericParameter );
////
////    internal RuntimeType GetRuntimeType()
////    {
////        if(!IsNullHandle())
////        {
////            return (System.RuntimeType)Type.GetTypeFromHandle( this );
////        }
////        else
////        {
////            return null;
////        }
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern CorElementType GetCorElementType();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern IntPtr _GetAssemblyHandle();
////    internal AssemblyHandle GetAssemblyHandle()
////    {
////        return new AssemblyHandle( _GetAssemblyHandle() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    private extern IntPtr _GetModuleHandle();
////
////    [CLSCompliant( false )]
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    public ModuleHandle GetModuleHandle()
////    {
////        return new ModuleHandle( _GetModuleHandle() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _GetBaseTypeHandle();
////    internal RuntimeTypeHandle GetBaseTypeHandle()
////    {
////        return new RuntimeTypeHandle( _GetBaseTypeHandle() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern TypeAttributes GetAttributes();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _GetElementType();
////    internal RuntimeTypeHandle GetElementType()
////    {
////        return new RuntimeTypeHandle( _GetElementType() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern RuntimeTypeHandle GetCanonicalHandle();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern int GetArrayRank();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern int GetToken();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _GetMethodAt( int slot );
////    internal RuntimeMethodHandle GetMethodAt( int slot )
////    {
////        return new RuntimeMethodHandle( _GetMethodAt( slot ) );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _GetMethodDescChunk();
////    internal MethodDescChunkHandle GetMethodDescChunk()
////    {
////        return new MethodDescChunkHandle( _GetMethodDescChunk() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool GetFields( int** result, int* count );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern RuntimeTypeHandle[] GetInterfaces();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern RuntimeTypeHandle[] GetConstraints();
////
////    [ResourceExposure( ResourceScope.AppDomain )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern IntPtr GetGCHandle( GCHandleType type );
////
////    [ResourceExposure( ResourceScope.AppDomain )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern void FreeGCHandle( IntPtr handle );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _GetMethodFromToken( int tkMethodDef );
////    internal RuntimeMethodHandle GetMethodFromToken( int tkMethodDef )
////    {
////        return new RuntimeMethodHandle( _GetMethodFromToken( tkMethodDef ) );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern int GetNumVtableSlots();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern int GetInterfaceMethodSlots();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern int GetFirstSlotForInterface( IntPtr interfaceHandle );
////    internal int GetFirstSlotForInterface( RuntimeTypeHandle interfaceHandle )
////    {
////        return GetFirstSlotForInterface( interfaceHandle.Value );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern int GetInterfaceMethodImplementationSlot( IntPtr interfaceHandle, IntPtr interfaceMethodHandle );
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.AppDomain, ResourceScope.AppDomain )]
////    internal int GetInterfaceMethodImplementationSlot( RuntimeTypeHandle interfaceHandle, RuntimeMethodHandle interfaceMethodHandle )
////    {
////        return GetInterfaceMethodImplementationSlot( interfaceHandle.Value, interfaceMethodHandle.Value );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool IsComObject( bool isGenericCOM );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool IsContextful();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool IsVisible();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool _IsVisibleFromModule( IntPtr module );
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.AppDomain, ResourceScope.AppDomain )]
////    internal bool IsVisibleFromModule( ModuleHandle module )
////    {
////        return _IsVisibleFromModule( (IntPtr)module.Value );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool HasProxyAttribute();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern string ConstructName( bool nameSpace, bool fullInst, bool assembly );
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
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern bool CanCastTo( IntPtr target );
////    internal bool CanCastTo( RuntimeTypeHandle target )
////    {
////        return CanCastTo( target.Value );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern RuntimeTypeHandle GetDeclaringType();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _GetDeclaringMethod();
////    internal RuntimeMethodHandle GetDeclaringMethod()
////    {
////        return new RuntimeMethodHandle( _GetDeclaringMethod() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _GetDefaultConstructor();
////    internal RuntimeMethodHandle GetDefaultConstructor()
////    {
////        return new RuntimeMethodHandle( _GetDefaultConstructor() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool SupportsInterface( object target );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern static void* _GetTypeByName( string name, bool throwOnError, bool ignoreCase, bool reflectionOnly, ref StackCrawlMark stackMark, bool loadTypeFromPartialName );
////    internal static RuntimeTypeHandle GetTypeByName( string name, bool throwOnError, bool ignoreCase, bool reflectionOnly, ref StackCrawlMark stackMark )
////    {
////        if(name == null || name.Length == 0)
////        {
////            if(throwOnError)
////            {
////                throw new TypeLoadException( Environment.GetResourceString( "Arg_TypeLoadNullStr" ) );
////            }
////
////            return new RuntimeTypeHandle();
////        }
////
////        return new RuntimeTypeHandle( _GetTypeByName( name, throwOnError, ignoreCase, reflectionOnly, ref stackMark, false ) );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern static void* _GetTypeByNameUsingCARules( string name, IntPtr scope );
////    internal static Type GetTypeByNameUsingCARules( string name, Module scope )
////    {
////        if(name == null || name.Length == 0)
////        {
////            throw new ArgumentException();
////        }
////
////        return new RuntimeTypeHandle( _GetTypeByNameUsingCARules( name, (IntPtr)scope.GetModuleHandle().Value ) ).GetRuntimeType();
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern RuntimeTypeHandle[] GetInstantiation();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _Instantiate( RuntimeTypeHandle[] inst );
////    internal RuntimeTypeHandle Instantiate( RuntimeTypeHandle[] inst )
////    {
////        return new RuntimeTypeHandle( _Instantiate( inst ) );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _MakeArray( int rank );
////    internal RuntimeTypeHandle MakeArray( int rank )
////    {
////        return new RuntimeTypeHandle( _MakeArray( rank ) );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _MakeSZArray();
////    internal RuntimeTypeHandle MakeSZArray()
////    {
////        return new RuntimeTypeHandle( _MakeSZArray() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _MakeByRef();
////    internal RuntimeTypeHandle MakeByRef()
////    {
////        return new RuntimeTypeHandle( _MakeByRef() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _MakePointer();
////    internal RuntimeTypeHandle MakePointer()
////    {
////        return new RuntimeTypeHandle( _MakePointer() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool HasInstantiation();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern void* _GetGenericTypeDefinition();
////    internal RuntimeTypeHandle GetGenericTypeDefinition()
////    {
////        return new RuntimeTypeHandle( _GetGenericTypeDefinition() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool IsGenericTypeDefinition();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool IsGenericVariable();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern int GetGenericVariableIndex();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool ContainsGenericVariables();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern bool SatisfiesConstraints( RuntimeTypeHandle[] typeContext, RuntimeTypeHandle[] methodContext, RuntimeTypeHandle toType );
////
////    private RuntimeTypeHandle( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////
////        Type m = (RuntimeType)info.GetValue( "TypeObj", typeof( RuntimeType ) );
////
////        m_ptr = m.TypeHandle.Value;
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
////        RuntimeType type = (RuntimeType)Type.GetTypeFromHandle( this );
////
////        info.AddValue( "TypeObj", type, typeof( RuntimeType ) );
////    }
    }
}
