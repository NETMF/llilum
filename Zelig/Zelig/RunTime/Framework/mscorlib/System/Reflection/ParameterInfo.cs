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
    using System.Diagnostics;
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
    public unsafe class ParameterInfo /*: ICustomAttributeProvider*/
    {
        #region Static Members
////    internal unsafe static ParameterInfo[] GetParameters( MethodBase method, MemberInfo member, Signature sig )
////    {
////        ParameterInfo dummy;
////
////        return GetParameters( method, member, sig, out dummy, false );
////    }
////
////    internal unsafe static ParameterInfo GetReturnParameter( MethodBase method, MemberInfo member, Signature sig )
////    {
////        ParameterInfo returnParameter;
////
////        GetParameters( method, member, sig, out returnParameter, true );
////
////        return returnParameter;
////    }
////
////    internal unsafe static ParameterInfo[] GetParameters( MethodBase        method               ,
////                                                          MemberInfo        member               ,
////                                                          Signature         sig                  ,
////                                                          out ParameterInfo returnParameter      ,
////                                                          bool              fetchReturnParameter )
////    {
////        returnParameter = null;
////
////        RuntimeMethodHandle methodHandle = method.GetMethodHandle();
////        int                 sigArgCount  = sig.Arguments.Length;
////        ParameterInfo[]     args         = fetchReturnParameter ? null : new ParameterInfo[sigArgCount];
////
////        int tkMethodDef = methodHandle.GetMethodDef();
////        int cParamDefs  = 0;
////
////        // Not all methods have tokens. Arrays, pointers and byRef types do not have tokens as they
////        // are generated on the fly by the runtime.
////        if(!MdToken.IsNullToken( tkMethodDef ))
////        {
////            MetadataImport scope = methodHandle.GetDeclaringType().GetModuleHandle().GetMetadataImport();
////
////            cParamDefs = scope.EnumParamsCount( tkMethodDef );
////
////            int* tkParamDefs = stackalloc int[cParamDefs];
////
////            scope.EnumParams( tkMethodDef, tkParamDefs, cParamDefs );
////
////            // Not all parameters have tokens. Parameters may have no token
////            // if they have no name and no attributes.
////            ASSERT.CONSISTENCY_CHECK( cParamDefs <= sigArgCount + 1 /* return type */);
////
////
////            for(uint i = 0; i < cParamDefs; i++)
////            {
////                #region Populate ParameterInfos
////                ParameterAttributes attr;
////                int                 position;
////                int                 tkParamDef = tkParamDefs[i];
////
////                scope.GetParamDefProps( tkParamDef, out position, out attr );
////
////                position--;
////
////                if(fetchReturnParameter == true && position == -1)
////                {
////                    ASSERT.CONSISTENCY_CHECK( returnParameter == null );
////                    returnParameter = new ParameterInfo( sig, scope, tkParamDef, position, attr, member );
////                }
////                else if(fetchReturnParameter == false && position >= 0)
////                {
////                    ASSERT.CONSISTENCY_CHECK( position < sigArgCount );
////                    args[position] = new ParameterInfo( sig, scope, tkParamDef, position, attr, member );
////                }
////                #endregion
////            }
////        }
////
////        // Fill in empty ParameterInfos for those without tokens
////        if(fetchReturnParameter)
////        {
////            if(returnParameter == null)
////            {
////                returnParameter = new ParameterInfo( sig, MetadataImport.EmptyImport, 0, -1, (ParameterAttributes)0, member );
////            }
////        }
////        else
////        {
////            if(cParamDefs < args.Length + 1)
////            {
////                for(int i = 0; i < args.Length; i++)
////                {
////                    if(args[i] != null)
////                    {
////                        continue;
////                    }
////
////                    args[i] = new ParameterInfo( sig, MetadataImport.EmptyImport, 0, i, (ParameterAttributes)0, member );
////                }
////            }
////        }
////
////        return args;
////    }
        #endregion

        #region Private Statics
////    private static readonly Type s_DecimalConstantAttributeType = typeof( DecimalConstantAttribute );
////    private static readonly Type s_CustomConstantAttributeType  = typeof( CustomConstantAttribute );
////    private static          Type ParameterInfoType              = typeof( System.Reflection.ParameterInfo );
        #endregion

        #region Definitions
////    [Flags]
////    private enum WhatIsCached
////    {
////        Nothing       = 0x0,
////        Name          = 0x1,
////        ParameterType = 0x2,
////        DefaultValue  = 0x4,
////        All           = Name | ParameterType | DefaultValue
////    }
        #endregion

        #region Legacy Protected Members
////    protected String              NameImpl;
////    protected Type                ClassImpl;
////    protected int                 PositionImpl;
////    protected ParameterAttributes AttrsImpl;
////    protected Object              DefaultValueImpl; // cannot cache this as it may be non agile user defined enum
////    protected MemberInfo          MemberImpl;
        #endregion

        #region Legacy Private Members
        // These are here only for backwards compatibility -- they are not set
        // until this instance is serialized, so don't rely on their values from
        // arbitrary code.
#pragma warning disable 414
////    private IntPtr _importer;
////    private int    _token;
////    private bool   bExtraConstChecked;
#pragma warning restore 414
        #endregion

        #region Private Data Members
////    // These are new in Whidbey, so we cannot serialize them directly or we break backwards compatibility.
////    [NonSerialized]
////    private int            m_tkParamDef;
////    [NonSerialized]
////    private MetadataImport m_scope;
////    [NonSerialized]
////    private Signature      m_signature;
////    [NonSerialized]
////    private volatile bool  m_nameIsCached = false;
////    [NonSerialized]
////    private readonly bool  m_noDefaultValue = false;
        #endregion

        #region VTS magic to serialize/deserialized to/from pre-Whidbey endpoints.
////    [OnSerializing]
////    private void OnSerializing( StreamingContext context )
////    {
////        // We could be serializing for consumption by a pre-Whidbey
////        // endpoint. Therefore we set up all the serialized fields to look
////        // just like a v1.0/v1.1 instance.
////
////        // First force all the protected fields (*Impl) which are computed
////        // to be set if they aren't already.
////        Object dummy;
////        dummy = ParameterType;
////        dummy = Name;
////        DefaultValueImpl = DefaultValue;
////
////        // Now set the legacy fields that the current implementation doesn't
////        // use any more. Note that _importer is a raw pointer that should
////        // never have been serialized in V1. We set it to zero here; if the
////        // deserializer uses it (by calling GetCustomAttributes() on this
////        // instance) they'll AV, but at least it will be a well defined
////        // exception and not a random AV.
////        _importer          = IntPtr.Zero;
////        _token             = m_tkParamDef;
////        bExtraConstChecked = false;
////    }
////
////    [OnDeserialized]
////    private void OnDeserialized( StreamingContext context )
////    {
////        // Once all the serializable fields have come in we can setup this
////        // instance based on just two of them (MemberImpl and PositionImpl).
////        // Use these members to lookup a template ParameterInfo then clone
////        // that instance into this one.
////
////        ParameterInfo targetInfo = null;
////
////        if(MemberImpl == null)
////        {
////            throw new SerializationException( Environment.GetResourceString( ResId.Serialization_InsufficientState ) );
////        }
////
////        ParameterInfo[] args = null;
////
////        switch(MemberImpl.MemberType)
////        {
////            case MemberTypes.Constructor:
////            case MemberTypes.Method:
////                if(PositionImpl == -1)
////                {
////                    if(MemberImpl.MemberType == MemberTypes.Method)
////                    {
////                        targetInfo = ((MethodInfo)MemberImpl).ReturnParameter;
////                    }
////                    else
////                    {
////                        throw new SerializationException( Environment.GetResourceString( ResId.Serialization_BadParameterInfo ) );
////                    }
////                }
////                else
////                {
////                    args = ((MethodBase)MemberImpl).GetParametersNoCopy();
////
////                    if(args != null && PositionImpl < args.Length)
////                    {
////                        targetInfo = args[PositionImpl];
////                    }
////                    else
////                    {
////                        throw new SerializationException( Environment.GetResourceString( ResId.Serialization_BadParameterInfo ) );
////                    }
////                }
////                break;
////
////            case MemberTypes.Property:
////                args = ((PropertyInfo)MemberImpl).GetIndexParameters();
////
////                if(args != null && PositionImpl > -1 && PositionImpl < args.Length)
////                {
////                    targetInfo = args[PositionImpl];
////                }
////                else
////                {
////                    throw new SerializationException( Environment.GetResourceString( ResId.Serialization_BadParameterInfo ) );
////                }
////                break;
////
////            default:
////                throw new SerializationException( Environment.GetResourceString( ResId.Serialization_NoParameterInfo ) );
////        }
////
////        // We've got a ParameterInfo that matches the incoming information,
////        // clone it into ourselves. We really only need to copy the private
////        // members we didn't receive via serialization.
////        ASSERT.PRECONDITION( targetInfo != null );
////
////        m_tkParamDef   = targetInfo.m_tkParamDef;
////        m_scope        = targetInfo.m_scope;
////        m_signature    = targetInfo.m_signature;
////        m_nameIsCached = true;
////    }
        #endregion

        #region Constructor
////    protected ParameterInfo()
////    {
////        m_nameIsCached   = true;
////        m_noDefaultValue = true;
////    }
////
////    internal ParameterInfo( ParameterInfo accessor, RuntimePropertyInfo property ) : this( accessor, (MemberInfo)property )
////    {
////        m_signature = property.Signature;
////    }
////
////    internal ParameterInfo( ParameterInfo accessor, MethodBuilderInstantiation method ) : this( accessor, (MemberInfo)method )
////    {
////        m_signature = accessor.m_signature;
////
////        if(ClassImpl.IsGenericParameter)
////        {
////            ClassImpl = method.GetGenericArguments()[ClassImpl.GenericParameterPosition];
////        }
////    }
////
////    private ParameterInfo( ParameterInfo accessor, MemberInfo member )
////    {
////        // Change ownership
////        MemberImpl     = member;
////
////        // Populate all the caches -- we inherit this behavior from RTM
////        NameImpl       = accessor.Name;
////        m_nameIsCached = true;
////        ClassImpl      = accessor.ParameterType;
////        PositionImpl   = accessor.Position;
////        AttrsImpl      = accessor.Attributes;
////
////        // Strictly speeking, property's don't contain paramter tokens
////        // However we need this to make ca's work... oh well...
////        m_tkParamDef   = MdToken.IsNullToken( accessor.MetadataToken ) ? (int)MetadataTokenType.ParamDef : accessor.MetadataToken;
////        m_scope        = accessor.m_scope;
////    }
////
////    private ParameterInfo( Signature           signature ,
////                           MetadataImport      scope     ,
////                           int                 tkParamDef,
////                           int                 position  ,
////                           ParameterAttributes attributes,
////                           MemberInfo          member    )
////    {
////        ASSERT.PRECONDITION( member != null );
////        ASSERT.PRECONDITION( LOGIC.BIJECTION( MdToken.IsNullToken( tkParamDef ), scope.Equals( null ) ) );
////        ASSERT.PRECONDITION( LOGIC.IMPLIES( !MdToken.IsNullToken( tkParamDef ), MdToken.IsTokenOfType( tkParamDef, MetadataTokenType.ParamDef ) ) );
////
////        PositionImpl = position;
////        MemberImpl   = member;
////        m_signature  = signature;
////        m_tkParamDef = MdToken.IsNullToken( tkParamDef ) ? (int)MetadataTokenType.ParamDef : tkParamDef;
////        m_scope      = scope;
////        AttrsImpl    = attributes;
////
////        ClassImpl    = null;
////        NameImpl     = null;
////    }
////
////    // ctor for no metadata MethodInfo
////    internal ParameterInfo( MethodInfo owner, String name, RuntimeType parameterType, int position )
////    {
////        MemberImpl       = owner;
////        NameImpl         = name;
////        m_nameIsCached   = true;
////        m_noDefaultValue = true;
////        ClassImpl        = parameterType;
////        PositionImpl     = position;
////        AttrsImpl        = ParameterAttributes.None;
////        m_tkParamDef     = (int)MetadataTokenType.ParamDef;
////        m_scope          = MetadataImport.EmptyImport;
////    }
        #endregion

        #region Private Members
////    private bool IsLegacyParameterInfo
////    {
////        get
////        {
////            return GetType() != typeof( ParameterInfo );
////        }
////    }
        #endregion

        #region Internal Members
////    // this is an internal api for DynamicMethod. A better solution is to change the relationship
////    // between ParameterInfo and ParameterBuilder so that a ParameterBuilder can be seen as a writer
////    // api over a ParameterInfo. However that is a possible breaking change so it needs to go through some process first
////    internal void SetName( String name )
////    {
////        NameImpl = name;
////    }
////
////    internal void SetAttributes( ParameterAttributes attributes )
////    {
////        AttrsImpl = attributes;
////    }
        #endregion

        #region Public Methods
        public extern virtual Type ParameterType
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            // only instance of ParameterInfo has ClassImpl, all its subclasses don't
////            if(ClassImpl == null && this.GetType() == typeof( ParameterInfo ))
////            {
////                RuntimeTypeHandle parameterTypeHandle;
////                if(PositionImpl == -1)
////                {
////                    parameterTypeHandle = m_signature.ReturnTypeHandle;
////                }
////                else
////                {
////                    parameterTypeHandle = m_signature.Arguments[PositionImpl];
////                }
////
////                ASSERT.CONSISTENCY_CHECK( !parameterTypeHandle.IsNullHandle() );
////                // different thread could only write ClassImpl to the same value, so race is not a problem here
////                ClassImpl = parameterTypeHandle.GetRuntimeType();
////            }
////
////            BCLDebug.Assert( ClassImpl != null || this.GetType() != typeof( ParameterInfo ), "ClassImple should already be initialized for ParameterInfo class" );
////
////            return ClassImpl;
////        }
        }

        public virtual String Name
        {
            get
            {
                //////if(!m_nameIsCached)
                //////{
                //////    if(!MdToken.IsNullToken( m_tkParamDef ))
                //////    {
                //////        string name = m_scope.GetName( m_tkParamDef ).ToString();

                //////        NameImpl = name;
                //////    }

                //////    // other threads could only write it to true, so race is OK
                //////    // this field is volatile, so the write ordering is guaranteed
                //////    m_nameIsCached = true;
                //////}

                //////// name may be null
                //////return NameImpl;
                return String.Empty;
            }
        }

        ////    public virtual Object DefaultValue
        ////    {
        ////        get
        ////        {
        ////            return GetDefaultValue( false );
        ////        }
        ////    }
        ////
        ////    public virtual Object RawDefaultValue
        ////    {
        ////        get
        ////        {
        ////            return GetDefaultValue( true );
        ////        }
        ////    }
        ////
        ////    internal Object GetDefaultValue( bool raw )
        ////    {
        ////        // Cannot cache because default value could be non-agile user defined enumeration.
        ////        object defaultValue = null;
        ////
        ////        // for dynamic method we pretend to have cached the value so we do not go to metadata
        ////        if(!m_noDefaultValue)
        ////        {
        ////            if(ParameterType == typeof( DateTime ))
        ////            {
        ////                if(raw)
        ////                {
        ////                    CustomAttributeTypedArgument value = CustomAttributeData.Filter( CustomAttributeData.GetCustomAttributes( this ), typeof( DateTimeConstantAttribute ), 0 );
        ////
        ////                    if(value.ArgumentType != null)
        ////                    {
        ////                        return new DateTime( (long)value.Value );
        ////                    }
        ////                }
        ////                else
        ////                {
        ////                    object[] dt = GetCustomAttributes( typeof( DateTimeConstantAttribute ), false );
        ////
        ////                    if(dt != null && dt.Length != 0)
        ////                    {
        ////                        return ((DateTimeConstantAttribute)dt[0]).Value;
        ////                    }
        ////                }
        ////            }
        ////
        ////            #region Look for a default value in metadata
        ////            if(!MdToken.IsNullToken( m_tkParamDef ))
        ////            {
        ////                defaultValue = MdConstant.GetValue( m_scope, m_tkParamDef, ParameterType.GetTypeHandleInternal(), raw );
        ////            }
        ////            #endregion
        ////
        ////            if(defaultValue == DBNull.Value)
        ////            {
        ////                #region Look for a default value in the custom attributes
        ////                if(raw)
        ////                {
        ////                    System.Collections.Generic.IList<CustomAttributeData> attrs = CustomAttributeData.GetCustomAttributes( this );
        ////                    CustomAttributeTypedArgument value = CustomAttributeData.Filter( attrs, s_CustomConstantAttributeType, "Value" );
        ////
        ////                    if(value.ArgumentType == null)
        ////                    {
        ////                        value = CustomAttributeData.Filter( attrs, s_DecimalConstantAttributeType, "Value" );
        ////
        ////
        ////                        if(value.ArgumentType == null)
        ////                        {
        ////                            for(int i = 0; i < attrs.Count; i++)
        ////                            {
        ////                                if(attrs[i].Constructor.DeclaringType == s_DecimalConstantAttributeType)
        ////                                {
        ////                                    ParameterInfo[] parameters = attrs[i].Constructor.GetParameters();
        ////
        ////                                    if(parameters.Length != 0)
        ////                                    {
        ////                                        if(parameters[2].ParameterType == typeof( uint ))
        ////                                        {
        ////                                            System.Collections.Generic.IList<CustomAttributeTypedArgument> args = attrs[i].ConstructorArguments;
        ////
        ////                                            int  low   = (int)(UInt32)args[4].Value;
        ////                                            int  mid   = (int)(UInt32)args[3].Value;
        ////                                            int  hi    = (int)(UInt32)args[2].Value;
        ////                                            byte sign  = (byte)args[1].Value;
        ////                                            byte scale = (byte)args[0].Value;
        ////
        ////                                            value = new CustomAttributeTypedArgument( new System.Decimal( low, mid, hi, (sign != 0), scale ) );
        ////                                        }
        ////                                        else
        ////                                        {
        ////                                            System.Collections.Generic.IList<CustomAttributeTypedArgument> args = attrs[i].ConstructorArguments;
        ////
        ////                                            int  low   = (int)args[4].Value;
        ////                                            int  mid   = (int)args[3].Value;
        ////                                            int  hi    = (int)args[2].Value;
        ////                                            byte sign  = (byte)args[1].Value;
        ////                                            byte scale = (byte)args[0].Value;
        ////
        ////                                            value = new CustomAttributeTypedArgument( new System.Decimal( low, mid, hi, (sign != 0), scale ) );
        ////                                        }
        ////                                    }
        ////                                }
        ////                            }
        ////                        }
        ////                    }
        ////
        ////                    if(value.ArgumentType != null)
        ////                    {
        ////                        defaultValue = value.Value;
        ////                    }
        ////                }
        ////                else
        ////                {
        ////                    Object[] CustomAttrs = GetCustomAttributes( s_CustomConstantAttributeType, false );
        ////                    if(CustomAttrs.Length != 0)
        ////                    {
        ////                        defaultValue = ((CustomConstantAttribute)CustomAttrs[0]).Value;
        ////                    }
        ////                    else
        ////                    {
        ////                        CustomAttrs = GetCustomAttributes( s_DecimalConstantAttributeType, false );
        ////                        if(CustomAttrs.Length != 0)
        ////                        {
        ////                            defaultValue = ((DecimalConstantAttribute)CustomAttrs[0]).Value;
        ////                        }
        ////                    }
        ////                }
        ////                #endregion
        ////            }
        ////
        ////            if(defaultValue == DBNull.Value)
        ////            {
        ////                #region Handle case if no default value was found
        ////                if(IsOptional)
        ////                {
        ////                    // If the argument is marked as optional then the default value is Missing.Value.
        ////                    defaultValue = Type.Missing;
        ////                }
        ////                #endregion
        ////            }
        ////        }
        ////
        ////        return defaultValue;
        ////    }
        ////
        ////    public virtual int Position
        ////    {
        ////        get
        ////        {
        ////            return PositionImpl;
        ////        }
        ////    }
        ////
        ////    public virtual ParameterAttributes Attributes
        ////    {
        ////        get
        ////        {
        ////            return AttrsImpl;
        ////        }
        ////    }
        ////
        ////    public virtual MemberInfo Member
        ////    {
        ////        get
        ////        {
        ////            return MemberImpl;
        ////        }
        ////    }
        ////
        ////    public bool IsIn
        ////    {
        ////        get
        ////        {
        ////            return ((Attributes & ParameterAttributes.In) != 0);
        ////        }
        ////    }
        ////
        ////    public bool IsOut
        ////    {
        ////        get
        ////        {
        ////            return ((Attributes & ParameterAttributes.Out) != 0);
        ////        }
        ////    }
        ////
        ////    public bool IsLcid
        ////    {
        ////        get
        ////        {
        ////            return ((Attributes & ParameterAttributes.Lcid) != 0);
        ////        }
        ////    }
        ////
        ////    public bool IsRetval
        ////    {
        ////        get
        ////        {
        ////            return ((Attributes & ParameterAttributes.Retval) != 0);
        ////        }
        ////    }
        ////
        ////    public bool IsOptional
        ////    {
        ////        get
        ////        {
        ////            return ((Attributes & ParameterAttributes.Optional) != 0);
        ////        }
        ////    }
        ////
        ////    public int MetadataToken
        ////    {
        ////        get
        ////        {
        ////            return m_tkParamDef;
        ////        }
        ////    }
        ////
        ////    public virtual Type[] GetRequiredCustomModifiers()
        ////    {
        ////        if(IsLegacyParameterInfo)
        ////        {
        ////            return new Type[0];
        ////        }
        ////
        ////        return m_signature.GetCustomModifiers( PositionImpl + 1, true );
        ////    }
        ////
        ////    public virtual Type[] GetOptionalCustomModifiers()
        ////    {
        ////        if(IsLegacyParameterInfo)
        ////        {
        ////            return new Type[0];
        ////        }
        ////
        ////        return m_signature.GetCustomModifiers( PositionImpl + 1, false );
        ////    }
        ////
        #endregion

        #region Object Overrides
        ////    public override String ToString()
        ////    {
        ////        return ParameterType.SigToString() + " " + Name;
        ////    }
        #endregion

        #region ICustomAttributeProvider
        ////    public virtual Object[] GetCustomAttributes( bool inherit )
        ////    {
        ////        if(IsLegacyParameterInfo)
        ////        {
        ////            return null;
        ////        }
        ////
        ////        if(MdToken.IsNullToken( m_tkParamDef ))
        ////        {
        ////            return new object[0];
        ////        }
        ////
        ////        return CustomAttribute.GetCustomAttributes( this, typeof( object ) as RuntimeType );
        ////    }
        ////
        ////    public virtual Object[] GetCustomAttributes( Type attributeType, bool inherit )
        ////    {
        ////        if(IsLegacyParameterInfo)
        ////        {
        ////            return null;
        ////        }
        ////
        ////        if(attributeType == null)
        ////        {
        ////            throw new ArgumentNullException( "attributeType" );
        ////        }
        ////
        ////        if(MdToken.IsNullToken( m_tkParamDef ))
        ////        {
        ////            return new object[0];
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
        ////    }
        ////
        ////    public virtual bool IsDefined( Type attributeType, bool inherit )
        ////    {
        ////        if(IsLegacyParameterInfo)
        ////        {
        ////            return false;
        ////        }
        ////
        ////        if(attributeType == null)
        ////        {
        ////            throw new ArgumentNullException( "attributeType" );
        ////        }
        ////
        ////        if(MdToken.IsNullToken( m_tkParamDef ))
        ////        {
        ////            return false;
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
        ////    }
        #endregion

        #region Remoting Cache
        ////    private InternalCache m_cachedData;
        ////
        ////    internal InternalCache Cache
        ////    {
        ////        get
        ////        {
        ////            // This grabs an internal copy of m_cachedData and uses
        ////            // that instead of looking at m_cachedData directly because
        ////            // the cache may get cleared asynchronously.  This prevents
        ////            // us from having to take a lock.
        ////            InternalCache cache = m_cachedData;
        ////            if(cache == null)
        ////            {
        ////                cache = new InternalCache( "ParameterInfo" );
        ////                InternalCache ret = Interlocked.CompareExchange( ref m_cachedData, cache, null );
        ////                if(ret != null)
        ////                {
        ////                    cache = ret;
        ////                }
        ////
        ////                GC.ClearCache += new ClearCacheHandler( OnCacheClear );
        ////            }
        ////
        ////            return cache;
        ////        }
        ////    }
        ////
        ////    internal void OnCacheClear( Object sender, ClearCacheEventArgs cacheEventArgs )
        ////    {
        ////        m_cachedData = null;
        ////    }
        #endregion
    }
}
