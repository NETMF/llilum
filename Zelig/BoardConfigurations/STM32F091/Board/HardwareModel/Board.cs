//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Llilum.STM32F091
{
    using System;
    
    using Chipset           = Microsoft.CortexM0OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM0;

    public sealed class Board : Chipset.Board
    {
        internal const int GPIO_PORT_SHIFT = 12;

        //
        // Serial Ports
        //

        // NOTICE: Names are 1-indexed as per STM Board Description
        private static readonly string[] m_serialPorts = { "UART2", "UART3" };

        public static readonly ChipsetAbstration.Board.SerialPortInfo UART2 = new ChipsetAbstration.Board.SerialPortInfo() 
        {
            TxPin =             (int)PinName.PA_14,
            RxPin =             (int)PinName.PA_15,
            RtsPin = unchecked( (int)PinName.NC  ),
            CtsPin = unchecked( (int)PinName.NC  ),
        };

        public static readonly ChipsetAbstration.Board.SerialPortInfo UART3 = new ChipsetAbstration.Board.SerialPortInfo() 
        {
            TxPin =             (int)PinName.PB_11,
            RxPin =             (int)PinName.PC_4 ,
            RtsPin = unchecked( (int)PinName.NC  ),
            CtsPin = unchecked( (int)PinName.NC  ),
        };

        //--//

        private static readonly int[] s_ledPins = new int[] 
        {
            (int)STM32F091.PinName.LED1,
        };

        private static readonly int[] s_pwmPins = new int[]
        {
            (int)STM32F091.PinName.PB_0,
        };

        //
        // Gpio discovery
        //

        public override int PinCount
        {
            get
            {
                return 51;
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
                case "UART2":
                    return UART2;
                case "UART3":
                    return UART3;
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
                case "UART2":
                    return (int)IRQn.USART2_IRQn;
                case "UART3":
                    return (int)IRQn.USART3_8_IRQn;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
