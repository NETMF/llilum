// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace System.Globalization
{
    using System;
    using System.Security;
    using System.Threading;
    using System.Collections;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Text;

    //
    // Flags used to indicate different styles of month names.
    // This is an internal flag used by internalGetMonthName().
    // Use flag here in case that we need to provide a combination of these styles
    // (such as month name of a leap year in genitive form.  Not likely for now,
    // but would like to keep the option open).
    //
    
    [Flags]
    internal enum MonthNameStyles
    {
        Regular = 0x00000000,
        Genitive = 0x00000001,
        LeapYear = 0x00000002,
    }
    
    //
    // Flags used to indicate special rule used in parsing/formatting
    // for a specific DateTimeFormatInfo instance.
    // This is an internal flag.
    //
    // This flag is different from MonthNameStyles because this flag
    // can be expanded to accomodate parsing behaviors like CJK month names
    // or alternative month names, etc.
    
    [Flags]
    internal enum DateTimeFormatFlags
    {
        None = 0x00000000,
        UseGenitiveMonth = 0x00000001,
        UseLeapYearMonth = 0x00000002,
        UseSpacesInMonthNames = 0x00000004, // Has spaces or non-breaking space in the month names.
        UseHebrewRule = 0x00000008,   // Format/Parse using the Hebrew calendar rule.
        UseSpacesInDayNames = 0x00000010,   // Has spaces or non-breaking space in the day names.
        UseDigitPrefixInTokens = 0x00000020,   // Has token starting with numbers.
    
        NotInitialized = -1,
    }


    [Serializable]
    public sealed class DateTimeFormatInfo /*: ICloneable, IFormatProvider*/
    {
    
        // cache for the invarinat culture.
        // invariantInfo is constant irrespective of your current culture.
        private static DateTimeFormatInfo invariantInfo;
    
////    // an index which points to a record in Culture Data Table.
////    [NonSerialized]
////    internal CultureTableRecord m_cultureTableRecord;
////
////    // The culture name used to create this DTFI.
////    [OptionalField( VersionAdded = 2 )]
////    internal String m_name = null;
////
////    // The language name of the culture used to create this DTFI.
////    [NonSerialized]
////    internal String m_langName = null;
////
////    // CompareInfo usually used by the parser.
////    [NonSerialized]
////    internal CompareInfo m_compareInfo = null;
    
        // Flag to indicate if the specified calendar for this DTFI is the
        // default calendar stored in the culture.nlp.
        internal bool m_isDefaultCalendar;
    
////    internal int CultureId { get { return this.m_cultureTableRecord.CultureID; } }
////
////    // Flags to indicate if we want to retreive the information from calendar data table (calendar.nlp) or from culture data table (culture.nlp).
////    // If the flag is true, we will retrieve the data from calendar data table (calendar.nlp).
////    // If the flag is false, we will retrieve the data from culture data table (culture.nlp) or from the control panel settings.
////    // The follwoing set of information both exist in culture.nlp and calendar.nlp.
////    //
////    //  LongDatePattern
////    //  ShortDatePattern
////    //  YearMonthPattern
////    //
////    // This flag is needed so that we can support the following scenario:
////    //      CultureInfo ci = new CultureInfo("ja-jp");  // Japanese.  The default calendar for it is GregorianCalendar.
////    //      ci.Calendar = new JapaneseCalendar();   // Assign the calendar to be Japanese now.
////    //      String str = DateTimeFormatInfo.GetInstance(ci).LongDatePattern;
////    //
////    //      The new behavior will return "gg y'\x5e74'M'\x6708'd'\x65e5'".. This is the right pattern for Japanese calendar.
////    //      Previous, it returned "yyyy'\x5e74'M'\x6708'd'\x65e5'". This is wrong because it is the format for Gregorain.
////    //
////    // The default value is false, so we will get information from culture for the invariant culture.
////    //
////    // The value is decided when DateTimeFormatInfo is created in CultureInfo.GetDateTimeFormatInfo()
////    // The logic is like this:
////    //      If the specified culture is the user default culture in the system, we have to look at the calendar setting in the control panel.
////    //          If the calendar is the same as the calendar setting in the control panel, we have to take the date patterns/month names/day names
////    //             from the control panel.  By doing this, we can get the user overridden values in the control panel.
////    //          Otherwise, we should get the date patterns/month names/day names from the calendar.nlp if the calendar is not Gregorian localized.
////    //      If the specified culture is NOT the user default culture in the system,
////    //          Check if the calendar is Gregorian localized?
////    //          If yes, we use the date patterns/month names/day names from culture.nlp.
////    //          Otherwise, use the date patterns/month names/day names from calendar.nlp.
////    internal bool bUseCalendarInfo = false;
////
////    //
////    // Caches for various properties.
////    //
////    internal String amDesignator = null;
////    internal String pmDesignator = null;
////    internal String dateSeparator = null;
////    internal String longTimePattern = null;
////    internal String shortTimePattern = null;
////    internal String generalShortTimePattern = null;
////    internal String generalLongTimePattern = null;
////    internal String timeSeparator = null;
////    internal String monthDayPattern = null;
////
////    // In case default time/date pattern included in the all patterns array then we always set it as first item.
////    // that is to be easy to know if we need to add the default pattern to the array when the array is requested.
////    // look at SetDefaultPatternAsFirstItem to see how we do that.
////    internal String[] allShortTimePatterns = null;
////    internal String[] allLongTimePatterns = null;
    
        //
        // The following are constant values.
        //
        internal const String rfc1123Pattern = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";
        // The sortable pattern is based on ISO 8601.
        internal const String sortableDateTimePattern = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";
        internal const String universalSortableDateTimePattern = "yyyy'-'MM'-'dd HH':'mm':'ss'Z'";
    
        //
        // The following are affected by calendar settings.
        //
        internal Calendar calendar = null;
    
////    internal int firstDayOfWeek = -1;
////    internal int calendarWeekRule = -1;
////
////    internal String fullDateTimePattern = null;
////
////    internal String longDatePattern = null;
////
////    internal String shortDatePattern = null;
////
////    internal String yearMonthPattern = null;
////
////    internal String[] abbreviatedDayNames = null;
////
////    [OptionalField( VersionAdded = 2 )]
////    internal String[] m_superShortDayNames = null;
    
        internal String[] dayNames = null;
        internal String[] abbreviatedMonthNames = null;
        internal String[] monthNames = null;
////    // Cache the genitive month names that we retrieve from the data table.
////    [OptionalField( VersionAdded = 2 )]
////    internal String[] genitiveMonthNames = null;
////
////    // Cache the abbreviated genitive month names that we retrieve from the data table.
////    [OptionalField( VersionAdded = 2 )]
////    internal String[] m_genitiveAbbreviatedMonthNames = null;
////
////    // Cache the month names of a leap year that we retrieve from the data table.
////    [OptionalField( VersionAdded = 2 )]
////    internal String[] leapYearMonthNames = null;
////
////    // In case default time/date pattern included in the all patterns array then we always set it as first item.
////    // that is to be easy to know if we need to add the default pattern to the array when the array is requested.
////    // look at SetDefaultPatternAsFirstItem to see how we do that.
////
////    [NonSerialized] // this is lazy intialized so no need to serialize it.
////    internal String[] allYearMonthPatterns = null;
////    internal String[] allShortDatePatterns = null;
////    internal String[] allLongDatePatterns = null;
////
////
////
////    // Cache the era names for this DateTimeFormatInfo instance.
////    internal String[] m_eraNames = null;
////    internal String[] m_abbrevEraNames = null;
////    internal String[] m_abbrevEnglishEraNames = null;
////
////    internal String[] m_dateWords = null;
////
////    internal int[] optionalCalendars = null;
////
////    private const int DEFAULT_ALL_DATETIMES_SIZE = 132;
////
////    internal bool m_isReadOnly = false;
////    // This flag gives hints about if formatting/parsing should perform special code path for things like
////    // genitive form or leap year month names.
////    [OptionalField( VersionAdded = 2 )]
////    internal DateTimeFormatFlags formatFlags = DateTimeFormatFlags.NotInitialized;
////
////    private static Hashtable m_calendarNativeNames;   // Maps from calendar Id to calendar native name.
////    private static Object s_InternalSyncObject;
////
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
////    internal String CultureName
////    {
////        get
////        {
////            if(m_name == null)
////            {
////                m_name = this.m_cultureTableRecord.SNAME;
////            }
////            return (m_name);
////        }
////    }
////
////    internal String LanguageName
////    {
////        get
////        {
////            if(m_langName == null)
////            {
////                m_langName = this.m_cultureTableRecord.SISO639LANGNAME;
////            }
////            return (m_langName);
////        }
////    }
////
////    ////////////////////////////////////////////////////////////////////////////
////    //
////    // Create an array of string which contains the abbreviated day names.
////    //
////    ////////////////////////////////////////////////////////////////////////////
////
////    private String[] GetAbbreviatedDayOfWeekNames()
////    {
////        if(abbreviatedDayNames == null)
////        {
////            if(abbreviatedDayNames == null)
////            {
////                String[] temp = null;
////                if(!m_isDefaultCalendar)
////                {
////                    BCLDebug.Assert( Calendar.ID > 0, "[DateTimeFormatInfo.GetAbbreviatedDayOfWeekNames] Expected Calendar.ID > 0" );
////                    temp = CalendarTable.Default.SABBREVDAYNAMES( Calendar.ID );
////                }
////                if(temp == null || temp.Length == 0 ||
////                    temp[0].Length == 0)
////                    temp = this.m_cultureTableRecord.SABBREVDAYNAMES;
////                System.Threading.Thread.MemoryBarrier();
////                abbreviatedDayNames = temp;
////                BCLDebug.Assert( abbreviatedDayNames.Length == 7, "[DateTimeFormatInfo.GetAbbreviatedDayOfWeekNames] Expected 7 day names in a week" );
////            }
////        }
////        return (abbreviatedDayNames);
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    // Action: Returns the string array of the one-letter day of week names.
////    // Returns:
////    //  an array of one-letter day of week names
////    // Arguments:
////    //  None
////    // Exceptions:
////    //  None
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////    private String[] internalGetSuperShortDayNames()
////    {
////        if(this.m_superShortDayNames == null)
////        {
////            if(this.m_superShortDayNames == null)
////            {
////                String[] temp = null;
////                if(!m_isDefaultCalendar)
////                {
////                    BCLDebug.Assert( Calendar.ID > 0, "[DateTimeFormatInfo.internalGetSuperShortDayNames] Expected Calendar.ID > 0" );
////                    temp = CalendarTable.Default.SSUPERSHORTDAYNAMES( Calendar.ID );
////                }
////                if(temp == null || temp.Length == 0 ||
////                    temp[0].Length == 0)
////                    temp = this.m_cultureTableRecord.SSUPERSHORTDAYNAMES;
////                System.Threading.Thread.MemoryBarrier();
////                this.m_superShortDayNames = temp;
////                BCLDebug.Assert( this.m_superShortDayNames.Length == 7, "[DateTimeFormatInfo.internalGetSuperShortDayNames] Expected 7 day names in a week" );
////            }
////        }
////        return (this.m_superShortDayNames);
////    }
////
////    ////////////////////////////////////////////////////////////////////////////
////    //
////    // Create an array of string which contains the day names.
////    //
////    ////////////////////////////////////////////////////////////////////////////
////
////    private String[] GetDayOfWeekNames()
////    {
////        if(dayNames == null)
////        {
////            if(dayNames == null)
////            {
////                String[] temp = null;
////                if(!m_isDefaultCalendar)
////                {
////                    BCLDebug.Assert( Calendar.ID > 0, "[DateTimeFormatInfo.GetDayOfWeekNames] Expected Calendar.ID > 0" );
////                    temp = CalendarTable.Default.SDAYNAMES( Calendar.ID );
////                }
////                if(temp == null || temp.Length == 0 ||
////                    temp[0].Length == 0)
////                    temp = this.m_cultureTableRecord.SDAYNAMES;
////                System.Threading.Thread.MemoryBarrier();
////                dayNames = temp;
////                BCLDebug.Assert( dayNames.Length == 7, "[DateTimeFormatInfo.GetDayOfWeekNames] Expected 7 day names in a week" );
////            }
////        }
////        return (dayNames);
////    }
    
        ////////////////////////////////////////////////////////////////////////////
        //
        // Create an array of string which contains the abbreviated month names.
        //
        ////////////////////////////////////////////////////////////////////////////
    
        private String[] GetAbbreviatedMonthNames()
        {
            if(abbreviatedMonthNames == null)
            {
////            String[] temp = null;
////            if(!m_isDefaultCalendar)
////            {
////                BCLDebug.Assert( Calendar.ID > 0, "[DateTimeFormatInfo.GetAbbreviatedMonthNames] Expected Calendar.ID > 0" );
////                temp = CalendarTable.Default.SABBREVMONTHNAMES( Calendar.ID );
////            }
////            if(temp == null || temp.Length == 0 ||
////                temp[0].Length == 0)
////                temp = this.m_cultureTableRecord.SABBREVMONTHNAMES;
////            System.Threading.Thread.MemoryBarrier();
                abbreviatedMonthNames = new [] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "" };
////            BCLDebug.Assert( abbreviatedMonthNames.Length == 12 || abbreviatedMonthNames.Length == 13,
////                "[DateTimeFormatInfo.GetAbbreviatedMonthNames] Expected 12 or 13 month names in a year" );
            }
            return (abbreviatedMonthNames);
        }
    
    
        ////////////////////////////////////////////////////////////////////////////
        //
        // Create an array of string which contains the month names.
        //
        ////////////////////////////////////////////////////////////////////////////
    
        private String[] GetMonthNames()
        {
            if(monthNames == null)
            {
////            String[] temp = null;
////            if(!m_isDefaultCalendar)
////            {
////                BCLDebug.Assert( Calendar.ID > 0, "[DateTimeFormatInfo.GetMonthNames] Expected Calendar.ID > 0" );
////                temp = CalendarTable.Default.SMONTHNAMES( Calendar.ID );
////            }
////            if(temp == null || temp.Length == 0 ||
////                temp[0].Length == 0)
////                temp = this.m_cultureTableRecord.SMONTHNAMES;
////            System.Threading.Thread.MemoryBarrier();
                monthNames = new [] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December", "" };
////            BCLDebug.Assert( MonthNames.Length == 12 || MonthNames.Length == 13,
////                "[DateTimeFormatInfo.GetMonthNames] Expected 12 or 13 month names in a year" );
            }
    
            return (monthNames);
        }
    
    
        public DateTimeFormatInfo()
        {
////        //
////        // Invariant DateTimeFormatInfo doesn't have user-overriden values, so we pass false in useUserOverride.
////        //
////        this.m_cultureTableRecord = CultureInfo.InvariantCulture.m_cultureTableRecord;
            // In Invariant culture, the default calendar store in culture.nlp is Gregorian localized.
            // And the date/time pattern for invariant culture stored in
            this.m_isDefaultCalendar = true;
            this.calendar = GregorianCalendar.GetDefaultInstance();
////
////        // We don't have to call the setter of Calendar property here because the Calendar getter
////        // will return Gregorian localized calendar by default.
////        InitializeOverridableProperties();
        }
    
////    internal DateTimeFormatInfo( CultureTableRecord cultureTable, int cultureID, Calendar cal )
////    {
////        this.m_cultureTableRecord = cultureTable;
////        // m_isDefaultCalendar is set in the setter of Calendar below.
////        this.Calendar = cal;
////    }
////
////    #region Serialization
////    // The following fields are defined to keep the serialization compatibility with .NET V1.0/V1.1.
////    private int CultureID;
////    private bool m_useUserOverride;
////    private int nDataItem;
////
////    [OnDeserialized]
////    private void OnDeserialized( StreamingContext ctx )
////    {
////        BCLDebug.Assert( CultureID >= 0, "[DateTimeFormatInfo.OnDeserialized] clulture ID < 0" );
////
////        if(CultureTableRecord.IsCustomCultureId( CultureID ))
////            m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord( m_name, m_useUserOverride );
////        else
////            m_cultureTableRecord = CultureTableRecord.GetCultureTableRecord( CultureID, m_useUserOverride );
////
////        if(calendar == null)
////        {
////            calendar = (Calendar)GregorianCalendar.GetDefaultInstance().Clone();
////            calendar.SetReadOnlyState( m_isReadOnly );
////        }
////        else
////        {
////            CultureInfo.CheckDomainSafetyObject( calendar, this );
////        }
////
////        InitializeOverridableProperties();
////    }
////
////    [OnSerializing]
////    private void OnSerializing( StreamingContext ctx )
////    {
////        CultureID = m_cultureTableRecord.CultureID;
////        m_useUserOverride = m_cultureTableRecord.UseUserOverride;
////        nDataItem = m_cultureTableRecord.EverettDataItem();
////
////        //
////        // If we have custom culture then we serialize a valid name so it can be used
////        // during the deserialization as we cannot use the CultureID at that time.
////        //
////
////        if(CultureTableRecord.IsCustomCultureId( CultureID ))
////        {
////            // make sure the m_name is initialized.
////            m_name = this.CultureName;
////        }
////    }
////    #endregion Serialization
    
        // Returns a default DateTimeFormatInfo that will be universally
        // supported and constant irrespective of the current culture.
        // Used by FromString methods.
        //
    
        public static DateTimeFormatInfo InvariantInfo
        {
            get
            {
                if(invariantInfo == null)
                {
                    DateTimeFormatInfo info = new DateTimeFormatInfo();
////                info.Calendar.SetReadOnlyState( true );
////                info.m_isReadOnly = true;
                    invariantInfo = info;
                }
                return (invariantInfo);
            }
        }
    
        // Returns the current culture's DateTimeFormatInfo.  Used by Parse methods.
        //
    
        public static DateTimeFormatInfo CurrentInfo
        {
            get
            {
                System.Globalization.CultureInfo culture = System.Threading.Thread.CurrentThread.CurrentCulture;
////            if(!culture.m_isInherited)
////            {
                    DateTimeFormatInfo info = culture.DateTimeFormat;
////                if(info != null)
////                {
                        return info;
////                }
////            }
////            return (DateTimeFormatInfo)culture.GetFormat( typeof( DateTimeFormatInfo ) );
            }
        }
    
    
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        public static extern DateTimeFormatInfo GetInstance( IFormatProvider provider );
////    {
////        // Fast case for a regular CultureInfo
////        DateTimeFormatInfo info;
////        CultureInfo cultureProvider = provider as CultureInfo;
////        if(cultureProvider != null && !cultureProvider.m_isInherited)
////        {
////            info = cultureProvider.dateTimeInfo;
////            if(info != null)
////            {
////                return info;
////            }
////            else
////            {
////                return cultureProvider.DateTimeFormat;
////            }
////        }
////        // Fast case for a DTFI;
////        info = provider as DateTimeFormatInfo;
////        if(info != null)
////        {
////            return info;
////        }
////        if(provider != null)
////        {
////            info = provider.GetFormat( typeof( DateTimeFormatInfo ) ) as DateTimeFormatInfo;
////            if(info != null)
////            {
////                return info;
////            }
////        }
////        return CurrentInfo;
////    }
////
////
////    public Object GetFormat( Type formatType )
////    {
////        return (formatType == typeof( DateTimeFormatInfo ) ? this : null);
////    }
////
////
////    public Object Clone()
////    {
////        DateTimeFormatInfo n = (DateTimeFormatInfo)MemberwiseClone();
////        // We can use the data member calendar in the setter, instead of the property Calendar,
////        // since the cloned copy should have the same state as the original copy.
////        n.calendar = (Calendar)this.Calendar.Clone();
////        n.m_isReadOnly = false;
////        return n;
////    }
    
    
        public String AMDesignator
        {
            get
            {
                return "AM";
////            BCLDebug.Assert( amDesignator != null, "DateTimeFormatInfo.AMDesignator, amDesignator != null" );
////            return (amDesignator);
            }
    
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////            ClearTokenHashTable( true );
////            amDesignator = value;
////        }
        }
    
////    private void InitializeOverridableProperties()
////    {
////        // These properties are not affected by calendar settings.  And they should be always initialized.
////
////        if(amDesignator == null) { amDesignator = m_cultureTableRecord.S1159; }
////        if(pmDesignator == null) { pmDesignator = m_cultureTableRecord.S2359; }
////        if(longTimePattern == null) { longTimePattern = m_cultureTableRecord.STIMEFORMAT; }
////        if(firstDayOfWeek == -1) { firstDayOfWeek = m_cultureTableRecord.IFIRSTDAYOFWEEK; }
////        if(calendarWeekRule == -1) { calendarWeekRule = m_cultureTableRecord.IFIRSTWEEKOFYEAR; }
////
////        // These 3 properties are affected by calendar settings.
////        if(yearMonthPattern == null) { yearMonthPattern = GetYearMonthPattern( calendar.ID ); }
////        if(shortDatePattern == null) { shortDatePattern = GetShortDatePattern( calendar.ID ); }
////        if(longDatePattern == null) { longDatePattern = GetLongDatePattern( calendar.ID ); }
////    }
    
    
        public Calendar Calendar
        {
            get
            {
////            BCLDebug.Assert( calendar != null, "DateTimeFormatInfo.Calendar: calendar != null" );
                return (calendar);
            }
    
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_Obj" ) );
////            }
////            if(value == calendar)
////            {
////                return;
////            }
////
////            //
////            // Because the culture is agile object which can be attached to a thread and then thread can travel
////            // to another app domain then we prevent attaching any customized object to culture that we cannot contol.
////            //
////            CultureInfo.CheckDomainSafetyObject( value, this );
////
////            for(int i = 0; i < OptionalCalendars.Length; i++)
////            {
////                if(OptionalCalendars[i] == value.ID)
////                {
////                    ClearTokenHashTable( false );
////                    //
////                    // Get the current Win32 user culture.
////                    //
////                    m_isDefaultCalendar = (value.ID == Calendar.CAL_GREGORIAN);
////                    /*
////                        When UseUserOverride is TRUE, we should follow the following table
////                        to retrieve date/time patterns.
////
////                        CurrentCulture:     Is the culture which creates the DTFI the current user default culture
////                                            specified in the control panel?
////                        CurrentCalendar:    Is the specified calendar the current calendar specified in the control panel?
////                        n/r: not relavent, don't care.
////
////                        Case    CurrentCulture? CurrentCalendar?    GregorianLocalized? Get Data from
////                        ----    --------------- ----------------    ------------------- --------------------------
////                        1       Y               Y                   n/r                 registry & culture.nlp (for user-overridable data)
////                        2       n/r             n/r                 Y                   culture.nlp
////                        3       n/r             n/r                 N                   CALENDAR.nlp*
////                    */
////
////                    if(calendar != null)
////                    {
////                        // clean related properties which are affected by the calendar setting,
////                        // so that they will be refreshed when they are accessed next time.
////                        //
////
////                        // These properites are in the order as appearing in calendar.xml.
////                        m_eraNames = null;
////                        m_abbrevEraNames = null;
////                        m_abbrevEnglishEraNames = null;
////
////                        shortDatePattern = null;
////                        yearMonthPattern = null;
////                        monthDayPattern = null;
////                        longDatePattern = null;
////
////                        dayNames = null;
////                        abbreviatedDayNames = null;
////                        m_superShortDayNames = null;
////                        monthNames = null;
////                        abbreviatedMonthNames = null;
////                        genitiveMonthNames = null;
////                        m_genitiveAbbreviatedMonthNames = null;
////                        leapYearMonthNames = null;
////                        formatFlags = DateTimeFormatFlags.NotInitialized;
////
////                        // These properies are not in calendar.xml, but they are dependent on the values like shortDatePattern.
////                        fullDateTimePattern = null;
////                        generalShortTimePattern = null;
////                        generalLongTimePattern = null;
////                        allShortDatePatterns = null;
////                        allLongDatePatterns = null;
////                        allYearMonthPatterns = null;
////
////
////                    }
////
////                    calendar = value;
////
////                    // Retrieve the settings that can be overridden by users.
////                    // TODO: This doesn't guarantee current cultureness for custom culture.
////                    if(this.m_cultureTableRecord.UseCurrentCalendar( value.ID ))
////                    {
////                        //
////                        // [Case 1]
////                        //
////                        // If user overriden values are allowed, and the culture is the user default culture
////                        // and the specified calendar matches the calendar setting in the control panel,
////                        // use data from registry by setting bUseCalendarInfo to be false.
////                        //
////                        DTFIUserOverrideValues temp = new DTFIUserOverrideValues();
////
////                        // If this is the first time calendar is set, just assign
////                        // the value to calendar.  We don't have to clean up
////                        // related fields.
////                        // We need to retrieve all user-override values and cache them to avoid to get different
////                        // values between calls to CultureInfo.ClearCachedData()
////                        m_cultureTableRecord.GetDTFIOverrideValues( ref temp );
////
////
////                        amDesignator = temp.amDesignator;
////                        pmDesignator = temp.pmDesignator;
////                        longTimePattern = temp.longTimePattern;
////                        firstDayOfWeek = (int)temp.firstDayOfWeek;
////                        calendarWeekRule = (int)temp.calendarWeekRule;
////                        shortDatePattern = temp.shortDatePattern;
////                        longDatePattern = temp.longDatePattern;
////                        yearMonthPattern = temp.yearMonthPattern;
////
////                        // There is also a NLS bug that GetLocaleInfo returns incorrect YearMonth pattern when the reg key of for YearMonth does not
////                        // exists. GetDTFIOverrideValues() will leave yearMonthPattern to be null in that case.
////
////                        // In these cases, fall back to the table value.
////
////                        if(yearMonthPattern == null)
////                        {
////                            yearMonthPattern = GetYearMonthPattern( value.ID );
////                        }
////
////                    }
////                    else
////                    {
////                        // Case 2 & Case 3
////
////                        InitializeOverridableProperties();
////                    }
////                    return;
////                }
////            }
////            // The assigned calendar is not a valid calendar for this culture.
////            throw new ArgumentOutOfRangeException( "value", Environment.GetResourceString( "Argument_InvalidCalendar" ) );
////        }
        }
    
////    internal int[] OptionalCalendars
////    {
////        get
////        {
////            if(optionalCalendars == null)
////            {
////                optionalCalendars = this.m_cultureTableRecord.IOPTIONALCALENDARS;
////            }
////            return (optionalCalendars);
////        }
////    }
////
////    /*=================================GetEra==========================
////    **Action: Get the era value by parsing the name of the era.
////    **Returns: The era value for the specified era name.
////    **      -1 if the name of the era is not valid or not supported.
////    **Arguments: eraName    the name of the era.
////    **Exceptions: None.
////    ============================================================================*/
////
////
////    public int GetEra( String eraName )
////    {
////        if(eraName == null)
////        {
////            throw new ArgumentNullException( "eraName",
////                Environment.GetResourceString( "ArgumentNull_String" ) );
////        }
////
////        // The following is based on the assumption that the era value is starting from 1, and has a
////        // serial values.
////        // If that ever changes, the code has to be changed.
////
////        // The calls to String.Compare should use the current culture for the string comparisons, but the
////        // invariant culture when comparing against the english names.
////        for(int i = 0; i < EraNames.Length; i++)
////        {
////            // Compare the era name in a case-insensitive way.
////            if(m_eraNames[i].Length > 0)
////            {
////                if(String.Compare( eraName, m_eraNames[i], true, CultureInfo.CurrentCulture ) == 0)
////                {
////                    return (i + 1);
////                }
////            }
////        }
////        for(int i = 0; i < AbbreviatedEraNames.Length; i++)
////        {
////            if(String.Compare( eraName, m_abbrevEraNames[i], true, CultureInfo.CurrentCulture ) == 0)
////            {
////                return (i + 1);
////            }
////        }
////        for(int i = 0; i < AbbreviatedEnglishEraNames.Length; i++)
////        {
////            // this comparison should use the InvariantCulture.  The English name could have linguistically
////            // interesting characters.
////            if(String.Compare( eraName, m_abbrevEnglishEraNames[i], true, CultureInfo.InvariantCulture ) == 0)
////            {
////                return (i + 1);
////            }
////        }
////        return (-1);
////    }
////
////    internal String[] EraNames
////    {
////        get
////        {
////            if(m_eraNames == null)
////            {
////                if(Calendar.ID == Calendar.CAL_GREGORIAN)
////                {
////                    // If the calendar is Gregorian localized calendar,
////                    // grab the localized name from culture.nlp.
////                    m_eraNames = new String[1] {
////                        this.m_cultureTableRecord.SADERA
////                    };
////                }
////                else if(Calendar.ID != Calendar.CAL_TAIWAN)
////                {
////                    // Use Calendar property so that we initialized the calendar when necessary.
////                    BCLDebug.Assert( Calendar.ID > 0, "[DateTimeFormatInfo.EraNames] Expected Calendar.ID > 0" );
////                    m_eraNames = CalendarTable.Default.SERANAMES( Calendar.ID );
////                }
////                else
////                {
////                    // Special case for Taiwan calendar.
////                    // 0x0404 is the locale ID for Taiwan.
////                    m_eraNames = new String[] { CalendarTable.nativeGetEraName( 0x0404, Calendar.ID ) };
////                }
////            }
////            return (m_eraNames);
////        }
////    }
////
////    /*=================================GetEraName==========================
////    **Action: Get the name of the era for the specified era value.
////    **Returns: The name of the specified era.
////    **Arguments:
////    **      era the era value.
////    **Exceptions:
////    **      ArguementException if the era valie is invalid.
////    ============================================================================*/
////
////
////    public String GetEraName( int era )
////    {
////        if(era == Calendar.CurrentEra)
////        {
////            era = Calendar.CurrentEraValue;
////        }
////
////        // The following is based on the assumption that the era value is starting from 1, and has a
////        // serial values.
////        // If that ever changes, the code has to be changed.
////        if((--era) < EraNames.Length && (era >= 0))
////        {
////            return (m_eraNames[era]);
////        }
////        throw new ArgumentOutOfRangeException( "era", Environment.GetResourceString( "ArgumentOutOfRange_InvalidEraValue" ) );
////    }
////
////    internal String[] AbbreviatedEraNames
////    {
////        get
////        {
////            if(m_abbrevEraNames == null)
////            {
////                if(Calendar.ID == Calendar.CAL_TAIWAN)
////                {
////                    String twnEra = GetEraName( 1 );
////                    if(twnEra.Length > 0)
////                    {
////                        if(twnEra.Length == 4)
////                        {
////                            // Windows 2000 & above use 4-character era name.
////                            // Special case for Taiwan because of geo-political issue.
////                            m_abbrevEraNames = new String[] { twnEra.Substring( 2, 2 ) };
////                        }
////                        else
////                        {
////                            // Windows 98/Windows ME use 2-character era name.
////                            m_abbrevEraNames = new String[] { twnEra };
////                        }
////                    }
////                    else
////                    {
////                        // Return an etmpy string.
////                        m_abbrevEraNames = new String[0];
////                    }
////                }
////                else
////                {
////                    if(Calendar.ID == Calendar.CAL_GREGORIAN)
////                    {
////                        // If the calendar is Gregorian localized calendar,
////                        // grab the localized name from culture.nlp.
////                        m_abbrevEraNames = new String[1] {
////                            this.m_cultureTableRecord.SABBREVADERA
////                        };
////                    }
////                    else
////                    {
////                        BCLDebug.Assert( Calendar.ID > 0, "[DateTimeFormatInfo.AbbreviatedEraNames] Expected Calendar.ID > 0" );
////                        m_abbrevEraNames = CalendarTable.Default.SABBREVERANAMES( Calendar.ID );
////                    }
////                }
////            }
////            return (m_abbrevEraNames);
////        }
////    }
////
////
////    public String GetAbbreviatedEraName( int era )
////    {
////        if(AbbreviatedEraNames.Length == 0)
////        {
////            // If abbreviation era name is not used in this culture,
////            // return the full era name.
////            return (GetEraName( era ));
////        }
////        if(era == Calendar.CurrentEra)
////        {
////            era = Calendar.CurrentEraValue;
////        }
////        if((--era) < m_abbrevEraNames.Length && (era >= 0))
////        {
////            return (m_abbrevEraNames[era]);
////        }
////        throw new ArgumentOutOfRangeException( "era", Environment.GetResourceString( "ArgumentOutOfRange_InvalidEraValue" ) );
////    }
////
////    internal String[] AbbreviatedEnglishEraNames
////    {
////        get
////        {
////            if(m_abbrevEnglishEraNames == null)
////            {
////                BCLDebug.Assert( Calendar.ID > 0, "[DateTimeFormatInfo.AbbreviatedEnglishEraNames] Expected Calendar.ID > 0" );
////                m_abbrevEnglishEraNames = CalendarTable.Default.SABBREVENGERANAMES( Calendar.ID );
////            }
////            return (m_abbrevEnglishEraNames);
////        }
////    }
    
    
        public String DateSeparator
        {
            get
            {
                return "/";
////            if(dateSeparator == null)
////            {
////                dateSeparator = this.m_cultureTableRecord.SDATE;
////            }
////            return (dateSeparator);
            }
    
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////            ClearTokenHashTable( true );
////            dateSeparator = value;
////        }
        }
    
    
////    public DayOfWeek FirstDayOfWeek
////    {
////        get
////        {
////            // FirstDayOfWeek is always set in the Calendar setter.
////            return ((DayOfWeek)firstDayOfWeek);
////        }
////
////        set
////        {
////            VerifyWritable();
////            if(value >= DayOfWeek.Sunday && value <= DayOfWeek.Saturday)
////            {
////                firstDayOfWeek = (int)value;
////            }
////            else
////            {
////                throw new ArgumentOutOfRangeException(
////                    "value", String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
////                    DayOfWeek.Sunday, DayOfWeek.Saturday ) );
////            }
////        }
////    }
////
////
////    public CalendarWeekRule CalendarWeekRule
////    {
////        get
////        {
////            // CalendarWeekRule is always set in the Calendar setter.
////            return ((CalendarWeekRule)this.calendarWeekRule);
////        }
////
////        set
////        {
////            VerifyWritable();
////            if(value >= CalendarWeekRule.FirstDay && value <= CalendarWeekRule.FirstFourDayWeek)
////            {
////                calendarWeekRule = (int)value;
////            }
////            else
////            {
////                throw new ArgumentOutOfRangeException(
////                    "value", String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
////                    CalendarWeekRule.FirstDay, CalendarWeekRule.FirstFourDayWeek ) );
////            }
////        }
////    }
    
    
    
        public String FullDateTimePattern
        {
            get
            {
////            if(fullDateTimePattern == null)
////            {
////                fullDateTimePattern = LongDatePattern + " " + LongTimePattern;
////            }
////            return (fullDateTimePattern);
                return LongDatePattern + " " + LongTimePattern;
            }
    
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////            fullDateTimePattern = value;
////        }
        }
    
////    private String GetLongDatePattern( int calID )
////    {
////        String strTemp = String.Empty;
////        if(!m_isDefaultCalendar)
////        {
////            // Has to be > 0 or the data'll be null
////            BCLDebug.Assert( calID > 1, "[DateTimeFormatInfo.LongDatePattern] Expected calID > 1" );
////            strTemp = CalendarTable.Default.SLONGDATE( calID )[0];
////
////            BCLDebug.Assert( strTemp.Length != 0, "Calendar.nlp should have SLONGDATE value" );
////        }
////        else
////        {
////            strTemp = this.m_cultureTableRecord.SLONGDATE;
////        }
////        return (strTemp);
////    }
    
    
        public String LongDatePattern
        {
            get
            {
                return "dddd, dd MMMM yyyy";
////            BCLDebug.Assert( longDatePattern != null, "DateTimeFormatInfo.LongDatePattern, longDatePattern != null" );
////            return (longDatePattern);
            }
    
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////            ClearTokenHashTable( true );
////
////            SetDefaultPatternAsFirstItem( allLongDatePatterns, value );
////            longDatePattern = value;
////            // Clean up cached values that will be affected by this property.
////            fullDateTimePattern = null;
////        }
        }
    
    
        public String LongTimePattern
        {
            get
            {
                return "HH:mm:ss";
////            // LongTimeProperty is always set in the Calendar setter.
////            BCLDebug.Assert( longTimePattern != null, "DateTimeFormatInfo.LongTimePattern, longTimePattern != null" );
////            return (longTimePattern);
            }
    
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////
////            longTimePattern = value;
////            // Clean up cached values that will be affected by this property.
////            fullDateTimePattern = null;     // Full date = long date + long Time
////            generalLongTimePattern = null;  // General long date = short date + long Time
////        }
    
        }
    
    
        public String MonthDayPattern
        {
            get
            {
                return "MMMM dd";
////            if(monthDayPattern == null)
////            {
////                // strTemp avoids Empty (but not null) string problems in the else.
////                // presumably Empty could be set.
////                String strTemp;
////                if(m_isDefaultCalendar)
////                {
////                    strTemp = this.m_cultureTableRecord.SMONTHDAY;
////                }
////                else
////                {
////                    BCLDebug.Assert( Calendar.ID > 0, "[DateTimeFormatInfo.GetStringFromTable] Expected Calendar.ID > 0" );
////                    strTemp = CalendarTable.Default.SMONTHDAY( Calendar.ID );
////                    // Note that for a tiny bit of time we're not null here, but we're also not necessarily non-empty
////                    if(strTemp.Length == 0)
////                        strTemp = this.m_cultureTableRecord.SMONTHDAY;
////                }
////                monthDayPattern = strTemp;
////            }
////            return (monthDayPattern);
            }
    
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////            monthDayPattern = value;
////        }
        }
    
    
        public String PMDesignator
        {
            get
            {
                return "PM";
////            BCLDebug.Assert( pmDesignator != null, "DateTimeFormatInfo.PMDesignator, pmDesignator != null" );
////            return (pmDesignator);
            }
    
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////            ClearTokenHashTable( true );
////            pmDesignator = value;
////        }
    
        }
    
    
        public String RFC1123Pattern
        {
            get
            {
                return (rfc1123Pattern);
            }
        }
    
////    internal String GetShortDatePattern( int calID )
////    {
////        String strTemp = String.Empty;
////        if(!m_isDefaultCalendar)
////        {
////            // Has to be > 0 or the data'll be null
////            BCLDebug.Assert( calID > 1, "[DateTimeFormatInfo.ShortDatePattern] Expected calID > 1" );
////            strTemp = CalendarTable.Default.SSHORTDATE( calID )[0];
////
////            BCLDebug.Assert( strTemp.Length != 0, "Calendar.nlp should have SHORTDATE value" );
////        }
////        else
////        {
////            strTemp = this.m_cultureTableRecord.SSHORTDATE;
////        }
////        return (strTemp);
////    }
    
    
        public String ShortDatePattern
        {
            get
            {
                return "MM/dd/yyyy";
////            BCLDebug.Assert( shortDatePattern != null, "DateTimeFormatInfo.ShortDatePattern, shortDatePattern != null" );
////            return (shortDatePattern);
            }
    
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////
////            SetDefaultPatternAsFirstItem( allShortDatePatterns, value );
////            shortDatePattern = value;
////            // Clean up cached values that will be affected by this property.
////            generalLongTimePattern = null;
////            generalShortTimePattern = null;
////        }
        }
    
    
        public String ShortTimePattern
        {
            get
            {
                return "HH:mm";
////            if(shortTimePattern == null)
////            {
////                shortTimePattern = this.m_cultureTableRecord.SSHORTTIME;
////            }
////            return (shortTimePattern);
            }
    
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////            shortTimePattern = value;
////            // Clean up cached values that will be affected by this property.
////            generalShortTimePattern = null; // General short date = short date + short time.
////        }
        }
    
    
        public String SortableDateTimePattern
        {
            get
            {
                return (sortableDateTimePattern);
            }
        }
    
        /*=================================GeneralShortTimePattern=====================
        **Property: Return the pattern for 'g' general format: shortDate + short time
        **Note: This is used by DateTimeFormat.cs to get the pattern for 'g'
        **      We put this internal property here so that we can avoid doing the
        **      concatation every time somebody asks for the general format.
        ==============================================================================*/
    
        internal String GeneralShortTimePattern
        {
            get
            {
////            if(generalShortTimePattern == null)
////            {
////                generalShortTimePattern = ShortDatePattern + " " + ShortTimePattern;
////            }
////            return (generalShortTimePattern);
                return ShortDatePattern + " " + ShortTimePattern;
            }
        }
    
        /*=================================GeneralLongTimePattern=====================
        **Property: Return the pattern for 'g' general format: shortDate + Long time
        **Note: This is used by DateTimeFormat.cs to get the pattern for 'g'
        **      We put this internal property here so that we can avoid doing the
        **      concatation every time somebody asks for the general format.
        ==============================================================================*/
    
        internal String GeneralLongTimePattern
        {
            get
            {
////            if(generalLongTimePattern == null)
////            {
////                generalLongTimePattern = ShortDatePattern + " " + LongTimePattern;
////            }
////            return (generalLongTimePattern);
                return ShortDatePattern + " " + LongTimePattern;
            }
        }
    
    
        public String TimeSeparator
        {
            get
            {
                return ":";
////            if(timeSeparator == null)
////            {
////                timeSeparator = this.m_cultureTableRecord.STIME;
////            }
////            return (timeSeparator);
            }
    
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////            ClearTokenHashTable( true );
////            timeSeparator = value;
////        }
    
        }
    
    
        public String UniversalSortableDateTimePattern
        {
            get
            {
                return (universalSortableDateTimePattern);
            }
        }
    
////    private String GetYearMonthPattern( int calID )
////    {
////        String result = null;
////        if(!m_isDefaultCalendar)
////        {
////            // The calendar is the calendar not specified in culture.nlp.  Use the calendar table.
////            // Has to be > 0 or the data'll be null                
////            BCLDebug.Assert( calID > 1, "[DateTimeFormatInfo.YearMonthPattern] Expected calID > 1" );
////            result = CalendarTable.Default.SYEARMONTH( calID )[0];
////
////            BCLDebug.Assert( result.Length != 0, "Calendar.nlp should have SYEARMONTH value" );
////        }
////        else
////        {
////            // The calendar is the calendar specified in culture.nlp.  Use it.
////            result = this.m_cultureTableRecord.SYEARMONTHS[0];
////        }
////        return (result);
////
////    }
    
    
        public String YearMonthPattern
        {
            get
            {
////            BCLDebug.Assert( yearMonthPattern != null, "DateTimeFormatInfo.YearMonthPattern: yearMonthPattern != null" );
////            return (yearMonthPattern);
                return "yyyy MMMM";
            }
    
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_String" ) );
////            }
////            yearMonthPattern = value;
////            SetDefaultPatternAsFirstItem( allYearMonthPatterns, yearMonthPattern );
////        }
        }
    
    
////    //
////    // Check if a string array contains a null value, and throw ArgumentNullException with parameter name "value"
////    //
////    private void CheckNullValue( String[] values, int length )
////    {
////        BCLDebug.Assert( values != null, "value != null" );
////        for(int i = 0; i < length; i++)
////        {
////            if(values[i] == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_ArrayValue" ) );
////            }
////        }
////    }
////
////
////    public String[] AbbreviatedDayNames
////    {
////        get
////        {
////            return ((String[])GetAbbreviatedDayOfWeekNames().Clone());
////        }
////
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_Array" ) );
////            }
////            if(value.Length != 7)
////            {
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_InvalidArrayLength" ), 7 ), "value" );
////            }
////            CheckNullValue( value, value.Length );
////            ClearTokenHashTable( true );
////            abbreviatedDayNames = value;
////        }
////    }
////
////
////    // Returns the string array of the one-letter day of week names.
////    public String[] ShortestDayNames
////    {
////        get
////        {
////            return ((String[])internalGetSuperShortDayNames().Clone());
////        }
////
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_Array" ) );
////            }
////            if(value.Length != 7)
////            {
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_InvalidArrayLength" ), 7 ), "value" );
////            }
////            CheckNullValue( value, value.Length );
////            this.m_superShortDayNames = value;
////        }
////    }
////
////
////    public String[] DayNames
////    {
////        get
////        {
////            return ((String[])GetDayOfWeekNames().Clone());
////        }
////
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_Array" ) );
////            }
////            if(value.Length != 7)
////            {
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_InvalidArrayLength" ), 7 ), "value" );
////            }
////            CheckNullValue( value, value.Length );
////            ClearTokenHashTable( true );
////            dayNames = value;
////        }
////    }
////
////
////    public String[] AbbreviatedMonthNames
////    {
////        get
////        {
////            return ((String[])GetAbbreviatedMonthNames().Clone());
////        }
////
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_Array" ) );
////            }
////            if(value.Length != 13)
////            {
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_InvalidArrayLength" ), 13 ), "value" );
////            }
////            CheckNullValue( value, value.Length - 1 );
////            ClearTokenHashTable( true );
////            abbreviatedMonthNames = value;
////        }
////    }
    
    
        public String[] MonthNames
        {
            get
            {
                return ((String[])GetMonthNames().Clone());
            }
    
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_Array" ) );
////            }
////            if(value.Length != 13)
////            {
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_InvalidArrayLength" ), 13 ), "value" );
////            }
////            CheckNullValue( value, value.Length - 1 );
////            monthNames = value;
////            ClearTokenHashTable( true );
////        }
        }
    
////    // Whitespaces that we allow in the month names.
////    // U+00a0 is non-breaking space.
////    static char[] MonthSpaces = { ' ', '\u00a0' };
////
////    internal bool HasSpacesInMonthNames
////    {
////        get
////        {
////            return (FormatFlags & DateTimeFormatFlags.UseSpacesInMonthNames) != 0;
////        }
////    }
////
////    internal bool HasSpacesInDayNames
////    {
////        get
////        {
////            return (FormatFlags & DateTimeFormatFlags.UseSpacesInDayNames) != 0;
////        }
////    }
////
////
////    //
////    //  internalGetMonthName
////    //
////    // Actions: Return the month name using the specified MonthNameStyles in either abbreviated form
////    //      or full form.
////    // Arguments:
////    //      month
////    //      style           To indicate a form like regular/genitive/month name in a leap year.
////    //      abbreviated     When true, return abbreviated form.  Otherwise, return a full form.
////    //  Exceptions:
////    //      ArgumentOutOfRangeException When month name is invalid.
////    //
////    internal String internalGetMonthName( int month, MonthNameStyles style, bool abbreviated )
////    {
////        //
////        // Right now, style is mutual exclusive, but I make the style to be flag so that
////        // maybe we can combine flag if there is such a need.
////        //
////        String[] monthNamesArray = null;
////        switch(style)
////        {
////            case MonthNameStyles.Genitive:
////                monthNamesArray = internalGetGenitiveMonthNames( abbreviated );
////                break;
////            case MonthNameStyles.LeapYear:
////                monthNamesArray = internalGetLeapYearMonthNames(/*abbreviated*/);
////                break;
////            default:
////                monthNamesArray = (abbreviated ? GetAbbreviatedMonthNames() : GetMonthNames());
////                break;
////        }
////        // The month range is from 1 ~ this.m_monthNames.Length
////        // (actually is 13 right now for all cases)
////        if((month < 1) || (month > monthNamesArray.Length))
////        {
////            throw new ArgumentOutOfRangeException(
////                "month", String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
////                1, monthNamesArray.Length ) );
////        }
////        return (monthNamesArray[month - 1]);
////    }
////
////    //
////    //  internalGetGenitiveMonthNames
////    //
////    //  Action: Retrieve the array which contains the month names in genitive form.
////    //      If this culture does not use the gentive form, the normal month name is returned.
////    //  Arguments:
////    //      abbreviated     When true, return abbreviated form.  Otherwise, return a full form.
////    //
////    private String[] internalGetGenitiveMonthNames( bool abbreviated )
////    {
////        if(abbreviated)
////        {
////            if(this.m_genitiveAbbreviatedMonthNames == null)
////            {
////                if(m_isDefaultCalendar)
////                {
////                    // Only Gregorian localized calendar can have genitive form.
////                    String[] temp = this.m_cultureTableRecord.SABBREVMONTHGENITIVENAMES;
////                    if(temp.Length > 0)
////                    {
////                        // Genitive form exists.
////                        this.m_genitiveAbbreviatedMonthNames = temp;
////                    }
////                    else
////                    {
////                        // Genitive form does not exist.  Use the regular month names.
////                        this.m_genitiveAbbreviatedMonthNames = GetAbbreviatedMonthNames();
////                    }
////                }
////                else
////                {
////                    this.m_genitiveAbbreviatedMonthNames = GetAbbreviatedMonthNames();
////                }
////                BCLDebug.Assert( this.m_genitiveAbbreviatedMonthNames.Length == 13,
////                    "[DateTimeFormatInfo.GetGenitiveMonthNames] Expected 13 abbreviated genitive month names in a year" );
////            }
////            return (this.m_genitiveAbbreviatedMonthNames);
////        }
////
////        if(genitiveMonthNames == null)
////        {
////            if(m_isDefaultCalendar)
////            {
////                // Only Gregorian localized calendar can have genitive form.
////                String[] temp = this.m_cultureTableRecord.SMONTHGENITIVENAMES;
////                if(temp.Length > 0)
////                {
////                    // Genitive form exists.
////                    genitiveMonthNames = temp;
////                }
////                else
////                {
////                    // Genitive form does not exist.  Use the regular month names.
////                    genitiveMonthNames = GetMonthNames();
////                }
////            }
////            else
////            {
////                genitiveMonthNames = GetMonthNames();
////            }
////            BCLDebug.Assert( genitiveMonthNames.Length == 13,
////                "[DateTimeFormatInfo.GetGenitiveMonthNames] Expected 13 genitive month names in a year" );
////        }
////        return (genitiveMonthNames);
////    }
////
////    //
////    //  internalGetLeapYearMonthNames
////    //
////    //  Actions: Retrieve the month names used in a leap year.
////    //      If this culture does not have different month names in a leap year, the normal month name is returned.
////    //  Agruments: None. (can use abbreviated later if needed)
////    //
////    internal String[] internalGetLeapYearMonthNames(/*bool abbreviated*/)
////    {
////        if(leapYearMonthNames == null)
////        {
////            if(m_isDefaultCalendar)
////            {
////                //
////                // If this is a Gregorian localized calendar, there is no differences between the month names in a regular year
////                // and those in a leap year.  Just return the regular month names.
////                //
////                leapYearMonthNames = GetMonthNames();
////            }
////            else
////            {
////                BCLDebug.Assert( Calendar.ID > 0, "[DateTimeFormatInfo.internalGetLeapYearMonthNames] Expected Calendar.ID > 0" );
////                String[] temp = CalendarTable.Default.SLEAPYEARMONTHNAMES( Calendar.ID );
////                if(temp.Length > 0)
////                {
////                    leapYearMonthNames = temp;
////                }
////                else
////                {
////                    leapYearMonthNames = GetMonthNames();
////                }
////            }
////        }
////        return (leapYearMonthNames);
////    }
////
////

        public String GetAbbreviatedDayName( DayOfWeek dayofweek )
        {
            switch(dayofweek)
            {
                case DayOfWeek.Sunday   : return "Sun";
                case DayOfWeek.Monday   : return "Mon";
                case DayOfWeek.Tuesday  : return "Tue";
                case DayOfWeek.Wednesday: return "Wed";
                case DayOfWeek.Thursday : return "Thu";
                case DayOfWeek.Friday   : return "Fri";
                case DayOfWeek.Saturday : return "Sat";
            }

            return String.Empty;
        }

////    public String GetAbbreviatedDayName( DayOfWeek dayofweek )
////    {
////
////        if((int)dayofweek < 0 || (int)dayofweek > 6)
////        {
////            throw new ArgumentOutOfRangeException(
////                "dayofweek", String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
////                DayOfWeek.Sunday, DayOfWeek.Saturday ) );
////        }
////        //
////        // Don't call the public property AbbreviatedDayNames here since a clone is needed in that
////        // property, so it will be slower.  Instead, use GetAbbreviatedDayOfWeekNames() directly.
////        //
////        return (GetAbbreviatedDayOfWeekNames()[(int)dayofweek]);
////    }
////
////    // Returns the super short day of week names for the specified day of week.
////
////    public String GetShortestDayName( DayOfWeek dayOfWeek )
////    {
////
////        if((int)dayOfWeek < 0 || (int)dayOfWeek > 6)
////        {
////            throw new ArgumentOutOfRangeException(
////                "dayOfWeek", String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
////                DayOfWeek.Sunday, DayOfWeek.Saturday ) );
////        }
////        //
////        // Don't call the public property SuperShortDayNames here since a clone is needed in that
////        // property, so it will be slower.  Instead, use internalGetSuperShortDayNames() directly.
////        //
////        return (internalGetSuperShortDayNames()[(int)dayOfWeek]);
////    }
////
////
////    internal String[] GetCombinedPatterns( String[] patterns1, String[] patterns2, String connectString )
////    {
////        String[] result = new String[patterns1.Length * patterns2.Length];
////        for(int i = 0; i < patterns1.Length; i++)
////        {
////            for(int j = 0; j < patterns2.Length; j++)
////            {
////                result[i * patterns2.Length + j] = patterns1[i] + connectString + patterns2[j];
////            }
////        }
////        return (result);
////    }
////
////
////    public String[] GetAllDateTimePatterns()
////    {
////        ArrayList results = new ArrayList( DEFAULT_ALL_DATETIMES_SIZE );
////
////        for(int i = 0; i < DateTimeFormat.allStandardFormats.Length; i++)
////        {
////            String[] strings = GetAllDateTimePatterns( DateTimeFormat.allStandardFormats[i] );
////            for(int j = 0; j < strings.Length; j++)
////            {
////                results.Add( strings[j] );
////            }
////        }
////        String[] value = new String[results.Count];
////        results.CopyTo( 0, value, 0, results.Count );
////        return (value);
////    }
////
////
////    public String[] GetAllDateTimePatterns( char format )
////    {
////        String[] result = null;
////
////        switch(format)
////        {
////            case 'd':
////                result = ClonedAllShortDatePatterns;
////                break;
////            case 'D':
////                result = ClonedAllLongDatePatterns;
////                break;
////            case 'f':
////                result = GetCombinedPatterns( ClonedAllLongDatePatterns, ClonedAllShortTimePatterns, " " );
////                break;
////            case 'F':
////                result = GetCombinedPatterns( ClonedAllLongDatePatterns, ClonedAllLongTimePatterns, " " );
////                break;
////            case 'g':
////                result = GetCombinedPatterns( ClonedAllShortDatePatterns, ClonedAllShortTimePatterns, " " );
////                break;
////            case 'G':
////                result = GetCombinedPatterns( ClonedAllShortDatePatterns, ClonedAllLongTimePatterns, " " );
////                break;
////            case 'm':
////            case 'M':
////                result = new String[] { MonthDayPattern };
////                break;
////            case 'o':
////            case 'O':
////                result = new String[] { DateTimeFormat.RoundtripFormat };
////                break;
////            case 'r':
////            case 'R':
////                result = new String[] { rfc1123Pattern };
////                break;
////            case 's':
////                result = new String[] { sortableDateTimePattern };
////                break;
////            case 't':
////                result = ClonedAllShortTimePatterns;
////                break;
////            case 'T':
////                result = ClonedAllLongTimePatterns;
////                break;
////            case 'u':
////                result = new String[] { UniversalSortableDateTimePattern };
////                break;
////            case 'U':
////                result = GetCombinedPatterns( ClonedAllLongDatePatterns, ClonedAllLongTimePatterns, " " );
////                break;
////            case 'y':
////            case 'Y':
////                result = ClonedAllYearMonthPatterns;
////                break;
////            default:
////                throw new ArgumentException( Environment.GetResourceString( "Argument_BadFormatSpecifier" ), "format" );
////        }
////        return (result);
////    }
    
    
        public String GetDayName( DayOfWeek dayofweek )
        {
            switch(dayofweek)
            {
                case DayOfWeek.Sunday   : return "Sunday";
                case DayOfWeek.Monday   : return "Monday";
                case DayOfWeek.Tuesday  : return "Tuesday";
                case DayOfWeek.Wednesday: return "Wednesday";
                case DayOfWeek.Thursday : return "Thursday";
                case DayOfWeek.Friday   : return "Friday";
                case DayOfWeek.Saturday : return "Saturday";
            }

            return String.Empty;
        }

////    public String GetDayName( DayOfWeek dayofweek )
////    {
////        if((int)dayofweek < 0 || (int)dayofweek > 6)
////        {
////            throw new ArgumentOutOfRangeException(
////                "dayofweek", String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
////                DayOfWeek.Sunday, DayOfWeek.Saturday ) );
////        }
////
////        return (GetDayOfWeekNames()[(int)dayofweek]);
////    }
    
    
    
        public String GetAbbreviatedMonthName( int month )
        {
            if(month < 1 || month > 13)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException(
                    "month", String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
                    1, 13 ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
            return (GetAbbreviatedMonthNames()[month - 1]);
        }
    
    
        public String GetMonthName( int month )
        {
            if(month < 1 || month > 13)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException(
                    "month", String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "ArgumentOutOfRange_Range" ),
                    1, 13 ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }
            return (GetMonthNames()[month - 1]);
        }
    
    
    
////    internal String[] ClonedAllYearMonthPatterns
////    {
////        get
////        {
////            if(allYearMonthPatterns == null)
////            {
////                String[] temp = null;
////
////                if(!m_isDefaultCalendar)
////                {
////                    BCLDebug.Assert( Calendar.ID > 0, "[DateTimeFormatInfo.AllYearMonthPatternsNeedClone] Expected Calendar.ID > 0" );
////
////                    temp = CalendarTable.Default.SYEARMONTH( Calendar.ID );
////
////                    // In the data table, some calendars store null for long date pattern.
////                    // This means that we have to use the default format of the user culture for that calendar.
////                    // So, get the pattern from culture.
////                    if(temp == null)
////                    {
////                        temp = this.m_cultureTableRecord.SYEARMONTHS;
////                    }
////                }
////                else
////                {
////                    temp = this.m_cultureTableRecord.SYEARMONTHS;
////                }
////
////                System.Threading.Thread.MemoryBarrier();
////                SetDefaultPatternAsFirstItem( temp, YearMonthPattern );
////                allYearMonthPatterns = temp;
////            }
////
////            if(allYearMonthPatterns[0].Equals( YearMonthPattern ))
////            {
////                return (String[])allYearMonthPatterns.Clone();
////            }
////
////            return AddDefaultFormat( allYearMonthPatterns, YearMonthPattern );
////        }
////    }
////
////
////    // NOTE: Clone this string array if you want to return it to user.  Otherwise, you are returnning a writable cache copy.
////    internal String[] ClonedAllShortDatePatterns
////    {
////        get
////        {
////            if(allShortDatePatterns == null)
////            {
////                String[] temp = null;
////                if(!m_isDefaultCalendar)
////                {
////                    BCLDebug.Assert( Calendar.ID > 0, "[DateTimeFormatInfo.AllShortDatePatternsNeedClone] Expected Calendar.ID > 0" );
////                    temp = CalendarTable.Default.SSHORTDATE( Calendar.ID );
////                    // In the data table, some calendars store null for long date pattern.
////                    // This means that we have to use the default format of the user culture for that calendar.
////                    // So, get the pattern from culture.
////                    if(temp == null)
////                    {
////                        temp = this.m_cultureTableRecord.SSHORTDATES;
////                    }
////                }
////                else
////                {
////                    temp = this.m_cultureTableRecord.SSHORTDATES;
////                }
////                System.Threading.Thread.MemoryBarrier();
////                SetDefaultPatternAsFirstItem( temp, ShortDatePattern );
////                allShortDatePatterns = temp;
////            }
////
////            if(allShortDatePatterns[0].Equals( ShortDatePattern ))
////            {
////                return (String[])allShortDatePatterns.Clone();
////            }
////
////            return (AddDefaultFormat( allShortDatePatterns, ShortDatePattern ));
////        }
////    }
////
////    // NOTE: Clone this string array if you want to return it to user.  Otherwise, you are returnning a writable cache copy.
////    internal String[] ClonedAllLongDatePatterns
////    {
////        get
////        {
////            if(allLongDatePatterns == null)
////            {
////                String[] temp = null;
////                if(!m_isDefaultCalendar)
////                {
////                    BCLDebug.Assert( Calendar.ID > 0, "[DateTimeFormatInfo.AllLongDatePatternsNeedClone] Expected Calendar.ID > 0" );
////                    temp = CalendarTable.Default.SLONGDATE( Calendar.ID );
////                    // In the data table, some calendars store null for long date pattern.
////                    // This means that we have to use the default format of the user culture for that calendar.
////                    // So, get the pattern from culture.
////                    if(temp == null)
////                    {
////                        temp = this.m_cultureTableRecord.SLONGDATES;
////                    }
////                }
////                else
////                {
////                    temp = this.m_cultureTableRecord.SLONGDATES;
////                }
////                System.Threading.Thread.MemoryBarrier();
////                SetDefaultPatternAsFirstItem( temp, LongDatePattern );
////                allLongDatePatterns = temp;
////            }
////
////            if(allLongDatePatterns[0].Equals( LongDatePattern ))
////            {
////                return (String[])allLongDatePatterns.Clone();
////            }
////
////            return (AddDefaultFormat( allLongDatePatterns, LongDatePattern ));
////        }
////    }
////
////    internal void SetDefaultPatternAsFirstItem( string[] patterns, string defaultPattern )
////    {
////        if(patterns == null) { return; }
////
////        for(int i = 0; i < patterns.Length; i++)
////        {
////            if(patterns[i].Equals( defaultPattern ))
////            {
////                if(i != 0)
////                {
////                    // default date is already exist in the list. set it as first item in the list.
////                    string temp = patterns[i];
////
////                    for(int j = i; j > 0; j--)
////                    {
////                        patterns[j] = patterns[j - 1];
////                    }
////
////                    patterns[0] = temp;
////                }
////
////                return;
////            }
////        }
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    // The default LongDatePattern is not in the standard list.  Add it as the first item.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////    internal string[] AddDefaultFormat( string[] datePatterns, string defaultDateFormat )
////    {
////        string[] updatedPatterns = new string[datePatterns.Length + 1];
////        updatedPatterns[0] = defaultDateFormat;
////        Array.Copy( datePatterns, 0, updatedPatterns, 1, datePatterns.Length );
////        // We need to scan the non-standard longDatePattern.
////        m_scanDateWords = true;
////        return updatedPatterns;
////    }
////
////    // NOTE: Clone this string array if you want to return it to user.  Otherwise, you are returnning a writable cache copy.
////    internal String[] ClonedAllShortTimePatterns
////    {
////        get
////        {
////            if(allShortTimePatterns == null)
////            {
////                allShortTimePatterns = this.m_cultureTableRecord.SSHORTTIMES;
////                SetDefaultPatternAsFirstItem( allShortTimePatterns, ShortTimePattern );
////            }
////
////            if(allShortTimePatterns[0].Equals( ShortTimePattern ))
////            {
////                return (String[])allShortTimePatterns.Clone();
////            }
////
////            return (AddDefaultFormat( allShortTimePatterns, ShortTimePattern ));
////        }
////    }
////
////    internal String[] ClonedAllLongTimePatterns
////    {
////        get
////        {
////            if(allLongTimePatterns == null)
////            {
////                allLongTimePatterns = this.m_cultureTableRecord.STIMEFORMATS;
////                SetDefaultPatternAsFirstItem( allLongTimePatterns, LongTimePattern );
////            }
////            if(allLongTimePatterns[0].Equals( LongTimePattern ))
////            {
////                return (String[])allLongTimePatterns.Clone();
////            }
////
////            return (AddDefaultFormat( allLongTimePatterns, LongTimePattern ));
////        }
////    }
////
////    //
////    // The known word used in date pattern for this culture.  E.g. Spanish cultures often
////    // have 'de' in their long date pattern.
////    // This is used by DateTime.Parse() to decide if a word should be ignored or not.
////    //
////    internal String[] DateWords
////    {
////        get
////        {
////            if(m_dateWords == null)
////            {
////                m_dateWords = this.m_cultureTableRecord.SDATEWORDS;
////            }
////
////            return (m_dateWords);
////        }
////    }
////
////
////    public static DateTimeFormatInfo ReadOnly( DateTimeFormatInfo dtfi )
////    {
////        if(dtfi == null)
////        {
////            throw new ArgumentNullException( "dtfi",
////                Environment.GetResourceString( "ArgumentNull_Obj" ) );
////        }
////        if(dtfi.IsReadOnly)
////        {
////            return (dtfi);
////        }
////        DateTimeFormatInfo info = (DateTimeFormatInfo)(dtfi.MemberwiseClone());
////        info.Calendar = Calendar.ReadOnly( info.Calendar );
////        info.m_isReadOnly = true;
////        return (info);
////    }
////
////
////    public bool IsReadOnly
////    {
////        get
////        {
////            return (m_isReadOnly);
////        }
////    }
////
////    private static int CalendarIdToCultureId( int calendarId )
////    {
////        switch(calendarId)
////        {
////            case Calendar.CAL_GREGORIAN_US:
////                return 0x0429;             // "fa-IR" Iran
////
////            case Calendar.CAL_JAPAN:
////                return 0x0411;             // "ja-JP" Japan
////
////            case Calendar.CAL_TAIWAN:
////                return 0x0404;             // zh-TW Taiwan
////
////            case Calendar.CAL_KOREA:
////                return 0x0412;             // "ko-KR" Korea
////
////            case Calendar.CAL_HIJRI:
////            case Calendar.CAL_GREGORIAN_ARABIC:
////            case Calendar.CAL_UMALQURA:
////                return 0x0401;             // "ar-SA" Saudi Arabia
////
////            case Calendar.CAL_THAI:
////                return 0x041e;             // "th-TH" Thailand
////
////            case Calendar.CAL_HEBREW:
////                return 0x040d;             // "he-IL" Israel
////
////            case Calendar.CAL_GREGORIAN_ME_FRENCH:
////                return 0x1401;             // "ar-DZ" Algeria
////
////            case Calendar.CAL_GREGORIAN_XLIT_ENGLISH:
////            case Calendar.CAL_GREGORIAN_XLIT_FRENCH:
////                return 0x0801;             // "ar-IQ"; Iraq
////
////            default:
////                BCLDebug.Assert( false,
////                            "[DateTimeFormatInfo.CalendarIdToCultureId] we shouldn't come here." );
////                break;
////        }
////
////        return 0;
////    }
////
////    //
////    // GetCalendarNativeNameFallback is used when we got empty string native calendar name from the culture data
////    // in the case of the custom cultures. 
////    // GetCalendarNativeNameFallback is getting the name from the framework data from some specific cultures
////    // for example to get the native name for Hijri calendar we use ar-SA culture to get that name.
////    //
////    private string GetCalendarNativeNameFallback( int calendarId )
////    {
////        BCLDebug.Assert( calendarId != Calendar.CAL_GREGORIAN,
////                    "[DateTimeFormatInfo.GetCalendarNativeNameFallback] Unexpected Gregorian localized calendar." );
////
////        if(m_calendarNativeNames == null)
////        {
////            lock(InternalSyncObject)
////            {
////                if(m_calendarNativeNames == null)
////                    m_calendarNativeNames = new Hashtable();
////            }
////        }
////
////        BCLDebug.Assert( m_calendarNativeNames != null,
////                    "[DateTimeFormatInfo.GetCalendarNativeNameFallback] m_calendarNativeNames should be valid" );
////
////        string temp = (string)m_calendarNativeNames[calendarId];
////        if(temp != null)
////            return temp;
////
////        string name = String.Empty;
////
////        int cultureId = CalendarIdToCultureId( calendarId );
////
////        if(cultureId != 0)
////        {
////            String[] values = new CultureTableRecord( cultureId, false ).SNATIVECALNAMES;
////
////            BCLDebug.Assert( calendar.ID >= 1, "[DateTimeFormatInfo.GetCalendarNativeNameFallback] calendar.ID >= 1" );
////
////            int id = calendar.ID - 1;
////
////            // The element 0 stored the name for calendar ID 1 (since there is no calendar ID 0)
////            if(id < values.Length)
////            {
////                // If U+FEFF is stored, it means that no information for that calendar is available.
////                if(values[id].Length > 0 && values[id][0] != '\xfeff')
////                    name = values[id];
////            }
////        }
////
////        lock(InternalSyncObject)
////        {
////            if(m_calendarNativeNames[calendarId] == null)
////                m_calendarNativeNames[calendarId] = name;
////        }
////
////        return name;
////    }
////
////    // Return the native name for the calendar in DTFI.Calendar.  The native name is referred to
////    // the culture used to create the DTFI.  E.g. in the following example, the native language is Japanese.
////    // DateTimeFormatInfo dtfi = new CutlureInfo("ja-JP", false).DateTimeFormat.Calendar = new JapaneseCalendar();
////    // String nativeName = dtfi.NativeCalendarName; // Get the Japanese name for the Japanese calendar.
////    // DateTimeFormatInfo dtfi = new CutlureInfo("ja-JP", false).DateTimeFormat.Calendar = new GregorianCalendar(GregorianCalendarTypes.Localized);
////    // String nativeName = dtfi.NativeCalendarName; // Get the Japanese name for the Gregorian calendar.
////
////
////    public String NativeCalendarName
////    {
////        get
////        {
////
////            if(Calendar.ID == Calendar.CAL_TAIWAN)
////            {
////                //
////                // Specail case for Taiwan calenadr.
////                //
////
////                // In non-Taiwan machine, the following call will return null.
////                // 0x0404 is the locale ID for Taiwan.
////                String result = GetCalendarInfo( 0x0404, Calendar.CAL_TAIWAN, CAL_SCALNAME );
////                if(result == null)
////                {
////                    // 0x0404 is the locale ID for Taiwan.
////                    // In Win9x, the Win32 GetCalendarInfo() does not support CAL_SCALNAME.  In that case,
////                    // fallback to use the era name.
////                    result = CalendarTable.nativeGetEraName( 0x0404, Calendar.CAL_TAIWAN );
////                    if(result == null)
////                    {
////                        // In non-CHT platform, the previous two Win32 calls will fail.  Just return an empty string.
////                        result = String.Empty;
////                    }
////                }
////                return (result);
////            }
////            else
////            {
////                String[] values = this.m_cultureTableRecord.SNATIVECALNAMES;
////                BCLDebug.Assert( calendar.ID >= 1, "calendar.ID >= 1" );
////                int id = calendar.ID - 1;
////                // The element 0 stored the name for calendar ID 1 (since there is no calendar ID 0)
////                if(id < values.Length)
////                {
////                    if(values[id].Length > 0)
////                    {
////                        // If U+FEFF is stored, it means that no information for that calendar is available.
////                        if(values[id][0] != '\xfeff')
////                            return (values[id]);
////                    }
////                    else
////                    {
////                        // 
////                        // Empty string means we have custom culture. Then try the fallback.
////                        //
////                        BCLDebug.Assert( this.m_cultureTableRecord.IsCustomCulture, "[DateTimeFormatInfo.NativeCalendarName] Expected custom culture" );
////                        return GetCalendarNativeNameFallback( calendar.ID );
////                    }
////                }
////            }
////            // If data is not available, just return an empty string.
////            return (String.Empty);
////        }
////    }
////
////    //
////    // Used by custom cultures and others to set the list of available formats. Note that none of them are
////    // explicitly used unless someone calls GetAllDateTimePatterns and subsequently uses one of the items
////    // from the list.
////    //
////    // Most of the format characters that can be used in GetAllDateTimePatterns are
////    // not really needed since they are one of the following:
////    //
////    //  r/R/s/u     locale-independent constants -- cannot be changed!
////    //  m/M/y/Y     fields with a single string in them -- that can be set through props directly
////    //  f/F/g/G/U   derived fields based on combinations of various of the below formats
////    //
////    // NOTE: No special validation is done here beyond what is done when the actual respective fields
////    // are used (what would be the point of disallowing here what we allow in the appropriate property?)
////    //
////    // WARNING: If more validation is ever done in one place, it should be done in the other.
////    //
////
////    public void SetAllDateTimePatterns( String[] patterns, char format )
////    {
////        VerifyWritable();
////        if(patterns == null)
////        {
////            throw new ArgumentNullException( "patterns",
////                Environment.GetResourceString( "ArgumentNull_Array" ) );
////        }
////
////        if(patterns.Length == 0)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Arg_ArrayZeroError" ), "patterns" );
////        }
////
////        for(int i = 0; i < patterns.Length; i++)
////        {
////            if(patterns[i] == null)
////            {
////                throw new ArgumentNullException( Environment.GetResourceString( "ArgumentNull_ArrayValue" ) );
////            }
////        }
////
////        switch(format)
////        {
////            case 'd':
////                ShortDatePattern = patterns[0];
////                this.allShortDatePatterns = patterns;
////                break;
////
////            case 'D':
////                LongDatePattern = patterns[0];
////                this.allLongDatePatterns = patterns;
////                break;
////
////            case 't':
////                ShortTimePattern = patterns[0];
////                this.allShortTimePatterns = patterns;
////                break;
////
////            case 'T':
////                LongTimePattern = patterns[0];
////                this.allLongTimePatterns = patterns;
////                break;
////
////            case 'y':
////            case 'Y':
////                yearMonthPattern = patterns[0];
////                this.allYearMonthPatterns = patterns;
////                break;
////
////            default:
////                throw new ArgumentException( Environment.GetResourceString( "Argument_BadFormatSpecifier" ), "format" );
////        }
////        return;
////    }
////
////    public String[] AbbreviatedMonthGenitiveNames
////    {
////        get
////        {
////            return ((String[])internalGetGenitiveMonthNames( true ).Clone());
////        }
////
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_Array" ) );
////            }
////            if(value.Length != 13)
////            {
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_InvalidArrayLength" ), 13 ), "value" );
////            }
////            CheckNullValue( value, value.Length - 1 );
////            ClearTokenHashTable( true );
////            this.m_genitiveAbbreviatedMonthNames = value;
////        }
////    }
////
////    public String[] MonthGenitiveNames
////    {
////        get
////        {
////            return ((String[])internalGetGenitiveMonthNames( false ).Clone());
////        }
////
////        set
////        {
////            VerifyWritable();
////            if(value == null)
////            {
////                throw new ArgumentNullException( "value",
////                    Environment.GetResourceString( "ArgumentNull_Array" ) );
////            }
////            if(value.Length != 13)
////            {
////                throw new ArgumentException( String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Argument_InvalidArrayLength" ), 13 ), "value" );
////            }
////            CheckNullValue( value, value.Length - 1 );
////            genitiveMonthNames = value;
////            ClearTokenHashTable( true );
////        }
////    }
////
////
////    //
////    // Get suitable CompareInfo from current DTFI object.
////    //
////
////    internal CompareInfo CompareInfo
////    {
////        get
////        {
////            if(m_compareInfo == null)
////            {
////                // We use the regular GetCompareInfo here to make sure the created CompareInfo object is stored in the
////                // CompareInfo cache. otherwise we would just create CompareInfo using m_cultureTableRecord.
////                if(CultureTableRecord.IsCustomCultureId( CultureId ))
////                    m_compareInfo = CompareInfo.GetCompareInfo( (int)m_cultureTableRecord.ICOMPAREINFO );
////                else
////                    m_compareInfo = CompareInfo.GetCompareInfo( CultureId );
////            }
////
////            return m_compareInfo;
////        }
////    }
////
////
////    private void VerifyWritable()
////    {
////        if(m_isReadOnly)
////        {
////            throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_ReadOnly" ) );
////        }
////    }
////
////    //private const DateTimeStyles InvalidDateTimeStyles = unchecked((DateTimeStyles)0xFFFFFF00);
////    private const DateTimeStyles InvalidDateTimeStyles = ~(DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite
////                                                           | DateTimeStyles.AllowInnerWhite | DateTimeStyles.NoCurrentDateDefault
////                                                           | DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal
////                                                           | DateTimeStyles.AssumeUniversal | DateTimeStyles.RoundtripKind);
    
        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        internal static extern void ValidateStyles( DateTimeStyles style         ,
                                                    String         parameterName );
////    {
////        if((style & InvalidDateTimeStyles) != 0)
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidDateTimeStyles" ), parameterName );
////        }
////        if(((style & (DateTimeStyles.AssumeLocal)) != 0) && ((style & (DateTimeStyles.AssumeUniversal)) != 0))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_ConflictingDateTimeStyles" ), parameterName );
////        }
////        if(((style & DateTimeStyles.RoundtripKind) != 0)
////            && ((style & (DateTimeStyles.AssumeLocal | DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)) != 0))
////        {
////            throw new ArgumentException( Environment.GetResourceString( "Argument_ConflictingDateTimeRoundtripStyles" ), parameterName );
////        }
////    }
////
////    //
////    // Actions: Return the internal flag used in formatting and parsing.
////    //  The flag can be used to indicate things like if genitive forms is used in this DTFi, or if leap year gets different month names.
////    //
////    internal DateTimeFormatFlags FormatFlags
////    {
////        get
////        {
////            if(formatFlags == DateTimeFormatFlags.NotInitialized)
////            {
////                if(m_scanDateWords || m_cultureTableRecord.IsSynthetic)
////                {
////                    formatFlags = DateTimeFormatFlags.None;
////                    formatFlags |= (DateTimeFormatFlags)DateTimeFormatInfoScanner.GetFormatFlagGenitiveMonth( MonthNames, internalGetGenitiveMonthNames( false ), AbbreviatedMonthNames, internalGetGenitiveMonthNames( true ) );
////                    formatFlags |= (DateTimeFormatFlags)DateTimeFormatInfoScanner.GetFormatFlagUseSpaceInMonthNames( MonthNames, internalGetGenitiveMonthNames( false ), AbbreviatedMonthNames, internalGetGenitiveMonthNames( true ) );
////                    formatFlags |= (DateTimeFormatFlags)DateTimeFormatInfoScanner.GetFormatFlagUseSpaceInDayNames( DayNames, AbbreviatedDayNames );
////                    formatFlags |= (DateTimeFormatFlags)DateTimeFormatInfoScanner.GetFormatFlagUseHebrewCalendar( (int)Calendar.ID );
////                }
////                else
////                {
////                    // No customized format, we can use the pre-calculated formatflags.
////                    if(m_isDefaultCalendar)
////                    {
////                        formatFlags = this.m_cultureTableRecord.IFORMATFLAGS;
////                    }
////                    else
////                    {
////                        BCLDebug.Assert( Calendar.ID > 0, "[DateTimeFormatInfo.FormatFlags] Expected Calendar.ID > 0" );
////                        formatFlags = (DateTimeFormatFlags)CalendarTable.Default.IFORMATFLAGS( Calendar.ID );
////                    }
////                }
////            }
////            return (formatFlags);
////        }
////    }
////
////    internal Boolean HasForceTwoDigitYears
////    {
////        get
////        {
////            switch(calendar.ID)
////            {
////                // Add a special case for Japanese and Taiwan.
////                // If is y/yy, do not get (year % 100). "y" will print
////                // year without leading zero.  "yy" will print year with two-digit in leading zero.
////                // If pattern is yyy/yyyy/..., print year value with two-digit in leading zero.
////                // So year 5 is "05", and year 125 is "125".
////                // The reason for not doing (year % 100) is for Taiwan calendar.
////                // If year 125, then output 125 and not 25.
////                // Note: OS uses "yyyy" for Taiwan calendar by default.
////                case (Calendar.CAL_JAPAN):
////                case (Calendar.CAL_TAIWAN):
////                    return true;
////            }
////            return false;
////        }
////    }
////
////    // Returns whether the YearMonthAdjustment function has any fix-up work to do for this culture/calendar.
////    internal Boolean HasYearMonthAdjustment
////    {
////        get
////        {
////            return ((FormatFlags & DateTimeFormatFlags.UseHebrewRule) != 0);
////        }
////    }
////
////    // This is a callback that the parser can make back into the DTFI to let it fiddle with special
////    // cases associated with that culture or calendar. Currently this only has special cases for
////    // the Hebrew calendar, but this could be extended to other cultures.
////    //
////    // The return value is whether the year and month are actually valid for this calendar.
////    internal Boolean YearMonthAdjustment( ref int year, ref int month, Boolean parsedMonthName )
////    {
////        if((FormatFlags & DateTimeFormatFlags.UseHebrewRule) != 0)
////        {
////
////            // Special rules to fix up the Hebrew year/month
////
////            // When formatting, we only format up to the hundred digit of the Hebrew year, although Hebrew year is now over 5000.
////            // E.g. if the year is 5763, we only format as 763.
////            if(year < 1000)
////            {
////                year += 5000;
////            }
////
////            // Because we need to calculate leap year, we should fall out now for an invalid year.
////            if(year < Calendar.GetYear( Calendar.MinSupportedDateTime ) || year > Calendar.GetYear( Calendar.MaxSupportedDateTime ))
////            {
////                return false;
////            }
////
////            // To handle leap months, the set of month names in the symbol table does not always correspond to the numbers.
////            // For non-leap years, month 7 (Adar Bet) is not present, so we need to make using this month invalid and
////            // shuffle the other months down.
////            if(parsedMonthName)
////            {
////                if(!Calendar.IsLeapYear( year ))
////                {
////                    if(month >= 8)
////                    {
////                        month--;
////                    }
////                    else if(month == 7)
////                    {
////                        return false;
////                    }
////                }
////            }
////        }
////        return true;
////    }
////
////    //
////    // DateTimeFormatInfo tokenizer.  This is used by DateTime.Parse() to break input string into tokens.
////    //
////    [NonSerialized]
////    TokenHashValue[] m_dtfiTokenHash;
////    // The flag to indicate if we need to re-generate date words & formatflags.
////    [NonSerialized]
////    bool m_scanDateWords = false;
////
////
////    private const int TOKEN_HASH_SIZE = 199;
////    private const int SECOND_PRIME = 197;
////    private const String alternativeDateSeparator = "-";
////    private const String invariantDateSeparator = "/";
////    private const String invariantTimeSeparator = ":";
////
////    //
////    // Year/Month/Day suffixes
////    //
////    internal const String CJKYearSuff = "\u5e74";
////    internal const String CJKMonthSuff = "\u6708";
////    internal const String CJKDaySuff = "\u65e5";
////
////    internal const String KoreanYearSuff = "\ub144";
////    internal const String KoreanMonthSuff = "\uc6d4";
////    internal const String KoreanDaySuff = "\uc77c";
////
////    internal const String KoreanHourSuff = "\uc2dc";
////    internal const String KoreanMinuteSuff = "\ubd84";
////    internal const String KoreanSecondSuff = "\ucd08";
////
////    internal const String CJKHourSuff = "\u6642";
////    internal const String ChineseHourSuff = "\u65f6";
////
////    internal const String CJKMinuteSuff = "\u5206";
////    internal const String CJKSecondSuff = "\u79d2";
////
////    internal const String LocalTimeMark = "T";
////
////    internal const String KoreanLangName = "ko";
////    internal const String JapaneseLangName = "ja";
////    internal const String EnglishLangName = "en";
////
////    private static DateTimeFormatInfo m_jajpDTFI = null;
////    private static DateTimeFormatInfo m_zhtwDTFI = null;
////
////    //
////    // Create a Japanese DTFI which uses JapaneseCalendar.  This is used to parse
////    // date string with Japanese era name correctly even when the supplied DTFI
////    // does not use Japanese calendar.
////    // The created instance is stored in global m_jajpDTFI.
////    //
////    internal static DateTimeFormatInfo GetJapaneseCalendarDTFI()
////    {
////        DateTimeFormatInfo temp = m_jajpDTFI;
////        if(temp == null)
////        {
////            temp = new CultureInfo( "ja-JP", false ).DateTimeFormat;
////            temp.Calendar = JapaneseCalendar.GetDefaultInstance();
////            m_jajpDTFI = temp;
////        }
////        return (temp);
////    }
////
////
////    //
////    // Create a Taiwan DTFI which uses TaiwanCalendar.  This is used to parse
////    // date string with era name correctly even when the supplied DTFI
////    // does not use Taiwan calendar.
////    // The created instance is stored in global m_zhtwDTFI.
////    //
////    internal static DateTimeFormatInfo GetTaiwanCalendarDTFI()
////    {
////        DateTimeFormatInfo temp = m_zhtwDTFI;
////        if(temp == null)
////        {
////            temp = new CultureInfo( "zh-TW", false ).DateTimeFormat;
////            temp.Calendar = TaiwanCalendar.GetDefaultInstance();
////            m_zhtwDTFI = temp;
////        }
////        return (temp);
////    }
////
////
////    // DTFI properties should call this when the setter are called.
////    private void ClearTokenHashTable( bool scanDateWords )
////    {
////        m_dtfiTokenHash = null;
////        m_dateWords = null;
////        // The properties in this class have been changed, and we need to re-generate
////        // date words & format flags, instead of using the table values.
////        m_scanDateWords = scanDateWords;
////        formatFlags = DateTimeFormatFlags.NotInitialized;
////    }
////
////    internal TokenHashValue[] CreateTokenHashTable()
////    {
////        TokenHashValue[] temp = m_dtfiTokenHash;
////        if(temp == null)
////        {
////            temp = new TokenHashValue[TOKEN_HASH_SIZE];
////
////            InsertHash( temp, ",", TokenType.IgnorableSymbol, 0 );
////            InsertHash( temp, ".", TokenType.IgnorableSymbol, 0 );
////
////            InsertHash( temp, this.TimeSeparator, TokenType.SEP_Time, 0 );
////            InsertHash( temp, this.AMDesignator, TokenType.SEP_Am | TokenType.Am, 0 );
////            InsertHash( temp, this.PMDesignator, TokenType.SEP_Pm | TokenType.Pm, 1 );
////
////            if(CultureName.Equals( "sq-AL" ))
////            {
////                // Algerian allows time formats like "12:00.PD"
////                InsertHash( temp, "." + this.AMDesignator, TokenType.SEP_Am | TokenType.Am, 0 );
////                InsertHash( temp, "." + this.PMDesignator, TokenType.SEP_Pm | TokenType.Pm, 1 );
////            }
////
////            // CJK suffix
////            InsertHash( temp, CJKYearSuff, TokenType.SEP_YearSuff, 0 );
////            InsertHash( temp, KoreanYearSuff, TokenType.SEP_YearSuff, 0 );
////            InsertHash( temp, CJKMonthSuff, TokenType.SEP_MonthSuff, 0 );
////            InsertHash( temp, KoreanMonthSuff, TokenType.SEP_MonthSuff, 0 );
////            InsertHash( temp, CJKDaySuff, TokenType.SEP_DaySuff, 0 );
////            InsertHash( temp, KoreanDaySuff, TokenType.SEP_DaySuff, 0 );
////
////            InsertHash( temp, CJKHourSuff, TokenType.SEP_HourSuff, 0 );
////            InsertHash( temp, ChineseHourSuff, TokenType.SEP_HourSuff, 0 );
////            InsertHash( temp, CJKMinuteSuff, TokenType.SEP_MinuteSuff, 0 );
////            InsertHash( temp, CJKSecondSuff, TokenType.SEP_SecondSuff, 0 );
////
////            if(LanguageName.Equals( KoreanLangName ))
////            {
////                // Korean suffix
////                InsertHash( temp, KoreanHourSuff, TokenType.SEP_HourSuff, 0 );
////                InsertHash( temp, KoreanMinuteSuff, TokenType.SEP_MinuteSuff, 0 );
////                InsertHash( temp, KoreanSecondSuff, TokenType.SEP_SecondSuff, 0 );
////            }
////
////            String[] dateWords = null;
////            DateTimeFormatInfoScanner scanner = null;
////
////            // Get the all of the long date pattern.  The getter will check if the default LongDatePattern
////            // is in the standard list or not.  If not, m_scanDateWords will be true, and we will
////            // need to scan the date words.
////            // Note that dateWords is used as a temp buffer here.  It will be reset as the real date words later.
////            if(!m_scanDateWords)
////            {
////                dateWords = ClonedAllLongDatePatterns;
////            }
////            if(m_scanDateWords || m_cultureTableRecord.IsSynthetic)
////            {
////                scanner = new DateTimeFormatInfoScanner();
////                // Enumarate all LongDatePatterns, and get the DateWords and scan for month postfix.
////                m_dateWords = dateWords = scanner.GetDateWordsOfDTFI( this );
////                // Ensure the formatflags is initialized.
////                DateTimeFormatFlags flag = FormatFlags;
////                m_scanDateWords = false;
////            }
////            else
////            {
////                // Use the table value.
////                dateWords = this.DateWords; ;
////            }
////
////            // For some cultures, the date separator works more like a comma, being allowed before or after any date part.
////            // In this cultures, we do not use normal date separator since we disallow date separator after a date terminal state.
////            // This is determinted in DateTimeFormatInfoScanner.  Use this flag to determine if we should treat date separator as ignorable symbol.
////            bool useDateSepAsIgnorableSymbol = false;
////
////            String monthPostfix = null;
////            if(dateWords != null)
////            {
////                // There are DateWords.  It could be a real date word (such as "de"), or a monthPostfix.
////                // The monthPostfix starts with '\xfffe' (MonthPostfixChar), followed by the real monthPostfix.
////                for(int i = 0; i < dateWords.Length; i++)
////                {
////                    switch(dateWords[i][0])
////                    {
////                        // This is a month postfix
////                        case DateTimeFormatInfoScanner.MonthPostfixChar:
////                            // Get the real month postfix.
////                            monthPostfix = dateWords[i].Substring( 1 );
////                            // Add the month name + postfix into the token.
////                            AddMonthNames( temp, monthPostfix );
////                            break;
////                        case DateTimeFormatInfoScanner.IgnorableSymbolChar:
////                            String symbol = dateWords[i].Substring( 1 );
////                            InsertHash( temp, symbol, TokenType.IgnorableSymbol, 0 );
////                            if(this.DateSeparator.Trim( null ).Equals( symbol ))
////                            {
////                                // The date separator is the same as the ingorable symbol.
////                                useDateSepAsIgnorableSymbol = true;
////                            }
////                            break;
////                        default:
////                            InsertHash( temp, dateWords[i], TokenType.DateWordToken, 0 );
////                            if(CultureName.Equals( "eu-ES" ))
////                            {
////                                // Basque has date words with leading dots
////                                InsertHash( temp, "." + dateWords[i], TokenType.DateWordToken, 0 );
////                            }
////                            break;
////                    }
////                }
////            }
////
////            if(!useDateSepAsIgnorableSymbol)
////            {
////                // Use the normal date separator.
////                InsertHash( temp, this.DateSeparator, TokenType.SEP_Date, 0 );
////            }
////            // Add the regular month names.
////            AddMonthNames( temp, null );
////
////            // Add the abbreviated month names.
////            for(int i = 1; i <= 13; i++)
////            {
////                InsertHash( temp, GetAbbreviatedMonthName( i ), TokenType.MonthToken, i );
////            }
////
////
////            if(CultureName.Equals( "gl-ES" ))
////            {
////                //
////                // Special case for gl-ES.  It has a potential incorrect format in year/month: MMMM'de 'yyyy.
////                // It probably has to be MMMM' de 'yyyy.
////                //
////
////                // We keep this so that we can still parse dates formatted in the older version.
////                for(int i = 1; i <= 13; i++)
////                {
////                    String str;
////                    //str = internalGetMonthName(i, MonthNameStyles.Regular, false);
////                    // We have to call public methods here to work with inherited DTFI.
////                    // Insert the month name first, so that they are at the front of abbrevaited
////                    // month names.
////                    str = GetMonthName( i );
////                    if(str.Length > 0)
////                    {
////                        // Insert the month name with the postfix first, so it can be matched first.
////                        InsertHash( temp, str + "de", TokenType.MonthToken, i );
////                    }
////                }
////            }
////
////            if((FormatFlags & DateTimeFormatFlags.UseGenitiveMonth) != 0)
////            {
////                for(int i = 1; i <= 13; i++)
////                {
////                    String str;
////                    str = internalGetMonthName( i, MonthNameStyles.Genitive, false );
////                    InsertHash( temp, str, TokenType.MonthToken, i );
////                }
////            }
////
////            if((FormatFlags & DateTimeFormatFlags.UseLeapYearMonth) != 0)
////            {
////                for(int i = 1; i <= 13; i++)
////                {
////                    String str;
////                    str = internalGetMonthName( i, MonthNameStyles.LeapYear, false );
////                    InsertHash( temp, str, TokenType.MonthToken, i );
////                }
////            }
////
////            for(int i = 0; i < 7; i++)
////            {
////                //String str = GetDayOfWeekNames()[i];
////                // We have to call public methods here to work with inherited DTFI.
////                String str = GetDayName( (DayOfWeek)i );
////                InsertHash( temp, str, TokenType.DayOfWeekToken, i );
////
////                str = GetAbbreviatedDayName( (DayOfWeek)i );
////                InsertHash( temp, str, TokenType.DayOfWeekToken, i );
////
////            }
////
////            int[] eras = calendar.Eras;
////            for(int i = 1; i <= eras.Length; i++)
////            {
////                InsertHash( temp, GetEraName( i ), TokenType.EraToken, i );
////                InsertHash( temp, GetAbbreviatedEraName( i ), TokenType.EraToken, i );
////            }
////
////            if(LanguageName.Equals( JapaneseLangName ))
////            {
////                // Japanese allows day of week forms like: "(Tue)"
////                for(int i = 0; i < 7; i++)
////                {
////                    String specialDayOfWeek = "(" + GetAbbreviatedDayName( (DayOfWeek)i ) + ")";
////                    InsertHash( temp, specialDayOfWeek, TokenType.DayOfWeekToken, i );
////                }
////                if(this.Calendar.GetType() != typeof( JapaneseCalendar ))
////                {
////                    // Special case for Japanese.  If this is a Japanese DTFI, and the calendar is not Japanese calendar,
////                    // we will check Japanese Era name as well when the calendar is Gregorian.
////                    DateTimeFormatInfo jaDtfi = GetJapaneseCalendarDTFI();
////                    for(int i = 1; i <= jaDtfi.Calendar.Eras.Length; i++)
////                    {
////                        InsertHash( temp, jaDtfi.GetEraName( i ), TokenType.JapaneseEraToken, i );
////                        InsertHash( temp, jaDtfi.GetAbbreviatedEraName( i ), TokenType.JapaneseEraToken, i );
////                        // m_abbrevEnglishEraNames[0] contains the name for era 1, so the token value is i+1.
////                        InsertHash( temp, jaDtfi.AbbreviatedEnglishEraNames[i - 1], TokenType.JapaneseEraToken, i );
////                    }
////                }
////            }
////            else if(CultureName.Equals( "zh-TW" ))
////            {
////                DateTimeFormatInfo twDtfi = GetTaiwanCalendarDTFI();
////                for(int i = 1; i <= twDtfi.Calendar.Eras.Length; i++)
////                {
////                    if(twDtfi.GetEraName( i ).Length > 0)
////                    {
////                        InsertHash( temp, twDtfi.GetEraName( i ), TokenType.TEraToken, i );
////                    }
////                }
////            }
////
////            InsertHash( temp, InvariantInfo.AMDesignator, TokenType.SEP_Am | TokenType.Am, 0 );
////            InsertHash( temp, InvariantInfo.PMDesignator, TokenType.SEP_Pm | TokenType.Pm, 1 );
////
////            // Add invariant month names and day names.
////            for(int i = 1; i <= 12; i++)
////            {
////                String str;
////                // We have to call public methods here to work with inherited DTFI.
////                // Insert the month name first, so that they are at the front of abbrevaited
////                // month names.
////                str = InvariantInfo.GetMonthName( i );
////                InsertHash( temp, str, TokenType.MonthToken, i );
////                str = InvariantInfo.GetAbbreviatedMonthName( i );
////                InsertHash( temp, str, TokenType.MonthToken, i );
////            }
////
////            for(int i = 0; i < 7; i++)
////            {
////                // We have to call public methods here to work with inherited DTFI.
////                String str = InvariantInfo.GetDayName( (DayOfWeek)i );
////                InsertHash( temp, str, TokenType.DayOfWeekToken, i );
////
////                str = InvariantInfo.GetAbbreviatedDayName( (DayOfWeek)i );
////                InsertHash( temp, str, TokenType.DayOfWeekToken, i );
////
////            }
////
////            for(int i = 0; i < AbbreviatedEnglishEraNames.Length; i++)
////            {
////                // m_abbrevEnglishEraNames[0] contains the name for era 1, so the token value is i+1.
////                InsertHash( temp, AbbreviatedEnglishEraNames[i], TokenType.EraToken, i + 1 );
////            }
////
////            InsertHash( temp, LocalTimeMark, TokenType.SEP_LocalTimeMark, 0 );
////            InsertHash( temp, DateTimeParse.GMTName, TokenType.TimeZoneToken, 0 );
////            InsertHash( temp, DateTimeParse.ZuluName, TokenType.TimeZoneToken, 0 );
////
////            InsertHash( temp, invariantDateSeparator, TokenType.SEP_Date, 0 );
////            InsertHash( temp, invariantTimeSeparator, TokenType.SEP_Time, 0 );
////
////            if(CultureName.Equals( "ky-KG" ))
////            {
////                // For some cultures, the date separator works more like a comma, being allowed before or after any date part
////                InsertHash( temp, alternativeDateSeparator, TokenType.IgnorableSymbol, 0 );
////            }
////            else
////            {
////                InsertHash( temp, alternativeDateSeparator, TokenType.SEP_Date, 0 );
////            }
////
////            m_dtfiTokenHash = temp;
////        }
////        return (temp);
////    }
////
////    private void AddMonthNames( TokenHashValue[] temp, String monthPostfix )
////    {
////        for(int i = 1; i <= 13; i++)
////        {
////            String str;
////            //str = internalGetMonthName(i, MonthNameStyles.Regular, false);
////            // We have to call public methods here to work with inherited DTFI.
////            // Insert the month name first, so that they are at the front of abbrevaited
////            // month names.
////            str = GetMonthName( i );
////            if(str.Length > 0)
////            {
////                if(monthPostfix != null)
////                {
////                    // Insert the month name with the postfix first, so it can be matched first.
////                    InsertHash( temp, str + monthPostfix, TokenType.MonthToken, i );
////                }
////                else
////                {
////                    InsertHash( temp, str, TokenType.MonthToken, i );
////                }
////            }
////            str = GetAbbreviatedMonthName( i );
////            InsertHash( temp, str, TokenType.MonthToken, i );
////        }
////
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    // Actions:
////    // Try to parse the current word to see if it is a Hebrew number.
////    // Tokens will be updated accordingly.
////    // This is called by the Lexer of DateTime.Parse().
////    //
////    // Unlike most of the functions in this class, the return value indicates
////    // whether or not it started to parse. The badFormat parameter indicates
////    // if parsing began, but the format was bad.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////    private static bool TryParseHebrewNumber(
////        ref __DTString str,
////        out Boolean badFormat,
////        out int number )
////    {
////
////        number = -1;
////        badFormat = false;
////
////        int i = str.Index;
////        if(!HebrewNumber.IsDigit( str.Value[i] ))
////        {
////            // If the current character is not a Hebrew digit, just return false.
////            // There is no chance that we can parse a valid Hebrew number from here.
////            return (false);
////        }
////        // The current character is a Hebrew digit.  Try to parse this word as a Hebrew number.
////        HebrewNumberParsingContext context = new HebrewNumberParsingContext( 0 );
////        HebrewNumberParsingState state;
////
////        do
////        {
////            state = HebrewNumber.ParseByChar( str.Value[i++], ref context );
////            switch(state)
////            {
////                case HebrewNumberParsingState.InvalidHebrewNumber:    // Not a valid Hebrew number.
////                case HebrewNumberParsingState.NotHebrewDigit:         // The current character is not a Hebrew digit character.
////                    // Break out so that we don't continue to try parse this as a Hebrew number.
////                    return (false);
////            }
////        } while(i < str.Value.Length && (state != HebrewNumberParsingState.FoundEndOfHebrewNumber));
////
////        // When we are here, we are either at the end of the string, or we find a valid Hebrew number.
////        BCLDebug.Assert( state == HebrewNumberParsingState.ContinueParsing || state == HebrewNumberParsingState.FoundEndOfHebrewNumber,
////            "Invalid returned state from HebrewNumber.ParseByChar()" );
////
////        if(state != HebrewNumberParsingState.FoundEndOfHebrewNumber)
////        {
////            // We reach end of the string but we can't find a terminal state in parsing Hebrew number.
////            return (false);
////        }
////
////        // We have found a valid Hebrew number.  Update the index.
////        str.Advance( i - str.Index );
////
////        // Get the final Hebrew number value from the HebrewNumberParsingContext.
////        number = context.result;
////
////        return (true);
////    }
////
////    private static bool IsHebrewChar( char ch )
////    {
////        return (ch >= '\x0590' && ch <= '\x05ff');
////    }
////
////    internal bool Tokenize( TokenType TokenMask, out TokenType tokenType, out int tokenValue, ref __DTString str )
////    {
////        tokenType = TokenType.UnknownToken;
////        tokenValue = 0;
////
////        TokenHashValue value;
////        BCLDebug.Assert( str.Index < str.Value.Length, "DateTimeFormatInfo.Tokenize(): start < value.Length" );
////
////        char ch = str.m_current;
////        bool isLetter = Char.IsLetter( ch );
////        if(isLetter)
////        {
////            ch = Char.ToLower( ch, CultureInfo.CurrentCulture );
////            if(IsHebrewChar( ch ) && TokenMask == TokenType.RegularTokenMask)
////            {
////                bool badFormat;
////                if(TryParseHebrewNumber( ref str, out badFormat, out tokenValue ))
////                {
////                    if(badFormat)
////                    {
////                        tokenType = TokenType.UnknownToken;
////                        return (false);
////                    }
////                    // This is a Hebrew number.
////                    // Do nothing here.  TryParseHebrewNumber() will update token accordingly.
////                    tokenType = TokenType.HebrewNumber;
////                    return (true);
////                }
////            }
////        }
////
////
////        int hashcode = ch % TOKEN_HASH_SIZE;
////        int hashProbe = 1 + ch % SECOND_PRIME;
////        int remaining = str.len - str.Index;
////        int i = 0;
////
////        TokenHashValue[] hashTable = m_dtfiTokenHash;
////        if(hashTable == null)
////        {
////            hashTable = CreateTokenHashTable();
////        }
////        do
////        {
////            value = hashTable[hashcode];
////            if(value == null)
////            {
////                // Not found.
////                break;
////            }
////            // Check this value has the right category (regular token or separator token) that we are looking for.
////            if(((int)value.tokenType & (int)TokenMask) > 0 && value.tokenString.Length <= remaining)
////            {
////                if(String.Compare( str.Value, str.Index, value.tokenString, 0, value.tokenString.Length, true, CultureInfo.CurrentCulture ) == 0)
////                {
////                    if(isLetter)
////                    {
////                        // If this token starts with a letter, make sure that we won't allow partial match.  So you can't tokenize "MarchWed" separately.
////                        int nextCharIndex;
////                        if((nextCharIndex = str.Index + value.tokenString.Length) < str.len)
////                        {
////                            // Check word boundary.  The next character should NOT be a letter.
////                            char nextCh = str.Value[nextCharIndex];
////                            if(Char.IsLetter( nextCh ))
////                            {
////                                return (false);
////                            }
////                        }
////                    }
////                    tokenType = value.tokenType & TokenMask;
////                    tokenValue = value.tokenValue;
////                    str.Advance( value.tokenString.Length );
////                    return (true);
////                }
////                else if(value.tokenType == TokenType.MonthToken && HasSpacesInMonthNames)
////                {
////                    // For month token, we will match the month names which have spaces.
////                    int matchStrLen = 0;
////                    if(str.MatchSpecifiedWords( value.tokenString, true, ref matchStrLen ))
////                    {
////                        tokenType = value.tokenType & TokenMask;
////                        tokenValue = value.tokenValue;
////                        str.Advance( matchStrLen );
////                        return (true);
////                    }
////                }
////                else if(value.tokenType == TokenType.DayOfWeekToken && HasSpacesInDayNames)
////                {
////                    // For month token, we will match the month names which have spaces.
////                    int matchStrLen = 0;
////                    if(str.MatchSpecifiedWords( value.tokenString, true, ref matchStrLen ))
////                    {
////                        tokenType = value.tokenType & TokenMask;
////                        tokenValue = value.tokenValue;
////                        str.Advance( matchStrLen );
////                        return (true);
////                    }
////                }
////            }
////            i++;
////            hashcode += hashProbe;
////            if(hashcode >= TOKEN_HASH_SIZE) hashcode -= TOKEN_HASH_SIZE;
////        } while(i < TOKEN_HASH_SIZE);
////
////        return (false);
////    }
////
////    void InsertAtCurrentHashNode( TokenHashValue[] hashTable, String str, char ch, TokenType tokenType, int tokenValue, int pos, int hashcode, int hashProbe )
////    {
////        // Remember the current slot.
////        TokenHashValue previousNode = hashTable[hashcode];
////
////        //// Console.WriteLine("   Insert Key: {0} in {1}", str, slotToInsert);
////        // Insert the new node into the current slot.
////        hashTable[hashcode] = new TokenHashValue( str, tokenType, tokenValue ); ;
////
////        while(++pos < TOKEN_HASH_SIZE)
////        {
////            hashcode += hashProbe;
////            if(hashcode >= TOKEN_HASH_SIZE) hashcode -= TOKEN_HASH_SIZE;
////            // Remember this slot
////            TokenHashValue temp = hashTable[hashcode];
////
////            if(temp != null && Char.ToLower( temp.tokenString[0], CultureInfo.CurrentCulture ) != ch)
////            {
////                continue;
////            }
////            // Put the previous slot into this slot.
////            hashTable[hashcode] = previousNode;
////            //// Console.WriteLine("  Move {0} to slot {1}", previousNode.tokenString, hashcode);
////            if(temp == null)
////            {
////                // Done
////                return;
////            }
////            previousNode = temp;
////        };
////        BCLDebug.Assert( true, "The hashtable is full.  This should not happen." );
////    }
////
////    void InsertHash( TokenHashValue[] hashTable, String str, TokenType tokenType, int tokenValue )
////    {
////        // The month of the 13th month is allowed to be null, so make sure that we ignore null value here.
////        if(str == null || str.Length == 0)
////        {
////            return;
////        }
////        TokenHashValue value;
////        int i = 0;
////        // If there is whitespace characters in the beginning and end of the string, trim them since whitespaces are skipped by
////        // DateTime.Parse().
////        if(Char.IsWhiteSpace( str[0] ) || Char.IsWhiteSpace( str[str.Length - 1] ))
////        {
////            str = str.Trim( null );   // Trim white space characters.
////            // Could have space for separators
////            if(str.Length == 0)
////                return;
////        }
////        char ch = Char.ToLower( str[0], CultureInfo.CurrentCulture );
////        int hashcode = ch % TOKEN_HASH_SIZE;
////        int hashProbe = 1 + ch % SECOND_PRIME;
////        do
////        {
////            value = hashTable[hashcode];
////            if(value == null)
////            {
////                //// Console.WriteLine("   Put Key: {0} in {1}", str, hashcode);
////                hashTable[hashcode] = new TokenHashValue( str, tokenType, tokenValue );
////                return;
////            }
////            else
////            {
////                // Collision happens. Find another slot.
////                if(str.Length >= value.tokenString.Length)
////                {
////                    // If there are two tokens with the same prefix, we have to make sure that the longer token should be at the front of
////                    // the shorter ones.
////                    if(String.Compare( str, 0, value.tokenString, 0, value.tokenString.Length, true, CultureInfo.CurrentCulture ) == 0)
////                    {
////                        if(str.Length > value.tokenString.Length)
////                        {
////                            // The str to be inserted has the same prefix as the current token, and str is longer.
////                            // Insert str into this node, and shift every node behind it.
////                            InsertAtCurrentHashNode( hashTable, str, ch, tokenType, tokenValue, i, hashcode, hashProbe );
////                            return;
////                        }
////                        else
////                        {
////                            // Same token.  If they have different types (regular token vs separator token.  Add them.
////                            if(((int)tokenType & 0xff00) != ((int)value.tokenType & 0xff00))
////                            {
////                                value.tokenType |= tokenType;
////                                if(tokenValue != 0)
////                                {
////                                    value.tokenValue = tokenValue;
////                                }
////                            }
////                            // The token to be inserted is already in the table.  Skip it.
////                        }
////                    }
////                }
////            }
////            //// Console.WriteLine("  COLLISION. Old Key: {0}, New Key: {1}", hashTable[hashcode].tokenString, str);
////            i++;
////            hashcode += hashProbe;
////            if(hashcode >= TOKEN_HASH_SIZE) hashcode -= TOKEN_HASH_SIZE;
////        } while(i < TOKEN_HASH_SIZE);
////        BCLDebug.Assert( true, "The hashtable is full.  This should not happen." );
////    }
////
////    internal static string GetCalendarInfo( int culture, int calendar, int calType )
////    {
////        int size = Microsoft.Win32.Win32Native.GetCalendarInfo( culture, calendar, calType, null, 0, IntPtr.Zero );
////        if(size > 0)
////        {
////            StringBuilder buffer = new StringBuilder( size );
////            size = Microsoft.Win32.Win32Native.GetCalendarInfo( culture, calendar, calType, buffer, size, IntPtr.Zero );
////            if(size > 0)
////                return buffer.ToString( 0, size - 1 ); // Exclude the null termination.
////        }
////        return null;
////    }
////
////    // Win32 Constants for calling GetCalendarInfo().
////    internal const int CAL_SCALNAME = 0x00000002;  // native name of calendar
    }   // class DateTimeFormatInfo

////internal class TokenHashValue
////{
////    internal String tokenString;
////    internal TokenType tokenType;
////    internal int tokenValue;
////
////    internal TokenHashValue( String tokenString, TokenType tokenType, int tokenValue )
////    {
////        this.tokenString = tokenString;
////        this.tokenType = tokenType;
////        this.tokenValue = tokenValue;
////    }
////}
}
