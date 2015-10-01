#ifndef _MODULE_BINDINGS_H_
#define _MODULE_BINDINGS_H_

#include "llvm-c/Core.h"

#ifdef __cplusplus
extern "C" {
#endif

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

#ifdef __cplusplus
}
#endif

#endif