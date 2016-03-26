//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Test.mbed.Simple
{
    using System.Threading;
    using Windows.Devices.Gpio;

    using ZeligSupport = Microsoft.Zelig.Support.mbed;
    
    //--//

    internal partial class Program
    {
        private const float period      = 0.75f;
        private const float timePerMode = 4.0f;
        
        //--//

        private int[] m_ledPinNumbers;
        private int   m_threadPinNumber;
        private int   m_pwmPinNumber;

        //--//

        internal Program()
        {
            m_ledPinNumbers   =      DeviceModels.Chipset.CortexM.Board.Instance.LedPins;
            m_pwmPinNumber    = (int)DeviceModels.Chipset.CortexM.Board.Instance.PwmPins[0];

            m_threadPinNumber = m_ledPinNumbers[ m_ledPinNumbers.Length - 1 ];
        }

        private void Run()
        {
            System.Diagnostics.Debug.WriteLine( "User program starting..." );
            
            int currentMode     = 0;
            float periodDivider = 1;
            
            TestGpioPerf();

            TestGpioInterrupt( 5 );
            
            TestSpiLcd( );

            var controller = GpioController.GetDefault();
            var pins = new GpioPin[m_ledPinNumbers.Length - 1];

            for (int i = 0; i < m_ledPinNumbers.Length  - 1; ++i)
            {
                GpioPin pin = controller.OpenPin(m_ledPinNumbers[i]);

                // Start with all LEDs on.
                pin.Write(GpioPinValue.High);
                pin.SetDriveMode(GpioPinDriveMode.Output);

                pins[i] = pin;
            }
            
            var solitary = controller.OpenPin(m_threadPinNumber);

            solitary.SetDriveMode(GpioPinDriveMode.Output);

            LedToggler[] blinkingModes = new LedToggler[3];
            blinkingModes[0] = new LedTogglerSimultaneous(pins);
            blinkingModes[1] = new LedTogglerSequential(pins);
            blinkingModes[2] = new LedTogglerAlternate(pins);

            var blinkingTimer = new ZeligSupport.Timer();
            var blinkingModeSwitchTimer = new ZeligSupport.Timer();

            blinkingTimer.start();
            blinkingModeSwitchTimer.start();

            InitSPI( ); 
           
            int pinState = 1;
            solitary.Write((GpioPinValue)pinState);
            Thread.Sleep(300);
            solitary.Write((GpioPinValue)0);
            Thread.Sleep(300);
            solitary.Write((GpioPinValue)pinState);
            pinState = 0;
            
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

            TestADC( );

            TestPWM( m_pwmPinNumber );

            while (true)
            {
                float readVal = TestADC( ); 

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

                    currentMode = TestI2C( currentMode );

                    TestSPI( currentMode );
                }

                blinkingModes[currentMode].run(blinkingTimer.read() / (period / periodDivider));

            }
        }

        //--//

        public static void Main()
        {
            new Program( ).Run( );
        }
    }
}
