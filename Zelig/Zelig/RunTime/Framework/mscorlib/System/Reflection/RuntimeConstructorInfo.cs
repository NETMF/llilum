// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace System.Reflection
{
    using System;
    using System.Reflection;
////using System.Reflection.Cache;
////using System.Reflection.Emit;
    using System.Globalization;
////using System.Threading;
////using System.Diagnostics;
////using System.Security.Permissions;
////using System.Collections;
////using System.Security;
////using System.Text;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.CompilerServices;
////using System.Runtime.InteropServices;
////using System.Runtime.Serialization;
////using System.Runtime.Versioning;
////using RuntimeTypeCache = System.RuntimeType.RuntimeTypeCache;
////using CorElementType   = System.Reflection.CorElementType;
////using MdToken          = System.Reflection.MetadataToken;

    [Serializable]
    internal sealed class RuntimeConstructorInfo : ConstructorInfo /*, ISerializable*/
    {
        #region Private Data Members
#pragma warning disable 169 // The private field 'class member' is never used
        private RuntimeMethodHandle m_handle;
#pragma warning restore 169
////    private RuntimeTypeCache    m_reflectedTypeCache;
////    private RuntimeType         m_declaringType;
////    private string              m_toString;
////    private MethodAttributes    m_methodAttributes;
////    private BindingFlags        m_bindingFlags;
////    private ParameterInfo[]     m_parameters = null; // Created lazily when GetParameters() is called.
////    private uint                m_invocationFlags;
////    private Signature           m_signature;
        #endregion

        #region Constructor
////    internal RuntimeConstructorInfo()
////    {
////        // Used for dummy head node during population
////    }
////
////    internal RuntimeConstructorInfo( RuntimeMethodHandle handle              ,
////                                     RuntimeTypeHandle   declaringTypeHandle ,
////                                     RuntimeTypeCache    reflectedTypeCache  ,
////                                     MethodAttributes    methodAttributes    ,
////                                     BindingFlags        bindingFlags        )
////    {
////        ASSERT.POSTCONDITION( methodAttributes == handle.GetAttributes() );
////
////        m_bindingFlags       = bindingFlags;
////        m_handle             = handle;
////        m_reflectedTypeCache = reflectedTypeCache;
////        m_declaringType      = declaringTypeHandle.GetRuntimeType();
////        m_parameters         = null; // Created lazily when GetParameters() is called.
////        m_toString           = null;
////        m_methodAttributes   = methodAttributes;
////    }
        #endregion

        #region NonPublic Methods
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    internal override bool CacheEquals( object o )
////    {
////        RuntimeConstructorInfo m = o as RuntimeConstructorInfo;
////
////        if(m == null)
////        {
////            return false;
////        }
////
////        return m.m_handle.Equals( m_handle );
////    }
////
////    private Signature Signature
////    {
////        get
////        {
////            if(m_signature == null)
////            {
////                m_signature = new Signature( m_handle, m_declaringType.GetTypeHandleInternal() );
////            }
////
////            return m_signature;
////        }
////    }
////
////    private RuntimeTypeHandle ReflectedTypeHandle
////    {
////        get
////        {
////            return m_reflectedTypeCache.RuntimeTypeHandle;
////        }
////    }
////
////    private void CheckConsistency( Object target )
////    {
////        if(target == null && IsStatic)
////        {
////            return;
////        }
////
////        if(!m_declaringType.IsInstanceOfType( target ))
////        {
////            if(target == null)
////            {
////                throw new TargetException( Environment.GetResourceString( "RFLCT.Targ_StatMethReqTarg" ) );
////            }
////
////            throw new TargetException( Environment.GetResourceString( "RFLCT.Targ_ITargMismatch" ) );
////        }
////    }
////
////    internal BindingFlags BindingFlags
////    {
////        get
////        {
////            return m_bindingFlags;
////        }
////    }
////
////    internal override RuntimeMethodHandle GetMethodHandle()
////    {
////        return m_handle;
////    }
////
////    internal override bool IsOverloaded
////    {
////        get
////        {
////            return m_reflectedTypeCache.GetConstructorList( MemberListType.CaseSensitive, Name ).Count > 1;
////        }
////    }
////
////    internal override uint GetOneTimeSpecificFlags()
////    {
////        uint invocationFlags = INVOCATION_FLAGS_IS_CTOR; // this is a given
////
////        if(
////            (DeclaringType != null && DeclaringType.IsAbstract) ||
////            (IsStatic)
////           )
////        {
////            invocationFlags |= INVOCATION_FLAGS_NO_CTOR_INVOKE;
////        }
////        else if(DeclaringType == typeof( void ))
////        {
////            invocationFlags |= INVOCATION_FLAGS_NO_INVOKE;
////        }
////        // Check for attempt to create a delegate class, we demand unmanaged
////        // code permission for this since it's hard to validate the target address.
////        else if(typeof( Delegate ).IsAssignableFrom( DeclaringType ))
////        {
////            invocationFlags |= INVOCATION_FLAGS_IS_DELEGATE_CTOR;
////        }
////
////        return invocationFlags;
////    }
        #endregion

        #region Object Overrides
////    public override String ToString()
////    {
////        if(m_toString == null)
////        {
////            m_toString = "Void " + RuntimeMethodInfo.ConstructName( this );
////        }
////
////        return m_toString;
////    }
        #endregion

        #region ICustomAttributeProvider
        public override Object[] GetCustomAttributes( bool inherit )
        {
            throw new NotImplementedException();
////        return CustomAttribute.GetCustomAttributes( this, typeof( object ) as RuntimeType );
        }
    
        public override Object[] GetCustomAttributes( Type attributeType, bool inherit )
        {
            throw new NotImplementedException();
////        if(attributeType == null)
////        {
////            throw new ArgumentNullException( "attributeType" );
////        }
////
////        RuntimeType attributeRuntimeType = attributeType.UnderlyingSystemType as RuntimeType;
////
////        if(attributeRuntimeType == null)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "attributeType" );
////        }
////
////        return CustomAttribute.GetCustomAttributes( this, attributeRuntimeType );
        }
    
        public override bool IsDefined( Type attributeType, bool inherit )
        {
            throw new NotImplementedException();
////        if(attributeType == null)
////        {
////            throw new ArgumentNullException( "attributeType" );
////        }
////
////        RuntimeType attributeRuntimeType = attributeType.UnderlyingSystemType as RuntimeType;
////
////        if(attributeRuntimeType == null)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "attributeType" );
////        }
////
////        return CustomAttribute.IsDefined( this, attributeRuntimeType );
        }
        #endregion

        #region MemberInfo Overrides
        public override String Name
        {
////        [MethodImpl( MethodImplOptions.InternalCall )]
            get
            {
                throw new NotImplementedException();
////            return m_handle.GetName();
            }
        }
    
////    public override MemberTypes MemberType
////    {
////        get
////        {
////            return MemberTypes.Constructor;
////        }
////    }
    
        public override Type DeclaringType
        {
            get
            {
                throw new NotImplementedException();
////            return m_reflectedTypeCache.IsGlobal ? null : m_declaringType;
            }
        }
    
////    public override Type ReflectedType
////    {
////        get
////        {
////            return m_reflectedTypeCache.IsGlobal ? null : m_reflectedTypeCache.RuntimeType;
////        }
////    }
////
////    public override int MetadataToken
////    {
////        get
////        {
////            return m_handle.GetMethodDef();
////        }
////    }
////
////    public override Module Module
////    {
////        get
////        {
////            return m_declaringType.GetTypeHandleInternal().GetModuleHandle().GetModule();
////        }
////    }
        #endregion

        #region MethodBase Overrides
////    internal override Type GetReturnType()
////    {
////        return Signature.ReturnTypeHandle.GetRuntimeType();
////    }
////
////    internal override ParameterInfo[] GetParametersNoCopy()
////    {
////        if(m_parameters == null)
////        {
////            m_parameters = ParameterInfo.GetParameters( this, this, Signature );
////        }
////
////        return m_parameters;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern override ParameterInfo[] GetParameters();
////    {
////        ParameterInfo[] parameters = GetParametersNoCopy();
////
////        if(parameters.Length == 0)
////        {
////            return parameters;
////        }
////
////        ParameterInfo[] ret = new ParameterInfo[parameters.Length];
////
////        Array.Copy( parameters, ret, parameters.Length );
////
////        return ret;
////    }
////
////    public override MethodImplAttributes GetMethodImplementationFlags()
////    {
////        return m_handle.GetImplAttributes();
////    }
////
////    public override RuntimeMethodHandle MethodHandle
////    {
////        get
////        {
////            Type declaringType = DeclaringType;
////            if((declaringType == null && Module.Assembly.ReflectionOnly) || declaringType is ReflectionOnlyType)
////            {
////                throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_NotAllowedInReflectionOnly" ) );
////            }
////
////            return m_handle;
////        }
////    }
////
////    public override MethodAttributes Attributes
////    {
////        get
////        {
////            return m_methodAttributes;
////        }
////    }
////
////    public override CallingConventions CallingConvention
////    {
////        get
////        {
////            return Signature.CallingConvention;
////        }
////    }
////
////    internal static void CheckCanCreateInstance( Type declaringType, bool isVarArg )
////    {
////        if(declaringType == null)
////        {
////            throw new ArgumentNullException( "declaringType" );
////        }
////
////        // ctor is ReflectOnly
////        if(declaringType is ReflectionOnlyType)
////        {
////            throw new InvalidOperationException( Environment.GetResourceString( "Arg_ReflectionOnlyInvoke" ) );
////        }
////        // ctor is declared on interface class
////        else if(declaringType.IsInterface)
////        {
////            throw new MemberAccessException( String.Format( CultureInfo.CurrentUICulture, Environment.GetResourceString( "Acc_CreateInterfaceEx" ), declaringType ) );
////        }
////        // ctor is on an abstract class
////        else if(declaringType.IsAbstract)
////        {
////            throw new MemberAccessException( String.Format( CultureInfo.CurrentUICulture, Environment.GetResourceString( "Acc_CreateAbstEx" ), declaringType ) );
////        }
////        // ctor is on a class that contains stack pointers
////        else if(declaringType.GetRootElementType() == typeof( ArgIterator ))
////        {
////            throw new NotSupportedException();
////        }
////        // ctor is vararg
////        else if(isVarArg)
////        {
////            throw new NotSupportedException();
////        }
////        // ctor is generic or on a generic class
////        else if(declaringType.ContainsGenericParameters)
////        {
////            throw new MemberAccessException( String.Format( CultureInfo.CurrentUICulture, Environment.GetResourceString( "Acc_CreateGenericEx" ), declaringType ) );
////        }
////        // ctor is declared on System.Void
////        else if(declaringType == typeof( void ))
////        {
////            throw new MemberAccessException( Environment.GetResourceString( "Access_Void" ) );
////        }
////    }
////
////    internal void ThrowNoInvokeException()
////    {
////        CheckCanCreateInstance( DeclaringType, (CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs );
////
////        // ctor is .cctor
////        if((Attributes & MethodAttributes.Static) == MethodAttributes.Static)
////        {
////            throw new MemberAccessException( Environment.GetResourceString( "Acc_NotClassInit" ) );
////        }
////
////        throw new TargetException();
////    }
////
////    [DebuggerStepThroughAttribute]
        [Diagnostics.DebuggerHidden]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern override Object Invoke( Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture );
////    {
////        // set one time info for invocation
////        if(m_invocationFlags == INVOCATION_FLAGS_UNKNOWN)
////        {
////            m_invocationFlags = GetOneTimeFlags();
////        }
////
////        if((m_invocationFlags & INVOCATION_FLAGS_NO_INVOKE) != 0)
////        {
////            ThrowNoInvokeException();
////        }
////
////        // check basic method consistency. This call will throw if there are problems in the target/method relationship
////        CheckConsistency( obj );
////
////        if(obj != null)
////        {
////            new SecurityPermission( SecurityPermissionFlag.SkipVerification ).Demand();
////        }
////
////        if((m_invocationFlags & (INVOCATION_FLAGS_RISKY_METHOD | INVOCATION_FLAGS_NEED_SECURITY)) != 0)
////        {
////            if((m_invocationFlags & INVOCATION_FLAGS_RISKY_METHOD) != 0)
////            {
////                CodeAccessPermission.DemandInternal( PermissionType.ReflectionMemberAccess );
////            }
////            if((m_invocationFlags & INVOCATION_FLAGS_NEED_SECURITY) != 0)
////            {
////                PerformSecurityCheck( obj, m_handle, m_declaringType.TypeHandle.Value, m_invocationFlags );
////            }
////        }
////
////        // get the signature
////        int formalCount = Signature.Arguments.Length;
////        int actualCount = (parameters != null) ? parameters.Length : 0;
////        if(formalCount != actualCount)
////        {
////            throw new TargetParameterCountException( Environment.GetResourceString( "Arg_ParmCnt" ) );
////        }
////
////        // if we are here we passed all the previous checks. Time to look at the arguments
////        if(actualCount > 0)
////        {
////            Object[] arguments = CheckArguments( parameters, binder, invokeAttr, culture, Signature );
////            Object   retValue = m_handle.InvokeMethodFast( obj, arguments, Signature, m_methodAttributes, (ReflectedType != null) ? ReflectedType.TypeHandle : RuntimeTypeHandle.EmptyHandle );
////
////            // copy out. This should be made only if ByRef are present.
////            for(int index = 0; index < actualCount; index++)
////            {
////                parameters[index] = arguments[index];
////            }
////
////            return retValue;
////        }
////
////        return m_handle.InvokeMethodFast( obj, null, Signature, m_methodAttributes, (DeclaringType != null) ? DeclaringType.TypeHandle : RuntimeTypeHandle.EmptyHandle );
////    }
////
////
////    [ReflectionPermissionAttribute( SecurityAction.Demand, Flags = ReflectionPermissionFlag.MemberAccess )]
////    public override MethodBody GetMethodBody()
////    {
////        MethodBody mb = m_handle.GetMethodBody( ReflectedTypeHandle );
////        if(mb != null)
////        {
////            mb.m_methodBase = this;
////        }
////        return mb;
////    }
        #endregion

        #region ConstructorInfo Overrides
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    public override Object Invoke( BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture )
////    {
////        // get the declaring TypeHandle early for consistent exceptions in IntrospectionOnly context
////        RuntimeTypeHandle declaringTypeHandle = m_declaringType.TypeHandle;
////
////        // set one time info for invocation
////        if(m_invocationFlags == INVOCATION_FLAGS_UNKNOWN)
////        {
////            m_invocationFlags = GetOneTimeFlags();
////        }
////
////        if((m_invocationFlags & (INVOCATION_FLAGS_NO_INVOKE | INVOCATION_FLAGS_CONTAINS_STACK_POINTERS | INVOCATION_FLAGS_NO_CTOR_INVOKE)) != 0)
////        {
////            ThrowNoInvokeException();
////        }
////
////        if((m_invocationFlags & (INVOCATION_FLAGS_RISKY_METHOD | INVOCATION_FLAGS_NEED_SECURITY | INVOCATION_FLAGS_IS_DELEGATE_CTOR)) != 0)
////        {
////            if((m_invocationFlags & INVOCATION_FLAGS_RISKY_METHOD) != 0)
////            {
////                CodeAccessPermission.DemandInternal( PermissionType.ReflectionMemberAccess );
////            }
////            if((m_invocationFlags & INVOCATION_FLAGS_NEED_SECURITY) != 0)
////            {
////                PerformSecurityCheck( null, m_handle, m_declaringType.TypeHandle.Value, m_invocationFlags & INVOCATION_FLAGS_CONSTRUCTOR_INVOKE );
////            }
////            if((m_invocationFlags & INVOCATION_FLAGS_IS_DELEGATE_CTOR) != 0)
////            {
////                new SecurityPermission( SecurityPermissionFlag.UnmanagedCode ).Demand();
////            }
////        }
////
////        // get the signature
////        int formalCount = Signature.Arguments.Length;
////        int actualCount = (parameters != null) ? parameters.Length : 0;
////        if(formalCount != actualCount)
////        {
////            throw new TargetParameterCountException( Environment.GetResourceString( "Arg_ParmCnt" ) );
////        }
////
////        // make sure the class ctor has been run
////        RuntimeHelpers.RunClassConstructor( declaringTypeHandle );
////
////        // if we are here we passed all the previous checks. Time to look at the arguments
////        if(actualCount > 0)
////        {
////            Object[] arguments = CheckArguments( parameters, binder, invokeAttr, culture, Signature );
////            Object   retValue  = m_handle.InvokeConstructor( arguments, Signature, declaringTypeHandle );
////
////            // copy out. This should be made only if ByRef are present.
////            for(int index = 0; index < actualCount; index++)
////            {
////                parameters[index] = arguments[index];
////            }
////
////            return retValue;
////        }
////
////        return m_handle.InvokeConstructor( null, Signature, declaringTypeHandle );
////    }
        #endregion

        #region ISerializable Implementation
////    public void GetObjectData( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////
////        MemberInfoSerializationHolder.GetSerializationInfo( info, Name, ReflectedTypeHandle.GetRuntimeType(), ToString(), MemberTypes.Constructor );
////    }
////
////    internal void SerializationInvoke( Object target, SerializationInfo info, StreamingContext context )
////    {
////        MethodHandle.SerializationInvoke( target, Signature, info, context );
////    }
        #endregion
    }
}
