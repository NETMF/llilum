// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: Assembly
**
**
** Purpose: For Assembly-related stuff.
**
**
=============================================================================*/

namespace System.Reflection
{
    using System;
    using System.Collections;
////using System.Security;
////using System.Security.Policy;
////using System.Security.Permissions;
////using System.IO;
////using System.Reflection.Emit;
////using System.Reflection.Cache;
    using System.Configuration.Assemblies;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Threading;
////using System.Runtime.Versioning;
////using Microsoft.Win32;
////using BinaryFormatter  = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;
////using IEvidenceFactory = System.Security.IEvidenceFactory;
////using CultureInfo      = System.Globalization.CultureInfo;
////using SecurityZone     = System.Security.SecurityZone;
////using StringBuilder    = System.Text.StringBuilder;
////using StackCrawlMark   = System.Threading.StackCrawlMark;
////using __HResults       = System.__HResults;



////[Serializable]
////public delegate Module ModuleResolveEventHandler( Object sender, ResolveEventArgs e );


    [Serializable]
    public class Assembly /*: IEvidenceFactory, ICustomAttributeProvider, ISerializable*/
    {
////    private const String s_localFilePrefix = "file:";
////
////    // READ ME
////    // If you modify any of these fields, you must also update the
////    // AssemblyBaseObject structure in object.h
////    internal AssemblyBuilderData m_assemblyData;
////
////    [method: SecurityPermissionAttribute( SecurityAction.LinkDemand, ControlAppDomain = true )]
////    public event ModuleResolveEventHandler ModuleResolve;
////
////    private InternalCache m_cachedData;
#pragma warning disable 649 // Field 'field' is never assigned to, and will always have its default value 'value'
        private IntPtr        m_assembly;    // slack for ptr datum on unmanaged side
#pragma warning restore 649


////    [ResourceExposure( ResourceScope.Machine )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern void GetCodeBase( AssemblyHandle assembly, bool copiedName, StringHandleOnStack retString );
////
////    internal String GetCodeBase( bool copiedName )
////    {
////        String codeBase = null;
////
////        GetCodeBase( GetNativeHandle(), copiedName, JitHelpers.GetStringHandleOnStack( ref codeBase ) );
////
////        return codeBase;
////    }
////
////    public virtual String CodeBase
////    {
////        [ResourceExposure( ResourceScope.Machine )]
////        [ResourceConsumption( ResourceScope.Machine )]
////        get
////        {
////            String codeBase = GetCodeBase( false );
////
////            VerifyCodeBaseDiscovery( codeBase );
////
////            return codeBase;
////        }
////    }
////
////
////    public virtual String EscapedCodeBase
////    {
////        [ResourceExposure( ResourceScope.Machine )]
////        [ResourceConsumption( ResourceScope.Machine )]
////        get
////        {
////            return AssemblyName.EscapeCodeBase( CodeBase );
////        }
////    }
////
////    public virtual AssemblyName GetName()
////    {
////        return GetName( false );
////    }

        internal AssemblyHandle AssemblyHandle
        {
            get
            {
                return new AssemblyHandle( m_assembly );
            }
        }

////    // Returns handle for interop with EE. The handle is guaranteed to be non-null.
////    internal unsafe AssemblyHandle GetNativeHandle()
////    {
////        IntPtr assembly = m_assembly;
////
////        // This should never happen under normal circumstances. m_assembly is always assigned before it is handed out to the user.
////        // There are ways how to create an unitialized objects through remoting, etc. Avoid AVing in the EE by throwing a nice
////        // exception here.
////        if(assembly.IsNull())
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidHandle" ) );
////        }
////
////        return new AssemblyHandle( assembly );
////    }

////    // If the assembly is copied before it is loaded, the codebase will be set to the
////    // actual file loaded if copiedName is true. If it is false, then the original code base
////    // is returned.
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////    public virtual AssemblyName GetName( bool copiedName )
////    {
////        AssemblyName an = new AssemblyName();
////
////        String codeBase = GetCodeBase( copiedName );
////        VerifyCodeBaseDiscovery( codeBase );
////
////        an.Init( GetSimpleName(),
////                 GetPublicKey(),
////                 null, // public key token
////                 GetVersion(),
////                 GetLocale(),
////                 GetHashAlgorithm(),
////                 AssemblyVersionCompatibility.SameMachine,
////                 codeBase,
////                 GetFlags() | AssemblyNameFlags.PublicKey,
////                 null ); // strong name key pair
////
////        an.ProcessorArchitecture = ComputeProcArchIndex();
////        return an;
////    }
////
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    [ResourceExposure( ResourceScope.None )]
////    private extern static void GetFullName( AssemblyHandle assembly, StringHandleOnStack retString );
////
////    public virtual String FullName
////    {
////        get
////        {
////            // If called by Object.ToString(), return val may be NULL.
////            String s;
////            if((s = (String)Cache[CacheObjType.AssemblyName]) != null)
////            {
////                return s;
////            }
////
////            GetFullName( GetNativeHandle(), JitHelpers.GetStringHandleOnStack( ref s ) );
////            if(s != null)
////            {
////                Cache[CacheObjType.AssemblyName] = s;
////            }
////
////            return s;
////        }
////    }

