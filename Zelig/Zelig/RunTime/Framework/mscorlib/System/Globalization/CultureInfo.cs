// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
////////////////////////////////////////////////////////////////////////////
//
//  Class:    CultureInfo
//
//
//  Purpose:  This class represents the software preferences of a particular
//            culture or community.  It includes information such as the
//            language, writing system, and a calendar used by the culture
//            as well as methods for common operations such as printing
//            dates and sorting strings.
//
//  Date:     March 31, 1999
//
//
//  !!!! NOTE WHEN CHANGING THIS CLASS !!!!
//
//  If adding or removing members to this class, please update CultureInfoBaseObject
//  in ndp/clr/src/vm/object.h. Note, the "actual" layout of the class may be
//  different than the order in which members are declared. For instance, all
//  reference types will come first in the class before value types (like ints, bools, etc)
//  regardless of the order in which they are declared. The best way to see the
//  actual order of the class is to do a !dumpobj on an instance of the managed
//  object inside of the debugger.
//
////////////////////////////////////////////////////////////////////////////

namespace System.Globalization
{
    using System;
    using System.Threading;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
////using System.Security.Permissions;
    using System.Reflection;
////using Microsoft.Win32;
////using System.Runtime.Versioning;

    [Serializable]
    public class CultureInfo : /*ICloneable,*/ IFormatProvider
    {
////    //
////    // Special culture IDs
////    //
////    internal const int zh_CHT_CultureID     = 0x7c04;
////    // Serbian Neutral culture ID.
////    internal const int sr_CultureID         = 0x7c1a;
////    internal const int sr_SP_Latn_CultureID = 0x081a;
////
////    // Ietf name registry location
////    internal const string ietfRegistryKey = "SYSTEM\\CurrentControlSet\\Control\\Nls\\IetfLanguage";
////
////    //--------------------------------------------------------------------//
////    //                        Internal Information                        //
////    //--------------------------------------------------------------------//
////
////    //--------------------------------------------------------------------//
////    //
////    // Static data members
////    //
////    //--------------------------------------------------------------------//
////
////    //Get the current user default culture.  This one is almost always used, so we create it by default.
////    private static CultureInfo  m_userDefaultCulture = null;
    
        //
        // All of the following will be created on demand.
        //
    
        //The Invariant culture;
        private static CultureInfo  m_InvariantCultureInfo = null;
    
////    //The culture used in the user interface. This is mostly used to load correct localized resources.
////    private static CultureInfo  m_userDefaultUICulture = null;
////
////    //This is the UI culture used to install the OS.
////    private static CultureInfo  m_InstalledUICultureInfo = null;
////
////    //This is a cache of all previously created cultures.  Valid keys are LCIDs or the name.  We use two hashtables to track them,
////    // depending on how they are called.
////    private static Hashtable    m_LcidCachedCultures = null;
////    private static Hashtable    m_NameCachedCultures = null;
////    private static Hashtable    m_IetfCachedCultures = null;
////
////    // May need a registry key if looking up ietf names by name
////    private static RegistryKey  m_ietfNameLookupKey = null;
////
////
////    //--------------------------------------------------------------------//
////    // Data members to be serialized:
////    //--------------------------------------------------------------------//
////
////    // This is the string used to construct CultureInfo.
////    // It is in the format of ISO639 (2 letter language name) plus dash plus
////    // ISO 3166 (2 letter region name).  The language name is in lowercase and region name
////    // are in uppercase. (now part of cultureTableRecord)
////
////    //
////    // This is the culture ID used in the NLS+ world.  The concept of cultureID is similar
////    // to the concept of LCID in Win32.  However, NLS+ support "neutral" culture
////    // which Win32 doesn't support.
////    //
////    // The format of culture ID (32 bits) is:
////    //
////    // 31 - 20 19 18 17 16 15 14 13 12 11 10 9 8 7 6 5 4 3 2 1 0
////    // +-----+ +---------+ +---------------+ +-----------------+
////    //    |         |           |            Primary language ID (10 bits)
////    //    |         |           +----------- Sublanguage ID (6 its)
////    //    |         +----------------------- Sort ID (4 bits)
////    //    +--------------------------------- Reserved (12 bits)
////    //
////    // Primary language ID and sublanguage ID can be zero to specify 'neutral' language.
////    // For example, cultureID 0x(0000)0009 is the English neutral culture.
////    // cultureID 0x(0000)0000 means the invariant culture (or called neutral culture).
////    //
////    // We'd just use cultureTableInfo.cultureID for ID, however cultureTableInfo doesn't
////    // remember the sort info.
////
////    // WARNING
////    // WARNING: All member fields declared here must also be in ndp/clr/src/vm/object.h
////    // WARNING: They aren't really private because object.h can access them, but other C# stuff cannot
////    // WARNING
////    internal int                cultureID;
        internal bool               m_isReadOnly = false;
////
////    internal CompareInfo        compareInfo  = null;
        internal TextInfo           textInfo     = null;
        internal NumberFormatInfo   numInfo      = null;
        internal DateTimeFormatInfo dateTimeInfo = null;
////    internal Calendar           calendar     = null;
    
