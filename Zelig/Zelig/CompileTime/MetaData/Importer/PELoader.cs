//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    public sealed class PELoader
    {
        //
        // State
        //

        private String          m_path;
        private byte[]          m_peImage;
        private DOSHeader       m_dosHeader;
        private NTHeader        m_ntHeader;
        private COMHeader       m_comHeader;
        private SectionHeader[] m_sectionArray;

        //
        // Constructor Methods
        //

        public PELoader( String path    ,
                         byte[] peImage )
        {
            m_path    = path;
            m_peImage = peImage;

            loadPEHeaders();
        }

        //
        // Access Methods
        //

        internal ArrayReader getStream()
        {
            return new ArrayReader( m_peImage );
        }

        internal int getEntryPoint()
        {
            return m_comHeader.entryPointToken;
        }

        internal int getMetaDataOffset()
        {
            return this.VaToOffset( m_comHeader.metaData.virtualAddress );
        }

        internal int getMetaDataSize()
        {
            return m_comHeader.metaData.size;
        }

        internal int getResourceOffset()
        {
            return this.VaToOffsetSafe( m_comHeader.resources.virtualAddress );
        }

        internal int getRelocationOffset()
        {
            int limit = m_ntHeader.numberOfSections;

            for(int i = 0; i < limit; i++)
            {
                SectionHeader section = m_sectionArray[i];

                if(section.name.StartsWith( ".reloc" ))
                {
                    return this.VaToOffset( section.virtualAddress );
                }
            }

            return 0;
        }

        internal int getVtableFixupOffset()
        {
            return this.VaToOffsetSafe( m_comHeader.vtableFixups.virtualAddress );
        }

        internal int getDelayIATOffset()
        {
            return this.VaToOffsetSafe( m_ntHeader.dataDirectory[DirectoryEntry.DELAYLOADIMPORTADDRESSTABLE].virtualAddress );
        }

        internal long getImageBase()
        {
            return m_ntHeader.imageBase;
        }

        internal int getResourceSize()
        {
            return m_comHeader.resources.size;
        }

        internal int getVtableFixupSize()
        {
            return m_comHeader.vtableFixups.size;
        }

        internal int getDelayIATSize()
        {
            return m_ntHeader.dataDirectory[DirectoryEntry.DELAYLOADIMPORTADDRESSTABLE].size;
        }

        internal bool IsExecutableImage()
        {
            return ((m_ntHeader.characteristics & NTHeader.IMAGE_FILE_EXECUTABLE_IMAGE) != 0) &&
                   ((m_ntHeader.characteristics & NTHeader.IMAGE_FILE_DLL             ) == 0);
        }

        // Output Routines

        public void DumpHeader( LogWriter outputStream )
        {
            outputStream.WriteLine( "// DOS Header:" );
            outputStream.WriteLine();
            m_dosHeader.DumpLimitedToStream( outputStream );
            outputStream.WriteLine( "// PE Header:" );
            outputStream.WriteLine();
            m_ntHeader.DumpLimitedToStream( outputStream );

            this.DumpIAT( outputStream, "Import directory"               , m_ntHeader.dataDirectory[DirectoryEntry.IMPORT                     ] );
            this.DumpIAT( outputStream, "Import Address Table"           , m_ntHeader.dataDirectory[DirectoryEntry.IMPORTADDRESSTABLE         ] );
            this.DumpIAT( outputStream, "Delay Load Import Address Table", m_ntHeader.dataDirectory[DirectoryEntry.DELAYLOADIMPORTADDRESSTABLE] );

            m_comHeader.DumpHeader( outputStream );
        }

        internal void DumpCodeManager( LogWriter outputStream )
        {
            outputStream.WriteLine( "// Code Manager Table" );
            if(m_comHeader.codeManagerTable.size == 0)
            {
                outputStream.WriteLine( "//  default" );
                return;
            }
            // BUGBUG
            throw new NotYetImplemented( "DumpCodeManager" );
        }

        internal void DumpVTables( LogWriter outputStream )
        {
            outputStream.WriteLine( "// VTableFixup Directory:" );
            if(m_comHeader.vtableFixups.virtualAddress == 0)
            {
                outputStream.WriteLine( "//  No data." );
                return;
            }
            // BUGBUG
            throw new NotYetImplemented( "DumpVTables" );
        }

        internal void DumpEATTable( LogWriter outputStream )
        {
            outputStream.WriteLine( "// Export Address Table Jumps:" );
            if(m_comHeader.exportAddressTableJumps.virtualAddress == 0)
            {
                outputStream.WriteLine( "//  No data." );
                return;
            }
            // BUGBUG
            throw new NotYetImplemented( "DumpEATTable" );
        }

        internal void DumpStatistics( LogWriter outputStream )
        {
            throw new NotYetImplemented( "DumpStatistics" );
        }

        public override String ToString()
        {
            return "PELoader(" + m_path + ")";
        }

        //
        // Private Helper Methods
        //

        private void loadPEHeaders()
        {
            ArrayReader reader = new ArrayReader( m_peImage );

            m_dosHeader = new DOSHeader( reader );

            reader.Position = m_dosHeader.lfanew;
            m_ntHeader      = new NTHeader( reader );

            // Load the sections
            int             sectionCount = m_ntHeader.numberOfSections;
            SectionHeader[] sectionArray = new SectionHeader[sectionCount];

            m_sectionArray = sectionArray;

            for(int i = 0; i < sectionCount; i++)
            {
                m_sectionArray[i] = new SectionHeader( reader );

                int startAddr = sectionArray[i].virtualAddress;
                int endAddr   = sectionArray[i].virtualAddress   + sectionArray[i].sizeOfRawData;
                int delta     = sectionArray[i].pointerToRawData - sectionArray[i].virtualAddress;
            }

            // Load the COM/COR20 header
            DirectoryEntry entry = m_ntHeader.dataDirectory[DirectoryEntry.CLRHEADER];

            int comOffset = this.VaToOffsetSafe( entry.virtualAddress );
            if(comOffset == -1)
            {
                throw new MissingCLRheaderException( "Missing CLR header in " + m_path );
            }

            reader.Position = comOffset;
            m_comHeader     = new COMHeader( reader );
        }

        internal int VaToOffset( int virtualAddress )
        {
            int result = VaToOffsetSafe( virtualAddress );

            if(result == -1)
            {
                throw new IllegalPEFormatException( "Unknown VA " + virtualAddress );
            }

            return result;
        }

        internal int VaToOffsetSafe( int virtualAddress )
        {
            int limit = m_ntHeader.numberOfSections;

            for(int i = 0; i < limit; i++)
            {
                SectionHeader section = m_sectionArray[i];

                if(virtualAddress >= section.virtualAddress && virtualAddress < (section.virtualAddress + section.sizeOfRawData))
                {
                    return (virtualAddress - section.virtualAddress + section.pointerToRawData);
                }
            }

            return -1;
        }

        private void DumpIAT( LogWriter      outputStream ,
                              String         title        ,
                              DirectoryEntry entry        )
        {
            outputStream.WriteLine( "// " + title );

            if(entry.size == 0)
            {
                outputStream.WriteLine( "// No data." );
                outputStream.WriteLine();
                return;
            }

            if(entry.size < ImportDescriptor.SIZE)
            {
                outputStream.WriteLine( "Not enough data for IMAGE_IMPORT_DESCRIPTOR" );
                return;
            }

            int importOffset = this.VaToOffset( entry.virtualAddress );

            ArrayReader reader = new ArrayReader( m_peImage );

            while(true)
            {
                reader.Position = importOffset;

                ImportDescriptor importDescriptor = new ImportDescriptor( reader );
                if(importDescriptor.firstChunk == 0)
                {
                    return;
                }

                String name = null;

                if(importDescriptor.name != 0)
                {
                    int nameOffset = this.VaToOffset( importDescriptor.name );

                    reader.Position = nameOffset;

                    name = reader.ReadZeroTerminatedUInt8String();
                }

                outputStream.WriteLine( "//     " + name );
                importDescriptor.DumpToStream( outputStream );
                outputStream.WriteLine( "//" );

                int importTableOffset = this.VaToOffset( importDescriptor.firstChunk );
                while(true)
                {
                    reader.Position = importTableOffset;

                    int importTableID = reader.ReadInt32();
                    if(importTableID == 0)
                    {
                        break;
                    }

                    outputStream.WriteLine( "importTableID is " + importTableID.ToString( "x" ) );
                    outputStream.Flush();

                    int nameStringOffset = this.VaToOffset( importTableID & 0x7fffffff );

                    reader.Position = nameStringOffset;

                    if((importTableID & 0x8000000) != 0)
                    {
                        outputStream.WriteLine( "//              " + reader.ReadInt16().ToString( "x8" ) + " by ordinal " + (importTableID & 0x7ffffff) );
                    }
                    else
                    {
                        outputStream.WriteLine( "//              " + reader.ReadInt16().ToString( "x8" ) + " " + reader.ReadZeroTerminatedUInt8String() );
                    }

                    importTableOffset += 4;
                }

                outputStream.WriteLine();

                importOffset += ImportDescriptor.SIZE;
            }
        }

        internal Section[] loadSections()
        {
            ArrayReader reader = new ArrayReader( m_peImage );

            Section[] sections = new Section[m_sectionArray.Length];
            for(int i = 0; i < m_sectionArray.Length; i++)
            {
                Section section = new Section( m_sectionArray[i] );

                section.LoadSection( this, reader );

                sections[i] = section;
            }

            return sections;
        }

        // Nested classes

        internal class DOSHeader
        {
            // Corresponds to the WinNT IMAGE_DOS_HEADER data structure

            internal const short IMAGE_DOS_SIGNATURE = 0x5A4D;

            //
            // State
            //

            internal short magic;    // Magic number
            internal short cblp;     // Bytes on last page of file
            internal short cp;       // Pages in file
            internal short crlc;     // Relocations
            internal short cparhdr;  // Size of header in paragraphs
            internal short minalloc; // Minimum extra paragraphs needed
            internal short maxalloc; // Maximum extra paragraphs needed
            internal short ss;       // Initial (relative) SS value
            internal short sp;       // Initial SP value
            internal short csum;     // Checksum
            internal short ip;       // Initial IP value
            internal short cs;       // Initial (relative) CS value
            internal short lfarlc;   // File address of relocation table
            internal short ovno;     // Overlay number
            internal short res_0;    // Reserved words
            internal short res_1;
            internal short res_2;
            internal short res_3;
            internal short oemid;    // OEM identifier (for e_oeminfo)
            internal short oeminfo;  // OEM information; e_oemid specific
            internal short res2_0;   // Reserved words
            internal short res2_1;
            internal short res2_2;
            internal short res2_3;
            internal short res2_4;
            internal short res2_5;
            internal short res2_6;
            internal short res2_7;
            internal short res2_8;
            internal short res2_9;
            internal int   lfanew;   // File address of new exe header

            //
            // Constructor Methods
            //

            internal DOSHeader( ArrayReader reader )
            {
                // We could just read the magic and lfanew fields, but let's read everything for now
                this.magic    = reader.ReadInt16();
                this.cblp     = reader.ReadInt16();
                this.cp       = reader.ReadInt16();
                this.crlc     = reader.ReadInt16();
                this.cparhdr  = reader.ReadInt16();
                this.minalloc = reader.ReadInt16();
                this.maxalloc = reader.ReadInt16();
                this.ss       = reader.ReadInt16();
                this.sp       = reader.ReadInt16();
                this.csum     = reader.ReadInt16();
                this.ip       = reader.ReadInt16();
                this.cs       = reader.ReadInt16();
                this.lfarlc   = reader.ReadInt16();
                this.ovno     = reader.ReadInt16();
                this.res_0    = reader.ReadInt16();
                this.res_1    = reader.ReadInt16();
                this.res_2    = reader.ReadInt16();
                this.res_3    = reader.ReadInt16();
                this.oemid    = reader.ReadInt16();
                this.oeminfo  = reader.ReadInt16();
                this.res2_0   = reader.ReadInt16();
                this.res2_1   = reader.ReadInt16();
                this.res2_2   = reader.ReadInt16();
                this.res2_3   = reader.ReadInt16();
                this.res2_4   = reader.ReadInt16();
                this.res2_5   = reader.ReadInt16();
                this.res2_6   = reader.ReadInt16();
                this.res2_7   = reader.ReadInt16();
                this.res2_8   = reader.ReadInt16();
                this.res2_9   = reader.ReadInt16();
                this.lfanew   = reader.ReadInt32();

                // Verify that we have a correct DOS header and a valid
                // pointer to the NT header
                if(this.magic != DOSHeader.IMAGE_DOS_SIGNATURE || this.lfanew <= 0)
                {
                    throw new IllegalPEFormatException( "DOS header problems" );
                }
            }

            // Output Routines

            internal void DumpLimitedToStream( LogWriter outputStream )
            {
                outputStream.WriteLine( "// Magic:                          {0:X2}", this.magic );
                // TODO: More here
            }
        }

        internal class NTHeader
        {
            // Corresponds to the WinNT IMAGE_NT_HEADERS data structure

            internal const int   IMAGE_NT_SIGNATURE                    = 0x00004550; // PE00
            internal const short IMAGE_SIZEOF_NT_OPTIONAL32_HEADER     = 224;
            internal const short IMAGE_SIZEOF_NT_OPTIONAL32PLUS_HEADER = 240;
            internal const short IMAGE_NT_OPTIONAL_HDR32_MAGIC         = 0x010B;
            internal const short IMAGE_NT_OPTIONAL_HDR32PLUS_MAGIC     = 0x020B;
            internal const short IMAGE_FILE_EXECUTABLE_IMAGE           = 0x0002; // File is executable  (i.e. no unresolved external references).
            internal const short IMAGE_FILE_DLL                        = 0x2000; // File is a DLL.

            //
            // State
            //

            internal int              signature;
            // IMAGE_FILE_HEADER
            internal short            machine;
            internal short            numberOfSections;
            internal int              timeDateStamp;
            internal int              pointerToSymbolTable;
            internal int              numberOfSymbols;
            internal short            sizeOfOptionalHeader;
            internal short            characteristics;
            // IMAGE_OPTIONAL_HEADER32
            internal short            magic;
            internal Byte             majorLinkerVersion;
            internal Byte             minorLinkerVersion;
            internal int              sizeOfCode;
            internal int              sizeOfInitializedData;
            internal int              sizeOfUninitializedData;
            internal int              addressOfEntryPoint;
            internal int              baseOfCode;
            private  int              baseOfData; //this is not used on x64
            internal long             imageBase;
            internal int              sectionAlignment;
            internal int              fileAlignment;
            internal short            majorOperatingSystemVersion;
            internal short            minorOperatingSystemVersion;
            internal short            majorImageVersion;
            internal short            minorImageVersion;
            internal short            majorSubsystemVersion;
            internal short            minorSubsystemVersion;
            internal int              win32VersionValue;
            internal int              sizeOfImage;
            internal int              sizeOfHeaders;
            internal int              checkSum;
            internal short            subsystem;
            internal short            dllCharacteristics;
            internal long             sizeOfStackReserve;
            internal long             sizeOfStackCommit;
            internal long             sizeOfHeapReserve;
            internal long             sizeOfHeapCommit;
            internal int              loaderFlags;
            internal int              numberOfRvaAndSizes;
            // IMAGE_DATA_DIRECTORY
            internal DirectoryEntry[] dataDirectory;

            //whether or not image is PE32+ (determined from magic number)
            internal bool             isPe32Plus;

            //
            // Constructor Methods
            //

            internal NTHeader( ArrayReader reader )
            {
                // We could read a selection of these, but read every
                // field for now.
                this.signature                   = reader.ReadInt32();
                this.machine                     = reader.ReadInt16();
                this.numberOfSections            = reader.ReadInt16();
                this.timeDateStamp               = reader.ReadInt32();
                this.pointerToSymbolTable        = reader.ReadInt32();
                this.numberOfSymbols             = reader.ReadInt32();
                this.sizeOfOptionalHeader        = reader.ReadInt16();
                this.characteristics             = reader.ReadInt16();
                this.magic                       = reader.ReadInt16();
                this.isPe32Plus                  = (magic == NTHeader.IMAGE_NT_OPTIONAL_HDR32PLUS_MAGIC);
                this.majorLinkerVersion          = reader.ReadUInt8();
                this.minorLinkerVersion          = reader.ReadUInt8();
                this.sizeOfCode                  = reader.ReadInt32();
                this.sizeOfInitializedData       = reader.ReadInt32();
                this.sizeOfUninitializedData     = reader.ReadInt32();
                this.addressOfEntryPoint         = reader.ReadInt32();
                this.baseOfCode                  = reader.ReadInt32();
                if(!this.isPe32Plus)
                {
                    this.baseOfData              = reader.ReadInt32();
                }
                else
                {
                    this.baseOfData              = 0;
                }
                this.imageBase                   = readIntPtr( reader );
                this.sectionAlignment            = reader.ReadInt32();
                this.fileAlignment               = reader.ReadInt32();
                this.majorOperatingSystemVersion = reader.ReadInt16();
                this.minorOperatingSystemVersion = reader.ReadInt16();
                this.majorImageVersion           = reader.ReadInt16();
                this.minorImageVersion           = reader.ReadInt16();
                this.majorSubsystemVersion       = reader.ReadInt16();
                this.minorSubsystemVersion       = reader.ReadInt16();
                this.win32VersionValue           = reader.ReadInt32();
                this.sizeOfImage                 = reader.ReadInt32();
                this.sizeOfHeaders               = reader.ReadInt32();
                this.checkSum                    = reader.ReadInt32();
                this.subsystem                   = reader.ReadInt16();
                this.dllCharacteristics          = reader.ReadInt16();
                this.sizeOfStackReserve          = readIntPtr( reader );
                this.sizeOfStackCommit           = readIntPtr( reader );
                this.sizeOfHeapReserve           = readIntPtr( reader );
                this.sizeOfHeapCommit            = readIntPtr( reader );
                this.loaderFlags                 = reader.ReadInt32();

                int count = reader.ReadInt32();
                this.numberOfRvaAndSizes = count;

                DirectoryEntry[] directoryArray = new DirectoryEntry[count];
                this.dataDirectory = directoryArray;

                for(int i = 0; i < count; i++)
                {
                    directoryArray[i] = new DirectoryEntry( reader );
                }

                int iSizeOfOptionalHeader = this.isPe32Plus ? NTHeader.IMAGE_SIZEOF_NT_OPTIONAL32PLUS_HEADER : NTHeader.IMAGE_SIZEOF_NT_OPTIONAL32_HEADER;
                int iMagic                = this.isPe32Plus ? NTHeader.IMAGE_NT_OPTIONAL_HDR32PLUS_MAGIC     : NTHeader.IMAGE_NT_OPTIONAL_HDR32_MAGIC;

                // Verify that we have a valid NT header
                if(this.signature            != NTHeader.IMAGE_NT_SIGNATURE ||
                   this.sizeOfOptionalHeader != iSizeOfOptionalHeader       ||
                   this.magic                != iMagic                      ||
                   this.numberOfRvaAndSizes  != 16                           )
                {
                    throw new IllegalPEFormatException( "NT header problems" );
                }
            }

            // Output Routines

            internal void DumpLimitedToStream( LogWriter outputStream )
            {
                outputStream.WriteLine( "// Subsystem:                      {0:X8}", this.subsystem           );
                outputStream.WriteLine( "// Native entry point address:     {0:X8}", this.addressOfEntryPoint );
                outputStream.WriteLine( "// Image base:                     {0:X8}", this.imageBase           );
                outputStream.WriteLine( "// Section alignment:              {0:X8}", this.sectionAlignment    );
                outputStream.WriteLine( "// File alignment:                 {0:X8}", this.fileAlignment       );
                outputStream.WriteLine( "// Stack reserve size:             {0:X8}", this.sizeOfStackReserve  );
                outputStream.WriteLine( "// Stack commit size:              {0:X8}", this.sizeOfStackCommit   );
                outputStream.WriteLine( "// Directories:                    {0:X8}", this.numberOfRvaAndSizes );

                this.dataDirectory[DirectoryEntry.EXPORT                     ].DumpToStream( outputStream, "of Export directory "        );
                this.dataDirectory[DirectoryEntry.IMPORT                     ].DumpToStream( outputStream, "of Import directory"         );
                this.dataDirectory[DirectoryEntry.RESOURCE                   ].DumpToStream( outputStream, "of Resource directory"       );
                this.dataDirectory[DirectoryEntry.EXCEPTION                  ].DumpToStream( outputStream, "of Exception directory"      );
                this.dataDirectory[DirectoryEntry.SECURITY                   ].DumpToStream( outputStream, "of Security directory"       );
                this.dataDirectory[DirectoryEntry.BASERELOCATIONTABLE        ].DumpToStream( outputStream, "of Base Relocation Table"    );
                this.dataDirectory[DirectoryEntry.DEBUG                      ].DumpToStream( outputStream, "of Debug directory"          );
                this.dataDirectory[DirectoryEntry.ARCHITECTURE               ].DumpToStream( outputStream, "of Architecture specific"    );
                this.dataDirectory[DirectoryEntry.GLOBALPOINTER              ].DumpToStream( outputStream, "of Global pointer directory" );
                this.dataDirectory[DirectoryEntry.TLS                        ].DumpToStream( outputStream, "of TLS directory "           );
                this.dataDirectory[DirectoryEntry.LOADCONFIG                 ].DumpToStream( outputStream, "of Load config directory"    );
                this.dataDirectory[DirectoryEntry.BOUNDIMPORT                ].DumpToStream( outputStream, "of Bound import directory"   );
                this.dataDirectory[DirectoryEntry.IMPORTADDRESSTABLE         ].DumpToStream( outputStream, "of Import Address Table"     );
                this.dataDirectory[DirectoryEntry.DELAYLOADIMPORTADDRESSTABLE].DumpToStream( outputStream, "of Delay Load IAT"           );
                this.dataDirectory[DirectoryEntry.CLRHEADER                  ].DumpToStream( outputStream, "of CLR Header"               );
            }

            private long readIntPtr( ArrayReader reader )
            {
                if(this.isPe32Plus)
                {
                    return reader.ReadInt64();
                }
                else
                {
                    return (long)reader.ReadInt32();
                }
            }
        }

        internal struct DirectoryEntry
        {
            internal const int EXPORT                      =  0;
            internal const int IMPORT                      =  1;
            internal const int RESOURCE                    =  2;
            internal const int EXCEPTION                   =  3;
            internal const int SECURITY                    =  4;
            internal const int BASERELOCATIONTABLE         =  5;
            internal const int DEBUG                       =  6;
            internal const int ARCHITECTURE                =  7;
            internal const int GLOBALPOINTER               =  8;
            internal const int TLS                         =  9;
            internal const int LOADCONFIG                  = 10;
            internal const int BOUNDIMPORT                 = 11;
            internal const int IMPORTADDRESSTABLE          = 12;
            internal const int DELAYLOADIMPORTADDRESSTABLE = 13;
            internal const int CLRHEADER                   = 14;

            //
            // State
            //

            internal int virtualAddress;
            internal int size;

            //
            // Constructor Methods
            //

            internal DirectoryEntry( ArrayReader reader )
            {
                this.virtualAddress = reader.ReadInt32();
                this.size           = reader.ReadInt32();
            }

            // Output Routines

            internal void DumpToStream( LogWriter outputStream ,
                                        String    suffix       )
            {
                outputStream.WriteLine( "//     {0:X8} [{1:X8}] {2}", this.virtualAddress, this.size, suffix );
            }
        }

        public class Section
        {
            //
            // State
            //

            public SectionHeader header;
            byte[]               rawData;

            //
            // Constructor Methods
            //

            public Section( SectionHeader header )
            {
                this.header = header;
            }

            public void LoadSection( PELoader    peLoader ,
                                     ArrayReader reader   )
            {
                reader.Position = peLoader.VaToOffset( header.virtualAddress );

                this.rawData = reader.ReadUInt8Array( header.sizeOfRawData );
            }
        }

        public class SectionHeader
        {
            //
            // State
            //

            public String name;
            public int    virtualSize;
            public int    virtualAddress;
            public int    sizeOfRawData;
            public int    pointerToRawData;
            public int    pointerToRelocations;
            public int    pointerToLinenumbers;
            public short  numberOfRelocations;
            public short  numberOfLinenumbers;
            public int    characteristics;

            //
            // Constructor Methods
            //

            public SectionHeader( ArrayReader reader )
            {
                char[] chars = new char[8];
                for(int j = 0; j < 8; j++)
                {
                    chars[j] = (char)reader.ReadUInt8();
                }

                this.name                  = new String( chars );
                this.virtualSize           = reader.ReadInt32();
                this.virtualAddress        = reader.ReadInt32();
                this.sizeOfRawData         = reader.ReadInt32();
                this.pointerToRawData      = reader.ReadInt32();
                this.pointerToRelocations  = reader.ReadInt32();
                this.pointerToLinenumbers  = reader.ReadInt32();
                this.numberOfRelocations   = reader.ReadInt16();
                this.numberOfLinenumbers   = reader.ReadInt16();
                this.characteristics       = reader.ReadInt32();
            }
        }

        internal class COMHeader
        {
            //
            // State
            //

            // Corresponds to the WinNT IMAGE_COR20_HEADER data structure

            // Header Versioning
            internal int            cb;
            internal short          majorRuntimeVersion;
            internal short          minorRuntimeVersion;
            // Symbol table and startup information
            internal DirectoryEntry metaData;
            internal int            flags;
            internal int            entryPointToken;
            // Binding information
            internal DirectoryEntry resources;
            internal DirectoryEntry strongNameSignature;
            // Regular fixup and binding information
            internal DirectoryEntry codeManagerTable;
            internal DirectoryEntry vtableFixups;
            internal DirectoryEntry exportAddressTableJumps;
            // Managed Native Code
            internal DirectoryEntry managedNativeHeader;

            //
            // Constructor Methods
            //

            internal COMHeader( ArrayReader reader )
            {
                this.cb                      = reader.ReadInt32();
                this.majorRuntimeVersion     = reader.ReadInt16();
                this.minorRuntimeVersion     = reader.ReadInt16();
                this.metaData                = new DirectoryEntry( reader );
                this.flags                   = reader.ReadInt32();
                this.entryPointToken         = reader.ReadInt32();
                this.resources               = new DirectoryEntry( reader );
                this.strongNameSignature     = new DirectoryEntry( reader );
                this.codeManagerTable        = new DirectoryEntry( reader );
                this.vtableFixups            = new DirectoryEntry( reader );
                this.exportAddressTableJumps = new DirectoryEntry( reader );
                this.managedNativeHeader     = new DirectoryEntry( reader );

                // Verify that we have a valid header
                if(this.majorRuntimeVersion == 1 || this.majorRuntimeVersion > 2)
                {
                    throw new IllegalPEFormatException( "COM header problems" );
                }
            }

            internal void DumpHeader( LogWriter outputStream )
            {
                outputStream.WriteLine( "// CLR Header:"                                                                                                                              );
                outputStream.WriteLine( "// {0:X8} Header size"                                      , this.cb                                                                        );
                outputStream.WriteLine( "// {0:X4} Major runtime version"                            , this.majorRuntimeVersion                                                       );
                outputStream.WriteLine( "// {0:X4} Minor runtime version"                            , this.minorRuntimeVersion                                                       );
                outputStream.WriteLine( "// {0:X8} Flags"                                            , this.flags                                                                     );
                outputStream.WriteLine( "// {0:X8} Entrypoint token"                                 , this.entryPointToken                                                           );
                outputStream.WriteLine( "// {0:X8} [{1:X8}] address [size] of Metadata directory"    , this.metaData               .virtualAddress, this.metaData               .size );
                outputStream.WriteLine( "// {0:X8} [{1:X8}] address [size] of Resources directory"   , this.resources              .virtualAddress, this.resources              .size );
                outputStream.WriteLine( "// {0:X8} [{1:X8}] address [size] of Strong name signature" , this.strongNameSignature    .virtualAddress, this.strongNameSignature    .size );
                outputStream.WriteLine( "// {0:X8} [{1:X8}] address [size] of CodeManager table"     , this.codeManagerTable       .virtualAddress, this.codeManagerTable       .size );
                outputStream.WriteLine( "// {0:X8} [{1:X8}] address [size] of VTableFixups directory", this.vtableFixups           .virtualAddress, this.vtableFixups           .size );
                outputStream.WriteLine( "// {0:X8} [{1:X8}] address [size] of Export address table"  , this.exportAddressTableJumps.virtualAddress, this.exportAddressTableJumps.size );
                outputStream.WriteLine( "// {0:X8} [{1:X8}] address [size] of Precompile header"     , this.managedNativeHeader    .virtualAddress, this.managedNativeHeader    .size );
            }
        }

        internal class ImportDescriptor
        {
            // Size of this structure in stream

            internal const int SIZE = 20;

            //
            // State
            //

            internal int characteristics;
            internal int timeDateStamp;
            internal int forwarderChain;
            internal int name;
            internal int firstChunk;

            //
            // Constructor Methods
            //

            internal ImportDescriptor( ArrayReader reader )
            {
                this.characteristics = reader.ReadInt32();
                this.timeDateStamp   = reader.ReadInt32();
                this.forwarderChain  = reader.ReadInt32();
                this.name            = reader.ReadInt32();
                this.firstChunk      = reader.ReadInt32();
            }

            internal void DumpToStream( LogWriter outputStream )
            {
                outputStream.WriteLine( "//              {0:X8} Import Address Table"              , this.firstChunk      );
                outputStream.WriteLine( "//              {0:X8} Import Name Table"                 , this.name            );
                outputStream.WriteLine( "//              {0:X8} time date stamp"                   , this.timeDateStamp   );
                outputStream.WriteLine( "//              {0:X8} characteristics"                   , this.characteristics );
                outputStream.WriteLine( "//              {0:X8} index of first forwarder reference", this.forwarderChain  );
            }
        }

        public class IllegalPEFormatException : Exception
        {
            //
            // Constructor Methods
            //

            internal IllegalPEFormatException( String reason ) : base( reason )
            {
            }
        }

        public class MissingCLRheaderException : Exception
        {
            //
            // Constructor Methods
            //

            internal MissingCLRheaderException( String reason ) : base( reason )
            {
            }
        }
        public class NotYetImplemented : Exception
        {
            //
            // Constructor Methods
            //

            internal NotYetImplemented( String reason ) : base( reason )
            {
            }
        }
    }
}
