using System;
using System.Collections.Generic;

namespace Microsoft.binutils.elflib
{
    public class ElfSection : IElfElement, IElfElementStatusPublisher<ElfSection>
    {
        //
        // State
        //

        internal  ElfObject      m_parent;
        internal  string         m_name;
        internal  Elf32_Shdr     m_header;
        internal  UInt16         m_index;
        internal  long           m_elfOffset;
        
        protected byte[]         m_raw;

        internal List<RelocationSection> m_relocations;
        internal List<DebugInfoEntry>    m_dbgInfo;
        internal List<SectionReference>  m_references;
        internal Dictionary<string, uint> m_aliases;

        //
        // Constructor Methods
        //

        internal ElfSection(ElfObject parent, Elf32_Shdr hdr, 
            UInt16 index) : this(parent, hdr, index, new byte[0], 0)
        {
        }

        internal ElfSection(ElfObject parent, Elf32_Shdr hdr, 
            UInt16 index, byte[] raw, long elfOffset) : this(parent, raw)
        {
            m_header    = hdr;
            m_index     = index;
            m_elfOffset = elfOffset;
        }

        internal ElfSection(ElfObject parent, Elf32_Shdr hdr) : this(parent, new byte[0])
        {
            m_header = hdr;
        }

        internal ElfSection(ElfObject parent, byte[] raw) : this(parent)
        {
            m_raw    = raw;
        }

        internal ElfSection(ElfObject parent)
        {
            m_parent      = parent;
            m_relocations = new List<RelocationSection>();
            m_dbgInfo     = new List<DebugInfoEntry   >();
            m_references  = new List<SectionReference >();
            m_aliases     = new Dictionary<string,uint>();
        }

        //
        // Access Methods
        //

        public List<DebugInfoEntry> DebugEntries
        {
            get
            {
                return m_dbgInfo;
            }
        }

        public List<SectionReference> References
        {
            get
            {
                return m_references;
            }
        }

        public List<RelocationSection> Relocations
        {
            get
            {
                return m_relocations;
            }
        }

        public Dictionary<string, uint> Aliases
        {
            get
            {
                return m_aliases;
            }
        }

        public ElfObject Parent
        {
            get
            {
                return m_parent;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }

            set
            {
                var tbl = m_parent.SectionHeaderStringTable;

                if (tbl != null)
                {
                    m_header.sh_name = tbl.AddString(value);
                    m_name           = value;
                }
                else
                {
                    throw new ElfConsistencyException("Missing string table");
                }
            }
        }

        public Elf32_Shdr Header
        {
            get
            {
                return m_header;
            }
        }

        public UInt32 AddressInMemory
        {
            get
            {
                return m_header.sh_addr;
            }

            set
            {
                m_header.sh_addr = value;

                if( ElementStatusChangedEvent != null )
                {
                    ElementStatusChangedEvent( this, ElfElementStatus.AddressChanged );
                }

            }
        }

        public bool IsDataSection
        {
            get
            {
                sh_flags flags = ~(sh_flags.SHF_WRITE | sh_flags.SHF_MERGE | sh_flags.SHF_STRINGS);

                return sh_flags.SHF_ALLOC == ( m_header.sh_flags & flags );
            }
        }

        internal UInt16 Index
        {
            get
            {
                return m_index;
            }

            set
            {
                m_index = value;
                
                // Update subcribers about index change
                if (ElementStatusChangedEvent != null)
                {
                    ElementStatusChangedEvent(this, ElfElementStatus.IndexChanged);    
                }
            }
        }
        
        public virtual byte[] Raw
        {
            get
            {
                return m_raw;
            }
        }

        public void AddReference(RelocationEntry entry)
        {
            bool fAdded = false;

            foreach( SectionReference sr in m_references )
            {
                if( entry.m_symbolRef.m_sectionRef != null )
                {
                    int refIndex = entry.m_symbolRef.m_sectionRef.m_index;

                    if( refIndex == sr.Section.m_index && refIndex != 0 && sr.Section.m_name == entry.m_symbolRef.Name )
                    {
                        sr.CallOffsets.Add( new SectionReference.SymbolReference( entry.SectionOffset, entry, sr ) );
                        fAdded = true;
                        break;
                    }
                }
            }

            if( !fAdded )
            {
                if(entry.m_symbolRef.m_sectionRef != null)
                {
                    m_references.Add( new SectionReference( m_parent.Sections[entry.m_symbolRef.m_sectionRef.m_index], entry, entry.SectionOffset ) );
                }
                else
                {
                    m_references.Add( new SectionReference( m_parent.Sections[0], entry, entry.SectionOffset ) );
                }
            }
        }

        //
        // Interface Implementation
        //

        public event Action<ElfSection, ElfElementStatus> ElementStatusChangedEvent;

        public virtual void BuildReferences()
        {
            var tbl = m_parent.SectionHeaderStringTable;

            if (tbl != null)
            {
                m_name = tbl.GetString(m_header.sh_name);
            }
            else
            {
                throw new ElfConsistencyException("Missing string table");
            }
        }
    }
}