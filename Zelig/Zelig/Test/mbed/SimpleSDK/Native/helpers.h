//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Platform dependent configurations
//
#ifdef TARGET_LPC1768
#include "LPC17xx.h"
#elif TARGET_K64F
#include "MK64F12.h"
#elif TARGET_STM32L152RE
#include "stm32l1xx.h"
#else
#error Undefined plaform
#endif

#include "analogin_api.h"
#include "gpio_api.h"
#include "gpio_irq_api.h"
#include "spi_api.h"
#include "i2c_api.h"
#include "core_cmFunc.h"
#include "us_ticker_api.h"
#include "serial_api.h"
#include <stdint.h>
#include <stdlib.h>


