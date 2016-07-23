//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define LPC1768
//#define K64F
//#define USE_I2C
//#define USE_SPI
#define USE_GPIO
#define USE_THREADING


namespace Microsoft.Zelig.Test.mbed.Simple
{
    using System;
    using System.Collections.Generic;

#if (USE_THREADING)
    using System.Threading;
#endif
    using Windows.Devices.Gpio;
    using Windows.Devices.Spi;
    using Windows.Devices.I2c;
    using Windows.Devices.Enumeration;

    using ZeligSupport = Microsoft.Zelig.Support.mbed;

#if (LPC1768)
    using LPC1768 = Llilum.LPC1768;
    using System.Runtime.InteropServices;
#elif (K64F)
    using K64F = Llilum.K64F;
#else
#error No target board defined.
#endif

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

    class Program
    {
        public static byte[] i2cReadWrite1 = new byte[1];
        public static byte[] i2cReadWrite2 = new byte[2];

        public static double ReadTemp(I2cDevice device)
        {
            i2cReadWrite1[0] = 0x0;
            device.WriteRead(i2cReadWrite1, i2cReadWrite2);
            double temp = ((i2cReadWrite2[0] << 8) | i2cReadWrite2[1]) / 256.0;
            return temp;
        }

        const float period = 0.75f;
        const float timePerMode = 4.0f;

#if LPC1768
        static int threadPin = (int)LPC1768.PinName.LED4;
#elif K64F
        static int threadPin = (int)K64F.PinName.LED4;
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
#else
#error No target board defined.
#endif
        };

        static void Main()
        {
            var now = DateTime.Now.Ticks;

            ZeligSupport.Timer.wait_ms(1000);

            var now2 = DateTime.Now.Ticks;

            long now3 = now2 - now;

            Microsoft.Zelig.Runtime.Device.Instance.ProcessLog("NOW=0x%08x%08x", (int)(now3 >> 32), (int)(now3));

            int currentMode = 0;
            int count = 0;

            var controller = GpioController.GetDefault();
            var pins = new GpioPin[pinNumbers.Length];

            for (int i = 0; i < pinNumbers.Length; ++i)
            {
                GpioPin pin = controller.OpenPin(pinNumbers[i]);

                // Start with all LEDs on.
                pin.Write(GpioPinValue.High);
                pin.SetDriveMode(GpioPinDriveMode.Output);

                pins[i] = pin;
            }

            var solitary = controller.OpenPin(threadPin);
            solitary.SetDriveMode(GpioPinDriveMode.Output);

            LedToggler[] blinkingModes = new LedToggler[3];
            blinkingModes[0] = new LedTogglerSimultaneous(pins);
            blinkingModes[1] = new LedTogglerSequential(pins);
            blinkingModes[2] = new LedTogglerAlternate(pins);

            var blinkingTimer = new ZeligSupport.Timer();
            var blinkingModeSwitchTimer = new ZeligSupport.Timer();

            blinkingTimer.start();
            blinkingModeSwitchTimer.start();

            #region SPI
#if USE_GPIO
            // Get the device selector by friendly name
            string deviceSelector = SpiDevice.GetDeviceSelector("SPI0");
            var acqs = DeviceInformation.FindAllAsync(deviceSelector);
            string busId = acqs[0].Id;

            // Set up a non-default frequency
#if (LPC1768)
            int chipSelect = (int)LPC1768.PinName.p8;
#elif (K64F)
            int chipSelect = (int)K64F.PinName.PTD0;
#else
#error No target board defined.
#endif
            SpiConnectionSettings settings = new SpiConnectionSettings(chipSelect) {
                ClockFrequency = 1000000,
                Mode = SpiMode.Mode2
            };

            SpiDevice spiDevice = SpiDevice.FromIdAsync(busId, settings);
            byte[] writeBuffer = new byte[1];
            byte[] writeBuffer2 = new byte[] { 0x1, 0x2, 0x3 };
            byte[] readBuffer = new byte[1];
#endif
            #endregion

            #region I2C
#if USE_I2C
            //
            // I2C Init
            //

            string i2cDeviceSelector = I2cDevice.GetDeviceSelector("I2C1");
            var i2cAcqs = DeviceInformation.FindAllAsync(i2cDeviceSelector);
            string i2cBusId = i2cAcqs[0].Id;

            I2cDevice i2cDevice = I2cDevice.FromIdAsync(i2cBusId, new I2cConnectionSettings(0x90) {
                SharingMode = I2cSharingMode.Shared
            });

            //
            // Init temp sensor
            //
            i2cReadWrite2[0] = 0x1;
            i2cReadWrite2[1] = 0x0;
            i2cDevice.Write(i2cReadWrite2);
#endif
            #endregion

            int pinState = 1;
            solitary.Write((GpioPinValue)pinState);
            ZeligSupport.Timer.wait_ms(300);
            solitary.Write((GpioPinValue)0);
            ZeligSupport.Timer.wait_ms(300);
            solitary.Write((GpioPinValue)pinState);
            pinState = 0;

#if (USE_THREADING)
            var ev = new AutoResetEvent(false);

            var solitaryBlinker = new Thread(delegate () {
                while (true)
                {
                    ev.WaitOne();

                    solitary.Write((GpioPinValue)pinState);

                    pinState = ++pinState % 2;
                }
            });

            solitaryBlinker.Start();

            //var solitaryAlerter = new System.Threading.Timer( ( obj ) => { ((AutoResetEvent)obj).Set(); }, ev, 2000, 5000 );

            //Microsoft.CortexM3OnMBED.Drivers.SystemTimer.Timer tm = null;
            //tm =
            //    Microsoft.CortexM3OnMBED.Drivers.SystemTimer.Instance.CreateTimer((timer, currentTime) => { ev.Set(); tm.RelativeTimeout = 5 * 1000 * 1000; });
            //tm.RelativeTimeout = 5 * 1000 * 1000;
#endif
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

                    #region I2C_Impl
#if USE_I2C
                    try
                    {
                        double temp = ReadTemp(i2cDevice);
                        if (temp < 31.0)
                        {
                            currentMode = 0;
                        }
                        else if (temp >= 31.0 && temp < 32.5)
                        {
                            currentMode = 1;
                        }
                        else
                        {
                            currentMode = 2;
                        }
                    }
                    catch
                    {
                        // If an application board is not attached, we will get an Exception.
                        // Continue as normal in this case
                    }
#endif
                    #endregion

                    #region SPI_Impl
#if USE_SPI
                    writeBuffer[0] = (byte)currentMode;
                    spiDevice.TransferFullDuplex(writeBuffer, readBuffer);
                    if (readBuffer[0] == writeBuffer[0])
                    {
                        writeBuffer[0] = (byte)5;
                        spiDevice.TransferSequential(writeBuffer, readBuffer);
                    }
                    spiDevice.Write(writeBuffer2);
#endif
                    #endregion

                    count++;

                }


                blinkingModes[currentMode].run(blinkingTimer.read() / period);

            }
        }
    }
}
