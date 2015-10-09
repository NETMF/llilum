//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.K64F
{
    using System;
    
    using Chipset           = Microsoft.CortexM4OnMBED;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM4;


    public sealed class Board : Chipset.Board
    {
        internal const int GPIO_PORT_SHIFT = 12;

        //
        // SPI Ports
        //

        private  static readonly string[] _spiDevices    = { "SPI0", "SPI1", "SPI2" };
        
        public static readonly ChipsetAbstration.Board.SpiChannelInfo SPI0 = new ChipsetAbstration.Board.SpiChannelInfo()
        {
            Mosi            = (int)PinName.PTD2,
            Miso            = (int)PinName.PTD3,
            Sclk            = (int)PinName.PTD1,
            ChipSelect      = unchecked((int)PinName.NC), 
            ChipSelectLines = 1,
            MinFreq         = 1000,
            MaxFreq         = 30000000,
            Supports16      = true,
            ActiveLow       = true,
            SetupTime       = 10,
            HoldTime        = 10,
        };

        public static readonly ChipsetAbstration.Board.SpiChannelInfo SPI1 = new ChipsetAbstration.Board.SpiChannelInfo()
        {
            Mosi            = (int)PinName.PTD6,
            Miso            = (int)PinName.PTD7,
            Sclk            = (int)PinName.PTD5,
            ChipSelect      = (int)PinName.PTD4,
            ChipSelectLines = 1,
            MinFreq         = 1000,
            MaxFreq         = 30000000,
            Supports16      = true,
            ActiveLow       = true,
            SetupTime       = 10,
            HoldTime        = 10,
        };

        public static readonly ChipsetAbstration.Board.SpiChannelInfo SPI2 = new ChipsetAbstration.Board.SpiChannelInfo()
        {
            Mosi            = (int)PinName.PTE3,
            Miso            = (int)PinName.PTE1,
            Sclk            = (int)PinName.PTE2,
            ChipSelect      = (int)PinName.PTE4,
            ChipSelectLines = 1,
            MinFreq         = 1000,
            MaxFreq         = 30000000,
            Supports16      = true,
            ActiveLow       = true,
            SetupTime       = 10,
            HoldTime        = 10,
        };

        //
        // I2c Ports
        //
        private static readonly string[] _i2cDevices = { "I2C0" };

        public static readonly ChipsetAbstration.Board.I2cChannelInfo I2C0 = new ChipsetAbstration.Board.I2cChannelInfo() {
            SdaPin = (int)PinName.PTE25,
            SclPin = (int)PinName.PTE24,
        };

        //
        // Serial Ports
        //
        private static readonly string[] m_serialPorts = { "UART0", "UART1" };

        public static readonly ChipsetAbstration.Board.SerialPortInfo UART0 = new ChipsetAbstration.Board.SerialPortInfo() 
        {
            TxPin = (int)PinName.PTC14,
            RxPin = (int)PinName.PTC15,
            RtsPin = unchecked((int)PinName.NC),
            CtsPin = unchecked((int)PinName.NC)
        };

        public static readonly ChipsetAbstration.Board.SerialPortInfo UART1 = new ChipsetAbstration.Board.SerialPortInfo() 
        {
            TxPin = (int)PinName.USBTX,
            RxPin = (int)PinName.USBRX,
            RtsPin = unchecked((int)PinName.NC),
            CtsPin = unchecked ((int)PinName.NC)
        };

        //
        // SPI discovery
        //

        public override string[] GetSpiChannels()
        {
            return _spiDevices;
        }

        public override int GetSpiChannelIndexFromString( string busId )
        {
            switch(busId)
            {
                case "SPI0":
                    return 0;
                case "SPI1":
                    return 1;
                case "SPI2":
                    return 2;
                default:
                    return -1;
            }
        }

        // The cases should match the device selector strings
        public override ChipsetAbstration.Board.SpiChannelInfo GetSpiChannelInfo( int id )
        {
            switch(id)
            {
                case 0:
                    return SPI0;
                case 1:
                    return SPI1;
                case 2:
                    return SPI2;
                default:
                    return null;
            }
        }

        //
        // Gpio discovery
        //

        public override int PinCount
        {
            get
            {
                return 160;
            }
        }

        public override int NCPin
        {
            get
            {
                return -1;
            }
        }

        public override bool SpiBusySupported
        {
            get
            {
                return false;
            }
        }
        
        public override int PinToIndex( int pin )
        {
            int port = pin >> Board.GPIO_PORT_SHIFT;
            int portIndex = pin & 0x000000FF;

            return ( port * 32 ) + portIndex;
        }

        //
        // I2C Discovery
        //

        public override ChipsetAbstration.Board.I2cChannelInfo GetI2cChannelInfo(int index)
        {
            switch (index)
            {
                case 0:
                    return I2C0;
                default:
                    return null;
            }
        }

        public override string[] GetI2cChannels()
        {
            return _i2cDevices;
        }

        public override int GetI2cChannelIndexFromString(string busId)
        {
            switch (busId)
            {
                case "I2C0":
                    return 0;
                default:
                    return -1;
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
            return (int)IRQn.PIT3_IRQn;
        }
    }
}
