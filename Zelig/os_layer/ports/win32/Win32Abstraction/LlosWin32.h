#include <Windows.h>
#include <stdio.h>
#include <tchar.h>

#include <llos_types.h>

#pragma once

typedef struct LlosThread
{
    LLOS_ThreadEntry entry;
    LLOS_Context     param;
    LLOS_Context     managedThread;
    HANDLE           hndThread;
    DWORD            globalLockRefCount;
    DWORD            waitAddress;
    DWORD            compareAddress;
} LlosThread;

extern "C" void LLILUM_main(void);
extern LlosThread* GetThreadLocalStorage(void);

extern HANDLE g_globalMutex;
extern DWORD  g_dwTlsIndex;
