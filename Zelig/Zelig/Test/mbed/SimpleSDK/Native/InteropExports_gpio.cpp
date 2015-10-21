//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#include "helpers.h" 

//--//

extern "C"
{

	void tmp_gpio_write(gpio_t *obj, int32_t value)
	{
		gpio_write(obj, value);
	}

	int32_t tmp_gpio_read(gpio_t *obj)
	{
		return gpio_read(obj);
	}

	void tmp_gpio_alloc_init(gpio_t **obj, int32_t pinNumber)
	{
		*obj = (gpio_t*)calloc(sizeof(gpio_t), 1);
		gpio_init_in(*obj, (PinName)pinNumber);
	}

	void tmp_gpio_free(gpio_t *obj)
	{
		free(obj);
	}

	void tmp_gpio_dir(gpio_t *obj, int32_t dir)
	{
		gpio_dir(obj, (PinDirection)dir);
	}

	void tmp_gpio_mode(gpio_t *obj, int32_t mode)
	{
		gpio_mode(obj, (PinMode)mode);
	}
}
