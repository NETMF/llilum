// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  ResourceManager
**
**
** Purpose: Default way to access String resources from 
** an assembly.
**
** 
===========================================================*/
namespace System.Resources
{
    using System;
    using System.IO;
    using System.Globalization;
    using System.Collections;
    using System.Text;
    using System.Reflection;
    using System.Runtime.CompilerServices;
////using System.Runtime.Remoting.Activation;
////using System.Runtime.Serialization;
////using System.Security.Permissions;
    using System.Threading;
    using System.Runtime.InteropServices;
////using Microsoft.Win32;
    using System.Collections.Generic;
////using System.Runtime.Versioning;

    // Resource Manager exposes an assembly's resources to an application for
    // the correct CultureInfo.  An example would be localizing text for a 
    // user-visible message.  Create a set of resource files listing a name 
    // for a message and its value, compile them using ResGen, put them in
    // an appropriate place (your assembly manifest(?)), then create a Resource 
    // Manager and query for the name of the message you want.  The Resource
    // Manager will use CultureInfo.GetCurrentUICulture() to look
    // up a resource for your user's locale settings.
    // 
    // Users should ideally create a resource file for every culture, or
    // at least a meaningful subset.  The filenames will follow the naming 
    // scheme:
    // 
    // basename.culture name.resources
    // 
    // The base name can be the name of your application, or depending on 
    // the granularity desired, possibly the name of each class.  The culture 
    // name is determined from CultureInfo's Name property.  
    // An example file name may be MyApp.en-US.resources for
    // MyApp's US English resources.
    // 
    [Microsoft.Zelig.Internals.WellKnownType( "System_Resources_ResourceManager" )]
    [Serializable]
    public class ResourceManager
    {
        protected String BaseNameField;
////    // Sets is a many-to-one table of CultureInfos mapped to ResourceSets.
////    // Don't synchronize ResourceSets - too fine-grained a lock to be effective
////    protected Hashtable ResourceSets;
////    private String moduleDir;      // For assembly-ignorant directory location
        protected Assembly MainAssembly;   // Need the assembly manifest sometimes.
////    private Type _locationInfo;    // For Assembly or type-based directory layout
////    private Type _userResourceSet;  // Which ResourceSet instance to create
////    private CultureInfo _neutralResourcesCulture;  // For perf optimizations.
////
////    private bool _ignoreCase;   // Whether case matters in GetString & GetObject
////    // When we create a separate FileBasedResourceManager subclass,
////    // UseManifest can be replaced with "false".
////    private bool UseManifest;  // Use Assembly manifest, or grovel disk.
////
////    // When we create a separate FileBasedResourceManager subclass,
////    // UseSatelliteAssem can be replaced with "true".
////    private bool UseSatelliteAssem;  // Are all the .resources files in the 
////    // main assembly, or in satellite assemblies for each culture?
////
////    // Whether to fall back to the main assembly or a particular 
////    // satellite for the neutral resources.
////    [OptionalField]
////    private UltimateResourceFallbackLocation _fallbackLoc;
////    // Version number of satellite assemblies to look for.  May be null.
////    [OptionalField]
////    private Version _satelliteContractVersion;
////    [OptionalField]
////    private bool _lookedForSatelliteContractVersion;
////
////    private Assembly _callingAssembly;  // Assembly who created the ResMgr.
////
////    public static readonly int MagicNumber = unchecked( (int)0xBEEFCACE );  // If only hex had a K...
////
////    // Version number so ResMgr can get the ideal set of classes for you.
////    // ResMgr header is:
////    // 1) MagicNumber (little endian Int32)
////    // 2) HeaderVersionNumber (little endian Int32)
////    // 3) Num Bytes to skip past ResMgr header (little endian Int32)
////    // 4) IResourceReader type name for this file (bytelength-prefixed UTF-8 String)
////    // 5) ResourceSet type name for this file (bytelength-prefixed UTF8 String)
////    public static readonly int HeaderVersionNumber = 1;
////
////    //
////    //It would be better if we could use _neutralCulture instead of calling
////    //CultureInfo.InvariantCulture everywhere, but we run into problems with the .cctor.  CultureInfo 
////    //initializes assembly, which initializes ResourceManager, which tries to get a CultureInfo which isn't
////    //there yet because CultureInfo's class initializer hasn't finished.  If we move SystemResMgr off of
////    //Assembly (or at least make it an internal property) we should be able to circumvent this problem.
////    //
////    //      private static CultureInfo _neutralCulture = null;
////
////    // This is our min required ResourceSet type.
////    private static readonly Type _minResourceSet = typeof( ResourceSet );
////    // These Strings are used to avoid using Reflection in CreateResourceSet.
////    // The first set are used by ResourceWriter.  The second are used by
////    // InternalResGen.
////    internal static readonly String ResReaderTypeName = typeof( ResourceReader ).FullName;
////    internal static readonly String ResSetTypeName = typeof( RuntimeResourceSet ).FullName;
////    internal static readonly String MscorlibName = typeof( ResourceReader ).Assembly.FullName;
////    internal const String ResFileExtension = ".resources";
////    internal const int ResFileExtensionLength = 10;
////
////    // My private debugging aid.  Set to 5 or 6 for verbose output.  Set to 3
////    // for summary level information.
////    internal static readonly int DEBUG = 0; //Making this const causes C# to consider all of the code that it guards unreachable.

