//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Elf
{
    using System.Runtime.InteropServices;

    public partial class ElfObject
    {
        public static ElfObject CreateNew(ElfType type)
        {
            ElfObject  obj = new ElfObject();
            ElfSection section;

            obj.m_header = GenerateElfHeader(type);

            obj.AddSection(new ElfSection(obj, GenerateSectionZeroHeader()));
            
            // -- //

            section = new StringTable(obj);

            obj.AddSection(section);

            // We can't add names to sections until we set this property
            obj.SectionHeaderStringTable = (StringTable)section;

            section.Name = ".shstrtab";

            // -- //

            section = new StringTable(obj);

            obj.AddSection(section);

            section.Name = ".strtab";

            // -- //

            section = new SymbolTable(obj, (StringTable)section);

            obj.AddSection(section);

            section.Name = ".symtab";

            obj.SymbolTable = (SymbolTable)section;

            InsertSymbolZeroEntry((SymbolTable) section);

            // -- //
            
            return obj;
        }

        private static Elf32_Ehdr GenerateElfHeader(ElfType type)
        {
            Elf32_Ehdr hdr = new Elf32_Ehdr();

            hdr.e_type = (e_type)type;
            
            // At this time we are only supporting generation of ARM, 32-bit files.
            hdr.e_ident.EI_MAG0        = 0x7f;
            hdr.e_ident.EI_MAG1        = (byte)'E';
            hdr.e_ident.EI_MAG2        = (byte)'L';
            hdr.e_ident.EI_MAG3        = (byte)'F';
            hdr.e_ident.EI_CLASS       = EI_CLASS.ELFCLASS32;
            hdr.e_ident.EI_DATA        = EI_DATA.ELFDATA2LSB;
            hdr.e_ident.EI_VERSION     = (byte)e_version.EV_CURRENT;
            hdr.e_ident.EI_OSABI       = EI_OSABI.ARM;
            hdr.e_ident.EI_ABIVERSION  = 0;

            hdr.e_machine = e_machine.EM_ARM;
            hdr.e_version = e_version.EV_CURRENT;

            hdr.e_ehsize    = (ushort)Marshal.SizeOf(typeof(Elf32_Ehdr));
            hdr.e_phentsize = (ushort)Marshal.SizeOf(typeof(Elf32_Phdr));
            hdr.e_shentsize = (ushort)Marshal.SizeOf(typeof(Elf32_Shdr));

            return hdr;
        }

        private static Elf32_Shdr GenerateSectionZeroHeader()
        {
            // The section entry for index 0 is reserved, it holds the following
            // Name         Value       Note 
            // sh_name      0           No name 
            // sh_type      SHT_NULL    Inactive 
            // sh_flags     0           No flags 
            // sh_addr      0           No address 
            // sh_offset    0           No offset 
            // sh_size      Unspecified If non-zero, the actual number of section header entries 
            // sh_link      Unspecified If non-zero, the index of the section header string table section 
            // sh_info      0           No auxiliary information 
            // sh_addralign 0           No alignment 
            // sh_entsize   0           No entries

            Elf32_Shdr hdr = new Elf32_Shdr();

            hdr.sh_name      = 0;
            hdr.sh_type      = sh_type.SHT_NULL;
            hdr.sh_flags     = 0;
            hdr.sh_addr      = 0;
            hdr.sh_offset    = 0;
            hdr.sh_size      = 0;
            hdr.sh_link      = 0;
            hdr.sh_info      = 0;
            hdr.sh_addralign = 0;
            hdr.sh_entsize   = 0;

            return hdr;
        }

        private static void InsertSymbolZeroEntry(SymbolTable tbl)
        {
            // The symbol table entry for index 0 is reserved, it holds the following
            // Name     Value       Note
            // st_name  0           No name 
            // st_value 0           Zero value 
            // st_size  0           No size 
            // st_info  0           No type, local binding 
            // st_other 0           Default visibility 
            // st_shndx SHN_UNDEF   No section 
            tbl.AddSymbol("", 0, 0, SymbolType.STT_NOTYPE,
                                      SymbolBinding.STB_LOCAL, SymbolVisibility.STV_DEFAULT, null);
        }
    }
}
