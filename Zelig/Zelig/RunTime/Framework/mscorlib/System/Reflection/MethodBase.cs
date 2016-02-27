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
////[PermissionSetAttribute( SecurityAction.InheritanceDemand, Name = "FullTrust" )]
    public abstract class MethodBase : MemberInfo
    {
        #region Static Members
////    public static MethodBase GetMethodFromHandle( RuntimeMethodHandle handle )
////    {
////        if(handle.IsNullHandle())
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidHandle" ) );
////        }
////
////        MethodBase m = RuntimeType.GetMethodBase( handle );
////
////        if(m.DeclaringType != null && m.DeclaringType.IsGenericType)
////        {
////            throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_MethodDeclaringTypeGeneric" ), m, m.DeclaringType.GetGenericTypeDefinition() ) );
////        }
////
////        return m;
////    }
////
////    public static MethodBase GetMethodFromHandle( RuntimeMethodHandle handle, RuntimeTypeHandle declaringType )
////    {
////        if(handle.IsNullHandle())
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidHandle" ) );
////        }
////
////        return RuntimeType.GetMethodBase( declaringType, handle );
////    }
////
////    [DynamicSecurityMethod] // Specify DynamicSecurityMethod attribute to prevent inlining of the caller.
////    public static MethodBase GetCurrentMethod()
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return RuntimeMethodInfo.InternalGetCurrentMethod( ref stackMark );
////    }
        #endregion

        #region Constructor
////    protected MethodBase()
////    {
////    }
        #endregion

        #region Internal Members
////    internal virtual bool IsOverloaded
////    {
////        get
////        {
////            throw new NotSupportedException( Environment.GetResourceString( "InvalidOperation_Method" ) );
////        }
////    }
////
////    internal virtual RuntimeMethodHandle GetMethodHandle()
////    {
////        return MethodHandle;
////    }
        #endregion

        #region Public Abstract\Virtual Members
////    internal virtual Type GetReturnType()
////    {
////        throw new NotImplementedException();
////    }
////
////    internal virtual ParameterInfo[] GetParametersNoCopy()
////    {
////        return GetParameters();
////    }
    
        public abstract ParameterInfo[] GetParameters();
    
////    public abstract MethodImplAttributes GetMethodImplementationFlags();
////
////    public abstract RuntimeMethodHandle MethodHandle
////    {
////        get;
////    }
////
////    public abstract MethodAttributes Attributes
////    {
////        get;
////    }
    
        public abstract Object Invoke( Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture );

        ////    public virtual CallingConventions CallingConvention
        ////    {
        ////        get
        ////        {
        ////            return CallingConventions.Standard;
        ////        }
        ////    }

        public virtual Type[ ] GetGenericArguments( )
        {
            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SubclassOverride" ) );
        }

        ////    public virtual bool IsGenericMethodDefinition
        ////    {
        ////        get
        ////        {
        ////            return false;
        ////        }
        ////    }
        ////
        ////    public virtual bool ContainsGenericParameters
        ////    {
        ////        get
        ////        {
        ////            return false;
        ////        }
        ////    }

        public virtual bool IsGenericMethod
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region Public Members
        ////    [DebuggerStepThroughAttribute]
        [Diagnostics.DebuggerHidden]
        public Object Invoke( Object obj, Object[] parameters )
        {
            return Invoke( obj, BindingFlags.Default, null, parameters, null );
        }
    
////    public bool IsPublic
////    {
////        get
////        {
////            return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
////        }
////    }
////
////    public bool IsPrivate
////    {
////        get
////        {
////            return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private;
////        }
////    }
////
////    public bool IsFamily
////    {
////        get
////        {
////            return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Family;
////        }
////    }
////
////    public bool IsAssembly
////    {
////        get
////        {
////            return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Assembly;
////        }
////    }
////
////    public bool IsFamilyAndAssembly
////    {
////        get
////        {
////            return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamANDAssem;
////        }
////    }
////
////    public bool IsFamilyOrAssembly
////    {
////        get
////        {
////            return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamORAssem;
////        }
////    }
////
////    public bool IsStatic
////    {
////        get
////        {
////            return (Attributes & MethodAttributes.Static) != 0;
////        }
////    }
////
////    public bool IsFinal
////    {
////        get
////        {
////            return (Attributes & MethodAttributes.Final) != 0;
////        }
////    }
////
////    public bool IsVirtual
////    {
////        get
////        {
////            return (Attributes & MethodAttributes.Virtual) != 0;
////        }
////    }
////
////    public bool IsHideBySig
////    {
////        get
////        {
////            return (Attributes & MethodAttributes.HideBySig) != 0;
////        }
////    }
    
        public extern bool IsAbstract
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            return (Attributes & MethodAttributes.Abstract) != 0;
////        }
        }
    
