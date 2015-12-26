//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

namespace Microsoft.Zelig.Elf
{
    #region Elf Header
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Elf32_Ehdr
    {
        public e_ident      e_ident;        // The initial bytes mark the file as an object file and provide machine-independent data with which to decode and interpret the file's contents.
        public e_type       e_type;         // This member identifies the object file type.
        public e_machine    e_machine;      // This member's value specifies the required architecture for an individual file.
        public e_version    e_version;      // This member identifies the object file version.
        public UInt32       e_entry;        // This member gives the virtual address to which the system first transfers control, thus starting the process. If the file has no associated entry point, this member holds zero.
        public UInt32       e_phoff;        // This member holds the program header table's file offset in bytes. If the file has no program header table, this member holds zero.
        public UInt32       e_shoff;        // This member holds the section header table's file offset in bytes. If the file has no section header table, this member holds zero.
        public UInt32       e_flags;        // This member holds processor-specific flags associated with the file. Flag names take the form EF_machine_flag.
        public UInt16       e_ehsize;       // This member holds the ELF header's size in bytes.
        public UInt16       e_phentsize;    // This member holds the size in bytes of one entry in the file's program header table; all entries are the same size.
        public UInt16       e_phnum;        // This member holds the number of entries in the program header table. Thus the product of e_phentsize and e_phnum gives the table's size in bytes. If a file has no program header table, e_phnum holds the value zero.
        public UInt16       e_shentsize;    // This member holds a section header's size in bytes. A section header is one entry in the section header table; all entries are the same size. 
        public UInt16       e_shnum;        // This member holds the number of entries in the section header table. Thus the product of e_shentsize and e_shnum gives the section header table's size in bytes. If a file has no section header table, e_shnum holds the value zero.
        public UInt16       e_shtrndx;      // This member holds the section header table index of the entry associated with the section name string table. If the file has no section name string table, this member holds the value SHN_UNDEF.
    }

    // -- Header IDENT descriptor -- //

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct e_ident
    {
        public  Byte        EI_MAG0;        // File identification 
        public  Byte        EI_MAG1;        // File identification 
        public  Byte        EI_MAG2;        // File identification 
        public  Byte        EI_MAG3;        // File identification 
        public  EI_CLASS    EI_CLASS;       // File class 
        public  EI_DATA     EI_DATA;        // Data encoding 
        public  Byte        EI_VERSION;     // File version 
        public  EI_OSABI    EI_OSABI;       // Operating system/ABI identification 
        public  Byte        EI_ABIVERSION;  // ABI version 
        private Byte        EI_PAD1;        // Start of padding bytes 
        private Byte        EI_PAD2;        // ... padding bytes 
        private Byte        EI_PAD3;        // ... padding bytes 
        private Byte        EI_PAD4;        // ... padding bytes 
        private Byte        EI_PAD5;        // ... padding bytes 
        private Byte        EI_PAD6;        // ... padding bytes 
        private Byte        EI_PAD7;        // ... padding bytes
    }

    public enum EI_CLASS : byte
    {
        ELFCLASSNONE = 0, // Invalid class 
        ELFCLASS32   = 1, // 32-bit objects 
        ELFCLASS64   = 2, // 64-bit objects 
    }

    public enum EI_DATA : byte
    {
        ELFDATANONE = 0, // Invalid data encoding 
        ELFDATA2LSB = 1, // Encoding ELFDATA2LSB specifies 2's complement values, with the least significant byte occupying the lowest address.
        ELFDATA2MSB = 2, // Encoding ELFDATA2MSB specifies 2's complement values, with the most significant byte occupying the lowest address.
    }

    public enum EI_OSABI : byte
    {
        ELFOSABI_NONE    = 0,  // No extensions or unspecified 
        ELFOSABI_HPUX    = 1,  // Hewlett-Packard HP-UX 
        ELFOSABI_NETBSD  = 2,  // NetBSD 
        ELFOSABI_SOLARIS = 6,  // Sun Solaris 
        ELFOSABI_AIX     = 7,  // AIX 
        ELFOSABI_IRIX    = 8,  // IRIX 
        ELFOSABI_FREEBSD = 9,  // FreeBSD 
        ELFOSABI_TRU64   = 10, // Compaq TRU64 UNIX 
        ELFOSABI_MODESTO = 11, // Novell Modesto 
        ELFOSABI_OPENBSD = 12, // Open BSD 
        ELFOSABI_OPENVMS = 13, // Open VMS 
        ELFOSABI_NSK     = 14, // Hewlett-Packard Non-Stop Kernel 
        ELFOSABI_AROS    = 15, // Amiga Research OS 
        ARCHSPEC64TO255  = 64, // Architecture-specific values

        ARM              = 97, // ARM
    }

    // ----------------------------- //

    public enum e_type : ushort
    {
        ET_NONE   = 0,      // No file type 
        ET_REL    = 1,      // Relocatable file 
        ET_EXEC   = 2,      // Executable file 
        ET_DYN    = 3,      // Shared object file 
        ET_CORE   = 4,      // Core file 
        ET_LOOS   = 0xfe00, // Operating system-specific 
        ET_HIOS   = 0xfeff, // Operating system-specific 
        ET_LOPROC = 0xff00, // Processor-specific 
        ET_HIPROC = 0xffff, // Processor-specific 
    }

