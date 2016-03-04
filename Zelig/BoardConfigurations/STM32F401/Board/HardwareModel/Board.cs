//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Llilum.STM32F401
{
    using System;
    
    using Chipset           = Microsoft.CortexM4OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM4;

    public sealed class Board : Chipset.Board
    {
        internal const int GPIO_PORT_SHIFT = 12;

        //
        // Serial Ports
        //

        // NOTICE: Names are 1-indexed as per STM Board Description
        private static readonly string[] m_serialPorts = { "UART1", "UART2", "UART6" };

        public static readonly ChipsetAbstration.Board.SerialPortInfo UART1 = new ChipsetAbstration.Board.SerialPortInfo() 
        {
            TxPin = (int)PinName.PA_9,
            RxPin = (int)PinName.PA_10,
            RtsPin = unchecked((int)PinName.NC),
            CtsPin = unchecked((int)PinName.NC)
        };

        public static readonly ChipsetAbstration.Board.SerialPortInfo UART2 = new ChipsetAbstration.Board.SerialPortInfo() 
        {
            TxPin = (int)PinName.USBTX,
            RxPin = (int)PinName.USBRX,
            RtsPin = unchecked((int)PinName.NC),
            CtsPin = unchecked((int)PinName.NC)
        };

        public static readonly ChipsetAbstration.Board.SerialPortInfo UART6 = new ChipsetAbstration.Board.SerialPortInfo()
        {
            TxPin = (int)PinName.PA_11,
            RxPin = (int)PinName.PA_12,
            RtsPin = unchecked((int)PinName.NC),
            CtsPin = unchecked((int)PinName.NC)
        };

        //--//

        private static readonly int[] s_ledPins = new int[] 
        {
            (int)STM32F401.PinName.LED1,
        };

        private static readonly int[] s_pwmPins = new int[] 
        {
            (int)STM32F401.PinName.D3,
        };

        //
        // Gpio discovery
        //

        public override int PinCount
        {
            get
            {
                return 48;
            }
        }
        
        public override int PinToIndex( int pin )
        {
            return pin;
        }

        public override int NCPin
        {
            get
            {
                return -1;
            }
        }

        public override int[] LedPins
        {
            get
            {
                return s_ledPins;
            }
        }

        public override int[] PwmPins
        {
            get
            {
                return s_pwmPins;
            }
        }

        //
        // Serial Port
        //
        public override string[] GetSerialPorts()
        {
            return m_serialPorts;
        }

        public override ChipsetAbstration.Board.SerialPortInfo GetSerialPortInfo(string portName)
        {
            switch (portName)
            {
                case "UART1":
                    return UART1;
                case "UART2":
                    return UART2;
                case "UART6":
                    return UART6;
                default:
                    return null;
            }
        }

        //
        // System timer
        //
        public override int GetSystemTimerIRQ( )
        {
            return (int)IRQn.TIM2_IRQn;
        }

        public override int GetSerialPortIRQ(string portName)
        {
            switch (portName)
            {
                case "UART1":
                    return (int)IRQn.USART1_IRQn;
                case "UART2":
                    return (int)IRQn.USART2_IRQn;
                case "UART6":
                    return (int)IRQn.USART6_IRQn;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
