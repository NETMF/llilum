// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
namespace System.Globalization
{
    using System;
    using System.Collections;
    using System.IO;
////using System.Runtime.Remoting;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Security.Permissions;
////using System.Runtime.Versioning;

    ////////////////////////////////////////////////////////////////////////
    //
    // Base class used to read NLS+ data tables for culture/region/calendar data
    //
    //
    // Data types supported in this version:
    // 0: WORD      A WORD value.  It is returend as signed int.  The interpretation of sign is left to caller.
    // 1: String    A string value.  The first WORD is the size of the string.
    // 2: WORD[]    A WORD array.  The first DWORD is the size of the array, and it is followed by the DWORD values.
    // 3: String[]  A string array.  The first WORD is the size of the string array.  And it is folowed by offsets to counted string values.
    //
    ////////////////////////////////////////////////////////////////////////
    internal abstract class BaseInfoTable
    {
////    // The base pointer of the data table (beginning of the data file)
////    unsafe internal byte* m_pDataFileStart;
////
////    protected MemoryMapFile memoryMapFile = null;
////
////    // The pointer to the main header
////    unsafe protected CultureTableHeader* m_pCultureHeader;
////
////    // The pointer to where the item data begins.
////    unsafe internal byte* m_pItemData;
////
////    // The total number of data items.
////    internal uint m_numItem;
////
////    // The size of each data item.  This is the total size (in bytes) of each calendar or culture record.
////    internal uint m_itemSize;
////
////    internal unsafe ushort* m_pDataPool;
////
////    // Where we came from
////    internal bool fromAssembly;
////    internal String fileName;
////
////    protected bool m_valid = true;
////
////    //private static String m_InternationalRegKey = "Control Panel\\International";
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    // Read the table and set up pointers pointing to different parts of
////    // the table.
////    // Parameters:
////    //  fromAssembly    When true, we will load data table from mscorlib assemlby.
////    //
////    ////////////////////////////////////////////////////////////////////////
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    internal unsafe BaseInfoTable( String fileName, bool fromAssembly )
////    {
////        this.fileName = fileName;
////        this.fromAssembly = fromAssembly;
////        InitializeBaseInfoTablePointers( fileName, fromAssembly );
////    }
////
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    internal unsafe void InitializeBaseInfoTablePointers( String fileName, bool fromAssembly )
////    {
////        if(fromAssembly)
////        {
////            m_pDataFileStart = GlobalizationAssembly.GetGlobalizationResourceBytePtr( typeof( BaseInfoTable ).Assembly, fileName );
////        }
////        else
////        {
////            this.memoryMapFile = new MemoryMapFile( fileName );
////
////            if(this.memoryMapFile.FileSize == 0)
////            {
////                m_valid = false;
////                return;
////            }
////
////            this.m_pDataFileStart = this.memoryMapFile.GetBytePtr();
////        }
////        EndianessHeader* pEndianHeader = (EndianessHeader*)m_pDataFileStart;
////
////        // Set up pointer to the CultureTableHeader
////
////#if BIGENDIAN
////        BCLDebug.Assert(pEndianHeader->beOffset != 0, "Big-Endian data is expected.");
////        m_pCultureHeader = (CultureTableHeader*)(m_pDataFileStart + pEndianHeader->beOffset);
////#else
////        BCLDebug.Assert( pEndianHeader->leOffset != 0, "Little-Endian data is expected." );
////        m_pCultureHeader = (CultureTableHeader*)(m_pDataFileStart + pEndianHeader->leOffset);
////#endif
////
////        // Set up misc pointers and variables.
////        // Different data items for calendar and culture, so they each have their own setting thingy.
////        SetDataItemPointers();
////    }
////
////    //
////    // IsValid
////    // tell if the current object is valid to use or not. note the only cases this object br invalid is 
////    // when we have corrupted custom culture file.
////    //
////
////    internal bool IsValid
////    {
////        get { return m_valid; }
////    }
////
////    public override unsafe bool Equals( Object value )
////    {
////        BaseInfoTable that = value as BaseInfoTable;
////        return (that != null) &&
////                (this.fromAssembly == that.fromAssembly &&
////                 CultureInfo.InvariantCulture.CompareInfo.Compare(
////                    this.fileName, that.fileName, CompareOptions.IgnoreCase ) == 0);
////    }
////
////    public override int GetHashCode()
////    {
////        return (this.fileName.GetHashCode());
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    // Set Data Item Pointers that are unique to calendar or culture table
////    //
////    ////////////////////////////////////////////////////////////////////////
////    internal abstract unsafe void SetDataItemPointers();
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    // Read a string from the string pool using the given string offset
////    //
////    ////////////////////////////////////////////////////////////////////////
////    internal unsafe String GetStringPoolString( uint offset )
////    {
////        char* pCharValues = unchecked( (char*)(m_pDataPool + offset) );
////        // In the case of empty string, pCharValues[0] will have a size of 1, so we should check pCharValues[1] for empty string.
////        if(pCharValues[1] == 0) return (String.Empty);
////        return (new String( pCharValues + 1, 0, (int)pCharValues[0] ));
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    // Read a string array from the pool using the given offset
////    //
////    ////////////////////////////////////////////////////////////////////////
////    internal unsafe String[] GetStringArray( uint iOffset )
////    {
////        // If its empty return null
////        if(iOffset == 0)
////            return new String[0];
////
////        // The offset value is in char, and is related to the begining of string pool.
////        ushort* pCount = m_pDataPool + iOffset;
////        int count = (int)pCount[0];    // The number of strings in the array
////        String[] values = new String[count];
////
////        // Get past count and cast to uint
////        uint* pStringArray = (uint*)(pCount + 1);
////
////        // Get our strings
////        for(int i = 0; i < count; i++)
////            values[i] = GetStringPoolString( pStringArray[i] );
////
////        return (values);
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    // Return multiple WORD field arrays values for the specified data item in the data table.
////    // The result is retured as an array of int since that's what most CI/NFI/DTFI properties return.
////    //
////    ////////////////////////////////////////////////////////////////////////
////    internal unsafe int[][] GetWordArrayArray( uint iOffset )
////    {
////        // if its empty return null
////        if(iOffset == 0)
////            return new int[0][];
////
////        // The offset value is in char, and is related to the begining of string pool.
////        // Need short* to get proper negative numbers for negative eras.
////        short* pCount = (short*)(m_pDataPool + iOffset);
////        int countArrays = (int)pCount[0];    // The number of word arrays in the array
////        int[][] values = new int[countArrays][];
////
////        // Get past count and cast to uint to get the pointers to the word arrays
////        uint* pWordArrays = (uint*)(pCount + 1);
////
////        // Get our word arrays
////        for(int i = 0; i < countArrays; i++)
////        {
////            pCount = (short*)(m_pDataPool + pWordArrays[i]);
////            int count = pCount[0];
////            pCount++;                           // Advance past count and reuse for word pointer
////            values[i] = new int[count];
////            for(int j = 0; j < count; j++)
////            {
////                values[i][j] = pCount[j];
////            }
////        }
////
////        return (values);
////    }
////
////    ////////////////////////////////////////////////////////////////////////
////    //
////    // Compare a managed string to a string pool using the given string offset.
////    // Does binary comparisons. This is a bit better for perf. and reources than  
////    // creating a string object and making an f-call.
////    //
////    // Parameters
////    //      name: The name to be compared.
////    //      offset: an offset into the string table.
////    //
////    // Note
////    //  name should be in lowercase when this is called.
////    //
////    ////////////////////////////////////////////////////////////////////////
////    internal unsafe int CompareStringToStringPoolStringBinary( String name, int offset )
////    {
////        int test = 0;
////        char* pCharValues = unchecked( (char*)(m_pDataPool + offset) );
////        // In the case of empty string, pCharValues[0] will have a size of 1, so we should check pCharValues[1] for empty string.
////        if(pCharValues[1] == 0)
////        {
////            if(name.Length == 0)
////            {
////                return (0);
////            }
////            return (1);
////        }
////
////        for(int i = 0; (i < (int)pCharValues[0]) && (i < name.Length); i++)
////        {
////            BCLDebug.Assert( !(name[i] >= 'A' && name[i] <= 'Z'), "name should be in lowercase" );
////            // A little case insensitivity, for ASCII only.
////            test = name[i] - ((pCharValues[i + 1] <= 'Z' && pCharValues[i + 1] >= 'A') ?
////                              (pCharValues[i + 1] + 'a' - 'A') :
////                              (pCharValues[i + 1]));
////
////            if(test != 0)
////            {
////                break;
////            }
////        }
////
////        return (test == 0 ? name.Length - (int)pCharValues[0] : test);
////    }
    }
}
