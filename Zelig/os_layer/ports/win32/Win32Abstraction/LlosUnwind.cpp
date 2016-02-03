//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

#include "LlosWin32.h"
#include "llos_unwind.h"

extern "C" LLOS_Unwind_Reason_Code LLOS_Personality(
    int /*version*/,
    LLOS_Unwind_Actions actions,
    uint64_t exceptionClass,
    struct _Unwind_Exception* exceptionObject,
    struct _Unwind_Context* context)
{
    return LLOS_Unwind_Personality(actions, exceptionClass, (uintptr_t)exceptionObject, (uintptr_t)context);
}

uintptr_t LLOS_AllocateException(LLOS_Opaque exception, uint64_t exceptionClass)
{
    return 0;
}

LLOS_Opaque LLOS_GetExceptionObject(uintptr_t exception)
{
    return NULL;
}

uintptr_t LLOS_Unwind_GetIP(uintptr_t context)
{
    return 0;
}

uintptr_t LLOS_Unwind_GetLanguageSpecificData(uintptr_t context)
{
    return 0;
}

uintptr_t LLOS_Unwind_GetRegionStart(uintptr_t context)
{
    return 0;
}

void LLOS_Unwind_RaiseException(uintptr_t exceptionObject)
{
    abort();
}

void LLOS_Unwind_SetRegisters(
    uintptr_t context,
    uintptr_t landingPad,
    uintptr_t exceptionObject,
    uintptr_t selector)
{
}

void LLOS_Terminate()
{
    abort();
}
