// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
////////////////////////////////////////////////////////////////////////////////

namespace System
{
    using System;
    using System.Reflection;
////using System.Reflection.Emit;
////using System.Reflection.Cache;
    using System.Threading;
////using System.Runtime.Remoting;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
////using System.Security.Permissions;
    using System.Collections.Generic;
////using System.Runtime.Versioning;
    using CultureInfo                  = System.Globalization.CultureInfo;
////using SignatureHelper              = System.Reflection.Emit.SignatureHelper;
////using StackCrawlMark               = System.Threading.StackCrawlMark;
////using DebuggerStepThroughAttribute = System.Diagnostics.DebuggerStepThroughAttribute;

    [Serializable]
    public abstract class Type : MemberInfo, IReflect
    {
////    public static readonly MemberFilter FilterAttribute;
////    public static readonly MemberFilter FilterName;
////    public static readonly MemberFilter FilterNameIgnoreCase;
////
////    public static readonly Object Missing = System.Reflection.Missing.Value;
////
////    public static readonly char Delimiter = '.';
////
////    // EmptyTypes is used to indicate that we are looking for someting without any parameters.
////    public readonly static Type[] EmptyTypes = new Type[0];
////
////    // The Default binder.  We create a single one and expose that.
////    private static object defaultBinder;

        // private convenience data
        private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

////    private static readonly Type valueType  = typeof( System.ValueType );
////    private static readonly Type enumType   = typeof( System.Enum );
////    private static readonly Type objectType = typeof( System.Object );

////    // Because the current compiler doesn't support static delegates
////    //  the _Filters object is an object that we create to contain all of
////    //  the filters.
////    //private static final Type _filterClass = new RuntimeType();
////    static Type()
////    {
////        __Filters _filterClass = new __Filters();
////
////        FilterAttribute      = new MemberFilter( _filterClass.FilterAttribute  );
////        FilterName           = new MemberFilter( _filterClass.FilterName       );
////        FilterNameIgnoreCase = new MemberFilter( _filterClass.FilterIgnoreCase );
////    }

        // Prevent from begin created, and allow subclass
        //      to create.
        protected Type()
        {
        }


        // MemberInfo Methods....
        // The Member type Field.
        public override MemberTypes MemberType
        {
            get
            {
                return System.Reflection.MemberTypes.TypeInfo;
            }
        }

////    // Return the class that declared this Field.
////    public override Type DeclaringType
////    {
////        get
////        {
////            return this;
////        }
////    }

        public virtual MethodBase DeclaringMethod
        {
            get
            {
                return null;
            }
        }

////    // Return the class that was used to obtain this field.
////    public override Type ReflectedType
////    {
////        get
////        {
////            return this;
////        }
////    }

        ////////////////////////////////////////////////////////////////////////////////
        // This is a static method that returns a Class based upon the name of the class
        // (this name needs to be fully qualified with the package name and is
        // case-sensitive by default).
        ////

        // this method is required so Object.GetType is not made virtual by the compiler
        //
        public new Type GetType()
        {
            return base.GetType();
        }

////    public static Type GetType( String typeName, bool throwOnError, bool ignoreCase )
////    {
////        unsafe
////        {
////            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////            return RuntimeType.PrivateGetType( typeName, throwOnError, ignoreCase, ref stackMark );
////        }
////    }
////
////    public static Type GetType( String typeName, bool throwOnError )
////    {
////        unsafe
////        {
////            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////            return RuntimeType.PrivateGetType( typeName, throwOnError, false, ref stackMark );
////        }
////    }
////
////    public static Type GetType( String typeName )
////    {
////        unsafe
////        {
////            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////            return RuntimeType.PrivateGetType( typeName, false, false, ref stackMark );
////        }
////    }
////
////    public static Type ReflectionOnlyGetType( String typeName, bool throwIfNotFound, bool ignoreCase )
////    {
////        unsafe
////        {
////            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////            return RuntimeType.PrivateGetType( typeName, throwIfNotFound, ignoreCase, true /*reflectionOnly*/, ref stackMark );
////        }
////    }
////
////    public virtual Type MakePointerType()
////    {
////        throw new NotSupportedException();
////    }
////
////    public virtual StructLayoutAttribute StructLayoutAttribute
////    {
////        get
////        {
////            throw new NotSupportedException();
////        }
////    }
////
////    public virtual Type MakeByRefType()
////    {
////        throw new NotSupportedException();
////    }
////
////    public virtual Type MakeArrayType()
////    {
////        throw new NotSupportedException();
////    }
////
////    public virtual Type MakeArrayType( int rank )
////    {
////        throw new NotSupportedException();
////    }
////
////    internal string SigToString()
////    {
////        Type elementType = this;
////
////        while(elementType.HasElementType)
////        {
////            elementType = elementType.GetElementType();
////        }
////
////        if(elementType.IsNested)
////        {
////            return Name;
////        }
////
////        string sigToString = ToString();
////
////        if(elementType.IsPrimitive ||
////            elementType == typeof( void ) ||
////            elementType == typeof( TypedReference ))
////        {
////            sigToString = sigToString.Substring( @"System.".Length );
////        }
////
////        return sigToString;
////    }
////
////    // GetTypeCode
////    // This method will return a TypeCode for the passed
////    //  type.
////    public static TypeCode GetTypeCode( Type type )
////    {
////        if(type == null)
////        {
////            return TypeCode.Empty;
////        }
////
////        return type.GetTypeCodeInternal();
////    }
////
////    internal virtual TypeCode GetTypeCodeInternal()
////    {
////        Type type = this;
////        if(type is SymbolType)
////        {
////            return TypeCode.Object;
////        }
////
////        if(type is TypeBuilder)
////        {
////            TypeBuilder typeBuilder = (TypeBuilder)type;
////            if(typeBuilder.IsEnum == false)
////            {
////                return TypeCode.Object;
////            }
////
////            // if it is an Enum, just let the underlyingSystemType do the work
////        }
////
////        if(type != type.UnderlyingSystemType)
////        {
////            return Type.GetTypeCode( type.UnderlyingSystemType );
////        }
////
////        return TypeCode.Object;
////    }
////
////    // Property representing the GUID associated with a class.
////    public abstract Guid GUID
////    {
////        get;
////    }
////
////    // Return the Default binder used by the system.
////    static public Binder DefaultBinder
////    {
////        get
////        {
////            // Allocate the default binder if it hasn't been allocated yet.
////            if(defaultBinder == null)
////            {
////                CreateBinder();
////            }
////
////            return defaultBinder as Binder;
////        }
////    }
////
////    static private void CreateBinder()
////    {
////        if(defaultBinder == null)
////        {
////            object binder = new DefaultBinder();
////
////            Interlocked.CompareExchange( ref defaultBinder, binder, null );
////        }
////    }
////
////    // Description of the Binding Process.
////    // We must invoke a method that is accessable and for which the provided
////    // parameters have the most specific match.  A method may be called if
////    // 1. The number of parameters in the method declaration equals the number of
////    //      arguments provided to the invocation
////    // 2. The type of each argument can be converted by the binder to the
////    //      type of the type of the parameter.
////    //
////    // The binder will find all of the matching methods.  These method are found based
////    // upon the type of binding requested (MethodInvoke, Get/Set Properties).  The set
////    // of methods is filtered by the name, number of arguments and a set of search modifiers
////    // defined in the Binder.
////    //
////    // After the method is selected, it will be invoked.  Accessability is checked
////    // at that point.  The search may be control which set of methods are searched based
////    // upon the accessibility attribute associated with the method.
////    //
////    // The BindToMethod method is responsible for selecting the method to be invoked.
////    // For the default binder, the most specific method will be selected.
////    //
////    // This will invoke a specific member...

