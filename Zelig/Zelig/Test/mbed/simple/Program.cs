//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define LPC1768
//#define K64F
//#define STM32F411
//#define STM32F401
//#define STM32F091
//#define WIN32

//#define USE_I2C
//#define USE_SPI
#define USE_GPIO
#define USE_THREADING

#if !LPC1768
#if !STM32F091
#define USE_THREADING_TIMER
#endif
#endif

//#define USE_ADC
//#define USE_PWM

#if LPC1768
//
// The following define enables a SPI tests that communicates over SPI with the LCD on the MBED Application
// Board for the LPC1768 device.  This test is only valid for the LPC1768 device with the MBED Application
// Board.
//
//#define TEST_SPI_LCD

//
// The following test GPIO output toggling performance in a tight loop.  You will need an oscilloscope to 
// measure the performance.
// 
//#define TEST_GPIO_PERF
#endif

//
// The following test tests GPIO interrupts by using a set of interruptible pins.
//
//#define TEST_GPIO_INTERRUPTS

namespace Microsoft.Zelig.Test.mbed.Simple
{
    using System;

#if (USE_THREADING) || (TEST_GPIO_INTERRUPTS)
    using System.Threading;
#endif
    using Windows.Devices.Gpio;
    using Windows.Devices.Spi;
    using Windows.Devices.I2c;
    using Windows.Devices.Enumeration;
    using Windows.Devices.Adc;
    using System.IO.Ports;
    using Windows.Devices.Pwm;

