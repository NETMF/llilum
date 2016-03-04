//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define GPIO_INTERRUPTS_PERF

#define LPC1768
//#define K64F

namespace Microsoft.Zelig.Test.mbed.Simple
{
    using System;
    using System.Threading;
    using Windows.Devices.Gpio;
    
    using LPC1768   = Llilum.LPC1768;
    using K64F      = Llilum.K64F;


    partial class Program
    {
        private class GpioInterruptTestData : IDisposable
        {
            private AutoResetEvent m_evtComplete;
            private int m_pinVal;
            private Llilum.Devices.Gpio.GpioPin m_pinLED;
            private Llilum.Devices.Gpio.GpioPin m_pinIn;
            private Llilum.Devices.Gpio.GpioPin m_pinOut;
            private Llilum.Devices.Gpio.GpioPin m_pinMon;
            private bool m_disposed;

            public GpioInterruptTestData( int LedPinNumber, int InPinNumber, int OutPinNumber, int MonitorPin )
            {
                m_evtComplete = new AutoResetEvent( false );
                m_pinVal = 1;
                m_pinLED = Llilum.Devices.Gpio.GpioPin.TryCreateGpioPin( LedPinNumber );
                m_pinLED.Direction = Llilum.Devices.Gpio.PinDirection.Output;
                m_pinLED.Write( 0 );
                m_pinOut = Llilum.Devices.Gpio.GpioPin.TryCreateGpioPin( OutPinNumber );
                m_pinOut.Direction = Llilum.Devices.Gpio.PinDirection.Output;
                m_pinOut.Write( 0 );
                m_pinMon = Llilum.Devices.Gpio.GpioPin.TryCreateGpioPin( MonitorPin );
                m_pinMon.Direction = Llilum.Devices.Gpio.PinDirection.Output;
                m_pinMon.Write( 0 );
                m_pinIn = Llilum.Devices.Gpio.GpioPin.TryCreateGpioPin( InPinNumber );
                m_pinIn.Direction = Llilum.Devices.Gpio.PinDirection.Input;
            }

            public Llilum.Devices.Gpio.GpioPin PinLED => m_pinLED;
            public Llilum.Devices.Gpio.GpioPin PinIn => m_pinIn;
            public Llilum.Devices.Gpio.GpioPin PinOut => m_pinOut;
            public Llilum.Devices.Gpio.GpioPin PinMon => m_pinMon;

            public void TogglePinValue( )
            {
                m_pinLED.Write( m_pinVal );
                m_pinVal = m_pinVal == 1 ? 0 : 1;
            }
            public void SignalComplete( ) { m_evtComplete.Set( ); }
            public void WaitComplete( ) { m_evtComplete.WaitOne( ); }

            ~GpioInterruptTestData( )
            {
                Dispose( false );
            }

            public void Dispose( )
            {
                if(!m_disposed)
                {
                    Dispose( true );
                    GC.SuppressFinalize( this );
                    m_disposed = true;
                }
            }

            protected void Dispose( bool disposing )
            {
                if(disposing)
                {
                    s_gpioInterruptTestData = null;

                    m_pinIn .Dispose( );
                    m_pinLED.Dispose( );
                    m_pinOut.Dispose( );
                    m_pinMon.Dispose( );
                }
            }
        };
        
        private static GpioInterruptTestData s_gpioInterruptTestData;

        //--//

        private static void TestGpioInterrupt( int loops )
        {
#if GPIO_INTERRUPTS_PERF
            int[] TEST_GPIO_PINS =
            {
#if( LPC1768 )
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
#elif ( K64F )
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
#endif
            };
            
            using(s_gpioInterruptTestData = new GpioInterruptTestData( TEST_GPIO_PINS[ 0 ], TEST_GPIO_PINS[ 1 ], TEST_GPIO_PINS[ 2 ], TEST_GPIO_PINS[ 3 ] ))
            {
                s_gpioInterruptTestData.PinIn.ValueChanged += HandlePinValueChanged;

                while(loops-- > 0)
                {
                    s_gpioInterruptTestData.PinMon.Write( 1 );
                    s_gpioInterruptTestData.PinOut.Write( 1 );

                    s_gpioInterruptTestData.WaitComplete( );

                    s_gpioInterruptTestData.PinOut.Write( 0 );

                    Thread.Sleep( 2000 );
                }

                s_gpioInterruptTestData.PinIn.ValueChanged -= HandlePinValueChanged;
            }
#endif
        }

        private static void HandlePinValueChanged( object sender, Llilum.Devices.Gpio.PinEdge edge )
        {
            if(s_gpioInterruptTestData != null)
            {
                s_gpioInterruptTestData.PinMon.Write( 0 );

                s_gpioInterruptTestData.TogglePinValue( );

                s_gpioInterruptTestData.SignalComplete( );
            }
        }
    }
}