        public abstract Object InvokeMember( String              name            ,
                                             BindingFlags        invokeAttr      ,
                                             Binder              binder          ,
                                             Object              target          ,
                                             Object[]            args            ,
                                             ParameterModifier[] modifiers       ,
                                             CultureInfo         culture         ,
                                             String[]            namedParameters );

////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    public Object InvokeMember( String       name      ,
////                                BindingFlags invokeAttr,
////                                Binder       binder    ,
////                                Object       target    ,
////                                Object[]     args      ,
////                                CultureInfo  culture   )
////    {
////        return InvokeMember( name, invokeAttr, binder, target, args, null, culture, null );
////    }
////
////    [DebuggerStepThroughAttribute]
////    [Diagnostics.DebuggerHidden]
////    public Object InvokeMember( String       name      ,
////                                BindingFlags invokeAttr,
////                                Binder       binder    ,
////                                Object       target    ,
////                                Object[]     args      )
////    {
////        return InvokeMember( name, invokeAttr, binder, target, args, null, null, null );
////    }
////
////
////    // Module Property associated with a class.
////    public new abstract Module Module
////    {
////        get;
////    }

        // Assembly Property associated with a class.
        public abstract Assembly Assembly
        {
            get;
        }

        // A class handle is a unique integer value associated with
        // each class.  The handle is unique during the process life time.
        public abstract RuntimeTypeHandle TypeHandle
        {
            get;
        }

////    internal virtual RuntimeTypeHandle GetTypeHandleInternal()
////    {
////        return TypeHandle;
////    }

////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern RuntimeTypeHandle GetTypeHandle( Object o );

        // Given a class handle, this will return the class for that handle.
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern Type GetTypeFromHandle( RuntimeTypeHandle handle );

        // Return the fully qualified name.  The name does contain the namespace.
        public abstract String FullName
        {
            get;
        }
    
        // Return the name space of the class.
        public abstract String Namespace
        {
            get;
        }
    
        public abstract String AssemblyQualifiedName
        {
            get;
        }
    
////    public virtual int GetArrayRank()
////    {
////        throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SubclassOverride" ) );
////    }

        // Returns the base class for a class.  If this is an interface or has
        // no base class null is returned.  Object is the only Type that does not
        // have a base class.
        public abstract Type BaseType
        {
            get;
        }


////    // GetConstructor
////    // This method will search for the specified constructor.  For constructors,
////    //  unlike everything else, the default is to not look for static methods.  The
////    //  reason is that we don't typically expose the class initializer.
////    public ConstructorInfo GetConstructor( BindingFlags        bindingAttr   ,
////                                           Binder              binder        ,
////                                           CallingConventions  callConvention,
////                                           Type[]              types         ,
////                                           ParameterModifier[] modifiers     )
////    {
////        // Must provide some types (Type[0] for nothing)
////        if(types == null)
////        {
////            throw new ArgumentNullException( "types" );
////        }
////        for(int i = 0; i < types.Length; i++)
////        {
////            if(types[i] == null)
////            {
////                throw new ArgumentNullException( "types" );
////            }
////        }
////
////        return GetConstructorImpl( bindingAttr, binder, callConvention, types, modifiers );
////    }
////
////    public ConstructorInfo GetConstructor( BindingFlags        bindingAttr,
////                                           Binder              binder     ,
////                                           Type[]              types      ,
////                                           ParameterModifier[] modifiers  )
////    {
////        return GetConstructorImpl( bindingAttr, binder, CallingConventions.Any, types, modifiers );
////    }
////
////    public ConstructorInfo GetConstructor( Type[] types )
////    {
////        // The arguments are checked in the called version of GetConstructor.
////        return GetConstructor( BindingFlags.Public | BindingFlags.Instance, null, types, null );
////    }
////
////    abstract protected ConstructorInfo GetConstructorImpl( BindingFlags        bindingAttr   ,
////                                                           Binder              binder        ,
////                                                           CallingConventions  callConvention,
////                                                           Type[]              types         ,
////                                                           ParameterModifier[] modifiers     );
////
////    // GetConstructors()
////    // This routine will return an array of all constructors supported by the class.
////    //  Unlike everything else, the default is to not look for static methods.  The
////    //  reason is that we don't typically expose the class initializer.
////    public ConstructorInfo[] GetConstructors()
////    {
////        return GetConstructors( BindingFlags.Public | BindingFlags.Instance );
////    }
////
////    public abstract ConstructorInfo[] GetConstructors( BindingFlags bindingAttr );
////
////    public ConstructorInfo TypeInitializer
////    {
////        get
////        {
////            return GetConstructorImpl( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
////                                       null,
////                                       CallingConventions.Any,
////                                       Type.EmptyTypes,
////                                       null );
////        }
////    }

