//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#include "helpers.h" 

//--//

extern "C"
{

	unsigned char *callocWrapper(uint32_t num, uint32_t size)
	{
		return (unsigned char *)calloc(num, size);
	}
}

