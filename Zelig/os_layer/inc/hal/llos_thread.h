//
//    LLILUM OS Abstraction Layer - Thread
// 

#ifndef __LLOS_THREAD_H__
#define __LLOS_THREAD_H__

#ifdef __cplusplus
extern "C" {
#endif

#include "llos_types.h"

    typedef enum LLOS_ThreadPriority
    {
        ThreadPriority_Lowest = 0,
        ThreadPriority_BelowNormal,
        ThreadPriority_Normal,
        ThreadPriority_AboveNormal,
        ThreadPriority_Highest,

    } LLOS_ThreadPriority;

    HRESULT  LLOS_THREAD_CreateThread       (LLOS_ThreadEntry threadEntry, LLOS_Context threadParameter, LLOS_Context managedThread, uint32_t stackSize, LLOS_Handle* threadHandle);
    HRESULT  LLOS_THREAD_DeleteThread       (LLOS_Handle threadHandle);
    HRESULT  LLOS_THREAD_Start              (LLOS_Handle threadHandle);
    HRESULT  LLOS_THREAD_Yield              (VOID);
    HRESULT  LLOS_THREAD_Signal             (LLOS_Handle threadHandle);
    HRESULT  LLOS_THREAD_Wait               (LLOS_Handle threadHandle, int32_t timeoutMs);
    VOID     LLOS_THREAD_Sleep              (int32_t timeoutMilliseconds);
    HRESULT  LLOS_THREAD_GetCurrentThread   (LLOS_Context* managedThread);
    HRESULT  LLOS_THREAD_SetPriority        (LLOS_Handle threadHandle, LLOS_ThreadPriority  threadPriority);
    HRESULT  LLOS_THREAD_GetPriority        (LLOS_Handle threadHandle, LLOS_ThreadPriority* threadPriority);
    uint32_t LLOS_THREAD_GetMainStackAddress( );
    uint32_t LLOS_THREAD_GetMainStackSize   ( );

#ifdef __cplusplus
}
#endif

#endif // __LLOS_THREAD_H__
