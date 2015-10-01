#include "ValueBindings.h"
#include <llvm\IR\Constant.h>
#include <llvm\IR\GlobalVariable.h>
#include <llvm\IR\IRBuilder.h>

using namespace llvm;

LLVMBool LLVMIsConstantZeroValue( LLVMValueRef valueRef )
{
    auto pConstant = dyn_cast< Constant >( unwrap( valueRef ) );
    if( pConstant == nullptr )
        return 0;

    return pConstant->isZeroValue( ) ? 1 : 0;
}

void LLVMRemoveGlobalFromParent( LLVMValueRef valueRef )
{
    auto pGlobal = dyn_cast< GlobalVariable >( unwrap( valueRef ) ); 
    if( pGlobal == nullptr )
        return;

    pGlobal->removeFromParent();
}

LLVMValueRef LLVMBuildIntCast2(LLVMBuilderRef B, LLVMValueRef Val, LLVMTypeRef DestTy, LLVMBool isSigned, const char *Name)
{
  return wrap( unwrap(B)->CreateIntCast( unwrap(Val), unwrap(DestTy), isSigned, Name ) );
}

int LLVMGetValueID( LLVMValueRef valueRef )
{
    return unwrap( valueRef )->getValueID();
}

LLVMValueRef LLVMMetadataAsValue( LLVMContextRef context, LLVMMetadataRef metadataRef )
{
    LLVMContext* ctx = unwrap( context );
    auto md = unwrap( metadataRef );
    return wrap( MetadataAsValue::get( *ctx, md ) );
}