        protected ResourceManager()
        {
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////        _callingAssembly = Assembly.GetExecutingAssembly( ref stackMark );
        }

////    // Constructs a Resource Manager for files beginning with 
////    // baseName in the directory specified by resourceDir
////    // or in the current directory.  This Assembly-ignorant constructor is 
////    // mostly useful for testing your own ResourceSet implementation.
////    //
////    // A good example of a baseName might be "Strings".  BaseName 
////    // should not end in ".resources".
////    //
////    // Note: System.Windows.Forms uses this method at design time.
////    // 
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    private ResourceManager( String baseName, String resourceDir, Type usingResourceSet )
////    {
////
////        //
////        // __FileBasedResourceManager: We should move all the file-based functionality into a 
////        // (potentially internal) subclass of ResourceManager.  This will simplify
////        // InternalGetResourceSet a lot.  We can also make several fields like UseManifest
////        // private.  
////        //
////
////
////        if(null == baseName)
////            throw new ArgumentNullException( "baseName" );
////        if(null == resourceDir)
////            throw new ArgumentNullException( "resourceDir" );
////
////        BaseNameField = baseName;
////
////        moduleDir = resourceDir;
////        _userResourceSet = usingResourceSet;
////        ResourceSets = new Hashtable();
////        UseManifest = false;
////    }

