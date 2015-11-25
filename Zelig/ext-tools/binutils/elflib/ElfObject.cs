using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.binutils.elflib
{

    public partial class ElfObject : IElfElement
    {
        //
        // State
        //

        private  Elf32_Ehdr           m_header;
        private  string               m_file;
        internal List<ElfSection>     m_sections;
        internal List<ElfSection>     m_groups;
        internal List<ElfSection>     m_dataEntries;
        private  StringTable          m_sectionHeaderStringTable;
        internal SymbolTable          m_symbolTable;
        private  List<ElfSegment>     m_segments;
        private  List<DebugInfoEntry> m_globalDbgInfo;

        private  UInt16               m_nextSectionIndex;

        //
        // Constructor Method
        //

        private ElfObject()
        {
            m_sections = new List<ElfSection>();

            m_segments = new List<ElfSegment>();

            m_groups = new List<ElfSection>();

            m_globalDbgInfo = new List<DebugInfoEntry>();

            m_dataEntries = new List<ElfSection>();

            m_nextSectionIndex = 0;
        }
        
        //
        // Helper Methods
        //

        public ElfSection AddBinarySection(byte[] raw)
        {
            var section = new ElfSection(this, raw);

            section.m_header.sh_type      = sh_type.SHT_PROGBITS;
            section.m_header.sh_flags     = sh_flags.SHF_ALLOC | sh_flags.SHF_EXECINSTR;
            section.m_header.sh_addralign = 4;

            AddSection(section);
            
            return section;
        }

        public RelocationSection AddRelocationSection(ElfSection sectionRef, SymbolTable symTableRef)
        {
            var section = new RelocationSection(this, sectionRef, symTableRef);

            AddSection(section);

            return section;
        }

        private void AddSection(ElfSection section)
        {
            if (!m_sections.Contains(section))
            {
                // Increment the number of section headers the Elf knows about
                m_header.e_shnum++;

                // Set the index.
                section.Index = m_nextSectionIndex++;

                // Add the section to the list
                m_sections.Add(section);
            }
            else
            {
                throw new ElfConsistencyException("Cannot add duplicate section to Elf");
            }
        }

        public ElfSegment AddLoadableSegment(UInt32 addressInMemory)
        {
            var segment = new ElfSegment(this, addressInMemory);

            m_segments.Add(segment);

            return segment;
        }

        private static int CompareSectionsByIndex(ElfSection x, ElfSection y)
        {
            if (x == y)
            {
                return 0;
            }

            if (x.m_index <= y.m_index)
            {
                return -1;
            }

            return 1;
        }

        private static int CompareSegmentsByAddress(ElfSegment x, ElfSegment y)
        {
            if (x == y)
            {
                return 0;
            }

            if (x.m_header.p_vaddr <= y.m_header.p_vaddr)
            {
                return -1;
            }

            return 1;
        }

        //
        // Access Methods
        //
        
        internal ElfSegment[] GetSortedSegmentList()
        {
            var tempList = new List<ElfSegment>();

            foreach (var segment in m_segments)
            {
                if (segment.Type == SegmentType.PT_LOAD)
                {
                    tempList.Add(segment);
                }
            }

            tempList.Sort(CompareSegmentsByAddress);

            foreach (var segment in m_segments)
            {
                if (!tempList.Contains(segment))
                {
                    tempList.Add(segment);
                }
            }

            return tempList.ToArray();
        }
        
        public Elf32_Ehdr Header
        {
            get
            {
                return m_header;
            }
        }

        public ElfSection[] DataEntries
        {
            get
            {
                m_dataEntries.Sort( CompareSectionsByIndex );
                return m_dataEntries.ToArray();
            }
        }

        public ElfSection[] Sections
        {
            get
            {
                m_sections.Sort(CompareSectionsByIndex);
                return m_sections.ToArray();
            }
        }        

        public ElfSegment[] Segments
        {
            get
            {
                return m_segments.ToArray();
            }
        }

        public RelocationSection[] RelocationSections
        {
            get
            {
                var list = new List<RelocationSection>();

                foreach (var section in m_sections)
                {
                    if(section is RelocationSection)
                    {
                        list.Add((RelocationSection)section);
                    }
                }

                return list.ToArray();
            }
        }

        public ElfSection this[uint index]
        {
            get
            {
                foreach (var section in m_sections)
                {
                    if (section.Index == index)
                    {
                        return section;
                    }
                }

                return null;
            }
        }

        public ElfSection this[string name]
        {
            get
            {
                foreach (var section in m_sections)
                {
                    if (section.Name == name)
                    {
                        return section;
                    }
                }

                return null;
            }
        }

        public string FileName
        {
            get
            {
                return m_file;
            }

            set
            {
                m_file = value;
            }
        }

        public UInt32 EntryPoint
        {
            get
            {
                return m_header.e_entry;
            }
            
            set
            {
                m_header.e_entry = value;
            }
        }

        public SymbolTable SymbolTable
        {
            get
            {
                return m_symbolTable;
            }
            set
            {
                if (m_symbolTable == null)
                {
                    m_symbolTable = value;
                }
                else
                {
                    throw new ElfConsistencyException("Elf already contains a symbol table");
                }
            }
        }

        internal StringTable SectionHeaderStringTable
        {
            get
            {
                return m_sectionHeaderStringTable;
            }

            set
            {
                if (m_sectionHeaderStringTable == null)
                {
                    m_sectionHeaderStringTable = value;

                    m_header.e_shtrndx = value.Index;

                    value.ElementStatusChangedEvent += SectionStringTableStatusChanged;
                }
                else
                {
                    throw new ElfConsistencyException("Elf already contains a section header string table");
                }
            }
        }

        //
        // Interface Implementations
        //

        public void BuildReferences()
        {
            Dictionary<long  , ElfSection              > secMap       = new Dictionary<long  , ElfSection              >();
            Dictionary<string, ElfSection              > secNameMap   = new Dictionary<string, ElfSection              >();
            Dictionary<long  , DebugInfoEntry          > dbgMap       = new Dictionary<long  , DebugInfoEntry          >();
            Dictionary<string, DebugInfoEntry          > dbgNameMap   = new Dictionary<string, DebugInfoEntry          >();
            Dictionary<long  , DebugLineEntry          > dbgLineMap   = new Dictionary<long  , DebugLineEntry          >();
            Dictionary<uint  , DebugInfoCompUnitSection> globalLookup = new Dictionary<uint  , DebugInfoCompUnitSection>();
            DebugInfoAbbrev                              debugAbbrev  = new DebugInfoAbbrev();

            // Set section header string table reference
            var tbl = this[m_header.e_shtrndx];

            if (tbl != null && tbl is StringTable)
            {
                m_sectionHeaderStringTable = (StringTable) tbl;
                tbl.ElementStatusChangedEvent += SectionStringTableStatusChanged;
            }
            
            // Build all references for all sections
            foreach (var section in m_sections)
            {
                section.BuildReferences();

                // Set symboltable reference if we have one
                if (section is SymbolTable)
                {
                    m_symbolTable = (SymbolTable) section;
                }
                //else if( section.Name.StartsWith( ".debug_abbrev" ) )
                //{
                //    debugAbbrev.ParseDebugInfoSection( section );
                //}
                //else if( section is RelocationSection )
                //{
                //    RelocationSection reloc = (RelocationSection)section;

                //    if( reloc.m_name.Contains( ".debug_info" ))
                //    {
                //        dbgMap[reloc.m_header.sh_info].m_relocations.Add( reloc );
                //    }
                //    else if( reloc.m_name.Contains( ".debug_line" ) )
                //    {
                //        dbgLineMap[reloc.m_header.sh_info].m_relocations.Add( reloc );
                //    }
                //    else if( !section.m_name.Contains( ".debug_" ) )
                //    {
                //        ElfSection sec = secMap[reloc.m_header.sh_info];

                //        sec.m_relocations.Add( reloc );
                //    }
                //}
                //else if( section.m_name.StartsWith( ".debug_info" ) )
                //{
                //    string         name = section.Name.Replace( ".debug_info", "" );
                //    name = name.TrimStart( '$' );
                //    DebugInfoEntry die  = new DebugInfoEntry( section, debugAbbrev, globalLookup );

                //    dbgMap    [section.m_index] = die;
                //    dbgNameMap[name           ] = die;

                //    if( secNameMap.ContainsKey( name ) )
                //    {
                //        secNameMap[name].DebugEntries.Add( die );
                //    }
                //    else
                //    {
                //        m_globalDbgInfo.Add( die );
                //    }
                //}
                //else if( section.Name.StartsWith( ".debug_line" ) )
                //{
                //    string         name = section.Name.Replace( ".debug_line", "" );
                //    DebugLineEntry dle = new DebugLineEntry( section );
                //    name = name.TrimStart( '$' );

                //    dbgNameMap[name           ].m_lines.Add( dle );
                //    dbgLineMap[section.m_index] = dle;
                //}
                else
                {
                    secMap[section.m_index] = section;
                    secNameMap[section.m_name] = section;

                    foreach(string alias in section.Aliases.Keys)
                    {
                        secNameMap[alias] = section;
                    }

                    if(section.IsDataSection)
                    {
                        m_dataEntries.Add( section );
                    }
                }
            }

            foreach(string key in dbgNameMap.Keys)
            {
                if(secNameMap.ContainsKey( key ))
                {
                    if(!secNameMap[key].DebugEntries.Contains(dbgNameMap[key]))
                    {
                        secNameMap[key].DebugEntries.Add( dbgNameMap[key] );
                    }
                }
            }

            // Rename group sections because they can use the same name as the 
            // symbols (TODO: Fix this later by identifying only symbols not 
            // groups.
            foreach(ElfSection sec in m_groups)
            {
                sec.m_name = ".group." + sec.m_name;
            }

            foreach( ElfSection sec in m_sections )
            {
                foreach( RelocationSection rs in sec.m_relocations )
                {
                    foreach( RelocationEntry re in rs.Entries )
                    {
                        if( re.Type != RelocationType.R_ARM_V4BX )
                        {
                            sec.AddReference( re );
                        }
                    }

                }
            }

            // build global references
            foreach( DebugInfoEntry die in dbgMap.Values )
            {
                die.ResolveGlobalReferences( secNameMap, globalLookup );
            }


            // Build all references for all segments
            foreach (var segment in m_segments)
            {
                segment.BuildReferences();
            }
        }

        private void SectionStringTableStatusChanged(ElfSection section, ElfElementStatus status)
        {
            if (status == ElfElementStatus.IndexChanged)
            {
                m_header.e_shtrndx = section.Index;
            }
        }
    }
}