        // Return a method based upon the passed criteria.  The name of the method
        // must be provided, and exception is thrown if it is not.  The bindingAttr
        // parameter indicates if non-public methods should be searched.  The types
        // array indicates the types of the parameters being looked for.
        public MethodInfo GetMethod( String              name          ,
                                     BindingFlags        bindingAttr   ,
                                     Binder              binder        ,
                                     CallingConventions  callConvention,
                                     Type[]              types         ,
                                     ParameterModifier[] modifiers     )
        {
            if(name == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "name" );
#else
                throw new ArgumentNullException();
#endif
            }
            if(types == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "types" );
#else
                throw new ArgumentNullException();
#endif
            }
            for(int i = 0; i < types.Length; i++)
            {
                if(types[i] == null)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentNullException( "types" );
#else
                    throw new ArgumentNullException();
#endif
                }
            }

            return GetMethodImpl( name, bindingAttr, binder, callConvention, types, modifiers );
        }

        public MethodInfo GetMethod( String              name       ,
                                     BindingFlags        bindingAttr,
                                     Binder              binder     ,
                                     Type[]              types      ,
                                     ParameterModifier[] modifiers  )
        {
            return GetMethod( name, bindingAttr, binder, CallingConventions.Any, types, modifiers );
        }

////    public MethodInfo GetMethod( String name, Type[] types, ParameterModifier[] modifiers )
////    {
////        return GetMethod( name, Type.DefaultLookup, null, CallingConventions.Any, types, modifiers );
////    }
////
////    public MethodInfo GetMethod( String name, Type[] types )
////    {
////        return GetMethod( name, Type.DefaultLookup, null, CallingConventions.Any, types, null );
////    }

        public MethodInfo GetMethod( String name, BindingFlags bindingAttr )
        {
            if(name == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "name" );
#else
                throw new ArgumentNullException();
#endif
            }

            return GetMethodImpl( name, bindingAttr, null, CallingConventions.Any, null, null );
        }

////    public MethodInfo GetMethod( String name )
////    {
////        return GetMethod( name, Type.DefaultLookup );
////    }

        protected abstract MethodInfo GetMethodImpl( String              name          ,
                                                     BindingFlags        bindingAttr   ,
                                                     Binder              binder        ,
                                                     CallingConventions  callConvention,
                                                     Type[]              types         ,
                                                     ParameterModifier[] modifiers     );

        // GetMethods
        // This routine will return all the methods implemented by the class
        public MethodInfo[] GetMethods()
        {
            return GetMethods( Type.DefaultLookup );
        }

        public abstract MethodInfo[] GetMethods( BindingFlags bindingAttr );

        // GetField
        // Get Field will return a specific field based upon name
        public FieldInfo GetField( String name )
        {
            return GetField( name, Type.DefaultLookup );
        }

        public abstract FieldInfo GetField( String name, BindingFlags bindingAttr );


        // GetFields
        // Get fields will return a full array of fields implemented by a class
        public FieldInfo[] GetFields()
        {
            return GetFields( Type.DefaultLookup );
        }

        public abstract FieldInfo[] GetFields( BindingFlags bindingAttr );

////    // GetInterface
////    // This method will return an interface (as a class) based upon
////    //  the passed in name.
////    public Type GetInterface( String name )
////    {
////        return GetInterface( name, false );
////    }
////
////    public abstract Type GetInterface( String name, bool ignoreCase );
////
////    // GetInterfaces
////    // This method will return all of the interfaces implemented by a
////    //  class
////    public abstract Type[] GetInterfaces();
////
////    // FindInterfaces
////    // This method will filter the interfaces supported the class
////    public virtual Type[] FindInterfaces( TypeFilter filter, Object filterCriteria )
////    {
////        if(filter == null)
////        {
////            throw new ArgumentNullException( "filter" );
////        }
////
////        Type[] c   = GetInterfaces();
////        int    cnt = 0;
////
////        for(int i = 0; i < c.Length; i++)
////        {
////            if(!filter( c[i], filterCriteria ))
////            {
////                c[i] = null;
////            }
////            else
////            {
////                cnt++;
////            }
////        }
////
////        if(cnt == c.Length)
////        {
////            return c;
////        }
////
////        Type[] ret = new Type[cnt];
////
////        cnt = 0;
////
////        for(int i = 0; i < c.Length; i++)
////        {
////            if(c[i] != null)
////            {
////                ret[cnt++] = c[i];
////            }
////        }
////
////        return ret;
////    }
////
////    // GetEvent
////    // This method will return a event by name if it is found.
////    //  null is returned if the event is not found
////    public EventInfo GetEvent( String name )
////    {
////        return GetEvent( name, Type.DefaultLookup );
////    }
////
////    public abstract EventInfo GetEvent( String name, BindingFlags bindingAttr );
////
////    // GetEvents
////    // This method will return an array of EventInfo.  If there are not Events
////    //  an empty array will be returned.
////
////    virtual public EventInfo[] GetEvents()
////    {
////        return GetEvents( Type.DefaultLookup );
////    }
////
////    public abstract EventInfo[] GetEvents( BindingFlags bindingAttr );

        // Return a property based upon the passed criteria.  The nameof the
        // parameter must be provided.
        public PropertyInfo GetProperty( String              name       ,
                                         BindingFlags        bindingAttr,
                                         Binder              binder     ,
                                         Type                returnType ,
                                         Type[]              types      ,
                                         ParameterModifier[] modifiers  )
        {
            if(name == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "name" );
#else
                throw new ArgumentNullException();
#endif
            }
            if(types == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "types" );
