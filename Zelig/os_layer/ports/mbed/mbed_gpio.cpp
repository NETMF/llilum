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
        gpio_t                      pin;
        gpio_irq_t                  irq;
        LLOS_GPIO_InterruptCallback callback;
        LLOS_Context                context;
    } LLOS_MbedGpio;

    HRESULT LLOS_GPIO_AllocatePin(uint32_t pin_number, LLOS_Context* pin)
    {
        LLOS_MbedGpio *pgpio = (LLOS_MbedGpio*)calloc(sizeof(LLOS_MbedGpio), 1);

        if (pgpio == NULL)
        {
            return LLOS_E_OUT_OF_MEMORY;
        }

        gpio_init( &pgpio->pin, (PinName)pin_number);
        pgpio->callback = NULL;
        pgpio->context  = NULL;

        *pin = (LLOS_Context)pgpio;

        return S_OK;
    }

    void LLOS_GPIO_FreePin(LLOS_Context pin)
    {
        LLOS_MbedGpio *pgpio = (LLOS_MbedGpio*)pin;

        if (pgpio != NULL)
        {
            gpio_irq_disable(&pgpio->irq);
            gpio_irq_free(&pgpio->irq);
            free(pin);
        }
    }

    static void HandleGpioInterrupt(uint32_t id, gpio_irq_event evt)
    {
        LLOS_MbedGpio   *pgpio = (LLOS_MbedGpio*)id;
        LLOS_GPIO_Edge edge = evt == IRQ_RISE ? LLOS_GPIO_EdgeRising : LLOS_GPIO_EdgeFalling;

        pgpio->callback((LLOS_Context)&pgpio->pin, pgpio->context, edge);
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

        pgpio->callback = callback;
        pgpio->context = callback_context;

        if (0 != gpio_irq_init(&pgpio->irq, pgpio->pin.pin, (gpio_irq_handler)HandleGpioInterrupt, (uint32_t)pgpio))
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

        gpio_irq_set(&pgpio->irq, IRQ_RISE, edgeRiseEnable);
        gpio_irq_set(&pgpio->irq, IRQ_FALL, edgeFallEnable);
        gpio_irq_enable(&pgpio->irq);

        return S_OK;
    }

    HRESULT LLOS_GPIO_DisablePin(LLOS_Context pin)
    {
        LLOS_MbedGpio *pgpio = (LLOS_MbedGpio*)pin;

        if (pgpio != NULL)
        {
            gpio_irq_disable( &pgpio->irq );
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

        gpio_mode(&pgpio->pin, mode);

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
            gpio_dir(&pgpio->pin, PIN_OUTPUT);
        }
        else
        {
            gpio_dir(&pgpio->pin, PIN_INPUT);
        }

        return S_OK;
    }

    HRESULT LLOS_GPIO_SetDebounce(LLOS_Context pin, LLOS_Timespan debounce_time)
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

        gpio_write( &pgpio->pin, value );

        return S_OK;
    }

    int32_t LLOS_GPIO_Read(LLOS_Context pin)
    {
        LLOS_MbedGpio *pgpio = (LLOS_MbedGpio*)pin;

        if (pgpio == NULL)
        {
            return -1;
        }

        return gpio_read( &pgpio->pin );
    }

    HRESULT LLOS_GPIO_GetConfig(LLOS_Context pin, uint32_t* pin_number, LLOS_GPIO_Edge* edge, LLOS_GPIO_Resistor* resistor, LLOS_GPIO_Polarity* polarity)
    {
        return LLOS_E_NOT_SUPPORTED;
    }
}
