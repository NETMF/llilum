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
        // SPI Ports
        //

        private static readonly string[] m_spiDevices = { "SPI0", "SPI1" };
        
        public static readonly ChipsetAbstration.Board.SpiChannelInfo SPI0 = new ChipsetAbstration.Board.SpiChannelInfo()
        {
            Mosi            = (int)PinName.p5,
            Miso            = (int)PinName.p6,
            Sclk            = (int)PinName.p7,
            ChipSelect      = (int)PinName.p8,
            ChipSelectLines = 1,
            MinFreq         = 1000,
            MaxFreq         = 30000000,
            Supports16      = true,
            ActiveLow       = true,
            SetupTime       = 0,
            HoldTime        = 0,
        };

        public static readonly ChipsetAbstration.Board.SpiChannelInfo SPI1 = new ChipsetAbstration.Board.SpiChannelInfo()
        {
            Mosi            = (int)PinName.p11,
            Miso            = (int)PinName.p12,
            Sclk            = (int)PinName.p13,
            ChipSelect      = (int)PinName.p14,
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
        private static readonly string[] m_i2cDevices = { "I2C0", "I2C1" };

        public static readonly ChipsetAbstration.Board.I2cChannelInfo I2C0 = new ChipsetAbstration.Board.I2cChannelInfo() 
        {
            SdaPin = (int)PinName.p9,
            SclPin = (int)PinName.p10,
        };

        public static readonly ChipsetAbstration.Board.I2cChannelInfo I2C1 = new ChipsetAbstration.Board.I2cChannelInfo() 
        {
            SdaPin = (int)PinName.p28,
            SclPin = (int)PinName.p27,
        };

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
        // SPI discovery
        //

        public override string[] GetSpiChannels()
        {
            return m_spiDevices;
        }

        public override int GetSpiChannelIndexFromString( string busId )
        {
            switch(busId)
            {
                case "SPI0":
                    return 0;
                case "SPI1":
                    return 1;
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

        public override bool SpiBusySupported
        {
            get
            {
                return true;
            }
        }
        
        public override int PinToIndex( int pin )
        {
            return pin - (int)PinName.P0_0;
        }

        //
        // I2c Methods
        //

        public override ChipsetAbstration.Board.I2cChannelInfo GetI2cChannelInfo(int index)
        {
            switch (index)
            {
                case 0:
                    return I2C0;
                case 1:
                    return I2C1;
                default:
                    return null;
            }
        }

        public override string[] GetI2cChannels()
        {
            return m_i2cDevices;
        }

        public override int GetI2cChannelIndexFromString(string busId)
        {
            switch (busId)
            {
                case "I2C0":
                    return 0;
                case "I2C1":
                    return 1;
                default:
                    return -1;
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

        //
        // System timer
        //
        public override int GetSystemTimerIRQNumber( )
        {
            return (int)IRQn.TIMER3_IRQn;
        }
    }
}

