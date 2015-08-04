
namespace Llvm.NET.Dwarf
{
    public enum SourceLanguage
    {
        // Language names
        C89 = 0x0001,
        C = 0x0002,
        Ada83 = 0x0003,
        CPlusPlus = 0x0004,
        Cobol74 = 0x0005,
        Cobol85 = 0x0006,
        Fortran77 = 0x0007,
        Fortran90 = 0x0008,
        Pascal83 = 0x0009,
        Modula2 = 0x000a,
        Java = 0x000b,
        C99 = 0x000c,
        Ada95 = 0x000d,
        Fortran95 = 0x000e,
        PLI = 0x000f,
        ObjC = 0x0010,
        ObjCPlusPlus = 0x0011,
        UPC = 0x0012,
        D = 0x0013,

        // New in DWARF 5:
        Python = 0x0014,
        OpenCL = 0x0015,
        Go = 0x0016,
        Modula3 = 0x0017,
        Haskell = 0x0018,
        CPlusPlus_03 = 0x0019,
        CPlusPlus_11 = 0x001a,
        OCaml = 0x001b,

        UserMin = 0x8000,
        LLvmMipsAssembler = UserMin + 1,
        CSharp = UserMin + 0x0100,
        ILAsm = UserMin + 0x01001,
        UserMax = 0xffff
    }
}
