//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#include "mbed_helpers.h" 
#include "llos_spi.h"

//--//

extern "C"
{
    typedef struct LLOS_MbedSpi
    {
        spi_t                     spi;
        LLOS_SPI_Callback         callback;
        LLOS_Context              context;
        LLOS_SPI_ControllerConfig config;
    } LLOS_MbedSpi;

    static void InternalZeroMemory(void* ptr, int32_t size)
    {
        int32_t i;
        uint8_t *pb = (uint8_t*)ptr;

        if (pb != NULL)
        {
            for (i = 0; i < size; i++)
            {
                *pb++ = 0;
            }
        }
    }

    HRESULT LLOS_SPI_Initialize(uint32_t mosi, uint32_t miso, uint32_t sclk, uint32_t chipSelect, LLOS_Context* ppChannel, LLOS_SPI_ControllerConfig** ppConfiguration )
    {
        LLOS_MbedSpi *pCtx;

        if (ppChannel == NULL)
        {
            return LLOS_E_INVALID_PARAMETER;
        }

        pCtx = (LLOS_MbedSpi*)calloc(sizeof(LLOS_MbedSpi), 1);

        if (pCtx == NULL)
        {
            return LLOS_E_OUT_OF_MEMORY;
        }

        InternalZeroMemory(pCtx, sizeof(*pCtx));

        spi_init(&pCtx->spi, (PinName)mosi, (PinName)miso, (PinName)sclk, (PinName)chipSelect);
        pCtx->callback = NULL;
        pCtx->context = NULL;

        *ppChannel = (LLOS_Context)pCtx;
        *ppConfiguration = &pCtx->config;

        return S_OK;
    }

    VOID LLOS_SPI_Uninitialize(LLOS_Context channel)
    {
        free(channel);
    }

    HRESULT LLOS_SPI_Configure(LLOS_Context channel, LLOS_SPI_ControllerConfig* pConfig)
    {
        LLOS_MbedSpi *pCtx = (LLOS_MbedSpi*)channel;
        int32_t mode = 0;

        if (pCtx == NULL || pConfig == NULL)
        {
            return LLOS_E_INVALID_PARAMETER;
        }

        if (pConfig->wordSize != 8 && pConfig->wordSize != 16)
        {
            return LLOS_E_INVALID_OPERATION;
        }

        if (pConfig->phaseMode == 1)
        {
            mode |= 1<<0;
        }

        if (pConfig->inversePolarity)
        {
            mode |= 1<<1;
        }

        pCtx->config = *pConfig;

        spi_format(&pCtx->spi, pConfig->wordSize, mode, pConfig->master ? 0 : 1);

        if (pConfig->clockRateHz != 0)
        {
            spi_frequency(&pCtx->spi, pConfig->clockRateHz);
        }

        return S_OK;
    }

    HRESULT LLOS_SPI_SetCallback(LLOS_Context channel, LLOS_SPI_Callback request, LLOS_Context context)
    {
        return LLOS_E_NOTIMPL;
    }

    HRESULT LLOS_SPI_SetFrequency(LLOS_Context channel, uint32_t frequencyHz)
    {
        LLOS_MbedSpi *pCtx = (LLOS_MbedSpi*)channel;

        if (pCtx == NULL)
        {
            return LLOS_E_INVALID_PARAMETER;
        }
        
        spi_frequency(&pCtx->spi, frequencyHz);

        return S_OK;
    }

    HRESULT LLOS_SPI_Transfer(LLOS_Context channel, uint8_t* txBuffer, int32_t txOffset, int32_t txCount, uint8_t* rxBuffer, int32_t rxOffset, int32_t rxCount, int32_t rxStartOffset)
    {
        LLOS_MbedSpi *pCtx          = (LLOS_MbedSpi*)channel;
        int32_t       wordSizeBytes = (pCtx->config.wordSize >> 3);
        int32_t       ibNextWord    = 0;
        int32_t       ibRx          = 0;

        if (pCtx == NULL || txBuffer == NULL || rxBuffer == NULL)
        {
            return LLOS_E_INVALID_PARAMETER;
        }

        if (!pCtx->config.master)
        {
            return LLOS_E_INVALID_OPERATION;
        }

        while (ibNextWord < txCount || ibRx < rxCount)
        {
            int32_t value = 0;
            int32_t response;
            
            if (ibNextWord < txCount)
            {
                value = (wordSizeBytes == 1) ? (int32_t)txBuffer[ibNextWord + txOffset] : (int32_t)*(uint16_t*)&txBuffer[ibNextWord + txOffset];
            }

            response = spi_master_write(&pCtx->spi, value);

            if (ibNextWord >= rxStartOffset && ibRx < rxCount)
            {
                if (wordSizeBytes == 8)
                {
                    rxBuffer[ibRx + rxOffset] = (uint8_t)response;
                }
                else
                {
                    *((uint16_t*)&rxBuffer[ibRx + rxOffset]) = (uint16_t)response;
                }

                ibRx += wordSizeBytes;
            }

            ibNextWord += wordSizeBytes;
        }

        return S_OK;
    }

    HRESULT LLOS_SPI_Write(LLOS_Context channel, uint8_t* txBuffer, int32_t txOffset, int32_t txCount)
    {
        LLOS_MbedSpi *pCtx          = (LLOS_MbedSpi*)channel;
        int32_t       wordSizeBytes = (pCtx->config.wordSize >> 3);
        int32_t       ibNextWord    = 0;

        if (pCtx == NULL || txBuffer == NULL)
        {
            return LLOS_E_INVALID_PARAMETER;
        }

        while (ibNextWord < txCount)
        {
            int32_t value = (wordSizeBytes == 1) ? (int32_t)txBuffer[ibNextWord + txOffset] : (int32_t)*(uint16_t*)&txBuffer[ibNextWord + txOffset];

            if (pCtx->config.master)
            {
                spi_master_write(&pCtx->spi, value);
            }
            else
            {
                spi_slave_write(&pCtx->spi, value);
            }

            ibNextWord += wordSizeBytes;
        }

        return S_OK;
    }

    HRESULT LLOS_SPI_Read(LLOS_Context channel, uint8_t* rxBuffer, int32_t rxOffset, int32_t rxCount, int32_t rxStartOffset)
    {
        LLOS_MbedSpi *pCtx          = (LLOS_MbedSpi*)channel;
        int32_t       wordSizeBytes = (pCtx->config.wordSize >> 3);
        int32_t       ibNextWord    = 0;
        int32_t       ibRx          = 0;

        if (pCtx == NULL || rxBuffer == NULL || pCtx->config.master)
        {
            return LLOS_E_INVALID_PARAMETER;
        }

        while (ibRx < rxCount)
        {
            int32_t response = spi_slave_read(&pCtx->spi);

            if (ibNextWord >= rxStartOffset)
            {
                if (wordSizeBytes == 1)
                {
                    rxBuffer[ibRx + rxOffset] = (uint8_t)response;
                }
                else
                {
                    *(uint16_t*)&rxBuffer[ibRx + rxOffset] = (uint16_t)response;
                }

                ibRx += wordSizeBytes;
            }

            ibNextWord += wordSizeBytes;
        }

        return S_OK;
    }

    HRESULT LLOS_SPI_IsBusy(LLOS_Context channel, BOOL* isBusy)
    {
        LLOS_MbedSpi *pCtx = (LLOS_MbedSpi*)channel;

        if (pCtx == NULL || isBusy == NULL)
        {
            return LLOS_E_INVALID_PARAMETER;
        }

        *isBusy = spi_busy( &pCtx->spi ) != 0;

        return S_OK;
    }

    HRESULT LLOS_SPI_Suspend(LLOS_Context channel)
    {
        return LLOS_E_NOTIMPL;
    }

    HRESULT LLOS_SPI_Resume(LLOS_Context channel)
    {
        return LLOS_E_NOTIMPL;
    }
}