#else
                throw new ArgumentNullException();
#endif
            }

            return GetPropertyImpl( name, bindingAttr, binder, returnType, types, modifiers );
        }

////    public PropertyInfo GetProperty( String              name       ,
////                                     Type                returnType ,
////                                     Type[]              types      ,
////                                     ParameterModifier[] modifiers  )
////    {
////        return GetProperty( name, Type.DefaultLookup, null, returnType, types, modifiers );
////    }

        public PropertyInfo GetProperty( String name, BindingFlags bindingAttr )
        {
            if(name == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "name" );
#else
                throw new ArgumentNullException();
#endif
            }

            return GetPropertyImpl( name, bindingAttr, null, null, null, null );
        }

////    public PropertyInfo GetProperty( String name, Type returnType, Type[] types )
////    {
////        return GetProperty( name, Type.DefaultLookup, null, returnType, types, null );
////    }
////
////    public PropertyInfo GetProperty( String name, Type[] types )
////    {
////        return GetProperty( name, Type.DefaultLookup, null, null, types, null );
////    }
////
////    public PropertyInfo GetProperty( String name, Type returnType )
////    {
////        if(name == null)
////        {
////            throw new ArgumentNullException( "name" );
////        }
////
////        if(returnType == null)
////        {
////            throw new ArgumentNullException( "returnType" );
////        }
////
////        return GetPropertyImpl( name, Type.DefaultLookup, null, returnType, null, null );
////    }
////
////    public PropertyInfo GetProperty( String name )
////    {
////        if(name == null)
////        {
////            throw new ArgumentNullException( "name" );
////        }
////
////        return GetPropertyImpl( name, Type.DefaultLookup, null, null, null, null );
////    }

        protected abstract PropertyInfo GetPropertyImpl( String              name       ,
                                                         BindingFlags        bindingAttr,
                                                         Binder              binder     ,
                                                         Type                returnType ,
                                                         Type[]              types      ,
                                                         ParameterModifier[] modifiers  );


        // GetProperties
        // This method will return an array of all of the properties defined
        //  for a Type.
        public PropertyInfo[] GetProperties()
        {
            return GetProperties( Type.DefaultLookup );
        }

        public abstract PropertyInfo[] GetProperties( BindingFlags bindingAttr );

////    // GetNestedTypes()
////    // This set of method will return any nested types that are found inside
////    //  of the type.
////    public Type[] GetNestedTypes()
////    {
////        return GetNestedTypes( Type.DefaultLookup );
////    }
////
////    public abstract Type[] GetNestedTypes( BindingFlags bindingAttr );
////
////    // GetNestedType()
////    public Type GetNestedType( String name )
////    {
////        return GetNestedType( name, Type.DefaultLookup );
////    }
////
////    public abstract Type GetNestedType( String name, BindingFlags bindingAttr );
////
////    // GetMember
////    // This method will return all of the members which match the specified string
////    // passed into the method
////    public MemberInfo[] GetMember( String name )
////    {
////        return GetMember( name, Type.DefaultLookup );
////    }
////
        public virtual MemberInfo[] GetMember( String name, BindingFlags bindingAttr )
        {
            return GetMember( name, MemberTypes.All, bindingAttr );
        }

        public virtual MemberInfo[] GetMember( String name, MemberTypes type, BindingFlags bindingAttr )
        {
#if EXCEPTION_STRINGS
            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SubclassOverride" ) );
#else
            throw new NotSupportedException();
#endif
        }

        // GetMembers
        // This will return a Member array of all of the members of a class
        public MemberInfo[] GetMembers()
        {
            return GetMembers( Type.DefaultLookup );
        }

        public abstract MemberInfo[] GetMembers( BindingFlags bindingAttr );

