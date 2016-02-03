//
// LLILUM OS Abstraction Layer - Unwind
//

#ifndef __LLOS_UNWIND_H__
#define __LLOS_UNWIND_H__

#include <llos_types.h>

// NOTE: This is a very light abstraction over the Itanium ABI. Different platforms implement the
// ABI in slighty different ways. This allows a specific platform to override the behavior where
// they're not entirely compliant with the spec as described at the following URL:
// http://mentorembedded.github.io/cxx-abi/abi-eh.html

// Status flags describing the unwind phase and options. Should match _Unwind_Action in ABI.
enum LLOS_Unwind_Actions
{
    LLOS_UA_SEARCH_PHASE    = 0x01, // Mutually exclusive with CLEANUP_PHASE
    LLOS_UA_CLEANUP_PHASE   = 0x02, // Mutually exclusive with SEARCH_PHASE
    LLOS_UA_HANDLER_FRAME   = 0x04,
    LLOS_UA_FORCE_UNWIND    = 0x08,
    LLOS_UA_END_OF_STACK    = 0x16,
};

// Result of any given unwind operation; values should match _Unwind_Reason_Code.
enum LLOS_Unwind_Reason_Code
{
    LLOS_URC_NO_REASON                  = 0, // Success
    LLOS_URC_FOREIGN_EXCEPTION_CAUGHT   = 1,
    LLOS_URC_PHASE2_ERROR               = 2,
    LLOS_URC_PHASE1_ERROR               = 3,
    LLOS_URC_NORMAL_STOP                = 4,
    LLOS_URC_END_OF_STACK               = 5,
    LLOS_URC_HANDLER_FOUND              = 6,
    LLOS_URC_INSTALL_CONTEXT            = 7,
    LLOS_URC_CONTINUE_UNWIND            = 8,
    LLOS_URC_FAILURE                    = 9,
};

extern "C"
{
    LLOS_Unwind_Reason_Code LLOS_Unwind_Personality(
        LLOS_Unwind_Actions actions,
        uint64_t exceptionClass,
        uintptr_t exceptionObject,
        uintptr_t context);

    uintptr_t LLOS_AllocateException(LLOS_Opaque exception, uint64_t exceptionClass);
    LLOS_Opaque LLOS_GetExceptionObject(uintptr_t exception);
    uintptr_t LLOS_Unwind_GetIP(uintptr_t context);
    uintptr_t LLOS_Unwind_GetLanguageSpecificData(uintptr_t context);
    uintptr_t LLOS_Unwind_GetRegionStart(uintptr_t context);
    void LLOS_Unwind_RaiseException(uintptr_t exceptionObject);
    void LLOS_Unwind_SetRegisters(
        uintptr_t context,
        uintptr_t landingPad,
        uintptr_t exceptionObject,
        uintptr_t selector);
    void LLOS_Terminate();
}

#endif // __LLOS_UNWIND_H__
