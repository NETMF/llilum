//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Elf
{
    using System;
    using System.Collections.Generic;

    public class SectionReference
    {
        public class SymbolReference
        {
            public uint Offset;
            public RelocationEntry RelocationRef;
            public SectionReference Owner;

            public SymbolReference(uint offset, RelocationEntry relocationRef, SectionReference owner)
            {
                Offset = offset;
                RelocationRef = relocationRef;
                Owner = owner;
            }
        }

        internal SectionReference(ElfSection sec, RelocationEntry relocRef, uint callOffset)
            : this( sec, relocRef, callOffset, 0 )
        {
        }

        internal SectionReference(ElfSection sec, RelocationEntry relocRef, uint callOffset, uint sectionOffset)
        {
            Section = sec;
            SectionName = relocRef.m_symbolRef.Name;

            SectionOffset = sectionOffset;
            CallOffsets = new List<SymbolReference>();
            FinalAddress = 0;

            DataBaseOffset = callOffset;

            CallOffsets.Add( new SymbolReference( callOffset, relocRef, this ) );

            sec.ElementStatusChangedEvent += new Action<ElfSection, ElfElementStatus>( sec_ElementStatusChangedEvent );
        }

        void sec_ElementStatusChangedEvent(ElfSection section, ElfElementStatus action)
        {
            if( action == ElfElementStatus.AddressChanged )
            {
                FinalAddress = section.Header.sh_addr;
            }
        }

        public ElfSection Section;
        public string SectionName;
        public uint SectionOffset;
        public readonly uint DataBaseOffset;
        public List<SymbolReference> CallOffsets;
        public uint FinalAddress;
    }
}