////    // GetDefaultMembers
////    // This will return a MemberInfo that has been marked with the
////    //      DefaultMemberAttribute
////    public virtual MemberInfo[] GetDefaultMembers()
////    {
////        // See if we have cached the default member name
////        String defaultMember = (String)this.Cache[CacheObjType.DefaultMember];
////
////        if(defaultMember == null)
////        {
////            // Get all of the custom attributes
////
////            CustomAttributeData attr = null;
////
////            for(Type t = this; t != null; t = t.BaseType)
////            {
////                IList<CustomAttributeData> attrs = CustomAttributeData.GetCustomAttributes( t );
////                for(int i = 0; i < attrs.Count; i++)
////                {
////                    if(attrs[i].Constructor.DeclaringType == typeof( DefaultMemberAttribute ))
////                    {
////                        attr = attrs[i];
////                        break;
////                    }
////                }
////
////                if(attr != null)
////                {
////                    break;
////                }
////            }
////
////            if(attr == null)
////            {
////                return new MemberInfo[0];
////            }
////
////            defaultMember = attr.ConstructorArguments[0].Value as string;
////            this.Cache[CacheObjType.DefaultMember] = defaultMember;
////        }
////
////        MemberInfo[] members = GetMember( defaultMember );
////        if(members == null)
////        {
////            members = new MemberInfo[0];
////        }
////        return members;
////    }
////
////    internal virtual String GetDefaultMemberName()
////    {
////        // See if we have cached the default member name
////        String defaultMember = (String)this.Cache[CacheObjType.DefaultMember];
////
////        if(defaultMember == null)
////        {
////            Object[] attrs = GetCustomAttributes( typeof( DefaultMemberAttribute ), true );
////
////            // We assume that there is only one DefaultMemberAttribute (Allow multiple = false)
////            if(attrs.Length > 1)
////            {
////                throw new ExecutionEngineException( Environment.GetResourceString( "ExecutionEngine_InvalidAttribute" ) );
////            }
////
////            if(attrs.Length == 0)
////            {
////                return null;
////            }
////
////            defaultMember = ((DefaultMemberAttribute)attrs[0]).MemberName;
////
////            this.Cache[CacheObjType.DefaultMember] = defaultMember;
////        }
////
////        return defaultMember;
////    }
////
////    // FindMembers
////    // This will return a filtered version of the member information
////    public virtual MemberInfo[] FindMembers( MemberTypes memberType, BindingFlags bindingAttr, MemberFilter filter, Object filterCriteria )
////    {
////        // Define the work arrays
////        MethodInfo[]      m = null;
////        ConstructorInfo[] c = null;
////        FieldInfo[]       f = null;
////        PropertyInfo[]    p = null;
////        EventInfo[]       e = null;
////        Type[]            t = null;
////
////        int i   = 0;
////        int cnt = 0; // Total Matchs
////
////        // Check the methods
////        if((memberType & System.Reflection.MemberTypes.Method) != 0)
////        {
////            m = GetMethods( bindingAttr );
////            if(filter != null)
////            {
////                for(i = 0; i < m.Length; i++)
////                {
////                    if(!filter( m[i], filterCriteria ))
////                    {
////                        m[i] = null;
////                    }
////                    else
////                    {
////                        cnt++;
////                    }
////                }
////            }
////            else
////            {
////                cnt += m.Length;
////            }
////        }
////
////        // Check the constructors
////        if((memberType & System.Reflection.MemberTypes.Constructor) != 0)
////        {
////            c = GetConstructors( bindingAttr );
////            if(filter != null)
////            {
////                for(i = 0; i < c.Length; i++)
////                {
////                    if(!filter( c[i], filterCriteria ))
////                    {
////                        c[i] = null;
////                    }
////                    else
////                    {
////                        cnt++;
////                    }
////                }
////            }
////            else
////            {
////                cnt += c.Length;
////            }
////        }
////
////        // Check the fields
////        if((memberType & System.Reflection.MemberTypes.Field) != 0)
////        {
////            f = GetFields( bindingAttr );
////            if(filter != null)
////            {
////                for(i = 0; i < f.Length; i++)
////                {
////                    if(!filter( f[i], filterCriteria ))
////                    {
////                        f[i] = null;
////                    }
////                    else
////                    {
////                        cnt++;
////                    }
////                }
////            }
////            else
////            {
////                cnt += f.Length;
////            }
////        }
////
////        // Check the Properties
////        if((memberType & System.Reflection.MemberTypes.Property) != 0)
////        {
////            p = GetProperties( bindingAttr );
////            if(filter != null)
////            {
////                for(i = 0; i < p.Length; i++)
////                {
////                    if(!filter( p[i], filterCriteria ))
////                    {
////                        p[i] = null;
////                    }
////                    else
////                    {
////                        cnt++;
////                    }
////                }
////            }
////            else
////            {
////                cnt += p.Length;
////            }
////        }
////
////        // Check the Events
////        if((memberType & System.Reflection.MemberTypes.Event) != 0)
////        {
////            e = GetEvents();
////            if(filter != null)
////            {
////                for(i = 0; i < e.Length; i++)
////                {
////                    if(!filter( e[i], filterCriteria ))
////                    {
////                        e[i] = null;
////                    }
////                    else
////                    {
////                        cnt++;
////                    }
////                }
////            }
////            else
////            {
////                cnt += e.Length;
////            }
////        }
////
////        // Check the Types
////        if((memberType & System.Reflection.MemberTypes.NestedType) != 0)
////        {
////            t = GetNestedTypes( bindingAttr );
////            if(filter != null)
////            {
////                for(i = 0; i < t.Length; i++)
////                {
////                    if(!filter( t[i], filterCriteria ))
////                    {
////                        t[i] = null;
////                    }
////                    else
////                    {
////                        cnt++;
////                    }
////                }
////            }
////            else
////            {
////                cnt += t.Length;
////            }
////        }
////
////        // Allocate the Member Info
////        MemberInfo[] ret = new MemberInfo[cnt];
////
////        // Copy the Methods
////        cnt = 0;
////        if(m != null)
////        {
////            for(i = 0; i < m.Length; i++)
////            {
////                if(m[i] != null)
////                {
////                    ret[cnt++] = m[i];
////                }
////            }
////        }
////
////        // Copy the Constructors
////        if(c != null)
////        {
////            for(i = 0; i < c.Length; i++)
////            {
////                if(c[i] != null)
////                {
////                    ret[cnt++] = c[i];
////                }
////            }
////        }
////
////        // Copy the Fields
////        if(f != null)
////        {
////            for(i = 0; i < f.Length; i++)
////            {
////                if(f[i] != null)
////                {
////                    ret[cnt++] = f[i];
////                }
////            }
////        }
////
////        // Copy the Properties
////        if(p != null)
////        {
////            for(i = 0; i < p.Length; i++)
////            {
////                if(p[i] != null)
////                {
////                    ret[cnt++] = p[i];
////                }
////            }
////        }
////
////        // Copy the Events
////        if(e != null)
////        {
////            for(i = 0; i < e.Length; i++)
////            {
////                if(e[i] != null)
////                {
////                    ret[cnt++] = e[i];
////                }
////            }
////        }
////
////        // Copy the Types
////        if(t != null)
////        {
////            for(i = 0; i < t.Length; i++)
////            {
////                if(t[i] != null)
////                {
////                    ret[cnt++] = t[i];
////                }
////            }
////        }
////
////        return ret;
////    }
////
////    ////////////////////////////////////////////////////////////////////////////////
////    //
////    // Attributes
////    //
////    //   The attributes are all treated as read-only properties on a class.  Most of
////    //  these boolean properties have flag values defined in this class and act like
////    //  a bit mask of attributes.  There are also a set of boolean properties that
////    //  relate to the classes relationship to other classes and to the state of the
////    //  class inside the runtime.
////    //
////    ////////////////////////////////////////////////////////////////////////////////
////
////    public bool IsNested
////    {
////        get
////        {
////            return DeclaringType != null;
////        }
////    }
////
////    // The attribute property on the Type.
////    public TypeAttributes Attributes
////    {
////        get
////        {
////            return GetAttributeFlagsImpl();
////        }
////    }
////
////    public virtual GenericParameterAttributes GenericParameterAttributes
////    {
////        get
////        {
////            throw new NotSupportedException();
////        }
////    }
////
////    public bool IsVisible
////    {
////        get
////        {
////            return GetTypeHandleInternal().IsVisible();
////        }
////    }
////
////    public bool IsNotPublic
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NotPublic);
////        }
////    }
////
////    public bool IsPublic
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.Public);
////        }
////    }
////
////    public bool IsNestedPublic
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic);
////        }
////    }
////
////    public bool IsNestedPrivate
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPrivate);
////        }
////    }
////
////    public bool IsNestedFamily
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamily);
////        }
////    }
////
////    public bool IsNestedAssembly
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedAssembly);
////        }
////    }
////
////    public bool IsNestedFamANDAssem
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamANDAssem);
////        }
////    }
////
////    public bool IsNestedFamORAssem
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamORAssem);
////        }
////    }
////
////    public bool IsAutoLayout
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.AutoLayout);
////        }
////    }
////
////    public bool IsLayoutSequential
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.SequentialLayout);
////        }
////    }
////
////    public bool IsExplicitLayout
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout);
////        }
////    }
    
        public extern bool IsClass
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Class && !IsSubclassOf( Type.valueType ));
////        }
        }
    
