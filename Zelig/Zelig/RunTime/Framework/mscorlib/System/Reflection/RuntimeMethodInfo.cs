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
    using System.Threading;
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
    internal sealed class RuntimeMethodInfo : MethodInfo /*, ISerializable*/
    {
        #region Static Members
////    internal static string ConstructParameters( ParameterInfo[] parameters, CallingConventions callingConvention )
////    {
////        Type[] parameterTypes = new Type[parameters.Length];
////
////        for(int i = 0; i < parameters.Length; i++)
////        {
////            parameterTypes[i] = parameters[i].ParameterType;
////        }
////
////        return ConstructParameters( parameterTypes, callingConvention );
////    }
////
////    internal static string ConstructParameters( Type[] parameters, CallingConventions callingConvention )
////    {
////        string toString = "";
////        string comma    = "";
////
////        for(int i = 0; i < parameters.Length; i++)
////        {
////            Type t = parameters[i];
////
////            toString += comma;
////            toString += t.SigToString();
////            if(t.IsByRef)
////            {
////                toString = toString.TrimEnd( new char[] { '&' } );
////                toString += " ByRef";
////            }
////
////            comma = ", ";
////        }
////
////        if((callingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
////        {
////            toString += comma;
////            toString += "...";
////        }
////
////        return toString;
////    }
////
////    internal static string ConstructName( MethodBase mi )
////    {
////        // Serialization uses ToString to resolve MethodInfo overloads.
////        string toString = null;
////
////        toString += mi.Name;
////
////        RuntimeMethodInfo rmi = mi as RuntimeMethodInfo;
////
////        if(rmi != null && rmi.IsGenericMethod)
////        {
////            toString += rmi.m_handle.ConstructInstantiation();
////        }
////
////        toString += "(" + ConstructParameters( mi.GetParametersNoCopy(), mi.CallingConvention ) + ")";
////
////        return toString;
////    }
        #endregion

        #region Private Data Members
#pragma warning disable 169 // The private field 'class member' is never used
        private RuntimeMethodHandle m_handle;
#pragma warning restore 169
////    private RuntimeTypeCache    m_reflectedTypeCache;
////    private string              m_name;
////    private string              m_toString;
////    private ParameterInfo[]     m_parameters;
////    private ParameterInfo       m_returnParameter;
////    private BindingFlags        m_bindingFlags;
////    private MethodAttributes    m_methodAttributes;
////    private Signature           m_signature;
////    private RuntimeType         m_declaringType;
////    private uint                m_invocationFlags;
        #endregion

        #region Constructor
        internal RuntimeMethodInfo()
        {
            // Used for dummy head node during population
        }

////    internal RuntimeMethodInfo( RuntimeMethodHandle handle             ,
////                                RuntimeTypeHandle   declaringTypeHandle,
////                                RuntimeTypeCache    reflectedTypeCache ,
////                                MethodAttributes    methodAttributes   ,
////                                BindingFlags        bindingFlags       )
////    {
////        ASSERT.PRECONDITION( !handle.IsNullHandle() );
////        ASSERT.PRECONDITION( methodAttributes == handle.GetAttributes() );
////
////        m_toString           = null;
////        m_bindingFlags       = bindingFlags;
////        m_handle             = handle;
////        m_reflectedTypeCache = reflectedTypeCache;
////        m_parameters         = null; // Created lazily when GetParameters() is called.
////        m_methodAttributes   = methodAttributes;
////        m_declaringType      = declaringTypeHandle.GetRuntimeType();
////
////        ASSERT.POSTCONDITION( !m_handle.IsNullHandle() );
////    }
        #endregion

        #region Private Methods
////    private RuntimeTypeHandle ReflectedTypeHandle
////    {
////        get
////        {
////            return m_reflectedTypeCache.RuntimeTypeHandle;
////        }
////    }
////
////    internal ParameterInfo[] FetchNonReturnParameters()
////    {
////        if(m_parameters == null)
////        {
////            m_parameters = ParameterInfo.GetParameters( this, this, Signature );
////        }
////
////        return m_parameters;
////    }
////
////    internal ParameterInfo FetchReturnParameter()
////    {
////        if(m_returnParameter == null)
////        {
////            m_returnParameter = ParameterInfo.GetReturnParameter( this, this, Signature );
////        }
////
////        return m_returnParameter;
////    }
////
        #endregion

        #region Internal Members
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    internal override bool CacheEquals( object o )
////    {
////        RuntimeMethodInfo m = o as RuntimeMethodInfo;
////
////        if(m == null)
////        {
////            return false;
////        }
////
////        return m.m_handle.Equals( m_handle );
////    }
////
////    internal Signature Signature
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
////    internal override MethodInfo GetParentDefinition()
////    {
////        if(!IsVirtual || m_declaringType.IsInterface)
////        {
////            return null;
////        }
////
////        Type parent = m_declaringType.BaseType;
////
////        if(parent == null)
////        {
////            return null;
////        }
////
////        int slot = m_handle.GetSlot();
////
////        if(parent.GetTypeHandleInternal().GetNumVtableSlots() <= slot)
////        {
////            return null;
////        }
////
////        return (MethodInfo)RuntimeType.GetMethodBase( parent.GetTypeHandleInternal(), parent.GetTypeHandleInternal().GetMethodAt( slot ) );
////    }
////
////    internal override uint GetOneTimeFlags()
////    {
////        uint invocationFlags = 0;
////
////        if(ReturnType.IsByRef)
////        {
////            invocationFlags = INVOCATION_FLAGS_NO_INVOKE;
////        }
////
////        invocationFlags |= base.GetOneTimeFlags();
////
////        return invocationFlags;
////    }
        #endregion

        #region Object Overrides
////    public override String ToString()
////    {
////        if(m_toString == null)
////        {
////            m_toString = ReturnType.SigToString() + " " + ConstructName( this );
////        }
////
////        return m_toString;
////    }
////
////    public override int GetHashCode()
////    {
////        return m_handle.GetHashCode();
////    }
////
////    public override bool Equals( object obj )
////    {
////        if(!IsGenericMethod)
////        {
////            return obj == this;
////        }
////
////        RuntimeMethodInfo mi = obj as RuntimeMethodInfo;
////
////        RuntimeMethodHandle handle1 =    GetMethodHandle().StripMethodInstantiation();
////        RuntimeMethodHandle handle2 = mi.GetMethodHandle().StripMethodInstantiation();
////        if(handle1 != handle2)
////        {
////            return false;
////        }
////
////        if(mi == null || !mi.IsGenericMethod)
////        {
////            return false;
////        }
////
////        Type[] lhs =    GetGenericArguments();
////        Type[] rhs = mi.GetGenericArguments();
////
////        if(lhs.Length != rhs.Length)
////        {
////            return false;
////        }
////
////        for(int i = 0; i < lhs.Length; i++)
////        {
////            if(lhs[i] != rhs[i])
////            {
////                return false;
////            }
////        }
////
////        return true;
////    }
        #endregion

        #region ICustomAttributeProvider
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern override Object[] GetCustomAttributes( bool inherit );
////    {
////        return CustomAttribute.GetCustomAttributes( this, typeof( object ) as RuntimeType as RuntimeType, inherit );
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern override Object[] GetCustomAttributes( Type attributeType, bool inherit );
////    {
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
////        return CustomAttribute.GetCustomAttributes( this, attributeRuntimeType, inherit );
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern override bool IsDefined( Type attributeType, bool inherit );
////    {
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
////        return CustomAttribute.IsDefined( this, attributeRuntimeType, inherit );
////    }
        #endregion

        #region MemberInfo Overrides
        public override String Name
        {
////        [MethodImpl( MethodImplOptions.InternalCall )]
            get
            {
                throw new NotImplementedException();
////            if(m_name == null)
////            {
////                m_name = m_handle.GetName();
////            }
////
////            return m_name;
            }
        }
    
        public override Type DeclaringType
        {
            get
            {
                throw new NotImplementedException();
////            if(m_reflectedTypeCache.IsGlobal)
////            {
////                return null;
////            }
////
////            return m_declaringType;
            }
        }
    
////    public override Type ReflectedType
////    {
////        get
////        {
////            if(m_reflectedTypeCache.IsGlobal)
////            {
////                return null;
////            }
////
////            return m_reflectedTypeCache.RuntimeType;
////        }
////    }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Method;
            }
        }

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
////            return m_declaringType.Module;
////        }
////    }
        #endregion

        #region MethodBase Overrides
