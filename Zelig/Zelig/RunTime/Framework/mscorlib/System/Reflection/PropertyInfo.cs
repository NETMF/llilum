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
////using System.Runtime.CompilerServices;
////using System.Runtime.InteropServices;
////using System.Runtime.Serialization;
////using System.Runtime.Versioning;
////using RuntimeTypeCache = System.RuntimeType.RuntimeTypeCache;
////using CorElementType   = System.Reflection.CorElementType;
////using MdToken          = System.Reflection.MetadataToken;

    [Serializable]
////[PermissionSetAttribute( SecurityAction.InheritanceDemand, Name = "FullTrust" )]
    public abstract class PropertyInfo : MemberInfo
    {
        #region Constructor
////    protected PropertyInfo()
////    {
////    }
        #endregion

        #region MemberInfo Overrides
        public override MemberTypes MemberType
        {
            get
            {
                return System.Reflection.MemberTypes.Property;
            }
        }
        #endregion

        #region Public Abstract\Virtual Members
////    public virtual object GetConstantValue()
////    {
////        throw new NotImplementedException();
////    }
////
////    public virtual object GetRawConstantValue()
////    {
////        throw new NotImplementedException();
////    }
////
////    public abstract Type PropertyType
////    {
////        get;
////    }
////
////    public abstract void SetValue( Object obj, Object value, BindingFlags invokeAttr, Binder binder, Object[] index, CultureInfo culture );
////
////    public abstract MethodInfo[] GetAccessors( bool nonPublic );
////
////    public abstract MethodInfo GetGetMethod( bool nonPublic );
////
////    public abstract MethodInfo GetSetMethod( bool nonPublic );
////
////    public abstract ParameterInfo[] GetIndexParameters();
////
////    public abstract PropertyAttributes Attributes
////    {
////        get;
////    }
////
////    public abstract bool CanRead
////    {
////        get;
////    }
////
////    public abstract bool CanWrite
////    {
////        get;
////    }
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    public virtual Object GetValue( Object obj, Object[] index )
////    {
////        return GetValue( obj, BindingFlags.Default, null, index, null );
////    }
////
////    public abstract Object GetValue( Object obj, BindingFlags invokeAttr, Binder binder, Object[] index, CultureInfo culture );
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    public virtual void SetValue( Object obj, Object value, Object[] index )
////    {
////        SetValue( obj, value, BindingFlags.Default, null, index, null );
////    }
        #endregion

        #region Public Members
////    public virtual Type[] GetRequiredCustomModifiers()
////    {
////        return new Type[0];
////    }
////
////    public virtual Type[] GetOptionalCustomModifiers()
////    {
////        return new Type[0];
////    }
////
////    public MethodInfo[] GetAccessors()
////    {
////        return GetAccessors( false );
////    }
////
////    public MethodInfo GetGetMethod()
////    {
////        return GetGetMethod( false );
////    }
////
////    public MethodInfo GetSetMethod()
////    {
////        return GetSetMethod( false );
////    }
////
////    public bool IsSpecialName
////    {
////        get
////        {
////            return (Attributes & PropertyAttributes.SpecialName) != 0;
////        }
////    }
        #endregion
    }
}
