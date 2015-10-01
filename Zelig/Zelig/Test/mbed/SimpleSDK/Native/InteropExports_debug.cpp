//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#include "helpers.h" 

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

	void BreakWithTrap()
	{
		// this will likely generate a hard fault
		__builtin_trap();
	}

	void Break()
	{
		asm("bkpt");
	}

	void Breakpoint(unsigned n)
	{
		volatile uint32_t valueToWatch = n;

		Break();
	}

	void Nop()
	{
		asm("nop");
	}

#include <stdio.h>

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



	void HardFault_Handler(void)
	{
		if (CoreDebug->DHCSR & 1)
		{
			// check C_DEBUGEN == 1 -> Debugger Connected    
			uint32_t code = *(uint32_t*)0xE000ED2C;
			if (code & 0x40000000)
			{

				Breakpoint(42);
			}
			else
			{
				Breakpoint(code);
			}
			// halt program execution here   
		}

		// enter endless loop otherwise 
		while (1);
	}
}