        //
        // The CultureTable instance that we are going to read data from.
        // For supported culture, this will be the CultureTable instance that read data from mscorlib assembly.
        // For customized culture, this will be the CultureTable instance that read data from user customized culture binary file.
        //
        [NonSerialized]
        internal CultureTableRecord m_cultureTableRecord;
        [NonSerialized]
        internal bool               m_isInherited;
////    [NonSerialized]
////    private bool                m_isSafeCrossDomain = false;
////    [NonSerialized]
////    private int                 m_createdDomainID = 0;
////    [NonSerialized]
////    private CultureInfo         m_consoleFallbackCulture = null;
////
////    // Names are confusing.  Here are 3 names we have:
////    //
////    //  new CultureInfo()   m_name        m_nonSortName   m_sortName
////    //      en-US           en-US           en-US           en-US
////    //      de-de_phoneb    de-DE_phoneb    de-DE           de-DE_phoneb
////    //      fj-fj (custom)  fj-FJ           fj-FJ           en-US (if specified sort is en-US)
////
////    // Note that the name used to be serialized for Everett; it is now serialized
////    // because alernate sorts can have alternate names.
////    // This has a de-DE, de-DE_phoneb or fj-FJ style name
////    internal string             m_name = null;
////
////    // This will hold the non sorting name to be returned from CultureInfo.Name property.
////    // This has a de-DE style name even for de-DE_phoneb type cultures
////    [NonSerialized]
////    private string              m_nonSortName = null;
////
////    // This will hold the sorting name to be returned from CultureInfo.SortName property.
////    // This might be completely unrelated to the culture name if a custom culture.  Ie en-US for fj-FJ.
////    // Otherwise its the sort name, ie: de-DE or de-DE_phoneb
////    [NonSerialized]
////    private string              m_sortName = null;
////
////    // Ietf name
////    [NonSerialized]
////    private string              m_ietfName = null;
////
////    //The parent culture.
////    [NonSerialized]
////    private CultureInfo         m_parent;
////
////
////    //
////    //  Helper Methods.
////    //
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern bool IsValidLCID( int LCID, int flag );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern unsafe String nativeGetUserDefaultLCID( int* LCID, int lcidType );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern unsafe String nativeGetUserDefaultUILanguage( int* LCID );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern unsafe String nativeGetSystemDefaultUILanguage( int* LCID );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern bool nativeSetThreadLocale( int LCID );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern String nativeGetLocaleInfo( int LCID, int field );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern int nativeGetCurrentCalendar();
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern unsafe bool nativeGetDTFIUserValues( int lcid, ref DTFIUserOverrideValues values );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern unsafe bool nativeGetNFIUserValues( int lcid, NumberFormatInfo nfi );
////
////    // <SyntheticSupport/>
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern unsafe bool nativeGetCultureData( int lcid, ref CultureData cultureData );
////
////    // <SyntheticSupport/>
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern bool nativeEnumSystemLocales( out int[] localesArray );
////
////    // <SyntheticSupport/>
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern string nativeGetCultureName( int lcid, bool useSNameLCType, bool getMonthName );
////
////    //
////    // Return the path to the Windows directory (such as c:\Windows).  The returned string does NOT include the backslash.
////    //
////    [ResourceExposure( ResourceScope.Machine )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern string nativeGetWindowsDirectory();
////
////    //
////    // Check if the specified fileName exists in the file system or not.
////    //
////    [ResourceExposure( ResourceScope.Machine )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern bool nativeFileExists( String fileName );
////
////    //
////    // Get the static int array data used by globalization classes.
////    //
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern unsafe int* nativeGetStaticInt32DataTable( int type, out int tableSize );
////
////    //
////    // we didn't make LCMapStringW fast call because in the debug mscorwks we should call LCMapStringW through a wrapper
////    // which can switch to ansi version randomally (to simulate the running in the ansi platforms).
////    //
////    // we just need to call the Unicode version all the time as we check running on the Unicode platforms before we proceed
////    // while getting the culture data (in nativeGetCultureData)
////    //
////    // <SyntheticSupport/>
////    internal static unsafe int GetNativeSortKey( int lcid, int flags, string source, int cchSrc, out byte[] sortKey )
////    {
////        sortKey = null;
////
////        int length;
////
////        fixed(char* src = source)
////        {
////            length = Win32Native.LCMapStringW( lcid, flags | Win32Native.LCMAP_SORTKEY, src, cchSrc, null, 0 );
////            if(length == 0)
////            {
////                return 0;
////            }
////
////            sortKey = new byte[length];
////
////            fixed(byte* key = sortKey)
////            {
////                length = Win32Native.LCMapStringW( lcid, flags | Win32Native.LCMAP_SORTKEY, src, cchSrc, (char*)key, length );
////            }
////        }
////
////        return length;
////    }
////
////    private  const int LOCALE_NEUTRAL             = 0x0000;
        internal const int LOCALE_INVARIANT           = 0x007f;
////    internal const int LOCALE_USER_DEFAULT        = 0x0400;
////    internal const int LOCALE_SYSTEM_DEFAULT      = 0x0800;
////    internal const int LOCALE_CUSTOM_DEFAULT      = 0x0c00;
////    internal const int LOCALE_CUSTOM_UNSPECIFIED  = 0x1000;
////    internal const int LOCALE_TRADITIONAL_SPANISH = 0x040a;
////    internal const int LCID_INSTALLED             = 0x0001;
////    internal const int LCID_SUPPORTED             = 0x0002;
    
        //
        // The CultureTable instance that read data from culture.nlp in the mscorlib assembly.
        //
        static unsafe CultureInfo()
        {
            if(m_InvariantCultureInfo == null)
            {
                CultureInfo temp = new CultureInfo( LOCALE_INVARIANT, false );
    
////            temp.m_isReadOnly = true;
    
                m_InvariantCultureInfo = temp;
            }
    
////        // First we set it to Invariant in case someone needs it before we're done finding it.
////        // For example, if we throw an exception in InitUserDefaultCulture, we will still need an valid
////        // m_userDefaultCulture to be used in Thread.CurrentCulture.
////        m_userDefaultCulture   = m_userDefaultUICulture = m_InvariantCultureInfo;
////
////        m_userDefaultCulture   = InitUserDefaultCulture();
////        m_userDefaultUICulture = InitUserDefaultUICulture();
    
        }
    
////    static unsafe CultureInfo InitUserDefaultCulture()
////    {
////        int         LCID;
////        String      strDefault = nativeGetUserDefaultLCID( &LCID, LOCALE_USER_DEFAULT );
////        CultureInfo temp       = GetCultureByLCIDOrName  (  LCID, strDefault          );
////        if(temp == null)
////        {
////            strDefault = nativeGetUserDefaultLCID( &LCID, LOCALE_SYSTEM_DEFAULT );
////            temp       = GetCultureByLCIDOrName  (  LCID, strDefault            );
////
////            if(temp == null)
////            {
////                // If system default doesn't work, keep using the invariant
////                return (CultureInfo.InvariantCulture);
////            }
////        }
////
////        temp.m_isReadOnly = true;
////
////        return (temp);
////    }
////
////    static unsafe CultureInfo InitUserDefaultUICulture()
////    {
////        int    LCID;
////        String strDefault = nativeGetUserDefaultUILanguage( &LCID );
////
////        // In most of cases, UserDefaultCulture == UserDefaultUICulture, so we should use the same instance if possible.
////        if(LCID == UserDefaultCulture.LCID || strDefault == UserDefaultCulture.Name)
////        {
////            return (UserDefaultCulture);
////        }
////
////        CultureInfo temp = GetCultureByLCIDOrName( LCID, strDefault );
////
////        if(temp == null)
////        {
////            strDefault = nativeGetSystemDefaultUILanguage( &LCID             );
////            temp       = GetCultureByLCIDOrName          (  LCID, strDefault );
////        }
////
////        if(temp == null)
////        {
////            return (CultureInfo.InvariantCulture);
////        }
////
////        temp.m_isReadOnly = true;
////
////        return (temp);
////    }
    
    
    
        ////////////////////////////////////////////////////////////////////////
        //
        //  CultureInfo Constructors
        //
        ////////////////////////////////////////////////////////////////////////
    
    
        public CultureInfo( String name ) : this( name, true )
        {
        }
    
    
        public CultureInfo( String name, bool useUserOverride )
        {
////        if(name == null)
////        {
////            throw new ArgumentNullException( "name", Environment.GetResourceString( "ArgumentNull_String" ) );
////        }
////
////        this.m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord( name, useUserOverride );
////        this.cultureID            = this.m_cultureTableRecord.ActualCultureID;
////        this.m_name               = this.m_cultureTableRecord.ActualName;
////        this.m_isInherited        = (this.GetType() != typeof( System.Globalization.CultureInfo ));
        }
    
        public CultureInfo( int culture ) : this( culture, true )
        {
        }
    
        public unsafe CultureInfo( int culture, bool useUserOverride )
        {
////        // We don't check for other invalid LCIDS here...
////        if(culture < 0)
////        {
////            throw new ArgumentOutOfRangeException( "culture", Environment.GetResourceString( "ArgumentOutOfRange_NeedPosNum" ) );
////        }
////
////        switch(culture)
////        {
////            case LOCALE_CUSTOM_DEFAULT:
////            // CONSDIER: Support this LCID value any time the OS is using a custom culture as the user default.
////            //           Note that if this is to be supported that the code below is not correct since it assumes
////            //           the LCID value is describing the NLS+ culture, not the NLS locale.
////            /*
////                                this.m_cultureTableRecord = CultureInfo.CurrentCulture.m_cultureTableRecord;
////                                this.cultureID = CultureInfo.CurrentCulture.cultureID;
////                                break;
////            */
////            case LOCALE_SYSTEM_DEFAULT:
////            /*
////                                String strSystemDefault = nativeGetUserDefaultLCID(&culture, LOCALE_SYSTEM_DEFAULT);
////                                CultureInfo systemDefault;
////
////                                // See if we can get our LOCALE_SYSTEM_DEFAULT
////                                systemDefault = GetCultureByLCIDOrName(culture, strSystemDefault);
////
////                                if (systemDefault == null)
////                                    // That didn't work, try invariant
////                                    systemDefault = InvariantCulture;
////
////                                this.cultureID = systemDefault.cultureID;
////                                this.m_cultureTableRecord = systemDefault.m_cultureTableRecord;
////                                break;
////            */
////            case LOCALE_NEUTRAL:
////            case LOCALE_USER_DEFAULT:
////            case LOCALE_CUSTOM_UNSPECIFIED:
////                // Can't support unknown custom cultures and we do not support neutral or
////                // non-custom user locales.
////                throw new ArgumentException( Environment.GetResourceString( "Argument_CultureNotSupported", culture ), "culture" );
////
////            default:
////                this.cultureID = culture;
////                // Now see if this LCID is supported in the system default culture.nlp table.
////                this.m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord( this.cultureID, useUserOverride );
////                this.m_name               = this.m_cultureTableRecord.ActualName;
////                break;
////        }
////
////        m_isInherited = (this.GetType() != typeof( System.Globalization.CultureInfo ));
        }
////
////
////    //
////    // CheckDomainSafetyObject throw if the object is customized object which cannot be attached to
////    // other object (like CultureInfo or DateTimeFormatInfo).
////    //
////
////    internal static void CheckDomainSafetyObject( Object obj, Object container )
////    {
////        if(obj.GetType().Assembly != typeof( System.Globalization.CultureInfo ).Assembly)
////        {
////            throw new InvalidOperationException( String.Format( CultureInfo.CurrentCulture,
////                            Environment.GetResourceString( "InvalidOperation_SubclassedObject" ),
////                            obj.GetType(),
////                            container.GetType() ) );
////        }
////    }

