// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
////////////////////////////////////////////////////////////////////////////
//
//  Class:    CompareInfo
//
//
//  Purpose:  This class implements a set of methods for comparing
//            strings.
//
//  Date:     August 12, 1998
//
////////////////////////////////////////////////////////////////////////////

namespace System.Globalization
{
    //
    // We're dependent on the m_pSortingTable getting created when an instance of the
    // class is initialized (through a call to InitializeCompareInfo).  When in
    // native, we assume that the table has already been allocated.  we may decide
    // to delay-allocate any of the tables (as we may do for US English)//
    // System.Globalization.SortKey also uses the m_pSortingTables.  Currently the only
    // way to get a SortKey is to call through CompareInfo, which means that the table
    // has already been initialized.  If this invariant changes, SortKey's constructor
    // needs to call InitializedSortingTable (which can safely be called multiple times
    // for the same locale.)
    //

    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
////using System.Threading;
////using System.Security.Permissions;
    using System.Runtime.InteropServices;
////using System.Runtime.Versioning;
////using Microsoft.Win32;

    //
    //  Options can be used during string comparison.
    //
    //  Native implementation (COMNlsInfo.cpp & SortingTable.cpp) relies on the values of these,
    //  If you change the values below, be sure to change the values in native part as well.
    //


    [Flags]
    [Serializable]
    public enum CompareOptions
    {
        None              = 0x00000000,
        IgnoreCase        = 0x00000001,
        IgnoreNonSpace    = 0x00000002,
        IgnoreSymbols     = 0x00000004,
        IgnoreKanaType    = 0x00000008,   // ignore kanatype
        IgnoreWidth       = 0x00000010,   // ignore width
        OrdinalIgnoreCase = 0x10000000,   // This flag can not be used with other flags.
        StringSort        = 0x20000000,   // use string sort method
        Ordinal           = 0x40000000,   // This flag can not be used with other flags.
        // StopOnNull      = 0x10000000,

        // StopOnNull is defined in SortingTable.h, but we didn't enable this option here.
        // Do not use this value for other flags accidentally.
    }


    [Serializable]
    public class CompareInfo /*: IDeserializationCallback*/
    {
        // Mask used to check if IndexOf()/LastIndexOf()/IsPrefix()/IsPostfix() has the right flags.
        private const CompareOptions ValidIndexMaskOffFlags            = ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType);
        // Mask used to check if Compare() has the right flags.
        private const CompareOptions ValidCompareMaskOffFlags          = ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.StringSort);
        // Mask used to check if GetHashCodeOfString() has the right flags.
        private const CompareOptions ValidHashCodeOfStringMaskOffFlags = ~(CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType);
    
