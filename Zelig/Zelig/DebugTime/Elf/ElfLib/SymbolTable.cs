//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Elf
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;


    public class SymbolTable : ElfSection
    {
        internal List<Symbol>   m_symbols;
        internal List<string>   m_buildAttribs;
        internal StringTable    m_stringTable;

        private  UInt16         m_nextSymbolIndex;

        protected bool          m_fDirtyRaw;

        internal SymbolTable(ElfObject parent, StringTable stringTable) : base(parent)
        {
            m_symbols           =        new List<Symbol>();
            m_buildAttribs      =        new List<string>();

            m_header.sh_type    =        sh_type.SHT_SYMTAB;
            m_header.sh_link    =        stringTable.Index;   
            m_header.sh_entsize = (uint) Marshal.SizeOf(typeof (Elf32_Sym));

            m_stringTable       = stringTable;
            
            m_stringTable.ElementStatusChangedEvent += StringTableStatusChanged;

            m_nextSymbolIndex   = 0;
            
            m_fDirtyRaw         = true;
        }

        internal SymbolTable(ElfObject parent, Elf32_Shdr hdr, 
            UInt16 index, Elf32_Sym[] symbolDefs) : base(parent, hdr, index)
        {
            m_symbols = new List<Symbol>();

            for (UInt16 i = 0; i < symbolDefs.Length; i++)
            {
                m_symbols.Add(new Symbol(this, symbolDefs[i], i));
            }

            m_nextSymbolIndex = (UInt16)symbolDefs.Length;
            m_buildAttribs    = new List<string>();
            m_fDirtyRaw       = true;
        }

        public Symbol AddSymbol(string name, UInt32 value, UInt32 size, SymbolType type, 
            SymbolBinding binding, SymbolVisibility visibility, ElfSection sectionRef)
        {
            var symbol = new Symbol(this, name, value, size, type,
                                    binding, visibility, sectionRef);

            symbol.m_index = m_nextSymbolIndex++;

            m_symbols.Add(symbol);

            m_fDirtyRaw = true;

            return symbol;
        }

        public Symbol[] Symbols
        {
            get
            {
                m_symbols.Sort(CompareSymbolsByIndex);
                return m_symbols.ToArray();
            }
        }

        public List<string> BuildAttributes
        {
            get
            {
                return m_buildAttribs;
            }
        }

        public Symbol this[UInt16 index]
        {
            get
            {
                foreach (var symbol in m_symbols)
                {
                    if (symbol.Index == index)
                    {
                        return symbol;
                    }
                }

                return null;
            }
        }

        public StringTable StringTable
        {
            get
            {
                return m_stringTable;
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

                var symSize = Marshal.SizeOf(typeof (Elf32_Sym));

                var raw = new byte[symSize * m_symbols.Count];

                var ptr = Marshal.AllocHGlobal(symSize);

                for (int i = 0; i < m_symbols.Count; i++)
                {
                    Marshal.StructureToPtr(m_symbols[i].m_symbolDef, ptr, false);

                    Marshal.Copy(ptr, raw, i * symSize, symSize);
                }

                Marshal.FreeHGlobal(ptr);

                m_fDirtyRaw = false;

                return m_raw = raw;
            }
        }

        internal static int CompareSymbolsByIndex(Symbol x, Symbol y)
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

        //
        // Interface Implementation Methods
        //
        
        public override void BuildReferences()
        {
            // Set string table reference
            var tbl = m_parent[m_header.sh_link];

            if (tbl != null && tbl is StringTable)
            {
                m_stringTable = (StringTable) tbl;
                tbl.ElementStatusChangedEvent += StringTableStatusChanged;
            }

            // Build all references for all symbols
            foreach (var symbol in m_symbols)
            {
                symbol.BuildReferences();

                if( symbol.Name.StartsWith( "BuildAttributes" ) )
                {
                    m_buildAttribs.AddRange( symbol.Name.Split( new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries ) );
                }

                if( symbol.m_sectionRef != null && symbol.m_sectionRef.m_index != 0 && symbol.m_sectionRef.m_name != symbol.Name)
                {
                    symbol.m_sectionRef.Aliases[symbol.Name] = symbol.SymbolDef.st_value;
                }
            }

            base.BuildReferences();
        }

        //
        // Event Notification Methods
        //

        private void StringTableStatusChanged(ElfSection section, ElfElementStatus status)
        {
            if (status == ElfElementStatus.IndexChanged)
            {
                m_header.sh_link = section.Index;   
            }   
        }
    }
}
