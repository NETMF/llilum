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
    public abstract class MethodInfo : MethodBase
    {
        #region Constructor
        protected MethodInfo()
        {
        }
        #endregion

        #region MemberInfo Overrides
        public override MemberTypes MemberType
        {
            get
            {
                return System.Reflection.MemberTypes.Method;
            }
        }
        #endregion

        #region Internal Members
        ////    internal virtual MethodInfo GetParentDefinition()
        ////    {
        ////        return null;
        ////    }
        #endregion

        #region Public Abstract\Virtual Members
        ////    public virtual Type ReturnType
        ////    {
        ////        get
        ////        {
        ////            return GetReturnType();
        ////        }
        ////    }
        ////
        ////    internal override Type GetReturnType()
        ////    {
        ////        return ReturnType;
        ////    }
        ////
        ////    public virtual ParameterInfo ReturnParameter
        ////    {
        ////        get
        ////        {
        ////            throw new NotImplementedException();
        ////        }
        ////    }
        ////
        ////    public abstract ICustomAttributeProvider ReturnTypeCustomAttributes
        ////    {
        ////        get;
        ////    }
        ////
        ////    public abstract MethodInfo GetBaseDefinition();

        public override Type[ ] GetGenericArguments( )
        {
            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SubclassOverride" ) );
        }

        ////    public virtual MethodInfo GetGenericMethodDefinition()
        ////    {
        ////        throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SubclassOverride" ) );
        ////    }
        ////
        ////    public override bool IsGenericMethodDefinition
        ////    {
        ////        get
        ////        {
        ////            return false;
        ////        }
        ////    }
        ////
        ////    public override bool ContainsGenericParameters
        ////    {
        ////        get
        ////        {
        ////            return false;
        ////        }
        ////    }
        ////
        ////    public virtual MethodInfo MakeGenericMethod( params Type[] typeArguments )
        ////    {
        ////        throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SubclassOverride" ) );
        ////    }
        ////
        public override bool IsGenericMethod
        {
            get
            {
                return false;
            }
        }
        #endregion
    }
}