    public enum e_machine : ushort
    {
        EM_NONE         = 0,  // No machine 
        EM_M32          = 1,  // AT&T WE 32100 
        EM_SPARC        = 2,  // SPARC 
        EM_386          = 3,  // Intel 80386 
        EM_68K          = 4,  // Motorola 68000 
        EM_88K          = 5,  // Motorola 88000 
        RESERVED6       = 6,  // Reserved for future use 
        EM_860          = 7,  // Intel 80860 
        EM_MIPS         = 8,  // MIPS I Architecture 
        RESERVED2       = 9,  // Reserved for future use 
        EM_MIPS_RS3_LE  = 10, // MIPS RS3000 Little-endian 
        RESERVED11TO14  = 11, // Reserved for future use 
        EM_PARISC       = 15, // Hewlett-Packard PA-RISC 
        RESERVED16      = 16, // Reserved for future use 
        EM_VPP500       = 17, // Fujitsu VPP500 
        EM_SPARC32PLUS  = 18, // Enhanced instruction set SPARC 
        EM_960          = 19, // Intel 80960 
        EM_PPC          = 20, // Power PC 
        EM_PPC64        = 21, // 64-bit PowerPC 
        EM_S390         = 22, // IBM System/390 Processor 
        RESERVED23TO35  = 23, // Reserved for future use 
        EM_V800         = 36, // NEC V800 
        EM_FR20         = 37, // Fujitsu FR20 
        EM_RH32         = 38, // TRW RH-32 
        EM_RCE          = 39, // Motorola RCE 
        EM_ARM          = 40, // Advanced RISC Machines ARM 
        EM_ALPHA        = 41, // Digital Alpha 
        EM_SH           = 42, // Hitachi SH 
        EM_SPARCV9      = 43, // SPARC Version 9 
        EM_TRICORE      = 44, // Siemens Tricore embedded processor 
        EM_ARC          = 45, // Argonaut RISC Core, Argonaut Technologies Inc. 
        EM_H8_300       = 46, // Hitachi H8/300 
        EM_H8_300H      = 47, // Hitachi H8/300H 
        EM_H8S          = 48, // Hitachi H8S 
        EM_H8_500       = 49, // Hitachi H8/500 
        EM_IA_64        = 50, // Intel MercedTM Processor 
        EM_MIPS_X       = 51, // Stanford MIPS-X 
        EM_COLDFIRE     = 52, // Motorola Coldfire 
        EM_68HC12       = 53, // Motorola M68HC12 
    }

    public enum e_version : uint
    {
        EV_NONE     = 0, // Invalid version 
        EV_CURRENT  = 1, // Current version 
    }

    #endregion ElfHeader

    #region Elf Section Header

    public enum NamedIndexes : ushort
    {
        SHN_UNDEF       = 0,
        SHN_LORESERVE   = 0xff00, // This value specifies the lower bound of the range of reserved indexes. 
        SHN_LOPROC      = 0xff00, // Values in this inclusive range are reserved for processor-specific semantics. 
        SHN_HIPROC      = 0xff1f, // Values in this inclusive range are reserved for processor-specific semantics. 
        SHN_LOOS        = 0xff20, // Values in this inclusive range are reserved for operating system-specific semantics. 
        SHN_HIOS        = 0xff3f, // Values in this inclusive range are reserved for operating system-specific semantics. 
        SHN_ABS         = 0xfff1, // Symbols defined relative to this section are common symbols, such as FORTRAN COMMON or unallocated C external variables.
        SHN_COMMON      = 0xfff2, // This value specifies absolute values for the corresponding reference. For example, symbols defined relative to section number SHN_ABS have absolute values and are not affected by relocation. 
        SHN_XINDEX      = 0xffff, // This value is an escape value. It indicates that the actual section header index is too large to fit in the containing field and is to be found in another location (specific to the structure where it appears).
        SHN_HIRESERVE   = 0xffff, // This value specifies the upper bound of the range of reserved indexes. The system reserves indexes between SHN_LORESERVE and SHN_HIRESERVE, inclusive; the values do not reference the section header table. The section header table does not contain entries for the reserved indexes. 
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Elf32_Shdr
    {
        public UInt32    sh_name;        // This member specifies the name of the section. Its value is an index into the section header string table section [see ``String Table'' below], giving the location of a null-terminated string. 
        public sh_type    sh_type;        // This member categorizes the section's contents and semantics.
        public sh_flags    sh_flags;       // Sections support 1-bit flags that describe miscellaneous attributes. 
        public UInt32    sh_addr;        // If the section will appear in the memory image of a process, this member gives the address at which the section's first byte should reside. Otherwise, the member contains 0. 
        public UInt32    sh_offset;      // This member's value gives the byte offset from the beginning of the file to the first byte in the section. One section type, SHT_NOBITS described below, occupies no space in the file, and its sh_offset member locates the conceptual placement in the file. 
        public UInt32    sh_size;        // This member gives the section's size in bytes. Unless the section type is SHT_NOBITS, the section occupies sh_size bytes in the file. A section of type SHT_NOBITS may have a non-zero size, but it occupies no space in the file. 
        public UInt32    sh_link;        // This member holds a section header table index link, whose interpretation depends on the section type. A table below describes the values.
        public UInt32    sh_info;        // This member holds extra information, whose interpretation depends on the section type. A table below describes the values. If the sh_flags field for this section header includes the attribute SHF_INFO_LINK, then this member represents a section header table index. 
        public UInt32    sh_addralign;   // Some sections have address alignment constraints. For example, if a section holds a doubleword, the system must ensure doubleword alignment for the entire section. The value of sh_addr must be congruent to 0, modulo the value of sh_addralign. Currently, only 0 and positive integral powers of two are allowed. Values 0 and 1 mean the section has no alignment constraints. 
        public UInt32    sh_entsize;     // Some sections hold a table of fixed-size entries, such as a symbol table. For such a section, this member gives the size in bytes of each entry. The member contains 0 if the section does not hold a table of fixed-size entries. 
    }
    
