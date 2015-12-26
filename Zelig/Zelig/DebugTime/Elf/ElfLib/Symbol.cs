//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Elf
{
    using System;


    public class Symbol : IElfElement, IElfElementStatusPublisher<Symbol>
    {
        internal string         m_name;
        internal UInt16         m_index;
        internal Elf32_Sym      m_symbolDef;
        internal ElfSection     m_sectionRef;
        internal SymbolTable    m_parent;

        internal Symbol(SymbolTable parent, string name, UInt32 value, 
            UInt32 size, SymbolType type, SymbolBinding binding, 
            SymbolVisibility visibility, ElfSection sectionRef)
        {
            m_parent                = parent;
            Name                    = name;
            m_symbolDef.st_value    = value;
            m_symbolDef.st_size     = size;
            m_symbolDef.st_info     = (byte)(((byte)binding << 4) + (byte)type);
            m_symbolDef.st_other    = (byte)visibility;

            if (sectionRef != null)
            {
                m_symbolDef.st_shndx    = sectionRef.Index;
                m_sectionRef            = sectionRef;

                sectionRef.ElementStatusChangedEvent += SectionReferenceStatusChanged;    
            }
        }
        
        internal Symbol(SymbolTable parent, Elf32_Sym def, UInt16 index)
        {
            m_parent    = parent;
            m_symbolDef = def;
            m_index     = index;
        }

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                var tbl = m_parent.StringTable;

                if (tbl != null)
                {
                    m_symbolDef.st_name = m_parent.m_stringTable.AddString(value);
                    m_name = value;
                }
                else
                {
                    throw new ElfConsistencyException("Missing string table");
                }
            }
        }

        public UInt16 Index
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
        
        public Elf32_Sym SymbolDef
        {
            get
            {
                return m_symbolDef;
            }
        }

        public SymbolType Type
        {
            get
            {
                return (SymbolType)(m_symbolDef.st_info & 0xf);
            }
        }

        public SymbolBinding Binding
        {
            get
            {
                return (SymbolBinding)(m_symbolDef.st_info >> 4);
            }
        }

        public SymbolVisibility Visibility
        {
            get
            {
                return (SymbolVisibility)(m_symbolDef.st_other & 0x3);
            }
        }

        public ElfSection ReferencedSection
        {
            get
            {
                return m_sectionRef;
            }
        }

        //
        // Interface Implementation Methods
        //

        public event Action<Symbol, ElfElementStatus> ElementStatusChangedEvent;

        public void BuildReferences()
        {
            // Set name
            var tbl = m_parent.StringTable;

            if (tbl != null)
            {
                m_name = tbl.GetString(m_symbolDef.st_name);
            }
            else
            {
                throw new ElfConsistencyException("Missing string table");
            }

            // Set referenced section
            m_sectionRef = m_parent.m_parent[m_symbolDef.st_shndx];

            if (m_sectionRef != null)
            {
                m_sectionRef.ElementStatusChangedEvent += SectionReferenceStatusChanged;
            }
        }

        //
        // Event Notification Methods
        //

        private void SectionReferenceStatusChanged(ElfSection section, ElfElementStatus status)
        {
            if (status == ElfElementStatus.IndexChanged)
            {
                m_symbolDef.st_shndx = section.Index;
            }
        }       
    }
}
