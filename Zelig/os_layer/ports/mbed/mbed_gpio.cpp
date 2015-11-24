//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#include "mbed_helpers.h" 
#include "llos_gpio.h"

//--//

extern "C"
{
    typedef struct LLOS_MbedGpio
    {
        gpio_t                      Pin;
        gpio_irq_t                  Irq;
        LLOS_GPIO_InterruptCallback Callback;
        LLOS_Context                Context;
    } LLOS_MbedGpio;

    HRESULT LLOS_GPIO_AllocatePin(uint32_t pin_number, LLOS_Context* pin)
    {
        LLOS_MbedGpio *pgpio = (LLOS_MbedGpio*)calloc(sizeof(LLOS_MbedGpio), 1);

        if (pgpio == NULL)
        {
            return LLOS_E_OUT_OF_MEMORY;
        }

        gpio_init( &pgpio->Pin, (PinName)pin_number);
        pgpio->Callback = NULL;
        pgpio->Context  = NULL;

        *pin = (LLOS_Context)pgpio;

        return S_OK;
    }

    void LLOS_GPIO_FreePin(LLOS_Context pin)
    {
        LLOS_MbedGpio *pgpio = (LLOS_MbedGpio*)pin;

        if (pgpio != NULL)
        {
            gpio_irq_disable(&pgpio->Irq);
            gpio_irq_free(&pgpio->Irq);
            free(pin);
        }
    }

    static void HandleGpioInterrupt(uint32_t id, gpio_irq_event evt)
    {
        LLOS_MbedGpio   *pgpio = (LLOS_MbedGpio*)id;
        LLOS_GPIO_Edge edge = evt == IRQ_RISE ? LLOS_GPIO_EdgeRising : LLOS_GPIO_EdgeFalling;

        pgpio->Callback((LLOS_Context)&pgpio->Pin, pgpio->Context, edge);
    }

    HRESULT LLOS_GPIO_EnablePin(LLOS_Context pin, LLOS_GPIO_Edge edge, LLOS_GPIO_InterruptCallback callback, LLOS_Context callback_context)
    {
        LLOS_MbedGpio *pgpio = (LLOS_MbedGpio*)pin;
        int edgeRiseEnable = 0;
        int edgeFallEnable = 0;

        if (pgpio == NULL || callback == NULL || edge == LLOS_GPIO_EdgeNone)
        {
            return LLOS_E_INVALID_PARAMETER;
        }

        pgpio->Callback = callback;
        pgpio->Context = callback_context;

        if (0 != gpio_irq_init(&pgpio->Irq, pgpio->Pin.pin, (gpio_irq_handler)HandleGpioInterrupt, (uint32_t)pgpio))
        {
            return LLOS_E_PIN_UNAVAILABLE;
        }

        switch (edge)
        {
        case LLOS_GPIO_EdgeBoth:
            edgeRiseEnable = 1;
            edgeFallEnable = 1;
            break;
        case LLOS_GPIO_EdgeFalling:
            edgeFallEnable = 1;
            break;
        case LLOS_GPIO_EdgeRising:
            edgeRiseEnable = 1;
            break;
        default:
            return LLOS_E_NOT_SUPPORTED;
        }

        gpio_irq_set(&pgpio->Irq, IRQ_RISE, edgeRiseEnable);
        gpio_irq_set(&pgpio->Irq, IRQ_FALL, edgeFallEnable);
        gpio_irq_enable(&pgpio->Irq);

        return S_OK;
    }

    HRESULT LLOS_GPIO_DisablePin(LLOS_Context pin)
    {
        LLOS_MbedGpio *pgpio = (LLOS_MbedGpio*)pin;

        if (pgpio != NULL)
        {
            gpio_irq_disable( &pgpio->Irq );
        }

        return S_OK;
    }

    HRESULT LLOS_GPIO_SetPolarity(LLOS_Context pin, LLOS_GPIO_Polarity polarity)
    {
        return LLOS_E_NOT_SUPPORTED;
    }

    HRESULT LLOS_GPIO_SetMode(LLOS_Context pin, LLOS_GPIO_Resistor resistor)
    {
        LLOS_MbedGpio *pgpio = (LLOS_MbedGpio*)pin;
        PinMode mode;

        switch (resistor)
        {
        case LLOS_GPIO_ResistorDefault:
            mode = PullDefault;
            break;
        case LLOS_GPIO_ResistorPullNone:
            mode = PullNone;
            break;
        case LLOS_GPIO_ResistorPullup:
            mode = PullUp;
            break;
        case LLOS_GPIO_ResistorPulldown:
            mode = PullDown;
            break;
        default:
            return LLOS_E_NOT_SUPPORTED;
        }

        gpio_mode(&pgpio->Pin, mode);

        return S_OK;
    }

    HRESULT LLOS_GPIO_SetDirection(LLOS_Context pin, LLOS_GPIO_Direction direction)
    {
        LLOS_MbedGpio *pgpio = (LLOS_MbedGpio*)pin;

        if (pgpio == NULL)
        {
            return LLOS_E_INVALID_PARAMETER;
        }

        if (direction == LLOS_GPIO_Output)
        {
            gpio_dir(&pgpio->Pin, PIN_OUTPUT);
        }
        else
        {
            gpio_dir(&pgpio->Pin, PIN_INPUT);
        }

        return S_OK;
    }

    HRESULT LLOS_GPIO_SetDebounce(LLOS_Context pin, LLOS_TimeSpan debounceTime)
    {
        return LLOS_E_NOT_SUPPORTED;
    }

    HRESULT LLOS_GPIO_Write(LLOS_Context pin, int32_t value)
    {
        LLOS_MbedGpio *pgpio = (LLOS_MbedGpio*)pin;

        if (pgpio == NULL)
        {
            return LLOS_E_INVALID_PARAMETER;
        }

        gpio_write( &pgpio->Pin, value );

        return S_OK;
    }

    int32_t LLOS_GPIO_Read(LLOS_Context pin)
    {
        LLOS_MbedGpio *pgpio = (LLOS_MbedGpio*)pin;

        if (pgpio == NULL)
        {
            return -1;
        }

        return gpio_read( &pgpio->Pin );
    }

    HRESULT LLOS_GPIO_GetConfig(LLOS_Context pin, uint32_t* pin_number, LLOS_GPIO_Edge* edge, LLOS_GPIO_Resistor* resistor, LLOS_GPIO_Polarity* polarity)
    {
        return LLOS_E_NOT_SUPPORTED;
    }
}
