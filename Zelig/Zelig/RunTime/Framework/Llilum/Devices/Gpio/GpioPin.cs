//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.Devices.Gpio
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class GpioPin : IDisposable
    {
        private PinDirection    m_pinDirection;
        private PinMode         m_pinMode;


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

        public abstract void Dispose();

        public abstract int Read();

        public abstract void SetPinMode(PinMode pinMode);

        public abstract void SetPinDirection(PinDirection pinDirection);

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
            newPin.Mode = PinMode.PullNone;

            return newPin;
        }
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern GpioPin TryAcquireGpioPin(int pinNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int GetBoardPinCount();
    }

    public enum PinDirection
    {
        Input = 0,
        Output = 1,
    }

    public enum PinMode
    {
        PullUp = 0,
        Repeater = 1,
        PullNone = 2,
        PullDown = 3,
        OpenDrain = 4,
        PullDefault = PullDown,
    }
}
