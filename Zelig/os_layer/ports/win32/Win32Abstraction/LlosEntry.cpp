#include "LlosWin32.h"
#include <llos_mutex.h>

DWORD g_dwTlsIndex;

int LlosWin32_Main()
{
    g_dwTlsIndex = TlsAlloc();

    // Thread local storage for refcount on global mutex
    LlosThread** ppvData = (LlosThread**)LocalAlloc(LPTR, sizeof(LlosThread) + sizeof(LlosThread*));
    *ppvData = (LlosThread*)&ppvData[1];
    TlsSetValue(g_dwTlsIndex, ppvData);

    LLILUM_main();

    LocalFree(ppvData);

    TlsFree(g_dwTlsIndex);

    return 0;
}