////    public bool IsInterface
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface);
////        }
////    }
    
        public extern bool IsValueType
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            return IsValueTypeImpl();
////        }
        }
    
////    public bool IsAbstract
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.Abstract) != 0);
////        }
////    }
////
////    public bool IsSealed
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.Sealed) != 0);
////        }
////    }
////
////    public bool IsEnum
////    {
////        get
////        {
////            return IsSubclassOf( Type.enumType );
////        }
////    }
////
////    public bool IsSpecialName
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.SpecialName) != 0);
////        }
////    }
////
////    public bool IsImport
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.Import) != 0);
////        }
////    }
////
////    public bool IsSerializable
////    {
////        get
////        {
////            return (((GetAttributeFlagsImpl() & TypeAttributes.Serializable) != 0) || (this.QuickSerializationCastCheck()));
////        }
////    }
////
////    private bool QuickSerializationCastCheck()
////    {
////        Type c = this.UnderlyingSystemType;
////        while(c != null)
////        {
////            if(c == typeof( Enum ) || c == typeof( Delegate ))
////            {
////                return true;
////            }
////
////            c = c.BaseType;
////        }
////
////        return false;
////    }
////
////    public bool IsAnsiClass
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.StringFormatMask) == TypeAttributes.AnsiClass);
////        }
////    }
////
////    public bool IsUnicodeClass
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.StringFormatMask) == TypeAttributes.UnicodeClass);
////        }
////    }
////
////    public bool IsAutoClass
////    {
////        get
////        {
////            return ((GetAttributeFlagsImpl() & TypeAttributes.StringFormatMask) == TypeAttributes.AutoClass);
////        }
////    }
////
////    // These are not backed up by attributes.  Instead they are implemented
////    //      based internally.
////    public bool IsArray
////    {
////        get
////        {
////            return IsArrayImpl();
////        }
////    }
////
////    internal virtual bool IsSzArray
////    {
////        get
////        {
////            return false;
////        }
////    }
////
////    public virtual bool IsGenericType
////    {
////        get
////        {
////            return false;
////        }
////    }
////
////    public virtual bool IsGenericTypeDefinition
////    {
////        get
////        {
////            return false;
////        }
////    }
////
////    public virtual bool IsGenericParameter
////    {
////        get
////        {
////            return false;
////        }
////    }
////
////    public virtual int GenericParameterPosition
////    {
////        get
////        {
////            throw new InvalidOperationException( Environment.GetResourceString( "Arg_NotGenericParameter" ) );
////        }
////    }
////
////    public virtual bool ContainsGenericParameters
////    {
////        get
////        {
////            if(HasElementType)
////            {
////                return GetRootElementType().ContainsGenericParameters;
////            }
////
////            if(IsGenericParameter)
////            {
////                return true;
////            }
////
////            if(!IsGenericType)
////            {
////                return false;
////            }
////
////            Type[] genericArguments = GetGenericArguments();
////            for(int i = 0; i < genericArguments.Length; i++)
////            {
////                if(genericArguments[i].ContainsGenericParameters)
////                {
////                    return true;
////                }
////            }
////
////            return false;
////        }
////    }
////
////    public virtual Type[] GetGenericParameterConstraints()
////    {
////        if(!IsGenericParameter)
////        {
////            throw new InvalidOperationException( Environment.GetResourceString( "Arg_NotGenericParameter" ) );
////        }
////
////        throw new InvalidOperationException();
////    }
    
        public extern bool IsByRef
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            return IsByRefImpl();
////        }
        }
    
