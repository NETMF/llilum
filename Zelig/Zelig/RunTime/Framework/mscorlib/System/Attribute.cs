// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace System
{
    using System;
    using System.Reflection;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Globalization;

    [Microsoft.Zelig.Internals.WellKnownType( "System_Attribute" )]
    [Serializable]
    [AttributeUsage( AttributeTargets.All, Inherited = true, AllowMultiple = false )]
    public abstract class Attribute /*: _Attribute*/
    {
        #region Private Statics

        #region PropertyInfo
////    private static Attribute[] InternalGetCustomAttributes( PropertyInfo element, Type type, bool inherit )
////    {
////        // walk up the hierarchy chain
////        Attribute[] attributes = (Attribute[])element.GetCustomAttributes( type, inherit );
////
////        if(!inherit)
////        {
////            return attributes;
////        }
////
////        // create the hashtable that keeps track of inherited types
////        Hashtable types = new Hashtable( 11 );
////
////        // create an array list to collect all the requested attibutes
////        ArrayList attributeList = new ArrayList();
////        CopyToArrayList( attributeList, attributes, types );
////
////        PropertyInfo baseProp = GetParentDefinition( element );
////        while(baseProp != null)
////        {
////            attributes = GetCustomAttributes( baseProp, type, false );
////
////            AddAttributesToList( attributeList, attributes, types );
////
////            baseProp = GetParentDefinition( baseProp );
////        }
////
////        return (Attribute[])attributeList.ToArray( type );
////    }
////
////    private static bool InternalIsDefined( PropertyInfo element, Type attributeType, bool inherit )
////    {
////        // walk up the hierarchy chain
////        if(element.IsDefined( attributeType, inherit ))
////        {
////            return true;
////        }
////
////        if(inherit)
////        {
////            AttributeUsageAttribute usage = InternalGetAttributeUsage( attributeType );
////
////            if(!usage.Inherited)
////            {
////                return false;
////            }
////
////            PropertyInfo baseProp = GetParentDefinition( element );
////
////            while(baseProp != null)
////            {
////                if(baseProp.IsDefined( attributeType, false ))
////                {
////                    return true;
////                }
////
////                baseProp = GetParentDefinition( baseProp );
////            }
////        }
////
////        return false;
////    }
////
////    private static PropertyInfo GetParentDefinition( PropertyInfo property )
////    {
////        // for the current property get the base class of the getter and the setter, they might be different
////        MethodInfo propAccessor = property.GetGetMethod( true );
////
////        if(propAccessor == null)
////        {
////            propAccessor = property.GetSetMethod( true );
////        }
////
////        if(propAccessor != null)
////        {
////            propAccessor = propAccessor.GetParentDefinition();
////
////            if(propAccessor != null)
////            {
////                return propAccessor.DeclaringType.GetProperty( property.Name, property.PropertyType );
////            }
////        }
////
////        return null;
////    }

        #endregion

        #region EventInfo
////    private static Attribute[] InternalGetCustomAttributes( EventInfo element, Type type, bool inherit )
////    {
////        // walk up the hierarchy chain
////        Attribute[] attributes = (Attribute[])element.GetCustomAttributes( type, inherit );
////        if(inherit)
////        {
////            // create the hashtable that keeps track of inherited types
////            Hashtable types = new Hashtable( 11 );
////
////            // create an array list to collect all the requested attibutes
////            ArrayList attributeList = new ArrayList();
////            CopyToArrayList( attributeList, attributes, types );
////
////            EventInfo baseEvent = GetParentDefinition( element );
////            while(baseEvent != null)
////            {
////                attributes = GetCustomAttributes( baseEvent, type, false );
////
////                AddAttributesToList( attributeList, attributes, types );
////
////                baseEvent = GetParentDefinition( baseEvent );
////            }
////            return (Attribute[])attributeList.ToArray( type );
////        }
////        else
////        {
////            return attributes;
////        }
////    }
////
////    private static EventInfo GetParentDefinition( EventInfo ev )
////    {
////        MethodInfo add = ev.GetAddMethod( true );
////        if(add != null)
////        {
////            add = add.GetParentDefinition();
////            if(add != null)
////            {
////                return add.DeclaringType.GetEvent( ev.Name );
////            }
////        }
////
////        return null;
////    }
////
////    private static bool InternalIsDefined( EventInfo element, Type attributeType, bool inherit )
////    {
////        // walk up the hierarchy chain
////        if(element.IsDefined( attributeType, inherit ))
////        {
////            return true;
////        }
////
////        if(inherit)
////        {
////            AttributeUsageAttribute usage = InternalGetAttributeUsage( attributeType );
////
////            if(!usage.Inherited)
////            {
////                return false;
////            }
////
////            EventInfo baseEvent = GetParentDefinition( element );
////
////            while(baseEvent != null)
////            {
////                if(baseEvent.IsDefined( attributeType, false ))
////                {
////                    return true;
////                }
////
////                baseEvent = GetParentDefinition( baseEvent );
////            }
////        }
////
////        return false;
////    }
////
        #endregion

        #region ParameterInfo
////    private static Attribute[] InternalParamGetCustomAttributes( MethodInfo method, ParameterInfo param, Type type, bool inherit )
////    {
////        // For ParameterInfo's we need to make sure that we chain through all the MethodInfo's in the inheritance chain that
////        // have this ParameterInfo defined. .We pick up all the CustomAttributes for the starting ParameterInfo. We need to pick up only attributes
////        // that are marked inherited from the remainder of the MethodInfo's in the inheritance chain.
////        // For MethodInfo's on an interface we do not do an inheritance walk so the default ParameterInfo attributes are returned.
////        // For MethodInfo's on a class we walk up the inheritance chain but do not look at the MethodInfo's on the interfaces that the
////        // class inherits from and return the respective ParameterInfo attributes
////
////        ArrayList disAllowMultiple = new ArrayList();
////        Object[] objAttr;
////
////        if(type == null)
////        {
////            type = typeof( Attribute );
////        }
////
////        objAttr = param.GetCustomAttributes( type, false );
////
////        for(int i = 0; i < objAttr.Length; i++)
////        {
////            Type objType = objAttr[i].GetType();
////
////            AttributeUsageAttribute attribUsage = InternalGetAttributeUsage( objType );
////
////            if(attribUsage.AllowMultiple == false)
////            {
////                disAllowMultiple.Add( objType );
////            }
////        }
////
////        // Get all the attributes that have Attribute as the base class
////        Attribute[] ret = null;
////        if(objAttr.Length == 0)
////        {
////            ret = (Attribute[])Array.CreateInstance( type, 0 );
////        }
////        else
////        {
////            ret = (Attribute[])objAttr;
////        }
////
////        if(method.DeclaringType == null) // This is an interface so we are done.
////        {
////            return ret;
////        }
////
////        if(!inherit)
////        {
////            return ret;
////        }
////
////        int paramPosition = param.Position;
////        method = method.GetParentDefinition();
////
////        while(method != null)
////        {
////            // Find the ParameterInfo on this method
////            ParameterInfo[] parameters = method.GetParameters();
////            param = parameters[paramPosition]; // Point to the correct ParameterInfo of the method
////
////            objAttr = param.GetCustomAttributes( type, false );
////
////            int count = 0;
////            for(int i = 0; i < objAttr.Length; i++)
////            {
////                Type objType = objAttr[i].GetType();
////                AttributeUsageAttribute attribUsage = InternalGetAttributeUsage( objType );
////
////                if((attribUsage.Inherited) && (disAllowMultiple.Contains( objType ) == false))
////                {
////                    if(attribUsage.AllowMultiple == false)
////                        disAllowMultiple.Add( objType );
////                    count++;
////                }
////                else
////                {
////                    objAttr[i] = null;
////                }
////            }
////
////            // Get all the attributes that have Attribute as the base class
////            Attribute[] attributes = (Attribute[])Array.CreateInstance( type, count );
////
////            count = 0;
////            for(int i = 0; i < objAttr.Length; i++)
////            {
////                if(objAttr[i] != null)
////                {
////                    attributes[count] = (Attribute)objAttr[i];
////                    count++;
////                }
////            }
////
////            Attribute[] temp = ret;
////            ret = (Attribute[])Array.CreateInstance( type, temp.Length + count );
////            Array.Copy( temp, ret, temp.Length );
////
////            int offset = temp.Length;
////
////            for(int i = 0; i < attributes.Length; i++)
////            {
////                ret[offset + i] = attributes[i];
////            }
////
////            method = method.GetParentDefinition();
////
////        }
////
////        return ret;
////
////    }
////
////    private static bool InternalParamIsDefined( MethodInfo method, ParameterInfo param, Type type, bool inherit )
////    {
////        // For ParameterInfo's we need to make sure that we chain through all the MethodInfo's in the inheritance chain.
////        // We pick up all the CustomAttributes for the starting ParameterInfo. We need to pick up only attributes
////        // that are marked inherited from the remainder of the ParameterInfo's in the inheritance chain.
////        // For MethodInfo's on an interface we do not do an inheritance walk. For ParameterInfo's on a
////        // Class we walk up the inheritance chain but do not look at the MethodInfo's on the interfaces that the class inherits from.
////
////        if(param.IsDefined( type, false ))
////        {
////            return true;
////        }
////
////        if(method.DeclaringType == null || !inherit) // This is an interface so we are done.
////        {
////            return false;
////        }
////
////        int paramPosition = param.Position;
////        method = method.GetParentDefinition();
////
////        while(method != null)
////        {
////            ParameterInfo[] parameters = method.GetParameters();
////            param = parameters[paramPosition];
////
////            Object[] objAttr = param.GetCustomAttributes( type, false );
////
////            for(int i = 0; i < objAttr.Length; i++)
////            {
////                Type objType = objAttr[i].GetType();
////                AttributeUsageAttribute attribUsage = InternalGetAttributeUsage( objType );
////
////                if((objAttr[i] is Attribute) && (attribUsage.Inherited))
////                {
////                    return true;
////                }
////            }
////
////            method = method.GetParentDefinition();
////        }
////
////        return false;
////    }
////
        #endregion

        #region Utility
////    private static void CopyToArrayList( ArrayList attributeList, Attribute[] attributes, Hashtable types )
////    {
////        for(int i = 0; i < attributes.Length; i++)
////        {
////            attributeList.Add( attributes[i] );
////
////            Type attrType = attributes[i].GetType();
////
////            if(!types.Contains( attrType ))
////            {
////                types[attrType] = InternalGetAttributeUsage( attrType );
////            }
////        }
////    }
////
////    private static void AddAttributesToList( ArrayList attributeList, Attribute[] attributes, Hashtable types )
////    {
////        for(int i = 0; i < attributes.Length; i++)
////        {
////            Type attrType = attributes[i].GetType();
////            AttributeUsageAttribute usage = (AttributeUsageAttribute)types[attrType];
////
////            if(usage == null)
////            {
////                // the type has never been seen before if it's inheritable add it to the list
////                usage = InternalGetAttributeUsage( attrType );
////                types[attrType] = usage;
////
////                if(usage.Inherited)
////                {
////                    attributeList.Add( attributes[i] );
////                }
////            }
////            else if(usage.Inherited && usage.AllowMultiple)
////            {
////                // we saw this type already add it only if it is inheritable and it does allow multiple
////                attributeList.Add( attributes[i] );
////            }
////        }
////    }
////
////    private static AttributeUsageAttribute InternalGetAttributeUsage( Type type )
////    {
////        // Check if the custom attributes is Inheritable
////        Object[] obj = type.GetCustomAttributes( typeof( AttributeUsageAttribute ), false );
////
////        if(obj.Length == 1)
////        {
////            return (AttributeUsageAttribute)obj[0];
////        }
////
////        if(obj.Length == 0)
////        {
////            return AttributeUsageAttribute.Default;
////        }
////
////        throw new FormatException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Format_AttributeUsage" ), type ) );
////    }
////
////    private static void ValidateParameters( Object element, Type attributeType )
////    {
////        if(element == null)
////        {
////            throw new ArgumentNullException( "element" );
////        }
////
////        if(attributeType == null)
////        {
////            throw new ArgumentNullException( "attributeType" );
////        }
////
////        if(!attributeType.IsSubclassOf( typeof( Attribute ) ) && attributeType != typeof( Attribute ))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_MustHaveAttributeBaseClass" ) );
////        }
////    }
////
        #endregion

        #endregion

        #region Public Statics

        #region MemberInfo
////    public static Attribute[] GetCustomAttributes( MemberInfo element, Type type )
////    {
////        return GetCustomAttributes( element, type, true );
////    }
////
////    public static Attribute[] GetCustomAttributes( MemberInfo element, Type type, bool inherit )
////    {
////        if(element == null)
////        {
////            throw new ArgumentNullException( "element" );
////        }
////
////        if(type == null)
////        {
////            throw new ArgumentNullException( "type" );
////        }
////
////        if(!type.IsSubclassOf( typeof( Attribute ) ) && type != typeof( Attribute ))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_MustHaveAttributeBaseClass" ) );
////        }
////
////        switch(element.MemberType)
////        {
////            case MemberTypes.Property:
////                return InternalGetCustomAttributes( (PropertyInfo)element, type, inherit );
////
////            case MemberTypes.Event:
////                return InternalGetCustomAttributes( (EventInfo)element, type, inherit );
////
////            default:
////                return element.GetCustomAttributes( type, inherit ) as Attribute[];
////        }
////    }
////
////    public static Attribute[] GetCustomAttributes( MemberInfo element )
////    {
////        return GetCustomAttributes( element, true );
////    }
////
////    public static Attribute[] GetCustomAttributes( MemberInfo element, bool inherit )
////    {
////        return GetCustomAttributes( element, typeof( Attribute ), inherit );
////    }
////
////
////    public static bool IsDefined( MemberInfo element, Type attributeType )
////    {
////        return IsDefined( element, attributeType, true );
////    }
////
////    public static bool IsDefined( MemberInfo element, Type attributeType, bool inherit )
////    {
////        ValidateParameters( element, attributeType );
////
////        switch(element.MemberType)
////        {
////            case MemberTypes.Property:
////                return InternalIsDefined( (PropertyInfo)element, attributeType, inherit );
////
////            case MemberTypes.Event:
////                return InternalIsDefined( (EventInfo)element, attributeType, inherit );
////
////            default:
////                return element.IsDefined( attributeType, inherit );
////        }
////
////    }
////
////    public static Attribute GetCustomAttribute( MemberInfo element, Type attributeType )
////    {
////        return GetCustomAttribute( element, attributeType, true );
////    }
////
////    public static Attribute GetCustomAttribute( MemberInfo element, Type attributeType, bool inherit )
////    {
////        Attribute[] attrib = GetCustomAttributes( element, attributeType, inherit );
////
////        if(attrib == null || attrib.Length == 0)
////        {
////            return null;
////        }
////
////        if(attrib.Length == 1)
////        {
////            return attrib[0];
////        }
////
////        throw new AmbiguousMatchException( Environment.GetResourceString( "RFLCT.AmbigCust" ) );
////    }
////
        #endregion

        #region ParameterInfo
////    public static Attribute[] GetCustomAttributes( ParameterInfo element )
////    {
////        return GetCustomAttributes( element, true );
////    }
////
////    public static Attribute[] GetCustomAttributes( ParameterInfo element, Type attributeType )
////    {
////        return GetCustomAttributes( element, attributeType, true );
////    }
////
////    public static Attribute[] GetCustomAttributes( ParameterInfo element, bool inherit )
////    {
////        return GetCustomAttributes( element, typeof( Attribute ), inherit );
////    }
////
////    public static Attribute[] GetCustomAttributes( ParameterInfo element, Type attributeType, bool inherit )
////    {
////        if(element == null)
////        {
////            throw new ArgumentNullException( "element" );
////        }
////
////        if(attributeType == null)
////        {
////            throw new ArgumentNullException( "attributeType" );
////        }
////
////        if(attributeType != typeof( Attribute ) && !attributeType.IsSubclassOf( typeof( Attribute ) ))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_MustHaveAttributeBaseClass" ) );
////        }
////
////        MemberInfo member = element.Member;
////
////        if(member.MemberType == MemberTypes.Method && inherit)
////        {
////            return InternalParamGetCustomAttributes( (MethodInfo)member, element, attributeType, inherit ) as Attribute[];
////        }
////
////        return element.GetCustomAttributes( attributeType, inherit ) as Attribute[];
////    }
////
////
////    public static bool IsDefined( ParameterInfo element, Type attributeType )
////    {
////        return IsDefined( element, attributeType, true );
////    }
////
////    public static bool IsDefined( ParameterInfo element, Type attributeType, bool inherit )
////    {
////        // Returns true is a custom attribute subclass of attributeType class/interface with inheritance walk
////        ValidateParameters( element, attributeType );
////
////        MemberInfo member = element.Member;
////
////        switch(member.MemberType)
////        {
////            case MemberTypes.Method: // We need to climb up the member hierarchy
////                return InternalParamIsDefined( (MethodInfo)member, element, attributeType, inherit );
////
////            case MemberTypes.Constructor:
////                return element.IsDefined( attributeType, false );
////
////            case MemberTypes.Property:
////                return element.IsDefined( attributeType, false );
////
////            default:
////                BCLDebug.Assert( false, "Invalid type for ParameterInfo member in Attribute class" );
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidParamInfo" ) );
////        }
////    }
////
////    public static Attribute GetCustomAttribute( ParameterInfo element, Type attributeType )
////    {
////        return GetCustomAttribute( element, attributeType, true );
////    }
////
////    public static Attribute GetCustomAttribute( ParameterInfo element, Type attributeType, bool inherit )
////    {
////        // Returns an Attribute of base class/inteface attributeType on the ParameterInfo or null if none exists.
////        // throws an AmbiguousMatchException if there are more than one defined.
////        Attribute[] attrib = GetCustomAttributes( element, attributeType, inherit );
////
////        if(attrib == null || attrib.Length == 0)
////        {
////            return null;
////        }
////
////        if(attrib.Length == 0)
////        {
////            return null;
////        }
////
////        if(attrib.Length == 1)
////        {
////            return attrib[0];
////        }
////
////        throw new AmbiguousMatchException( Environment.GetResourceString( "RFLCT.AmbigCust" ) );
////    }
////
        #endregion

        #region Module
////    public static Attribute[] GetCustomAttributes( Module element, Type attributeType )
////    {
////        return GetCustomAttributes( element, attributeType, true );
////    }
////
////    public static Attribute[] GetCustomAttributes( Module element )
////    {
////        return GetCustomAttributes( element, true );
////    }
////
////    public static Attribute[] GetCustomAttributes( Module element, bool inherit )
////    {
////        return GetCustomAttributes( element, typeof( Attribute ), inherit );
////    }
////
////    public static Attribute[] GetCustomAttributes( Module element, Type attributeType, bool inherit )
////    {
////        ValidateParameters( element, attributeType );
////
////        return (Attribute[])element.GetCustomAttributes( attributeType, inherit );
////    }
////
////
////    public static bool IsDefined( Module element, Type attributeType )
////    {
////        return IsDefined( element, attributeType, false );
////    }
////
////    public static bool IsDefined( Module element, Type attributeType, bool inherit )
////    {
////        ValidateParameters( element, attributeType );
////
////        return element.IsDefined( attributeType, false );
////    }
////
////
////    public static Attribute GetCustomAttribute( Module element, Type attributeType )
////    {
////        return GetCustomAttribute( element, attributeType, true );
////    }
////
////    public static Attribute GetCustomAttribute( Module element, Type attributeType, bool inherit )
////    {
////        // Returns an Attribute of base class/inteface attributeType on the Module or null if none exists.
////        // throws an AmbiguousMatchException if there are more than one defined.
////        Attribute[] attrib = GetCustomAttributes( element, attributeType, inherit );
////
////        if(attrib == null || attrib.Length == 0)
////        {
////            return null;
////        }
////
////        if(attrib.Length == 1)
////        {
////            return attrib[0];
////        }
////
////        throw new AmbiguousMatchException( Environment.GetResourceString( "RFLCT.AmbigCust" ) );
////    }
////
        #endregion

        #region Assembly
////    public static Attribute[] GetCustomAttributes( Assembly element )
////    {
////        return GetCustomAttributes( element, true );
////    }
////
////    public static Attribute[] GetCustomAttributes( Assembly element, Type attributeType )
////    {
////        return GetCustomAttributes( element, attributeType, true );
////    }
////
////    public static Attribute[] GetCustomAttributes( Assembly element, bool inherit )
////    {
////        return GetCustomAttributes( element, typeof( Attribute ), inherit );
////    }
////
////    public static Attribute[] GetCustomAttributes( Assembly element, Type attributeType, bool inherit )
////    {
////        ValidateParameters( element, attributeType );
////
////        return (Attribute[])element.GetCustomAttributes( attributeType, inherit );
////    }
////
////
////    public static bool IsDefined( Assembly element, Type attributeType )
////    {
////        return IsDefined( element, attributeType, true );
////    }
////
////    public static bool IsDefined( Assembly element, Type attributeType, bool inherit )
////    {
////        ValidateParameters( element, attributeType );
////
////        return element.IsDefined( attributeType, false );
////    }
////
////
////    public static Attribute GetCustomAttribute( Assembly element, Type attributeType )
////    {
////        return GetCustomAttribute( element, attributeType, true );
////    }
////
////    public static Attribute GetCustomAttribute( Assembly element, Type attributeType, bool inherit )
////    {
////        // Returns an Attribute of base class/inteface attributeType on the Assembly or null if none exists.
////        // throws an AmbiguousMatchException if there are more than one defined.
////        Attribute[] attrib = GetCustomAttributes( element, attributeType, inherit );
////
////        if(attrib == null || attrib.Length == 0)
////        {
////            return null;
////        }
////
////        if(attrib.Length == 1)
////        {
////            return attrib[0];
////        }
////
////        throw new AmbiguousMatchException( Environment.GetResourceString( "RFLCT.AmbigCust" ) );
////    }

        #endregion

        #endregion

        #region Constructor
        protected Attribute()
        {
        }
        #endregion

        #region Object Overrides
////    public override bool Equals( Object obj )
////    {
////        if(obj == null)
////        {
////            return false;
////        }
////
////        RuntimeType thisType = (RuntimeType)this.GetType();
////        RuntimeType thatType = (RuntimeType)obj.GetType();
////
////        if(thatType != thisType)
////        {
////            return false;
////        }
////
////        Object thisObj = this;
////        Object thisResult;
////        Object thatResult;
////
////        FieldInfo[] thisFields = thisType.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
////
////        for(int i = 0; i < thisFields.Length; i++)
////        {
////            thisResult = ((RuntimeFieldInfo)thisFields[i]).GetValue( thisObj );
////            thatResult = ((RuntimeFieldInfo)thisFields[i]).GetValue( obj );
////
////            if(thisResult == null)
////            {
////                if(thatResult != null)
////                {
////                    return false;
////                }
////            }
////            else if(!thisResult.Equals( thatResult ))
////            {
////                return false;
////            }
////        }
////
////        return true;
////    }
////
////    public override int GetHashCode()
////    {
////        Type type = GetType();
////
////        FieldInfo[] fields = type.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
////        Object      vThis  = null;
////
////        for(int i = 0; i < fields.Length; i++)
////        {
////            FieldInfo field = fields[i];
////
////            vThis = field.GetValue( this );
////
////            if(vThis != null)
////            {
////                break;
////            }
////        }
////
////        if(vThis != null)
////        {
////            return vThis.GetHashCode();
////        }
////
////        return type.GetHashCode();
////    }
        #endregion

        #region Public Virtual Members
////    public virtual Object TypeId
////    {
////        get
////        {
////            return GetType();
////        }
////    }
////
////    public virtual bool Match( Object obj )
////    {
////        return Equals( obj );
////    }
        #endregion

        #region Public Members
////    public virtual bool IsDefaultAttribute()
////    {
////        return false;
////    }
        #endregion
    }
}

