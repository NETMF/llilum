#include "mbed.h"
#include "gpio_api.h"
#include <stdlib.h>
#include <stdio.h>

extern "C" {
void tmp_gpio_write(gpio_t *obj, int value)
{
	gpio_write(obj, value);
}

int tmp_gpio_read(gpio_t *obj)
{
	return gpio_read(obj);
}

void tmp_gpio_alloc(gpio_t **obj)
{
	*obj = (gpio_t*)calloc(sizeof(gpio_t), 1);
}

unsigned char *callocWrapper(unsigned num, unsigned size)
{
	return (unsigned char *)calloc(num, size);
}

void SetPriMaskRegister(unsigned primask)
{

}

unsigned GetPriMaskRegister()
{
	return 0;
}

void SetFaultMaskRegister(unsigned primask)
{

}

void mbedPrint(const char *str)
{
	printf("%s", str);
}

unsigned GetFaultMaskRegister()
{
	return 0;
}
}