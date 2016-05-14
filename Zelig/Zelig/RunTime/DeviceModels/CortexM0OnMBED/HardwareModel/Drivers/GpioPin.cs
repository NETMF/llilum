//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM0OnMBED.HardwareModel
{
    using System;
    using Llilum    = Llilum.Devices.Gpio;
    using RT        = Microsoft.Zelig.Runtime;
    using M         = DeviceModels.Chipset.CortexM;
    using MBED      = CortexM0OnMBED.Drivers;
    using LLOS      = Zelig.LlilumOSAbstraction.API.IO;
    using LLGPIO    = Zelig.LlilumOSAbstraction.API.IO.Gpio;
    using LLINTC    = DeviceModels.Chipset.CortexM0.Drivers.InterruptController;


    public class GpioPin : Llilum.GpioPin
    {
        private readonly int             m_pinNumber;
        private unsafe LLOS.GpioContext* m_gpio;
        private LLINTC.Handler           m_handler;
        private LLOS.GpioPinEdge         m_activeEdge;
        private LLOS.GpioPinResistor     m_pinMode;

        //-//
        
        //
        // Constructor methods 
        //

        public static GpioPin Create( int pinNumber )
        {
            return new GpioPin( pinNumber );
        }
        
        public static void Release( GpioPin pin )
        {
            pin.Dispose();
        }

        internal GpioPin( int pinNumber )
        {
            m_pinNumber = pinNumber;

            unsafe
            {
                fixed (LLOS.GpioContext** gpio_ptr = &m_gpio)
                {
                    LLGPIO.LLOS_GPIO_AllocatePin( m_pinNumber, gpio_ptr );
                }

                // Default to Rising edge
                ActivePinEdge = Llilum.PinEdge.RisingEdge;
            }
        }

        ~GpioPin( )
        {
            Dispose( false );
        }

        public override void Dispose( )
        {
            unsafe
            {
                if(m_gpio != null)
                {
                    Dispose( true );
                    m_gpio = null;
                    GC.SuppressFinalize( this );
                }
            }
        }
        
        //
        // Helper methods 
        //
        
        [RT.Inline]
        public override int Read( )
        {
            unsafe
            {
                return LLGPIO.LLOS_GPIO_Read( m_gpio );
            }
        }

        [RT.Inline]
        public override void Write( int value )
        {
            unsafe
            {
                LLGPIO.LLOS_GPIO_Write( m_gpio, value );
            }
        }

        protected override void SetPinMode( Llilum.PinMode pinMode )
        {
            unsafe
            {
                m_pinMode = (LLOS.GpioPinResistor)pinMode;
                LLGPIO.LLOS_GPIO_SetMode( m_gpio, m_pinMode );
            }
        }

        protected override void SetPinDirection( Llilum.PinDirection pinDirection )
        {
            unsafe
            {
                LLGPIO.LLOS_GPIO_SetDirection( m_gpio, pinDirection == Llilum.PinDirection.Input ? LLOS.GpioPinDirection.Input : LLOS.GpioPinDirection.Output );
            }
        }

        protected override void SetActivePinEdge( Llilum.PinEdge pinEdge )
        {
            m_activeEdge = (LLOS.GpioPinEdge)pinEdge;

            //if(m_handler != null)
            //{
            //    DisableInterrupt( );
            //    EnableInterrupt( );
            //}
        }

        protected override void EnableInterrupt( )
        {
            if(m_handler == null)
            {
                m_handler = LLINTC.Handler.Create( 
                    RT.GpioProvider.Instance.GetGpioPinIRQNumber( m_pinNumber ),
                    M.Drivers.InterruptPriority.Normal,
                    M.Drivers.InterruptSettings.RisingEdge,
                    ProcessGpioInterrupt );
            }

            unsafe
            {
                UIntPtr hndPtr = MBED.InterruptController.CastInterruptHandlerAsPtr( m_handler ); 

                LLGPIO.LLOS_GPIO_EnablePin( m_gpio, m_activeEdge, HandleGpioInterruptNative, hndPtr );
            }
        }

        protected override void DisableInterrupt( )
        {
            unsafe
            {
                LLGPIO.LLOS_GPIO_DisablePin( m_gpio );
            }
        }

        protected void Dispose( bool disposing )
        {
            unsafe
            {
                LLGPIO.LLOS_GPIO_FreePin( m_gpio );
            }

            using(RT.SmartHandles.InterruptState.Disable( ))
            {
                LLINTC.Instance.Deregister( m_handler );
            }

            if(disposing)
            {
                RT.HardwareProvider.Instance.ReleasePins( m_pinNumber );
            }
        }
        
        
        //
        // Access methods 
        //

        public override int PinNumber
        {
            get
            {
                return m_pinNumber;
            }
        }

        //--//

        private void ProcessGpioInterrupt( LLINTC.InterruptData data )
        {
            SendEventInternal( (Llilum.PinEdge)data.Context );
        }

        private static unsafe void HandleGpioInterruptNative( LLOS.GpioContext* pin, UIntPtr context, LLOS.GpioPinEdge evt )
        {
            LLINTC.InterruptData data;

            data.Handler = MBED.InterruptController.CastAsInterruptHandler( context );
            data.Context = (uint)evt;

            //
            // This interrupt handler does not come from the ISR vector table, but rather from the handler set up 
            // by mBed through 'NVIC_SetVector( <ISR NUMBER>, (uint32_t)us_ticker_irq_handler)' during initialization. 
            // Therefore we need to wrap this specific handler here, which is where it first shows up. 
            //
            using(RT.SmartHandles.InterruptState.Disable( ))
            {
                using(RT.SmartHandles.SwapCurrentThreadUnderInterrupt hnd = RT.ThreadManager.InstallInterruptThread( ))
                {
                    CortexM0OnCMSISCore.Drivers.InterruptController.Instance.PostInterrupt( data );
                }
            }
        }
    }
}

