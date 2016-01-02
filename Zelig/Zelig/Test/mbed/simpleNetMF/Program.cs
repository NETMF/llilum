//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define LPC1768
//#define K64F
//#define STM32F411
#define USE_GPIO
//#define USE_ADC
//#define USE_SPI
//#define USE_I2C
//#define USE_PWM

//
// The following test tests GPIO interrupts by using a set of interruptible pins.
//
//#define TEST_GPIO_INTERRUPTS

namespace Microsoft.Zelig.Test.mbed.Simple
{
    using System;

    using Windows.Devices.Gpio;
    using Windows.Devices.Spi;
    using Windows.Devices.I2c;
    using Windows.Devices.Enumeration;
    using Windows.Devices.Adc;
    using System.IO.Ports;
    using Windows.Devices.Pwm;

    using ZeligSupport = Microsoft.Zelig.Support.mbed;

#if (LPC1768)
    using LPC1768 = Llilum.LPC1768;
    using SPOT.Hardware;
#elif ( K64F )
    using K64F = Llilum.K64F;
#elif ( STM32F411 )
    using STM32F411 = Llilum.STM32F411;
#else
#error No target board defined.
#endif

    //--//

    class Program
    {
        const float period      = 0.75f;
        const float timePerMode = 4.0f;

#if LPC1768
        static int ledPin = (int)LPC1768.PinName.LED4;
#elif K64F
        static int ledPin = (int)K64F.PinName.LED4;
#elif STM32F411

#else
#error No target board defined.
#endif

        static int[] pinNumbers =
        {
#if (LPC1768)
            (int)LPC1768.PinName.LED1,
            (int)LPC1768.PinName.LED2,
            (int)LPC1768.PinName.LED3,
#elif (K64F)
            (int)K64F.PinName.LED1,
            (int)K64F.PinName.LED2,
            (int)K64F.PinName.LED3,
#elif (STM32F411)
            (int)STM32F411.PinName.LED1,
            (int)STM32F411.PinName.D13,
            (int)STM32F411.PinName.D12,
            (int)STM32F411.PinName.D11,
#else
#error No target board defined.
#endif
        }; 

        //--//

        static void Main()
        {
            OutputPort op = new OutputPort( (Cpu.Pin)ledPin, true ); 

            var blinkingTimer = new ZeligSupport.Timer();
            blinkingTimer.start( ); 

            while(true)
            {
                if (blinkingTimer.read() >= period)
                {
                    blinkingTimer.reset();

                    op.Write( !op.Read( ) ); 
                }
            }
        }
    }
}