        #region Serialization
////    // the following fields are defined to keep the compatibility with .NET V1.0/V1.1.
////    // don't change/remove the names/types of these fields.
////    private int  m_dataItem;
////    private bool m_useUserOverride;
////
////    [OnDeserialized]
////    private void OnDeserialized( StreamingContext ctx )
////    {
////        // In .NET ver 1.0 we can get m_name = null if the Name property never requested
////        // before serializing the culture info object.
////
////        BCLDebug.Assert( m_name != null || cultureID > 0, "[CultureInfo.OnDeserialized] m_name != null || cultureID>0" );
////
////        if(m_name != null && cultureID != LOCALE_TRADITIONAL_SPANISH)
////        {
////            m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord( m_name, m_useUserOverride );
////        }
////        else
////        {
////            m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord( cultureID, m_useUserOverride );
////        }
////
////        m_isInherited = (this.GetType() != typeof( System.Globalization.CultureInfo ));
////
////        if(m_name == null)
////        {
////            m_name = m_cultureTableRecord.ActualName;
////        }
////
////        // in case we have non customized CultureInfo object we shouldn't allow any customized object
////        // to be attached to it for cross app domain safety.
////        if(this.GetType().Assembly == typeof( System.Globalization.CultureInfo ).Assembly)
////        {
////            if(textInfo != null)
////            {
////                CheckDomainSafetyObject( textInfo, this );
////            }
////
////            if(compareInfo != null)
////            {
////                CheckDomainSafetyObject( compareInfo, this );
////            }
////        }
////    }
////
////    [OnSerializing]
////    private void OnSerializing( StreamingContext ctx )
////    {
////        m_name            = m_cultureTableRecord.CultureName;
////        m_useUserOverride = m_cultureTableRecord.UseUserOverride;
////        m_dataItem        = m_cultureTableRecord.EverettDataItem();
////    }
        #endregion Serialization

////    internal bool IsSafeCrossDomain
////    {
////        get
////        {
////            BCLDebug.Assert( m_createdDomainID != 0, "[CultureInfo.IsSafeCrossDomain] m_createdDomainID != 0" );
////            return m_isSafeCrossDomain;
////        }
////    }
////
////    internal int CreatedDomainID
////    {
////        get
////        {
////            BCLDebug.Assert( m_createdDomainID != 0, "[CultureInfo.CreatedDomain] m_createdDomainID != 0" );
////            return m_createdDomainID;
////        }
////    }
////
////    internal void StartCrossDomainTracking()
////    {
////        // If we have decided about cross domain safety of this instance, we are done
////        if(m_createdDomainID != 0)
////        {
////            return;
////        }
////
////        if(this.GetType() == typeof( System.Globalization.CultureInfo ))
////        {
////            m_isSafeCrossDomain = true;
////        }
////
////        // m_createdDomainID has to be assigned last. We use it to signal that we have
////        // completed the check.
////        System.Threading.Thread.MemoryBarrier();
////
////        m_createdDomainID = Thread.GetDomainID();
////    }
////
////    // Constructor called by SQL Server's special munged culture - creates a culture with
////    // a TextInfo and CompareInfo that come from a supplied alternate source. This object
////    // is ALWAYS read-only.
////    // Note that we really cannot use an LCID version of this override as the cached
////    // name we create for it has to include both names, and the logic for this is in
////    // the GetCultureInfo override *only*.
////    internal CultureInfo( String cultureName, String textAndCompareCultureName )
////    {
////        if(cultureName == null)
////        {
////            throw new ArgumentNullException( "cultureName", Environment.GetResourceString( "ArgumentNull_String" ) );
////        }
////
////        this.m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord( cultureName, false );
////        this.cultureID            = this.m_cultureTableRecord.ActualCultureID;
////        this.m_name               = this.m_cultureTableRecord.ActualName;
////
////        CultureInfo altCulture = GetCultureInfo( textAndCompareCultureName );
////
////        this.compareInfo = altCulture.CompareInfo;
////        this.textInfo    = altCulture.TextInfo;
////    }
////
////
////    // Get a culture when trying to create it from LCID or name
////    // This happens when asking windows for the locale information.
////    // In this case we want to use LCID if provided, otherwise use
////    // the name.  Using LCID gives us system default values, using
////    // names would allow users to override system cultures when we
////    // don't expect them to.
////    //
////    // Note that this should pick up a CC of the same name for
////    // OS ELKS/LIPS/whatever, but will throw if no CC is installed.
////    private static CultureInfo GetCultureByLCIDOrName( int preferLCID, String fallbackToString )
////    {
////        // If we don't have a string or we don't have
////        CultureInfo culture = null;
////
////        // We prefer LCID because that gives us the system values, but we can't use
////        // it if there's no lang part
////        if((preferLCID & 0x3ff) != 0)
////        {
////            // Try to get it
////            try
////            {
////                culture = new CultureInfo( preferLCID );
////            }
////            catch(ArgumentException)
////            {
////            }
////        }
////
////        // If it didn't work by # we need to try string (if possible)
////        if(culture == null && fallbackToString != null && fallbackToString.Length > 0)
////        {
////            // Try to get it
////            try
////            {
////                culture = new CultureInfo( fallbackToString );
////            }
////            catch(ArgumentException)
////            {
////            }
////        }
////
////        return culture;
////    }
////
////
////
////    public static CultureInfo CreateSpecificCulture( String name )
////    {
////        CultureInfo culture;
////
////        try
////        {
////            culture = new CultureInfo( name );
////        }
////        catch(ArgumentException exp)
////        {
////            // When CultureInfo throws this exception, it may be because someone passed the form
////            // like "az-az" because it came out of an http accept lang. We should try a little
////            // parsing to perhaps fall back to "az" here and use *it* to create the neutral.
////
////            int idx;
////
////            culture = null;
////            for(idx = 0; idx < name.Length; idx++)
////            {
////                if('-' == name[idx])
////                {
////                    try
////                    {
////                        culture = new CultureInfo( name.Substring( 0, idx ) );
////                        break;
////                    }
////                    catch(ArgumentException)
////                    {
////                        // throw the original exception so the name in the string will be right
////                        throw (exp);
////                    }
////                }
////            }
////
////            if(culture == null)
////            {
////                // nothing to save here; throw the original exception
////                throw (exp);
////            }
////        }
////
////        //In the most common case, they've given us a specific culture, so we'll just return that.
////        if(!(culture.IsNeutralCulture))
////        {
////            return culture;
////        }
////
////        int lcid = culture.LCID;
////        //If we have the Chinese locale, we have no way of producing a
////        //specific culture without encountering significant geopolitical
////        //issues.  Based on that, we have no choice but to return.
////        if((lcid & 0x3FF) == 0x04)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_NoSpecificCulture" ) );
////        }
////
////        return (new CultureInfo( culture.m_cultureTableRecord.SSPECIFICCULTURE ));
////    }
////
////    internal static bool VerifyCultureName( CultureInfo culture, bool throwException )
////    {
////        BCLDebug.Assert( culture != null, "[CultureInfo.VerifyCultureName]culture!=null" );
////
////        //If we have an instance of one of our CultureInfos, the user can't have changed the
////        //name and we know that all names are valid in files.
////        if(!culture.m_isInherited)
////        {
////            return true;
////        }
////
////        // This function is used by ResourceManager.GetResourceFileName().
////        // ResourceManager searches for resource using CultureInfo.Name,
////        // so we should check against CultureInfo.Name.
////        String name = culture.Name;
////
////        for(int i = 0; i < name.Length; i++)
////        {
////            char c = name[i];
////            if(Char.IsLetterOrDigit( c ) || c == '-' || c == '_')
////            {
////                continue;
////            }
////
////            if(throwException)
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidResourceCultureName", name ) );
////            }
////
////            return false;
////        }
////
////        return true;
////    }
////
////    //--------------------------------------------------------------------//
////    //                        Misc static functions                       //
////    //--------------------------------------------------------------------//
////
////    internal static int GetSubLangID( int culture )
////    {
////        return ((culture >> 10) & 0x3f);
////    }
////
////    internal static int GetLangID( int culture )
////    {
////        return (culture & 0xffff);
////    }
////
////    internal static int GetSortID( int lcid )
////    {
////        return ((lcid >> 16) & 0xf);
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  CurrentCulture
////    //
////    //  This instance provides methods based on the current user settings.
////    //  These settings are volatile and may change over the lifetime of the
////    //  thread.
////    //
////    ////////////////////////////////////////////////////////////////////////


