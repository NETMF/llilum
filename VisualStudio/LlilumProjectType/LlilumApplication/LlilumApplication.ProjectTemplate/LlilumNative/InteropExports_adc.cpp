#include "helpers.h"

//--//

extern "C"
{
	void tmp_adc_alloc_init(analogin_t** obj, int pinNumber)
	{
		*obj = (analogin_t*)calloc(sizeof(analogin_t), 1);
		analogin_init(*obj, (PinName)pinNumber);
	}

	uint32_t tmp_adc_read_u16(analogin_t* obj)
	{
		return (uint32_t)analogin_read_u16(obj);
	}

	float tmp_adc_read_float(analogin_t* obj)
	{
		return analogin_read(obj);
	}

	void tmp_adc_free(analogin_t* obj)
	{
		free(obj);
	}
}