//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.Devices.Gpio
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void ValueChangedHandler( object sender, PinEdge args );

    public abstract class GpioPin : IDisposable
    {
        private PinDirection        m_pinDirection;
        private PinMode             m_pinMode;
        private PinEdge             m_activePinEdge;
        private ValueChangedHandler m_evt;

        public event ValueChangedHandler ValueChanged
        {
            add
            {
                var old = m_evt;
                m_evt += value;

                if (old == null)
                {
                    EnableInterrupt();
                }
            }
            remove
            {
                m_evt -= value;

                if (m_evt == null)
                {
                    DisableInterrupt();
                }
            }
        }

        public static int BoardPinCount
        {
            get
            {
                return GetBoardPinCount();
            }
        }

        public abstract int PinNumber { get; }

        public PinDirection Direction
        {
            get
            {
                return m_pinDirection;
            }
            set
            {
                m_pinDirection = value;
                SetPinDirection(value);
            }
        }

        public PinMode Mode
        {
            get
            {
                return m_pinMode;
            }
            set
            {
                m_pinMode = value;
                SetPinMode(value);
            }
        }

        public PinEdge ActivePinEdge
        {
            get
            {
                return m_activePinEdge;
            }
            set
            {
                m_activePinEdge = value;

                SetActivePinEdge(value);
            }
        }

        public abstract void Dispose();

        public abstract int Read();

        protected abstract void SetPinMode(PinMode pinMode);

        protected abstract void SetPinDirection(PinDirection pinDirection);

        protected abstract void SetActivePinEdge(PinEdge pinEdge);

        public abstract void Write(int value);

        public static GpioPin TryCreateGpioPin(int pinNumber)
        {
            GpioPin newPin = TryAcquireGpioPin(pinNumber);
            if(newPin == null)
            {
                return null;
            }

            // Set default values
            newPin.Direction = PinDirection.Input;
            newPin.Mode = PinMode.Default;

            return newPin;
        }

        protected void SendEventInternal(PinEdge pinEdge)
        {
            m_evt?.Invoke(this, pinEdge);
        }

        protected abstract void EnableInterrupt();

        protected abstract void DisableInterrupt();


        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern GpioPin TryAcquireGpioPin(int pinNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int GetBoardPinCount();
    }

    public enum PinDirection
    {
        Input = 0,
        Output,
    }

    public enum PinMode
    {
        Default = 0,
        PullNone,
        PullUp,
        PullDown,
        OpenDrain,
        Repeater,
    }

    public enum PinEdge
    {
        None = 0,
        RisingEdge,
        FallingEdge,
        BothEdges,
        LevelLow,
        LevelHigh,
    }
}