////    public bool IsSpecialName
////    {
////        get
////        {
////            return (Attributes & MethodAttributes.SpecialName) != 0;
////        }
////    }
////
////    public bool IsConstructor
////    {
////        get
////        {
////            return ((Attributes & MethodAttributes.RTSpecialName) != 0) && Name.Equals( ConstructorInfo.ConstructorName );
////        }
////    }
////
////    [ReflectionPermissionAttribute( SecurityAction.Demand, Flags = ReflectionPermissionFlag.MemberAccess )]
////    public virtual MethodBody GetMethodBody()
////    {
////        throw new InvalidOperationException();
////    }
        #endregion

        #region Private Invocation Helpers
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    static extern internal uint GetSpecialSecurityFlags( RuntimeMethodHandle method );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    static extern internal void PerformSecurityCheck( Object obj, RuntimeMethodHandle method, IntPtr parent, uint invocationFlags );
        #endregion

        #region Internal Methods
////    internal virtual Type[] GetParameterTypes()
////    {
////        ParameterInfo[] paramInfo      = GetParametersNoCopy();
////        Type[]          parameterTypes = null;
////
////        parameterTypes = new Type[paramInfo.Length];
////        for(int i = 0; i < paramInfo.Length; i++)
////        {
////            parameterTypes[i] = paramInfo[i].ParameterType;
////        }
////
////        return parameterTypes;
////    }
////
////    virtual internal uint GetOneTimeFlags()
////    {
////        RuntimeMethodHandle handle = MethodHandle;
////
////        uint invocationFlags = 0;
////        Type declaringType   = DeclaringType;
////
////        //
////        // first take care of all the NO_INVOKE cases
////        if(
////            (ContainsGenericParameters) ||
////            (declaringType != null && declaringType.ContainsGenericParameters) ||
////            ((CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs) ||
////            ((Attributes & MethodAttributes.RequireSecObject) == MethodAttributes.RequireSecObject) ||
////            (Module.Assembly.GetType() == typeof( AssemblyBuilder ) && ((AssemblyBuilder)Module.Assembly).m_assemblyData.m_access == AssemblyBuilderAccess.Save)
////           )
////        {
////            invocationFlags |= INVOCATION_FLAGS_NO_INVOKE;
////        }
////        else
////        {
////            //
////            // this should be an invocable method, determine the other flags that participate in invocation
////            invocationFlags |= MethodBase.GetSpecialSecurityFlags( handle );
////
////            if((invocationFlags & INVOCATION_FLAGS_NEED_SECURITY) == 0)
////            {
////                // determine whether the method needs security
////                if(
////                    ((Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public) ||
////                    (declaringType != null && !declaringType.IsVisible)
////                   )
////                {
////                    invocationFlags |= INVOCATION_FLAGS_NEED_SECURITY;
////                }
////                else if(IsGenericMethod)
////                {
////                    Type[] genericArguments = GetGenericArguments();
////
////                    for(int i = 0; i < genericArguments.Length; i++)
////                    {
////                        if(!genericArguments[i].IsVisible)
////                        {
////                            invocationFlags |= INVOCATION_FLAGS_NEED_SECURITY;
////                            break;
////                        }
////                    }
////                }
////            }
////
////        }
////
////        invocationFlags |= GetOneTimeSpecificFlags();
////
////        invocationFlags |= INVOCATION_FLAGS_INITIALIZED;
////        return invocationFlags;
////    }
////
////    // only ctors have special flags for now
////    internal virtual uint GetOneTimeSpecificFlags()
////    {
////        return 0;
////    }
////
////    internal Object[] CheckArguments( Object[] parameters, Binder binder, BindingFlags invokeAttr, CultureInfo culture, Signature sig )
////    {
////        int      actualCount      = (parameters != null) ? parameters.Length : 0;
////        // copy the arguments in a different array so we detach from any user changes
////        Object[] copyOfParameters = new Object[actualCount];
////
////        ParameterInfo[] p = null;
////        for(int i = 0; i < actualCount; i++)
////        {
////            Object            arg    = parameters[i];
////            RuntimeTypeHandle argRTH = sig.Arguments[i];
////
////            if(arg == Type.Missing)
////            {
////                if(p == null)
////                {
////                    p = GetParametersNoCopy();
////                }
////
////                if(p[i].DefaultValue == System.DBNull.Value)
////                {
////                    throw new ArgumentException( Environment.GetResourceString( "Arg_VarMissNull" ), "parameters" );
////                }
////
////                arg = p[i].DefaultValue;
////            }
////
////            if(argRTH.IsInstanceOfType( arg ))
////            {
////                copyOfParameters[i] = arg;
////            }
////            else
////            {
////                copyOfParameters[i] = argRTH.GetRuntimeType().CheckValue( arg, binder, culture, invokeAttr );
////            }
////        }
////
////        return copyOfParameters;
////    }
        #endregion
    }
}
