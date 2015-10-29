//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using System.Runtime.InteropServices;

    using Llilum  = Llilum.Devices.Gpio;
    using Runtime = Zelig.Runtime;
    using M3      = DeviceModels.Chipset.CortexM3;
    using MBED    = CortexM3OnMBED.Drivers;

    public class GpioPin : Llilum.GpioPin
    {
        private readonly int                           m_pinNumber;
        private unsafe GpioImpl*                       m_gpio;
        private unsafe GpioIrqImpl*                    m_gpioIrq;
        private M3.Drivers.InterruptController.Handler m_handler;

        internal GpioPin(int pinNumber)
        {
            m_pinNumber = pinNumber;

            unsafe
            {
                fixed (GpioImpl** gpio_ptr = &m_gpio)
                {
                    tmp_gpio_alloc_init(gpio_ptr, m_pinNumber);
                }

                fixed (GpioIrqImpl** ppIrq = &m_gpioIrq)
                {
                    tmp_gpio_irq_alloc(ppIrq);
                }

                // Default to Rising edge
                ActivePinEdge = Llilum.PinEdge.RisingEdge;
            }

            m_handler = M3.Drivers.InterruptController.Handler.Create(
                (Runtime.TargetPlatform.ARMv7.ProcessorARMv7M.IRQn_Type)Runtime.GpioProvider.Instance.GetGpioPinIRQNumber(m_pinNumber),
                M3.Drivers.InterruptPriority.Normal,
                M3.Drivers.InterruptSettings.RisingEdge,
                HandleGpioInterrupt);
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
                return tmp_gpio_read(m_gpio);
            }
        }

        public override void Write(int value)
        {
            unsafe
            {
                tmp_gpio_write(m_gpio, value);
            }
        }

        protected override void SetPinMode(Llilum.PinMode pinMode)
        {
            unsafe
            {
                tmp_gpio_mode(m_gpio, pinMode);
            }
        }

        protected override void SetPinDirection(Llilum.PinDirection pinDirection)
        {
            unsafe
            {
                tmp_gpio_dir(m_gpio, pinDirection);
            }
        }

        protected override void SetActivePinEdge(Llilum.PinEdge pinEdge)
        {
            unsafe
            {
                switch ( pinEdge )
                {
                    case Llilum.PinEdge.BothEdges:
                        tmp_gpio_irq_set(m_gpioIrq, (int)MbedGpioIrq.IRQ_RISE, 1);
                        tmp_gpio_irq_set(m_gpioIrq, (int)MbedGpioIrq.IRQ_FALL, 1);
                        break;
                    case Llilum.PinEdge.FallingEdge:
                        tmp_gpio_irq_set(m_gpioIrq, (int)MbedGpioIrq.IRQ_RISE, 0);
                        tmp_gpio_irq_set(m_gpioIrq, (int)MbedGpioIrq.IRQ_FALL, 1);
                        break;
                    case Llilum.PinEdge.RisingEdge:
                        tmp_gpio_irq_set(m_gpioIrq, (int)MbedGpioIrq.IRQ_RISE, 1);
                        tmp_gpio_irq_set(m_gpioIrq, (int)MbedGpioIrq.IRQ_FALL, 0);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        protected override void EnableInterrupt()
        {
            unsafe
            {
                UIntPtr hndPtr = MBED.InterruptController.CastInterruptHandlerAsPtr(m_handler);

                tmp_gpio_irq_init(m_gpioIrq, m_pinNumber, (uint)hndPtr);
                SetActivePinEdge(ActivePinEdge);
                tmp_gpio_irq_enable(m_gpioIrq);
            }
        }

        protected override void DisableInterrupt()
        {
            unsafe
            {
                tmp_gpio_irq_disable(m_gpioIrq);
            }
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
                tmp_gpio_irq_uninit(m_gpioIrq);
                tmp_gpio_free(m_gpio);
                tmp_gpio_irq_free(m_gpioIrq);
            }

            using (Runtime.SmartHandles.InterruptState.Disable())
            {
                M3.Drivers.InterruptController.Instance.Deregister(m_handler);
            }

            if (disposing)
            {
                Runtime.HardwareProvider.Instance.ReleasePins(m_pinNumber);
            }
        }

        private void HandleGpioInterrupt(M3.Drivers.InterruptController.InterruptData data)
        {
            SendEventInternal((Llilum.PinEdge)data.Context);
        }

        private enum MbedGpioIrq
        {
            IRQ_NONE = 0,
            IRQ_RISE = 1,
            IRQ_FALL = 2,
        }

        [Runtime.ExportedMethod]
        private static void HandleGpioInterrupt(uint id, MbedGpioIrq evt)
        {
            M3.Drivers.InterruptController.InterruptData data;

            data.Handler = MBED.InterruptController.CastAsInterruptHandler((UIntPtr)id);
            data.Context = evt == MbedGpioIrq.IRQ_RISE ? (uint)Llilum.PinEdge.RisingEdge : (uint)Llilum.PinEdge.FallingEdge;
            data.Subcontext = 0;

            using (Runtime.SmartHandles.InterruptState.Disable())
            {
                M3.Drivers.InterruptController.Instance.PostInterrupt(data);
            }
        }

        [DllImport("C")]
        private static unsafe extern int tmp_gpio_alloc_init(GpioImpl** obj, int pinNumber);
        [DllImport("C")]
        private static unsafe extern int tmp_gpio_free(GpioImpl* obj);
        [DllImport("C")]
        private static unsafe extern void tmp_gpio_write(GpioImpl* obj, int value);
        [DllImport("C")]
        private static unsafe extern int tmp_gpio_read(GpioImpl* obj);
        [DllImport("C")]
        private static unsafe extern void tmp_gpio_mode(GpioImpl* obj, Llilum.PinMode mode);
        [DllImport("C")]
        private static unsafe extern void tmp_gpio_dir(GpioImpl* obj, Llilum.PinDirection direction);

        [DllImport("C")]
        private static unsafe extern void tmp_gpio_irq_alloc(GpioIrqImpl** obj);
        [DllImport("C")]
        private static unsafe extern void tmp_gpio_irq_free(GpioIrqImpl* obj);
        [DllImport("C")]
        private static unsafe extern int tmp_gpio_irq_init(GpioIrqImpl* obj, int pinNumber, uint id);
        [DllImport("C")]
        private static unsafe extern void tmp_gpio_irq_uninit(GpioIrqImpl* obj);
        [DllImport("C")]
        private static unsafe extern void tmp_gpio_irq_set(GpioIrqImpl* obj, int edge, uint enable);
        [DllImport("C")]
        private static unsafe extern void tmp_gpio_irq_enable(GpioIrqImpl* obj);
        [DllImport("C")]
        private static unsafe extern void tmp_gpio_irq_disable(GpioIrqImpl* obj);
    }

    internal unsafe struct GpioImpl
    {
    };
    internal unsafe struct GpioIrqImpl
    {
    };
}
