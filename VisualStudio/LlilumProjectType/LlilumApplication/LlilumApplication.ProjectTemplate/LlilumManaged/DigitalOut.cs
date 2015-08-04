namespace $safeprojectname$
{
    public class DigitalOut
    {
        unsafe public DigitalOut(PinName pin)
        {
            fixed (GPIOimpl** gpio_ptr = &gpio)
            {
                GPIO.tmp_gpio_alloc(gpio_ptr);
            }
           
            GPIO.gpio_init_out(gpio, pin);
        }

        unsafe public DigitalOut(PinName pin, int value)
        {
            GPIO.gpio_init_out_ex(gpio, pin, value);
        }

        unsafe public void write(int value)
        {
            GPIO.tmp_gpio_write(gpio, value);
        }
        unsafe public int read()
        {
            return GPIO.tmp_gpio_read(gpio);
        }

        public int value
        {
            get { return read(); }
            set { write(value); }
        }

        private unsafe GPIOimpl* gpio;
    }
}
