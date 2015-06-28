#include "gpio_api.h"
#include <stdlib.h>

void tmp_gpio_write(gpio_t *obj, int value)
{
  gpio_write(obj,value);
}

int tmp_gpio_read(gpio_t *obj)
{
  return gpio_read(obj);
}

void tmp_gpio_alloc(gpio_t **obj)
{
  *obj=calloc(sizeof(gpio_t),1);
}