        public ResourceManager( String baseName, Assembly assembly )
        {
////        if(null == baseName)
////            throw new ArgumentNullException( "baseName" );
////        if(null == assembly)
////            throw new ArgumentNullException( "assembly" );
    
            MainAssembly = assembly;
////        _locationInfo = null;
            BaseNameField = baseName;
    
////        CommonSatelliteAssemblyInit();
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////        _callingAssembly = Assembly.GetExecutingAssembly( ref stackMark );
////        // Special case for mscorlib - protect mscorlib's private resources.
////        // This isn't for security reasons, but to ensure we can make
////        // breaking changes to mscorlib's internal resources without 
////        // assuming users may have taken a dependency on them.
////        if(assembly == typeof( Object ).Assembly && _callingAssembly != assembly)
////            _callingAssembly = null;
        }

////    public ResourceManager( String baseName, Assembly assembly, Type usingResourceSet )
////    {
////        if(null == baseName)
////            throw new ArgumentNullException( "baseName" );
////        if(null == assembly)
////            throw new ArgumentNullException( "assembly" );
////
////        MainAssembly = assembly;
////        _locationInfo = null;
////        BaseNameField = baseName;
////
////        if(usingResourceSet != null && (usingResourceSet != _minResourceSet) && !(usingResourceSet.IsSubclassOf( _minResourceSet )))
////            throw new ArgumentException( Environment.GetResourceString( "Arg_ResMgrNotResSet" ), "usingResourceSet" );
////        _userResourceSet = usingResourceSet;
////
////        CommonSatelliteAssemblyInit();
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////        _callingAssembly = Assembly.GetExecutingAssembly( ref stackMark );
////        // Special case for mscorlib - protect mscorlib's private resources.
////        // This isn't for security reasons, but to ensure we can make
////        // breaking changes to mscorlib's internal resources without 
////        // assuming users may have taken a dependency on them.
////        if(assembly == typeof( Object ).Assembly && _callingAssembly != assembly)
////            _callingAssembly = null;
////    }
////
////    public ResourceManager( Type resourceSource )
////    {
////        if(null == resourceSource)
////            throw new ArgumentNullException( "resourceSource" );
////
////        _locationInfo = resourceSource;
////        MainAssembly = _locationInfo.Assembly;
////        BaseNameField = resourceSource.Name;
////
////        CommonSatelliteAssemblyInit();
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////        _callingAssembly = Assembly.GetExecutingAssembly( ref stackMark );
////        // Special case for mscorlib - protect mscorlib's private resources.
////        if(MainAssembly == typeof( Object ).Assembly && _callingAssembly != MainAssembly)
////            _callingAssembly = null;
////    }
////
////    // Trying to unify code as much as possible, even though having to do a
////    // security check in each constructor prevents it.
////    private void CommonSatelliteAssemblyInit()
////    {
////        UseManifest = true;
////        UseSatelliteAssem = true;
////
////        ResourceSets = new Hashtable();
////
////        _fallbackLoc = UltimateResourceFallbackLocation.MainAssembly;
////    }
////
////    // Gets the base name for the ResourceManager.
////    public virtual String BaseName
////    {
////        get { return BaseNameField; }
////    }
////
////    // Whether we should ignore the capitalization of resources when calling
////    // GetString or GetObject.
////    public virtual bool IgnoreCase
////    {
////        get { return _ignoreCase; }
////        set { _ignoreCase = value; }
////    }
////
////    // Returns the Type of the ResourceSet the ResourceManager uses
////    // to construct ResourceSets.
////    public virtual Type ResourceSetType
////    {
////        get { return (_userResourceSet == null) ? typeof( RuntimeResourceSet ) : _userResourceSet; }
////    }
////
////    protected UltimateResourceFallbackLocation FallbackLocation
////    {
////        get { return _fallbackLoc; }
////        set { _fallbackLoc = value; }
////    }
////
////    // Tells the ResourceManager to call Close on all ResourceSets and 
////    // release all resources.  This will shrink your working set by
////    // potentially a substantial amount in a running application.  Any
////    // future resource lookups on this ResourceManager will be as 
////    // expensive as the very first lookup, since it will need to search
////    // for files and load resources again.
////    // 
////    // This may be useful in some complex threading scenarios, where 
////    // creating a new ResourceManager isn't quite the correct behavior.
////    public virtual void ReleaseAllResources()
////    {
////        IDictionaryEnumerator setEnum = ResourceSets.GetEnumerator();
////        // If any calls to Close throw, at least leave ourselves in a
////        // consistent state.
////        ResourceSets = new Hashtable();
////        while(setEnum.MoveNext())
////        {
////            ((ResourceSet)setEnum.Value).Close();
////        }
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    public static ResourceManager CreateFileBasedResourceManager( String baseName, String resourceDir, Type usingResourceSet )
////    {
////        return new ResourceManager( baseName, resourceDir, usingResourceSet );
////    }
////
////    // Given a CultureInfo, it generates the path &; file name for 
////    // the .resources file for that CultureInfo.  This method will grovel
////    // the disk looking for the correct file name & path.  Uses CultureInfo's
////    // Name property.  If the module directory was set in the ResourceManager 
////    // constructor, we'll look there first.  If it couldn't be found in the module
////    // diretory or the module dir wasn't provided, look in the current
////    // directory.
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    private String FindResourceFile( CultureInfo culture )
////    {
////
////        // __FileBasedResourceManager: When we create a __FileBasedResourceManager, move this method
////        // to that class.  This is part of the assembly-ignorant implementation
////        // that really doesn't belong on ResourceManager.
////
////        String fileName = GetResourceFileName( culture );
////
////        // If we have a moduleDir, check there first.  Get module fully 
////        // qualified name, append path to that.
////        if(moduleDir != null)
////        {
////            String path = Path.Combine( moduleDir, fileName );
////            if(File.Exists( path ))
////            {
////                return path;
////            }
////        }
////
////        // look in .
////        if(File.Exists( fileName ))
////            return fileName;
////
////        return null;  // give up.
////    }
////
////    // Given a CultureInfo, GetResourceFileName generates the name for 
////    // the binary file for the given CultureInfo.  This method uses 
////    // CultureInfo's Name property as part of the file name for all cultures
////    // other than the invariant culture.  This method does not touch the disk, 
////    // and is used only to construct what a resource file name (suitable for
////    // passing to the ResourceReader constructor) or a manifest resource blob
////    // name should look like.
////    // 
////    // This method can be overriden to look for a different extension,
////    // such as ".ResX", or a completely different format for naming files.
////    protected virtual String GetResourceFileName( CultureInfo culture )
////    {
////        StringBuilder sb = new StringBuilder( 255 );
////        sb.Append( BaseNameField );
////        // If this is the neutral culture, don't append culture name.
////        if(!culture.Equals( CultureInfo.InvariantCulture ))
////        {
////            CultureInfo.VerifyCultureName( culture, true );
////            sb.Append( '.' );
////            sb.Append( culture.Name );
////        }
////        sb.Append( ResFileExtension );
////        return sb.ToString();
////    }
////
////    // Looks up a set of resources for a particular CultureInfo.  This is
////    // not useful for most users of the ResourceManager - call 
////    // GetString() or GetObject() instead.  
////    //
////    // The parameters let you control whether the ResourceSet is created 
////    // if it hasn't yet been loaded and if parent CultureInfos should be 
////    // loaded as well for resource inheritance.
////    // 
////    public virtual ResourceSet GetResourceSet( CultureInfo culture, bool createIfNotExists, bool tryParents )
////    {
////        if(null == culture)
////            throw new ArgumentNullException( "culture" );
////
////
////        Hashtable localResourceSets = ResourceSets;
////        ResourceSet rs;
////        if(localResourceSets != null)
////        {
////            rs = (ResourceSet)localResourceSets[culture];
////            if(null != rs)
////                return rs;
////        }
////
////        if(UseManifest && culture.Equals( CultureInfo.InvariantCulture ))
////        {
////            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////            string fileName = GetResourceFileName( culture );
////            Stream stream = MainAssembly.GetManifestResourceStream( _locationInfo, fileName, _callingAssembly == MainAssembly, ref stackMark );
////            if(createIfNotExists && stream != null)
////            {
////                rs = CreateResourceSet( stream, MainAssembly );
////                lock(localResourceSets)
////                {
////                    localResourceSets.Add( culture, rs );
////                }
////                return rs;
////            }
////        }
////
////        return InternalGetResourceSet( culture, createIfNotExists, tryParents );
////    }
////
////    // InternalGetResourceSet is a non-threadsafe method where all the logic
////    // for getting a resource set lives.  Access to it is controlled by
////    // threadsafe methods such as GetResourceSet, GetString, & GetObject.  
////    // This will take a minimal number of locks.
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////    protected virtual ResourceSet InternalGetResourceSet( CultureInfo culture, bool createIfNotExists, bool tryParents )
////    {
////        BCLDebug.Assert( culture != null, "culture != null" );
////
////        Hashtable localResourceSets = ResourceSets;
////        ResourceSet rs = (ResourceSet)localResourceSets[culture];
////        if(null != rs)
////            return rs;
////
////        // InternalGetResourceSet will never be threadsafe.  However, it must
////        // be protected against reentrancy from the SAME THREAD.  (ie, calling
////        // GetSatelliteAssembly may send some window messages or trigger the
////        // Assembly load event, which could fail then call back into the 
////        // ResourceManager).  It's happened.
////
////        Stream stream = null;
////        String fileName = null;
////        Assembly satellite = null;  // Which assembly we loaded from
////        if(UseManifest)
////        {
////            fileName = GetResourceFileName( culture );
////            // Ask assembly for resource stream named fileName.
////            StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////            if(UseSatelliteAssem)
////            {
////                CultureInfo lookForCulture = culture;
////                if(_neutralResourcesCulture == null)
////                    _neutralResourcesCulture = GetNeutralResourcesLanguage( MainAssembly, ref _fallbackLoc );
////                // If our neutral resources were written in this culture
////                // AND we know the main assembly does NOT contain neutral
////                // resources, don't probe for this satellite.
////                if(culture.Equals( _neutralResourcesCulture ) &&
////                    FallbackLocation == UltimateResourceFallbackLocation.MainAssembly)
////                {
////                    // Update internal state.
////                    lookForCulture = CultureInfo.InvariantCulture;
////                    fileName = GetResourceFileName( lookForCulture );
////                }
////
////                // For neutral locale, look in the main assembly
////                // if and only if our fallback location is MainAssembly.
////                if(lookForCulture.Equals( CultureInfo.InvariantCulture ))
////                {
////                    if(FallbackLocation == UltimateResourceFallbackLocation.Satellite)
////                    {
////                        satellite = GetSatelliteAssembly( _neutralResourcesCulture );
////                        // If your ultimate fallback satellite couldn't be
////                        // loaded, it's a fatal error.  We'll give
////                        // you a nice error message.
////                        if(satellite == null)
////                        {
////                            String satAssemName = MainAssembly.GetSimpleName() + ".resources.dll";
////                            if(_satelliteContractVersion != null)
////                                satAssemName += ", Version=" + _satelliteContractVersion.ToString();
////
////                            AssemblyName an = new AssemblyName();
////                            an.SetPublicKey( MainAssembly.GetPublicKey() );
////                            byte[] token = an.GetPublicKeyToken();
////
////                            int iLen = token.Length;
////                            StringBuilder publicKeyTok = new StringBuilder( iLen * 2 );
////                            for(int i = 0; i < iLen; i++)
////                                publicKeyTok.Append( token[i].ToString( "x", CultureInfo.InvariantCulture ) );
////                            satAssemName += ", PublicKeyToken=" + publicKeyTok;
////                            String cultureName = _neutralResourcesCulture.Name;
////                            if(cultureName.Length == 0)
////                                cultureName = "<invariant>";
////                            throw new MissingSatelliteAssemblyException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "MissingSatelliteAssembly_Culture_Name" ), _neutralResourcesCulture, satAssemName ), cultureName );
////                        }
////                        // Correct fileName for this satellite.
////                        fileName = GetResourceFileName( _neutralResourcesCulture );
////                    }
////                    else
////                        satellite = MainAssembly;
////                }
////                else
////                    satellite = GetSatelliteAssembly( lookForCulture );
////
////
////                // Look for a possibly-NameSpace-qualified resource file, ie,
////                // Microsoft.Name.myApp.en-US.resources.
////                // (A null _locationInfo is legal)
////                if(satellite != null)
////                {
////                    // Handle case in here where someone added a callback
////                    // for assembly load events.  While no other threads
////                    // have called into GetResourceSet, our own thread can!
////                    // At that point, we could already have an RS in our 
////                    // hash table, and we don't want to add it twice.
////                    rs = (ResourceSet)localResourceSets[lookForCulture];
////                    if(null != rs)
////                    {
////                        return rs;
////                    }
////
////                    // If we're looking in the main assembly AND if the main
////                    // assembly was the person who created the ResourceManager,
////                    // skip a security check for private manifest resources.
////                    bool canSkipSecurityCheck = MainAssembly == satellite && _callingAssembly == MainAssembly;
////                    stream = satellite.GetManifestResourceStream( _locationInfo, fileName, canSkipSecurityCheck, ref stackMark );
////                    if(stream == null)
////                        stream = CaseInsensitiveManifestResourceStreamLookup( satellite, fileName );
////                }
////            }
////            else
////            {  // if !UseSatelliteAssembly
////                satellite = MainAssembly;
////                stream = MainAssembly.GetManifestResourceStream( _locationInfo, fileName, _callingAssembly == MainAssembly, ref stackMark );
////            }
////
////            if(stream == null && tryParents)
////            {
////                // If we've hit top of the Culture tree, return.
////                if(culture.Equals( CultureInfo.InvariantCulture ))
////                {
////                    // Keep people from bothering me about resources problems
////                    if(MainAssembly == typeof( Object ).Assembly && BaseName.Equals( "mscorlib" ))
////                    {
////                        // This would break CultureInfo & all our exceptions.
////                        BCLDebug.Assert( false, "Couldn't get mscorlib" + ResFileExtension + " from mscorlib's assembly" + Environment.NewLine + Environment.NewLine + "Are you building the runtime on your machine?  Chances are the BCL directory didn't build correctly.  Type 'build -c' in the BCL directory.  If you get build errors, look at buildd.log.  If you then can't figure out what's wrong (and you aren't changing the assembly-related metadata code), ask a BCL dev.\n\nIf you did NOT build the runtime, you shouldn't be seeing this and you've found a bug." );
////                        throw new ExecutionEngineException( "mscorlib" + ResFileExtension + " couldn't be found!  Large parts of the BCL won't work!" );
////                    }
////                    // We really don't think this should happen - we always
////                    // expect the neutral locale's resources to be present.
////                    String resName = String.Empty;
////                    if(_locationInfo != null && _locationInfo.Namespace != null)
////                        resName = _locationInfo.Namespace + Type.Delimiter;
////                    resName += fileName;
////                    throw new MissingManifestResourceException( Environment.GetResourceString( "MissingManifestResource_NoNeutralAsm", resName, MainAssembly.GetSimpleName() ) );
////                }
////
////                CultureInfo parent = culture.Parent;
////
////                // Recurse now.
////                rs = InternalGetResourceSet( parent, createIfNotExists, tryParents );
////                if(rs != null)
////                {
////                    AddResourceSet( localResourceSets, culture, ref rs );
////                }
////                return rs;
////            }
////        } //UseManifest
////        else
////        {
////            // Don't use Assembly manifest, but grovel on disk for a file.
////            new System.Security.Permissions.FileIOPermission( System.Security.Permissions.PermissionState.Unrestricted ).Assert();
////
////            // Create new ResourceSet, if a file exists on disk for it.
////            fileName = FindResourceFile( culture );
////            if(fileName == null)
////            {
////                if(tryParents)
////                {
////                    // If we've hit top of the Culture tree, return.
////                    if(culture.Equals( CultureInfo.InvariantCulture ))
////                    {
////                        // We really don't think this should happen - we always
////                        // expect the neutral locale's resources to be present.
////
////                        throw new MissingManifestResourceException( Environment.GetResourceString( "MissingManifestResource_NoNeutralDisk" ) + Environment.NewLine + "baseName: " + BaseNameField + "  locationInfo: " + (_locationInfo == null ? "<null>" : _locationInfo.FullName) + "  fileName: " + GetResourceFileName( culture ) );
////                    }
////
////                    CultureInfo parent = culture.Parent;
////
////                    // Recurse now.
////                    rs = InternalGetResourceSet( parent, createIfNotExists, tryParents );
////                    if(rs != null)
////                    {
////                        AddResourceSet( localResourceSets, culture, ref rs );
////                    }
////                    return rs;
////                }
////            }
////            else
////            {
////                rs = CreateResourceSet( fileName );
////
////                // To speed up ResourceSet lookups in the future, store this
////                // culture with its parent culture's ResourceSet.
////                if(rs != null)
////                {
////                    AddResourceSet( localResourceSets, culture, ref rs );
////                }
////                return rs;
////            }
////        }
////
////        if(createIfNotExists && stream != null && rs == null)
////        {
////            rs = CreateResourceSet( stream, satellite );
////            AddResourceSet( localResourceSets, culture, ref rs );
////        }
////
////        return rs;
////    }
////
////    // Simple helper to ease maintenance and improve readability.
////    private static void AddResourceSet( Hashtable localResourceSets, CultureInfo culture, ref ResourceSet rs )
////    {
////        // InternalGetResourceSet is both recursive and reentrant - 
////        // assembly load callbacks in particular are a way we can call
////        // back into the ResourceManager in unexpectedly on the same thread.
////        lock(localResourceSets)
////        {
////            // If another thread added this culture, return that.
////            ResourceSet lostRace = (ResourceSet)localResourceSets[culture];
////            if(lostRace != null)
////            {
////                if(!Object.ReferenceEquals( lostRace, rs ))
////                {
////                    rs.Dispose();
////                    rs = lostRace;
////                }
////            }
////            else
////            {
////                localResourceSets.Add( culture, rs );
////            }
////        }
////    }
////
////    // Looks up a .resources file in the assembly manifest using 
////    // case-insensitive lookup rules.  Yes, this is slow.  The metadata
////    // dev lead refuses to make all manifest blob lookups case-insensitive,
////    // even optionally case-insensitive.
////    private Stream CaseInsensitiveManifestResourceStreamLookup( Assembly satellite, String name )
////    {
////        StringBuilder sb = new StringBuilder();
////        if(_locationInfo != null)
////        {
////            String nameSpace = _locationInfo.Namespace;
////            if(nameSpace != null)
////            {
////                sb.Append( nameSpace );
////                if(name != null)
////                    sb.Append( Type.Delimiter );
////            }
////        }
////        sb.Append( name );
////
////        String givenName = sb.ToString();
////        CompareInfo comparer = CultureInfo.InvariantCulture.CompareInfo;
////        String canonicalName = null;
////        foreach(String existingName in satellite.GetManifestResourceNames())
////        {
////            if(comparer.Compare( existingName, givenName, CompareOptions.IgnoreCase ) == 0)
////            {
////                if(canonicalName == null)
////                    canonicalName = existingName;
////                else
////                    throw new MissingManifestResourceException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "MissingManifestResource_MultipleBlobs" ), givenName, satellite.ToString() ) );
////            }
////        }
////
////        if(canonicalName == null)
////            return null;
////        // If we're looking in the main assembly AND if the main
////        // assembly was the person who created the ResourceManager,
////        // skip a security check for private manifest resources.
////        bool canSkipSecurityCheck = MainAssembly == satellite && _callingAssembly == MainAssembly;
////        StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
////        Stream s = satellite.GetManifestResourceStream( canonicalName, ref stackMark, canSkipSecurityCheck );
////        // GetManifestResourceStream will return null if we don't have 
////        // permission to read this stream from the assembly.  For example,
////        // if the stream is private and we're trying to access it from another
////        // assembly (ie, ResMgr in mscorlib accessing anything else), we 
////        // require Reflection TypeInformation permission to be able to read 
////        // this.  <STRIP>This meaning of private in satellite assemblies is a really
////        // odd concept, and is orthogonal to the ResourceManager.  
////        // We should not assume we can skip this security check,
////        // which means satellites must always use public manifest resources
////        // if you want to support semi-trusted code.  </STRIP>
////        BCLDebug.Correctness( s != null, "Could not access the manifest resource from your satellite.  Make " + canonicalName + " in assembly " + satellite.GetSimpleName() + " public." );
////        return s;
////    }
////
////    protected static Version GetSatelliteContractVersion( Assembly a )
////    {
////        BCLDebug.Assert( a != null, "assembly != null" );
////        Object[] attrs = a.GetCustomAttributes( typeof( SatelliteContractVersionAttribute ), false );
////        if(attrs.Length == 0)
////            return null;
////        BCLDebug.Assert( attrs.Length == 1, "Cannot have multiple instances of SatelliteContractVersionAttribute on an assembly!" );
////        String v = ((SatelliteContractVersionAttribute)attrs[0]).Version;
////        Version ver;
////        try
////        {
////            ver = new Version( v );
////        }
////        catch(Exception e)
////        {
////            // Note we are prone to hitting infinite loops if mscorlib's
////            // SatelliteContractVersionAttribute contains bogus values.
////            // If this assert fires, please fix the build process for the
////            // BCL directory.
////            if(a == typeof( Object ).Assembly)
////            {
////                BCLDebug.Assert( false, "mscorlib's SatelliteContractVersionAttribute is a malformed version string!" );
////                return null;
////            }
////
////            throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_InvalidSatelliteContract_Asm_Ver" ), a.ToString(), v ), e );
////        }
////        return ver;
////    }
////
////    protected static CultureInfo GetNeutralResourcesLanguage( Assembly a )
////    {
////        // This method should be obsolete - replace it with the one below.
////        // Unfortunately, we made it protected.
////        UltimateResourceFallbackLocation ignoringUsefulData = UltimateResourceFallbackLocation.MainAssembly;
////        CultureInfo culture = GetNeutralResourcesLanguage( a, ref ignoringUsefulData );
////        return culture;
////    }
////
////    private static CultureInfo GetNeutralResourcesLanguage( Assembly a, ref UltimateResourceFallbackLocation fallbackLocation )
////    {
////        BCLDebug.Assert( a != null, "assembly != null" );
////
////        IList<CustomAttributeData> attrs = CustomAttributeData.GetCustomAttributes( a );
////        CustomAttributeData attr = null;
////        for(int i = 0; i < attrs.Count; i++)
////        {
////            if(attrs[i].Constructor.DeclaringType == typeof( NeutralResourcesLanguageAttribute ))
////            {
////                attr = attrs[i];
////                break;
////            }
////        }
////
////        if(attr == null)
////        {
////            BCLDebug.Perf( false, "Consider adding NeutralResourcesLanguageAttribute to assembly " + a.FullName );
////            fallbackLocation = UltimateResourceFallbackLocation.MainAssembly;
////            return CultureInfo.InvariantCulture;
////        }
////
////        string cultureName = null;
////        if(attr.Constructor.GetParameters().Length == 2)
////        {
////            fallbackLocation = (UltimateResourceFallbackLocation)attr.ConstructorArguments[1].Value;
////            if(fallbackLocation < UltimateResourceFallbackLocation.MainAssembly || fallbackLocation > UltimateResourceFallbackLocation.Satellite)
////                throw new ArgumentException( Environment.GetResourceString( "Arg_InvalidNeutralResourcesLanguage_FallbackLoc", fallbackLocation ) );
////        }
////        else
////            fallbackLocation = UltimateResourceFallbackLocation.MainAssembly;
////
////        cultureName = attr.ConstructorArguments[0].Value as string;
////
////        try
////        {
////            CultureInfo c = CultureInfo.GetCultureInfo( cultureName );
////            return c;
////        }
////        catch(ArgumentException e)
////        { // we should catch ArgumentException only.
////            // Note we could go into infinite loops if mscorlib's 
////            // NeutralResourcesLanguageAttribute is mangled.  If this assert
////            // fires, please fix the build process for the BCL directory.
////            if(a == typeof( Object ).Assembly)
////            {
////                BCLDebug.Assert( false, "mscorlib's NeutralResourcesLanguageAttribute is a malformed culture name! name: \"" + cultureName + "\"  Exception: " + e );
////                return CultureInfo.InvariantCulture;
////            }
////
////            throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_InvalidNeutralResourcesLanguage_Asm_Culture" ), a.ToString(), cultureName ), e );
////        }
////    }
////
////    private Assembly GetSatelliteAssembly( CultureInfo lookForCulture )
////    {
////        if(!_lookedForSatelliteContractVersion)
////        {
////            _satelliteContractVersion = GetSatelliteContractVersion( MainAssembly );
////            _lookedForSatelliteContractVersion = true;
////        }
////
////        Assembly satellite = null;
////
////        // Look up the satellite assembly, but don't let problems
////        // like a partially signed satellite assembly stop us from
////        // doing fallback and displaying something to the user.
////        // Yet also somehow log this error for a developer.
////        try
////        {
////            satellite = MainAssembly.InternalGetSatelliteAssembly( lookForCulture, _satelliteContractVersion, false );
////        }
////
////        // Don't throw for FUSION_E_REFDEFMISMATCH, for compat with MOM, which may cause assembly w/ incorrect culture to be found
////        // @todo: seems that we would want to rethrow for all other cases...
////        catch(FileLoadException fle)
////        {
////            // Ignore cases where the loader gets an access
////            // denied back from the OS.  This showed up for
////            // href-run exe's at one point.  
////            int hr = Marshal.GetHRForException( fle );
////            if(hr != Win32Native.MakeHRFromErrorCode( Win32Native.ERROR_ACCESS_DENIED ))
////            {
////                BCLDebug.Assert( false, "[This is an ignorable assert to catch satellite assembly build problems - talk to your build lab & loc engineer]" + Environment.NewLine + "GetSatelliteAssembly failed for culture " + lookForCulture.Name + " and version " + (_satelliteContractVersion == null ? MainAssembly.GetVersion().ToString() : _satelliteContractVersion.ToString()) + " of assembly " + MainAssembly.GetSimpleName() + " with error code 0x" + hr.ToString( "X", CultureInfo.InvariantCulture ) + "\r\nException: " + fle );
////            }
////        }
////
////        // Don't throw for zero-length satellite assemblies, for compat with v1
////        catch(BadImageFormatException bife)
////        {
////            BCLDebug.Assert( false, "[This is an ignorable assert to catch satellite assembly build problems - talk to your build lab & loc engineer]" + Environment.NewLine + "GetSatelliteAssembly failed for culture " + lookForCulture.Name + " and version " + (_satelliteContractVersion == null ? MainAssembly.GetVersion().ToString() : _satelliteContractVersion.ToString()) + " of assembly " + MainAssembly.GetSimpleName() + "\r\nException: " + bife );
////        }
////
////        return satellite;
////    }
////
////    // Constructs a new ResourceSet for a given file name.  The logic in
////    // here avoids a ReflectionPermission check for our RuntimeResourceSet
////    // for perf and working set reasons.
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    private ResourceSet CreateResourceSet( String file )
////    {
////        if(_userResourceSet == null)
////        {
////            // Explicitly avoid CreateInstance if possible, because it
////            // requires ReflectionPermission to call private & protected
////            // constructors.  
////            return new RuntimeResourceSet( file );
////        }
////        else
////        {
////            Object[] args = new Object[1];
////            args[0] = file;
////            try
////            {
////                return (ResourceSet)Activator.CreateInstance( _userResourceSet, args );
////            }
////            catch(MissingMethodException e)
////            {
////                throw new InvalidOperationException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "InvalidOperation_ResMgrBadResSet_Type" ), _userResourceSet.AssemblyQualifiedName ), e );
////            }
////        }
////    }
////
////    // Constructs a new ResourceSet for a given file name.  The logic in
////    // here avoids a ReflectionPermission check for our RuntimeResourceSet
////    // for perf and working set reasons.
////    // Use the assembly to resolve assembly manifest resource references.
////    // Note that is can be null, but probably shouldn't be.
////    private ResourceSet CreateResourceSet( Stream store, Assembly assembly )
////    {
////        BCLDebug.Assert( store != null, "I need a Stream!" );
////        // Check to see if this is a Stream the ResourceManager understands,
////        // and check for the correct resource reader type.
////        if(store.CanSeek && store.Length > 4)
////        {
////            long startPos = store.Position;
////            BinaryReader br = new BinaryReader( store );
////            // Look for our magic number as a little endian Int32.
////            int bytes = br.ReadInt32();
////            if(bytes == MagicNumber)
////            {
////                int resMgrHeaderVersion = br.ReadInt32();
////                String readerTypeName = null, resSetTypeName = null;
////                if(resMgrHeaderVersion == HeaderVersionNumber)
////                {
////                    br.ReadInt32();  // We don't want the number of bytes to skip.
////                    readerTypeName = br.ReadString();
////                    resSetTypeName = br.ReadString();
////                }
////                else if(resMgrHeaderVersion > HeaderVersionNumber)
////                {
////                    // Assume that the future ResourceManager headers will
////                    // have two strings for us - the reader type name and
////                    // resource set type name.  Read those, then use the num
////                    // bytes to skip field to correct our position.
////                    int numBytesToSkip = br.ReadInt32();
////                    long endPosition = br.BaseStream.Position + numBytesToSkip;
////
////                    readerTypeName = br.ReadString();
////                    resSetTypeName = br.ReadString();
////
////                    br.BaseStream.Seek( endPosition, SeekOrigin.Begin );
////                }
////                else
////                {
////                    // resMgrHeaderVersion is older than this ResMgr version.
////                    // We should add in backwards compatibility support here.
////
////                    throw new NotSupportedException( Environment.GetResourceString( "NotSupported_ObsoleteResourcesFile", MainAssembly.GetSimpleName() ) );
////                }
////
////                store.Position = startPos;
////                // Perf optimization - Don't use Reflection for our defaults.
////                // Note there are two different sets of strings here - the
////                // assembly qualified strings emitted by ResourceWriter, and
////                // the abbreviated ones emitted by InternalResGen.
////                if(CanUseDefaultResourceClasses( readerTypeName, resSetTypeName ))
////                {
////                    RuntimeResourceSet rs;
////                    rs = new RuntimeResourceSet( store );
////                    return rs;
////                }
////                else
////                {
////                    // we do not want to use partial binding here.
////                    Type readerType = Type.GetType( readerTypeName, true );
////                    Object[] args = new Object[1];
////                    args[0] = store;
////                    IResourceReader reader = (IResourceReader)Activator.CreateInstance( readerType, args );
////
////                    Object[] resourceSetArgs = new Object[1];
////                    resourceSetArgs[0] = reader;
////                    Type resSetType;
////                    if(_userResourceSet == null)
////                    {
////                        BCLDebug.Assert( resSetTypeName != null, "We should have a ResourceSet type name from the custom resource file here." );
////                        resSetType = Type.GetType( resSetTypeName, true, false );
////                    }
////                    else
////                        resSetType = _userResourceSet;
////                    ResourceSet rs = (ResourceSet)Activator.CreateInstance( resSetType,
////                                                                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance,
////                                                                            null,
////                                                                            resourceSetArgs,
////                                                                            null,
////                                                                            null );
////                    return rs;
////                }
////            }
////            else
////                store.Position = startPos;
////        }
////
////        if(_userResourceSet == null)
////        {
////            // Explicitly avoid CreateInstance if possible, because it
////            // requires ReflectionPermission to call private & protected
////            // constructors.  
////            return new RuntimeResourceSet( store );
////        }
////        else
////        {
////            Object[] args = new Object[2];
////            args[0] = store;
////            args[1] = assembly;
////            try
////            {
////                ResourceSet rs = null;
////                // Add in a check for a constructor taking in an assembly first.
////                try
////                {
////                    rs = (ResourceSet)Activator.CreateInstance( _userResourceSet, args );
////                    return rs;
////                }
////                catch(MissingMethodException) { }
////
////                args = new Object[1];
////                args[0] = store;
////                rs = (ResourceSet)Activator.CreateInstance( _userResourceSet, args );
////                return rs;
////            }
////            catch(MissingMethodException e)
////            {
////                throw new InvalidOperationException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "InvalidOperation_ResMgrBadResSet_Type" ), _userResourceSet.AssemblyQualifiedName ), e );
////            }
////        }
////    }
////
////    // Perf optimization - Don't use Reflection for most cases with
////    // our .resources files.  This makes our code run faster and we can
////    // creating a ResourceReader via Reflection.  This would incur
////    // a security check (since the link-time check on the constructor that
////    // takes a String is turned into a full demand with a stack walk)
////    // and causes partially trusted localized apps to fail.
////    private bool CanUseDefaultResourceClasses( String readerTypeName, String resSetTypeName )
////    {
////        if(_userResourceSet != null)
////            return false;
////
////        // Ignore the actual version of the ResourceReader and 
////        // RuntimeResourceSet classes.  Let those classes deal with
////        // versioning themselves.
////        AssemblyName mscorlib = new AssemblyName( MscorlibName );
////
////        if(readerTypeName != null)
////        {
////            if(!CompareNames( readerTypeName, ResReaderTypeName, mscorlib ))
////                return false;
////        }
////
////        if(resSetTypeName != null)
////        {
////            if(!CompareNames( resSetTypeName, ResSetTypeName, mscorlib ))
////                return false;
////        }
////
////        return true;
////    }
////
////
////    // IGNORES VERSION
////    internal static bool CompareNames( String asmTypeName1,
////                                      String typeName2,
////                                      AssemblyName asmName2 )
////    {
////        BCLDebug.Assert( asmTypeName1 != null, "asmTypeName1 was unexpectedly null" );
////
////        // First, compare type names
////        int comma = asmTypeName1.IndexOf( ',' );
////        if(((comma == -1) ? asmTypeName1.Length : comma) != typeName2.Length)
////            return false;
////
////        // case sensitive
////        if(String.Compare( asmTypeName1, 0, typeName2, 0, typeName2.Length, StringComparison.Ordinal ) != 0)
////            return false;
////        if(comma == -1)
////            return true;
////
////        // Now, compare assembly display names (IGNORES VERSION AND PROCESSORARCHITECTURE)
////        while(Char.IsWhiteSpace( asmTypeName1[++comma] )) ;
////
////        // case insensitive
////        AssemblyName an1 = new AssemblyName( asmTypeName1.Substring( comma ) );
////        if(String.Compare( an1.Name, asmName2.Name, StringComparison.OrdinalIgnoreCase ) != 0)
////            return false;
////
////        if((an1.CultureInfo != null) && (asmName2.CultureInfo != null) &&
////            (an1.CultureInfo.LCID != asmName2.CultureInfo.LCID))
////            return false;
////
////        byte[] pkt1 = an1.GetPublicKeyToken();
////        byte[] pkt2 = asmName2.GetPublicKeyToken();
////        if((pkt1 != null) && (pkt2 != null))
////        {
////            if(pkt1.Length != pkt2.Length)
////                return false;
////
////            for(int i = 0; i < pkt1.Length; i++)
////            {
////                if(pkt1[i] != pkt2[i])
////                    return false;
////            }
////        }
////
////        return true;
////    }

