//
//    LLILUM OS Abstraction Layer - Debug
// 

#ifndef __LLOS_DEBUG_H__
#define __LLOS_DEBUG_H__

#ifdef __cplusplus
extern "C" {
#endif

#include "llos_types.h"

VOID LLOS_DEBUG_Break(uint32_t code);
VOID LLOS_DEBUG_LogText(wchar_t* text, int32_t textLength);

#ifdef __cplusplus
}
#endif

#endif // __LLOS_DEBUG_H__
