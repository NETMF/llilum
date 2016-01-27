#include "LlosWin32.h"
#include <llos_mutex.h>
#include <llos_system_timer.h>

typedef struct LlosTimerEntry
{
    HANDLE hndThread;
    HANDLE hndEvent;
    BOOL fExit;
    LLOS_SYSTEM_TIMER_Callback callback;
    LLOS_Context callbackContext;
    uint64_t waitTime;
    CRITICAL_SECTION cs;
    
} LlosTimerEntry;

DWORD WINAPI LlosTimerThreadProc( LPVOID lpThreadParameter )
{
    LlosTimerEntry *pTimer = (LlosTimerEntry*)lpThreadParameter;
    uint32_t waitTimeout = 0;

    if (pTimer == nullptr)
    {
        return E_INVALIDARG;
    } 

    LlosThread** ppvData = (LlosThread**)LocalAlloc(LPTR, sizeof(LlosThread) + sizeof(LlosThread*));

    if (ppvData == nullptr)
    {
        return E_OUTOFMEMORY;
    }

    *ppvData = (LlosThread*)&ppvData[1];
    TlsSetValue(g_dwTlsIndex, ppvData);

    while (!pTimer->fExit)
    {
        BOOL fTimerExpired = FALSE;

        EnterCriticalSection(&pTimer->cs);
        if (pTimer->waitTime >= INFINITE)
        {
            waitTimeout = 10000;
        }
        else
        {
            waitTimeout = (uint32_t)(pTimer->waitTime / 1000);
        }
        pTimer->waitTime = INFINITE;
        LeaveCriticalSection(&pTimer->cs);


        switch (WaitForSingleObject(pTimer->hndEvent, (uint32_t)waitTimeout))
        {
        case WAIT_OBJECT_0:
            break;

        case WAIT_TIMEOUT:
            fTimerExpired = TRUE;
            break;

        default:
            DebugBreak();
            break;
        }

        LLOS_MUTEX_Acquire((LLOS_Context)g_globalMutex, -1);

        pTimer->callback(pTimer->callbackContext, LLOS_SYSTEM_TIMER_GetTicks(pTimer));

        LLOS_MUTEX_Release((LLOS_Context)g_globalMutex);
    }

    LocalFree(ppvData);

    return 0;
}

HRESULT LLOS_SYSTEM_TIMER_AllocateTimer(LLOS_SYSTEM_TIMER_Callback callback, LLOS_Context callbackContext, uint64_t microsecondsFromNow, LLOS_Context *pTimer)
{
    LlosTimerEntry *pEntry = nullptr;

    if (pTimer == nullptr || callback == nullptr)
    {
        return E_INVALIDARG;
    }

    pEntry = (LlosTimerEntry*)calloc(1, sizeof(LlosTimerEntry));

    if (pEntry != nullptr)
    {
        pEntry->callback = callback;
        pEntry->callbackContext = callbackContext;
        pEntry->fExit = FALSE;
        pEntry->waitTime = microsecondsFromNow;

        if (!InitializeCriticalSectionAndSpinCount(&pEntry->cs, 0x00000400))
        {
            return E_FAIL;
        }

        pEntry->hndEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
        pEntry->hndThread = CreateThread(nullptr, 1024 * 1024, LlosTimerThreadProc, pEntry, 0, nullptr);

        *pTimer = pEntry;
    }

    return pEntry != nullptr ? S_OK : E_FAIL;
}

VOID LLOS_SYSTEM_TIMER_FreeTimer(LLOS_Context pTimer)
{
    if (pTimer != nullptr)
    {
        LlosTimerEntry *pEntry = (LlosTimerEntry*)pTimer;
        pEntry->fExit = TRUE;
        SetEvent(pEntry->hndEvent);
        WaitForSingleObject(pEntry->hndThread, INFINITE);
        CloseHandle(pEntry->hndThread);
        CloseHandle(pEntry->hndEvent);
        DeleteCriticalSection(&pEntry->cs);
        free(pTimer);
    }
}

HRESULT LLOS_SYSTEM_TIMER_ScheduleTimer(LLOS_Context pTimer, uint64_t microsecondsFromNow)
{
    LlosTimerEntry *pEntry = (LlosTimerEntry*)pTimer;

    if (pTimer == nullptr)
    {
        return E_INVALIDARG;
    }

    EnterCriticalSection(&pEntry->cs);
    pEntry->waitTime = microsecondsFromNow;
    LeaveCriticalSection(&pEntry->cs);

    SetEvent(pEntry->hndEvent);
    SwitchToThread();

    return S_OK;
}

uint64_t LLOS_SYSTEM_TIMER_GetTicks(LLOS_Context pTimer)
{
    return GetTickCount64();
}

uint64_t LLOS_SYSTEM_TIMER_GetTimerFrequency(LLOS_Context pTimer)
{
    return 1000;
}

