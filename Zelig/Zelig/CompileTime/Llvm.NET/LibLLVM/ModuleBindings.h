#ifndef _MODULE_BINDINGS_H_
#define _MODULE_BINDINGS_H_

#include "llvm-c/Core.h"
#include "IRBindings.h"

#ifdef __cplusplus
extern "C" {
#endif
    typedef struct OpaqueNameMDNodeRef* LLVMNamedMDNodeRef;

    /// See Module::ModFlagBehavior
    enum LLVMModFlagBehavior
    {
        Error = 1,
        Warning = 2,
        Require = 3,
        Override = 4,
        Append = 5,
        AppendUnique = 6,
        ModFlagBehaviorFirstVal = Error,
        ModFlagBehaviorLastVal = AppendUnique
    };

    void LLVMAddModuleFlag( LLVMModuleRef M
                            , LLVMModFlagBehavior behavior
                            , const char *name
                            , uint32_t value
                            );

    LLVMValueRef LLVMGetOrInsertFunction( LLVMModuleRef module, const char* name, LLVMTypeRef functionType );
    char const* LLVMGetModuleName( LLVMModuleRef module );
    LLVMValueRef LLVMGetGlobalAlias( LLVMModuleRef module, char const* name );

    LLVMNamedMDNodeRef LLVMModuleGetModuleFlagsMetadata( LLVMModuleRef module );
    unsigned LLVMNamedMDNodeGetNumOperands( LLVMNamedMDNodeRef namedMDNode );
    /*MDNode*/ LLVMMetadataRef LLVMNamedMDNodeGetOperand( LLVMNamedMDNodeRef namedMDNode, unsigned index );
    LLVMModuleRef LLVMNamedMDNodeGetParentModule( LLVMNamedMDNodeRef namedMDNode );

#ifdef __cplusplus
}
#endif

#endif