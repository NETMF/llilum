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
    public abstract class EventInfo : MemberInfo
    {
        #region Constructor
        protected EventInfo()
        {
        }
        #endregion

        #region MemberInfo Overrides
        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Event;
            }
        }
        #endregion

        #region Public Abstract\Virtual Members
////    public virtual MethodInfo[] GetOtherMethods( bool nonPublic )
////    {
////        throw new NotImplementedException();
////    }
////
////    public abstract MethodInfo GetAddMethod( bool nonPublic );
////
////    public abstract MethodInfo GetRemoveMethod( bool nonPublic );
////
////    public abstract MethodInfo GetRaiseMethod( bool nonPublic );
////
////    public abstract EventAttributes Attributes
////    {
////        get;
////    }
        #endregion

        #region Public Members
////    public MethodInfo[] GetOtherMethods()
////    {
////        return GetOtherMethods( false );
////    }
////
////    public MethodInfo GetAddMethod()
////    {
////        return GetAddMethod( false );
////    }
////
////    public MethodInfo GetRemoveMethod()
////    {
////        return GetRemoveMethod( false );
////    }
////
////    public MethodInfo GetRaiseMethod()
////    {
////        return GetRaiseMethod( false );
////    }
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    public void AddEventHandler( Object target, Delegate handler )
////    {
////        MethodInfo addMethod = GetAddMethod();
////
////        if(addMethod == null)
////        {
////            throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_NoPublicAddMethod" ) );
////        }
////
////        addMethod.Invoke( target, new object[] { handler } );
////    }
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    public void RemoveEventHandler( Object target, Delegate handler )
////    {
////        MethodInfo removeMethod = GetRemoveMethod();
////
////        if(removeMethod == null)
////        {
////            throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_NoPublicRemoveMethod" ) );
////        }
////
////        removeMethod.Invoke( target, new object[] { handler } );
////    }
////
////    public Type EventHandlerType
////    {
////        get
////        {
////            MethodInfo m = GetAddMethod( true );
////
////            ParameterInfo[] p = m.GetParametersNoCopy();
////
////            Type del = typeof( Delegate );
////
////            for(int i = 0; i < p.Length; i++)
////            {
////                Type c = p[i].ParameterType;
////
////                if(c.IsSubclassOf( del ))
////                {
////                    return c;
////                }
////            }
////            return null;
////        }
////    }
////
////    public bool IsSpecialName
////    {
////        get
////        {
////            return (Attributes & EventAttributes.SpecialName) != 0;
////        }
////    }
////
////    public bool IsMulticast
////    {
////        get
////        {
////            Type cl = EventHandlerType;
////            Type mc = typeof( MulticastDelegate );
////
////            return mc.IsAssignableFrom( cl );
////        }
////    }
        #endregion
    }
}
