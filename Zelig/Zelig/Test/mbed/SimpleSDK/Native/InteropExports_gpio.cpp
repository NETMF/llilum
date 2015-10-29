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

	void tmp_gpio_irq_alloc(gpio_irq_t **obj)
	{
		*obj = (gpio_irq_t*)calloc(sizeof(gpio_irq_t), 1);
	}

	void tmp_gpio_irq_free(gpio_irq_t *obj)
	{
		free(obj);
	}

	extern void HandleGpioInterrupt(uint32_t id, uint32_t data);

	int tmp_gpio_irq_init(gpio_irq_t *obj, int32_t pin, uint32_t id)
	{
		return gpio_irq_init(obj, ( PinName )pin, ( gpio_irq_handler )HandleGpioInterrupt, id);
	}

	void tmp_gpio_irq_uninit(gpio_irq_t *obj)
	{
		gpio_irq_free(obj);
	}

	void tmp_gpio_irq_set(gpio_irq_t *obj, int32_t edge, uint32_t enable)
	{
        gpio_irq_set(obj, (gpio_irq_event)edge, enable);
    }

	void tmp_gpio_irq_enable(gpio_irq_t *obj)
	{
		gpio_irq_enable(obj);
	}

	void tmp_gpio_irq_disable(gpio_irq_t *obj)
	{
		gpio_irq_disable(obj);
	}
}
