#include <llvm/IR/Module.h>
#include "ModuleBindings.h"
#include "IRBindings.h"

using namespace llvm;

DEFINE_SIMPLE_CONVERSION_FUNCTIONS( NamedMDNode, LLVMNamedMDNodeRef )

extern "C"
{
    void LLVMAddModuleFlag( LLVMModuleRef M
                            , LLVMModFlagBehavior behavior
                            , const char *name
                            , uint32_t value
                            )
    {
        unwrap( M )->addModuleFlag( (Module::ModFlagBehavior)behavior, name, value );
    }

    LLVMValueRef LLVMGetOrInsertFunction( LLVMModuleRef module, const char* name, LLVMTypeRef functionType )
    {
        auto pModule = unwrap( module );
        auto pSignature = cast< FunctionType >( unwrap( functionType ) );
        return wrap( pModule->getOrInsertFunction( name, pSignature ) );
    }

    char const* LLVMGetModuleName( LLVMModuleRef module )
    {
        auto pModule = unwrap( module );
        return pModule->getModuleIdentifier( ).c_str( );
    }

    LLVMValueRef LLVMGetGlobalAlias( LLVMModuleRef module, char const* name )
    {
        auto pModule = unwrap( module );
        return wrap( pModule->getNamedAlias( name ) );
    }

    LLVMNamedMDNodeRef LLVMModuleGetModuleFlagsMetadata( LLVMModuleRef module )
    {
        auto pModule = unwrap( module );
        return wrap( pModule->getModuleFlagsMetadata( ) );
    }

    unsigned LLVMNamedMDNodeGetNumOperands( LLVMNamedMDNodeRef namedMDNode )
    {
        auto pMDNode = unwrap( namedMDNode );
        return pMDNode->getNumOperands( );
    }

    LLVMMetadataRef LLVMNamedMDNodeGetOperand( LLVMNamedMDNodeRef namedMDNode, unsigned index )
    {
        auto pMDNode = unwrap( namedMDNode );
        if( index >= pMDNode->getNumOperands( ) )
            return nullptr;

        return wrap( pMDNode->getOperand( index ) );
    }

    LLVMModuleRef LLVMNamedMDNodeGetParentModule( LLVMNamedMDNodeRef namedMDNode )
    {
        auto pMDNode = unwrap( namedMDNode );
        return wrap( pMDNode->getParent( ) );
    }
}