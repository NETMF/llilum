// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
// Activator is an object that contains the Activation (CreateInstance/New) 
//  methods for late bound support.
//
// 
// 
//
namespace System
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
////using System.Runtime.Remoting;
////using System.Runtime.Remoting.Activation;
////using Message = System.Runtime.Remoting.Messaging.Message;
////using CultureInfo = System.Globalization.CultureInfo;
////using Evidence = System.Security.Policy.Evidence;
////using StackCrawlMark = System.Threading.StackCrawlMark;
////using System.Runtime.InteropServices;
////using System.Security.Permissions;
////using AssemblyHashAlgorithm = System.Configuration.Assemblies.AssemblyHashAlgorithm;
////using System.Runtime.Versioning;

    // Only statics, does not need to be marked with the serializable attribute
////[ClassInterface( ClassInterfaceType.None )]
////[ComDefaultInterface( typeof( _Activator ) )]
    public sealed class Activator //: _Activator
    {
////    internal const int LookupMask = 0x000000FF;
////    internal const BindingFlags ConLookup = (BindingFlags)(BindingFlags.Instance | BindingFlags.Public);
////    internal const BindingFlags ConstructorDefault = BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;

        // This class only contains statics, so hide the worthless constructor
        private Activator()
        {
        }

////    // CreateInstance
////    // The following methods will create a new instance of an Object
////    // Full Binding Support
////    // For all of these methods we need to get the underlying RuntimeType and
////    //  call the Impl version.
////    static public Object CreateInstance( Type type,
////                                        BindingFlags bindingAttr,
////                                        Binder binder,
////                                        Object[] args,
////                                        CultureInfo culture )
////    {
////        return CreateInstance( type, bindingAttr, binder, args, culture, null );
////    }
////
////    static public Object CreateInstance( Type type,
////                                        BindingFlags bindingAttr,
////                                        Binder binder,
////                                        Object[] args,
////                                        CultureInfo culture,
////                                        Object[] activationAttributes )
////    {
////        if(type == null)
////            throw new ArgumentNullException( "type" );
////
////        if(type is System.Reflection.Emit.TypeBuilder)
////            throw new NotSupportedException( Environment.GetResourceString( "NotSupported_CreateInstanceWithTypeBuilder" ) );
////
////        // If they didn't specify a lookup, then we will provide the default lookup.
////        if((bindingAttr & (BindingFlags)LookupMask) == 0)
////            bindingAttr |= Activator.ConstructorDefault;
////
////        if(activationAttributes != null && activationAttributes.Length > 0)
////        {
////            // If type does not derive from MBR
////            // throw notsupportedexception
////            if(type.IsMarshalByRef)
////            {
////                // The fix below is preventative.
////                //
////                if(!(type.IsContextful))
////                {
////                    if(activationAttributes.Length > 1 || !(activationAttributes[0] is UrlAttribute))
////                        throw new NotSupportedException( Environment.GetResourceString( "NotSupported_NonUrlAttrOnMBR" ) );
////                }
////            }
////            else
////                throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ActivAttrOnNonMBR" ) );
////        }
////
////        RuntimeType rt = type.UnderlyingSystemType as RuntimeType;
////
////        if(rt == null)
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "type" );
////
////        return rt.CreateInstanceImpl( bindingAttr, binder, args, culture, activationAttributes );
////    }
////
////    static public Object CreateInstance( Type type, params Object[] args )
////    {
////        return CreateInstance( type,
////                              Activator.ConstructorDefault,
////                              null,
////                              args,
////                              null,
////                              null );
////    }
////
////    static public Object CreateInstance( Type type,
////                                        Object[] args,
////                                        Object[] activationAttributes )
////    {
////        return CreateInstance( type,
////                              Activator.ConstructorDefault,
////                              null,
////                              args,
////                              null,
////                              activationAttributes );
////    }

        static public Object CreateInstance( Type type )
        {
            throw new NotImplementedException();
////        return Activator.CreateInstance( type, false );
        }