        public static CultureInfo CurrentCulture
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture;
            }
        }

        //
        // This is the equivalence of the Win32 GetUserDefaultLCID()
        //
        internal static CultureInfo UserDefaultCulture
        {
            get
            {
////            CultureInfo temp = m_userDefaultCulture;
////            if(temp == null)
////            {
////                //
////                // setting the m_userDefaultCulture with invariant culture before intializing it is a protection
////                // against recursion problem just in case if somebody called CurrentCulture from the CultureInfo
////                // creation path. the recursion can happen if the current user culture is a replaced custom culture.
////                //
////
////                m_userDefaultCulture = CultureInfo.InvariantCulture;
////
////                temp = InitUserDefaultCulture();
////
////                m_userDefaultCulture = temp;
////            }
////            return (temp);
                return CultureInfo.InvariantCulture;
            }
        }
    
////    //
////    //  This is the equivalence of the Win32 GetUserDefaultUILanguage()
////    //
////    internal static unsafe CultureInfo UserDefaultUICulture
////    {
////        get
////        {
////            CultureInfo temp = m_userDefaultUICulture;
////            if(temp == null)
////            {
////                //
////                // setting the m_userDefaultCulture with invariant culture before intializing it is a protection
////                // against recursion problem just in case if somebody called CurrentUICulture from the CultureInfo
////                // creation path. the recursion can happen if the current user culture is a replaced custom culture.
////                //
////
////                m_userDefaultUICulture = CultureInfo.InvariantCulture;
////
////                temp = InitUserDefaultUICulture();
////
////                m_userDefaultUICulture = temp;
////            }
////            return (temp);
////        }
////    }
////
////
////    public static CultureInfo CurrentUICulture
////    {
////        get
////        {
////            return Thread.CurrentThread.CurrentUICulture;
////        }
////    }
////
////
////    //
////    // This is the equivalence of the Win32 GetSystemDefaultUILanguage()
////    //
////    public static unsafe CultureInfo InstalledUICulture
////    {
////        get
////        {
////            CultureInfo temp = m_InstalledUICultureInfo;
////            if(temp == null)
////            {
////                int    LCID;
////                String strDefault = nativeGetSystemDefaultUILanguage( &LCID             );
////                temp              = GetCultureByLCIDOrName          (  LCID, strDefault );
////
////                if(temp == null)
////                {
////                    temp = new CultureInfo( LOCALE_INVARIANT, true );
////                }
////
////                temp.m_isReadOnly = true;
////
////                m_InstalledUICultureInfo = temp;
////            }
////            return temp;
////        }
////    }
    
        ////////////////////////////////////////////////////////////////////////
        //
        //  InvariantCulture
        //
        //  This instance provides methods, for example for casing and sorting,
        //  that are independent of the system and current user settings.  It
        //  should be used only by processes such as some system services that
        //  require such invariant results (eg. file systems).  In general,
        //  the results are not linguistically correct and do not match any
        //  culture info.
        //
        ////////////////////////////////////////////////////////////////////////
    
    
        public static CultureInfo InvariantCulture
        {
            get
            {
                return m_InvariantCultureInfo;
            }
        }
    
    
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  Parent
////    //
////    //  Return the parent CultureInfo for the current instance.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////    public virtual CultureInfo Parent
////    {
////        get
////        {
////            if(m_parent == null)
////            {
////                try
////                {
////                    int parentCulture = this.m_cultureTableRecord.IPARENT;
////                    if(parentCulture == LOCALE_INVARIANT)
////                    {
////                        m_parent = InvariantCulture;
////                    }
////                    else if(CultureTableRecord.IsCustomCultureId( parentCulture ))
////                    {
////                        // Customized culture -- use the string for the parent.
////                        m_parent = new CultureInfo( this.m_cultureTableRecord.SPARENT );
////                    }
////                    else
////                    {
////                        m_parent = new CultureInfo( parentCulture, this.m_cultureTableRecord.UseUserOverride );
////                    }
////                }
////                catch(ArgumentException)
////                {
////                    // For whatever reason our IPARENT or SPARENT wasn't correct, so use invariant
////                    // We can't allow ourselves to fail.  In case of custom cultures the parent of the
////                    // current custom culture isn't installed.
////                    m_parent = InvariantCulture;
////                }
////            }
////
////            return m_parent;
////        }
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  LCID
////    //
////    //  Returns a properly formed culture identifier for the current
////    //  culture info.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public virtual int LCID
////    {
////        get
////        {
////            return this.cultureID;
////        }
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  BaseInputLanguage
////    //
////    //  Essentially an LCID, though one that may be different than LCID in the case
////    //  of a customized culture (LCID == -1).
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////    public virtual int KeyboardLayoutId
////    {
////        get
////        {
////            int keyId = this.m_cultureTableRecord.IINPUTLANGUAGEHANDLE;
////
////            // Not a customized culture, return the default Keyboard layout ID, which is the same as the language ID.
////            return keyId;
////        }
////    }
////
////
////    public static CultureInfo[] GetCultures( CultureTypes types )
////    {
////        return CultureTable.Default.GetCultures( types );
////    }
////
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  Name
////    //
////    //  Returns the full name of the CultureInfo. The name is in format like
////    //  "en-US"  This version does NOT include sort information in the name.
////    //
////    ////////////////////////////////////////////////////////////////////////
////    public virtual String Name
////    {
////        get
////        {
////            // We return non sorting name here.
////            if(this.m_nonSortName == null)
////            {
////                this.m_nonSortName = this.m_cultureTableRecord.CultureName;
////            }
////
////            return this.m_nonSortName;
////        }
////    }
////
////
////    internal String SortName
////    {
////        get
////        {
////            if(this.m_sortName == null)
////            {
////                if(CultureTableRecord.IsCustomCultureId( cultureID ))
////                {
////                    CultureInfo sortCI = CultureInfo.GetCultureInfo( CompareInfoId );
////
////                    BCLDebug.Assert( !CultureTableRecord.IsCustomCultureId( sortCI.cultureID ),
////                                    "[CultureInfo.SortName]Expected non-custom sort id" );
////
////                    if(CultureTableRecord.IsCustomCultureId( sortCI.cultureID ))
////                    {
////                        // m_name create could call SortName (not supposed to for CI),
////                        // but just to be safe, use SNAME not m_name
////                        this.m_sortName = m_cultureTableRecord.SNAME;
////                    }
////                    else
////                    {
////                        this.m_sortName = sortCI.SortName;  // Name of culture doing our sort
////                    }
////                }
////                else
////                {
////                    // If its not custom, our sort is our full name
////                    BCLDebug.Assert( m_name != null, "[CultureInfo.SortName]Always expect m_name to be set" );
////                    this.m_sortName = this.m_name;          // full name, including sort
////                }
////            }
////
////            return this.m_sortName;
////        }
////    }
////
////    public String IetfLanguageTag
////    {
////        get
////        {
////            if(this.m_ietfName == null)
////            {
////                this.m_ietfName = this.m_cultureTableRecord.SIETFTAG;
////            }
////
////            // IETF RFC 3066 name goes here
////            return this.m_ietfName;
////        }
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  DisplayName
////    //
////    //  Returns the full name of the CultureInfo in the localized language.
////    //  For example, if the localized language of the runtime is Spanish and the CultureInfo is
////    //  US English, "Ingles (Estados Unidos)" will be returned.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public virtual String DisplayName
////    {
////        get
////        {
////            BCLDebug.Assert( m_name != null, "[CultureInfo.DisplayName]Always expect m_name to be set" );
////
////            if(m_cultureTableRecord.IsCustomCulture)
////            {
////                if(m_cultureTableRecord.IsReplacementCulture)
////                {
////                    // <SyntheticSupport/>
////                    if(m_cultureTableRecord.IsSynthetic)
////                    {
////                        return m_cultureTableRecord.CultureNativeDisplayName;
////                    }
////                    else
////                    {
////                        return (Environment.GetResourceString( "Globalization.ci_" + this.m_name ));
////                    }
////                }
////                else
////                {
////                    return m_cultureTableRecord.SNATIVEDISPLAYNAME;
////                }
////            }
////            else
////            {
////                // <SyntheticSupport/>
////                if(m_cultureTableRecord.IsSynthetic)
////                {
////                    return m_cultureTableRecord.CultureNativeDisplayName;
////                }
////                else
////                {
////                    return (Environment.GetResourceString( "Globalization.ci_" + this.m_name ));
////                }
////            }
////        }
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  GetNativeName
////    //
////    //  Returns the full name of the CultureInfo in the native language.
////    //  For example, if the CultureInfo is US English, "English
////    //  (United States)" will be returned.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public virtual String NativeName
////    {
////        get
////        {
////            return (this.m_cultureTableRecord.SNATIVEDISPLAYNAME);
////        }
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  GetEnglishName
////    //
////    //  Returns the full name of the CultureInfo in English.
////    //  For example, if the CultureInfo is US English, "English
////    //  (United States)" will be returned.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public virtual String EnglishName
////    {
////        get
////        {
////            return this.m_cultureTableRecord.SENGDISPLAYNAME;
////        }
////    }
////
////
////    public virtual String TwoLetterISOLanguageName
////    {
////        get
////        {
////            return this.m_cultureTableRecord.SISO639LANGNAME;
////        }
////    }
////
////
////    public virtual String ThreeLetterISOLanguageName
////    {
////        get
////        {
////            return this.m_cultureTableRecord.SISO639LANGNAME2;
////        }
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  GetAbbreviatedName
////    //
////    //  Returns the abbreviated name for the current instance.  The
////    //  abbreviated form is usually based on the ISO 639 standard, for
////    //  example the two letter abbreviation for English is "en".
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public virtual String ThreeLetterWindowsLanguageName
////    {
////        get
////        {
////            return this.m_cultureTableRecord.SABBREVLANGNAME;
////        }
////    }

        ////////////////////////////////////////////////////////////////////////
        //
        //  CompareInfo               Read-Only Property
        //
        //  Gets the CompareInfo for this culture.
        //
        ////////////////////////////////////////////////////////////////////////


        public extern virtual CompareInfo CompareInfo
        {
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
////        {
////            if(compareInfo == null)
////            {
////
////                CompareInfo temp;
////
////                // In case of non custom neutral cultures we always return compare info with neutral culture id
////                // and neutral culture name. this to be compatible with Everett.
////
////                if(IsNeutralCulture && !CultureTableRecord.IsCustomCultureId( cultureID ))
////                {
////                    temp = CompareInfo.GetCompareInfo( cultureID );
////                }
////                else
////                {
////                    temp = CompareInfo.GetCompareInfo( CompareInfoId );
////                }
////
////                // The name is not preset when CompareInfo is set by LCID, so lets set it now.
////                temp.SetName( this.SortName );
////                compareInfo = temp;
////
////            }
////            return compareInfo;
////        }
        }