////    [ResourceExposure( ResourceScope.Process )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    unsafe internal static extern byte[] nativeCreateSortKey( void* pSortingFile, String pString, int dwFlags, int win32LCID );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    unsafe internal static extern int nativeGetGlobalizedHashCode( void* pSortingFile, String pString, int dwFlags, int win32LCID );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    unsafe internal static extern bool nativeIsSortable( void* pSortingFile, String pString );
////
////    // <SyntheticSupport/>
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    internal static extern int nativeCompareString( int lcid, string string1, int offset1, int length1,
////                                                             string string2, int offset2, int length2, int flags );
////
////    // Private object for locking instead of locking on a public type for SQL reliability work.
////    private static Object s_InternalSyncObject;
////
////    private static Object InternalSyncObject
////    {
////        get
////        {
////            if(s_InternalSyncObject == null)
////            {
////                Object o = new Object();
////
////                Interlocked.CompareExchange( ref s_InternalSyncObject, o, null );
////            }
////
////            return s_InternalSyncObject;
////        }
////    }
////
////    /*=================================GetCompareInfo==========================
////    **Action: Get the CompareInfo constructed from the data table in the specified assembly for the specified culture.
////    **       The purpose of this method is to provide versioning for CompareInfo tables.
////    **       If you pass Assembly which contains different sorting table, you will sort your strings using the data
////    **       in the assembly.
////    **Returns: The CompareInfo for the specified culture.
////    **Arguments:
////    **   culture     the ID of the culture
////    **   assembly   the assembly which contains the sorting table.
////    **Exceptions:
////    **  ArugmentNullException when the assembly is null
////    **  ArgumentException if culture is invalid.
////    ============================================================================*/
////
////    /* The design goal here is that we can still provide version even when the underlying algorithm for CompareInfo
////         is totally changed in the future.
////         In the case that the algorithm for CompareInfo is changed, we can use this method to
////         provide the old algorithm for the old tables.  The idea is to change the implementation for GetCompareInfo()
////         to something like:
////           1. Check the ID of the assembly.
////           2. If the assembly needs old code, create an instance of the old CompareInfo class. The name of CompareInfo
////              will be like CompareInfoVersion1 and extends from CompareInfo.
////           3. Otherwise, create an instance of the current CompareInfo.
////         The CompareInfo ctor always provides the implementation for the current data table.
////    */
////
////
////    public static CompareInfo GetCompareInfo( int culture, Assembly assembly )
////    {
////        // Parameter checking.
////        if(assembly == null)
////        {
////            throw new ArgumentNullException( "assembly" );
////        }
////
////        if(CultureTableRecord.IsCustomCultureId( culture ))
////        {
////            // Customized culture cannot be created by the LCID.
////            throw new ArgumentException( Environment.GetResourceString( "Argument_CustomCultureCannotBePassedByNumber", "culture" ) );
////        }
////
////        if(assembly != typeof( Object ).Module.Assembly)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_OnlyMscorlib" ) );
////        }
////
////        // culture is verified to see if it is valid when CompareInfo is constructed.
////
////        GlobalizationAssembly ga = GlobalizationAssembly.GetGlobalizationAssembly( assembly );
////
////        Object compInfo = ga.compareInfoCache[culture];
////        if(compInfo == null)
////        {
////            lock(InternalSyncObject)
////            {
////                //
////                // Re-check again to make sure that no one has created the CompareInfo for the culture yet before the current
////                // thread enters this sync block.
////                //
////                if((compInfo = ga.compareInfoCache[culture]) == null)
////                {
////                    compInfo = new CompareInfo( ga, culture );
////
////                    System.Threading.Thread.MemoryBarrier();
////                    ga.compareInfoCache[culture] = compInfo;
////                }
////            }
////        }
////
////        return (CompareInfo)compInfo;
////    }
////
////
////    //
////    // GetCompareInfoByName get CompareInfo object using the name. it is the shared code
////    // between GetCompareInfo(name) and GetCompareInfo(name, assembly)
////    //
////
////    private const int TraditionalChineseCultureId = 0x7C04;
////    private const int HongKongCultureId           = 0x0C04;
////
////    private static CompareInfo GetCompareInfoByName( string name, Assembly assembly )
////    {
////        CultureInfo cultureInfo = CultureInfo.GetCultureInfo( name );
////        if(cultureInfo.IsNeutralCulture && !CultureTableRecord.IsCustomCultureId( cultureInfo.cultureID ))
////        {
////            // When requesting CompareInfo with name we always retun a specific (non neutral) one even
////            // the input name is neutral. this to keep compatibility with Everett.
////            // "passing "en" we'll get "en-US" with 0x409 LCID
////            if(cultureInfo.cultureID == TraditionalChineseCultureId)
////            {
////                cultureInfo = CultureInfo.GetCultureInfo( HongKongCultureId );
////            }
////            else
////            {
////                cultureInfo = CultureInfo.GetCultureInfo( cultureInfo.CompareInfoId );
////            }
////        }
////
////        CompareInfo compareInfo;
////
////        if(assembly != null)
////        {
////            compareInfo = GetCompareInfo( cultureInfo.CompareInfoId, assembly );
////        }
////        else
////        {
////            compareInfo = GetCompareInfo( cultureInfo.CompareInfoId );
////        }
////
////        // Compare info name is known by the cultureInfo
////        compareInfo.m_name = cultureInfo.SortName;
////
////        return compareInfo;
////    }
////
////
////    /*=================================GetCompareInfo==========================
////    **Action: Get the CompareInfo constructed from the data table in the specified assembly for the specified culture.
////    **       The purpose of this method is to provide version for CompareInfo tables.
////    **Returns: The CompareInfo for the specified culture.
////    **Arguments:
////    **   name    the name of the culture
////    **   assembly   the assembly which contains the sorting table.
////    **Exceptions:
////    **  ArugmentNullException when the assembly is null
////    **  ArgumentException if name is invalid.
////    ============================================================================*/
////
////    public static CompareInfo GetCompareInfo( String name, Assembly assembly )
////    {
////        if(name == null || assembly == null)
////        {
////            throw new ArgumentNullException( name == null ? "name" : "assembly" );
////        }
////
////        if(assembly != typeof( Object ).Module.Assembly)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_OnlyMscorlib" ) );
////        }
////
////        return GetCompareInfoByName( name, assembly );
////    }
////
////    /*=================================GetCompareInfo==========================
////    **Action: Get the CompareInfo for the specified culture.
////    ** This method is provided for ease of integration with NLS-based software.
////    **Returns: The CompareInfo for the specified culture.
////    **Arguments:
////    **   culture    the ID of the culture.
////    **Exceptions:
////    **  ArgumentException if culture is invalid.
////    **Notes:
////    **  We optimize in the default case.  We don't go thru the GlobalizationAssembly hashtable.
////    **  Instead, we access the m_defaultInstance directly.
////    ============================================================================*/
////
////    public static CompareInfo GetCompareInfo( int culture )
////    {
////        if(CultureTableRecord.IsCustomCultureId( culture ))
////        {
////            // Customized culture cannot be created by the LCID.
////            throw new ArgumentException( Environment.GetResourceString( "Argument_CustomCultureCannotBePassedByNumber", "culture" ) );
////        }
////
////        // culture is verified to see if it is valid when CompareInfo is constructed.
////        Object compInfo = GlobalizationAssembly.DefaultInstance.compareInfoCache[culture];
////        if(compInfo == null)
////        {
////            lock(InternalSyncObject)
////            {
////                //
////                // Re-check again to make sure that no one has created the CompareInfo for the culture yet before the current
////                // thread enters this sync block.
////                //
////                if((compInfo = GlobalizationAssembly.DefaultInstance.compareInfoCache[culture]) == null)
////                {
////                    compInfo = new CompareInfo( GlobalizationAssembly.DefaultInstance, culture );
////
////                    System.Threading.Thread.MemoryBarrier();
////                    GlobalizationAssembly.DefaultInstance.compareInfoCache[culture] = compInfo;
////                }
////            }
////        }
////        return (CompareInfo)compInfo;
////    }
////
////    /*=================================GetCompareInfo==========================
////    **Action: Get the CompareInfo for the specified culture.
////    **Returns: The CompareInfo for the specified culture.
////    **Arguments:
////    **   name    the name of the culture.
////    **Exceptions:
////    **  ArgumentException if name is invalid.
////    ============================================================================*/
////
////    public static CompareInfo GetCompareInfo( String name )
////    {
////        if(name == null)
////        {
////            throw new ArgumentNullException( "name" );
////        }
////
////        return GetCompareInfoByName( name, null );
////    }
////
////    public static bool IsSortable( char ch )
////    {
////        return (IsSortable( ch.ToString() ));
////    }
////
////    public static bool IsSortable( String text )
////    {
////        if(text == null)
////        {
////            // A null param is invalid here.
////            throw new ArgumentNullException( "text" );
////        }
////
////        if(text.Length == 0)
////        {
////            // A zero length string is not invalid, but it is also not sortable.
////            return false;
////        }
////
////        unsafe
////        {
////            return nativeIsSortable( CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable, text );
////        }
////    }

        #region Serialization
////    // the following fields are defined to keep the compatibility with Everett.
////    // don't change/remove the names/types of these fields.
////    private int win32LCID;             // mapped sort culture id of this instance
////    private int culture;               // the culture ID used to create this instance.
////
////    [OnDeserializing]
////    private void OnDeserializing( StreamingContext ctx )
////    {
////        this.culture       = -1;
////        this.m_sortingLCID = -1;
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////    private unsafe void OnDeserialized()
////    {
////        BCLDebug.Assert( this.culture >= 0, "[CompareInfo.OnDeserialized] - culture >= 0" );
////
////        // Get the Win32 LCID used to create NativeCompareInfo, which only takes Win32 LCID.
////        if(m_sortingLCID <= 0)
////        {
////            m_sortingLCID = GetSortingLCID( this.culture );
////        }
////
////        if(m_pSortingTable == null && !IsSynthetic)
////        {
////            m_pSortingTable = InitializeCompareInfo( GlobalizationAssembly.DefaultInstance.pNativeGlobalizationAssembly, this.m_sortingLCID );
////        }
////
////        BCLDebug.Assert( IsSynthetic || m_pSortingTable != null, "m_pSortingTable != null" );
////        // m_name is intialized later if it not exist in the serialization stream
////    }
////
////
////    [OnDeserialized]
////    private void OnDeserialized( StreamingContext ctx )
////    {
////        OnDeserialized();
////    }
////
////    [OnSerializing]
////    private void OnSerializing( StreamingContext ctx )
////    {
////        win32LCID = m_sortingLCID;
////    }
        #endregion Serialization

////    ///////////////////////////----- Name -----/////////////////////////////////
////    //
////    //  Returns the name of the culture (well actually, of the sort).
////    //  Very important for providing a non-LCID way of identifying
////    //  what the sort is.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////    public virtual String Name
////    {
////        get
////        {
////            if(m_name == null)
////            {
////                m_name = CultureInfo.GetCultureInfo( culture ).SortName;
////            }
////
////            return (m_name);
////        }
////    }
////
////    internal void SetName( string name )
////    {
////        m_name = name;
////    }
////
////    internal static void ClearDefaultAssemblyCache()
////    {
////        lock(InternalSyncObject)
////        {
////            GlobalizationAssembly.DefaultInstance.compareInfoCache = new Hashtable( 4 );
////        }
////    }
////
////
////    internal CultureTableRecord CultureTableRecord
////    {
////        get
////        {
////            if(m_cultureTableRecord == null)
////            {
////                m_cultureTableRecord = CultureInfo.GetCultureInfo( this.m_sortingLCID ).m_cultureTableRecord;
////            }
////
////            return m_cultureTableRecord;
////        }
////    }
////
////    // <SyntheticSupport/>
////    private bool IsSynthetic
////    {
////        get
////        {
////            return CultureTableRecord.IsSynthetic;
////        }
////    }
////
////    //
////    // pSortingTable is a 32-bit pointer value pointing to a native C++ SortingTable object.
////    //
////    [NonSerialized]
////    unsafe internal void*      m_pSortingTable;
////    [NonSerialized]
////    private int                m_sortingLCID;             // mapped sort culture id of this instance
////    [NonSerialized]
////    private CultureTableRecord m_cultureTableRecord;
////
////    private String             m_name = null;                          // The name of the culture of this instance
////
////
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  CompareInfo Constructor
////    //
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    // Constructs an instance that most closely corresponds to the NLS locale
////    // identifier.
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////    internal unsafe CompareInfo( GlobalizationAssembly ga, int culture )
////    {
////        if(culture < 0)
////        {
////            throw new ArgumentOutOfRangeException( "culture", Environment.GetResourceString( "ArgumentOutOfRange_NeedPosNum" ) );
////        }
////
////        // Get the Win32 LCID used to create NativeCompareInfo, which only takes Win32 LCID.
////        this.m_sortingLCID = GetSortingLCID( culture );
////
////        // If version support is enabled, InitializeCompareInfo should use ga instead of getting the default
////        // instance.
////
////        // Call to the native side to create/get the corresponding native C++ SortingTable for this culture.
////        // The returned value is a 32-bit pointer to the native C++ SortingTable instance.
////        // We cache this pointer so that we can call methods of the SortingTable directly.
////        if(!IsSynthetic)
////        {
////            m_pSortingTable = InitializeCompareInfo( GlobalizationAssembly.DefaultInstance.pNativeGlobalizationAssembly, this.m_sortingLCID );
////        }
////
////        // Since this.m_sortingLCID can be different from the passed-in culture in the case of neutral cultures, store the culture ID in a different place.
////        this.culture = culture;
////    }
////
////
////    internal int GetSortingLCID( int culture )
////    {
////        int sortingLCID = 0;
////        //
////        // Verify that this is a valid culture.
////        //
////        CultureInfo cultureObj = CultureInfo.GetCultureInfo( culture );
////
////        // We do the following because the native C++ SortingTable is based on the
////        // WIN32 LCID.  It doesn't work for neutral cultures liek 0x0009.  So we convert culture
////        // to a Win32 LCID here.
////
////        // Get Sort ID from culture.  This will affect the result of string comparison.
////        sortingLCID = cultureObj.CompareInfoId;
////
////        int sortID = CultureInfo.GetSortID( culture );
////        if(sortID != 0)
////        {
////            // Need to verify if the Sort ID is valid.
////            if(!cultureObj.m_cultureTableRecord.IsValidSortID( sortID ))
////            {
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_CultureNotSupported" ), culture ), "culture" );
////            }
////
////            // This is an alterinative sort LCID.  Hey man, don't forget to take your SORTID with you.
////            sortingLCID |= sortID << 16;
////        }
////
////        return sortingLCID;
////
////    }
////
////    // <SyntheticSupport/>
////    // These flags is used in the native Win32. so we need to map the managed options to those flags
////
////    private const int NORM_IGNORECASE     = 0x00000001; // Ignores case.
////    private const int NORM_IGNOREKANATYPE = 0x00010000; // Does not differentiate between Hiragana and Katakana characters. Corresponding Hiragana and Katakana will compare as equal.
////    private const int NORM_IGNORENONSPACE = 0x00000002; // Ignores nonspacing. This flag also removes Japanese accent characters.
////    private const int NORM_IGNORESYMBOLS  = 0x00000004; // Ignores symbols.
////    private const int NORM_IGNOREWIDTH    = 0x00020000; // Does not differentiate between a single-byte character and the same character as a double-byte character.
////    private const int SORT_STRINGSORT     = 0x00001000; // Treats punctuation the same as symbols.
////
////    internal static int GetNativeCompareFlags( CompareOptions options )
////    {
////        int nativeCompareFlags = 0;
////
////        if((options & CompareOptions.IgnoreCase    ) != 0) { nativeCompareFlags |= NORM_IGNORECASE    ; }
////        if((options & CompareOptions.IgnoreKanaType) != 0) { nativeCompareFlags |= NORM_IGNOREKANATYPE; }
////        if((options & CompareOptions.IgnoreNonSpace) != 0) { nativeCompareFlags |= NORM_IGNORENONSPACE; }
////        if((options & CompareOptions.IgnoreSymbols ) != 0) { nativeCompareFlags |= NORM_IGNORESYMBOLS ; }
////        if((options & CompareOptions.IgnoreWidth   ) != 0) { nativeCompareFlags |= NORM_IGNOREWIDTH   ; }
////        if((options & CompareOptions.StringSort    ) != 0) { nativeCompareFlags |= SORT_STRINGSORT    ; }
////
////        return nativeCompareFlags;
////    }


        ////////////////////////////////////////////////////////////////////////
        //
        //  Compare
        //
        //  Compares the two strings with the given options.  Returns 0 if the
        //  two strings are equal, a number less than 0 if string1 is less
        //  than string2, and a number greater than 0 if string1 is greater
        //  than string2.
        //
        ////////////////////////////////////////////////////////////////////////


        public virtual int Compare( String string1, String string2 )
        {
            return Compare( string1, string2, CompareOptions.None );
        }


        public unsafe virtual int Compare( String string1, String string2, CompareOptions options )
        {
            if(options == CompareOptions.OrdinalIgnoreCase)
            {
                return String.Compare( string1, string2, StringComparison.OrdinalIgnoreCase );
            }

            // Verify the options before we do any real comparison.
            if((options & CompareOptions.Ordinal) != 0)
            {
                if(options == CompareOptions.Ordinal)
                {
                    //Our paradigm is that null sorts less than any other string and
                    //that two nulls sort as equal.
                    if(string1 == null)
                    {
                        if(string2 == null)
                        {
                            return (0);     // Equal
                        }
                        return (-1);    // null < non-null
                    }

                    if(string2 == null)
                    {
                        return (1);     // non-null > null
                    }

                    return String.nativeCompareOrdinal( string1, string2, false );
                }
                else
                {
#if EXCEPTION_STRINGS
                    throw new ArgumentException( Environment.GetResourceString( "Argument_CompareOptionOrdinal" ), "options" );
#else
                    throw new ArgumentException();
#endif
                }
            }

            throw new NotImplementedException();
////        if((options & ValidCompareMaskOffFlags) != 0)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidFlag" ), "options" );
////        }
////
////        //Our paradigm is that null sorts less than any other string and
////        //that two nulls sort as equal.
////        if(string1 == null)
////        {
////            if(string2 == null)
////            {
////                return (0);     // Equal
////            }
////
////            return (-1);    // null < non-null
////        }
////
////        if(string2 == null)
////        {
////            return (1);     // non-null > null
////        }
////
////        // <SyntheticSupport/>
////        if(IsSynthetic)
////        {
////            if(options == CompareOptions.Ordinal)
////            {
////                return Compare( CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable, this.m_sortingLCID, string1, string2, options );
////            }
////
////            return nativeCompareString( m_sortingLCID, string1, 0, string1.Length, string2, 0, string2.Length, GetNativeCompareFlags( options ) );
////        }
////
////        return (Compare( m_pSortingTable, this.m_sortingLCID, string1, string2, options ));
        }

////    // This native method will check the parameters and validate them accordingly.
////    // COMNlsInfo::Compare
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    unsafe private static extern int Compare( void* pSortingTable, int sortingLCID, String string1, String string2, CompareOptions options );
////
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  Compare
////    //
////    //  Compares the specified regions of the two strings with the given
////    //  options.
////    //  Returns 0 if the two strings are equal, a number less than 0 if
////    //  string1 is less than string2, and a number greater than 0 if
////    //  string1 is greater than string2.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public unsafe virtual int Compare( String string1, int offset1, int length1, String string2, int offset2, int length2 )
////    {
////        return Compare( string1, offset1, length1,
////                        string2, offset2, length2, 0 );
////    }
////
////
////    public unsafe virtual int Compare( String string1, int offset1,
////                                       String string2, int offset2, CompareOptions options )
////    {
////        return Compare( string1, offset1, string1 == null ? 0 : string1.Length - offset1,
////                        string2, offset2, string2 == null ? 0 : string2.Length - offset2, options );
////    }
////
////
////    public unsafe virtual int Compare( String string1, int offset1,
////                                       String string2, int offset2 )
////    {
////        return Compare( string1, offset1, string2, offset2, 0 );
////    }
////
////
////    public unsafe virtual int Compare( String string1, int offset1, int length1,
////                                       String string2, int offset2, int length2, CompareOptions options )
////    {
////        if(options == CompareOptions.OrdinalIgnoreCase)
////        {
////            int result = String.Compare( string1, offset1,
////                                         string2, offset2, length1 < length2 ? length1 : length2, StringComparison.OrdinalIgnoreCase );
////
////            if((length1 != length2) && result == 0)
////            {
////                return (length1 > length2 ? 1 : -1);
////            }
////
////            return (result);
////        }
////
////        // Verify inputs
////        if(length1 < 0 || length2 < 0)
////        {
////            throw new ArgumentOutOfRangeException( (length1 < 0) ? "length1" : "length2", Environment.GetResourceString( "ArgumentOutOfRange_NeedPosNum" ) );
////        }
////
////        if(offset1 < 0 || offset2 < 0)
////        {
////            throw new ArgumentOutOfRangeException( (offset1 < 0) ? "offset1" : "offset2", Environment.GetResourceString( "ArgumentOutOfRange_NeedPosNum" ) );
////        }
////
////        if(offset1 > (string1 == null ? 0 : string1.Length) - length1)
////        {
////            throw new ArgumentOutOfRangeException( "string1", Environment.GetResourceString( "ArgumentOutOfRange_OffsetLength" ) );
////        }
////
////        if(offset2 > (string2 == null ? 0 : string2.Length) - length2)
////        {
////            throw new ArgumentOutOfRangeException( "string2", Environment.GetResourceString( "ArgumentOutOfRange_OffsetLength" ) );
////        }
////
////        if((options & CompareOptions.Ordinal) != 0)
////        {
////            if(options == CompareOptions.Ordinal)
////            {
////                //
////                // Check for the null case.
////                //
////                if(string1 == null)
////                {
////                    if(string2 == null)
////                    {
////                        return (0);
////                    }
////                    return (-1);
////                }
////                if(string2 == null)
////                {
////                    return (1);
////                }
////
////                int result = String.nativeCompareOrdinalEx( string1, offset1, string2, offset2, (length1 < length2 ? length1 : length2) );
////                if((length1 != length2) && result == 0)
////                {
////                    return (length1 > length2 ? 1 : -1);
////                }
////                return (result);
////            }
////            else
////            {
////                throw new ArgumentException( Environment.GetResourceString( "Argument_CompareOptionOrdinal" ), "options" );
////            }
////        }
////
////        if((options & ValidCompareMaskOffFlags) != 0)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidFlag" ), "options" );
////        }
////
////        //
////        // Check for the null case.
////        //
////        if(string1 == null)
////        {
////            if(string2 == null)
////            {
////                return (0);
////            }
////            return (-1);
////        }
////
////        if(string2 == null)
////        {
////            return (1);
////        }
////
////        // <SyntheticSupport/>
////        if(IsSynthetic)
////        {
////            if(options == CompareOptions.Ordinal)
////            {
////                return CompareRegion(
////                            CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable,
////                            this.m_sortingLCID,
////                            string1,
////                            offset1,
////                            length1,
////                            string2,
////                            offset2,
////                            length2,
////                            options );
////            }
////
////            return nativeCompareString( m_sortingLCID, string1, offset1, length1, string2, offset2, length2, GetNativeCompareFlags( options ) );
////        }
////
////        // Call native code to do comparison
////        return (CompareRegion( m_pSortingTable, this.m_sortingLCID, string1, offset1, length1, string2, offset2, length2, options ));
////    }
////
////    // This native method will check the parameters and validate them accordingly.
////    // Native method: COMNlsInfo::CompareRegion
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    unsafe private static extern int CompareRegion( void* pSortingTable, int sortingLCID, String string1, int offset1, int length1, String string2, int offset2, int length2, CompareOptions options );
////
////    [NonSerialized]
////    static int fFindNLSStringSupported;   // Defaults to 0 -- means valid, or not yet checked
////
////    private static unsafe int FindNLSStringWrap( int lcid, int flags, string src, int start, int cchSrc, string value, int cchValue )
////    {
////        int retval = -1;
////
////        fixed(char* lpStringSource = src)
////        {
////            fixed(char* lpStringValue = value)
////            {
////                if(1 == fFindNLSStringSupported)
////                {
////                    // Already verified the API is present, so just use it.
////                    retval = Win32Native.FindNLSString( lcid, flags, &lpStringSource[start], cchSrc, lpStringValue, cchValue, IntPtr.Zero );
////                }
////                else
////                {
////                    try
////                    {
////                        // Call the API once with a handler (in case it is not there)
////                        retval = Win32Native.FindNLSString( lcid, flags, &lpStringSource[start], cchSrc, lpStringValue, cchValue, IntPtr.Zero );
////                        fFindNLSStringSupported = 1;
////                    }
////                    catch(System.EntryPointNotFoundException)
////                    {
////                        // Unable to find the method, so we will fall back to the old behavior. Return -2
////                        // so the caller knows this happeneed.
////                        retval = fFindNLSStringSupported = -2;
////                    }
////                }
////            }
////        }
////
////        return (retval);
////    }
////
////    private bool SyntheticIsPrefix( string source, int start, int length, string prefix, int nativeCompareFlags )
////    {
////        BCLDebug.Assert( start + length <= source.Length, "[CompareInfo.SyntheticIsPrefix] invalid parameters." );
////
////        if(fFindNLSStringSupported >= 0)
////        {
////            int retval = FindNLSStringWrap( m_sortingLCID, nativeCompareFlags | Win32Native.FIND_STARTSWITH, source, start, length, prefix, prefix.Length );
////            if(retval >= -1)
////            {
////                return (retval != -1);
////            }
////        }
////
////        for(int i = 1; i <= length; i++)
////        {
////            if(nativeCompareString( m_sortingLCID, prefix, 0, prefix.Length, source, start, i, nativeCompareFlags ) == 0) { return true; }
////        }
////
////        return false;
////    }
////
////    // Returns the index of the substring if found one. otherwise it returns -1
////    private int SyntheticIsSuffix( string source, int end, int length, string suffix, int nativeCompareFlags )
////    {
////        BCLDebug.Assert( end < source.Length && (end + 1 >= length), "[CompareInfo.SyntheticIsSuffix] invalid parameters." );
////
////        if(fFindNLSStringSupported >= 0)
////        {
////            int retval = FindNLSStringWrap( m_sortingLCID, nativeCompareFlags | Win32Native.FIND_ENDSWITH, source, 0, length, suffix, suffix.Length );
////            if(retval >= -1)
////            {
////                return (retval);
////            }
////        }
////
////        for(int i = 0; i < length; i++)
////        {
////            if(nativeCompareString( m_sortingLCID, suffix, 0, suffix.Length, source, end - i, i + 1, nativeCompareFlags ) == 0) { return end - i; }
////        }
////
////        return -1;
////    }
////
////    private int SyntheticIndexOf( string source, string value, int start, int length, int nativeCompareFlags )
////    {
////        BCLDebug.Assert( start + length <= source.Length, "[CompareInfo.SyntheticIndexOf] invalid parameters." );
////
////        if(fFindNLSStringSupported >= 0)
////        {
////            int retval = FindNLSStringWrap( m_sortingLCID, nativeCompareFlags | Win32Native.FIND_FROMSTART, source, start, length, value, value.Length );
////            if(retval >= -1)
////            {
////                return (retval);
////            }
////        }
////
////        for(int i = 0; i < length; i++)
////        {
////            if(SyntheticIsPrefix( source, start + i, length - i, value, nativeCompareFlags )) { return start + i; }
////        }
////
////        return -1;
////    }
////
////    private int SyntheticLastIndexOf( string source, string value, int end, int length, int nativeCompareFlags )
////    {
////        BCLDebug.Assert( end + 1 >= length, "[CompareInfo.SyntheticLastIndexOf] invalid parameters." );
////
////        if(fFindNLSStringSupported >= 0)
////        {
////            int retval = FindNLSStringWrap( m_sortingLCID, nativeCompareFlags | Win32Native.FIND_FROMEND, source, end - length + 1, length, value, value.Length );
////            if(retval >= -1)
////            {
////                return (retval);
////            }
////        }
////
////        for(int i = 0; i < length; i++)
////        {
////            int index = SyntheticIsSuffix( source, end - i, length - i, value, nativeCompareFlags );
////            if(index >= 0) { return index; }
////        }
////
////        return -1;
////    }
    
    
        ////////////////////////////////////////////////////////////////////////
        //
        //  IsPrefix
        //
        //  Determines whether prefix is a prefix of string.  If prefix equals
        //  String.Empty, true is returned.
        //
        ////////////////////////////////////////////////////////////////////////
    
    
        public unsafe virtual bool IsPrefix( String source, String prefix, CompareOptions options )
        {
            if(source == null || prefix == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( (source == null ? "source" : "prefix"), Environment.GetResourceString( "ArgumentNull_String" ) );
#else
                throw new ArgumentNullException();
#endif
            }
    
            int prefixLen = prefix.Length;
    
            if(prefixLen == 0)
            {
                return (true);
            }
    
            if(options == CompareOptions.OrdinalIgnoreCase)
            {
                return source.StartsWith( prefix, StringComparison.OrdinalIgnoreCase );
            }
    
            if((options & ValidIndexMaskOffFlags) != 0 && (options != CompareOptions.Ordinal))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidFlag" ), "options" );
#else
                throw new ArgumentException();
#endif
            }
    
////        // <SyntheticSupport/>
////        if(IsSynthetic)
////        {
////            if(options == CompareOptions.Ordinal)
////            {
////                return nativeIsPrefix( CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable, this.m_sortingLCID, source, prefix, options );
////            }
////
////            return SyntheticIsPrefix( source, 0, source.Length, prefix, GetNativeCompareFlags( options ) );
////        }
////
////        return (nativeIsPrefix( m_pSortingTable, this.m_sortingLCID, source, prefix, options ));
            throw new NotSupportedException();
        }
    
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        unsafe private static extern bool nativeIsPrefix( void* pSortingTable, int sortingLCID, String source, String prefix, CompareOptions options );
    
    
        public virtual bool IsPrefix( String source, String prefix )
        {
            return (IsPrefix( source, prefix, 0 ));
        }
    
    
        ////////////////////////////////////////////////////////////////////////
        //
        //  IsSuffix
        //
        //  Determines whether suffix is a suffix of string.  If suffix equals
        //  String.Empty, true is returned.
        //
        ////////////////////////////////////////////////////////////////////////
    
    
        public unsafe virtual bool IsSuffix( String source, String suffix, CompareOptions options )
        {
            if(source == null || suffix == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( (source == null ? "source" : "suffix"), Environment.GetResourceString( "ArgumentNull_String" ) );
#else
                throw new ArgumentNullException();
#endif
            }
    
            int suffixLen = suffix.Length;
    
            if(suffixLen == 0)
            {
                return (true);
            }
    
            if(options == CompareOptions.OrdinalIgnoreCase)
            {
                return source.EndsWith( suffix, StringComparison.OrdinalIgnoreCase );
            }
    
            if((options & ValidIndexMaskOffFlags) != 0 && (options != CompareOptions.Ordinal))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidFlag" ), "options" );
#else
                throw new ArgumentException();
#endif
            }
    
