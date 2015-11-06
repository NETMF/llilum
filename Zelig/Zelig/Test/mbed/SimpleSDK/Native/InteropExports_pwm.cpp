#include "helpers.h"

extern "C"
{
	void tmp_pwm_alloc_init(pwmout_t** obj, int pinNumber)
	{
		*obj = (pwmout_t*)calloc(sizeof(pwmout_t), 1);
		pwmout_init(*obj, (PinName)pinNumber);
	}

	void tmp_pwm_free(pwmout_t* obj)
	{
		pwmout_free(obj);
	}

	void tmp_pwm_dutycycle(pwmout_t* obj, float ratio)
	{
		pwmout_write(obj, ratio);
	}

	void tmp_pwm_period_us(pwmout_t* obj, int32_t uSeconds)
	{
		pwmout_period_us(obj, uSeconds);
	}

	void tmp_pwm_pulsewidth_us(pwmout_t* obj, int32_t uSeconds)
	{
		pwmout_pulsewidth_us(obj, uSeconds);
	}
}