        // Looks up a resource value for a particular name.  Looks in the 
        // current thread's CultureInfo, and if not found, all parent CultureInfos.
        // Returns null if the resource wasn't found.
        // 
        public virtual String GetString( String name )
        {
            return GetString( name, (CultureInfo)null );
        }

        // Looks up a resource value for a particular name.  Looks in the 
        // specified CultureInfo, and if not found, all parent CultureInfos.
        // Returns null if the resource wasn't found.
        // 
        [MethodImpl( MethodImplOptions.InternalCall )]
        public virtual extern String GetString( String name, CultureInfo culture );
////    {
////        if(null == name)
////            throw new ArgumentNullException( "name" );
////        if(null == culture)
////        {
////            culture = CultureInfo.CurrentUICulture;
////        }
////
////        ResourceSet rs = InternalGetResourceSet( culture, true, true );
////
////        if(rs != null)
////        {
////            String value = rs.GetString( name, _ignoreCase );
////            if(value != null)
////                return value;
////        }
////
////        // This is the CultureInfo hierarchy traversal code for resource 
////        // lookups, similar but necessarily orthogonal to the ResourceSet 
////        // lookup logic.
////        ResourceSet last = null;
////        while(!culture.Equals( CultureInfo.InvariantCulture ) && !culture.Equals( _neutralResourcesCulture ))
////        {
////            culture = culture.Parent;
////
////            rs = InternalGetResourceSet( culture, true, true );
////            if(rs == null)
////                break;
////            if(rs != last)
////            {
////                String value = rs.GetString( name, _ignoreCase );
////                if(value != null)
////                    return value;
////                last = rs;
////            }
////        }
////        return null;
////    }