////        // <SyntheticSupport/>
////        if(IsSynthetic)
////        {
////            if(options == CompareOptions.Ordinal)
////            {
////                return nativeIsSuffix( CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable, this.m_sortingLCID, source, suffix, options );
////            }
////            return SyntheticIsSuffix( source, source.Length - 1, source.Length, suffix, GetNativeCompareFlags( options ) ) >= 0;
////        }
////
////        return (nativeIsSuffix( m_pSortingTable, this.m_sortingLCID, source, suffix, options ));
            throw new NotSupportedException();
        }
    
    
        public virtual bool IsSuffix( String source, String suffix )
        {
            return (IsSuffix( source, suffix, 0 ));
        }
    
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    private static unsafe extern bool nativeIsSuffix( void* pSortingTable, int sortingLCID, String source, String prefix, CompareOptions options );
    
        ////////////////////////////////////////////////////////////////////////
        //
        //  IndexOf
        //
        //  Returns the first index where value is found in string.  The
        //  search starts from startIndex and ends at endIndex.  Returns -1 if
        //  the specified value is not found.  If value equals String.Empty,
        //  startIndex is returned.  Throws IndexOutOfRange if startIndex or
        //  endIndex is less than zero or greater than the length of string.
        //  Throws ArgumentException if value is null.
        //
        ////////////////////////////////////////////////////////////////////////
    
    
        public unsafe virtual int IndexOf( String source, char value )
        {
            if(source == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "source" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            return IndexOf( source, value, 0, source.Length, CompareOptions.None );
        }
    
        public unsafe virtual int IndexOf( String source, String value )
        {
            if(source == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "source" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            return IndexOf( source, value, 0, source.Length, CompareOptions.None );
        }
    
        public unsafe virtual int IndexOf( String source, char value, CompareOptions options )
        {
            if(source == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "source" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            return IndexOf( source, value, 0, source.Length, options );
        }
    
        public unsafe virtual int IndexOf( String source, String value, CompareOptions options )
        {
            if(source == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "source" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            return IndexOf( source, value, 0, source.Length, options );
        }
    
        public unsafe virtual int IndexOf( String source, char value, int startIndex )
        {
            if(source == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "source" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            return IndexOf( source, value, startIndex, source.Length - startIndex, CompareOptions.None );
        }
    
        public unsafe virtual int IndexOf( String source, String value, int startIndex )
        {
            if(source == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "source" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            return IndexOf( source, value, startIndex, source.Length - startIndex, CompareOptions.None );
        }
    
        public unsafe virtual int IndexOf( String source, char value, int startIndex, CompareOptions options )
        {
            if(source == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "source" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            return IndexOf( source, value, startIndex, source.Length - startIndex, options );
        }
    
        public unsafe virtual int IndexOf( String source, String value, int startIndex, CompareOptions options )
        {
            if(source == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "source" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            return IndexOf( source, value, startIndex, source.Length - startIndex, options );
        }
    
        public unsafe virtual int IndexOf( String source, char value, int startIndex, int count )
        {
            return IndexOf( source, value, startIndex, count, CompareOptions.None );
        }
    
        public unsafe virtual int IndexOf( String source, String value, int startIndex, int count )
        {
            return IndexOf( source, value, startIndex, count, CompareOptions.None );
        }
    
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
        public unsafe virtual int IndexOf( String source, char value, int startIndex, int count, CompareOptions options )
        {
            // Validate inputs
            if(source == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "source" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(startIndex < 0 || startIndex > source.Length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            if(count < 0 || startIndex > source.Length - count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
////        if(options == CompareOptions.OrdinalIgnoreCase)
////        {
////            return TextInfo.nativeIndexOfCharOrdinalIgnoreCase(
////                        TextInfo.InvariantNativeTextInfo,
////                        source,
////                        value,
////                        startIndex,
////                        count );
////        }
////
////        // Validate CompareOptions
////        // Ordinal can't be selected with other flags
////        if((options & ValidIndexMaskOffFlags) != 0 && (options != CompareOptions.Ordinal))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidFlag" ), "options" );
////        }
////
////        // <SyntheticSupport/>
////        if(IsSynthetic)
////        {
////            if(options == CompareOptions.Ordinal)
////            {
////                return IndexOfChar(
////                        CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable,
////                        this.m_sortingLCID,
////                        source,
////                        value,
////                        startIndex,
////                        count,
////                        (int)options );
////            }
////            return SyntheticIndexOf( source, new String( value, 1 ), startIndex, count, GetNativeCompareFlags( options ) );
////        }
////
////        return IndexOfChar( m_pSortingTable, this.m_sortingLCID, source, value, startIndex, count, (int)options );
            throw new NotSupportedException();
        }
    
    
        public unsafe virtual int IndexOf( String source, String value, int startIndex, int count, CompareOptions options )
        {
            // Validate inputs
            if(source == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "source" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(value == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "value" );
#else
                throw new ArgumentNullException();
#endif
            }
    
            if(startIndex > source.Length)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            // In Everett we used to return -1 for empty string even if startIndex is negative number so we keeping same behavior here.
            // We return 0 if both source and value are empty strings for Everett compatibility too.
            if(source.Length == 0)
            {
                if(value.Length == 0)
                {
                    return 0;
                }
                return -1;
            }
    
            if(startIndex < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
            if(count < 0 || startIndex > source.Length - count)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
    
////        if(options == CompareOptions.OrdinalIgnoreCase)
////        {
////            return TextInfo.IndexOfStringOrdinalIgnoreCase( source, value, startIndex, count );
////        }
////
////        // Validate CompareOptions
////        // Ordinal can't be selected with other flags
////        if((options & ValidIndexMaskOffFlags) != 0 && (options != CompareOptions.Ordinal))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidFlag" ), "options" );
////        }
////
////        // <SyntheticSupport/>
////        if(IsSynthetic)
////        {
////            if(options == CompareOptions.Ordinal)
////            {
////                return IndexOfString(
////                        CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable,
////                        this.m_sortingLCID,
////                        source,
////                        value,
////                        startIndex,
////                        count,
////                        (int)options );
////            }
////
////            return SyntheticIndexOf( source, value, startIndex, count, GetNativeCompareFlags( options ) );
////        }
////
////        return IndexOfString( m_pSortingTable, this.m_sortingLCID, source, value, startIndex, count, (int)options );
            throw new NotSupportedException();
        }
    
        // This native method will check the parameters and validate them accordingly.
        // Native method: COMNlsInfo::IndexOfChar
////    [ResourceExposure( ResourceScope.None )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        unsafe private static extern int IndexOfChar( void* pSortingTable, int sortingLCID, String source, char value, int startIndex, int count, int options );
    
////    // This native method will check the parameters and validate them accordingly.
////    // Native method: COMNlsInfo::IndexOfString
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    unsafe private static extern int IndexOfString( void* pSortingTable, int sortingLCID, String source, String value, int startIndex, int count, int options );
////
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  LastIndexOf
////    //
////    //  Returns the last index where value is found in string.  The
////    //  search starts from startIndex and ends at endIndex.  Returns -1 if
////    //  the specified value is not found.  If value equals String.Empty,
////    //  endIndex is returned.  Throws IndexOutOfRange if startIndex or
////    //  endIndex is less than zero or greater than the length of string.
////    //  Throws ArgumentException if value is null.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public unsafe virtual int LastIndexOf( String source, char value )
////    {
////        if(source == null)
////        {
////            throw new ArgumentNullException( "source" );
////        }
////
////        // Can't start at negative index, so make sure we check for the length == 0 case.
////        return LastIndexOf( source, value, source.Length - 1, source.Length, CompareOptions.None );
////    }
////
////
////    public unsafe virtual int LastIndexOf( String source, String value )
////    {
////        if(source == null)
////        {
////            throw new ArgumentNullException( "source" );
////        }
////
////        // Can't start at negative index, so make sure we check for the length == 0 case.
////        return LastIndexOf( source, value, source.Length - 1, source.Length, CompareOptions.None );
////    }
////
////
////    public unsafe virtual int LastIndexOf( String source, char value, CompareOptions options )
////    {
////        if(source == null)
////        {
////            throw new ArgumentNullException( "source" );
////        }
////
////        // Can't start at negative index, so make sure we check for the length == 0 case.
////        return LastIndexOf( source, value, source.Length - 1, source.Length, options );
////    }
////
////    public unsafe virtual int LastIndexOf( String source, String value, CompareOptions options )
////    {
////        if(source == null)
////        {
////            throw new ArgumentNullException( "source" );
////        }
////
////        // Can't start at negative index, so make sure we check for the length == 0 case.
////        return LastIndexOf( source, value, source.Length - 1, source.Length, options );
////    }
////
////
////    public unsafe virtual int LastIndexOf( String source, char value, int startIndex )
////    {
////        return LastIndexOf( source, value, startIndex, startIndex + 1, CompareOptions.None );
////    }
////
////
////    public unsafe virtual int LastIndexOf( String source, String value, int startIndex )
////    {
////        return LastIndexOf( source, value, startIndex, startIndex + 1, CompareOptions.None );
////    }
////
////
////    public unsafe virtual int LastIndexOf( String source, char value, int startIndex, CompareOptions options )
////    {
////        return LastIndexOf( source, value, startIndex, startIndex + 1, options );
////    }
////
////
////    public unsafe virtual int LastIndexOf( String source, String value, int startIndex, CompareOptions options )
////    {
////        return LastIndexOf( source, value, startIndex, startIndex + 1, options );
////    }
////
////
////    public unsafe virtual int LastIndexOf( String source, char value, int startIndex, int count )
////    {
////        return LastIndexOf( source, value, startIndex, count, CompareOptions.None );
////    }
////
////
////    public unsafe virtual int LastIndexOf( String source, String value, int startIndex, int count )
////    {
////        return LastIndexOf( source, value, startIndex, count, CompareOptions.None );
////    }
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////    public unsafe virtual int LastIndexOf( String source, char value, int startIndex, int count, CompareOptions options )
////    {
////        // Verify Arguments
////        if(source == null)
////        {
////            throw new ArgumentNullException( "source" );
////        }
////
////        // Validate CompareOptions
////        // Ordinal can't be selected with other flags
////        if((options & ValidIndexMaskOffFlags) != 0 &&
////            (options != CompareOptions.Ordinal) &&
////            (options != CompareOptions.OrdinalIgnoreCase))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidFlag" ), "options" );
////        }
////
////        // Special case for 0 length input strings
////        if(source.Length == 0 && (startIndex == -1 || startIndex == 0))
////        {
////            return -1;
////        }
////
////        // Make sure we're not out of range
////        if(startIndex < 0 || startIndex > source.Length)
////        {
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////        }
////
////        // Make sure that we allow startIndex == source.Length
////        if(startIndex == source.Length)
////        {
////            startIndex--;
////            if(count > 0)
////            {
////                count--;
////            }
////        }
////
////        // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
////        if(count < 0 || startIndex - count + 1 < 0)
////        {
////            throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
////        }
////
////        if(options == CompareOptions.OrdinalIgnoreCase)
////        {
////            return TextInfo.nativeLastIndexOfCharOrdinalIgnoreCase(
////                        TextInfo.InvariantNativeTextInfo,
////                        source,
////                        value,
////                        startIndex,
////                        count );
////        }
////
////        // <SyntheticSupport/>
////        if(IsSynthetic)
////        {
////            if(options == CompareOptions.Ordinal)
////            {
////                return LastIndexOfChar(
////                        CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable,
////                        this.m_sortingLCID,
////                        source,
////                        value,
////                        startIndex,
////                        count,
////                        (int)options );
////            }
////
////            return SyntheticLastIndexOf( source, new String( value, 1 ), startIndex, count, GetNativeCompareFlags( options ) );
////        }
////
////        return (LastIndexOfChar( m_pSortingTable, this.m_sortingLCID, source, value, startIndex, count, (int)options ));
////    }
////
////
////    public unsafe virtual int LastIndexOf( String source, String value, int startIndex, int count, CompareOptions options )
////    {
////        // Verify Arguments
////        if(source == null)
////        {
////            throw new ArgumentNullException( "source" );
////        }
////
////        if(value == null)
////        {
////            throw new ArgumentNullException( "value" );
////        }
////
////        // Validate CompareOptions
////        // Ordinal can't be selected with other flags
////        if((options & ValidIndexMaskOffFlags) != 0 &&
////            (options != CompareOptions.Ordinal) &&
////            (options != CompareOptions.OrdinalIgnoreCase))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidFlag" ), "options" );
////        }
////
////        // Special case for 0 length input strings
////        if(source.Length == 0 && (startIndex == -1 || startIndex == 0))
////        {
////            return (value.Length == 0) ? 0 : -1;
////        }
////
////        // Make sure we're not out of range
////        if(startIndex < 0 || startIndex > source.Length)
////        {
////            throw new ArgumentOutOfRangeException( "startIndex", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
////        }
////
////        // Make sure that we allow startIndex == source.Length
////        if(startIndex == source.Length)
////        {
////            startIndex--;
////            if(count > 0)
////            {
////                count--;
////            }
////
////            // If we are looking for nothing, just return 0
////            if(value.Length == 0 && count >= 0 && startIndex - count + 1 >= 0)
////            {
////                return startIndex;
////            }
////        }
////
////        // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
////        if(count < 0 || startIndex - count + 1 < 0)
////        {
////            throw new ArgumentOutOfRangeException( "count", Environment.GetResourceString( "ArgumentOutOfRange_Count" ) );
////        }
////
////        if(options == CompareOptions.OrdinalIgnoreCase)
////        {
////            return TextInfo.LastIndexOfStringOrdinalIgnoreCase( source, value, startIndex, count );
////        }
////
////        // <SyntheticSupport/>
////        if(IsSynthetic)
////        {
////            if(options == CompareOptions.Ordinal)
////            {
////                return LastIndexOfString(
////                        CultureInfo.InvariantCulture.CompareInfo.m_pSortingTable,
////                        this.m_sortingLCID,
////                        source,
////                        value,
////                        startIndex,
////                        count,
////                        (int)options );
////            }
////
////            return SyntheticLastIndexOf( source, value, startIndex, count, GetNativeCompareFlags( options ) );
////        }
////
////        return (LastIndexOfString( m_pSortingTable, this.m_sortingLCID, source, value, startIndex, count, (int)options ));
////    }
////
////    // This native method will check the parameters and validate them accordingly.
////    // Native method: COMNlsInfo::LastIndexOfChar
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    unsafe internal static extern int LastIndexOfChar( void* pSortingTable, int sortingLCID, String source, char value, int startIndex, int count, int options );
////
////    // This native method will check the parameters and validate them accordingly.
////    // Native method: COMNlsInfo::LastIndexOfString
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    unsafe internal static extern int LastIndexOfString( void* pSortingTable, int sortingLCID, String source, String value, int startIndex, int count, int options );
////
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  GetSortKey
////    //
////    //  Gets the SortKey for the given string with the given options.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public unsafe virtual SortKey GetSortKey( String source, CompareOptions options )
////    {
////        // <SyntheticSupport/>
////        if(IsSynthetic)
////        {
////            return new SortKey( this.m_sortingLCID, source, options );
////        }
////
////        // SortKey() will check the parameters and validate them accordingly.
////
////        return new SortKey( m_pSortingTable, this.m_sortingLCID, source, options );
////    }
////
////
////    public unsafe virtual SortKey GetSortKey( String source )
////    {
////        // <SyntheticSupport/>
////        if(IsSynthetic)
////        {
////            return new SortKey( this.m_sortingLCID, source, CompareOptions.None );
////        }
////
////        return new SortKey( m_pSortingTable, this.m_sortingLCID, source, CompareOptions.None );
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  Equals
////    //
////    //  Implements Object.Equals().  Returns a boolean indicating whether
////    //  or not object refers to the same CompareInfo as the current
////    //  instance.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////    public override bool Equals( Object value )
////    {
////        CompareInfo that = value as CompareInfo;
////        if(that != null)
////        {
////            return this.m_sortingLCID == that.m_sortingLCID && this.Name.Equals( that.Name );
////        }
////
////        return false;
////    }
////
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  GetHashCode
////    //
////    //  Implements Object.GetHashCode().  Returns the hash code for the
////    //  CompareInfo.  The hash code is guaranteed to be the same for
////    //  CompareInfo A and B where A.Equals(B) is true.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public override int GetHashCode()
////    {
////        return this.Name.GetHashCode();
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  GetHashCodeOfString
////    //
////    //  This internal method allows a method that allows the equivalent of creating a Sortkey for a
////    //  string from CompareInfo, and generate a hashcode value from it.  It is not very convenient
////    //  to use this method as is and it creates an unnecessary Sortkey object that will be GC'ed.
////    //
////    //  The hash code is guaranteed to be the same for string A and B where A.Equals(B) is true and both
////    //  the CompareInfo and the CompareOptions are the same. If two different CompareInfo objects
////    //  treat the string the same way, this implementation will treat them differently (the same way that
////    //  Sortkey does at the moment).
////    //
////    //  This method will never be made public itself, but public consumers of it could be created, e.g.:
////    //
////    //      string.GetHashCode(CultureInfo)
////    //      string.GetHashCode(CompareInfo)
////    //      string.GetHashCode(CultureInfo, CompareOptions)
////    //      string.GetHashCode(CompareInfo, CompareOptions)
////    //      etc.
////    //
////    //  (the methods above that take a CultureInfo would use CultureInfo.CompareInfo)
////    //
////    ////////////////////////////////////////////////////////////////////////
////    unsafe internal int GetHashCodeOfString( string source, CompareOptions options )
////    {
////        //
////        //  Parameter validation
////        //
////        if(source == null)
////        {
////            throw new ArgumentNullException( "source" );
////        }
////
////        if((options & ValidHashCodeOfStringMaskOffFlags) != 0)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidFlag" ), "options" );
////        }
////
////        if(source.Length == 0)
////        {
////            return (0);
////        }
////
////        //
////        ////////////////////////////////////////////////////////////////////////
////
////        //
////        // CONSIDER: An update to nativeGetGlobalizedHashCode that called the NLS LCMapString and then
////        //           subsequently used the same hashcode logic would be a much better globalized hashcode.
////        //
////
////        // <SyntheticSupport/>
////        if(IsSynthetic)
////        {
////            return CultureInfo.InvariantCulture.CompareInfo.GetHashCodeOfString( source, options );
////        }
////
////        return nativeGetGlobalizedHashCode( m_pSortingTable, source, (int)options, this.m_sortingLCID );
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  ToString
////    //
////    //  Implements Object.ToString().  Returns a string describing the
////    //  CompareInfo.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////    public override String ToString()
////    {
////        return "CompareInfo - " + this.culture;
////    }
////
////
////    public int LCID
////    {
////        get
////        {
////            return this.culture;
////        }
////    }
////
////    // This is a thin wrapper for InitializeNativeCompareInfo() to provide the necessary
////    // synchronization.
////
////    /*=================================InitializeCompareInfo==========================
////    **Action: Makes a native C++ SortingTable pointer using the specified assembly and Win32 LCID.
////    **Returns: a 32-bit pointer value to native C++ SrotingTable instance.
////    **
////    ** This method shouldn't be called when having synthetic culture (ELK)
////    ** Couldn't assert her because this is a static method and cannot detect the synthetic cultures case.
////    **
////    **Arguments:
////    **      pNativeGlobalizationAssembly    the 32-bit pointer value to a native C++ NLSAssembly instance.
////    **      sortingLCID       the Win32 LCID.
////    **Exceptions:
////    **      None.
////    ============================================================================*/
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    unsafe private static void* InitializeCompareInfo( void* pNativeGlobalizationAssembly, int sortingLCID )
////    {
////        void* pTemp    = null;
////        bool  tookLock = false;
////
////        RuntimeHelpers.PrepareConstrainedRegions();
////        try
////        {
////            // This is intentionally taking a process-global lock, using a CER
////            // to guarantee we release the lock.
////            // Note that we should lock on a internal mscorlib type, so that
////            // no public caller can block us by locking on the same type.
////            Monitor.ReliableEnter( typeof( CultureTableRecord ), ref tookLock );
////
////            // we should lock on the call of InitializeNativeCompareInfo because it uses native static
////            // variables that are not safe to be accessed in same time from different threads.
////            // Since we're just calling an FCALL method here that will
////            // probe for stack space, we don't need to use
////            // ExecuteCodeWithGuaranteedCleanup here.
////            pTemp = InitializeNativeCompareInfo( pNativeGlobalizationAssembly, sortingLCID );
////        }
////        finally
////        {
////            if(tookLock)
////            {
////                Monitor.Exit( typeof( CultureTableRecord ) );
////            }
////        }
////
////        BCLDebug.Assert( pTemp != null, "pTemp != null" );
////        return pTemp;
////    }
////
////    unsafe void IDeserializationCallback.OnDeserialization( Object sender )
////    {
////        OnDeserialized();
////    }
////
////    // This method requires synchonization because class global data member is used
////    // in the native side.
////    // Native method: COMNlsInfo::InitializeNativeCompareInfo
////    [ResourceExposure( ResourceScope.Process )]
////    [MethodImpl( MethodImplOptions.InternalCall )]
////    unsafe private static extern void* InitializeNativeCompareInfo( void* pNativeGlobalizationAssembly, int sortingLCID );
    }
}
