//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define LPC1768
//#define K64F
//#define USE_I2C
//#define USE_SPI
#define USE_GPIO
#define USE_THREADING
#define USE_ADC

//
// The following define requires the serial port RX/TX pin be wired for loop-back
// For LPC1768, the target serial port is "UART2", and for K64F the target is
// "UART3". 
//
//#define TEST_SERIALPORT

//
// The following test tests GPIO interrupts by using a set of interruptible pins.
//
//#define TEST_GPIO_INTERRUPTS

namespace Microsoft.Zelig.Test.mbed.Simple
{
    using System;
    using System.Collections.Generic;

#if (USE_THREADING)
    using System.Threading;
#endif
    using Windows.Devices.Gpio;
    using Windows.Devices.Gpio.Provider;
    using Windows.Devices.Spi;
    using Windows.Devices.I2c;
    using Windows.Devices.Enumeration;
    using Windows.Devices.Adc;
    using System.IO.Ports;

    using ZeligSupport = Microsoft.Zelig.Support.mbed;

#if (LPC1768)
    using LPC1768 = Zelig.LPC1768;
    using System.Runtime.InteropServices;
#elif (K64F)
    using K64F = Zelig.K64F;
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
            int currentMode = 0;
            int count = 0;
            float periodDivider = 1;

#if TEST_SERIALPORT
            TestSerialPort();
#endif

#if (TEST_GPIO_INTERRUPTS)
            TestGpioInterrupt(5);
#endif

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

            var solitary = controller.OpenPin( threadPin );
            solitary.SetDriveMode( GpioPinDriveMode.Output );

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
            ZeligSupport.Timer.wait_ms(300);
            solitary.Write((GpioPinValue)0);
            ZeligSupport.Timer.wait_ms(300);
            solitary.Write((GpioPinValue)pinState);
            pinState = 0;

#if (USE_THREADING)
            var ev = new AutoResetEvent(false);
            var solitaryBlinker = new Thread(delegate ()
            {
                while (true)
                {
                    ev.WaitOne(1000, false);

                    solitary.Write((GpioPinValue)pinState);

                    pinState = ++pinState % 2;
                }
            });
            solitaryBlinker.Start();

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

#if (USE_ADC)
            AdcController adcController = AdcController.GetDefaultAsync();

