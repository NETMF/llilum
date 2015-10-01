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
////using System.Globalization;
////using System.Threading;
////using System.Diagnostics;
////using System.Security.Permissions;
////using System.Collections;
////using System.Security;
////using System.Text;
    using System.Runtime.ConstrainedExecution;
////using System.Runtime.CompilerServices;
////using System.Runtime.InteropServices;
////using System.Runtime.Serialization;
////using System.Runtime.Versioning;
////using RuntimeTypeCache = System.RuntimeType.RuntimeTypeCache;
////using CorElementType   = System.Reflection.CorElementType;
////using MdToken          = System.Reflection.MetadataToken;

    [Serializable]
    internal unsafe sealed class RuntimePropertyInfo : PropertyInfo /*, ISerializable*/
    {
        #region Private Data Members
////    private int                m_token;
////    private string             m_name;
////    private void*              m_utf8name;
////    private PropertyAttributes m_flags;
////    private RuntimeTypeCache   m_reflectedTypeCache;
////    private RuntimeMethodInfo  m_getterMethod;
////    private RuntimeMethodInfo  m_setterMethod;
////    private MethodInfo[]       m_otherMethod;
////    private RuntimeType        m_declaringType;
////    private BindingFlags       m_bindingFlags;
////    private Signature          m_signature;
        #endregion

        #region Constructor
////    internal RuntimePropertyInfo( int tkProperty, RuntimeType declaredType, RuntimeTypeCache reflectedTypeCache, out bool isPrivate )
////    {
////        ASSERT.PRECONDITION( declaredType != null );
////        ASSERT.PRECONDITION( reflectedTypeCache != null );
////        ASSERT.PRECONDITION( !reflectedTypeCache.IsGlobal );
////
////        MetadataImport scope = declaredType.Module.MetadataImport;
////
////        m_token              = tkProperty;
////        m_reflectedTypeCache = reflectedTypeCache;
////        m_declaringType      = declaredType;
////
////        RuntimeTypeHandle declaredTypeHandle  = declaredType.GetTypeHandleInternal();
////        RuntimeTypeHandle reflectedTypeHandle = reflectedTypeCache.RuntimeTypeHandle;
////        RuntimeMethodInfo dummy;
////
////        scope.GetPropertyProps( tkProperty, out m_utf8name, out m_flags, out MetadataArgs.Skip.ConstArray );
////
////        int cAssociateRecord = scope.GetAssociatesCount( tkProperty );
////
////        AssociateRecord* associateRecord = stackalloc AssociateRecord[cAssociateRecord];
////
////        scope.GetAssociates( tkProperty, associateRecord, cAssociateRecord );
////
////        Associates.AssignAssociates( associateRecord, cAssociateRecord, declaredTypeHandle, reflectedTypeHandle,
////            out dummy, out dummy, out dummy,
////            out m_getterMethod, out m_setterMethod, out m_otherMethod,
////            out isPrivate, out m_bindingFlags );
////    }
        #endregion

        #region Internal Members
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    internal override bool CacheEquals( object o )
////    {
////        RuntimePropertyInfo m = o as RuntimePropertyInfo;
////
////        if(m == null)
////        {
////            return false;
////        }
////
////        return m.m_token == m_token && m_declaringType.GetTypeHandleInternal().GetModuleHandle().Equals( m.m_declaringType.GetTypeHandleInternal().GetModuleHandle() );
////    }
////
////    internal Signature Signature
////    {
////        get
////        {
////            if(m_signature == null)
////            {
////                ConstArray sig;
////
////                void* name;
////
////                Module.MetadataImport.GetPropertyProps( m_token, out name, out MetadataArgs.Skip.PropertyAttributes, out sig );
////
////                m_signature = new Signature( sig.Signature.ToPointer(), (int)sig.Length, m_declaringType.GetTypeHandleInternal() );
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
////    internal bool EqualsSig( RuntimePropertyInfo target )
////    {
////        //@Asymmetry - Legacy policy is to remove duplicate properties, including hidden properties.
////        //             The comparison is done by name and by sig. The EqualsSig comparison is expensive
////        //             but forutnetly it is only called when an inherited property is hidden by name or
////        //             when an interfaces declare properies with the same signature.
////
////        ASSERT.PRECONDITION( Name.Equals( target.Name ) );
////        ASSERT.PRECONDITION( this != target );
////        ASSERT.PRECONDITION( this.ReflectedType == target.ReflectedType );
////
////        return Signature.DiffSigs( target.Signature );
////    }
        #endregion

        #region Object Overrides
////    public override String ToString()
////    {
////        string toString = PropertyType.SigToString() + " " + Name;
////
////        RuntimeTypeHandle[] argumentHandles = Signature.Arguments;
////        if(argumentHandles.Length > 0)
////        {
////            Type[] paramters = new Type[argumentHandles.Length];
////            for(int i = 0; i < paramters.Length; i++)
////            {
////                paramters[i] = argumentHandles[i].GetRuntimeType();
////            }
////
////            toString += " [" + RuntimeMethodInfo.ConstructParameters( paramters, Signature.CallingConvention ) + "]";
////        }
////
////        return toString;
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
////    public override MemberTypes MemberType
////    {
////        get
////        {
////            return MemberTypes.Property;
////        }
////    }
    
        public override String Name
        {
            get
            {
                throw new NotImplementedException();
////            if(m_name == null)
////            {
////                m_name = new Utf8String( m_utf8name ).ToString();
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
////            return m_declaringType;
            }
        }
    
////    public override Type ReflectedType
////    {
////        get
////        {
////            return m_reflectedTypeCache.RuntimeType;
////        }
////    }
////
////    public override int MetadataToken
////    {
////        get
////        {
////            return m_token;
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

        #region PropertyInfo Overrides

        #region Non Dynamic

////    public override Type[] GetRequiredCustomModifiers()
////    {
////        return Signature.GetCustomModifiers( 0, true );
////    }
////
////    public override Type[] GetOptionalCustomModifiers()
////    {
////        return Signature.GetCustomModifiers( 0, false );
////    }
////
////    internal object GetConstantValue( bool raw )
////    {
////        Object defaultValue = MdConstant.GetValue( Module.MetadataImport, m_token, PropertyType.GetTypeHandleInternal(), raw );
////
////        if(defaultValue == DBNull.Value)
////        {
////            // Arg_EnumLitValueNotFound -> "Literal value was not found."
////            throw new InvalidOperationException( Environment.GetResourceString( "Arg_EnumLitValueNotFound" ) );
////        }
////
////        return defaultValue;
////    }
////
////    public override object GetConstantValue()
////    {
////        return GetConstantValue( false );
////    }
////
////    public override object GetRawConstantValue()
////    {
////        return GetConstantValue( true );
////    }
////
////    public override MethodInfo[] GetAccessors( bool nonPublic )
////    {
////        ArrayList accessorList = new ArrayList();
////
////        if(Associates.IncludeAccessor( m_getterMethod, nonPublic ))
////        {
////            accessorList.Add( m_getterMethod );
////        }
////
////        if(Associates.IncludeAccessor( m_setterMethod, nonPublic ))
////        {
////            accessorList.Add( m_setterMethod );
////        }
////
////        if(m_otherMethod != null)
////        {
////            for(int i = 0; i < m_otherMethod.Length; i++)
////            {
////                if(Associates.IncludeAccessor( m_otherMethod[i] as MethodInfo, nonPublic ))
////                {
////                    accessorList.Add( m_otherMethod[i] );
////                }
////            }
////        }
////
////        return accessorList.ToArray( typeof( MethodInfo ) ) as MethodInfo[];
////    }
////
////    public override Type PropertyType
////    {
////        get
////        {
////            return Signature.ReturnTypeHandle.GetRuntimeType();
////        }
////    }
////
////    public override MethodInfo GetGetMethod( bool nonPublic )
////    {
////        if(!Associates.IncludeAccessor( m_getterMethod, nonPublic ))
////        {
////            return null;
////        }
////
////        return m_getterMethod;
////    }
////
////    public override MethodInfo GetSetMethod( bool nonPublic )
////    {
////        if(!Associates.IncludeAccessor( m_setterMethod, nonPublic ))
////        {
////            return null;
////        }
////
////        return m_setterMethod;
////    }
////
////    public override ParameterInfo[] GetIndexParameters()
////    {
////        // @History - Logic ported from RTM
////
////        int             numParams  = 0;
////        ParameterInfo[] methParams = null;
////
////        // First try to get the Get method.
////        MethodInfo m = GetGetMethod( true );
////        if(m != null)
////        {
////            // There is a Get method so use it.
////            methParams = m.GetParametersNoCopy();
////            numParams  = methParams.Length;
////        }
////        else
////        {
////            // If there is no Get method then use the Set method.
////            m = GetSetMethod( true );
////
////            if(m != null)
////            {
////                methParams = m.GetParametersNoCopy();
////                numParams  = methParams.Length - 1;
////            }
////        }
////
////        // Now copy over the parameter info's and change their
////        // owning member info to the current property info.
////
////        if(methParams != null && methParams.Length == 0)
////        {
////            return methParams;
////        }
////
////        ParameterInfo[] propParams = new ParameterInfo[numParams];
////
////        for(int i = 0; i < numParams; i++)
////        {
////            propParams[i] = new ParameterInfo( methParams[i], this );
////        }
////
////        return propParams;
////    }
////
////    public override PropertyAttributes Attributes
////    {
////        get
////        {
////            return m_flags;
////        }
////    }
////
////    public override bool CanRead
////    {
////        get
////        {
////            return m_getterMethod != null;
////        }
////    }
////
////    public override bool CanWrite
////    {
////        get
////        {
////            return m_setterMethod != null;
////        }
////    }
        #endregion

        #region Dynamic
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    public override Object GetValue( Object obj, Object[] index )
////    {
////        return GetValue( obj, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, index, null );
////    }
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    public override Object GetValue( Object obj, BindingFlags invokeAttr, Binder binder, Object[] index, CultureInfo culture )
////    {
////        MethodInfo m = GetGetMethod( true );
////        if(m == null)
////        {
////            throw new ArgumentException( System.Environment.GetResourceString( "Arg_GetMethNotFnd" ) );
////        }
////
////        return m.Invoke( obj, invokeAttr, binder, index, null );
////    }
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    public override void SetValue( Object obj, Object value, Object[] index )
////    {
////        SetValue( obj,
////                value,
////                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
////                null,
////                index,
////                null );
////    }
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    public override void SetValue( Object obj, Object value, BindingFlags invokeAttr, Binder binder, Object[] index, CultureInfo culture )
////    {
////        MethodInfo m = GetSetMethod( true );
////        if(m == null)
////        {
////            throw new ArgumentException( System.Environment.GetResourceString( "Arg_SetMethNotFnd" ) );
////        }
////
////        Object[] args = null;
////
////        if(index != null)
////        {
////            args = new Object[index.Length + 1];
////
////            for(int i = 0; i < index.Length; i++)
////            {
////                args[i] = index[i];
////            }
////
////            args[index.Length] = value;
////        }
////        else
////        {
////            args = new Object[1];
////            args[0] = value;
////        }
////
////        m.Invoke( obj, invokeAttr, binder, args, culture );
////    }
        #endregion

        #endregion

        #region ISerializable Implementation
////    public void GetObjectData( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////
////        MemberInfoSerializationHolder.GetSerializationInfo( info, Name, ReflectedType, ToString(), MemberTypes.Property );
////    }
        #endregion
    }
}
