//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Elf
{
    using System;
    using System.Linq;


    public class RelocationEntry : IElfElement
    {
        internal Elf32_Rela         m_entryDef;
        internal Symbol             m_symbolRef;
        internal RelocationSection  m_parent;

        internal RelocationEntry(RelocationSection parent, RelocationType type,
            Symbol symRef, UInt32 sectionOffset)
        {
            m_parent            = parent;
            m_symbolRef         = symRef;
            m_entryDef.r_offset = sectionOffset;
            m_entryDef.r_info   = (uint)((symRef.m_index << 8) + (uint)type);

            m_symbolRef.ElementStatusChangedEvent += ReferencedSymbolStatusChanged;
        }

        internal RelocationEntry(RelocationSection parent, Elf32_Rel def)
        {
            m_parent   = parent;
            m_entryDef.r_info   = def.r_info;
            m_entryDef.r_offset = def.r_offset;
            m_entryDef.r_addend = 0;
        }

        internal RelocationEntry(RelocationSection parent, Elf32_Rela def)
        {
            m_parent = parent;
            m_entryDef = def;
        }

        public Elf32_Rela EntryDef
        {
            get
            {
                return m_entryDef;
            }
        }

        public RelocationType Type
        {
            get
            {
                return (RelocationType) m_entryDef.r_info;
            }
        }

        public UInt16 SymbolIndex
        {
            get
            {
                return (UInt16)(m_entryDef.r_info >> 8);
            }
        }

        public UInt32 SectionOffset
        {
            get
            {
                return m_entryDef.r_offset;
            }
        }

        public Symbol ReferencedSymbol
        {
            get
            {
                return m_symbolRef;
            }
        }

        //
        // Interface Implementation Methods
        //

        public void BuildReferences()
        {
            var symbol = m_parent.ReferencedSymbolTable[SymbolIndex];

            if (symbol != null)
            {
                m_symbolRef = symbol;
                m_symbolRef.ElementStatusChangedEvent += ReferencedSymbolStatusChanged;
            }
        }

        //
        // Event Notification Methods
        //

        private void ReferencedSymbolStatusChanged(Symbol symbol, ElfElementStatus status)
        {
            if (status == ElfElementStatus.IndexChanged)
            {
                m_entryDef.r_info = (UInt32) ((symbol.Index << 8) + (m_entryDef.r_info & -0xFF));
            }
        }

    }
}