////    internal int CompareInfoId
////    {
////        get
////        {
////            int compareId;
////
////            if(this.cultureID == LOCALE_TRADITIONAL_SPANISH)
////            {
////                // Special case Trad. Spanish since there is no
////                // ICOMPAREINFO entry for them.
////                compareId = LOCALE_TRADITIONAL_SPANISH;
////            }
////            else if(GetSortID( this.cultureID ) != 0)
////            {
////                // Special case alternate sorts since there is no
////                // ICOMPAREINFO entry for them.
////                compareId = this.cultureID;
////            }
////            else
////            {
////                compareId = unchecked( (int)this.m_cultureTableRecord.ICOMPAREINFO );
////            }
////
////            return compareId;
////        }
////    }
    
        ////////////////////////////////////////////////////////////////////////
        //
        //  TextInfo
        //
        //  Gets the TextInfo for this culture.
        //
        ////////////////////////////////////////////////////////////////////////
    
    
        public virtual TextInfo TextInfo
        {
            get
            {
                if(textInfo == null)
                {
////                // Make a new textInfo
////                TextInfo tempTextInfo = new TextInfo( this.m_cultureTableRecord );
////
////                tempTextInfo.SetReadOnlyState( m_isReadOnly );
////
////                textInfo = tempTextInfo;
                    textInfo = new TextInfo();
                }
    
                return textInfo;
            }
        }
    
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  Equals
////    //
////    //  Implements Object.Equals().  Returns a boolean indicating whether
////    //  or not object refers to the same CultureInfo as the current instance.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public override bool Equals( Object value )
////    {
////        if(Object.ReferenceEquals( this, value ))
////        {
////            return true;
////        }
////
////        CultureInfo that = value as CultureInfo;
////        if(that != null)
////        {
////            // using CompareInfo to verify the data passed through the constructor
////            // CultureInfo(String cultureName, String textAndCompareCultureName)
////
////            return (this.Name.Equals( that.Name ) && this.CompareInfo.Equals( that.CompareInfo ));
////        }
////
////        return (false);
////    }
////
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  GetHashCode
////    //
////    //  Implements Object.GetHashCode().  Returns the hash code for the
////    //  CultureInfo.  The hash code is guaranteed to be the same for CultureInfo A
////    //  and B where A.Equals(B) is true.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public override int GetHashCode()
////    {
////        return this.Name.GetHashCode() + this.CompareInfo.GetHashCode();
////    }
////
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  ToString
////    //
////    //  Implements Object.ToString().  Returns the name of the CultureInfo,
////    //  eg. "English (United States)".
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public override String ToString()
////    {
////        BCLDebug.Assert( m_name != null, "[CultureInfo.ToString]Always expect m_name to be set" );
////        return m_name;
////    }


        public virtual Object GetFormat( Type formatType )
        {
            if(formatType == typeof( NumberFormatInfo ))
            {
                return NumberFormat;
            }
    
////        if(formatType == typeof( DateTimeFormatInfo ))
////        {
////            return DateTimeFormat;
////        }

            return null;
        }

