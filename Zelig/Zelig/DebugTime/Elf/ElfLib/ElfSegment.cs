//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Elf
{
    using System;
    using System.Collections.Generic;

    //
    // We assume that our ELF file has section headers, even for an 'EXEC' type file.
    // We use section references to build up our view of what a segment contains.
    // It is possible to have ELF 'EXEC' type files without any section headers. Although
    // in practice it is not normally seen. Be aware of this fact when building en execution
    // view ELF file. Build the binary portion of the file out of sections that get 
    // referenced by the segments, even if the sections serve no other purpose.
    // 
    public class ElfSegment : IElfElement
    {
        private  ElfObject          m_parent;
        internal Elf32_Phdr         m_header;
        internal List<ElfSection>   m_sectionRefs;

        internal ElfSegment(ElfObject parent, UInt32 addressInMemory) : this()
        {
            m_parent = parent;

            m_header.p_vaddr = addressInMemory;
            m_header.p_paddr = addressInMemory;

            m_header.p_type  = SegmentType.PT_LOAD;
            m_header.p_flags = SegmentFlag.PF_R & SegmentFlag.PF_W & SegmentFlag.PF_X;
        }

        internal ElfSegment(ElfObject parent, Elf32_Phdr hdr) : this()
        {
            m_parent = parent;
            m_header = hdr;
        }

        private ElfSegment()
        {
            m_sectionRefs = new List<ElfSection>();
        }

        public void AddReferencedSection(ElfSection section)
        {
            if (!m_sectionRefs.Contains(section))
            {
                m_sectionRefs.Add(section);
            }
        }

        public Elf32_Phdr Header
        {
            get
            {
                return m_header;
            }
        }

        public SegmentType Type
        {
            get
            {
                return m_header.p_type;
            }
        }

        public UInt32 AddressInMemory
        {
            get
            {
                return m_header.p_vaddr;
            }
            set
            {
                m_header.p_vaddr = value;
                m_header.p_paddr = value;
            }
        }

        public ElfSection[] ReferencedSections
        {
            get
            {
                m_sectionRefs.Sort(CompareReferencedSectionsByAddress);
                return m_sectionRefs.ToArray();
            }
        }

        internal static int CompareReferencedSectionsByAddress(ElfSection x, ElfSection y)
        {
            if (x == y)
            {
                return 0;
            }

            if (x.m_header.sh_addr <= y.m_header.sh_addr)
            {
                return -1;
            }

            return 1;
        }

        public void BuildReferences()
        {
            // Build list of referenced sections
            foreach (var section in m_parent.Sections)
            {
                // Compare allocated sections by VMA
                if ((section.Header.sh_flags & sh_flags.SHF_ALLOC) != 0)
                {
                    if (section.m_header.sh_addr >= m_header.p_vaddr && 
                        (section.m_header.sh_addr + section.m_header.sh_size) <= (m_header.p_vaddr + m_header.p_memsz))
                    {
                        m_sectionRefs.Add(section);    
                    }
                }
                // Otherwise, compare by file offsets
                else
                {
                    if (section.m_header.sh_offset >= m_header.p_offset &&
                      (section.m_header.sh_offset + section.m_header.sh_size) <= (m_header.p_offset + m_header.p_filesz))
                    {
                        m_sectionRefs.Add(section);
                    }    
                }
            }
        }
    }
}
