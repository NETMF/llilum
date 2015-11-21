//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Test.mbed
{
    using SPI = Microsoft.Llilum.Devices.Spi;
    using GPIO = Microsoft.Llilum.Devices.Gpio;
    using Runtime;
    using System;

    public class SpiLcdC12832 : IDisposable
    {
        private enum LcdSpiType : int
        {
            Command = 0,
            Data,
        }

        private enum LcdSpiCommand : byte
        {
            DisplayOff            = 0xAE,
            BiasVoltage           = 0xA2,
            SetColumn             = 0xA0,
            VoltageResistorRation = 0x22,
            PowerOn               = 0x2F,
            DisplayRAM            = 0xA4,
            StartLineZero         = 0x40,
            DisplayOn             = 0xAF,
            SetContrast           = 0x81,
            DisplayNormal         = 0xA6,

            Page0                 = 0xB0,
            Page1                 = 0xB1,
            Page2                 = 0xB2,
            Page3                 = 0xB3,
        }

        private enum LcdSpiCommandDefaults : byte
        {
            Contrast = 0x17,
            Column   = 0xC8,
        }

        private const byte c_ContrastMask = 0x3F;
        private const int  c_SpiClockFreq = 1000000;

        private SPI.SpiDevice m_spi;
        private GPIO.GpioPin m_cmdPin;
        private GPIO.GpioPin m_resetPin;
        private byte[] m_lcdBuffer;
        private byte[] m_resetCmds;
        private byte[] m_flushCmds;

        public SpiLcdC12832( SPI.ISpiChannelInfo channelInfo, int commandPin, int resetPin, int csPin )
        {
            m_spi       = new SPI.SpiDevice( channelInfo, csPin, true );
            m_cmdPin    = GpioProvider.Instance.CreateGpioPin( commandPin );
            m_resetPin  = GpioProvider.Instance.CreateGpioPin( resetPin );
            m_lcdBuffer = new byte[512];

            m_cmdPin.Direction   = GPIO.PinDirection.Output;
            m_cmdPin.Mode        = GPIO.PinMode.Default;
            m_resetPin.Direction = GPIO.PinDirection.Output;
            m_resetPin.Mode      = GPIO.PinMode.Default;

            m_spi.Mode           = SPI.SpiMode.Cpol1Cpha1;
            m_spi.DataBitLength  = 8;
            m_spi.ClockFrequency = c_SpiClockFreq;

            m_spi.Open();

            m_resetCmds = new byte[] {
                (byte)LcdSpiCommand.DisplayOff,
                (byte)LcdSpiCommand.BiasVoltage,
                (byte)LcdSpiCommand.SetColumn,
                (byte)LcdSpiCommandDefaults.Column,
                (byte)LcdSpiCommand.VoltageResistorRation,
                (byte)LcdSpiCommand.PowerOn,
                //(byte)LcdSpiCommand.DisplayRAM,
                (byte)LcdSpiCommand.StartLineZero,
                (byte)LcdSpiCommand.DisplayOn,
                (byte)LcdSpiCommand.SetContrast,
                (byte)LcdSpiCommandDefaults.Contrast,
                (byte)LcdSpiCommand.DisplayNormal,
            };

            m_flushCmds = new byte[] { 0x00, 0x10, (byte)LcdSpiCommand.Page0 };

            Reset();
        }

        public void SetContast( byte contrast )
        {
            WriteCommand( LcdSpiCommand.SetContrast );
            WriteCommand( (LcdSpiCommand)(contrast & c_ContrastMask) );
        }

        public void Reset()
        {
            // reset the display
            m_cmdPin.Write( (int)LcdSpiType.Command );
            m_resetPin.Write( 0 );
            Processor.DelayMicroseconds( 50 );
            m_resetPin.Write( 1 );
            Processor.DelayMicroseconds( 5000 );

            // Send the default setup parameters
            WriteCommands( m_resetCmds );
            Clear( );
        }

        public void Clear()
        {
            Array.Clear( m_lcdBuffer, 0, m_lcdBuffer.Length );

            Flush( );
        }

        public void Fill()
        {
            for(int i=0; i<m_lcdBuffer.Length; i++)
            {
                m_lcdBuffer[ i ] = 0xFF;
            }

            Flush( );
        }

        public void SetPixel(int x, int y, int color)
        {
            if(x > 128 || y > 32 || x < 0 || y < 0)
            {
                return;
            }

            if(color == 0)
            {
                m_lcdBuffer[ x + ( ( y/8 ) * 128 ) ] &= (byte)~( 1 << ( y%8 ) );
            }
            else
            {
                m_lcdBuffer[ x + ( ( y/8 ) * 128 ) ] |= (byte)( 1 << ( y%8 ) );
            }
        }

        public void Flush()
        {
            m_flushCmds[ 2 ] = (byte)LcdSpiCommand.Page0;
            WriteCommands( m_flushCmds );
            WriteData( m_lcdBuffer, 0, 128 );

            m_flushCmds[ 2 ] = (byte)LcdSpiCommand.Page1;
            WriteCommands( m_flushCmds );
            WriteData( m_lcdBuffer, 128, 128 );

            m_flushCmds[ 2 ] = (byte)LcdSpiCommand.Page2;
            WriteCommands( m_flushCmds );
            WriteData( m_lcdBuffer, 256, 128 );

            m_flushCmds[ 2 ] = (byte)LcdSpiCommand.Page3;
            WriteCommands( m_flushCmds );
            WriteData( m_lcdBuffer, 384, 128 );
        }

        public void Dispose( )
        {
            m_spi.Dispose( );
            m_cmdPin.Dispose( );
            m_resetPin.Dispose( );
        }

        private void WriteCommand( LcdSpiCommand command )
        {
            WriteCommands( new byte[] { (byte)command } );
        }

        private void WriteCommands( byte[] commands )
        {
            m_cmdPin.Write( (int)LcdSpiType.Command );
            m_spi.Write( commands, 0, commands.Length );
        }

        private void WriteData( byte data )
        {
            WriteCommands( new byte[] { (byte)data } );
        }

        private void WriteData( byte[] data, int offset, int count )
        {
            m_cmdPin.Write( (int)LcdSpiType.Data );
            m_spi.Write( data, offset, count );
        }
    }
}
