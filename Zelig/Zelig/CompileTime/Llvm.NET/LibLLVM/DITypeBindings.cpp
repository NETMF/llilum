#include "DITypeBindings.h"
#include "llvm/IR/DIBuilder.h"
#include "llvm/IR/IRBuilder.h"
#include "llvm/IR/Module.h"
#include "llvm/IR/Constant.h"
#include "llvm/Support/raw_ostream.h"

using namespace llvm;

extern "C"
{
    unsigned LLVMDITypeGetLine( LLVMMetadataRef typeRef )
    {
        DIType* pType = unwrap<DIType>( typeRef );
        return pType->getLine( );
    }

    uint64_t LLVMDITypeGetSizeInBits( LLVMMetadataRef typeRef )
    {
        DIType* pType = unwrap<DIType>( typeRef );
        return pType->getSizeInBits( );
    }

    uint64_t LLVMDITypeGetAlignInBits( LLVMMetadataRef typeRef )
    {
        DIType* pType = unwrap<DIType>( typeRef );
        return pType->getAlignInBits( );
    }

    uint64_t LLVMDITypeGetOffsetInBits( LLVMMetadataRef typeRef )
    {
        DIType* pType = unwrap<DIType>( typeRef );
        return pType->getOffsetInBits( );
    }

    unsigned LLVMDITypeGetFlags( LLVMMetadataRef typeRef )
    {
        DIType* pType = unwrap<DIType>( typeRef );
        return pType->getFlags( );
    }

    LLVMMetadataRef LLVMDITypeGetScope( LLVMMetadataRef typeRef )
    {
        DIType* pType = unwrap<DIType>( typeRef );
        return wrap( pType->getScope( ) );
    }

    char const* LLVMDITypeGetName( LLVMMetadataRef typeRef )
    {
        DIType* pType = unwrap<DIType>( typeRef );
        return pType->getName( ).data( );
    }

    LLVMMetadataRef LLVMDIScopeGetFile( LLVMMetadataRef typeRef )
    {
        DIType* pType = unwrap<DIType>( typeRef );
        return wrap( pType->getFile() );
    }
}
