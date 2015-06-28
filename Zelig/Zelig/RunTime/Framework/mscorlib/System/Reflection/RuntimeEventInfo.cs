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
    internal unsafe sealed class RuntimeEventInfo : EventInfo /*, ISerializable*/
    {
        #region Private Data Members
////    private int               m_token;
////    private EventAttributes   m_flags;
////    private string            m_name;
////    private void*             m_utf8name;
////    private RuntimeTypeCache  m_reflectedTypeCache;
////    private RuntimeMethodInfo m_addMethod;
////    private RuntimeMethodInfo m_removeMethod;
////    private RuntimeMethodInfo m_raiseMethod;
////    private MethodInfo[]      m_otherMethod;
////    private RuntimeType       m_declaringType;
////    private BindingFlags      m_bindingFlags;
        #endregion

        #region Constructor
////    internal RuntimeEventInfo()
////    {
////        // Used for dummy head node during population
////    }
////
////    internal RuntimeEventInfo( int tkEvent, RuntimeType declaredType, RuntimeTypeCache reflectedTypeCache, out bool isPrivate )
////    {
////        ASSERT.PRECONDITION( declaredType != null );
////        ASSERT.PRECONDITION( reflectedTypeCache != null );
////        ASSERT.PRECONDITION( !reflectedTypeCache.IsGlobal );
////
////        MetadataImport scope = declaredType.Module.MetadataImport;
////
////        m_token              = tkEvent;
////        m_reflectedTypeCache = reflectedTypeCache;
////        m_declaringType      = declaredType;
////
////        RuntimeTypeHandle declaredTypeHandle  = declaredType.GetTypeHandleInternal();
////        RuntimeTypeHandle reflectedTypeHandle = reflectedTypeCache.RuntimeTypeHandle;
////        RuntimeMethodInfo dummy;
////
////        scope.GetEventProps( tkEvent, out m_utf8name, out m_flags );
////
////        int cAssociateRecord = scope.GetAssociatesCount( tkEvent );
////
////        AssociateRecord* associateRecord = stackalloc AssociateRecord[cAssociateRecord];
////
////        scope.GetAssociates( tkEvent, associateRecord, cAssociateRecord );
////
////        Associates.AssignAssociates( associateRecord, cAssociateRecord, declaredTypeHandle, reflectedTypeHandle,
////            out m_addMethod, out m_removeMethod, out m_raiseMethod,
////            out dummy, out dummy, out m_otherMethod, out isPrivate, out m_bindingFlags );
////    }
        #endregion

        #region Internal Members
////    [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
////    internal override bool CacheEquals( object o )
////    {
////        RuntimeEventInfo m = o as RuntimeEventInfo;
////
////        if(m == null)
////        {
////            return false;
////        }
////
////        return m.m_token == m_token && m_declaringType.GetTypeHandleInternal().GetModuleHandle().Equals( m.m_declaringType.GetTypeHandleInternal().GetModuleHandle() );
////    }
////
////    internal BindingFlags BindingFlags
////    {
////        get
////        {
////            return m_bindingFlags;
////        }
////    }
        #endregion

        #region Object Overrides
////    public override String ToString()
////    {
////        if(m_addMethod == null || m_addMethod.GetParametersNoCopy().Length == 0)
////        {
////            throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_NoPublicAddMethod" ) );
////        }
////
////        return m_addMethod.GetParametersNoCopy()[0].ParameterType.SigToString() + " " + Name;
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
////            return MemberTypes.Event;
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

        #region ISerializable
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter )]
////    public void GetObjectData( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////
////        MemberInfoSerializationHolder.GetSerializationInfo( info, Name, ReflectedType, null, MemberTypes.Event );
////    }
        #endregion

        #region EventInfo Overrides
////    public override MethodInfo[] GetOtherMethods( bool nonPublic )
////    {
////        ArrayList ret = new ArrayList();
////
////        if(m_otherMethod == null)
////        {
////            return new MethodInfo[0];
////        }
////
////        for(int i = 0; i < m_otherMethod.Length; i++)
////        {
////            if(Associates.IncludeAccessor( (MethodInfo)m_otherMethod[i], nonPublic ))
////            {
////                ret.Add( m_otherMethod[i] );
////            }
////        }
////
////        return ret.ToArray( typeof( MethodInfo ) ) as MethodInfo[];
////    }
////
////    public override MethodInfo GetAddMethod( bool nonPublic )
////    {
////        if(!Associates.IncludeAccessor( m_addMethod, nonPublic ))
////        {
////            return null;
////        }
////
////        return m_addMethod;
////    }
////
////    public override MethodInfo GetRemoveMethod( bool nonPublic )
////    {
////        if(!Associates.IncludeAccessor( m_removeMethod, nonPublic ))
////        {
////            return null;
////        }
////
////        return m_removeMethod;
////    }
////
////    public override MethodInfo GetRaiseMethod( bool nonPublic )
////    {
////        if(!Associates.IncludeAccessor( m_raiseMethod, nonPublic ))
////        {
////            return null;
////        }
////
////        return m_raiseMethod;
////    }
////
////    public override EventAttributes Attributes
////    {
////        get
////        {
////            return m_flags;
////        }
////    }
        #endregion
    }
}