    using ZeligSupport = Microsoft.Zelig.Support.mbed;
    using Runtime;
#if (LPC1768)
    using LPC1768 = Llilum.LPC1768;
#elif (K64F)
    using K64F = Llilum.K64F;
#elif (STM32F411)
    using STM32F411 = Llilum.STM32F411;
#elif (WIN32)
#elif (STM32F401)
    using STM32F401 = Llilum.STM32F401;
#elif (STM32F091)
    using STM32F091 = Llilum.STM32F091;
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
        static int pwmPinNumber = (int)LPC1768.PinName.p21;
#elif K64F
        static int threadPin = (int)K64F.PinName.LED4;
        static int pwmPinNumber = (int)K64F.PinName.D3;
#elif STM32F411
        static int pwmPinNumber = (int)STM32F411.PinName.D3;
#elif (WIN32)
#elif STM32F401
        static int pwmPinNumber = (int)STM32F401.PinName.D3;
#elif STM32F091
        static int threadPin = (int)STM32F091.PinName.LED4;
        static int pwmPinNumber = (int)STM32F091.PinName.PB_0;
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
#elif (WIN32)
#elif (STM32F401)
            (int)STM32F401.PinName.LED1,
            (int)STM32F401.PinName.D13,
            (int)STM32F401.PinName.D12,
            (int)STM32F401.PinName.D11,
#elif (STM32F091)
            (int)STM32F091.PinName.LED1,
            (int)STM32F091.PinName.LED2,
            (int)STM32F091.PinName.LED3,
#else
#error No target board defined.
#endif
    };


        static void Main()
        {
#if (WIN32)
            TestWin32Threading();
#else
            int currentMode = 0;
            int count = 0;
            float periodDivider = 1;

#if TEST_GPIO_PERF && LPC1768
            TestGpioPerf();
#endif // TEST_GPIO_PERF && LPC1768

#if (TEST_GPIO_INTERRUPTS)
            TestGpioInterrupt(5);
#endif // TEST_GPIO_INTERRUPTS

#if TEST_SPI_LCD && LPC1768
            TestSpiLcd( );
#endif // TEST_SPI_LCD && LPC1768


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

#if !(STM32F411 || STM32F401)
            var solitary = controller.OpenPin(threadPin);
            solitary.SetDriveMode(GpioPinDriveMode.Output);
#endif
            LedToggler[] blinkingModes = new LedToggler[3];
            blinkingModes[0] = new LedTogglerSimultaneous(pins);
            blinkingModes[1] = new LedTogglerSequential(pins);
            blinkingModes[2] = new LedTogglerAlternate(pins);

            var blinkingTimer = new ZeligSupport.Timer();
            var blinkingModeSwitchTimer = new ZeligSupport.Timer();

            blinkingTimer.start();
            blinkingModeSwitchTimer.start();
#if !(STM32F411 || STM32F401)
#region SPI
#if USE_SPI
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
            SpiConnectionSettings settings = new SpiConnectionSettings(chipSelect)
            {
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
            Thread.Sleep(300);
            solitary.Write((GpioPinValue)0);
            Thread.Sleep(300);
            solitary.Write((GpioPinValue)pinState);
            pinState = 0;

#if (USE_THREADING)
            var ev = new AutoResetEvent(false);
            var solitaryBlinker = new Thread(delegate ()
            {
                while (true)
                {
                    ev.WaitOne(500, false);

                    solitary.Write((GpioPinValue)pinState);

                    pinState = ++pinState % 2;
                }
            });
            solitaryBlinker.Start();

#if (USE_THREADING_TIMER)
            var solitaryAlerter = new System.Threading.Timer((obj) =>
            {
                // blink 20 times very fast
                int fastBlinks = 20;
                while (fastBlinks-- > 0)
                {
                    ((AutoResetEvent)obj).Set();

                    Thread.Sleep(50);
                }
            }, ev, 2000, 5000);
#endif

#endif
#endif // !STM32F411

#if (USE_ADC)
            AdcController adcController = AdcController.GetDefaultAsync();

            // This is the left potentiometer on the mBed application board
            AdcChannel adcChannel = adcController.OpenChannel(4);
#endif

#if (USE_PWM)
            var pwmController = PwmController.GetDefaultAsync();
            pwmController.SetDesiredFrequency(1000000);

            var pwmPin = pwmController.OpenPin(pwmPinNumber);
            pwmPin.SetActiveDutyCyclePercentage(0.7F);
            pwmPin.Polarity = PwmPulsePolarity.ActiveLow;
            pwmPin.Start();

#endif
            float readVal = 0;
            while (true)
            {
#if (USE_ADC)
                readVal = ((float)adcChannel.ReadValue()) / adcController.MaxValue * 2;
#endif
                // If ADC isn't in use, this will always be 1
                periodDivider = 1 + readVal;

                if (blinkingTimer.read() >= (period / periodDivider))
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


                blinkingModes[currentMode].run(blinkingTimer.read() / (period / periodDivider));

            }
#endif // WIN32
        }

#if (WIN32)
        static int s_Counter1 = 0;
        static int s_Counter2 = 0;
        static int s_Counter3 = 0;

        static void TestWin32Threading()
        {
            Thread th1 = new Thread( ( ) =>
            {
                int i=0;

                while(true)
                {
                    BugCheck.Log( "Thread1: " + i++);
                    Thread.Sleep(500);
                }
            } );

            Thread th2 = new Thread( ( ) =>
            {
                int i=0;

                while(true)
                {
                    BugCheck.Log( "Thread2: " + i++);
                    Thread.Sleep(500);
                }
            } );

            Timer t1 = new Timer( (object arg) =>
            {
                BugCheck.Log( "Timer1: " + s_Counter1++ );
            }, null, 1000, 1000 );

            Timer t2 = new Timer( (object arg) =>
            {
                BugCheck.Log( "Timer2: " + s_Counter2++ );
            }, null, 2000, 2000 );

            Timer t3 = new Timer( (object arg) =>
            {
                BugCheck.Log( "Timer3: " + s_Counter3++ );
            }, null, 4000, 4000 );

            th1.Start( );
            th2.Start( );

            while(true)
            {
                Thread.Sleep(-1);
            }
        }
#endif // WIN32

#if TEST_GPIO_INTERRUPTS
        internal class GpioInterruptTestData : IDisposable
        {
            private AutoResetEvent m_evtComplete;
            private int m_pinVal;
            private Llilum.Devices.Gpio.GpioPin m_pinLED;
            private Llilum.Devices.Gpio.GpioPin m_pinIn;
            private Llilum.Devices.Gpio.GpioPin m_pinOut;
            private Llilum.Devices.Gpio.GpioPin m_pinMon;
            private bool m_disposed;

            public GpioInterruptTestData(int LedPinNumber, int InPinNumber, int OutPinNumber, int MonitorPin)
            {
                m_evtComplete = new AutoResetEvent(false);
                m_pinVal = 1;
                m_pinLED = Llilum.Devices.Gpio.GpioPin.TryCreateGpioPin(LedPinNumber);
                m_pinLED.Direction = Llilum.Devices.Gpio.PinDirection.Output;
                m_pinLED.Write(0);
                m_pinOut = Llilum.Devices.Gpio.GpioPin.TryCreateGpioPin(OutPinNumber);
                m_pinOut.Direction = Llilum.Devices.Gpio.PinDirection.Output;
                m_pinOut.Write(0);
                m_pinMon = Llilum.Devices.Gpio.GpioPin.TryCreateGpioPin(MonitorPin);
                m_pinMon.Direction = Llilum.Devices.Gpio.PinDirection.Output;
                m_pinMon.Write(0);
                m_pinIn = Llilum.Devices.Gpio.GpioPin.TryCreateGpioPin( InPinNumber );
                m_pinIn.Direction = Llilum.Devices.Gpio.PinDirection.Input;
            }

            public Llilum.Devices.Gpio.GpioPin PinLED => m_pinLED;
            public Llilum.Devices.Gpio.GpioPin PinIn => m_pinIn;
            public Llilum.Devices.Gpio.GpioPin PinOut => m_pinOut;
            public Llilum.Devices.Gpio.GpioPin PinMon => m_pinMon;

            public void TogglePinValue()
            {
                m_pinLED.Write(m_pinVal);
                m_pinVal = m_pinVal == 1 ? 0 : 1;
            }
            public void SignalComplete() { m_evtComplete.Set(); }
            public void WaitComplete() { m_evtComplete.WaitOne(); }

            ~GpioInterruptTestData()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                if (!m_disposed)
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                    m_disposed = true;
                }
            }

            protected void Dispose(bool disposing)
            {
                if (disposing)
                {
                    s_GpioInterruptTestData = null;

                    m_pinIn.Dispose();
                    m_pinLED.Dispose();
                    m_pinOut.Dispose();
                    m_pinMon.Dispose();
                }
            }
        };

        public static GpioInterruptTestData s_GpioInterruptTestData;

        static int[] TEST_GPIO_PINS =
        {
#if (LPC1768)
            //
            // LPC1768 only supports interrupts on pins P0_* and P2_* (ports 0 and 2).
            // P0_0 == p9, p2_0 == p26
            //
            // On the LPC1768, pins p9, p10, p11 and LED1 are used for this tests.  Pins p9
            // and p10 should be physically connected with a jumper.  Pin p11 is used as a 
            // monitor pin which indicates how much time was taken to service the interrupt.
            //
            (int)LPC1768.PinName.LED1,
            (int)LPC1768.PinName.p9,
            (int)LPC1768.PinName.p10,
            (int)LPC1768.PinName.p11,
#elif (K64F)
            //
            // On the Freescale FRDM K64F board, this test requires you to jumper pins
            // PTC5 and PTC7 together to test GPIO interrupts.  Pin PTC0 can be used as
            // a monitor pin with an oscilloscope to measure the time to service a GPIO
            // interrupt from creation to event handling.
            //
            (int)K64F.PinName.LED1,
            (int)K64F.PinName.PTC7,
            (int)K64F.PinName.PTC5,
            (int)K64F.PinName.PTC0,
#elif (WIN32)
#else
#error No target board defined.
#endif
        };

        static void TestGpioInterrupt( int loops)
        {
            using(s_GpioInterruptTestData = new GpioInterruptTestData( TEST_GPIO_PINS[ 0 ], TEST_GPIO_PINS[ 1 ], TEST_GPIO_PINS[ 2 ], TEST_GPIO_PINS[ 3 ] ))
            {
                s_GpioInterruptTestData.PinIn.ValueChanged += HandlePinValueChanged;

                while( loops-- > 0)
                {
                    s_GpioInterruptTestData.PinMon.Write( 1 );
                    s_GpioInterruptTestData.PinOut.Write( 1 );
                    s_GpioInterruptTestData.WaitComplete( );
                    s_GpioInterruptTestData.PinOut.Write( 0 );
                    Thread.Sleep( 2000 );
                }

                s_GpioInterruptTestData.PinIn.ValueChanged -= HandlePinValueChanged;
            }
        }

        private static void HandlePinValueChanged(object sender, Llilum.Devices.Gpio.PinEdge edge)
        {
            if (s_GpioInterruptTestData != null)
            {
                s_GpioInterruptTestData.PinMon.Write(0);
                s_GpioInterruptTestData.TogglePinValue();
                s_GpioInterruptTestData.SignalComplete();
            }
        }
