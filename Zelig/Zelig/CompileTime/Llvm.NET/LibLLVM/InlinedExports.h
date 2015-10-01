#ifndef _INLINED_EXPORTS_H_
#define _INLINED_EXPORTS_H_

#include "llvm-c\Support.h"

#ifdef __cplusplus
extern "C" {
#endif
    // exportable wrappers around static inlined functions
    // EXPORTS.DEF uses aliasing to export these as the standard name
    // (e.g. dropping the trailing "Export" from the name.) This 
    // mechanism is needed since the inlined functions are marked static
    // and therefore the linker doesn't see them as externally visible
    // so it can't export them. 

    /** LLVMInitializeAllTargetInfos - The main program should call this function if
    it wants access to all available targets that LLVM is configured to
    support. */
    void LLVMInitializeAllTargetInfosExport( void );

    /** LLVMInitializeAllTargets - The main program should call this function if it
    wants to link in all available targets that LLVM is configured to
    support. */
    void LLVMInitializeAllTargetsExport( void );

    /** LLVMInitializeAllTargetMCs - The main program should call this function if
    it wants access to all available target MC that LLVM is configured to
    support. */
    void LLVMInitializeAllTargetMCsExport( void );

    /** LLVMInitializeAllAsmPrinters - The main program should call this function if
    it wants all asm printers that LLVM is configured to support, to make them
    available via the TargetRegistry. */
    void LLVMInitializeAllAsmPrintersExport( void );

    /** LLVMInitializeAllAsmParsers - The main program should call this function if
    it wants all asm parsers that LLVM is configured to support, to make them
    available via the TargetRegistry. */
    void LLVMInitializeAllAsmParsersExport( void );

    /** LLVMInitializeAllDisassemblers - The main program should call this function
    if it wants all disassemblers that LLVM is configured to support, to make
    them available via the TargetRegistry. */
    void LLVMInitializeAllDisassemblersExport( void );

    LLVMBool LLVMInitializeNativeTargetExport(void);
    LLVMBool LLVMInitializeNativeAsmParserExport(void);
    LLVMBool LLVMInitializeNativeAsmPrinterExport(void);
    LLVMBool LLVMInitializeNativeDisassemblerExport(void);
#ifdef __cplusplus
}
#endif

#endif