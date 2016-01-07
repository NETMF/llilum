#include "stdafx.h"
#include <llos_event.h>

HRESULT LLOS_EVENT_Create(LLOS_Context attributes, BOOL manualReset, BOOL initialState, wchar_t* eventName, LLOS_Handle* eventHandle)
{
    HANDLE hndEvent = CreateEvent((LPSECURITY_ATTRIBUTES)attributes, manualReset, initialState, eventName);

    if (hndEvent != INVALID_HANDLE_VALUE)
    {
        *eventHandle = hndEvent;
        return S_OK;
    }
    else
    { 
        *eventHandle = nullptr;
        return E_FAIL;
    }
}

HRESULT LLOS_EVENT_Delete(LLOS_Handle eventHandle)
{
    if (eventHandle != INVALID_HANDLE_VALUE)
    {
        return CloseHandle((HANDLE)eventHandle) ? S_OK : E_FAIL;
    }

    return S_OK;
}

HRESULT LLOS_EVENT_Set(LLOS_Handle eventHandle)
{
    if (eventHandle == INVALID_HANDLE_VALUE)
    {
        return E_HANDLE;
    }
    SwitchToThread();
    return SetEvent((HANDLE)eventHandle) ? S_OK : E_FAIL;
}

HRESULT LLOS_EVENT_Reset(LLOS_Handle eventHandle)
{
    if (eventHandle == INVALID_HANDLE_VALUE)
    {
        return E_HANDLE;
    }

    return ResetEvent((HANDLE)eventHandle) ? S_OK : E_FAIL;
}

HRESULT LLOS_EVENT_Wait(LLOS_Handle eventHandle, int32_t timeoutMs)
{
    HRESULT hr = S_OK;

    if (eventHandle == INVALID_HANDLE_VALUE)
    {
        return E_HANDLE;
    }

    switch (WaitForSingleObject((HANDLE)eventHandle, timeoutMs))
    {
    case WAIT_ABANDONED:
        hr = E_ABORT;
        break;

    case WAIT_TIMEOUT:
        hr = E_BOUNDS;
        break;

    case WAIT_OBJECT_0:
        break;

    default:
        DebugBreak();
        hr = E_UNEXPECTED;
        break;
    }

    return hr;
}