        // Looks up a resource value for a particular name.  Looks in the 
        // current thread's CultureInfo, and if not found, all parent CultureInfos.
        // Returns null if the resource wasn't found.
        // 
        public virtual Object GetObject( String name )
        {
            return GetObject( name, (CultureInfo)null, true );
        }

        // Looks up a resource value for a particular name.  Looks in the 
        // specified CultureInfo, and if not found, all parent CultureInfos.
        // Returns null if the resource wasn't found.
        // 
        public virtual Object GetObject( String name, CultureInfo culture )
        {
            return GetObject( name, culture, true );
        }

        [MethodImpl( MethodImplOptions.InternalCall )]
        private extern Object GetObject( String name, CultureInfo culture, bool wrapUnmanagedMemStream );
////    {
////        if(null == name)
////            throw new ArgumentNullException( "name" );
////        if(null == culture)
////        {
////            culture = CultureInfo.CurrentUICulture;
////        }
////
////        ResourceSet rs = InternalGetResourceSet( culture, true, true );
////
////        if(rs != null)
////        {
////            Object value = rs.GetObject( name, _ignoreCase );
////            if(value != null)
////            {
////                UnmanagedMemoryStream stream = value as UnmanagedMemoryStream;
////                if(stream != null && wrapUnmanagedMemStream)
////                    return new UnmanagedMemoryStreamWrapper( stream );
////                else
////                    return value;
////            }
////        }
////
////        // This is the CultureInfo hierarchy traversal code for resource 
////        // lookups, similar but necessarily orthogonal to the ResourceSet 
////        // lookup logic.
////        ResourceSet last = null;
////        while(!culture.Equals( CultureInfo.InvariantCulture ) && !culture.Equals( _neutralResourcesCulture ))
////        {
////            culture = culture.Parent;
////            rs = InternalGetResourceSet( culture, true, true );
////            if(rs == null)
////                break;
////            if(rs != last)
////            {
////                Object value = rs.GetObject( name, _ignoreCase );
////                if(value != null)
////                {
////                    UnmanagedMemoryStream stream = value as UnmanagedMemoryStream;
////                    if(stream != null && wrapUnmanagedMemStream)
////                        return new UnmanagedMemoryStreamWrapper( stream );
////                    else
////                        return value;
////                }
////                last = rs;
////            }
////        }
////        return null;
////    }
////
////    [CLSCompliant( false )]
////    public UnmanagedMemoryStream GetStream( String name )
////    {
////        return GetStream( name, (CultureInfo)null );
////    }
////
////    [CLSCompliant( false )]
////    public UnmanagedMemoryStream GetStream( String name, CultureInfo culture )
////    {
////        Object obj = GetObject( name, culture, false );
////        UnmanagedMemoryStream ums = obj as UnmanagedMemoryStream;
////        if(ums == null && obj != null)
////            throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_ResourceNotStream_Name", name ) );
////        return ums;
////    }
    }
}
