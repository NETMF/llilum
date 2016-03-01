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
#elif TARGET_STM32F411RE
#include "stm32f4xx.h"
#elif TARGET_STM32F401RE
#include "stm32f4xx.h"
#elif TARGET_STM32F091RC
#include "stm32f0xx.h"
#else
#error Undefined plaform
#endif

#include "analogin_api.h"
#include "analogout_api.h"
#include "gpio_api.h"
#include "gpio_irq_api.h"
#include "spi_api.h"
#include "i2c_api.h"
#include "core_cmFunc.h"
#include "us_ticker_api.h"
#include "serial_api.h"
#include "pwmout_api.h"
#include <stdint.h>
#include <stdlib.h>

//--//

#ifndef LLOS__UNREFERENCED_PARAMETER
#define LLOS__UNREFERENCED_PARAMETER(P) ((void)P)
#endif

//--//


#define LLOS__PRESERVE_PRIMASK_STATE__SAVE()        \
    { uint32_t __primask = __get_PRIMASK(); {       \

#define LLOS__PRESERVE_PRIMASK_STATE__RESTORE()     \
    } __set_PRIMASK(__primask); }                   \

//--//

#if (__CORTEX_M >= 0x03) || (__CORTEX_SC >= 300)
#define LLOS__PRESERVE_PRIMASK_STATE_M0_1__SAVE       __NOP
#else
#define LLOS__PRESERVE_PRIMASK_STATE_M0_1__SAVE       LLOS__PRESERVE_PRIMASK_STATE__SAVE
#endif

#if (__CORTEX_M >= 0x03) || (__CORTEX_SC >= 300)
#define LLOS__PRESERVE_PRIMASK_STATE_M0_1__RESTORE    __NOP 
#else
#define LLOS__PRESERVE_PRIMASK_STATE_M0_1__RESTORE    LLOS__PRESERVE_PRIMASK_STATE__RESTORE
#endif

//--//

#define LLOS__PRESERVE_PRIMASK_STATE__FUNC(f)   \
    LLOS__PRESERVE_PRIMASK_STATE__SAVE();       \
    f;                                          \
    LLOS__PRESERVE_PRIMASK_STATE__RESTORE();    \

//--//

int32_t WStringToCharBuffer(char* output, uint32_t outputBufferLength, const uint16_t* input, const uint32_t length);

//--//

extern uint32_t* __StackTop;
extern uint32_t* __StackLimit;
