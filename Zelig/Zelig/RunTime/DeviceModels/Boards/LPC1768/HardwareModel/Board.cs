//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LPC1768
{
    using System;
    
    using Chipset           = Microsoft.CortexM3OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;

    public sealed class Board : Chipset.Board
    {
        //
        // Serial Ports
        //
        private static readonly string[] m_serialPorts = { "UART0", "UART1" };

        public static readonly ChipsetAbstration.Board.SerialPortInfo UART0 = new ChipsetAbstration.Board.SerialPortInfo() 
        {
            TxPin = (int)PinName.USBTX,
            RxPin = (int)PinName.USBRX,
            RtsPin = unchecked((int)PinName.NC),
            CtsPin = unchecked((int)PinName.NC)
        };

        public static readonly ChipsetAbstration.Board.SerialPortInfo UART1 = new ChipsetAbstration.Board.SerialPortInfo() 
        {
            TxPin = (int)PinName.p13,
            RxPin = (int)PinName.p14,
            RtsPin = unchecked((int)PinName.NC),
            CtsPin = unchecked ((int)PinName.NC)
        };

        //
        // Gpio discovery
        //

        public override int PinCount
        {
            get
            {
                return 40;
            }
        }

        public override int NCPin
        {
            get
            {
                return -1;
            }
        }
        
        public override int PinToIndex( int pin )
        {
            return pin - (int)PinName.P0_0;
        }

        //
        // Serial Ports
        //

        public override string[] GetSerialPorts()
        {
            return m_serialPorts;
        }

        public override ChipsetAbstration.Board.SerialPortInfo GetSerialPortInfo(string portName)
        {
            switch (portName)
            {
                case "UART0":
                    return UART0;
                case "UART1":
                    return UART1;
                default:
                    return null;
            }
        }

        //
        // System timer
        //
        public override int GetSystemTimerIRQNumber( )
        {
            return (int)IRQn.TIMER3_IRQn;
        }
    }
}

