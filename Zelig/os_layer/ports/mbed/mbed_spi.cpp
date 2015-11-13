//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#include "mbed_helpers.h" 

//--//

extern "C"
{

	int tmp_spi_master_write(spi_t* obj, int32_t value)
	{
		return spi_master_write(obj, value);
	}

	void tmp_spi_format(spi_t* obj, int32_t bits, int32_t mode, int32_t slave)
	{
		spi_format(obj, bits, mode, slave);
	}

	void tmp_spi_frequency(spi_t* obj, int32_t hz)
	{
		spi_frequency(obj, hz);
	}

	void tmp_spi_alloc_init(spi_t** obj, int32_t mosi, int32_t miso, int32_t scl, int32_t cs)
	{
		*obj = (spi_t*)calloc(sizeof(spi_t), 1);
		spi_init(*obj, (PinName)mosi, (PinName)miso, (PinName)scl, (PinName)cs);
	}

	void tmp_spi_free(spi_t* obj)
	{
		free(obj);
	}

	int tmp_spi_busy(spi_t* obj)
	{
#ifndef TARGET_K64F
		return spi_busy(obj);
#endif
        return 0;
	}
}
