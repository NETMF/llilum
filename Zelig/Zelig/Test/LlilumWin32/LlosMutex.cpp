#include "stdafx.h"
#include <llos_mutex.h>

HANDLE g_globalMutex = INVALID_HANDLE_VALUE;

HRESULT LLOS_MUTEX_Create(LLOS_Context attributes, LLOS_Context name, LLOS_Handle* mutexHandle)
{
    *mutexHandle = CreateMutex((LPSECURITY_ATTRIBUTES)attributes, false, (LPCWSTR)name);

    g_globalMutex = *mutexHandle;

    return *mutexHandle != INVALID_HANDLE_VALUE ? S_OK : E_FAIL;
}

HRESULT LLOS_MUTEX_Acquire(LLOS_Handle mutexHandle, int32_t timeout)
{
    if (mutexHandle != INVALID_HANDLE_VALUE && mutexHandle != 0)
    {
        if (WAIT_OBJECT_0 == WaitForSingleObject(mutexHandle, timeout))
        {
            volatile LlosThread *tlsThread = GetThreadLocalStorage();
            if (tlsThread != nullptr)
            {
                InterlockedIncrement(&tlsThread->globalLockRefCount);
            }
        }
    }

    return S_OK;
}

HRESULT LLOS_MUTEX_Release(LLOS_Handle mutexHandle)
{
    if (mutexHandle != INVALID_HANDLE_VALUE && mutexHandle != 0)
    {
        if (ReleaseMutex(mutexHandle))
        {
            volatile LlosThread *tlsThread = GetThreadLocalStorage();
            if (tlsThread != nullptr)
            {
                InterlockedDecrement(&tlsThread->globalLockRefCount);
            }
        }
    }

    return S_OK;
}

BOOL LLOS_MUTEX_CurrentThreadHasLock(LLOS_Handle mutexHandle)
{
    BOOL ownsMutex = FALSE;

    if (mutexHandle == INVALID_HANDLE_VALUE)
    {
        return FALSE;
    }

    switch(WaitForSingleObject(mutexHandle, 0))
    {
        case WAIT_OBJECT_0:
            {
                volatile LlosThread *tlsThread = GetThreadLocalStorage();
                ownsMutex = tlsThread == nullptr || tlsThread->globalLockRefCount > 0;
                ReleaseMutex(mutexHandle);
            }
            break;

        case WAIT_TIMEOUT:
        case WAIT_ABANDONED:
        default:
            break;
    }

    return ownsMutex;
}

HRESULT LLOS_MUTEX_Delete(LLOS_Handle mutexHandle)
{
    return CloseHandle(mutexHandle) ? S_OK : E_FAIL;
}