////    internal override ParameterInfo[] GetParametersNoCopy()
////    {
////        FetchNonReturnParameters();
////
////        return m_parameters;
////    }
    
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern override ParameterInfo[] GetParameters();
////    {
////        FetchNonReturnParameters();
////
////        if(m_parameters.Length == 0)
////        {
////            return m_parameters;
////        }
////
////        ParameterInfo[] ret = new ParameterInfo[m_parameters.Length];
////
////        Array.Copy( m_parameters, ret, m_parameters.Length );
////
////        return ret;
////    }
////
////    public override MethodImplAttributes GetMethodImplementationFlags()
////    {
////        return m_handle.GetImplAttributes();
////    }
////
////    internal override bool IsOverloaded
////    {
////        get
////        {
////            return m_reflectedTypeCache.GetMethodList( MemberListType.CaseSensitive, Name ).Count > 1;
////        }
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
////    [ReflectionPermissionAttribute( SecurityAction.Demand, Flags = ReflectionPermissionFlag.MemberAccess )]
////    public override MethodBody GetMethodBody()
////    {
////        MethodBody mb = m_handle.GetMethodBody( ReflectedTypeHandle );
////        if(mb != null)
////        {
////            mb.m_methodBase = this;
////        }
////
////        return mb;
////    }
        #endregion

        #region Invocation Logic(On MemberBase)
