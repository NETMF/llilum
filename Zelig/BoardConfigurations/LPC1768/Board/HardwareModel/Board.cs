//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.LPC1768
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
            TxPin =             (int)PinName.USBTX,
            RxPin =             (int)PinName.USBRX,
            RtsPin = unchecked( (int)PinName.NC ) ,
            CtsPin = unchecked( (int)PinName.NC ) ,
        };

        public static readonly ChipsetAbstration.Board.SerialPortInfo UART1 = new ChipsetAbstration.Board.SerialPortInfo() 
        {
            TxPin =             (int)PinName.p13 ,
            RxPin =             (int)PinName.p14 ,
            RtsPin = unchecked( (int)PinName.NC ),
            CtsPin = unchecked( (int)PinName.NC ),
        };

        //--//

        private static readonly int[] s_ledPins = new int[] 
        {
            (int)LPC1768.PinName.LED1,
            (int)LPC1768.PinName.LED2,
            (int)LPC1768.PinName.LED3,
            (int)LPC1768.PinName.LED4,
        };

        private static readonly int[] s_pwmPins = new int[] 
        {
            (int)LPC1768.PinName.p21,
        };

        //--//

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
        
        public override int PinToIndex( int pin )
        {
            return pin - (int)PinName.P0_0;
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

        public override int GetSerialPortIRQ(string portName)
        {
            switch (portName)
            {
                case "UART0":
                    return (int)IRQn.UART0_IRQn;
                case "UART1":
                    return (int)IRQn.UART1_IRQn;
                default:
                    throw new NotSupportedException();
            }
        }
        
        //////public override void RemapSerialPortInterrupts( )
        //////{
        //////    Processor.RemapInterrupt( IRQn.UART0_ERR_IRQn  ); 
        //////    Processor.RemapInterrupt( IRQn.UART0_ERR_IRQn  ); 
        //////    Processor.RemapInterrupt( IRQn.UART1_ERR_IRQn  ); 
        //////    Processor.RemapInterrupt( IRQn.UART1_ERR_IRQn  ); 
        //////    Processor.RemapInterrupt( IRQn.UART3_ERR_IRQn  ); 
        //////    Processor.RemapInterrupt( IRQn.UART3_ERR_IRQn  ); 
        //////}

        //
        // System timer
        //
        public override int GetSystemTimerIRQ( )
        {
            return (int)IRQn.TIMER3_IRQn;
        }
    }
}