        public static String CreateQualifiedName( String assemblyName, String typeName )
        {
            return typeName + ", " + assemblyName;
        }

////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private static extern RuntimeMethodHandle GetEntryPoint( AssemblyHandle assembly );
////
////    public virtual MethodInfo EntryPoint
////    {
////        get
////        {
////            RuntimeMethodHandle methodHandle = GetEntryPoint( GetNativeHandle() );
////
////            if(methodHandle.IsNullHandle())
////            {
////                return null;
////            }
////
////            return (MethodInfo)RuntimeType.GetMethodBase( methodHandle );
////        }
////    }
////
////    public static Assembly GetAssembly( Type type )
////    {
////        if(type == null)
////        {
////            throw new ArgumentNullException( "type" );
////        }
////
////        Module m = type.Module;
////        if(m == null)
////        {
////            return null;
////        }
////        else
////        {
////            return m.Assembly;
////        }
////    }
////
////    public virtual Type GetType( String name )
////    {
////        return GetType( name, false, false );
////    }
////
////    public virtual Type GetType( String name, bool throwOnError )
////    {
////        return GetType( name, throwOnError, false );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private static extern RuntimeTypeHandle GetType( AssemblyHandle assembly, String name, bool throwOnError, bool ignoreCase );
////
////    public Type GetType( String name, bool throwOnError, bool ignoreCase )
////    {
////        return GetType( GetNativeHandle(), name, throwOnError, ignoreCase ).GetRuntimeType();
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private extern static void GetExportedTypes( AssemblyHandle assembly, ObjectHandleOnStack retTypes );
////
////    public virtual Type[] GetExportedTypes()
////    {
////        Type[] types = null;
////
////        GetExportedTypes( GetNativeHandle(), JitHelpers.GetObjectHandleOnStack( ref types ) );
////
////        return types;
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine | ResourceScope.Assembly, ResourceScope.Machine | ResourceScope.Assembly )]
////    public virtual Type[] GetTypes()
////    {
////        Module[]       m            = GetModules( true, false );
////        int            iNumModules  = m.Length;
////        int            iFinalLength = 0;
////        StackCrawlMark stackMark    = StackCrawlMark.LookForMyCaller;
////        Type[][]       ModuleTypes  = new Type[iNumModules][];
////
////        for(int i = 0; i < iNumModules; i++)
////        {
////            ModuleTypes[i] = m[i].GetTypes( ref stackMark );
////
////            iFinalLength += ModuleTypes[i].Length;
////        }
////
////        int    iCurrent = 0;
////        Type[] ret      = new Type[iFinalLength];
////        for(int i = 0; i < iNumModules; i++)
////        {
////            int iLength = ModuleTypes[i].Length;
////
////            Array.Copy( ModuleTypes[i], 0, ret, iCurrent, iLength );
////
////            iCurrent += iLength;
////        }
////
////        return ret;
////    }
////
////    // Load a resource based on the NameSpace of the type.
////    public virtual Stream GetManifestResourceStream( Type type, String name )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return GetManifestResourceStream( type, name, false, ref stackMark );
////    }
////
////    public virtual Stream GetManifestResourceStream( String name )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return GetManifestResourceStream( name, ref stackMark, false );
////    }
////
////    public Assembly GetSatelliteAssembly( CultureInfo culture )
////    {
////        return InternalGetSatelliteAssembly( culture, null, true );
////    }
////
////    // Useful for binding to a very specific version of a satellite assembly
////    public Assembly GetSatelliteAssembly( CultureInfo culture, Version version )
////    {
////        return InternalGetSatelliteAssembly( culture, version, true );
////    }
////
////    public virtual Evidence Evidence
////    {
////        [SecurityPermissionAttribute( SecurityAction.Demand, ControlEvidence = true )]
////        get
////        {
////            return nGetEvidence().Copy();
////        }
////    }
////
////
////    // ISerializable implementation
////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter )]
////    public virtual void GetObjectData( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////
////
////        UnitySerializationHolder.GetUnitySerializationInfo( info, UnitySerializationHolder.AssemblyUnity, this.FullName, this );
////    }
////
////    internal bool AptcaCheck( Assembly sourceAssembly )
////    {
////        return AssemblyHandle.AptcaCheck( sourceAssembly.AssemblyHandle );
////    }
////
////    public Module ManifestModule
////    {
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Machine | ResourceScope.Assembly, ResourceScope.Machine | ResourceScope.Assembly )]
////        get
////        {
////            return AssemblyHandle.GetManifestModule().GetModule();
////        }
////    }
////
////    public virtual Object[] GetCustomAttributes( bool inherit )
////    {
////        return CustomAttribute.GetCustomAttributes( this, typeof( object ) as RuntimeType );
////    }
////
////    public virtual Object[] GetCustomAttributes( Type attributeType, bool inherit )
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
////    public virtual bool IsDefined( Type attributeType, bool inherit )
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
////            throw new ArgumentException( Environment.GetResourceString( "Arg_MustBeType" ), "caType" );
////        }
////
////        return CustomAttribute.IsDefined( this, attributeRuntimeType );
////    }
////
////
////    // Locate an assembly by the name of the file containing the manifest.
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static Assembly LoadFrom( String assemblyFile )
////    {
////        // The stack mark is used for MDA filtering
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return InternalLoadFrom( assemblyFile,
////                                 null, // securityEvidence
////                                 null, // hashValue
////                                 AssemblyHashAlgorithm.None,
////                                 false, // forIntrospection
////                                 ref stackMark );
////    }
////
////    // Locate an assembly for reflection by the name of the file containing the manifest.
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static Assembly ReflectionOnlyLoadFrom( String assemblyFile )
////    {
////        // The stack mark is ingored for ReflectionOnlyLoadFrom
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return InternalLoadFrom( assemblyFile,
////                                 null, //securityEvidence
////                                 null, //hashValue
////                                 AssemblyHashAlgorithm.None,
////                                 true,  //forIntrospection
////                                 ref stackMark );
////    }
////
////    // Evidence is protected in Assembly.Load()
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static Assembly LoadFrom( String assemblyFile, Evidence securityEvidence )
////    {
////        // The stack mark is used for MDA filtering
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return InternalLoadFrom( assemblyFile,
////                                 securityEvidence,
////                                 null, // hashValue
////                                 AssemblyHashAlgorithm.None,
////                                 false, // forIntrospection
////                                 ref stackMark );
////    }
////
////    // Evidence is protected in Assembly.Load()
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static Assembly LoadFrom( String assemblyFile, Evidence securityEvidence, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm )
////    {
////        // The stack mark is used for MDA filtering
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return InternalLoadFrom( assemblyFile, securityEvidence, hashValue, hashAlgorithm, false, ref stackMark );
////    }
////
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    private static Assembly InternalLoadFrom( String                assemblyFile     ,
////                                              Evidence              securityEvidence ,
////                                              byte[]                hashValue        ,
////                                              AssemblyHashAlgorithm hashAlgorithm    ,
////                                              bool                  forIntrospection ,
////                                              ref StackCrawlMark    stackMark        )
////    {
////        if(assemblyFile == null)
////        {
////            throw new ArgumentNullException( "assemblyFile" );
////        }
////
////        AssemblyName an = new AssemblyName();
////
////        an.CodeBase = assemblyFile;
////        an.SetHashControl( hashValue, hashAlgorithm );
////
////        return InternalLoad( an, securityEvidence, ref stackMark, forIntrospection );
////    }
////
////
////    // Locate an assembly by the long form of the assembly name.
////    // eg. "Toolbox.dll, version=1.1.10.1220, locale=en, publickey=1234567890123456789012345678901234567890"
////    public static Assembly Load( String assemblyString )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return InternalLoad( assemblyString, null, ref stackMark, false );
////    }
////
////    // Locate an assembly for reflection by the long form of the assembly name.
////    // eg. "Toolbox.dll, version=1.1.10.1220, locale=en, publickey=1234567890123456789012345678901234567890"
////    //
////    public static Assembly ReflectionOnlyLoad( String assemblyString )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return InternalLoad( assemblyString, null, ref stackMark, true /*forIntrospection*/);
////    }
////
////    public static Assembly Load( String assemblyString, Evidence assemblySecurity )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return InternalLoad( assemblyString, assemblySecurity, ref stackMark, false );
////    }
////
////    // Locate an assembly by its name. The name can be strong or
////    // weak. The assembly is loaded into the domain of the caller.
////    static public Assembly Load( AssemblyName assemblyRef )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return InternalLoad( assemblyRef, null, ref stackMark, false );
////    }
////
////
////    static public Assembly Load( AssemblyName assemblyRef, Evidence assemblySecurity )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return InternalLoad( assemblyRef, assemblySecurity, ref stackMark, false );
////    }
////
////    // used by vm
////    static unsafe private IntPtr LoadWithPartialNameHack( String partialName, bool cropPublicKey )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        Assembly result = null;
////        AssemblyName an = new AssemblyName( partialName );
////
////        if(!IsSimplyNamed( an ))
////        {
////            if(cropPublicKey)
////            {
////                an.SetPublicKey( null );
////                an.SetPublicKeyToken( null );
////            }
////
////            AssemblyName GACAssembly = EnumerateCache( an );
////            if(GACAssembly != null)
////            {
////                result = InternalLoad( GACAssembly, null, ref stackMark, false );
////            }
////        }
////
////        if(result == null)
////        {
////            return (IntPtr)0;
////        }
////
////        return (IntPtr)result.AssemblyHandle.Value;
////    }
////
////    [Obsolete( "This method has been deprecated. Please use Assembly.Load() instead. http://go.microsoft.com/fwlink/?linkid=14202" )]
////    static public Assembly LoadWithPartialName( String partialName )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return LoadWithPartialNameInternal( partialName, null, ref stackMark );
////    }
////
////    [Obsolete( "This method has been deprecated. Please use Assembly.Load() instead. http://go.microsoft.com/fwlink/?linkid=14202" )]
////    static public Assembly LoadWithPartialName( String partialName, Evidence securityEvidence )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return LoadWithPartialNameInternal( partialName, securityEvidence, ref stackMark );
////    }
////
////    static internal Assembly LoadWithPartialNameInternal( String partialName, Evidence securityEvidence, ref StackCrawlMark stackMark )
////    {
////        if(securityEvidence != null)
////        {
////            new SecurityPermission( SecurityPermissionFlag.ControlEvidence ).Demand();
////        }
////
////        Assembly result = null;
////        AssemblyName an = new AssemblyName( partialName );
////        try
////        {
////            result = nLoad( an, null, securityEvidence, null, ref stackMark, true, false );
////        }
////        catch(Exception e)
////        {
////            if(e.IsTransient)
////            {
////                throw e;
////            }
////
////            if(IsSimplyNamed( an ))
////            {
////                return null;
////            }
////
////            AssemblyName GACAssembly = EnumerateCache( an );
////            if(GACAssembly != null)
////            {
////                return InternalLoad( GACAssembly, securityEvidence, ref stackMark, false );
////            }
////        }
////
////        return result;
////    }
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern bool IsReflectionOnly( IntPtr assembly );
////
////    public virtual bool ReflectionOnly
////    {
////        get
////        {
////            return IsReflectionOnly( GetNativeHandle().Value );
////        }
////    }
////
////
////    static private AssemblyName EnumerateCache( AssemblyName partialName )
////    {
////        new SecurityPermission( SecurityPermissionFlag.UnmanagedCode ).Assert();
////
////        partialName.Version = null;
////
////        ArrayList a = new ArrayList();
////        Fusion.ReadCache( a, partialName.FullName, ASM_CACHE.GAC );
////
////        IEnumerator  myEnum    = a.GetEnumerator();
////        AssemblyName ainfoBest = null;
////        CultureInfo  refCI     = partialName.CultureInfo;
////
////        while(myEnum.MoveNext())
////        {
////            AssemblyName ainfo = new AssemblyName( (String)myEnum.Current );
////
////            if(CulturesEqual( refCI, ainfo.CultureInfo ))
////            {
////                if(ainfoBest == null)
////                {
////                    ainfoBest = ainfo;
////                }
////                else
////                {
////                    // Choose highest version
////                    if(ainfo.Version > ainfoBest.Version)
////                    {
////                        ainfoBest = ainfo;
////                    }
////                }
////            }
////        }
////
////        return ainfoBest;
////    }
////
////    static private bool CulturesEqual( CultureInfo refCI, CultureInfo defCI )
////    {
////        bool defNoCulture = defCI.Equals( CultureInfo.InvariantCulture );
////
////        // cultured asms aren't allowed to be bound to if
////        // the ref doesn't ask for them specifically
////        if((refCI == null) || refCI.Equals( CultureInfo.InvariantCulture ))
////        {
////            return defNoCulture;
////        }
////
////        if(defNoCulture || (!defCI.Equals( refCI )))
////        {
////            return false;
////        }
////
////        return true;
////    }
////
////    static private bool IsSimplyNamed( AssemblyName partialName )
////    {
////        byte[] pk = partialName.GetPublicKeyToken();
////        if((pk != null) && (pk.Length == 0))
////        {
////            return true;
////        }
////
////        pk = partialName.GetPublicKey();
////        if((pk != null) && (pk.Length == 0))
////        {
////            return true;
////        }
////
////        return false;
////    }
////
////    // Loads the assembly with a COFF based IMAGE containing
////    // an emitted assembly. The assembly is loaded into the domain
////    // of the caller.
////    static public Assembly Load( byte[] rawAssembly )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return nLoadImage( rawAssembly,
////                           null, // symbol store
////                           null, // evidence
////                           ref stackMark,
////                           false  // fIntrospection
////                         );
////    }
////
////    // Loads the assembly for reflection with a COFF based IMAGE containing
////    // an emitted assembly. The assembly is loaded into the domain
////    // of the caller.
////    static public Assembly ReflectionOnlyLoad( byte[] rawAssembly )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return nLoadImage( rawAssembly,
////                           null, // symbol store
////                           null, // evidence
////                           ref stackMark,
////                           true  // fIntrospection
////                         );
////    }
////
////    // Loads the assembly with a COFF based IMAGE containing
////    // an emitted assembly. The assembly is loaded into the domain
////    // of the caller. The second parameter is the raw bytes
////    // representing the symbol store that matches the assembly.
////    static public Assembly Load( byte[] rawAssembly, byte[] rawSymbolStore )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return nLoadImage( rawAssembly,
////                           rawSymbolStore,
////                           null, // evidence
////                           ref stackMark,
////                           false  // fIntrospection
////                         );
////    }
////
////    [SecurityPermissionAttribute( SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlEvidence )]
////    static public Assembly Load( byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence )
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////        return nLoadImage( rawAssembly,
////                           rawSymbolStore,
////                           securityEvidence,
////                           ref stackMark,
////                           false  // fIntrospection
////                         );
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    static public Assembly LoadFile( String path )
////    {
////        new FileIOPermission( FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, path ).Demand();
////
////        return nLoadFile( path, null );
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    [SecurityPermissionAttribute( SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlEvidence )]
////    static public Assembly LoadFile( String path, Evidence securityEvidence )
////    {
////        new FileIOPermission( FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, path ).Demand();
////
////        return nLoadFile( path, securityEvidence );
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public Module LoadModule( String moduleName, byte[] rawModule )
////    {
////        return LoadModule( moduleName, rawModule, null );
////    }
////
////    [ResourceExposure( ResourceScope.Machine | ResourceScope.Assembly )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private extern static ModuleHandle LoadModule( AssemblyHandle assembly      ,
////                                                   String         moduleName    ,
////                                                   byte[]         rawModule     ,
////                                                   int            cbModule      ,
////                                                   byte[]         rawSymbolStore,
////                                                   int            cbSymbolStore );
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    [SecurityPermissionAttribute( SecurityAction.Demand, ControlEvidence = true )]
////    public Module LoadModule( String moduleName, byte[] rawModule, byte[] rawSymbolStore )
////    {
////        return LoadModule( GetNativeHandle(), moduleName, rawModule, (rawModule != null) ? rawModule.Length : 0, rawSymbolStore, (rawSymbolStore != null) ? rawSymbolStore.Length : 0 ).GetModule();
////    }
////
////    //
////    // Locates a type from this assembly and creates an instance of it using
////    // the system activator.
////    //
////    public Object CreateInstance( String typeName )
////    {
////        return CreateInstance( typeName,
////                               false, // ignore case
////                               BindingFlags.Public | BindingFlags.Instance,
////                               null, // binder
////                               null, // args
////                               null, // culture
////                               null ); // activation attributes
////    }
////
////    public Object CreateInstance( String typeName, bool ignoreCase )
////    {
////        return CreateInstance( typeName,
////                               ignoreCase,
////                               BindingFlags.Public | BindingFlags.Instance,
////                               null, // binder
////                               null, // args
////                               null, // culture
////                               null ); // activation attributes
////    }
////
////    public Object CreateInstance( String       typeName             ,
////                                  bool         ignoreCase           ,
////                                  BindingFlags bindingAttr          ,
////                                  Binder       binder               ,
////                                  Object[]     args                 ,
////                                  CultureInfo  culture              ,
////                                  Object[]     activationAttributes )
////    {
////        Type t = GetType( typeName, false, ignoreCase );
////        if(t == null)
////        {
////            return null;
////        }
////
////        return Activator.CreateInstance( t                    ,
////                                         bindingAttr          ,
////                                         binder               ,
////                                         args                 ,
////                                         culture              ,
////                                         activationAttributes );
////    }
////
////    [ResourceExposure( ResourceScope.Machine | ResourceScope.Assembly )]
////    [ResourceConsumption( ResourceScope.Machine | ResourceScope.Assembly )]
////    public Module[] GetLoadedModules()
////    {
////        return GetModules( false, false );
////    }
////
////    [ResourceExposure( ResourceScope.Machine | ResourceScope.Assembly )]
////    [ResourceConsumption( ResourceScope.Machine | ResourceScope.Assembly )]
////    public Module[] GetLoadedModules( bool getResourceModules )
////    {
////        return GetModules( false, getResourceModules );
////    }
////
////    [ResourceExposure( ResourceScope.Machine | ResourceScope.Assembly )]
////    [ResourceConsumption( ResourceScope.Machine | ResourceScope.Assembly )]
////    public Module[] GetModules()
////    {
////        return GetModules( true, false );
////    }
////
////    [ResourceExposure( ResourceScope.Machine | ResourceScope.Assembly )]
////    [ResourceConsumption( ResourceScope.Machine | ResourceScope.Assembly )]
////    public Module[] GetModules( bool getResourceModules )
////    {
////        return GetModules( true, getResourceModules );
////    }
////
////    // Returns the module in this assembly with name 'name'
////
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private static extern ModuleHandle GetModule( AssemblyHandle assembly, String name );
////
////    [ResourceExposure( ResourceScope.Machine | ResourceScope.Assembly )]
////    public Module GetModule( String name )
////    {
////        return GetModule( GetNativeHandle(), name ).GetModule();
////    }
////
////    // Returns the file in the File table of the manifest that matches the
////    // given name.  (Name should not include path.)
////    [ResourceExposure( ResourceScope.Machine | ResourceScope.Assembly )]
////    [ResourceConsumption( ResourceScope.Machine | ResourceScope.Assembly )]
////    public virtual FileStream GetFile( String name )
////    {
////        Module m = GetModule( name );
////        if(m == null)
////        {
////            return null;
////        }
////
////        return new FileStream( m.GetFullyQualifiedName(), FileMode.Open, FileAccess.Read, FileShare.Read );
////    }
////
////    [ResourceExposure( ResourceScope.Machine | ResourceScope.Assembly )]
////    [ResourceConsumption( ResourceScope.Machine | ResourceScope.Assembly )]
////    public virtual FileStream[] GetFiles()
////    {
////        return GetFiles( false );
////    }
////
////    [ResourceExposure( ResourceScope.Machine | ResourceScope.Assembly )]
////    [ResourceConsumption( ResourceScope.Machine | ResourceScope.Assembly )]
////    public virtual FileStream[] GetFiles( bool getResourceModules )
////    {
////        Module[]     m       = GetModules( true, getResourceModules );
////        int          iLength = m.Length;
////        FileStream[] fs      = new FileStream[iLength];
////
////        for(int i = 0; i < iLength; i++)
////        {
////            fs[i] = new FileStream( m[i].GetFullyQualifiedName(), FileMode.Open, FileAccess.Read, FileShare.Read );
////        }
////
////        return fs;
////    }
////
////
////    // Returns the names of all the resources
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern String[] GetManifestResourceNames( IntPtr assembly );
////
////    // Returns the names of all the resources
////    public virtual String[] GetManifestResourceNames()
////    {
////        return GetManifestResourceNames( GetNativeHandle().Value );
////    }
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private extern static AssemblyHandle GetExecutingAssembly( StackCrawlMarkHandle stackMark );
////
////    internal static Assembly GetExecutingAssembly( ref StackCrawlMark stackMark )
////    {
////        return GetExecutingAssembly( JitHelpers.GetStackCrawlMarkHandle( ref stackMark ) ).GetAssembly();
////    }
////
////    /*
////     * Get the assembly that the current code is running from.
////     */
////    public static Assembly GetExecutingAssembly()
////    {
////        // passing address of local will also prevent inlining
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        return GetExecutingAssembly( ref stackMark );
////    }
////
////    public static Assembly GetCallingAssembly()
////    {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCallersCaller;
////
////        return GetExecutingAssembly( ref stackMark );
////    }
////
////
////    public static Assembly GetEntryAssembly()
////    {
////        AppDomainManager domainManager = AppDomain.CurrentDomain.DomainManager;
////        if(domainManager == null)
////        {
////            domainManager = new AppDomainManager();
////        }
////
////        return domainManager.EntryAssembly;
////    }
////
////
////    // Returns the names of all the resources
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [ResourceExposure( ResourceScope.None )]
////    private static extern AssemblyName[] GetReferencedAssemblies( IntPtr assembly );
////
////    public AssemblyName[] GetReferencedAssemblies()
////    {
////        return GetReferencedAssemblies( GetNativeHandle().Value );
////    }
////
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private static extern int GetManifestResourceInfo( AssemblyHandle       assembly    ,
////                                                       String               resourceName,
////                                                       out AssemblyHandle   assemblyRef ,
////                                                       StringHandleOnStack  retFileName ,
////                                                       StackCrawlMarkHandle stackMark   );
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public virtual ManifestResourceInfo GetManifestResourceInfo( String resourceName )
////    {
////        AssemblyHandle refAssembly;
////        String         fileName  = null;
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        int location = GetManifestResourceInfo( GetNativeHandle(), resourceName, out refAssembly,
////                                               JitHelpers.GetStringHandleOnStack( ref fileName ),
////                                               JitHelpers.GetStackCrawlMarkHandle( ref stackMark ) );
////
////        if(location == -1)
////        {
////            return null;
////        }
////
////        return new ManifestResourceInfo( refAssembly.GetAssembly(), fileName, (ResourceLocation)location );
////    }
////
////
////    public override String ToString()
////    {
////        String displayName = FullName;
////        if(displayName == null)
////        {
////            return base.ToString();
////        }
////        else
////        {
////            return displayName;
////        }
////    }
////
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private static extern void GetLocation( AssemblyHandle assembly, StringHandleOnStack retString );
////
////    public virtual String Location
////    {
////        [ResourceExposure( ResourceScope.Machine )]
////        [ResourceConsumption( ResourceScope.Machine )]
////        get
////        {
////            String location = null;
////
////            GetLocation( GetNativeHandle(), JitHelpers.GetStringHandleOnStack( ref location ) );
////
////            if(location != null)
////            {
////                new FileIOPermission( FileIOPermissionAccess.PathDiscovery, location ).Demand();
////            }
////
////            return location;
////        }
////    }
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private extern static void GetImageRuntimeVersion( AssemblyHandle assembly, StringHandleOnStack retString );
////
////    public virtual String ImageRuntimeVersion
////    {
////        get
////        {
////            String s = null;
////
////            GetImageRuntimeVersion( GetNativeHandle(), JitHelpers.GetStringHandleOnStack( ref s ) );
////
////            return s;
////        }
////    }
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern static bool IsGlobalAssemblyCache( IntPtr assembly );
////
////    /*
////      Returns true if the assembly was loaded from the global assembly cache.
////    */
////
////    public bool GlobalAssemblyCache
////    {
////        get
////        {
////            return IsGlobalAssemblyCache( GetNativeHandle().Value );
////        }
////    }
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private extern static Int64 GetHostContext( AssemblyHandle assembly );
////
////    public Int64 HostContext
////    {
////        get
////        {
////            return GetHostContext( GetNativeHandle() );
////        }
////    }
////
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    internal static String VerifyCodeBase( String codebase )
////    {
////        if(codebase == null)
////        {
////            return null;
////        }
////
////        int len = codebase.Length;
////        if(len == 0)
////        {
////            return null;
////        }
////
////
////        int j = codebase.IndexOf( ':' );
////        // Check to see if the url has a prefix
////        if((j != -1) && (j + 2 < len) &&
////            ((codebase[j + 1] == '/') || (codebase[j + 1] == '\\')) &&
////            ((codebase[j + 2] == '/') || (codebase[j + 2] == '\\')))
////        {
////            return codebase;
////        }
////        else if((len > 2) && (codebase[0] == '\\') && (codebase[1] == '\\'))
////        {
////            return "file://" + codebase;
////        }
////        else
////        {
////            return "file:///" + Path.GetFullPathInternal( codebase );
////        }
////    }
////
////    internal virtual Stream GetManifestResourceStream( Type type, String name, bool skipSecurityCheck, ref StackCrawlMark stackMark )
////    {
////        StringBuilder sb = new StringBuilder();
////        if(type == null)
////        {
////            if(name == null)
////            {
////                throw new ArgumentNullException( "type" );
////            }
////        }
////        else
////        {
////            String nameSpace = type.Namespace;
////            if(nameSpace != null)
////            {
////                sb.Append( nameSpace );
////                if(name != null)
////                {
////                    sb.Append( Type.Delimiter );
////                }
////            }
////        }
////
////        if(name != null)
////        {
////            sb.Append( name );
////        }
////
////        return GetManifestResourceStream( sb.ToString(), ref stackMark, skipSecurityCheck );
////    }
////
////    internal Assembly()
////    {
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern static IntPtr GetOnDiskAssemblyModule( IntPtr assembly );
////
////    internal Module GetOnDiskAssemblyModule()
////    {
////        return new ModuleHandle( GetOnDiskAssemblyModule( GetNativeHandle().Value ) ).GetModule();
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private extern static IntPtr GetInMemoryAssemblyModule( IntPtr assembly );
////
////    internal Module GetInMemoryAssemblyModule()
////    {
////        return new ModuleHandle( GetInMemoryAssemblyModule( GetNativeHandle().Value ) ).GetModule();
////    }
////
////    private static void DecodeSerializedEvidence( Evidence evidence, byte[] serializedEvidence )
////    {
////        BinaryFormatter formatter = new BinaryFormatter();
////
////        Evidence asmEvidence = null;
////
////        PermissionSet permSet = new PermissionSet( false );
////        permSet.SetPermission( new SecurityPermission( SecurityPermissionFlag.SerializationFormatter ) );
////        permSet.PermitOnly();
////        permSet.Assert();
////
////        try
////        {
////            using(MemoryStream ms = new MemoryStream( serializedEvidence ))
////            {
////                asmEvidence = (Evidence)formatter.Deserialize( ms );
////            }
////        }
////        catch
////        {
////        }
////
////        if(asmEvidence != null)
////        {
////            IEnumerator enumerator = asmEvidence.GetAssemblyEnumerator();
////
////            while(enumerator.MoveNext())
////            {
////                Object obj = enumerator.Current;
////
////                evidence.AddAssembly( obj );
////            }
////        }
////    }
////
////    private static void AddX509Certificate( Evidence evidence, byte[] cert )
////    {
////        evidence.AddHost( new Publisher( new System.Security.Cryptography.X509Certificates.X509Certificate( cert ) ) );
////    }
////
////    private static void AddStrongName( Evidence evidence, byte[] blob, String strSimpleName, int major, int minor, int build, int revision )
////    {
////        evidence.AddHost( new StrongName( new StrongNamePublicKeyBlob( blob ), strSimpleName, new Version( major, minor, build, revision ) ) );
////    }
////
////    private static Evidence CreateSecurityIdentity( Assembly asm                ,
////                                                    String   strUrl             ,
////                                                    int      zone               ,
////                                                    byte[]   cert               ,
////                                                    byte[]   publicKeyBlob      ,
////                                                    String   strSimpleName      ,
////                                                    int      major              ,
////                                                    int      minor              ,
////                                                    int      build              ,
////                                                    int      revision           ,
////                                                    byte[]   serializedEvidence ,
////                                                    Evidence additionalEvidence )
////    {
////        Evidence evidence = new Evidence();
////
////        if(zone != -1)
////        {
////            evidence.AddHost( new Zone( (SecurityZone)zone ) );
////        }
////
////        if(strUrl != null)
////        {
////            evidence.AddHost( new Url( strUrl, true ) );
////
////            // Only create a site piece of evidence if we are not loading from a file.
////            if(String.Compare( strUrl, 0, s_localFilePrefix, 0, 5, StringComparison.OrdinalIgnoreCase ) != 0)
////            {
////                evidence.AddHost( Site.CreateFromUrl( strUrl ) );
////            }
////        }
////
////        if(cert != null)
////        {
////            AddX509Certificate( evidence, cert );
////        }
////
////        // Determine if it's in the GAC and add some evidence about it
////        if(asm != null && System.Runtime.InteropServices.RuntimeEnvironment.FromGlobalAccessCache( asm ))
////        {
////            evidence.AddHost( new GacInstalled() );
////        }
////
////        // This code was moved to a different function because:
////        // 1) it is rarely called so we should only JIT it if we need it.
////        // 2) it references lots of classes that otherwise aren't loaded.
////        if(serializedEvidence != null)
////        {
////            DecodeSerializedEvidence( evidence, serializedEvidence );
////        }
////
////        if((publicKeyBlob != null) && (publicKeyBlob.Length != 0))
////        {
////            AddStrongName( evidence, publicKeyBlob, strSimpleName, major, minor, build, revision );
////        }
////
////        if(asm != null && !asm.IsDynamic())
////        {
////            evidence.AddHost( new Hash( asm ) );
////        }
////
////        // If the host (caller of Assembly.Load) provided evidence, merge it
////        // with the evidence we've just created. The host evidence takes
////        // priority.
////        if(additionalEvidence != null)
////        {
////            evidence.MergeWithNoDuplicates( additionalEvidence );
////        }
////
////        if(asm != null)
////        {
////            // The host might want to modify the evidence of the assembly through
////            // the HostSecurityManager provided in AppDomainManager, so take that into account.
////            HostSecurityManager securityManager = AppDomain.CurrentDomain.HostSecurityManager;
////
////            if((securityManager.Flags & HostSecurityManagerOptions.HostAssemblyEvidence) == HostSecurityManagerOptions.HostAssemblyEvidence)
////            {
////                return securityManager.ProvideAssemblyEvidence( asm, evidence );
////            }
////        }
////
////        return evidence;
////    }
////
////    // GetResource will return a pointer to the resources in memory.
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private static unsafe extern byte* GetResource( AssemblyHandle       assembly          ,
////                                                    String               resourceName      ,
////                                                    out ulong            length            ,
////                                                    StackCrawlMarkHandle stackMark         ,
////                                                    bool                 skipSecurityCheck );
////
////    internal unsafe virtual Stream GetManifestResourceStream( String name, ref StackCrawlMark stackMark, bool skipSecurityCheck )
////    {
////        ulong length             = 0;
////        byte* pbInMemoryResource = GetResource( GetNativeHandle(), name, out length, JitHelpers.GetStackCrawlMarkHandle( ref stackMark ), skipSecurityCheck );
////
////        if(pbInMemoryResource != null)
////        {
////            //Console.WriteLine("Creating an unmanaged memory stream of length "+length);
////            if(length > Int64.MaxValue)
////            {
////                throw new NotImplementedException( Environment.GetResourceString( "NotImplemented_ResourcesLongerThan2^63" ) );
////            }
////
////            // <STRIP>For cases where we're loading an embedded resource from an assembly,
////            // in V1 we do not have any serious lifetime issues with the
////            // UnmanagedMemoryStream.  If the Stream is only used
////            // in the AppDomain that contains the assembly, then if that AppDomain
////            // is unloaded, we will collect all of the objects in the AppDomain first
////            // before unloading assemblies.  If the Stream is shared across AppDomains,
////            // then the original AppDomain was unloaded, accesses to this Stream will
////            // throw an exception saying the appdomain was unloaded.  This is
////            // guaranteed be EE AppDomain goo.  And for shared assemblies like
////            // mscorlib, their lifetime is the lifetime of the process, so the
////            // assembly will NOT be unloaded, so the resource will always be in memory.</STRIP>
////            return new UnmanagedMemoryStream( pbInMemoryResource, (long)length, (long)length, FileAccess.Read, true );
////        }
////
////        //Console.WriteLine("GetManifestResourceStream: Blob "+name+" not found...");
////        return null;
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private static extern void GetVersion( AssemblyHandle assembly,
////                                           out int        majVer  ,
////                                           out int        minVer  ,
////                                           out int        buildNum,
////                                           out int        revNum  );
////
////    internal Version GetVersion()
////    {
////        int majorVer;
////        int minorVer;
////        int build;
////        int revision;
////
////        GetVersion( GetNativeHandle(), out majorVer, out minorVer, out build, out revision );
////
////        return new Version( majorVer, minorVer, build, revision );
////    }
////
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private static extern void GetLocale( AssemblyHandle assembly, StringHandleOnStack retString );
////
////    internal CultureInfo GetLocale()
////    {
////        String locale = null;
////
////        GetLocale( GetNativeHandle(), JitHelpers.GetStringHandleOnStack( ref locale ) );
////
////        if(locale == null)
////        {
////            return CultureInfo.InvariantCulture;
////        }
////
////        return new CultureInfo( locale );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern bool IsDynamic( IntPtr assembly );
////
////    internal bool IsDynamic()
////    {
////        return IsDynamic( GetNativeHandle().Value );
////    }
////
////    private void VerifyCodeBaseDiscovery( String codeBase )
////    {
////        if((codeBase != null) && (String.Compare( codeBase, 0, s_localFilePrefix, 0, 5, StringComparison.OrdinalIgnoreCase ) == 0))
////        {
////            System.Security.Util.URLString urlString = new System.Security.Util.URLString( codeBase, true );
////
////            new FileIOPermission( FileIOPermissionAccess.PathDiscovery, urlString.GetFileName() ).Demand();
////        }
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private static extern void GetSimpleName( AssemblyHandle assembly, StringHandleOnStack retSimpleName );
////
////    internal String GetSimpleName()
////    {
////        string name = null;
////
////        GetSimpleName( GetNativeHandle(), JitHelpers.GetStringHandleOnStack( ref name ) );
////
////        return name;
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private extern static AssemblyHashAlgorithm GetHashAlgorithm( AssemblyHandle assembly );
////
////    AssemblyHashAlgorithm GetHashAlgorithm()
////    {
////        return GetHashAlgorithm( GetNativeHandle() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private extern static AssemblyNameFlags GetFlags( AssemblyHandle assembly );
////
////    AssemblyNameFlags GetFlags()
////    {
////        return GetFlags( GetNativeHandle() );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private static extern void GetPublicKey( AssemblyHandle assembly, ObjectHandleOnStack retPublicKey );
////
////    internal byte[] GetPublicKey()
////    {
////        byte[] publicKey = null;
////
////        GetPublicKey( GetNativeHandle(), JitHelpers.GetObjectHandleOnStack( ref publicKey ) );
////
////        return publicKey;
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal extern Evidence nGetEvidence();
////
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    [ResourceExposure( ResourceScope.None )]
////    private extern static void GetGrantSet( AssemblyHandle assembly, ObjectHandleOnStack granted, ObjectHandleOnStack denied );
////
////    internal void GetGrantSet( out PermissionSet newGrant, out PermissionSet newDenied )
////    {
////        PermissionSet granted = null, denied = null;
////
////        GetGrantSet( GetNativeHandle(), JitHelpers.GetObjectHandleOnStack( ref granted ), JitHelpers.GetObjectHandleOnStack( ref denied ) );
////
////        newGrant = granted; newDenied = denied;
////    }
////
////    internal static Assembly InternalLoad( String                  assemblyString   ,
////                                           Evidence                assemblySecurity ,
////                                           ref      StackCrawlMark stackMark        ,
////                                           bool                    forIntrospection )
////    {
////        if(assemblyString == null)
////        {
////            throw new ArgumentNullException( "assemblyString" );
////        }
////        if((assemblyString.Length == 0) || (assemblyString[0] == '\0'))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Format_StringZeroLength" ) );
////        }
////
////        AssemblyName an       = new AssemblyName();
////        Assembly     assembly = null;
////
////        an.Name = assemblyString;
////        int hr = an.nInit( out assembly, forIntrospection, true );
////
////        if(hr == System.__HResults.FUSION_E_INVALID_NAME)
////        {
////            return assembly;
////        }
////
////        return InternalLoad( an, assemblySecurity, ref stackMark, forIntrospection );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////    internal static Assembly InternalLoad( AssemblyName       assemblyRef      ,
////                                           Evidence           assemblySecurity ,
////                                           ref StackCrawlMark stackMark        ,
////                                           bool               forIntrospection )
////    {
////        if(assemblyRef == null)
////        {
////            throw new ArgumentNullException( "assemblyRef" );
////        }
////
////        assemblyRef = (AssemblyName)assemblyRef.Clone();
////        if(assemblySecurity != null)
////        {
////            new SecurityPermission( SecurityPermissionFlag.ControlEvidence ).Demand();
////        }
////
////        String codeBase = VerifyCodeBase( assemblyRef.CodeBase );
////        if(codeBase != null)
////        {
////
////            if(String.Compare( codeBase, 0, s_localFilePrefix, 0, 5, StringComparison.OrdinalIgnoreCase ) != 0)
////            {
////                IPermission perm = CreateWebPermission( assemblyRef.EscapedCodeBase );
////                perm.Demand();
////            }
////            else
////            {
////                System.Security.Util.URLString urlString = new System.Security.Util.URLString( codeBase, true );
////
////                new FileIOPermission( FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, urlString.GetFileName() ).Demand();
////            }
////        }
////
////        return nLoad( assemblyRef, codeBase, assemblySecurity, null, ref stackMark, true, forIntrospection );
////    }
////
////    // demandFlag:
////    // 0 demand PathDiscovery permission only
////    // 1 demand Read permission only
////    // 2 demand both Read and PathDiscovery
////    // 3 demand Web permission only
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////    private static void DemandPermission( String codeBase, bool havePath, int demandFlag )
////    {
////        FileIOPermissionAccess access = FileIOPermissionAccess.PathDiscovery;
////        switch(demandFlag)
////        {
////
////            case 0: // default
////                break;
////
////            case 1:
////                access = FileIOPermissionAccess.Read;
////                break;
////
////            case 2:
////                access = FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read;
////                break;
////
////            case 3:
////                IPermission perm = CreateWebPermission( AssemblyName.EscapeCodeBase( codeBase ) );
////                perm.Demand();
////                return;
////        }
////
////        if(!havePath)
////        {
////            System.Security.Util.URLString urlString = new System.Security.Util.URLString( codeBase, true );
////
////            codeBase = urlString.GetFileName();
////        }
////
////        codeBase = Path.GetFullPathInternal( codeBase );  // canonicalize
////
////        new FileIOPermission( access, codeBase ).Demand();
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static extern Assembly nLoad( AssemblyName       fileName           ,
////                                          String             codeBase           ,
////                                          Evidence           assemblySecurity   ,
////                                          Assembly           locationHint       ,
////                                          ref StackCrawlMark stackMark          ,
////                                          bool               throwOnFileNotFound,
////                                          bool               forIntrospection   );
////
////    private static IPermission CreateWebPermission( String codeBase )
////    {
////        BCLDebug.Assert( codeBase != null, "Must pass in a valid CodeBase" );
////        Assembly sys = Assembly.Load( "System, Version=" + ThisAssembly.Version + ", Culture=neutral, PublicKeyToken=" + AssemblyRef.EcmaPublicKeyToken );
////
////        Type type = sys.GetType( "System.Net.NetworkAccess", true );
////
////        IPermission retval = null;
////        if(!type.IsEnum || !type.IsVisible)
////        {
////            goto Exit;
////        }
////
////        Object[] webArgs = new Object[2];
////        webArgs[0] = (Enum)Enum.Parse( type, "Connect", true );
////        if(webArgs[0] == null)
////        {
////            goto Exit;
////        }
////
////        webArgs[1] = codeBase;
////
////        type = sys.GetType( "System.Net.WebPermission", true );
////
////        if(!type.IsVisible)
////        {
////            goto Exit;
////        }
////
////        retval = (IPermission)Activator.CreateInstance( type, webArgs );
////
////        Exit:
////        if(retval == null)
////        {
////            BCLDebug.Assert( false, "Unable to create WebPermission" );
////            throw new ExecutionEngineException();
////        }
////
////        return retval;
////    }
////
////
////    private Module OnModuleResolveEvent( String moduleName )
////    {
////        ModuleResolveEventHandler moduleResolve = ModuleResolve;
////        if(moduleResolve == null)
////        {
////            return null;
////        }
////
////        Delegate[] ds = moduleResolve.GetInvocationList();
////        int len = ds.Length;
////        for(int i = 0; i < len; i++)
////        {
////            Module ret = ((ModuleResolveEventHandler)ds[i])( this, new ResolveEventArgs( moduleName ) );
////            if(ret != null)
////            {
////                return ret;
////            }
////        }
////
////        return null;
////    }
////
////
////    internal Assembly InternalGetSatelliteAssembly( CultureInfo culture, Version version, bool throwOnFileNotFound )
////    {
////        if(culture == null)
////        {
////            throw new ArgumentNullException( "culture" );
////        }
////
////        AssemblyName an = new AssemblyName();
////
////        an.SetPublicKey( GetPublicKey() );
////        an.Flags = GetFlags() | AssemblyNameFlags.PublicKey;
////
////        if(version == null)
////        {
////            an.Version = GetVersion();
////        }
////        else
////        {
////            an.Version = version;
////        }
////
////        an.CultureInfo = culture;
////        an.Name        = GetSimpleName() + ".resources";
////
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////
////        Assembly a = nLoad( an, null, null, this, ref stackMark, throwOnFileNotFound, false );
////        if(a == this)
////        {
////            throw new FileNotFoundException( String.Format( culture, Environment.GetResourceString( "IO.FileNotFound_FileName" ), an.Name ) );
////        }
////
////        return a;
////    }
////
////    internal InternalCache Cache
////    {
////        get
////        {
////            // This grabs an internal copy of m_cachedData and uses
////            // that instead of looking at m_cachedData directly because
////            // the cache may get cleared asynchronously.  This prevents
////            // us from having to take a lock.
////            InternalCache cache = m_cachedData;
////            if(cache == null)
////            {
////                cache = new InternalCache( "Assembly" );
////
////                m_cachedData = cache;
////
////                GC.ClearCache += new ClearCacheHandler( OnCacheClear );
////            }
////
////            return cache;
////        }
////    }
////
////    internal void OnCacheClear( Object sender, ClearCacheEventArgs cacheEventArgs )
////    {
////        m_cachedData = null;
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    static internal extern Assembly nLoadFile( String path, Evidence evidence );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    static internal extern Assembly nLoadImage( byte[]             rawAssembly    ,
////                                                byte[]             rawSymbolStore ,
////                                                Evidence           evidence       ,
////                                                ref StackCrawlMark stackMark      ,
////                                                bool               fIntrospection );
////
////
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    [SuppressUnmanagedCodeSecurity]
////    private extern static void GetModules( AssemblyHandle      assembly          ,
////                                           bool                loadIfNotFound    ,
////                                           bool                getResourceModules,
////                                           ObjectHandleOnStack retModuleHandles  );
////
////    [ResourceExposure( ResourceScope.Machine | ResourceScope.Assembly )]
////    internal Module[] GetModules( bool loadIfNotFound, bool getResourceModules )
////    {
////        Module[] modules = null;
////
////        GetModules( GetNativeHandle(), loadIfNotFound, getResourceModules, JitHelpers.GetObjectHandleOnStack( ref modules ) );
////
////        return modules;
////    }
////
////
////    internal ProcessorArchitecture ComputeProcArchIndex()
////    {
////        PortableExecutableKinds pek;
////        ImageFileMachine        ifm;
////
////        Module manifestModule = ManifestModule;
////        if(manifestModule != null)
////        {
////            if(manifestModule.MDStreamVersion > 0x10000)
////            {
////                ManifestModule.GetPEKind( out pek, out ifm );
////                if((pek & System.Reflection.PortableExecutableKinds.PE32Plus) == System.Reflection.PortableExecutableKinds.PE32Plus)
////                {
////                    switch(ifm)
////                    {
////                        case System.Reflection.ImageFileMachine.IA64:
////                            return ProcessorArchitecture.IA64;
////
////                        case System.Reflection.ImageFileMachine.AMD64:
////                            return ProcessorArchitecture.Amd64;
////
////                        case System.Reflection.ImageFileMachine.I386:
////                            if((pek & System.Reflection.PortableExecutableKinds.ILOnly) == System.Reflection.PortableExecutableKinds.ILOnly)
////                            {
////                                return ProcessorArchitecture.MSIL;
////                            }
////                            break;
////                    }
////                }
////
////                else
////                {
////                    if(ifm == System.Reflection.ImageFileMachine.I386)
////                    {
////                        if((pek & System.Reflection.PortableExecutableKinds.Required32Bit) == System.Reflection.PortableExecutableKinds.Required32Bit)
////                        {
////                            return ProcessorArchitecture.X86;
////                        }
////
////                        if((pek & System.Reflection.PortableExecutableKinds.ILOnly) == System.Reflection.PortableExecutableKinds.ILOnly)
////                        {
////                            return ProcessorArchitecture.MSIL;
////                        }
////
////                        return ProcessorArchitecture.X86;
////                    }
////                }
////            }
////        }
////
////        return ProcessorArchitecture.None;
////    }
    }
}

