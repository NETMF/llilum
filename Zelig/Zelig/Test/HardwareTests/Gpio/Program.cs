//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define LPC1768
//#define K64F

namespace Microsoft.Zelig.Test.GpioTest
{
    using System;
    using System.Threading;
    using Llilum.Devices.Gpio;

#if (LPC1768)
    using LPC1768 = Llilum.LPC1768;
#elif ( K64F )
    using K64F = Llilum.K64F;
#else
#error No target board defined.
#endif

    //--//

    class Program
    {
        internal class GpioInterruptTestData : IDisposable
        {
            private readonly AutoResetEvent m_evtComplete;
            private readonly GpioPin m_pinLED;
            private readonly GpioPin m_pinIn;
            private readonly GpioPin m_pinOut;
            private readonly GpioPin m_pinMon;

            private int m_pinVal;
            private bool m_disposed;

            public GpioInterruptTestData(int LedPinNumber, int InPinNumber, int OutPinNumber, int MonitorPin)
            {
                m_evtComplete = new AutoResetEvent(false);
                m_pinVal = 1;
                m_pinLED = GpioPin.TryCreateGpioPin(LedPinNumber);
                m_pinLED.Direction = PinDirection.Output;
                m_pinLED.Write(0);
                m_pinOut = GpioPin.TryCreateGpioPin(OutPinNumber);
                m_pinOut.Direction = PinDirection.Output;
                m_pinOut.Write(0);
                m_pinMon = GpioPin.TryCreateGpioPin(MonitorPin);
                m_pinMon.Direction = PinDirection.Output;
                m_pinMon.Write(0);
                m_pinIn = GpioPin.TryCreateGpioPin( InPinNumber );
                m_pinIn.Direction = PinDirection.Input;
            }

            public GpioPin PinLED => m_pinLED;
            public GpioPin PinIn => m_pinIn;
            public GpioPin PinOut => m_pinOut;
            public GpioPin PinMon => m_pinMon;

            public void TogglePinValue()
            {
                m_pinLED.Write(m_pinVal);
                m_pinVal = m_pinVal == 1 ? 0 : 1;
            }
            public void SignalComplete() { m_evtComplete.Set(); }
            public void WaitComplete(int timeout) { m_evtComplete.WaitOne(timeout, false); }

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
#else
#error No target board defined.
#endif
        };

        static void Main( )
        {
            while(true)
            {
                if (!TestGpioInterrupts())
                {
                    Console.WriteLine( "TestGpioInterrupts FAILED!" );
                    break;
                }

                // Sleep 1 second between test runs
                Thread.Sleep( 1000 );
            }
        }

        static bool TestGpioInterrupts()
        {
            bool result = true;

            using(s_GpioInterruptTestData = new GpioInterruptTestData( TEST_GPIO_PINS[ 0 ], TEST_GPIO_PINS[ 1 ], TEST_GPIO_PINS[ 2 ], TEST_GPIO_PINS[ 3 ] ))
            {
                s_GpioInterruptTestData.PinIn.ValueChanged += HandlePinValueChanged;

                try
                {
                    s_GpioInterruptTestData.PinMon.Write( 1 );
                    s_GpioInterruptTestData.PinOut.Write( 1 );
                    s_GpioInterruptTestData.WaitComplete( 1000 );
                    s_GpioInterruptTestData.PinOut.Write( 0 );
                }
                catch(TimeoutException)
                {
                    result = false;
                }

                s_GpioInterruptTestData.PinIn.ValueChanged -= HandlePinValueChanged;
            }

            return result;
        }

        private static void HandlePinValueChanged(object sender, PinEdge edge)
        {
            if (s_GpioInterruptTestData != null)
            {
                s_GpioInterruptTestData.PinMon.Write(0);
                s_GpioInterruptTestData.TogglePinValue();
                s_GpioInterruptTestData.SignalComplete();
            }
        }
    }
}
