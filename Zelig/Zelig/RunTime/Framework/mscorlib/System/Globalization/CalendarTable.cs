// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
namespace System.Globalization
{
////using System.Runtime.Remoting;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
////using System.Runtime.Versioning;
    using System.Runtime.InteropServices;

    /*=============================================================================
     *
     * Calendar Data table for DateTimeFormatInfo classes.  Used by System.Globalization.DateTimeFormatInfo.
     *
     ==============================================================================*/

    internal class CalendarTable : BaseInfoTable
    {
////
////    // The default instance of calendar table.  We should only create one instance per app-domain.
////    private static CalendarTable m_defaultInstance;
////    unsafe CalendarTableData* m_calendars;
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    static CalendarTable()
////    {
////        m_defaultInstance = new CalendarTable( "culture.nlp", true );
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    internal CalendarTable( String fileName, bool fromAssembly )
////        : base( fileName, fromAssembly )
////    {
////        // Do nothing here.
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    // Set Data Item Pointers that are unique to calendar table
////    //
////    ////////////////////////////////////////////////////////////////////////
////    internal override unsafe void SetDataItemPointers()
////    {
////        m_itemSize = m_pCultureHeader->sizeCalendarItem;
////        m_numItem = m_pCultureHeader->numCalendarItems;
////        m_pDataPool = (ushort*)(m_pDataFileStart + m_pCultureHeader->offsetToDataPool);
////        // We subtract item size because calender indices start at 1 but our table doesn't have an entry for 0
////        m_pItemData = m_pDataFileStart + m_pCultureHeader->offsetToCalendarItemData - m_itemSize;
////        // Get calendar list.  We subtract 1 because calender indices start at 1 but our table doesn't have an entry for 0
////        m_calendars = (CalendarTableData*)(m_pDataFileStart + m_pCultureHeader->offsetToCalendarItemData) - 1;
////    }
////
////    internal static CalendarTable Default
////    {
////        get
////        {
////            BCLDebug.Assert( m_defaultInstance != null, "CalendarTable.Default: default instance is not created." );
////            return (m_defaultInstance);
////        }
////    }
////
////    internal unsafe int ICURRENTERA( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.ICURRENTERA]id out of calendar range" );
////
////        return m_calendars[id].iCurrentEra;
////    }
////
////    internal unsafe int IFORMATFLAGS( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.IFORMATFLAGS]id out of calendar range" );
////
////        return m_calendars[id].iFormatFlags;
////    }
////
////    internal unsafe String[] SDAYNAMES( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.SDAYNAMES]id out of calendar range" );
////
////        return GetStringArray( m_calendars[id].saDayNames );
////    }
////
////    internal unsafe String[] SABBREVDAYNAMES( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.SABBREVDAYNAMES]id out of calendar range" );
////
////        return GetStringArray( m_calendars[id].saAbbrevDayNames );
////    }
////
////    internal unsafe String[] SSUPERSHORTDAYNAMES( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.SSUPERSHORTDAYNAMES]id out of calendar range" );
////
////        return GetStringArray( m_calendars[id].saSuperShortDayNames );
////    }
////
////    internal unsafe String[] SMONTHNAMES( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.SMONTHNAMES]id out of calendar range" );
////
////        return GetStringArray( m_calendars[id].saMonthNames );
////    }
////
////    internal unsafe String[] SABBREVMONTHNAMES( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.SABBREVMONTHNAMES]id out of calendar range" );
////
////        return GetStringArray( m_calendars[id].saAbbrevMonthNames );
////    }
////
////    internal unsafe String[] SLEAPYEARMONTHNAMES( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.SLEAPYEARMONTHNAMES]id out of calendar range" );
////
////        return GetStringArray( m_calendars[id].saLeapYearMonthNames );
////    }
////
////    internal unsafe String[] SSHORTDATE( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.SSHORTDATE]id out of calendar range" );
////
////        return GetStringArray( m_calendars[id].saShortDate );
////    }
////
////    internal unsafe String[] SLONGDATE( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.SLONGDATE]id out of calendar range" );
////
////        return GetStringArray( m_calendars[id].saLongDate );
////    }
////
////    internal unsafe String[] SYEARMONTH( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.SYEARMONTH]id out of calendar range" );
////
////        return GetStringArray( m_calendars[id].saYearMonth );
////    }
////
////    internal unsafe String SMONTHDAY( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.SMONTHDAY]id out of calendar range" );
////
////        return GetStringPoolString( m_calendars[id].sMonthDay );
////    }
////
////    internal unsafe int[][] SERARANGES( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.SERARANGES]id out of calendar range" );
////
////        return GetWordArrayArray( m_calendars[id].waaEraRanges );
////    }
////
////    internal unsafe String[] SERANAMES( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.SERANAMES]id out of calendar range" );
////
////        return GetStringArray( m_calendars[id].saEraNames );
////    }
////
////    internal unsafe String[] SABBREVERANAMES( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.SABBREVERANAMES]id out of calendar range" );
////
////        return GetStringArray( m_calendars[id].saAbbrevEraNames );
////    }
////
////    internal unsafe String[] SABBREVENGERANAMES( int id )
////    {
////        BCLDebug.Assert( id > 0 && id <= m_numItem,
////            "[CalendarTable.SABBREVENGERANAMES]id out of calendar range" );
////
////        return GetStringArray( m_calendars[id].saAbbrevEnglishEraNames );
////    }
////
////    [ResourceExposure( ResourceScope.None )]
////    [MethodImplAttribute( MethodImplOptions.InternalCall )]
////    internal static extern String nativeGetEraName( int culture, int calID );
    }
}