////    internal static void CheckNeutral( CultureInfo culture )
////    {
////        if(culture.IsNeutralCulture)
////        {
////            BCLDebug.Assert( culture.m_name != null, "[CultureInfo.CheckNeutral]Always expect m_name to be set" );
////            throw new NotSupportedException( Environment.GetResourceString( "Argument_CultureInvalidFormat", culture.m_name ) );
////        }
////    }
////
////    public virtual bool IsNeutralCulture
////    {
////        get
////        {
////            return this.m_cultureTableRecord.IsNeutralCulture;
////        }
////    }
////
////    public CultureTypes CultureTypes
////    {
////        get
////        {
////            CultureTypes types = 0;
////
////            if(m_cultureTableRecord.IsNeutralCulture)
////            {
////                types |= CultureTypes.NeutralCultures;
////            }
////            else
////            {
////                types |= CultureTypes.SpecificCultures;
////            }
////
////            if(m_cultureTableRecord.IsSynthetic)
////            {
////                types |= CultureTypes.WindowsOnlyCultures | CultureTypes.InstalledWin32Cultures; // Synthetic is installed culture too.
////            }
////            else
////            {
////                // Not Synthetic
////                if(CultureTable.IsInstalledLCID( cultureID ))
////                {
////                    types |= CultureTypes.InstalledWin32Cultures;
////                }
////
////                if(!m_cultureTableRecord.IsCustomCulture || m_cultureTableRecord.IsReplacementCulture)
////                {
////                    types |= CultureTypes.FrameworkCultures;
////                }
////            }
////
////            if(m_cultureTableRecord.IsCustomCulture)
////            {
////                types |= CultureTypes.UserCustomCulture;
////
////                if(m_cultureTableRecord.IsReplacementCulture)
////                {
////                    types |= CultureTypes.ReplacementCultures;
////                }
////            }
////
////            return types;
////        }
////    }
    
        public virtual NumberFormatInfo NumberFormat
        {
            get
            {
////            CultureInfo.CheckNeutral( this );
                if(numInfo == null)
                {
                    NumberFormatInfo temp = new NumberFormatInfo( this.m_cultureTableRecord );
    
                    temp.isReadOnly = m_isReadOnly;
    
                    numInfo = temp;
                }
                return (numInfo);
            }
            set
            {
////            VerifyWritable();
                if(value == null)
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentNullException( "value", Environment.GetResourceString( "ArgumentNull_Obj" ) );
#else
                    throw new ArgumentNullException();
#endif
                }
                numInfo = value;
            }
        }
    
        ////////////////////////////////////////////////////////////////////////
        //
        // GetDateTimeFormatInfo
        //
        // Create a DateTimeFormatInfo, and fill in the properties according to
        // the CultureID.
        //
        ////////////////////////////////////////////////////////////////////////
    
    
        public virtual DateTimeFormatInfo DateTimeFormat
        {
            get
            {
                if(dateTimeInfo == null)
                {
////                CultureInfo.CheckNeutral( this );
////                // Change the calendar of DTFI to the specified calendar of this CultureInfo.
////                DateTimeFormatInfo temp = new DateTimeFormatInfo( this.m_cultureTableRecord, GetLangID( cultureID ), this.Calendar );
////
////                temp.m_isReadOnly = m_isReadOnly;
////
////                System.Threading.Thread.MemoryBarrier();
////
////                dateTimeInfo = temp;
                    dateTimeInfo = new DateTimeFormatInfo();
                }
    
                return dateTimeInfo;
            }
    
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value", Environment.GetResourceString( "ArgumentNull_Obj" ) );
////            }
////            dateTimeInfo = value;
////        }
        }
    
    
    