////    public bool IsPointer
////    {
////        get
////        {
////            return IsPointerImpl();
////        }
////    }
////
////    public bool IsPrimitive
////    {
////        get
////        {
////            return IsPrimitiveImpl();
////        }
////    }
////
////    public bool IsCOMObject
////    {
////        get
////        {
////            return IsCOMObjectImpl();
////        }
////    }
////
////    public bool HasElementType
////    {
////        get
////        {
////            return HasElementTypeImpl();
////        }
////    }
////
////    public bool IsContextful
////    {
////        get
////        {
////            return IsContextfulImpl();
////        }
////    }
////
////    public bool IsMarshalByRef
////    {
////        get
////        {
////            return IsMarshalByRefImpl();
////        }
////    }
////
////    internal bool HasProxyAttribute
////    {
////        get
////        {
////            return HasProxyAttributeImpl();
////        }
////    }
////
////    // Protected routine to determine if this class represents a value class
////    protected virtual bool IsValueTypeImpl()
////    {
////        Type type = this;
////        if(type == Type.valueType || type == Type.enumType)
////        {
////            return false;
////        }
////
////        return IsSubclassOf( Type.valueType );
////    }
////
////    // Protected routine to get the attributes.
////    protected abstract TypeAttributes GetAttributeFlagsImpl();
////
////    // Protected routine to determine if this class represents an Array
////    protected abstract bool IsArrayImpl();
////
////    // Protected routine to determine if this class is a ByRef
////    protected abstract bool IsByRefImpl();
////
////    // Protected routine to determine if this class is a Pointer
////    protected abstract bool IsPointerImpl();
////
////    // Protected routine to determine if this class represents a primitive type
////    protected abstract bool IsPrimitiveImpl();
////
////    // Protected routine to determine if this class represents a COM object
////    protected abstract bool IsCOMObjectImpl();
////
////    public virtual Type MakeGenericType( params Type[] typeArguments ) { throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SubclassOverride" ) ); }
////
////
////    // Protected routine to determine if this class is contextful
////    protected virtual bool IsContextfulImpl()
////    {
////        return typeof( ContextBoundObject ).IsAssignableFrom( this );
////    }
////
////
////    // Protected routine to determine if this class is marshaled by ref
////    protected virtual bool IsMarshalByRefImpl()
////    {
////        return typeof( MarshalByRefObject ).IsAssignableFrom( this );
////    }
////
////    internal virtual bool HasProxyAttributeImpl()
////    {
////        // We will override this in RuntimeType
////        return false;
////    }

        public abstract Type GetElementType();

////    public virtual Type[] GetGenericArguments()
////    {
////        throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SubclassOverride" ) );
////    }
////
////    public virtual Type GetGenericTypeDefinition()
////    {
////        throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SubclassOverride" ) );
////    }
////
////    protected abstract bool HasElementTypeImpl();
////
////    internal virtual Type GetRootElementType()
////    {
////        Type rootElementType = this;
////
////        while(rootElementType.HasElementType)
////        {
////            rootElementType = rootElementType.GetElementType();
////        }
////
////        return rootElementType;
////    }


        // Return the underlying Type that represents the IReflect Object.  For expando object,
        // this is the (Object) IReflectInstance.GetType().  For Type object it is this.
        public abstract Type UnderlyingSystemType
        {
            get;
        }

        // Returns true of this class is a true subclass of c.  Everything
        // else returns false.  If this class and c are the same class false is
        // returned.
        //
        public virtual bool IsSubclassOf( Type c )
        {
            Type p = this;
            if(p == c)
            {
                return false;
            }

            while(p != null)
            {
                if(p == c)
                {
                    return true;
                }

                p = p.BaseType;
            }

            return false;
        }

        // Returns true if the object passed is assignable to an instance of this class.
        // Everything else returns false.
        //
        public virtual bool IsInstanceOfType( Object o )
        {
            if(this is RuntimeType)
            {
                return IsInstanceOfType( o );
            }

            if(o == null)
            {
                return false;
            }

////        // For transparent proxies we have to check the cast
////        // using remoting specific facilities.
////        if(RemotingServices.IsTransparentProxy( o ))
////        {
////            return (null != RemotingServices.CheckCast( o, this ));
////        }
////
////        if(IsInterface && o.GetType().IsCOMObject)
////        {
////            if(this is RuntimeType)
////            {
////                return ((RuntimeType)this).SupportsInterface( o );
////            }
////        }

            return IsAssignableFrom( o.GetType() );
        }

        // Returns true if an instance of Type c may be assigned
        // to an instance of this class.  Return false otherwise.
        //
        public virtual bool IsAssignableFrom( Type c )
        {
            throw new NotImplementedException();
////        if(c == null)
////        {
////            return false;
////        }
////
////        try
////        {
////            RuntimeType fromType = c   .UnderlyingSystemType as RuntimeType;
////            RuntimeType toType   = this.UnderlyingSystemType as RuntimeType;
////
////            if(fromType == null || toType == null)
////            {
////            ////// special case for TypeBuilder
////            ////TypeBuilder fromTypeBuilder = c as TypeBuilder;
////            ////if(fromTypeBuilder == null)
////            ////{
////            ////    return false;
////            ////}
////            ////
////            ////if(TypeBuilder.IsTypeEqual( this, c ))
////            ////{
////            ////    return true;
////            ////}
////            ////
////            ////// If fromTypeBuilder is a subclass of this class, then c can be cast to this type.
////            ////if(fromTypeBuilder.IsSubclassOf( this ))
////            ////{
////            ////    return true;
////            ////}
////            ////
////            ////if(this.IsInterface == false)
////            ////{
////            ////    return false;
////            ////}
////            ////
////            ////// now is This type a base type on one of the interface impl?
////            ////Type[] interfaces = fromTypeBuilder.GetInterfaces();
////            ////for(int i = 0; i < interfaces.Length; i++)
////            ////{
////            ////    if(TypeBuilder.IsTypeEqual( interfaces[i], this ))
////            ////    {
////            ////        return true;
////            ////    }
////            ////    if(interfaces[i].IsSubclassOf( this ))
////            ////    {
////            ////        return true;
////            ////    }
////            ////}
////                return false;
////            }
////
////            bool b = RuntimeType.CanCastTo( fromType, toType );
////
////            return b;
////        }
////        catch(ArgumentException)
////        {
////        }
////
////        // Check for interfaces
////        if(IsInterface)
////        {
////            Type[] ifaces = c.GetInterfaces();
////            for(int i = 0; i < ifaces.Length; i++)
////            {
////                if(this == ifaces[i])
////                {
////                    return true;
////                }
////            }
////        }
////        else if(IsGenericParameter)
////        {
////            Type[] constraints = GetGenericParameterConstraints();
////            for(int i = 0; i < constraints.Length; i++)
////            {
////                if(!constraints[i].IsAssignableFrom( c ))
////                {
////                    return false;
////                }
////            }
////
////            return true;
////        }
////        // Check the class relationship
////        else
////        {
////            while(c != null)
////            {
////                if(c == this)
////                {
////                    return true;
////                }
////
////                c = c.BaseType;
////            }
////        }
////
////        return false;
        }

