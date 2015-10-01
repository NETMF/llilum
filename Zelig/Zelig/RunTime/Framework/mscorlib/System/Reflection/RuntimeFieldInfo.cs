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
    internal abstract class RuntimeFieldInfo : FieldInfo
    {
        #region Private Data Members
////    private   BindingFlags     m_bindingFlags;
////    protected RuntimeTypeCache m_reflectedTypeCache;
////    protected RuntimeType      m_declaringType;
        #endregion

        #region Constructor
////    protected RuntimeFieldInfo()
////    {
////        // Used for dummy head node during population
////    }
////
////    protected RuntimeFieldInfo( RuntimeTypeCache reflectedTypeCache, RuntimeType declaringType, BindingFlags bindingFlags )
////    {
////        m_bindingFlags       = bindingFlags;
////        m_declaringType      = declaringType;
////        m_reflectedTypeCache = reflectedTypeCache;
////    }
        #endregion

        #region NonPublic Members

////    internal BindingFlags BindingFlags
////    {
////        get
////        {
////            return m_bindingFlags;
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
////    internal RuntimeTypeHandle DeclaringTypeHandle
////    {
////        get
////        {
////            Type declaringType = DeclaringType;
////
////            if(declaringType == null)
////            {
////                return Module.ModuleHandle.GetModuleTypeHandle();
////            }
////
////            return declaringType.GetTypeHandleInternal();
////        }
////    }
////
////    internal virtual RuntimeFieldHandle GetFieldHandle()
////    {
////        return FieldHandle;
////    }

        #endregion

        #region MemberInfo Overrides
////    public override MemberTypes MemberType
////    {
////        get
////        {
////            return MemberTypes.Field;
////        }
////    }
////
////    public override Type ReflectedType
////    {
////        get
////        {
////            return m_reflectedTypeCache.IsGlobal ? null : m_reflectedTypeCache.RuntimeType;
////        }
////    }
////
////    public override Type DeclaringType
////    {
////        get
////        {
////            return m_reflectedTypeCache.IsGlobal ? null : m_declaringType;
////        }
////    }
        #endregion

        #region Object Overrides
////    public unsafe override String ToString()
////    {
////        return FieldType.SigToString() + " " + Name;
////    }
        #endregion

        #region ICustomAttributeProvider
////    public override Object[] GetCustomAttributes( bool inherit )
////    {
////        return CustomAttribute.GetCustomAttributes( this, typeof( object ) as RuntimeType );
////    }
////
////    public override Object[] GetCustomAttributes( Type attributeType, bool inherit )
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
////        return CustomAttribute.GetCustomAttributes( this, attributeRuntimeType );
////    }
////
////    public override bool IsDefined( Type attributeType, bool inherit )
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
////        return CustomAttribute.IsDefined( this, attributeRuntimeType );
////    }
        #endregion

        #region FieldInfo Overrides
        // All implemented on derived classes
        #endregion

        #region ISerializable Implementation
////    public void GetObjectData( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////
////        MemberInfoSerializationHolder.GetSerializationInfo( info, this.Name, this.ReflectedType, this.ToString(), MemberTypes.Field );
////    }
        #endregion
    }
}
