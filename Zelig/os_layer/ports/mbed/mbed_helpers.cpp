#include "mbed_helpers.h"

//--//

extern "C"
{
    void InternalZeroMemory(void* ptr, int32_t size)
    {
        int32_t i;
        uint8_t *pb = (uint8_t*)ptr;

        if (pb != NULL)
        {
            for (i = 0; i < size; i++)
            {
                *pb++ = 0;
            }
        }
    }
}