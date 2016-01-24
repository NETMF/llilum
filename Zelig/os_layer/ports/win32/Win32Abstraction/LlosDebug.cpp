#include "LlosWin32.h"
#include <llos_debug.h>

VOID LLOS_DEBUG_Break(uint32_t code)
{
    DebugBreak();
}

VOID LLOS_DEBUG_LogText(wchar_t* text, int32_t textLength)
{
    wprintf(text);
    wprintf(L"\r\n");
}
