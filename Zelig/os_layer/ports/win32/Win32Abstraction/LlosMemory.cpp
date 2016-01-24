#include "LlosWin32.h"
#include <llos_memory.h>

HRESULT LLOS_MEMORY_GetMaxHeapSize(uint32_t* pMaxHeapSize)
{
    if (pMaxHeapSize == nullptr)
    {
        return E_INVALIDARG;
    }

    *pMaxHeapSize = 2 * 1024 * 1024;

    return S_OK;
}

HRESULT LLOS_MEMORY_Allocate(uint32_t size, uint8_t fill, LLOS_Opaque* pAllocation)
{
    VOID *ptr = nullptr;
    
    if (pAllocation == nullptr)
    {
        return E_INVALIDARG;
    }

    *pAllocation = nullptr;

    ptr = malloc(size);
    if (ptr != nullptr)
    {
        memset(ptr, fill, size);
        *pAllocation = ptr;
    }

    return *pAllocation != NULL ? S_OK : E_FAIL;
}


HRESULT LLOS_MEMORY_Reallocate(LLOS_Opaque* allocation, uint32_t newSize)
{
    return E_NOTIMPL;
}


VOID LLOS_MEMORY_Free(LLOS_Opaque address)
{
    free(address);
}


