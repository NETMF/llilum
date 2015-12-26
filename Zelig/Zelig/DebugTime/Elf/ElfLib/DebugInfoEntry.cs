using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace Microsoft.Zelig.Elf
{
    public struct DebugInfoAbbrevAttrib
    {
        public Dwarf2_Attribute Attrib;
        public Dwarf2_Form      Form;

        public DebugInfoAbbrevAttrib(BinaryReader br)
        {
            Attrib = (Dwarf2_Attribute)LEB128.DecodeUnsigned( br );
            Form   = (Dwarf2_Form     )LEB128.DecodeUnsigned( br );
        }
    }

    public class DebugInfoAbbrevCompUnit
    {
        public UInt32 m_index;
        public Dwarf2_TAG m_tag;
        public byte m_childLevel;
        public List<DebugInfoAbbrevAttrib> m_attribs;

        public DebugInfoAbbrevCompUnit(uint abbrevCode, BinaryReader br)
        {
            m_tag = (Dwarf2_TAG)LEB128.DecodeUnsigned( br );
            m_childLevel = br.ReadByte();
            m_attribs = new List<DebugInfoAbbrevAttrib>();

            while(true)
            {
                DebugInfoAbbrevAttrib attrib = new DebugInfoAbbrevAttrib( br );
                
                if( attrib.Attrib == 0 && attrib.Form == 0 ) break;

                m_attribs.Add( attrib );
            }
        }
    }

    public class DebugInfoAbbrev
    {
        public Dictionary<uint, DebugInfoAbbrevCompUnit> m_compUnitEntries;

        public DebugInfoAbbrevCompUnit this[uint index]
        {
            get
            {
                if( m_compUnitEntries.ContainsKey( index ) )
                {
                    return m_compUnitEntries[index];
                }

                return null;
            }
        }

        public DebugInfoAbbrev()
        {
            m_compUnitEntries = new Dictionary<uint, DebugInfoAbbrevCompUnit>();
        }

        public void ParseDebugInfoSection(ElfSection debugInfoSection)
        {
            byte[] raw = debugInfoSection.Raw;

            m_compUnitEntries = new Dictionary<uint, DebugInfoAbbrevCompUnit>();

            using(MemoryStream ms = new MemoryStream(raw))
            using(BinaryReader br = new BinaryReader(ms))
            {
                while( ms.Position < ms.Length )
                {
                    uint abbrevCode = LEB128.DecodeUnsigned( br );

                    if( abbrevCode != 0 )
                    {
                        m_compUnitEntries[abbrevCode] = new DebugInfoAbbrevCompUnit( abbrevCode, br );
                    }
                }
            }
        }
    }

    public class DebugInfoCompUnitSection
    {
        public uint m_abbrevCode;
        public Dictionary<Dwarf2_Attribute, object> m_properties;
        public List<DebugInfoCompUnitSection> m_children;
        public List<Dwarf2_Attribute> m_unresolvedLocal;
        public List<Dwarf2_Attribute> m_unresolvedGlobal;
        DebugInfoCompUnit m_owner;

        public DebugInfoCompUnitSection(DebugInfoCompUnit owner)
        {
            m_abbrevCode = 0;
            m_owner = owner;
        }
        
        public DebugInfoCompUnitSection(uint abbrevCode, BinaryReader br, DebugInfoAbbrev abbrev, DebugInfoCompUnit owner, Dictionary<uint,DebugInfoCompUnitSection> addressMap)
        {
            m_abbrevCode = abbrevCode;
            m_properties = new Dictionary<Dwarf2_Attribute, object>();
            m_children   = new List<DebugInfoCompUnitSection>();
            m_unresolvedLocal  = new List<Dwarf2_Attribute>();
            m_unresolvedGlobal = new List<Dwarf2_Attribute>();
            m_owner = owner;

            DebugInfoAbbrevCompUnit cu = abbrev[abbrevCode];

            if( cu != null )
            {
                //Console.WriteLine( "Tag   : " + cu.m_tag );

                ParseAttributes( br, cu, owner );

                if( cu.m_childLevel != 0 )
                {
                    uint childAbbrevCode;

                    do
                    {
                        uint addr = (uint)(br.BaseStream.Position - m_owner.m_streamOffset + owner.m_offset);

                        childAbbrevCode = LEB128.DecodeUnsigned( br );

                        if( childAbbrevCode != 0 )
                        {
                            DebugInfoCompUnitSection sec = new DebugInfoCompUnitSection( childAbbrevCode, br, abbrev, owner, addressMap );
                            m_children.Add( sec );
                            addressMap[addr] = sec;
                        }
                        else
                        {
                            addressMap[addr] = new DebugInfoCompUnitSection( owner );
                        }
                    } 
                    while( childAbbrevCode != 0 );
                }
            }
        }

        private void ParseAttributes(BinaryReader bs, DebugInfoAbbrevCompUnit abbrev, DebugInfoCompUnit owner)
        {
            foreach( DebugInfoAbbrevAttrib attrib in abbrev.m_attribs )
            {
                //Console.WriteLine( "Attrib: " + attrib.Attrib.ToString() );
                Dwarf2_Form form = attrib.Form;

            reparse_form:
                //Console.WriteLine( "Form  : " + form.ToString() );
                switch( form )
                {
                    case Dwarf2_Form.DW_FORM_string:
                        m_properties[attrib.Attrib] = Dwarf_Utility.DwarfReadString( bs );
                        break;

                    case Dwarf2_Form.DW_FORM_addr:
                        if( owner.m_header.uh_addressSize == 4 )
                        {
                            m_properties[attrib.Attrib] = bs.ReadUInt32();
                        }
                        else if( owner.m_header.uh_addressSize == 2 )
                        {
                            m_properties[attrib.Attrib] = bs.ReadUInt16();
                        }
                        break;

                    case Dwarf2_Form.DW_FORM_block:
                        {
                            uint size = LEB128.DecodeUnsigned( bs );
                            m_properties[attrib.Attrib] = size;
                            bs.BaseStream.Seek( size, SeekOrigin.Current );
                        }
                        break;

                    case Dwarf2_Form.DW_FORM_block1:
                        {
                            uint size = bs.ReadByte();

                            m_properties[attrib.Attrib] = size;

                            bs.BaseStream.Seek( size, SeekOrigin.Current );
                        }
                        break;

                    case Dwarf2_Form.DW_FORM_block2:
                        {
                            uint size = bs.ReadUInt16();

                            m_properties[attrib.Attrib] = size;

                            bs.BaseStream.Seek( size, SeekOrigin.Current );
                        }
                        break;
                    case Dwarf2_Form.DW_FORM_block4:
                        {
                            uint size = bs.ReadUInt32();

                            m_properties[attrib.Attrib] = size;

                            bs.BaseStream.Seek( size, SeekOrigin.Current );
                        }
                        break;
                    case Dwarf2_Form.DW_FORM_data1:
                        m_properties[attrib.Attrib] = bs.ReadByte();
                        break;
                    case Dwarf2_Form.DW_FORM_data2:
                        m_properties[attrib.Attrib] = bs.ReadInt16();
                        break;
                    case Dwarf2_Form.DW_FORM_data4:
                        m_properties[attrib.Attrib] = bs.ReadInt32();
                        break;
                    case Dwarf2_Form.DW_FORM_data8:
                        m_properties[attrib.Attrib] = bs.ReadUInt64();
                        break;
                    case Dwarf2_Form.DW_FORM_flag:
                        m_properties[attrib.Attrib] = bs.ReadByte();
                        break;
                    case Dwarf2_Form.DW_FORM_indirect:
                        {
                            form = (Dwarf2_Form)LEB128.DecodeUnsigned( bs );
                            //Console.WriteLine( "Indir : " + form.ToString() );

                            goto reparse_form;
                        }
                    case Dwarf2_Form.DW_FORM_ref_addr:
                        // TODO: FIND OUT HOW TO MAKE THIS WORK PROPERLY
                        m_properties[attrib.Attrib] =  (uint)(bs.ReadUInt32() + m_owner.m_owner.m_elfOffset);
                        m_unresolvedGlobal.Add( attrib.Attrib );
                        break;
                    case Dwarf2_Form.DW_FORM_ref_udata:
                        m_properties[attrib.Attrib] = (uint)(LEB128.DecodeUnsigned( bs ) + owner.m_offset);
                        m_unresolvedLocal.Add( attrib.Attrib );
                        break;
                    case Dwarf2_Form.DW_FORM_ref1:
                        m_properties[attrib.Attrib] = (uint)(bs.ReadByte() + owner.m_offset);
                        m_unresolvedLocal.Add( attrib.Attrib );
                        break;
                    case Dwarf2_Form.DW_FORM_ref2:
                        m_properties[attrib.Attrib] = (uint)(bs.ReadInt16() + owner.m_offset);
                        m_unresolvedLocal.Add( attrib.Attrib );
                        break;
                    case Dwarf2_Form.DW_FORM_ref4:
                        m_properties[attrib.Attrib] = (uint)(bs.ReadInt32() + owner.m_offset);
                        m_unresolvedLocal.Add( attrib.Attrib );
                        break;
                    case Dwarf2_Form.DW_FORM_ref8:
                        m_properties[attrib.Attrib] = (uint)(bs.ReadInt64() + owner.m_offset);
                        m_unresolvedLocal.Add( attrib.Attrib );
                        break;
                    case Dwarf2_Form.DW_FORM_sdata:
                        m_properties[attrib.Attrib] = LEB128.DecodeSigned( bs );
                        break;
                    case Dwarf2_Form.DW_FORM_strp:
                        break;
                    case Dwarf2_Form.DW_FORM_udata:
                        m_properties[attrib.Attrib] = LEB128.DecodeUnsigned( bs );
                        break;


                    case Dwarf2_Form.DW_FORM_sec_offset:
                        m_properties[attrib.Attrib] = LEB128.DecodeUnsigned( bs );
                        break;

                    case Dwarf2_Form.DW_FORM_exprloc:
                        m_properties[attrib.Attrib] = LEB128.DecodeUnsigned( bs );
                        break;

                    case Dwarf2_Form.DW_FORM_flag_present:
                        m_properties[attrib.Attrib] = LEB128.DecodeUnsigned( bs );
                        break;

                    case Dwarf2_Form.DW_FORM_ref_sig8:
                        break;


                    default:
                        break;
                }
                //Console.WriteLine( "Value : " + m_properties[attrib.Attrib].ToString() );
                //Console.WriteLine();
            }
        }

        public void ResolveLocalReferences(Dictionary<uint, DebugInfoCompUnitSection> localLookup)
        {
            if( m_unresolvedLocal != null )
            {
                foreach( Dwarf2_Attribute unres in m_unresolvedLocal )
                {
                    uint addr = (uint)m_properties[unres];

                    if( localLookup.ContainsKey( addr ) )
                    {
                        m_properties[unres] = localLookup[addr];
                    }
                    else
                    {
                        //Console.WriteLine( "Could not resolve: " + unres );
                    }
                }
            }

            //if(m_children != null)
            //{
            //    foreach( DebugInfoCompUnitSection sec in m_children )
            //    {
            //        sec.ResolveLocalReferences( localLookup );
            //    }
            //}
        }
        public void ResolveGlobalReferences(Dictionary<uint, DebugInfoCompUnitSection> globalLookup)
        {
            if( m_unresolvedGlobal != null )
            {
                foreach( Dwarf2_Attribute unres in m_unresolvedGlobal )
                {
                    if(!(m_properties[unres] is DebugInfoCompUnitSection))
                    {
                        uint addr = (uint)m_properties[unres];

                        if(globalLookup.ContainsKey( addr ))
                        {
                            m_properties[unres] = globalLookup[addr];
                        }
                        else
                        {
                            //Console.WriteLine( "Could not resolve: " + unres );
                        }
                    }
                }
            }

            if( m_children != null )
            {
                foreach( DebugInfoCompUnitSection sec in m_children )
                {
                    sec.ResolveGlobalReferences( globalLookup );
                }
            }
        }
    }

    public class DebugInfoCompUnit
    {
        public Dwarf2_DebugInfoUnitHeader m_header;
        public Dictionary<uint, DebugInfoCompUnitSection> m_sections;
        public uint m_offset, m_streamOffset;
        public DebugInfoEntry m_owner;

        public DebugInfoCompUnit(BinaryReader bs, DebugInfoAbbrev abbrev, DebugInfoEntry owner, Dictionary<uint, DebugInfoCompUnitSection> addressMap)
        {
            long start = bs.BaseStream.Position;
            m_header = new Dwarf2_DebugInfoUnitHeader();

            m_offset = (uint)( owner.m_elfOffset + bs.BaseStream.Position );
            m_streamOffset = (uint)bs.BaseStream.Position;

            m_header.uh_length       = bs.ReadUInt32();
            m_header.uh_version      = bs.ReadUInt16(); System.Diagnostics.Debug.Assert( m_header.uh_version == 4 ); 
            m_header.uh_abbrevOffset = bs.ReadUInt32();
            m_header.uh_addressSize  = bs.ReadByte();
            m_header.uh_segmentSize  = bs.ReadByte();
            m_owner = owner;

            m_sections = new Dictionary<uint, DebugInfoCompUnitSection>();

            uint addr = (uint)(owner.m_elfOffset + bs.BaseStream.Position);

            uint abbrevCode = LEB128.DecodeUnsigned( bs );

            while( bs.BaseStream.Position - start < m_header.uh_length ) // abbrevCode != 0 )
            {
                if( abbrevCode != 0 )
                {
                    DebugInfoCompUnitSection sec = new DebugInfoCompUnitSection( abbrevCode, bs, abbrev, this, m_sections );

                    m_sections[abbrevCode] = sec;

                    addressMap[addr] = sec;
                }
                else
                {
                    DebugInfoCompUnitSection sec = new DebugInfoCompUnitSection( this );

                    addressMap[addr] = sec;
                }

                addr = (uint)( owner.m_elfOffset + bs.BaseStream.Position );
                abbrevCode = LEB128.DecodeUnsigned( bs );
            }

            foreach( DebugInfoCompUnitSection sec in m_sections.Values )
            {
                sec.ResolveLocalReferences( m_sections );
            }
        }

        // TODO: use interface for parameter so that dictionary is not modifiable
        public void ResolveGlobalReferences(Dictionary<uint, DebugInfoCompUnitSection> globalLookup)
        {
            foreach( DebugInfoCompUnitSection cus in m_sections.Values )
            {
                cus.ResolveGlobalReferences( globalLookup );
            }
        }
    }

    public class DebugInfoEntry 
    {
        public Dictionary<uint,DebugInfoCompUnit> m_entries;
        public List<ElfSection> m_relocations;
        public string m_name;
        public long m_elfOffset;
        public List<DebugLineEntry> m_lines;
        public int m_version;


        public DebugInfoEntry(ElfSection debugInfoSection, DebugInfoAbbrev abbrev, Dictionary<uint,DebugInfoCompUnitSection> addressMap)
        {
            byte[] raw = debugInfoSection.Raw;

            m_entries   = new Dictionary<uint,DebugInfoCompUnit>();
            m_name      = debugInfoSection.Name;
            m_elfOffset = debugInfoSection.m_elfOffset;
            m_relocations = new List<ElfSection>();
            m_lines       = new List<DebugLineEntry>();

            //Console.WriteLine( "Sect  : " + debugInfoSection.Name );

            using(MemoryStream ms = new MemoryStream(raw))
            using(BinaryReader bs = new BinaryReader(ms))
            {
                while( ms.Position < ms.Length )
                {
                    DebugInfoCompUnit entry = new DebugInfoCompUnit( bs, abbrev, this, addressMap );
                    
                    m_version = entry.m_header.uh_version;

                    if( entry.m_header.uh_version != 2 && entry.m_header.uh_version != 3 )
                    {
                        //
                        // Debug infos for version 4 are not supported
                        //
                        throw new NotSupportedException( "Parsing debug info for DWARF v4 is not supported" );
                    }

                    m_entries[entry.m_offset] = entry;

                    if( ms.Length > ms.Position && (ms.Length - ms.Position) < Marshal.SizeOf( typeof( Dwarf2_DebugInfoUnitHeader ) ) )
                    {
                        break;
                    }
                }
            }
        }

        public void ResolveGlobalReferences(Dictionary<string, ElfSection> sectionMap, Dictionary<uint, DebugInfoCompUnitSection> globalLookup)
        {
            foreach( DebugInfoCompUnit cu in m_entries.Values )
            {
                cu.ResolveGlobalReferences( globalLookup );
            }
        }
    }

    public class DebugLineEntry
    {
        public string m_name;
        public uint m_index;

        public Dwarf2_DebugLineHeader m_header;
        public Dictionary<int,UInt32> m_opcodLengths;
        public List<string> m_includeDirs;
        public List<Dwarf_DebugLineHeaderFile> m_files;
        public List<OpCode> m_opcode;
        public List<RelocationSection> m_relocations;

        public class OpCode
        {
            public Dwarf_DebugLineOpCodes    Opcode;
            public Dwarf_DebugLineOpCodesExt OpcodeExt;
            public object Arg;
        }

        public DebugLineEntry(ElfSection debugLineSection)
        {
            byte[] raw = debugLineSection.Raw;

            m_name = debugLineSection.Name;
            m_index = debugLineSection.m_index;
            m_header = new Dwarf2_DebugLineHeader();
            m_opcodLengths = new Dictionary<int, uint>();
            m_includeDirs = new List<string>();
            m_files = new List<Dwarf_DebugLineHeaderFile>();
            m_opcode = new List<OpCode>();
            m_relocations = new List<RelocationSection>();

            using( MemoryStream ms = new MemoryStream( raw ) )
            using( BinaryReader br = new BinaryReader( ms ) )
            {
                m_header.lh_length = br.ReadUInt32();
                m_header.lh_version = br.ReadUInt16();
                m_header.lh_headerLen = br.ReadUInt32();
                m_header.lh_minInstructionLen = br.ReadByte();
                m_header.lh_maxOpsPerInstruction = br.ReadByte();
                m_header.lh_defaultStmt = br.ReadByte();
                m_header.lh_lineBase = br.ReadSByte();
                m_header.lh_lineRange = br.ReadByte();
                m_header.lh_opcodeBase = br.ReadByte();

                for( int i = 1; i < m_header.lh_opcodeBase; i++ )
                {
                    m_opcodLengths[i] = LEB128.DecodeUnsigned( br );
                }

                string incDir = Dwarf_Utility.DwarfReadString( br );

                while( !string.IsNullOrEmpty( incDir ) )
                {
                    m_includeDirs.Add( incDir );

                    incDir = Dwarf_Utility.DwarfReadString( br );
                }

                while( ms.Position < ms.Length )
                {
                    Dwarf_DebugLineHeaderFile file = new Dwarf_DebugLineHeaderFile();

                    file.m_name = Dwarf_Utility.DwarfReadString( br );

                    if( string.IsNullOrEmpty( file.m_name ) )
                    {
                        break;
                    }

                    file.m_directoryIndex = LEB128.DecodeUnsigned( br );
                    file.m_fileModTime = LEB128.DecodeUnsigned( br );
                    file.m_fileLen = LEB128.DecodeUnsigned( br );

                    m_files.Add( file );
                }

                while( ms.Position < ms.Length )
                {
                    OpCode op = new OpCode();
                    int argSize = 0;

                    op.Opcode = (Dwarf_DebugLineOpCodes) br.ReadByte();

                    if( op.Opcode == 0 )
                    {
                        argSize = br.ReadByte() - 1;

                        op.OpcodeExt = (Dwarf_DebugLineOpCodesExt)( br.ReadByte() );

                        switch( op.OpcodeExt )
                        {
                            case Dwarf_DebugLineOpCodesExt.DW_LNE_end_sequence:
                                break;

                            case Dwarf_DebugLineOpCodesExt.DW_LNE_set_address:
                                if( argSize == 4 )
                                {
                                    op.Arg = br.ReadUInt32();
                                }
                                else if( argSize == 2 )
                                {
                                    op.Arg = br.ReadUInt16();
                                }
                                break;

                            case Dwarf_DebugLineOpCodesExt.DW_LNE_define_file:
                                Dwarf_DebugLineHeaderFile file = new Dwarf_DebugLineHeaderFile();

                                file.m_name = Dwarf_Utility.DwarfReadString( br );
                                file.m_directoryIndex = LEB128.DecodeUnsigned( br );
                                file.m_fileModTime = LEB128.DecodeUnsigned( br );
                                file.m_fileLen = LEB128.DecodeUnsigned( br );

                                op.Arg = file;
                                break;

                            default:
                                if( argSize > 0 )
                                {
                                    op.Arg = br.ReadBytes( argSize );
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch( op.Opcode )
                        {
                            case Dwarf_DebugLineOpCodes.DW_LNS_copy:
                            case Dwarf_DebugLineOpCodes.DW_LNS_negate_stmt:
                            case Dwarf_DebugLineOpCodes.DW_LNS_set_basic_block:
                            case Dwarf_DebugLineOpCodes.DW_LNS_const_add_pc:
                            case Dwarf_DebugLineOpCodes.DW_LNS_set_prologue_end:
                            case Dwarf_DebugLineOpCodes.DW_LNS_set_epilogue_begin:
                                break;

                            case Dwarf_DebugLineOpCodes.DW_LNS_advance_pc:
                            case Dwarf_DebugLineOpCodes.DW_LNS_set_file:
                            case Dwarf_DebugLineOpCodes.DW_LNS_set_column:
                            case Dwarf_DebugLineOpCodes.DW_LNS_set_isa:
                                op.Arg = LEB128.DecodeUnsigned( br );
                                break;

                            case Dwarf_DebugLineOpCodes.DW_LNS_advance_line:
                                op.Arg = LEB128.DecodeSigned( br );
                                break;

                            case Dwarf_DebugLineOpCodes.DW_LNS_fixed_advance_pc:
                                op.Arg = br.ReadUInt16();
                                break;
                        }
                    }

                    m_opcode.Add( op );
                }
            }
        }
    }
}
