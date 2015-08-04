#ifndef _ANALYSIS_BINDINGS_H_
#define _ANALYSIS_BINDINGS_H_

#include "llvm-c/Core.h"
#include "llvm-c/Analysis.h"

#ifdef __cplusplus
extern "C" {
#endif

    LLVMBool LLVMVerifyFunctionEx( LLVMValueRef Fn
                                   , LLVMVerifierFailureAction Action
                                   , char **OutMessages
                                   );
#ifdef __cplusplus
}
#endif

#endif