////    private void CheckConsistency( Object target )
////    {
////        // only test instance methods
////        if((m_methodAttributes & MethodAttributes.Static) != MethodAttributes.Static)
////        {
////            if(!m_declaringType.IsInstanceOfType( target ))
////            {
////                if(target == null)
////                {
////                    throw new TargetException( Environment.GetResourceString( "RFLCT.Targ_StatMethReqTarg" ) );
////                }
////                else
////                {
////                    throw new TargetException( Environment.GetResourceString( "RFLCT.Targ_ITargMismatch" ) );
////                }
////            }
////        }
////    }
////
////    private void ThrowNoInvokeException()
////    {
////        // method is ReflectionOnly
////        Type declaringType = DeclaringType;
////        if((declaringType == null && Module.Assembly.ReflectionOnly) || declaringType is ReflectionOnlyType)
////        {
////            throw new InvalidOperationException( Environment.GetResourceString( "Arg_ReflectionOnlyInvoke" ) );
////        }
////
////        // method is on a class that contains stack pointers
////        if(DeclaringType.GetRootElementType() == typeof( ArgIterator ))
////        {
////            throw new NotSupportedException();
////        }
////        // method is vararg
////        else if((CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
////        {
////            throw new NotSupportedException();
////        }
////        // method is generic or on a generic class
////        else if(DeclaringType.ContainsGenericParameters || ContainsGenericParameters)
////        {
////            throw new InvalidOperationException( Environment.GetResourceString( "Arg_UnboundGenParam" ) );
////        }
////        // method is abstract class
////        else if(IsAbstract)
////        {
////            throw new MemberAccessException();
////        }
////        // ByRef return are not allowed in reflection
////        else if(ReturnType.IsByRef)
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ByRefReturn" ) );
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
////        return Invoke( obj, invokeAttr, binder, parameters, culture, false );
////    }
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    internal object Invoke( Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture, bool skipVisibilityChecks )
////    {
////        // get the signature
////        int formalCount = Signature.Arguments.Length;
////        int actualCount = (parameters != null) ? parameters.Length : 0;
////
////        // set one time info for invocation
////        if((m_invocationFlags & INVOCATION_FLAGS_INITIALIZED) == 0)
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
////        if(formalCount != actualCount)
////        {
////            throw new TargetParameterCountException( Environment.GetResourceString( "Arg_ParmCnt" ) );
////        }
////
////        // Don't allow more than 65535 parameters.
////        if(actualCount > UInt16.MaxValue)
////        {
////            throw new TargetParameterCountException( Environment.GetResourceString( "NotSupported_TooManyArgs" ) );
////        }
////
////        if(!skipVisibilityChecks && (m_invocationFlags & (INVOCATION_FLAGS_RISKY_METHOD | INVOCATION_FLAGS_NEED_SECURITY)) != 0)
////        {
////            if((m_invocationFlags & INVOCATION_FLAGS_RISKY_METHOD) != 0)
////            {
////                CodeAccessPermission.DemandInternal( PermissionType.ReflectionMemberAccess );
////            }
////
////            if((m_invocationFlags & INVOCATION_FLAGS_NEED_SECURITY) != 0)
////            {
////                PerformSecurityCheck( obj, m_handle, m_declaringType.TypeHandle.Value, m_invocationFlags );
////            }
////        }
////
////        // if we are here we passed all the previous checks. Time to look at the arguments
////        RuntimeTypeHandle declaringTypeHandle = RuntimeTypeHandle.EmptyHandle;
////        if(!m_reflectedTypeCache.IsGlobal)
////        {
////            declaringTypeHandle = m_declaringType.TypeHandle;
////        }
////
////        if(actualCount == 0)
////        {
////            return m_handle.InvokeMethodFast( obj, null, Signature, m_methodAttributes, declaringTypeHandle );
////        }
////
////        Object[] arguments = CheckArguments( parameters, binder, invokeAttr, culture, Signature );
////
////        Object retValue = m_handle.InvokeMethodFast( obj, arguments, Signature, m_methodAttributes, declaringTypeHandle );
////
////        // copy out. This should be made only if ByRef are present.
////        for(int index = 0; index < actualCount; index++)
////        {
////            parameters[index] = arguments[index];
////        }
////
////        return retValue;
////    }
        #endregion

        #region MethodInfo Overrides
