//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Test.mbed.Simple
{
    using System;
    using Windows.Devices.Gpio;


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
                this[i] = ((int)(t * 4) == i) ? GpioPinValue.High : GpioPinValue.Low;
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
}