////    // ToString
////    // Print the String Representation of the Type
////    public override String ToString()
////    {
////        return "Type: " + Name;
////    }

        // This method will return an array of classes based upon the array of
        // types.
        public static Type[] GetTypeArray( Object[] args )
        {
            if(args == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "args" );
#else
                throw new ArgumentNullException();
#endif
            }

            Type[] cls = new Type[args.Length];
            for(int i = 0; i < cls.Length; i++)
            {
                if(args[i] == null)
                {
                    throw new ArgumentNullException();
                }
                cls[i] = args[i].GetType();
            }

            return cls;
        }

        public override bool Equals( Object o )
        {
            if(!(o is Type))
            {
                return false;
            }

            return Equals( (Type)o );
        }

        public bool Equals( Type o )
        {
            if(o == null)
            {
                return false;
            }

            //Miguel: switched to pointer comparison to avoid circular ref
            //return (this.UnderlyingSystemType == o.UnderlyingSystemType);
            return ( object )this == ( object )o;
        }

        public override int GetHashCode()
        {
            Type SystemType = UnderlyingSystemType;
            if(SystemType != this)
            {
                return SystemType.GetHashCode();
            }

            return base.GetHashCode();
        }

        public static bool operator ==( Type left,
                                        Type right )
        {
            if ((object)left == null)
            {
                return (object)right == null;
            }

            if ((object)right == null)
            {
                return false;
            }

            return left.Equals( right );
        }

        public static bool operator !=( Type left,
                                        Type right )
        {
            return !(left == right);
        }

////    // GetInterfaceMap
////    // This method will return an interface mapping for the interface
////    //  requested.  It will throw an argument exception if the Type doesn't
////    //  implemenet the interface.
////    public virtual InterfaceMapping GetInterfaceMap( Type interfaceType )
////    {
////        throw new NotSupportedException( Environment.GetResourceString( "NotSupported_SubclassOverride" ) );
////    }
////
////    // This is used by Remoting
////    internal static Type ResolveTypeRelativeTo( String typeName, int offset, int count, Type serverType )
////    {
////        Type type = ResolveTypeRelativeToBaseTypes( typeName, offset, count, serverType );
////        if(type == null)
////        {
////            // compare against the interface list
////            // GetInterfaces() returns a complete list of interfaces this type supports
////            Type[] interfaces = serverType.GetInterfaces();
////            foreach(Type iface in interfaces)
////            {
////                String ifaceTypeName = iface.FullName;
////                if(ifaceTypeName.Length == count)
////                {
////                    if(String.CompareOrdinal( typeName, offset, ifaceTypeName, 0, count ) == 0)
////                    {
////                        return iface;
////                    }
////                }
////            }
////        }
////
////        return type;
////    } // ResolveTypeRelativeTo
////
////    // This is used by Remoting
////    internal static Type ResolveTypeRelativeToBaseTypes( String typeName, int offset, int count, Type serverType )
////    {
////        // typeName is excepted to contain the full type name
////        // offset is the start of the full type name within typeName
////        // count us the number of characters in the full type name
////        // serverType is the type of the server object
////
////        if((typeName == null) || (serverType == null))
////        {
////            return null;
////        }
////
////        String serverTypeName = serverType.FullName;
////        if(serverTypeName.Length == count)
////        {
////            if(String.CompareOrdinal( typeName, offset, serverTypeName, 0, count ) == 0)
////            {
////                return serverType;
////            }
////        }
////
////        return ResolveTypeRelativeToBaseTypes( typeName, offset, count, serverType.BaseType );
////    } // ResolveTypeRelativeTo
    }
}

