// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
////////////////////////////////////////////////////////////////////////////
//
//  Class:    TextInfo
//
//  Purpose:  This Class defines behaviors specific to a writing system.
//            A writing system is the collection of scripts and
//            orthographic rules required to represent a language as text.
//
//  Date:     March 31, 1999
//
////////////////////////////////////////////////////////////////////////////

namespace System.Globalization
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
////using System.Security.Permissions;
////using System.Runtime.Versioning;


    [Serializable]
    public class TextInfo /*: ICloneable, IDeserializationCallback*/
    {
////    //--------------------------------------------------------------------//
////    //                        Internal Information                        //
////    //--------------------------------------------------------------------//
////
////
////    //
////    //  Variables.
////    //
////
////    [OptionalField( VersionAdded = 2 )]
////    private String m_listSeparator;
////
////    [OptionalField( VersionAdded = 2 )]
////    private bool m_isReadOnly = false;
////
////    //
////    // Basically, this is the language ID (LANGID) used to call Win32 NLS APIs except that
////    // the value can be zero for the invariant culture.
////    // The reason for this data member to exist is that Win32 APIs
////    // doesn't take all of the culture IDs supported in NLS+.
////    // For example, NLS+ support culture IDs like 0x0000, 0x0009.
////    // However, these are not valid locale IDs in Win32.  Therefore,
////    // we use a table to map a culutre ID like
////    // 0x0009 to 0x0409.
////    //
////
////    // m_textInfoID should be either 0 or a supported language ID.  See TextInfo(m_textInfoID)
////    // for comments.
////    [NonSerialized]
////    private int m_textInfoID;
////    [NonSerialized]
////    private string m_name = null;
////    [NonSerialized]
////    private CultureTableRecord m_cultureTableRecord;
////    [NonSerialized]
////    private TextInfo m_casingTextInfo;
////
////    //
////    // m_pNativeTextInfo is a 32-bit pointer value pointing to a native C++ NativeTextInfo object.
////    // The C++ NativeTextInfo is providing the implemenation of uppercasing/lowercasing.
////    // 
////    // Note: m_pNativeTextInfo is intialized with invariant in case of synthetic cultuers
////    [NonSerialized]
////    private unsafe void* m_pNativeTextInfo;
////    private static unsafe void* m_pInvariantNativeTextInfo;
////    private static unsafe void* m_pDefaultCasingTable;
////
////    // This file contains the default casing data (uppercasing/lowercasing/titlecasing), and the table to
////    // map cultures with exceptions to an exception sub-table in CASING_EXCEPTIONS_FILE_NAME.
////    private const String CASING_FILE_NAME = "l_intl.nlp";
////    // This file contains the casing data for cultures with exceptions.
////    private const String CASING_EXCEPTIONS_FILE_NAME = "l_except.nlp";
////
////    //
////    // This is the header for the native data table that we load from charinfo.nlp.
////    //
////    [StructLayout( LayoutKind.Explicit )]
////    internal unsafe struct TextInfoDataHeader
////    {
////        [FieldOffset( 0 )]
////        internal char TableName;    // WCHAR[16]
////        [FieldOffset( 0x20 )]
////        internal ushort version;    // WORD[4]
////        [FieldOffset( 0x28 )]
////        internal uint OffsetToUpperCasingTable; // DWORD
////        [FieldOffset( 0x2c )]
////        internal uint OffsetToLowerCasingTable; // DWORD
////        [FieldOffset( 0x30 )]
////        internal uint OffsetToTitleCaseTable; // DWORD
////        [FieldOffset( 0x34 )]
////        internal uint PlaneOffset;
////        // Each plane has DWORD offset for uppercase  and DWORD offset for lowercase.
////        // 0xb4 = 0x34 + 8*16
////        [FieldOffset( 0xb4 )]
////        internal ushort exceptionCount;
////        [FieldOffset( 0xb6 )]
////        internal ushort exceptionLangId;
////    }
////
////    [StructLayout( LayoutKind.Sequential, Pack = 2 )]
////    internal struct ExceptionTableItem
////    {
////        internal ushort langID;   // The lcid that contains the exceptions.
////        internal ushort exceptIndex;   // The name of the exception tables.
////    }
////
////
////    // The base pointer of the defult casing table
////    static unsafe byte* m_pDataTable;
////    // The total count of cultures with exceptions.
////    static int m_exceptionCount;
////    // The pointer to the exception index table.  This table maps a culture with exceptions
////    // to a sub-table in the exception data table.
////    static unsafe ExceptionTableItem* m_exceptionTable;
////
////    // The base pointer for exception data file.
////    static unsafe byte* m_pExceptionFile;
////    // This array caches the native pointer of the NativeTextInfo get by calling InternalAllocateCasingTable.
////
////    // NOTE: use long to hold native pointers.
////    //static unsafe void*[] m_exceptionNativeTextInfo;
////    static unsafe long[] m_exceptionNativeTextInfo;
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  Actions:
////    //      This is the static ctor for TextInfo.  It does the following items:
////    //      * Get the total count of cultures with exceptions.
////    //      * Set up an exception index table so that we can check if a culture has exception.  If yes, which sub-table
////    //        in the exception table file we should use for this culture.
////    //      * Set up a cache for NativeTextInfo that we create for cultures with exceptions.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////    static unsafe TextInfo()
////    {
////        //with AppDomains active, the static initializer is no longer good enough to ensure that only one
////        //thread is ever in AllocateDefaultCasingTable at a given time.
////        //We use InterlockedExchangePointer in the native side to ensure that only one instance of native CasingTable instance
////        //is created per process.
////
////        //We check if the table is already allocated in native, so we only need to synchronize
////        //access in managed.
////        byte* temp = GlobalizationAssembly.GetGlobalizationResourceBytePtr( typeof( TextInfo ).Assembly, CASING_FILE_NAME );
////        System.Threading.Thread.MemoryBarrier();
////        m_pDataTable = temp;
////
////        TextInfoDataHeader* pHeader = (TextInfoDataHeader*)m_pDataTable;
////        m_exceptionCount = pHeader->exceptionCount;
////        // Setup exception tables
////        m_exceptionTable = (ExceptionTableItem*)&(pHeader->exceptionLangId);
////        m_exceptionNativeTextInfo = new long[m_exceptionCount];
////
////        // Create the native NativeTextInfo for the default linguistic casing table.
////        m_pDefaultCasingTable = AllocateDefaultCasingTable( m_pDataTable );
////
////        BCLDebug.Assert( m_pDataTable != null, "Error in reading the table." );
////        BCLDebug.Assert( m_pDefaultCasingTable != null, "m_pDefaultCasingTable != null" );
////    }
////
////    // Private object for locking instead of locking on a public type for SQL reliability work.
////    private static Object s_InternalSyncObject;
////    private static Object InternalSyncObject
////    {
////        get
////        {
////            if(s_InternalSyncObject == null)
////            {
////                Object o = new Object();
////                Interlocked.CompareExchange( ref s_InternalSyncObject, o, null );
////            }
////            return s_InternalSyncObject;
////        }
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  TextInfo Constructors
////    //
////    //  Implements CultureInfo.TextInfo.
////    //
////    ////////////////////////////////////////////////////////////////////////
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////    internal unsafe TextInfo( CultureTableRecord table )
////    {
////        this.m_cultureTableRecord = table;
////        this.m_textInfoID = this.m_cultureTableRecord.ITEXTINFO;
////
////        if(table.IsSynthetic)
////        {
////            // <SyntheticSupport/>
////            //
////            // we just initialize m_pNativeTextInfo with variant to make the synthetic TextInfo works when 
////            // GetCaseInsensitiveHashCode and ChangeCaseSurrogate get called. otherwise m_pNativeTextInfo
////            // is not used at all in TextInfo with synthetic cultures.
////            // 
////            m_pNativeTextInfo = InvariantNativeTextInfo;
////        }
////        else
////        {
////            this.m_pNativeTextInfo = GetNativeTextInfo( this.m_textInfoID );
////        }
////    }
////
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    // Return the native TextInfo instance for the invariant culture.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////    internal unsafe static void* InvariantNativeTextInfo
////    {
////        [ResourceExposure( ResourceScope.Process )]
////        [ResourceConsumption( ResourceScope.Process )]
////        get
////        {
////            if(m_pInvariantNativeTextInfo == null)
////            {
////                lock(InternalSyncObject)
////                {
////                    if(m_pInvariantNativeTextInfo == null)
////                    {
////                        m_pInvariantNativeTextInfo = GetNativeTextInfo( CultureInfo.LOCALE_INVARIANT );
////                    }
////                }
////            }
////            BCLDebug.Assert( m_pInvariantNativeTextInfo != null, "TextInfo.InvariantNativeTextInfo: m_pInvariantNativeTextInfo != null" );
////            return (m_pInvariantNativeTextInfo);
////        }
////    }
////
////    #region Serialization
////    // The following field is used only for the supplemental custom culture serialization to remember 
////    // the name of the custom culture so we can reconstruct the text info properly during the deserialization.
////
////    [OptionalField( VersionAdded = 2 )]
////    private string customCultureName;
////
////    // the following fields is defined to keep the compatibility with Everett.
////    // don't change/remove the names/types of these fields.
////
////    internal int m_nDataItem;
////    internal bool m_useUserOverride;
////    internal int m_win32LangID;
////
////
////    [OnDeserializing]
////    private void OnDeserializing( StreamingContext ctx )
////    {
////        m_cultureTableRecord = null;
////        m_win32LangID = 0;
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////    private unsafe void OnDeserialized()
////    {
////        // this method will be called twice because of the support of IDeserializationCallback
////        if(m_cultureTableRecord == null)
////        {
////            // Due to our versioning, getting the CORRECT culture ID for an Everett structure is challenging.
////            if(m_win32LangID == 0)
////            {
////                m_win32LangID = CultureTableRecord.IdFromEverettDataItem( m_nDataItem );
////            }
////
////            if(customCultureName != null)
////            {
////                m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord( customCultureName, m_useUserOverride );
////            }
////            else
////            {
////                m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord( m_win32LangID, m_useUserOverride );
////            }
////
////            m_textInfoID = m_cultureTableRecord.ITEXTINFO;
////
////            if(m_cultureTableRecord.IsSynthetic)
////            {
////                // <SyntheticSupport/>
////                m_pNativeTextInfo = InvariantNativeTextInfo;
////            }
////            else
////            {
////                m_pNativeTextInfo = GetNativeTextInfo( m_textInfoID );
////            }
////        }
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
////        m_nDataItem = m_cultureTableRecord.EverettDataItem();
////        m_useUserOverride = m_cultureTableRecord.UseUserOverride;
////
////        if(CultureTableRecord.IsCustomCultureId( m_cultureTableRecord.CultureID ))
////        {
////            customCultureName = m_cultureTableRecord.SNAME;
////            m_win32LangID = m_textInfoID;
////        }
////        else
////        {
////            customCultureName = null;
////            m_win32LangID = m_cultureTableRecord.CultureID;
////        }
////
////    }
////
////    #endregion Serialization
////
////
////    [ResourceExposure( ResourceScope.Process )]
////    [ResourceConsumption( ResourceScope.Process )]
////    internal static unsafe void* GetNativeTextInfo( int cultureID )
////    {
////        // First, assume this culture does not has exceptions. I.e. we should use the default casingg table.
////        // So we assign the native NativeTextInfo for the default casing table to it.
////        void* pNativeTextInfo = m_pDefaultCasingTable;
////
////        // Now, go thru the exception table to see if it has exception or not.
////        for(int i = 0; i < m_exceptionCount; i++)
////        {
////            if(m_exceptionTable[i].langID == cultureID)
////            {
////                // This culture has exceptions.
////                if(m_exceptionNativeTextInfo[i] == 0)
////                {
////                    lock(InternalSyncObject)
////                    {
////                        // Read the exception casing file.
////                        if(m_pExceptionFile == null)
////                        {
////                            m_pExceptionFile = GlobalizationAssembly.GetGlobalizationResourceBytePtr( typeof( TextInfo ).Assembly, CASING_EXCEPTIONS_FILE_NAME );
////                        }
////                        long tempPtr = (long)(InternalAllocateCasingTable( m_pExceptionFile, m_exceptionTable[i].exceptIndex ));
////                        System.Threading.Thread.MemoryBarrier();
////                        m_exceptionNativeTextInfo[i] = tempPtr;
////                    }
////                }
////                pNativeTextInfo = (void*)m_exceptionNativeTextInfo[i];
////                break;
////            }
////        }
////        return (pNativeTextInfo);
////
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        internal static extern unsafe int CompareOrdinalIgnoreCase( String str1, String str2 );
////    {
////        return (nativeCompareOrdinalIgnoreCase( InvariantNativeTextInfo, str1, str2 ));
////    }
////
////    // This function doesn't check arguments. Please do check in the caller.
////    // The underlying unmanaged code will assert the sanity of arguments.
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////    internal static unsafe int CompareOrdinalIgnoreCaseEx( String strA, int indexA, String strB, int indexB, int length )
////    {
////        return (nativeCompareOrdinalIgnoreCaseEx( InvariantNativeTextInfo, strA, indexA, strB, indexB, length ));
////    }
////
////    // This function doesn't check arguments. Please do check in the caller.
////    // The underlying unmanaged code will assert the sanity of arguments.
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////    internal static unsafe int GetHashCodeOrdinalIgnoreCase( String s )
////    {
////        return (nativeGetHashCodeOrdinalIgnoreCase( InvariantNativeTextInfo, s ));
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////    internal static unsafe int IndexOfStringOrdinalIgnoreCase( String source, String value, int startIndex, int count )
////    {
////        if(source == null)
////            throw new ArgumentNullException( "source" );
////
////        return nativeIndexOfStringOrdinalIgnoreCase( InvariantNativeTextInfo, source, value, startIndex, count );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [ResourceConsumption( ResourceScope.Process, ResourceScope.Process )]
////    internal static unsafe int LastIndexOfStringOrdinalIgnoreCase( String source, String value, int startIndex, int count )
////    {
////
////        if(source == null)
////            throw new ArgumentNullException( "source" );
////
////        return nativeLastIndexOfStringOrdinalIgnoreCase( InvariantNativeTextInfo, source, value, startIndex, count );
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  CodePage
////    //
////    //  Returns the number of the code page used by this writing system.
////    //  The type parameter can be any of the following values:
////    //      ANSICodePage
////    //      OEMCodePage
////    //      MACCodePage
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public virtual int ANSICodePage
////    {
////        get
////        {
////            return (this.m_cultureTableRecord.IDEFAULTANSICODEPAGE);
////        }
////    }
////
////
////
////    public virtual int OEMCodePage
////    {
////        get
////        {
////            return (this.m_cultureTableRecord.IDEFAULTOEMCODEPAGE);
////        }
////    }
////
////
////    public virtual int MacCodePage
////    {
////        get
////        {
////            return (this.m_cultureTableRecord.IDEFAULTMACCODEPAGE);
////        }
////    }
////
////
////    public virtual int EBCDICCodePage
////    {
////        get
////        {
////            return (this.m_cultureTableRecord.IDEFAULTEBCDICCODEPAGE);
////        }
////    }
////
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  LCID
////    //
////    //  We need a way to get an LCID from outside of the BCL. This prop is the way.
////    //
////    // neutral cultures will cause GPS incorrect LCIDS from this
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public int LCID
////    {
////        get
////        {
////            return (this.m_textInfoID);
////        }
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  CultureName
////    //
////    //  The name of the culture from which the TextInfo was created. Even better than
////    //  the LCID since the LCID is not always unique (like in custom cultures).
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public string CultureName
////    {
////        get
////        {
////            if(null == this.m_name)
////            {
////                this.m_name = CultureInfo.GetCultureInfo( this.m_textInfoID ).Name;
////            }
////
////            return (this.m_name);
////        }
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  IsReadOnly
////    //
////    //  Detect if the object is readonly.
////    //
////    ////////////////////////////////////////////////////////////////////////
////    public bool IsReadOnly
////    {
////        get { return (m_isReadOnly); }
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  Clone
////    //
////    //  Is the implementation of IColnable.
////    //
////    ////////////////////////////////////////////////////////////////////////
////    public virtual Object Clone()
////    {
////        object o = MemberwiseClone();
////        ((TextInfo)o).SetReadOnlyState( false );
////        return (o);
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  ReadOnly
////    //
////    //  Create a cloned readonly instance or return the input one if it is 
////    //  readonly.
////    //
////    ////////////////////////////////////////////////////////////////////////
////    public static TextInfo ReadOnly( TextInfo textInfo )
////    {
////        if(textInfo == null) { throw new ArgumentNullException( "textInfo" ); }
////        if(textInfo.IsReadOnly) { return (textInfo); }
////
////        TextInfo clonedTextInfo = (TextInfo)(textInfo.MemberwiseClone());
////        clonedTextInfo.SetReadOnlyState( true );
////
////        return (clonedTextInfo);
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
////    internal void SetReadOnlyState( bool readOnly )
////    {
////        m_isReadOnly = readOnly;
////    }
////
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  ListSeparator
////    //
////    //  Returns the string used to separate items in a list.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public virtual String ListSeparator
////    {
////        get
////        {
////            if(m_listSeparator == null)
////            {
////                m_listSeparator = this.m_cultureTableRecord.SLIST;
////            }
////            return (m_listSeparator);
////        }
////
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value", Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////            m_listSeparator = value;
////        }
////    }
////
////    internal TextInfo CasingTextInfo
////    {
////        get
////        {
////            if(m_casingTextInfo == null)
////            {
////                if(ANSICodePage == TurkishAnsiCodepage)
////                {
////                    // Turkish cultures have different uppercase and lowercase for 'i' and 'I' characters. so we need 
////                    // to treat it as special case.
////                    m_casingTextInfo = CultureInfo.GetCultureInfo( "tr-TR" ).TextInfo;
////                }
////                else
////                {
////                    m_casingTextInfo = CultureInfo.GetCultureInfo( "en-US" ).TextInfo;
////                }
////            }
////
////            return m_casingTextInfo;
////        }
////    }
    
        ////////////////////////////////////////////////////////////////////////
        //
        //  ToLower
        //
        //  Converts the character or string to lower case.  Certain locales
        //  have different casing semantics from the file systems in Win32.
        //
        ////////////////////////////////////////////////////////////////////////
    
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        public unsafe extern virtual char ToLower( char c );
////    {
////        return (nativeChangeCaseChar( m_textInfoID, m_pNativeTextInfo, c, false ));
////    }
    
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        public unsafe extern virtual String ToLower( String str );
////    {
////        if(str == null) { throw new ArgumentNullException( "str" ); }
////
////        return (nativeChangeCaseString( m_textInfoID, m_pNativeTextInfo, str, false ));
////    }
    
        ////////////////////////////////////////////////////////////////////////
        //
        //  ToUpper
        //
        //  Converts the character or string to upper case.  Certain locales
        //  have different casing semantics from the file systems in Win32.
        //
        ////////////////////////////////////////////////////////////////////////
    
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        public unsafe extern virtual char ToUpper( char c );
////    {
////        return (nativeChangeCaseChar( m_textInfoID, m_pNativeTextInfo, c, true ));
////    }
    
    
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        public unsafe extern virtual String ToUpper( String str );
////    {
////        if(str == null) { throw new ArgumentNullException( "str" ); }
////
////        return (nativeChangeCaseString( m_textInfoID, m_pNativeTextInfo, str, true ));
////    }
    
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
////    public override bool Equals( Object obj )
////    {
////        TextInfo that = obj as TextInfo;
////
////        if(that != null)
////        {
////            return this.CultureName.Equals( that.CultureName );
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
////        return (this.CultureName.GetHashCode());
////    }
////
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    //  ToString
////    //
////    //  Implements Object.ToString().  Returns a string describing the
////    //  TextInfo.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////
////    public override String ToString()
////    {
////        return ("TextInfo - " + this.m_textInfoID);
////    }
////
////    //
////    // Used in ToTitleCase():
////    // When we find a starting letter, the following array decides if a category should be
////    // considered as word seprator or not.
////    //
////    private const int wordSeparatorMask =
////        /* false */ (0 << 0) | // UppercaseLetter = 0,
////        /* false */ (0 << 1) | // LowercaseLetter = 1,
////        /* false */ (0 << 2) | // TitlecaseLetter = 2,
////        /* false */ (0 << 3) | // ModifierLetter = 3,
////        /* false */ (0 << 4) | // OtherLetter = 4,
////        /* false */ (0 << 5) | // NonSpacingMark = 5,
////        /* false */ (0 << 6) | // SpacingCombiningMark = 6,
////        /* false */ (0 << 7) | // EnclosingMark = 7,
////        /* false */ (0 << 8) | // DecimalDigitNumber = 8,
////        /* false */ (0 << 9) | // LetterNumber = 9,
////        /* false */ (0 << 10) | // OtherNumber = 10,
////        /* true  */ (1 << 11) | // SpaceSeparator = 11,
////        /* true  */ (1 << 12) | // LineSeparator = 12,
////        /* true  */ (1 << 13) | // ParagraphSeparator = 13,
////        /* true  */ (1 << 14) | // Control = 14,
////        /* true  */ (1 << 15) | // Format = 15,
////        /* false */ (0 << 16) | // Surrogate = 16,
////        /* false */ (0 << 17) | // PrivateUse = 17,
////        /* true  */ (1 << 18) | // ConnectorPunctuation = 18,
////        /* true  */ (1 << 19) | // DashPunctuation = 19,
////        /* true  */ (1 << 20) | // OpenPunctuation = 20,
////        /* true  */ (1 << 21) | // ClosePunctuation = 21,
////        /* true  */ (1 << 22) | // InitialQuotePunctuation = 22,
////        /* true  */ (1 << 23) | // FinalQuotePunctuation = 23,
////        /* true  */ (1 << 24) | // OtherPunctuation = 24,
////        /* true  */ (1 << 25) | // MathSymbol = 25,
////        /* true  */ (1 << 26) | // CurrencySymbol = 26,
////        /* true  */ (1 << 27) | // ModifierSymbol = 27,
////        /* true  */ (1 << 28) | // OtherSymbol = 28,
////        /* false */ (0 << 29);  // OtherNotAssigned = 29;
////
////    private bool IsWordSeparator( UnicodeCategory category )
////    {
////        return (wordSeparatorMask & (1 << (int)category)) != 0;
////    }
////
////    // Returns the mapping of the specified string to title case.  Note that the
////    // returned string may differ in length from the input string.
////    // Generally, the first character of every word in str is uppercased.
////    // For titlecase characters, they are uppercased in a specail way.
////
////    internal const int TurkishAnsiCodepage = 1254;
////
////    public unsafe String ToTitleCase( String str )
////    {
////        if(str == null)
////        {
////            throw new ArgumentNullException( "str" );
////        }
////
////        int len = str.Length;
////        if(len == 0)
////        {
////            return (str);
////        }
////
////        int i;
////        StringBuilder result = new StringBuilder();
////        String lowercaseData = null;
////
////        for(i = 0; i < len; i++)
////        {
////            UnicodeCategory charType;
////            int charLen;
////
////            charType = CharUnicodeInfo.InternalGetUnicodeCategory( str, i, out charLen );
////            if(Char.CheckLetter( charType ))
////            {
////                // Do the uppercasing for the first character of the word.
////                // There are titlecase characters that need to be special treated.
////
////                if(charLen == 1)
////                {
////                    result.Append( nativeGetTitleCaseChar( m_pNativeTextInfo, str[i] ) );
////                }
////                else
////                {
////                    //
////                    // ASSUMPTION: There is no titlecase char in the surrogate.
////                    //
////                    char resultHighSurrogate;
////                    char resultLowSurrogate;
////
////                    ChangeCaseSurrogate( str[i], str[i + 1], out resultHighSurrogate, out resultLowSurrogate, true );
////                    result.Append( resultHighSurrogate );
////                    result.Append( resultLowSurrogate );
////                }
////                i += charLen;
////
////                //
////                // Convert the characters until the end of the this word
////                // to lowercase.
////                //
////                int lowercaseStart = i;
////
////                //
////                // Use hasLowerCase flag to prevent from lowercasing acronyms (like "URT", "USA", etc)
////                // This is in line with Word 2000 behavior of titilecasing.
////                //
////                bool hasLowerCase = (charType == UnicodeCategory.LowercaseLetter);
////                // Use a loop to find all of the other letters following this letter.
////                while(i < len)
////                {
////                    charType = CharUnicodeInfo.InternalGetUnicodeCategory( str, i, out charLen );
////                    if(IsLetterCategory( charType ))
////                    {
////                        if(charType == UnicodeCategory.LowercaseLetter)
////                        {
////                            hasLowerCase = true;
////                        }
////                        i += charLen;
////                    }
////                    else if(str[i] == '\'')
////                    {
////                        // Special case for APOSTROPHE.  It should be considered part of the word.  E.g. "can't".
////                        i++;
////                        if(hasLowerCase)
////                        {
////                            if(lowercaseData == null)
////                            {
////                                lowercaseData = this.ToLower( str );
////                            }
////                            result.Append( lowercaseData, lowercaseStart, i - lowercaseStart );
////                        }
////                        else
////                        {
////                            result.Append( str, lowercaseStart, i - lowercaseStart );
////                        }
////                        lowercaseStart = i;
////                        hasLowerCase = true;
////                    }
////                    else if(!IsWordSeparator( charType ))
////                    {
////                        // This category is considered to be part of the word.
////                        // This is any category that is marked as false in wordSeprator array.
////                        i += charLen;
////                    }
////                    else
////                    {
////                        // A word separator. Break out of the loop.
////                        break;
////                    }
////                }
////
////                int count = i - lowercaseStart;
////
////                if(count > 0)
////                {
////                    if(hasLowerCase)
////                    {
////                        if(lowercaseData == null)
////                        {
////                            lowercaseData = this.ToLower( str );
////                        }
////                        result.Append( lowercaseData, lowercaseStart, count );
////                    }
////                    else
////                    {
////                        result.Append( str, lowercaseStart, count );
////                    }
////                }
////
////                if(i < len)
////                {
////                    // Add the non-letter character.
////                    if(charLen == 1)
////                    {
////                        result.Append( str[i] );
////                    }
////                    else
////                    {
////                        // Surrogate.
////                        result.Append( str[i++] );
////                        result.Append( str[i] );
////                    }
////                }
////            }
////            else
////            {
////                //
////                // Not a letter, just append them.
////                //
////                if(charLen == 1)
////                {
////                    result.Append( str[i] );
////                }
////                else
////                {
////                    // Surrogate.
////                    result.Append( str[i++] );
////                    result.Append( str[i] );
////                }
////            }
////        }
////        return (result.ToString());
////    }
////
////    // The dominant direction of text and UI such as the relative position of buttons and scroll bars.
////
////
////    public bool IsRightToLeft
////    {
////        get
////        {
////            // The highest bit indicates writing order for left-to-right (0) or right-to-left (1)
////            return ((this.m_cultureTableRecord.ILINEORIENTATIONS & 0x8000) != 0);
////        }
////    }
////
////    private bool IsLetterCategory( UnicodeCategory uc )
////    {
////        return (uc == UnicodeCategory.UppercaseLetter
////             || uc == UnicodeCategory.LowercaseLetter
////             || uc == UnicodeCategory.TitlecaseLetter
////             || uc == UnicodeCategory.ModifierLetter
////             || uc == UnicodeCategory.OtherLetter);
////    }
////
////    /// <internalonly/>
////    void IDeserializationCallback.OnDeserialization( Object sender )
////    {
////        OnDeserialized();
////    }
////
////    //
////    // Get case-insensitive hash code for the specified string.
////    //
////    // NOTENOTE: this is an internal function.  The caller should verify the string
////    // is not null before calling this.  Currenlty, CaseInsensitiveHashCodeProvider
////    // does that.
////    //
////    internal unsafe int GetCaseInsensitiveHashCode( String str )
////    {
////        // This must be called to guarantee m_pNativeTextInfo is initialized.
////        // The reason is that the order of calling OnDeserializtion on dependent
////        // objects are not guaranteed, so a class using
////        // TextInfo class (Hashtable is an example) will have problems in
////        // its deserializtion process if methods of TextInfo class is called in the
////        // deserialization process.
////        //
////
////        if(str == null)
////        {
////            throw new ArgumentNullException( "str" );
////        }
////
////        if(m_pNativeTextInfo == null)
////        {
////            OnDeserialized();
////        }
////
////        // This is the fix to avoid introduce
////        // a dependency on mscorlib.dll and mscorwks.dll, which the real fix needs.
////        // By doing this, we will do uppercase twice for Turkish/Azeri, so it is slower
////        // in these two cultures.  The benefit is that we only have to do the fix in the managed side.
////        switch(m_textInfoID)
////        {
////            case 0x041f:    // Turkish
////            case 0x042c:    // Azeri
////                // Uppercase the specified characters.
////                str = nativeChangeCaseString( m_textInfoID, m_pNativeTextInfo, str, true );
////                break;
////        }
////        return (nativeGetCaseInsHash( str, m_pNativeTextInfo ));
////        // A better fix is to exam the m_wing32LangID and the high-char state in the native side to decide if we can do "fast hashing".
////        //return nativeGetCaseInsHash(m_textInfoID, str, m_pNativeTextInfo);
////
////    }
////
////    // A thin wrapper to avoid us to mark ToTitleCase() as unsafe.
////    internal unsafe void ChangeCaseSurrogate( char highSurrogate, char lowSurrogate, out char resultHighSurrogate, out char resultLowSurrogate, bool isToUpper )
////    {
////        fixed(char* pResultChar1 = &resultHighSurrogate, pResultChar2 = &resultLowSurrogate)
////        {
////            nativeChangeCaseSurrogate( m_pNativeTextInfo, highSurrogate, lowSurrogate, pResultChar1, pResultChar2, isToUpper );
////        }
////    }
////
////    //This method requires synchronization and should only be called from the Class Initializer.
////    [ResourceExposure( ResourceScope.Process )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static unsafe extern void* AllocateDefaultCasingTable( byte* ptr );
////
////    //This method requires synchronization and should only be called from the Class Initializer.
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static unsafe extern void* nativeGetInvariantTextInfo();
////
////    [ResourceExposure( ResourceScope.Process )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static unsafe extern void* InternalAllocateCasingTable( byte* ptr, int exceptionIndex );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static unsafe extern int nativeGetCaseInsHash( String str, void* pNativeTextInfo );
////    // private static extern int nativeGetCaseInsHash(int win32LangID, String str, void* pNativeTextInfo);
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static unsafe extern char nativeGetTitleCaseChar( void* pNativeTextInfo, char ch );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    internal static unsafe extern char nativeChangeCaseChar( int win32LangID, void* pNativeTextInfo, char ch, bool isToUpper );
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    internal static unsafe extern String nativeChangeCaseString( int win32LangID, void* pNativeTextInfo, String str, bool isToUpper );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    internal static unsafe extern void nativeChangeCaseSurrogate( void* pNativeTextInfo, char highSurrogate, char lowSurrogate, char* resultHighSurrogate, char* resultLowSurrogate, bool isToUpper );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static unsafe extern int nativeCompareOrdinalIgnoreCase( void* pNativeTextInfo, String str1, String str2 );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static unsafe extern int nativeCompareOrdinalIgnoreCaseEx( void* pNativeTextInfo, String strA, int indexA, String strB, int indexB, int length );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static unsafe extern int nativeGetHashCodeOrdinalIgnoreCase( void* pNativeTextInfo, String s );
////
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static unsafe extern int nativeIndexOfStringOrdinalIgnoreCase( void* pNativeTextInfo, String str, String value, int startIndex, int count );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    private static unsafe extern int nativeLastIndexOfStringOrdinalIgnoreCase( void* pNativeTextInfo, String str, String value, int startIndex, int count );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    internal static unsafe extern int nativeIndexOfCharOrdinalIgnoreCase( void* pNativeTextInfo, String str, Char value, int startIndex, int count );
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    internal static unsafe extern int nativeLastIndexOfCharOrdinalIgnoreCase( void* pNativeTextInfo, String str, Char value, int startIndex, int count );
    }
}