    public enum sh_type : uint
    {
        SHT_NULL            = 0,            // This value marks the section header as inactive; it does not have an associated section. Other members of the section header have undefined values. 
        SHT_PROGBITS        = 1,            // The section holds information defined by the program, whose format and meaning are determined solely by the program.
        SHT_SYMTAB          = 2,            // These sections hold a symbol table. Currently, an object file may have only one section of each type, but this restriction may be relaxed in the future. Typically, SHT_SYMTAB provides symbols for link editing, though it may also be used for dynamic linking. As a complete symbol table, it may contain many symbols unnecessary for dynamic linking. Consequently, an object file may also contain a SHT_DYNSYM section, which holds a minimal set of dynamic linking symbols, to save space. 
        SHT_STRTAB          = 3,            // The section holds a string table. An object file may have multiple string table sections.
        SHT_RELA            = 4,            // The section holds relocation entries with explicit addends, such as type Elf32_Rela for the 32-bit class of object files or type Elf64_Rela for the 64-bit class of object files. An object file may have multiple relocation sections.
        SHT_HASH            = 5,            // The section holds a symbol hash table. Currently, an object file may have only one hash table, but this restriction may be relaxed in the future.
        SHT_DYNAMIC         = 6,            // The section holds information for dynamic linking. Currently, an object file may have only one dynamic section, but this restriction may be relaxed in the future.
        SHT_NOTE            = 7,            // The section holds information that marks the file in some way.
        SHT_NOBITS          = 8,            // A section of this type occupies no space in the file but otherwise resembles SHT_PROGBITS. Although this section contains no bytes, the sh_offset member contains the conceptual file offset. 
        SHT_REL             = 9,            // The section holds relocation entries without explicit addends, such as type Elf32_Rel for the 32-bit class of object files or type Elf64_Rel for the 64-bit class of object files. An object file may have multiple relocation sections.
        SHT_SHLIB           = 10,           // This section type is reserved but has unspecified semantics. 
        SHT_DYNSYM          = 11,           // These sections hold a symbol table. Currently, an object file may have only one section of each type, but this restriction may be relaxed in the future. Typically, SHT_SYMTAB provides symbols for link editing, though it may also be used for dynamic linking. As a complete symbol table, it may contain many symbols unnecessary for dynamic linking. Consequently, an object file may also contain a SHT_DYNSYM section, which holds a minimal set of dynamic linking symbols, to save space. 
        SHT_INIT_ARRAY      = 14,           // This section contains an array of pointers to initialization functions, as described in ``Initialization and Termination Functions'' in Chapter 5. Each pointer in the array is taken as a parameterless procedure with a void return.
        SHT_FINI_ARRAY      = 15,           // This section contains an array of pointers to termination functions, as described in ``Initialization and Termination Functions'' in Chapter 5. Each pointer in the array is taken as a parameterless procedure with a void return. 
        SHT_PREINIT_ARRAY   = 16,           // This section contains an array of pointers to functions that are invoked before all other initialization functions, as described in ``Initialization and Termination Functions'' in Chapter 5. Each pointer in the array is taken as a parameterless procedure with a void return. 
        SHT_GROUP           = 17,           // This section defines a section group. A section group is a set of sections that are related and that must be treated specially by the linker (see below for further details). Sections of type SHT_GROUP may appear only in relocatable objects (objects with the ELF header e_type member set to ET_REL). The section header table entry for a group section must appear in the section header table before the entries for any of the sections that are members of the group. 
        SHT_SYMTAB_SHNDX    = 18,           // This section is associated with a section of type SHT_SYMTAB and is required if any of the section header indexes referenced by that symbol table contain the escape value SHN_XINDEX. The section is an array of Elf32_Word values. Each value corresponds one to one with a symbol table entry and appear in the same order as those entries. The values represent the section header indexes against which the symbol table entries are defined. Only if corresponding symbol table entry's st_shndx field contains the escape value SHN_XINDEX will the matching Elf32_Word hold the actual section header index; otherwise, the entry must be SHN_UNDEF (0). 
        SHT_LOOS            = 0x60000000,   // Values in this inclusive range are reserved for operating system-specific semantics. 
        SHT_HIOS            = 0x6fffffff,   // Values in this inclusive range are reserved for operating system-specific semantics. 
        SHT_LOPROC          = 0x70000000,   // Values in this inclusive range are reserved for processor-specific semantics. 
        SHT_HIPROC          = 0x7fffffff,   // Values in this inclusive range are reserved for processor-specific semantics. 
        SHT_LOUSER          = 0x80000000,   // This value specifies the lower bound of the range of indexes reserved for application programs. 
        SHT_HIUSER          = 0xffffffff,   // This value specifies the upper bound of the range of indexes reserved for application programs. Section types between SHT_LOUSER and SHT_HIUSER may be used by the application, without conflicting with current or future system-defined section types. 
    }

    public enum sh_type_ARM : uint
    {
        SHT_ARM_EXIDX          = 0x70000001,  // Exception Index Table
        SHT_ARM_PREEMPTMAP     = 0x70000002,  // BPABI DLL dynamic linking pre-emption map
        SHT_ARM_ATTRIBUTES     = 0x70000003,  // Object file compatibility attributes
        SHT_ARM_DEBUGOVERLAY   = 0x70000004,  // Support for Debugging Overlaid Programs
        SHT_ARM_OVERLAYSECTION = 0x70000005,  // Support for Debugging Overlaid Programs
    }

