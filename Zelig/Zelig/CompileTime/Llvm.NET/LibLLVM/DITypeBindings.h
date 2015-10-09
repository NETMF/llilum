#ifndef LLVM_BINDINGS_LLVM_DITYPEBINDINGS_H
#define LLVM_BINDINGS_LLVM_DITYPEBINDINGS_H

#include "IRBindings.h"
#include "llvm-c/Core.h"

#ifdef __cplusplus
extern "C" {
#endif

    unsigned LLVMDITypeGetLine( LLVMMetadataRef typeRef );
    uint64_t LLVMDITypeGetSizeInBits( LLVMMetadataRef typeRef );
    uint64_t LLVMDITypeGetAlignInBits( LLVMMetadataRef typeRef );
    uint64_t LLVMDITypeGetOffsetInBits( LLVMMetadataRef typeRef );
    unsigned LLVMDITypeGetFlags( LLVMMetadataRef typeRef );
    LLVMMetadataRef LLVMDITypeGetScope( LLVMMetadataRef typeRef );
    char const* LLVMDITypeGetName( LLVMMetadataRef typeRef );
    LLVMMetadataRef LLVMDIScopeGetFile( LLVMMetadataRef typeRef );

#ifdef __cplusplus
}
#endif

#endif
