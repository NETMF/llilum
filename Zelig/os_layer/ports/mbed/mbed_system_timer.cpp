//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#include "mbed_helpers.h" 

//--//

extern "C"
{

	extern void SystemTimer_Handler(uint32_t id);

	void tmp_timer_event_alloc(ticker_event_t** obj)
	{
		*obj = (ticker_event_t*)calloc(sizeof(ticker_event_t), 1);
	}

	uint32_t tmp_sys_timer_read()
	{
		return us_ticker_read();
	}

	// This is used to call back into the Kernel using a WellKnownMethod
	void tmp_interrupt_handler(uint32_t id)
	{
		SystemTimer_Handler(id);
	}

	void tmp_sys_timer_init()
	{
		us_ticker_init();

		ticker_set_handler(get_us_ticker_data(), tmp_interrupt_handler);
	}

	void tmp_sys_timer_insert_event(ticker_event_t* tick_event, uint32_t relativeTimeout)
	{
		// We can leave the id as 0 since it is only used for timer event identification
		// And we keep track of that ourselves

		ticker_insert_event(get_us_ticker_data(), tick_event, us_ticker_read() + relativeTimeout, 0);
	}

	void tmp_sys_timer_remove_event(ticker_event_t* tick_event)
	{
		// We can leave the id as 0 since it is only used for timer event identification
		// And we keep track of that ourselves

		ticker_remove_event(get_us_ticker_data(), tick_event);
	}
}