////    public override Type ReturnType
////    {
////        get
////        {
////            return Signature.ReturnTypeHandle.GetRuntimeType();
////        }
////    }
////
////    public override ICustomAttributeProvider ReturnTypeCustomAttributes
////    {
////        get
////        {
////            return ReturnParameter;
////        }
////    }
////
////    public override ParameterInfo ReturnParameter
////    {
////        get
////        {
////            FetchReturnParameter();
////            ASSERT.POSTCONDITION( m_returnParameter != null );
////            return m_returnParameter as ParameterInfo;
////        }
////    }
////
////    public override MethodInfo GetBaseDefinition()
////    {
////        if(!IsVirtual || m_declaringType.IsInterface)
////        {
////            return this;
////        }
////
////        int                 slot             = m_handle.GetSlot();
////        RuntimeTypeHandle   parent           = m_handle.GetDeclaringType();
////        RuntimeMethodHandle baseMethodHandle = RuntimeMethodHandle.EmptyHandle;
////
////        do
////        {
////            int cVtblSlots = parent.GetNumVtableSlots();
////
////            if(cVtblSlots <= slot)
////            {
////                break;
////            }
////
////            baseMethodHandle = parent.GetMethodAt( slot );
////            parent           = baseMethodHandle.GetDeclaringType().GetBaseTypeHandle();
////        } while(!parent.IsNullHandle());
////
////        ASSERT.CONSISTENCY_CHECK( (baseMethodHandle.GetAttributes() & MethodAttributes.Virtual) != 0 );
////
////        return (MethodInfo)RuntimeType.GetMethodBase( baseMethodHandle );
////    }
        #endregion

        #region Generics
