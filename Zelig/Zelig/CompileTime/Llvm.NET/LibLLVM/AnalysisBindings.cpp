#include "AnalysisBindings.h"
#include "llvm-c/Analysis.h"
#include "llvm-c/Initialization.h"
#include "llvm/IR/Module.h"
#include "llvm/IR/Verifier.h"
#include "llvm/Support/raw_ostream.h"
#include <cstring>

using namespace llvm;

extern "C"
{
    // The standard LLVMVerifyFunction (unlike LLVMVerifyModule) doesn't provide
    // a way to capture error message strings (only success/fail or print to
    // stderr is available). This provides a version that matches the general
    // patern established by LLVMVerifyModule(). 
    LLVMBool LLVMVerifyFunctionEx( LLVMValueRef Fn
                                   , LLVMVerifierFailureAction Action
                                   , char **OutMessages
                                   )
    {
        raw_ostream *DebugOS = Action != LLVMReturnStatusAction ? &errs( ) : nullptr;
        std::string Messages;
        raw_string_ostream MsgsOS( Messages );

        LLVMBool Result = verifyFunction( *unwrap<Function>( Fn ), OutMessages ? &MsgsOS : DebugOS );

        // Duplicate the output to stderr.
        if( DebugOS && OutMessages )
            *DebugOS << MsgsOS.str( );

        if( Action == LLVMAbortProcessAction && Result )
            report_fatal_error( "Broken function found, compilation aborted!" );

        if( OutMessages )
            *OutMessages = strdup( MsgsOS.str( ).c_str( ) );

        return Result;
    }
}