////    /*
////     * Create an instance using the name of type and the assembly where it exists. This allows
////     * types to be created remotely without having to load the type locally.
////     */
////
////    static public ObjectHandle CreateInstance( String assemblyName,
////                                              String typeName )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////        return CreateInstance( assemblyName,
////                              typeName,
////                              false,
////                              Activator.ConstructorDefault,
////                              null,
////                              null,
////                              null,
////                              null,
////                              null,
////                              ref stackMark );
////    }
////
////    static public ObjectHandle CreateInstance( String assemblyName,
////                                              String typeName,
////                                              Object[] activationAttributes )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////        return CreateInstance( assemblyName,
////                              typeName,
////                              false,
////                              Activator.ConstructorDefault,
////                              null,
////                              null,
////                              null,
////                              activationAttributes,
////                              null,
////                              ref stackMark );
////    }
////
////    static public Object CreateInstance( Type type, bool nonPublic )
////    {
////        if(type == null)
////            throw new ArgumentNullException( "type" );
////
////        RuntimeType rt = type.UnderlyingSystemType as RuntimeType;
////
////        if(rt == null)
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "type" );
////
////        return rt.CreateInstanceImpl( !nonPublic );
////    }
////
////    static internal Object InternalCreateInstanceWithNoMemberAccessCheck( Type type, bool nonPublic )
////    {
////        if(type == null)
////            throw new ArgumentNullException( "type" );
////
////        RuntimeType rt = type.UnderlyingSystemType as RuntimeType;
////
////        if(rt == null)
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "type" );
////
////        return rt.CreateInstanceImpl( !nonPublic, false, false );
////    }
    
        static public T CreateInstance<T>()
        {
            return (T)CreateInstanceInner( typeof(T) );
////        bool bNeedSecurityCheck = true;
////        bool bCanBeCached = false;
////        RuntimeMethodHandle mh = RuntimeMethodHandle.EmptyHandle;
////        return (T)RuntimeTypeHandle.CreateInstance( typeof( T ) as RuntimeType, true, true, ref bCanBeCached, ref mh, ref bNeedSecurityCheck );
        }

        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern object CreateInstanceInner( Type t );
    
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    static public ObjectHandle CreateInstanceFrom( String assemblyFile,
////                                                  String typeName )
////    {
////        return CreateInstanceFrom( assemblyFile, typeName, null );
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    static public ObjectHandle CreateInstanceFrom( String assemblyFile,
////                                                  String typeName,
////                                                  Object[] activationAttributes )
////    {
////        return CreateInstanceFrom( assemblyFile,
////                                  typeName,
////                                  false,
////                                  Activator.ConstructorDefault,
////                                  null,
////                                  null,
////                                  null,
////                                  activationAttributes,
////                                  null );
////    }
////
////
////    static public ObjectHandle CreateInstance( String assemblyName,
////                                              String typeName,
////                                              bool ignoreCase,
////                                              BindingFlags bindingAttr,
////                                              Binder binder,
////                                              Object[] args,
////                                              CultureInfo culture,
////                                              Object[] activationAttributes,
////                                              Evidence securityInfo )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////        return CreateInstance( assemblyName,
////                              typeName,
////                              ignoreCase,
////                              bindingAttr,
////                              binder,
////                              args,
////                              culture,
////                              activationAttributes,
////                              securityInfo,
////                              ref stackMark );
////    }
////
////    static internal ObjectHandle CreateInstance( String assemblyName,
////                                                String typeName,
////                                                bool ignoreCase,
////                                                BindingFlags bindingAttr,
////                                                Binder binder,
////                                                Object[] args,
////                                                CultureInfo culture,
////                                                Object[] activationAttributes,
////                                                Evidence securityInfo,
////                                                ref StackCrawlMark stackMark )
////    {
////        Assembly assembly;
////        if(assemblyName == null)
////            assembly = Assembly.GetExecutingAssembly( ref stackMark );
////        else
////            assembly = Assembly.InternalLoad( assemblyName, securityInfo, ref stackMark, false );
////
////        Log( assembly != null, "CreateInstance:: ", "Loaded " + assembly.FullName, "Failed to Load: " + assemblyName );
////        if(assembly == null) return null;
////
////        Type t = assembly.GetType( typeName, true, ignoreCase );
////
////        Object o = Activator.CreateInstance( t,
////                                            bindingAttr,
////                                            binder,
////                                            args,
////                                            culture,
////                                            activationAttributes );
////
////        Log( o != null, "CreateInstance:: ", "Created Instance of class " + typeName, "Failed to create instance of class " + typeName );
////        if(o == null)
////            return null;
////        else
////        {
////            ObjectHandle Handle = new ObjectHandle( o );
////            return Handle;
////        }
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    static public ObjectHandle CreateInstanceFrom( String assemblyFile,
////                                                  String typeName,
////                                                  bool ignoreCase,
////                                                  BindingFlags bindingAttr,
////                                                  Binder binder,
////                                                  Object[] args,
////                                                  CultureInfo culture,
////                                                  Object[] activationAttributes,
////                                                  Evidence securityInfo )
////    {
////        Assembly assembly = Assembly.LoadFrom( assemblyFile, securityInfo );
////        Type t = assembly.GetType( typeName, true, ignoreCase );
////
////        Object o = Activator.CreateInstance( t,
////                                            bindingAttr,
////                                            binder,
////                                            args,
////                                            culture,
////                                            activationAttributes );
////
////        Log( o != null, "CreateInstanceFrom:: ", "Created Instance of class " + typeName, "Failed to create instance of class " + typeName );
////        if(o == null)
////            return null;
////        else
////        {
////            ObjectHandle Handle = new ObjectHandle( o );
////            return Handle;
////        }
////    }
////
////    //
////    // This API is designed to be used when a host needs to execute code in an AppDomain
////    // with restricted security permissions. In that case, we demand in the client domain
////    // and assert in the server domain because the server domain might not be trusted enough
////    // to pass the security checks when activating the type.
////    //
////
////    [PermissionSetAttribute( SecurityAction.LinkDemand, Unrestricted = true )]
////    public static ObjectHandle CreateInstance( AppDomain domain, string assemblyName, string typeName )
////    {
////        if(domain == null)
////            throw new ArgumentNullException( "domain" );
////        return domain.InternalCreateInstanceWithNoSecurity( assemblyName, typeName );
////    }
////
////    [PermissionSetAttribute( SecurityAction.LinkDemand, Unrestricted = true )]
////    public static ObjectHandle CreateInstance( AppDomain domain,
////                                               string assemblyName,
////                                               string typeName,
////                                               bool ignoreCase,
////                                               BindingFlags bindingAttr,
////                                               Binder binder,
////                                               Object[] args,
////                                               CultureInfo culture,
////                                               Object[] activationAttributes,
////                                               Evidence securityAttributes )
////    {
////        if(domain == null)
////            throw new ArgumentNullException( "domain" );
////        return domain.InternalCreateInstanceWithNoSecurity( assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes );
////    }
////
////    //
////    // This API is designed to be used when a host needs to execute code in an AppDomain
////    // with restricted security permissions. In that case, we demand in the client domain
////    // and assert in the server domain because the server domain might not be trusted enough
////    // to pass the security checks when activating the type.
////    //
////
////    [PermissionSetAttribute( SecurityAction.LinkDemand, Unrestricted = true )]
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static ObjectHandle CreateInstanceFrom( AppDomain domain, string assemblyFile, string typeName )
////    {
////        if(domain == null)
////            throw new ArgumentNullException( "domain" );
////        return domain.InternalCreateInstanceFromWithNoSecurity( assemblyFile, typeName );
////    }
////
////    [PermissionSetAttribute( SecurityAction.LinkDemand, Unrestricted = true )]
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static ObjectHandle CreateInstanceFrom( AppDomain domain,
////                                                   string assemblyFile,
////                                                   string typeName,
////                                                   bool ignoreCase,
////                                                   BindingFlags bindingAttr,
////                                                   Binder binder,
////                                                   Object[] args,
////                                                   CultureInfo culture,
////                                                   Object[] activationAttributes,
////                                                   Evidence securityAttributes )
////    {
////        if(domain == null)
////            throw new ArgumentNullException( "domain" );
////        return domain.InternalCreateInstanceFromWithNoSecurity( assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes );
////    }
////
////    //  This method is a helper method and delegates to the remoting 
////    //  services to do the actual work. 
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.RemotingConfiguration )]
////    static public Object GetObject( Type type, String url )
////    {
////        return GetObject( type, url, null );
////    }
////
////    //  This method is a helper method and delegates to the remoting 
////    //  services to do the actual work. 
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.RemotingConfiguration )]
////    static public Object GetObject( Type type, String url, Object state )
////    {
////        if(type == null)
////            throw new ArgumentNullException( "type" );
////        return RemotingServices.Connect( type, url, state );
////    }
////
////    [System.Diagnostics.Conditional( "_DEBUG" )]
////    private static void Log( bool test, string title, string success, string failure )
////    {
////        if(test)
////            BCLDebug.Trace( "REMOTE", "{0}{1}", title, success );
////        else
////            BCLDebug.Trace( "REMOTE", "{0}{1}", title, failure );
////    }
    }
}