////    public override MethodInfo MakeGenericMethod( params Type[] methodInstantiation )
////    {
////        if(methodInstantiation == null)
////        {
////            throw new ArgumentNullException( "methodInstantiation" );
////        }
////
////        Type[] methodInstantiationCopy = new Type[methodInstantiation.Length];
////        for(int i = 0; i < methodInstantiation.Length; i++)
////        {
////            methodInstantiationCopy[i] = methodInstantiation[i];
////        }
////        methodInstantiation = methodInstantiationCopy;
////
////        if(!IsGenericMethodDefinition)
////        {
////            throw new InvalidOperationException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_NotGenericMethodDefinition" ), this ) );
////        }
////
////        for(int i = 0; i < methodInstantiation.Length; i++)
////        {
////            if(methodInstantiation[i] == null)
////            {
////                throw new ArgumentNullException();
////            }
////
////            if(!(methodInstantiation[i] is RuntimeType))
////            {
////                return MethodBuilderInstantiation.MakeGenericMethod( this, methodInstantiation );
////            }
////        }
////
////        Type[] genericParameters = GetGenericArguments();
////
////        RuntimeType.SanityCheckGenericArguments( methodInstantiation, genericParameters );
////
////        RuntimeTypeHandle[] typeHandles = new RuntimeTypeHandle[methodInstantiation.Length];
////
////        for(int i = 0; i < methodInstantiation.Length; i++)
////        {
////            typeHandles[i] = methodInstantiation[i].GetTypeHandleInternal();
////        }
////
////        MethodInfo ret = null;
////
////        try
////        {
////            ret = RuntimeType.GetMethodBase( m_reflectedTypeCache.RuntimeTypeHandle, m_handle.GetInstantiatingStub( m_declaringType.GetTypeHandleInternal(), typeHandles ) ) as MethodInfo;
////        }
////        catch(VerificationException e)
////        {
////            RuntimeType.ValidateGenericArguments( this, methodInstantiation, e );
////            throw e;
////        }
////
////        return ret;
////    }
////
////    public override Type[] GetGenericArguments()
////    {
////        RuntimeType[]       rtypes = null;
////        RuntimeTypeHandle[] types  = m_handle.GetMethodInstantiation();
////
////        if(types != null)
////        {
////            rtypes = new RuntimeType[types.Length];
////
////            for(int i = 0; i < types.Length; i++)
////            {
////                rtypes[i] = types[i].GetRuntimeType();
////            }
////        }
////        else
////        {
////            rtypes = new RuntimeType[0];
////        }
////
////        return rtypes;
////    }
////
////    public override MethodInfo GetGenericMethodDefinition()
////    {
////        if(!IsGenericMethod)
////        {
////            throw new InvalidOperationException();
////        }
////
////        return RuntimeType.GetMethodBase( m_declaringType.GetTypeHandleInternal(), m_handle.StripMethodInstantiation() ) as MethodInfo;
////    }
////
////    public override bool IsGenericMethod
////    {
////        get
////        {
////            return m_handle.HasMethodInstantiation();
////        }
////    }
////
////    public override bool IsGenericMethodDefinition
////    {
////        get
////        {
////            return m_handle.IsGenericMethodDefinition();
////        }
////    }
////
////    public override bool ContainsGenericParameters
////    {
////        get
////        {
////            if(DeclaringType != null && DeclaringType.ContainsGenericParameters)
////            {
////                return true;
////            }
////
////            if(!IsGenericMethod)
////            {
////                return false;
////            }
////
////            Type[] pis = GetGenericArguments();
////            for(int i = 0; i < pis.Length; i++)
////            {
////                if(pis[i].ContainsGenericParameters)
////                {
////                    return true;
////                }
////            }
////
////            return false;
////        }
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
////        if(m_reflectedTypeCache.IsGlobal)
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_GlobalMethodSerialization" ) );
////        }
////
////        MemberInfoSerializationHolder.GetSerializationInfo(
////                            info, Name, ReflectedTypeHandle.GetRuntimeType(), ToString(), MemberTypes.Method,
////                            IsGenericMethod & !IsGenericMethodDefinition ? GetGenericArguments() : null );
////    }
        #endregion

        #region Legacy Internal
////    internal static MethodBase InternalGetCurrentMethod( ref StackCrawlMark stackMark )
////    {
////        RuntimeMethodHandle method = RuntimeMethodHandle.GetCurrentMethod( ref stackMark );
////
////        if(method.IsNullHandle())
////        {
////            return null;
////        }
////
////        // If C<Foo>.m<Bar> was called, GetCurrentMethod returns C<object>.m<object>. We cannot
////        // get know that the instantiation used Foo or Bar at that point. So the next best thing
////        // is to return C<T>.m<P> and that's what GetTypicalMethodDefinition will do for us.
////        method = method.GetTypicalMethodDefinition();
////
////        return RuntimeType.GetMethodBase( method );
////    }
        #endregion
    }
}