    [Flags]
    public enum sh_flags : uint
    {
        SHF_WRITE               = 0x1,          // The section contains data that should be writable during process execution. 
        SHF_ALLOC               = 0x2,          // The section occupies memory during process execution. Some control sections do not reside in the memory image of an object file; this attribute is off for those sections. 
        SHF_EXECINSTR           = 0x4,          // The section contains executable machine instructions.
        SHF_MERGE               = 0x10,         // The data in the section may be merged to eliminate duplication. Unless the SHF_STRINGS flag is also set, the data elements in the section are of a uniform size. The size of each element is specified in the section header's sh_entsize field. If the SHF_STRINGS flag is also set, the data elements consist of null-terminated character strings. The size of each character is specified in the section header's sh_entsize field. 
        SHF_STRINGS             = 0x20,         // The data elements in the section consist of null-terminated character strings. The size of each character is specified in the section header's sh_entsize field. 
        SHF_INFO_LINK           = 0x40,         // The sh_info field of this section header holds a section header table index. 
        SHF_LINK_ORDER          = 0x80,         // This flag adds special ordering requirements for link editors. The requirements apply if the sh_link field of this section's header references another section (the linked-to section). If this section is combined with other sections in the output file, it must appear in the same relative order with respect to those sections, as the linked-to section appears with respect to sections the linked-to section is combined with. 
        SHF_OS_NONCONFORMING    = 0x100,        // This section requires special OS-specific processing (beyond the standard linking rules) to avoid incorrect behavior. If this section has either an sh_type value or contains sh_flags bits in the OS-specific ranges for those fields, and a link editor processing this section does not recognize those values, then the link editor should reject the object file containing this section with an error. 
        SHF_GROUP               = 0x200,        // This section is a member (perhaps the only one) of a section group. The section must be referenced by a section of type SHT_GROUP. The SHF_GROUP flag may be set only for sections contained in relocatable objects (objects with the ELF header e_type member set to ET_REL). 
        SHF_TLS                 = 0x400,        // This section holds Thread-Local Storage, meaning that each separate execution flow has its own distinct instance of this data. Implementations need not support this flag. 
        SHF_MASKOS              = 0x0ff00000,   // All bits included in this mask are reserved for operating system-specific semantics.
        SHF_MASKPROC            = 0xf0000000,   // All bits included in this mask are reserved for processor-specific semantics. If meanings are specified, the processor supplement explains them.
    }

    #endregion Elf Section Header

    #region Symbol Table Descriptor

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Elf32_Sym
    {
        public UInt32   st_name;    // This member holds an index into the object file's symbol string table, which holds the character representations of the symbol names. If the value is non-zero, it represents a string table index that gives the symbol name. Otherwise, the symbol table entry has no name.
        public UInt32   st_value;   // This member gives the value of the associated symbol. Depending on the context, this may be an absolute value, an address, and so on; details appear below.
        public UInt32   st_size;    // Many symbols have associated sizes. For example, a data object's size is the number of bytes contained in the object. This member holds 0 if the symbol has no size or an unknown size. 
        public Byte     st_info;    // This member specifies the symbol's type and binding attributes. A list of the values and meanings appears below.
        public Byte     st_other;   // This member currently specifies a symbol's visibility. A list of the values and meanings appears below.
        public UInt16   st_shndx;   // Every symbol table entry is defined in relation to some section. This member holds the relevant section header table index. As the sh_link and sh_info interpretation table and the related text describe, some section indexes indicate special meanings. If this member contains SHN_XINDEX, then the actual section header index is too large to fit in this field. The actual value is contained in the associated section of type SHT_SYMTAB_SHNDX. 
    }

    public enum SymbolType
    {
        STT_NOTYPE  = 0,    // The symbol's type is not specified. 
        STT_OBJECT  = 1,    // The symbol is associated with a data object, such as a variable, an array, and so on.
        STT_FUNC    = 2,    // The symbol is associated with a function or other executable code. 
        STT_SECTION = 3,    // The symbol is associated with a section. Symbol table entries of this type exist primarily for relocation and normally have STB_LOCAL binding. 
        STT_FILE    = 4,    // Conventionally, the symbol's name gives the name of the source file associated with the object file. A file symbol has STB_LOCAL binding, its section index is SHN_ABS, and it precedes the other STB_LOCAL symbols for the file, if it is present. 
        STT_COMMON  = 5,    // The symbol labels an uninitialized common block.  
        STT_TLS     = 6,    // The symbol specifies a Thread-Local Storage entity. When defined, it gives the assigned offset for the symbol, not the actual address. Symbols of type STT_TLS can be referenced by only special thread-local storage relocations and thread-local storage relocations can only reference symbols with type STT_TLS. Implementation need not support thread-local storage. 
        STT_LOOS    = 10,   // Values in this inclusive range are reserved for operating system-specific semantics.  
        STT_HIOS    = 12,   // Values in this inclusive range are reserved for operating system-specific semantics. 
        STT_LOPROC  = 13,   // Values in this inclusive range are reserved for processor-specific semantics. If meanings are specified, the processor supplement explains them. 
        STT_HIPROC  = 15,   // Values in this inclusive range are reserved for processor-specific semantics. If meanings are specified, the processor supplement explains them. 
    }

    public enum SymbolBinding
    {
        STB_LOCAL   = 0,    // Local symbols are not visible outside the object file containing their definition. Local symbols of the same name may exist in multiple files without interfering with each other. 
        STB_GLOBAL  = 1,    // Global symbols are visible to all object files being combined. One file's definition of a global symbol will satisfy another file's undefined reference to the same global symbol. 
        STB_WEAK    = 2,    // Weak symbols resemble global symbols, but their definitions have lower precedence. 
        STB_LOOS    = 10,   // Values in this inclusive range are reserved for operating system-specific semantics. 
        STB_HIOS    = 12,   // Values in this inclusive range are reserved for operating system-specific semantics. 
        STB_LOPROC  = 13,   // Values in this inclusive range are reserved for processor-specific semantics. If meanings are specified, the processor supplement explains them. 
        STB_HIPROC  = 15,   // Values in this inclusive range are reserved for processor-specific semantics. If meanings are specified, the processor supplement explains them. 
    }