#endif // #if TEST_GPIO_INTERRUPTS

#if TEST_GPIO_PERF && LPC1768
        private static void TestGpioPerf()
        {
            using(var pinOut = Runtime.GpioProvider.Instance.CreateGpioPin( (int)LPC1768.PinName.p28 ))
            {
                pinOut.Direction = Llilum.Devices.Gpio.PinDirection.Output;
                pinOut.Mode      = Llilum.Devices.Gpio.PinMode.Default;

                for(int i = 0; i<0x1000; i++)
                {
                    pinOut.Write( 0 );
                    pinOut.Write( 1 );
                }
            }
        }
#endif // TEST_GPIO_PERF && LPC1768

#if TEST_SPI_LCD && LPC1768
       private static void TestSpiLcd()
        {
            int commandPin = (int)LPC1768.PinName.p8;
            int resetPin   = (int)LPC1768.PinName.p6;
            int chipSelPin = (int)LPC1768.PinName.p11;

            Runtime.SpiChannelInfo channelInfo = Runtime.SpiProvider.Instance.GetSpiChannelInfo( 0 );

            using(SpiLcdC12832 lcd = new SpiLcdC12832( channelInfo, commandPin, resetPin, chipSelPin ))
            {
                lcd.Clear( );

                for(int x = 0; x<128; x++)
                {
                    for(int y = 0; y<32; y++)
                    {
                        lcd.SetPixel( x, y, 1 );

                        if(0x10 == ( y & 0x10 ))
                        {
                            lcd.Flush( );
                        }
                    }
                }

                lcd.Clear( );
                Thread.Sleep( 200 );
                lcd.Fill( );

                for(byte contrast = 0; contrast < 0x3f; contrast++)
                {
                    lcd.SetContast( contrast );
                    Thread.Sleep( 100 );
                }
            }
        }
#endif // USE_SPI && LPC1768
    }
}
