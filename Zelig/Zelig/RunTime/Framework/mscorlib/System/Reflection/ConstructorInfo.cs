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
////using System.Runtime.CompilerServices;
////using System.Runtime.InteropServices;
////using System.Runtime.Serialization;
////using System.Runtime.Versioning;
////using RuntimeTypeCache = System.RuntimeType.RuntimeTypeCache;
////using CorElementType   = System.Reflection.CorElementType;
////using MdToken          = System.Reflection.MetadataToken;

    [Serializable]
////[PermissionSetAttribute( SecurityAction.InheritanceDemand, Name = "FullTrust" )]
    public abstract class ConstructorInfo : MethodBase
    {
        #region Static Members
////    public readonly static String ConstructorName = ".ctor";
////
////    public readonly static String TypeConstructorName = ".cctor";
        #endregion

        #region Constructor
        protected ConstructorInfo()
        {
        }
        #endregion

        #region MemberInfo Overrides
        public override MemberTypes MemberType
        {
            get
            {
                return System.Reflection.MemberTypes.Constructor;
            }
        }
        #endregion

        #region Public Abstract\Virtual Members
////    public abstract Object Invoke( BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture );
        #endregion

        #region Public Members
////    internal override Type GetReturnType()
////    {
////        return DeclaringType;
////    }
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    public Object Invoke( Object[] parameters )
////    {
////        return Invoke( BindingFlags.Default, null, parameters, null );
////    }
        #endregion
    }
}
