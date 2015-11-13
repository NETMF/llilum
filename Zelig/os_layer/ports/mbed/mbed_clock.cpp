//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#include "mbed_helpers.h" 

//--//

extern "C"
{
	/*__STATIC_INLINE*/ uint32_t CMSIS_STUB_CLOCK__GetSystemCoreClock()
	{
		return SystemCoreClock;
	}
}

