//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#include "gpio_api.h"
#ifdef TARGET_LPC1768
#include "core_cm3.h"
#elif TARGET_K64F
#include "core_cm4.h"
#else
#error Undefined plaform
#endif
#include "spi_api.h"
#include "i2c_api.h"
#include "core_cmFunc.h"
#include <stdint.h>
#include <stdlib.h>
#include "us_ticker_api.h"


//
// Platform dependent configurations
//
#ifdef TARGET_LPC1768
#include "system_LPC17xx.h"
#elif TARGET_K64F
#include "system_MK64F12.h"
#else
#error Undefined plaform
#endif