            // This is the left potentiometer on the mBed application board
            AdcChannel adcChannel = adcController.OpenChannel(4);
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
        }

        //
        // This test requires the RX/TX pins to be connected for hardware loop-back.
        // For LPC1768, connect pins p13 and p14.
        // For FRDM K64F, connect pins PTC17 and PTC16.
        //
        private static bool TestLoopback(SerialPort port, ref byte[] txtBuffer)
        {
            byte b = 0;
            bool result = true;
            int len = txtBuffer.Length;

            //
            // Fill TX buffer with incrementing pattern
            //
            for(int i=0;i<len;i++)
            {
                txtBuffer[i] = b++;
            }

            port.Write(txtBuffer, 0, len);

            //
            // Clear the buffer for the read
            //
            Array.Clear(txtBuffer, 0, len);

            //
            // The read can return less than the requested length if
            // the buffer contains data.
            //
            int read = 0;
            while ( read < len )
            {
                read += port.Read(txtBuffer, read, len-read);
            }
            
            //
            // Reset the test byte value and increment over the length
            // of the RX buffer and verify the pattern.
            //
            b = 0;
            for (int i = 0; i < len; i++)
            {
                if( txtBuffer[i] != b++)
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

#if TEST_SERIALPORT
        private static bool TestSerialPort()
        {
            string serialPortName;
            bool result = true;

#if (LPC1768)
            serialPortName = "UART1";
#elif (K64F)
            serialPortName = "UART3";
#else
            throw new NotSupportedException();
#endif

            using (SerialPort port = new SerialPort(serialPortName, 115200, Parity.None, 8, StopBits.One))
            {
                port.Open();
                byte[] txtBuffer = new byte[32];

                for (int i = 0; i < 5; i++)
                {
                    if( !TestLoopback(port, ref txtBuffer) )
                    {
                        result = false;
                    }

                    Thread.Sleep(2000);
                }
            }

            return result;
        }
#endif // #if TEST_SERIALPORT

#if (TEST_GPIO_INTERRUPTS)

        internal class GpioInterruptTestData : IDisposable
        {
            private AutoResetEvent m_evtComplete;
            private GpioPinValue m_pinVal;
            private GpioPin m_pinLED;
            private GpioPin m_pinOut;
            private GpioPin m_pinMon;
            private int m_loops;
            private bool m_disposed;

            public GpioInterruptTestData(int LedPinNumber, int OutPinNumber, int MonitorPin, int loops)
            {
                GpioController ctrl = GpioController.GetDefault();
                m_evtComplete = new AutoResetEvent(false);
                m_pinVal = GpioPinValue.High;
                m_pinLED = ctrl.OpenPin(LedPinNumber);
                m_pinLED.SetDriveMode(GpioPinDriveMode.Output);
                m_pinLED.Write(GpioPinValue.Low);
                m_pinOut = ctrl.OpenPin(OutPinNumber);
                m_pinOut.SetDriveMode(GpioPinDriveMode.Output);
                m_pinOut.Write(GpioPinValue.Low);
                m_pinMon = ctrl.OpenPin(MonitorPin);
                m_pinMon.SetDriveMode(GpioPinDriveMode.Output);
                m_pinMon.Write(GpioPinValue.Low);
                m_loops = loops;
            }

            public int Loops { get { return m_loops; } set { m_loops = value; } }
            public GpioPin PinLED { get { return m_pinLED; } }
            public GpioPin PinOut { get { return m_pinOut; } }
            public GpioPin PinMon { get { return m_pinMon; } }

            public void TogglePinValue()
            {
                m_pinLED.Write(m_pinVal);
                m_pinVal = m_pinVal == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High;
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
                    m_pinLED.Dispose();
                    m_pinOut.Dispose();
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
            (int)LPC1768.PinName.p10,
            (int)LPC1768.PinName.p11,
            (int)LPC1768.PinName.p9
#elif (K64F)
            //
            // On the Freescale FRDM K64F board, this test requires you to jumper pins
            // PTC5 and PTC7 together to test GPIO interrupts.  Pin PTC0 can be used as
            // a monitor pin with an oscilloscope to measure the time to service a GPIO
            // interrupt from creation to event handling.
            //
            (int)K64F.PinName.LED1,
            (int)K64F.PinName.PTC5,
            (int)K64F.PinName.PTC0,
            (int)K64F.PinName.PTC7
#else
#error No target board defined.
#endif
        };

        private static void TestGpioInterrupt(int loops)
        {
            using (s_GpioInterruptTestData = new GpioInterruptTestData(TEST_GPIO_PINS[0], TEST_GPIO_PINS[1], TEST_GPIO_PINS[2], loops))
            {
                using (GpioPin pin = GpioController.GetDefault().OpenPin(TEST_GPIO_PINS[3]))
                {
                    pin.SetDriveMode(GpioPinDriveMode.Input);
                    pin.ValueChanged += Pin_ValueChanged;

                    for (int i = 0; i < loops; i++)
                    {
                        s_GpioInterruptTestData.PinMon.Write(GpioPinValue.High);
                        s_GpioInterruptTestData.PinOut.Write(GpioPinValue.High);
                        s_GpioInterruptTestData.WaitComplete();
                        s_GpioInterruptTestData.PinOut.Write(GpioPinValue.Low);
                        Thread.Sleep(2000);
                    }

                    pin.ValueChanged -= Pin_ValueChanged;
                }
            }

            s_GpioInterruptTestData = null;
        }
        private static void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            if (s_GpioInterruptTestData != null)
            {
                s_GpioInterruptTestData.PinMon.Write(GpioPinValue.Low);
                s_GpioInterruptTestData.TogglePinValue();
                s_GpioInterruptTestData.SignalComplete();
            }
        }
#endif // #if (TEST_GPIO_INTERRUPTS)
    }
}