    public enum SymbolVisibility
    {
        STV_DEFAULT     = 0,    // The visibility of symbols with the STV_DEFAULT attribute is as specified by the symbol's binding type. That is, global and weak symbols are visible outside of their defining component (executable file or shared object). Local symbols are hidden, as described below. Global and weak symbols are also preemptable, that is, they may by preempted by definitions of the same name in another component. 
        STV_INTERNAL    = 1,    // The meaning of this visibility attribute may be defined by processor supplements to further constrain hidden symbols. A processor supplement's definition should be such that generic tools can safely treat internal symbols as hidden. An internal symbol contained in a relocatable object must be either removed or converted to STB_LOCAL binding by the link-editor when the relocatable object is included in an executable file or shared object. 
        STV_HIDDEN      = 2,    // A symbol defined in the current component is hidden if its name is not visible to other components. Such a symbol is necessarily protected. This attribute may be used to control the external interface of a component. Note that an object named by such a symbol may still be referenced from another component if its address is passed outside. A hidden symbol contained in a relocatable object must be either removed or converted to STB_LOCAL binding by the link-editor when the relocatable object is included in an executable file or shared object. 
        STV_PROTECTED   = 3,    // A symbol defined in the current component is protected if it is visible in other components but not preemptable, meaning that any reference to such a symbol from within the defining component must be resolved to the definition in that component, even if there is a definition in another component that would preempt by the default rules. A symbol with STB_LOCAL binding may not have STV_PROTECTED visibility. If a symbol definition with STV_PROTECTED visibility from a shared object is taken as resolving a reference from an executable or another shared object, the SHN_UNDEF symbol table entry created has STV_DEFAULT visibility. 
    }

    #endregion Symbol Table Descriptor

    #region Elf Program Header

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Elf32_Phdr 
    {
        public SegmentType    p_type;     // This member tells what kind of segment this array element describes or how to interpret the array element's information. Type values and their meanings appear below. 
        public UInt32        p_offset;   // This member gives the offset from the beginning of the file at which the first byte of the segment resides. 
        public UInt32        p_vaddr;    // This member gives the virtual address at which the first byte of the segment resides in memory. 
        public UInt32        p_paddr;    // On systems for which physical addressing is relevant, this member is reserved for the segment's physical address. Because System V ignores physical addressing for application programs, this member has unspecified contents for executable files and shared objects. 
        public UInt32        p_filesz;   // This member gives the number of bytes in the file image of the segment; it may be zero. 
        public UInt32        p_memsz;    // This member gives the number of bytes in the memory image of the segment; it may be zero. 
        public SegmentFlag    p_flags;    // This member gives flags relevant to the segment. Defined flag values appear below. 
        public UInt32        p_align;    // As ``Program Loading'' describes in this chapter of the processor supplement, loadable process segments must have congruent values for p_vaddr and p_offset, modulo the page size. This member gives the value to which the segments are aligned in memory and in the file. Values 0 and 1 mean no alignment is required. Otherwise, p_align should be a positive, integral power of 2, and p_vaddr should equal p_offset, modulo p_align. 
    }

    public enum SegmentType : uint
    {
        PT_NULL     = 0,            // The array element is unused; other members' values are undefined. This type lets the program header table have ignored entries. 
        PT_LOAD     = 1,            // The array element specifies a loadable segment, described by p_filesz and p_memsz. The bytes from the file are mapped to the beginning of the memory segment. If the segment's memory size (p_memsz) is larger than the file size (p_filesz), the ``extra'' bytes are defined to hold the value 0 and to follow the segment's initialized area. The file size may not be larger than the memory size. Loadable segment entries in the program header table appear in ascending order, sorted on the p_vaddr member. 
        PT_DYNAMIC  = 2,            // The array element specifies dynamic linking information. See ``Dynamic Section'' below for more information. 
        PT_INTERP   = 3,            // The array element specifies the location and size of a null-terminated path name to invoke as an interpreter. This segment type is meaningful only for executable files (though it may occur for shared objects); it may not occur more than once in a file. If it is present, it must precede any loadable segment entry. See ``Program Interpreter'' below for more information. 
        PT_NOTE     = 4,            // The array element specifies the location and size of auxiliary information. See ``Note Section'' below for more information. 
        PT_SHLIB    = 5,            // This segment type is reserved but has unspecified semantics. Programs that contain an array element of this type do not conform to the ABI. 
        PT_PHDR     = 6,            // The array element, if present, specifies the location and size of the program header table itself, both in the file and in the memory image of the program. This segment type may not occur more than once in a file. Moreover, it may occur only if the program header table is part of the memory image of the program. If it is present, it must precede any loadable segment entry. See ``Program Interpreter'' below for more information. 
        PT_TLS      = 7,            // The array element specifies the Thread-Local Storage template. Implementations need not support this program table entry. See ``Thread-Local Storage'' below for more information. 
        PT_LOOS     = 0x60000000,   // Values in this inclusive range are reserved for operating system-specific semantics. 
        PT_HIOS     = 0x6fffffff,   // Values in this inclusive range are reserved for operating system-specific semantics. 
        PT_LOPROC   = 0x70000000,   // Values in this inclusive range are reserved for processor-specific semantics. If meanings are specified, the processor supplement explains them. 
        PT_HIPROC   = 0x7fffffff,   // Values in this inclusive range are reserved for processor-specific semantics. If meanings are specified, the processor supplement explains them. 
    }

    [Flags]
    public enum SegmentFlag : uint
    {
        PF_X        = 0x1,          // Execute 
        PF_W        = 0x2,          // Write 
        PF_R        = 0x4,          // Read 
        PF_MASKOS   = 0x0ff00000,   // Unspecified 
        PF_MASKPROC = 0xf0000000,   // Unspecified 
    }

    #endregion Elf Program Header

