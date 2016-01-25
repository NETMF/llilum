//
//    LLILUM OS Abstraction Layer - Mutex
// 

#ifndef __LLOS_MUTEX_H__
#define __LLOS_MUTEX_H__

#ifdef __cplusplus
extern "C" {
#endif

#include "llos_types.h"

HRESULT  LLOS_MUTEX_CreateGlobalLock(LLOS_Handle* mutexHandle);
HRESULT  LLOS_MUTEX_Create(LLOS_Context attributes, LLOS_Context name, LLOS_Handle* mutexHandle);
HRESULT  LLOS_MUTEX_Acquire(LLOS_Handle mutexHandle, int32_t timeout);
HRESULT  LLOS_MUTEX_Release(LLOS_Handle mutexHandle);
BOOL     LLOS_MUTEX_CurrentThreadHasLock(LLOS_Handle mutexHandle);
HRESULT  LLOS_MUTEX_Delete(LLOS_Handle mutexHandle);

#ifdef __cplusplus
}
#endif

#endif // __LLOS_MUTEX_H__
