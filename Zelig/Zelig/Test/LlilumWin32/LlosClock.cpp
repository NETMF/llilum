#include "stdafx.h"
#include <llos_clock.h>

uint64_t LLOS_CLOCK_GetClockTicks()
{
    uint64_t time = GetTickCount64();
    return time;
}

uint64_t LLOS_CLOCK_GetPerformanceCounter()
{
    return GetTickCount64();
}

uint64_t LLOS_CLOCK_GetPerformanceCounterFrequency()
{
    return 1000;
}
