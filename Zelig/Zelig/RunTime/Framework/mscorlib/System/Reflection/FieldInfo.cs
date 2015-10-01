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
    public abstract class FieldInfo : MemberInfo
    {
        #region Static Members
////    public static FieldInfo GetFieldFromHandle( RuntimeFieldHandle handle )
////    {
////        if(handle.IsNullHandle())
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidHandle" ) );
////        }
////
////        FieldInfo f = RuntimeType.GetFieldInfo( handle );
////
////        if(f.DeclaringType != null && f.DeclaringType.IsGenericType)
////        {
////            throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_FieldDeclaringTypeGeneric" ), f.Name, f.DeclaringType.GetGenericTypeDefinition() ) );
////        }
////
////        return f;
////    }
////
////    public static FieldInfo GetFieldFromHandle( RuntimeFieldHandle handle, RuntimeTypeHandle declaringType )
////    {
////        if(handle.IsNullHandle())
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidHandle" ) );
////        }
////
////        return RuntimeType.GetFieldInfo( declaringType, handle );
////    }

        public static bool operator ==( FieldInfo left,
                                        FieldInfo right )
        {
            if ((object)left == null)
            {
                return (object)right == null;
            }

            if ((object)right == null)
            {
                return false;
            }

            return left.Equals(right); 
        }

        public static bool operator !=( FieldInfo left,
                                        FieldInfo right )
        {
            return !(left == right);
        }
        #endregion

        #region Constructor
        protected FieldInfo()
        {
        }
        #endregion

        #region MemberInfo Overrides
        public override MemberTypes MemberType
        {
            get
            {
                return System.Reflection.MemberTypes.Field;
            }
        }

        public override bool Equals( object obj )
        {
            if(obj == null) 
            {
                return false;
            }
            if ((obj is FieldInfo) == false)
            {
                return false;
            }
            return (this.FieldType == ((FieldInfo)obj).FieldType) && (this.GetValue( this ) == ((FieldInfo)obj).GetValue( obj ));
        }

        public override int GetHashCode()
        {
            return GetValue( this ).GetHashCode();
        }
        #endregion

        #region Public Abstract\Virtual Members

////    public virtual Type[] GetRequiredCustomModifiers()
////    {
////        throw new NotImplementedException();
////    }
////
////    public virtual Type[] GetOptionalCustomModifiers()
////    {
////        throw new NotImplementedException();
////    }
////
////    [CLSCompliant( false )]
////    public virtual void SetValueDirect( TypedReference obj, Object value )
////    {
////        throw new NotSupportedException( Environment.GetResourceString( "NotSupported_AbstractNonCLS" ) );
////    }
////
////    [CLSCompliant( false )]
////    public virtual Object GetValueDirect( TypedReference obj )
////    {
////        throw new NotSupportedException( Environment.GetResourceString( "NotSupported_AbstractNonCLS" ) );
////    }
////
////    public abstract RuntimeFieldHandle FieldHandle
////    {
////        get;
////    }
    
        public abstract Type FieldType
        {
            get;
        }
    
        public abstract Object GetValue( Object obj );
    
////    public virtual Object GetRawConstantValue()
////    {
////        throw new NotSupportedException( Environment.GetResourceString( "NotSupported_AbstractNonCLS" ) );
////    }
////
////    public abstract void SetValue( Object obj, Object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture );
////
////    public abstract FieldAttributes Attributes
////    {
////        get;
////    }
        #endregion

        #region Public Members
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
        public void SetValue( Object obj, Object value )
        {
            throw new NotImplementedException();
////        SetValue( obj, value, BindingFlags.Default, Type.DefaultBinder, null );
        }
    
////    public bool IsPublic
////    {
////        get
////        {
////            return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;
////        }
////    }
////
////    public bool IsPrivate
////    {
////        get
////        {
////            return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Private;
////        }
////    }
////
////    public bool IsFamily
////    {
////        get
////        {
////            return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family;
////        }
////    }
////
////    public bool IsAssembly
////    {
////        get
////        {
////            return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly;
////        }
////    }
////
////    public bool IsFamilyAndAssembly
////    {
////        get
////        {
////            return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamANDAssem;
////        }
////    }
////
////    public bool IsFamilyOrAssembly
////    {
////        get
////        {
////            return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamORAssem;
////        }
////    }
////
////    public bool IsStatic
////    {
////        get
////        {
////            return (Attributes & FieldAttributes.Static) != 0;
////        }
////    }
////
////    public bool IsInitOnly
////    {
////        get
////        {
////            return (Attributes & FieldAttributes.InitOnly) != 0;
////        }
////    }
////
////    public bool IsLiteral
////    {
////        get
////        {
////            return (Attributes & FieldAttributes.Literal) != 0;
////        }
////    }
////
////    public bool IsNotSerialized
////    {
////        get
////        {
////            return (Attributes & FieldAttributes.NotSerialized) != 0;
////        }
////    }
////
////    public bool IsSpecialName
////    {
////        get
////        {
////            return (Attributes & FieldAttributes.SpecialName) != 0;
////        }
////    }
////
////    public bool IsPinvokeImpl
////    {
////        get
////        {
////            return (Attributes & FieldAttributes.PinvokeImpl) != 0;
////        }
////    }
        #endregion
    }
}
