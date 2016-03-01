//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#include "mbed_helpers.h" 

//--//

extern "C"
{
    __attribute__((section(".managed_exception_thread")))
    uint32_t stackAdjustForExceptionThread[4];

    uint32_t LLOS_THREAD_GetMainStackAddress( )
    {
        uint32_t addr = (uint32_t)&__StackLimit;

        return addr;
    }

    uint32_t LLOS_THREAD_GetMainStackSize( )
    {
        uint32_t top   = (uint32_t)&__StackTop;
        uint32_t limit = (uint32_t)&__StackLimit;

        return top - limit;
    }

	//
	// Threading 
    // TODO TODO TODO: Eliminate this portion or convert the Win32 port over
	//

	void* CreateNativeContext(void* entryPoint, void* stack, int32_t stackSize)
	{
		return NULL;
	}

	void Yield(void* nativeContext)
	{
	}

	void Retire(void* nativeContext)
	{
	}

	void SwitchToContext(void* nativeContext)
	{
	}

	void* GetPriority(void* nativeContext)
	{
		return 0;
	}

	void SetPriority(void* nativeContext, void* priority)
	{
	}
}