//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM0OnMBED.HardwareModel
{
    using System;
    using Llilum  = Llilum.Devices.Gpio;
    using RT      = Microsoft.Zelig.Runtime;
    using M       = DeviceModels.Chipset.CortexM;
    using M0      = DeviceModels.Chipset.CortexM0;
    using MBED    = CortexM0OnMBED.Drivers;
    using LLOS    = Zelig.LlilumOSAbstraction.API.IO;
    using LLGPIO  = Zelig.LlilumOSAbstraction.API.IO.Gpio;

    public class GpioPin : Llilum.GpioPin
    {
        private readonly int                           m_pinNumber;
        private unsafe LLOS.GpioContext*               m_gpio;
        private M0.Drivers.InterruptController.Handler m_handler;
        private LLOS.GpioPinEdge                       m_activeEdge;
        private LLOS.GpioPinResistor                   m_pinMode;
        private bool                                   m_irqEnabled;

        internal GpioPin(int pinNumber)
        {
            m_pinNumber = pinNumber;

            unsafe
            {
                fixed (LLOS.GpioContext** gpio_ptr = &m_gpio)
                {
                    LLGPIO.LLOS_GPIO_AllocatePin(m_pinNumber, gpio_ptr);
                }

                // Default to Rising edge
                ActivePinEdge = Llilum.PinEdge.RisingEdge;
            }

            m_handler = M0.Drivers.InterruptController.Handler.Create(
                RT.GpioProvider.Instance.GetGpioPinIRQNumber(m_pinNumber),
                M.Drivers.InterruptPriority.Normal,
                M.Drivers.InterruptSettings.RisingEdge,
                ProcessGpioInterrupt);
        }

        ~GpioPin()
        {
            Dispose(false);
        }

        public override int PinNumber
        {
            get
            {
                return m_pinNumber;
            }
        }

        public override int Read()
        {
            unsafe
            {
                return LLGPIO.LLOS_GPIO_Read(m_gpio);
            }
        }

        public override void Write(int value)
        {
            unsafe
            {
                LLGPIO.LLOS_GPIO_Write(m_gpio, value);
            }
        }

        protected override void SetPinMode(Llilum.PinMode pinMode)
        {
            unsafe
            {
                m_pinMode = (LLOS.GpioPinResistor)pinMode;
                LLGPIO.LLOS_GPIO_SetMode( m_gpio, m_pinMode );
            }
        }

        protected override void SetPinDirection(Llilum.PinDirection pinDirection)
        {
            unsafe
            {
                LLGPIO.LLOS_GPIO_SetDirection( m_gpio, pinDirection == Llilum.PinDirection.Input ? LLOS.GpioPinDirection.Input : LLOS.GpioPinDirection.Output );
            }
        }

        protected override void SetActivePinEdge(Llilum.PinEdge pinEdge)
        {
            m_activeEdge = (LLOS.GpioPinEdge)pinEdge;

            if( m_irqEnabled )
            {
                DisableInterrupt( );
                EnableInterrupt( );
            }
        }

        protected override void EnableInterrupt()
        {
            unsafe
            {
                UIntPtr hndPtr = MBED.InterruptController.CastInterruptHandlerAsPtr(m_handler);
                LLGPIO.LLOS_GPIO_EnablePin( m_gpio, m_activeEdge, HandleGpioInterrupt, hndPtr );
            }

            m_irqEnabled = true;
        }

        protected override void DisableInterrupt()
        {
            unsafe
            {
                LLGPIO.LLOS_GPIO_DisablePin( m_gpio );
            }

            m_irqEnabled = false;
        }

        public override void Dispose()
        {
            unsafe
            {
                if (m_gpio != null)
                {
                    Dispose(true);
                    m_gpio = null;
                    GC.SuppressFinalize(this);
                }
            }
        }

        private void Dispose(bool disposing)
        {
            unsafe
            {
                LLGPIO.LLOS_GPIO_FreePin( m_gpio );
            }

            using (RT.SmartHandles.InterruptState.Disable())
            {
                M0.Drivers.InterruptController.Instance.Deregister(m_handler);
            }

            if (disposing)
            {
                RT.HardwareProvider.Instance.ReleasePins(m_pinNumber);
            }
        }

        private void ProcessGpioInterrupt(M0.Drivers.InterruptController.InterruptData data)
        {
            SendEventInternal((Llilum.PinEdge)data.Context);
        }

        private static unsafe void HandleGpioInterrupt(LLOS.GpioContext *pin, UIntPtr context, LLOS.GpioPinEdge evt)
        {
            M0.Drivers.InterruptController.InterruptData data;

            data.Handler = MBED.InterruptController.CastAsInterruptHandler(context);
            data.Context = (uint)evt;
            data.Subcontext = 0;

            using (RT.SmartHandles.InterruptState.Disable())
            {
                M0.Drivers.InterruptController.Instance.PostInterrupt(data);
            }
        }
    }
}