    #region Relocation Table Description

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Elf32_Rel
    {
        public UInt32 r_offset;
        public UInt32 r_info;
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct Elf32_Rela
    {
        public UInt32 r_offset;
        public UInt32 r_info;
        public Int32  r_addend;
    }

    public enum RelocationType : byte
    {
        R_ARM_NONE, 
        R_ARM_PC24, 
        R_ARM_ABS32, 
        R_ARM_REL32 ,
        R_ARM_LDR_PC_G0 ,
        R_ARM_ABS16 ,
        R_ARM_ABS12 ,
        R_ARM_THM_ABS5 ,
        R_ARM_ABS8 ,
        R_ARM_SBREL32 ,
        R_ARM_THM_CALL ,
        R_ARM_THM_PC8 ,
        R_ARM_BREL_ADJ ,
        R_ARM_TLS_DESC ,
        R_ARM_THM_SWI8 ,
        R_ARM_XPC25 ,
        R_ARM_THM_XPC22 ,
        R_ARM_TLS_DTPMOD32 ,
        R_ARM_TLS_DTPOFF32 ,
        R_ARM_TLS_TPOFF32 ,
        R_ARM_COPY ,
        R_ARM_GLOB_DAT ,
        R_ARM_JUMP_SLOT ,
        R_ARM_RELATIVE ,
        R_ARM_GOTOFF32 ,
        R_ARM_BASE_PREL ,
        R_ARM_GOT_BREL ,
        R_ARM_PLT32 ,
        R_ARM_CALL ,
        R_ARM_JUMP24, 
        R_ARM_THM_JUMP24 ,
        R_ARM_BASE_ABS ,
        R_ARM_ALU_PCREL_7_0 ,
        R_ARM_ALU_PCREL_15_8 ,
        R_ARM_ALU_PCREL_23_15 ,
        R_ARM_LDR_SBREL_11_0_NC ,
        R_ARM_ALU_SBREL_19_12_NC,
        R_ARM_ALU_SBREL_27_20_CK,
        R_ARM_TARGET1 ,
        R_ARM_SBREL31 ,
        R_ARM_V4BX ,
        R_ARM_TARGET2 ,
        R_ARM_PREL31 ,
        R_ARM_MOVW_ABS_NC ,
        R_ARM_MOVT_ABS ,
        R_ARM_MOVW_PREL_NC ,
        R_ARM_MOVT_PREL ,
        R_ARM_THM_MOVW_ABS_NC ,
        R_ARM_THM_MOVT_ABS ,
        R_ARM_THM_MOVW_PREL_NC ,
        R_ARM_THM_MOVT_PREL ,
        R_ARM_THM_JUMP19 ,
        R_ARM_THM_JUMP6 ,
        R_ARM_THM_ALU_PREL_11_0 ,
        R_ARM_THM_PC12 ,
        R_ARM_ABS32_NOI ,
        R_ARM_REL32_NOI ,
        R_ARM_ALU_PC_G0_NC ,
        R_ARM_ALU_PC_G0 ,
        R_ARM_ALU_PC_G1_NC ,
        R_ARM_ALU_PC_G1 ,
        R_ARM_ALU_PC_G2 ,
        R_ARM_LDR_PC_G1 ,
        R_ARM_LDR_PC_G2 ,
        R_ARM_LDRS_PC_G0 ,
        R_ARM_LDRS_PC_G1 ,
        R_ARM_LDRS_PC_G2 ,
        R_ARM_LDC_PC_G0 ,
        R_ARM_LDC_PC_G1 ,
        R_ARM_LDC_PC_G2 ,
        R_ARM_ALU_SB_G0_NC ,
        R_ARM_ALU_SB_G0 ,
        R_ARM_ALU_SB_G1_NC ,
        R_ARM_ALU_SB_G1 ,
        R_ARM_ALU_SB_G2 ,
        R_ARM_LDR_SB_G0 ,
        R_ARM_LDR_SB_G1 ,
        R_ARM_LDR_SB_G2 ,
        R_ARM_LDRS_SB_G0 ,
        R_ARM_LDRS_SB_G1 ,
        R_ARM_LDRS_SB_G2 ,
        R_ARM_LDC_SB_G0 ,
        R_ARM_LDC_SB_G1 ,
        R_ARM_LDC_SB_G2 ,
        R_ARM_MOVW_BREL_NC ,
        R_ARM_MOVT_BREL ,
        R_ARM_MOVW_BREL ,
        R_ARM_THM_MOVW_BREL_NC ,
        R_ARM_THM_MOVT_BREL ,
        R_ARM_THM_MOVW_BREL ,
        R_ARM_TLS_GOTDESC ,
        R_ARM_TLS_CALL ,
        R_ARM_TLS_DESCSEQ ,
        R_ARM_THM_TLS_CALL ,
        R_ARM_PLT32_ABS ,
        R_ARM_GOT_ABS ,
        R_ARM_GOT_PREL ,
        R_ARM_GOT_BREL12 ,
        R_ARM_GOTOFF12 ,
        R_ARM_GOTRELAX ,
        R_ARM_GNU_VTENTRY ,
        R_ARM_GNU_VTINHERIT ,
        R_ARM_THM_JUMP11 ,
        R_ARM_THM_JUMP8 ,
        R_ARM_TLS_GD32 ,
        R_ARM_TLS_LDM32 ,
        R_ARM_TLS_LDO32 ,
        R_ARM_TLS_IE32 ,
        R_ARM_TLS_LE32 ,
        R_ARM_TLS_LDO12 ,
        R_ARM_TLS_LE12 ,
        R_ARM_TLS_IE12GP ,
    }

    #endregion Relocation Table Description

    public static class LEB128
    {
        public static uint DecodeUnsigned(BinaryReader br)
        {
            uint result = 0;
            int shift = 0;

            while(true)
            {
                byte by = br.ReadByte();
                result |= ((uint)(0x7F & by) << shift);
                if ((0x80 & by) == 0)
                    break;
                shift += 7;
            }

            return result;
        }

        public static int DecodeSigned(BinaryReader br)
        {
            int result = 0;
            int shift = 0;
            int size = sizeof(int);
            byte by;

            while( true )
            {
                by = br.ReadByte();

                result |= ( (int)( 0x7F & by ) << shift );

                if( ( 0x80 & by ) == 0 )
                    break;

                shift += 7;
            }

            if( ( shift < size ) && 0 != ( 0x80 & by ) )
            {
                result |= -( 1 << shift );
            }

            return result;
        }
    }


    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct Dwarf2_DebugInfoUnitHeader
    {
        public UInt32 uh_length;
        public UInt16 uh_version;
        public UInt32 uh_abbrevOffset;
        public byte   uh_addressSize;
        public byte   uh_segmentSize;
    }

    public enum Dwarf2_TAG : uint
    {
        DW_TAG_array_type               = 0x01,
        DW_TAG_class_type               = 0x02,
        DW_TAG_entry_point              = 0x03,
        DW_TAG_enumeration_type         = 0x04,
        DW_TAG_formal_parameter         = 0x05,
        DW_TAG_imported_declaration     = 0x08,
        DW_TAG_label                    = 0x0a,
        DW_TAG_lexical_block            = 0x0b,
        DW_TAG_member                   = 0x0d,
        DW_TAG_pointer_type             = 0x0f,
        DW_TAG_reference_type           = 0x10,
        DW_TAG_compile_unit             = 0x11,
        DW_TAG_string_type              = 0x12,
        DW_TAG_structure_type           = 0x13,
        DW_TAG_subroutine_type          = 0x15,
        DW_TAG_typedef                  = 0x16,
        DW_TAG_union_type               = 0x17,
        DW_TAG_unspecified_parameters   = 0x18,
        DW_TAG_variant                  = 0x19,
        DW_TAG_common_block             = 0x1a,
        DW_TAG_common_inclusion         = 0x1b,
        DW_TAG_inheritance              = 0x1c,
        DW_TAG_inlined_subroutine       = 0x1d,
        DW_TAG_module                   = 0x1e,
        DW_TAG_ptr_to_member_type       = 0x1f,
        DW_TAG_set_type                 = 0x20,
        DW_TAG_subrange_type            = 0x21,
        DW_TAG_with_stmt                = 0x22,
        DW_TAG_access_declaration       = 0x23,
        DW_TAG_base_type                = 0x24,
        DW_TAG_catch_block              = 0x25,
        DW_TAG_const_type               = 0x26,
        DW_TAG_constant                 = 0x27,
        DW_TAG_enumerator               = 0x28,
        DW_TAG_file_type                = 0x29,
        DW_TAG_friend                   = 0x2a,
        DW_TAG_namelist                 = 0x2b,
        DW_TAG_namelist_item            = 0x2c,
        DW_TAG_packed_type              = 0x2d,
        DW_TAG_subprogram               = 0x2e,
        DW_TAG_template_type_param      = 0x2f,
        DW_TAG_template_value_param     = 0x30,
        DW_TAG_thrown_type              = 0x31,
        DW_TAG_try_block                = 0x32,
        DW_TAG_variant_part             = 0x33,
        DW_TAG_variable                 = 0x34,
        DW_TAG_volatile_type            = 0x35,
        DW_TAG_lo_user                  = 0x4080,
        DW_TAG_hi_user                  = 0xffff,
    }

    public enum Dwarf2_Attribute : uint
    {
        DW_AT_sibling               = 0x01, // reference
        DW_AT_location              = 0x02, // block, constant
        DW_AT_name                  = 0x03, // string
        DW_AT_ordering              = 0x09, // constant
        DW_AT_byte_size             = 0x0b, // constant
        DW_AT_bit_offset            = 0x0c, // constant
        DW_AT_bit_size              = 0x0d, // constant
        DW_AT_stmt_list             = 0x10, // constant
        DW_AT_low_pc                = 0x11, // address
        DW_AT_high_pc               = 0x12, // address
        DW_AT_language              = 0x13, // constant
        DW_AT_discr                 = 0x15, // reference
        DW_AT_discr_value           = 0x16, // constant
        DW_AT_visibility            = 0x17, // constant
        DW_AT_import                = 0x18, // reference
        DW_AT_string_length         = 0x19, // block, constant
        DW_AT_common_reference      = 0x1a, // reference
        DW_AT_comp_dir              = 0x1b, // string
        DW_AT_const_value           = 0x1c, // string, constant, block
        DW_AT_containing_type       = 0x1d, // reference
        DW_AT_default_value         = 0x1e, // reference
        DW_AT_inline                = 0x20, // constant
        DW_AT_is_optional           = 0x21, // flag
        DW_AT_lower_bound           = 0x22, // constant, reference
        DW_AT_producer              = 0x25, // string
        DW_AT_prototyped            = 0x27, // flag
        DW_AT_return_addr           = 0x2a, // block, constant
        DW_AT_start_scope           = 0x2c, // constant
        DW_AT_stride_size           = 0x2e, // constant
        DW_AT_upper_bound           = 0x2f, // constant, reference 
        DW_AT_abstract_origin       = 0x31, //  reference
        DW_AT_accessibility         = 0x32, //  constant
        DW_AT_address_class         = 0x33, //  constant
        DW_AT_artificial            = 0x34, //  flag
        DW_AT_base_types            = 0x35, //  reference
        DW_AT_calling_convention    = 0x36, //  constant
        DW_AT_count                 = 0x37, //  constant, reference
        DW_AT_data_member_location  = 0x38, //  block, reference
        DW_AT_decl_column           = 0x39, //  constant
        DW_AT_decl_file             = 0x3a, //  constant
        DW_AT_decl_line             = 0x3b, //  constant
        DW_AT_declaration           = 0x3c, //  flag
        DW_AT_discr_list            = 0x3d, //  block
        DW_AT_encoding              = 0x3e, //  constant
        DW_AT_external              = 0x3f, //  flag
        DW_AT_frame_base            = 0x40, //  block, constant
        DW_AT_friend                = 0x41, //  reference
        DW_AT_identifier_case       = 0x42, //  constant
        DW_AT_macro_info            = 0x43, //  constant
        DW_AT_namelist_item         = 0x44, //  block
        DW_AT_priority              = 0x45, //  reference
        DW_AT_segment               = 0x46, //  block, constant
        DW_AT_specification         = 0x47, //  reference
        DW_AT_static_link           = 0x48, //  block, constant
        DW_AT_type                  = 0x49, //  reference
        DW_AT_use_location          = 0x4a, //  block, constant
        DW_AT_variable_parameter    = 0x4b, //  flag
        DW_AT_virtuality            = 0x4c, //  constant
        DW_AT_vtable_elem_location  = 0x4d, //  block, reference
        //--// DWARF 4 //--//
        DW_AT_allocated             = 0x4e, // constant, exprloc, reference
        DW_AT_associated            = 0x4f, // constant, exprloc, reference
        DW_AT_data_location         = 0x50, // exprloc
        DW_AT_byte_stride           = 0x51, // constant, exprloc, reference
        DW_AT_entry_pc              = 0x52, // address
        DW_AT_use_UTF8              = 0x53, // flag
        DW_AT_extension             = 0x54, // reference
        DW_AT_ranges                = 0x55, // rangelistptr
        DW_AT_trampoline            = 0x56, // address, flag, reference, string
        DW_AT_call_column           = 0x57, // constant
        DW_AT_call_file             = 0x58, // constant
        DW_AT_call_line             = 0x59, // constant
        DW_AT_description           = 0x5a, // string
        DW_AT_binary_scale          = 0x5b, // constant
        DW_AT_decimal_scale         = 0x5c, // constant
        DW_AT_small                 = 0x5d, // reference
        DW_AT_decimal_sign          = 0x5e, // constant
        DW_AT_digit_count           = 0x5f, // constant
        DW_AT_picture_string        = 0x60, // string
        DW_AT_mutable               = 0x61, // flag
        DW_AT_threads_scaled        = 0x62, // flag
        DW_AT_explicit              = 0x63, // flag
        DW_AT_object_pointer        = 0x64, // reference
        DW_AT_endianity             = 0x65, // constant
        DW_AT_elemental             = 0x66, // flag
        DW_AT_pure                  = 0x67, // flag
        DW_AT_recursive             = 0x68, // flag
        DW_AT_signature             = 0x69, // reference
        DW_AT_main_subprogram       = 0x6a, // flag
        DW_AT_data_bit_offset       = 0x6b, // constant
        DW_AT_const_expr            = 0x6c, // flag
        DW_AT_enum_class            = 0x6d, // flag
        DW_AT_linkage_name          = 0x6e, // string
        DW_AT_lo_user               = 0x2000,
        DW_AT_hi_user               = 0x3fff
    }                                     

    public enum Dwarf2_Form : uint
    {
        DW_FORM_addr        = 0x01, // address
        DW_FORM_block2      = 0x03, // block
        DW_FORM_block4      = 0x04, // block
        DW_FORM_data2       = 0x05, // constant
        DW_FORM_data4       = 0x06, // constant
        DW_FORM_data8       = 0x07, // constant
        DW_FORM_string      = 0x08, // string
        DW_FORM_block       = 0x09, // block
        DW_FORM_block1      = 0x0a, // block
        DW_FORM_data1       = 0x0b, // constant
        DW_FORM_flag        = 0x0c, // flag
        DW_FORM_sdata       = 0x0d, // constant
        DW_FORM_strp        = 0x0e, // string
        DW_FORM_udata       = 0x0f, // constant
        DW_FORM_ref_addr    = 0x10, // reference
        DW_FORM_ref1        = 0x11, // reference
        DW_FORM_ref2        = 0x12, // reference
        DW_FORM_ref4        = 0x13, // reference
        DW_FORM_ref8        = 0x14, // reference
        DW_FORM_ref_udata   = 0x15, // reference
        DW_FORM_indirect    = 0x16, 
        //--// DWARF 4 //--//
        DW_FORM_sec_offset  = 0x17, // lineptr, loclistptr, macptr, rangelistptr
        DW_FORM_exprloc     = 0x18, // exprloc
        DW_FORM_flag_present= 0x19, // flag
        DW_FORM_ref_sig8    = 0x20, // reference
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct Dwarf2_DebugLineHeader
    {
        public UInt32 lh_length;
        public UInt16 lh_version;
        public UInt32 lh_headerLen;
        public byte   lh_minInstructionLen;
        public byte   lh_maxOpsPerInstruction;
        public byte   lh_defaultStmt;
        public sbyte  lh_lineBase;
        public byte   lh_lineRange;
        public byte   lh_opcodeBase;
    }

    public class Dwarf_DebugLineHeaderFile
    {
        public string m_name;
        public uint   m_directoryIndex;
        public uint   m_fileModTime;
        public uint   m_fileLen;
    }

    public static class Dwarf_Utility
    {
        public static string DwarfReadString(BinaryReader bs)
        {
            StringBuilder name = new StringBuilder();
            char c;

            while( '\0' != ( c = (char)bs.ReadByte() ) )
            {
                name.Append( c );
            }
            return name.ToString();
        }
    }

    public enum Dwarf_DebugLineOpCodes : byte
    {
        DW_LNS_copy = 1,
        DW_LNS_advance_pc,
        DW_LNS_advance_line,
        DW_LNS_set_file,
        DW_LNS_set_column,
        DW_LNS_negate_stmt,
        DW_LNS_set_basic_block,
        DW_LNS_const_add_pc,
        DW_LNS_fixed_advance_pc,
        DW_LNS_set_prologue_end,
        DW_LNS_set_epilogue_begin,
        DW_LNS_set_isa,
    }

    public enum Dwarf_DebugLineOpCodesExt : byte
    {
        DW_LNE_end_sequence = 1,
        DW_LNE_set_address,
        DW_LNE_define_file,
    }
}