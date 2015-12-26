//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Elf
{   
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;


    public class RelocationSection : ElfSection
    {
        internal List<RelocationEntry>  m_relocationEntries;
        internal ElfSection             m_sectionRef;
        internal SymbolTable            m_symTableRef;

        protected bool                  m_fDirtyRaw;

        internal RelocationSection(ElfObject parent, 
            ElfSection sectionRef, SymbolTable symTableRef) : base(parent)
        {
            m_relocationEntries = new List<RelocationEntry>();

            m_sectionRef  = sectionRef;
            m_sectionRef.ElementStatusChangedEvent += ReferencedSectionStatusChanged;

            m_symTableRef = symTableRef;
            m_symTableRef.ElementStatusChangedEvent += SymbolTableStatusChanged;

            m_header.sh_type    =        sh_type.SHT_REL;
            m_header.sh_entsize = (uint) Marshal.SizeOf(typeof (Elf32_Rel));
            m_header.sh_link    =        m_symTableRef.m_index;
            m_header.sh_info    =        m_sectionRef.m_index;

            m_fDirtyRaw         = true;
        }

        internal RelocationSection(ElfObject parent, Elf32_Shdr hdr, 
            UInt16 index, Elf32_Rel[] entries) : base(parent, hdr, index)
        {
            m_relocationEntries = new List<RelocationEntry>();

            for (int i = 0; i < entries.Length; i++)
            {
                m_relocationEntries.Add(new RelocationEntry(this, entries[i]));
            }

            m_fDirtyRaw = true;
        }

        internal RelocationSection(ElfObject parent, Elf32_Shdr hdr, 
            UInt16 index, Elf32_Rela[] entries) : base(parent, hdr, index)
        {
            m_relocationEntries = new List<RelocationEntry>();

            for (int i = 0; i < entries.Length; i++)
            {
                m_relocationEntries.Add(new RelocationEntry(this, entries[i]));
            }

            m_fDirtyRaw = true;
        }


        public RelocationEntry AddRelocationEntry(RelocationType type, Symbol symRef,
            UInt32 sectionOffset)
        {
            var entry = new RelocationEntry(this, type, symRef, sectionOffset);

            m_relocationEntries.Add(entry);

            m_fDirtyRaw = true;

            return entry;
        }

        public ElfSection ReferencedSection
        {
            get
            {
                return m_sectionRef;
            }
        }

        public SymbolTable ReferencedSymbolTable
        {
            get
            {
                return m_symTableRef;
            }
        }

        public RelocationEntry[] Entries
        {
            get
            {
                return m_relocationEntries.ToArray();
            }
        }

        public override byte[] Raw
        {
            get
            {
                if (!m_fDirtyRaw && (m_raw != null))
                {
                    return m_raw;
                }

                var symSize = Marshal.SizeOf(typeof (Elf32_Rel));

                var raw = new byte[symSize * m_relocationEntries.Count];

                var ptr = Marshal.AllocHGlobal(symSize);

                for (int i = 0; i < m_relocationEntries.Count; i++)
                {
                    Marshal.StructureToPtr(m_relocationEntries[i].m_entryDef, ptr, false);

                    Marshal.Copy(ptr, raw, i * symSize, symSize);
                }

                Marshal.FreeHGlobal(ptr);

                m_fDirtyRaw = false;

                return m_raw = raw;
            }
        }    

        //
        // Interface Implementation Methods
        //

        public override void BuildReferences()
        {
            // Set symbol table reference
            var tbl = m_parent[m_header.sh_link];

            if (tbl != null && tbl is SymbolTable)
            {
                m_symTableRef = (SymbolTable) tbl;
                tbl.ElementStatusChangedEvent += SymbolTableStatusChanged;
            }

            // Set section table reference
            var section = m_parent[m_header.sh_info];

            if (section != null)
            {
                m_sectionRef = section;
                section.ElementStatusChangedEvent += ReferencedSectionStatusChanged;
            }

            // Build all references for all relocation entries
            foreach (var entry in m_relocationEntries)
            {
                entry.BuildReferences();
            }

            base.BuildReferences();
        }

        //
        // Event Notification Methods
        //

        private void SymbolTableStatusChanged(ElfSection section, ElfElementStatus status)
        {
            if (status == ElfElementStatus.IndexChanged)
            {
                m_header.sh_link = section.Index;
            }
        }

        private void ReferencedSectionStatusChanged(ElfSection section, ElfElementStatus status)
        {
            if (status == ElfElementStatus.IndexChanged)
            {
                m_header.sh_info = section.Index;
            }
        }
    }
}
