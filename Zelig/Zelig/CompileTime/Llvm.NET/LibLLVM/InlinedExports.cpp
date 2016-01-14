#include "llvm-c/Target.h"

extern "C"
{
    // exportable wrappers around static in-lined functions
    // EXPORTS.DEF uses aliasing to export these as the standard name
    // (e.g. dropping the trailing "Export" from the name.) This 
    // mechanism is needed since the in-lined functions are marked static
    // and therefore the linker doesn't see them as externally visible
    // so it can't export them. 

    /** LLVMInitializeAllTargetInfos - The main program should call this function if
    it wants access to all available targets that LLVM is configured to
    support. */
    void LLVMInitializeAllTargetInfosExport( void )
    {
        LLVMInitializeAllTargetInfos( );
    }

    /** LLVMInitializeAllTargets - The main program should call this function if it
    wants to link in all available targets that LLVM is configured to
    support. */
    void LLVMInitializeAllTargetsExport( void )
    {
        LLVMInitializeAllTargets( );
    }

    /** LLVMInitializeAllTargetMCs - The main program should call this function if
    it wants access to all available target MC that LLVM is configured to
    support. */
    void LLVMInitializeAllTargetMCsExport( void )
    {
        LLVMInitializeAllTargetMCs( );
    }

    /** LLVMInitializeAllAsmPrinters - The main program should call this function if
    it wants all asm printers that LLVM is configured to support, to make them
    available via the TargetRegistry. */
    void LLVMInitializeAllAsmPrintersExport( void )
    {
        LLVMInitializeAllAsmPrinters( );
    }

    /** LLVMInitializeAllAsmParsers - The main program should call this function if
    it wants all asm parsers that LLVM is configured to support, to make them
    available via the TargetRegistry. */
    void LLVMInitializeAllAsmParsersExport( void )
    {
        LLVMInitializeAllAsmParsers( );
    }

    /** LLVMInitializeAllDisassemblers - The main program should call this function
    if it wants all disassemblers that LLVM is configured to support, to make
    them available via the TargetRegistry. */
    void LLVMInitializeAllDisassemblersExport( void )
    {
        LLVMInitializeAllDisassemblers( );
    }

    LLVMBool LLVMInitializeNativeTargetExport(void)
    {
        return LLVMInitializeNativeTarget();
    }

    LLVMBool LLVMInitializeNativeAsmParserExport(void)
    {
        return LLVMInitializeNativeAsmParser();
    }

    LLVMBool LLVMInitializeNativeAsmPrinterExport(void)
    {
        return LLVMInitializeNativeAsmPrinter();
    }

    LLVMBool LLVMInitializeNativeDisassemblerExport(void)
    {
        return LLVMInitializeNativeDisassembler();
    }
}
