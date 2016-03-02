#include "LlosWin32.h"
#include <llos_thread.h>

static DWORD WINAPI LlosThreadEntryWrapper(LPVOID lpThreadParameter)
{
    LlosThread *pThread = (LlosThread*)lpThreadParameter;
    LlosThread** ppvData = nullptr;

    if (pThread == nullptr)
    {
        return 1;
    }

    try
    {
        ppvData = (LlosThread**)LocalAlloc(LPTR, sizeof(LlosThread*));

        if (ppvData == nullptr)
        {
            return 1;
        }

        *ppvData = pThread;
        TlsSetValue(g_dwTlsIndex, ppvData);

        pThread->entry(pThread->param);
    }
    catch (...)
    {
    }

    if (ppvData != nullptr)
    {
        TlsSetValue(g_dwTlsIndex, nullptr);
        LocalFree(ppvData);
    }

    return 0;
}

LlosThread* GetThreadLocalStorage()
{
    LlosThread** lpvData = (LlosThread**)TlsGetValue(g_dwTlsIndex);

    if (lpvData != nullptr)
    {
        return *lpvData;
    }

    return nullptr;
}

HRESULT LLOS_THREAD_GetCurrentThread(LLOS_Context* threadHandle)
{
    LlosThread *pThread;

    if (threadHandle == nullptr)
    {
        return E_INVALIDARG;
    }

    pThread = GetThreadLocalStorage();

    if (pThread != nullptr)
    {
        *threadHandle = pThread->managedThread;
    }
    else
    {
        *threadHandle = nullptr;
    }

    return *threadHandle != nullptr ? S_OK : E_FAIL;
}

HRESULT LLOS_THREAD_CreateThread(LLOS_ThreadEntry threadEntry, LLOS_Context threadParameter, LLOS_Context managedThread, uint32_t stackSize, LLOS_Handle* threadHandle)
{
    LlosThread *pThread = (LlosThread*)calloc(1, sizeof(LlosThread));

    if (threadHandle == nullptr)
    {
        return E_INVALIDARG;
    }

    if (pThread == nullptr)
    {
        return E_OUTOFMEMORY;
    }

    pThread->entry = threadEntry;
    pThread->param = threadParameter;
    pThread->managedThread = managedThread;
    pThread->globalLockRefCount = 0;

    pThread->hndThread = CreateThread(NULL, stackSize, LlosThreadEntryWrapper, pThread, CREATE_SUSPENDED, NULL);

    if (pThread->hndThread == INVALID_HANDLE_VALUE)
    {
        free(pThread);
        pThread = nullptr;
    }

    *threadHandle = pThread;

    return pThread != nullptr ? S_OK : E_FAIL;
}

HRESULT LLOS_THREAD_Start(LLOS_Handle threadHandle)
{
    LlosThread *pThread = (LlosThread*)threadHandle;

    if (pThread == nullptr)
    {
        return E_INVALIDARG;
    }

    return ResumeThread(pThread->hndThread);
}

HRESULT LLOS_THREAD_Yield(VOID)
{
    SwitchToThread();

    return S_OK;
}

HRESULT LLOS_THREAD_Wait(LLOS_Handle threadHandle, int32_t timeoutMs)
{
    HRESULT hr;
    LlosThread *pThread = (LlosThread*)threadHandle;

    if (pThread == nullptr)
    {
        return E_INVALIDARG;
    }

    hr = WaitOnAddress(&pThread->waitAddress, &pThread->compareAddress, sizeof(DWORD), timeoutMs) ? S_OK : E_ABORT;
    pThread->waitAddress = 0;

    return hr;
}

HRESULT LLOS_THREAD_Signal(LLOS_Handle threadHandle)
{
    LlosThread *pThread = (LlosThread*)threadHandle;

    if (pThread == nullptr)
    {
        return E_INVALIDARG;
    }

    pThread->waitAddress = 1;
    WakeByAddressAll(&pThread->waitAddress);

    return S_OK;
}

HRESULT LLOS_THREAD_SetPriority(LLOS_Handle threadHandle, LLOS_ThreadPriority threadPriority)
{
    int pri = THREAD_PRIORITY_NORMAL;
    LlosThread *pThread = (LlosThread*)threadHandle;

    if (pThread == nullptr)
    {
        return E_INVALIDARG;
    }

    switch (threadPriority)
    {
    case ThreadPriority_Lowest:
        pri = THREAD_PRIORITY_LOWEST;
        break;
    case ThreadPriority_BelowNormal:
        pri = THREAD_PRIORITY_BELOW_NORMAL;
        break;
    case ThreadPriority_Normal:
        pri = THREAD_PRIORITY_NORMAL;
        break;
    case ThreadPriority_AboveNormal:
        pri = THREAD_PRIORITY_ABOVE_NORMAL;
        break;
    case ThreadPriority_Highest:
        pri = THREAD_PRIORITY_HIGHEST;
        break;

    default:
        break;
    }

    return SetThreadPriority(pThread->hndThread, pri) ? S_OK : E_FAIL;
}

HRESULT LLOS_THREAD_GetPriority(LLOS_Handle threadHandle, LLOS_ThreadPriority* threadPriority)
{
    LLOS_ThreadPriority llosPri = ThreadPriority_Normal;
    LlosThread *pThread = (LlosThread*)threadHandle;

    if (pThread == nullptr || threadPriority == nullptr)
    {
        return E_INVALIDARG;
    }

    int pri = GetThreadPriority(pThread->hndThread);

    switch (pri)
    {
    case THREAD_PRIORITY_LOWEST:
        llosPri = ThreadPriority_Lowest;
        break;
    case THREAD_PRIORITY_BELOW_NORMAL:
        llosPri = ThreadPriority_BelowNormal;
        break;
    case THREAD_PRIORITY_NORMAL:
        llosPri = ThreadPriority_Normal;
        break;
    case THREAD_PRIORITY_ABOVE_NORMAL:
        llosPri = ThreadPriority_AboveNormal;
        break;
    case THREAD_PRIORITY_HIGHEST:
        llosPri = ThreadPriority_Highest;
        break;

    default:
        break;
    }

    *threadPriority = llosPri;

    return S_OK;
}

HRESULT LLOS_THREAD_DeleteThread(LLOS_Handle threadHandle)
{
    LlosThread *pThread = (LlosThread*)threadHandle;

    if (pThread != nullptr)
    {
        WaitForSingleObject(pThread->hndThread, -1);
        CloseHandle(pThread->hndThread);
        free(pThread);
    }

    return S_OK;
}

VOID LLOS_THREAD_Sleep(int32_t timeoutMilliseconds)
{
    Sleep(timeoutMilliseconds);
}

uint32_t LLOS_THREAD_GetMainStackAddress()
{
    return 0;
}

uint32_t LLOS_THREAD_GetMainStackSize()
{
    return 0;
}


