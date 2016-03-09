//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

#include "unwind.h"
#include "mbed_helpers.h"
#include "llos_unwind.h"

struct ExceptionWrapper
{
    // Header must be the last member of the struct as it may be extended with arbitrary data.
    LLOS_Opaque Exception;
    _Unwind_Exception Header;

    static void Free(_Unwind_Reason_Code reason, _Unwind_Exception* header)
    {
        LLOS__UNREFERENCED_PARAMETER(reason);

        LLOS_FREE(FromHeader(header));
    }

    static ExceptionWrapper* FromHeader(_Unwind_Exception* header)
    {
        uint8_t* wrapper = reinterpret_cast<uint8_t*>(header) - offsetof(ExceptionWrapper, Header);
        return reinterpret_cast<ExceptionWrapper*>(wrapper);
    }
};

// Note: These function parameters are inconsistent with the Itanium ABI detailed at
// (http://mentorembedded.github.io/cxx-abi/abi-eh.html). The prototype defined here must match
// the personality_routine type defined in unwind.h.
extern "C" LLOS_Unwind_Reason_Code LLOS_Personality(
    _Unwind_State state,
    _Unwind_Exception* exceptionObject,
    _Unwind_Context* context)
{
    uint32_t actions = state & (~_US_ACTION_MASK);
    switch (state & _US_ACTION_MASK)
    {
    case _US_VIRTUAL_UNWIND_FRAME:
        actions |= LLOS_UA_SEARCH_PHASE;
        break;

    case _US_UNWIND_FRAME_STARTING:
        actions |= LLOS_UA_CLEANUP_PHASE | LLOS_UA_HANDLER_FRAME;
        break;

    case _US_UNWIND_FRAME_RESUME:
        actions |= LLOS_UA_CLEANUP_PHASE;
        break;

    default:
        abort();
    }

    // Many of the unwind context functions seem to expect the exception object to be set in the
    // first scratch register. This is a departure from the Itanium ABI.
    _Unwind_SetGR(context, 12, (uintptr_t)exceptionObject);

    LLOS_Unwind_Reason_Code result = LLOS_Unwind_Personality(
        static_cast<LLOS_Unwind_Actions>(actions),
        *(uint64_t*)exceptionObject->exception_class,
        (uintptr_t)exceptionObject,
        (uintptr_t)context);

    // The GNU unwinder appears to assume the personality function will unwind the stack. If we're
    // to continue unwinding, reset the context to the next frame up the stack.
    if (result == LLOS_URC_CONTINUE_UNWIND)
    {
        __gnu_unwind_frame(exceptionObject, context);
    }

    return result;
}

uintptr_t LLOS_AllocateException(LLOS_Opaque exception, uint64_t exceptionClass)
{
    ExceptionWrapper* wrapper = (ExceptionWrapper*)LLOS_CALLOC(1, sizeof(ExceptionWrapper));
    *(uint64_t*)wrapper->Header.exception_class = exceptionClass;
    wrapper->Header.exception_cleanup = ExceptionWrapper::Free;
    wrapper->Exception = exception;
    return (uintptr_t)&wrapper->Header;
}

LLOS_Opaque LLOS_GetExceptionObject(uintptr_t exception)
{
    return ExceptionWrapper::FromHeader((_Unwind_Control_Block*)exception)->Exception;
}

uintptr_t LLOS_Unwind_GetIP(uintptr_t context)
{
    return _Unwind_GetIP((_Unwind_Context*)context);
}

uintptr_t LLOS_Unwind_GetLanguageSpecificData(uintptr_t context)
{
    return (uintptr_t)_Unwind_GetLanguageSpecificData((_Unwind_Context*)context);
}

uintptr_t LLOS_Unwind_GetRegionStart(uintptr_t context)
{
    return _Unwind_GetRegionStart((_Unwind_Context*)context);
}

void LLOS_Unwind_RaiseException(uintptr_t exceptionObject)
{
    _Unwind_RaiseException((_Unwind_Exception*)exceptionObject);

    // There was an error while unwinding (usually end of stack). Terminate the app.
    abort();
}

void LLOS_Unwind_SetRegisters(
    uintptr_t context,
    uintptr_t landingPad,
    uintptr_t exceptionObject,
    uintptr_t selector)
{
    _Unwind_Context* contextPtr = (_Unwind_Context*)context;
    _Unwind_SetGR(contextPtr, __builtin_eh_return_data_regno(0), exceptionObject);
    _Unwind_SetGR(contextPtr, __builtin_eh_return_data_regno(1), selector);
    _Unwind_SetIP(contextPtr, landingPad);
}

void LLOS_Terminate()
{
    abort();
}
