//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define LPC1768
//#define K64F


namespace Microsoft.Zelig.Test.mbed.Simple
{ 
    using System.Collections.Generic;
    using Windows.Devices.Gpio;

    using Microsoft.Zelig.Support.mbed;
    
    //--//

    internal abstract class LedToggler
    {
        public LedToggler(GpioPin[] pins)
        {
            _pins = pins;
        }

        public abstract void run(float t);

        public int PinCount
        {
            get
            {
                return _pins.Length;
            }
        }

        public GpioPinValue this[int key]
        {
            get
            {
                return _pins[key].Read();
            }
            set
            {
                _pins[key].Write(value);
            }
        }

        private readonly GpioPin[] _pins;
    };

    internal class LedTogglerSimultaneous : LedToggler
    {
        public LedTogglerSimultaneous(GpioPin[] pins)
            : base(pins)
        {
        }

        public override void run(float t)
        {
            for (int i = 0; i < PinCount; ++i)
            {
                this[i] = (t < 0.5) ? GpioPinValue.High : GpioPinValue.Low;
            }
        }
    }

    internal class LedTogglerSequential : LedToggler
    {
        public LedTogglerSequential(GpioPin[] pins)
            : base(pins)
        {
        }

        public override void run(float t)
        {
            for (int i = 0; i < PinCount; ++i)
            {
                this[i] = ((int)(t * 4)  == i) ? GpioPinValue.High : GpioPinValue.Low;
            }
        }
    }

    internal class LedTogglerAlternate : LedToggler
    {
        public LedTogglerAlternate(GpioPin[] pins)
            : base(pins)
        {
        }

        public override void run(float t)
        {
            for (int i = 0; i < PinCount; ++i)
            {
                this[i] = ((i % 2) == (int)(t * 2)) ? GpioPinValue.High : GpioPinValue.Low;
            }
        }
    }


    class Program
    {
        const float period = 0.75f;
        const float timePerMode = 4.0f;

        static int[] pinNumbers =
        {
#if (LPC1768)
            (int)PinName.LED1,
            (int)PinName.LED2,
            (int)PinName.LED3,
            (int)PinName.LED4,
#elif (K64F)
            (1 << 12) | 22,
            (1 << 12) | 21,
#else
            #error No target board defined.
#endif
        };

        static void Main()
        {
            var controller = new GpioController(new MbedGpioProvider());
            var pins = new GpioPin[pinNumbers.Length];

            for (int i = 0; i < pinNumbers.Length; ++i)
            {
                GpioPin pin = controller.OpenPin(pinNumbers[i]);

                // Start with all LEDs on.
                pin.Write(GpioPinValue.High);
                pin.SetDriveMode(GpioPinDriveMode.Output);

                pins[i] = pin;
            }

            LedToggler[] blinkingModes = new LedToggler[3];
            blinkingModes[0] = new LedTogglerSimultaneous(pins);
            blinkingModes[1] = new LedTogglerSequential(pins);
            blinkingModes[2] = new LedTogglerAlternate(pins);

            var blinkingTimer = new Timer();
            var blinkingModeSwitchTimer = new Timer();

            blinkingTimer.start();
            blinkingModeSwitchTimer.start();

            int currentMode = 0;

            while (true)
            {
                if (blinkingTimer.read() >= period)
                {
                    blinkingTimer.reset();
                }

                if (blinkingModeSwitchTimer.read() >= timePerMode)
                {
                    currentMode = (currentMode + 1) % blinkingModes.Length;
                    blinkingModeSwitchTimer.reset();
                }

                blinkingModes[currentMode].run(blinkingTimer.read() / period);
            }
        }
    }
}
