#include "mbed_helpers.h"

//--//

extern "C"
{

	void tmp_i2c_alloc(i2c_t** obj)
	{
		*obj = (i2c_t*)calloc(sizeof(i2c_t), 1);
	}

	void tmp_i2c_init(i2c_t* obj, int sdaPin, int sclPin)
	{
		i2c_init(obj, (PinName)sdaPin, (PinName)sclPin);
	}

	void tmp_i2c_free(i2c_t* obj)
	{
		free(obj);
	}

	void tmp_i2c_frequency(i2c_t* obj, int hz)
	{
		i2c_frequency(obj, hz);
	}

	int tmp_i2c_write(i2c_t* obj, int address, char* data, int length, int stop)
	{
		return i2c_write(obj, address, data, length, stop);
	}

	int tmp_i2c_read(i2c_t* obj, int address, char* data, int length, int stop)
	{
		return i2c_read(obj, address, data, length, stop);
	}
}