////    public unsafe void ClearCachedData()
////    {
////        m_userDefaultUICulture = null;
////        m_userDefaultCulture   = null;
////
////        RegionInfo.m_currentRegionInfo = null;
////
////        TimeZone.ResetTimeZone();
////
////        // Delete the cached cultures.
////        m_LcidCachedCultures = null;
////        m_NameCachedCultures = null;
////        m_IetfCachedCultures = null;
////
////        CultureTableRecord.ResetCustomCulturesCache();
////        CompareInfo.ClearDefaultAssemblyCache();
////    }
////
////    /*=================================GetCalendarInstance==========================
////    **Action: Map a Win32 CALID to an instance of supported calendar.
////    **Returns: An instance of calendar.
////    **Arguments: calType    The Win32 CALID
////    **Exceptions:
////    **      Shouldn't throw exception since the calType value is from our data table or from Win32 registry.
////    **      If we are in trouble (like getting a weird value from Win32 registry), just return the GregorianCalendar.
////    ============================================================================*/
////    internal static Calendar GetCalendarInstance( int calType )
////    {
////        if(calType == Calendar.CAL_GREGORIAN)
////        {
////            return new GregorianCalendar();
////        }
////
////        return GetCalendarInstanceRare( calType );
////    }
////
////    //This function exists as a shortcut to prevent us from loading all of the non-gregorian
////    //calendars unless they're required.
////    internal static Calendar GetCalendarInstanceRare( int calType )
////    {
////        BCLDebug.Assert( calType != Calendar.CAL_GREGORIAN, "calType!=Calendar.CAL_GREGORIAN" );
////
////        switch(calType)
////        {
////            case Calendar.CAL_GREGORIAN_US:               // Gregorian (U.S.) calendar
////            case Calendar.CAL_GREGORIAN_ME_FRENCH:        // Gregorian Middle East French calendar
////            case Calendar.CAL_GREGORIAN_ARABIC:           // Gregorian Arabic calendar
////            case Calendar.CAL_GREGORIAN_XLIT_ENGLISH:     // Gregorian Transliterated English calendar
////            case Calendar.CAL_GREGORIAN_XLIT_FRENCH:      // Gregorian Transliterated French calendar
////                return new GregorianCalendar( (GregorianCalendarTypes)calType );
////
////            case Calendar.CAL_TAIWAN:                     // Taiwan Era calendar
////                return new TaiwanCalendar();
////
////            case Calendar.CAL_JAPAN:                      // Japanese Emperor Era calendar
////                return new JapaneseCalendar();
////
////            case Calendar.CAL_KOREA:                      // Korean Tangun Era calendar
////                return new KoreanCalendar();
////
////            case Calendar.CAL_HIJRI:                      // Hijri (Arabic Lunar) calendar
////                return new HijriCalendar();
////
////            case Calendar.CAL_THAI:                       // Thai calendar
////                return new ThaiBuddhistCalendar();
////
////            case Calendar.CAL_HEBREW:                     // Hebrew (Lunar) calendar
////                return new HebrewCalendar();
////
////            case Calendar.CAL_PERSIAN:
////                return new PersianCalendar();
////
////            case Calendar.CAL_UMALQURA:
////                return new UmAlQuraCalendar();
////
////            case Calendar.CAL_CHINESELUNISOLAR:
////                return new ChineseLunisolarCalendar();
////
////            case Calendar.CAL_JAPANESELUNISOLAR:
////                return new JapaneseLunisolarCalendar();
////
////            case Calendar.CAL_KOREANLUNISOLAR:
////                return new KoreanLunisolarCalendar();
////
////            case Calendar.CAL_TAIWANLUNISOLAR:
////                return new TaiwanLunisolarCalendar();
////        }
////
////        return new GregorianCalendar();
////    }
////
////
////    /*=================================Calendar==========================
////    **Action: Return/set the default calendar used by this culture.
////    ** This value can be overridden by regional option if this is a current culture.
////    **Returns:
////    **Arguments:
////    **Exceptions:
////    **  ArgumentNull_Obj if the set value is null.
////    ============================================================================*/
////
////
////    public virtual Calendar Calendar
////    {
////        get
////        {
////            if(calendar == null)
////            {
////                BCLDebug.Assert( this.m_cultureTableRecord.IOPTIONALCALENDARS.Length > 0, "this.m_cultureTableRecord.IOPTIONALCALENDARS.Length > 0" );
////                // Get the default calendar for this culture.  Note that the value can be
////                // from registry if this is a user default culture.
////                int calType = this.m_cultureTableRecord.ICALENDARTYPE;
////
////                Calendar newObj = GetCalendarInstance( calType );
////
////                System.Threading.Thread.MemoryBarrier();
////
////                newObj.SetReadOnlyState( m_isReadOnly );
////
////                calendar = newObj;
////            }
////            return calendar;
////        }
////    }
////
////    /*=================================OptionCalendars==========================
////    **Action: Return an array of the optional calendar for this culture.
////    **Returns: an array of Calendar.
////    **Arguments:
////    **Exceptions:
////    ============================================================================*/
////
////
////    public virtual Calendar[] OptionalCalendars
////    {
////        get
////        {
////            //
////            // This property always returns a new copy of the calendar array.
////            //
////            int[]      calID = this.m_cultureTableRecord.IOPTIONALCALENDARS;
////            Calendar[] cals  = new Calendar[calID.Length];
////            for(int i = 0; i < cals.Length; i++)
////            {
////                cals[i] = GetCalendarInstance( calID[i] );
////            }
////            return cals;
////        }
////    }
////
////
////    public bool UseUserOverride
////    {
////        get
////        {
////            return this.m_cultureTableRecord.UseUserOverride;
////        }
////    }
////
////    public CultureInfo GetConsoleFallbackUICulture()
////    {
////        CultureInfo temp = m_consoleFallbackCulture;
////        if(temp == null)
////        {
////            temp = GetCultureInfo( this.m_cultureTableRecord.SCONSOLEFALLBACKNAME );
////
////            temp.m_isReadOnly = true;
////
////            m_consoleFallbackCulture = temp;
////        }
////        return temp;
////    }
////
////
////    public virtual Object Clone()
////    {
////        CultureInfo ci = (CultureInfo)MemberwiseClone();
////
////        ci.m_isReadOnly = false;
////
////        if(!ci.IsNeutralCulture)
////        {
////            //If this is exactly our type, we can make certain optimizations so that we don't allocate NumberFormatInfo or DTFI unless
////            //they've already been allocated.  If this is a derived type, we'll take a more generic codepath.
////            if(!m_isInherited)
////            {
////                if(dateTimeInfo != null)
////                {
////                    ci.dateTimeInfo = (DateTimeFormatInfo)dateTimeInfo.Clone();
////                }
////                if(numInfo != null)
////                {
////                    ci.numInfo = (NumberFormatInfo)numInfo.Clone();
////                }
////
////            }
////            else
////            {
////                ci.DateTimeFormat = (DateTimeFormatInfo)this.DateTimeFormat.Clone();
////                ci.NumberFormat   = (NumberFormatInfo  )this.NumberFormat  .Clone();
////            }
////        }
////
////        if(textInfo != null)
////        {
////            ci.textInfo = (TextInfo)textInfo.Clone();
////        }
////
////        if(calendar != null)
////        {
////            ci.calendar = (Calendar)calendar.Clone();
////        }
////
////        return ci;
////    }
////
////
////    public static CultureInfo ReadOnly( CultureInfo ci )
////    {
////        if(ci == null)
////        {
////            throw new ArgumentNullException( "ci" );
////        }
////
////        if(ci.IsReadOnly)
////        {
////            return ci;
////        }
////
////        CultureInfo info = (CultureInfo)ci.MemberwiseClone();
////
////        if(!ci.IsNeutralCulture)
////        {
////            //If this is exactly our type, we can make certain optimizations so that we don't allocate NumberFormatInfo or DTFI unless
////            //they've already been allocated.  If this is a derived type, we'll take a more generic codepath.
////            if(!ci.m_isInherited)
////            {
////                if(ci.dateTimeInfo != null)
////                {
////                    info.dateTimeInfo = DateTimeFormatInfo.ReadOnly( ci.dateTimeInfo );
////                }
////
////                if(ci.numInfo != null)
////                {
////                    info.numInfo = NumberFormatInfo.ReadOnly( ci.numInfo );
////                }
////
////            }
////            else
////            {
////                info.DateTimeFormat = DateTimeFormatInfo.ReadOnly( ci.DateTimeFormat );
////                info.NumberFormat   = NumberFormatInfo  .ReadOnly( ci.NumberFormat   );
////            }
////        }
////
////        if(ci.textInfo != null)
////        {
////            info.textInfo = TextInfo.ReadOnly( ci.textInfo );
////        }
////
////        if(ci.calendar != null)
////        {
////            info.calendar = Calendar.ReadOnly( ci.calendar );
////        }
////
////        // Don't set the read-only flag too early.
////        // We should set the read-only flag here.  Otherwise, info.DateTimeFormat will not be able to set.
////        info.m_isReadOnly = true;
////
////        return info;
////    }
////
////
////    public bool IsReadOnly
////    {
////        get
////        {
////            return m_isReadOnly;
////        }
////    }
////
////    private void VerifyWritable()
////    {
////        if(m_isReadOnly)
////        {
////            throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_ReadOnly" ) );
////        }
////    }
////
////    // Helper function both both overloads of GetCachedReadOnlyCulture.  If lcid is 0, we use the name.
////    // If lcid is -1, use the altName and create one of those special SQL cultures.
////    // If lcid is -2 we're looking for Ietf name
////    internal static CultureInfo GetCultureInfoHelper( int lcid, string name, string altName )
////    {
////        // retval is our return value.
////        CultureInfo retval;
////
////        // Temporary hashtable for the names.
////        Hashtable tempNameHT = m_NameCachedCultures;
////
////        if(name != null)
////        {
////            name = CultureTableRecord.AnsiToLower( name );
////        }
////
////        if(altName != null)
////        {
////            altName = CultureTableRecord.AnsiToLower( altName );
////        }
////
////        // We expect the same result for all 3 hashtables, but will test individually for added safety.
////        if(tempNameHT == null)
////        {
////            tempNameHT = Hashtable.Synchronized( new Hashtable() );
////        }
////        else
////        {
////            // If we are called by name, check if the object exists in the hashtable.  If so, return it.
////            if(lcid == -1)
////            {
////                retval = (CultureInfo)tempNameHT[name + '\xfffd' + altName];
////                if(retval != null)
////                {
////                    return retval;
////                }
////            }
////            else if(lcid == 0)
////            {
////                retval = (CultureInfo)tempNameHT[name];
////                if(retval != null)
////                {
////                    return retval;
////                }
////            }
////        }
////
////        // Next, the Lcid table.
////        Hashtable tempLcidHT = m_LcidCachedCultures;
////
////        if(tempLcidHT == null)
////        {
////            // Case insensitive is not an issue here, save the constructor call.
////            tempLcidHT = Hashtable.Synchronized( new Hashtable() );
////        }
////        else
////        {
////            // If we were called by Lcid, check if the object exists in the table.  If so, return it.
////            if(lcid > 0)
////            {
////                retval = (CultureInfo)tempLcidHT[lcid];
////                if(retval != null)
////                {
////                    return retval;
////                }
////            }
////        }
////
////        // Last the Ietf name table
////        Hashtable tempIetfHT = m_IetfCachedCultures;
////
////        if(tempIetfHT == null)
////        {
////            tempIetfHT = Hashtable.Synchronized( new Hashtable() );
////        }
////        else
////        {
////            // If we wanted ietf name, then lcid is -2
////            if(lcid == -2)
////            {
////                retval = (CultureInfo)tempIetfHT[name];
////                if(retval != null)
////                {
////                    return retval;
////                }
////            }
////        }
////
////        // We now have two temporary hashtables and the desired object was not found.
////        // We'll construct it.  We catch any exceptions from the constructor call and return null.
////        try
////        {
////            switch(lcid)
////            {
////                case -2:
////                    {
////                        // -2 is Ietf name, map Ietf name to Culture name and get it by name
////                        String cultureName = CultureTableRecord.GetCultureNameFromIetfName( name );
////                        if(cultureName == null)
////                        {
////                            return null;
////                        }
////
////                        retval = new CultureInfo( cultureName, false );
////                        break;
////                    }
////
////                case -1:
////                    // call the private constructor
////                    retval = new CultureInfo( name, altName );
////                    break;
////
////                case 0:
////                    retval = new CultureInfo( name, false );
////                    break;
////
////                default:
////
////                    //
////                    // checking the current culture first is a protection against recursion so the creation
////                    // code of CompareInfo is calling GetCultureInfo.
////                    // The recursion can happen if we have current user culture is replaced custom culture
////                    // and we are trying to create object for that culture and current user culture is not
////                    // stored in our cache.
////                    //
////
////                    if(m_userDefaultCulture != null && m_userDefaultCulture.LCID == lcid)
////                    {
////                        retval = (CultureInfo)m_userDefaultCulture.Clone();
////                        retval.m_cultureTableRecord = retval.m_cultureTableRecord.CloneWithUserOverride( false );
////                    }
////                    else
////                    {
////                        retval = new CultureInfo( lcid, false );
////                    }
////                    break;
////            }
////        }
////        catch(ArgumentException)
////        {
////            return null;
////        }
////
////        // Set it to read-only
////        retval.m_isReadOnly = true;
////
////        if(lcid == -1)
////        {
////            // This new culture will be added only to the name hash table.
////            tempNameHT[name + '\xfffd' + altName] = retval;
////
////            // when lcid == -1 then TextInfo object is already get created and we need to set it as read only.
////            retval.TextInfo.SetReadOnlyState( true );
////        }
////        else
////        {
////            // We add this new culture info object to all 3 hash tables.
////            tempLcidHT[retval.LCID] = retval;
////
////            // Remember our name (as constructed).  Do NOT use alternate sort name versions because
////            // we have internal state representing the sort.  (So someone would get the wrong cached version)
////            string newName = CultureTableRecord.AnsiToLower( retval.m_name );
////
////            tempNameHT[newName] = retval;
////
////            newName = retval.IetfLanguageTag;
////            if(newName != null)
////            {
////                newName = CultureTableRecord.AnsiToLower( newName );
////                tempIetfHT[newName] = retval;
////            }
////
////            if(lcid == -2 && !name.Equals( newName ))
////            {
////                tempIetfHT[name] = retval;
////            }
////
////        }
////
////        // Copy the two hashtables to the corresponding member variables.  This will potentially overwrite
////        // new tables simultaneously created by a new thread, but maximizes thread safety.
////        // If it was funky name we don't need to cache Ietf or LCID names.
////        if(-1 != lcid)
////        {
////            // Only when we modify the lcid hash table, is there a need to overwrite.
////            m_LcidCachedCultures = tempLcidHT;
////            m_IetfCachedCultures = tempIetfHT;
////        }
////
////        m_NameCachedCultures = tempNameHT;
////
////        // Finally, return our new CultureInfo object.
////        return retval;
////    }
////
////    // Gets a cached copy of the specified culture from an internal hashtable (or creates it
////    // if not found).
////    public static CultureInfo GetCultureInfo( int culture )
////    {
////        // Must check for -1 now since the helper function uses the value to signal
////        // the altCulture code path for SQL Server.
////        // Also check for zero as this would fail trying to add as a key to the hash.
////        if(culture <= 0)
////        {
////            throw new ArgumentOutOfRangeException( "culture", Environment.GetResourceString( "ArgumentOutOfRange_NeedPosNum" ) );
////        }
////
////        CultureInfo retval = GetCultureInfoHelper( culture, null, null );
////        if(retval == null)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_CultureNotSupported", culture ), "culture" );
////        }
////
////        return retval;
////    }
////
////    // Gets a cached copy of the specified culture from an internal hashtable (or creates it
////    // if not found).
////    public static CultureInfo GetCultureInfo( string name )
////    {
////        // Make sure we have a valid, non-zero length string as name
////        if(name == null)
////        {
////            throw new ArgumentNullException( "name" );
////        }
////
////        CultureInfo retval = GetCultureInfoHelper( 0, name, null );
////        if(retval == null)
////        {
////            throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_InvalidCultureName" ), name ), "name" );
////        }
////
////        return retval;
////    }
////
////    // Gets a cached copy of the specified culture from an internal hashtable (or creates it
////    // if not found).
////    public static CultureInfo GetCultureInfo( string name, string altName )
////    {
////        // Make sure we have a valid, non-zero length string as name
////        if(name == null)
////        {
////            throw new ArgumentNullException( "name" );
////        }
////
////        if(altName == null)
////        {
////            throw new ArgumentNullException( "altName" );
////        }
////
////        CultureInfo retval = GetCultureInfoHelper( -1, name, altName );
////        if(retval == null)
////        {
////            throw new ArgumentException(
////                            String.Format(
////                                CultureInfo.CurrentCulture,
////                                Environment.GetResourceString( "Argument_OneOfCulturesNotSupported" ),
////                                name,
////                                altName ),
////                            "name" );
////        }
////
////        return retval;
////    }
////
////    // Get a cached copy of the specified culture from ietf language tag.
////    public static CultureInfo GetCultureInfoByIetfLanguageTag( string name )
////    {
////        // Make sure we have a valid, non-zero length string as name
////        if(name == null)
////        {
////            throw new ArgumentNullException( "name" );
////        }
////
////        CultureInfo retval = GetCultureInfoHelper( -2, name, null );
////        if(retval == null)
////        {
////            throw new ArgumentException(
////                            String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString(
////                                "Argument_CultureIetfNotSupported" ), name ),
////                                "name" );
////        }
////
////        return retval;
////    }
////
////    // Looks for Ietf names in the registry.
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////    internal static string LookupIetfNameFromRegistry( string name )
////    {
////        // Make sure our key is open
////        if(m_ietfNameLookupKey == null)
////        {
////            m_ietfNameLookupKey = Registry.LocalMachine.OpenSubKey( ietfRegistryKey );
////
////            // If not present that's OK, we just won't find one.
////            if(m_ietfNameLookupKey == null)
////            {
////                return null;
////            }
////        }
////
////        // Look up our particular name to see if it works
////        String realNames = m_ietfNameLookupKey.GetValue( name ) as string;
////
////        if(String.IsNullOrEmpty( realNames ))
////        {
////            return null;
////        }
////
////        int iEndOfFirstName = realNames.IndexOf( ';' );
////
////        // If only one name then return it
////        if(iEndOfFirstName < 0)
////        {
////            return realNames;
////        }
////
////        // If no first name, return null
////        if(iEndOfFirstName == 0)
////        {
////            return null;
////        }
////
////        // Return just the first name
////        return realNames.Substring( 0, iEndOfFirstName );
////    }
    }
}

