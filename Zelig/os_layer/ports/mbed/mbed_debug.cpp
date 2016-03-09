//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#include "mbed_helpers.h" 
#include <stdio.h>

//--//

extern "C"
{

    int GetANumber()
    {
        //
        // we will get max a 24 bit numer, never smaller than 42
        //
        const int32_t max = (1 << 24) - 1;

        return (rand() % max) + 42;
    }

    void Nop()
    {
        asm("nop");
    }


    void mbedPrint(const char *str)
    {
        printf("%s", str);
    }

    //
    // Debugging from BugCheck 
    //

#define MAXLOGSTRINGSIZE 256

    void ConvertToCharString(char* output, const uint16_t* input, const uint32_t length)
    {
        for (unsigned i = 0; i < length; i++)
        {
            uint16_t ch = input[i];
            output[i] = (ch > 0xFF) ? '?' : (char)ch;
        }
    }

    void DebugLogPrint(char* message)
    {
        LLOS__UNREFERENCED_PARAMETER(message);

        // To automatically dump out the debug log into gdb output, input the following commands
        // to set a breakpoint as such:
        /*

            br DebugLogPrint
            commands
            silent
            printf "DebugLog: %s\n", message
            cont
            end

        */
        asm("nop");
    }

    void DebugLog0(uint16_t* message, uint32_t length)
    {
        char buffer[MAXLOGSTRINGSIZE];
        if (length < MAXLOGSTRINGSIZE)
        {
            ConvertToCharString(buffer, message, length);
            buffer[length] = '\0';

            DebugLogPrint(buffer);
        }
        else
        {
            DebugLogPrint((char*)"ERROR: MAXLOGSTRINGSIZE exceeded");
        }
    }

    void DebugLog1(uint16_t* message, uint32_t length, int32_t p1)
    {
        char buffer[MAXLOGSTRINGSIZE];
        if (length < MAXLOGSTRINGSIZE)
        {
            ConvertToCharString(buffer, message, length);
            buffer[length] = '\0';
            {
                char buffer2[MAXLOGSTRINGSIZE];
                snprintf(buffer2, MAXLOGSTRINGSIZE, buffer, p1);
                DebugLogPrint(buffer2);
            }
        }
        else
        {
            DebugLogPrint((char*)"ERROR: MAXLOGSTRINGSIZE exceeded");
        }
    }

    void DebugLog2(uint16_t* message, uint32_t length, int32_t p1, int32_t p2)
    {
        char buffer[MAXLOGSTRINGSIZE];
        if (length < MAXLOGSTRINGSIZE)
        {
            ConvertToCharString(buffer, message, length);
            buffer[length] = '\0';
            {
                char buffer2[MAXLOGSTRINGSIZE];
                snprintf(buffer2, MAXLOGSTRINGSIZE, buffer, p1, p2);
                DebugLogPrint(buffer2);
            }
        }
        else
        {
            DebugLogPrint((char*)"ERROR: MAXLOGSTRINGSIZE exceeded");
        }
    }

    void DebugLog3(uint16_t* message, uint32_t length, int32_t p1, int32_t p2, int32_t p3)
    {
        char buffer[MAXLOGSTRINGSIZE];
        if (length < MAXLOGSTRINGSIZE)
        {
            ConvertToCharString(buffer, message, length);
            buffer[length] = '\0';
            {
                char buffer2[MAXLOGSTRINGSIZE];
                snprintf(buffer2, MAXLOGSTRINGSIZE, buffer, p1, p2, p3);
                DebugLogPrint(buffer2);
            }
        }
        else
        {
            DebugLogPrint((char*)"ERROR: MAXLOGSTRINGSIZE exceeded");
        }
    }

    void DebugLog4(uint16_t* message, uint32_t length, int32_t p1, int32_t p2, int32_t p3, int32_t p4)
    {
        char buffer[MAXLOGSTRINGSIZE];
        if (length < MAXLOGSTRINGSIZE)
        {
            ConvertToCharString(buffer, message, length);
            buffer[length] = '\0';
            {
                char buffer2[MAXLOGSTRINGSIZE];
                snprintf(buffer2, MAXLOGSTRINGSIZE, buffer, p1, p2, p3, p4);
                DebugLogPrint(buffer2);
            }
        }
        else
        {
            DebugLogPrint((char*)"ERROR: MAXLOGSTRINGSIZE exceeded");
        }
    }

    void DebugLog5(uint16_t* message, uint32_t length, int32_t p1, int32_t p2, int32_t p3, int32_t p4, int32_t p5)
    {
        char buffer[MAXLOGSTRINGSIZE];
        if (length < MAXLOGSTRINGSIZE)
        {
            ConvertToCharString(buffer, message, length);
            buffer[length] = '\0';
            {
                char buffer2[MAXLOGSTRINGSIZE];
                snprintf(buffer2, MAXLOGSTRINGSIZE, buffer, p1, p2, p3, p4, p5);
                DebugLogPrint(buffer2);
            }
        }
        else
        {
            DebugLogPrint((char*)"ERROR: MAXLOGSTRINGSIZE exceeded");
        }
    }

    //
    // Faults and Diagnostic
    //

    uint32_t CUSTOM_STUB_DebuggerConnected()
    {
#if __CORTEX_M0
        return false;
#else
        return (CoreDebug->DHCSR & 0x00000001);
#endif
    }

    uint32_t CUSTOM_STUB_GetProgramCounter()
    {
        return 0;
    }

    uint32_t CUSTOM_STUB_SCB__get_CFSR()
    {
        return *((uint32_t volatile *)0xE000ED28);
    }

    uint32_t CUSTOM_STUB_SCB__get_HFSR()
    {
        return *((uint32_t volatile *)0xE000ED2C);
    }

    uint32_t CUSTOM_STUB_SCB__get_MMFAR()
    {
        return *((uint32_t volatile *)0xE000ED34);
    }

    uint32_t CUSTOM_STUB_SCB__get_BFAR()
    {
        return *((uint32_t volatile *)0xE000ED38);
    }

    void BreakWithTrap()
    {
        // this will likely generate a hard fault
        __builtin_trap();
    }

    void Break()
    {
        if (CUSTOM_STUB_DebuggerConnected())
        {
            asm("bkpt");
        }
        else
        {
            while (1)
            {
                __WFE();
            }
        }
    }

    // placing this outside the BreakPoint() function to avoid compiler unused var warning
    volatile uint32_t valueToWatch;
    void Breakpoint(unsigned n)
    {
        valueToWatch = n;

        Break();
    }

    //
    // Stubs for Faults
    //

    extern void Zelig_Exception_MemManage_Handler ();
    extern void Zelig_Exception_BusFault_Handler  ();
    extern void Zelig_Exception_UsageFault_Handler();
    extern void Zelig_Exception_HardFault_Handler ();

    //--//

#if __CORTEX_M0
    
#define DEFAULT_FAULT_HANDLER(handler)      \
    //////__ASM volatile ("MOVS    R0, #0x4");    \
    //////__ASM volatile ("MOV     R1,   LR");    \
    //////__ASM volatile ("TST     R0,   R1");    \
    //////__ASM volatile ("BEQ     __FROM_MSP");  \
    //////__ASM volatile ("__FROM_PSP:");         \
    __ASM volatile ("MRS     R0, psp");     \
    //////__ASM volatile ("__FROM_MSP:");         \
    //////__ASM volatile ("MRS     R0, msp");     \
    handler();                              \
    __ASM volatile ("BX     LR");           \

#else

#define DEFAULT_FAULT_HANDLER(handler)      \
    __ASM volatile ("TST    LR, #0x4");     \
    __ASM volatile ("ITE    EQ");           \
    __ASM volatile ("MRSEQ  R0, msp");      \
    __ASM volatile ("MRSNE  R0, psp");      \
    handler();                              \
    __ASM volatile ("BX     LR");           \

#endif

    __attribute__((naked)) void HardFault_Handler(void)
    {
        DEFAULT_FAULT_HANDLER( Zelig_Exception_HardFault_Handler );
    }

    __attribute__((naked)) void MemManage_Handler(void)
    {
        DEFAULT_FAULT_HANDLER( Zelig_Exception_MemManage_Handler );
    }

    __attribute__((naked)) void BusFault_Handler(void)
    {
        DEFAULT_FAULT_HANDLER( Zelig_Exception_BusFault_Handler );
    }

    __attribute__((naked)) void UsageFault_Handler(void)
    {
        DEFAULT_FAULT_HANDLER( Zelig_Exception_UsageFault_Handler );
    }
}
