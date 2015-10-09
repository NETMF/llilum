#include "helpers.h"

//--//

extern "C"
{
	void tmp_serial_alloc(serial_t** obj)
	{
		*obj = (serial_t*)calloc(sizeof(serial_t), 1);
	}

	void tmp_serial_free(serial_t* obj)
	{
		serial_free(obj);
	}

	void tmp_serial_baud(serial_t* obj, int32_t baudRate)
	{
		serial_baud(obj, baudRate);
	}

	void tmp_serial_format(serial_t* obj, int32_t dataBits, int32_t parity, int32_t stopBits)
	{
		serial_format(obj, dataBits, (SerialParity)parity, stopBits);
	}

	void tmp_serial_putc(serial_t* obj, int32_t c)
	{
		serial_putc(obj, c);
	}

	int32_t tmp_serial_getc(serial_t* obj)
	{
		return serial_getc(obj);
	}

	void tmp_serial_init(serial_t* obj, int32_t txPin, int32_t rxPin)
	{
		serial_init(obj, (PinName)txPin, (PinName)rxPin);
	}

	void tmp_serial_set_flow_control(serial_t* obj, int32_t flowControlType, int32_t rtsPin, int32_t ctsPin)
	{
		serial_set_flow_control(obj, (FlowControl)flowControlType, (PinName)rtsPin, (PinName)ctsPin);
	}

	bool tmp_serial_readable(serial_t* obj)
	{
		return (bool)serial_readable(obj);
	}

	bool tmp_serial_writable(serial_t* obj)
	{
		return (bool)serial_writable(obj);
	}
}
